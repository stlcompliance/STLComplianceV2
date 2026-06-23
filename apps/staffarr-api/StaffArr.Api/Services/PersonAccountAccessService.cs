using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PersonAccountAccessService(
    StaffArrDbContext db,
    NexArrPlatformIdentityClient nexArrPlatformIdentityClient,
    NexArrLoginEnableClient nexArrLoginEnableClient,
    NexArrLoginDisableClient nexArrLoginDisableClient,
    IStaffArrAuditService audit)
{
    public async Task<PersonAccountAccessSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var person = await LoadPersonAsync(tenantId, personId, cancellationToken);
        return await BuildSummaryAsync(person, cancellationToken);
    }

    public async Task<PersonAccountAccessActionResponse> ProvisionAsync(
        Guid tenantId,
        Guid personId,
        Guid? actorUserId,
        ProvisionPersonAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var person = await LoadPersonAsync(tenantId, personId, cancellationToken);
        var loginEmail = NormalizeEmail(request.LoginEmail, "Login email");
        if (string.IsNullOrWhiteSpace(request.TemporaryPassword))
        {
            throw new StlApiException(
                "people.validation",
                "A temporary sign-in password is required to provision login access.",
                400);
        }

        if (person.ExternalUserId is Guid existingExternalUserId)
        {
            var currentIdentity = await nexArrPlatformIdentityClient.GetIdentityAsync(
                tenantId,
                existingExternalUserId,
                cancellationToken);
            if (currentIdentity.CanLogin)
            {
                throw new StlApiException(
                    "people.account_exists",
                    "This person already has a linked platform login.",
                    409);
            }

            if (!string.Equals(currentIdentity.Email, loginEmail, StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException(
                    "people.account_login_email_mismatch",
                    "Update the pending login email before completing sign-in setup.",
                    409);
            }
        }

        if (request.SyncWorkEmail)
        {
            await EnsurePrimaryEmailAvailableAsync(tenantId, loginEmail, person.Id, cancellationToken);
            person.PrimaryEmail = loginEmail;
        }

        var identity = await nexArrPlatformIdentityClient.CreateIdentityAsync(
            tenantId,
            loginEmail,
            person.DisplayName,
            request.TemporaryPassword,
            actorUserId,
            cancellationToken);

        person.ExternalUserId = identity.ExternalUserId;
        ApplyPlatformIdentitySnapshot(person, identity);
        person.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "person.account_provision",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        var summary = await BuildSummaryAsync(person, cancellationToken);
        return new PersonAccountAccessActionResponse(summary, "Platform login was provisioned for this person.");
    }

    public async Task<PersonAccountAccessActionResponse> UpdateLoginEmailAsync(
        Guid tenantId,
        Guid personId,
        Guid? actorUserId,
        UpdatePersonLoginEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var person = await LoadPersonAsync(tenantId, personId, cancellationToken);
        var externalUserId = person.ExternalUserId
            ?? throw new StlApiException(
                "people.account_not_found",
                "This person does not have a linked platform account yet.",
                409);

        var currentIdentity = await nexArrPlatformIdentityClient.GetIdentityAsync(
            tenantId,
            externalUserId,
            cancellationToken);
        var loginEmail = NormalizeEmail(request.LoginEmail, "Login email");

        if (request.SyncWorkEmail)
        {
            await EnsurePrimaryEmailAvailableAsync(tenantId, loginEmail, person.Id, cancellationToken);
            person.PrimaryEmail = loginEmail;
        }

        var identity = await nexArrPlatformIdentityClient.SyncIdentityAsync(
            tenantId,
            externalUserId,
            loginEmail,
            person.DisplayName,
            currentIdentity.MembershipRoleKey,
            actorUserId,
            cancellationToken);

        ApplyPlatformIdentitySnapshot(person, identity);
        person.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "person.account_login_email_update",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        var summary = await BuildSummaryAsync(person, cancellationToken);
        return new PersonAccountAccessActionResponse(summary, "Login email was updated.");
    }

    public async Task<PersonAccountAccessActionResponse> RequestPasswordResetAsync(
        Guid tenantId,
        Guid personId,
        Guid? actorUserId,
        PersonAccountActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var person = await LoadPersonAsync(tenantId, personId, cancellationToken);
        var externalUserId = RequireExternalUserId(person);

        var message = await nexArrPlatformIdentityClient.RequestPasswordResetAsync(
            tenantId,
            personId,
            externalUserId,
            actorUserId,
            request.Reason,
            cancellationToken);

        await audit.WriteAsync(
            "person.account_password_reset",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            "success",
            reasonCode: string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            cancellationToken: cancellationToken);

        var summary = await BuildSummaryAsync(person, cancellationToken);
        return new PersonAccountAccessActionResponse(summary, message);
    }

    public async Task<PersonAccountAccessActionResponse> ResetMfaAsync(
        Guid tenantId,
        Guid personId,
        Guid? actorUserId,
        PersonAccountActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var person = await LoadPersonAsync(tenantId, personId, cancellationToken);
        var externalUserId = RequireExternalUserId(person);

        var wasMfaEnabled = await nexArrPlatformIdentityClient.ResetMfaAsync(
            tenantId,
            personId,
            externalUserId,
            actorUserId,
            request.Reason,
            cancellationToken);

        await audit.WriteAsync(
            "person.account_mfa_reset",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            "success",
            reasonCode: string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            cancellationToken: cancellationToken);

        var summary = await BuildSummaryAsync(person, cancellationToken);
        var message = wasMfaEnabled
            ? "Multi-factor authentication was reset and active sessions were revoked."
            : "Multi-factor authentication was already cleared for this account.";
        return new PersonAccountAccessActionResponse(summary, message);
    }

    public async Task<PersonAccountAccessActionResponse> DisableLoginAsync(
        Guid tenantId,
        Guid personId,
        Guid? actorUserId,
        PersonAccountActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var person = await LoadPersonAsync(tenantId, personId, cancellationToken);
        var result = await nexArrLoginDisableClient.TryRequestLoginDisableAsync(
            tenantId,
            personId,
            person.ExternalUserId,
            actorUserId,
            string.IsNullOrWhiteSpace(request.Reason) ? "Login access disabled from StaffArr." : request.Reason.Trim(),
            cancellationToken);

        if (!string.Equals(result.Outcome, "requested", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "people.account_disable_failed",
                "Login access could not be disabled right now.",
                502);
        }

        await RefreshPersonLoginSnapshotAsync(tenantId, person, cancellationToken);

        await audit.WriteAsync(
            "person.account_login_disable",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            "success",
            reasonCode: string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            cancellationToken: cancellationToken);

        var summary = await BuildSummaryAsync(person, cancellationToken);
        return new PersonAccountAccessActionResponse(summary, "Login access was disabled.");
    }

    public async Task<PersonAccountAccessActionResponse> EnableLoginAsync(
        Guid tenantId,
        Guid personId,
        Guid? actorUserId,
        PersonAccountActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var person = await LoadPersonAsync(tenantId, personId, cancellationToken);
        var externalUserId = RequireExternalUserId(person);

        var result = await nexArrLoginEnableClient.RequestLoginEnableAsync(
            tenantId,
            personId,
            externalUserId,
            actorUserId,
            request.Reason,
            cancellationToken);

        if (!string.Equals(result.Outcome, "requested", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "people.account_enable_failed",
                "Login access could not be re-enabled right now.",
                502);
        }

        await RefreshPersonLoginSnapshotAsync(tenantId, person, cancellationToken);

        await audit.WriteAsync(
            "person.account_login_enable",
            tenantId,
            actorUserId,
            "person",
            person.Id.ToString(),
            "success",
            reasonCode: string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            cancellationToken: cancellationToken);

        var summary = await BuildSummaryAsync(person, cancellationToken);
        return new PersonAccountAccessActionResponse(summary, "Login access was re-enabled.");
    }

    private async Task<StaffPerson> LoadPersonAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        return await db.People.FirstOrDefaultAsync(
                   p => p.TenantId == tenantId && p.Id == personId,
                   cancellationToken)
               ?? throw new StlApiException("people.not_found", "Person was not found.", 404);
    }

    private async Task<PersonAccountAccessSummaryResponse> BuildSummaryAsync(
        StaffPerson person,
        CancellationToken cancellationToken)
    {
        if (person.ExternalUserId is not Guid externalUserId)
        {
            return new PersonAccountAccessSummaryResponse(
                person.Id,
                person.PrimaryEmail,
                false,
                false,
                "no_platform_login",
                null,
                false,
                false,
                false,
                false,
                false,
                null,
                null,
                null,
                nexArrPlatformIdentityClient.IsConfigured,
                nexArrPlatformIdentityClient.IsConfigured
                    ? "This person does not currently have platform login access."
                    : "NexArr account access is not configured in this environment.");
        }

        if (!nexArrPlatformIdentityClient.IsConfigured)
        {
            return new PersonAccountAccessSummaryResponse(
                person.Id,
                person.PrimaryEmail,
                true,
                person.CanLoginSnapshot,
                person.CanLoginSnapshot ? "account_unavailable" : "invite_pending",
                person.PrimaryEmail,
                true,
                person.HasUserAccountSnapshot,
                false,
                false,
                false,
                null,
                null,
                null,
                false,
                "NexArr account details are temporarily unavailable.");
        }

        try
        {
            var identity = await nexArrPlatformIdentityClient.GetIdentityAsync(
                person.TenantId,
                externalUserId,
                cancellationToken);

            return MapSummary(person, identity, true, null);
        }
        catch (StlApiException ex) when (ex.StatusCode == 404)
        {
            return new PersonAccountAccessSummaryResponse(
                person.Id,
                person.PrimaryEmail,
                false,
                false,
                "account_unavailable",
                null,
                false,
                false,
                false,
                false,
                false,
                null,
                null,
                null,
                true,
                "The linked platform account could not be found in NexArr.");
        }
    }

    private static PersonAccountAccessSummaryResponse MapSummary(
        StaffPerson person,
        NexArrPlatformIdentityResult identity,
        bool integrationAvailable,
        string? notice)
    {
        var loginEmailMatchesWorkEmail = string.Equals(
            person.PrimaryEmail,
            identity.Email,
            StringComparison.OrdinalIgnoreCase);

        return new PersonAccountAccessSummaryResponse(
            person.Id,
            person.PrimaryEmail,
            true,
            identity.CanLogin,
            ResolveAccountState(identity),
            identity.Email,
            loginEmailMatchesWorkEmail,
            identity.IsActive,
            identity.IsMfaEnabled,
            identity.RequiresPasswordChange,
            identity.LaunchEligible,
            HumanizeKey(identity.MembershipRoleKey),
            identity.LastLoginAt,
            identity.LastProductLaunchAt,
            integrationAvailable,
            notice);
    }

    private async Task RefreshPersonLoginSnapshotAsync(
        Guid tenantId,
        StaffPerson person,
        CancellationToken cancellationToken)
    {
        if (person.ExternalUserId is not Guid externalUserId)
        {
            return;
        }

        var identity = await nexArrPlatformIdentityClient.GetIdentityAsync(
            tenantId,
            externalUserId,
            cancellationToken);
        ApplyPlatformIdentitySnapshot(person, identity);
        person.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyPlatformIdentitySnapshot(
        StaffPerson person,
        NexArrPlatformIdentityResult identity)
    {
        person.ExternalUserId = identity.ExternalUserId;
        person.CanLoginSnapshot = identity.CanLogin && identity.IsActive;
        person.HasUserAccountSnapshot = true;
    }

    private async Task EnsurePrimaryEmailAvailableAsync(
        Guid tenantId,
        string normalizedEmail,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var exists = await db.People.AnyAsync(
            p => p.TenantId == tenantId
                && p.Id != personId
                && p.PrimaryEmail.ToLower() == normalizedEmail,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "people.email_conflict",
                "That work email is already used by another person in this tenant.",
                409);
        }
    }

    private static Guid RequireExternalUserId(StaffPerson person)
    {
        return person.ExternalUserId
            ?? throw new StlApiException(
                "people.account_not_found",
                "This person does not have a linked platform account yet.",
                409);
    }

    private static string NormalizeEmail(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 320)
        {
            throw new StlApiException("people.validation", $"{fieldName} is required and must be 320 characters or less.", 400);
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!new EmailAddressAttribute().IsValid(normalized))
        {
            throw new StlApiException("people.validation", $"{fieldName} must be valid.", 400);
        }

        return normalized;
    }

    private static string ResolveAccountState(NexArrPlatformIdentityResult identity)
    {
        if (!identity.IsActive)
        {
            return "login_disabled";
        }

        if (!identity.CanLogin)
        {
            return "invite_pending";
        }

        return identity.Status switch
        {
            "locked" => "login_locked",
            "password_change_required" => "password_change_required",
            "pending_verification" => "pending_verification",
            _ => "login_enabled",
        };
    }

    private static string? HumanizeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return string.Join(
            ' ',
            value
                .Trim()
                .Split(['_', '-'], StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
    }
}
