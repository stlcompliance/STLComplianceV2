using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace STLCompliance.OpenApi.Tests.Support;

internal interface IOpenApiHost : IAsyncDisposable
{
    HttpClient Client { get; }
}

internal sealed class OpenApiTestHost<TProgram> : IOpenApiHost
    where TProgram : class
{
    private readonly WebApplicationFactory<TProgram> _factory;

    public OpenApiTestHost()
    {
        _factory = new WebApplicationFactory<TProgram>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
        });
    }

    public HttpClient Client => _factory.CreateClient();

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();
}
