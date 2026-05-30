using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class CertificationPublicationEndpoints
{
    public const string PublicationActionScope = "trainarr.certification_publications.write";

    public static void MapTrainArrCertificationPublicationEndpoints(this WebApplication app)
    {
        var routes = new[]
        {
            (Route: "/api/certification-publications", Suffix: string.Empty),
            (Route: "/api/v1/certification-publications", Suffix: "V1CertificationPublications"),
            (Route: "/api/v1/certificates", Suffix: "V1Certificates"),
        };

        foreach (var (route, suffix) in routes)
        {
            var publications = app.MapGroup(route)
                .WithTags("CertificationPublications");

            publications.MapPost("/", async (
            CreateCertificationPublicationRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            CertificationPublicationService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "trainarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = PublicationActionScope
                });

            var created = await service.PublishTrainingBlockerAsync(request, cancellationToken);
            return Results.Created($"/api/certification-publications/{created.PublicationId}", created);
        })
        .WithName($"CreateCertificationPublication{suffix}");
        }
    }
}
