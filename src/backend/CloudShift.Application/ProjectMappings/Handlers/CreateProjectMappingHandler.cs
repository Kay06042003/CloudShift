using System.Text.Json;
using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Application.ProjectMappings.Commands;
using CloudShift.Application.ProjectMappings.DTOs;
using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Application.ProjectMappings.Models;
using CloudShift.Domain.Entities;
using MediatR;

namespace CloudShift.Application.ProjectMappings.Handlers;

/// <summary>
/// Handles <see cref="CreateProjectMappingCommand"/>.
/// Validates that both profiles exist, serializes <see cref="FilterConfig"/> to JSON,
/// persists the new mapping, and returns the hydrated DTO.
/// </summary>
public sealed class CreateProjectMappingHandler : IRequestHandler<CreateProjectMappingCommand, ProjectMappingDto>
{
    private readonly IProjectMappingRepository _mappingRepository;
    private readonly IAppProfileRepository _profileRepository;

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public CreateProjectMappingHandler(
        IProjectMappingRepository mappingRepository,
        IAppProfileRepository profileRepository)
    {
        _mappingRepository = mappingRepository;
        _profileRepository = profileRepository;
    }

    public async Task<ProjectMappingDto> Handle(
        CreateProjectMappingCommand command,
        CancellationToken cancellationToken)
    {
        // ── Validation: ensure both profiles exist ────────────────────────────
        var sourceProfile = await _profileRepository.GetByIdAsync(command.SourceProfileId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Source AppProfile '{command.SourceProfileId}' was not found.");

        var destProfile = await _profileRepository.GetByIdAsync(command.DestProfileId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Destination AppProfile '{command.DestProfileId}' was not found.");

        // ── Serialize FilterConfig → JSON string ──────────────────────────────
        var filterConfigJson = JsonSerializer.Serialize(command.FilterConfig, _jsonOptions);

        // ── Build entity ──────────────────────────────────────────────────────
        var mapping = new ProjectMapping
        {
            UserId                = command.UserId,
            Name                  = command.Name,
            SourceProfileId       = command.SourceProfileId,
            DestProfileId         = command.DestProfileId,
            SourcePath            = command.SourcePath,
            DestPath              = command.DestPath,
            FilterConfigJson      = filterConfigJson,
            ConflictResolutionRule = command.ConflictResolutionRule,
            CreatedAt             = DateTime.UtcNow,
            UpdatedAt             = DateTime.UtcNow
        };

        var saved = await _mappingRepository.AddAsync(mapping, cancellationToken);

        // Attach the navigation properties loaded above for DTO mapping
        saved.SourceProfile = sourceProfile;
        saved.DestProfile   = destProfile;

        return MapToDto(saved, command.FilterConfig);
    }

    // ─── Mapping helpers ──────────────────────────────────────────────────────

    internal static ProjectMappingDto MapToDto(ProjectMapping m, FilterConfig? filterConfig = null)
    {
        var config = filterConfig
            ?? JsonSerializer.Deserialize<FilterConfig>(m.FilterConfigJson, _jsonOptions)
            ?? new FilterConfig();

        return new ProjectMappingDto(
            m.Id,
            m.UserId,
            m.Name,
            m.SourceProfileId,
            m.SourceProfile?.Provider.ToString() ?? string.Empty,
            m.DestProfileId,
            m.DestProfile?.Provider.ToString() ?? string.Empty,
            m.SourcePath,
            m.DestPath,
            config,
            m.ConflictResolutionRule,
            m.CreatedAt,
            m.UpdatedAt
        );
    }
}
