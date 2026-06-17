using CustomArr.Api.Services;

namespace CustomArr.Api.Endpoints;

public static class CrmWorkspaceEndpoints
{
    public static void MapCustomArrCrmWorkspaceEndpoints(this WebApplication app)
    {
        var workspace = app.MapGroup("/api/v1/workspace")
            .WithTags("CRM workspace")
            .RequireAuthorization();

        workspace.MapGet("/crm-overview", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.GetOverviewAsync(context.User, cancellationToken))
            .WithName("GetCustomArrCrmOverview");

        workspace.MapGet("/accounts", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListAccountsAsync(context.User, cancellationToken))
            .WithName("ListCustomArrAccounts");

        workspace.MapGet("/locations", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListLocationsAsync(context.User, cancellationToken))
            .WithName("ListCustomArrCustomerLocations");

        workspace.MapGet("/contacts", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListContactsAsync(context.User, cancellationToken))
            .WithName("ListCustomArrCustomerContacts");

        workspace.MapGet("/leads", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListLeadsAsync(context.User, cancellationToken))
            .WithName("ListCustomArrLeads");

        workspace.MapPost("/leads", (
            HttpContext context,
            CustomArrCreateLeadRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CreateLeadAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrLead");

        workspace.MapPost("/leads/{leadId}/convert", (
            string leadId,
            HttpContext context,
            CustomArrConvertLeadRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ConvertLeadAsync(context.User, leadId, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("ConvertCustomArrLead");

        workspace.MapGet("/opportunities", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListOpportunitiesAsync(context.User, cancellationToken))
            .WithName("ListCustomArrOpportunities");

        workspace.MapPost("/opportunities", (
            HttpContext context,
            CustomArrCreateOpportunityRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CreateOpportunityAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrOpportunity");

        workspace.MapPost("/opportunities/{opportunityId}/won", (
            string opportunityId,
            HttpContext context,
            CustomArrOpportunityWonRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.MarkOpportunityWonAsync(context.User, opportunityId, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("WinCustomArrOpportunity");

        workspace.MapGet("/proposals", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListProposalsAsync(context.User, cancellationToken))
            .WithName("ListCustomArrProposals");

        workspace.MapPost("/proposals", (
            HttpContext context,
            CustomArrCreateProposalRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CreateProposalAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrProposal");

        workspace.MapPost("/proposals/{proposalId}/accept", (
            string proposalId,
            HttpContext context,
            CustomArrProposalAcceptanceRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.AcceptProposalAsync(context.User, proposalId, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("AcceptCustomArrProposal");

        workspace.MapGet("/agreements", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListAgreementsAsync(context.User, cancellationToken))
            .WithName("ListCustomArrAgreements");

        workspace.MapGet("/cases", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListCasesAsync(context.User, cancellationToken))
            .WithName("ListCustomArrCases");

        workspace.MapPost("/cases", (
            HttpContext context,
            CustomArrCreateCaseRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CreateCaseAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrCase");

        workspace.MapGet("/activities", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListRecordsAsyncPublic(context.User, "activity", cancellationToken))
            .WithName("ListCustomArrActivities");

        workspace.MapPost("/activities", (
            HttpContext context,
            CustomArrCreateActivityRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CreateActivityAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrActivity");

        workspace.MapGet("/tasks", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListTasksAsync(context.User, cancellationToken))
            .WithName("ListCustomArrTasks");

        workspace.MapPost("/tasks", (
            HttpContext context,
            CustomArrCreateTaskRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CreateTaskAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrTask");

        workspace.MapGet("/portal-access", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListPortalAccessAsync(context.User, cancellationToken))
            .WithName("ListCustomArrPortalAccess");

        workspace.MapGet("/eligibility", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListRecordsAsyncPublic(context.User, "eligibility", cancellationToken))
            .WithName("ListCustomArrEligibilityChecks");

        workspace.MapPost("/eligibility", (
            HttpContext context,
            CustomArrEligibilityCheckRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CheckEligibilityAsync(context.User, request, ResolveIdempotencyKey(context), requireIdempotency: true, cancellationToken))
            .WithName("CheckCustomArrEligibility");

        workspace.MapGet("/onboarding", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListOnboardingAsync(context.User, cancellationToken))
            .WithName("ListCustomArrOnboarding");

        workspace.MapGet("/health", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListHealthAsync(context.User, cancellationToken))
            .WithName("ListCustomArrHealthProfiles");

        workspace.MapGet("/imports", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListImportsAsync(context.User, cancellationToken))
            .WithName("ListCustomArrImports");

        workspace.MapPost("/imports", (
            HttpContext context,
            CustomArrCreateImportBatchRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CreateImportBatchAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrImport");

        workspace.MapGet("/merge-review", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListMergeReviewAsync(context.User, cancellationToken))
            .WithName("ListCustomArrMergeReview");

        workspace.MapPost("/merge-review", (
            HttpContext context,
            CustomArrCreateMergeRecordRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CreateMergeRecordAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrMergeReview");

        workspace.MapGet("/integration-references", (
            HttpContext context,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.ListIntegrationReferencesAsync(context.User, cancellationToken))
            .WithName("ListCustomArrIntegrationReferences");

        var integrations = app.MapGroup("/api/v1/integrations")
            .WithTags("CRM integrations")
            .RequireAuthorization();

        integrations.MapPost("/customer-eligibility-checks", (
            HttpContext context,
            CustomArrEligibilityCheckRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CheckEligibilityAsync(context.User, request, ResolveIdempotencyKey(context), requireIdempotency: true, cancellationToken))
            .WithName("CreateCustomArrCustomerEligibilityCheck");

        integrations.MapPost("/customer-requirement-evaluations", (
            HttpContext context,
            CustomArrRequirementEvaluationRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.EvaluateRequirementsAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrCustomerRequirementEvaluation");

        integrations.MapPost("/customer-activity-events", (
            HttpContext context,
            CustomArrCreateActivityRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CreateActivityAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrCustomerActivityEvent");

        integrations.MapPost("/customer-external-mappings", (
            HttpContext context,
            CustomArrCreateExternalMappingRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.CreateExternalMappingAsync(context.User, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrCustomerExternalMapping");

        integrations.MapPost("/opportunities/{opportunityId}/ordarr-handoffs", (
            string opportunityId,
            HttpContext context,
            CustomArrOpportunityWonRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.MarkOpportunityWonAsync(context.User, opportunityId, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrOpportunityOrdArrHandoff");

        integrations.MapPost("/proposals/{proposalId}/ordarr-handoffs", (
            string proposalId,
            HttpContext context,
            CustomArrProposalAcceptanceRequest request,
            CustomArrCrmWorkspaceService crm,
            CancellationToken cancellationToken) =>
            crm.AcceptProposalAsync(context.User, proposalId, request, ResolveIdempotencyKey(context), cancellationToken))
            .WithName("CreateCustomArrProposalOrdArrHandoff");
    }

    private static string ResolveIdempotencyKey(HttpContext context) =>
        context.Request.Headers["Idempotency-Key"].ToString();
}
