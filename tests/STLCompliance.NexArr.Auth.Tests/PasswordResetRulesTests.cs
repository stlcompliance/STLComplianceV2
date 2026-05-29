using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

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
}
