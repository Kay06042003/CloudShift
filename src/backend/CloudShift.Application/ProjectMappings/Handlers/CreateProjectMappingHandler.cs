using System.Text.Json;
using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Application.ProjectMappings.Commands;
using CloudShift.Application.ProjectMappings.DTOs;
using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Application.ProjectMappings.Models;
using CloudShift.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CloudShift.Application.ProjectMappings.Handlers;

public sealed class CreateProjectMappingHandler : IRequestHandler<CreateProjectMappingCommand, ProjectMappingDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IProjectMappingRepository _mappingRepository;
    private readonly IAppProfileRepository _profileRepository;
    private readonly IProviderRoutePolicy _routePolicy;
    private readonly ILogger<CreateProjectMappingHandler> _logger;

    public CreateProjectMappingHandler(
        IProjectMappingRepository mappingRepository,
        IAppProfileRepository profileRepository,
        IProviderRoutePolicy routePolicy,
        ILogger<CreateProjectMappingHandler> logger)
    {
        _mappingRepository = mappingRepository;
        _profileRepository = profileRepository;
        _routePolicy = routePolicy;
        _logger = logger;
    }

    public async Task<ProjectMappingDto> Handle(
        CreateProjectMappingCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating project mapping. UserId: {UserId}, SourceProfileId: {SourceProfileId}, DestProfileId: {DestProfileId}",
            command.UserId,
            command.SourceProfileId,
            command.DestProfileId);

        var sourceProfile = await _profileRepository.GetByIdAsync(command.SourceProfileId, cancellationToken)
            ?? throw new KeyNotFoundException($"Source AppProfile '{command.SourceProfileId}' was not found.");

        var destProfile = await _profileRepository.GetByIdAsync(command.DestProfileId, cancellationToken)
            ?? throw new KeyNotFoundException($"Destination AppProfile '{command.DestProfileId}' was not found.");

        if (sourceProfile.UserId != command.UserId || destProfile.UserId != command.UserId)
        {
            _logger.LogWarning(
                "Rejected project mapping because one or more profiles are not owned by the user. UserId: {UserId}, SourceProfileOwnerId: {SourceProfileOwnerId}, DestProfileOwnerId: {DestProfileOwnerId}",
                command.UserId,
                sourceProfile.UserId,
                destProfile.UserId);

            throw new UnauthorizedAccessException("Both source and destination profiles must be owned by the mapping user.");
        }

        if (sourceProfile.Id == destProfile.Id)
        {
            throw new InvalidOperationException("Source and destination profiles must be different.");
        }

        if (!IsSupportedConflictRule(command.ConflictResolutionRule))
        {
            throw new InvalidOperationException("ConflictResolutionRule must be Skip, Overwrite, or Rename.");
        }

        if (!_routePolicy.CanMigrate(sourceProfile.Provider, destProfile.Provider))
        {
            _logger.LogWarning(
                "Rejected project mapping because provider route is unsupported. UserId: {UserId}, SourceProvider: {SourceProvider}, DestinationProvider: {DestinationProvider}",
                command.UserId,
                sourceProfile.Provider,
                destProfile.Provider);

            throw new InvalidOperationException(
                $"Migration route '{sourceProfile.Provider}' to '{destProfile.Provider}' is not supported.");
        }

        var mapping = new ProjectMapping
        {
            UserId = command.UserId,
            Name = command.Name,
            SourceProfileId = command.SourceProfileId,
            DestProfileId = command.DestProfileId,
            SourcePath = command.SourcePath,
            DestPath = command.DestPath,
            FilterConfigJson = JsonSerializer.Serialize(command.FilterConfig, JsonOptions),
            ConflictResolutionRule = command.ConflictResolutionRule,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var saved = await _mappingRepository.AddAsync(mapping, cancellationToken);
        saved.SourceProfile = sourceProfile;
        saved.DestProfile = destProfile;

        _logger.LogInformation(
            "Created project mapping. MappingId: {MappingId}, UserId: {UserId}, SourceProvider: {SourceProvider}, DestinationProvider: {DestinationProvider}",
            saved.Id,
            saved.UserId,
            sourceProfile.Provider,
            destProfile.Provider);

        return MapToDto(saved, command.FilterConfig);
    }

    internal static ProjectMappingDto MapToDto(ProjectMapping mapping, FilterConfig? filterConfig = null)
    {
        var config = filterConfig
            ?? JsonSerializer.Deserialize<FilterConfig>(mapping.FilterConfigJson, JsonOptions)
            ?? new FilterConfig();

        return new ProjectMappingDto(
            mapping.Id,
            mapping.UserId,
            mapping.Name,
            mapping.SourceProfileId,
            mapping.SourceProfile?.Provider.ToString() ?? string.Empty,
            mapping.DestProfileId,
            mapping.DestProfile?.Provider.ToString() ?? string.Empty,
            mapping.SourcePath,
            mapping.DestPath,
            config,
            mapping.ConflictResolutionRule,
            mapping.CreatedAt,
            mapping.UpdatedAt);
    }

    private static bool IsSupportedConflictRule(string conflictResolutionRule)
    {
        return conflictResolutionRule.Equals("Skip", StringComparison.OrdinalIgnoreCase)
            || conflictResolutionRule.Equals("Overwrite", StringComparison.OrdinalIgnoreCase)
            || conflictResolutionRule.Equals("Rename", StringComparison.OrdinalIgnoreCase);
    }
}
