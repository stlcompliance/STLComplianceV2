using System.Security.Cryptography;
using System.Text;
using LoadArr.Api.Endpoints;
using STLCompliance.Shared.Contracts;

namespace LoadArr.Api.Services;

public sealed class FieldInboxService(
    IConfiguration configuration,
    LoadArrOperationalWorkflowStore workflowStore)
{
    private readonly string? _frontendBaseUrl = configuration["LoadArr:FrontendBaseUrl"]
        ?? configuration["Cors:LoadArrFrontendOrigin"];

    public async Task<FieldInboxResponse> GetAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var items = (await workflowStore.ListReceivingSessionsAsync(tenantId, cancellationToken))
            .Where(IsActionableReceivingSession)
            .Select(MapReceivingTask)
            .ToList();

        return FieldInboxRules.BuildProductResponse(items);
    }

    private FieldInboxTaskItem MapReceivingTask(LoadArrReceivingSessionResponse session)
    {
        var taskKey = $"loadarr:receiving:{CreateStableTaskId(session.Id):D}";
        var deepLinkPath = BuildDeepLinkPath(session.Id, taskKey);

        return new FieldInboxTaskItem(
            taskKey,
            "loadarr",
            "receiving",
            session.ReceivingNumber,
            BuildSubtitle(session),
            session.Status,
            session.Status.Equals("inspection_required", StringComparison.OrdinalIgnoreCase) ? "high" : null,
            DueAt: null,
            SortAt: ParseTimestamp(session.StartedAtUtc),
            deepLinkPath,
            ResolveBlockedReason(session),
            FieldInboxDeepLinkBuilder.BuildProductDeepLinkUrl(_frontendBaseUrl, deepLinkPath));
    }

    private static bool IsActionableReceivingSession(LoadArrReceivingSessionResponse session) =>
        session.Status.Equals("open", StringComparison.OrdinalIgnoreCase)
        || session.Status.Equals("inspection_required", StringComparison.OrdinalIgnoreCase);

    private static string? BuildSubtitle(LoadArrReceivingSessionResponse session)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(session.SourceObjectId))
        {
            parts.Add(session.SourceObjectId);
        }

        if (!string.IsNullOrWhiteSpace(session.SupplierNameSnapshot))
        {
            parts.Add(session.SupplierNameSnapshot);
        }

        if (!string.IsNullOrWhiteSpace(session.StaffarrSiteNameSnapshot))
        {
            parts.Add(session.StaffarrSiteNameSnapshot);
        }

        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    private static string? ResolveBlockedReason(LoadArrReceivingSessionResponse session)
    {
        if (session.Status.Equals("inspection_required", StringComparison.OrdinalIgnoreCase))
        {
            return "Compliance inspection required before receiving can complete";
        }

        if (session.Lines.Any(line => line.Status.Equals("blocked_by_compliance", StringComparison.OrdinalIgnoreCase)))
        {
            return "Compliance review required before receiving can complete";
        }

        return null;
    }

    private static DateTimeOffset? ParseTimestamp(string value) =>
        DateTimeOffset.TryParse(value, out var parsed) ? parsed : null;

    private static string BuildDeepLinkPath(string sessionId, string taskKey) =>
        $"/work/receiving/{sessionId}?taskKey={Uri.EscapeDataString(taskKey)}";

    // LoadArr receiving ids are currently opaque strings, while the mobile task pipeline
    // still expects GUID-shaped task ids for validation and offline acknowledgments.
    private static Guid CreateStableTaskId(string sessionId)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes($"loadarr:receiving:{sessionId}"));
        return new Guid(hash);
    }
}
