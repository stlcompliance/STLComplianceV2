using STLCompliance.Shared.Integration;
using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E.Catalog;

[Trait("Category", "E2e")]
[Trait("Area", "WorkflowCatalog")]
public sealed class StlMasterWorkflowCatalogTests
{
    [Fact]
    public void Master_catalog_matches_declared_960_workflow_scope()
    {
        Assert.Equal(26, StlMasterWorkflowCatalog.Sections.Count);
        Assert.Equal(
            StlMasterWorkflowCatalog.ExpectedWorkflowFamilyCount,
            StlMasterWorkflowCatalog.AllWorkflows.Count);
        Assert.Equal(
            StlMasterWorkflowCatalog.ExpectedWorkflowFamilyCount,
            StlMasterWorkflowCatalog.Sections.Sum(section => section.Count));

        Assert.Equal("GEN-01", StlMasterWorkflowCatalog.AllWorkflows.First().WorkflowId);
        Assert.Equal("E2E-RES-32", StlMasterWorkflowCatalog.AllWorkflows.Last().WorkflowId);
    }

    [Theory]
    [InlineData("GEN", 30)]
    [InlineData("NEX", 35)]
    [InlineData("STA", 49)]
    [InlineData("TRN", 44)]
    [InlineData("MNT", 64)]
    [InlineData("RTE", 52)]
    [InlineData("SUP", 42)]
    [InlineData("LOD", 50)]
    [InlineData("ASR", 37)]
    [InlineData("REC", 36)]
    [InlineData("ORD", 34)]
    [InlineData("CUS", 40)]
    [InlineData("LED", 68)]
    [InlineData("RPT", 27)]
    [InlineData("CC", 54)]
    [InlineData("FC", 27)]
    [InlineData("SITE", 18)]
    [InlineData("E2E-PLAT", 25)]
    [InlineData("E2E-WORK", 25)]
    [InlineData("E2E-CUST", 29)]
    [InlineData("E2E-PROC", 31)]
    [InlineData("E2E-AST", 24)]
    [InlineData("E2E-TRN", 26)]
    [InlineData("E2E-CMP", 36)]
    [InlineData("E2E-FIN", 25)]
    [InlineData("E2E-RES", 32)]
    public void Each_catalog_section_generates_expected_contiguous_ids(
        string codePrefix,
        int expectedCount)
    {
        var workflows = StlMasterWorkflowCatalog.AllWorkflows
            .Where(workflow => workflow.WorkflowId.StartsWith($"{codePrefix}-", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(expectedCount, workflows.Length);
        Assert.Equal($"{codePrefix}-01", workflows.First().WorkflowId);
        Assert.Equal($"{codePrefix}-{expectedCount:00}", workflows.Last().WorkflowId);
    }

    [Fact]
    public void Every_workflow_has_a_completion_contract_with_canonical_owners()
    {
        foreach (var workflow in StlMasterWorkflowCatalog.AllWorkflows)
        {
            Assert.True(
                workflow.HasCompletionContract,
                $"Workflow {workflow.WorkflowId} is missing owner-aware completion metadata.");

            Assert.DoesNotContain(
                "workflowarr",
                workflow.CompletionOwnerProductKeys,
                StringComparer.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Product_workflows_keep_primary_completion_with_the_owning_product()
    {
        var expectedOwners = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["NEX"] = StlProductKeys.NexArr,
            ["STA"] = StlProductKeys.StaffArr,
            ["TRN"] = StlProductKeys.TrainArr,
            ["MNT"] = StlProductKeys.MaintainArr,
            ["RTE"] = StlProductKeys.RoutArr,
            ["SUP"] = StlProductKeys.SupplyArr,
            ["LOD"] = StlProductKeys.LoadArr,
            ["ASR"] = StlProductKeys.AssurArr,
            ["REC"] = StlProductKeys.RecordArr,
            ["ORD"] = StlProductKeys.OrdArr,
            ["CUS"] = StlProductKeys.CustomArr,
            ["LED"] = StlProductKeys.LedgArr,
            ["RPT"] = StlProductKeys.ReportArr,
            ["CC"] = StlProductKeys.ComplianceCore,
            ["FC"] = StlProductKeys.FieldCompanion,
            ["SITE"] = StlProductKeys.StlComplianceSite,
        };

        foreach (var (prefix, owner) in expectedOwners)
        {
            var workflows = StlMasterWorkflowCatalog.AllWorkflows
                .Where(workflow => workflow.WorkflowId.StartsWith($"{prefix}-", StringComparison.Ordinal))
                .ToArray();

            Assert.NotEmpty(workflows);
            Assert.All(workflows, workflow =>
            {
                Assert.Equal(owner, workflow.PrimaryOwnerProductKey);
                Assert.Contains(owner, workflow.CompletionOwnerProductKeys);
                Assert.NotEqual(StlWorkflowCompletionMode.CrossProductWorkflowPack, workflow.CompletionMode);
            });
        }
    }

    [Fact]
    public void Cross_suite_workflows_require_handoffs_events_tasks_and_evidence()
    {
        var crossSuiteWorkflows = StlMasterWorkflowCatalog.AllWorkflows
            .Where(workflow => workflow.Kind == StlMasterWorkflowSectionKind.CrossSuiteWorkflow)
            .ToArray();

        Assert.Equal(253, crossSuiteWorkflows.Length);
        Assert.All(crossSuiteWorkflows, workflow =>
        {
            Assert.Equal(StlWorkflowCompletionMode.CrossProductWorkflowPack, workflow.CompletionMode);
            Assert.Null(workflow.PrimaryOwnerProductKey);
            Assert.Contains("approved_product_api_or_handoff", workflow.RequiredCompletionCapabilities);
            Assert.Contains("event_envelope_and_idempotency", workflow.RequiredCompletionCapabilities);
            Assert.Contains("product_owned_tasks", workflow.RequiredCompletionCapabilities);
            Assert.Contains("recordarr_evidence_package", workflow.RequiredCompletionCapabilities);
            Assert.Contains("completion_or_closeout_state", workflow.RequiredCompletionCapabilities);
        });
    }

    [Fact]
    public void Lookup_finds_workflows_from_each_catalog_boundary()
    {
        Assert.True(StlMasterWorkflowCatalog.TryGetWorkflow("GEN-01", out var gen));
        Assert.Equal(StlMasterWorkflowSectionKind.UniversalMechanic, gen!.Kind);

        Assert.Equal(
            StlProductKeys.LedgArr,
            StlMasterWorkflowCatalog.GetWorkflow("LED-68").PrimaryOwnerProductKey);

        var resilience = StlMasterWorkflowCatalog.GetWorkflow("E2E-RES-32");
        Assert.Equal(StlWorkflowCompletionMode.CrossProductWorkflowPack, resilience.CompletionMode);
        Assert.Contains(StlProductKeys.ComplianceCore, resilience.CompletionOwnerProductKeys);
        Assert.Contains(StlProductKeys.FieldCompanion, resilience.CompletionOwnerProductKeys);
    }
}
