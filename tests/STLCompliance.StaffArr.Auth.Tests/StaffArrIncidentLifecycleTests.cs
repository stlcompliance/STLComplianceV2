using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class StaffArrIncidentLifecycleTests : IAsyncLifetime
{
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _staffarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"StaffArrIncidentLifecycle-{Guid.NewGuid():N}";

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Incident_status_lifecycle_closes_reopens_and_tracks_timeline()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Incident", "Lifecycle", "incident.lifecycle@example.com");

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin", personId: personId);

        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", token);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "safety",
            "medium",
            "Dock slip incident",
            "Employee slipped on a wet dock surface during inbound operations and the case needs follow-up.",
            DateTimeOffset.UtcNow.AddHours(-4)));

        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.Equal("open", created.Status);

        var closeRequest = Authorized(HttpMethod.Patch, $"/api/v1/incidents/{created.IncidentId}/status", token);
        closeRequest.Content = JsonContent.Create(new UpdatePersonnelIncidentStatusRequest("closed"));
        var closeResponse = await _staffarrClient.SendAsync(closeRequest);
        closeResponse.EnsureSuccessStatusCode();
        var closed = (await closeResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.Equal("closed", closed.Status);
        Assert.True(closed.UpdatedAt >= created.UpdatedAt);

        var reopenRequest = Authorized(HttpMethod.Patch, $"/api/incidents/{created.IncidentId}/status", token);
        reopenRequest.Content = JsonContent.Create(new UpdatePersonnelIncidentStatusRequest("open"));
        var reopenResponse = await _staffarrClient.SendAsync(reopenRequest);
        reopenResponse.EnsureSuccessStatusCode();
        var reopened = (await reopenResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.Equal("open", reopened.Status);

        var timelineResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/timeline?page=1&pageSize=25", token));
        timelineResponse.EnsureSuccessStatusCode();
        var timeline = (await timelineResponse.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;
        Assert.Contains(timeline.Items, x => x.EventType == "incident_reported");
        Assert.Contains(timeline.Items, x => x.EventType == "incident_closed");
        Assert.Contains(timeline.Items, x => x.EventType == "incident_reopened");

        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var auditCount = await db.AuditEvents.CountAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.TargetId == created.IncidentId.ToString()
                && x.Action == "incident.status_update");
        Assert.Equal(2, auditCount);
    }

    [Fact]
    public async Task Incident_notes_corrective_actions_and_attachments_are_tracked()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "Incident", "Evidence", "incident.evidence@example.com");

        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin", personId: personId);

        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", token);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "safety",
            "medium",
            "Lighting issue in dock area",
            "A dock light is out and the area needs follow-up, corrective action, and evidence uploads.",
            DateTimeOffset.UtcNow.AddHours(-2)));

        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;

        var noteRequest = Authorized(HttpMethod.Post, $"/api/incidents/{created.IncidentId}/notes", token);
        noteRequest.Content = JsonContent.Create(new CreateIncidentNoteRequest(
            "note",
            "Investigation started",
            "The shift lead has started investigating the dock lighting issue."));
        var noteResponse = await _staffarrClient.SendAsync(noteRequest);
        noteResponse.EnsureSuccessStatusCode();
        var withNote = (await noteResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.Single(withNote.Notes!);

        var correctiveRequest = Authorized(HttpMethod.Post, $"/api/incidents/{created.IncidentId}/notes", token);
        correctiveRequest.Content = JsonContent.Create(new CreateIncidentNoteRequest(
            "corrective_action",
            "Replace dock bulb",
            "Facilities should replace the dock bulb and verify the light is working.",
            DateTimeOffset.UtcNow.AddDays(1)));
        var correctiveResponse = await _staffarrClient.SendAsync(correctiveRequest);
        correctiveResponse.EnsureSuccessStatusCode();
        var withCorrectiveAction = (await correctiveResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        var correctiveAction = Assert.Single(withCorrectiveAction.Notes!, x => x.NoteTypeKey == "corrective_action");

        var completeRequest = Authorized(HttpMethod.Patch, $"/api/incidents/{created.IncidentId}/notes/{correctiveAction.NoteId}/status", token);
        completeRequest.Content = JsonContent.Create(new UpdateIncidentNoteStatusRequest("completed"));
        var completeResponse = await _staffarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completedDetail = (await completeResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.Contains(completedDetail.Notes!, x => x.NoteId == correctiveAction.NoteId && x.Status == "completed");

        var attachmentBytes = "dock-light-evidence"u8.ToArray();
        var attachmentRequest = Authorized(HttpMethod.Post, $"/api/incidents/{created.IncidentId}/attachments", token);
        attachmentRequest.Content = JsonContent.Create(new CreateIncidentAttachmentRequest(
            "Dock light photo",
            "dock-light.txt",
            "text/plain",
            Convert.ToBase64String(attachmentBytes),
            "Evidence from the shift lead."));
        var attachmentResponse = await _staffarrClient.SendAsync(attachmentRequest);
        attachmentResponse.EnsureSuccessStatusCode();
        var withAttachment = (await attachmentResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;
        Assert.Single(withAttachment.Attachments!);

        var attachment = withAttachment.Attachments!.Single();
        var downloadResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/incidents/{created.IncidentId}/attachments/{attachment.AttachmentId}/content", token));
        downloadResponse.EnsureSuccessStatusCode();
        var downloadedBytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        Assert.Equal(attachmentBytes, downloadedBytes);

        var timelineResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/timeline?page=1&pageSize=25", token));
        timelineResponse.EnsureSuccessStatusCode();
        var timeline = (await timelineResponse.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;
        Assert.Contains(timeline.Items, x => x.EventType == "incident_note_added");
        Assert.Contains(timeline.Items, x => x.EventType == "incident_corrective_action_added");
        Assert.Contains(timeline.Items, x => x.EventType == "incident_corrective_action_completed");
        Assert.Contains(timeline.Items, x => x.EventType == "incident_attachment_uploaded");
    }

    private async Task SeedPersonAsync(Guid personId, string givenName, string familyName, string email)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = givenName,
            FamilyName = familyName,
            DisplayName = $"{givenName} {familyName}",
            PrimaryEmail = email,
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
