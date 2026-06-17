using NexArr.Api.Contracts;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public sealed record TenantIntegrationProviderDefinition(
    string ProviderKey,
    string DisplayName,
    string Category,
    string ConnectorFamily,
    string AuthType,
    string DefaultDirection,
    bool SupportsWriteback,
    bool RequiresManualMapping,
    IReadOnlyList<string> OwningProducts,
    IReadOnlyList<string> Capabilities);

public static class TenantIntegrationProviderCatalog
{
    private static readonly IReadOnlyList<TenantIntegrationProviderDefinition> Definitions =
    [
        Provider("microsoft-entra", "Microsoft Entra", "Identity", "saml_oidc_scim", "SAML/OIDC + SCIM", TenantIntegrationDirections.ReadOnly, true, false, ["nexarr", "staffarr"], ["sso", "directory", "provisioning"]),
        Provider("okta", "Okta", "Identity", "saml_oidc_scim", "SAML/OIDC + SCIM", TenantIntegrationDirections.ReadOnly, true, false, ["nexarr", "staffarr"], ["sso", "directory", "provisioning"]),
        Provider("google-workspace", "Google Workspace", "Identity", "oauth2_directory", "OAuth2", TenantIntegrationDirections.ReadOnly, true, false, ["nexarr", "staffarr", "recordarr"], ["directory", "groups", "drive"]),

        Provider("quickbooks", "QuickBooks", "Finance / ERP", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, false, ["supplyarr", "customarr", "ordarr", "reportarr"], ["customers", "vendors", "invoice_status_snapshots", "bill_status_snapshots"]),
        Provider("xero", "Xero", "Finance / ERP", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, false, ["supplyarr", "customarr", "ordarr", "reportarr"], ["contacts", "invoice_status_snapshots", "bill_status_snapshots"]),
        Provider("netsuite", "NetSuite", "Finance / ERP", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, true, ["supplyarr", "customarr", "ordarr", "reportarr"], ["customers", "vendors", "items", "financial_snapshots"]),
        Provider("sap", "SAP", "Finance / ERP", "openapi", "OAuth2/API key", TenantIntegrationDirections.ReadOnly, true, true, ["supplyarr", "customarr", "ordarr", "reportarr"], ["business_partner_snapshots", "erp_status_snapshots"]),
        Provider("oracle", "Oracle", "Finance / ERP", "openapi", "OAuth2/API key", TenantIntegrationDirections.ReadOnly, true, true, ["supplyarr", "customarr", "ordarr", "reportarr"], ["business_partner_snapshots", "erp_status_snapshots"]),

        Provider("samsara", "Samsara", "Fleet / ELD", "api_key", "API key", TenantIntegrationDirections.ReadOnly, false, false, ["routarr", "maintainarr"], ["hos", "gps", "fault_codes", "vehicle_telemetry"]),
        Provider("geotab", "Geotab", "Fleet / ELD", "api_key", "API key", TenantIntegrationDirections.ReadOnly, false, false, ["routarr", "maintainarr"], ["hos", "gps", "fault_codes", "vehicle_telemetry"]),
        Provider("motive", "Motive", "Fleet / ELD", "api_key", "API key", TenantIntegrationDirections.ReadOnly, false, false, ["routarr", "maintainarr"], ["hos", "gps", "fault_codes", "vehicle_telemetry"]),
        Provider("eroad", "EROAD", "Fleet / ELD", "api_key", "API key", TenantIntegrationDirections.ReadOnly, false, false, ["routarr", "maintainarr"], ["hos", "gps", "vehicle_telemetry"]),
        Provider("teletrac-navman", "Teletrac Navman", "Fleet / ELD", "api_key", "API key", TenantIntegrationDirections.ReadOnly, false, false, ["routarr", "maintainarr"], ["hos", "gps", "vehicle_telemetry"]),

        Provider("fleetio-fuel-imports", "Fleetio-style fuel imports", "Fuel Cards / Imports", "file_import", "CSV/XLSX", TenantIntegrationDirections.Inbound, false, true, ["supplyarr", "maintainarr", "reportarr"], ["fuel_transactions", "odometer_snapshots"]),
        Provider("wex", "WEX", "Fuel Cards / Imports", "file_or_api", "API key/SFTP", TenantIntegrationDirections.ReadOnly, false, true, ["supplyarr", "maintainarr", "reportarr"], ["fuel_transactions", "card_snapshots"]),
        Provider("comdata-corpay", "Comdata / Corpay", "Fuel Cards / Imports", "file_or_api", "API key/SFTP", TenantIntegrationDirections.ReadOnly, false, true, ["supplyarr", "maintainarr", "reportarr"], ["fuel_transactions", "card_snapshots"]),
        Provider("us-bank-voyager", "U.S. Bank Voyager", "Fuel Cards / Imports", "file_or_api", "API key/SFTP", TenantIntegrationDirections.ReadOnly, false, true, ["supplyarr", "maintainarr", "reportarr"], ["fuel_transactions", "card_snapshots"]),

        Provider("shipstation", "ShipStation", "Shipping / Commerce", "api_key", "API key", TenantIntegrationDirections.ReadOnly, true, false, ["ordarr", "loadarr", "routarr", "customarr"], ["orders", "shipments", "tracking"]),
        Provider("easypost", "EasyPost", "Shipping / Commerce", "api_key", "API key", TenantIntegrationDirections.ReadOnly, true, false, ["ordarr", "loadarr", "routarr"], ["rates", "labels", "tracking"]),
        Provider("shopify", "Shopify", "Shipping / Commerce", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, false, ["customarr", "ordarr", "loadarr"], ["customers", "orders", "fulfillment_snapshots"]),
        Provider("fedex", "FedEx", "Shipping / Commerce", "oauth2_api", "OAuth2/API key", TenantIntegrationDirections.ReadOnly, true, false, ["ordarr", "loadarr", "routarr"], ["rates", "labels", "tracking"]),
        Provider("ups", "UPS", "Shipping / Commerce", "oauth2_api", "OAuth2/API key", TenantIntegrationDirections.ReadOnly, true, false, ["ordarr", "loadarr", "routarr"], ["rates", "labels", "tracking"]),
        Provider("usps", "USPS", "Shipping / Commerce", "api_key", "API key", TenantIntegrationDirections.ReadOnly, true, false, ["ordarr", "loadarr", "routarr"], ["rates", "labels", "tracking"]),

        Provider("google-drive", "Google Drive", "Documents / E-sign", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, true, ["recordarr"], ["documents", "folders", "evidence_import"]),
        Provider("sharepoint", "SharePoint", "Documents / E-sign", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, true, ["recordarr"], ["documents", "sites", "evidence_import"]),
        Provider("docusign", "DocuSign", "Documents / E-sign", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, false, ["recordarr"], ["envelopes", "signatures", "completed_documents"]),

        Provider("bamboohr", "BambooHR", "HR / Payroll", "api_key", "API key", TenantIntegrationDirections.ReadOnly, false, false, ["staffarr"], ["people", "employment_status", "org_snapshots"]),
        Provider("gusto", "Gusto", "HR / Payroll", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, false, ["staffarr"], ["people", "payroll_status_snapshots"]),
        Provider("adp", "ADP", "HR / Payroll", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, true, ["staffarr"], ["people", "payroll_status_snapshots"]),
        Provider("workday", "Workday", "HR / Payroll", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, true, ["staffarr"], ["workers", "orgs", "cost_centers"]),
        Provider("ukg", "UKG", "HR / Payroll", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, true, ["staffarr"], ["people", "orgs", "payroll_status_snapshots"]),
        Provider("paychex", "Paychex", "HR / Payroll", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, true, ["staffarr"], ["people", "payroll_status_snapshots"]),
        Provider("paylocity", "Paylocity", "HR / Payroll", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, true, ["staffarr"], ["people", "payroll_status_snapshots"]),

        Provider("ecfr", "eCFR", "Government / Reference", "public_api", "Public API", TenantIntegrationDirections.ReadOnly, false, false, ["compliancecore"], ["regulatory_reference", "citation_snapshots"]),
        Provider("fmcsa", "FMCSA", "Government / Reference", "public_or_api", "Public/API key", TenantIntegrationDirections.ReadOnly, false, false, ["compliancecore", "routarr", "maintainarr"], ["regulatory_reference", "safety_snapshots"]),
        Provider("nhtsa", "NHTSA", "Government / Reference", "public_api", "Public API", TenantIntegrationDirections.ReadOnly, false, false, ["compliancecore", "maintainarr"], ["vin_decode", "recalls", "complaints"]),
        Provider("power-bi", "Power BI", "BI / Reporting", "oauth2_api", "OAuth2", TenantIntegrationDirections.ReadOnly, true, true, ["reportarr"], ["dataset_refresh", "report_exports"]),

        Provider("dat", "DAT", "Freight Visibility / TMS", "api_key", "API key", TenantIntegrationDirections.ReadOnly, true, true, ["routarr", "supplyarr", "reportarr"], ["rates", "load_board_snapshots"]),
        Provider("truckstop", "Truckstop", "Freight Visibility / TMS", "api_key", "API key", TenantIntegrationDirections.ReadOnly, true, true, ["routarr", "supplyarr", "reportarr"], ["rates", "load_board_snapshots"]),
        Provider("project44", "project44", "Freight Visibility / TMS", "api_key", "API key", TenantIntegrationDirections.ReadOnly, true, false, ["routarr", "reportarr"], ["visibility_events", "tracking"]),
        Provider("fourkites", "FourKites", "Freight Visibility / TMS", "api_key", "API key", TenantIntegrationDirections.ReadOnly, true, false, ["routarr", "reportarr"], ["visibility_events", "tracking"]),
        Provider("macropoint", "MacroPoint", "Freight Visibility / TMS", "api_key", "API key", TenantIntegrationDirections.ReadOnly, true, false, ["routarr", "reportarr"], ["visibility_events", "tracking"]),
        Provider("manhattan", "Manhattan", "Freight Visibility / TMS", "openapi", "OAuth2/API key", TenantIntegrationDirections.ReadOnly, true, true, ["loadarr", "ordarr", "routarr", "reportarr"], ["wms_snapshots", "orders", "shipments"]),
        Provider("extensiv", "Extensiv", "Freight Visibility / TMS", "openapi", "OAuth2/API key", TenantIntegrationDirections.ReadOnly, true, true, ["loadarr", "ordarr", "routarr", "reportarr"], ["wms_snapshots", "orders", "shipments"]),

        Provider("edi-x12", "EDI X12", "Generic Protocols", "edi_x12", "AS2/SFTP", TenantIntegrationDirections.Inbound, true, true, ["ordarr", "loadarr", "routarr", "supplyarr"], ["x12_204", "x12_210", "x12_214", "x12_850", "x12_855", "x12_856", "x12_997"]),
        Provider("as2", "AS2", "Generic Protocols", "as2", "Certificate", TenantIntegrationDirections.Inbound, true, true, ["ordarr", "loadarr", "routarr", "supplyarr", "recordarr"], ["secure_message_intake", "mdn_tracking"]),
        Provider("sftp", "SFTP", "Generic Protocols", "sftp", "SSH key/password", TenantIntegrationDirections.Inbound, true, true, ["ordarr", "loadarr", "routarr", "supplyarr", "recordarr"], ["file_drop", "scheduled_pickup"]),
        Provider("csv-xlsx", "CSV/XLSX import/export", "Generic Protocols", "file_import", "File upload", TenantIntegrationDirections.Inbound, true, true, ["nexarr", "staffarr", "supplyarr", "loadarr", "maintainarr", "routarr", "recordarr", "reportarr"], ["imports", "exports", "manual_mapping"]),
        Provider("webhooks", "Webhooks", "Generic Protocols", "webhook", "Shared secret", TenantIntegrationDirections.Inbound, true, true, ["nexarr", "ordarr", "loadarr", "routarr", "supplyarr", "recordarr"], ["event_intake", "signature_validation"]),
        Provider("openapi", "OpenAPI", "Generic Protocols", "openapi", "OAuth2/API key", TenantIntegrationDirections.ReadOnly, true, true, ["nexarr", "staffarr", "supplyarr", "loadarr", "maintainarr", "routarr", "recordarr", "reportarr"], ["schema_driven_sync", "manual_mapping"]),
        Provider("oauth2", "OAuth2", "Generic Protocols", "oauth2", "OAuth2", TenantIntegrationDirections.ReadOnly, true, true, ["nexarr"], ["authorization_handoff", "token_refresh"]),
        Provider("scim", "SCIM", "Generic Protocols", "scim", "Bearer token", TenantIntegrationDirections.Inbound, true, true, ["nexarr", "staffarr"], ["users", "groups", "provisioning"]),
        Provider("saml-oidc", "SAML/OIDC", "Generic Protocols", "saml_oidc", "SAML/OIDC", TenantIntegrationDirections.ReadOnly, false, true, ["nexarr"], ["sso", "metadata", "assertion_consumer"]),
    ];

    private static readonly IReadOnlyDictionary<string, TenantIntegrationProviderDefinition> ByKey =
        Definitions.ToDictionary(x => x.ProviderKey, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, TenantIntegrationBrandResponse> BrandByKey =
        new Dictionary<string, TenantIntegrationBrandResponse>(StringComparer.OrdinalIgnoreCase)
        {
            ["microsoft-entra"] = Brand("MS", "#0078D4", "https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks", "Microsoft trademark and brand guidelines"),
            ["okta"] = Brand("OK", "#007DC1", "https://www.okta.com/press-room/media-assets/", "Okta media assets"),
            ["google-workspace"] = Brand("GWS", "#4285F4", "https://partnermarketinghub.withgoogle.com", "Google partner marketing and brand guidance"),

            ["quickbooks"] = Brand("QB", "#2CA01C", "https://design.intuit.com/quickbooks/brand", "Intuit QuickBooks brand guidance"),
            ["xero"] = Brand("XE", "#13B5EA", "https://www.xero.com/uk/about/media/downloads", "Xero media downloads"),
            ["netsuite"] = Brand("NS", "#125B84", "https://www.netsuite.com/portal/company/newsroom.shtml", "NetSuite newsroom"),
            ["sap"] = Brand("SAP", "#0FAAFF", "https://www.sap.com", "SAP public brand presence"),
            ["oracle"] = Brand("OR", "#C74634", "https://www.oracle.com/corporate/press/", "Oracle press resources"),

            ["samsara"] = Brand("SA", "#FF5A00", "https://www.samsara.com", "Samsara public brand presence"),
            ["geotab"] = Brand("GT", "#005DAA", "https://www.geotab.com", "Geotab public brand presence"),
            ["motive"] = Brand("MO", "#0B57D0", "https://gomotive.com", "Motive public brand presence"),
            ["eroad"] = Brand("ER", "#E52521", "https://www.eroad.com", "EROAD public brand presence"),
            ["teletrac-navman"] = Brand("TN", "#F58220", "https://www.teletracnavman.com", "Teletrac Navman public brand presence"),

            ["fleetio-fuel-imports"] = Brand("FL", "#3C7DFF", "https://www.fleetio.com", "Fleetio public brand presence"),
            ["wex"] = Brand("WEX", "#009FDF", "https://www.wexinc.com", "WEX public brand presence"),
            ["comdata-corpay"] = Brand("CP", "#004B8D", "https://www.corpay.com", "Corpay public brand presence"),
            ["us-bank-voyager"] = Brand("USB", "#D71920", "https://www.usbank.com/corporate-and-commercial-banking/commercial-products/transportation-freight-solutions/fleet-cards.html", "U.S. Bank fleet cards"),

            ["shipstation"] = Brand("SS", "#4D72B8", "https://www.shipstation.com", "ShipStation public brand presence"),
            ["easypost"] = Brand("EP", "#202B85", "https://www.easypost.com", "EasyPost public brand presence"),
            ["shopify"] = Brand("SH", "#7AB55C", "https://www.shopify.com/brand-assets", "Shopify brand assets"),
            ["fedex"] = Brand("FX", "#4D148C", "https://newsroom.fedex.com", "FedEx newsroom"),
            ["ups"] = Brand("UPS", "#150400", "https://www.ups.com", "UPS public brand presence"),
            ["usps"] = Brand("USPS", "#333366", "https://www.usps.com", "USPS public brand presence"),

            ["google-drive"] = Brand("GD", "#4285F4", "https://developers.google.com/drive/web/branding", "Google Drive branding guidance"),
            ["sharepoint"] = Brand("SP", "#03787C", "https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks", "Microsoft trademark and brand guidelines"),
            ["docusign"] = Brand("DS", "#4C00FF", "https://www.docusign.com", "Docusign public brand presence"),

            ["bamboohr"] = Brand("BHR", "#6DB33F", "https://www.bamboohr.com", "BambooHR public brand presence"),
            ["gusto"] = Brand("GU", "#F45D48", "https://gusto.com", "Gusto public brand presence"),
            ["adp"] = Brand("ADP", "#D0271D", "https://www.adp.com", "ADP public brand presence"),
            ["workday"] = Brand("WD", "#0875E1", "https://www.workday.com", "Workday public brand presence"),
            ["ukg"] = Brand("UKG", "#005EB8", "https://www.ukg.com", "UKG public brand presence"),
            ["paychex"] = Brand("PX", "#004B8D", "https://www.paychex.com", "Paychex public brand presence"),
            ["paylocity"] = Brand("PL", "#EF3E33", "https://www.paylocity.com", "Paylocity public brand presence"),

            ["ecfr"] = Brand("CFR", "#1E3A8A", "https://www.ecfr.gov", "eCFR public reference"),
            ["fmcsa"] = Brand("FM", "#1D4ED8", "https://www.fmcsa.dot.gov", "FMCSA public reference"),
            ["nhtsa"] = Brand("NH", "#0A4C8A", "https://www.nhtsa.gov", "NHTSA public reference"),
            ["power-bi"] = Brand("PBI", "#F2C811", "https://powerbi.microsoft.com", "Power BI public brand presence"),

            ["dat"] = Brand("DAT", "#0033A0", "https://www.dat.com", "DAT public brand presence"),
            ["truckstop"] = Brand("TS", "#F7941D", "https://truckstop.com", "Truckstop public brand presence"),
            ["project44"] = Brand("P44", "#FF6B00", "https://www.project44.com", "project44 public brand presence"),
            ["fourkites"] = Brand("FK", "#0B3D91", "https://www.fourkites.com", "FourKites public brand presence"),
            ["macropoint"] = Brand("MP", "#1F6FEB", "https://www.descartes.com/solutions/macropoint", "Descartes MacroPoint public brand presence"),
            ["manhattan"] = Brand("MA", "#005EB8", "https://www.manh.com", "Manhattan public brand presence"),
            ["extensiv"] = Brand("EX", "#4B32C3", "https://www.extensiv.com", "Extensiv public brand presence"),

            ["edi-x12"] = Brand("X12", "#38BDF8", "https://x12.org", "X12 standards organization"),
            ["as2"] = Brand("AS2", "#22C55E", "https://datatracker.ietf.org/doc/html/rfc4130", "IETF AS2 RFC"),
            ["sftp"] = Brand("SFTP", "#14B8A6", "https://datatracker.ietf.org/doc/html/draft-ietf-secsh-filexfer", "IETF SSH file transfer draft"),
            ["csv-xlsx"] = Brand("CSV", "#22C55E", "https://www.rfc-editor.org/rfc/rfc4180", "CSV RFC reference"),
            ["webhooks"] = Brand("WH", "#A855F7", "https://www.standardwebhooks.com", "Standard Webhooks reference"),
            ["openapi"] = Brand("OA", "#6BA539", "https://www.openapis.org", "OpenAPI Initiative"),
            ["oauth2"] = Brand("OA2", "#2F80ED", "https://oauth.net/2/", "OAuth 2.0 reference"),
            ["scim"] = Brand("SCIM", "#06B6D4", "https://www.rfc-editor.org/rfc/rfc7644", "SCIM protocol RFC"),
            ["saml-oidc"] = Brand("SSO", "#F59E0B", "https://openid.net/developers/how-connect-works/", "OpenID Connect reference"),
        };

    public static IReadOnlyList<TenantIntegrationProviderDefinition> All => Definitions;

    public static TenantIntegrationCatalogResponse BuildResponse() =>
        new(Definitions.Select(Map).ToList());

    public static bool TryGet(string providerKey, out TenantIntegrationProviderDefinition definition) =>
        ByKey.TryGetValue(NormalizeProviderKey(providerKey), out definition!);

    public static TenantIntegrationProviderDefinition GetRequired(string providerKey)
    {
        if (TryGet(providerKey, out var definition))
        {
            return definition;
        }

        throw new KeyNotFoundException($"Integration provider '{providerKey}' is not registered.");
    }

    public static string NormalizeProviderKey(string providerKey) =>
        providerKey.Trim().ToLowerInvariant();

    public static IReadOnlyList<TenantIntegrationRouteResponse> BuildRoutes(string providerKey)
    {
        var key = NormalizeProviderKey(providerKey);
        return
        [
            new("suite_detail", "GET", $"/app/nexarr/integrations/{key}", "Tenant integration detail route."),
            new("suite_mappings", "GET", $"/app/nexarr/integrations/{key}/mappings", "Tenant mapping workspace route."),
            new("api_config", "GET/PUT", $"/api/v1/tenants/{{tenantId}}/integrations/{key}", "Tenant-scoped integration configuration."),
            new("api_credentials", "PUT/DELETE", $"/api/v1/tenants/{{tenantId}}/integrations/{key}/credentials", "Tenant-scoped encrypted credential management."),
            new("api_test", "POST", $"/api/v1/tenants/{{tenantId}}/integrations/{key}/test", "Connection health check."),
            new("api_sync", "POST", $"/api/v1/tenants/{{tenantId}}/integrations/{key}/sync-runs", "Manual sync trigger."),
            new("oauth_callback", "GET/POST", $"/api/v1/integrations/{key}/oauth/callback", "OAuth/OIDC callback endpoint."),
            new("saml_metadata", "GET", $"/api/v1/integrations/{key}/saml/metadata", "SAML metadata endpoint."),
            new("saml_acs", "POST", $"/api/v1/integrations/{key}/saml/acs", "SAML assertion consumer endpoint."),
            new("scim", "GET/POST/PUT/PATCH/DELETE", $"/api/v1/integrations/{key}/scim/{{*path}}", "SCIM provisioning endpoint."),
            new("webhook", "POST", $"/api/v1/integrations/{key}/webhooks/{{*path}}", "Webhook intake endpoint."),
            new("as2", "POST", $"/api/v1/integrations/{key}/as2/receive", "AS2 message intake endpoint."),
            new("sftp_intake", "POST", $"/api/v1/integrations/{key}/sftp/intake", "SFTP pickup/intake recording endpoint."),
            new("csv_xlsx", "POST", $"/api/v1/integrations/{key}/csv-xlsx/import", "CSV/XLSX intake endpoint."),
        ];
    }

    public static TenantIntegrationProviderResponse Map(TenantIntegrationProviderDefinition definition) =>
        new(
            definition.ProviderKey,
            definition.DisplayName,
            definition.Category,
            BuildBrand(definition.ProviderKey, definition.DisplayName),
            definition.ConnectorFamily,
            definition.AuthType,
            definition.DefaultDirection,
            definition.SupportsWriteback,
            definition.RequiresManualMapping,
            definition.OwningProducts,
            definition.Capabilities,
            BuildRoutes(definition.ProviderKey));

    public static TenantIntegrationBrandResponse BuildBrand(string providerKey, string displayName) =>
        BrandByKey.TryGetValue(NormalizeProviderKey(providerKey), out var brand)
            ? brand
            : Brand(BuildFallbackMark(displayName), "#38BDF8", "https://www.stlcompliance.com", "STL fallback integration branding");

    private static TenantIntegrationProviderDefinition Provider(
        string providerKey,
        string displayName,
        string category,
        string connectorFamily,
        string authType,
        string defaultDirection,
        bool supportsWriteback,
        bool requiresManualMapping,
        IReadOnlyList<string> owningProducts,
        IReadOnlyList<string> capabilities) =>
        new(
            providerKey,
            displayName,
            category,
            connectorFamily,
            authType,
            defaultDirection,
            supportsWriteback,
            requiresManualMapping,
            owningProducts,
            capabilities);

    private static TenantIntegrationBrandResponse Brand(
        string mark,
        string accentColor,
        string assetSourceUrl,
        string assetSourceLabel) =>
        new(
            mark,
            accentColor,
            "#0F172A",
            "#F8FAFC",
            assetSourceUrl,
            assetSourceUrl,
            assetSourceLabel,
            "Vendor-owned trademark metadata. NexArr renders a neutral text mark unless approved logo artwork is supplied.");

    private static string BuildFallbackMark(string displayName)
    {
        var letters = displayName
            .Split(new[] { ' ', '-', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(word => word[0])
            .Take(4)
            .ToArray();
        return letters.Length == 0
            ? "INT"
            : new string(letters).ToUpperInvariant();
    }
}
