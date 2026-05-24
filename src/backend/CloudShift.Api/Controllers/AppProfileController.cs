using CloudShift.Application.AppProfiles.Commands;
using CloudShift.Application.AppProfiles.DTOs;
using CloudShift.Application.AppProfiles.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CloudShift.Api.Controllers;

/// <summary>
/// Manages App Profiles (connected cloud storage accounts).
/// </summary>
[ApiController]
[Route("api/app-profiles")]
[Produces("application/json")]
public sealed class AppProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ─────────────────────────────────────────────────────────────────
    // POST /api/app-profiles
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a new App Profile for a user.
    /// The OAuth flow is handled on the frontend; supply the resulting tokens here.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAppProfile(
        [FromBody] AddAppProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new AddAppProfileCommand(
            request.UserId,
            request.Provider,
            request.Email,
            request.AccessToken,
            request.RefreshToken,
            request.ExpiresAt
        );

        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetAppProfiles),
            new { userId = result.UserId },
            result);
    }

    // ─────────────────────────────────────────────────────────────────
    // GET /api/app-profiles?userId={userId}
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all App Profiles for the specified user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AppProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppProfiles(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetAppProfilesQuery(userId);
        var profiles = await _mediator.Send(query, cancellationToken);
        return Ok(profiles);
    }

    // ─────────────────────────────────────────────────────────────────
    // POST /api/app-profiles/{id}/test-connection
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Performs a mock connection health check for the specified App Profile.
    /// Always returns healthy (true) — replace with real OAuth token validation later.
    /// </summary>
    [HttpPost("{id:guid}/test-connection")]
    [ProducesResponseType(typeof(ConnectionTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult TestConnection(Guid id)
    {
        // TODO: Replace with real token introspection / API ping in a future sprint.
        return Ok(new ConnectionTestResult(
            ProfileId: id,
            IsHealthy: true,
            Message: "Connection successful (mocked)",
            TestedAt: DateTime.UtcNow
        ));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Local response records (small enough to live next to the controller)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Result of a connection health check.</summary>
public sealed record ConnectionTestResult(
    Guid ProfileId,
    bool IsHealthy,
    string Message,
    DateTime TestedAt
);
