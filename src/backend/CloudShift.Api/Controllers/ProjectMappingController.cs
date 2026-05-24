using CloudShift.Application.ProjectMappings.Commands;
using CloudShift.Application.ProjectMappings.DTOs;
using CloudShift.Application.ProjectMappings.Models;
using CloudShift.Application.ProjectMappings.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CloudShift.Api.Controllers;

/// <summary>
/// Manages Project Mappings (migration configurations) and triggers Migration Jobs.
/// </summary>
[ApiController]
[Route("api/mappings")]
[Produces("application/json")]
public sealed class ProjectMappingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectMappingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ─────────────────────────────────────────────────────────────────
    // POST /api/mappings
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new Project Mapping (migration configuration).
    /// </summary>
    /// <remarks>
    /// Validates that both SourceProfileId and DestProfileId exist in the database before persisting.
    /// The FilterConfig object is stored as a JSON string internally but returned as a typed object.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectMappingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMapping(
        [FromBody] CreateProjectMappingRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new CreateProjectMappingCommand(
            request.UserId,
            request.Name,
            request.SourceProfileId,
            request.DestProfileId,
            request.SourcePath,
            request.DestPath,
            request.FilterConfig ?? new FilterConfig(),
            request.ConflictResolutionRule
        );

        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(
                nameof(GetMappings),
                new { userId = result.UserId },
                result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // GET /api/mappings?userId={userId}
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all Project Mappings belonging to the specified user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectMappingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMappings(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectMappingsQuery(userId);
        var mappings = await _mediator.Send(query, cancellationToken);
        return Ok(mappings);
    }

    // ─────────────────────────────────────────────────────────────────
    // POST /api/mappings/{id}/start
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts a migration job for the given mapping.
    /// </summary>
    /// <remarks>
    /// This endpoint:
    /// 1. Creates a new MigrationJob row in the database with status <c>Queued</c>.
    /// 2. Publishes a <c>MigrationJobStartedEvent</c> to RabbitMQ so the Worker Service begins the transfer.
    /// </remarks>
    /// <param name="id">The ProjectMapping ID to run the migration for.</param>
    /// <param name="request">Optional job parameters (e.g. JobType). Omit for a full migration.</param>
    /// <param name="cancellationToken"></param>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(MigrationJobDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartMigration(
        Guid id,
        [FromBody] StartMigrationJobRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new StartMigrationJobCommand(
            MappingId: id,
            JobType: request?.JobType ?? Domain.Enums.JobType.Full
        );

        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            // 202 Accepted — the job has been queued and is being processed asynchronously
            return Accepted(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Local request record
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Optional body for the start-migration endpoint.</summary>
public sealed record StartMigrationJobRequest(
    CloudShift.Domain.Enums.JobType JobType = CloudShift.Domain.Enums.JobType.Full
);
