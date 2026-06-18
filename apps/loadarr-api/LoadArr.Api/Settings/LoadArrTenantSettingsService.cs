using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;

namespace LoadArr.Api.Settings;

public sealed class LoadArrTenantSettingsService(
    LoadArr.Api.Data.LoadArrDbContext db,
    LoadArrTenantSettingsDefaults defaults,
    LoadArrTenantSettingsValidator validator)
{
    public const string FullResetConfirmationPhrase = "RESET LOADARR TENANT SETTINGS";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<LoadArrTenantSettingsResponse> GetCurrentAsync(
        Guid tenantId,
        ClaimsPrincipal actor,
        CancellationToken cancellationToken = default)
    {
        var entity = await EnsureCurrentEntityAsync(tenantId, actor, cancellationToken);
        return MapResponse(entity);
    }

    public async Task<LoadArrTenantSettingsResponse> ReplaceAsync(
        Guid tenantId,
        ClaimsPrincipal actor,
        LoadArrTenantSettingsReplaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await EnsureCurrentEntityAsync(tenantId, actor, cancellationToken);
        RequireRowVersion(entity, request.RowVersion);

        var validation = validator.Validate(request.Settings);
        EnsureSavable(validation, request.WarningsAcknowledged);

        var before = Deserialize(entity.SettingsJson);
        ApplySettingsChange(
            entity,
            before,
            request.Settings,
            LoadArrTenantSettingsSectionKeys.All,
            actor,
            request.Reason,
            LoadArrTenantSettingChangeSources.Api,
            request.WarningsAcknowledged ?? []);

        await db.SaveChangesAsync(cancellationToken);
        return MapResponse(entity);
    }

    public async Task<LoadArrTenantSettingsResponse> PatchSectionAsync(
        Guid tenantId,
        ClaimsPrincipal actor,
        string sectionKey,
        LoadArrTenantSettingsSectionPatchRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedSectionKey = NormalizeSectionKey(sectionKey);
        var entity = await EnsureCurrentEntityAsync(tenantId, actor, cancellationToken);
        RequireRowVersion(entity, request.RowVersion);

        var before = Deserialize(entity.SettingsJson);
        var after = ReplaceSection(before, normalizedSectionKey, request.Section);
        var validation = validator.Validate(after);
        EnsureSavable(validation, request.WarningsAcknowledged);

        ApplySettingsChange(
            entity,
            before,
            after,
            normalizedSectionKey,
            actor,
            request.Reason,
            LoadArrTenantSettingChangeSources.Api,
            request.WarningsAcknowledged ?? []);

        await db.SaveChangesAsync(cancellationToken);
        return MapResponse(entity);
    }

    public async Task<LoadArrTenantSettingsResponse> ResetSectionAsync(
        Guid tenantId,
        ClaimsPrincipal actor,
        string sectionKey,
        LoadArrTenantSettingsResetRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedSectionKey = NormalizeSectionKey(sectionKey);
        var entity = await EnsureCurrentEntityAsync(tenantId, actor, cancellationToken);
        RequireRowVersion(entity, request.RowVersion);

        var before = Deserialize(entity.SettingsJson);
        var defaultSettings = defaults.CreateDefaultSettings();
        var after = ResetSection(before, defaultSettings, normalizedSectionKey);
        var validation = validator.Validate(after);
        EnsureSavable(validation, request.WarningsAcknowledged);

        ApplySettingsChange(
            entity,
            before,
            after,
            normalizedSectionKey,
            actor,
            request.Reason,
            LoadArrTenantSettingChangeSources.Api,
            request.WarningsAcknowledged ?? []);

        await db.SaveChangesAsync(cancellationToken);
        return MapResponse(entity);
    }

    public async Task<LoadArrTenantSettingsResponse> ResetAllAsync(
        Guid tenantId,
        ClaimsPrincipal actor,
        LoadArrTenantSettingsFullResetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(request.ConfirmationPhrase, FullResetConfirmationPhrase, StringComparison.Ordinal))
        {
            throw new LoadArrTenantSettingsRequestException(
                "loadarr.settings.reset.confirmation_required",
                $"Full reset requires confirmation phrase '{FullResetConfirmationPhrase}'.",
                400);
        }

        var entity = await EnsureCurrentEntityAsync(tenantId, actor, cancellationToken);
        RequireRowVersion(entity, request.RowVersion);

        var before = Deserialize(entity.SettingsJson);
        var after = defaults.CreateDefaultSettings();
        var validation = validator.Validate(after);
        EnsureSavable(validation, request.WarningsAcknowledged);

        ApplySettingsChange(
            entity,
            before,
            after,
            LoadArrTenantSettingsSectionKeys.All,
            actor,
            request.Reason,
            LoadArrTenantSettingChangeSources.Api,
            request.WarningsAcknowledged ?? []);

        await db.SaveChangesAsync(cancellationToken);
        return MapResponse(entity);
    }

    public async Task<LoadArrTenantSettingsAuditListResponse> ListAuditAsync(
        Guid tenantId,
        int? limit,
        int? offset,
        CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit ?? 50, 1, 200);
        var skip = Math.Max(offset ?? 0, 0);

        var query = db.LoadArrTenantSettingAuditEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(skip)
            .Take(take)
            .Select(x => new LoadArrTenantSettingsAuditEntryResponse(
                x.SettingsVersionBefore,
                x.SettingsVersionAfter,
                x.SectionKey,
                x.ChangedByPersonId,
                x.ChangedByDisplayNameSnapshot,
                x.ChangedAt,
                x.Reason,
                x.ChangeSource,
                DeserializeStringList(x.ChangedFieldsJson),
                DeserializeStringList(x.WarningsAcknowledgedJson),
                DeserializeSummary(x.BeforeSummaryJson),
                DeserializeSummary(x.AfterSummaryJson)))
            .ToListAsync(cancellationToken);

        return new LoadArrTenantSettingsAuditListResponse(items, total, take, skip);
    }

    public async Task<LoadArrTenantSettingsExportResponse> ExportAsync(
        Guid tenantId,
        ClaimsPrincipal actor,
        CancellationToken cancellationToken = default)
    {
        var current = await GetCurrentAsync(tenantId, actor, cancellationToken);
        var audit = await ListAuditAsync(tenantId, 100, 0, cancellationToken);
        return new LoadArrTenantSettingsExportResponse(
            current.Version,
            DateTimeOffset.UtcNow,
            current.Settings,
            audit.Items);
    }

    public LoadArrTenantSettingsOptionsResponse GetOptions() => defaults.CreateOptions();

    public LoadArrTenantSettingsValidationResult Validate(LoadArrTenantSettingsSections settings) =>
        validator.Validate(settings);

    private async Task<LoadArrTenantSettings> EnsureCurrentEntityAsync(
        Guid tenantId,
        ClaimsPrincipal actor,
        CancellationToken cancellationToken)
    {
        var entity = await db.LoadArrTenantSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IsActive, cancellationToken);

        if (entity is not null)
        {
            return entity;
        }

        var now = DateTimeOffset.UtcNow;
        var actorPersonId = ResolveActorPersonId(actor);
        var actorDisplayName = ResolveActorDisplayName(actor);
        var settings = defaults.CreateDefaultSettings();
        var settingsJson = Serialize(settings);

        entity = new LoadArrTenantSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Version = 1,
            IsActive = true,
            SettingsJson = settingsJson,
            NormalizedSnapshotJson = settingsJson,
            RowVersion = NewRowVersion(),
            CreatedAt = now,
            CreatedByPersonId = actorPersonId,
            UpdatedAt = now,
            UpdatedByPersonId = actorPersonId,
            UpdatedByDisplayNameSnapshot = actorDisplayName
        };

        db.LoadArrTenantSettings.Add(entity);
        db.LoadArrTenantSettingAuditEntries.Add(new LoadArrTenantSettingAuditEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SettingsId = entity.Id,
            SettingsVersionBefore = 0,
            SettingsVersionAfter = 1,
            SectionKey = LoadArrTenantSettingsSectionKeys.All,
            ChangedByPersonId = actorPersonId,
            ChangedByDisplayNameSnapshot = actorDisplayName,
            ChangedAt = now,
            ChangeSource = LoadArrTenantSettingChangeSources.Seed,
            BeforeSummaryJson = SerializeSummary("Settings did not exist."),
            AfterSummaryJson = SerializeSummary(BuildSummary(LoadArrTenantSettingsSectionKeys.All, settings)),
            ChangedFieldsJson = SerializeStringList(Flatten(settings).Keys.OrderBy(x => x, StringComparer.Ordinal).ToList()),
            WarningsAcknowledgedJson = "[]"
        });

        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    private void ApplySettingsChange(
        LoadArrTenantSettings entity,
        LoadArrTenantSettingsSections before,
        LoadArrTenantSettingsSections after,
        string sectionKey,
        ClaimsPrincipal actor,
        string? reason,
        string changeSource,
        IReadOnlyList<string> warningsAcknowledged)
    {
        var now = DateTimeOffset.UtcNow;
        var beforeVersion = entity.Version;
        var afterJson = Serialize(after);
        var changedFields = ComputeChangedFields(before, after, sectionKey);

        entity.Version++;
        entity.SettingsJson = afterJson;
        entity.NormalizedSnapshotJson = afterJson;
        entity.RowVersion = NewRowVersion();
        entity.UpdatedAt = now;
        entity.UpdatedByPersonId = ResolveActorPersonId(actor);
        entity.UpdatedByDisplayNameSnapshot = ResolveActorDisplayName(actor);

        db.LoadArrTenantSettingAuditEntries.Add(new LoadArrTenantSettingAuditEntry
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            SettingsId = entity.Id,
            SettingsVersionBefore = beforeVersion,
            SettingsVersionAfter = entity.Version,
            SectionKey = sectionKey,
            ChangedByPersonId = entity.UpdatedByPersonId,
            ChangedByDisplayNameSnapshot = entity.UpdatedByDisplayNameSnapshot,
            ChangedAt = now,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            ChangeSource = changeSource,
            BeforeSummaryJson = SerializeSummary(BuildSummary(sectionKey, before)),
            AfterSummaryJson = SerializeSummary(BuildSummary(sectionKey, after)),
            ChangedFieldsJson = SerializeStringList(changedFields),
            WarningsAcknowledgedJson = SerializeStringList(warningsAcknowledged)
        });
    }

    private LoadArrTenantSettingsResponse MapResponse(LoadArrTenantSettings entity)
    {
        var settings = Deserialize(entity.SettingsJson);
        return new LoadArrTenantSettingsResponse(
            entity.Version,
            entity.RowVersion,
            entity.CreatedAt,
            entity.CreatedByPersonId,
            entity.UpdatedAt,
            entity.UpdatedByPersonId,
            entity.UpdatedByDisplayNameSnapshot,
            settings,
            validator.Validate(settings));
    }

    private static LoadArrTenantSettingsSections ReplaceSection(
        LoadArrTenantSettingsSections current,
        string sectionKey,
        JsonElement section) =>
        sectionKey switch
        {
            LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel => current with { WarehouseOperatingModel = DeserializeSection<WarehouseOperatingModelSettings>(section) },
            LoadArrTenantSettingsSectionKeys.Receiving => current with { Receiving = DeserializeSection<ReceivingPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.DockAppointments => current with { DockAppointments = DeserializeSection<DockAppointmentPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.Putaway => current with { Putaway = DeserializeSection<PutawayPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.InventoryControl => current with { InventoryControl = DeserializeSection<InventoryControlPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.Traceability => current with { Traceability = DeserializeSection<TraceabilityPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.Movement => current with { Movement = DeserializeSection<MovementPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.Exceptions => current with { Exceptions = DeserializeSection<ExceptionHandoffPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.Compliance => current with { Compliance = DeserializeSection<ComplianceEnforcementPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.TaskAssignment => current with { TaskAssignment = DeserializeSection<TaskAssignmentPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.MobileScanner => current with { MobileScanner = DeserializeSection<MobileScannerPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.LabelingAndDocuments => current with { LabelingAndDocuments = DeserializeSection<LabelingAndDocumentPolicySettings>(section) },
            LoadArrTenantSettingsSectionKeys.NotificationsAndEvents => current with { NotificationsAndEvents = DeserializeSection<NotificationAndEventPolicySettings>(section) },
            _ => throw UnknownSection(sectionKey)
        };

    private static LoadArrTenantSettingsSections ResetSection(
        LoadArrTenantSettingsSections current,
        LoadArrTenantSettingsSections defaults,
        string sectionKey) =>
        sectionKey switch
        {
            LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel => current with { WarehouseOperatingModel = defaults.WarehouseOperatingModel },
            LoadArrTenantSettingsSectionKeys.Receiving => current with { Receiving = defaults.Receiving },
            LoadArrTenantSettingsSectionKeys.DockAppointments => current with { DockAppointments = defaults.DockAppointments },
            LoadArrTenantSettingsSectionKeys.Putaway => current with { Putaway = defaults.Putaway },
            LoadArrTenantSettingsSectionKeys.InventoryControl => current with { InventoryControl = defaults.InventoryControl },
            LoadArrTenantSettingsSectionKeys.Traceability => current with { Traceability = defaults.Traceability },
            LoadArrTenantSettingsSectionKeys.Movement => current with { Movement = defaults.Movement },
            LoadArrTenantSettingsSectionKeys.Exceptions => current with { Exceptions = defaults.Exceptions },
            LoadArrTenantSettingsSectionKeys.Compliance => current with { Compliance = defaults.Compliance },
            LoadArrTenantSettingsSectionKeys.TaskAssignment => current with { TaskAssignment = defaults.TaskAssignment },
            LoadArrTenantSettingsSectionKeys.MobileScanner => current with { MobileScanner = defaults.MobileScanner },
            LoadArrTenantSettingsSectionKeys.LabelingAndDocuments => current with { LabelingAndDocuments = defaults.LabelingAndDocuments },
            LoadArrTenantSettingsSectionKeys.NotificationsAndEvents => current with { NotificationsAndEvents = defaults.NotificationsAndEvents },
            _ => throw UnknownSection(sectionKey)
        };

    private static string NormalizeSectionKey(string sectionKey)
    {
        var normalized = sectionKey.Trim();
        return normalized.ToLowerInvariant() switch
        {
            "warehouse" or "warehouseoperatingmodel" or "warehouse-operating-model" => LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel,
            "receiving" => LoadArrTenantSettingsSectionKeys.Receiving,
            "dock" or "dockappointments" or "dock-appointments" or "appointments" => LoadArrTenantSettingsSectionKeys.DockAppointments,
            "putaway" => LoadArrTenantSettingsSectionKeys.Putaway,
            "inventory" or "inventorycontrol" or "inventory-control" => LoadArrTenantSettingsSectionKeys.InventoryControl,
            "traceability" => LoadArrTenantSettingsSectionKeys.Traceability,
            "movement" => LoadArrTenantSettingsSectionKeys.Movement,
            "exceptions" or "exceptionhandoff" or "exception-handoff" => LoadArrTenantSettingsSectionKeys.Exceptions,
            "compliance" => LoadArrTenantSettingsSectionKeys.Compliance,
            "tasks" or "taskassignment" or "task-assignment" => LoadArrTenantSettingsSectionKeys.TaskAssignment,
            "mobile" or "scanner" or "mobilescanner" or "mobile-scanner" => LoadArrTenantSettingsSectionKeys.MobileScanner,
            "documents" or "labels" or "labelinganddocuments" or "labeling-and-documents" => LoadArrTenantSettingsSectionKeys.LabelingAndDocuments,
            "events" or "notifications" or "notificationsandevents" or "notifications-and-events" => LoadArrTenantSettingsSectionKeys.NotificationsAndEvents,
            _ when LoadArrTenantSettingsSectionKeys.AllSectionKeys.Contains(normalized, StringComparer.OrdinalIgnoreCase) => LoadArrTenantSettingsSectionKeys.AllSectionKeys.Single(x => string.Equals(x, normalized, StringComparison.OrdinalIgnoreCase)),
            _ => throw UnknownSection(sectionKey)
        };
    }

    private void EnsureSavable(
        LoadArrTenantSettingsValidationResult validation,
        IReadOnlyList<string>? warningsAcknowledged)
    {
        if (validation.Errors.Count > 0)
        {
            throw new LoadArrTenantSettingsValidationException(
                "loadarr.settings.validation_failed",
                "LoadArr tenant settings contain blocking validation errors.",
                validation,
                400);
        }

        var missingWarnings = validator.GetUnacknowledgedWarningCodes(validation, warningsAcknowledged);
        if (missingWarnings.Count > 0)
        {
            var missingValidation = validation with
            {
                Errors =
                [
                    new LoadArrTenantSettingsValidationMessage(
                        "loadarr.settings.warnings_not_acknowledged",
                        LoadArrTenantSettingsSectionKeys.All,
                        "warningsAcknowledged",
                        $"Acknowledge warning code(s) before saving: {string.Join(", ", missingWarnings)}.",
                        "error")
                ]
            };

            throw new LoadArrTenantSettingsValidationException(
                "loadarr.settings.warnings_not_acknowledged",
                "Risk warnings must be acknowledged before saving LoadArr tenant settings.",
                missingValidation,
                409);
        }
    }

    private static void RequireRowVersion(LoadArrTenantSettings entity, string rowVersion)
    {
        if (string.IsNullOrWhiteSpace(rowVersion) ||
            !string.Equals(entity.RowVersion, rowVersion, StringComparison.Ordinal))
        {
            throw new LoadArrTenantSettingsRequestException(
                "loadarr.settings.concurrency_conflict",
                "LoadArr tenant settings were updated by another request. Reload the settings and try again.",
                409);
        }
    }

    private static string ResolveActorPersonId(ClaimsPrincipal actor)
    {
        try
        {
            return actor.GetPersonId().ToString();
        }
        catch (InvalidOperationException)
        {
            return "system";
        }
    }

    private static string? ResolveActorDisplayName(ClaimsPrincipal actor) =>
        actor.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name)?.Value
        ?? actor.Identity?.Name;

    private static T DeserializeSection<T>(JsonElement section)
    {
        try
        {
            return section.Deserialize<T>(JsonOptions)
                ?? throw new JsonException("Section payload was empty.");
        }
        catch (JsonException ex)
        {
            throw new LoadArrTenantSettingsRequestException(
                "loadarr.settings.invalid_section_payload",
                $"Section payload is invalid: {ex.Message}",
                400);
        }
    }

    private static LoadArrTenantSettingsSections Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new LoadArrTenantSettingsSections();
        }

        return JsonSerializer.Deserialize<LoadArrTenantSettingsSections>(json, JsonOptions)
            ?? new LoadArrTenantSettingsSections();
    }

    private static string Serialize(LoadArrTenantSettingsSections settings) =>
        JsonSerializer.Serialize(settings, JsonOptions);

    private static string NewRowVersion() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();

    private static IReadOnlyList<string> ComputeChangedFields(
        LoadArrTenantSettingsSections before,
        LoadArrTenantSettingsSections after,
        string sectionKey)
    {
        var beforeFlat = Flatten(SelectSection(before, sectionKey));
        var afterFlat = Flatten(SelectSection(after, sectionKey));
        return beforeFlat.Keys
            .Concat(afterFlat.Keys)
            .Distinct(StringComparer.Ordinal)
            .Where(key => !beforeFlat.TryGetValue(key, out var beforeValue) ||
                !afterFlat.TryGetValue(key, out var afterValue) ||
                !string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToList();
    }

    private static object SelectSection(LoadArrTenantSettingsSections settings, string sectionKey) =>
        sectionKey switch
        {
            LoadArrTenantSettingsSectionKeys.WarehouseOperatingModel => settings.WarehouseOperatingModel,
            LoadArrTenantSettingsSectionKeys.Receiving => settings.Receiving,
            LoadArrTenantSettingsSectionKeys.DockAppointments => settings.DockAppointments,
            LoadArrTenantSettingsSectionKeys.Putaway => settings.Putaway,
            LoadArrTenantSettingsSectionKeys.InventoryControl => settings.InventoryControl,
            LoadArrTenantSettingsSectionKeys.Traceability => settings.Traceability,
            LoadArrTenantSettingsSectionKeys.Movement => settings.Movement,
            LoadArrTenantSettingsSectionKeys.Exceptions => settings.Exceptions,
            LoadArrTenantSettingsSectionKeys.Compliance => settings.Compliance,
            LoadArrTenantSettingsSectionKeys.TaskAssignment => settings.TaskAssignment,
            LoadArrTenantSettingsSectionKeys.MobileScanner => settings.MobileScanner,
            LoadArrTenantSettingsSectionKeys.LabelingAndDocuments => settings.LabelingAndDocuments,
            LoadArrTenantSettingsSectionKeys.NotificationsAndEvents => settings.NotificationsAndEvents,
            LoadArrTenantSettingsSectionKeys.All => settings,
            _ => throw UnknownSection(sectionKey)
        };

    private static Dictionary<string, string?> Flatten(object value)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(value, JsonOptions));
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);
        FlattenElement(document.RootElement, string.Empty, result);
        return result;
    }

    private static void FlattenElement(JsonElement element, string path, Dictionary<string, string?> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var childPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
                    FlattenElement(property.Value, childPath, result);
                }

                break;
            case JsonValueKind.Array:
                result[path] = element.GetRawText();
                break;
            case JsonValueKind.String:
                result[path] = element.GetString();
                break;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                result[path] = element.GetRawText();
                break;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                result[path] = null;
                break;
        }
    }

    private static string BuildSummary(string sectionKey, LoadArrTenantSettingsSections settings)
    {
        var flat = Flatten(SelectSection(settings, sectionKey));
        var preview = flat
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Take(10)
            .Select(x => $"{x.Key}={x.Value ?? "null"}");
        return $"{sectionKey}: {string.Join("; ", preview)}";
    }

    private static string SerializeSummary(string summary) =>
        JsonSerializer.Serialize(summary, JsonOptions);

    private static string DeserializeSummary(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string>(json, JsonOptions) ?? string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static string SerializeStringList(IReadOnlyList<string> values) =>
        JsonSerializer.Serialize(values, JsonOptions);

    private static IReadOnlyList<string> DeserializeStringList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<string>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static LoadArrTenantSettingsRequestException UnknownSection(string sectionKey) =>
        new(
            "loadarr.settings.unknown_section",
            $"Unknown LoadArr tenant settings section '{sectionKey}'.",
            404);
}

public class LoadArrTenantSettingsRequestException(string code, string message, int statusCode) : Exception(message)
{
    public string Code { get; } = code;

    public int StatusCode { get; } = statusCode;
}

public sealed class LoadArrTenantSettingsValidationException(
    string code,
    string message,
    LoadArrTenantSettingsValidationResult validation,
    int statusCode)
    : LoadArrTenantSettingsRequestException(code, message, statusCode)
{
    public LoadArrTenantSettingsValidationResult Validation { get; } = validation;
}
