using StaffArr.Api.Options;
using StaffArr.Api.Services;

namespace StaffArr.Api;

public static class StaffArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<NexArrClientOptions>(builder.Configuration.GetSection(NexArrClientOptions.SectionName));
        builder.Services.Configure<HandoffOptions>(builder.Configuration.GetSection(HandoffOptions.SectionName));

        builder.Services.AddHttpClient<NexArrHandoffClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NexArrClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        });

        builder.Services.AddScoped<StaffArrTokenService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<MeService>();
        builder.Services.AddScoped<StaffArrAuthorizationService>();
        builder.Services.AddScoped<PersonProvisioningService>();
        builder.Services.AddScoped<PeopleService>();
        builder.Services.AddScoped<ManagerHierarchyService>();
        builder.Services.AddScoped<OrgUnitService>();
        builder.Services.AddScoped<OrgUnitAssignmentService>();
        builder.Services.AddScoped<RoleTemplateService>();
        builder.Services.AddScoped<IStaffArrAuditService, StaffArrAuditService>();

        var frontendOrigin = builder.Configuration["Cors:StaffArrFrontendOrigin"] ?? "http://localhost:5175";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("StaffArrFrontend", policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("StaffArrFrontend");
    }
}
