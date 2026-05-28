using RoutArr.Api.Entities;
using RoutArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrAuditPackageGenerationRulesTests
{
    [Theory]
    [InlineData(null, 5)]
    [InlineData(0, 1)]
    [InlineData(100, 25)]
    public void NormalizeBatchSize_clamps(int? input, int expected) =>
        Assert.Equal(expected, AuditPackageGenerationRules.NormalizeBatchSize(input));

    [Theory]
    [InlineData("zip", "zip")]
    [InlineData("JSON", "json")]
    public void NormalizeFormat_accepts_zip_and_json(string raw, string expected) =>
        Assert.Equal(expected, AuditPackageGenerationRules.NormalizeFormat(raw));

    [Fact]
    public void NormalizeFormat_rejects_invalid()
    {
        var ex = Assert.Throws<StlApiException>(() => AuditPackageGenerationRules.NormalizeFormat("pdf"));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void IsDownloadReady_requires_completed_artifact()
    {
        var zipJob = new AuditPackageGenerationJob
        {
            Status = AuditPackageGenerationJobStatuses.Completed,
            Format = AuditPackageGenerationFormats.Zip,
            ArtifactZip = [1, 2, 3],
        };
        Assert.True(AuditPackageGenerationRules.IsDownloadReady(zipJob));

        var jsonJob = new AuditPackageGenerationJob
        {
            Status = AuditPackageGenerationJobStatuses.Completed,
            Format = AuditPackageGenerationFormats.Json,
            ArtifactJson = "{}",
        };
        Assert.True(AuditPackageGenerationRules.IsDownloadReady(jsonJob));

        var pending = new AuditPackageGenerationJob
        {
            Status = AuditPackageGenerationJobStatuses.Pending,
            Format = AuditPackageGenerationFormats.Zip,
        };
        Assert.False(AuditPackageGenerationRules.IsDownloadReady(pending));
    }
}
