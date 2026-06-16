using System.Collections;

namespace STLCompliance.Shared.Integration;

public static class StlIntegrationEventActorTypes
{
    public const string Person = "person";
    public const string Service = "service";
    public const string PortalCustomer = "portalCustomer";
    public const string Vendor = "vendor";
    public const string System = "system";
    public const string Integration = "integration";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Person,
        Service,
        PortalCustomer,
        Vendor,
        System,
        Integration,
    };
}

public sealed record StlEventTraceMetadata(
    string? TraceId,
    string? SpanId,
    IReadOnlyDictionary<string, string>? Baggage = null);

public sealed record StlProductObjectReference(
    string ProductKey,
    string ObjectType,
    string ObjectId,
    string? ObjectNumber = null);

public sealed record StlIntegrationEventEnvelope(
    Guid EventId,
    string EventType,
    string ProductKey,
    Guid TenantId,
    string AggregateType,
    string AggregateId,
    long? AggregateVersion,
    DateTimeOffset OccurredAt,
    string? ActorPersonId,
    string ActorType,
    string SourceProductKey,
    Guid CorrelationId,
    Guid? CausationId,
    string IdempotencyKey,
    string SchemaVersion,
    IReadOnlyDictionary<string, object?> Payload,
    string? VisibilityClassification = null,
    StlEventTraceMetadata? Trace = null);

public sealed record StlOutboxEnvelopeSnapshot(
    Guid OutboxId,
    Guid TenantId,
    StlIntegrationEventEnvelope Envelope,
    string ProcessingStatus,
    int AttemptCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? NextRetryAt,
    DateTimeOffset? PublishedAt,
    string? LastError);

public sealed record StlInboxProcessingResult(
    Guid TenantId,
    Guid SourceEventId,
    string SourceProductKey,
    string EventType,
    string IdempotencyKey,
    bool IsDuplicate,
    string Outcome,
    DateTimeOffset ProcessedAt,
    string? Message = null);

public static class StlIntegrationEventEnvelopeRules
{
    private static readonly string[] SensitiveFieldFragments =
    [
        "secret",
        "password",
        "token",
        "accesstoken",
        "servicetoken",
        "refreshtoken",
        "hiddenprompt",
        "apikey",
        "api_key",
    ];

    public static IReadOnlyList<string> Validate(StlIntegrationEventEnvelope envelope)
    {
        var errors = new List<string>();

        if (envelope.EventId == Guid.Empty)
        {
            errors.Add("eventId is required.");
        }

        if (string.IsNullOrWhiteSpace(envelope.EventType))
        {
            errors.Add("eventType is required.");
        }

        if (!StlProductKeys.IsCanonical(envelope.ProductKey))
        {
            errors.Add("productKey must be a canonical lowercase product key.");
        }

        if (!EventTypePrefixMatchesProductKey(envelope.EventType, envelope.ProductKey))
        {
            errors.Add("eventType prefix must match productKey.");
        }

        if (envelope.TenantId == Guid.Empty)
        {
            errors.Add("tenantId is required.");
        }

        if (string.IsNullOrWhiteSpace(envelope.AggregateType))
        {
            errors.Add("aggregateType is required.");
        }

        if (string.IsNullOrWhiteSpace(envelope.AggregateId))
        {
            errors.Add("aggregateId is required.");
        }

        if (envelope.OccurredAt == default)
        {
            errors.Add("occurredAt is required.");
        }

        if (!StlIntegrationEventActorTypes.All.Contains(envelope.ActorType))
        {
            errors.Add("actorType is invalid.");
        }

        if (!StlProductKeys.IsCanonical(envelope.SourceProductKey))
        {
            errors.Add("sourceProductKey must be a canonical lowercase product key.");
        }

        if (envelope.CorrelationId == Guid.Empty)
        {
            errors.Add("correlationId is required.");
        }

        if (string.IsNullOrWhiteSpace(envelope.IdempotencyKey))
        {
            errors.Add("idempotencyKey is required.");
        }

        if (string.IsNullOrWhiteSpace(envelope.SchemaVersion))
        {
            errors.Add("schemaVersion is required.");
        }

        if (envelope.Payload is null)
        {
            errors.Add("payload is required.");
        }
        else
        {
            var sensitivePath = FindSensitivePayloadPath(envelope.Payload);
            if (sensitivePath is not null)
            {
                errors.Add($"payload must not contain sensitive field '{sensitivePath}'.");
            }
        }

        return errors;
    }

    public static bool EventTypePrefixMatchesProductKey(string eventType, string productKey) =>
        eventType.StartsWith($"{productKey}.", StringComparison.Ordinal);

    public static string BuildIdempotencyKey(
        string productKey,
        string operation,
        Guid tenantId,
        string aggregateType,
        string aggregateId,
        string? discriminator = null)
    {
        var suffix = string.IsNullOrWhiteSpace(discriminator) ? string.Empty : $":{discriminator.Trim()}";
        return $"{productKey}:{operation}:{tenantId:D}:{aggregateType}:{aggregateId}{suffix}".ToLowerInvariant();
    }

    private static string? FindSensitivePayloadPath(object value, string path = "payload")
    {
        if (value is IReadOnlyDictionary<string, object?> readOnlyDictionary)
        {
            foreach (var (key, child) in readOnlyDictionary)
            {
                var currentPath = $"{path}.{key}";
                if (IsSensitiveFieldName(key))
                {
                    return currentPath;
                }

                if (child is not null)
                {
                    var nested = FindSensitivePayloadPath(child, currentPath);
                    if (nested is not null)
                    {
                        return nested;
                    }
                }
            }
        }
        else if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                var key = entry.Key?.ToString() ?? string.Empty;
                var currentPath = $"{path}.{key}";
                if (IsSensitiveFieldName(key))
                {
                    return currentPath;
                }

                if (entry.Value is not null)
                {
                    var nested = FindSensitivePayloadPath(entry.Value, currentPath);
                    if (nested is not null)
                    {
                        return nested;
                    }
                }
            }
        }
        else if (value is IEnumerable enumerable && value is not string)
        {
            var index = 0;
            foreach (var child in enumerable)
            {
                if (child is not null)
                {
                    var nested = FindSensitivePayloadPath(child, $"{path}[{index}]");
                    if (nested is not null)
                    {
                        return nested;
                    }
                }

                index++;
            }
        }

        return null;
    }

    private static bool IsSensitiveFieldName(string fieldName)
    {
        var normalized = fieldName.Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return SensitiveFieldFragments.Any(fragment =>
            normalized.Contains(fragment.Replace("_", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal));
    }
}
