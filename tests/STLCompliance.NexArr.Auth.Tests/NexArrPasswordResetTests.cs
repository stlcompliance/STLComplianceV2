using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrPasswordResetTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrPasswordResetTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<NexArrDbContext>)
                        || d.ServiceType == typeof(NexArrDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<NexArrDbContext>(options =>
                    options.UseInMemoryDatabase("NexArrPasswordResetTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Forgot_password_returns_generic_message_for_unknown_email()
    {
        await SeedDatabaseAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/auth/password/forgot",
            new ForgotPasswordRequest("nobody@example.com"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(payload);
        Assert.Null(payload.DevResetToken);
    }

    [Fact]
    public async Task Forgot_password_issues_dev_token_for_active_user()
    {
        await SeedDatabaseAsync();
        var response = await _client.PostAsJsonAsync(
            "/api/auth/password/forgot",
            new ForgotPasswordRequest(PlatformSeeder.DemoAdminEmail));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.DevResetToken));
    }

    [Fact]
    public async Task Reset_password_updates_credential_and_revokes_sessions()
    {
        await SeedDatabaseAsync();
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        var tokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        Assert.NotNull(tokens);

        var forgotResponse = await _client.PostAsJsonAsync(
            "/api/auth/password/forgot",
            new ForgotPasswordRequest(PlatformSeeder.DemoAdminEmail));
        var forgot = await forgotResponse.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(forgot?.DevResetToken);

        const string newPassword = "NewSecurePass1!";
        var resetResponse = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(forgot.DevResetToken, newPassword));
        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);

        var oldLogin = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var newLogin = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(PlatformSeeder.DemoAdminEmail, newPassword, PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);

        var renewResponse = await _client.PostAsJsonAsync(
            "/api/auth/renew",
            new RenewSessionRequest(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, renewResponse.StatusCode);
    }

    [Fact]
    public async Task Reset_password_rejects_reused_token()
    {
        await SeedDatabaseAsync();
        var forgot = await _client.PostAsJsonAsync(
            "/api/auth/password/forgot",
            new ForgotPasswordRequest(PlatformSeeder.DemoAdminEmail));
        var payload = await forgot.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(payload?.DevResetToken);

        const string newPassword = "AnotherSecure1!";
        var firstReset = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(payload.DevResetToken, newPassword));
        Assert.Equal(HttpStatusCode.NoContent, firstReset.StatusCode);

        var secondReset = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(payload.DevResetToken, "ThirdSecurePass1!"));
        Assert.Equal(HttpStatusCode.BadRequest, secondReset.StatusCode);
    }

    [Fact]
    public async Task Reset_password_rejects_weak_password()
    {
        await SeedDatabaseAsync();
        var forgot = await _client.PostAsJsonAsync(
            "/api/auth/password/forgot",
            new ForgotPasswordRequest(PlatformSeeder.DemoAdminEmail));
        var payload = await forgot.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(payload?.DevResetToken);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(payload.DevResetToken, "short"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task SeedDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }
}
