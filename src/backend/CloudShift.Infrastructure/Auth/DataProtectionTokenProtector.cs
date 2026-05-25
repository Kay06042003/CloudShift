using CloudShift.Application.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace CloudShift.Infrastructure.Auth;

public sealed class DataProtectionTokenProtector : ITokenProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionTokenProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("CloudShift.OAuthTokens.v1");
    }

    public string Protect(string plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
        {
            return string.Empty;
        }

        return _protector.Protect(plaintext);
    }

    public string Unprotect(string ciphertext)
    {
        if (string.IsNullOrWhiteSpace(ciphertext))
        {
            return string.Empty;
        }

        return _protector.Unprotect(ciphertext);
    }
}
