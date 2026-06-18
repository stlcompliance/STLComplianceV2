using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class RoutArrTenantSettingsService(
    RoutArrDbContext db,
    IntegrationOutboxEnqueueService outbox)
{
    private const string TenantSettingsTarget = "routarr_tenant_settings";
    private const string OverrideTarget = "routarr_tenant_setting_override";

    public async Task<RoutArrTenantSettingsResponse> GetEditableAsync(
        Guid tenantId,
        string actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(tenantId, actorPersonId, cancellationToken);
        return BuildResponse(settings, []);
    }

    public async Task<RoutArrTenantSettingsResponse> GetEffectiveAsync(
        Guid tenantId,
        IReadOnlyList<RoutArrSettingsScopeReference> scopes,
        string actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(tenantId, actorPersonId, cancellationToken);
        return BuildResponse(settings, scopes);
    }

    public RoutArrTenantSettingsOptionsResponse GetOptions() =>
        new(
            RoutArrTenantSettingsDefinitions.ToOptionsResponse(),
            RoutArrTenantSettingsDefinitions.ScopeTypes,
            RoutArrTenantSettingsDefinitions.Permissions
                .Select(permission => new RoutArrSettingOptionResponse(permission, permission))
                .ToList());

    public async Task<RoutArrSettingsValidationResponse> ValidateGroupAsync(
        Guid tenantId,
        ValidateRoutArrTenantSettingGroupRequest request,
        string actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(tenantId, actorPersonId, cancellationToken);
        var updates = ParseGroupValues(request.SettingGroup, request.Values);
        var values = BuildMutableValueDictionary(settings);
        foreach (var (path, value) in updates)
        {
            values[path] = value;
        }

        var issues = ValidateValues(values);
        return new RoutArrSettingsValidationResponse(issues.Count == 0, issues);
    }

    public async Task<RoutArrTenantSettingsResponse> UpdateGroupAsync(
        Guid tenantId,
        string actorPersonId,
        string groupKey,
        UpdateRoutArrTenantSettingGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(tenantId, actorPersonId, cancellationToken);
        RequireVersion(settings.Version, request.ExpectedVersion);

        var updates = ParseGroupValues(groupKey, request.Values);
        var values = BuildMutableValueDictionary(settings);
        foreach (var (path, value) in updates)
        {
            values[path] = value;
        }

        ThrowIfInvalid(ValidateValues(values));

        var changedKeys = new List<string>();
        foreach (var (path, value) in updates)
        {
            var definition = RoutArrTenantSettingsDefinitions.ByPath[path];
            var current = ReadValue(settings, definition);
            if (ValuesEqual(current, value))
            {
                continue;
            }

            WriteValue(settings, definition, value, isTenantConfigured: true);
            changedKeys.Add(definition.SettingKey);
        }

        if (changedKeys.Count == 0)
        {
            return BuildResponse(settings, []);
        }

        var previousVersion = settings.Version;
        settings.Version++;
        settings.UpdatedAt = DateTimeOffset.UtcNow;
        settings.UpdatedByPersonId = actorPersonId;

        AddAudit(
            settings.TenantId,
            "updated",
            NormalizeGroup(groupKey),
            changedKeys,
            actorPersonId,
            previousVersion,
            settings.Version,
            $"Updated RoutArr {GroupLabel(groupKey)} settings.",
            previousSummary: null,
            newSummary: request.Reason);

        await db.SaveChangesAsync(cancellationToken);
        await EnqueueSettingsEventAsync(
            tenantId,
            settings.Id,
            RoutArrIntegrationOutboxEventKinds.TenantSettingsUpdated,
            NormalizeGroup(groupKey),
            actorPersonId,
            previousVersion,
            settings.Version,
            changedKeys,
            cancellationToken);

        return BuildResponse(settings, []);
    }

    public async Task<RoutArrTenantSettingsResponse> ResetGroupAsync(
        Guid tenantId,
        string actorPersonId,
        string groupKey,
        ResetRoutArrTenantSettingGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(tenantId, actorPersonId, cancellationToken);
        RequireVersion(settings.Version, request.ExpectedVersion);

        var definitions = DefinitionsForGroup(groupKey);
        var changedKeys = new List<string>();
        foreach (var definition in definitions)
        {
            var current = ReadValue(settings, definition);
            var defaultValue = NormalizeDefault(definition);
            if (!ValuesEqual(current, defaultValue))
            {
                changedKeys.Add(definition.SettingKey);
            }

            WriteValue(settings, definition, defaultValue, isTenantConfigured: false);
        }

        if (changedKeys.Count == 0)
        {
            return BuildResponse(settings, []);
        }

        var previousVersion = settings.Version;
        settings.Version++;
        settings.UpdatedAt = DateTimeOffset.UtcNow;
        settings.UpdatedByPersonId = actorPersonId;

        AddAudit(
            settings.TenantId,
            "reset",
            NormalizeGroup(groupKey),
            changedKeys,
            actorPersonId,
            previousVersion,
            settings.Version,
            $"Reset RoutArr {GroupLabel(groupKey)} settings to platform defaults.",
            previousSummary: null,
            newSummary: request.Reason);

        await db.SaveChangesAsync(cancellationToken);
        await EnqueueSettingsEventAsync(
            tenantId,
            settings.Id,
            RoutArrIntegrationOutboxEventKinds.TenantSettingsReset,
            NormalizeGroup(groupKey),
            actorPersonId,
            previousVersion,
            settings.Version,
            changedKeys,
            cancellationToken);

        return BuildResponse(settings, []);
    }

    public async Task<RoutArrTenantSettingOverrideResponse> CreateOverrideAsync(
        Guid tenantId,
        string actorPersonId,
        CreateRoutArrTenantSettingOverrideRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(tenantId, actorPersonId, cancellationToken);
        var definition = DefinitionFor(request.SettingGroup, request.SettingKey);
        var value = ParseJsonValue(definition, request.Value);
        var scope = NormalizeScope(request.Scope);
        ThrowIfInvalid(ValidateScope(scope));
        ThrowIfInvalid(ValidateOverrideRequest(definition, value, request.Reason));

        var now = DateTimeOffset.UtcNow;
        var entity = new RoutArrTenantSettingOverride
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TenantSettingsId = settings.Id,
            PublicKey = $"routarr_set_ovr_{Guid.NewGuid():N}",
            ScopeType = ParseScopeType(scope.ScopeType),
            ScopeSourceProduct = scope.SourceProduct,
            ScopeEntityType = scope.EntityType,
            ScopeStableId = scope.StableId,
            ScopeDisplayLabelSnapshot = scope.DisplayLabelSnapshot,
            ScopeStatusSnapshot = scope.StatusSnapshot ?? "unknown",
            ScopeSnapshotAt = scope.SnapshotAt ?? now,
            SettingGroup = definition.GroupKey,
            SettingKey = definition.SettingKey,
            ValueKind = definition.ValueKind,
            IsEmergencyOverride = request.IsEmergencyOverride,
            Reason = request.Reason.Trim(),
            Version = 1,
            CreatedAt = now,
            CreatedByPersonId = actorPersonId,
            UpdatedAt = now,
            UpdatedByPersonId = actorPersonId,
        };
        WriteOverrideValue(entity, definition, value);
        db.RoutArrTenantSettingOverrides.Add(entity);

        var previousVersion = settings.Version;
        settings.Version++;
        settings.UpdatedAt = now;
        settings.UpdatedByPersonId = actorPersonId;

        AddAudit(
            tenantId,
            "override_created",
            definition.GroupKey,
            [definition.SettingKey],
            actorPersonId,
            previousVersion,
            settings.Version,
            $"Created scoped override for {definition.Label}.",
            scope.ScopeType,
            ScopeAuditRef(scope),
            null,
            request.Reason);

        await db.SaveChangesAsync(cancellationToken);
        await EnqueueOverrideEventAsync(
            tenantId,
            settings.Id,
            RoutArrIntegrationOutboxEventKinds.TenantSettingOverrideCreated,
            definition.GroupKey,
            actorPersonId,
            previousVersion,
            settings.Version,
            [definition.SettingKey],
            scope,
            cancellationToken);

        return MapOverride(entity);
    }

    public async Task<RoutArrTenantSettingOverrideResponse> UpdateOverrideAsync(
        Guid tenantId,
        string actorPersonId,
        string overrideKey,
        UpdateRoutArrTenantSettingOverrideRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(tenantId, actorPersonId, cancellationToken);
        var entity = settings.Overrides.FirstOrDefault(x =>
            string.Equals(x.PublicKey, overrideKey, StringComparison.OrdinalIgnoreCase));
        if (entity is null)
        {
            throw new StlApiException("routarr.settings.override_not_found", "Scoped override was not found.", 404);
        }

        RequireVersion(entity.Version, request.ExpectedVersion);
        var definition = DefinitionFor(entity.SettingGroup, entity.SettingKey);
        var value = ParseJsonValue(definition, request.Value);
        ThrowIfInvalid(ValidateOverrideRequest(definition, value, request.Reason));

        var previousVersion = settings.Version;
        var existingListItems = entity.ListItems.ToList();
        if (existingListItems.Count > 0)
        {
            db.RoutArrTenantSettingOverrideListItems.RemoveRange(existingListItems);
            entity.ListItems.Clear();
        }

        WriteOverrideValue(entity, definition, value);
        if (definition.ValueKind == RoutArrTenantSettingValueKind.MultiSelect)
        {
            db.RoutArrTenantSettingOverrideListItems.AddRange(entity.ListItems);
        }

        entity.IsEmergencyOverride = request.IsEmergencyOverride;
        entity.Reason = request.Reason.Trim();
        entity.Version++;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedByPersonId = actorPersonId;

        settings.Version++;
        settings.UpdatedAt = entity.UpdatedAt;
        settings.UpdatedByPersonId = actorPersonId;

        var scope = MapScope(entity);
        AddAudit(
            tenantId,
            "override_updated",
            definition.GroupKey,
            [definition.SettingKey],
            actorPersonId,
            previousVersion,
            settings.Version,
            $"Updated scoped override for {definition.Label}.",
            scope.ScopeType,
            ScopeAuditRef(scope),
            null,
            request.Reason);

        await db.SaveChangesAsync(cancellationToken);
        await EnqueueOverrideEventAsync(
            tenantId,
            settings.Id,
            RoutArrIntegrationOutboxEventKinds.TenantSettingOverrideUpdated,
            definition.GroupKey,
            actorPersonId,
            previousVersion,
            settings.Version,
            [definition.SettingKey],
            scope,
            cancellationToken);

        return MapOverride(entity);
    }

    public async Task DeleteOverrideAsync(
        Guid tenantId,
        string actorPersonId,
        string overrideKey,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(tenantId, actorPersonId, cancellationToken);
        var entity = settings.Overrides.FirstOrDefault(x =>
            string.Equals(x.PublicKey, overrideKey, StringComparison.OrdinalIgnoreCase));
        if (entity is null)
        {
            throw new StlApiException("routarr.settings.override_not_found", "Scoped override was not found.", 404);
        }

        var definition = DefinitionFor(entity.SettingGroup, entity.SettingKey);
        var scope = MapScope(entity);
        var previousVersion = settings.Version;
        settings.Overrides.Remove(entity);
        settings.Version++;
        settings.UpdatedAt = DateTimeOffset.UtcNow;
        settings.UpdatedByPersonId = actorPersonId;

        AddAudit(
            tenantId,
            "override_deleted",
            definition.GroupKey,
            [definition.SettingKey],
            actorPersonId,
            previousVersion,
            settings.Version,
            $"Deleted scoped override for {definition.Label}.",
            scope.ScopeType,
            ScopeAuditRef(scope),
            null,
            entity.Reason);

        await db.SaveChangesAsync(cancellationToken);
        await EnqueueOverrideEventAsync(
            tenantId,
            settings.Id,
            RoutArrIntegrationOutboxEventKinds.TenantSettingOverrideDeleted,
            definition.GroupKey,
            actorPersonId,
            previousVersion,
            settings.Version,
            [definition.SettingKey],
            scope,
            cancellationToken);
    }

    public async Task<RoutArrTenantSettingAuditHistoryResponse> ListAuditHistoryAsync(
        Guid tenantId,
        string? groupKey,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedGroup = string.IsNullOrWhiteSpace(groupKey) ? null : NormalizeGroup(groupKey);
        var take = Math.Clamp(limit ?? 50, 1, 200);
        var query = db.RoutArrTenantSettingAuditEntries.AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (normalizedGroup is not null)
        {
            query = query.Where(x => x.SettingGroup == normalizedGroup);
        }

        var items = await query
            .OrderByDescending(x => x.ChangedAt)
            .Take(take)
            .Select(x => new RoutArrTenantSettingAuditEntryResponse(
                x.PublicKey,
                x.Action,
                x.SettingGroup,
                SplitKeys(x.ChangedKeys),
                x.ChangedByPersonId,
                x.ChangedAt,
                x.PreviousVersion,
                x.NewVersion,
                x.AffectedScopeType,
                x.AffectedScopeRef,
                x.Summary))
            .ToListAsync(cancellationToken);

        return new RoutArrTenantSettingAuditHistoryResponse(items);
    }

    public static IReadOnlyList<RoutArrSettingValidationIssue> ValidateValues(
        IReadOnlyDictionary<string, object?> values)
    {
        var issues = new List<RoutArrSettingValidationIssue>();

        var routingGuideEnabled = BoolValue(values, "tendering.routingGuideEnabled");
        var defaultTenderMethod = TextValue(values, "tendering.defaultTenderMethod");
        if (string.Equals(defaultTenderMethod, "auto_tender", StringComparison.OrdinalIgnoreCase)
            && routingGuideEnabled is false)
        {
            issues.Add(Error("tendering.defaultTenderMethod", "Auto-tender requires routing guide to be enabled."));
        }

        var ratingEnabled = BoolValue(values, "rating.ratingEnabled");
        var requireRateBeforeTender = TextValue(values, "rating.requireRateBeforeTender");
        var defaultRatingMode = TextValue(values, "rating.defaultRatingMode");
        if (ratingEnabled is false
            && !string.Equals(requireRateBeforeTender, "off", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(defaultRatingMode, "integration", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(Error("rating.requireRateBeforeTender", "Rate-before-tender gates require rating to be enabled or an integration rating mode."));
        }

        if (BoolValue(values, "stopsAppointments.autoCompleteStopFromGeofence") is true
            && (BoolValue(values, "trackingVisibility.visibilityEnabled") is false
                || IntValue(values, "trackingVisibility.geofenceRadius") <= 0))
        {
            issues.Add(Error("stopsAppointments.autoCompleteStopFromGeofence", "Geofence stop completion requires visibility and a geofence radius."));
        }

        if (string.Equals(TextValue(values, "trackingVisibility.locationPrecisionSharing"), "exact", StringComparison.OrdinalIgnoreCase)
            && BoolValue(values, "trackingVisibility.customerVisibilityEnabled") is false)
        {
            issues.Add(Error("trackingVisibility.locationPrecisionSharing", "Exact customer location sharing requires customer visibility to be enabled."));
        }

        if (BoolValue(values, "closeout.autoCloseEligibleTrips") is true
            && BoolValue(values, "closeout.requireDispatcherReview") is true)
        {
            issues.Add(Error("closeout.autoCloseEligibleTrips", "Auto-close cannot run while manual dispatcher review is required."));
        }

        if (BoolValue(values, "dockYardHandoffs.requireDockAppointmentBeforeDispatch") is true
            && BoolValue(values, "dockYardHandoffs.allowRoutArrCreatedDockRequest") is false
            && BoolValue(values, "dockYardHandoffs.notifyLoadArrForInboundAppointments") is false)
        {
            issues.Add(Error("dockYardHandoffs.requireDockAppointmentBeforeDispatch", "Dock appointment dispatch gates require LoadArr handoff or RoutArr dock request capability."));
        }

        if (string.Equals(TextValue(values, "assignment.allowUnqualifiedDriverAssignment"), "allow", StringComparison.OrdinalIgnoreCase)
            && BoolValue(values, "overridesApprovals.overrideReasonRequired") is false)
        {
            issues.Add(Error("assignment.allowUnqualifiedDriverAssignment", "Silently allowing unqualified driver assignment requires override reason audit to remain enabled."));
        }

        if (BoolValue(values, "documents.recordArrHandoffEnabled") is true
            && BoolValue(values, "integrations.recordArrIntegrationEnabled") is false)
        {
            issues.Add(Error("documents.recordArrHandoffEnabled", "RecordArr handoff requires RecordArr integration to be enabled."));
        }

        var assetGuard = TextValue(values, "assignment.allowUnavailableAssetAssignment");
        if ((string.Equals(assetGuard, "block", StringComparison.OrdinalIgnoreCase)
                || string.Equals(assetGuard, "require_override", StringComparison.OrdinalIgnoreCase))
            && BoolValue(values, "integrations.maintainArrIntegrationEnabled") is false)
        {
            issues.Add(Error("assignment.allowUnavailableAssetAssignment", "MaintainArr asset-readiness blocking requires MaintainArr integration to be enabled."));
        }

        var hosGate = TextValue(values, "hosAvailability.requireHosCheckBeforeDispatch");
        if ((string.Equals(hosGate, "block", StringComparison.OrdinalIgnoreCase)
                || string.Equals(hosGate, "require_review", StringComparison.OrdinalIgnoreCase))
            && BoolValue(values, "integrations.complianceCoreIntegrationEnabled") is false)
        {
            issues.Add(Error("hosAvailability.requireHosCheckBeforeDispatch", "HOS/compliance blocking requires Compliance Core integration to be enabled."));
        }

        return issues;
    }

    private async Task<RoutArrTenantSettings> EnsureSettingsAsync(
        Guid tenantId,
        string actorPersonId,
        CancellationToken cancellationToken)
    {
        var settings = await db.RoutArrTenantSettings
            .Include(x => x.Values)
            .Include(x => x.ListItems)
            .Include(x => x.Overrides)
                .ThenInclude(x => x.ListItems)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            var now = DateTimeOffset.UtcNow;
            settings = new RoutArrTenantSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Version = 1,
                CreatedAt = now,
                CreatedByPersonId = actorPersonId,
                UpdatedAt = now,
                UpdatedByPersonId = actorPersonId,
            };

            db.RoutArrTenantSettings.Add(settings);
            foreach (var definition in RoutArrTenantSettingsDefinitions.All)
            {
                WriteValue(settings, definition, NormalizeDefault(definition), isTenantConfigured: false);
            }

            await db.SaveChangesAsync(cancellationToken);
            return settings;
        }

        var added = false;
        foreach (var definition in RoutArrTenantSettingsDefinitions.All)
        {
            if (settings.Values.Any(x =>
                    string.Equals(x.SettingGroup, definition.GroupKey, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.SettingKey, definition.SettingKey, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            WriteValue(settings, definition, NormalizeDefault(definition), isTenantConfigured: false);
            added = true;
        }

        if (added)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return settings;
    }

    private RoutArrTenantSettingsResponse BuildResponse(
        RoutArrTenantSettings settings,
        IReadOnlyList<RoutArrSettingsScopeReference> scopes)
    {
        var normalizedScopes = scopes.Select(NormalizeScope).ToList();
        var matchingOverrides = normalizedScopes.Count == 0
            ? []
            : settings.Overrides
                .Where(x => normalizedScopes.Any(scope => ScopeMatches(scope, x)))
                .OrderBy(x => OverridePrecedence(x, normalizedScopes))
                .ToList();

        var groups = RoutArrTenantSettingsDefinitions.All
            .GroupBy(x => x.GroupKey)
            .Select(group =>
            {
                var first = group.First();
                var fields = group.OrderBy(x => x.DisplayOrder)
                    .Select(definition =>
                    {
                        var baseValue = ReadValue(settings, definition);
                        var platformDefault = NormalizeDefault(definition);
                        var source = IsTenantConfigured(settings, definition) ? "tenantDefault" : "platformDefault";
                        var appliedOverride = matchingOverrides
                            .Where(x => string.Equals(x.SettingGroup, definition.GroupKey, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(x.SettingKey, definition.SettingKey, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(x => OverridePrecedence(x, normalizedScopes))
                            .LastOrDefault();
                        if (appliedOverride is not null)
                        {
                            baseValue = ReadOverrideValue(appliedOverride, definition);
                            source = appliedOverride.IsEmergencyOverride
                                ? "emergencyOverride"
                                : $"{ToScopeKey(appliedOverride.ScopeType)}Override";
                        }

                        return new RoutArrSettingFieldResponse(
                            definition.SettingKey,
                            definition.Label,
                            RoutArrTenantSettingsDefinitions.ToKindKey(definition.ValueKind),
                            baseValue,
                            platformDefault,
                            source,
                            definition.HelpText,
                            definition.Options);
                    })
                    .ToList();

                return new RoutArrSettingGroupResponse(
                    first.GroupKey,
                    first.GroupLabel,
                    first.GroupDescription,
                    fields,
                    settings.UpdatedAt,
                    settings.UpdatedByPersonId);
            })
            .ToList();

        return new RoutArrTenantSettingsResponse(
            settings.TenantId,
            settings.Version,
            DateTimeOffset.UtcNow,
            groups,
            settings.Overrides
                .OrderByDescending(x => x.UpdatedAt)
                .Select(MapOverride)
                .ToList());
    }

    private IReadOnlyDictionary<string, object?> ParseGroupValues(
        string groupKey,
        IReadOnlyDictionary<string, JsonElement> payload)
    {
        var normalizedGroup = NormalizeGroup(groupKey);
        _ = DefinitionsForGroup(normalizedGroup);
        var parsed = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (settingKey, value) in payload)
        {
            var definition = DefinitionFor(normalizedGroup, settingKey);
            parsed[RoutArrTenantSettingsDefinitions.Path(definition.GroupKey, definition.SettingKey)] =
                ParseJsonValue(definition, value);
        }

        return parsed;
    }

    private static object? ParseJsonValue(RoutArrTenantSettingDefinition definition, JsonElement value)
    {
        try
        {
            return definition.ValueKind switch
            {
                RoutArrTenantSettingValueKind.Boolean => value.ValueKind == JsonValueKind.True
                    || (value.ValueKind == JsonValueKind.False ? false : value.GetBoolean()),
                RoutArrTenantSettingValueKind.Integer => value.GetInt32(),
                RoutArrTenantSettingValueKind.Decimal => value.GetDecimal(),
                RoutArrTenantSettingValueKind.Text => value.GetString() ?? string.Empty,
                RoutArrTenantSettingValueKind.Enum => ValidateEnum(definition, value.GetString() ?? string.Empty),
                RoutArrTenantSettingValueKind.Time => ParseTime(value.GetString() ?? string.Empty),
                RoutArrTenantSettingValueKind.DurationMinutes => value.GetInt32(),
                RoutArrTenantSettingValueKind.MultiSelect => ParseList(definition, value),
                _ => value.GetString(),
            };
        }
        catch (Exception exception) when (exception is InvalidOperationException or FormatException)
        {
            throw new StlApiException(
                "routarr.settings.invalid_value",
                $"{definition.Label} has an invalid value.",
                400,
                new[] { Error($"{definition.GroupKey}.{definition.SettingKey}", $"{definition.Label} has an invalid value.") });
        }
    }

    private static IReadOnlyList<string> ParseList(RoutArrTenantSettingDefinition definition, JsonElement value)
    {
        var items = value.ValueKind switch
        {
            JsonValueKind.Array => value.EnumerateArray()
                .Select(x => x.GetString() ?? string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            JsonValueKind.String => value.GetString()?
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [],
            _ => [],
        };

        var allowed = definition.Options.Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var invalid = items.Where(x => !allowed.Contains(x)).ToList();
        if (invalid.Count > 0)
        {
            throw new StlApiException(
                "routarr.settings.unsupported_option",
                $"{definition.Label} contains unsupported option values.",
                400,
                invalid.Select(x => Error($"{definition.GroupKey}.{definition.SettingKey}", $"{x} is not an allowed value.")).ToList());
        }

        return items;
    }

    private static string ValidateEnum(RoutArrTenantSettingDefinition definition, string value)
    {
        var normalized = value.Trim();
        if (!definition.Options.Any(x => string.Equals(x.Value, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            throw new StlApiException(
                "routarr.settings.unsupported_enum",
                $"{definition.Label} is not an allowed value.",
                400,
                new[] { Error($"{definition.GroupKey}.{definition.SettingKey}", $"{normalized} is not an allowed value.") });
        }

        return normalized;
    }

    private static TimeOnly ParseTime(string value) =>
        TimeOnly.TryParse(value.Trim(), out var parsed)
            ? parsed
            : throw new FormatException("Invalid time.");

    private static Dictionary<string, object?> BuildMutableValueDictionary(RoutArrTenantSettings settings)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in RoutArrTenantSettingsDefinitions.All)
        {
            values[RoutArrTenantSettingsDefinitions.Path(definition.GroupKey, definition.SettingKey)] =
                ReadValue(settings, definition);
        }

        return values;
    }

    private static object? ReadValue(RoutArrTenantSettings settings, RoutArrTenantSettingDefinition definition)
    {
        var value = settings.Values.FirstOrDefault(x =>
            string.Equals(x.SettingGroup, definition.GroupKey, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.SettingKey, definition.SettingKey, StringComparison.OrdinalIgnoreCase));

        if (value is null)
        {
            return NormalizeDefault(definition);
        }

        return definition.ValueKind switch
        {
            RoutArrTenantSettingValueKind.Boolean => value.BooleanValue ?? (bool)NormalizeDefault(definition)!,
            RoutArrTenantSettingValueKind.Integer => value.IntegerValue ?? (int)NormalizeDefault(definition)!,
            RoutArrTenantSettingValueKind.Decimal => value.DecimalValue ?? Convert.ToDecimal(NormalizeDefault(definition)),
            RoutArrTenantSettingValueKind.Text => value.TextValue ?? string.Empty,
            RoutArrTenantSettingValueKind.Enum => value.EnumValue ?? (string)NormalizeDefault(definition)!,
            RoutArrTenantSettingValueKind.Time => (value.TimeValue ?? ParseTime((string)NormalizeDefault(definition)!)).ToString("HH:mm"),
            RoutArrTenantSettingValueKind.DurationMinutes => value.DurationMinutesValue ?? (int)NormalizeDefault(definition)!,
            RoutArrTenantSettingValueKind.MultiSelect => settings.ListItems
                .Where(x => string.Equals(x.SettingGroup, definition.GroupKey, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.SettingKey, definition.SettingKey, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.SortOrder)
                .Select(x => x.ItemKey)
                .ToList(),
            _ => value.TextValue,
        };
    }

    private static object? ReadOverrideValue(
        RoutArrTenantSettingOverride entity,
        RoutArrTenantSettingDefinition definition) =>
        definition.ValueKind switch
        {
            RoutArrTenantSettingValueKind.Boolean => entity.BooleanValue,
            RoutArrTenantSettingValueKind.Integer => entity.IntegerValue,
            RoutArrTenantSettingValueKind.Decimal => entity.DecimalValue,
            RoutArrTenantSettingValueKind.Text => entity.TextValue,
            RoutArrTenantSettingValueKind.Enum => entity.EnumValue,
            RoutArrTenantSettingValueKind.Time => entity.TimeValue?.ToString("HH:mm"),
            RoutArrTenantSettingValueKind.DurationMinutes => entity.DurationMinutesValue,
            RoutArrTenantSettingValueKind.MultiSelect => entity.ListItems
                .OrderBy(x => x.SortOrder)
                .Select(x => x.ItemKey)
                .ToList(),
            _ => entity.TextValue,
        };

    private void WriteValue(
        RoutArrTenantSettings settings,
        RoutArrTenantSettingDefinition definition,
        object? rawValue,
        bool isTenantConfigured)
    {
        var entity = settings.Values.FirstOrDefault(x =>
            string.Equals(x.SettingGroup, definition.GroupKey, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.SettingKey, definition.SettingKey, StringComparison.OrdinalIgnoreCase));
        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new RoutArrTenantSettingValue
            {
                Id = Guid.NewGuid(),
                TenantId = settings.TenantId,
                TenantSettingsId = settings.Id,
                SettingGroup = definition.GroupKey,
                SettingKey = definition.SettingKey,
                ValueKind = definition.ValueKind,
                CreatedAt = now,
            };
            settings.Values.Add(entity);
            db.RoutArrTenantSettingValues.Add(entity);
        }

        entity.BooleanValue = null;
        entity.IntegerValue = null;
        entity.DecimalValue = null;
        entity.TextValue = null;
        entity.EnumValue = null;
        entity.TimeValue = null;
        entity.DurationMinutesValue = null;
        entity.IsTenantConfigured = isTenantConfigured;
        entity.UpdatedAt = now;

        switch (definition.ValueKind)
        {
            case RoutArrTenantSettingValueKind.Boolean:
                entity.BooleanValue = Convert.ToBoolean(rawValue);
                break;
            case RoutArrTenantSettingValueKind.Integer:
                entity.IntegerValue = Convert.ToInt32(rawValue);
                break;
            case RoutArrTenantSettingValueKind.Decimal:
                entity.DecimalValue = Convert.ToDecimal(rawValue);
                break;
            case RoutArrTenantSettingValueKind.Text:
                entity.TextValue = Convert.ToString(rawValue) ?? string.Empty;
                break;
            case RoutArrTenantSettingValueKind.Enum:
                entity.EnumValue = Convert.ToString(rawValue) ?? string.Empty;
                break;
            case RoutArrTenantSettingValueKind.Time:
                entity.TimeValue = rawValue is TimeOnly time ? time : ParseTime(Convert.ToString(rawValue) ?? string.Empty);
                break;
            case RoutArrTenantSettingValueKind.DurationMinutes:
                entity.DurationMinutesValue = Convert.ToInt32(rawValue);
                break;
            case RoutArrTenantSettingValueKind.MultiSelect:
                var items = ToStringList(rawValue);
                var existing = settings.ListItems
                    .Where(x => string.Equals(x.SettingGroup, definition.GroupKey, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(x.SettingKey, definition.SettingKey, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (existing.Count > 0)
                {
                    db.RoutArrTenantSettingListItems.RemoveRange(existing);
                }

                foreach (var item in existing)
                {
                    settings.ListItems.Remove(item);
                }

                var optionsByValue = definition.Options.ToDictionary(x => x.Value, x => x.Label, StringComparer.OrdinalIgnoreCase);
                var sortOrder = 0;
                foreach (var item in items)
                {
                    var listItem = new RoutArrTenantSettingListItem
                    {
                        Id = Guid.NewGuid(),
                        TenantId = settings.TenantId,
                        TenantSettingsId = settings.Id,
                        SettingGroup = definition.GroupKey,
                        SettingKey = definition.SettingKey,
                        ItemKey = item,
                        DisplayLabel = optionsByValue.GetValueOrDefault(item, item),
                        SortOrder = sortOrder++,
                        IsTenantConfigured = isTenantConfigured,
                    };
                    settings.ListItems.Add(listItem);
                    db.RoutArrTenantSettingListItems.Add(listItem);
                }

                break;
        }
    }

    private static void WriteOverrideValue(
        RoutArrTenantSettingOverride entity,
        RoutArrTenantSettingDefinition definition,
        object? rawValue)
    {
        entity.BooleanValue = null;
        entity.IntegerValue = null;
        entity.DecimalValue = null;
        entity.TextValue = null;
        entity.EnumValue = null;
        entity.TimeValue = null;
        entity.DurationMinutesValue = null;

        switch (definition.ValueKind)
        {
            case RoutArrTenantSettingValueKind.Boolean:
                entity.BooleanValue = Convert.ToBoolean(rawValue);
                break;
            case RoutArrTenantSettingValueKind.Integer:
                entity.IntegerValue = Convert.ToInt32(rawValue);
                break;
            case RoutArrTenantSettingValueKind.Decimal:
                entity.DecimalValue = Convert.ToDecimal(rawValue);
                break;
            case RoutArrTenantSettingValueKind.Text:
                entity.TextValue = Convert.ToString(rawValue) ?? string.Empty;
                break;
            case RoutArrTenantSettingValueKind.Enum:
                entity.EnumValue = Convert.ToString(rawValue) ?? string.Empty;
                break;
            case RoutArrTenantSettingValueKind.Time:
                entity.TimeValue = rawValue is TimeOnly time ? time : ParseTime(Convert.ToString(rawValue) ?? string.Empty);
                break;
            case RoutArrTenantSettingValueKind.DurationMinutes:
                entity.DurationMinutesValue = Convert.ToInt32(rawValue);
                break;
            case RoutArrTenantSettingValueKind.MultiSelect:
                entity.ListItems.Clear();
                var optionsByValue = definition.Options.ToDictionary(x => x.Value, x => x.Label, StringComparer.OrdinalIgnoreCase);
                var sortOrder = 0;
                foreach (var item in ToStringList(rawValue))
                {
                    entity.ListItems.Add(new RoutArrTenantSettingOverrideListItem
                    {
                        Id = Guid.NewGuid(),
                        TenantId = entity.TenantId,
                        OverrideId = entity.Id,
                        ItemKey = item,
                        DisplayLabel = optionsByValue.GetValueOrDefault(item, item),
                        SortOrder = sortOrder++,
                    });
                }

                break;
        }
    }

    private static bool IsTenantConfigured(
        RoutArrTenantSettings settings,
        RoutArrTenantSettingDefinition definition) =>
        settings.Values.Any(x =>
            string.Equals(x.SettingGroup, definition.GroupKey, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.SettingKey, definition.SettingKey, StringComparison.OrdinalIgnoreCase)
            && x.IsTenantConfigured);

    private static object? NormalizeDefault(RoutArrTenantSettingDefinition definition) =>
        definition.ValueKind switch
        {
            RoutArrTenantSettingValueKind.Time => ParseTime((string)definition.PlatformDefaultValue!).ToString("HH:mm"),
            RoutArrTenantSettingValueKind.MultiSelect => ToStringList(definition.PlatformDefaultValue),
            _ => definition.PlatformDefaultValue,
        };

    private static IReadOnlyList<string> ToStringList(object? value) =>
        value switch
        {
            IReadOnlyList<string> list => list,
            IEnumerable<string> enumerable => enumerable.ToList(),
            string text => text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList(),
            _ => [],
        };

    private static IReadOnlyList<RoutArrTenantSettingDefinition> DefinitionsForGroup(string groupKey)
    {
        var normalized = NormalizeGroup(groupKey);
        var definitions = RoutArrTenantSettingsDefinitions.All
            .Where(x => string.Equals(x.GroupKey, normalized, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (definitions.Count == 0)
        {
            throw new StlApiException("routarr.settings.group_unknown", "RoutArr settings group is not supported.", 404);
        }

        return definitions;
    }

    private static RoutArrTenantSettingDefinition DefinitionFor(string groupKey, string settingKey)
    {
        var normalized = NormalizeGroup(groupKey);
        var path = RoutArrTenantSettingsDefinitions.Path(normalized, settingKey.Trim());
        if (!RoutArrTenantSettingsDefinitions.ByPath.TryGetValue(path, out var definition))
        {
            throw new StlApiException(
                "routarr.settings.setting_unknown",
                "RoutArr setting is not supported.",
                404,
                new[] { Error(path, "Setting is not supported.") });
        }

        return definition;
    }

    private static string NormalizeGroup(string groupKey)
    {
        var normalized = groupKey.Trim();
        return normalized switch
        {
            "dispatch-board" => "dispatchBoard",
            "hos" => "hosAvailability",
            "hos-availability" => "hosAvailability",
            "stops" => "stopsAppointments",
            "stop-appointments" => "stopsAppointments",
            "tracking" => "trackingVisibility",
            "visibility" => "trackingVisibility",
            "detention-accessorials" => "detentionAccessorials",
            "status" => "statusModel",
            "overrides" => "overridesApprovals",
            "approvals" => "overridesApprovals",
            _ => normalized,
        };
    }

    private static string GroupLabel(string groupKey) =>
        DefinitionsForGroup(groupKey).First().GroupLabel;

    private static void RequireVersion(int currentVersion, int? expectedVersion)
    {
        if (expectedVersion.HasValue && currentVersion != expectedVersion.Value)
        {
            throw new StlApiException(
                "routarr.settings.concurrency_conflict",
                "RoutArr settings changed after this page loaded. Reload and apply the change again.",
                409);
        }
    }

    private static void ThrowIfInvalid(IReadOnlyList<RoutArrSettingValidationIssue> issues)
    {
        if (issues.Count > 0)
        {
            throw new StlApiException(
                "routarr.settings.validation_failed",
                "RoutArr settings have invalid combinations.",
                400,
                issues);
        }
    }

    private static IReadOnlyList<RoutArrSettingValidationIssue> ValidateOverrideRequest(
        RoutArrTenantSettingDefinition definition,
        object? value,
        string reason)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            [RoutArrTenantSettingsDefinitions.Path(definition.GroupKey, definition.SettingKey)] = value,
        };
        var issues = ValidateFieldBounds(definition, value);
        if (string.IsNullOrWhiteSpace(reason))
        {
            issues.Add(Error($"{definition.GroupKey}.{definition.SettingKey}", "Override reason is required."));
        }

        return issues;
    }

    private static List<RoutArrSettingValidationIssue> ValidateFieldBounds(
        RoutArrTenantSettingDefinition definition,
        object? value)
    {
        var issues = new List<RoutArrSettingValidationIssue>();
        if (definition.MinValue.HasValue || definition.MaxValue.HasValue)
        {
            var numeric = definition.ValueKind switch
            {
                RoutArrTenantSettingValueKind.Integer => Convert.ToDecimal(value),
                RoutArrTenantSettingValueKind.Decimal => Convert.ToDecimal(value),
                RoutArrTenantSettingValueKind.DurationMinutes => Convert.ToDecimal(value),
                _ => (decimal?)null,
            };
            if (numeric.HasValue && definition.MinValue.HasValue && numeric.Value < definition.MinValue.Value)
            {
                issues.Add(Error($"{definition.GroupKey}.{definition.SettingKey}", $"{definition.Label} must be at least {definition.MinValue.Value}."));
            }

            if (numeric.HasValue && definition.MaxValue.HasValue && numeric.Value > definition.MaxValue.Value)
            {
                issues.Add(Error($"{definition.GroupKey}.{definition.SettingKey}", $"{definition.Label} must be at most {definition.MaxValue.Value}."));
            }
        }

        return issues;
    }

    private static IReadOnlyList<RoutArrSettingValidationIssue> ValidateScope(RoutArrSettingsScopeReference scope)
    {
        var issues = new List<RoutArrSettingValidationIssue>();
        if (string.IsNullOrWhiteSpace(scope.StableId))
        {
            issues.Add(Error("scope.stableId", "Scope reference stable ID is required."));
        }

        if (string.IsNullOrWhiteSpace(scope.DisplayLabelSnapshot))
        {
            issues.Add(Error("scope.displayLabelSnapshot", "Scope display label snapshot is required."));
        }

        var source = scope.SourceProduct.ToLowerInvariant();
        switch (scope.ScopeType)
        {
            case "site":
            case "terminal":
                if (source != "staffarr")
                {
                    issues.Add(Error("scope.sourceProduct", "Site and terminal overrides must reference StaffArr location identity."));
                }

                break;
            case "customer":
                if (source != "customarr")
                {
                    issues.Add(Error("scope.sourceProduct", "Customer overrides must reference CustomArr customer identity."));
                }

                break;
            case "carrier":
                if (source != "supplyarr")
                {
                    issues.Add(Error("scope.sourceProduct", "Carrier overrides must reference SupplyArr carrier/vendor identity."));
                }

                break;
            case "demand":
            case "trip":
            case "lane":
            case "routeType":
            case "serviceType":
            case "tenant":
                if (source != "routarr")
                {
                    issues.Add(Error("scope.sourceProduct", $"{scope.ScopeType} overrides must reference RoutArr-owned scope identity."));
                }

                break;
            default:
                issues.Add(Error("scope.scopeType", "Scope type is not supported."));
                break;
        }

        return issues;
    }

    private static RoutArrSettingsScopeReference NormalizeScope(RoutArrSettingsScopeReference scope) =>
        new(
            NormalizeScopeKey(scope.ScopeType),
            scope.SourceProduct.Trim().ToLowerInvariant(),
            scope.EntityType.Trim(),
            scope.StableId.Trim(),
            scope.DisplayLabelSnapshot.Trim(),
            string.IsNullOrWhiteSpace(scope.StatusSnapshot) ? "unknown" : scope.StatusSnapshot.Trim(),
            scope.SnapshotAt);

    private static string NormalizeScopeKey(string scopeType)
    {
        var normalized = scopeType.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        return normalized switch
        {
            "route_type" => "routeType",
            "service_type" => "serviceType",
            "transportationDemand" => "demand",
            "transportation_demand" => "demand",
            _ => char.ToLowerInvariant(normalized[0]) + normalized[1..],
        };
    }

    private static RoutArrTenantSettingScopeType ParseScopeType(string scopeType) =>
        NormalizeScopeKey(scopeType) switch
        {
            "tenant" => RoutArrTenantSettingScopeType.Tenant,
            "site" => RoutArrTenantSettingScopeType.Site,
            "terminal" => RoutArrTenantSettingScopeType.Terminal,
            "customer" => RoutArrTenantSettingScopeType.Customer,
            "carrier" => RoutArrTenantSettingScopeType.Carrier,
            "lane" => RoutArrTenantSettingScopeType.Lane,
            "routeType" => RoutArrTenantSettingScopeType.RouteType,
            "serviceType" => RoutArrTenantSettingScopeType.ServiceType,
            "demand" => RoutArrTenantSettingScopeType.Demand,
            "trip" => RoutArrTenantSettingScopeType.Trip,
            _ => throw new StlApiException("routarr.settings.scope_unknown", "Scope type is not supported.", 400),
        };

    private static string ToScopeKey(RoutArrTenantSettingScopeType scopeType) =>
        scopeType switch
        {
            RoutArrTenantSettingScopeType.Tenant => "tenant",
            RoutArrTenantSettingScopeType.Site => "site",
            RoutArrTenantSettingScopeType.Terminal => "terminal",
            RoutArrTenantSettingScopeType.Customer => "customer",
            RoutArrTenantSettingScopeType.Carrier => "carrier",
            RoutArrTenantSettingScopeType.Lane => "lane",
            RoutArrTenantSettingScopeType.RouteType => "routeType",
            RoutArrTenantSettingScopeType.ServiceType => "serviceType",
            RoutArrTenantSettingScopeType.Demand => "demand",
            RoutArrTenantSettingScopeType.Trip => "trip",
            _ => "tenant",
        };

    private static bool ScopeMatches(
        RoutArrSettingsScopeReference scope,
        RoutArrTenantSettingOverride entity) =>
        string.Equals(scope.ScopeType, ToScopeKey(entity.ScopeType), StringComparison.OrdinalIgnoreCase)
        && string.Equals(scope.SourceProduct, entity.ScopeSourceProduct, StringComparison.OrdinalIgnoreCase)
        && string.Equals(scope.EntityType, entity.ScopeEntityType, StringComparison.OrdinalIgnoreCase)
        && string.Equals(scope.StableId, entity.ScopeStableId, StringComparison.OrdinalIgnoreCase);

    private static int OverridePrecedence(
        RoutArrTenantSettingOverride entity,
        IReadOnlyList<RoutArrSettingsScopeReference> scopes)
    {
        if (entity.IsEmergencyOverride)
        {
            return 800;
        }

        return entity.ScopeType switch
        {
            RoutArrTenantSettingScopeType.Tenant => 100,
            RoutArrTenantSettingScopeType.Site => 200,
            RoutArrTenantSettingScopeType.Terminal => 300,
            RoutArrTenantSettingScopeType.Customer => 400,
            RoutArrTenantSettingScopeType.Carrier => 450,
            RoutArrTenantSettingScopeType.Lane => 500,
            RoutArrTenantSettingScopeType.RouteType => 600,
            RoutArrTenantSettingScopeType.ServiceType => 600,
            RoutArrTenantSettingScopeType.Demand => 700,
            RoutArrTenantSettingScopeType.Trip => 750,
            _ => 0,
        };
    }

    private static RoutArrTenantSettingOverrideResponse MapOverride(RoutArrTenantSettingOverride entity)
    {
        var definition = DefinitionFor(entity.SettingGroup, entity.SettingKey);
        return new RoutArrTenantSettingOverrideResponse(
            entity.PublicKey,
            entity.Version,
            MapScope(entity),
            definition.GroupKey,
            definition.SettingKey,
            RoutArrTenantSettingsDefinitions.ToKindKey(definition.ValueKind),
            ReadOverrideValue(entity, definition),
            entity.IsEmergencyOverride,
            entity.Reason,
            entity.UpdatedAt,
            entity.UpdatedByPersonId);
    }

    private static RoutArrSettingsScopeReference MapScope(RoutArrTenantSettingOverride entity) =>
        new(
            ToScopeKey(entity.ScopeType),
            entity.ScopeSourceProduct,
            entity.ScopeEntityType,
            entity.ScopeStableId,
            entity.ScopeDisplayLabelSnapshot,
            entity.ScopeStatusSnapshot,
            entity.ScopeSnapshotAt);

    private static string ScopeAuditRef(RoutArrSettingsScopeReference scope) =>
        $"{scope.SourceProduct}:{scope.EntityType}:{scope.StableId}";

    private static IReadOnlyList<string> SplitKeys(string changedKeys) =>
        changedKeys.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private void AddAudit(
        Guid tenantId,
        string action,
        string settingGroup,
        IReadOnlyList<string> changedKeys,
        string actorPersonId,
        int previousVersion,
        int newVersion,
        string summary,
        string? affectedScopeType = null,
        string? affectedScopeRef = null,
        string? previousSummary = null,
        string? newSummary = null)
    {
        db.RoutArrTenantSettingAuditEntries.Add(new RoutArrTenantSettingAuditEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PublicKey = $"routarr_set_audit_{Guid.NewGuid():N}",
            Action = action,
            SettingGroup = settingGroup,
            ChangedKeys = string.Join(",", changedKeys),
            ChangedByPersonId = actorPersonId,
            ChangedAt = DateTimeOffset.UtcNow,
            PreviousVersion = previousVersion,
            NewVersion = newVersion,
            AffectedScopeType = affectedScopeType,
            AffectedScopeRef = affectedScopeRef,
            Summary = summary,
            PreviousSummary = previousSummary,
            NewSummary = newSummary,
        });
    }

    private Task EnqueueSettingsEventAsync(
        Guid tenantId,
        Guid settingsId,
        string eventKind,
        string settingGroup,
        string actorPersonId,
        int previousVersion,
        int newVersion,
        IReadOnlyList<string> changedKeys,
        CancellationToken cancellationToken)
    {
        var summary = $"{eventKind}: {settingGroup}; changed by {actorPersonId}; version {previousVersion}->{newVersion}; keys {string.Join(", ", changedKeys)}";
        return outbox.TryEnqueueAsync(
            tenantId,
            eventKind,
            TenantSettingsTarget,
            settingsId,
            new RoutArrIntegrationOutboxPayload(
                tenantId,
                summary,
                TripId: null),
            idempotencySuffix: $"{newVersion}-{Guid.NewGuid():N}",
            cancellationToken: cancellationToken);
    }

    private Task EnqueueOverrideEventAsync(
        Guid tenantId,
        Guid settingsId,
        string eventKind,
        string settingGroup,
        string actorPersonId,
        int previousVersion,
        int newVersion,
        IReadOnlyList<string> changedKeys,
        RoutArrSettingsScopeReference scope,
        CancellationToken cancellationToken)
    {
        var summary = $"{eventKind}: {settingGroup}; {ScopeAuditRef(scope)}; changed by {actorPersonId}; version {previousVersion}->{newVersion}; keys {string.Join(", ", changedKeys)}";
        return outbox.TryEnqueueAsync(
            tenantId,
            eventKind,
            OverrideTarget,
            settingsId,
            new RoutArrIntegrationOutboxPayload(
                tenantId,
                summary,
                TripId: null),
            idempotencySuffix: $"{newVersion}-{Guid.NewGuid():N}",
            cancellationToken: cancellationToken);
    }

    private static bool ValuesEqual(object? left, object? right)
    {
        if (left is IReadOnlyList<string> leftList && right is IReadOnlyList<string> rightList)
        {
            return leftList.SequenceEqual(rightList, StringComparer.OrdinalIgnoreCase);
        }

        return string.Equals(Convert.ToString(left), Convert.ToString(right), StringComparison.OrdinalIgnoreCase);
    }

    private static bool? BoolValue(IReadOnlyDictionary<string, object?> values, string path) =>
        values.TryGetValue(path, out var value) && value is bool boolValue ? boolValue : null;

    private static string? TextValue(IReadOnlyDictionary<string, object?> values, string path) =>
        values.TryGetValue(path, out var value) ? Convert.ToString(value) : null;

    private static int? IntValue(IReadOnlyDictionary<string, object?> values, string path) =>
        values.TryGetValue(path, out var value) && int.TryParse(Convert.ToString(value), out var parsed)
            ? parsed
            : null;

    private static RoutArrSettingValidationIssue Error(string fieldPath, string message) =>
        new(fieldPath, message, "error");
}
