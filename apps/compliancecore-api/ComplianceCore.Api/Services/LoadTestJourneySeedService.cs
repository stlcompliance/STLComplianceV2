using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Operations.LoadTesting;

namespace ComplianceCore.Api.Services;

public sealed class LoadTestJourneySeedService(
    ComplianceCoreDbContext db,
    GoverningBodyService governingBodyService,
    JurisdictionService jurisdictionService,
    RegulatoryProgramService regulatoryProgramService,
    RulePackService rulePackService,
    RuleContentService ruleContentService,
    FactDefinitionService factDefinitionService,
    FactSourceService factSourceService,
    DispatchWorkflowGateSeedService dispatchWorkflowGateSeedService,
    IComplianceCoreAuditService auditService)
{
    public async Task<LoadTestJourneySeedResponse> EnsureSeededAsync(
        Guid tenantId,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        var programId = await EnsureRegulatoryProgramAsync(tenantId, actorUserId, cancellationToken);
        var (rulePackId, rulePackCreated, contentEnsured) = await EnsureRulePackAsync(
            tenantId,
            actorUserId,
            programId,
            cancellationToken);

        var factEnsured = await EnsureDriverLicenseFactAsync(tenantId, actorUserId, cancellationToken);

        var gatesBefore = await db.WorkflowGateDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.GateKey)
            .ToListAsync(cancellationToken);

        var gates = await dispatchWorkflowGateSeedService.EnsureDispatchGatesAsync(
            tenantId,
            actorUserId,
            cancellationToken);

        var gatesCreated = gates.Count(gate =>
            !gatesBefore.Contains(gate.GateKey, StringComparer.OrdinalIgnoreCase));

        await auditService.WriteAsync(
            "load_test_journey.seed",
            tenantId,
            actorUserId,
            "rule_pack",
            rulePackId.ToString(),
            "success",
            reasonCode: StlLoadTestJourneySeedCatalog.RulePackKey,
            cancellationToken: cancellationToken);

        return new LoadTestJourneySeedResponse(
            StlLoadTestJourneySeedCatalog.RulePackKey,
            rulePackId,
            rulePackCreated,
            contentEnsured,
            factEnsured,
            gatesCreated,
            gates.Select(gate => gate.GateKey).ToList());
    }

    private async Task<Guid> EnsureRegulatoryProgramAsync(
        Guid tenantId,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        var existingProgram = await db.RegulatoryPrograms
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .OrderBy(x => x.ProgramKey == StlLoadTestJourneySeedCatalog.RegulatoryProgramKey ? 0 : 1)
            .ThenBy(x => x.ProgramKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingProgram is not null)
        {
            return existingProgram.Id;
        }

        var bodies = await governingBodyService.ListAsync(tenantId, cancellationToken);
        var body = bodies.FirstOrDefault(item =>
            string.Equals(item.BodyKey, StlLoadTestJourneySeedCatalog.GoverningBodyKey, StringComparison.OrdinalIgnoreCase));

        body ??= await governingBodyService.CreateAsync(
            tenantId,
            actorUserId,
            new CreateGoverningBodyRequest(
                StlLoadTestJourneySeedCatalog.GoverningBodyKey,
                "U.S. Department of Transportation",
                "Federal transportation safety and compliance authority for load-test journey seeds."),
            cancellationToken);

        var jurisdictions = await jurisdictionService.ListAsync(tenantId, body.GoverningBodyId, cancellationToken);
        var jurisdiction = jurisdictions.FirstOrDefault(item =>
            string.Equals(item.JurisdictionKey, StlLoadTestJourneySeedCatalog.JurisdictionKey, StringComparison.OrdinalIgnoreCase));

        jurisdiction ??= await jurisdictionService.CreateAsync(
            tenantId,
            actorUserId,
            new CreateJurisdictionRequest(
                body.GoverningBodyId,
                StlLoadTestJourneySeedCatalog.JurisdictionKey,
                "United States Federal",
                "Federal jurisdiction for load-test journey rule packs."),
            cancellationToken);

        var programs = await regulatoryProgramService.ListAsync(tenantId, jurisdiction.JurisdictionId, cancellationToken);
        var program = programs.FirstOrDefault(item =>
            string.Equals(item.ProgramKey, StlLoadTestJourneySeedCatalog.RegulatoryProgramKey, StringComparison.OrdinalIgnoreCase));

        program ??= await regulatoryProgramService.CreateAsync(
            tenantId,
            actorUserId,
            new CreateRegulatoryProgramRequest(
                jurisdiction.JurisdictionId,
                StlLoadTestJourneySeedCatalog.RegulatoryProgramKey,
                "FMCSA Driver Compliance",
                "Driver qualification program for load-test journey scenarios."),
            cancellationToken);

        return program.RegulatoryProgramId;
    }

    private async Task<(Guid RulePackId, bool Created, bool ContentEnsured)> EnsureRulePackAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid regulatoryProgramId,
        CancellationToken cancellationToken)
    {
        var existing = await db.RulePacks
            .Where(x => x.TenantId == tenantId
                && x.PackKey == StlLoadTestJourneySeedCatalog.RulePackKey
                && x.IsActive)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var created = false;
        Guid rulePackId;

        if (existing is null)
        {
            var createdPack = await rulePackService.CreateAsync(
                tenantId,
                actorUserId,
                new CreateRulePackRequest(
                    regulatoryProgramId,
                    StlLoadTestJourneySeedCatalog.RulePackKey,
                    StlLoadTestJourneySeedCatalog.RulePackLabel,
                    StlLoadTestJourneySeedCatalog.RulePackDescription),
                cancellationToken);
            rulePackId = createdPack.RulePackId;
            created = true;
            existing = await db.RulePacks.FirstAsync(x => x.Id == rulePackId, cancellationToken);
        }
        else
        {
            rulePackId = existing.Id;
        }

        if (!string.IsNullOrWhiteSpace(existing.RuleContentJson))
        {
            return (rulePackId, created, false);
        }

        var content = new RulePackContentBody(
            1,
            "all",
            [
                new RuleDefinitionDto(
                    "license_valid",
                    "Valid driver license",
                    "fact_boolean",
                    StlLoadTestJourneySeedCatalog.DriverLicenseValidFactKey,
                    true),
            ]);

        await ruleContentService.UpdateContentAsync(
            tenantId,
            actorUserId,
            rulePackId,
            new UpdateRulePackContentRequest(content),
            cancellationToken);

        return (rulePackId, created, true);
    }

    private async Task<bool> EnsureDriverLicenseFactAsync(
        Guid tenantId,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        var definitions = await factDefinitionService.ListAsync(tenantId, cancellationToken);
        var definition = definitions.FirstOrDefault(item =>
            string.Equals(item.FactKey, StlLoadTestJourneySeedCatalog.DriverLicenseValidFactKey, StringComparison.OrdinalIgnoreCase));

        var createdDefinition = false;
        if (definition is null)
        {
            definition = await factDefinitionService.CreateAsync(
                tenantId,
                actorUserId,
                new CreateFactDefinitionRequest(
                    StlLoadTestJourneySeedCatalog.DriverLicenseValidFactKey,
                    "Driver license valid",
                    "Boolean fact for load-test journey rule evaluation.",
                    "boolean"),
                cancellationToken);
            createdDefinition = true;
        }

        var sources = await factSourceService.ListAsync(tenantId, definition.FactDefinitionId, cancellationToken);
        if (sources.Any(source =>
                string.Equals(source.SourceKey, StlLoadTestJourneySeedCatalog.DriverLicenseFactSourceKey, StringComparison.OrdinalIgnoreCase)))
        {
            return createdDefinition;
        }

        await factSourceService.CreateAsync(
            tenantId,
            actorUserId,
            new CreateFactSourceRequest(
                definition.FactDefinitionId,
                StlLoadTestJourneySeedCatalog.DriverLicenseFactSourceKey,
                "static_config",
                "Load-test journey license flag",
                "Static default returning true for k6 journey scenarios.",
                null,
                null,
                """{"booleanValue":true}""",
                0),
            cancellationToken);

        return true;
    }
}
