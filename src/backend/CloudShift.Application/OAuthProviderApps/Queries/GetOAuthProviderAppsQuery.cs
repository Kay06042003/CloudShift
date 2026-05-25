using CloudShift.Application.OAuthProviderApps.DTOs;
using MediatR;

namespace CloudShift.Application.OAuthProviderApps.Queries;

public sealed record GetOAuthProviderAppsQuery(Guid UserId) : IRequest<IReadOnlyList<OAuthProviderAppDto>>;
