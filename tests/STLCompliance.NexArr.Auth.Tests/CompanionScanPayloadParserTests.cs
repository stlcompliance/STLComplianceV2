using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class CompanionScanPayloadParserTests
{
    private static readonly Guid AssignmentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public void TryExtractTaskKey_accepts_direct_task_key()
    {
        var raw = $"trainarr:assignment:{AssignmentId:D}";
        Assert.True(CompanionScanPayloadParser.TryExtractTaskKey(raw, out var taskKey, out _));
        Assert.Equal(raw, taskKey);
    }

    [Fact]
    public void TryExtractTaskKey_accepts_stl_field_task_prefix()
    {
        var raw = $"stl-field-task:trainarr:assignment:{AssignmentId:D}";
        Assert.True(CompanionScanPayloadParser.TryExtractTaskKey(raw, out var taskKey, out _));
        Assert.Equal($"trainarr:assignment:{AssignmentId:D}", taskKey);
    }

    [Fact]
    public void TryExtractTaskKey_accepts_json_payload()
    {
        var raw = $"{{\"taskKey\":\"trainarr:assignment:{AssignmentId:D}\"}}";
        Assert.True(CompanionScanPayloadParser.TryExtractTaskKey(raw, out var taskKey, out _));
        Assert.Equal($"trainarr:assignment:{AssignmentId:D}", taskKey);
    }

    [Fact]
    public void TryExtractTaskKey_accepts_deep_link_path()
    {
        Assert.True(
            CompanionScanPayloadParser.TryExtractTaskKey(
                $"/assignments/{AssignmentId:D}",
                out var taskKey,
                out _));
        Assert.Equal($"trainarr:assignment:{AssignmentId:D}", taskKey);
    }

    [Fact]
    public void TryExtractTaskKey_rejects_unknown_payload()
    {
        Assert.False(CompanionScanPayloadParser.TryExtractTaskKey("not-a-task", out _, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }
}
