using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MaintainArrTenantSettingsService(MaintainArrDbContext db)
{
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public async Task<MaintainArrTenantSettingsResponse> GetOrCreateAsync(
        Guid tenantId,
        string actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.MaintainArrTenantSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            var now = DateTimeOffset.UtcNow;
            var defaults = MaintainArrTenantSettingsDefaults.Create();
            entity = new MaintainArrTenantSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SchemaVersion = CurrentSchemaVersion,
                SettingsJson = Serialize(defaults),
                CreatedAtUtc = now,
                CreatedByPersonId = NormalizePersonId(actorPersonId),
                UpdatedAtUtc = now,
                UpdatedByPersonId = NormalizePersonId(actorPersonId)
            };

            db.MaintainArrTenantSettings.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
        }

        return MapResponse(entity);
    }

    public async Task<MaintainArrTenantSettingsDto> LoadEffectiveSettingsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.MaintainArrTenantSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return entity is null ? MaintainArrTenantSettingsDefaults.Create() : Deserialize(entity.SettingsJson);
    }

    public async Task<MaintainArrTenantSettingsResponse> UpsertAsync(
        Guid tenantId,
        string actorPersonId,
        UpsertMaintainArrTenantSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Settings is null)
        {
            throw Validation("settings.required", "Settings payload is required.", "settings");
        }

        var after = MaintainArrTenantSettingsValidator.Normalize(request.Settings);
        var entity = await db.MaintainArrTenantSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var before = entity is null
            ? MaintainArrTenantSettingsDefaults.Create()
            : Deserialize(entity.SettingsJson);

        if (entity is null)
        {
            entity = new MaintainArrTenantSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAtUtc = now,
                CreatedByPersonId = NormalizePersonId(actorPersonId)
            };
            db.MaintainArrTenantSettings.Add(entity);
        }

        entity.SchemaVersion = CurrentSchemaVersion;
        entity.SettingsJson = Serialize(after);
        entity.UpdatedAtUtc = now;
        entity.UpdatedByPersonId = NormalizePersonId(actorPersonId);

        db.MaintainArrTenantSettingsAudit.Add(new MaintainArrTenantSettingsAudit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SettingsId = entity.Id,
            SchemaVersion = CurrentSchemaVersion,
            ChangedAtUtc = now,
            ChangedByPersonId = NormalizePersonId(actorPersonId),
            ChangeReason = NormalizeReason(request.ChangeReason),
            BeforeJson = Serialize(before),
            AfterJson = Serialize(after),
            DiffJson = SerializeDiff(BuildDiff(before, after))
        });

        await db.SaveChangesAsync(cancellationToken);
        return MapResponse(entity);
    }

    public async Task<MaintainArrTenantSettingsResponse> ResetToDefaultsAsync(
        Guid tenantId,
        string actorPersonId,
        ResetMaintainArrTenantSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var defaults = MaintainArrTenantSettingsDefaults.Create();
        var entity = await db.MaintainArrTenantSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var before = entity is null
            ? MaintainArrTenantSettingsDefaults.Create()
            : Deserialize(entity.SettingsJson);

        if (entity is null)
        {
            entity = new MaintainArrTenantSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAtUtc = now,
                CreatedByPersonId = NormalizePersonId(actorPersonId)
            };
            db.MaintainArrTenantSettings.Add(entity);
        }

        entity.SchemaVersion = CurrentSchemaVersion;
        entity.SettingsJson = Serialize(defaults);
        entity.UpdatedAtUtc = now;
        entity.UpdatedByPersonId = NormalizePersonId(actorPersonId);

        db.MaintainArrTenantSettingsAudit.Add(new MaintainArrTenantSettingsAudit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SettingsId = entity.Id,
            SchemaVersion = CurrentSchemaVersion,
            ChangedAtUtc = now,
            ChangedByPersonId = NormalizePersonId(actorPersonId),
            ChangeReason = NormalizeReason(request.ChangeReason) ?? "Reset to canonical MaintainArr defaults.",
            BeforeJson = Serialize(before),
            AfterJson = Serialize(defaults),
            DiffJson = SerializeDiff(BuildDiff(before, defaults))
        });

        await db.SaveChangesAsync(cancellationToken);
        return MapResponse(entity);
    }

    public async Task<MaintainArrTenantSettingsAuditResponse> ListAuditAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(limit ?? 25, 1, 100);
        var rows = await db.MaintainArrTenantSettingsAudit
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new MaintainArrTenantSettingsAuditResponse(rows
            .Select(x => new MaintainArrTenantSettingsAuditItem(
                x.ChangedAtUtc,
                x.ChangedByPersonId,
                x.ChangeReason,
                x.SchemaVersion,
                DeserializeDiff(x.DiffJson)))
            .ToList());
    }

    private MaintainArrTenantSettingsResponse MapResponse(MaintainArrTenantSettings entity) =>
        new(
            Deserialize(entity.SettingsJson),
            entity.CreatedAtUtc,
            entity.CreatedByPersonId,
            entity.UpdatedAtUtc,
            entity.UpdatedByPersonId);

    private static MaintainArrTenantSettingsDto Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return MaintainArrTenantSettingsDefaults.Create();
        }

        var dto = JsonSerializer.Deserialize<MaintainArrTenantSettingsDto>(json, JsonOptions)
            ?? MaintainArrTenantSettingsDefaults.Create();
        return MaintainArrTenantSettingsValidator.Normalize(dto);
    }

    private static string Serialize(MaintainArrTenantSettingsDto settings) =>
        JsonSerializer.Serialize(settings, JsonOptions);

    private static string SerializeDiff(IReadOnlyList<MaintainArrTenantSettingsAuditChange> diff) =>
        JsonSerializer.Serialize(diff, JsonOptions);

    private static IReadOnlyList<MaintainArrTenantSettingsAuditChange> DeserializeDiff(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<MaintainArrTenantSettingsAuditChange>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static IReadOnlyList<MaintainArrTenantSettingsAuditChange> BuildDiff(
        MaintainArrTenantSettingsDto before,
        MaintainArrTenantSettingsDto after)
    {
        using var beforeDoc = JsonDocument.Parse(Serialize(before));
        using var afterDoc = JsonDocument.Parse(Serialize(after));
        var beforeFlat = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var afterFlat = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        Flatten(beforeDoc.RootElement, string.Empty, beforeFlat);
        Flatten(afterDoc.RootElement, string.Empty, afterFlat);

        return beforeFlat.Keys
            .Union(afterFlat.Keys, StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Where(path => !string.Equals(
                beforeFlat.GetValueOrDefault(path),
                afterFlat.GetValueOrDefault(path),
                StringComparison.Ordinal))
            .Select(path => new MaintainArrTenantSettingsAuditChange(
                path,
                beforeFlat.GetValueOrDefault(path),
                afterFlat.GetValueOrDefault(path)))
            .ToList();
    }

    private static void Flatten(JsonElement element, string path, IDictionary<string, string?> values)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var childPath = string.IsNullOrWhiteSpace(path)
                        ? property.Name
                        : $"{path}.{property.Name}";
                    Flatten(property.Value, childPath, values);
                }

                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    Flatten(item, $"{path}[{index++}]", values);
                }

                break;
            case JsonValueKind.String:
                values[path] = element.GetString();
                break;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                values[path] = null;
                break;
            default:
                values[path] = element.GetRawText();
                break;
        }
    }

    private static string? NormalizePersonId(string? personId) =>
        string.IsNullOrWhiteSpace(personId) ? null : personId.Trim();

    private static string? NormalizeReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return null;
        }

        var trimmed = reason.Trim();
        return trimmed.Length <= 512 ? trimmed : trimmed[..512];
    }

    internal static StlApiException Validation(string code, string message, string field) =>
        new(code, message, 400, new { field });
}

public static class MaintainArrTenantSettingsDefaults
{
    public static MaintainArrTenantSettingsDto Create() =>
        new(
            MaintainArrTenantSettingsService.CurrentSchemaVersion,
            new MaintainArrOperatingSettingsDto("mixed", "controlled"),
            new MaintainArrAssetSettingsDto("auto", "AST", true, true, false, "active"),
            new MaintainArrWorkOrderSettingsDto("auto", "WO", "normal", true, true, false, false, true, true),
            new MaintainArrDefectSettingsDto(true, true, true, true, true, true, true, false, true),
            new MaintainArrOutOfServiceSettingsDto(true, true, true, false, true, true),
            new MaintainArrPreventiveMaintenanceSettingsDto(true, 14, 7, true, true, true),
            new MaintainArrInspectionSettingsDto(true, true, false, true, false),
            new MaintainArrLaborSettingsDto(true, false, true, "both", 5),
            new MaintainArrPartsSettingsDto(true, true, true, "request_only"),
            new MaintainArrSchedulingSettingsDto(true, 60, true, true, true, true, true),
            new MaintainArrEvidenceSettingsDto(true, true, false, true, false),
            new MaintainArrNotificationDefaultsDto(true, true, true, true, true, true, false, 7),
            new MaintainArrMobileSettingsDto(true, true, true, true, true, true),
            new MaintainArrComplianceSettingsDto(true, "warn", true, true, false),
            new MaintainArrIntegrationSettingsDto(true, true, true, true, true, true, false),
            new MaintainArrUiSettingsDto("dashboard", true, true, true, false));
}

public static class MaintainArrTenantSettingsValidator
{
    private static readonly string[] OperatingModes = ["fleet", "facility", "mixed"];
    private static readonly string[] StrictnessModes = ["advisory", "controlled", "strict"];
    private static readonly string[] NumberingModes = ["manual", "auto"];
    private static readonly string[] AssetStatuses = ["active", "pending_inspection", "out_of_service"];
    private static readonly string[] Priorities = ["low", "normal", "high", "urgent"];
    private static readonly string[] TimeEntryModes = ["manual", "timer", "both"];
    private static readonly string[] PartsReservationModes = ["none", "request_only", "reserve_on_assignment"];
    private static readonly string[] ComplianceModes = ["advisory", "warn", "block"];
    private static readonly string[] LandingPages = ["dashboard", "work_orders", "assets", "inspections"];
    private static readonly int[] LaborRoundMinutes = [1, 5, 6, 10, 15];

    public static MaintainArrTenantSettingsDto Normalize(MaintainArrTenantSettingsDto settings)
    {
        if (settings is null)
        {
            throw MaintainArrTenantSettingsService.Validation(
                "settings.required",
                "Settings payload is required.",
                "settings");
        }

        return settings with
        {
            SchemaVersion = MaintainArrTenantSettingsService.CurrentSchemaVersion,
            Operating = NormalizeOperating(settings.Operating),
            Assets = NormalizeAssets(settings.Assets),
            WorkOrders = NormalizeWorkOrders(settings.WorkOrders),
            Defects = Require(settings.Defects, "defects"),
            OutOfService = Require(settings.OutOfService, "outOfService"),
            PreventiveMaintenance = NormalizePreventiveMaintenance(settings.PreventiveMaintenance),
            Inspections = Require(settings.Inspections, "inspections"),
            Labor = NormalizeLabor(settings.Labor),
            Parts = NormalizeParts(settings.Parts),
            Scheduling = NormalizeScheduling(settings.Scheduling),
            Evidence = Require(settings.Evidence, "evidence"),
            Notifications = NormalizeNotifications(settings.Notifications),
            Mobile = Require(settings.Mobile, "mobile"),
            Compliance = NormalizeCompliance(settings.Compliance),
            Integrations = Require(settings.Integrations, "integrations"),
            Ui = NormalizeUi(settings.Ui)
        };
    }

    private static MaintainArrOperatingSettingsDto NormalizeOperating(MaintainArrOperatingSettingsDto value)
    {
        value = Require(value, "operating");
        return new MaintainArrOperatingSettingsDto(
            NormalizeEnum(value.MaintenanceOperatingMode, OperatingModes, "operating.maintenanceOperatingMode"),
            NormalizeEnum(value.MaintenanceStrictness, StrictnessModes, "operating.maintenanceStrictness"));
    }

    private static MaintainArrAssetSettingsDto NormalizeAssets(MaintainArrAssetSettingsDto value)
    {
        value = Require(value, "assets");
        return value with
        {
            AssetNumberingMode = NormalizeEnum(value.AssetNumberingMode, NumberingModes, "assets.assetNumberingMode"),
            AssetNumberPrefix = NormalizePrefix(value.AssetNumberPrefix, "assets.assetNumberPrefix"),
            DefaultAssetStatus = NormalizeEnum(value.DefaultAssetStatus, AssetStatuses, "assets.defaultAssetStatus")
        };
    }

    private static MaintainArrWorkOrderSettingsDto NormalizeWorkOrders(MaintainArrWorkOrderSettingsDto value)
    {
        value = Require(value, "workOrders");
        return value with
        {
            WorkOrderNumberingMode = NormalizeEnum(value.WorkOrderNumberingMode, NumberingModes, "workOrders.workOrderNumberingMode"),
            WorkOrderNumberPrefix = NormalizePrefix(value.WorkOrderNumberPrefix, "workOrders.workOrderNumberPrefix"),
            DefaultPriority = NormalizeEnum(value.DefaultPriority, Priorities, "workOrders.defaultPriority")
        };
    }

    private static MaintainArrPreventiveMaintenanceSettingsDto NormalizePreventiveMaintenance(
        MaintainArrPreventiveMaintenanceSettingsDto value)
    {
        value = Require(value, "preventiveMaintenance");
        return value with
        {
            PmGenerateDaysAhead = NormalizeRange(
                value.PmGenerateDaysAhead,
                0,
                365,
                "preventiveMaintenance.pmGenerateDaysAhead"),
            PmGracePeriodDays = NormalizeRange(
                value.PmGracePeriodDays,
                0,
                365,
                "preventiveMaintenance.pmGracePeriodDays")
        };
    }

    private static MaintainArrLaborSettingsDto NormalizeLabor(MaintainArrLaborSettingsDto value)
    {
        value = Require(value, "labor");
        if (!LaborRoundMinutes.Contains(value.RoundLaborMinutesTo))
        {
            throw MaintainArrTenantSettingsService.Validation(
                "settings.invalid_labor_rounding",
                "roundLaborMinutesTo must be 1, 5, 6, 10, or 15.",
                "labor.roundLaborMinutesTo");
        }

        return value with
        {
            LaborTimeEntryMode = NormalizeEnum(value.LaborTimeEntryMode, TimeEntryModes, "labor.laborTimeEntryMode")
        };
    }

    private static MaintainArrPartsSettingsDto NormalizeParts(MaintainArrPartsSettingsDto value)
    {
        value = Require(value, "parts");
        return value with
        {
            PartsReservationMode = NormalizeEnum(value.PartsReservationMode, PartsReservationModes, "parts.partsReservationMode")
        };
    }

    private static MaintainArrSchedulingSettingsDto NormalizeScheduling(MaintainArrSchedulingSettingsDto value)
    {
        value = Require(value, "scheduling");
        return value with
        {
            DefaultScheduleDurationMinutes = NormalizeRange(
                value.DefaultScheduleDurationMinutes,
                5,
                1440,
                "scheduling.defaultScheduleDurationMinutes")
        };
    }

    private static MaintainArrNotificationDefaultsDto NormalizeNotifications(MaintainArrNotificationDefaultsDto value)
    {
        value = Require(value, "notifications");
        return value with
        {
            PmDueNotificationDaysAhead = NormalizeRange(
                value.PmDueNotificationDaysAhead,
                0,
                365,
                "notifications.pmDueNotificationDaysAhead")
        };
    }

    private static MaintainArrComplianceSettingsDto NormalizeCompliance(MaintainArrComplianceSettingsDto value)
    {
        value = Require(value, "compliance");
        return value with
        {
            ComplianceCheckMode = NormalizeEnum(value.ComplianceCheckMode, ComplianceModes, "compliance.complianceCheckMode")
        };
    }

    private static MaintainArrUiSettingsDto NormalizeUi(MaintainArrUiSettingsDto value)
    {
        value = Require(value, "ui");
        return value with
        {
            DefaultLandingPage = NormalizeEnum(value.DefaultLandingPage, LandingPages, "ui.defaultLandingPage")
        };
    }

    private static T Require<T>(T? value, string field)
        where T : class
    {
        if (value is null)
        {
            throw MaintainArrTenantSettingsService.Validation(
                "settings.section_required",
                $"{field} settings are required.",
                field);
        }

        return value;
    }

    private static int NormalizeRange(int value, int min, int max, string field)
    {
        if (value < min || value > max)
        {
            throw MaintainArrTenantSettingsService.Validation(
                "settings.numeric_out_of_range",
                $"{field} must be between {min} and {max}.",
                field);
        }

        return value;
    }

    private static string NormalizeEnum(string? value, IReadOnlyCollection<string> allowed, string field)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || !allowed.Contains(normalized))
        {
            throw MaintainArrTenantSettingsService.Validation(
                "settings.invalid_enum",
                $"{field} is invalid.",
                field);
        }

        return normalized;
    }

    private static string? NormalizePrefix(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 16)
        {
            throw MaintainArrTenantSettingsService.Validation(
                "settings.prefix_too_long",
                $"{field} must be 16 characters or fewer.",
                field);
        }

        if (normalized.Any(c => !char.IsLetterOrDigit(c) && c is not '-' and not '_'))
        {
            throw MaintainArrTenantSettingsService.Validation(
                "settings.prefix_invalid",
                $"{field} may contain only letters, numbers, hyphen, or underscore.",
                field);
        }

        return normalized;
    }
}
