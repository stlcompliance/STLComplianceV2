using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace STLCompliance.Shared.Workers;

public sealed class WorkerHealthServer(IConfiguration configuration) : IHostedService, IAsyncDisposable
{
    private WebApplication? application;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!int.TryParse(configuration["PORT"], out var port) || port is < 1 or > 65535)
        {
            throw new InvalidOperationException("PORT must be an integer between 1 and 65535.");
        }

        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [] });
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        application = builder.Build();
        application.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
        await application.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        application?.StopAsync(cancellationToken) ?? Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        if (application is not null)
        {
            await application.DisposeAsync();
        }
    }
}
