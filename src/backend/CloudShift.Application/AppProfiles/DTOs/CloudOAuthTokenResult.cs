namespace CloudShift.Application.AppProfiles.DTOs;

public sealed record CloudOAuthTokenResult(
    string ExternalAccountId,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string GrantedScopes
);
