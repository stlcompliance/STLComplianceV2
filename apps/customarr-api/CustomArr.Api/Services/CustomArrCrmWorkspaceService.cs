using System.Security.Claims;
using CustomArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace CustomArr.Api.Services;

public sealed class CustomArrCrmWorkspaceService(CustomArrDbContext db)
{
    private const string FreshnessLive = "live";

    public async Task<CustomArrCrmOverviewResponse> GetOverviewAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureCrmSeededAsync(tenantId, principal, cancellationToken);

        return new CustomArrCrmOverviewResponse(
            GeneratedAt: DateTimeOffset.UtcNow,
            AccountCount: await db.Customers.CountAsync(x => x.TenantId == tenantId, cancellationToken),
            LeadCount: await db.Leads.CountAsync(x => x.TenantId == tenantId, cancellationToken),
            OpportunityCount: await db.Opportunities.CountAsync(x => x.TenantId == tenantId && x.StatusKey == "open", cancellationToken),
            ProposalCount: await db.Proposals.CountAsync(x => x.TenantId == tenantId, cancellationToken),
            AgreementCount: await db.Agreements.CountAsync(x => x.TenantId == tenantId, cancellationToken),
            OpenCaseCount: await db.CustomerCases.CountAsync(x => x.TenantId == tenantId && x.StatusKey != "closed", cancellationToken),
            OpenTaskCount: await db.CustomerTasks.CountAsync(x => x.TenantId == tenantId && x.StatusKey != "completed", cancellationToken),
            BlockedEligibilityCount: await db.EligibilityChecks.CountAsync(x => x.TenantId == tenantId && x.ResultKey == "blocked", cancellationToken));
    }

    public async Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListAccountsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var customers = await db.Customers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.CustomerNumber)
            .ToListAsync(cancellationToken);

        return customers.Select(customer => new CustomArrCrmRecordResponse(
            "customer",
            customer.CustomerId,
            customer.CustomerNumber,
            customer.CustomerId,
            customer.DisplayName,
            customer.DisplayName,
            customer.StatusKey,
            customer.AccountOwnerPersonId,
            customer.ServiceEligibilityStatusKey,
            null,
            null,
            customer.UpdatedAt,
            customer.RelationshipSummary ?? customer.Notes,
            StlProductKeys.CustomArr,
            FreshnessLive)).ToArray();
    }

    public async Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListLocationsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var customerNames = await CustomerNamesAsync(tenantId, cancellationToken);
        var locations = await db.CustomerAddresses
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.CustomerId)
            .ThenBy(x => x.AddressTypeKey)
            .ThenBy(x => x.LocationName)
            .ToListAsync(cancellationToken);

        return locations.Select(location => new CustomArrCrmRecordResponse(
            "customer_location",
            location.AddressId,
            string.IsNullOrWhiteSpace(location.LocationCode) ? location.AddressId : location.LocationCode,
            location.CustomerId,
            customerNames.GetValueOrDefault(location.CustomerId, "Customer"),
            location.LocationName,
            location.StatusKey,
            null,
            location.AddressTypeKey,
            null,
            null,
            null,
            FormatAddress(location),
            StlProductKeys.CustomArr,
            FreshnessLive)).ToArray();
    }

    public async Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListContactsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var customerNames = await CustomerNamesAsync(tenantId, cancellationToken);
        var contacts = await db.CustomerContacts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.CustomerId)
            .ThenBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        return contacts.Select(contact => new CustomArrCrmRecordResponse(
            "customer_contact",
            contact.ContactId,
            contact.ContactId,
            contact.CustomerId,
            customerNames.GetValueOrDefault(contact.CustomerId, "Customer"),
            contact.DisplayName,
            contact.StatusKey,
            null,
            contact.PortalAccessEnabled ? "portal_enabled" : contact.PreferredContactMethodKey,
            null,
            null,
            contact.LastVerifiedAt,
            string.Join(", ", contact.AuthorizationScopes.DefaultIfEmpty(contact.Email)),
            StlProductKeys.CustomArr,
            FreshnessLive)).ToArray();
    }

    public async Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListLeadsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureCrmSeededAsync(tenantId, principal, cancellationToken);
        var leads = await db.Leads.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);
        return leads.Select(ToRecord).ToArray();
    }

    public async Task<CustomArrCrmRecordResponse> CreateLeadAsync(
        ClaimsPrincipal principal,
        CustomArrCreateLeadRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "lead create");
        if (string.IsNullOrWhiteSpace(request.CompanyName))
        {
            throw new StlApiException("customarr.lead.company_required", "Lead company name is required.", 400);
        }

        if (await ResolveIdempotencyAsync(tenantId, "customarr.lead.create", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.Leads.AsNoTracking().SingleAsync(x => x.LeadId == existingId, cancellationToken);
            return ToRecord(existing);
        }

        var now = DateTimeOffset.UtcNow;
        var lead = new CustomArrLead
        {
            LeadId = NewId("lead"),
            TenantId = tenantId,
            LeadNumber = await NextNumberAsync(tenantId, "LEAD", "lead", cancellationToken),
            CompanyName = request.CompanyName.Trim(),
            PersonName = NormalizeOptional(request.PersonName) ?? string.Empty,
            Email = NormalizeOptional(request.Email),
            Phone = NormalizeOptional(request.Phone),
            SourceKey = NormalizeKey(request.SourceKey, "manual"),
            StatusKey = NormalizeKey(request.StatusKey, "new"),
            FitScore = request.FitScore,
            NeedSummary = NormalizeOptional(request.NeedSummary),
            BudgetSummary = NormalizeOptional(request.BudgetSummary),
            TimingSummary = NormalizeOptional(request.TimingSummary),
            AuthoritySummary = NormalizeOptional(request.AuthoritySummary),
            ServiceInterest = NormalizeOptional(request.ServiceInterest),
            OwnerPersonId = NormalizeOptional(request.OwnerPersonId) ?? principal.GetPersonId().ToString("D"),
            AssignedTeamId = NormalizeOptional(request.AssignedTeamId),
            NextFollowUpAt = request.NextFollowUpAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Leads.Add(lead);
        AddIdempotency(tenantId, "customarr.lead.create", idempotencyKey, lead.LeadId, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToRecord(lead);
    }

    public async Task<CustomArrLeadConversionResponse> ConvertLeadAsync(
        ClaimsPrincipal principal,
        string leadId,
        CustomArrConvertLeadRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "lead conversion");
        if (await ResolveIdempotencyAsync(tenantId, "customarr.lead.convert", idempotencyKey, cancellationToken) is { } existingOpportunityId)
        {
            var existingOpportunity = await db.Opportunities.AsNoTracking().SingleAsync(x => x.OpportunityId == existingOpportunityId, cancellationToken);
            return new CustomArrLeadConversionResponse(existingOpportunity.CustomerId, existingOpportunity.OpportunityId, ToRecord(existingOpportunity));
        }

        var lead = await db.Leads.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.LeadId == leadId, cancellationToken)
            ?? throw new StlApiException("customarr.lead.not_found", "Lead was not found in CustomArr.", 404);

        var now = DateTimeOffset.UtcNow;
        var customerId = NormalizeOptional(request.ExistingCustomerId);
        if (customerId is null)
        {
            var customerNumber = await NextCustomerNumberAsync(tenantId, cancellationToken);
            var customer = new CustomArrCustomer
            {
                CustomerId = NewId("cust"),
                TenantId = tenantId,
                CustomerNumber = customerNumber,
                CustomerCode = customerNumber,
                LegalName = NormalizeOptional(request.CustomerLegalName) ?? lead.CompanyName,
                DisplayName = NormalizeOptional(request.CustomerDisplayName) ?? lead.CompanyName,
                CustomerTypeKey = "business",
                StatusKey = "prospect",
                RelationshipRoleKey = "buyer",
                AccountClassKey = "standard",
                OnboardingStatusKey = "not_started",
                ServiceEligibilityStatusKey = "pending_review",
                ComplianceStatusKey = "unknown",
                AccountOwnerPersonId = lead.OwnerPersonId,
                SourceKey = lead.SourceKey,
                Notes = "Converted from CustomArr lead.",
                Tags = ["lead-conversion"],
                HealthScoreKey = "unknown",
                HoldStatusKey = "clear",
                RiskRatingKey = "medium",
                CreatedAt = now,
                UpdatedAt = now,
                CreatedByPersonId = principal.GetPersonId().ToString("D"),
                UpdatedByPersonId = principal.GetPersonId().ToString("D")
            };
            db.Customers.Add(customer);
            customerId = customer.CustomerId;
        }
        else
        {
            _ = await RequireCustomerAsync(tenantId, customerId, cancellationToken);
        }

        var opportunity = new CustomArrOpportunity
        {
            OpportunityId = NewId("opp"),
            TenantId = tenantId,
            OpportunityNumber = await NextNumberAsync(tenantId, "OPP", "opportunity", cancellationToken),
            LeadId = lead.LeadId,
            CustomerId = customerId,
            OpportunityName = NormalizeOptional(request.OpportunityName) ?? $"{lead.CompanyName} opportunity",
            StageKey = "qualified",
            ProbabilityPercent = 35,
            ForecastCategoryKey = "pipeline",
            EstimatedRevenue = request.EstimatedRevenue,
            ServiceInterestKeys = SplitKeys(lead.ServiceInterest),
            ScopeSummary = lead.NeedSummary,
            StatusKey = "open",
            NextStep = "Complete discovery and proposal scope.",
            CreatedAt = now,
            UpdatedAt = now
        };

        lead.StatusKey = "converted";
        lead.ConvertedAt = now;
        lead.ConvertedCustomerId = customerId;
        lead.ConvertedOpportunityId = opportunity.OpportunityId;
        lead.UpdatedAt = now;
        db.Opportunities.Add(opportunity);
        AddIdempotency(tenantId, "customarr.lead.convert", idempotencyKey, opportunity.OpportunityId, now);
        await db.SaveChangesAsync(cancellationToken);
        return new CustomArrLeadConversionResponse(customerId, opportunity.OpportunityId, ToRecord(opportunity));
    }

    public async Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListOpportunitiesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureCrmSeededAsync(tenantId, principal, cancellationToken);
        var customerNames = await CustomerNamesAsync(tenantId, cancellationToken);
        var opportunities = await db.Opportunities.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);
        return opportunities.Select(x => ToRecord(x, customerNames)).ToArray();
    }

    public async Task<CustomArrCrmRecordResponse> CreateOpportunityAsync(
        ClaimsPrincipal principal,
        CustomArrCreateOpportunityRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "opportunity create");
        var customer = await RequireCustomerAsync(tenantId, request.CustomerId, cancellationToken);
        if (await ResolveIdempotencyAsync(tenantId, "customarr.opportunity.create", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.Opportunities.AsNoTracking().SingleAsync(x => x.OpportunityId == existingId, cancellationToken);
            return ToRecord(existing, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
        }

        var now = DateTimeOffset.UtcNow;
        var opportunity = new CustomArrOpportunity
        {
            OpportunityId = NewId("opp"),
            TenantId = tenantId,
            OpportunityNumber = await NextNumberAsync(tenantId, "OPP", "opportunity", cancellationToken),
            CustomerId = customer.CustomerId,
            OpportunityName = request.OpportunityName.Trim(),
            StageKey = NormalizeKey(request.StageKey, "discovery"),
            ProbabilityPercent = request.ProbabilityPercent,
            ForecastCategoryKey = NormalizeKey(request.ForecastCategoryKey, "pipeline"),
            ExpectedCloseDate = request.ExpectedCloseDate,
            EstimatedRevenue = request.EstimatedRevenue,
            EstimatedMargin = request.EstimatedMargin,
            ServiceInterestKeys = NormalizeKeys(request.ServiceInterestKeys),
            ScopeSummary = NormalizeOptional(request.ScopeSummary),
            PrimaryContactId = NormalizeOptional(request.PrimaryContactId),
            StatusKey = "open",
            NextStep = NormalizeOptional(request.NextStep),
            NextFollowUpAt = request.NextFollowUpAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Opportunities.Add(opportunity);
        AddIdempotency(tenantId, "customarr.opportunity.create", idempotencyKey, opportunity.OpportunityId, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToRecord(opportunity, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
    }

    public async Task<CustomArrHandoffResponse> MarkOpportunityWonAsync(
        ClaimsPrincipal principal,
        string opportunityId,
        CustomArrOpportunityWonRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "opportunity handoff");
        if (await ResolveIdempotencyAsync(tenantId, "customarr.opportunity.win", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.Opportunities.AsNoTracking().SingleAsync(x => x.OpportunityId == existingId, cancellationToken);
            return Handoff("opportunity", existing.OpportunityId, existing.OpportunityNumber, StlProductKeys.OrdArr, "order_request", "requested");
        }

        var opportunity = await db.Opportunities.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.OpportunityId == opportunityId, cancellationToken)
            ?? throw new StlApiException("customarr.opportunity.not_found", "Opportunity was not found in CustomArr.", 404);

        var now = DateTimeOffset.UtcNow;
        opportunity.StatusKey = "won";
        opportunity.StageKey = "won";
        opportunity.OutcomeKey = "won";
        opportunity.WinLossReason = NormalizeOptional(request.WinReason);
        opportunity.HandoffProductKey = StlProductKeys.OrdArr;
        opportunity.HandoffObjectRef = $"customarr:opportunity:{opportunity.OpportunityId}";
        opportunity.ClosedAt = now;
        opportunity.UpdatedAt = now;
        AddIdempotency(tenantId, "customarr.opportunity.win", idempotencyKey, opportunity.OpportunityId, now);
        await db.SaveChangesAsync(cancellationToken);
        return Handoff("opportunity", opportunity.OpportunityId, opportunity.OpportunityNumber, StlProductKeys.OrdArr, "order_request", "requested");
    }

    public async Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListProposalsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureCrmSeededAsync(tenantId, principal, cancellationToken);
        var customerNames = await CustomerNamesAsync(tenantId, cancellationToken);
        var proposals = await db.Proposals.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);
        return proposals.Select(x => ToRecord(x, customerNames)).ToArray();
    }

    public async Task<CustomArrCrmRecordResponse> CreateProposalAsync(
        ClaimsPrincipal principal,
        CustomArrCreateProposalRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "proposal create");
        var customer = await RequireCustomerAsync(tenantId, request.CustomerId, cancellationToken);
        if (await ResolveIdempotencyAsync(tenantId, "customarr.proposal.create", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.Proposals.AsNoTracking().SingleAsync(x => x.ProposalId == existingId, cancellationToken);
            return ToRecord(existing, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
        }

        var now = DateTimeOffset.UtcNow;
        var proposal = new CustomArrProposal
        {
            ProposalId = NewId("prop"),
            TenantId = tenantId,
            ProposalNumber = await NextNumberAsync(tenantId, "PROP", "proposal", cancellationToken),
            CustomerId = customer.CustomerId,
            OpportunityId = NormalizeOptional(request.OpportunityId),
            VersionNumber = request.VersionNumber <= 0 ? 1 : request.VersionNumber,
            StatusKey = NormalizeKey(request.StatusKey, "draft"),
            ScopeSummary = request.ScopeSummary.Trim(),
            PricingSnapshotJson = NormalizeOptional(request.PricingSnapshotJson),
            TermsSnapshot = NormalizeOptional(request.TermsSnapshot),
            SlaSnapshot = NormalizeOptional(request.SlaSnapshot),
            ApprovalStatusKey = NormalizeKey(request.ApprovalStatusKey, "not_required"),
            CustomerResponseKey = "pending",
            ValidUntil = request.ValidUntil,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Proposals.Add(proposal);
        AddIdempotency(tenantId, "customarr.proposal.create", idempotencyKey, proposal.ProposalId, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToRecord(proposal, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
    }

    public async Task<CustomArrHandoffResponse> AcceptProposalAsync(
        ClaimsPrincipal principal,
        string proposalId,
        CustomArrProposalAcceptanceRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "proposal acceptance");
        if (await ResolveIdempotencyAsync(tenantId, "customarr.proposal.accept", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.Proposals.AsNoTracking().SingleAsync(x => x.ProposalId == existingId, cancellationToken);
            return Handoff("proposal", existing.ProposalId, existing.ProposalNumber, StlProductKeys.OrdArr, NormalizeKey(request.TargetObjectType, "order_request"), "requested");
        }

        var proposal = await db.Proposals.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ProposalId == proposalId, cancellationToken)
            ?? throw new StlApiException("customarr.proposal.not_found", "Proposal was not found in CustomArr.", 404);

        var now = DateTimeOffset.UtcNow;
        proposal.StatusKey = "accepted";
        proposal.CustomerResponseKey = "accepted";
        proposal.AcceptedAt = now;
        proposal.UpdatedAt = now;
        proposal.CreatedOrdArrOrderRef = $"customarr:proposal:{proposal.ProposalId}:accepted";
        AddIdempotency(tenantId, "customarr.proposal.accept", idempotencyKey, proposal.ProposalId, now);
        await db.SaveChangesAsync(cancellationToken);
        return Handoff("proposal", proposal.ProposalId, proposal.ProposalNumber, StlProductKeys.OrdArr, NormalizeKey(request.TargetObjectType, "order_request"), "requested");
    }

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListAgreementsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, "agreement", cancellationToken);

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListCasesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, "case", cancellationToken);

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListTasksAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, "task", cancellationToken);

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListPortalAccessAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, "portal_access", cancellationToken);

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListRequirementsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, "requirement", cancellationToken);

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListOnboardingAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, "onboarding", cancellationToken);

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListHealthAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, "health", cancellationToken);

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListImportsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, "import", cancellationToken);

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListMergeReviewAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, "merge", cancellationToken);

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListIntegrationReferencesAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, "integration_reference", cancellationToken);

    public Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListRecordsAsyncPublic(
        ClaimsPrincipal principal,
        string module,
        CancellationToken cancellationToken = default) =>
        ListRecordsAsync(principal, module, cancellationToken);

    public async Task<CustomArrCrmRecordResponse> CreateCaseAsync(
        ClaimsPrincipal principal,
        CustomArrCreateCaseRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "case create");
        var customer = await RequireCustomerAsync(tenantId, request.CustomerId, cancellationToken);
        if (await ResolveIdempotencyAsync(tenantId, "customarr.case.create", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.CustomerCases.AsNoTracking().SingleAsync(x => x.CaseId == existingId, cancellationToken);
            return ToRecord(existing, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new CustomArrCustomerCase
        {
            CaseId = NewId("case"),
            TenantId = tenantId,
            CaseNumber = await NextNumberAsync(tenantId, "CASE", "case", cancellationToken),
            CustomerId = customer.CustomerId,
            ContactId = NormalizeOptional(request.ContactId),
            CustomerLocationId = NormalizeOptional(request.CustomerLocationId),
            Subject = request.Subject.Trim(),
            Description = NormalizeOptional(request.Description) ?? string.Empty,
            SourceKey = NormalizeKey(request.SourceKey, "internal"),
            PriorityKey = NormalizeKey(request.PriorityKey, "normal"),
            SeverityKey = NormalizeKey(request.SeverityKey, "medium"),
            StatusKey = "new",
            SupportOwnerPersonId = NormalizeOptional(request.SupportOwnerPersonId) ?? principal.GetPersonId().ToString("D"),
            OwningProductKey = NormalizeOptional(request.OwningProductKey),
            OwningProductIssueRef = NormalizeOptional(request.OwningProductIssueRef),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.CustomerCases.Add(entity);
        AddIdempotency(tenantId, "customarr.case.create", idempotencyKey, entity.CaseId, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToRecord(entity, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
    }

    public async Task<CustomArrCrmRecordResponse> CreateActivityAsync(
        ClaimsPrincipal principal,
        CustomArrCreateActivityRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "activity create");
        var customer = await RequireCustomerAsync(tenantId, request.CustomerId, cancellationToken);
        if (await ResolveIdempotencyAsync(tenantId, "customarr.activity.create", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.CustomerActivity.AsNoTracking().SingleAsync(x => x.ActivityId == existingId, cancellationToken);
            return ToRecord(existing, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new CustomArrCustomerActivity
        {
            ActivityId = NewId("act"),
            TenantId = tenantId,
            CustomerId = customer.CustomerId,
            Kind = NormalizeKey(request.ActivityTypeKey, "note"),
            ActivityTypeKey = NormalizeKey(request.ActivityTypeKey, "note"),
            Subject = NormalizeOptional(request.Subject),
            Message = request.Message.Trim(),
            Body = NormalizeOptional(request.Body),
            SourceProductKey = NormalizeKey(request.SourceProductKey, StlProductKeys.CustomArr),
            SourceObjectRef = NormalizeOptional(request.SourceObjectRef),
            ContactId = NormalizeOptional(request.ContactId),
            CustomerLocationId = NormalizeOptional(request.CustomerLocationId),
            DirectionKey = NormalizeKey(request.DirectionKey, "internal"),
            VisibilityKey = NormalizeKey(request.VisibilityKey, "internal"),
            RelatedObjectRefs = NormalizeKeys(request.RelatedObjectRefs),
            RecordRefs = NormalizeKeys(request.RecordRefs),
            ActorPersonId = principal.GetPersonId().ToString("D"),
            OccurredAt = request.OccurredAt ?? now,
            CreatedAt = now
        };

        db.CustomerActivity.Add(entity);
        AddIdempotency(tenantId, "customarr.activity.create", idempotencyKey, entity.ActivityId, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToRecord(entity, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
    }

    public async Task<CustomArrCrmRecordResponse> CreateTaskAsync(
        ClaimsPrincipal principal,
        CustomArrCreateTaskRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "task create");
        var customer = await RequireCustomerAsync(tenantId, request.CustomerId, cancellationToken);
        if (await ResolveIdempotencyAsync(tenantId, "customarr.task.create", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.CustomerTasks.AsNoTracking().SingleAsync(x => x.TaskId == existingId, cancellationToken);
            return ToRecord(existing, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new CustomArrTask
        {
            TaskId = NewId("task"),
            TenantId = tenantId,
            TaskNumber = await NextNumberAsync(tenantId, "TASK", "task", cancellationToken),
            CustomerId = customer.CustomerId,
            RelatedObjectType = NormalizeOptional(request.RelatedObjectType),
            RelatedObjectId = NormalizeOptional(request.RelatedObjectId),
            Title = request.Title.Trim(),
            Description = NormalizeOptional(request.Description),
            OwnerPersonId = NormalizeOptional(request.OwnerPersonId) ?? principal.GetPersonId().ToString("D"),
            DueAt = request.DueAt,
            PriorityKey = NormalizeKey(request.PriorityKey, "normal"),
            StatusKey = "open",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.CustomerTasks.Add(entity);
        AddIdempotency(tenantId, "customarr.task.create", idempotencyKey, entity.TaskId, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToRecord(entity, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
    }

    public async Task<CustomArrEligibilityCheckResponse> EvaluateRequirementsAsync(
        ClaimsPrincipal principal,
        CustomArrRequirementEvaluationRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "requirement evaluation");
        var customer = await RequireCustomerAsync(tenantId, request.CustomerId, cancellationToken);
        if (await ResolveIdempotencyAsync(tenantId, "customarr.requirement.evaluate", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.EligibilityChecks.AsNoTracking().SingleAsync(x => x.EligibilityCheckId == existingId, cancellationToken);
            return ToEligibilityResponse(existing);
        }

        var requirementIds = NormalizeKeys(request.RequirementIds);
        var requirementsQuery = db.CustomerRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CustomerId == customer.CustomerId);
        if (requirementIds.Length > 0)
        {
            requirementsQuery = requirementsQuery.Where(x => requirementIds.Contains(x.RequirementId));
        }

        var requirements = await requirementsQuery
            .OrderBy(x => x.RequirementName)
            .ToListAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var blockers = requirements
            .Where(x => x.StatusKey is "missing" or "rejected" || x.ExpirationDate < now)
            .Select(x => $"Requirement blocked: {x.RequirementName}.")
            .ToList();
        var warnings = requirements
            .Where(x => x.StatusKey is "pending_review" or "draft")
            .Select(x => $"Requirement needs review: {x.RequirementName}.")
            .ToList();
        var result = blockers.Count > 0
            ? "blocked"
            : warnings.Count > 0
                ? "warned"
                : "passed";

        var check = new CustomArrEligibilityCheck
        {
            EligibilityCheckId = NewId("elig"),
            TenantId = tenantId,
            CustomerId = customer.CustomerId,
            WorkflowKey = NormalizeKey(request.WorkflowKey, "customer_requirement_evaluation"),
            SourceProductKey = NormalizeKey(request.SourceProductKey, "compliancecore"),
            SourceObjectRef = NormalizeOptional(request.SourceObjectRef),
            ResultKey = result,
            Explanation = result == "passed"
                ? "Customer requirements passed the CustomArr relationship evaluation."
                : "Customer requirements produced CustomArr warnings or blockers.",
            Blockers = blockers.ToArray(),
            Warnings = warnings.ToArray(),
            CheckedAt = now,
            ActorPersonId = principal.GetPersonId().ToString("D")
        };

        db.EligibilityChecks.Add(check);
        AddIdempotency(tenantId, "customarr.requirement.evaluate", idempotencyKey, check.EligibilityCheckId, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToEligibilityResponse(check);
    }

    public async Task<CustomArrEligibilityCheckResponse> CheckEligibilityAsync(
        ClaimsPrincipal principal,
        CustomArrEligibilityCheckRequest request,
        string? idempotencyKey = null,
        bool requireIdempotency = false,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        if (requireIdempotency)
        {
            idempotencyKey = RequireIdempotencyKey(idempotencyKey, "eligibility check");
            if (await ResolveIdempotencyAsync(tenantId, "customarr.eligibility.check", idempotencyKey, cancellationToken) is { } existingId)
            {
                var existing = await db.EligibilityChecks.AsNoTracking().SingleAsync(x => x.EligibilityCheckId == existingId, cancellationToken);
                return ToEligibilityResponse(existing);
            }
        }

        var customer = await RequireCustomerAsync(tenantId, request.CustomerId, cancellationToken);
        var blockers = new List<string>();
        var warnings = new List<string>();

        if (customer.StatusKey is "archived" or "inactive")
        {
            blockers.Add($"Customer lifecycle status is {customer.StatusKey}.");
        }

        if (customer.StatusKey is "blocked" || customer.HoldStatusKey is "hold" or "blocked")
        {
            blockers.Add("Customer has an active CustomArr hold.");
        }

        if (customer.ServiceEligibilityStatusKey is "blocked")
        {
            blockers.Add("Customer service eligibility is blocked.");
        }
        else if (customer.ServiceEligibilityStatusKey is "limited" or "pending_review" or "unknown")
        {
            warnings.Add($"Customer service eligibility is {customer.ServiceEligibilityStatusKey}.");
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerLocationId))
        {
            var location = await db.CustomerAddresses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AddressId == request.CustomerLocationId, cancellationToken);
            if (location is null)
            {
                blockers.Add("Customer location reference was not found.");
            }
            else if (location.StatusKey is not "active")
            {
                warnings.Add($"Customer location status is {location.StatusKey}.");
            }
        }

        var reviewRequirementStatuses = new[] { "pending_review", "missing", "rejected" };
        var openBlockingRequirements = await db.CustomerRequirements.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CustomerId == request.CustomerId && reviewRequirementStatuses.Contains(x.StatusKey))
            .Select(x => x.RequirementName)
            .Take(10)
            .ToListAsync(cancellationToken);
        warnings.AddRange(openBlockingRequirements.Select(name => $"Customer requirement needs review: {name}."));

        var result = blockers.Count > 0
            ? "blocked"
            : warnings.Count > 0
                ? "limited"
                : "eligible";

        var now = DateTimeOffset.UtcNow;
        var check = new CustomArrEligibilityCheck
        {
            EligibilityCheckId = NewId("elig"),
            TenantId = tenantId,
            CustomerId = request.CustomerId,
            CustomerLocationId = NormalizeOptional(request.CustomerLocationId),
            CustomerContactId = NormalizeOptional(request.CustomerContactId),
            WorkflowKey = NormalizeKey(request.WorkflowKey, "general"),
            SourceProductKey = NormalizeKey(request.SourceProductKey, StlProductKeys.CustomArr),
            SourceObjectRef = NormalizeOptional(request.SourceObjectRef),
            ResultKey = result,
            Explanation = result == "eligible"
                ? "Customer is eligible for the requested workflow based on CustomArr relationship facts."
                : "Customer has CustomArr relationship warnings or blockers for the requested workflow.",
            Blockers = blockers.ToArray(),
            Warnings = warnings.ToArray(),
            CheckedAt = now,
            ActorPersonId = principal.GetPersonId().ToString("D")
        };

        db.EligibilityChecks.Add(check);
        if (requireIdempotency && idempotencyKey is not null)
        {
            AddIdempotency(tenantId, "customarr.eligibility.check", idempotencyKey, check.EligibilityCheckId, now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return ToEligibilityResponse(check);
    }

    public async Task<CustomArrCrmRecordResponse> CreateExternalMappingAsync(
        ClaimsPrincipal principal,
        CustomArrCreateExternalMappingRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "external mapping create");
        var customer = await RequireCustomerAsync(tenantId, request.CustomerId, cancellationToken);
        if (await ResolveIdempotencyAsync(tenantId, "customarr.external_mapping.create", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.IntegrationReferences.AsNoTracking().SingleAsync(x => x.IntegrationReferenceId == existingId, cancellationToken);
            return ToRecord(existing, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
        }

        var relatedEntityType = NormalizeReferenceType(request.RelatedEntityType);
        var now = DateTimeOffset.UtcNow;
        var reference = new CustomArrIntegrationReference
        {
            IntegrationReferenceId = NewId("xref"),
            TenantId = tenantId,
            CustomerId = customer.CustomerId,
            CustomerLocationId = NormalizeOptional(request.CustomerLocationId),
            CustomerContactId = NormalizeOptional(request.CustomerContactId),
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = NormalizeOptional(request.RelatedEntityId) ?? customer.CustomerId,
            ExternalSystemKey = NormalizeKey(request.ExternalSystemKey, "external"),
            ExternalEntityType = NormalizeKey(request.ExternalEntityType, relatedEntityType),
            ExternalId = NormalizeOptional(request.ExternalId) ?? throw new StlApiException("customarr.external_mapping.external_id_required", "External id is required.", 400),
            ExternalDisplayName = NormalizeOptional(request.ExternalDisplayName),
            SyncDirectionKey = NormalizeKey(request.SyncDirectionKey, "bidirectional"),
            StatusKey = NormalizeKey(request.StatusKey, "active"),
            LastVerifiedAt = request.LastVerifiedAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.IntegrationReferences.Add(reference);
        AddIdempotency(tenantId, "customarr.external_mapping.create", idempotencyKey, reference.IntegrationReferenceId, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToRecord(reference, new Dictionary<string, string> { [customer.CustomerId] = customer.DisplayName });
    }

    public async Task<CustomArrCrmRecordResponse> CreateImportBatchAsync(
        ClaimsPrincipal principal,
        CustomArrCreateImportBatchRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "import batch create");
        if (await ResolveIdempotencyAsync(tenantId, "customarr.import.create", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.ImportBatches.AsNoTracking().SingleAsync(x => x.ImportBatchId == existingId, cancellationToken);
            return ToRecord(existing);
        }

        var now = DateTimeOffset.UtcNow;
        var import = new CustomArrImportBatch
        {
            ImportBatchId = NewId("imp"),
            TenantId = tenantId,
            SourceKey = NormalizeKey(request.SourceKey, "manual"),
            SourceFileName = NormalizeOptional(request.SourceFileName) ?? "customarr-import.csv",
            ImporterPersonId = NormalizeOptional(request.ImporterPersonId) ?? principal.GetPersonId().ToString("D"),
            StatusKey = NormalizeKey(request.StatusKey, "staged"),
            TotalRows = request.TotalRows,
            AcceptedRows = request.AcceptedRows,
            RejectedRows = request.RejectedRows,
            MappingSummaryJson = NormalizeOptional(request.MappingSummaryJson),
            ValidationErrors = request.ValidationErrors?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray() ?? [],
            CreatedAt = now,
            UpdatedAt = now
        };

        db.ImportBatches.Add(import);
        AddIdempotency(tenantId, "customarr.import.create", idempotencyKey, import.ImportBatchId, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToRecord(import);
    }

    public async Task<CustomArrCrmRecordResponse> CreateMergeRecordAsync(
        ClaimsPrincipal principal,
        CustomArrCreateMergeRecordRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        idempotencyKey = RequireIdempotencyKey(idempotencyKey, "merge review create");
        var survivor = await RequireCustomerAsync(tenantId, request.SurvivorCustomerId, cancellationToken);
        if (await ResolveIdempotencyAsync(tenantId, "customarr.merge.create", idempotencyKey, cancellationToken) is { } existingId)
        {
            var existing = await db.MergeRecords.AsNoTracking().SingleAsync(x => x.MergeRecordId == existingId, cancellationToken);
            return ToRecord(existing, new Dictionary<string, string> { [survivor.CustomerId] = survivor.DisplayName });
        }

        var mergedCustomerIds = NormalizeKeys(request.MergedCustomerIds);
        var now = DateTimeOffset.UtcNow;
        var merge = new CustomArrMergeRecord
        {
            MergeRecordId = NewId("mrg"),
            TenantId = tenantId,
            SurvivorCustomerId = survivor.CustomerId,
            MergedCustomerIds = mergedCustomerIds,
            MergeReason = NormalizeOptional(request.MergeReason) ?? "Customer merge proposed for review.",
            MergeStrategyKey = NormalizeKey(request.MergeStrategyKey, "manual_review"),
            StatusKey = NormalizeKey(request.StatusKey, "proposed"),
            FieldResolutionSummary = NormalizeOptional(request.FieldResolutionSummary),
            ProposedByPersonId = NormalizeOptional(request.ProposedByPersonId) ?? principal.GetPersonId().ToString("D"),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.MergeRecords.Add(merge);
        AddIdempotency(tenantId, "customarr.merge.create", idempotencyKey, merge.MergeRecordId, now);
        await db.SaveChangesAsync(cancellationToken);
        return ToRecord(merge, new Dictionary<string, string> { [survivor.CustomerId] = survivor.DisplayName });
    }

    public async Task<IReadOnlyList<CustomArrReferenceSearchResult>> SearchReferencesAsync(
        ClaimsPrincipal principal,
        string referenceType,
        string? query,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var tenantId = EnsureEntitled(principal);
        var normalized = NormalizeReferenceType(referenceType);
        limit = Math.Clamp(limit <= 0 ? 25 : limit, 1, 50);
        var q = NormalizeOptional(query);

        return normalized switch
        {
            "customer" => await db.Customers.AsNoTracking().Where(x => x.TenantId == tenantId)
                .Where(x => q == null || x.DisplayName.Contains(q) || x.LegalName.Contains(q) || x.CustomerNumber.Contains(q))
                .OrderBy(x => x.CustomerNumber)
                .Take(limit)
                .Select(x => new CustomArrReferenceSearchResult("customer", x.CustomerId, x.DisplayName, x.CustomerNumber, x.StatusKey, x.RowVersion.ToString()))
                .ToListAsync(cancellationToken),
            "customer_location" => await db.CustomerAddresses.AsNoTracking().Where(x => x.TenantId == tenantId)
                .Where(x => q == null || x.LocationName.Contains(q) || x.City.Contains(q) || x.AddressId.Contains(q))
                .OrderBy(x => x.LocationName)
                .Take(limit)
                .Select(x => new CustomArrReferenceSearchResult("customer_location", x.AddressId, x.LocationName, x.CustomerId, x.StatusKey, null))
                .ToListAsync(cancellationToken),
            "customer_contact" => await db.CustomerContacts.AsNoTracking().Where(x => x.TenantId == tenantId)
                .Where(x => q == null || x.DisplayName.Contains(q) || x.Email.Contains(q))
                .OrderBy(x => x.DisplayName)
                .Take(limit)
                .Select(x => new CustomArrReferenceSearchResult("customer_contact", x.ContactId, x.DisplayName, x.Email, x.StatusKey, null))
                .ToListAsync(cancellationToken),
            "customer_requirement" => await db.CustomerRequirements.AsNoTracking().Where(x => x.TenantId == tenantId)
                .Where(x => q == null || x.RequirementName.Contains(q) || x.RequirementTypeKey.Contains(q))
                .OrderBy(x => x.RequirementName)
                .Take(limit)
                .Select(x => new CustomArrReferenceSearchResult("customer_requirement", x.RequirementId, x.RequirementName, x.CustomerId, x.StatusKey, null))
                .ToListAsync(cancellationToken),
            "customer_agreement" => await db.Agreements.AsNoTracking().Where(x => x.TenantId == tenantId)
                .Where(x => q == null || x.Title.Contains(q) || x.AgreementNumber.Contains(q))
                .OrderBy(x => x.AgreementNumber)
                .Take(limit)
                .Select(x => new CustomArrReferenceSearchResult("customer_agreement", x.AgreementId, x.Title, x.AgreementNumber, x.StatusKey, null))
                .ToListAsync(cancellationToken),
            "customer_case" => await db.CustomerCases.AsNoTracking().Where(x => x.TenantId == tenantId)
                .Where(x => q == null || x.Subject.Contains(q) || x.CaseNumber.Contains(q))
                .OrderByDescending(x => x.UpdatedAt)
                .Take(limit)
                .Select(x => new CustomArrReferenceSearchResult("customer_case", x.CaseId, x.Subject, x.CaseNumber, x.StatusKey, null))
                .ToListAsync(cancellationToken),
            _ => throw UnsupportedReferenceType(referenceType)
        };
    }

    private async Task<IReadOnlyList<CustomArrCrmRecordResponse>> ListRecordsAsync(
        ClaimsPrincipal principal,
        string module,
        CancellationToken cancellationToken)
    {
        var tenantId = EnsureEntitled(principal);
        await EnsureCrmSeededAsync(tenantId, principal, cancellationToken);
        var customerNames = await CustomerNamesAsync(tenantId, cancellationToken);

        return module switch
        {
            "agreement" => (await db.Agreements.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            "case" => (await db.CustomerCases.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            "activity" => (await db.CustomerActivity.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.OccurredAt).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            "task" => (await db.CustomerTasks.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            "portal_access" => (await db.PortalAccessRecords.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            "requirement" => (await db.CustomerRequirements.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.RequirementName).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            "eligibility" => (await db.EligibilityChecks.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CheckedAt).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            "onboarding" => (await db.CustomerOnboarding.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            "health" => (await db.CustomerHealthProfiles.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            "import" => (await db.ImportBatches.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken)).Select(ToRecord).ToArray(),
            "merge" => (await db.MergeRecords.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            "integration_reference" => (await db.IntegrationReferences.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken)).Select(x => ToRecord(x, customerNames)).ToArray(),
            _ => []
        };
    }

    private async Task EnsureCrmSeededAsync(Guid tenantId, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        if (await db.Opportunities.AnyAsync(x => x.TenantId == tenantId, cancellationToken))
        {
            return;
        }

        var customer = await db.Customers.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (customer is null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        customer.ServiceEligibilityStatusKey = customer.StatusKey == "active" ? "eligible" : "pending_review";
        customer.HealthScoreKey = customer.StatusKey == "active" ? "green" : "yellow";
        customer.HealthScore = customer.StatusKey == "active" ? 88 : 62;
        customer.LastActivityAt ??= now;

        db.Leads.Add(new CustomArrLead
        {
            LeadId = NewId("lead"),
            TenantId = tenantId,
            LeadNumber = "LEAD-1001",
            CompanyName = "Harbor View Cold Chain",
            PersonName = "Avery Nolan",
            Email = "avery.nolan@harborview.example",
            SourceKey = "referral",
            StatusKey = "qualified",
            FitScore = 84,
            ServiceInterest = "warehouse, transportation",
            OwnerPersonId = customer.AccountOwnerPersonId ?? principal.GetPersonId().ToString("D"),
            NextFollowUpAt = now.AddDays(3),
            CreatedAt = now.AddDays(-8),
            UpdatedAt = now.AddDays(-1)
        });

        db.Opportunities.Add(new CustomArrOpportunity
        {
            OpportunityId = NewId("opp"),
            TenantId = tenantId,
            OpportunityNumber = "OPP-1001",
            CustomerId = customer.CustomerId,
            OpportunityName = $"{customer.DisplayName} recurring service expansion",
            StageKey = "proposal",
            ProbabilityPercent = 65,
            ForecastCategoryKey = "best_case",
            ExpectedCloseDate = now.AddDays(21),
            EstimatedRevenue = 125000m,
            EstimatedMargin = 26000m,
            ServiceInterestKeys = [StlProductKeys.OrdArr, StlProductKeys.RoutArr, StlProductKeys.LoadArr],
            ScopeSummary = "Recurring transportation and warehouse coordination for customer sites.",
            PrimaryContactId = customer.PrimaryContactId,
            StatusKey = "open",
            NextStep = "Review proposal with customer operations sponsor.",
            NextFollowUpAt = now.AddDays(2),
            CreatedAt = now.AddDays(-14),
            UpdatedAt = now
        });

        db.Proposals.Add(new CustomArrProposal
        {
            ProposalId = NewId("prop"),
            TenantId = tenantId,
            ProposalNumber = "PROP-1001",
            CustomerId = customer.CustomerId,
            VersionNumber = 1,
            StatusKey = "sent",
            ScopeSummary = "Proposed recurring order, transportation, and fulfillment coordination package.",
            PricingSnapshotJson = """{"currency":"USD","monthlyEstimate":10400,"assumptions":["volume snapshot","standard service tier"]}""",
            TermsSnapshot = "Net 30; customer-specific POD and appointment rules apply.",
            ApprovalStatusKey = "approved",
            CustomerResponseKey = "pending",
            ValidUntil = now.AddDays(30),
            CreatedAt = now.AddDays(-4),
            UpdatedAt = now
        });

        db.Agreements.Add(new CustomArrAgreement
        {
            AgreementId = NewId("agr"),
            TenantId = tenantId,
            AgreementNumber = "AGR-1001",
            CustomerId = customer.CustomerId,
            AgreementTypeKey = "master_service_agreement",
            Title = $"{customer.DisplayName} master service agreement",
            StatusKey = "active",
            EffectiveDate = now.AddMonths(-2),
            ExpirationDate = now.AddMonths(10),
            RenewalDate = now.AddMonths(9),
            ScopeSummary = "Customer services, portal terms, documentation requirements, and service expectations.",
            CoveredProductKeys = [StlProductKeys.OrdArr, StlProductKeys.RoutArr, StlProductKeys.LoadArr],
            RecordRefs = ["recordarr:record:contract-snapshot"],
            OwnerPersonId = customer.AccountOwnerPersonId,
            CreatedAt = now.AddMonths(-2),
            UpdatedAt = now
        });

        db.CustomerCases.Add(new CustomArrCustomerCase
        {
            CaseId = NewId("case"),
            TenantId = tenantId,
            CaseNumber = "CASE-1001",
            CustomerId = customer.CustomerId,
            ContactId = customer.PrimaryContactId,
            Subject = "Portal notification preference review",
            Description = "Customer requested confirmation that status notifications route to operations and billing contacts.",
            SourceKey = "portal",
            PriorityKey = "normal",
            SeverityKey = "medium",
            StatusKey = "in_progress",
            SupportOwnerPersonId = customer.SupportOwnerPersonId ?? customer.AccountOwnerPersonId,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now
        });

        db.CustomerTasks.Add(new CustomArrTask
        {
            TaskId = NewId("task"),
            TenantId = tenantId,
            TaskNumber = "TASK-1001",
            CustomerId = customer.CustomerId,
            RelatedObjectType = "proposal",
            Title = "Follow up on proposal response",
            OwnerPersonId = customer.AccountOwnerPersonId ?? principal.GetPersonId().ToString("D"),
            DueAt = now.AddDays(2),
            PriorityKey = "high",
            StatusKey = "open",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.CustomerServiceProfiles.Add(new CustomArrCustomerServiceProfile
        {
            ServiceProfileId = NewId("svc"),
            TenantId = tenantId,
            CustomerId = customer.CustomerId,
            StatusKey = "active",
            ServiceEligibilityStatusKey = "eligible",
            AllowedProductKeys = [StlProductKeys.OrdArr, StlProductKeys.RoutArr, StlProductKeys.LoadArr],
            ServiceLevelKey = "standard",
            LastEligibilityReason = "Seeded CustomArr CRM service profile.",
            LastEligibilityCalculatedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        });

        db.CustomerHealthProfiles.Add(new CustomArrCustomerHealthProfile
        {
            HealthProfileId = NewId("health"),
            TenantId = tenantId,
            CustomerId = customer.CustomerId,
            HealthStatusKey = customer.HealthScoreKey,
            Score = customer.HealthScore,
            ScoreReason = "Initial CustomArr relationship health snapshot.",
            ActiveContactCount = await db.CustomerContacts.CountAsync(x => x.TenantId == tenantId && x.CustomerId == customer.CustomerId && x.StatusKey == "active", cancellationToken),
            ActiveLocationCount = await db.CustomerAddresses.CountAsync(x => x.TenantId == tenantId && x.CustomerId == customer.CustomerId && x.StatusKey == "active", cancellationToken),
            ChurnRiskKey = "low",
            PaymentRiskKey = "unknown",
            NextBusinessReviewAt = now.AddMonths(3),
            UpdatedAt = now
        });

        db.CustomerOnboarding.Add(new CustomArrCustomerOnboarding
        {
            OnboardingId = NewId("onb"),
            TenantId = tenantId,
            CustomerId = customer.CustomerId,
            OnboardingNumber = "ONB-1001",
            OnboardingTypeKey = "new_customer",
            StatusKey = customer.StatusKey == "active" ? "approved" : "in_review",
            OwnerPersonId = customer.AccountOwnerPersonId,
            LaunchDate = customer.ActivatedAt,
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now
        });

        db.ImportBatches.Add(new CustomArrImportBatch
        {
            ImportBatchId = NewId("imp"),
            TenantId = tenantId,
            SourceKey = "sample",
            SourceFileName = "customarr-crm-seed.csv",
            ImporterPersonId = principal.GetPersonId().ToString("D"),
            StatusKey = "reviewed",
            TotalRows = 3,
            AcceptedRows = 3,
            CreatedAt = now,
            UpdatedAt = now
        });

        db.MergeRecords.Add(new CustomArrMergeRecord
        {
            MergeRecordId = NewId("mrg"),
            TenantId = tenantId,
            SurvivorCustomerId = customer.CustomerId,
            MergedCustomerIds = [],
            MergeReason = "No duplicates currently proposed.",
            StatusKey = "completed",
            CreatedAt = now,
            UpdatedAt = now
        });

        db.IntegrationReferences.Add(new CustomArrIntegrationReference
        {
            IntegrationReferenceId = NewId("xref"),
            TenantId = tenantId,
            CustomerId = customer.CustomerId,
            RelatedEntityType = "customer",
            RelatedEntityId = customer.CustomerId,
            ExternalSystemKey = "quickbooks",
            ExternalEntityType = "customer",
            ExternalId = $"qb-{customer.CustomerNumber}",
            ExternalDisplayName = customer.DisplayName,
            StatusKey = "active",
            LastVerifiedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static CustomArrCrmRecordResponse ToRecord(CustomArrLead lead) =>
        new("lead", lead.LeadId, lead.LeadNumber, lead.ConvertedCustomerId, null, lead.CompanyName, lead.StatusKey, lead.OwnerPersonId, lead.SourceKey, lead.FitScore, lead.NextFollowUpAt, lead.UpdatedAt, lead.ServiceInterest, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrOpportunity opportunity, IReadOnlyDictionary<string, string>? customerNames = null) =>
        new("opportunity", opportunity.OpportunityId, opportunity.OpportunityNumber, opportunity.CustomerId, Name(customerNames, opportunity.CustomerId), opportunity.OpportunityName, opportunity.StatusKey, null, opportunity.StageKey, opportunity.EstimatedRevenue, opportunity.NextFollowUpAt, opportunity.UpdatedAt, opportunity.ScopeSummary, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrProposal proposal, IReadOnlyDictionary<string, string> customerNames) =>
        new("proposal", proposal.ProposalId, proposal.ProposalNumber, proposal.CustomerId, Name(customerNames, proposal.CustomerId), $"Proposal v{proposal.VersionNumber}", proposal.StatusKey, null, proposal.CustomerResponseKey, null, proposal.ValidUntil, proposal.UpdatedAt, proposal.ScopeSummary, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrAgreement agreement, IReadOnlyDictionary<string, string> customerNames) =>
        new("agreement", agreement.AgreementId, agreement.AgreementNumber, agreement.CustomerId, Name(customerNames, agreement.CustomerId), agreement.Title, agreement.StatusKey, agreement.OwnerPersonId, agreement.AgreementTypeKey, null, agreement.ExpirationDate, agreement.UpdatedAt, agreement.ScopeSummary, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrCustomerCase customerCase, IReadOnlyDictionary<string, string> customerNames) =>
        new("case", customerCase.CaseId, customerCase.CaseNumber, customerCase.CustomerId, Name(customerNames, customerCase.CustomerId), customerCase.Subject, customerCase.StatusKey, customerCase.SupportOwnerPersonId, customerCase.PriorityKey, null, customerCase.ResolutionDueAt, customerCase.UpdatedAt, customerCase.Description, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrTask task, IReadOnlyDictionary<string, string> customerNames) =>
        new("task", task.TaskId, task.TaskNumber, task.CustomerId, Name(customerNames, task.CustomerId), task.Title, task.StatusKey, task.OwnerPersonId, task.PriorityKey, null, task.DueAt, task.UpdatedAt, task.Description, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrCustomerActivity activity, IReadOnlyDictionary<string, string> customerNames) =>
        new("activity", activity.ActivityId, activity.ActivityId, activity.CustomerId, Name(customerNames, activity.CustomerId), activity.Subject ?? activity.Message, activity.Kind, activity.ActorPersonId, activity.ActivityTypeKey, null, null, activity.OccurredAt, activity.Body ?? activity.Message, activity.SourceProductKey, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrPortalAccessRecord access, IReadOnlyDictionary<string, string> customerNames) =>
        new("portal_access", access.PortalAccessId, access.PortalAccessId, access.CustomerId, Name(customerNames, access.CustomerId), access.ContactId, access.StatusKey, access.InvitedByPersonId, access.PortalRoleKey, null, access.LastAccessSnapshotAt, access.UpdatedAt, access.NexArrExternalIdentityRef, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrCustomerRequirement requirement, IReadOnlyDictionary<string, string> customerNames) =>
        new("requirement", requirement.RequirementId, requirement.RequirementId, requirement.CustomerId, Name(customerNames, requirement.CustomerId), requirement.RequirementName, requirement.StatusKey, requirement.ReviewedByPersonId, requirement.RequiredBeforeKey, null, requirement.ExpirationDate, requirement.ReviewedAt, requirement.Description, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrEligibilityCheck check, IReadOnlyDictionary<string, string> customerNames) =>
        new("eligibility", check.EligibilityCheckId, check.EligibilityCheckId, check.CustomerId, Name(customerNames, check.CustomerId), check.WorkflowKey, check.ResultKey, check.ActorPersonId, check.SourceProductKey, null, null, check.CheckedAt, check.Explanation, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrEligibilityCheckResponse ToEligibilityResponse(CustomArrEligibilityCheck check) =>
        new(
            check.EligibilityCheckId,
            check.CustomerId,
            check.CustomerLocationId,
            check.CustomerContactId,
            check.WorkflowKey,
            check.SourceProductKey,
            check.ResultKey,
            check.Explanation,
            check.Blockers,
            check.Warnings,
            check.CheckedAt);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrCustomerOnboarding onboarding, IReadOnlyDictionary<string, string> customerNames) =>
        new("onboarding", onboarding.OnboardingId, onboarding.OnboardingNumber, onboarding.CustomerId, Name(customerNames, onboarding.CustomerId), onboarding.OnboardingTypeKey, onboarding.StatusKey, onboarding.OwnerPersonId, null, null, onboarding.DueAt, onboarding.UpdatedAt, string.Join("; ", onboarding.Blockers), StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrCustomerHealthProfile health, IReadOnlyDictionary<string, string> customerNames) =>
        new("health", health.HealthProfileId, health.HealthProfileId, health.CustomerId, Name(customerNames, health.CustomerId), $"{Name(customerNames, health.CustomerId)} health", health.HealthStatusKey, null, health.ChurnRiskKey, health.Score, health.NextBusinessReviewAt, health.UpdatedAt, health.ScoreReason, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrImportBatch importBatch) =>
        new("import", importBatch.ImportBatchId, importBatch.ImportBatchId, null, null, importBatch.SourceFileName, importBatch.StatusKey, importBatch.ImporterPersonId, importBatch.SourceKey, importBatch.TotalRows, null, importBatch.UpdatedAt, string.Join("; ", importBatch.ValidationErrors), StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrMergeRecord merge, IReadOnlyDictionary<string, string> customerNames) =>
        new("merge", merge.MergeRecordId, merge.MergeRecordId, merge.SurvivorCustomerId, Name(customerNames, merge.SurvivorCustomerId), "Merge review", merge.StatusKey, merge.ProposedByPersonId, merge.MergeStrategyKey, merge.MergedCustomerIds.Length, null, merge.UpdatedAt, merge.MergeReason, StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrCrmRecordResponse ToRecord(CustomArrIntegrationReference reference, IReadOnlyDictionary<string, string> customerNames) =>
        new("integration_reference", reference.IntegrationReferenceId, reference.ExternalId, reference.CustomerId, Name(customerNames, reference.CustomerId), reference.ExternalDisplayName ?? reference.ExternalId, reference.StatusKey, null, reference.ExternalSystemKey, null, reference.LastVerifiedAt, reference.UpdatedAt, $"{reference.ExternalEntityType} in {reference.ExternalSystemKey}", StlProductKeys.CustomArr, FreshnessLive);

    private static CustomArrHandoffResponse Handoff(string sourceObjectType, string sourceObjectId, string sourceObjectNumber, string targetProductKey, string targetObjectType, string state) =>
        new(
            $"handoff-{Guid.NewGuid():N}"[..20],
            StlProductKeys.CustomArr,
            sourceObjectType,
            sourceObjectId,
            sourceObjectNumber,
            targetProductKey,
            targetObjectType,
            state,
            "CustomArr recorded commercial intent and created an explicit handoff request. Target execution remains owned by the target product.",
            DateTimeOffset.UtcNow);

    private async Task<CustomArrCustomer> RequireCustomerAsync(Guid tenantId, string customerId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new StlApiException("customarr.customer_required", "A CustomArr customer reference is required.", 400);
        }

        return await db.Customers.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.CustomerId == customerId.Trim(), cancellationToken)
            ?? throw new StlApiException("customarr.customer_not_found", "Customer was not found in CustomArr.", 404);
    }

    private async Task<IReadOnlyDictionary<string, string>> CustomerNamesAsync(Guid tenantId, CancellationToken cancellationToken) =>
        await db.Customers.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.CustomerId, x => x.DisplayName, StringComparer.OrdinalIgnoreCase, cancellationToken);

    private async Task<string?> ResolveIdempotencyAsync(Guid tenantId, string operationKey, string idempotencyKey, CancellationToken cancellationToken) =>
        (await db.IdempotencyRecords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.OperationKey == operationKey && x.IdempotencyKey == idempotencyKey, cancellationToken))
        ?.ResourceId;

    private void AddIdempotency(Guid tenantId, string operationKey, string idempotencyKey, string resourceId, DateTimeOffset now)
    {
        db.IdempotencyRecords.Add(new CustomArrIdempotencyRecord
        {
            IdempotencyRecordId = NewId("idem"),
            TenantId = tenantId,
            OperationKey = operationKey,
            IdempotencyKey = idempotencyKey,
            ResourceId = resourceId,
            CreatedAt = now
        });
    }

    private async Task<string> NextCustomerNumberAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var count = await db.Customers.CountAsync(x => x.TenantId == tenantId, cancellationToken) + 1;
        return $"CUS-{1000 + count:0000}";
    }

    private async Task<string> NextNumberAsync(Guid tenantId, string prefix, string module, CancellationToken cancellationToken)
    {
        var count = module switch
        {
            "lead" => await db.Leads.CountAsync(x => x.TenantId == tenantId, cancellationToken),
            "opportunity" => await db.Opportunities.CountAsync(x => x.TenantId == tenantId, cancellationToken),
            "proposal" => await db.Proposals.CountAsync(x => x.TenantId == tenantId, cancellationToken),
            "case" => await db.CustomerCases.CountAsync(x => x.TenantId == tenantId, cancellationToken),
            "task" => await db.CustomerTasks.CountAsync(x => x.TenantId == tenantId, cancellationToken),
            _ => 0
        } + 1;
        return $"{prefix}-{1000 + count:0000}";
    }

    private static Guid EnsureEntitled(ClaimsPrincipal principal)
    {
        if (!principal.HasProductEntitlement(StlProductKeys.CustomArr))
        {
            throw new StlApiException("customarr.not_entitled", "Active CustomArr entitlement is required.", 403);
        }

        return principal.GetTenantId();
    }

    private static string RequireIdempotencyKey(string? idempotencyKey, string operation)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("customarr.idempotency_key_required", $"Idempotency-Key header is required for {operation}.", 400);
        }

        return idempotencyKey.Trim();
    }

    public static string NormalizeReferenceType(string referenceType)
    {
        var normalized = NormalizeKey(referenceType, string.Empty);
        return SupportedReferenceTypes.Contains(normalized, StringComparer.OrdinalIgnoreCase)
            ? normalized
            : throw UnsupportedReferenceType(referenceType);
    }

    public static StlApiException UnsupportedReferenceType(string referenceType) =>
        new("customarr.references.unsupported_type", $"CustomArr does not own reference type '{referenceType}'.", 404);

    private static readonly string[] SupportedReferenceTypes =
    [
        "customer",
        "customer_location",
        "customer_contact",
        "customer_requirement",
        "customer_agreement",
        "customer_case"
    ];

    private static string NewId(string prefix) => $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 13, prefix.Length + 33)];

    private static string NormalizeKey(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = value.Trim().Replace("-", "_", StringComparison.Ordinal).Replace(" ", "_", StringComparison.Ordinal).ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? fallback : normalized;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[] NormalizeKeys(IReadOnlyList<string>? values) =>
        values is null
            ? []
            : values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => NormalizeKey(value, string.Empty)).Where(value => value.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static string[] SplitKeys(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(value => NormalizeKey(value, value)).ToArray();

    private static string? Name(IReadOnlyDictionary<string, string>? customerNames, string? customerId) =>
        customerId is not null && customerNames is not null && customerNames.TryGetValue(customerId, out var name) ? name : null;

    private static string FormatAddress(CustomArrCustomerAddress address) =>
        string.Join(", ", new[] { address.Line1, address.Line2, address.City, address.StateProvince, address.PostalCode, address.CountryCode }.Where(part => !string.IsNullOrWhiteSpace(part)));

}

public sealed record CustomArrCrmOverviewResponse(
    DateTimeOffset GeneratedAt,
    int AccountCount,
    int LeadCount,
    int OpportunityCount,
    int ProposalCount,
    int AgreementCount,
    int OpenCaseCount,
    int OpenTaskCount,
    int BlockedEligibilityCount);

public sealed record CustomArrCrmRecordResponse(
    string Module,
    string Id,
    string Number,
    string? CustomerId,
    string? CustomerName,
    string Title,
    string StatusKey,
    string? OwnerPersonId,
    string? SecondaryStatusKey,
    decimal? Value,
    DateTimeOffset? DueAt,
    DateTimeOffset? UpdatedAt,
    string? Summary,
    string SourceProductKey,
    string Freshness);

public sealed record CustomArrCreateLeadRequest(
    string CompanyName,
    string? PersonName,
    string? Email,
    string? Phone,
    string? SourceKey,
    string? StatusKey,
    int? FitScore,
    string? NeedSummary,
    string? BudgetSummary,
    string? TimingSummary,
    string? AuthoritySummary,
    string? ServiceInterest,
    string? OwnerPersonId,
    string? AssignedTeamId,
    DateTimeOffset? NextFollowUpAt);

public sealed record CustomArrConvertLeadRequest(
    string? ExistingCustomerId,
    string? CustomerLegalName,
    string? CustomerDisplayName,
    string? OpportunityName,
    decimal? EstimatedRevenue);

public sealed record CustomArrLeadConversionResponse(
    string? CustomerId,
    string OpportunityId,
    CustomArrCrmRecordResponse Opportunity);

public sealed record CustomArrCreateOpportunityRequest(
    string CustomerId,
    string OpportunityName,
    string? StageKey,
    int ProbabilityPercent,
    string? ForecastCategoryKey,
    DateTimeOffset? ExpectedCloseDate,
    decimal? EstimatedRevenue,
    decimal? EstimatedMargin,
    IReadOnlyList<string>? ServiceInterestKeys,
    string? ScopeSummary,
    string? PrimaryContactId,
    string? NextStep,
    DateTimeOffset? NextFollowUpAt);

public sealed record CustomArrOpportunityWonRequest(string? WinReason);

public sealed record CustomArrCreateProposalRequest(
    string CustomerId,
    string? OpportunityId,
    int VersionNumber,
    string? StatusKey,
    string ScopeSummary,
    string? PricingSnapshotJson,
    string? TermsSnapshot,
    string? SlaSnapshot,
    string? ApprovalStatusKey,
    DateTimeOffset? ValidUntil);

public sealed record CustomArrProposalAcceptanceRequest(string? TargetObjectType);

public sealed record CustomArrCreateCaseRequest(
    string CustomerId,
    string? ContactId,
    string? CustomerLocationId,
    string Subject,
    string? Description,
    string? SourceKey,
    string? PriorityKey,
    string? SeverityKey,
    string? SupportOwnerPersonId,
    string? OwningProductKey,
    string? OwningProductIssueRef);

public sealed record CustomArrCreateActivityRequest(
    string CustomerId,
    string? ContactId,
    string? CustomerLocationId,
    string? ActivityTypeKey,
    string? Subject,
    string Message,
    string? Body,
    string? SourceProductKey,
    string? SourceObjectRef,
    string? DirectionKey,
    string? VisibilityKey,
    IReadOnlyList<string>? RelatedObjectRefs,
    IReadOnlyList<string>? RecordRefs,
    DateTimeOffset? OccurredAt);

public sealed record CustomArrCreateTaskRequest(
    string CustomerId,
    string? RelatedObjectType,
    string? RelatedObjectId,
    string Title,
    string? Description,
    string? OwnerPersonId,
    DateTimeOffset? DueAt,
    string? PriorityKey);

public sealed record CustomArrEligibilityCheckRequest(
    string CustomerId,
    string? CustomerLocationId,
    string? CustomerContactId,
    string? WorkflowKey,
    string? SourceProductKey,
    string? SourceObjectRef);

public sealed record CustomArrRequirementEvaluationRequest(
    string CustomerId,
    IReadOnlyList<string>? RequirementIds,
    string? WorkflowKey,
    string? SourceProductKey,
    string? SourceObjectRef);

public sealed record CustomArrEligibilityCheckResponse(
    string EligibilityCheckId,
    string CustomerId,
    string? CustomerLocationId,
    string? CustomerContactId,
    string WorkflowKey,
    string SourceProductKey,
    string ResultKey,
    string Explanation,
    IReadOnlyList<string> Blockers,
    IReadOnlyList<string> Warnings,
    DateTimeOffset CheckedAt);

public sealed record CustomArrCreateExternalMappingRequest(
    string CustomerId,
    string? CustomerLocationId,
    string? CustomerContactId,
    string RelatedEntityType,
    string? RelatedEntityId,
    string ExternalSystemKey,
    string ExternalEntityType,
    string ExternalId,
    string? ExternalDisplayName,
    string? SyncDirectionKey,
    string? StatusKey,
    DateTimeOffset? LastVerifiedAt);

public sealed record CustomArrCreateImportBatchRequest(
    string? SourceKey,
    string? SourceFileName,
    string? ImporterPersonId,
    string? StatusKey,
    int TotalRows,
    int AcceptedRows,
    int RejectedRows,
    string? MappingSummaryJson,
    IReadOnlyList<string>? ValidationErrors);

public sealed record CustomArrCreateMergeRecordRequest(
    string SurvivorCustomerId,
    IReadOnlyList<string>? MergedCustomerIds,
    string? MergeReason,
    string? MergeStrategyKey,
    string? StatusKey,
    string? FieldResolutionSummary,
    string? ProposedByPersonId);

public sealed record CustomArrHandoffResponse(
    string HandoffId,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SourceObjectNumber,
    string TargetProductKey,
    string TargetObjectType,
    string State,
    string Message,
    DateTimeOffset RequestedAt);

public sealed record CustomArrReferenceSearchResult(
    string ReferenceType,
    string Id,
    string DisplayName,
    string? SecondaryLabel,
    string StatusKey,
    string? Version);
