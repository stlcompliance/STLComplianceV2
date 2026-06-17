using System.Globalization;
using System.Security.Claims;
using CustomArr.Api.Data;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace CustomArr.Api.Endpoints;

public static class ReferenceIntegrationEndpoints
{
    private const string ProductKey = "customarr";
    private const string CustomerReferenceType = "customer";
    private const string CustomerQuickCreatePermission = "customarr.customers.quick_create";

    public static void MapCustomArrReferenceIntegrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/integrations")
            .WithTags("Integrations")
            .RequireAuthorization();

        group.MapGet("/reference-types", () =>
            Results.Ok(new[]
            {
                new ReferenceTypeDescriptor(
                    ProductKey,
                    CustomerReferenceType,
                    "Customer",
                    CanQuickCreate: true,
                    QuickCreatePermission: CustomerQuickCreatePermission,
                    Description: "CustomArr-owned customer account reference.")
            }))
            .WithName("ListCustomArrReferenceTypes");

        group.MapPost("/references/search", (
            ReferenceSearchRequest request,
            HttpContext context,
            CustomArrStore store) =>
        {
            RequireReferenceType(request.ReferenceType);
            var limit = Math.Clamp(request.Limit <= 0 ? 25 : request.Limit, 1, 50);
            var results = store.GetCustomers(context.User, request.Query)
                .Take(limit)
                .Select(ToSummary)
                .ToArray();

            return Results.Ok(new ReferenceSearchResponse(results));
        })
        .WithName("SearchCustomArrReferences");

        group.MapGet("/references/{referenceType}/{id}/summary", (
            string referenceType,
            string id,
            HttpContext context,
            CustomArrStore store) =>
        {
            RequireReferenceType(referenceType);
            var customer = store.GetCustomer(context.User, id);
            return customer is null ? Results.NotFound() : Results.Ok(ToSummary(customer));
        })
        .WithName("GetCustomArrReferenceSummary");

        group.MapGet("/references/{referenceType}/quick-create-schema", (
            string referenceType,
            HttpContext context,
            CustomArrStore store) =>
        {
            RequireReferenceType(referenceType);
            _ = store.GetCustomers(context.User, null);
            var allowed = CanQuickCreateCustomer(context.User);
            return Results.Ok(new QuickCreateSchemaResponse(
                ProductKey,
                CustomerReferenceType,
                allowed,
                "CustomArr",
                CustomerQuickCreatePermission,
                allowed ? null : "Customer quick create requires CustomArr customer creation access.",
                [
                    new QuickCreateFieldDescriptor("legalName", "Legal name", "text", Required: true, Placeholder: "Acme Manufacturing"),
                    new QuickCreateFieldDescriptor("displayName", "Display name", "text", Placeholder: "Acme"),
                    new QuickCreateFieldDescriptor("primaryContactName", "Primary contact", "text"),
                    new QuickCreateFieldDescriptor("primaryContactEmail", "Primary contact email", "email"),
                    new QuickCreateFieldDescriptor("primaryContactPhone", "Primary contact phone", "tel"),
                    new QuickCreateFieldDescriptor("customerCode", "Customer key", "text"),
                    new QuickCreateFieldDescriptor("notes", "Notes", "textarea")
                ]));
        })
        .WithName("GetCustomArrQuickCreateSchema");

        group.MapPost("/references/{referenceType}/quick-create", (
            string referenceType,
            QuickCreateRequest request,
            HttpContext context,
            CustomArrStore store) =>
        {
            RequireReferenceType(referenceType);
            RequireReferenceType(request.ReferenceType);
            if (!CanQuickCreateCustomer(context.User))
            {
                throw new StlApiException(
                    "customarr.references.quick_create_forbidden",
                    "Customer quick create requires customarr.customers.quick_create permission.",
                    403);
            }

            var duplicates = FindCustomerDuplicates(store, context.User, request).ToArray();
            if (duplicates.Length > 0)
            {
                return Results.Conflict(new QuickCreateResponse(
                    null,
                    duplicates,
                    Created: false,
                    ReviewStatus: "duplicate_candidates",
                    Message: "CustomArr found possible duplicate customers. Select the existing customer or review in CustomArr."));
            }

            var idempotencyKey = context.Request.Headers["Idempotency-Key"].ToString();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new StlApiException(
                    "customarr.references.idempotency_required",
                    "Idempotency-Key header is required for customer quick create.",
                    400);
            }

            var created = store.CreateCustomer(
                context.User,
                BuildCreateRequest(request, context.User),
                idempotencyKey);

            return Results.Created(
                $"/api/v1/integrations/references/{CustomerReferenceType}/{created.CustomerId}/summary",
                new QuickCreateResponse(
                    ToSummary(created).ToCrossProductReference("quick_create"),
                    [],
                    Created: true,
                    ReviewStatus: "provisional",
                    Message: "Customer was created in CustomArr as a provisional customer."));
        })
        .WithName("QuickCreateCustomArrReference");
    }

    private static void RequireReferenceType(string referenceType)
    {
        if (!string.Equals(referenceType, CustomerReferenceType, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "customarr.references.unsupported_type",
                $"CustomArr does not own reference type '{referenceType}'.",
                404);
        }
    }

    private static bool CanQuickCreateCustomer(ClaimsPrincipal principal)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        var role = principal.GetTenantRoleKey();
        return role.Equals("tenant_admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("customarr_admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("customarr_manager", StringComparison.OrdinalIgnoreCase);
    }

    private static ReferenceSummaryResponse ToSummary(CustomArrCustomerDetailResponse customer) =>
        new(
            ProductKey,
            CustomerReferenceType,
            customer.CustomerId,
            customer.DisplayName,
            BuildSecondaryLabel(customer),
            customer.StatusKey,
            customer.RowVersion.ToString(CultureInfo.InvariantCulture),
            $"/customers/{customer.CustomerId}",
            new Dictionary<string, string>
            {
                ["customerNumber"] = customer.CustomerNumber,
                ["customerCode"] = customer.CustomerCode,
                ["legalName"] = customer.LegalName,
                ["primaryContactEmail"] = customer.PrimaryContactEmail
            });

    private static string BuildSecondaryLabel(CustomArrCustomerDetailResponse customer)
    {
        var parts = new[] { customer.CustomerNumber, customer.PrimaryContactEmail }
            .Where(value => !string.IsNullOrWhiteSpace(value));
        return string.Join(" / ", parts);
    }

    private static CustomArrCreateCustomerRequest BuildCreateRequest(
        QuickCreateRequest request,
        ClaimsPrincipal principal)
    {
        var legalName = FirstValue(request.Values, "legalName", "name", "displayName");
        if (string.IsNullOrWhiteSpace(legalName))
        {
            throw new StlApiException(
                "customarr.references.legal_name_required",
                "Customer legal name is required.",
                400);
        }

        var displayName = FirstValue(request.Values, "displayName", "name", "legalName");
        return new CustomArrCreateCustomerRequest(
            LegalName: legalName,
            TradeName: string.Empty,
            Status: "prospect",
            Tier: "standard",
            Segment: "quick_create",
            OwnerPersonId: principal.GetPersonId().ToString("D"),
            ParentCustomerId: string.Empty,
            PrimaryContactName: GetValue(request.Values, "primaryContactName"),
            PrimaryContactEmail: GetValue(request.Values, "primaryContactEmail", "email"),
            PrimaryContactPhone: GetValue(request.Values, "primaryContactPhone", "phone"),
            BillingCity: string.Empty,
            BillingState: string.Empty,
            ShippingCity: string.Empty,
            ShippingState: string.Empty,
            Notes: GetValue(request.Values, "notes"),
            DisplayName: string.IsNullOrWhiteSpace(displayName) ? legalName : displayName,
            CustomerTypeKey: "standard",
            StatusKey: "prospect",
            SourceKey: "cross_product_quick_create",
            Tags: ["quick-create", "needs-review"]);
    }

    private static IEnumerable<DuplicateCandidateResponse> FindCustomerDuplicates(
        CustomArrStore store,
        ClaimsPrincipal principal,
        QuickCreateRequest request)
    {
        var name = NormalizeText(FirstValue(request.Values, "legalName", "displayName", "name"));
        var email = NormalizeText(GetValue(request.Values, "primaryContactEmail", "email"));
        var phone = DigitsOnly(GetValue(request.Values, "primaryContactPhone", "phone"));
        var customerKey = NormalizeText(GetValue(request.Values, "customerCode", "customerKey"));

        if (string.IsNullOrWhiteSpace(name)
            && string.IsNullOrWhiteSpace(email)
            && string.IsNullOrWhiteSpace(phone)
            && string.IsNullOrWhiteSpace(customerKey))
        {
            return [];
        }

        return store.GetCustomers(principal, null)
            .Select(customer =>
            {
                var reasons = new List<string>();
                if (!string.IsNullOrWhiteSpace(name)
                    && (NormalizeText(customer.DisplayName) == name
                        || NormalizeText(customer.LegalName) == name
                        || NormalizeText(customer.DbaName ?? string.Empty) == name))
                {
                    reasons.Add("matching name");
                }

                if (!string.IsNullOrWhiteSpace(email)
                    && NormalizeText(customer.PrimaryContactEmail) == email)
                {
                    reasons.Add("matching email");
                }

                if (!string.IsNullOrWhiteSpace(phone)
                    && customer.Contacts.Any(contact => DigitsOnly(contact.PrimaryPhone) == phone || DigitsOnly(contact.Phone) == phone))
                {
                    reasons.Add("matching phone");
                }

                if (!string.IsNullOrWhiteSpace(customerKey)
                    && NormalizeText(customer.CustomerCode) == customerKey)
                {
                    reasons.Add("matching customer key");
                }

                return (customer, reasons);
            })
            .Where(match => match.reasons.Count > 0)
            .Take(10)
            .Select(match => new DuplicateCandidateResponse(
                match.customer.CustomerId,
                match.customer.DisplayName,
                BuildSecondaryLabel(match.customer),
                match.customer.StatusKey,
                string.Join(", ", match.reasons),
                match.reasons.Count > 1 ? 0.95m : 0.8m));
    }

    private static string FirstValue(IReadOnlyDictionary<string, string> values, params string[] keys) =>
        keys.Select(key => GetValue(values, key)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static string GetValue(IReadOnlyDictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            var match = values.FirstOrDefault(entry => string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Value))
            {
                return match.Value.Trim();
            }
        }

        return string.Empty;
    }

    private static string NormalizeText(string value) => value.Trim().ToLowerInvariant();

    private static string DigitsOnly(string value) => new(value.Where(char.IsDigit).ToArray());
}
