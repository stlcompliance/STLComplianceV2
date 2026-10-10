using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using STLCompliance.Shared.Workers;

namespace STLCompliance.Shared.Tests;

public sealed class WorkerHealthServerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("65536")]
    public async Task RejectsInvalidPorts(string? port)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["PORT"] = port }).Build();
        await using var server = new WorkerHealthServer(configuration);
        await Assert.ThrowsAsync<InvalidOperationException>(() => server.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ServesHealthAndStopsWithWorker()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["PORT"] = port.ToString() }).Build();
        await using var server = new WorkerHealthServer(configuration);
        using var client = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") };
        try
        {
            await server.StartAsync(CancellationToken.None);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/work")).StatusCode);
        }
        finally
        {
            await server.StopAsync(CancellationToken.None);
        }
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync("/health"));
    }
}
