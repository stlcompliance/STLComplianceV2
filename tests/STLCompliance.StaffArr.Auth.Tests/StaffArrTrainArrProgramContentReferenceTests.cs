using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrProgramContentReferenceTests : IAsyncLifetime
{
    private WebApplicationFactory<global::TrainArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"TrainArrContentReferences-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Training_program_content_references_attach_list_remove_and_project_from_program_detail()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var programId = await CreateTrainingProgramAsync(adminToken, definitionId);

        var attachRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-programs/{programId}/content-references",
            adminToken);
        attachRequest.Content = JsonContent.Create(new CreateTrainingProgramContentReferenceRequest(
            TrainingProgramContentReferenceTypes.ExternalUrl,
            "Policy source",
            "https://example.com/policy",
            "Reference notes",
            "en-us"));
        var attachResponse = await _client.SendAsync(attachRequest);
        attachResponse.EnsureSuccessStatusCode();
        var attached = (await attachResponse.Content.ReadFromJsonAsync<TrainingProgramContentReferenceResponse>())!;
        Assert.Equal(TrainingProgramContentReferenceTypes.ExternalUrl, attached.ContentType);
        Assert.Equal("Policy source", attached.Title);
        Assert.Equal("Reference notes", attached.Notes);
        Assert.Equal("en-us", attached.LocaleTag);

        var listResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-programs/{programId}/content-references", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingProgramContentReferenceResponse>>())!;
        Assert.Single(listed);

        var detailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-programs/{programId}", adminToken));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<TrainingProgramDetailResponse>())!;
        Assert.Single(detail.ContentReferences);

        var removeResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Delete,
                $"/api/training-programs/{programId}/content-references/{attached.ContentReferenceId}",
                adminToken));
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        var listAfterRemove = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-programs/{programId}/content-references", adminToken));
        listAfterRemove.EnsureSuccessStatusCode();
        var remaining = (await listAfterRemove.Content.ReadFromJsonAsync<IReadOnlyList<TrainingProgramContentReferenceResponse>>())!;
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task Training_program_content_references_reject_duplicate()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var programId = await CreateTrainingProgramAsync(adminToken, definitionId);
        var payload = new CreateTrainingProgramContentReferenceRequest(
            TrainingProgramContentReferenceTypes.PolicyDocument,
            "Policy source",
            "POL-001",
            null,
            "en-us");

        var attachRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-programs/{programId}/content-references",
            adminToken);
        attachRequest.Content = JsonContent.Create(payload);
        (await _client.SendAsync(attachRequest)).EnsureSuccessStatusCode();

        var duplicateRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-programs/{programId}/content-references",
            adminToken);
        duplicateRequest.Content = JsonContent.Create(payload);
        var duplicateResponse = await _client.SendAsync(duplicateRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Training_program_content_references_deny_member_role()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var programId = await CreateTrainingProgramAsync(adminToken, definitionId);
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");

        var attachRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-programs/{programId}/content-references",
            memberToken);
        attachRequest.Content = JsonContent.Create(new CreateTrainingProgramContentReferenceRequest(
            TrainingProgramContentReferenceTypes.ExternalUrl,
            "Policy source",
            "https://example.com/policy",
            null,
            "es-mx"));
        var attachResponse = await _client.SendAsync(attachRequest);
        Assert.Equal(HttpStatusCode.Forbidden, attachResponse.StatusCode);
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string adminToken)
    {
        var definitionKey = $"content_ref_def_{Guid.NewGuid():N}"[..20];
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", adminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            definitionKey,
            "Content reference definition",
            "Training definition for content reference tests.",
            "hazmat_endorsement",
            "Hazmat Endorsement"));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return created.TrainingDefinitionId;
    }

    private async Task<Guid> CreateTrainingProgramAsync(string adminToken, Guid definitionId)
    {
        var programKey = $"content_ref_prog_{Guid.NewGuid():N}"[..20];
        var request = Authorized(HttpMethod.Post, "/api/training-programs", adminToken);
        request.Content = JsonContent.Create(new CreateTrainingProgramRequest(
            programKey,
            "Content reference program",
            "Program for content reference tests.",
            [definitionId]));
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<TrainingProgramDetailResponse>())!;
        return created.ProgramId;
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member")
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArr.Api.Services.TrainArrTokenService>();
        var userId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var (accessToken, _) = tokenService.CreateAccessToken(
            userId,
            personId,
            "admin@example.com",
            "Test Admin",
            tenantId,
            sessionId,
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
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<TContext>)
            || d.ServiceType == typeof(TContext)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
