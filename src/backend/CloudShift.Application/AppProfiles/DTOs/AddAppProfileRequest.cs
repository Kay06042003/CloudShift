using CloudShift.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CloudShift.Application.AppProfiles.DTOs;

/// <summary>
/// Request payload for adding a new App Profile.
/// The frontend is responsible for completing the OAuth flow and 
/// providing the resulting tokens (mocked for now).
/// </summary>
public sealed record AddAppProfileRequest(
    [Required] Guid UserId,
    [Required] ProviderType Provider,
    [Required, EmailAddress] string Email,
    [Required] string AccessToken,
    [Required] string RefreshToken,
    [Required] DateTime ExpiresAt
);
