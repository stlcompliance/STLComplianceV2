using ComplianceCore.Api.Contracts;

using ComplianceCore.Api.Services;

using STLCompliance.Shared.Auth;



namespace ComplianceCore.Api.Endpoints;



public static class RegulatoryRegistryEndpoints

{

    public static void MapComplianceCoreRegulatoryRegistryEndpoints(this WebApplication app)

    {

        var governingBodies = app.MapGroup("/api/governing-bodies")

            .WithTags("RegulatoryRegistries")

            .RequireAuthorization();



        governingBodies.MapGet("/", async (

            ComplianceCoreAuthorizationService authorization,

            GoverningBodyService service,

            HttpContext context,

            CancellationToken cancellationToken) =>

        {

            authorization.RequireRegulatoryRead(context.User);

            var tenantId = context.User.GetTenantId();

            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));

        })

        .WithName("ListGoverningBodies");



        governingBodies.MapPost("/", async (

            CreateGoverningBodyRequest request,

            ComplianceCoreAuthorizationService authorization,

            GoverningBodyService service,

            HttpContext context,

            CancellationToken cancellationToken) =>

        {

            authorization.RequireRegulatoryManage(context.User);

            var tenantId = context.User.GetTenantId();

            var created = await service.CreateAsync(

                tenantId,

                context.User.GetUserId(),

                request,

                cancellationToken);

            return Results.Created($"/api/governing-bodies/{created.GoverningBodyId}", created);

        })

        .WithName("CreateGoverningBody");



        var jurisdictions = app.MapGroup("/api/jurisdictions")

            .WithTags("RegulatoryRegistries")

            .RequireAuthorization();



        jurisdictions.MapGet("/", async (

            Guid? governingBodyId,

            ComplianceCoreAuthorizationService authorization,

            JurisdictionService service,

            HttpContext context,

            CancellationToken cancellationToken) =>

        {

            authorization.RequireRegulatoryRead(context.User);

            var tenantId = context.User.GetTenantId();

            return Results.Ok(await service.ListAsync(tenantId, governingBodyId, cancellationToken));

        })

        .WithName("ListJurisdictions");



        jurisdictions.MapPost("/", async (

            CreateJurisdictionRequest request,

            ComplianceCoreAuthorizationService authorization,

            JurisdictionService service,

            HttpContext context,

            CancellationToken cancellationToken) =>

        {

            authorization.RequireRegulatoryManage(context.User);

            var tenantId = context.User.GetTenantId();

            var created = await service.CreateAsync(

                tenantId,

                context.User.GetUserId(),

                request,

                cancellationToken);

            return Results.Created($"/api/jurisdictions/{created.JurisdictionId}", created);

        })

        .WithName("CreateJurisdiction");



        var programs = app.MapGroup("/api/regulatory-programs")

            .WithTags("RegulatoryRegistries")

            .RequireAuthorization();



        programs.MapGet("/", async (

            Guid? jurisdictionId,

            ComplianceCoreAuthorizationService authorization,

            RegulatoryProgramService service,

            HttpContext context,

            CancellationToken cancellationToken) =>

        {

            authorization.RequireRegulatoryRead(context.User);

            var tenantId = context.User.GetTenantId();

            return Results.Ok(await service.ListAsync(tenantId, jurisdictionId, cancellationToken));

        })

        .WithName("ListRegulatoryPrograms");



        programs.MapPost("/", async (

            CreateRegulatoryProgramRequest request,

            ComplianceCoreAuthorizationService authorization,

            RegulatoryProgramService service,

            HttpContext context,

            CancellationToken cancellationToken) =>

        {

            authorization.RequireRegulatoryManage(context.User);

            var tenantId = context.User.GetTenantId();

            var created = await service.CreateAsync(

                tenantId,

                context.User.GetUserId(),

                request,

                cancellationToken);

            return Results.Created($"/api/regulatory-programs/{created.RegulatoryProgramId}", created);

        })

        .WithName("CreateRegulatoryProgram");

    }

}


