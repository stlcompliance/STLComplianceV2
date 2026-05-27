using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class DispatchWorkflowGateSeedService(
    ComplianceCoreDbContext db,
    WorkflowGateService workflowGateService)
{
    public static readonly IReadOnlyList<DispatchWorkflowGateSeedDefinition> DefaultDefinitions =
    [
        new(
            "dispatch_driver_qualification",
            "Driver qualification dispatch gate",
            "Blocks or warns on driver assignment when driver qualification rules fail."),
        new(
            "dispatch_hazmat",
            "Hazmat dispatch gate",
            "Evaluates hazmat-sensitive trip loads before dispatch assignment."),
        new(
            "dispatch_hours_of_service",
            "Hours of service dispatch gate",
            "Evaluates hours-of-service constraints before dispatch assignment."),
    ];

    public async Task<IReadOnlyList<WorkflowGateDefinitionResponse>> EnsureDispatchGatesAsync(
        Guid tenantId,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        var rulePack = await db.RulePacks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .OrderBy(x => x.PackKey == "driver_qualification" ? 0 : 1)
            .ThenBy(x => x.PackKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (rulePack is null)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "workflow_gates.seed_rule_pack_missing",
                "At least one active rule pack is required before seeding dispatch workflow gates.",
                400);
        }

        var existing = await db.WorkflowGateDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.GateKey)
            .ToListAsync(cancellationToken);

        var existingKeys = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var created = new List<WorkflowGateDefinitionResponse>();

        foreach (var definition in DefaultDefinitions)
        {
            if (existingKeys.Contains(definition.GateKey))
            {
                continue;
            }

            var gate = await workflowGateService.CreateDefinitionAsync(
                tenantId,
                actorUserId,
                new CreateWorkflowGateDefinitionRequest(
                    definition.GateKey,
                    definition.Label,
                    definition.Description,
                    rulePack.Id),
                cancellationToken);

            created.Add(gate);
            existingKeys.Add(definition.GateKey);
        }

        if (created.Count == 0)
        {
            return await workflowGateService.ListDefinitionsAsync(tenantId, cancellationToken);
        }

        var all = await workflowGateService.ListDefinitionsAsync(tenantId, cancellationToken);
        return all
            .Where(gate => DefaultDefinitions.Any(def =>
                string.Equals(def.GateKey, gate.GateKey, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}

public sealed record DispatchWorkflowGateSeedDefinition(
    string GateKey,
    string Label,
    string Description);
