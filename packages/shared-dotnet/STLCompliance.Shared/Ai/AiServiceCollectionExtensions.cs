using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace STLCompliance.Shared.Ai;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddStlAiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AiProviderOptions>(options =>
        {
            var section = configuration.GetSection(AiProviderOptions.SectionName);
            section.Bind(options);

            options.ApiKey = FirstNonEmpty(
                configuration["OPENAI_API_KEY"],
                configuration["OpenAI:ApiKey"],
                options.ApiKey);
            options.AssistantModel = FirstNonEmpty(
                configuration["OPENAI_ASSISTANT_MODEL"],
                configuration["OpenAI:AssistantModel"],
                options.AssistantModel);
            options.SmartImportModel = FirstNonEmpty(
                configuration["OPENAI_SMART_IMPORT_MODEL"],
                configuration["OpenAI:SmartImportModel"],
                options.SmartImportModel);
            options.RedisUrl = FirstNonEmpty(
                configuration["REDIS_URL"],
                configuration["OpenAI:RedisUrl"],
                options.RedisUrl);
            options.ResponsesEndpoint = FirstNonEmpty(
                configuration["OPENAI_RESPONSES_ENDPOINT"],
                configuration["OpenAI:ResponsesEndpoint"],
                options.ResponsesEndpoint);
            options.TimeoutSeconds = ReadInt(configuration, "OPENAI_TIMEOUT_SECONDS", options.TimeoutSeconds);
            options.RetryAttempts = ReadInt(configuration, "OPENAI_RETRY_ATTEMPTS", options.RetryAttempts);
            options.RequestsPerMinute = ReadInt(configuration, "OPENAI_REQUESTS_PER_MINUTE", options.RequestsPerMinute);
            options.TokensPerMinute = ReadInt(configuration, "OPENAI_TOKENS_PER_MINUTE", options.TokensPerMinute);
            options.MaxOutputTokens = ReadInt(configuration, "OPENAI_MAX_OUTPUT_TOKENS", options.MaxOutputTokens);
        });

        services.AddSingleton<IAiTokenEstimator, HeuristicAiTokenEstimator>();
        services.AddSingleton<IAiRateLimiter, RedisAiRateLimiter>();
        services.AddSingleton<IAiRedactionService, DefaultAiRedactionService>();
        services.AddSingleton<IAiPromptRenderer, DefaultAiPromptRenderer>();
        services.AddSingleton<IAiPolicyEngine, DefaultAiPolicyEngine>();
        services.AddSingleton<IAiResponseValidator, DefaultAiResponseValidator>();
        services.AddHttpClient<IAiProvider, OpenAiResponsesProvider>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AiProviderOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 300));
        });

        return services;
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static int ReadInt(IConfiguration configuration, string key, int fallback) =>
        int.TryParse(configuration[key], out var parsed) && parsed > 0 ? parsed : fallback;
}
