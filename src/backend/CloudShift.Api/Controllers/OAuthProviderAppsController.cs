using CloudShift.Application.OAuthProviderApps.Commands;
using CloudShift.Application.OAuthProviderApps.DTOs;
using CloudShift.Application.OAuthProviderApps.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CloudShift.Api.Controllers;

[ApiController]
[Route("api/oauth-provider-apps")]
[Produces("application/json")]
public sealed class OAuthProviderAppsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OAuthProviderAppsController> _logger;

    public OAuthProviderAppsController(
        IMediator mediator,
        ILogger<OAuthProviderAppsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OAuthProviderAppDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOAuthProviderAppRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new CreateOAuthProviderAppCommand(
            request.UserId,
            request.Provider,
            request.Name,
            request.ClientId,
            request.ClientSecret,
            request.TenantId,
            request.RedirectUri,
            request.Scopes);

        var result = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation(
            "Created OAuth provider app. ProviderAppId: {ProviderAppId}, UserId: {UserId}, Provider: {Provider}",
            result.Id,
            result.UserId,
            result.Provider);

        return CreatedAtAction(nameof(Get), new { userId = result.UserId }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OAuthProviderAppDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateOAuthProviderAppRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _mediator.Send(
                new UpdateOAuthProviderAppCommand(
                    id,
                    request.UserId,
                    request.Provider,
                    request.Name,
                    request.ClientId,
                    request.ClientSecret,
                    request.TenantId,
                    request.RedirectUri,
                    request.Scopes,
                    request.IsActive),
                cancellationToken);

            _logger.LogInformation(
                "Updated OAuth provider app. ProviderAppId: {ProviderAppId}, UserId: {UserId}, Provider: {Provider}",
                result.Id,
                result.UserId,
                result.Provider);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "OAuth provider app update failed because the app was not found. ProviderAppId: {ProviderAppId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "OAuth provider app update failed because the user is not authorized. ProviderAppId: {ProviderAppId}, UserId: {UserId}", id, request.UserId);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "OAuth provider app update failed validation. ProviderAppId: {ProviderAppId}, UserId: {UserId}", id, request.UserId);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(DeleteOAuthProviderAppResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new DeleteOAuthProviderAppCommand(id, userId), cancellationToken);

            _logger.LogInformation(
                "Processed OAuth provider app delete. ProviderAppId: {ProviderAppId}, UserId: {UserId}, Deleted: {Deleted}, Deactivated: {Deactivated}, LinkedProfileCount: {LinkedProfileCount}",
                result.Id,
                result.UserId,
                result.Deleted,
                result.Deactivated,
                result.LinkedProfileCount);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "OAuth provider app delete failed because the app was not found. ProviderAppId: {ProviderAppId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "OAuth provider app delete failed because the user is not authorized. ProviderAppId: {ProviderAppId}, UserId: {UserId}", id, userId);
            return Forbid();
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OAuthProviderAppDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOAuthProviderAppsQuery(userId), cancellationToken);
        return Ok(result);
    }
}
