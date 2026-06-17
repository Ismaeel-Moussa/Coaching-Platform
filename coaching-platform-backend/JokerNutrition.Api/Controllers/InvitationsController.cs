using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.Forms.Invitations;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/invitations")]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class InvitationsController : ControllerBase
{
    private readonly IInvitationService _invitationService;

    public InvitationsController(IInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    /// <summary>List all invitations (paginated). Coach sees own; Admin sees all.</summary>
    [HttpGet]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> List([FromQuery] BasePaginationForm pagination)
    {
        var result = await _invitationService.ListInvitationsAsync(pagination);
        return Ok(result);
    }

    /// <summary>Create and send a new invitation.</summary>
    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateInvitationForm form)
    {
        var result = await _invitationService.CreateInvitationAsync(form);
        return Created($"/api/invitations/{result.Id}", result);
    }

    /// <summary>Resend an existing invitation (regenerates token + extends expiry).</summary>
    [HttpPost("resend/{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Resend(int id)
    {
        var result = await _invitationService.ResendInvitationAsync(id);
        return Ok(result);
    }

    /// <summary>Revoke an invitation.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Revoke(int id)
    {
        await _invitationService.RevokeInvitationAsync(id);
        return NoContent();
    }

    /// <summary>Validate an invitation token (public — used by registration form).</summary>
    [HttpGet("validate/{token}")]
    public async Task<IActionResult> Validate(string token)
    {
        var result = await _invitationService.ValidateTokenAsync(token);
        return Ok(result);
    }
}
