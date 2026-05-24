using CloudShift.Application.ProjectMappings.DTOs;
using CloudShift.Application.ProjectMappings.Models;
using MediatR;

namespace CloudShift.Application.ProjectMappings.Commands;

/// <summary>
/// CQRS Command: Creates a new <c>ProjectMapping</c> in the system.
/// Returns the fully hydrated <see cref="ProjectMappingDto"/>.
/// </summary>
public sealed record CreateProjectMappingCommand(
    Guid UserId,
    string Name,
    Guid SourceProfileId,
    Guid DestProfileId,
    string SourcePath,
    string DestPath,
    FilterConfig FilterConfig,
    string ConflictResolutionRule
) : IRequest<ProjectMappingDto>;
