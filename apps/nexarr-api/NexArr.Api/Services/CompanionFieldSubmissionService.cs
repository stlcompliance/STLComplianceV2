using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class CompanionFieldSubmissionService(NexArrDbContext db)
{
    public async Task RecordAsync(
        Guid tenantId,
        Guid userId,
        string taskKey,
        string productKey,
        string submissionKind,
        string status,
        string? detailMessage,
        DateTimeOffset clientSubmittedAt,
        CancellationToken cancellationToken = default)
    {
        db.CompanionFieldSubmissions.Add(new CompanionFieldSubmission
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            TaskKey = taskKey.Trim(),
            ProductKey = productKey.Trim().ToLowerInvariant(),
            SubmissionKind = submissionKind.Trim().ToLowerInvariant(),
            Status = status.Trim().ToLowerInvariant(),
            DetailMessage = string.IsNullOrWhiteSpace(detailMessage) ? null : detailMessage.Trim(),
            ClientSubmittedAt = clientSubmittedAt,
            RecordedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<FieldTaskSubmissionStatusResponse> ListLatestAsync(
        ClaimsPrincipal principal,
        IReadOnlyList<string> taskKeys,
        CancellationToken cancellationToken = default)
    {
        CompanionFieldInboxService.RequireCompanionAccess(principal);
        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();

        if (taskKeys.Count == 0)
        {
            return new FieldTaskSubmissionStatusResponse([]);
        }

        var normalizedKeys = taskKeys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key.Trim())
            .Distinct(StringComparer.Ordinal)
            .Take(50)
            .ToList();

        var submissions = await db.CompanionFieldSubmissions.AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.UserId == userId
                && normalizedKeys.Contains(x.TaskKey))
            .OrderByDescending(x => x.RecordedAt)
            .ToListAsync(cancellationToken);

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var items = new List<FieldTaskSubmissionStatusItem>();
        foreach (var submission in submissions)
        {
            var composite = $"{submission.TaskKey}\0{submission.SubmissionKind}";
            if (!seen.Add(composite))
            {
                continue;
            }

            items.Add(new FieldTaskSubmissionStatusItem(
                submission.TaskKey,
                submission.SubmissionKind,
                submission.Status,
                submission.DetailMessage,
                submission.RecordedAt));
        }

        return new FieldTaskSubmissionStatusResponse(items);
    }
}
