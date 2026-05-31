using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

[CollectionDefinition("PasswordResetRules", DisableParallelization = true)]
public sealed class PasswordResetRulesCollection;

[Collection("PasswordResetRules")]
public class PasswordResetRulesTests
{
    [Theory]
    [InlineData("Short1a", false)]
    [InlineData("longenoughbutnocaps1", false)]
    [InlineData("LONGENOUGHNOLOWER1", false)]
    [InlineData("NoDigitsHereAtAll!", false)]
    [InlineData("ValidPassword1!", true)]
    public void MeetsPasswordPolicy_validates_complexity(string password, bool expected) =>
        Assert.Equal(expected, PasswordResetRules.MeetsPasswordPolicy(password));

    [Fact]
    public void MeetsPasswordPolicy_uses_configured_min_length_when_provided()
    {
        var previous = Environment.GetEnvironmentVariable("AUTH_PASSWORD_MIN_LENGTH");
        Environment.SetEnvironmentVariable("AUTH_PASSWORD_MIN_LENGTH", "8");

        try
        {
            Assert.True(PasswordResetRules.MeetsPasswordPolicy("Valid1Ab"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("AUTH_PASSWORD_MIN_LENGTH", previous);
        }
    }

    [Fact]
    public void MeetsPasswordPolicy_can_disable_complexity_requirement()
    {
        var previous = Environment.GetEnvironmentVariable("AUTH_REQUIRE_PASSWORD_COMPLEXITY");
        Environment.SetEnvironmentVariable("AUTH_REQUIRE_PASSWORD_COMPLEXITY", "false");

        try
        {
            Assert.True(PasswordResetRules.MeetsPasswordPolicy("alllowercasepassword"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("AUTH_REQUIRE_PASSWORD_COMPLEXITY", previous);
        }
    }
}
