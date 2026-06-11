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
        MapIntegrationSurfaceAliases(app);
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

    private static void MapIntegrationSurfaceAliases(WebApplication app)
    {
        var integrations = app.MapGroup("/api/v1/integrations")
            .WithTags("Integrations")
            .RequireAuthorization();

        integrations.MapGet("/persons", async (
            string? query,
            Guid? orgUnitId,
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, query, orgUnitId, limit ?? 50, cancellationToken));
        })
        .WithName("ListIntegrationPersonsV1Alias");

        integrations.MapGet("/persons/{personId:guid}", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetByIdAsync(tenantId, personId, cancellationToken));
        })
        .WithName("GetIntegrationPersonV1Alias");

        integrations.MapGet("/persons/{personId:guid}/readiness", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.GetPersonReadinessAsync(
                tenantId,
                personId,
                cancellationToken,
                actorUserId,
                ReadinessService.PersonReadinessSnapshotKind));
        })
        .WithName("GetIntegrationPersonReadinessV1Alias");

        integrations.MapGet("/persons/{personId:guid}/permissions", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PermissionProjectionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePermissionProjectionRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetEffectivePermissionProjectionAsync(
                tenantId,
                personId,
                cancellationToken));
        })
        .WithName("GetIntegrationPersonPermissionsV1Alias");

        integrations.MapGet("/persons/{personId:guid}/qualifications-snapshot", async (
            Guid personId,
            int? limit,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            TrainarrPersonTrainingHistoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonHistoryRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.GetForPersonAsync(
                tenantId,
                actorUserId,
                personId,
                limit,
                cancellationToken));
        })
        .WithName("GetIntegrationPersonQualificationsSnapshotV1Alias");

        integrations.MapGet("/persons/{personId:guid}/history", async (
            Guid personId,
            int? page,
            int? pageSize,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonnelHistoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonHistoryRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPersonHistoryAsync(
                tenantId,
                personId,
                page ?? 1,
                pageSize ?? 50,
                cancellationToken));
        })
        .WithName("GetIntegrationPersonHistoryV1Alias");

        integrations.MapGet("/persons/{personId:guid}/summary", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PeopleService peopleService,
            ReadinessService readinessService,
            PermissionProjectionService permissionProjectionService,
            TrainarrPersonTrainingHistoryService trainingHistoryService,
            PersonnelHistoryService historyService,
            ReadinessOverrideService overrideService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireStaffArrEntitlement(context.User);
            var currentPersonId = context.User.GetPersonId();
            if (currentPersonId != personId)
            {
                authorization.RequirePeopleRead(context.User);
            }
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();

            var person = await peopleService.GetByIdAsync(tenantId, personId, cancellationToken);
            var readiness = await readinessService.GetPersonReadinessAsync(
                tenantId,
                personId,
                cancellationToken,
                actorUserId,
                ReadinessService.PersonReadinessSnapshotKind);
            var permissions = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                tenantId,
                personId,
                cancellationToken);
            var qualifications = await trainingHistoryService.GetForPersonAsync(
                tenantId,
                actorUserId,
                personId,
                limit: 20,
                cancellationToken);
            var historySummary = await historyService.GetSummaryAsync(tenantId, personId, cancellationToken);
            var activeOverride = await overrideService.GetEffectiveActiveOverrideAsync(
                tenantId,
                personId,
                cancellationToken);

            return Results.Ok(new StaffArrPersonIntegrationSummaryResponse(
                person,
                readiness,
                permissions,
                qualifications,
                historySummary,
                activeOverride is null
                    ? []
                    : [new ReadinessOverrideResponse(
                        activeOverride.Id,
                        activeOverride.PersonId,
                        activeOverride.Status,
                        activeOverride.Reason,
                        activeOverride.GrantedAt,
                        activeOverride.ExpiresAt,
                        activeOverride.GrantedByUserId,
                        activeOverride.ClearedAt,
                        activeOverride.ClearedByUserId)]));
        })
        .WithName("GetIntegrationPersonSummaryV1Alias");

        integrations.MapGet("/persons/{personId:guid}/restrictions", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessOverrideService overrideService,
            ReadinessService readinessService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var readiness = await readinessService.GetPersonReadinessAsync(
                tenantId,
                personId,
                cancellationToken,
                actorUserId,
                ReadinessService.PersonReadinessSnapshotKind);
            var activeOverride = await overrideService.GetEffectiveActiveOverrideAsync(tenantId, personId, cancellationToken);

            return Results.Ok(new StaffArrRestrictionSnapshotResponse(
                personId,
                activeOverride is null
                    ? []
                    : [new ReadinessOverrideResponse(
                        activeOverride.Id,
                        activeOverride.PersonId,
                        activeOverride.Status,
                        activeOverride.Reason,
                        activeOverride.GrantedAt,
                        activeOverride.ExpiresAt,
                        activeOverride.GrantedByUserId,
                        activeOverride.ClearedAt,
                        activeOverride.ClearedByUserId)],
                readiness.Blockers));
        })
        .WithName("GetIntegrationPersonRestrictionsV1Alias");

        integrations.MapPost("/person-readiness-checks", async (
            PersonReadinessCheckRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRead(context.User, request.PersonId);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.GetPersonReadinessAsync(
                tenantId,
                request.PersonId,
                cancellationToken,
                actorUserId,
                ReadinessService.PersonReadinessSnapshotKind));
        })
        .WithName("CreateIntegrationPersonReadinessCheckV1Alias");

        integrations.MapPost("/permission-checks", async (
            PersonPermissionCheckRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IntegrationPermissionCheckService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePermissionProjectionRead(context.User, request.PersonId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.CheckAsync(
                tenantId,
                request.PersonId,
                request.PermissionKey,
                cancellationToken));
        })
        .WithName("CreateIntegrationPermissionCheckV1Alias");

        integrations.MapPost("/assignment-checks", async (
            PersonAssignmentCheckRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessService readinessService,
            PermissionProjectionService permissionProjectionService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRead(context.User, request.PersonId);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var readiness = await readinessService.GetPersonReadinessAsync(
                tenantId,
                request.PersonId,
                cancellationToken,
                actorUserId,
                ReadinessService.PersonReadinessSnapshotKind);
            var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                tenantId,
                request.PersonId,
                cancellationToken);
            var permissionKeys = request.PermissionKey
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var granted = permissionKeys.Length == 0
                || permissionKeys.All(key => projection.Permissions.Any(permission => string.Equals(permission.PermissionKey, key, StringComparison.OrdinalIgnoreCase)));
            var blockingReasons = readiness.Blockers
                .Select(blocker => blocker.Message)
                .ToList();
            if (!granted)
            {
                blockingReasons.Add("Missing one or more required permissions.");
            }

            return Results.Ok(new StaffArrAssignmentCheckResponse(
                request.PersonId,
                granted && string.Equals(readiness.ReadinessStatus, "ready", StringComparison.OrdinalIgnoreCase),
                readiness,
                projection,
                blockingReasons));
        })
        .WithName("CreateIntegrationAssignmentCheckV1Alias");

        integrations.MapPost("/restrictions", async (
            CreateRestrictionRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessOverrideService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessOverrideWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.GrantOverrideAsync(
                tenantId,
                actorUserId,
                request.PersonId,
                new GrantReadinessOverrideRequest(request.Reason, request.ExpiresAt),
                cancellationToken);
            return Results.Ok(created);
        })
        .WithName("CreateIntegrationRestrictionV1Alias");

        integrations.MapPost("/restrictions/{restrictionId:guid}/lift", async (
            Guid restrictionId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessOverrideService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessOverrideWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ClearOverrideByIdAsync(
                tenantId,
                actorUserId,
                restrictionId,
                cancellationToken));
        })
        .WithName("LiftIntegrationRestrictionV1Alias");

        integrations.MapPost("/person-history-events", async (
            PersonHistoryEventRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IStaffArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonHistoryRead(context.User, request.PersonId);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await audit.WriteAsync(
                request.Action,
                tenantId,
                actorUserId,
                request.TargetType ?? "person_history_event",
                request.TargetId ?? request.PersonId.ToString(),
                request.Result,
                request.ReasonCode,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CreateIntegrationPersonHistoryEventV1Alias");

        integrations.MapGet("/org-units", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            OrgUnitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName("ListIntegrationOrgUnitsV1Alias");

        integrations.MapGet("/org-units/{orgUnitId:guid}", async (
            Guid orgUnitId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            OrgUnitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            var units = await service.ListAsync(tenantId, cancellationToken);
            var unit = units.FirstOrDefault(item => item.OrgUnitId == orgUnitId);
            return unit is null
                ? Results.NotFound(new { code = "org_unit.not_found", message = "Org unit was not found." })
                : Results.Ok(unit);
        })
        .WithName("GetIntegrationOrgUnitV1Alias");

        integrations.MapGet("/departments", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            OrgUnitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            var units = await service.ListAsync(tenantId, cancellationToken);
            return Results.Ok(units.Where(x => x.UnitType == "department").ToList());
        })
        .WithName("ListIntegrationDepartmentsV1Alias");

        integrations.MapGet("/positions", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            OrgUnitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            var units = await service.ListAsync(tenantId, cancellationToken);
            return Results.Ok(units.Where(x => x.UnitType == "position").ToList());
        })
        .WithName("ListIntegrationPositionsV1Alias");

        integrations.MapGet("/teams", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            OrgUnitService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            var units = await service.ListAsync(tenantId, cancellationToken);
            return Results.Ok(units.Where(x => x.UnitType == "team").ToList());
        })
        .WithName("ListIntegrationTeamsV1Alias");

        integrations.MapGet("/incidents", async (
            Guid? personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentsRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListIncidentsAsync(tenantId, personId, cancellationToken));
        })
        .WithName("ListIntegrationIncidentsV1Alias");

        integrations.MapPost("/incidents", async (
            CreatePersonnelIncidentRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentsManageWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CreateIncidentAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("CreateIntegrationIncidentV1Alias");

        integrations.MapGet("/incidents/{incidentId:guid}", async (
            Guid incidentId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentService service,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetIncidentAsync(tenantId, incidentId, cancellationToken);
            authorization.RequireIncidentsRead(context.User, detail.PersonId);
            return Results.Ok(detail);
        })
        .WithName("GetIntegrationIncidentV1Alias");

        integrations.MapPost("/incidents/{incidentId:guid}/status-updates", async (
            Guid incidentId,
            UpdatePersonnelIncidentStatusRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentsManageWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateIncidentStatusAsync(
                tenantId,
                actorUserId,
                incidentId,
                request,
                cancellationToken));
        })
        .WithName("UpdateIntegrationIncidentStatusV1Alias");

        integrations.MapPost("/incidents/{incidentId:guid}/training-impact", async (
            Guid incidentId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentRoutingService routingService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentsManageWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await routingService.RouteToTrainarrAsync(
                tenantId,
                actorUserId,
                incidentId,
                cancellationToken));
        })
        .WithName("RouteIntegrationIncidentTrainingImpactV1Alias");
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
                new { key = "persons", path = "/api/v1/integrations/persons" },
                new { key = "person-readiness-checks", path = "/api/v1/integrations/person-readiness-checks" },
                new { key = "permission-checks", path = "/api/v1/integrations/permission-checks" },
                new { key = "assignment-checks", path = "/api/v1/integrations/assignment-checks" },
                new { key = "restrictions", path = "/api/v1/integrations/restrictions" },
                new { key = "person-history-events", path = "/api/v1/integrations/person-history-events" },
                new { key = "routarr-readiness", path = "/api/v1/integrations/routarr-readiness" },
                new { key = "readiness-rollups-teams", path = "/api/v1/integrations/readiness-rollups/teams" },
                new { key = "readiness-rollups-sites", path = "/api/v1/integrations/readiness-rollups/sites" },
                new { key = "readiness-rollups-departments", path = "/api/v1/integrations/readiness-rollups/departments" },
                new { key = "org-units", path = "/api/v1/integrations/org-units" },
                new { key = "sites", path = "/api/v1/integrations/sites" },
                new { key = "locations", path = "/api/v1/integrations/locations" },
                new { key = "departments", path = "/api/v1/integrations/departments" },
                new { key = "positions", path = "/api/v1/integrations/positions" },
                new { key = "teams", path = "/api/v1/integrations/teams" },
                new { key = "incidents", path = "/api/v1/integrations/incidents" },
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
