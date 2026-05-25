using CloudShift.Application.AppProfiles.DTOs;
using CloudShift.Domain.Entities;

namespace CloudShift.Application.AppProfiles.Interfaces;

public interface ICloudOAuthService
{
    string BuildAuthorizationUrl(OAuthProviderApp providerApp, string state);

    Task<CloudOAuthTokenResult> ExchangeCodeAsync(
        OAuthProviderApp providerApp,
        string code,
        CancellationToken cancellationToken = default);
}
