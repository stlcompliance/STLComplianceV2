using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrStartupValidationTests
{
    [Fact]
    public void Production_startup_requires_auth_signing_key()
    {
        var factory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("AUTH_SIGNING_KEY", string.Empty);
            builder.UseSetting("Auth:SigningKey", string.Empty);
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
        });

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
        Assert.Contains("AUTH_SIGNING_KEY must be configured", exception.Message, StringComparison.Ordinal);
    }
}
