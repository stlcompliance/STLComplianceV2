using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class ProductSurfaceCatalogTests
{
    [Fact]
    public void BuildSurfaces_for_staffarr_includes_overview_and_launch()
    {
        var surfaces = ProductSurfaceCatalog.BuildSurfaces("staffarr", hasProductEntitlement: true, isPlatformAdmin: false);

        Assert.Contains(surfaces, s => s.SurfaceKey == "overview" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "launch" && s.IsEnabled);
        Assert.Contains(surfaces, s => s.SurfaceKey == "launch" && s.PermissionHint != null);
    }

    [Fact]
    public void BuildSurfaces_platform_admin_gate_on_nexarr_tenants()
    {
        var withoutAdmin = ProductSurfaceCatalog.BuildSurfaces("nexarr", true, isPlatformAdmin: false);
        var tenants = withoutAdmin.First(s => s.SurfaceKey == "tenants");
        Assert.False(tenants.IsEnabled);
        Assert.Contains("platform administrator", tenants.PermissionHint!, StringComparison.OrdinalIgnoreCase);

        var withAdmin = ProductSurfaceCatalog.BuildSurfaces("nexarr", true, isPlatformAdmin: true);
        Assert.True(withAdmin.First(s => s.SurfaceKey == "tenants").IsEnabled);
    }

    [Fact]
    public void BuildSurfaces_without_entitlement_marks_surfaces_disabled()
    {
        var surfaces = ProductSurfaceCatalog.BuildSurfaces("routarr", hasProductEntitlement: false, isPlatformAdmin: false);

        Assert.All(surfaces, s => Assert.False(s.IsEnabled));
        Assert.All(surfaces, s => Assert.NotNull(s.PermissionHint));
    }
}
