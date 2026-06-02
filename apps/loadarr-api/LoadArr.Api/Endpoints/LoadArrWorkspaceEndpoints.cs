namespace LoadArr.Api.Endpoints;

public static class LoadArrWorkspaceEndpoints
{
    public static void MapLoadArrWorkspaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/workspace")
            .WithTags("Workspace")
            .RequireAuthorization();

        group.MapGet("/site-sources", () => Results.Ok(new
        {
            canonicalInternalSites = "staffarr",
            selectableInventoryLocations = "supplyarr",
            canonicalSiteField = "staffarrSiteOrgUnitId",
            siteSnapshotField = "staffarrSiteNameSnapshot"
        }))
        .WithName("GetLoadArrSiteSources");
    }
}
