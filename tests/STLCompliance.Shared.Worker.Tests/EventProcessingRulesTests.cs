using TrainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class EventProcessingRulesTests
{
    [Theory]
    [InlineData(null, 25)]
    [InlineData(0, 1)]
    [InlineData(500, 200)]
    public void NormalizeBatchSize_clamps(int? input, int expected) =>
        Assert.Equal(expected, EventProcessingRules.NormalizeBatchSize(input));

    [Fact]
    public void BuildIdempotencyKey_is_stable() =>
        Assert.Equal(
            "assignment_created:training_assignment:11111111-1111-1111-1111-111111111111",
            EventProcessingRules.BuildIdempotencyKey(
                "assignment_created",
                "training_assignment",
                Guid.Parse("11111111-1111-1111-1111-111111111111")));
}
