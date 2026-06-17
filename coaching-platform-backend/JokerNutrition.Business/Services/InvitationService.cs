using System.Security.Principal;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.Configurations;
using JokerNutrition.Business.DTOs.Invitations;
using JokerNutrition.Data.Enums;
using JokerNutrition.Business.Forms.Invitations;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JokerNutrition.Business.Services;

public interface IInvitationService
{
    Task<InvitationDto> CreateInvitationAsync(CreateInvitationForm form);
    Task<PagedResult<InvitationDto>> ListInvitationsAsync(BasePaginationForm pagination);
    Task<InvitationDto> ResendInvitationAsync(int id);
    Task RevokeInvitationAsync(int id);
    Task<InvitationDto> ValidateTokenAsync(string token);
}

public class InvitationService : _BaseService, IInvitationService
{
    private readonly IInvitationRepository _invitationRepo;
    private readonly ICoachRepository _coachRepo;
    private readonly IEmailService _emailService;
    private readonly SmtpSettings _smtpSettings;

    public InvitationService(
        IPrincipal principal,
        ILogger<InvitationService> logger,
        IInvitationRepository invitationRepo,
        ICoachRepository coachRepo,
        IEmailService emailService,
        IOptions<SmtpSettings> smtpSettings)
        : base(principal, logger)
    {
        _invitationRepo = invitationRepo;
        _coachRepo = coachRepo;
        _emailService = emailService;
        _smtpSettings = smtpSettings.Value;
    }

    public async Task<InvitationDto> CreateInvitationAsync(CreateInvitationForm form)
    {
        var validRoles = new[] { "Athlete", "Coach", "Admin" };
        if (!validRoles.Contains(form.Role))
            throw new ArgumentException($"Invalid role. Must be one of: {string.Join(", ", validRoles)}");

        // Get the coach profile of the logged-in user
        var coach = await _coachRepo.Query()
            .FirstOrDefaultAsync(c => c.UserId == LoggedInUser.Id)
            ?? throw new InvalidOperationException("Coach profile not found.");

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var invitation = new Invitation
        {
            Email = form.Email,
            Token = token,
            Role = form.Role,
            Status = InvitationStatus.Pending,
            IssuedByCoachId = coach.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(form.ExpiryHours),
            CreatedAt = DateTime.UtcNow
        };

        await _invitationRepo.CreateAsync(invitation);
        await _invitationRepo.SaveChangesAsync();

        var inviteUrl = $"{_smtpSettings.SignUpBaseUrl}?token={token}";
        await _emailService.SendInvitationEmailAsync(form.Email, inviteUrl, form.Role);

        _logger.LogInformation("Invitation created for {Email} as {Role}", form.Email, form.Role);

        return InvitationMapper.ToDto(invitation, _smtpSettings);
    }

    public async Task<PagedResult<InvitationDto>> ListInvitationsAsync(BasePaginationForm pagination)
    {
        // Admins see all; coaches see only their own
        var query = LoggedInUser.Role == "Admin"
            ? _invitationRepo.QueryAll()
            : _invitationRepo.QueryAll().Where(i =>
                _coachRepo.Query()
                    .Where(c => c.UserId == LoggedInUser.Id)
                    .Select(c => c.Id)
                    .Contains(i.IssuedByCoachId));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<InvitationDto>
        {
            Items = items.Select(i => InvitationMapper.ToDto(i, _smtpSettings)),
            TotalCount = total,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<InvitationDto> ResendInvitationAsync(int id)
    {
        var invitation = await _invitationRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Invitation not found.");

        // Regenerate token and extend expiry
        invitation.Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        invitation.Status = InvitationStatus.Pending;
        invitation.ExpiresAt = DateTime.UtcNow.AddHours(72);

        _invitationRepo.Update(invitation);
        await _invitationRepo.SaveChangesAsync();

        var inviteUrl = $"{_smtpSettings.SignUpBaseUrl}?token={invitation.Token}";
        await _emailService.SendInvitationEmailAsync(invitation.Email, inviteUrl, invitation.Role);

        return InvitationMapper.ToDto(invitation, _smtpSettings);
    }

    public async Task RevokeInvitationAsync(int id)
    {
        var invitation = await _invitationRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Invitation not found.");

        invitation.Status = InvitationStatus.Revoked;
        _invitationRepo.Update(invitation);
        await _invitationRepo.SaveChangesAsync();
    }

    public async Task<InvitationDto> ValidateTokenAsync(string token)
    {
        var invitation = await _invitationRepo.Query()
            .FirstOrDefaultAsync(i => i.Token == token)
            ?? throw new KeyNotFoundException("Invitation not found.");

        // Check if expired in the DB
        if (invitation.ExpiresAt < DateTime.UtcNow && invitation.Status == InvitationStatus.Pending)
        {
            invitation.Status = InvitationStatus.Expired;
            _invitationRepo.Update(invitation);
            await _invitationRepo.SaveChangesAsync();
        }

        return InvitationMapper.ToDto(invitation, _smtpSettings);
    }
}
