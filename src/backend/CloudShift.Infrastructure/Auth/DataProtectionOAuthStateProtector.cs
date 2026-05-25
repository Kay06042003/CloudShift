using System.Text.Json;
using CloudShift.Application.AppProfiles.Interfaces;
using CloudShift.Domain.Enums;
using Microsoft.AspNetCore.DataProtection;

namespace CloudShift.Infrastructure.Auth;

public sealed class DataProtectionOAuthStateProtector : IOAuthStateProtector
{
    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(15);
    private readonly IDataProtector _protector;

    public DataProtectionOAuthStateProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("CloudShift.OAuthState.v1");
    }

    public string Protect(Guid userId, Guid providerAppId, ProviderType provider)
    {
        var state = new OAuthState(userId, providerAppId, provider, DateTimeOffset.UtcNow);
        return _protector.Protect(JsonSerializer.Serialize(state));
    }

    public OAuthState Unprotect(string protectedState)
    {
        var json = _protector.Unprotect(protectedState);
        var state = JsonSerializer.Deserialize<OAuthState>(json)
            ?? throw new InvalidOperationException("OAuth state payload is invalid.");

        if (DateTimeOffset.UtcNow - state.IssuedAt > MaxAge)
        {
            throw new InvalidOperationException("OAuth state has expired.");
        }

        return state;
    }
}
