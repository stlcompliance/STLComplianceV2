using TrainArr.Api.Services;



namespace STLCompliance.Shared.Worker.Tests;



public class OrphanReferenceRulesTests

{

    private static readonly DateTimeOffset AsOf = new(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);



    [Fact]

    public void IsStale_returns_true_when_never_scanned()

    {

        Assert.True(OrphanReferenceRules.IsStale(null, AsOf, 24));

    }



    [Fact]

    public void IsStale_respects_staleness_boundary()

    {

        var scannedAt = AsOf.AddHours(-25);

        Assert.True(OrphanReferenceRules.IsStale(scannedAt, AsOf, 24));

        Assert.False(OrphanReferenceRules.IsStale(AsOf.AddHours(-23), AsOf, 24));

    }



    [Theory]

    [InlineData(null, 10)]

    [InlineData(0, 1)]

    [InlineData(100, 50)]

    public void NormalizeBatchSize_clamps_to_supported_range(int? input, int expected)

    {

        Assert.Equal(expected, OrphanReferenceRules.NormalizeBatchSize(input));

    }



    [Theory]

    [InlineData(null, 24)]

    [InlineData(0, 1)]

    [InlineData(500, 168)]

    public void NormalizeStalenessHours_clamps_to_supported_range(int? input, int expected)

    {

        Assert.Equal(expected, OrphanReferenceRules.NormalizeStalenessHours(input));

    }



    [Fact]

    public void Build_reference_keys_use_expected_formats()

    {

        var personId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var citationId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");



        Assert.Equal(personId.ToString("D"), OrphanReferenceRules.BuildStaffarrPersonReferenceKey(personId));

        Assert.Equal(citationId.ToString("D"), OrphanReferenceRules.BuildComplianceCoreCitationReferenceKey(citationId));

        Assert.Equal("driver_qualification", OrphanReferenceRules.BuildComplianceCoreRulePackReferenceKey(" driver_qualification "));

    }

}


