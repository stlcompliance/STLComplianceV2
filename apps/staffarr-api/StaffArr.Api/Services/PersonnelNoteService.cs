using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PersonnelNoteService(StaffArrDbContext db, IStaffArrAuditService audit)
{
    private static readonly HashSet<string> AllowedCategoryKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "general",
        "performance",
        "coaching",
        "disciplinary",
        "medical",
        "other"
    };

    private static readonly HashSet<string> AllowedVisibilityKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "hr_only",
        "management",
        "personnel_visible"
    };

    public async Task<PersonnelNoteDetailResponse> CreateNoteAsync(
        Guid tenantId,
        Guid personId,
        Guid actorUserId,
        CreatePersonnelNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        var categoryKey = NormalizeCategoryKey(request.CategoryKey);
        var visibilityKey = NormalizeVisibilityKey(request.VisibilityKey);
        var subject = NormalizeSubject(request.Subject);
        var body = NormalizeBody(request.Body);
        var now = DateTimeOffset.UtcNow;

        var entity = new PersonnelNote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            CategoryKey = categoryKey,
            VisibilityKey = visibilityKey,
            Subject = subject,
            Body = body,
            Status = "active",
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PersonnelNotes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "personnel_note.create",
            tenantId,
            actorUserId,
            "personnel_note",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(entity);
    }

    public async Task<IReadOnlyList<PersonnelNoteSummaryResponse>> ListNotesAsync(
        Guid tenantId,
        Guid personId,
        Func<PersonnelNote, bool> visibilityFilter,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        var notes = await db.PersonnelNotes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId && x.Status == "active")
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return notes
            .Where(visibilityFilter)
            .Select(MapSummary)
            .ToList();
    }

    public async Task<PersonnelNoteDetailResponse> GetNoteAsync(
        Guid tenantId,
        Guid personId,
        Guid noteId,
        Func<PersonnelNote, bool> visibilityFilter,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PersonnelNotes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PersonId == personId && x.Id == noteId,
                cancellationToken);

        if (entity is null)
        {
            throw new StlApiException("personnel_notes.not_found", "Personnel note was not found.", 404);
        }

        if (!visibilityFilter(entity))
        {
            throw new StlApiException("auth.forbidden", "You do not have access to this personnel note.", 403);
        }

        return MapDetail(entity);
    }

    private async Task EnsurePersonExistsAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        var personExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }
    }

    private static string NormalizeCategoryKey(string categoryKey)
    {
        var normalized = categoryKey.Trim().ToLowerInvariant();
        if (!AllowedCategoryKeys.Contains(normalized))
        {
            throw new StlApiException(
                "personnel_notes.validation",
                $"Category must be one of: {string.Join(", ", AllowedCategoryKeys.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeVisibilityKey(string visibilityKey)
    {
        var normalized = visibilityKey.Trim().ToLowerInvariant();
        if (!AllowedVisibilityKeys.Contains(normalized))
        {
            throw new StlApiException(
                "personnel_notes.validation",
                $"Visibility must be one of: {string.Join(", ", AllowedVisibilityKeys.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeSubject(string subject)
    {
        var trimmed = subject.Trim();
        if (trimmed.Length < 4)
        {
            throw new StlApiException(
                "personnel_notes.validation",
                "Note subject must be at least 4 characters.",
                400);
        }

        if (trimmed.Length > 200)
        {
            throw new StlApiException(
                "personnel_notes.validation",
                "Note subject must be 200 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeBody(string body)
    {
        var trimmed = body.Trim();
        if (trimmed.Length < 8)
        {
            throw new StlApiException(
                "personnel_notes.validation",
                "Note body must be at least 8 characters.",
                400);
        }

        if (trimmed.Length > 8192)
        {
            throw new StlApiException(
                "personnel_notes.validation",
                "Note body must be 8192 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static PersonnelNoteSummaryResponse MapSummary(PersonnelNote entity) =>
        new(
            entity.Id,
            entity.PersonId,
            entity.CategoryKey,
            entity.VisibilityKey,
            entity.Subject,
            entity.Status,
            entity.CreatedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static PersonnelNoteDetailResponse MapDetail(PersonnelNote entity) =>
        new(
            entity.Id,
            entity.PersonId,
            entity.CategoryKey,
            entity.VisibilityKey,
            entity.Subject,
            entity.Body,
            entity.Status,
            entity.CreatedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt);
}
