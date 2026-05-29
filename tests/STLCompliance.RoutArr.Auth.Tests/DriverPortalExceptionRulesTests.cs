using RoutArr.Api.Entities;
using RoutArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DriverPortalExceptionRulesTests
{
    [Theory]
    [InlineData(DriverPortalExceptionRules.TrafficDelay, DispatchExceptionCategories.Delay)]
    [InlineData(DriverPortalExceptionRules.EquipmentIssue, DispatchExceptionCategories.Vehicle)]
    [InlineData(DriverPortalExceptionRules.CustomerAccess, DispatchExceptionCategories.Stop)]
    [InlineData(DriverPortalExceptionRules.RouteIssue, DispatchExceptionCategories.Route)]
    [InlineData(DriverPortalExceptionRules.Other, DispatchExceptionCategories.Other)]
    public void MapExceptionTypeToCategory_maps_driver_types(string exceptionType, string expectedCategory)
    {
        var category = DriverPortalExceptionRules.MapExceptionTypeToCategory(exceptionType);
        Assert.Equal(expectedCategory, category);
    }

    [Fact]
    public void EnsureReportableTripStatus_rejects_completed_trips()
    {
        var ex = Assert.Throws<StlApiException>(() =>
            DriverPortalExceptionRules.EnsureReportableTripStatus(TripDispatchStatuses.Completed));
        Assert.Equal("driver_portal.exception.trip_not_reportable", ex.Code);
    }

    [Fact]
    public void BuildDriverReportedDescription_prefixes_notes()
    {
        Assert.Equal("[Driver-reported] Delay at dock", DriverPortalExceptionRules.BuildDriverReportedDescription("Delay at dock"));
        Assert.Equal("[Driver-reported]", DriverPortalExceptionRules.BuildDriverReportedDescription("  "));
    }
}
