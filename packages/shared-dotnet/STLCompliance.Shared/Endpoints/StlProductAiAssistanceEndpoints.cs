using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using STLCompliance.Shared.Integration;

namespace STLCompliance.Shared.Endpoints;

public static class StlProductAiAssistanceEndpoints
{
    public static void MapStlProductAiAssistanceEndpoints(this WebApplication app)
    {
        static async Task<IResult> ForwardAssistantMessageAsync(
            HttpContext context,
            StlNexArrLaunchClient client,
            CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(context.Request.Body);
            var jsonBody = await reader.ReadToEndAsync(cancellationToken);

            var (statusCode, body, contentType) = await client.ForwardAsync(
                HttpMethod.Post,
                "/api/v1/ai/assistant/messages",
                context.Request.Headers.Authorization.ToString(),
                jsonBody,
                cancellationToken);

            return Results.Content(body, contentType, statusCode: statusCode);
        }

        var group = app.MapGroup("/api/ai").WithTags("AI Assistance").RequireAuthorization();

        group.MapPost("/assistant/messages", ForwardAssistantMessageAsync)
            .WithName("CreateProductAiAssistantMessage");

        var v1Group = app.MapGroup("/api/v1/ai").WithTags("AI Assistance").RequireAuthorization();

        v1Group.MapPost("/assistant/messages", ForwardAssistantMessageAsync)
            .WithName("CreateProductAiAssistantMessageV1");
    }
}
