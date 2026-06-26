using STLCompliance.Shared.Integration;

namespace STLCompliance.Shared.Tests;

public sealed class StlSuiteEventCatalogTests
{
    [Fact]
    public void NexArr_launch_destination_events_are_canonical()
    {
        Assert.Equal("nexarr.launch_destination.granted", StlSuiteEventCatalog.NexArr.LaunchDestinationGranted);
        Assert.Equal("nexarr.launch_destination.revoked", StlSuiteEventCatalog.NexArr.LaunchDestinationRevoked);
        Assert.Equal(StlSuiteEventCatalog.NexArr.LaunchDestinationGranted, StlSuiteEventCatalog.NexArr.AvailabilityGranted);
        Assert.Equal(StlSuiteEventCatalog.NexArr.LaunchDestinationRevoked, StlSuiteEventCatalog.NexArr.AvailabilityRevoked);
    }

    [Fact]
    public void NexArr_legacy_entitlement_events_remain_available_for_compatibility()
    {
        Assert.Equal(
            StlSuiteEventCatalog.NexArr.LaunchDestinationGranted,
            StlSuiteEventCatalog.NexArr.EntitlementGranted);
        Assert.Equal(
            StlSuiteEventCatalog.NexArr.LaunchDestinationRevoked,
            StlSuiteEventCatalog.NexArr.EntitlementRevoked);
    }
}
