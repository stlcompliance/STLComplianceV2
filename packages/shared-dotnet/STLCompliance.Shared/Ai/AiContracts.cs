using System.Text.Json;

namespace STLCompliance.Shared.Ai;

public static class AiProviderOutcomes
{
    public const string Success = "success";
    public const string Timeout = "timeout";
    public const string ProviderRateLimited = "provider_rate_limited";
    public const string ProviderUnavailable = "provider_unavailable";
    public const string InvalidResponse = "invalid_response";
    public const string MalformedStructuredOutput = "malformed_structured_output";
    public const string ExtractionFailure = "extraction_failure";
    public const string TokenBudgetExceeded = "token_budget_exceeded";
    public const string SafetyRefusal = "safety_refusal";
    public const string MissingConfig = "missing_config";
}

public static class AiRequestCategories
{
    public const string Guidance = "guidance";
    public const string Troubleshooting = "troubleshooting";
    public const string Explanation = "explanation";
    public const string Summarization = "summarization";
    public const string Drafting = "drafting";
    public const string RecordLookup = "record_lookup";
    public const string ActionRecommendation = "action_recommendation";
    public const string ActionExecution = "action_execution";
    public const string ComplianceInterpretation = "compliance_interpretation";
    public const string AdminDiagnostic = "admin_diagnostic";
    public const string SecuritySensitive = "security_sensitive";
    public const string PromptExtractionAttempt = "prompt_extraction_attempt";
    public const string UnauthorizedAccessAttempt = "unauthorized_access_attempt";
    public const string BypassOrWorkaroundAttempt = "bypass_or_workaround_attempt";
    public const string DestructiveAction = "destructive_action";
    public const string SmartImportClassification = "smart_import_classification";
    public const string SmartImportExtraction = "smart_import_extraction";
    public const string SmartImportMapping = "smart_import_mapping";
    public const string SmartImportReviewHelp = "smart_import_review_help";
}

public sealed class AiProviderOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;

    public string AssistantModel { get; set; } = "gpt-5.5";

    public string SmartImportModel { get; set; } = "gpt-5.5";

    public string AssistantVectorStoreIds { get; set; } = string.Empty;

    public int AssistantFileSearchMaxResults { get; set; } = 6;

    public string ResponsesEndpoint { get; set; } = "https://api.openai.com/v1/responses";

    public int RequestsPerMinute { get; set; } = 500;

    public int TokensPerMinute { get; set; } = 500_000;

    public int TimeoutSeconds { get; set; } = 120;

    public int RetryAttempts { get; set; } = 5;

    public int MaxOutputTokens { get; set; } = 1_200;

    public string RedisUrl { get; set; } = string.Empty;

    public string RateLimitKeyPrefix { get; set; } = "stl:ai:openai";
}

public sealed record AiContextPacket(
    Guid TenantId,
    Guid ActorPersonId,
    string Surface,
    string ProductKey,
    string Route,
    string Category,
    IReadOnlyList<string> Permissions,
    IReadOnlyDictionary<string, object?> PageContext,
    IReadOnlyList<AiSelectedRecordSummary> SelectedRecords,
    IReadOnlyList<AiValidationErrorSummary> ValidationErrors,
    string WorkflowState,
    IReadOnlyList<string> RedactionLabels,
    IReadOnlyList<string> AllowedBehaviors);

public sealed record AiSelectedRecordSummary(
    string SourceProduct,
    string ResourceType,
    string ResourceId,
    string DisplayName,
    string Freshness,
    IReadOnlyDictionary<string, object?> Fields);

public sealed record AiValidationErrorSummary(
    string Code,
    string Message,
    string? Field,
    string Severity,
    string SourceProduct);

public sealed record AiProviderRequest(
    string Purpose,
    string Category,
    string Model,
    string Instructions,
    string Input,
    string? JsonSchemaName,
    string? JsonSchema,
    int MaxOutputTokens,
    string CorrelationId,
    string RateLimitScope,
    int? EstimatedInputTokens = null,
    string ReasoningEffort = "low");

public sealed record AiProviderResult(
    string Outcome,
    string? OutputText,
    string? ProviderResponseId,
    string? ProviderRequestId,
    int? InputTokens,
    int? OutputTokens,
    int? TotalTokens,
    string? ErrorCode,
    string? SafeMessage,
    DateTimeOffset CompletedAt);

public sealed record AiRateLimitRequest(
    string Provider,
    string Scope,
    int EstimatedTokens,
    string CorrelationId);

public sealed record AiRateLimitLease(
    bool Allowed,
    int RemainingRequests,
    int RemainingTokens,
    TimeSpan RetryAfter);

public sealed record AiPolicyEvaluation(
    bool Allowed,
    string Category,
    string? RefusalCode,
    string? SafeMessage,
    bool RequiresConfirmation,
    IReadOnlyList<string> RequiredPermissions,
    IReadOnlyList<string> RequiredReviewReasons);

public sealed record AiStructuredValidationResult(
    bool Valid,
    JsonDocument? Json,
    string? ErrorCode,
    string? SafeMessage);

public interface IAiProvider
{
    Task<AiProviderResult> CompleteAsync(AiProviderRequest request, CancellationToken cancellationToken = default);
}

public interface IAiRateLimiter
{
    Task<AiRateLimitLease> ReserveAsync(AiRateLimitRequest request, CancellationToken cancellationToken = default);
}

public interface IAiPromptRenderer
{
    string RenderInstructions(string category, string productKey, IReadOnlyList<string> allowedBehaviors);
}

public interface IAiPolicyEngine
{
    AiPolicyEvaluation Evaluate(string category, string userInput, AiContextPacket context);
}

public interface IAiContextBuilder
{
    AiContextPacket Build(Guid tenantId, Guid actorPersonId, string surface, string productKey, string route, string category);
}

public interface IAiResponseValidator
{
    AiStructuredValidationResult ValidateJsonObject(string? outputText, IReadOnlySet<string> allowedProductKeys);
}

public interface IAiAuditLogger
{
    Task WriteAsync(string action, Guid tenantId, Guid actorPersonId, string targetId, string result, string? reasonCode, CancellationToken cancellationToken = default);
}

public interface IAiActionPlanner
{
    object BuildPreview(string category, JsonDocument structuredResponse);
}

public interface IAiToolExecutor
{
    Task<object> ExecuteConfirmedAsync(string proposalId, CancellationToken cancellationToken = default);
}

public interface IAiRedactionService
{
    string Redact(string value);
}

public interface IAiTokenEstimator
{
    int EstimateTokens(params string?[] values);
}
