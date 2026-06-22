using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NexArr.Api.Options;
using NexArr.Api.Services;
using STLCompliance.Shared.Health;
using STLCompliance.Shared.Operations;

namespace STLCompliance.NexArr.Auth.Tests;

public class PlatformHealthServiceTests
{
    [Fact]
    public async Task Aggregate_is_healthy_when_all_configured_products_report_healthy()
    {
        var service = CreateService(new AggregateHealthStubHandler(_ => HealthyResponse("staffarr")));

        var report = await service.GetAggregateHealthAsync();

        Assert.Equal("Healthy", report.Status);
        Assert.Equal(11, report.Products.Count);
        Assert.All(report.Products, probe => Assert.Equal("Healthy", probe.Status));
    }

    [Fact]
    public async Task Aggregate_is_degraded_when_some_products_are_unreachable()
    {
        var service = CreateService(new AggregateHealthStubHandler(request =>
        {
            if (request.RequestUri?.Host.Contains("staffarr", StringComparison.Ordinal) == true)
            {
                return HealthyResponse("staffarr");
            }

            throw new HttpRequestException("network down");
        }));

        var report = await service.GetAggregateHealthAsync();

        Assert.Equal("Degraded", report.Status);
        Assert.Contains(report.Products, p => p.ProductKey == "staffarr" && p.Status == "Healthy");
        Assert.Contains(report.Products, p => p.Status == "Unreachable");
    }

    [Fact]
    public async Task Aggregate_is_unhealthy_when_all_configured_products_fail()
    {
        var service = CreateService(new AggregateHealthStubHandler(_ => throw new HttpRequestException("network down")));

        var report = await service.GetAggregateHealthAsync();

        Assert.Equal("Unhealthy", report.Status);
        Assert.All(report.Products, probe => Assert.Equal("Unreachable", probe.Status));
    }

    [Fact]
    public async Task Missing_base_url_marks_product_not_configured()
    {
        var options = new PlatformProductUrlsOptions
        {
            StaffArrBaseUrl = string.Empty,
            TrainArrBaseUrl = "http://stub.trainarr",
            MaintainArrBaseUrl = "http://stub.maintainarr",
            RoutArrBaseUrl = "http://stub.routarr",
            SupplyArrBaseUrl = "http://stub.supplyarr",
            ComplianceCoreBaseUrl = "http://stub.compliancecore",
        };

        var service = CreateService(new AggregateHealthStubHandler(_ => HealthyResponse("trainarr")), options);

        var report = await service.GetAggregateHealthAsync();
        var staffarr = report.Products.Single(p => p.ProductKey == "staffarr");

        Assert.Equal("NotConfigured", staffarr.Status);
        Assert.Equal("not_configured", staffarr.ErrorCode);
    }

    private static PlatformHealthService CreateService(
        HttpMessageHandler handler,
        PlatformProductUrlsOptions? options = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOptions<PlatformProductUrlsOptions>>(Options.Create(options ?? new PlatformProductUrlsOptions
        {
            StaffArrBaseUrl = "http://stub.staffarr",
            TrainArrBaseUrl = "http://stub.trainarr",
            MaintainArrBaseUrl = "http://stub.maintainarr",
            RoutArrBaseUrl = "http://stub.routarr",
            SupplyArrBaseUrl = "http://stub.supplyarr",
            ComplianceCoreBaseUrl = "http://stub.compliancecore",
            LoadArrBaseUrl = "http://stub.loadarr",
            AssurArrBaseUrl = "http://stub.assurarr",
            ReportArrBaseUrl = "http://stub.reportarr",
            RecordArrBaseUrl = "http://stub.recordarr",
        }));
        services.AddSingleton<IHttpClientFactory>(new StubHttpClientFactory(handler));

        var provider = services.BuildServiceProvider();
        return new PlatformHealthService(
            provider.GetRequiredService<IHttpClientFactory>(),
            provider.GetRequiredService<IOptions<PlatformProductUrlsOptions>>());
    }

    private static HttpResponseMessage HealthyResponse(string productKey) => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(new HealthResponse(
            "Healthy",
            productKey,
            "1.0.0",
            DateTimeOffset.UtcNow)),
    };

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class AggregateHealthStubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }
}
