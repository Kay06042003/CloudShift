using CloudShift.Application.OAuthProviderApps.DTOs;
using MediatR;

namespace CloudShift.Application.OAuthProviderApps.Commands;

public sealed record DeleteOAuthProviderAppCommand(Guid Id, Guid UserId) : IRequest<DeleteOAuthProviderAppResult>;
