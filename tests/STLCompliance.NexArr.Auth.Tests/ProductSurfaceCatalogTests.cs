using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class ProductSurfaceCatalogTests
{
    [Fact]
    public void BuildSurfaces_for_staffarr_includes_overview_and_launch()
    {
        var surfaces = ProductSurfaceCatalog.BuildSurfaces(
            "staffarr",
            "available",
            hasProductEntitlement: true,
            isPlatformAdmin: false);

        Assert.Contains(surfaces, s => s.SurfaceKey == "overview" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "launch" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "launch" && s.PermissionHint != null);
    }

    [Fact]
    public void BuildSurfaces_for_assurarr_and_loadarr_include_live_navigation_surfaces()
    {
        var assurarrSurfaces = ProductSurfaceCatalog.BuildSurfaces(
            "assurarr",
            "available",
            hasProductEntitlement: true,
            isPlatformAdmin: false);
        var loadarrSurfaces = ProductSurfaceCatalog.BuildSurfaces(
            "loadarr",
            "available",
            hasProductEntitlement: true,
            isPlatformAdmin: false);

        Assert.Contains(assurarrSurfaces, s => s.SurfaceKey == "cases" && s.IsEnabled);
        Assert.Contains(assurarrSurfaces, s => s.SurfaceKey == "capa" && s.IsEnabled);
        Assert.Contains(assurarrSurfaces, s => s.SurfaceKey == "launch" && s.IsEnabled);

        Assert.Contains(loadarrSurfaces, s => s.SurfaceKey == "inventory" && s.IsEnabled);
        Assert.Contains(loadarrSurfaces, s => s.SurfaceKey == "receiving" && s.IsEnabled);
        Assert.Contains(loadarrSurfaces, s => s.SurfaceKey == "launch" && s.IsEnabled);
    }

    [Fact]
    public void BuildSurfaces_for_supplyarr_reflects_procurement_owned_surfaces()
    {
        var surfaces = ProductSurfaceCatalog.BuildSurfaces(
            "supplyarr",
            "available",
            hasProductEntitlement: true,
            isPlatformAdmin: false);

        Assert.Contains(surfaces, s => s.SurfaceKey == "parties" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "catalog" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "purchasing" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "pricing" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "planning" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "readiness" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "launch" && s.IsEnabled);
        Assert.DoesNotContain(surfaces, s => s.SurfaceKey == "inventory");
        Assert.DoesNotContain(surfaces, s => s.SurfaceKey == "procurement");
    }

    [Fact]
    public void BuildSurfaces_for_customarr_includes_launch_surface()
    {
        var surfaces = ProductSurfaceCatalog.BuildSurfaces(
            "customarr",
            "available",
            hasProductEntitlement: true,
            isPlatformAdmin: false);

        Assert.Contains(surfaces, s => s.SurfaceKey == "launch" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "launch" && s.Label.Contains("CustomArr", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildSurfaces_for_fieldcompanion_includes_field_companion_navigation()
    {
        var surfaces = ProductSurfaceCatalog.BuildSurfaces(
            "fieldcompanion",
            "available",
            hasProductEntitlement: true,
            isPlatformAdmin: false);

        Assert.Contains(surfaces, s => s.SurfaceKey == "inbox" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "capture" && s.IsEnabled);
        Assert.Contains(
            surfaces,
            s => s.SurfaceKey == "launch" && s.Label.Contains("Field Companion", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildSurfaces_platform_admin_gate_on_nexarr_tenants()
    {
        var withoutAdmin = ProductSurfaceCatalog.BuildSurfaces("nexarr", "available", true, isPlatformAdmin: false);
        var tenants = withoutAdmin.First(s => s.SurfaceKey == "tenants");
        Assert.False(tenants.IsEnabled);
        Assert.Contains("platform administrator", tenants.PermissionHint!, StringComparison.OrdinalIgnoreCase);

        var withAdmin = ProductSurfaceCatalog.BuildSurfaces("nexarr", "available", true, isPlatformAdmin: true);
        Assert.True(withAdmin.First(s => s.SurfaceKey == "tenants").IsEnabled);
    }

    [Fact]
    public void BuildSurfaces_platform_admin_gate_on_compliancecore()
    {
        var withoutAdmin = ProductSurfaceCatalog.BuildSurfaces(
            "compliancecore",
            "available",
            hasProductEntitlement: true,
            isPlatformAdmin: false);

        Assert.All(withoutAdmin, surface => Assert.False(surface.IsEnabled));
        Assert.All(
            withoutAdmin,
            surface => Assert.Contains("platform administrator", surface.PermissionHint!, StringComparison.OrdinalIgnoreCase));

        var withAdmin = ProductSurfaceCatalog.BuildSurfaces(
            "compliancecore",
            "available",
            hasProductEntitlement: true,
            isPlatformAdmin: true);

        Assert.Contains(withAdmin, surface => surface.SurfaceKey == "launch" && surface.IsEnabled);
        Assert.Contains(withAdmin, surface => surface.SurfaceKey == "rules" && surface.IsEnabled);
    }

    [Fact]
    public void BuildSurfaces_without_entitlement_marks_surfaces_disabled()
    {
        var surfaces = ProductSurfaceCatalog.BuildSurfaces(
            "routarr",
            "available",
            hasProductEntitlement: false,
            isPlatformAdmin: false);

        Assert.All(surfaces, s => Assert.False(s.IsEnabled));
        Assert.All(surfaces, s => Assert.NotNull(s.PermissionHint));
    }

    [Fact]
    public void BuildSurfaces_for_worker_product_does_not_include_launch_surface()
    {
        var surfaces = ProductSurfaceCatalog.BuildSurfaces(
            "shared-worker",
            "worker",
            hasProductEntitlement: true,
            isPlatformAdmin: true);

        Assert.Contains(surfaces, s => s.SurfaceKey == "overview" && s.IsEnabled);
        Assert.DoesNotContain(surfaces, s => s.SurfaceKey == "launch");
    }
}
