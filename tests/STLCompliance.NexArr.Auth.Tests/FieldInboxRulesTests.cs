using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class FieldInboxRulesTests
{
    [Fact]
    public void BuildAggregatedResponse_orders_blocked_tasks_after_open_tasks()
    {
        var sources = new[]
        {
            new FieldInboxProductSlice(
                "maintainarr",
                true,
                true,
                null,
                null,
                [
                    new FieldInboxTaskItem(
                        "maintainarr:work-order:1",
                        "maintainarr",
                        "work_order",
                        "Blocked WO",
                        null,
                        "open",
                        "high",
                        null,
                        DateTimeOffset.Parse("2026-05-27T10:00:00Z"),
                        "/work-orders/1",
                        "Waiting on parts"),
                    new FieldInboxTaskItem(
                        "maintainarr:work-order:2",
                        "maintainarr",
                        "work_order",
                        "Ready WO",
                        null,
                        "open",
                        "medium",
                        null,
                        DateTimeOffset.Parse("2026-05-27T09:00:00Z"),
                        "/work-orders/2"),
                ]),
        };

        var response = FieldInboxRules.BuildAggregatedResponse(sources);

        Assert.Equal(2, response.Summary.TotalCount);
        Assert.Equal(1, response.Summary.BlockedCount);
        Assert.Equal("Ready WO", response.Items[0].Title);
        Assert.Equal("Blocked WO", response.Items[1].Title);
    }

    [Fact]
    public void BuildProductDeepLinkUrl_combines_frontend_base_and_path()
    {
        var url = FieldInboxDeepLinkBuilder.BuildProductDeepLinkUrl(
            "https://trainarr-frontend.onrender.com/",
            "/assignments/00000000-0000-0000-0000-000000000099/evidence");

        Assert.Equal(
            "https://trainarr-frontend.onrender.com/assignments/00000000-0000-0000-0000-000000000099/evidence",
            url);
    }

    [Fact]
    public void BuildProductDeepLinkUrl_returns_null_when_base_missing()
    {
        Assert.Null(FieldInboxDeepLinkBuilder.BuildProductDeepLinkUrl(null, "/assignments/1"));
    }
}
