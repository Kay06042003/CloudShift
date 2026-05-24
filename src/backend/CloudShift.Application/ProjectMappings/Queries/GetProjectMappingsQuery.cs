using CloudShift.Application.ProjectMappings.DTOs;
using MediatR;

namespace CloudShift.Application.ProjectMappings.Queries;

/// <summary>
/// CQRS Query: Returns all Project Mappings belonging to the specified user.
/// </summary>
public sealed record GetProjectMappingsQuery(Guid UserId) : IRequest<IReadOnlyList<ProjectMappingDto>>;
