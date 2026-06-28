using System.Security.Claims;
using CustomArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace CustomArr.Api.Data;

public sealed class CustomArrStore
{
    private const string PortalSubmissionCreateOperation = "customarr.portal_order_submission.create";
    private const string CustomerCreateOperation = "customarr.customer.create";
    private readonly CustomArrDbContext db;

    public CustomArrStore(CustomArrDbContext db)
    {
        this.db = db;
    }

    public CustomArrSessionBootstrapResponse BuildSession(
        string userId,
        string personId,
        string tenantId,
        string tenantRoleKey,
        bool isPlatformAdmin,
        IEnumerable<string> launchableProductKeys) =>
        new(userId, personId, tenantId, $"session-{userId}", tenantRoleKey, isPlatformAdmin, "customarr", launchableProductKeys.ToArray());

    public CustomArrDashboardResponse GetDashboard(ClaimsPrincipal principal)
    {
        EnsureEntitled(principal);
        var tenantId = principal.GetTenantId();
        EnsureSeeded(tenantId);

        var customers = QueryCustomers(tenantId).ToArray();
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

        var parentNames = BuildCustomerNameMap(tenantId);

        return new CustomArrDashboardResponse(
            DateTimeOffset.UtcNow,
            customers.Length,
            customers.Count(customer => customer.StatusKey is "active"),
            customers.Count(customer => customer.StatusKey is "prospect" or "onboarding"),
            customers.Count(customer => customer.StatusKey is "on_hold" or "blocked" or "watch"),
            customers.Sum(customer => customer.Contacts.Count),
            customers.Sum(customer => customer.Addresses.Count),
            customers.Sum(customer => customer.Requirements.Count),
            customers.Take(3).Select(customer => ProjectSummary(customer, parentNames)).ToArray(),
            recentActivity);
    }

    public IReadOnlyList<CustomArrCustomerDetailResponse> GetCustomers(ClaimsPrincipal principal, string? search = null)
    {
        EnsureEntitled(principal);
        var tenantId = principal.GetTenantId();
        EnsureSeeded(tenantId);

        var query = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var customers = QueryCustomers(tenantId);
        if (query is not null)
        {
            customers = customers.Where(customer =>
                customer.CustomerNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                customer.CustomerCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                customer.LegalName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                customer.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (customer.DbaName ?? string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                customer.CustomerTypeKey.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                customer.StatusKey.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                customer.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                customer.Contacts.Any(contact =>
                    contact.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    contact.Email.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        var parentNames = BuildCustomerNameMap(tenantId);
        return customers
            .OrderBy(customer => customer.CustomerNumber)
            .Select(customer => ProjectDetail(customer, parentNames))
            .ToArray();
    }

    public CustomArrCustomerDetailResponse? GetCustomer(ClaimsPrincipal principal, string customerId)
    {
        EnsureEntitled(principal);
        var tenantId = principal.GetTenantId();
        EnsureSeeded(tenantId);

        var customer = QueryCustomers(tenantId)
            .FirstOrDefault(candidate => string.Equals(candidate.CustomerId, customerId, StringComparison.OrdinalIgnoreCase));
        if (customer is null)
        {
            return null;
        }

        return ProjectDetail(customer, BuildCustomerNameMap(tenantId));
    }

    public CustomArrCustomerDetailResponse CreateCustomer(
        ClaimsPrincipal principal,
        CustomArrCreateCustomerRequest request,
        string? idempotencyKey)
    {
        EnsureEntitled(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("customarr.idempotency_key_required", "Idempotency-Key header is required to create a customer.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.LegalName))
        {
            throw new StlApiException("customarr.legal_name_required", "Customer legal name is required.", 400);
        }

        var tenantId = principal.GetTenantId();
        EnsureSeeded(tenantId);

        var scopedKey = idempotencyKey.Trim();
        var existing = db.IdempotencyRecords.AsNoTracking()
            .FirstOrDefault(record =>
                record.TenantId == tenantId &&
                record.OperationKey == CustomerCreateOperation &&
                record.IdempotencyKey == scopedKey);
        if (existing is not null)
        {
            return GetCustomer(principal, existing.ResourceId)
                ?? throw new StlApiException("customarr.idempotency_resource_missing", "The idempotent customer create result is no longer available.", 409);
        }

        var parent = string.IsNullOrWhiteSpace(request.ParentCustomerId)
            ? null
            : db.Customers.FirstOrDefault(customer =>
                customer.TenantId == tenantId &&
                customer.CustomerId == request.ParentCustomerId.Trim());
        if (!string.IsNullOrWhiteSpace(request.ParentCustomerId) && parent is null)
        {
            throw new StlApiException("customarr.parent_customer_not_found", "Parent customer must be an existing CustomArr customer in the same tenant.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        var actorPersonId = principal.GetPersonId().ToString("D");
        CustomArrTenantSettingsDefaults.EnsureSeeded(db, tenantId, actorPersonId);
        var customerId = $"cust-{Guid.NewGuid():N}"[..18];
        var customerNumber = NextCustomerNumber(tenantId);
        var displayName = FirstNonEmpty(request.DisplayName, request.TradeName, request.DbaName, request.LegalName).Trim();
        var customerTypeKey = NormalizeKey(FirstNonEmpty(request.CustomerTypeKey, request.Tier, "standard"), "standard");
        var statusKey = NormalizeStatusKey(FirstNonEmpty(request.StatusKey, request.Status, "prospect"));
        var sourceKey = NormalizeKey(FirstNonEmpty(request.SourceKey, "manual"), "manual");
        var accountOwnerRef = FirstNonEmpty(request.AccountOwnerPersonId, request.OwnerPersonId, actorPersonId).Trim();
        ValidateCustomerCreateAgainstTenantSettings(tenantId, request, statusKey, customerTypeKey, accountOwnerRef);

        var contact = BuildContact(
            tenantId,
            customerId,
            request.PrimaryContactName,
            request.PrimaryContactEmail,
            request.PrimaryContactPhone,
            isPrimary: true,
            portalAccessEnabled: request.PortalEnabled);
        var billingAddress = BuildAddress(
            tenantId,
            customerId,
            "billing",
            "Billing",
            request.BillingCity,
            request.BillingState,
            isDefaultBilling: true);
        var shippingAddress = BuildAddress(
            tenantId,
            customerId,
            "shipping",
            "Shipping",
            request.ShippingCity,
            request.ShippingState,
            isDefaultShipping: true);

        var customer = new CustomArrCustomer
        {
            CustomerId = customerId,
            TenantId = tenantId,
            CustomerNumber = customerNumber,
            CustomerCode = customerNumber,
            LegalName = request.LegalName.Trim(),
            DisplayName = displayName,
            DbaName = string.IsNullOrWhiteSpace(request.DbaName) ? request.TradeName.NullIfWhiteSpace() : request.DbaName.Trim(),
            CustomerTypeKey = customerTypeKey,
            StatusKey = statusKey,
            ParentCustomerId = parent?.CustomerId,
            PrimaryContactId = contact.ContactId,
            PrimaryBillingAddressId = billingAddress.AddressId,
            PrimaryShippingAddressId = shippingAddress.AddressId,
            PrimaryServiceAddressId = shippingAddress.AddressId,
            AccountOwnerPersonId = accountOwnerRef,
            AssignedTeamId = request.AssignedTeamId.NullIfWhiteSpace(),
            CustomerSinceDate = request.CustomerSinceDate,
            SourceKey = sourceKey,
            Notes = request.Notes?.Trim() ?? string.Empty,
            Tags = NormalizeTags(request.Tags, request.Segment),
            HoldStatusKey = statusKey is "on_hold" or "blocked" ? "hold" : "clear",
            RiskRatingKey = statusKey is "prospect" or "onboarding" ? "medium" : "low",
            PortalEnabled = request.PortalEnabled,
            PortalDisplayName = request.PortalDisplayName.NullIfWhiteSpace() ?? displayName,
            AllowPortalOrderCreate = request.PortalEnabled,
            AllowPortalDocumentUpload = request.PortalEnabled,
            AllowPortalStatusView = request.PortalEnabled,
            DefaultPortalContactId = request.PortalEnabled ? contact.ContactId : null,
            PortalInviteStatusKey = request.PortalEnabled ? "invited" : "not_invited",
            DefaultOrderTypeKey = NormalizeKey(FirstNonEmpty(request.DefaultOrderTypeKey, "customer_order"), "customer_order"),
            DefaultServiceLevelKey = NormalizeKey(FirstNonEmpty(request.DefaultServiceLevelKey, "standard"), "standard"),
            DefaultContactId = contact.ContactId,
            DefaultDeliveryAddressId = shippingAddress.AddressId,
            RequiresAppointment = request.RequiresAppointment,
            RequiresProofOfDelivery = request.RequiresProofOfDelivery,
            RequiresCustomerReference = request.RequiresCustomerReference,
            CustomerReferenceLabel = request.CustomerReferenceLabel.NullIfWhiteSpace(),
            DefaultInstructions = request.DefaultInstructions.NullIfWhiteSpace(),
            NotificationPreferenceKey = NormalizeKey(FirstNonEmpty(request.NotificationPreferenceKey, "email"), "email"),
            CreatedAt = now,
            CreatedByPersonId = actorPersonId,
            UpdatedAt = now,
            UpdatedByPersonId = actorPersonId,
            Contacts = [contact],
            Addresses = [billingAddress, shippingAddress],
            BillingProfiles =
            [
                new()
                {
                    BillingProfileId = $"bill-{Guid.NewGuid():N}"[..18],
                    TenantId = tenantId,
                    CustomerId = customerId,
                    BillingContactId = contact.ContactId,
                    BillingAddressId = billingAddress.AddressId,
                    PaymentTermsKey = NormalizeKey(FirstNonEmpty(request.PaymentTermsKey, "net_30"), "net_30"),
                    InvoiceDeliveryMethodKey = "email",
                    BillingEmail = request.PrimaryContactEmail.NullIfWhiteSpace(),
                    CurrencyCode = "USD",
                    CreditStatusKey = "good_standing"
                }
            ],
            Requirements = BuildDefaultRequirements(tenantId, customerId, statusKey),
            Activity =
            [
                new()
                {
                    ActivityId = $"act-{Guid.NewGuid():N}"[..18],
                    TenantId = tenantId,
                    CustomerId = customerId,
                    Kind = "created",
                    Message = "Canonical customer relationship created in CustomArr.",
                    SourceProductKey = StlProductKeys.CustomArr,
                    ActorPersonId = actorPersonId,
                    OccurredAt = now
                }
            ]
        };

        if (parent is not null)
        {
            customer.Relationships.Add(new CustomArrCustomerRelationship
            {
                RelationshipId = $"rel-{Guid.NewGuid():N}"[..18],
                TenantId = tenantId,
                CustomerId = customer.CustomerId,
                RelatedCustomerId = parent.CustomerId,
                RelationshipTypeKey = "parent",
                EffectiveDate = now
            });
        }

        db.Customers.Add(customer);
        db.IdempotencyRecords.Add(new CustomArrIdempotencyRecord
        {
            IdempotencyRecordId = $"idem-{Guid.NewGuid():N}"[..18],
            TenantId = tenantId,
            OperationKey = CustomerCreateOperation,
            IdempotencyKey = scopedKey,
            ResourceId = customer.CustomerId,
            CreatedAt = now
        });
        db.SaveChanges();

        return ProjectDetail(customer, BuildCustomerNameMap(tenantId));
    }

    public IReadOnlyList<CustomArrRequirementCatalogItemResponse> GetRequirements()
    {
        return RequirementCatalog.Select(item => new CustomArrRequirementCatalogItemResponse(
            item.RequirementKey,
            item.Title,
            item.Description,
            item.Status,
            item.OwnerTeam,
            item.AppliesTo)).ToArray();
    }

    public CustomArrPortalSubmissionResponse CreatePortalOrderSubmission(
        ClaimsPrincipal principal,
        CustomArrPortalOrderSubmissionRequest request,
        string? idempotencyKey)
    {
        EnsureEntitled(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("customarr.idempotency_key_required", "Idempotency-Key header is required to create a portal order submission.", 400);
        }

        var tenantId = principal.GetTenantId();
        EnsureSeeded(tenantId);
        var scopedKey = idempotencyKey.Trim();
        var existing = db.IdempotencyRecords.AsNoTracking()
            .FirstOrDefault(record =>
                record.TenantId == tenantId &&
                record.OperationKey == PortalSubmissionCreateOperation &&
                record.IdempotencyKey == scopedKey);
        if (existing is not null)
        {
            return ProjectPortalSubmission(
                db.PortalSubmissions.AsNoTracking().Single(submission => submission.SubmissionId == existing.ResourceId));
        }

        var customer = QueryCustomers(tenantId)
            .FirstOrDefault(candidate => string.Equals(candidate.CustomerId, request.CustomerId, StringComparison.OrdinalIgnoreCase));
        if (customer is null)
        {
            throw new StlApiException("customarr.customer_not_found", "Portal order submission requires an existing CustomArr customer.", 404);
        }

        var address = ResolveSubmissionAddress(customer, request.CustomerAddressId);
        var now = DateTimeOffset.UtcNow;
        var requestType = NormalizeRequestType(request.RequestType);
        var submission = new CustomArrPortalSubmission
        {
            SubmissionId = $"portal-{Guid.NewGuid():N}"[..18],
            TenantId = tenantId,
            Status = "created",
            CreatedEventType = StlSuiteEventCatalog.CustomArr.PortalSubmissionCreated,
            CustomerId = customer.CustomerId,
            CustomerNumberSnapshot = customer.CustomerNumber,
            CustomerNameSnapshot = string.IsNullOrWhiteSpace(request.CustomerName) ? customer.DisplayName : request.CustomerName.Trim(),
            CustomerAddressId = address?.AddressId,
            CustomerAddressSnapshot = address is null ? null : FormatAddress(address),
            RequestType = requestType,
            OwnerPersonId = string.IsNullOrWhiteSpace(request.OwnerPersonId) ? principal.GetPersonId().ToString("D") : request.OwnerPersonId.Trim(),
            Summary = request.Summary.Trim(),
            RequestedWindowStart = request.RequestedWindowStart,
            RequestedWindowEnd = request.RequestedWindowEnd,
            PromisedWindowStart = request.PromisedWindowStart,
            PromisedWindowEnd = request.PromisedWindowEnd,
            FulfillmentProductKeys = NormalizeFulfillmentProductKeys(request.FulfillmentProductKeys).ToArray(),
            SubmittedAt = now,
            UpdatedAt = now
        };

        db.PortalSubmissions.Add(submission);
        db.IdempotencyRecords.Add(new CustomArrIdempotencyRecord
        {
            IdempotencyRecordId = $"idem-{Guid.NewGuid():N}"[..18],
            TenantId = tenantId,
            OperationKey = PortalSubmissionCreateOperation,
            IdempotencyKey = scopedKey,
            ResourceId = submission.SubmissionId,
            CreatedAt = now
        });
        db.SaveChanges();

        return ProjectPortalSubmission(submission);
    }

    public CustomArrPortalSubmissionResponse? MarkPortalSubmissionForwarded(
        ClaimsPrincipal principal,
        string submissionId,
        string orderId,
        string orderNumber)
    {
        EnsureEntitled(principal);
        var tenantId = principal.GetTenantId();
        var submission = db.PortalSubmissions
            .FirstOrDefault(candidate =>
                candidate.TenantId == tenantId &&
                string.Equals(candidate.SubmissionId, submissionId, StringComparison.OrdinalIgnoreCase));
        if (submission is null)
        {
            return null;
        }

        submission.Status = "forwarded_to_ordarr";
        submission.UpdatedAt = DateTimeOffset.UtcNow;
        submission.OrdArrOrderId = orderId;
        submission.OrdArrOrderNumber = orderNumber;
        db.SaveChanges();
        return ProjectPortalSubmission(submission);
    }

    public IReadOnlyList<CustomArrPortalSubmissionResponse> ListPortalSubmissions(ClaimsPrincipal principal)
    {
        EnsureEntitled(principal);
        var tenantId = principal.GetTenantId();
        EnsureSeeded(tenantId);

        return db.PortalSubmissions
            .AsNoTracking()
            .Where(submission => submission.TenantId == tenantId)
            .OrderByDescending(submission => submission.SubmittedAt)
            .Select(ProjectPortalSubmission)
            .ToArray();
    }

    private IQueryable<CustomArrCustomer> QueryCustomers(Guid tenantId) =>
        db.Customers
            .AsSplitQuery()
            .Include(customer => customer.Contacts)
            .Include(customer => customer.Addresses)
            .Include(customer => customer.Identifiers)
            .Include(customer => customer.BillingProfiles)
            .Include(customer => customer.Requirements)
            .Include(customer => customer.ExternalRefs)
            .Include(customer => customer.Relationships)
            .Include(customer => customer.CustomFieldValues)
            .Include(customer => customer.Activity)
            .Where(customer => customer.TenantId == tenantId);

    private void EnsureSeeded(Guid tenantId)
    {
        if (db.Customers.Any(customer => customer.TenantId == tenantId))
        {
            return;
        }

        db.Customers.AddRange(BuildSeedCustomers(tenantId));
        db.SaveChanges();
    }

    private static IReadOnlyList<CustomArrCustomer> BuildSeedCustomers(Guid tenantId)
    {
        var acme = SeedCustomer(
            tenantId,
            "cust-1001",
            "CUST-001001",
            "Acme Freight Systems LLC",
            "Acme Freight",
            "Acme Freight",
            "business",
            "active",
            null,
            "person-101",
            "team-customer-success",
            DateTimeOffset.Parse("2024-03-01T00:00:00Z"),
            "migration",
            ["strategic", "enterprise_logistics"],
            "clear",
            "low",
            "Strategic customer with quarterly review cadence.\nPrimary onboarding and contract data is complete.",
            [
                ("ct-1001", "Maria", "Jensen", "Primary procurement contact", "Procurement", "maria.jensen@acmefreight.example", "+1 (972) 555-0191", true, true, true, false),
                ("ct-1002", "Derek", "Holt", "Accounts payable", "Finance", "derek.holt@acmefreight.example", "+1 (972) 555-0192", false, true, false, false),
                ("ct-1003", "Nina", "Rao", "Operations manager", "Operations", "nina.rao@acmefreight.example", "+1 (972) 555-0193", false, false, true, true)
            ],
            [
                ("loc-1001", "billing", "HQ", "410 Harbor Ave", "", "Dallas", "TX", "75201", true, false, false),
                ("loc-1002", "shipping", "Cross dock", "920 Terminal Loop", "", "Fort Worth", "TX", "76102", false, true, false),
                ("loc-1003", "service", "Service center", "1800 Fleet Way", "", "Arlington", "TX", "76010", false, false, true)
            ],
            "net_30",
            "good_standing",
            "TX-45-9988123",
            "qb",
            "QB-CUST-1001");

        var northwind = SeedCustomer(
            tenantId,
            "cust-1002",
            "CUST-001002",
            "Northwind Components Inc.",
            "Northwind Components",
            null,
            "business",
            "onboarding",
            "cust-1001",
            "person-102",
            "team-onboarding",
            DateTimeOffset.Parse("2026-05-15T00:00:00Z"),
            "manual",
            ["core", "industrial_supply"],
            "watch",
            "medium",
            "Pending e-invoicing confirmation.\nRequires final legal review before activation.",
            [
                ("ct-2001", "Harper", "Lane", "Commercial lead", "Operations", "harper.lane@northwind.example", "+1 (614) 555-0110", true, false, true, false),
                ("ct-2002", "Soren", "Patel", "Billing specialist", "Finance", "soren.patel@northwind.example", "+1 (614) 555-0111", false, true, false, false)
            ],
            [
                ("loc-2001", "service", "Primary plant", "88 Foundry Rd", "", "Columbus", "OH", "43085", false, false, true),
                ("loc-2002", "shipping", "Distribution yard", "701 Distribution Dr", "", "Newark", "OH", "43055", false, true, false)
            ],
            "net_45",
            "review",
            "OH-29-1188221",
            "erp",
            "ERP-NW-1002");

        var southridge = SeedCustomer(
            tenantId,
            "cust-1003",
            "CUST-001003",
            "South Ridge Logistics Partners",
            "South Ridge Logistics",
            null,
            "carrier",
            "on_hold",
            null,
            "person-103",
            "team-customer-success",
            DateTimeOffset.Parse("2025-11-07T00:00:00Z"),
            "import",
            ["standard", "regional_transportation"],
            "review",
            "elevated",
            "Watch list due to delayed document refresh.\nRequires ownership reassignment after transition.",
            [
                ("ct-3001", "Elena", "Park", "Customer success", "Operations", "elena.park@southridge.example", "+1 (901) 555-0122", true, false, true, false),
                ("ct-3002", "Jordan", "Price", "Finance contact", "Finance", "jordan.price@southridge.example", "+1 (901) 555-0123", false, true, false, false)
            ],
            [
                ("loc-3001", "service", "Operations office", "2100 Market St", "", "Memphis", "TN", "38103", false, false, true)
            ],
            "prepaid",
            "credit_hold",
            "TN-81-9044117",
            "legacy_crm",
            "SRLP-3001");

        northwind.Relationships.Add(new CustomArrCustomerRelationship
        {
            RelationshipId = "rel-2001",
            TenantId = tenantId,
            CustomerId = northwind.CustomerId,
            RelatedCustomerId = acme.CustomerId,
            RelationshipTypeKey = "parent",
            EffectiveDate = DateTimeOffset.Parse("2026-05-15T00:00:00Z")
        });

        return [acme, northwind, southridge];
    }

    private static CustomArrCustomer SeedCustomer(
        Guid tenantId,
        string customerId,
        string customerNumber,
        string legalName,
        string displayName,
        string? dbaName,
        string customerTypeKey,
        string statusKey,
        string? parentCustomerId,
        string accountOwnerPersonId,
        string assignedTeamId,
        DateTimeOffset customerSinceDate,
        string sourceKey,
        string[] tags,
        string holdStatusKey,
        string riskRatingKey,
        string notes,
        IReadOnlyList<(string ContactId, string FirstName, string LastName, string Title, string Department, string Email, string Phone, bool Primary, bool Billing, bool Ordering, bool Shipping)> contacts,
        IReadOnlyList<(string AddressId, string Type, string Name, string Line1, string Line2, string City, string State, string Postal, bool Billing, bool Shipping, bool Service)> addresses,
        string paymentTermsKey,
        string creditStatusKey,
        string taxId,
        string externalSystemKey,
        string externalId)
    {
        var now = DateTimeOffset.Parse("2026-06-11T13:05:00Z");
        var customer = new CustomArrCustomer
        {
            CustomerId = customerId,
            TenantId = tenantId,
            CustomerNumber = customerNumber,
            CustomerCode = customerNumber,
            LegalName = legalName,
            DisplayName = displayName,
            DbaName = dbaName,
            CustomerTypeKey = customerTypeKey,
            StatusKey = statusKey,
            ParentCustomerId = parentCustomerId,
            AccountOwnerPersonId = accountOwnerPersonId,
            AssignedTeamId = assignedTeamId,
            CustomerSinceDate = customerSinceDate,
            SourceKey = sourceKey,
            Notes = notes,
            Tags = tags,
            HoldStatusKey = holdStatusKey,
            RiskRatingKey = riskRatingKey,
            PortalEnabled = true,
            PortalDisplayName = displayName,
            AllowPortalOrderCreate = true,
            AllowPortalDocumentUpload = true,
            AllowPortalStatusView = true,
            PortalInviteStatusKey = statusKey == "active" ? "active" : "invited",
            DefaultOrderTypeKey = "customer_order",
            DefaultServiceLevelKey = tags.Contains("strategic") ? "expedited" : "standard",
            RequiresAppointment = true,
            RequiresProofOfDelivery = true,
            RequiresCustomerReference = true,
            CustomerReferenceLabel = "PO Number",
            DefaultInstructions = "Confirm dock or receiving window before dispatch.",
            NotificationPreferenceKey = "email",
            CreatedAt = now.AddDays(-4),
            CreatedByPersonId = "person-system",
            UpdatedAt = now,
            UpdatedByPersonId = accountOwnerPersonId
        };

        customer.Contacts = contacts.Select(contact => new CustomArrCustomerContact
        {
            ContactId = contact.ContactId,
            TenantId = tenantId,
            CustomerId = customerId,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            DisplayName = $"{contact.FirstName} {contact.LastName}",
            Title = contact.Title,
            Department = contact.Department,
            Email = contact.Email,
            Phone = contact.Phone,
            PreferredContactMethodKey = "email",
            Timezone = "America/Chicago",
            IsPrimary = contact.Primary,
            IsBillingContact = contact.Billing,
            IsOrderingContact = contact.Ordering,
            IsShippingContact = contact.Shipping,
            PortalAccessEnabled = contact.Primary,
            PortalRoleKey = contact.Primary ? "portal_admin" : null,
            StatusKey = "active",
            LastVerifiedAt = now.AddDays(-1)
        }).ToList();

        customer.Addresses = addresses.Select(address => new CustomArrCustomerAddress
        {
            AddressId = address.AddressId,
            TenantId = tenantId,
            CustomerId = customerId,
            AddressTypeKey = address.Type,
            LocationName = address.Name,
            Line1 = address.Line1,
            Line2 = address.Line2.NullIfWhiteSpace(),
            City = address.City,
            StateProvince = address.State,
            PostalCode = address.Postal,
            CountryCode = "US",
            Timezone = "America/Chicago",
            DeliveryInstructions = "Use customer receiving instructions before arrival.",
            AppointmentRequired = true,
            ReceivingHours = "Mon-Fri 08:00-16:00 local",
            DockDoorNotes = address.Type is "shipping" or "service" ? "Confirm assigned dock on arrival." : null,
            IsDefaultBilling = address.Billing,
            IsDefaultShipping = address.Shipping,
            IsDefaultService = address.Service,
            StatusKey = "active"
        }).ToList();

        customer.PrimaryContactId = customer.Contacts.First(contact => contact.IsPrimary).ContactId;
        customer.DefaultContactId = customer.PrimaryContactId;
        customer.DefaultPortalContactId = customer.PrimaryContactId;
        customer.PrimaryBillingAddressId = customer.Addresses.FirstOrDefault(address => address.IsDefaultBilling)?.AddressId;
        customer.PrimaryShippingAddressId = customer.Addresses.FirstOrDefault(address => address.IsDefaultShipping)?.AddressId;
        customer.PrimaryServiceAddressId = customer.Addresses.FirstOrDefault(address => address.IsDefaultService)?.AddressId;
        customer.DefaultPickupAddressId = customer.PrimaryServiceAddressId;
        customer.DefaultDeliveryAddressId = customer.PrimaryShippingAddressId;

        customer.Identifiers =
        [
            new()
            {
                IdentifierId = $"id-{customerId}",
                TenantId = tenantId,
                CustomerId = customerId,
                IdentifierTypeKey = "state_tax_id",
                IdentifierValue = taxId,
                JurisdictionKey = customer.Addresses.First().StateProvince.ToLowerInvariant(),
                IssuingAuthority = "State tax authority",
                VerificationStatusKey = statusKey == "active" ? "verified" : "unverified",
                EffectiveDate = customerSinceDate
            }
        ];

        customer.BillingProfiles =
        [
            new()
            {
                BillingProfileId = $"bill-{customerId}",
                TenantId = tenantId,
                CustomerId = customerId,
                BillingContactId = customer.Contacts.First(contact => contact.IsBillingContact).ContactId,
                BillingAddressId = customer.PrimaryBillingAddressId,
                PaymentTermsKey = paymentTermsKey,
                InvoiceDeliveryMethodKey = "email",
                BillingEmail = customer.Contacts.First(contact => contact.IsBillingContact).Email,
                PurchaseOrderRequired = true,
                TaxExempt = false,
                CurrencyCode = "USD",
                CreditStatusKey = creditStatusKey,
                ExternalAccountingCustomerRef = externalId
            }
        ];

        customer.Requirements = BuildDefaultRequirements(tenantId, customerId, statusKey);
        customer.ExternalRefs =
        [
            new()
            {
                ExternalRefId = $"xref-{customerId}",
                TenantId = tenantId,
                CustomerId = customerId,
                SystemKey = externalSystemKey,
                ExternalId = externalId,
                ExternalCode = externalId,
                LastSyncedAt = now,
                SyncStatusKey = "synced"
            }
        ];
        customer.CustomFieldValues =
        [
            new()
            {
                FieldValueId = $"cfv-{customerId}",
                TenantId = tenantId,
                FieldDefinitionId = "preferred-review-cadence",
                CustomerId = customerId,
                ValueOptionKey = tags.Contains("strategic") ? "quarterly" : "annual",
                SourceKey = "manual",
                LastVerifiedAt = now,
                UpdatedByPersonId = accountOwnerPersonId
            }
        ];
        customer.Activity =
        [
            new()
            {
                ActivityId = $"act-{customerId}-1",
                TenantId = tenantId,
                CustomerId = customerId,
                Kind = "status",
                Message = "Customer status updated.",
                SourceProductKey = StlProductKeys.CustomArr,
                ActorPersonId = accountOwnerPersonId,
                OccurredAt = now.AddHours(-2)
            },
            new()
            {
                ActivityId = $"act-{customerId}-2",
                TenantId = tenantId,
                CustomerId = customerId,
                Kind = "updated",
                Message = "Customer profile refreshed.",
                SourceProductKey = StlProductKeys.CustomArr,
                ActorPersonId = accountOwnerPersonId,
                OccurredAt = now
            }
        ];

        return customer;
    }

    private static List<CustomArrCustomerRequirement> BuildDefaultRequirements(Guid tenantId, string customerId, string statusKey) =>
        RequirementCatalog.Select((requirement, index) => new CustomArrCustomerRequirement
        {
            RequirementId = $"req-{customerId}-{index + 1}",
            TenantId = tenantId,
            CustomerId = customerId,
            RequirementTypeKey = requirement.RequirementKey,
            RequirementName = requirement.Title,
            Description = requirement.Description,
            RequiredBeforeKey = index < 2 ? "before_activation" : "before_order_creation",
            RecordArrDocumentId = statusKey == "active" && index < 2 ? $"record-{customerId}-{index + 1}" : null,
            ComplianceCoreRuleRef = requirement.RequirementKey == "cert-insurance" ? "compliancecore.customer.insurance" : null,
            StatusKey = statusKey == "active" && index < 2 ? "accepted" : index == 2 ? "pending_review" : "missing",
            EffectiveDate = DateTimeOffset.Parse("2026-06-01T00:00:00Z"),
            ExpirationDate = index == 0 ? DateTimeOffset.Parse("2026-12-31T00:00:00Z") : null,
            ReviewedByPersonId = statusKey == "active" && index < 2 ? "person-101" : null,
            ReviewedAt = statusKey == "active" && index < 2 ? DateTimeOffset.Parse("2026-06-10T00:00:00Z") : null,
            OwnerTeam = requirement.OwnerTeam
        }).ToList();

    private static CustomArrCustomerContact BuildContact(
        Guid tenantId,
        string customerId,
        string? displayName,
        string? email,
        string? phone,
        bool isPrimary,
        bool portalAccessEnabled)
    {
        var (firstName, lastName) = SplitName(displayName);
        return new CustomArrCustomerContact
        {
            ContactId = $"ct-{Guid.NewGuid():N}"[..18],
            TenantId = tenantId,
            CustomerId = customerId,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Primary contact" : displayName.Trim(),
            Title = "Primary contact",
            Email = email?.Trim() ?? string.Empty,
            Phone = phone?.Trim() ?? string.Empty,
            PreferredContactMethodKey = "email",
            IsPrimary = isPrimary,
            IsBillingContact = true,
            IsOrderingContact = true,
            IsShippingContact = true,
            PortalAccessEnabled = portalAccessEnabled,
            PortalRoleKey = portalAccessEnabled ? "portal_admin" : null,
            StatusKey = "active",
            LastVerifiedAt = DateTimeOffset.UtcNow
        };
    }

    private static CustomArrCustomerAddress BuildAddress(
        Guid tenantId,
        string customerId,
        string typeKey,
        string locationName,
        string? city,
        string? stateProvince,
        bool isDefaultBilling = false,
        bool isDefaultShipping = false)
    {
        return new CustomArrCustomerAddress
        {
            AddressId = $"addr-{Guid.NewGuid():N}"[..18],
            TenantId = tenantId,
            CustomerId = customerId,
            AddressTypeKey = typeKey,
            LocationName = locationName,
            City = city?.Trim() ?? string.Empty,
            StateProvince = stateProvince?.Trim() ?? string.Empty,
            CountryCode = "US",
            IsDefaultBilling = isDefaultBilling,
            IsDefaultShipping = isDefaultShipping,
            IsDefaultService = !isDefaultBilling,
            StatusKey = "active"
        };
    }

    private static CustomArrCustomerSummaryResponse ProjectSummary(
        CustomArrCustomer customer,
        IReadOnlyDictionary<string, string> customerNames)
    {
        var primaryContact = ResolvePrimaryContact(customer);
        var primaryBilling = customer.Addresses.FirstOrDefault(address => address.AddressId == customer.PrimaryBillingAddressId);
        var primaryShipping = customer.Addresses.FirstOrDefault(address => address.AddressId == customer.PrimaryShippingAddressId);
        var billingProfile = customer.BillingProfiles.FirstOrDefault();

        return new CustomArrCustomerSummaryResponse(
            customer.CustomerId,
            customer.TenantId,
            customer.CustomerNumber,
            customer.CustomerCode,
            customer.LegalName,
            customer.DisplayName,
            customer.DbaName,
            customer.CustomerTypeKey,
            customer.StatusKey,
            customer.DisplayName,
            customer.StatusKey,
            customer.CustomerTypeKey,
            SegmentFromTags(customer),
            customer.AccountOwnerPersonId ?? string.Empty,
            customer.ParentCustomerId,
            ResolveCustomerName(customer.ParentCustomerId, customerNames),
            primaryContact?.DisplayName ?? string.Empty,
            primaryContact?.Email ?? string.Empty,
            customer.Addresses.Count,
            customer.Contacts.Count,
            customer.Requirements.Count,
            customer.HoldStatusKey,
            LastActivityAt(customer),
            customer.UpdatedAt,
            customer.PrimaryContactId,
            customer.PrimaryBillingAddressId,
            customer.PrimaryShippingAddressId,
            customer.PrimaryServiceAddressId,
            customer.AssignedTeamId,
            customer.CustomerSinceDate,
            customer.SourceKey,
            customer.Tags,
            FormatAddress(primaryBilling),
            FormatAddress(primaryShipping),
            billingProfile?.PaymentTermsKey ?? "net_30",
            customer.RiskRatingKey);
    }

    private static CustomArrCustomerDetailResponse ProjectDetail(
        CustomArrCustomer customer,
        IReadOnlyDictionary<string, string> customerNames)
    {
        var summary = ProjectSummary(customer, customerNames);
        var hierarchyPath = BuildHierarchyPath(customer, customerNames);

        return new CustomArrCustomerDetailResponse(
            summary.CustomerId,
            summary.TenantId,
            summary.CustomerNumber,
            summary.CustomerCode,
            summary.LegalName,
            summary.DisplayName,
            summary.DbaName,
            summary.CustomerTypeKey,
            summary.StatusKey,
            summary.TradeName,
            summary.Status,
            summary.Tier,
            summary.Segment,
            summary.OwnerPersonId,
            summary.ParentCustomerId,
            summary.ParentCustomerName,
            summary.PrimaryContactName,
            summary.PrimaryContactEmail,
            summary.SiteCount,
            summary.ContactCount,
            summary.RequirementCount,
            summary.HoldStatus,
            summary.LastActivityAt,
            summary.UpdatedAt,
            hierarchyPath,
            summary.BillingAddress,
            summary.ShippingAddress,
            customer.Identifiers.FirstOrDefault()?.IdentifierValue ?? "pending",
            summary.PaymentTerms,
            summary.RiskRating,
            SplitNotes(customer.Notes),
            customer.Contacts.OrderByDescending(contact => contact.IsPrimary).ThenBy(contact => contact.DisplayName).Select(ProjectContact).ToArray(),
            customer.Addresses.OrderBy(address => address.AddressTypeKey).ThenBy(address => address.LocationName).Select(ProjectAddress).ToArray(),
            customer.Addresses.OrderBy(address => address.AddressTypeKey).ThenBy(address => address.LocationName).Select(ProjectAddress).ToArray(),
            customer.Identifiers.Select(ProjectIdentifier).ToArray(),
            customer.BillingProfiles.Select(ProjectBillingProfile).ToArray(),
            new CustomArrPortalSettingsResponse(
                customer.PortalEnabled,
                customer.PortalDisplayName,
                customer.AllowPortalOrderCreate,
                customer.AllowPortalDocumentUpload,
                customer.AllowPortalStatusView,
                customer.DefaultPortalContactId,
                customer.PortalInviteStatusKey,
                customer.PortalTermsAcceptedAt,
                customer.PortalTermsAcceptedByPersonId,
                customer.PortalNotes),
            new CustomArrOperationalPreferencesResponse(
                customer.DefaultOrderTypeKey,
                customer.DefaultServiceLevelKey,
                customer.DefaultPickupAddressId,
                customer.DefaultDeliveryAddressId,
                customer.DefaultContactId,
                customer.RequiresAppointment,
                customer.RequiresProofOfDelivery,
                customer.RequiresCustomerReference,
                customer.CustomerReferenceLabel,
                customer.DefaultInstructions,
                customer.RestrictedServiceNotes,
                customer.NotificationPreferenceKey,
                customer.OrderConfirmationRequired),
            customer.Requirements.Select(ProjectRequirement).ToArray(),
            customer.ExternalRefs.Select(ProjectExternalRef).ToArray(),
            customer.Relationships.Select(relationship => ProjectRelationship(relationship, customerNames)).ToArray(),
            customer.CustomFieldValues.Select(ProjectCustomField).ToArray(),
            customer.Activity.OrderByDescending(activity => activity.OccurredAt).Select(ProjectActivity).ToArray(),
            customer.PrimaryContactId,
            customer.PrimaryBillingAddressId,
            customer.PrimaryShippingAddressId,
            customer.PrimaryServiceAddressId,
            customer.AccountOwnerPersonId,
            customer.AssignedTeamId,
            customer.CustomerSinceDate,
            customer.SourceKey,
            customer.Tags,
            customer.CreatedAt,
            customer.CreatedByPersonId,
            customer.UpdatedByPersonId,
            customer.ArchivedAt,
            customer.ArchivedByPersonId,
            customer.RowVersion);
    }

    private static CustomArrCustomerContactResponse ProjectContact(CustomArrCustomerContact contact) =>
        new(
            contact.ContactId,
            contact.DisplayName,
            contact.Title ?? string.Empty,
            contact.Email,
            contact.Phone,
            contact.IsPrimary,
            contact.PersonId,
            contact.FirstName,
            contact.LastName,
            contact.DisplayName,
            contact.Title,
            contact.Department,
            contact.Email,
            contact.Phone,
            contact.MobilePhone,
            contact.PhoneExtension,
            contact.PreferredContactMethodKey,
            contact.PreferredLanguageKey,
            contact.Timezone,
            contact.IsPrimary,
            contact.IsBillingContact,
            contact.IsOrderingContact,
            contact.IsShippingContact,
            contact.IsEmergencyContact,
            contact.PortalAccessEnabled,
            contact.PortalRoleKey,
            contact.StatusKey,
            contact.LastVerifiedAt);

    private static CustomArrCustomerAddressResponse ProjectAddress(CustomArrCustomerAddress address) =>
        new(
            address.AddressId,
            address.LocationName,
            address.AddressTypeKey,
            address.City,
            address.StateProvince,
            address.AddressId,
            address.AddressTypeKey,
            address.LocationName,
            address.AttentionTo,
            address.Line1,
            address.Line2,
            address.City,
            address.StateProvince,
            address.PostalCode,
            address.CountryCode,
            address.Latitude,
            address.Longitude,
            address.Timezone,
            address.DeliveryInstructions,
            address.AppointmentRequired,
            address.ReceivingHours,
            address.DockDoorNotes,
            address.AccessRestrictions,
            address.IsDefaultBilling,
            address.IsDefaultShipping,
            address.IsDefaultService,
            address.StatusKey);

    private static CustomArrCustomerIdentifierResponse ProjectIdentifier(CustomArrCustomerIdentifier identifier) =>
        new(
            identifier.IdentifierId,
            identifier.IdentifierTypeKey,
            identifier.IdentifierValue,
            identifier.JurisdictionKey,
            identifier.IssuingAuthority,
            identifier.EffectiveDate,
            identifier.ExpirationDate,
            identifier.VerificationStatusKey,
            identifier.RecordArrDocumentId);

    private static CustomArrCustomerBillingProfileResponse ProjectBillingProfile(CustomArrCustomerBillingProfile profile) =>
        new(
            profile.BillingProfileId,
            profile.BillingContactId,
            profile.BillingAddressId,
            profile.PaymentTermsKey,
            profile.InvoiceDeliveryMethodKey,
            profile.BillingEmail,
            profile.PurchaseOrderRequired,
            profile.TaxExempt,
            profile.TaxExemptionRecordId,
            profile.CurrencyCode,
            profile.CreditStatusKey,
            profile.CreditLimit,
            profile.ExternalAccountingCustomerRef);

    private static CustomArrRequirementProgressResponse ProjectRequirement(CustomArrCustomerRequirement requirement) =>
        new(
            requirement.RequirementId,
            requirement.RequirementName,
            requirement.OwnerTeam,
            RequirementProgressStatus(requirement.StatusKey),
            requirement.ExpirationDate,
            requirement.RequirementId,
            requirement.RequirementTypeKey,
            requirement.RequirementName,
            requirement.Description,
            requirement.RequiredBeforeKey,
            requirement.RecordArrDocumentId,
            requirement.ComplianceCoreRuleRef,
            requirement.StatusKey,
            requirement.EffectiveDate,
            requirement.ExpirationDate,
            requirement.ReviewedByPersonId,
            requirement.ReviewedAt,
            requirement.WaiverReason,
            requirement.WaivedByPersonId,
            requirement.OwnerTeam);

    private static CustomArrCustomerExternalRefResponse ProjectExternalRef(CustomArrCustomerExternalRef externalRef) =>
        new(
            externalRef.ExternalRefId,
            externalRef.SystemKey,
            externalRef.ExternalId,
            externalRef.ExternalCode,
            externalRef.LastSyncedAt,
            externalRef.SyncStatusKey);

    private static CustomArrCustomerRelationshipResponse ProjectRelationship(
        CustomArrCustomerRelationship relationship,
        IReadOnlyDictionary<string, string> customerNames) =>
        new(
            relationship.RelationshipId,
            relationship.RelatedCustomerId,
            ResolveCustomerName(relationship.RelatedCustomerId, customerNames),
            relationship.RelationshipTypeKey,
            relationship.EffectiveDate,
            relationship.EndDate);

    private static CustomArrCustomerCustomFieldValueResponse ProjectCustomField(CustomArrCustomerCustomFieldValue field) =>
        new(
            field.FieldValueId,
            field.FieldDefinitionId,
            field.ValueText,
            field.ValueNumber,
            field.ValueBoolean,
            field.ValueDate,
            field.ValueOptionKey,
            field.EffectiveDate,
            field.SourceKey,
            field.LastVerifiedAt,
            field.UpdatedByPersonId);

    private static CustomArrActivityResponse ProjectActivity(CustomArrCustomerActivity activity) =>
        new(activity.ActivityId, activity.Kind, activity.Message, activity.OccurredAt, activity.SourceProductKey, activity.ActorPersonId);

    private static CustomArrPortalSubmissionResponse ProjectPortalSubmission(CustomArrPortalSubmission submission) =>
        new(
            submission.SubmissionId,
            submission.TenantId,
            submission.Status,
            submission.CreatedEventType,
            new StlProductObjectReference(StlProductKeys.CustomArr, "customer", submission.CustomerId, submission.CustomerNumberSnapshot),
            submission.CustomerNameSnapshot,
            submission.CustomerAddressId is null
                ? null
                : new StlProductObjectReference(StlProductKeys.CustomArr, "customer_address", submission.CustomerAddressId),
            submission.CustomerAddressSnapshot,
            submission.RequestType,
            submission.OwnerPersonId,
            submission.Summary,
            submission.RequestedWindowStart,
            submission.RequestedWindowEnd,
            submission.PromisedWindowStart,
            submission.PromisedWindowEnd,
            submission.FulfillmentProductKeys,
            submission.SubmittedAt,
            submission.UpdatedAt,
            submission.OrdArrOrderId is null ? null : new StlProductObjectReference(StlProductKeys.OrdArr, "order", submission.OrdArrOrderId, submission.OrdArrOrderNumber),
            submission.OrdArrOrderNumber);

    private static CustomArrCustomerContact? ResolvePrimaryContact(CustomArrCustomer customer) =>
        customer.Contacts.FirstOrDefault(contact => contact.ContactId == customer.PrimaryContactId)
        ?? customer.Contacts.FirstOrDefault(contact => contact.IsPrimary)
        ?? customer.Contacts.FirstOrDefault();

    private static CustomArrCustomerAddress? ResolveSubmissionAddress(CustomArrCustomer customer, string? addressId)
    {
        if (!string.IsNullOrWhiteSpace(addressId))
        {
            return customer.Addresses.FirstOrDefault(address =>
                string.Equals(address.AddressId, addressId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return customer.Addresses.FirstOrDefault(address => address.AddressId == customer.PrimaryShippingAddressId)
            ?? customer.Addresses.FirstOrDefault(address => address.IsDefaultShipping)
            ?? customer.Addresses.FirstOrDefault(address => address.IsDefaultService)
            ?? customer.Addresses.FirstOrDefault();
    }

    private string NextCustomerNumber(Guid tenantId)
    {
        CustomArrTenantSettingsDefaults.EnsureSeeded(db, tenantId);
        var numbering = db.CustomerNumberingSettings.Single(x => x.TenantId == tenantId);
        var next = CustomArrTenantSettingsDefaults.FormatCustomerNumber(numbering);
        numbering.NextNumber += 1;
        return next;
    }

    private void ValidateCustomerCreateAgainstTenantSettings(
        Guid tenantId,
        CustomArrCreateCustomerRequest request,
        string lifecycleStageKey,
        string customerTypeKey,
        string accountOwnerRef)
    {
        var configuredStage = db.CustomerLifecycleStages.Any(stage =>
            stage.TenantId == tenantId &&
            stage.Key == lifecycleStageKey);
        if (!configuredStage)
        {
            throw new StlApiException("customarr.customer.lifecycle_stage_invalid", "Lifecycle stage is not configured for this tenant.", 400);
        }

        var configuredType = db.CustomerClassificationCatalogs.Any(catalog =>
            catalog.TenantId == tenantId &&
            catalog.CatalogType == "customer_type" &&
            catalog.Key == customerTypeKey &&
            catalog.IsActive);
        if (!configuredType)
        {
            throw new StlApiException("customarr.customer.customer_type_invalid", "Customer type is not active for this tenant.", 400);
        }

        var requiredRules = db.CustomerRequiredFieldRules.AsNoTracking()
            .Where(rule =>
                rule.TenantId == tenantId &&
                rule.RequirementLevel == "required" &&
                rule.AppliesToInternalCreate &&
                (rule.LifecycleStageKey == null || rule.LifecycleStageKey == lifecycleStageKey) &&
                (rule.CustomerTypeKey == null || rule.CustomerTypeKey == customerTypeKey))
            .ToArray();

        foreach (var rule in requiredRules)
        {
            var missing = rule.FieldKey switch
            {
                "legalName" => string.IsNullOrWhiteSpace(request.LegalName),
                "customerType" => string.IsNullOrWhiteSpace(customerTypeKey),
                "lifecycleStage" => string.IsNullOrWhiteSpace(lifecycleStageKey),
                "primaryContact" => string.IsNullOrWhiteSpace(request.PrimaryContactName)
                    && string.IsNullOrWhiteSpace(request.PrimaryContactEmail)
                    && string.IsNullOrWhiteSpace(request.PrimaryContactPhone),
                "billingAddress" => string.IsNullOrWhiteSpace(request.BillingCity)
                    && string.IsNullOrWhiteSpace(request.BillingState),
                "accountOwner" => string.IsNullOrWhiteSpace(accountOwnerRef),
                "validAddress" => string.IsNullOrWhiteSpace(request.BillingCity)
                    && string.IsNullOrWhiteSpace(request.BillingState)
                    && string.IsNullOrWhiteSpace(request.ShippingCity)
                    && string.IsNullOrWhiteSpace(request.ShippingState),
                "primaryEmail" => string.IsNullOrWhiteSpace(request.PrimaryContactEmail),
                "primaryPhone" => string.IsNullOrWhiteSpace(request.PrimaryContactPhone),
                "paymentTerms" => string.IsNullOrWhiteSpace(request.PaymentTermsKey),
                _ => false
            };

            if (missing)
            {
                throw new StlApiException("customarr.customer.required_field_missing", rule.ValidationMessage, 400);
            }
        }
    }

    private static DateTimeOffset LastActivityAt(CustomArrCustomer customer) =>
        customer.Activity.Count == 0 ? customer.UpdatedAt : customer.Activity.Max(activity => activity.OccurredAt);

    private IReadOnlyDictionary<string, string> BuildCustomerNameMap(Guid tenantId) =>
        db.Customers.AsNoTracking()
            .Where(customer => customer.TenantId == tenantId)
            .ToDictionary(customer => customer.CustomerId, customer => customer.DisplayName, StringComparer.OrdinalIgnoreCase);

    private static string? ResolveCustomerName(string? customerId, IReadOnlyDictionary<string, string> customerNames) =>
        customerId is not null && customerNames.TryGetValue(customerId, out var name) ? name : null;

    private static IReadOnlyList<string> BuildHierarchyPath(CustomArrCustomer customer, IReadOnlyDictionary<string, string> customerNames)
    {
        var levels = new List<string>();
        if (ResolveCustomerName(customer.ParentCustomerId, customerNames) is { } parentName)
        {
            levels.Add(parentName);
        }

        levels.Add(customer.DisplayName);
        return levels;
    }

    private static string FormatAddress(CustomArrCustomerAddress? address)
    {
        if (address is null)
        {
            return string.Empty;
        }

        return new[] { address.Line1, address.Line2, address.City, address.StateProvince, address.PostalCode, address.CountryCode }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!)
            .JoinAddress();
    }

    private static string SegmentFromTags(CustomArrCustomer customer)
    {
        var segmentTags = customer.Tags
            .Where(tag => tag is not "strategic" and not "core" and not "standard")
            .Select(HumanizeKey)
            .ToArray();
        return segmentTags.Length == 0 ? HumanizeKey(customer.CustomerTypeKey) : string.Join(", ", segmentTags);
    }

    private static IReadOnlyList<string> SplitNotes(string notes) =>
        string.IsNullOrWhiteSpace(notes)
            ? []
            : notes.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string RequirementProgressStatus(string statusKey) =>
        statusKey is "accepted" or "verified" ? "complete" :
        statusKey is "missing" or "rejected" ? "pending" :
        "watch";

    private static string NormalizeRequestType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "customer_order";
        }

        var normalized = NormalizeKey(value, "customer_order");
        return normalized is "customer_order" or "service_request" or "internal_request"
            ? normalized
            : "customer_order";
    }

    private static string NormalizeStatusKey(string value)
    {
        var normalized = NormalizeKey(value, "prospect");
        return normalized switch
        {
            "watch" => "suspended",
            "inactive" => "inactive",
            "active" => "active",
            "archived" => "inactive",
            "blocked" => "suspended",
            "on_hold" => "suspended",
            "suspended" => "suspended",
            "onboarding" => "onboarding",
            "lead" => "lead",
            "qualified" => "qualified",
            "lost" => "lost",
            _ => normalized
        };
    }

    private static IReadOnlyList<string> NormalizeFulfillmentProductKeys(IReadOnlyList<string>? productKeys)
    {
        var keys = productKeys is { Count: > 0 }
            ? productKeys
            : [StlProductKeys.RoutArr, StlProductKeys.LoadArr];

        return keys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key.Trim().ToLowerInvariant())
            .Where(StlProductKeys.IsCanonical)
            .Where(key => key is not StlProductKeys.CustomArr and not StlProductKeys.OrdArr)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] NormalizeTags(IReadOnlyList<string>? tags, string? legacySegment)
    {
        var values = (tags ?? [])
            .Concat(string.IsNullOrWhiteSpace(legacySegment) ? [] : [legacySegment])
            .Select(value => NormalizeKey(value, string.Empty))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return values.Length == 0 ? ["standard"] : values;
    }

    private static string NormalizeKey(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = value.Trim()
            .Replace("-", "_", StringComparison.Ordinal)
            .Replace(" ", "_", StringComparison.Ordinal)
            .ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? fallback : normalized;
    }

    private static string HumanizeKey(string value) =>
        string.Join(' ', value.Split(['_', '-'], StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part[1..]));

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static (string FirstName, string LastName) SplitName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return (string.Empty, string.Empty);
        }

        var parts = displayName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 1 ? (parts[0], string.Empty) : (parts[0], parts[1]);
    }

    private static void EnsureEntitled(ClaimsPrincipal principal)
    {
        _ = principal.GetTenantId();
    }

    private static readonly IReadOnlyList<RequirementCatalogSeed> RequirementCatalog =
    [
        new("cert-insurance", "Certificate of insurance", "Current COI on file with liability and cargo coverage.", "complete", "Risk", ["strategic", "core"]),
        new("tax-w9", "Tax registration / W-9", "Tax identity and remittance details verified before activation.", "complete", "Finance", ["strategic", "core", "standard"]),
        new("billing-terms", "Billing terms acknowledgement", "Confirmed billing schedule, payment terms, and invoicing contact.", "complete", "Accounts receivable", ["strategic", "core", "standard"]),
        new("portal-agreement", "Portal agreement", "Portal terms and allowed customer actions are configured before customer portal use.", "watch", "Operations", ["strategic", "core"]),
        new("service-rules", "Customer service rules", "Customer-specific appointment, POD, receiving, and reference requirements are documented.", "pending", "Operations", ["strategic", "core", "standard"])
    ];

    private sealed record RequirementCatalogSeed(
        string RequirementKey,
        string Title,
        string Description,
        string Status,
        string OwnerTeam,
        IReadOnlyList<string> AppliesTo);
}

public sealed record CustomArrSessionBootstrapResponse(
    string UserId,
    string PersonId,
    string TenantId,
    string SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    IReadOnlyList<string> LaunchableProductKeys);

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
    IReadOnlyList<string> LaunchableProductKeys,
    string ThemePreference,
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
    Guid TenantId,
    string CustomerNumber,
    string CustomerCode,
    string LegalName,
    string DisplayName,
    string? DbaName,
    string CustomerTypeKey,
    string StatusKey,
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
    string? PrimaryContactId,
    string? PrimaryBillingAddressId,
    string? PrimaryShippingAddressId,
    string? PrimaryServiceAddressId,
    string? AssignedTeamId,
    DateTimeOffset? CustomerSinceDate,
    string SourceKey,
    IReadOnlyList<string> Tags,
    string BillingAddress,
    string ShippingAddress,
    string PaymentTerms,
    string RiskRating);

public sealed record CustomArrCustomerContactResponse(
    string ContactId,
    string Name,
    string Role,
    string Email,
    string Phone,
    bool IsPrimary,
    string? PersonId,
    string FirstName,
    string LastName,
    string DisplayName,
    string? Title,
    string? Department,
    string BusinessEmail,
    string PrimaryPhone,
    string? MobilePhone,
    string? PhoneExtension,
    string PreferredContactMethodKey,
    string? PreferredLanguageKey,
    string? Timezone,
    bool Primary,
    bool IsBillingContact,
    bool IsOrderingContact,
    bool IsShippingContact,
    bool IsEmergencyContact,
    bool PortalAccessEnabled,
    string? PortalRoleKey,
    string StatusKey,
    DateTimeOffset? LastVerifiedAt);

public sealed record CustomArrCustomerAddressResponse(
    string LocationId,
    string Label,
    string Type,
    string City,
    string State,
    string AddressId,
    string AddressTypeKey,
    string LocationName,
    string? AttentionTo,
    string Line1,
    string? Line2,
    string AddressCity,
    string StateProvince,
    string PostalCode,
    string CountryCode,
    decimal? Latitude,
    decimal? Longitude,
    string? Timezone,
    string? DeliveryInstructions,
    bool AppointmentRequired,
    string? ReceivingHours,
    string? DockDoorNotes,
    string? AccessRestrictions,
    bool IsDefaultBilling,
    bool IsDefaultShipping,
    bool IsDefaultService,
    string StatusKey);

public sealed record CustomArrCustomerIdentifierResponse(
    string IdentifierId,
    string IdentifierTypeKey,
    string IdentifierValue,
    string? JurisdictionKey,
    string? IssuingAuthority,
    DateTimeOffset? EffectiveDate,
    DateTimeOffset? ExpirationDate,
    string VerificationStatusKey,
    string? RecordArrDocumentId);

public sealed record CustomArrCustomerBillingProfileResponse(
    string BillingProfileId,
    string? BillingContactId,
    string? BillingAddressId,
    string PaymentTermsKey,
    string InvoiceDeliveryMethodKey,
    string? BillingEmail,
    bool PurchaseOrderRequired,
    bool TaxExempt,
    string? TaxExemptionRecordId,
    string CurrencyCode,
    string CreditStatusKey,
    decimal? CreditLimit,
    string? ExternalAccountingCustomerRef);

public sealed record CustomArrPortalSettingsResponse(
    bool PortalEnabled,
    string? PortalDisplayName,
    bool AllowPortalOrderCreate,
    bool AllowPortalDocumentUpload,
    bool AllowPortalStatusView,
    string? DefaultPortalContactId,
    string PortalInviteStatusKey,
    DateTimeOffset? PortalTermsAcceptedAt,
    string? PortalTermsAcceptedByPersonId,
    string? PortalNotes);

public sealed record CustomArrOperationalPreferencesResponse(
    string? DefaultOrderTypeKey,
    string? DefaultServiceLevelKey,
    string? DefaultPickupAddressId,
    string? DefaultDeliveryAddressId,
    string? DefaultContactId,
    bool RequiresAppointment,
    bool RequiresProofOfDelivery,
    bool RequiresCustomerReference,
    string? CustomerReferenceLabel,
    string? DefaultInstructions,
    string? RestrictedServiceNotes,
    string? NotificationPreferenceKey,
    bool OrderConfirmationRequired);

public sealed record CustomArrRequirementProgressResponse(
    string RequirementKey,
    string Title,
    string Owner,
    string Status,
    DateTimeOffset? DueAt,
    string RequirementId,
    string RequirementTypeKey,
    string RequirementName,
    string Description,
    string RequiredBeforeKey,
    string? RecordArrDocumentId,
    string? ComplianceCoreRuleRef,
    string StatusKey,
    DateTimeOffset? EffectiveDate,
    DateTimeOffset? ExpirationDate,
    string? ReviewedByPersonId,
    DateTimeOffset? ReviewedAt,
    string? WaiverReason,
    string? WaivedByPersonId,
    string OwnerTeam);

public sealed record CustomArrCustomerExternalRefResponse(
    string ExternalRefId,
    string SystemKey,
    string ExternalId,
    string? ExternalCode,
    DateTimeOffset? LastSyncedAt,
    string SyncStatusKey);

public sealed record CustomArrCustomerRelationshipResponse(
    string RelationshipId,
    string RelatedCustomerId,
    string? RelatedCustomerName,
    string RelationshipTypeKey,
    DateTimeOffset? EffectiveDate,
    DateTimeOffset? EndDate);

public sealed record CustomArrCustomerCustomFieldValueResponse(
    string FieldValueId,
    string FieldDefinitionId,
    string? ValueText,
    decimal? ValueNumber,
    bool? ValueBoolean,
    DateTimeOffset? ValueDate,
    string? ValueOptionKey,
    DateTimeOffset? EffectiveDate,
    string SourceKey,
    DateTimeOffset? LastVerifiedAt,
    string? UpdatedByPersonId);

public sealed record CustomArrActivityResponse(
    string ActivityId,
    string Kind,
    string Message,
    DateTimeOffset OccurredAt,
    string SourceProductKey,
    string? ActorPersonId);

public sealed record CustomArrRequirementCatalogItemResponse(
    string RequirementKey,
    string Title,
    string Description,
    string Status,
    string OwnerTeam,
    IReadOnlyList<string> AppliesTo);

public sealed record CustomArrCustomerDetailResponse(
    string CustomerId,
    Guid TenantId,
    string CustomerNumber,
    string CustomerCode,
    string LegalName,
    string DisplayName,
    string? DbaName,
    string CustomerTypeKey,
    string StatusKey,
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
    IReadOnlyList<CustomArrCustomerAddressResponse> Locations,
    IReadOnlyList<CustomArrCustomerAddressResponse> Addresses,
    IReadOnlyList<CustomArrCustomerIdentifierResponse> Identifiers,
    IReadOnlyList<CustomArrCustomerBillingProfileResponse> BillingProfiles,
    CustomArrPortalSettingsResponse PortalSettings,
    CustomArrOperationalPreferencesResponse OperationalPreferences,
    IReadOnlyList<CustomArrRequirementProgressResponse> Requirements,
    IReadOnlyList<CustomArrCustomerExternalRefResponse> ExternalRefs,
    IReadOnlyList<CustomArrCustomerRelationshipResponse> Relationships,
    IReadOnlyList<CustomArrCustomerCustomFieldValueResponse> CustomFieldValues,
    IReadOnlyList<CustomArrActivityResponse> Activity,
    string? PrimaryContactId,
    string? PrimaryBillingAddressId,
    string? PrimaryShippingAddressId,
    string? PrimaryServiceAddressId,
    string? AccountOwnerPersonId,
    string? AssignedTeamId,
    DateTimeOffset? CustomerSinceDate,
    string SourceKey,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt,
    string? CreatedByPersonId,
    string? UpdatedByPersonId,
    DateTimeOffset? ArchivedAt,
    string? ArchivedByPersonId,
    long RowVersion);

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
    string Notes,
    string? DisplayName = null,
    string? DbaName = null,
    string? CustomerTypeKey = null,
    string? StatusKey = null,
    string? AccountOwnerPersonId = null,
    string? AssignedTeamId = null,
    DateTimeOffset? CustomerSinceDate = null,
    string? SourceKey = null,
    IReadOnlyList<string>? Tags = null,
    bool PortalEnabled = false,
    string? PortalDisplayName = null,
    string? PaymentTermsKey = null,
    string? DefaultOrderTypeKey = null,
    string? DefaultServiceLevelKey = null,
    bool RequiresAppointment = false,
    bool RequiresProofOfDelivery = false,
    bool RequiresCustomerReference = false,
    string? CustomerReferenceLabel = null,
    string? DefaultInstructions = null,
    string? NotificationPreferenceKey = null);

public sealed record CustomArrPortalOrderSubmissionRequest(
    string CustomerId,
    string CustomerName,
    string RequestType,
    string OwnerPersonId,
    string Summary,
    DateTimeOffset? RequestedWindowStart = null,
    DateTimeOffset? RequestedWindowEnd = null,
    DateTimeOffset? PromisedWindowStart = null,
    DateTimeOffset? PromisedWindowEnd = null,
    IReadOnlyList<string>? FulfillmentProductKeys = null,
    string? CustomerAddressId = null);

public sealed record CustomArrPortalSubmissionResponse(
    string SubmissionId,
    Guid TenantId,
    string Status,
    string CreatedEventType,
    StlProductObjectReference CustomerRef,
    string CustomerName,
    StlProductObjectReference? CustomerAddressRef,
    string? CustomerAddressSnapshot,
    string RequestType,
    string OwnerPersonId,
    string Summary,
    DateTimeOffset? RequestedWindowStart,
    DateTimeOffset? RequestedWindowEnd,
    DateTimeOffset? PromisedWindowStart,
    DateTimeOffset? PromisedWindowEnd,
    IReadOnlyList<string> FulfillmentProductKeys,
    DateTimeOffset SubmittedAt,
    DateTimeOffset UpdatedAt,
    StlProductObjectReference? OrdArrOrderRef,
    string? OrdArrOrderNumber);

public sealed record CustomArrPortalOrderHandoffResponse(
    CustomArrPortalSubmissionResponse Submission,
    CustomArrOrdArrOrderResponse Order);

internal static class CustomArrStoreExtensions
{
    public static string JoinAddress(this IEnumerable<string> parts) =>
        string.Join(", ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));

    public static string? NullIfWhiteSpace(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
