using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class FieldCompanionScanPayloadParserTests
{
    private static readonly Guid AssignmentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid LoadArrTaskId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void TryExtractTaskKey_accepts_direct_task_key()
    {
        var raw = $"trainarr:assignment:{AssignmentId:D}";
        Assert.True(FieldCompanionScanPayloadParser.TryExtractTaskKey(raw, out var taskKey, out _));
        Assert.Equal(raw, taskKey);
    }

    [Fact]
    public void TryExtractTaskKey_accepts_stl_field_task_prefix()
    {
        var raw = $"stl-field-task:trainarr:assignment:{AssignmentId:D}";
        Assert.True(FieldCompanionScanPayloadParser.TryExtractTaskKey(raw, out var taskKey, out _));
        Assert.Equal($"trainarr:assignment:{AssignmentId:D}", taskKey);
    }

    [Fact]
    public void TryExtractTaskKey_accepts_json_payload()
    {
        var raw = $"{{\"taskKey\":\"trainarr:assignment:{AssignmentId:D}\"}}";
        Assert.True(FieldCompanionScanPayloadParser.TryExtractTaskKey(raw, out var taskKey, out _));
        Assert.Equal($"trainarr:assignment:{AssignmentId:D}", taskKey);
    }

    [Fact]
    public void TryExtractTaskKey_accepts_deep_link_path()
    {
        Assert.True(
            FieldCompanionScanPayloadParser.TryExtractTaskKey(
                $"/assignments/{AssignmentId:D}",
                out var taskKey,
                out _));
        Assert.Equal($"trainarr:assignment:{AssignmentId:D}", taskKey);
    }

    [Fact]
    public void TryExtractTaskKey_accepts_relative_loadarr_path_with_task_key_query()
    {
        Assert.True(
            FieldCompanionScanPayloadParser.TryExtractTaskKey(
                $"/work/receiving/recv-24018?taskKey=loadarr:receiving:{LoadArrTaskId:D}",
                out var taskKey,
                out _));
        Assert.Equal($"loadarr:receiving:{LoadArrTaskId:D}", taskKey);
    }

    [Fact]
    public void TryExtractTaskKey_rejects_legacy_supplyarr_receiving_path()
    {
        Assert.False(
            FieldCompanionScanPayloadParser.TryExtractTaskKey(
                $"/receiving/{AssignmentId:D}",
                out _,
                out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public void TryExtractTaskKey_rejects_unknown_payload()
    {
        Assert.False(FieldCompanionScanPayloadParser.TryExtractTaskKey("not-a-task", out _, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }
}
