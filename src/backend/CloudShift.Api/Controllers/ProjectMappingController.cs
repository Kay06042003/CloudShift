using CloudShift.Application.ProjectMappings.Commands;
using CloudShift.Application.ProjectMappings.DTOs;
using CloudShift.Application.ProjectMappings.Models;
using CloudShift.Application.ProjectMappings.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CloudShift.Api.Controllers;

[ApiController]
[Route("api/mappings")]
[Produces("application/json")]
public sealed class ProjectMappingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProjectMappingController> _logger;

    public ProjectMappingController(
        IMediator mediator,
        ILogger<ProjectMappingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectMappingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMapping(
        [FromBody] CreateProjectMappingRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Rejected project mapping create request because model state is invalid.");
            return BadRequest(ModelState);
        }

        var command = new CreateProjectMappingCommand(
            request.UserId,
            request.Name,
            request.SourceProfileId,
            request.DestProfileId,
            request.SourcePath,
            request.DestPath,
            request.FilterConfig ?? new FilterConfig(),
            request.ConflictResolutionRule);

        try
        {
            _logger.LogInformation(
                "Creating project mapping. UserId: {UserId}, SourceProfileId: {SourceProfileId}, DestProfileId: {DestProfileId}",
                request.UserId,
                request.SourceProfileId,
                request.DestProfileId);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation(
                "Created project mapping. MappingId: {MappingId}, UserId: {UserId}",
                result.Id,
                result.UserId);

            return CreatedAtAction(
                nameof(GetMappings),
                new { userId = result.UserId },
                result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot create project mapping because a referenced app profile was not found.");
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Cannot create project mapping because profile ownership validation failed.");
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot create project mapping because validation failed.");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectMappingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMappings(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectMappingsQuery(userId);
        var mappings = await _mediator.Send(query, cancellationToken);

        _logger.LogInformation(
            "Returned project mappings. UserId: {UserId}, Count: {Count}",
            userId,
            mappings.Count);

        return Ok(mappings);
    }

    [HttpGet("jobs")]
    [ProducesResponseType(typeof(IReadOnlyList<MigrationJobDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMigrationJobs(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetMigrationJobsQuery(userId);
        var jobs = await _mediator.Send(query, cancellationToken);

        _logger.LogInformation(
            "Returned migration jobs. UserId: {UserId}, Count: {Count}",
            userId,
            jobs.Count);

        return Ok(jobs);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(MigrationJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartMigration(
        Guid id,
        [FromBody] StartMigrationJobRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new StartMigrationJobCommand(
            UserId: request?.UserId ?? Guid.Empty,
            MappingId: id,
            JobType: request?.JobType ?? Domain.Enums.JobType.Full);

        if (command.UserId == Guid.Empty)
        {
            _logger.LogWarning("Rejected migration job start because UserId was not provided. MappingId: {MappingId}", id);
            return BadRequest(new { error = "UserId is required." });
        }

        try
        {
            _logger.LogInformation(
                "Starting migration job. MappingId: {MappingId}, JobType: {JobType}",
                id,
                command.JobType);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation(
                "Queued migration job. JobId: {JobId}, MappingId: {MappingId}, JobType: {JobType}",
                result.Id,
                result.ProjectMappingId,
                result.JobType);

            return Accepted(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Cannot start migration job because mapping {MappingId} was not found.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Cannot start migration job because mapping ownership validation failed. MappingId: {MappingId}", id);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot start migration job because validation failed. MappingId: {MappingId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }
}

public sealed record StartMigrationJobRequest(
    Guid UserId,
    CloudShift.Domain.Enums.JobType JobType = CloudShift.Domain.Enums.JobType.Full);
