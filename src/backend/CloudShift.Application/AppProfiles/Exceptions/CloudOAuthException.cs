using System.Net;

namespace CloudShift.Application.AppProfiles.Exceptions;

public sealed class CloudOAuthException : Exception
{
    public CloudOAuthException(
        string message,
        HttpStatusCode statusCode,
        string? providerError,
        string? providerErrorDescription,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ProviderError = providerError;
        ProviderErrorDescription = providerErrorDescription;
    }

    public HttpStatusCode StatusCode { get; }
    public string? ProviderError { get; }
    public string? ProviderErrorDescription { get; }
}
