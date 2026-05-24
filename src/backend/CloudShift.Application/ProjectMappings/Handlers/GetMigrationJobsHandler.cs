using CloudShift.Application.ProjectMappings.DTOs;
using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Application.ProjectMappings.Queries;
using MediatR;

namespace CloudShift.Application.ProjectMappings.Handlers;

/// <summary>
/// Handles <see cref="GetMigrationJobsQuery"/>.
/// </summary>
public sealed class GetMigrationJobsHandler : IRequestHandler<GetMigrationJobsQuery, IReadOnlyList<MigrationJobDto>>
{
    private readonly IMigrationJobRepository _repository;

    public GetMigrationJobsHandler(IMigrationJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<MigrationJobDto>> Handle(
        GetMigrationJobsQuery request,
        CancellationToken cancellationToken)
    {
        var jobs = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        return jobs
            .Select(j => new MigrationJobDto(
                j.Id,
                j.ProjectMappingId,
                j.Status,
                j.Status.ToString(),
                j.JobType,
                j.JobType.ToString(),
                j.TotalItems,
                j.ProcessedItems,
                j.StartedAt,
                j.CompletedAt,
                j.CreatedAt))
            .ToList()
            .AsReadOnly();
    }
}
