using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace STLCompliance.Shared.Ai;

public sealed class OpenAiResponsesProvider(
    HttpClient httpClient,
    IOptions<AiProviderOptions> options,
    IAiRateLimiter rateLimiter,
    IAiTokenEstimator tokenEstimator) : IAiProvider
{
    public async Task<AiProviderResult> CompleteAsync(
        AiProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return MissingConfig();
        }

        var tokenBudget = (request.EstimatedInputTokens ?? tokenEstimator.EstimateTokens(request.Instructions, request.Input))
            + Math.Max(1, request.MaxOutputTokens)
            + 256;
        var lease = await rateLimiter.ReserveAsync(
            new AiRateLimitRequest("openai", request.RateLimitScope, tokenBudget, request.CorrelationId),
            cancellationToken);
        if (!lease.Allowed)
        {
            return new AiProviderResult(
                AiProviderOutcomes.ProviderRateLimited,
                null,
                null,
                null,
                null,
                null,
                null,
                "ai.provider_rate_limited",
                "AI assistance is temporarily rate limited. Please try again shortly.",
                DateTimeOffset.UtcNow);
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, settings.ResponsesEndpoint);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        httpRequest.Headers.TryAddWithoutValidation("X-Client-Request-Id", request.CorrelationId);
        httpRequest.Content = new StringContent(BuildRequestJson(request, settings), Encoding.UTF8, "application/json");

        try
        {
            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            var providerRequestId = response.Headers.TryGetValues("request-id", out var requestIds)
                ? requestIds.FirstOrDefault()
                : null;
            var text = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return FailureForStatus(response.StatusCode, providerRequestId);
            }

            return ParseSuccess(text, providerRequestId);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new AiProviderResult(
                AiProviderOutcomes.Timeout,
                null,
                null,
                null,
                null,
                null,
                null,
                "ai.timeout",
                "AI assistance timed out. Please try again.",
                DateTimeOffset.UtcNow);
        }
        catch (HttpRequestException)
        {
            return new AiProviderResult(
                AiProviderOutcomes.ProviderUnavailable,
                null,
                null,
                null,
                null,
                null,
                null,
                "ai.provider_unavailable",
                "AI assistance is temporarily unavailable. Please try again later.",
                DateTimeOffset.UtcNow);
        }
    }

    private static AiProviderResult MissingConfig() =>
        new(
            AiProviderOutcomes.MissingConfig,
            null,
            null,
            null,
            null,
            null,
            null,
            "ai.missing_config",
            "AI assistance is not configured. Please contact your administrator.",
            DateTimeOffset.UtcNow);

    private static AiProviderResult FailureForStatus(HttpStatusCode statusCode, string? providerRequestId)
    {
        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            return new AiProviderResult(
                AiProviderOutcomes.ProviderRateLimited,
                null,
                null,
                providerRequestId,
                null,
                null,
                null,
                "ai.provider_rate_limited",
                "AI assistance is temporarily rate limited. Please try again shortly.",
                DateTimeOffset.UtcNow);
        }

        return new AiProviderResult(
            AiProviderOutcomes.ProviderUnavailable,
            null,
            null,
            providerRequestId,
            null,
            null,
            null,
            "ai.provider_unavailable",
            "AI assistance is temporarily unavailable. Please try again later.",
            DateTimeOffset.UtcNow);
    }

    private static AiProviderResult ParseSuccess(string body, string? providerRequestId)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var responseId = root.TryGetProperty("id", out var id) ? id.GetString() : null;
            var outputText = ExtractOutputText(root);
            var refusal = ExtractRefusal(root);
            var usage = ExtractUsage(root);

            if (!string.IsNullOrWhiteSpace(refusal))
            {
                return new AiProviderResult(
                    AiProviderOutcomes.SafetyRefusal,
                    null,
                    responseId,
                    providerRequestId,
                    usage.InputTokens,
                    usage.OutputTokens,
                    usage.TotalTokens,
                    "ai.safety_refusal",
                    "AI assistance cannot help with that request.",
                    DateTimeOffset.UtcNow);
            }

            if (string.IsNullOrWhiteSpace(outputText))
            {
                return new AiProviderResult(
                    AiProviderOutcomes.InvalidResponse,
                    null,
                    responseId,
                    providerRequestId,
                    usage.InputTokens,
                    usage.OutputTokens,
                    usage.TotalTokens,
                    "ai.invalid_response",
                    "AI returned an invalid response.",
                    DateTimeOffset.UtcNow);
            }

            return new AiProviderResult(
                AiProviderOutcomes.Success,
                outputText,
                responseId,
                providerRequestId,
                usage.InputTokens,
                usage.OutputTokens,
                usage.TotalTokens,
                null,
                null,
                DateTimeOffset.UtcNow);
        }
        catch (JsonException)
        {
            return new AiProviderResult(
                AiProviderOutcomes.InvalidResponse,
                null,
                null,
                providerRequestId,
                null,
                null,
                null,
                "ai.invalid_response",
                "AI returned an invalid response.",
                DateTimeOffset.UtcNow);
        }
    }

    internal static string BuildRequestJson(AiProviderRequest request, AiProviderOptions settings)
    {
        var payload = new JsonObject
        {
            ["model"] = request.Model,
            ["instructions"] = request.Instructions,
            ["input"] = request.Input,
            ["max_output_tokens"] = Math.Max(1, request.MaxOutputTokens),
            ["reasoning"] = new JsonObject { ["effort"] = request.ReasoningEffort }
        };

        if (!string.IsNullOrWhiteSpace(request.JsonSchema) && !string.IsNullOrWhiteSpace(request.JsonSchemaName))
        {
            payload["text"] = new JsonObject
            {
                ["format"] = new JsonObject
                {
                    ["type"] = "json_schema",
                    ["name"] = request.JsonSchemaName,
                    ["strict"] = true,
                    ["schema"] = JsonNode.Parse(request.JsonSchema)
                }
            };
        }

        var vectorStoreIds = ParseVectorStoreIds(settings.AssistantVectorStoreIds);
        if (ShouldAttachAssistantFileSearch(request, vectorStoreIds))
        {
            var vectorStoreIdArray = new JsonArray();
            foreach (var vectorStoreId in vectorStoreIds)
            {
                vectorStoreIdArray.Add(vectorStoreId);
            }

            var fileSearchTool = new JsonObject
            {
                ["type"] = "file_search",
                ["vector_store_ids"] = vectorStoreIdArray,
                ["max_num_results"] = Math.Clamp(settings.AssistantFileSearchMaxResults, 1, 50)
            };
            payload["tools"] = new JsonArray(fileSearchTool);
        }

        return payload.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static IReadOnlyList<string> ParseVectorStoreIds(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split([',', ';', ' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

    private static bool ShouldAttachAssistantFileSearch(
        AiProviderRequest request,
        IReadOnlyList<string> vectorStoreIds) =>
        vectorStoreIds.Count > 0
        && string.Equals(request.Purpose, "assistant", StringComparison.OrdinalIgnoreCase);

    private static string? ExtractOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString();
        }

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                {
                    return text.GetString();
                }
            }
        }

        return null;
    }

    private static string? ExtractRefusal(JsonElement root)
    {
        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("refusal", out var refusal) && refusal.ValueKind == JsonValueKind.String)
                {
                    return refusal.GetString();
                }
            }
        }

        return null;
    }

    private static (int? InputTokens, int? OutputTokens, int? TotalTokens) ExtractUsage(JsonElement root)
    {
        if (!root.TryGetProperty("usage", out var usage) || usage.ValueKind != JsonValueKind.Object)
        {
            return (null, null, null);
        }

        var input = usage.TryGetProperty("input_tokens", out var inputTokens) && inputTokens.TryGetInt32(out var inputValue)
            ? inputValue
            : (int?)null;
        var output = usage.TryGetProperty("output_tokens", out var outputTokens) && outputTokens.TryGetInt32(out var outputValue)
            ? outputValue
            : (int?)null;
        var total = usage.TryGetProperty("total_tokens", out var totalTokens) && totalTokens.TryGetInt32(out var totalValue)
            ? totalValue
            : (int?)null;
        return (input, output, total);
    }
}
