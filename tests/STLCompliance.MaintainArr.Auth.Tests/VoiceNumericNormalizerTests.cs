using MaintainArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class VoiceNumericNormalizerTests
{
    [Theory]
    [InlineData("12.5", 12.5)]
    [InlineData("twelve point five", 12.5)]
    [InlineData("one hundred", 100)]
    [InlineData("twenty three", 23)]
    public void Normalize_parses_numeric_transcripts(string transcript, decimal expected)
    {
        var result = VoiceNumericNormalizer.Normalize(transcript);
        Assert.True(result.Understood);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Normalize_rejects_unrecognized_transcript()
    {
        var result = VoiceNumericNormalizer.Normalize("looks fine");
        Assert.False(result.Understood);
        Assert.Null(result.Value);
    }
}
