using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Ai;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class AiAssistanceService(
    NexArrDbContext db,
    IAiProvider provider,
    IAiPromptRenderer promptRenderer,
    IAiPolicyEngine policyEngine,
    IAiRedactionService redactionService,
    IOptions<AiProviderOptions> options,
    ICorrelationIdAccessor correlationIdAccessor)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiAssistantMessageResponse> SendMessageAsync(
        ClaimsPrincipal principal,
        AiAssistantMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = ResolveTenantId(principal, request.TenantId);
        var actorPersonId = principal.GetPersonId();
        var category = string.IsNullOrWhiteSpace(request.Category) ? AiRequestCategories.Guidance : request.Category.Trim();
        var productKey = NormalizeProduct(request.ProductKey);

        RequireAiAccess(principal, tenantId, productKey);

        var contextPacket = BuildContextPacket(
            tenantId,
            actorPersonId,
            request.Surface,
            productKey,
            request.Route,
            category,
            request.PageContext);
        var policy = policyEngine.Evaluate(category, request.Message, contextPacket);

        var now = DateTimeOffset.UtcNow;
        var session = request.SessionId is Guid sessionId
            ? await db.AiSessions.FirstOrDefaultAsync(x => x.Id == sessionId && x.TenantId == tenantId, cancellationToken)
            : null;
        if (session is null)
        {
            session = new AiSession
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ActorPersonId = actorPersonId,
                ProductKey = productKey,
                Surface = request.Surface.TrimOrDefault("global"),
                Route = request.Route.TrimOrDefault("/"),
                Category = category,
                Title = BuildTitle(request.Message),
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now
            };
            db.AiSessions.Add(session);
        }
        else
        {
            session.UpdatedAt = now;
        }

        var redactedInput = redactionService.Redact(request.Message);
        if (!policy.Allowed)
        {
            var refused = new AiMessage
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                TenantId = tenantId,
                ActorPersonId = actorPersonId,
                Role = "assistant",
                Category = policy.Category,
                UserInputRedacted = redactedInput,
                OutputRedacted = policy.SafeMessage ?? "I cannot help with that request.",
                ContextJson = JsonSerializer.Serialize(contextPacket, JsonOptions),
                Outcome = AiProviderOutcomes.SafetyRefusal,
                ErrorCode = policy.RefusalCode,
                SafeMessage = policy.SafeMessage,
                CreatedAt = now
            };
            db.AiMessages.Add(refused);
            db.AiAuditEvents.Add(Audit(tenantId, actorPersonId, "ai.policy_refusal", "ai_session", session.Id.ToString(), "blocked", policy.RefusalCode));
            await db.SaveChangesAsync(cancellationToken);
            return new AiAssistantMessageResponse(
                session.Id,
                refused.Id,
                refused.Outcome,
                refused.OutputRedacted,
                refused.ErrorCode,
                refused.SafeMessage,
                policy.RequiredReviewReasons);
        }

        var providerResult = await provider.CompleteAsync(
            new AiProviderRequest(
                Purpose: "assistant",
                Category: category,
                Model: options.Value.AssistantModel,
                Instructions: promptRenderer.RenderInstructions(category, productKey, request.AllowedBehaviors ?? []),
                Input: BuildProviderInput(request.Message, contextPacket),
                JsonSchemaName: "stl_ai_assistant_response",
                JsonSchema: AssistantResponseSchema,
                MaxOutputTokens: options.Value.MaxOutputTokens,
                CorrelationId: correlationIdAccessor.CorrelationId.ToString("N"),
                RateLimitScope: "assistant"),
            cancellationToken);

        var assistantPayload = ExtractAssistantPayload(providerResult.OutputText);
        var answer = assistantPayload.Answer
            ?? providerResult.SafeMessage
            ?? "AI assistance is temporarily unavailable. Please try again later.";
        answer = AppendCitations(answer, assistantPayload.Citations);
        if (providerResult.Outcome == AiProviderOutcomes.MissingConfig)
        {
            answer = "AI assistance is not configured. Please contact your administrator.";
        }

        var message = new AiMessage
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            TenantId = tenantId,
            ActorPersonId = actorPersonId,
            Role = "assistant",
            Category = category,
            UserInputRedacted = redactedInput,
            OutputRedacted = redactionService.Redact(answer),
            ContextJson = JsonSerializer.Serialize(contextPacket, JsonOptions),
            Outcome = providerResult.Outcome,
            ProviderResponseId = providerResult.ProviderResponseId,
            ProviderRequestId = providerResult.ProviderRequestId,
            InputTokens = providerResult.InputTokens,
            OutputTokens = providerResult.OutputTokens,
            TotalTokens = providerResult.TotalTokens,
            ErrorCode = providerResult.ErrorCode,
            SafeMessage = providerResult.SafeMessage,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.AiMessages.Add(message);
        db.AiAuditEvents.Add(Audit(tenantId, actorPersonId, "ai.assistant_message", "ai_session", session.Id.ToString(), providerResult.Outcome, providerResult.ErrorCode));
        await db.SaveChangesAsync(cancellationToken);

        return new AiAssistantMessageResponse(
            session.Id,
            message.Id,
            providerResult.Outcome,
            message.OutputRedacted,
            providerResult.ErrorCode,
            providerResult.SafeMessage,
            policy.RequiredReviewReasons);
    }

    public AiAdminDiagnosticResponse BuildAdminDiagnostic()
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return new AiAdminDiagnosticResponse(
                "missing_config",
                false,
                settings.AssistantModel,
                "OPENAI_API_KEY is missing from the backend environment configuration.");
        }

        return new AiAdminDiagnosticResponse(
            "configured",
            true,
            settings.AssistantModel,
            "OpenAI provider configuration is present in the backend environment.");
    }

    public async Task<AiActionPreviewResponse> PreviewActionAsync(
        ClaimsPrincipal principal,
        AiActionPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await db.AiSessions.FirstOrDefaultAsync(
            x => x.Id == request.SessionId,
            cancellationToken)
            ?? throw new StlApiException("ai.session_not_found", "AI session was not found.", 404);

        RequireAiAccess(principal, session.TenantId, request.ProductKey);

        var proposal = new AiActionProposal
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            TenantId = session.TenantId,
            ActorPersonId = principal.GetPersonId(),
            ProductKey = NormalizeProduct(request.ProductKey),
            ActionCategory = request.ActionCategory.TrimOrDefault("action_recommendation"),
            Status = "preview",
            ProposalJson = request.Proposal.GetRawText(),
            RequiredPermissionsJson = JsonSerializer.Serialize(new[] { "human_confirmation_required" }, JsonOptions),
            ReviewReasonsJson = JsonSerializer.Serialize(new[] { "human_confirmation_required" }, JsonOptions),
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.AiActionProposals.Add(proposal);
        db.AiAuditEvents.Add(Audit(session.TenantId, principal.GetPersonId(), "ai.action_preview", "ai_action_proposal", proposal.Id.ToString(), "preview", null));
        await db.SaveChangesAsync(cancellationToken);

        return new AiActionPreviewResponse(
            proposal.Id,
            proposal.Status,
            ["human_confirmation_required"],
            ["human_confirmation_required"],
            request.Proposal);
    }

    private static AiContextPacket BuildContextPacket(
        Guid tenantId,
        Guid actorPersonId,
        string surface,
        string productKey,
        string route,
        string category,
        JsonElement? pageContext)
    {
        var pageContextDictionary = pageContext is { ValueKind: JsonValueKind.Object }
            ? JsonSerializer.Deserialize<Dictionary<string, object?>>(pageContext.Value.GetRawText(), JsonOptions) ?? []
            : [];

        return new AiContextPacket(
            tenantId,
            actorPersonId,
            surface.TrimOrDefault("global"),
            productKey,
            route.TrimOrDefault("/"),
            category,
            ["platform.ai.assistant.use"],
            pageContextDictionary,
            [],
            [],
            "unknown",
            ["secrets", "tokens", "raw_prompts"],
            ["explain", "summarize", "troubleshoot", "recommend", "prepare_review_only_actions"]);
    }

    private static string BuildProviderInput(string userMessage, AiContextPacket contextPacket) =>
        JsonSerializer.Serialize(new
        {
            userMessage,
            context = contextPacket,
            dataTrust = new
            {
                userMessage = "untrusted",
                pageContext = "permission_checked_summary"
            }
        }, JsonOptions);

    private static AssistantPayload ExtractAssistantPayload(string? outputText)
    {
        if (string.IsNullOrWhiteSpace(outputText))
        {
            return new AssistantPayload(null, []);
        }

        try
        {
            using var document = JsonDocument.Parse(outputText);
            var citations = new List<string>();
            if (document.RootElement.TryGetProperty("answer", out var answer) && answer.ValueKind == JsonValueKind.String)
            {
                if (document.RootElement.TryGetProperty("citations", out var citationArray)
                    && citationArray.ValueKind == JsonValueKind.Array)
                {
                    citations.AddRange(citationArray.EnumerateArray()
                        .Where(citation => citation.ValueKind == JsonValueKind.String)
                        .Select(citation => citation.GetString())
                        .Where(citation => !string.IsNullOrWhiteSpace(citation))
                        .Select(citation => citation!.Trim()));
                }

                return new AssistantPayload(answer.GetString(), citations.Distinct(StringComparer.OrdinalIgnoreCase).ToList());
            }
        }
        catch (JsonException)
        {
            return new AssistantPayload(outputText, []);
        }

        return new AssistantPayload(outputText, []);
    }

    private static string AppendCitations(string answer, IReadOnlyList<string> citations)
    {
        if (citations.Count == 0 || answer.Contains("References:", StringComparison.OrdinalIgnoreCase))
        {
            return answer;
        }

        return $"{answer.TrimEnd()}\n\nReferences: {string.Join(", ", citations)}";
    }

    private static Guid ResolveTenantId(ClaimsPrincipal principal, Guid? requestedTenantId)
    {
        if (requestedTenantId is Guid explicitTenantId)
        {
            if (!principal.IsPlatformAdmin() && principal.GetTenantId() != explicitTenantId)
            {
                throw new StlApiException("auth.tenant_forbidden", "Access to the requested tenant is forbidden.", 403);
            }

            return explicitTenantId;
        }

        return principal.GetTenantId();
    }

    private static void RequireAiAccess(ClaimsPrincipal principal, Guid tenantId, string productKey)
    {
        if (!principal.IsPlatformAdmin() && principal.GetTenantId() != tenantId)
        {
            throw new StlApiException("auth.tenant_forbidden", "Access to the requested tenant is forbidden.", 403);
        }

        if (!principal.IsPlatformAdmin() && !principal.HasProductEntitlement(productKey) && !principal.HasProductEntitlement("nexarr"))
        {
            throw new StlApiException("auth.forbidden", "Product entitlement is required for AI assistance on this surface.", 403);
        }
    }

    private static AiAuditEvent Audit(
        Guid tenantId,
        Guid actorPersonId,
        string eventType,
        string targetType,
        string targetId,
        string result,
        string? reasonCode) =>
        new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ActorPersonId = actorPersonId,
            EventType = eventType,
            TargetType = targetType,
            TargetId = targetId,
            Result = result,
            ReasonCode = reasonCode,
            MetadataJson = "{}",
            OccurredAt = DateTimeOffset.UtcNow
        };

    private static string BuildTitle(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length <= 80)
        {
            return trimmed.Length == 0 ? "AI assistance" : trimmed;
        }

        return trimmed[..80];
    }

    private static string NormalizeProduct(string value) =>
        string.IsNullOrWhiteSpace(value) ? "nexarr" : value.Trim().ToLowerInvariant();

    private sealed record AssistantPayload(string? Answer, IReadOnlyList<string> Citations);

    private const string AssistantResponseSchema = """
    {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "answer": { "type": "string" },
        "citations": {
          "type": "array",
          "items": { "type": "string" }
        },
        "requiredReviewReasons": {
          "type": "array",
          "items": { "type": "string" }
        }
      },
      "required": ["answer", "citations", "requiredReviewReasons"]
    }
    """;
}

internal static class NexArrAiStringExtensions
{
    public static string TrimOrDefault(this string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
