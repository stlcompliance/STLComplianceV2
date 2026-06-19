using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class TimekeepingService(StaffArrDbContext db, IStaffArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public const string PayrollReadySnapshotReadActionScope = "staffarr.timekeeping.payroll_ready_snapshot.read";
    public const string LaborEvidenceWriteActionScope = "staffarr.timekeeping.labor_evidence.write";

    public async Task<IReadOnlyList<TimekeepingProfileResponse>> ListProfilesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await db.TimekeepingProfiles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.WorkerNumber)
            .Select(x => MapProfile(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TimekeepingProfileResponse> GetProfileAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        var profile = await db.TimekeepingProfiles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .Select(x => MapProfile(x))
            .FirstOrDefaultAsync(cancellationToken);

        return profile ?? throw new StlApiException("staffarr.timekeeping.profile_not_found", "Timekeeping profile was not found.", 404);
    }

    public async Task<TimekeepingProfileResponse> UpsertProfileAsync(Guid tenantId, Guid? actorUserId, UpsertTimekeepingProfileRequest request, CancellationToken cancellationToken)
    {
        _ = await db.People.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.PersonId, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.person_not_found", "Worker person record was not found.", 404);

        var entity = await db.TimekeepingProfiles.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PersonId == request.PersonId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TimekeepingProfile
            {
                TenantId = tenantId,
                PersonId = request.PersonId,
                CreatedAt = now,
            };
            db.TimekeepingProfiles.Add(entity);
        }

        entity.WorkerNumber = Require(request.WorkerNumber, "Worker number is required.", 64);
        entity.DefaultLegalEntityRef = Optional(request.DefaultLegalEntityRef, 256);
        entity.DefaultSiteRef = Optional(request.DefaultSiteRef, 256);
        entity.DefaultDepartmentRef = Optional(request.DefaultDepartmentRef, 256);
        entity.DefaultPositionRef = Optional(request.DefaultPositionRef, 256);
        entity.DefaultSupervisorPersonId = request.DefaultSupervisorPersonId;
        entity.PayPolicyId = request.PayPolicyId;
        entity.PayrollEligibilityStatus = NormalizeEnum(request.PayrollEligibilityStatus, ["eligible", "ineligible", "pending"], "Payroll eligibility status");
        entity.TimeEntryMode = NormalizeEnum(request.TimeEntryMode, ["clock", "manual", "hybrid", "exempt-summary"], "Time entry mode");
        entity.OvertimeEligible = request.OvertimeEligible;
        entity.RequiresMealBreakAttestation = request.RequiresMealBreakAttestation;
        entity.RequiresEndOfShiftAttestation = request.RequiresEndOfShiftAttestation;
        entity.AllowMobileClock = request.AllowMobileClock;
        entity.AllowKioskClock = request.AllowKioskClock;
        entity.AllowManualCorrections = request.AllowManualCorrections;
        entity.DefaultLaborAllocationTemplateId = request.DefaultLaborAllocationTemplateId;
        entity.EffectiveStartDate = request.EffectiveStartDate;
        entity.EffectiveEndDate = request.EffectiveEndDate;
        entity.Status = NormalizeEnum(request.Status, ["active", "inactive", "pending"], "Profile status");
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.profile.upsert", tenantId, actorUserId, "timekeeping_profile", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return await GetProfileAsync(tenantId, entity.PersonId, cancellationToken);
    }

    public async Task<IReadOnlyList<PayPolicyResponse>> ListPayPoliciesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await db.TimekeepingPayPolicies.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.Name).Select(x => MapPayPolicy(x)).ToListAsync(cancellationToken);
    }

    public async Task<PayPolicyResponse> UpsertPayPolicyAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertPayPolicyRequest request, CancellationToken cancellationToken)
    {
        var entity = id.HasValue
            ? await db.TimekeepingPayPolicies.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;

        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.timekeeping.pay_policy_not_found", "Pay policy was not found.", 404);
        }

        entity ??= new PayPolicy { TenantId = tenantId };
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        entity.Name = Require(request.Name, "Pay policy name is required.", 128);
        entity.Description = Optional(request.Description, 1024);
        entity.JurisdictionRefs = Require(request.JurisdictionRefs, "Jurisdiction references are required.", 4000);
        entity.RoundingPolicy = Optional(request.RoundingPolicy, 1024);
        entity.MealBreakPolicy = Optional(request.MealBreakPolicy, 1024);
        entity.RestBreakPolicy = Optional(request.RestBreakPolicy, 1024);
        entity.OvertimePolicy = Optional(request.OvertimePolicy, 1024);
        entity.DoubleTimePolicy = Optional(request.DoubleTimePolicy, 1024);
        entity.HolidayPolicy = Optional(request.HolidayPolicy, 1024);
        entity.ShiftDifferentialPolicy = Optional(request.ShiftDifferentialPolicy, 1024);
        entity.TravelTimePolicy = Optional(request.TravelTimePolicy, 1024);
        entity.StandbyCalloutPolicy = Optional(request.StandbyCalloutPolicy, 1024);
        entity.ApprovalPolicy = Optional(request.ApprovalPolicy, 1024);
        entity.CorrectionPolicy = Optional(request.CorrectionPolicy, 1024);
        entity.AttestationPolicy = Optional(request.AttestationPolicy, 1024);
        entity.ComplianceRulepackRefs = Optional(request.ComplianceRulepackRefs, 2048);
        entity.EffectiveStartDate = request.EffectiveStartDate;
        entity.EffectiveEndDate = request.EffectiveEndDate;
        entity.Status = NormalizeEnum(request.Status, ["active", "inactive", "draft"], "Pay policy status");

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.TimekeepingPayPolicies.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.pay_policy.upsert", tenantId, actorUserId, "pay_policy", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapPayPolicy(entity);
    }

    public async Task<IReadOnlyList<PayCodeResponse>> ListPayCodesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await db.TimekeepingPayCodes.AsNoTracking().Where(x => x.TenantId == tenantId).OrderBy(x => x.Code).Select(x => MapPayCode(x)).ToListAsync(cancellationToken);
    }

    public async Task<PayCodeResponse> UpsertPayCodeAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertPayCodeRequest request, CancellationToken cancellationToken)
    {
        var entity = id.HasValue
            ? await db.TimekeepingPayCodes.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;

        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.timekeeping.pay_code_not_found", "Pay code was not found.", 404);
        }

        entity ??= new PayCode { TenantId = tenantId };
        entity.Code = Require(request.Code, "Pay code is required.", 64).ToUpperInvariant();
        entity.DisplayName = Require(request.DisplayName, "Pay code display name is required.", 128);
        entity.Category = NormalizeEnum(request.Category, ["worked", "nonworked", "paid_leave", "unpaid_leave", "premium", "adjustment", "correction"], "Pay code category");
        entity.CountsTowardWorkedHours = request.CountsTowardWorkedHours;
        entity.CountsTowardOvertimeBase = request.CountsTowardOvertimeBase;
        entity.RequiresAllocation = request.RequiresAllocation;
        entity.RequiresApproval = request.RequiresApproval;
        entity.RequiresReason = request.RequiresReason;
        entity.Active = request.Active;
        entity.EffectiveStartDate = request.EffectiveStartDate;
        entity.EffectiveEndDate = request.EffectiveEndDate;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.TimekeepingPayCodes.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.pay_code.upsert", tenantId, actorUserId, "pay_code", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapPayCode(entity);
    }

    public async Task<ClockEventResponse> CreateClockEventAsync(Guid tenantId, Guid? actorUserId, CreateClockEventRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var anomalyFlags = new List<string>();
        if (request.EventTimestamp > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            anomalyFlags.Add("future_timestamp");
        }

        var entity = new ClockEvent
        {
            TenantId = tenantId,
            PersonId = request.PersonId,
            SourceProductKey = Require(request.SourceProductKey, "Source product key is required.", 64).ToLowerInvariant(),
            SourceDeviceType = Require(request.SourceDeviceType, "Source device type is required.", 64).ToLowerInvariant(),
            SourceDeviceId = Optional(request.SourceDeviceId, 128),
            EventType = NormalizeEnum(request.EventType, ["clock_in", "clock_out", "start_break", "end_break", "transfer", "attest", "correction_request"], "Clock event type"),
            EventTimestamp = request.EventTimestamp,
            Timezone = Require(request.Timezone, "Timezone is required.", 64),
            GeoPoint = Optional(request.GeoPoint, 128),
            SiteRef = Optional(request.SiteRef, 256),
            LocationRef = Optional(request.LocationRef, 256),
            SourceRef = Optional(request.SourceRef, 256),
            EnteredByPersonId = request.EnteredByPersonId,
            Notes = Optional(request.Notes, 2048),
            AnomalyFlagsCsv = string.Join(',', anomalyFlags),
            ImmutableAuditHash = ComputeHash($"{tenantId}|{request.PersonId}|{request.EventType}|{request.EventTimestamp:O}|{request.SourceProductKey}|{request.SourceRef}"),
        };

        db.TimekeepingClockEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.clock_event.create", tenantId, actorUserId, "clock_event", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapClockEvent(entity);
    }

    public async Task<IReadOnlyList<ClockEventResponse>> ListClockEventsAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.TimekeepingClockEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query.OrderByDescending(x => x.EventTimestamp).Take(200).Select(x => MapClockEvent(x)).ToListAsync(cancellationToken);
    }

    public async Task<FieldCompanionClockStatusResponse> GetFieldCompanionClockStatusAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        await EnsureFieldCompanionClockAllowedAsync(tenantId, personId, cancellationToken);
        var events = await db.TimekeepingClockEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderByDescending(x => x.EventTimestamp)
            .ThenByDescending(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        var latest = events.FirstOrDefault();
        return new FieldCompanionClockStatusResponse(
            ResolveFieldCompanionClockState(latest?.EventType),
            latest is null ? null : MapFieldCompanionClockEvent(latest),
            events.Select(MapFieldCompanionClockEvent).ToList());
    }

    public async Task<FieldCompanionClockSubmissionResponse> SubmitFieldCompanionClockEventAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid personId,
        SubmitFieldCompanionClockEventRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureFieldCompanionClockAllowedAsync(tenantId, personId, cancellationToken);

        var eventType = NormalizeEnum(request.EventType, ["clock_in", "clock_out"], "Clock event type");
        var idempotencyKey = Require(request.IdempotencyKey, "Idempotency key is required.", 256);
        var sourceDeviceId = Optional(request.SourceDeviceId, 128);
        var timezone = Require(request.Timezone, "Timezone is required.", 64);
        var geoPoint = Optional(request.GeoPoint, 128);
        var siteRef = Optional(request.SiteRef, 256);
        var locationRef = Optional(request.LocationRef, 256);
        var notes = Optional(request.Notes, 2048);
        var sourceRef = $"fieldcompanion:{idempotencyKey}";

        var existing = await db.TimekeepingClockEvents.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.PersonId == personId
                && x.SourceProductKey == "fieldcompanion"
                && x.SourceRef == sourceRef,
            cancellationToken);

        if (existing is not null)
        {
            EnsureFieldCompanionIdempotencyMatch(existing, eventType, request.EventTimestamp, timezone, sourceDeviceId, geoPoint, siteRef, locationRef, notes);
            return new FieldCompanionClockSubmissionResponse(
                existing.Id,
                false,
                SplitCsv(existing.AnomalyFlagsCsv).Contains("duplicate_state_transition", StringComparer.OrdinalIgnoreCase),
                ResolveFieldCompanionClockSubmissionStatus(existing.AnomalyFlagsCsv),
                ResolveFieldCompanionClockState(existing.EventType),
                MapFieldCompanionClockEvent(existing));
        }

        var previous = await db.TimekeepingClockEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderByDescending(x => x.EventTimestamp)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var anomalyFlags = new List<string>();
        if (request.EventTimestamp > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            anomalyFlags.Add("future_timestamp");
        }

        if (previous is not null && string.Equals(previous.EventType, eventType, StringComparison.OrdinalIgnoreCase))
        {
            anomalyFlags.Add("duplicate_state_transition");
        }

        var entity = new ClockEvent
        {
            TenantId = tenantId,
            PersonId = personId,
            SourceProductKey = "fieldcompanion",
            SourceDeviceType = "mobile",
            SourceDeviceId = sourceDeviceId,
            EventType = eventType,
            EventTimestamp = request.EventTimestamp,
            CapturedTimestamp = request.CapturedAt ?? DateTimeOffset.UtcNow,
            Timezone = timezone,
            GeoPoint = geoPoint,
            SiteRef = siteRef,
            LocationRef = locationRef,
            SourceRef = sourceRef,
            EnteredByPersonId = personId,
            Notes = notes,
            AnomalyFlagsCsv = string.Join(',', anomalyFlags),
            ImmutableAuditHash = ComputeHash($"{tenantId}|{personId}|{eventType}|{request.EventTimestamp:O}|fieldcompanion|{sourceRef}"),
        };

        db.TimekeepingClockEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.clock_event.fieldcompanion", tenantId, actorUserId, "clock_event", entity.Id.ToString(), "success", cancellationToken: cancellationToken);

        return new FieldCompanionClockSubmissionResponse(
            entity.Id,
            true,
            anomalyFlags.Contains("duplicate_state_transition", StringComparer.OrdinalIgnoreCase),
            ResolveFieldCompanionClockSubmissionStatus(entity.AnomalyFlagsCsv),
            ResolveFieldCompanionClockState(entity.EventType),
            MapFieldCompanionClockEvent(entity));
    }

    public async Task<IReadOnlyList<WorkSessionResponse>> ListWorkSessionsAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.TimekeepingWorkSessions.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query.OrderByDescending(x => x.SessionDate).ThenByDescending(x => x.StartTime).Take(200).Select(x => MapWorkSession(x)).ToListAsync(cancellationToken);
    }

    public async Task<WorkSessionResponse> UpsertWorkSessionAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertWorkSessionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var entity = id.HasValue
            ? await db.TimekeepingWorkSessions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;

        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.timekeeping.work_session_not_found", "Work session was not found.", 404);
        }

        entity ??= new WorkSession { TenantId = tenantId, PersonId = request.PersonId, CreatedAt = DateTimeOffset.UtcNow };
        entity.PersonId = request.PersonId;
        entity.SessionDate = request.SessionDate;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.Timezone = Require(request.Timezone, "Timezone is required.", 64);
        entity.Status = NormalizeEnum(request.Status, ["open", "draft", "pending_review", "submitted", "approved", "payroll_ready", "locked", "corrected", "void"], "Work session status");
        entity.SourceType = NormalizeEnum(request.SourceType, ["clock", "manual", "operational_evidence", "import"], "Work session source type");
        entity.PrimarySourceProductKey = Require(request.PrimarySourceProductKey, "Primary source product key is required.", 64).ToLowerInvariant();
        entity.PrimarySourceRef = Optional(request.PrimarySourceRef, 256);
        entity.SiteRef = Optional(request.SiteRef, 256);
        entity.LocationRef = Optional(request.LocationRef, 256);
        entity.SupervisorPersonId = request.SupervisorPersonId;
        entity.UnpaidBreakMinutes = Math.Max(0, request.UnpaidBreakMinutes);
        entity.CalculatedDurationMinutes = CalculateDurationMinutes(request.StartTime, request.EndTime);
        entity.PaidDurationMinutes = Math.Max(0, entity.CalculatedDurationMinutes - entity.UnpaidBreakMinutes);
        entity.RequiresReview = request.RequiresReview;
        entity.AnomalyFlagsCsv = string.Join(',', BuildSessionAnomalies(request.StartTime, request.EndTime, entity.CalculatedDurationMinutes));
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.TimekeepingWorkSessions.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.work_session.upsert", tenantId, actorUserId, "work_session", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapWorkSession(entity);
    }

    public async Task<IReadOnlyList<LeaveRequestResponse>> ListLeaveRequestsAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.TimekeepingLeaveRequests.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.RequestedAt)
            .Take(250)
            .Select(x => MapLeaveRequest(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<LeaveRequestResponse> GetLeaveRequestAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var leave = await db.TimekeepingLeaveRequests.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.leave_request_not_found", "Leave request was not found.", 404);
        return MapLeaveRequest(leave);
    }

    public async Task<LeaveRequestResponse> CreateLeaveRequestAsync(Guid tenantId, Guid? actorUserId, CreateLeaveRequestRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var entity = new LeaveRequest
        {
            TenantId = tenantId,
            PersonId = request.PersonId,
            LeaveType = NormalizeEnum(request.LeaveType, ["pto", "sick", "bereavement", "jury", "military", "parental", "personal", "unpaid", "accommodation", "leave_of_absence", "vacation", "other"], "Leave type"),
            StartDate = request.StartDate,
            EndDate = request.EndDate < request.StartDate ? request.StartDate : request.EndDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Timezone = Require(request.Timezone, "Timezone is required.", 64),
            IsIntermittent = request.IsIntermittent,
            IsPaid = request.IsPaid,
            Status = "requested",
            RequestedByPersonId = request.RequestedByPersonId,
            RequestedAt = DateTimeOffset.UtcNow,
            Reason = Optional(request.Reason, 2048),
            PayrollLockStatus = "unlocked",
            SourceProductKey = Optional(request.SourceProductKey, 64),
            SourceRef = Optional(request.SourceRef, 256),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        db.TimekeepingLeaveRequests.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.leave.request.create", tenantId, actorUserId, "leave_request", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapLeaveRequest(entity);
    }

    public Task<LeaveRequestResponse> ApproveLeaveRequestAsync(Guid tenantId, Guid? actorUserId, Guid id, LeaveStatusChangeRequest request, CancellationToken cancellationToken) =>
        ChangeLeaveRequestStatusAsync(tenantId, actorUserId, id, "approved", request.ReviewNotes, cancellationToken);

    public Task<LeaveRequestResponse> DenyLeaveRequestAsync(Guid tenantId, Guid? actorUserId, Guid id, LeaveStatusChangeRequest request, CancellationToken cancellationToken) =>
        ChangeLeaveRequestStatusAsync(tenantId, actorUserId, id, "denied", request.ReviewNotes, cancellationToken);

    public Task<LeaveRequestResponse> CancelLeaveRequestAsync(Guid tenantId, Guid? actorUserId, Guid id, LeaveStatusChangeRequest request, CancellationToken cancellationToken) =>
        ChangeLeaveRequestStatusAsync(tenantId, actorUserId, id, "cancelled", request.ReviewNotes, cancellationToken);

    public async Task<IReadOnlyList<AttendanceEventResponse>> ListAttendanceEventsAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.TimekeepingAttendanceEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query
            .OrderByDescending(x => x.OccurredAt)
            .Take(250)
            .Select(x => MapAttendanceEvent(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<AttendanceEventResponse> CreateAttendanceEventAsync(Guid tenantId, Guid? actorUserId, CreateAttendanceEventRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        if (request.RelatedLeaveRequestId.HasValue)
        {
            _ = await db.TimekeepingLeaveRequests.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.RelatedLeaveRequestId.Value, cancellationToken)
                ?? throw new StlApiException("staffarr.timekeeping.leave_request_not_found", "Leave request was not found.", 404);
        }

        if (request.RelatedTimesheetPeriodId.HasValue)
        {
            _ = await db.TimekeepingTimesheetPeriods.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.RelatedTimesheetPeriodId.Value, cancellationToken)
                ?? throw new StlApiException("staffarr.timekeeping.timesheet_not_found", "Timesheet period was not found.", 404);
        }

        var entity = new AttendanceEvent
        {
            TenantId = tenantId,
            PersonId = request.PersonId,
            OccurredAt = request.OccurredAt,
            EventType = NormalizeEnum(request.EventType, ["tardy", "absence", "no_call_no_show", "early_departure", "missed_meal", "missed_break", "attendance_point_assessed", "attendance_point_removed"], "Attendance event type"),
            Severity = NormalizeEnum(request.Severity, ["low", "medium", "high", "critical"], "Attendance severity"),
            PointValue = request.PointValue,
            Status = NormalizeEnum(request.Status, ["open", "under_review", "resolved", "dismissed", "counted"], "Attendance status"),
            Notes = Optional(request.Notes, 2048),
            SourceProductKey = Require(request.SourceProductKey, "Source product key is required.", 64).ToLowerInvariant(),
            SourceRef = Optional(request.SourceRef, 256),
            RelatedLeaveRequestId = request.RelatedLeaveRequestId,
            RelatedTimesheetPeriodId = request.RelatedTimesheetPeriodId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        db.TimekeepingAttendanceEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.attendance.create", tenantId, actorUserId, "attendance_event", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapAttendanceEvent(entity);
    }

    public async Task<AttendanceEventResponse> ResolveAttendanceEventAsync(Guid tenantId, Guid? actorUserId, Guid id, ResolveAttendanceEventRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.TimekeepingAttendanceEvents.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.attendance_not_found", "Attendance event was not found.", 404);
        entity.Status = "resolved";
        entity.ReviewedByPersonId = actorUserId;
        entity.ReviewedAt = DateTimeOffset.UtcNow;
        entity.ResolutionNotes = Optional(request.ResolutionNotes, 2048);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.attendance.resolve", tenantId, actorUserId, "attendance_event", id.ToString(), "success", cancellationToken: cancellationToken);
        return MapAttendanceEvent(entity);
    }

    public async Task<IReadOnlyList<AvailabilityBlockResponse>> ListAvailabilityBlocksAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.TimekeepingAvailabilityBlocks.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query
            .OrderByDescending(x => x.EffectiveStartDate)
            .ThenByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => MapAvailabilityBlock(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<AvailabilityBlockResponse> UpsertAvailabilityBlockAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertAvailabilityBlockRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var entity = id.HasValue
            ? await db.TimekeepingAvailabilityBlocks.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;

        if (id.HasValue && entity is null)
        {
            throw new StlApiException("staffarr.timekeeping.availability_not_found", "Availability block was not found.", 404);
        }

        entity ??= new AvailabilityBlock { TenantId = tenantId, PersonId = request.PersonId, CreatedAt = DateTimeOffset.UtcNow };
        entity.PersonId = request.PersonId;
        entity.AvailabilityType = NormalizeEnum(request.AvailabilityType, ["available", "preferred", "unavailable", "restricted"], "Availability type");
        entity.DayOfWeekMaskCsv = Require(request.DayOfWeekMaskCsv, "Day of week mask is required.", 64);
        entity.StartLocalTime = request.StartLocalTime;
        entity.EndLocalTime = request.EndLocalTime;
        entity.Timezone = Require(request.Timezone, "Timezone is required.", 64);
        entity.EffectiveStartDate = request.EffectiveStartDate;
        entity.EffectiveEndDate = request.EffectiveEndDate;
        entity.Status = NormalizeEnum(request.Status, ["active", "inactive", "draft"], "Availability status");
        entity.Notes = Optional(request.Notes, 2048);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entity).State == EntityState.Detached)
        {
            db.TimekeepingAvailabilityBlocks.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.availability.upsert", tenantId, actorUserId, "availability_block", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapAvailabilityBlock(entity);
    }

    public async Task<IReadOnlyList<TimeEntryResponse>> ListTimeEntriesAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var entries = await LoadEntriesAsync(tenantId, personId, null, cancellationToken);
        entries = entries.OrderByDescending(x => x.EntryDate).Take(250).ToList();
        return entries.Select(MapTimeEntry).ToList();
    }

    public async Task<TimeEntryResponse> UpsertTimeEntryAsync(Guid tenantId, Guid? actorUserId, Guid? id, UpsertTimeEntryRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        _ = await db.TimekeepingTimesheetPeriods.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.TimesheetPeriodId, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.timesheet_not_found", "Timesheet period was not found.", 404);
        _ = await db.TimekeepingPayCodes.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.PayCodeId, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.pay_code_not_found", "Pay code was not found.", 404);

        var entry = id.HasValue
            ? await db.TimekeepingTimeEntries.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id.Value, cancellationToken)
            : null;

        if (id.HasValue && entry is null)
        {
            throw new StlApiException("staffarr.timekeeping.time_entry_not_found", "Time entry was not found.", 404);
        }

        entry ??= new TimeEntry { TenantId = tenantId, CreatedAt = DateTimeOffset.UtcNow };
        entry.PersonId = request.PersonId;
        entry.WorkSessionId = request.WorkSessionId;
        entry.TimesheetPeriodId = request.TimesheetPeriodId;
        entry.EntryDate = request.EntryDate;
        entry.StartTime = request.StartTime;
        entry.EndTime = request.EndTime;
        entry.DurationMinutes = request.DurationMinutes > 0 ? request.DurationMinutes : CalculateDurationMinutes(request.StartTime, request.EndTime);
        entry.PayCodeId = request.PayCodeId;
        entry.PayPolicyId = request.PayPolicyId;
        entry.Classification = NormalizeEnum(request.Classification, ["regular", "overtime", "double_time", "holiday", "pto", "unpaid", "training", "standby", "callout", "adjustment"], "Time entry classification");
        entry.SourceProductKey = Require(request.SourceProductKey, "Source product key is required.", 64).ToLowerInvariant();
        entry.SourceRef = Optional(request.SourceRef, 256);
        entry.SourceConfidence = NormalizeEnum(request.SourceConfidence, ["exact", "inferred", "estimated", "manual"], "Source confidence");
        entry.Description = Optional(request.Description, 2048);
        entry.RequiresApproval = request.RequiresApproval;
        entry.ApprovalStatus = NormalizeEnum(request.ApprovalStatus, ["pending", "approved", "rejected", "not_required"], "Approval status");
        entry.PayrollLockStatus = entry.PayrollLockStatus is "locked" ? "locked" : "unlocked";
        entry.CreatedByPersonId = actorUserId;
        entry.UpdatedAt = DateTimeOffset.UtcNow;

        if (db.Entry(entry).State == EntityState.Detached)
        {
            db.TimekeepingTimeEntries.Add(entry);
        }

        await db.SaveChangesAsync(cancellationToken);
        await ReplaceAllocationsAsync(tenantId, entry, request.Allocations ?? [], cancellationToken);
        await RecalculateTimesheetTotalsAsync(tenantId, entry.TimesheetPeriodId, cancellationToken);
        await ValidateTimesheetAsync(tenantId, entry.TimesheetPeriodId, cancellationToken);
        await audit.WriteAsync("timekeeping.time_entry.upsert", tenantId, actorUserId, "time_entry", entry.Id.ToString(), "success", cancellationToken: cancellationToken);
        return (await LoadEntriesAsync(tenantId, entry.PersonId, null, cancellationToken)).First(x => x.Id == entry.Id).Let(MapTimeEntry);
    }

    public async Task<IReadOnlyList<TimesheetPeriodResponse>> ListTimesheetsAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.TimekeepingTimesheetPeriods.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        var periods = await query.OrderByDescending(x => x.PeriodStartDate).Take(100).ToListAsync(cancellationToken);
        var entries = await LoadEntriesAsync(tenantId, personId, null, cancellationToken);
        return periods.Select(period => MapTimesheet(period, entries.Where(x => x.TimesheetPeriodId == period.Id).ToList())).ToList();
    }

    public async Task<TimesheetPeriodResponse> CreateTimesheetPeriodAsync(Guid tenantId, Guid? actorUserId, CreateTimesheetPeriodRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var entity = new TimesheetPeriod
        {
            TenantId = tenantId,
            PersonId = request.PersonId,
            PayrollCalendarRef = Require(request.PayrollCalendarRef, "Payroll calendar reference is required.", 256),
            PeriodStartDate = request.PeriodStartDate,
            PeriodEndDate = request.PeriodEndDate,
            Status = "open",
        };

        db.TimekeepingTimesheetPeriods.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.timesheet.create", tenantId, actorUserId, "timesheet_period", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapTimesheet(entity, []);
    }

    public async Task<TimesheetPeriodResponse> GetTimesheetAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        var period = await db.TimekeepingTimesheetPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.timesheet_not_found", "Timesheet period was not found.", 404);
        var entries = await LoadEntriesAsync(tenantId, null, id, cancellationToken);
        return MapTimesheet(period, entries);
    }

    public Task<TimesheetPeriodResponse> SubmitTimesheetAsync(Guid tenantId, Guid? actorUserId, Guid id, CancellationToken cancellationToken) =>
        ChangeTimesheetStatusAsync(tenantId, actorUserId, id, "submitted", cancellationToken, submittedAt: DateTimeOffset.UtcNow);

    public Task<TimesheetPeriodResponse> ApproveTimesheetAsync(Guid tenantId, Guid? actorUserId, Guid id, CancellationToken cancellationToken) =>
        ChangeTimesheetStatusAsync(tenantId, actorUserId, id, "supervisor_approved", cancellationToken, approvedAt: DateTimeOffset.UtcNow, approvedByPersonId: actorUserId);

    public Task<TimesheetPeriodResponse> RejectTimesheetAsync(Guid tenantId, Guid? actorUserId, Guid id, CancellationToken cancellationToken) =>
        ChangeTimesheetStatusAsync(tenantId, actorUserId, id, "open", cancellationToken);

    public Task<TimesheetPeriodResponse> ReopenTimesheetAsync(Guid tenantId, Guid? actorUserId, Guid id, CancellationToken cancellationToken) =>
        ChangeTimesheetStatusAsync(tenantId, actorUserId, id, "reopened", cancellationToken, payrollReadyAt: null);

    public async Task<TimesheetPeriodResponse> MarkPayrollReadyAsync(Guid tenantId, Guid? actorUserId, Guid id, CancellationToken cancellationToken)
    {
        var blocking = await db.TimekeepingExceptions.AnyAsync(
            x => x.TenantId == tenantId && x.TimesheetPeriodId == id && x.Severity == "blocking" && x.ResolutionStatus != "resolved",
            cancellationToken);
        if (blocking)
        {
            throw new StlApiException("staffarr.timekeeping.blocking_exceptions", "Blocking exceptions must be resolved before payroll-ready.", 409);
        }

        var period = await db.TimekeepingTimesheetPeriods.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.timesheet_not_found", "Timesheet period was not found.", 404);
        period.Status = "payroll_ready";
        period.PayrollReadyAt = DateTimeOffset.UtcNow;
        period.UpdatedAt = DateTimeOffset.UtcNow;

        var entries = await db.TimekeepingTimeEntries.Where(x => x.TenantId == tenantId && x.TimesheetPeriodId == id).ToListAsync(cancellationToken);
        foreach (var entry in entries)
        {
            entry.PayrollLockStatus = "locked";
            entry.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.timesheet.mark_payroll_ready", tenantId, actorUserId, "timesheet_period", id.ToString(), "success", cancellationToken: cancellationToken);
        return await GetTimesheetAsync(tenantId, id, cancellationToken);
    }

    public async Task<IReadOnlyList<TimeExceptionResponse>> ListExceptionsAsync(Guid tenantId, Guid? personId, CancellationToken cancellationToken)
    {
        var query = db.TimekeepingExceptions.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }

        return await query.OrderByDescending(x => x.CreatedAt).Take(200).Select(x => MapException(x)).ToListAsync(cancellationToken);
    }

    public async Task<TimeExceptionResponse> ResolveExceptionAsync(Guid tenantId, Guid? actorUserId, Guid id, ResolveTimeExceptionRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.TimekeepingExceptions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.exception_not_found", "Time exception was not found.", 404);
        entity.ResolutionStatus = "resolved";
        entity.ResolvedAt = DateTimeOffset.UtcNow;
        entity.ResolvedByPersonId = actorUserId;
        entity.ResolutionNotes = Optional(request.ResolutionNotes, 2048);
        await db.SaveChangesAsync(cancellationToken);
        await RecalculateTimesheetExceptionCountAsync(tenantId, entity.TimesheetPeriodId, cancellationToken);
        await audit.WriteAsync("timekeeping.exception.resolve", tenantId, actorUserId, "time_exception", id.ToString(), "success", cancellationToken: cancellationToken);
        return MapException(entity);
    }

    public async Task<TimeCorrectionResponse> CreateCorrectionAsync(Guid tenantId, Guid? actorUserId, CreateTimeCorrectionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, request.PersonId, cancellationToken);
        var entity = new TimeCorrection
        {
            TenantId = tenantId,
            PersonId = request.PersonId,
            TargetType = NormalizeEnum(request.TargetType, ["clock_event", "work_session", "time_entry", "timesheet_period"], "Correction target type"),
            TargetId = request.TargetId,
            RequestedByPersonId = request.RequestedByPersonId,
            ReasonCode = Require(request.ReasonCode, "Reason code is required.", 64),
            ReasonText = Require(request.ReasonText, "Reason text is required.", 2048),
            OldSnapshot = Require(request.OldSnapshot, "Old snapshot is required.", 16000),
            NewSnapshot = Require(request.NewSnapshot, "New snapshot is required.", 16000),
        };

        db.TimekeepingCorrections.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync("timekeeping.correction.request", tenantId, actorUserId, "time_correction", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapCorrection(entity);
    }

    public async Task<TimeCorrectionResponse> ApproveCorrectionAsync(Guid tenantId, Guid? actorUserId, Guid id, bool approved, CancellationToken cancellationToken)
    {
        var entity = await db.TimekeepingCorrections.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.correction_not_found", "Time correction was not found.", 404);
        entity.ApprovalStatus = approved ? "approved" : "rejected";
        entity.ApprovedByPersonId = actorUserId;
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync($"timekeeping.correction.{entity.ApprovalStatus}", tenantId, actorUserId, "time_correction", id.ToString(), "success", cancellationToken: cancellationToken);
        return MapCorrection(entity);
    }

    public async Task<TimeAttestationResponse> CreateAttestationAsync(Guid tenantId, Guid? actorUserId, CreateTimeAttestationRequest request, CancellationToken cancellationToken)
    {
        var entity = new TimeAttestation
        {
            TenantId = tenantId,
            PersonId = request.PersonId,
            TimesheetPeriodId = request.TimesheetPeriodId,
            AttestationType = NormalizeEnum(request.AttestationType, ["end_shift", "meal_break", "timesheet_submit", "supervisor_approval", "payroll_review"], "Attestation type"),
            StatementText = Require(request.StatementText, "Statement text is required.", 2048),
            Response = Require(request.Response, "Attestation response is required.", 512),
            AttestedByPersonId = request.AttestedByPersonId,
            SourceDeviceType = Require(request.SourceDeviceType, "Source device type is required.", 64),
            SourceProductKey = Require(request.SourceProductKey, "Source product key is required.", 64).ToLowerInvariant(),
        };

        db.TimekeepingAttestations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        var period = await db.TimekeepingTimesheetPeriods.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.TimesheetPeriodId, cancellationToken);
        if (period is not null)
        {
            period.AttestationStatus = "complete";
            period.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        await audit.WriteAsync("timekeeping.attestation.create", tenantId, actorUserId, "time_attestation", entity.Id.ToString(), "success", cancellationToken: cancellationToken);
        return MapAttestation(entity);
    }

    public async Task<LaborEvidenceIngestResponse> IngestLaborEvidenceAsync(LaborEvidenceIngestRequest request, CancellationToken cancellationToken)
    {
        var existing = await db.TimekeepingLaborEvidenceInbox.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId && x.IdempotencyKey == request.IdempotencyKey,
            cancellationToken);

        if (existing is not null)
        {
            return new LaborEvidenceIngestResponse(
                existing.TimeEntryId ?? Guid.Empty,
                existing.TimesheetPeriodId ?? Guid.Empty,
                false,
                existing.ConflictDetected,
                existing.Status);
        }

        await EnsurePersonAsync(request.TenantId, request.PersonId, cancellationToken);
        var profile = await db.TimekeepingProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == request.TenantId && x.PersonId == request.PersonId, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.profile_not_found", "Timekeeping profile is required before labor evidence can be ingested.", 409);

        var timesheet = await FindOrCreateTimesheetForEvidenceAsync(request, cancellationToken);
        var payCode = await ResolvePayCodeAsync(request.TenantId, request.SuggestedPayCodeKey, cancellationToken);
        var start = request.StartedAt ?? request.EmittedAt;
        var end = request.EndedAt ?? (request.DurationMinutes.HasValue ? start.AddMinutes(request.DurationMinutes.Value) : (DateTimeOffset?)null);
        var duration = request.DurationMinutes ?? CalculateDurationMinutes(start, end);

        var conflictDetected = await db.TimekeepingTimeEntries.AnyAsync(
            x => x.TenantId == request.TenantId
                && x.PersonId == request.PersonId
                && x.EntryDate == DateOnly.FromDateTime(start.UtcDateTime)
                && ((x.StartTime != null && end != null && x.StartTime < end && (x.EndTime ?? x.StartTime) > start)
                    || (x.SourceProductKey != request.SourceProductKey && x.SourceRef == $"{request.SourceEntityType}:{request.SourceEntityId}")),
            cancellationToken);

        var entry = new TimeEntry
        {
            TenantId = request.TenantId,
            PersonId = request.PersonId,
            TimesheetPeriodId = timesheet.Id,
            EntryDate = DateOnly.FromDateTime(start.UtcDateTime),
            StartTime = start,
            EndTime = end,
            DurationMinutes = duration,
            PayCodeId = payCode.Id,
            PayPolicyId = profile.PayPolicyId,
            Classification = InferClassification(payCode.Category, request.ActivityType),
            SourceProductKey = request.SourceProductKey.ToLowerInvariant(),
            SourceRef = $"{request.SourceEntityType}:{request.SourceEntityId}",
            SourceConfidence = NormalizeEnum(request.Confidence, ["exact", "inferred", "estimated", "manual"], "Evidence confidence"),
            Description = Optional(request.Notes, 2048),
            RequiresApproval = true,
            ApprovalStatus = "pending",
        };

        db.TimekeepingTimeEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);

        var defaultCostRef = request.CostObjectRefs?.FirstOrDefault();
        var allocation = new LaborAllocation
        {
            TenantId = request.TenantId,
            TimeEntryId = entry.Id,
            AllocationPercent = 100,
            AllocationMinutes = duration,
            ProductKey = defaultCostRef?.ProductKey ?? request.SourceProductKey,
            CostObjectType = defaultCostRef?.ObjectType ?? request.SourceEntityType,
            CostObjectRef = defaultCostRef?.ObjectId ?? request.SourceEntityId,
            LegalEntityRef = profile.DefaultLegalEntityRef ?? request.LegalEntityRef ?? "unassigned",
            SiteRef = profile.DefaultSiteRef ?? request.SiteRef ?? "unassigned",
            DepartmentRef = profile.DefaultDepartmentRef ?? "unassigned",
            CustomerRef = request.CostObjectRefs?.FirstOrDefault(x => x.ObjectType.Contains("customer", StringComparison.OrdinalIgnoreCase))?.ObjectId,
            OrderRef = request.CostObjectRefs?.FirstOrDefault(x => x.ObjectType.Contains("order", StringComparison.OrdinalIgnoreCase))?.ObjectId,
            AssetRef = request.CostObjectRefs?.FirstOrDefault(x => x.ObjectType.Contains("asset", StringComparison.OrdinalIgnoreCase))?.ObjectId,
            WorkOrderRef = request.CostObjectRefs?.FirstOrDefault(x => x.ObjectType.Contains("work", StringComparison.OrdinalIgnoreCase))?.ObjectId,
            TripRef = request.CostObjectRefs?.FirstOrDefault(x => x.ObjectType.Contains("trip", StringComparison.OrdinalIgnoreCase))?.ObjectId,
            RouteRef = request.CostObjectRefs?.FirstOrDefault(x => x.ObjectType.Contains("route", StringComparison.OrdinalIgnoreCase))?.ObjectId,
            WarehouseTaskRef = request.CostObjectRefs?.FirstOrDefault(x => x.ObjectType.Contains("warehouse", StringComparison.OrdinalIgnoreCase))?.ObjectId,
            TrainingSessionRef = request.CostObjectRefs?.FirstOrDefault(x => x.ObjectType.Contains("training", StringComparison.OrdinalIgnoreCase))?.ObjectId,
            QualityCaseRef = request.CostObjectRefs?.FirstOrDefault(x => x.ObjectType.Contains("quality", StringComparison.OrdinalIgnoreCase))?.ObjectId,
            ProjectRef = request.CostObjectRefs?.FirstOrDefault(x => x.ObjectType.Contains("project", StringComparison.OrdinalIgnoreCase))?.ObjectId,
        };
        db.TimekeepingLaborAllocations.Add(allocation);

        var inbox = new LaborEvidenceInboxItem
        {
            TenantId = request.TenantId,
            IdempotencyKey = Require(request.IdempotencyKey, "Idempotency key is required.", 256),
            SourceProductKey = request.SourceProductKey.ToLowerInvariant(),
            SourceEntityType = Require(request.SourceEntityType, "Source entity type is required.", 64),
            SourceEntityId = Require(request.SourceEntityId, "Source entity id is required.", 128),
            PersonId = request.PersonId,
            ActivityType = Require(request.ActivityType, "Activity type is required.", 64),
            SuggestedPayCodeKey = Optional(request.SuggestedPayCodeKey, 64),
            StartedAt = request.StartedAt,
            EndedAt = request.EndedAt,
            DurationMinutes = duration,
            Timezone = Require(request.Timezone, "Timezone is required.", 64),
            SiteRef = Optional(request.SiteRef, 256),
            LocationRef = Optional(request.LocationRef, 256),
            LegalEntityRef = Optional(request.LegalEntityRef, 256),
            CostObjectRefsJson = JsonSerializer.Serialize(request.CostObjectRefs ?? [], JsonOptions),
            Confidence = request.Confidence,
            Notes = Optional(request.Notes, 2048),
            EmittedAt = request.EmittedAt,
            Status = conflictDetected ? "flagged" : "accepted",
            TimeEntryId = entry.Id,
            TimesheetPeriodId = timesheet.Id,
            ConflictDetected = conflictDetected,
        };

        db.TimekeepingLaborEvidenceInbox.Add(inbox);
        await db.SaveChangesAsync(cancellationToken);
        await RecalculateTimesheetTotalsAsync(request.TenantId, timesheet.Id, cancellationToken);
        await ValidateTimesheetAsync(request.TenantId, timesheet.Id, cancellationToken);

        return new LaborEvidenceIngestResponse(entry.Id, timesheet.Id, true, conflictDetected, inbox.Status);
    }

    public async Task<IReadOnlyList<TimesheetPeriodResponse>> GetPayrollReadyPeriodsAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        return await ListTimesheetsAsync(tenantId, personId, cancellationToken)
            .ContinueWith(task => (IReadOnlyList<TimesheetPeriodResponse>)task.Result.Where(x => x.Status == "payroll_ready").ToList(), cancellationToken);
    }

    public async Task<PayrollReadySnapshotResponse> GetPayrollReadySnapshotAsync(Guid tenantId, DateOnly? periodStartDate, DateOnly? periodEndDate, CancellationToken cancellationToken)
    {
        var periodsQuery = db.TimekeepingTimesheetPeriods
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "payroll_ready");

        if (periodStartDate.HasValue)
        {
            periodsQuery = periodsQuery.Where(x => x.PeriodStartDate >= periodStartDate.Value);
        }

        if (periodEndDate.HasValue)
        {
            periodsQuery = periodsQuery.Where(x => x.PeriodEndDate <= periodEndDate.Value);
        }

        var periods = await periodsQuery.OrderBy(x => x.PeriodStartDate).ToListAsync(cancellationToken);
        var people = await db.TimekeepingProfiles.AsNoTracking().Where(x => x.TenantId == tenantId).ToDictionaryAsync(x => x.PersonId, cancellationToken);
        var payCodes = await db.TimekeepingPayCodes.AsNoTracking().Where(x => x.TenantId == tenantId).ToDictionaryAsync(x => x.Id, cancellationToken);
        var periodIds = periods.Select(p => p.Id).ToHashSet();
        var entries = (await LoadEntriesAsync(tenantId, null, null, cancellationToken)).Where(x => periodIds.Contains(x.TimesheetPeriodId)).ToList();

        var snapshotRows = periods.Select(period =>
        {
            people.TryGetValue(period.PersonId, out var profile);
            var periodEntries = entries.Where(x => x.TimesheetPeriodId == period.Id).ToList();
            var payload = periodEntries.Select(x =>
            {
                var payCode = payCodes[x.PayCodeId];
                return new PayrollReadyTimeEntryResponse(
                    x.Id,
                    x.EntryDate,
                    x.DurationMinutes,
                    x.Classification,
                    payCode.Code,
                    payCode.DisplayName,
                    x.SourceProductKey,
                    x.SourceRef,
                    MapAllocations(x.Allocations));
            }).ToList();

            var hash = ComputeHash(JsonSerializer.Serialize(new
            {
                period.Id,
                period.PersonId,
                period.PeriodStartDate,
                period.PeriodEndDate,
                payload,
            }, JsonOptions));

            return new PayrollReadyTimesheetResponse(
                period.Id,
                period.PersonId,
                profile?.WorkerNumber ?? string.Empty,
                profile?.DefaultLegalEntityRef,
                period.PayrollCalendarRef,
                period.PeriodStartDate,
                period.PeriodEndDate,
                period.Status,
                period.PayrollReadyAt,
                hash,
                payload);
        }).ToList();

        var snapshotId = DeterministicGuidFromHash(ComputeHash(JsonSerializer.Serialize(snapshotRows, JsonOptions)));
        return new PayrollReadySnapshotResponse(snapshotId, tenantId, DateTimeOffset.UtcNow, snapshotRows);
    }

    private async Task ReplaceAllocationsAsync(Guid tenantId, TimeEntry entry, IReadOnlyList<UpsertLaborAllocationRequest> allocations, CancellationToken cancellationToken)
    {
        var existing = await db.TimekeepingLaborAllocations.Where(x => x.TenantId == tenantId && x.TimeEntryId == entry.Id).ToListAsync(cancellationToken);
        db.TimekeepingLaborAllocations.RemoveRange(existing);

        foreach (var allocation in allocations)
        {
            db.TimekeepingLaborAllocations.Add(new LaborAllocation
            {
                TenantId = tenantId,
                TimeEntryId = entry.Id,
                AllocationPercent = allocation.AllocationPercent,
                AllocationMinutes = allocation.AllocationMinutes,
                ProductKey = Require(allocation.ProductKey, "Allocation product key is required.", 64).ToLowerInvariant(),
                CostObjectType = Require(allocation.CostObjectType, "Cost object type is required.", 64),
                CostObjectRef = Require(allocation.CostObjectRef, "Cost object ref is required.", 256),
                LegalEntityRef = Require(allocation.LegalEntityRef, "Legal entity ref is required.", 256),
                SiteRef = Require(allocation.SiteRef, "Site ref is required.", 256),
                DepartmentRef = Require(allocation.DepartmentRef, "Department ref is required.", 256),
                CustomerRef = Optional(allocation.CustomerRef, 256),
                OrderRef = Optional(allocation.OrderRef, 256),
                AssetRef = Optional(allocation.AssetRef, 256),
                WorkOrderRef = Optional(allocation.WorkOrderRef, 256),
                TripRef = Optional(allocation.TripRef, 256),
                RouteRef = Optional(allocation.RouteRef, 256),
                WarehouseTaskRef = Optional(allocation.WarehouseTaskRef, 256),
                TrainingSessionRef = Optional(allocation.TrainingSessionRef, 256),
                QualityCaseRef = Optional(allocation.QualityCaseRef, 256),
                ProjectRef = Optional(allocation.ProjectRef, 256),
                GlDimensionSnapshot = Optional(allocation.GlDimensionSnapshot, 2048),
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task RecalculateTimesheetTotalsAsync(Guid tenantId, Guid timesheetPeriodId, CancellationToken cancellationToken)
    {
        var period = await db.TimekeepingTimesheetPeriods.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == timesheetPeriodId, cancellationToken);
        if (period is null)
        {
            return;
        }

        var payCodes = await db.TimekeepingPayCodes.AsNoTracking().Where(x => x.TenantId == tenantId).ToDictionaryAsync(x => x.Id, cancellationToken);
        var entries = await db.TimekeepingTimeEntries.AsNoTracking().Where(x => x.TenantId == tenantId && x.TimesheetPeriodId == timesheetPeriodId).ToListAsync(cancellationToken);
        period.TotalPaidMinutes = entries.Sum(x => x.DurationMinutes);
        period.TotalWorkedMinutes = entries.Where(x => payCodes.TryGetValue(x.PayCodeId, out var code) && code.CountsTowardWorkedHours).Sum(x => x.DurationMinutes);
        period.TotalUnpaidMinutes = entries.Where(x => x.Classification == "unpaid").Sum(x => x.DurationMinutes);
        period.OvertimeMinutes = entries.Where(x => x.Classification == "overtime" || x.Classification == "double_time").Sum(x => x.DurationMinutes);
        period.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task RecalculateTimesheetExceptionCountAsync(Guid tenantId, Guid timesheetPeriodId, CancellationToken cancellationToken)
    {
        var period = await db.TimekeepingTimesheetPeriods.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == timesheetPeriodId, cancellationToken);
        if (period is null)
        {
            return;
        }

        period.ExceptionCount = await db.TimekeepingExceptions.CountAsync(x => x.TenantId == tenantId && x.TimesheetPeriodId == timesheetPeriodId && x.ResolutionStatus != "resolved", cancellationToken);
        period.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateTimesheetAsync(Guid tenantId, Guid timesheetPeriodId, CancellationToken cancellationToken)
    {
        var period = await db.TimekeepingTimesheetPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == timesheetPeriodId, cancellationToken);
        if (period is null)
        {
            return;
        }

        var entries = await LoadEntriesAsync(tenantId, null, timesheetPeriodId, cancellationToken);
        var sessions = await db.TimekeepingWorkSessions.AsNoTracking().Where(x => x.TenantId == tenantId && x.PersonId == period.PersonId && x.SessionDate >= period.PeriodStartDate && x.SessionDate <= period.PeriodEndDate).ToListAsync(cancellationToken);
        var profile = await db.TimekeepingProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PersonId == period.PersonId, cancellationToken);

        var existing = await db.TimekeepingExceptions.Where(x => x.TenantId == tenantId && x.TimesheetPeriodId == timesheetPeriodId && x.ResolutionStatus != "resolved").ToListAsync(cancellationToken);
        db.TimekeepingExceptions.RemoveRange(existing);

        var exceptions = new List<TimeException>();

        if (sessions.Any(x => x.Status == "open"))
        {
            exceptions.Add(NewException(tenantId, period.PersonId, timesheetPeriodId, null, null, "blocking", "missing_clock_out", "One or more work sessions are still open.", "staffarr", null));
        }

        foreach (var overlap in FindOverlappingSessions(sessions))
        {
            exceptions.Add(NewException(tenantId, period.PersonId, timesheetPeriodId, overlap.Id, null, "blocking", "overlapping_work_sessions", "Work sessions overlap.", overlap.PrimarySourceProductKey, overlap.PrimarySourceRef));
        }

        foreach (var entry in entries)
        {
            if (entry.PayCodeId == Guid.Empty)
            {
                exceptions.Add(NewException(tenantId, period.PersonId, timesheetPeriodId, entry.WorkSessionId, entry.Id, "blocking", "missing_pay_code", "Time entry is missing a pay code.", entry.SourceProductKey, entry.SourceRef));
            }

            if (!entry.Allocations.Any())
            {
                exceptions.Add(NewException(tenantId, period.PersonId, timesheetPeriodId, entry.WorkSessionId, entry.Id, "blocking", "missing_labor_allocation", "Time entry requires a labor allocation.", entry.SourceProductKey, entry.SourceRef));
            }

            if (entry.RequiresApproval && entry.ApprovalStatus != "approved")
            {
                exceptions.Add(NewException(tenantId, period.PersonId, timesheetPeriodId, entry.WorkSessionId, entry.Id, "warning", "missing_supervisor_approval", "Time entry is pending supervisor approval.", entry.SourceProductKey, entry.SourceRef));
            }
        }

        if (profile?.RequiresMealBreakAttestation == true && period.AttestationStatus != "complete")
        {
            exceptions.Add(NewException(tenantId, period.PersonId, timesheetPeriodId, null, null, "warning", "missing_meal_break_attestation", "Meal break attestation is required before payroll-ready.", "staffarr", null));
        }

        db.TimekeepingExceptions.AddRange(exceptions);
        await db.SaveChangesAsync(cancellationToken);
        await RecalculateTimesheetExceptionCountAsync(tenantId, timesheetPeriodId, cancellationToken);
    }

    private async Task<TimesheetPeriod> FindOrCreateTimesheetForEvidenceAsync(LaborEvidenceIngestRequest request, CancellationToken cancellationToken)
    {
        var entryDate = DateOnly.FromDateTime((request.StartedAt ?? request.EmittedAt).UtcDateTime);
        var period = await db.TimekeepingTimesheetPeriods.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId
                && x.PersonId == request.PersonId
                && x.PeriodStartDate <= entryDate
                && x.PeriodEndDate >= entryDate,
            cancellationToken);

        if (period is not null)
        {
            return period;
        }

        period = new TimesheetPeriod
        {
            TenantId = request.TenantId,
            PersonId = request.PersonId,
            PayrollCalendarRef = request.LegalEntityRef ?? "unassigned",
            PeriodStartDate = entryDate.AddDays(-(int)entryDate.DayOfWeek),
            PeriodEndDate = entryDate.AddDays(6 - (int)entryDate.DayOfWeek),
            Status = "draft",
        };
        db.TimekeepingTimesheetPeriods.Add(period);
        await db.SaveChangesAsync(cancellationToken);
        return period;
    }

    private async Task<PayCode> ResolvePayCodeAsync(Guid tenantId, string? suggestedPayCodeKey, CancellationToken cancellationToken)
    {
        PayCode? payCode = null;
        if (!string.IsNullOrWhiteSpace(suggestedPayCodeKey))
        {
            var normalized = suggestedPayCodeKey.Trim().ToUpperInvariant();
            payCode = await db.TimekeepingPayCodes.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == normalized, cancellationToken);
        }

        payCode ??= await db.TimekeepingPayCodes.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == "REG", cancellationToken);
        if (payCode is not null)
        {
            return payCode;
        }

        payCode = new PayCode
        {
            TenantId = tenantId,
            Code = "REG",
            DisplayName = "Regular",
            Category = "worked",
            CountsTowardWorkedHours = true,
            CountsTowardOvertimeBase = true,
            Active = true,
            EffectiveStartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
        };
        db.TimekeepingPayCodes.Add(payCode);
        await db.SaveChangesAsync(cancellationToken);
        return payCode;
    }

    private async Task<LeaveRequestResponse> ChangeLeaveRequestStatusAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid id,
        string status,
        string? reviewNotes,
        CancellationToken cancellationToken)
    {
        var entity = await db.TimekeepingLeaveRequests.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.leave_request_not_found", "Leave request was not found.", 404);
        entity.Status = status;
        entity.ReviewNotes = Optional(reviewNotes, 2048);
        entity.ReviewedAt = DateTimeOffset.UtcNow;
        entity.ApprovedByPersonId = actorUserId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        if (string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            entity.PayrollLockStatus = "locked";
        }
        else if (!string.Equals(status, "requested", StringComparison.OrdinalIgnoreCase))
        {
            entity.PayrollLockStatus = "unlocked";
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync($"timekeeping.leave.{status}", tenantId, actorUserId, "leave_request", id.ToString(), "success", cancellationToken: cancellationToken);
        return MapLeaveRequest(entity);
    }

    private async Task<List<TimeEntryAggregate>> LoadEntriesAsync(Guid tenantId, Guid? personId, Guid? timesheetPeriodId, CancellationToken cancellationToken)
    {
        var query = db.TimekeepingTimeEntries.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (personId.HasValue)
        {
            query = query.Where(x => x.PersonId == personId.Value);
        }
        if (timesheetPeriodId.HasValue)
        {
            query = query.Where(x => x.TimesheetPeriodId == timesheetPeriodId.Value);
        }

        var entries = await query.ToListAsync(cancellationToken);
        var entryIds = entries.Select(x => x.Id).ToList();
        var allocations = entryIds.Count == 0
            ? []
            : await db.TimekeepingLaborAllocations.AsNoTracking().Where(a => a.TenantId == tenantId && entryIds.Contains(a.TimeEntryId)).ToListAsync(cancellationToken);

        return entries.Select(x => new TimeEntryAggregate
        {
            Id = x.Id,
            PersonId = x.PersonId,
            WorkSessionId = x.WorkSessionId,
            TimesheetPeriodId = x.TimesheetPeriodId,
            EntryDate = x.EntryDate,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            DurationMinutes = x.DurationMinutes,
            PayCodeId = x.PayCodeId,
            PayPolicyId = x.PayPolicyId,
            Classification = x.Classification,
            SourceProductKey = x.SourceProductKey,
            SourceRef = x.SourceRef,
            SourceConfidence = x.SourceConfidence,
            Description = x.Description,
            RequiresApproval = x.RequiresApproval,
            ApprovalStatus = x.ApprovalStatus,
            PayrollLockStatus = x.PayrollLockStatus,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            Allocations = allocations.Where(a => a.TimeEntryId == x.Id).Select(a => new LaborAllocationAggregate
            {
                Id = a.Id,
                AllocationPercent = a.AllocationPercent,
                AllocationMinutes = a.AllocationMinutes,
                ProductKey = a.ProductKey,
                CostObjectType = a.CostObjectType,
                CostObjectRef = a.CostObjectRef,
                LegalEntityRef = a.LegalEntityRef,
                SiteRef = a.SiteRef,
                DepartmentRef = a.DepartmentRef,
                CustomerRef = a.CustomerRef,
                OrderRef = a.OrderRef,
                AssetRef = a.AssetRef,
                WorkOrderRef = a.WorkOrderRef,
                TripRef = a.TripRef,
                RouteRef = a.RouteRef,
                WarehouseTaskRef = a.WarehouseTaskRef,
                TrainingSessionRef = a.TrainingSessionRef,
                QualityCaseRef = a.QualityCaseRef,
                ProjectRef = a.ProjectRef,
                GlDimensionSnapshot = a.GlDimensionSnapshot,
            }).ToList(),
        }).ToList();
    }

    private async Task<TimesheetPeriodResponse> ChangeTimesheetStatusAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid id,
        string status,
        CancellationToken cancellationToken,
        DateTimeOffset? submittedAt = null,
        DateTimeOffset? approvedAt = null,
        Guid? approvedByPersonId = null,
        DateTimeOffset? payrollReadyAt = null)
    {
        var period = await db.TimekeepingTimesheetPeriods.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.timesheet_not_found", "Timesheet period was not found.", 404);
        period.Status = status;
        period.SubmittedAt = submittedAt ?? period.SubmittedAt;
        period.ApprovedAt = approvedAt ?? period.ApprovedAt;
        period.ApprovedByPersonId = approvedByPersonId ?? period.ApprovedByPersonId;
        period.PayrollReadyAt = payrollReadyAt ?? period.PayrollReadyAt;
        period.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync($"timekeeping.timesheet.{status}", tenantId, actorUserId, "timesheet_period", id.ToString(), "success", cancellationToken: cancellationToken);
        return await GetTimesheetAsync(tenantId, id, cancellationToken);
    }

    private static TimeException NewException(Guid tenantId, Guid personId, Guid timesheetPeriodId, Guid? workSessionId, Guid? timeEntryId, string severity, string type, string message, string sourceProductKey, string? sourceRef)
    {
        return new TimeException
        {
            TenantId = tenantId,
            PersonId = personId,
            TimesheetPeriodId = timesheetPeriodId,
            WorkSessionId = workSessionId,
            TimeEntryId = timeEntryId,
            Severity = severity,
            ExceptionType = type,
            Message = message,
            SourceProductKey = sourceProductKey,
            SourceRef = sourceRef,
        };
    }

    private async Task EnsurePersonAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        var exists = await db.People.AnyAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException("staffarr.timekeeping.person_not_found", "Worker person record was not found.", 404);
        }
    }

    private async Task EnsureFieldCompanionClockAllowedAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        await EnsurePersonAsync(tenantId, personId, cancellationToken);
        var profile = await db.TimekeepingProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PersonId == personId, cancellationToken)
            ?? throw new StlApiException("staffarr.timekeeping.profile_not_found", "Timekeeping profile is required before mobile clock actions can be submitted.", 409);

        if (!string.Equals(profile.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("staffarr.timekeeping.profile_inactive", "Mobile clock actions require an active timekeeping profile.", 409);
        }

        if (!profile.AllowMobileClock)
        {
            throw new StlApiException("staffarr.timekeeping.mobile_clock_disabled", "Mobile clock is not enabled for this worker profile.", 403);
        }
    }

    private static void EnsureFieldCompanionIdempotencyMatch(
        ClockEvent existing,
        string eventType,
        DateTimeOffset eventTimestamp,
        string timezone,
        string? sourceDeviceId,
        string? geoPoint,
        string? siteRef,
        string? locationRef,
        string? notes)
    {
        if (!string.Equals(existing.EventType, eventType, StringComparison.Ordinal)
            || existing.EventTimestamp != eventTimestamp
            || !string.Equals(existing.Timezone, timezone, StringComparison.Ordinal)
            || !string.Equals(existing.SourceDeviceId, sourceDeviceId, StringComparison.Ordinal)
            || !string.Equals(existing.GeoPoint, geoPoint, StringComparison.Ordinal)
            || !string.Equals(existing.SiteRef, siteRef, StringComparison.Ordinal)
            || !string.Equals(existing.LocationRef, locationRef, StringComparison.Ordinal)
            || !string.Equals(existing.Notes, notes, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "staffarr.timekeeping.fieldcompanion.idempotency_conflict",
                "This mobile clock idempotency key was already used for a different punch payload.",
                409);
        }
    }

    private static IReadOnlyList<string> BuildSessionAnomalies(DateTimeOffset startTime, DateTimeOffset? endTime, int durationMinutes)
    {
        var anomalies = new List<string>();
        if (endTime is null)
        {
            anomalies.Add("missing_clock_out");
        }

        if (durationMinutes > 16 * 60)
        {
            anomalies.Add("excessive_shift_duration");
        }

        return anomalies;
    }

    private static IEnumerable<WorkSession> FindOverlappingSessions(IReadOnlyList<WorkSession> sessions)
    {
        return sessions
            .Where(x => x.EndTime.HasValue)
            .OrderBy(x => x.StartTime)
            .Zip(sessions.Where(x => x.EndTime.HasValue).OrderBy(x => x.StartTime).Skip(1))
            .Where(pair => pair.First.EndTime > pair.Second.StartTime)
            .Select(pair => pair.Second);
    }

    private static int CalculateDurationMinutes(DateTimeOffset? startTime, DateTimeOffset? endTime)
    {
        if (startTime is null || endTime is null || endTime <= startTime)
        {
            return 0;
        }

        return (int)Math.Round((endTime.Value - startTime.Value).TotalMinutes, MidpointRounding.AwayFromZero);
    }

    private static string InferClassification(string payCodeCategory, string activityType)
    {
        if (string.Equals(activityType, "training", StringComparison.OrdinalIgnoreCase))
        {
            return "training";
        }

        return payCodeCategory switch
        {
            "paid_leave" => "pto",
            "unpaid_leave" => "unpaid",
            "premium" => "overtime",
            _ => "regular",
        };
    }

    private static string Require(string? value, string message, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("staffarr.timekeeping.validation", message, 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("staffarr.timekeeping.validation", $"{message.TrimEnd('.')} must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private static string? Optional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("staffarr.timekeeping.validation", $"Value must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private static string NormalizeEnum(string value, IReadOnlyCollection<string> allowed, string fieldName)
    {
        var normalized = Require(value, $"{fieldName} is required.", 64).ToLowerInvariant();
        if (!allowed.Contains(normalized))
        {
            throw new StlApiException("staffarr.timekeeping.validation", $"{fieldName} is invalid.", 400);
        }

        return normalized;
    }

    private static string ComputeHash(string payload)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static Guid DeterministicGuidFromHash(string hash)
    {
        var bytes = Convert.FromHexString(hash[..32]);
        return new Guid(bytes);
    }

    private static TimekeepingProfileResponse MapProfile(TimekeepingProfile x) =>
        new(
            x.Id,
            x.PersonId,
            x.WorkerNumber,
            x.DefaultLegalEntityRef,
            x.DefaultSiteRef,
            x.DefaultDepartmentRef,
            x.DefaultPositionRef,
            x.DefaultSupervisorPersonId,
            x.PayPolicyId,
            x.PayrollEligibilityStatus,
            x.TimeEntryMode,
            x.OvertimeEligible,
            x.RequiresMealBreakAttestation,
            x.RequiresEndOfShiftAttestation,
            x.AllowMobileClock,
            x.AllowKioskClock,
            x.AllowManualCorrections,
            x.DefaultLaborAllocationTemplateId,
            x.EffectiveStartDate,
            x.EffectiveEndDate,
            x.Status,
            x.CreatedAt,
            x.UpdatedAt);

    private static PayPolicyResponse MapPayPolicy(PayPolicy x) =>
        new(
            x.Id,
            x.Name,
            x.Description,
            x.JurisdictionRefs,
            x.RoundingPolicy,
            x.MealBreakPolicy,
            x.RestBreakPolicy,
            x.OvertimePolicy,
            x.DoubleTimePolicy,
            x.HolidayPolicy,
            x.ShiftDifferentialPolicy,
            x.TravelTimePolicy,
            x.StandbyCalloutPolicy,
            x.ApprovalPolicy,
            x.CorrectionPolicy,
            x.AttestationPolicy,
            x.ComplianceRulepackRefs,
            x.EffectiveStartDate,
            x.EffectiveEndDate,
            x.Status);

    private static PayCodeResponse MapPayCode(PayCode x) =>
        new(x.Id, x.Code, x.DisplayName, x.Category, x.CountsTowardWorkedHours, x.CountsTowardOvertimeBase, x.RequiresAllocation, x.RequiresApproval, x.RequiresReason, x.Active, x.EffectiveStartDate, x.EffectiveEndDate);

    private static ClockEventResponse MapClockEvent(ClockEvent x) =>
        new(x.Id, x.PersonId, x.SourceProductKey, x.SourceDeviceType, x.SourceDeviceId, x.EventType, x.EventTimestamp, x.CapturedTimestamp, x.Timezone, x.SiteRef, x.LocationRef, x.SourceRef, x.ImmutableAuditHash, SplitCsv(x.AnomalyFlagsCsv));

    private static FieldCompanionClockEventResponse MapFieldCompanionClockEvent(ClockEvent x) =>
        new(x.Id, x.EventType, x.EventTimestamp, x.CapturedTimestamp, x.Timezone, x.SourceDeviceId, x.GeoPoint, x.SiteRef, x.LocationRef, x.Notes, SplitCsv(x.AnomalyFlagsCsv));

    private static WorkSessionResponse MapWorkSession(WorkSession x) =>
        new(x.Id, x.PersonId, x.SessionDate, x.StartTime, x.EndTime, x.Timezone, x.Status, x.SourceType, x.PrimarySourceProductKey, x.PrimarySourceRef, x.SiteRef, x.LocationRef, x.SupervisorPersonId, x.CalculatedDurationMinutes, x.PaidDurationMinutes, x.UnpaidBreakMinutes, x.RequiresReview, SplitCsv(x.AnomalyFlagsCsv), x.CreatedAt, x.UpdatedAt);

    private static LeaveRequestResponse MapLeaveRequest(LeaveRequest x) =>
        new(x.Id, x.PersonId, x.LeaveType, x.StartDate, x.EndDate, x.StartTime, x.EndTime, x.Timezone, x.IsIntermittent, x.IsPaid, x.Status, x.RequestedByPersonId, x.RequestedAt, x.ApprovedByPersonId, x.ReviewedAt, x.ReviewNotes, x.Reason, x.PayrollLockStatus, x.SourceProductKey, x.SourceRef, x.CreatedAt, x.UpdatedAt);

    private static AttendanceEventResponse MapAttendanceEvent(AttendanceEvent x) =>
        new(x.Id, x.PersonId, x.OccurredAt, x.EventType, x.Severity, x.PointValue, x.Status, x.Notes, x.SourceProductKey, x.SourceRef, x.RelatedLeaveRequestId, x.RelatedTimesheetPeriodId, x.ReviewedByPersonId, x.ReviewedAt, x.ResolutionNotes, x.CreatedAt, x.UpdatedAt);

    private static AvailabilityBlockResponse MapAvailabilityBlock(AvailabilityBlock x) =>
        new(x.Id, x.PersonId, x.AvailabilityType, x.DayOfWeekMaskCsv, x.StartLocalTime, x.EndLocalTime, x.Timezone, x.EffectiveStartDate, x.EffectiveEndDate, x.Status, x.Notes, x.CreatedAt, x.UpdatedAt);

    private static TimeEntryResponse MapTimeEntry(TimeEntryAggregate x) =>
        new(x.Id, x.PersonId, x.WorkSessionId, x.TimesheetPeriodId, x.EntryDate, x.StartTime, x.EndTime, x.DurationMinutes, x.PayCodeId, x.PayPolicyId, x.Classification, x.SourceProductKey, x.SourceRef, x.SourceConfidence, x.Description, x.RequiresApproval, x.ApprovalStatus, x.PayrollLockStatus, x.CreatedAt, x.UpdatedAt, MapAllocations(x.Allocations));

    private static IReadOnlyList<LaborAllocationResponse> MapAllocations(IReadOnlyList<LaborAllocationAggregate> allocations) =>
        allocations.Select(x => new LaborAllocationResponse(x.Id, x.AllocationPercent, x.AllocationMinutes, x.ProductKey, x.CostObjectType, x.CostObjectRef, x.LegalEntityRef, x.SiteRef, x.DepartmentRef, x.CustomerRef, x.OrderRef, x.AssetRef, x.WorkOrderRef, x.TripRef, x.RouteRef, x.WarehouseTaskRef, x.TrainingSessionRef, x.QualityCaseRef, x.ProjectRef, x.GlDimensionSnapshot)).ToList();

    private static TimesheetPeriodResponse MapTimesheet(TimesheetPeriod x, IReadOnlyList<TimeEntryAggregate> entries) =>
        new(x.Id, x.PersonId, x.PayrollCalendarRef, x.PeriodStartDate, x.PeriodEndDate, x.Status, x.SubmittedAt, x.ApprovedAt, x.ApprovedByPersonId, x.PayrollReadyAt, x.ExportedAt, x.TotalWorkedMinutes, x.TotalPaidMinutes, x.TotalUnpaidMinutes, x.OvertimeMinutes, x.ExceptionCount, x.AttestationStatus, x.CreatedAt, x.UpdatedAt, entries.Select(MapTimeEntry).ToList());

    private static TimeExceptionResponse MapException(TimeException x) =>
        new(x.Id, x.PersonId, x.TimesheetPeriodId, x.WorkSessionId, x.TimeEntryId, x.Severity, x.ExceptionType, x.Message, x.SourceProductKey, x.SourceRef, x.ResolutionStatus, x.ResolvedByPersonId, x.ResolvedAt, x.ResolutionNotes, x.CreatedAt);

    private static TimeCorrectionResponse MapCorrection(TimeCorrection x) =>
        new(x.Id, x.PersonId, x.TargetType, x.TargetId, x.RequestedByPersonId, x.ReasonCode, x.ReasonText, x.OldSnapshot, x.NewSnapshot, x.ApprovalStatus, x.ApprovedByPersonId, x.ApprovedAt, x.CreatedAt);

    private static TimeAttestationResponse MapAttestation(TimeAttestation x) =>
        new(x.Id, x.PersonId, x.TimesheetPeriodId, x.AttestationType, x.StatementText, x.Response, x.AttestedAt, x.AttestedByPersonId, x.SourceDeviceType, x.SourceProductKey, x.CreatedAt);

    private static IReadOnlyList<string> SplitCsv(string csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? []
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string ResolveFieldCompanionClockState(string? eventType) =>
        eventType?.ToLowerInvariant() switch
        {
            "clock_in" => "clocked_in",
            "start_break" => "on_break",
            "clock_out" => "clocked_out",
            "end_break" => "clocked_in",
            _ => "not_clocked_in",
        };

    private static string ResolveFieldCompanionClockSubmissionStatus(string anomalyFlagsCsv) =>
        SplitCsv(anomalyFlagsCsv).Count == 0 ? "accepted" : "flagged";

    private sealed class TimeEntryAggregate
    {
        public Guid Id { get; init; }
        public Guid PersonId { get; init; }
        public Guid? WorkSessionId { get; init; }
        public Guid TimesheetPeriodId { get; init; }
        public DateOnly EntryDate { get; init; }
        public DateTimeOffset? StartTime { get; init; }
        public DateTimeOffset? EndTime { get; init; }
        public int DurationMinutes { get; init; }
        public Guid PayCodeId { get; init; }
        public Guid? PayPolicyId { get; init; }
        public string Classification { get; init; } = string.Empty;
        public string SourceProductKey { get; init; } = string.Empty;
        public string? SourceRef { get; init; }
        public string SourceConfidence { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool RequiresApproval { get; init; }
        public string ApprovalStatus { get; init; } = string.Empty;
        public string PayrollLockStatus { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
        public IReadOnlyList<LaborAllocationAggregate> Allocations { get; init; } = [];
    }

    private sealed class LaborAllocationAggregate
    {
        public Guid Id { get; init; }
        public decimal AllocationPercent { get; init; }
        public int AllocationMinutes { get; init; }
        public string ProductKey { get; init; } = string.Empty;
        public string CostObjectType { get; init; } = string.Empty;
        public string CostObjectRef { get; init; } = string.Empty;
        public string LegalEntityRef { get; init; } = string.Empty;
        public string SiteRef { get; init; } = string.Empty;
        public string DepartmentRef { get; init; } = string.Empty;
        public string? CustomerRef { get; init; }
        public string? OrderRef { get; init; }
        public string? AssetRef { get; init; }
        public string? WorkOrderRef { get; init; }
        public string? TripRef { get; init; }
        public string? RouteRef { get; init; }
        public string? WarehouseTaskRef { get; init; }
        public string? TrainingSessionRef { get; init; }
        public string? QualityCaseRef { get; init; }
        public string? ProjectRef { get; init; }
        public string? GlDimensionSnapshot { get; init; }
    }
}

internal static class TimekeepingServiceExtensions
{
    public static TResult Let<T, TResult>(this T value, Func<T, TResult> map) => map(value);
}
