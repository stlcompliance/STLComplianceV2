namespace MaintainArr.Api.Services;

public static class DowntimeDeepLinkBuilder
{
    public static string BuildPath(
        Guid assetId,
        Guid? workOrderId = null,
        Guid? defectId = null,
        Guid? eventId = null)
    {
        var query = new List<string> { $"assetId={assetId:D}" };
        if (workOrderId is Guid workOrder)
        {
            query.Add($"workOrderId={workOrder:D}");
        }

        if (defectId is Guid defect)
        {
            query.Add($"defectId={defect:D}");
        }

        if (eventId is Guid downtimeEvent)
        {
            query.Add($"eventId={downtimeEvent:D}");
        }

        return $"/downtime?{string.Join("&", query)}";
    }
}
