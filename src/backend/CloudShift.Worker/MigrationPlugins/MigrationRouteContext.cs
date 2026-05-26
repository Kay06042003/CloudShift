using CloudShift.Application.ProjectMappings.Models;
using CloudShift.Domain.Entities;

namespace CloudShift.Worker.MigrationPlugins;

public sealed record MigrationRouteContext(
    MigrationJob Job,
    ProjectMapping Mapping,
    AppProfile SourceProfile,
    AppProfile DestinationProfile,
    FilterConfig FilterConfig);
