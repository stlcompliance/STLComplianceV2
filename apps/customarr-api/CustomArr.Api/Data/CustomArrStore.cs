using System.Security.Claims;
using STLCompliance.Shared.Auth;

namespace CustomArr.Api.Data;

public sealed class CustomArrStore
{
    private readonly object _gate = new();
    private readonly List<CustomArrCustomerDetailResponse> _customers;
    private readonly List<CustomArrRequirementCatalogItemResponse> _requirements;

    public CustomArrStore()
    {
        _requirements =
        [
            new("cert-insurance", "Certificate of insurance", "Current COI on file with liability and cargo coverage.", "complete", "Risk", ["strategic", "core"]),
            new("tax-w9", "Tax registration / W-9", "Tax identity and remittance details verified before activation.", "complete", "Finance", ["strategic", "core", "standard"]),
            new("code-of-conduct", "Supplier code of conduct", "Signed commitment to the customer code of conduct and operating standards.", "watch", "Procurement", ["strategic", "core"]),
            new("billing-terms", "Billing terms acknowledgement", "Confirmed billing schedule, payment terms, and invoicing contact.", "complete", "Accounts receivable", ["strategic", "core", "standard"]),
            new("e-invoicing", "E-invoicing readiness", "E-invoice endpoint and remittance workflow are confirmed for launch.", "pending", "Operations", ["strategic"])
        ];

        _customers =
        [
            CreateSeedCustomer(
                "cust-1001",
                "CUS-1001",
                "Acme Freight Systems LLC",
                "Acme Freight",
                "active",
                "strategic",
                "Enterprise logistics",
                "person-101",
                null,
                null,
                "Maria Jensen",
                "maria.jensen@acmefreight.example",
                4,
                "clear",
                "2026-06-10T15:40:00Z",
                "2026-06-11T13:05:00Z",
                ["Acme Freight Systems LLC"],
                "410 Harbor Ave, Dallas, TX 75201",
                "410 Harbor Ave, Dallas, TX 75201",
                "TX-45-9988123",
                "Net 30",
                "low",
                ["Strategic customer with quarterly review cadence.", "Primary onboarding and contract data is complete."],
                [
                    new("ct-1001", "Maria Jensen", "Primary procurement contact", "maria.jensen@acmefreight.example", "+1 (972) 555-0191", true),
                    new("ct-1002", "Derek Holt", "Accounts payable", "derek.holt@acmefreight.example", "+1 (972) 555-0192", false),
                    new("ct-1003", "Nina Rao", "Operations manager", "nina.rao@acmefreight.example", "+1 (972) 555-0193", false)
                ],
                [
                    new("loc-1001", "HQ", "billing", "Dallas", "TX"),
                    new("loc-1002", "Cross dock", "shipping", "Fort Worth", "TX"),
                    new("loc-1003", "Service center", "service", "Arlington", "TX")
                ],
                0),
            CreateSeedCustomer(
                "cust-1002",
                "CUS-1002",
                "Northwind Components Inc.",
                "Northwind Components",
                "onboarding",
                "core",
                "Industrial supply",
                "person-102",
                "cust-1001",
                "Acme Freight Systems LLC",
                "Harper Lane",
                "harper.lane@northwind.example",
                2,
                "watch",
                "2026-06-09T11:15:00Z",
                "2026-06-11T09:30:00Z",
                ["Acme Freight Systems LLC", "Northwind Components Inc."],
                "88 Foundry Rd, Columbus, OH 43085",
                "88 Foundry Rd, Columbus, OH 43085",
                "OH-29-1188221",
                "Net 45",
                "medium",
                ["Pending e-invoicing confirmation.", "Requires final legal review before activation."],
                [
                    new("ct-2001", "Harper Lane", "Commercial lead", "harper.lane@northwind.example", "+1 (614) 555-0110", true),
                    new("ct-2002", "Soren Patel", "Billing specialist", "soren.patel@northwind.example", "+1 (614) 555-0111", false)
                ],
                [
                    new("loc-2001", "Primary plant", "service", "Columbus", "OH"),
                    new("loc-2002", "Distribution yard", "shipping", "Newark", "OH")
                ],
                1),
            CreateSeedCustomer(
                "cust-1003",
                "CUS-1003",
                "South Ridge Logistics Partners",
                "South Ridge Logistics",
                "watch",
                "standard",
                "Regional transportation",
                "person-103",
                null,
                null,
                "Elena Park",
                "elena.park@southridge.example",
                1,
                "review",
                "2026-06-08T18:15:00Z",
                "2026-06-10T10:45:00Z",
                ["South Ridge Logistics Partners"],
                "2100 Market St, Memphis, TN 38103",
                "2100 Market St, Memphis, TN 38103",
                "TN-81-9044117",
                "Prepaid",
                "elevated",
                ["Watch list due to delayed document refresh.", "Requires ownership reassignment after transition."],
                [
                    new("ct-3001", "Elena Park", "Customer success", "elena.park@southridge.example", "+1 (901) 555-0122", true),
                    new("ct-3002", "Jordan Price", "Finance contact", "jordan.price@southridge.example", "+1 (901) 555-0123", false)
                ],
                [new("loc-3001", "Operations office", "service", "Memphis", "TN")],
                2)
        ];
    }

    public CustomArrSessionBootstrapResponse BuildSession(
        string userId,
        string personId,
        string tenantId,
        string tenantRoleKey,
        bool isPlatformAdmin,
        IEnumerable<string> entitlements) =>
        new(userId, personId, tenantId, $"session-{userId}", tenantRoleKey, isPlatformAdmin, "customarr", true, entitlements.ToArray());

    public CustomArrDashboardResponse GetDashboard(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            var customers = _customers.Where(customer => CanReadCustomer(principal, customer)).ToArray();
            var recentActivity = customers
                .SelectMany(customer => customer.Activity.Select(activity => new CustomArrDashboardActivityResponse(
                    activity.ActivityId,
                    customer.CustomerId,
                    customer.CustomerNumber,
                    activity.Message,
                    activity.OccurredAt,
                    activity.Kind)))
                .OrderByDescending(activity => activity.OccurredAt)
                .Take(6)
                .ToArray();

            return new CustomArrDashboardResponse(
                DateTimeOffset.UtcNow,
                customers.Length,
                customers.Count(customer => customer.Status is "active"),
                customers.Count(customer => customer.Status is "onboarding"),
                customers.Count(customer => customer.Status is "watch"),
                customers.Sum(customer => customer.Contacts.Count),
                customers.Sum(customer => customer.SiteCount),
                customers.Sum(customer => customer.Requirements.Count),
                customers.Take(3).Select(ProjectSummary).ToArray(),
                recentActivity);
        }
    }

    public IReadOnlyList<CustomArrCustomerDetailResponse> GetCustomers(ClaimsPrincipal principal, string? search = null)
    {
        lock (_gate)
        {
            var query = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            var customers = _customers.AsEnumerable();
            if (query is not null)
            {
                customers = customers.Where(customer =>
                    customer.CustomerNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    customer.LegalName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    customer.TradeName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    customer.Segment.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    customer.PrimaryContactName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    customer.PrimaryContactEmail.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            return customers
                .Where(customer => CanReadCustomer(principal, customer))
                .OrderBy(customer => customer.CustomerNumber)
                .Select(ProjectDetail)
                .ToArray();
        }
    }

    public CustomArrCustomerDetailResponse? GetCustomer(ClaimsPrincipal principal, string customerId)
    {
        lock (_gate)
        {
            var customer = _customers.FirstOrDefault(candidate => string.Equals(candidate.CustomerId, customerId, StringComparison.OrdinalIgnoreCase));
            return customer is null || !CanReadCustomer(principal, customer) ? null : ProjectDetail(customer);
        }
    }

    public CustomArrCustomerDetailResponse CreateCustomer(CustomArrCreateCustomerRequest request)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(request.LegalName))
            {
                throw new InvalidOperationException("Customer legal name is required.");
            }

            var parent = string.IsNullOrWhiteSpace(request.ParentCustomerId)
                ? null
                : _customers.FirstOrDefault(customer => string.Equals(customer.CustomerId, request.ParentCustomerId, StringComparison.OrdinalIgnoreCase));
            var customerNumber = $"CUS-{1000 + _customers.Count + 1}";
            var customerId = $"cust-{Guid.NewGuid():N}"[..13];
            var now = DateTimeOffset.UtcNow;
            var detail = new CustomArrCustomerDetailResponse(
                customerId,
                customerNumber,
                request.LegalName.Trim(),
                string.IsNullOrWhiteSpace(request.TradeName) ? request.LegalName.Trim() : request.TradeName.Trim(),
                request.Status,
                request.Tier,
                request.Segment.Trim(),
                request.OwnerPersonId.Trim(),
                parent?.CustomerId,
                parent?.TradeName,
                request.PrimaryContactName.Trim(),
                request.PrimaryContactEmail.Trim(),
                1,
                1,
                3,
                "clear",
                now,
                now,
                parent is null
                    ? new[] { request.LegalName.Trim() }
                    : parent.HierarchyPath.Append(request.LegalName.Trim()).ToArray(),
                new[] { request.BillingCity.Trim(), request.BillingState.Trim() }.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray().JoinAddress(),
                new[] { request.ShippingCity.Trim(), request.ShippingState.Trim() }.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray().JoinAddress(),
                "pending",
                "Net 30",
                request.Status == "onboarding" ? "medium" : "low",
                string.IsNullOrWhiteSpace(request.Notes)
                    ? new[] { "Created in the CustomArr workspace." }
                    : new[] { request.Notes.Trim() },
                [
                    new CustomArrCustomerContactResponse($"ct-{Guid.NewGuid():N}"[..8], request.PrimaryContactName.Trim(), "Primary contact", request.PrimaryContactEmail.Trim(), request.PrimaryContactPhone.Trim(), true)
                ],
                [
                    new CustomArrCustomerLocationResponse($"loc-{Guid.NewGuid():N}"[..8], "Primary location", "service", request.ShippingCity.Trim().Length > 0 ? request.ShippingCity.Trim() : request.BillingCity.Trim(), request.ShippingState.Trim().Length > 0 ? request.ShippingState.Trim() : request.BillingState.Trim())
                ],
                _requirements.Take(3)
                    .Select((requirement, index) => new CustomArrRequirementProgressResponse($"req-{Guid.NewGuid():N}"[..8], requirement.Title, requirement.OwnerTeam, index == 0 ? "pending" : "watch", null))
                    .ToArray(),
                [
                    new CustomArrActivityResponse($"act-{Guid.NewGuid():N}"[..8], "created", "Customer created in the workspace.", now)
                ]);

            _customers.Insert(0, detail);
            return detail;
        }
    }

    public IReadOnlyList<CustomArrRequirementCatalogItemResponse> GetRequirements()
    {
        lock (_gate)
        {
            return _requirements.ToArray();
        }
    }

    private static bool CanReadCustomer(ClaimsPrincipal principal, CustomArrCustomerDetailResponse customer) =>
        principal.Identity?.IsAuthenticated == true || customer is not null;

    private static CustomArrCustomerSummaryResponse ProjectSummary(CustomArrCustomerDetailResponse customer) =>
        new(
            customer.CustomerId,
            customer.CustomerNumber,
            customer.LegalName,
            customer.TradeName,
            customer.Status,
            customer.Tier,
            customer.Segment,
            customer.OwnerPersonId,
            customer.ParentCustomerId,
            customer.ParentCustomerName,
            customer.PrimaryContactName,
            customer.PrimaryContactEmail,
            customer.SiteCount,
            customer.Contacts.Count,
            customer.Requirements.Count,
            customer.HoldStatus,
            customer.LastActivityAt,
            customer.UpdatedAt);

    private static CustomArrCustomerDetailResponse ProjectDetail(CustomArrCustomerDetailResponse customer) => customer;

    private static CustomArrCustomerDetailResponse CreateSeedCustomer(
        string customerId,
        string customerNumber,
        string legalName,
        string tradeName,
        string status,
        string tier,
        string segment,
        string ownerPersonId,
        string? parentCustomerId,
        string? parentCustomerName,
        string primaryContactName,
        string primaryContactEmail,
        int siteCount,
        string holdStatus,
        string lastActivityAt,
        string updatedAt,
        IReadOnlyList<string> hierarchyPath,
        string billingAddress,
        string shippingAddress,
        string taxId,
        string paymentTerms,
        string riskRating,
        IReadOnlyList<string> notes,
        IReadOnlyList<CustomArrCustomerContactResponse> contacts,
        IReadOnlyList<CustomArrCustomerLocationResponse> locations,
        int requirementSeedIndex)
    {
        var requirements = new List<CustomArrRequirementProgressResponse>();
        for (var index = 0; index < 5 - requirementSeedIndex; index++)
        {
            var requirement = index < 5
                ? index
                : 0;
            var requirementCatalogItem = new[]
            {
                ("cert-insurance", "Certificate of insurance", "Risk"),
                ("tax-w9", "Tax registration / W-9", "Finance"),
                ("code-of-conduct", "Supplier code of conduct", "Procurement"),
                ("billing-terms", "Billing terms acknowledgement", "Accounts receivable"),
                ("e-invoicing", "E-invoicing readiness", "Operations"),
            }[requirement];
            requirements.Add(new CustomArrRequirementProgressResponse(
                $"req-{Guid.NewGuid():N}"[..8],
                requirementCatalogItem.Item2,
                requirementCatalogItem.Item3,
                index == 0 ? "complete" : index == 1 ? "complete" : index == 2 ? "watch" : "pending",
                index == 4 ? DateTimeOffset.Parse("2026-07-15T17:00:00Z") : null));
        }

        var activities = new List<CustomArrActivityResponse>
        {
            new($"act-{Guid.NewGuid():N}"[..8], "status", "Customer status updated.", DateTimeOffset.Parse(lastActivityAt)),
            new($"act-{Guid.NewGuid():N}"[..8], "updated", "Customer profile refreshed.", DateTimeOffset.Parse(updatedAt)),
        };

        return new CustomArrCustomerDetailResponse(
            customerId,
            customerNumber,
            legalName,
            tradeName,
            status,
            tier,
            segment,
            ownerPersonId,
            parentCustomerId,
            parentCustomerName,
            primaryContactName,
            primaryContactEmail,
            siteCount,
            contacts.Count,
            requirements.Count,
            holdStatus,
            DateTimeOffset.Parse(lastActivityAt),
            DateTimeOffset.Parse(updatedAt),
            hierarchyPath,
            billingAddress,
            shippingAddress,
            taxId,
            paymentTerms,
            riskRating,
            notes,
            contacts,
            locations,
            requirements,
            activities);
    }
}

public sealed record CustomArrSessionBootstrapResponse(
    string UserId,
    string PersonId,
    string TenantId,
    string SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    bool HasCustomArrEntitlement,
    IReadOnlyList<string> Entitlements);

public sealed record CustomArrHandoffSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string UserId,
    string PersonId,
    string Email,
    string DisplayName,
    string TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> Entitlements,
    string? CallbackUrl);

public sealed record CustomArrDashboardResponse(
    DateTimeOffset GeneratedAt,
    int CustomerCount,
    int ActiveCustomerCount,
    int OnboardingCustomerCount,
    int WatchListCustomerCount,
    int ContactCount,
    int SiteCount,
    int RequirementCount,
    IReadOnlyList<CustomArrCustomerSummaryResponse> FeaturedCustomers,
    IReadOnlyList<CustomArrDashboardActivityResponse> RecentActivity);

public sealed record CustomArrDashboardActivityResponse(
    string ActivityId,
    string CustomerId,
    string CustomerNumber,
    string Message,
    DateTimeOffset OccurredAt,
    string Kind);

public sealed record CustomArrCustomerSummaryResponse(
    string CustomerId,
    string CustomerNumber,
    string LegalName,
    string TradeName,
    string Status,
    string Tier,
    string Segment,
    string OwnerPersonId,
    string? ParentCustomerId,
    string? ParentCustomerName,
    string PrimaryContactName,
    string PrimaryContactEmail,
    int SiteCount,
    int ContactCount,
    int RequirementCount,
    string HoldStatus,
    DateTimeOffset LastActivityAt,
    DateTimeOffset UpdatedAt);

public sealed record CustomArrCustomerContactResponse(
    string ContactId,
    string Name,
    string Role,
    string Email,
    string Phone,
    bool IsPrimary);

public sealed record CustomArrCustomerLocationResponse(
    string LocationId,
    string Label,
    string Type,
    string City,
    string State);

public sealed record CustomArrRequirementProgressResponse(
    string RequirementKey,
    string Title,
    string Owner,
    string Status,
    DateTimeOffset? DueAt);

public sealed record CustomArrActivityResponse(
    string ActivityId,
    string Kind,
    string Message,
    DateTimeOffset OccurredAt);

public sealed record CustomArrRequirementCatalogItemResponse(
    string RequirementKey,
    string Title,
    string Description,
    string Status,
    string OwnerTeam,
    IReadOnlyList<string> AppliesTo);

public sealed record CustomArrCustomerDetailResponse(
    string CustomerId,
    string CustomerNumber,
    string LegalName,
    string TradeName,
    string Status,
    string Tier,
    string Segment,
    string OwnerPersonId,
    string? ParentCustomerId,
    string? ParentCustomerName,
    string PrimaryContactName,
    string PrimaryContactEmail,
    int SiteCount,
    int ContactCount,
    int RequirementCount,
    string HoldStatus,
    DateTimeOffset LastActivityAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<string> HierarchyPath,
    string BillingAddress,
    string ShippingAddress,
    string TaxId,
    string PaymentTerms,
    string RiskRating,
    IReadOnlyList<string> Notes,
    IReadOnlyList<CustomArrCustomerContactResponse> Contacts,
    IReadOnlyList<CustomArrCustomerLocationResponse> Locations,
    IReadOnlyList<CustomArrRequirementProgressResponse> Requirements,
    IReadOnlyList<CustomArrActivityResponse> Activity);

public sealed record CustomArrCreateCustomerRequest(
    string LegalName,
    string TradeName,
    string Status,
    string Tier,
    string Segment,
    string OwnerPersonId,
    string ParentCustomerId,
    string PrimaryContactName,
    string PrimaryContactEmail,
    string PrimaryContactPhone,
    string BillingCity,
    string BillingState,
    string ShippingCity,
    string ShippingState,
    string Notes);

internal static class CustomArrStoreExtensions
{
    public static string JoinAddress(this IEnumerable<string> parts) =>
        string.Join(", ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
}
