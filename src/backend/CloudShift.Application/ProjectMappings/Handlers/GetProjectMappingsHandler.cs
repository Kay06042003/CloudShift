using CloudShift.Application.ProjectMappings.DTOs;
using CloudShift.Application.ProjectMappings.Interfaces;
using CloudShift.Application.ProjectMappings.Queries;
using MediatR;

namespace CloudShift.Application.ProjectMappings.Handlers;

/// <summary>
/// Handles <see cref="GetProjectMappingsQuery"/>.
/// Returns all mappings for the given user with deserialized <c>FilterConfig</c>.
/// </summary>
public sealed class GetProjectMappingsHandler : IRequestHandler<GetProjectMappingsQuery, IReadOnlyList<ProjectMappingDto>>
{
    private readonly IProjectMappingRepository _repository;

    public GetProjectMappingsHandler(IProjectMappingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProjectMappingDto>> Handle(
        GetProjectMappingsQuery request,
        CancellationToken cancellationToken)
    {
        var mappings = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        return mappings
            .Select(m => CreateProjectMappingHandler.MapToDto(m))
            .ToList()
            .AsReadOnly();
    }
}
