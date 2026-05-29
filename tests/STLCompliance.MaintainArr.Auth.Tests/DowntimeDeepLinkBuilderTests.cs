using MaintainArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class DowntimeDeepLinkBuilderTests
{
    [Fact]
    public void BuildPath_includes_asset_work_order_defect_and_event_ids()
    {
        var assetId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var workOrderId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var defectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var eventId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var path = DowntimeDeepLinkBuilder.BuildPath(assetId, workOrderId, defectId, eventId);

        Assert.Equal(
            $"/downtime?assetId={assetId:D}&workOrderId={workOrderId:D}&defectId={defectId:D}&eventId={eventId:D}",
            path);
    }
}
