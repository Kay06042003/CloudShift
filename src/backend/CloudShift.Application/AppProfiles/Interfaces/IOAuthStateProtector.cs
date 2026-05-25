using CloudShift.Domain.Enums;

namespace CloudShift.Application.AppProfiles.Interfaces;

public interface IOAuthStateProtector
{
    string Protect(Guid userId, Guid providerAppId, ProviderType provider);

    OAuthState Unprotect(string protectedState);
}

public sealed record OAuthState(
    Guid UserId,
    Guid ProviderAppId,
    ProviderType Provider,
    DateTimeOffset IssuedAt);
