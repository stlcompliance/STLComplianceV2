using STLCompliance.Shared.Integration;

namespace STLCompliance.Shared.Worker.Tests;

public sealed class IntelligentProductResponseFrameworkTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid EventId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid CorrelationId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    [Fact]
    public void Product_event_envelope_accepts_normalized_metadata_only_event()
    {
        var envelope = BuildCustomerCreatedEnvelope();

        var errors = StlProductEventEnvelopeRules.Validate(envelope);

        Assert.Empty(errors);
    }

    [Fact]
    public void Framework_contract_lists_required_envelope_fields_for_each_product()
    {
        var contract = StlProductResponseFrameworkContracts.Describe(
            StlProductKeys.LoadArr,
            "LoadArr");

        Assert.Equal(StlProductKeys.LoadArr, contract.ProductKey);
        Assert.Contains("eventId", contract.RequiredEnvelopeFields);
        Assert.Contains("detailProjectionUrl", contract.RequiredEnvelopeFields);
        Assert.Contains(StlProductOutboxStatuses.Published, contract.OutboxStatuses);
        Assert.Contains(StlProductInboxStatuses.PendingDetails, contract.InboxStatuses);
        Assert.Contains(StlEventInterestDecisions.ExecuteOwnedWorkflow, contract.InterestDecisions);
        Assert.Contains("no_cross_database_foreign_keys", contract.OwnershipBoundaryRules);
    }

    [Fact]
    public void Product_event_envelope_rejects_missing_projection_wrong_prefix_and_unmarked_sensitive_field()
    {
        var envelope = BuildCustomerCreatedEnvelope() with
        {
            SourceProductKey = StlProductKeys.CustomArr,
            EventName = StlSuiteEventCatalog.OrdArr.OrderCreated,
            DetailProjectionUrl = "",
            ChangedFieldsSummary =
            [
                new StlChangedFieldSummary("serviceToken", "updated"),
            ],
        };

        var errors = StlProductEventEnvelopeRules.Validate(envelope);

        Assert.Contains(errors, error => error.Contains("eventName prefix", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, error => error.Contains("detailProjectionUrl", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, error => error.Contains("serviceToken", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Product_event_envelope_rejects_free_text_relationship_refs()
    {
        var envelope = BuildCustomerCreatedEnvelope() with
        {
            RelationshipRefs =
            [
                new StlRelationshipReference(
                    "Customer product",
                    "customer",
                    "",
                    "customer"),
            ],
        };

        var errors = StlProductEventEnvelopeRules.Validate(envelope);

        Assert.Contains(errors, error => error.Contains("productKey", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, error => error.Contains("entityId", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Adapter_preserves_existing_integration_envelope_identity_and_adds_projection_metadata()
    {
        var existing = new StlIntegrationEventEnvelope(
            EventId,
            StlSuiteEventCatalog.CustomArr.CustomerUpdated,
            StlProductKeys.CustomArr,
            TenantId,
            "customer",
            "customer-100",
            12,
            new DateTimeOffset(2026, 6, 18, 13, 0, 0, TimeSpan.FromHours(-5)),
            "person-100",
            StlIntegrationEventActorTypes.Person,
            StlProductKeys.CustomArr,
            CorrelationId,
            CausationId: Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            "customarr:update:customer:customer-100",
            "1.0",
            new Dictionary<string, object?>
            {
                ["customerId"] = "customer-100",
            });

        var adapted = StlProductEventEnvelopeAdapter.FromIntegrationEnvelope(
            existing,
            StlProductEventOperations.Update,
            "/api/v1/integrations/event-projections/customers/customer-100",
            "1.0",
            changedFieldsSummary:
            [
                new StlChangedFieldSummary("status", "updated"),
            ],
            classificationTags: ["customer"]);

        Assert.Equal(existing.EventId, adapted.EventId);
        Assert.Equal(existing.SourceProductKey, adapted.SourceProductKey);
        Assert.Equal(existing.AggregateType, adapted.SourceEntityType);
        Assert.Equal(existing.AggregateId, adapted.SourceEntityId);
        Assert.Equal(TimeSpan.Zero, adapted.OccurredAtUtc.Offset);
        Assert.Equal(existing.CorrelationId, adapted.CorrelationId);
        Assert.Equal(existing.CausationId, adapted.CausationId);
        Assert.Empty(StlProductEventEnvelopeRules.Validate(adapted));
    }

    [Fact]
    public void Rule_based_interest_evaluator_moves_needs_details_to_pending_details()
    {
        var context = new StlEventInterestContext(
            BuildCustomerCreatedEnvelope(),
            StlProductKeys.RoutArr,
            ProductAvailable: true,
            ResponseBehaviorEnabled: true,
            EnabledFeatureFlags: new HashSet<string>(StringComparer.Ordinal),
            ExistingLocalReferenceKeys: new HashSet<string>(StringComparer.Ordinal));

        var rules = new[]
        {
            new StlEventInterestRule(
                StlProductKeys.RoutArr,
                StlEventInterestDecisions.NeedsDetails,
                "customer_delivery_location_may_affect_transport",
                SourceProductKeys: new HashSet<string>(StringComparer.Ordinal) { StlProductKeys.CustomArr },
                SourceEntityTypes: new HashSet<string>(StringComparer.Ordinal) { "customer" },
                EventNames: new HashSet<string>(StringComparer.Ordinal) { StlSuiteEventCatalog.CustomArr.CustomerCreated },
                DetailPurpose: "transportation_reference_projection",
                AllowedResponseActions:
                [
                    StlEventResponseResults.UpdatedLocalReadModel,
                    StlEventResponseResults.CreatedHumanReviewItem,
                ]),
        };

        var evaluation = StlRuleBasedEventInterestEvaluator.Evaluate(context, rules);

        Assert.Equal(StlEventInterestDecisions.NeedsDetails, evaluation.Decision);
        Assert.True(evaluation.ShouldFetchDetails);
        Assert.Equal(StlProductInboxStatuses.PendingDetails, evaluation.InboxStatus);
        Assert.Equal("transportation_reference_projection", evaluation.DetailPurpose);
    }

    [Fact]
    public void Rule_based_interest_evaluator_ignores_when_product_is_not_available()
    {
        var context = new StlEventInterestContext(
            BuildCustomerCreatedEnvelope(),
            StlProductKeys.RoutArr,
            ProductAvailable: false,
            ResponseBehaviorEnabled: true,
            EnabledFeatureFlags: new HashSet<string>(StringComparer.Ordinal),
            ExistingLocalReferenceKeys: new HashSet<string>(StringComparer.Ordinal));

        var evaluation = StlRuleBasedEventInterestEvaluator.Evaluate(
            context,
            [
                new StlEventInterestRule(
                    StlProductKeys.RoutArr,
                    StlEventInterestDecisions.NeedsDetails,
                    "would_match_when_entitled"),
            ]);

        Assert.Equal(StlEventInterestDecisions.NoInterest, evaluation.Decision);
        Assert.Equal("product_not_available", evaluation.ReasonCode);
        Assert.Equal(StlProductInboxStatuses.Ignored, evaluation.InboxStatus);
    }

    [Fact]
    public void Deduplication_key_scopes_duplicate_detection_to_tenant_consumer_and_source_event()
    {
        var key = StlProductEventDeduplication.BuildInboxDedupeKey(
            TenantId,
            StlProductKeys.RoutArr,
            StlProductKeys.CustomArr,
            EventId,
            "customarr:create:customer:customer-100");

        Assert.Equal(
            "inbox:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa:routarr:customarr:bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb:customarr:create:customer:customer-100",
            key);
    }

    [Theory]
    [InlineData(StlEventInterestDecisions.NoInterest, StlProductInboxStatuses.Ignored)]
    [InlineData(StlEventInterestDecisions.NeedsDetails, StlProductInboxStatuses.PendingDetails)]
    [InlineData(StlEventInterestDecisions.RequestMoreInformation, StlProductInboxStatuses.PendingDetails)]
    [InlineData(StlEventInterestDecisions.EscalateForHumanReview, StlProductInboxStatuses.WaitingForOwner)]
    [InlineData(StlEventInterestDecisions.UpdateLocalReadModel, StlProductInboxStatuses.Processed)]
    public void Inbox_status_follows_interest_decision(string decision, string expectedStatus) =>
        Assert.Equal(expectedStatus, StlProductEventStatusRules.InboxStatusForInterestDecision(decision));

    [Theory]
    [InlineData(1, 3, StlProductOutboxStatuses.Failed)]
    [InlineData(3, 3, StlProductOutboxStatuses.DeadLettered)]
    public void Outbox_failure_status_distinguishes_retryable_and_terminal_failures(
        int attemptCount,
        int maxAttempts,
        string expectedStatus) =>
        Assert.Equal(expectedStatus, StlProductEventStatusRules.OutboxFailureStatus(attemptCount, maxAttempts));

    [Fact]
    public void Loop_prevention_blocks_own_product_response_by_default()
    {
        var envelope = BuildCustomerCreatedEnvelope();

        var decision = StlEventLoopPreventionRules.EvaluateResponse(
            envelope,
            StlProductKeys.CustomArr,
            "customarr:response");

        Assert.False(decision.Allowed);
        Assert.Equal("own_source_event", decision.ReasonCode);
    }

    [Fact]
    public void Loop_prevention_blocks_when_response_depth_is_exhausted()
    {
        var envelope = BuildCustomerCreatedEnvelope() with
        {
            Loop = new StlEventLoopMetadata(EventId, StlProductKeys.CustomArr, 5, StlProductKeys.ReportArr),
        };

        var decision = StlEventLoopPreventionRules.EvaluateResponse(
            envelope,
            StlProductKeys.RoutArr,
            "routarr:update-read-model",
            maxResponseDepth: 5);

        Assert.False(decision.Allowed);
        Assert.Equal("max_response_depth_exceeded", decision.ReasonCode);
    }

    [Fact]
    public void Loop_metadata_preserves_origin_and_increments_depth_for_follow_up_events()
    {
        var envelope = BuildCustomerCreatedEnvelope();

        var metadata = StlEventLoopPreventionRules.BuildNextLoopMetadata(
            envelope,
            StlProductKeys.RoutArr);

        Assert.Equal(envelope.EventId, metadata.OriginEventId);
        Assert.Equal(envelope.SourceProductKey, metadata.OriginProductKey);
        Assert.Equal(1, metadata.ResponseDepth);
        Assert.Equal(StlProductKeys.RoutArr, metadata.LastResponderProductKey);
    }

    private static StlProductEventEnvelope BuildCustomerCreatedEnvelope() =>
        new(
            EventId,
            TenantId,
            StlProductKeys.CustomArr,
            "customer",
            "customer-100",
            StlSuiteEventCatalog.CustomArr.CustomerCreated,
            StlProductEventOperations.Create,
            "1.0",
            new DateTimeOffset(2026, 6, 18, 18, 0, 0, TimeSpan.Zero),
            new StlEventActorRef(
                StlIntegrationEventActorTypes.Person,
                PersonId: "person-100"),
            CorrelationId,
            CausationId: null,
            "customarr:create:customer:customer-100",
            StlEventVisibilityScopes.EntitledProducts,
            StlEventSensitivityLevels.Internal,
            [
                new StlChangedFieldSummary("displayName", "created"),
                new StlChangedFieldSummary("restrictedNote", "created", IsSensitive: true),
            ],
            [
                new StlRelationshipReference(
                    StlProductKeys.CustomArr,
                    "customer",
                    "customer-100",
                    "self",
                    "Northwind Chemical"),
            ],
            ["customer", "reporting_relevant", "transportation_relevant"],
            "/api/v1/integrations/event-projections/customers/customer-100",
            "1.0");
}
