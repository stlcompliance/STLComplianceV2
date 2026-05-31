using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class V1FeatureAliasEndpoints
{
    public static void MapStaffArrV1FeatureAliasEndpoints(this WebApplication app)
    {
        MapOrgUnitAliases(app);
        MapHierarchyAlias(app);
        MapPermissionAliases(app);
        MapOverrideAlias(app);
        MapOnboardingAlias(app);
        MapDocumentAlias(app);
        MapReportsIndexAlias(app);
        MapIntegrationsIndexAlias(app);
    }

    private static void MapOrgUnitAliases(WebApplication app)
    {
        static void MapTypedRoute(WebApplication app, string route, string unitType, string routeName)
        {
            app.MapGet(route, async (
                HttpContext context,
                StaffArrAuthorizationService authorization,
                OrgUnitService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePeopleRead(context.User);
                var tenantId = context.User.GetTenantId();
                var units = await service.ListAsync(tenantId, cancellationToken);
                return Results.Ok(units.Where(x => x.UnitType == unitType).ToList());
            })
            .WithTags("OrgUnits")
            .RequireAuthorization()
            .WithName(routeName);
        }

        MapTypedRoute(app, "/api/v1/sites", "site", "ListSitesV1");
        MapTypedRoute(app, "/api/v1/departments", "department", "ListDepartmentsV1");
        MapTypedRoute(app, "/api/v1/positions", "position", "ListPositionsV1");
        MapTypedRoute(app, "/api/v1/teams", "team", "ListTeamsV1");
    }

    private static void MapHierarchyAlias(WebApplication app)
    {
        app.MapGet("/api/v1/hierarchy", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ManagerHierarchyService service,
            CancellationToken cancellationToken) =>
        {
            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new { code = "hierarchy.validation", message = "personId query parameter is required." });
            }

            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            var managerChain = await service.GetManagerChainAsync(tenantId, personId, cancellationToken);
            var subordinates = await service.GetSubordinatesAsync(tenantId, personId, includeIndirect: true, limit: 200, cancellationToken);
            return Results.Ok(new { personId, managerChain, subordinates });
        })
        .WithTags("ManagerHierarchy")
        .RequireAuthorization()
        .WithName("GetHierarchyV1");
    }

    private static void MapPermissionAliases(WebApplication app)
    {
        app.MapGet("/api/v1/permissions", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPermissionTemplatesAsync(tenantId, cancellationToken));
        })
        .WithTags("PermissionTemplates")
        .RequireAuthorization()
        .WithName("ListPermissionsV1");

        app.MapGet("/api/v1/permission-templates", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            RoleTemplateService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoleTemplateRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPermissionTemplatesAsync(tenantId, cancellationToken));
        })
        .WithTags("PermissionTemplates")
        .RequireAuthorization()
        .WithName("ListPermissionTemplatesV1");

        app.MapGet("/api/v1/permissions/check", async (
            Guid personId,
            string[] permissionKey,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IntegrationPermissionCheckService service,
            CancellationToken cancellationToken) =>
        {
            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new
                {
                    code = "permission_check.validation",
                    message = "personId query parameter is required."
                });
            }

            if (permissionKey.Length == 0 || permissionKey.All(string.IsNullOrWhiteSpace))
            {
                return Results.BadRequest(new
                {
                    code = "permission_check.validation",
                    message = "At least one permissionKey query parameter is required."
                });
            }

            authorization.RequirePermissionProjectionRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            var result = await service.CheckAsync(
                tenantId,
                personId,
                permissionKey,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("PermissionTemplates")
        .RequireAuthorization()
        .WithName("CheckPermissionsV1");
    }

    private static void MapOverrideAlias(WebApplication app)
    {
        app.MapPost("/api/v1/overrides", async (
            GrantReadinessOverrideRequest request,
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessOverrideService overrideService,
            ReadinessService readinessService,
            CancellationToken cancellationToken) =>
        {
            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new { code = "overrides.validation", message = "personId query parameter is required." });
            }

            authorization.RequireReadinessOverrideWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await overrideService.GrantOverrideAsync(tenantId, actorUserId, personId, request, cancellationToken);
            return Results.Ok(await readinessService.GetPersonReadinessAsync(
                tenantId,
                personId,
                cancellationToken,
                actorUserId,
                ReadinessService.PersonReadinessSnapshotKind));
        })
        .WithTags("Readiness")
        .RequireAuthorization()
        .WithName("GrantReadinessOverrideV1Alias");

        app.MapDelete("/api/v1/overrides", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessOverrideService overrideService,
            ReadinessService readinessService,
            CancellationToken cancellationToken) =>
        {
            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new { code = "overrides.validation", message = "personId query parameter is required." });
            }

            authorization.RequireReadinessOverrideWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await overrideService.ClearOverrideAsync(tenantId, actorUserId, personId, cancellationToken);
            return Results.Ok(await readinessService.GetPersonReadinessAsync(
                tenantId,
                personId,
                cancellationToken,
                actorUserId,
                ReadinessService.PersonReadinessSnapshotKind));
        })
        .WithTags("Readiness")
        .RequireAuthorization()
        .WithName("ClearReadinessOverrideV1Alias");
    }

    private static void MapOnboardingAlias(WebApplication app)
    {
        app.MapGet("/api/v1/onboarding", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            WorkforceOnboardingJourneyService journeyService,
            IStaffArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new { code = "onboarding.validation", message = "personId query parameter is required." });
            }

            authorization.RequirePersonHistoryRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var journey = await journeyService.GetForPersonAsync(tenantId, actorUserId, personId, cancellationToken);
            await audit.WriteAsync(
                WorkforceOnboardingJourneyService.ReadAction,
                tenantId,
                actorUserId,
                "workforce_onboarding_journey",
                personId.ToString(),
                journey.OverallStatus,
                cancellationToken: cancellationToken);
            return Results.Ok(journey);
        })
        .WithTags("People")
        .RequireAuthorization()
        .WithName("GetOnboardingV1Alias");
    }

    private static void MapDocumentAlias(WebApplication app)
    {
        app.MapGet("/api/v1/documents", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelDocumentService service,
            CancellationToken cancellationToken) =>
        {
            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new { code = "documents.validation", message = "personId query parameter is required." });
            }

            authorization.RequirePersonnelDocumentsRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListDocumentsAsync(tenantId, personId, cancellationToken));
        })
        .WithTags("Personnel Documents")
        .RequireAuthorization()
        .WithName("ListDocumentsV1Alias");

        app.MapPost("/api/v1/documents", async (
            Guid personId,
            CreatePersonnelDocumentRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelDocumentService service,
            CancellationToken cancellationToken) =>
        {
            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new { code = "documents.validation", message = "personId query parameter is required." });
            }

            authorization.RequirePersonnelDocumentsManageWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateDocumentAsync(
                tenantId,
                personId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Created($"/api/v1/documents/{created.DocumentId}?personId={personId}", created);
        })
        .WithTags("Personnel Documents")
        .RequireAuthorization()
        .WithName("CreateDocumentV1Alias");

        app.MapGet("/api/v1/documents/{documentId:guid}", async (
            Guid documentId,
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelDocumentService service,
            CancellationToken cancellationToken) =>
        {
            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new { code = "documents.validation", message = "personId query parameter is required." });
            }

            authorization.RequirePersonnelDocumentsRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetDocumentAsync(tenantId, personId, documentId, cancellationToken));
        })
        .WithTags("Personnel Documents")
        .RequireAuthorization()
        .WithName("GetDocumentV1Alias");

        app.MapGet("/api/v1/documents/{documentId:guid}/content", async (
            Guid documentId,
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelDocumentService service,
            CancellationToken cancellationToken) =>
        {
            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new { code = "documents.validation", message = "personId query parameter is required." });
            }

            authorization.RequirePersonnelDocumentsRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            var (metadata, stream) = await service.OpenDocumentContentAsync(
                tenantId,
                personId,
                documentId,
                cancellationToken);
            return Results.File(stream, metadata.ContentType, metadata.FileName);
        })
        .WithTags("Personnel Documents")
        .RequireAuthorization()
        .WithName("DownloadDocumentContentV1Alias");
    }

    private static void MapReportsIndexAlias(WebApplication app)
    {
        app.MapGet("/api/v1/reports", (
            StaffArrAuthorizationService authorization,
            HttpContext context) =>
        {
            authorization.RequirePersonnelReportRead(context.User);
            var items = new[]
            {
                new { key = "personnel", path = "/api/v1/reports/personnel", description = "Personnel summaries and exports." },
                new { key = "readiness", path = "/api/v1/reports/readiness", description = "Readiness summaries and exports." },
                new { key = "certifications", path = "/api/v1/reports/certifications", description = "Certification readiness, missing, and expiring report exports." },
                new { key = "incidents", path = "/api/v1/reports/incidents", description = "Incident summaries and exports." }
            };
            return Results.Ok(new { items });
        })
        .WithTags("Reports")
        .RequireAuthorization()
        .WithName("GetReportsIndexV1");
    }

    private static void MapIntegrationsIndexAlias(WebApplication app)
    {
        app.MapGet("/api/v1/integrations", (
            StaffArrAuthorizationService authorization,
            HttpContext context) =>
        {
            authorization.RequireStaffArrEntitlement(context.User);
            var items = new[]
            {
                new { key = "training-blockers", path = "/api/v1/integrations/training-blockers" },
                new { key = "training-blockers-clear", path = "/api/v1/integrations/training-blockers/clear" },
                new { key = "certification-grants", path = "/api/v1/integrations/certification-grants" },
                new { key = "certification-lifecycle", path = "/api/v1/integrations/certification-lifecycle" },
                new { key = "person-lookup", path = "/api/v1/integrations/person-lookup" },
                new { key = "person-history", path = "/api/v1/integrations/person-history" },
                new { key = "person-history-summary", path = "/api/v1/integrations/person-history/summary" },
                new { key = "routarr-readiness", path = "/api/v1/integrations/routarr-readiness" },
                new { key = "readiness-rollups-teams", path = "/api/v1/integrations/readiness-rollups/teams" },
                new { key = "readiness-rollups-sites", path = "/api/v1/integrations/readiness-rollups/sites" },
                new { key = "readiness-rollups-departments", path = "/api/v1/integrations/readiness-rollups/departments" },
                new { key = "supplyarr-demand-status", path = "/api/v1/integrations/supplyarr-demand-status" },
                new { key = "training-acknowledgements", path = "/api/v1/integrations/training-acknowledgements" },
                new { key = "training-acknowledgements-supersede", path = "/api/v1/integrations/training-acknowledgements/supersede" },
                new { key = "training-acknowledgements-status", path = "/api/v1/integrations/training-acknowledgements/status" },
                new { key = "procurement-approval-authority", path = "/api/v1/integrations/procurement-approval-authority" },
                new { key = "permission-check", path = "/api/v1/integrations/permission-check" }
            };
            return Results.Ok(new { items });
        })
        .WithTags("Integrations")
        .RequireAuthorization()
        .WithName("GetIntegrationsIndexV1");
    }
}
