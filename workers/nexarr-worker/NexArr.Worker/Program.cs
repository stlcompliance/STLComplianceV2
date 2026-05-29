using Microsoft.Extensions.Options;
using NexArr.Worker.Clients;
using NexArr.Worker.Jobs;
using NexArr.Worker.Options;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Http;
using STLCompliance.Shared.Workers;

await StlWorkerHost.RunAsync(
    new ProductDescriptor("nexarr-worker", "NexArr Worker", 0),
    args,
    builder =>
    {
        builder.Services.Configure<NexArrPlatformOutboxPublisherOptions>(
            builder.Configuration.GetSection(NexArrPlatformOutboxPublisherOptions.SectionName));

        builder.Services.AddHttpClient<NexArrPlatformOutboxPublisherClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NexArrPlatformOutboxPublisherOptions>>().Value;
            client.BaseAddress = new Uri(StlServiceUrl.NormalizeHttpBaseUrl(options.NexArrBaseUrl) + "/");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddHostedService<NexArrPlatformOutboxPublisherJob>();
    });
