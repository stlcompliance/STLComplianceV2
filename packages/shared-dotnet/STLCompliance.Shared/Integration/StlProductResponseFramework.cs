namespace STLCompliance.Shared.Integration;

public static class StlProductEventOperations
{
    public const string Create = "create";
    public const string Update = "update";
    public const string Delete = "delete";
    public const string Restore = "restore";
    public const string Import = "import";
    public const string StatusChange = "status_change";
    public const string RelationshipChange = "relationship_change";
    public const string WorkflowChange = "workflow_change";
    public const string SettingsChange = "settings_change";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Create,
        Update,
        Delete,
        Restore,
        Import,
        StatusChange,
        RelationshipChange,
        WorkflowChange,
        SettingsChange,
    };

    public static string Normalize(string operation) =>
        operation.Trim().ToLowerInvariant();

    public static bool IsKnown(string operation) =>
        All.Contains(Normalize(operation));
}

public sealed record StlProductResponseFrameworkContract(
    string ProductKey,
    string DisplayName,
    IReadOnlyList<string> RequiredEnvelopeFields,
    IReadOnlyList<string> OutboxStatuses,
    IReadOnlyList<string> InboxStatuses,
    IReadOnlyList<string> InterestDecisions,
    IReadOnlyList<string> ResponseResults,
    string DetailProjectionEndpointPattern,
    IReadOnlyList<string> OwnershipBoundaryRules);

public static class StlProductResponseFrameworkContracts
{
    public static readonly IReadOnlyList<string> RequiredEnvelopeFields =
    [
        "eventId",
        "tenantId",
        "sourceProductKey",
        "sourceEntityType",
        "sourceEntityId",
        "eventName",
        "operation",
        "eventVersion",
        "occurredAtUtc",
        "actorRef",
        "correlationId",
        "causationId",
        "idempotencyKey",
        "visibilityScope",
        "sensitivityLevel",
        "changedFieldsSummary",
        "relationshipRefs",
        "classificationTags",
        "detailProjectionUrl",
        "detailProjectionVersion",
    ];

    public static StlProductResponseFrameworkContract Describe(
        string productKey,
        string displayName) =>
        new(
            productKey,
            displayName,
            RequiredEnvelopeFields,
            StlProductOutboxStatuses.All.Order(StringComparer.Ordinal).ToList(),
            StlProductInboxStatuses.All.Order(StringComparer.Ordinal).ToList(),
            StlEventInterestDecisions.All.Order(StringComparer.Ordinal).ToList(),
            [
                StlEventResponseResults.Ignored,
                StlEventResponseResults.UpdatedLocalReadModel,
                StlEventResponseResults.UpdatedLocalSnapshot,
                StlEventResponseResults.CreatedLocalTask,
                StlEventResponseResults.CreatedLocalReviewItem,
                StlEventResponseResults.CreatedActionProposal,
                StlEventResponseResults.StartedOwnedWorkflow,
                StlEventResponseResults.RequestedMoreDetail,
                StlEventResponseResults.RequestedComplianceInterpretation,
                StlEventResponseResults.RequestedDocumentClassification,
                StlEventResponseResults.EmittedFollowUpEvent,
                StlEventResponseResults.CreatedHumanReviewItem,
                StlEventResponseResults.Failed,
            ],
            "/api/v1/integrations/event-projections/{sourceEntityType}/{sourceEntityId}",
            [
                "events_are_facts_not_commands",
                "no_cross_database_foreign_keys",
                "no_direct_mutation_of_another_product_source_truth",
                "details_require_authorized_projection_fetch",
                "responses_write_only_owned_records_read_models_tasks_proposals_or_workflows",
                "duplicates_must_not_create_duplicate_actions",
                "response_generated_events_preserve_correlation_and_causation",
            ]);
}

public static class StlEventVisibilityScopes
{
    public const string ProductPrivate = "product_private";
    public const string EntitledProducts = "entitled_products";
    public const string Tenant = "tenant";
    public const string PlatformAdmin = "platform_admin";
    public const string AuditOnly = "audit_only";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        ProductPrivate,
        EntitledProducts,
        Tenant,
        PlatformAdmin,
        AuditOnly,
    };

    public static bool IsKnown(string visibilityScope) =>
        All.Contains(visibilityScope.Trim().ToLowerInvariant());
}

public static class StlEventSensitivityLevels
{
    public const string Public = "public";
    public const string Internal = "internal";
    public const string Confidential = "confidential";
    public const string Restricted = "restricted";
    public const string Regulated = "regulated";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Public,
        Internal,
        Confidential,
        Restricted,
        Regulated,
    };

    public static bool IsKnown(string sensitivityLevel) =>
        All.Contains(sensitivityLevel.Trim().ToLowerInvariant());
}

public static class StlProductOutboxStatuses
{
    public const string Pending = "pending";
    public const string Published = "published";
    public const string Failed = "failed";
    public const string Superseded = "superseded";
    public const string DeadLettered = "dead-lettered";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Pending,
        Published,
        Failed,
        Superseded,
        DeadLettered,
    };

    public static bool IsTerminal(string status) =>
        string.Equals(status, Published, StringComparison.Ordinal)
        || string.Equals(status, Superseded, StringComparison.Ordinal)
        || string.Equals(status, DeadLettered, StringComparison.Ordinal);
}

public static class StlProductInboxStatuses
{
    public const string Ignored = "ignored";
    public const string PendingDetails = "pending_details";
    public const string Processed = "processed";
    public const string Failed = "failed";
    public const string WaitingForOwner = "waiting_for_owner";
    public const string DeadLettered = "dead-lettered";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Ignored,
        PendingDetails,
        Processed,
        Failed,
        WaitingForOwner,
        DeadLettered,
    };

    public static bool IsTerminal(string status) =>
        string.Equals(status, Ignored, StringComparison.Ordinal)
        || string.Equals(status, Processed, StringComparison.Ordinal)
        || string.Equals(status, DeadLettered, StringComparison.Ordinal);
}

public static class StlEventInterestDecisions
{
    public const string NoInterest = "NoInterest";
    public const string NeedsDetails = "NeedsDetails";
    public const string UpdateLocalReadModel = "UpdateLocalReadModel";
    public const string CreateLocalTask = "CreateLocalTask";
    public const string CreateActionProposal = "CreateActionProposal";
    public const string ExecuteOwnedWorkflow = "ExecuteOwnedWorkflow";
    public const string RequestMoreInformation = "RequestMoreInformation";
    public const string EmitFollowUpEvent = "EmitFollowUpEvent";
    public const string EscalateForHumanReview = "EscalateForHumanReview";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        NoInterest,
        NeedsDetails,
        UpdateLocalReadModel,
        CreateLocalTask,
        CreateActionProposal,
        ExecuteOwnedWorkflow,
        RequestMoreInformation,
        EmitFollowUpEvent,
        EscalateForHumanReview,
    };

    public static bool IsKnown(string decision) =>
        All.Contains(decision.Trim());

    public static bool RequiresDetailFetch(string decision) =>
        string.Equals(decision, NeedsDetails, StringComparison.Ordinal)
        || string.Equals(decision, RequestMoreInformation, StringComparison.Ordinal);
}

public static class StlEventResponseResults
{
    public const string Ignored = "ignored";
    public const string UpdatedLocalReadModel = "updated_local_read_model";
    public const string UpdatedLocalSnapshot = "updated_local_snapshot";
    public const string CreatedLocalTask = "created_local_task";
    public const string CreatedLocalReviewItem = "created_local_review_item";
    public const string CreatedActionProposal = "created_action_proposal";
    public const string StartedOwnedWorkflow = "started_owned_workflow";
    public const string RequestedMoreDetail = "requested_more_detail";
    public const string RequestedComplianceInterpretation = "requested_compliance_interpretation";
    public const string RequestedDocumentClassification = "requested_document_classification";
    public const string EmittedFollowUpEvent = "emitted_follow_up_event";
    public const string CreatedHumanReviewItem = "created_human_review_item";
    public const string Failed = "failed";
}

public sealed record StlEventActorRef(
    string ActorType,
    string? PersonId = null,
    string? ServiceClientId = null,
    string? PortalCustomerId = null,
    string? VendorId = null,
    string? IntegrationRef = null);

public sealed record StlChangedFieldSummary(
    string FieldName,
    string ChangeKind,
    bool IsSensitive = false);

public sealed record StlRelationshipReference(
    string ProductKey,
    string EntityType,
    string EntityId,
    string RelationshipType,
    string? DisplayLabelSnapshot = null,
    DateTimeOffset? SnapshotAtUtc = null);

public sealed record StlEventLoopMetadata(
    Guid OriginEventId,
    string OriginProductKey,
    int ResponseDepth,
    string? LastResponderProductKey = null);

public sealed record StlProductEventEnvelope(
    Guid EventId,
    Guid TenantId,
    string SourceProductKey,
    string SourceEntityType,
    string SourceEntityId,
    string EventName,
    string Operation,
    string EventVersion,
    DateTimeOffset OccurredAtUtc,
    StlEventActorRef ActorRef,
    Guid CorrelationId,
    Guid? CausationId,
    string IdempotencyKey,
    string VisibilityScope,
    string SensitivityLevel,
    IReadOnlyList<StlChangedFieldSummary> ChangedFieldsSummary,
    IReadOnlyList<StlRelationshipReference> RelationshipRefs,
    IReadOnlyList<string> ClassificationTags,
    string DetailProjectionUrl,
    string DetailProjectionVersion,
    StlEventLoopMetadata? Loop = null);

public static class StlProductEventEnvelopeAdapter
{
    public static StlProductEventEnvelope FromIntegrationEnvelope(
        StlIntegrationEventEnvelope envelope,
        string operation,
        string detailProjectionUrl,
        string detailProjectionVersion,
        IReadOnlyList<StlChangedFieldSummary>? changedFieldsSummary = null,
        IReadOnlyList<StlRelationshipReference>? relationshipRefs = null,
        IReadOnlyList<string>? classificationTags = null,
        string visibilityScope = StlEventVisibilityScopes.EntitledProducts,
        string sensitivityLevel = StlEventSensitivityLevels.Internal)
    {
        var tags = classificationTags;
        if (tags is null && !string.IsNullOrWhiteSpace(envelope.VisibilityClassification))
        {
            tags = [envelope.VisibilityClassification];
        }

        return new StlProductEventEnvelope(
            envelope.EventId,
            envelope.TenantId,
            envelope.SourceProductKey,
            envelope.AggregateType,
            envelope.AggregateId,
            envelope.EventType,
            StlProductEventOperations.Normalize(operation),
            envelope.SchemaVersion,
            envelope.OccurredAt.ToUniversalTime(),
            new StlEventActorRef(
                envelope.ActorType,
                PersonId: envelope.ActorPersonId),
            envelope.CorrelationId,
            envelope.CausationId,
            envelope.IdempotencyKey,
            visibilityScope,
            sensitivityLevel,
            changedFieldsSummary ?? [],
            relationshipRefs ??
            [
                new StlRelationshipReference(
                    envelope.SourceProductKey,
                    envelope.AggregateType,
                    envelope.AggregateId,
                    "source"),
            ],
            tags ?? [],
            detailProjectionUrl,
            detailProjectionVersion);
    }
}

public sealed record StlEventDetailProjectionRequest(
    Guid TenantId,
    string RequestingProductKey,
    string SourceProductKey,
    string SourceEntityType,
    string SourceEntityId,
    string Purpose,
    string ProjectionVersion,
    Guid CorrelationId,
    string? DelegatedActorPersonId = null);

public sealed record StlEventDetailProjectionResponse(
    Guid TenantId,
    string SourceProductKey,
    string SourceEntityType,
    string SourceEntityId,
    string ProjectionVersion,
    DateTimeOffset SourceUpdatedAtUtc,
    string SourceRecordStatus,
    IReadOnlyDictionary<string, object?> Detail,
    string Freshness,
    IReadOnlyList<string> OmittedFields,
    Guid CorrelationId);

public sealed record StlProductEventInboxSnapshot(
    Guid InboxId,
    Guid TenantId,
    Guid SourceEventId,
    string SourceProductKey,
    string EventName,
    string IdempotencyKey,
    string ProcessingStatus,
    string? InterestDecision,
    string? InterestReasonCode,
    string? ResponseResult,
    int RetryCount,
    string? LastError,
    DateTimeOffset ReceivedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? ProcessedAtUtc);

public sealed record StlProductEventOutboxSnapshot(
    Guid OutboxId,
    Guid TenantId,
    Guid EventId,
    string SourceProductKey,
    string EventName,
    string IdempotencyKey,
    string ProcessingStatus,
    int AttemptCount,
    string? LastError,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? NextRetryAtUtc,
    DateTimeOffset? PublishedAtUtc);

public sealed record StlProductEventAuditEntry(
    Guid TenantId,
    Guid EventId,
    string ConsumerProductKey,
    string Step,
    string Outcome,
    string? ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAtUtc);

public sealed record StlEventInterestContext(
    StlProductEventEnvelope Envelope,
    string ConsumerProductKey,
    bool ProductEntitled,
    bool ResponseBehaviorEnabled,
    IReadOnlySet<string> EnabledFeatureFlags,
    IReadOnlySet<string> ExistingLocalReferenceKeys,
    bool AllowOwnProductEvents = false);

public sealed record StlEventInterestRule(
    string ConsumerProductKey,
    string Decision,
    string ReasonCode,
    IReadOnlySet<string>? SourceProductKeys = null,
    IReadOnlySet<string>? SourceEntityTypes = null,
    IReadOnlySet<string>? Operations = null,
    IReadOnlySet<string>? EventNames = null,
    IReadOnlySet<string>? ChangedFieldNames = null,
    IReadOnlySet<string>? RelationshipProductKeys = null,
    IReadOnlySet<string>? ClassificationTags = null,
    string? DetailPurpose = null,
    IReadOnlyList<string>? AllowedResponseActions = null);

public sealed record StlEventInterestEvaluation(
    string Decision,
    string ReasonCode,
    bool ShouldFetchDetails,
    string InboxStatus,
    string? DetailPurpose,
    IReadOnlyList<string> AllowedResponseActions);

public sealed record StlEventResponseContext(
    StlProductEventEnvelope Envelope,
    string ConsumerProductKey,
    StlEventInterestEvaluation Interest,
    StlEventDetailProjectionResponse? Projection = null);

public sealed record StlEventResponseResult(
    string Result,
    string InboxStatus,
    string ReasonCode,
    string? Message,
    Guid? FollowUpEventId = null);

public interface IStlEventInterestEvaluator
{
    ValueTask<StlEventInterestEvaluation> EvaluateAsync(
        StlEventInterestContext context,
        CancellationToken cancellationToken = default);
}

public interface IStlEventResponder
{
    ValueTask<StlEventResponseResult> RespondAsync(
        StlEventResponseContext context,
        CancellationToken cancellationToken = default);
}

public interface IStlEventDetailProjectionClient
{
    ValueTask<StlEventDetailProjectionResponse> FetchAsync(
        StlEventDetailProjectionRequest request,
        CancellationToken cancellationToken = default);
}

public static class StlProductEventEnvelopeRules
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
        "ssn",
        "socialsecurity",
    ];

    public static IReadOnlyList<string> Validate(StlProductEventEnvelope envelope)
    {
        var errors = new List<string>();

        if (envelope.EventId == Guid.Empty)
        {
            errors.Add("eventId is required.");
        }

        if (envelope.TenantId == Guid.Empty)
        {
            errors.Add("tenantId is required.");
        }

        if (!StlProductKeys.IsCanonical(envelope.SourceProductKey))
        {
            errors.Add("sourceProductKey must be a canonical lowercase product key.");
        }

        if (string.IsNullOrWhiteSpace(envelope.SourceEntityType))
        {
            errors.Add("sourceEntityType is required.");
        }

        if (string.IsNullOrWhiteSpace(envelope.SourceEntityId))
        {
            errors.Add("sourceEntityId is required.");
        }

        if (string.IsNullOrWhiteSpace(envelope.EventName))
        {
            errors.Add("eventName is required.");
        }
        else if (!StlIntegrationEventEnvelopeRules.EventTypePrefixMatchesProductKey(
            envelope.EventName,
            envelope.SourceProductKey))
        {
            errors.Add("eventName prefix must match sourceProductKey.");
        }

        if (string.IsNullOrWhiteSpace(envelope.Operation) || !StlProductEventOperations.IsKnown(envelope.Operation))
        {
            errors.Add("operation must be a known product event operation.");
        }

        if (string.IsNullOrWhiteSpace(envelope.EventVersion))
        {
            errors.Add("eventVersion is required.");
        }

        if (envelope.OccurredAtUtc == default)
        {
            errors.Add("occurredAtUtc is required.");
        }
        else if (envelope.OccurredAtUtc.Offset != TimeSpan.Zero)
        {
            errors.Add("occurredAtUtc must use UTC offset.");
        }

        ValidateActor(envelope.ActorRef, errors);

        if (envelope.CorrelationId == Guid.Empty)
        {
            errors.Add("correlationId is required.");
        }

        if (string.IsNullOrWhiteSpace(envelope.IdempotencyKey))
        {
            errors.Add("idempotencyKey is required.");
        }

        if (string.IsNullOrWhiteSpace(envelope.VisibilityScope)
            || !StlEventVisibilityScopes.IsKnown(envelope.VisibilityScope))
        {
            errors.Add("visibilityScope must be known.");
        }

        if (string.IsNullOrWhiteSpace(envelope.SensitivityLevel)
            || !StlEventSensitivityLevels.IsKnown(envelope.SensitivityLevel))
        {
            errors.Add("sensitivityLevel must be known.");
        }

        ValidateChangedFields(envelope.ChangedFieldsSummary, errors);
        ValidateRelationshipRefs(envelope.RelationshipRefs, errors);
        ValidateClassificationTags(envelope.ClassificationTags, errors);

        if (string.IsNullOrWhiteSpace(envelope.DetailProjectionUrl))
        {
            errors.Add("detailProjectionUrl is required.");
        }
        else if (!IsSafeProjectionUrl(envelope.DetailProjectionUrl))
        {
            errors.Add("detailProjectionUrl must be a relative /api route or an HTTPS URL.");
        }

        if (string.IsNullOrWhiteSpace(envelope.DetailProjectionVersion))
        {
            errors.Add("detailProjectionVersion is required.");
        }

        if (envelope.Loop is not null)
        {
            ValidateLoop(envelope.Loop, errors);
        }

        return errors;
    }

    private static void ValidateActor(StlEventActorRef? actorRef, List<string> errors)
    {
        if (actorRef is null)
        {
            errors.Add("actorRef is required.");
            return;
        }

        if (!StlIntegrationEventActorTypes.All.Contains(actorRef.ActorType))
        {
            errors.Add("actorRef.actorType is invalid.");
        }

        if (string.Equals(actorRef.ActorType, StlIntegrationEventActorTypes.Person, StringComparison.Ordinal)
            && string.IsNullOrWhiteSpace(actorRef.PersonId))
        {
            errors.Add("actorRef.personId is required for person actors.");
        }
    }

    private static void ValidateChangedFields(
        IReadOnlyList<StlChangedFieldSummary>? changedFields,
        List<string> errors)
    {
        if (changedFields is null)
        {
            errors.Add("changedFieldsSummary is required.");
            return;
        }

        for (var i = 0; i < changedFields.Count; i++)
        {
            var field = changedFields[i];
            if (string.IsNullOrWhiteSpace(field.FieldName))
            {
                errors.Add($"changedFieldsSummary[{i}].fieldName is required.");
            }

            if (string.IsNullOrWhiteSpace(field.ChangeKind))
            {
                errors.Add($"changedFieldsSummary[{i}].changeKind is required.");
            }

            if (!field.IsSensitive && LooksSensitive(field.FieldName))
            {
                errors.Add($"changedFieldsSummary[{i}] must mark sensitive field '{field.FieldName}' as sensitive.");
            }
        }
    }

    private static void ValidateRelationshipRefs(
        IReadOnlyList<StlRelationshipReference>? relationshipRefs,
        List<string> errors)
    {
        if (relationshipRefs is null)
        {
            errors.Add("relationshipRefs is required.");
            return;
        }

        for (var i = 0; i < relationshipRefs.Count; i++)
        {
            var reference = relationshipRefs[i];
            if (!StlProductKeys.IsCanonical(reference.ProductKey))
            {
                errors.Add($"relationshipRefs[{i}].productKey must be canonical.");
            }

            if (string.IsNullOrWhiteSpace(reference.EntityType))
            {
                errors.Add($"relationshipRefs[{i}].entityType is required.");
            }

            if (string.IsNullOrWhiteSpace(reference.EntityId))
            {
                errors.Add($"relationshipRefs[{i}].entityId is required.");
            }

            if (string.IsNullOrWhiteSpace(reference.RelationshipType))
            {
                errors.Add($"relationshipRefs[{i}].relationshipType is required.");
            }
        }
    }

    private static void ValidateClassificationTags(
        IReadOnlyList<string>? classificationTags,
        List<string> errors)
    {
        if (classificationTags is null)
        {
            errors.Add("classificationTags is required.");
            return;
        }

        for (var i = 0; i < classificationTags.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(classificationTags[i]))
            {
                errors.Add($"classificationTags[{i}] must not be blank.");
            }
        }
    }

    private static void ValidateLoop(StlEventLoopMetadata loop, List<string> errors)
    {
        if (loop.OriginEventId == Guid.Empty)
        {
            errors.Add("loop.originEventId is required.");
        }

        if (!StlProductKeys.IsCanonical(loop.OriginProductKey))
        {
            errors.Add("loop.originProductKey must be canonical.");
        }

        if (loop.ResponseDepth < 0)
        {
            errors.Add("loop.responseDepth must not be negative.");
        }
    }

    private static bool IsSafeProjectionUrl(string url)
    {
        if (url.StartsWith("/api/", StringComparison.Ordinal))
        {
            return true;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal);
    }

    private static bool LooksSensitive(string fieldName)
    {
        var normalized = fieldName.Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return SensitiveFieldFragments.Any(fragment =>
            normalized.Contains(fragment.Replace("_", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal));
    }
}

public static class StlProductEventStatusRules
{
    public static string InboxStatusForInterestDecision(string decision)
    {
        if (string.Equals(decision, StlEventInterestDecisions.NoInterest, StringComparison.Ordinal))
        {
            return StlProductInboxStatuses.Ignored;
        }

        if (StlEventInterestDecisions.RequiresDetailFetch(decision))
        {
            return StlProductInboxStatuses.PendingDetails;
        }

        if (string.Equals(decision, StlEventInterestDecisions.EscalateForHumanReview, StringComparison.Ordinal))
        {
            return StlProductInboxStatuses.WaitingForOwner;
        }

        return StlProductInboxStatuses.Processed;
    }

    public static string OutboxFailureStatus(int attemptCount, int maxAttempts) =>
        attemptCount >= Math.Max(1, maxAttempts)
            ? StlProductOutboxStatuses.DeadLettered
            : StlProductOutboxStatuses.Failed;

    public static string InboxFailureStatus(int retryCount, int maxRetries) =>
        retryCount >= Math.Max(1, maxRetries)
            ? StlProductInboxStatuses.DeadLettered
            : StlProductInboxStatuses.Failed;
}

public static class StlProductEventDeduplication
{
    public static string BuildInboxDedupeKey(
        Guid tenantId,
        string consumerProductKey,
        string sourceProductKey,
        Guid eventId,
        string idempotencyKey)
    {
        return $"inbox:{tenantId:D}:{consumerProductKey}:{sourceProductKey}:{eventId:D}:{idempotencyKey}"
            .ToLowerInvariant();
    }

    public static string BuildResponseIdempotencyKey(
        string consumerProductKey,
        StlProductEventEnvelope envelope,
        string responseAction)
    {
        return $"{consumerProductKey}:{responseAction}:{envelope.TenantId:D}:{envelope.EventId:D}:{envelope.IdempotencyKey}"
            .ToLowerInvariant();
    }
}

public static class StlRuleBasedEventInterestEvaluator
{
    public static StlEventInterestEvaluation Evaluate(
        StlEventInterestContext context,
        IReadOnlyList<StlEventInterestRule> rules)
    {
        if (!context.ProductEntitled)
        {
            return NoInterest("product_not_entitled");
        }

        if (!context.ResponseBehaviorEnabled)
        {
            return NoInterest("response_behavior_disabled");
        }

        if (!context.AllowOwnProductEvents
            && string.Equals(context.Envelope.SourceProductKey, context.ConsumerProductKey, StringComparison.Ordinal))
        {
            return NoInterest("own_source_event");
        }

        foreach (var rule in rules)
        {
            if (Matches(context, rule))
            {
                return BuildEvaluation(rule.Decision, rule.ReasonCode, rule.DetailPurpose, rule.AllowedResponseActions);
            }
        }

        return NoInterest("no_matching_interest_rule");
    }

    private static bool Matches(StlEventInterestContext context, StlEventInterestRule rule)
    {
        var envelope = context.Envelope;
        return MatchesValue(rule.ConsumerProductKey, context.ConsumerProductKey)
            && MatchesAny(rule.SourceProductKeys, envelope.SourceProductKey)
            && MatchesAny(rule.SourceEntityTypes, envelope.SourceEntityType)
            && MatchesAny(rule.Operations, StlProductEventOperations.Normalize(envelope.Operation))
            && MatchesAny(rule.EventNames, envelope.EventName)
            && MatchesChangedField(rule.ChangedFieldNames, envelope.ChangedFieldsSummary)
            && MatchesRelationship(rule.RelationshipProductKeys, envelope.RelationshipRefs)
            && MatchesTag(rule.ClassificationTags, envelope.ClassificationTags);
    }

    private static StlEventInterestEvaluation BuildEvaluation(
        string decision,
        string reasonCode,
        string? detailPurpose,
        IReadOnlyList<string>? allowedResponseActions)
    {
        if (!StlEventInterestDecisions.IsKnown(decision))
        {
            return new StlEventInterestEvaluation(
                StlEventInterestDecisions.EscalateForHumanReview,
                "invalid_interest_rule_decision",
                ShouldFetchDetails: false,
                StlProductInboxStatuses.WaitingForOwner,
                DetailPurpose: null,
                AllowedResponseActions: [StlEventResponseResults.CreatedHumanReviewItem]);
        }

        return new StlEventInterestEvaluation(
            decision,
            reasonCode,
            StlEventInterestDecisions.RequiresDetailFetch(decision),
            StlProductEventStatusRules.InboxStatusForInterestDecision(decision),
            detailPurpose,
            allowedResponseActions ?? []);
    }

    private static StlEventInterestEvaluation NoInterest(string reasonCode) =>
        BuildEvaluation(
            StlEventInterestDecisions.NoInterest,
            reasonCode,
            detailPurpose: null,
            allowedResponseActions: [StlEventResponseResults.Ignored]);

    private static bool MatchesValue(string expected, string actual) =>
        string.Equals(expected, actual, StringComparison.Ordinal);

    private static bool MatchesAny(IReadOnlySet<string>? allowed, string value) =>
        allowed is null
        || allowed.Count == 0
        || allowed.Contains(value);

    private static bool MatchesChangedField(
        IReadOnlySet<string>? requiredFields,
        IReadOnlyList<StlChangedFieldSummary> changedFields)
    {
        return requiredFields is null
            || requiredFields.Count == 0
            || changedFields.Any(field => requiredFields.Contains(field.FieldName));
    }

    private static bool MatchesRelationship(
        IReadOnlySet<string>? requiredProducts,
        IReadOnlyList<StlRelationshipReference> relationshipRefs)
    {
        return requiredProducts is null
            || requiredProducts.Count == 0
            || relationshipRefs.Any(reference => requiredProducts.Contains(reference.ProductKey));
    }

    private static bool MatchesTag(
        IReadOnlySet<string>? requiredTags,
        IReadOnlyList<string> classificationTags)
    {
        return requiredTags is null
            || requiredTags.Count == 0
            || classificationTags.Any(requiredTags.Contains);
    }
}

public sealed record StlEventLoopDecision(
    bool Allowed,
    string ReasonCode,
    string Message,
    int CurrentDepth,
    Guid CorrelationId,
    Guid? CausationId);

public static class StlEventLoopPreventionRules
{
    public const int DefaultMaxResponseDepth = 5;

    public static StlEventLoopDecision EvaluateResponse(
        StlProductEventEnvelope envelope,
        string respondingProductKey,
        string responseIdempotencyKey,
        int maxResponseDepth = DefaultMaxResponseDepth,
        bool allowOwnProductEvents = false)
    {
        if (!StlProductKeys.IsCanonical(respondingProductKey))
        {
            return Block(
                envelope,
                "invalid_responding_product",
                "Responding product key must be canonical.");
        }

        if (string.IsNullOrWhiteSpace(responseIdempotencyKey))
        {
            return Block(
                envelope,
                "missing_response_idempotency_key",
                "Response idempotency key is required.");
        }

        if (!allowOwnProductEvents
            && string.Equals(envelope.SourceProductKey, respondingProductKey, StringComparison.Ordinal))
        {
            return Block(
                envelope,
                "own_source_event",
                "Products do not respond to their own sync-derived events by default.");
        }

        var currentDepth = envelope.Loop?.ResponseDepth ?? 0;
        if (currentDepth >= Math.Max(1, maxResponseDepth))
        {
            return Block(
                envelope,
                "max_response_depth_exceeded",
                "Response depth limit was reached.");
        }

        return new StlEventLoopDecision(
            true,
            "allowed",
            "Response is allowed.",
            currentDepth,
            envelope.CorrelationId,
            envelope.EventId);
    }

    public static StlEventLoopMetadata BuildNextLoopMetadata(
        StlProductEventEnvelope originEnvelope,
        string responderProductKey)
    {
        var origin = originEnvelope.Loop;
        return new StlEventLoopMetadata(
            origin?.OriginEventId ?? originEnvelope.EventId,
            origin?.OriginProductKey ?? originEnvelope.SourceProductKey,
            (origin?.ResponseDepth ?? 0) + 1,
            responderProductKey);
    }

    private static StlEventLoopDecision Block(
        StlProductEventEnvelope envelope,
        string reasonCode,
        string message) =>
        new(
            false,
            reasonCode,
            message,
            envelope.Loop?.ResponseDepth ?? 0,
            envelope.CorrelationId,
            envelope.CausationId);
}
