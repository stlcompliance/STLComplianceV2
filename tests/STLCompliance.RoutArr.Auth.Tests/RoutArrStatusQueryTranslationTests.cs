using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrStatusQueryTranslationTests
{
    [Fact]
    public void Status_set_queries_translate_for_postgres_provider()
    {
        var options = new DbContextOptionsBuilder<RoutArrDbContext>()
            .UseNpgsql("Host=localhost;Database=routarr_translation_test;Username=test;Password=test")
            .Options;

        using var db = new RoutArrDbContext(options);
        var tenantId = Guid.NewGuid();
        var personId = Guid.NewGuid().ToString();
        var tripIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var asOf = DateTimeOffset.UtcNow;

        var exceptionQueueSql = db.DispatchExceptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .WhereDispatchExceptionOpenQueue()
            .OrderByDescending(x => x.UpdatedAt)
            .Take(200)
            .ToQueryString();

        var overdueExceptionSql = db.DispatchExceptions
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.SlaDueAt != null
                && x.SlaDueAt < asOf)
            .WhereDispatchExceptionOpenQueue()
            .ToQueryString();

        var driverPortalScheduleSql = db.Trips
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.AssignedDriverPersonId == personId)
            .WhereDriverPortalScheduleStatus()
            .OrderBy(x => x.ScheduledStartAt ?? DateTimeOffset.MaxValue)
            .ThenBy(x => x.TripNumber)
            .ToQueryString();

        var activeTripExceptionCountsSql = db.DispatchExceptions
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.TripId.HasValue
                && tripIds.Contains(x.TripId.Value))
            .WhereDispatchExceptionOpenQueue()
            .GroupBy(x => x.TripId!.Value)
            .Select(x => new { TripId = x.Key, Count = x.Count() })
            .ToQueryString();

        var fieldInboxSql = db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .WhereActiveDispatchStatus()
            .OrderByDescending(x => x.ScheduledStartAt ?? x.CreatedAt)
            .Take(50)
            .ToQueryString();

        Assert.Contains("open", exceptionQueueSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("assigned", overdueExceptionSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("completed", driverPortalScheduleSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("GROUP BY", activeTripExceptionCountsSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("planned", fieldInboxSql, StringComparison.OrdinalIgnoreCase);
    }
}
