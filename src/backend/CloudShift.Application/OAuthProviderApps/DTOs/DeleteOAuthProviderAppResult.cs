namespace CloudShift.Application.OAuthProviderApps.DTOs;

public sealed record DeleteOAuthProviderAppResult(
    Guid Id,
    Guid UserId,
    bool Deleted,
    bool Deactivated,
    int LinkedProfileCount,
    string Message
);
