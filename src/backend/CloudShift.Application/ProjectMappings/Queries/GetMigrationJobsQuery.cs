using CloudShift.Application.ProjectMappings.DTOs;
using MediatR;

namespace CloudShift.Application.ProjectMappings.Queries;

/// <summary>
/// CQRS Query: Returns migration jobs for a user.
/// </summary>
public sealed record GetMigrationJobsQuery(Guid UserId) : IRequest<IReadOnlyList<MigrationJobDto>>;
