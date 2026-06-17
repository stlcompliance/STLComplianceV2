using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using NexArr.Api.Options;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Http;
using STLCompliance.Shared.SmartImport;

namespace NexArr.Api.Services;

public sealed class SmartImportDestinationCommitException(
    string errorCode,
    string message,
    bool retryable,
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public string ErrorCode { get; } = errorCode;
    public bool Retryable { get; } = retryable;
}

public sealed class SmartImportDestinationClient(
    IHttpClientFactory httpClientFactory,
    IOptions<PlatformProductUrlsOptions> productUrls,
    IOptions<StlServiceTokenOptions> serviceTokenOptions,
    IConfiguration configuration)
{
    public const string HttpClientName = "SmartImportDestinationClient";
    private const string CommitScope = "platform.smart_import.commit";

    public async Task<SmartImportDestinationCommitResponse> CommitAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        var destinationProduct = NormalizeProduct(request.DestinationProduct);
        var baseUrl = ResolveBaseUrl(destinationProduct);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new SmartImportDestinationCommitException(
                "smart_import.destination_url_missing",
                $"Smart Import destination URL is not configured for {destinationProduct}.",
                retryable: false);
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl.TrimEnd('/')}/api/v1/integrations/smart-import/{Uri.EscapeDataString(request.EntityType)}/commit")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            ResolveServiceToken(destinationProduct, request.TenantId));

        try
        {
            using var response = await client.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new SmartImportDestinationCommitException(
                    "smart_import.destination_commit_failed",
                    string.IsNullOrWhiteSpace(body)
                        ? $"Smart Import destination commit failed with HTTP {(int)response.StatusCode}."
                        : body,
                    retryable: IsRetryable(response.StatusCode));
            }

            return await response.Content.ReadFromJsonAsync<SmartImportDestinationCommitResponse>(
                    cancellationToken)
                ?? throw new SmartImportDestinationCommitException(
                    "smart_import.destination_commit_empty_response",
                    "Smart Import destination returned an empty commit response.",
                    retryable: true);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new SmartImportDestinationCommitException(
                "smart_import.destination_unavailable",
                $"Smart Import destination {destinationProduct} is unavailable.",
                retryable: true,
                ex);
        }
    }

    private string ResolveBaseUrl(string productKey)
    {
        var urls = productUrls.Value;
        return productKey switch
        {
            "staffarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.StaffArrBaseUrl),
            "trainarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.TrainArrBaseUrl),
            "maintainarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.MaintainArrBaseUrl),
            "routarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.RoutArrBaseUrl),
            "supplyarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.SupplyArrBaseUrl),
            "customarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.CustomArrBaseUrl),
            "ordarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.OrdArrBaseUrl),
            "compliancecore" => StlServiceUrl.NormalizeHttpBaseUrl(urls.ComplianceCoreBaseUrl),
            "loadarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.LoadArrBaseUrl),
            "assurarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.AssurArrBaseUrl),
            "reportarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.ReportArrBaseUrl),
            "recordarr" => StlServiceUrl.NormalizeHttpBaseUrl(urls.RecordArrBaseUrl),
            "fieldcompanion" => StlServiceUrl.NormalizeHttpBaseUrl(urls.FieldCompanionBaseUrl),
            _ => string.Empty
        };
    }

    private string ResolveServiceToken(string destinationProduct, Guid tenantId)
    {
        var configured = configuration["SmartImport:DestinationServiceToken"]
            ?? configuration["SmartImport__DestinationServiceToken"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        return CreateScopedCommitToken(destinationProduct, tenantId);
    }

    private string CreateScopedCommitToken(string destinationProduct, Guid tenantId)
    {
        var options = serviceTokenOptions.Value;
        var issuer = StlServiceTokenKeyMaterial.ResolveIssuer(configuration, options);
        var audience = StlServiceTokenKeyMaterial.ResolveAudience(configuration, options);
        var credentials = StlServiceTokenKeyMaterial.CreateSigningCredentials(configuration, options);
        var tokenId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
            new(StlServiceTokenClaimTypes.TokenType, StlServiceTokenClaimTypes.ServiceTokenTypeValue),
            new(StlServiceTokenClaimTypes.ServiceClientId, Guid.Empty.ToString()),
            new(StlServiceTokenClaimTypes.SourceProduct, "nexarr"),
            new(StlServiceTokenClaimTypes.AllowedProducts, destinationProduct),
            new(StlServiceTokenClaimTypes.TokenId, tokenId.ToString()),
            new(StlServiceTokenClaimTypes.TenantScope, tenantId.ToString()),
            new(StlServiceTokenClaimTypes.ActionScope, CommitScope)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool IsRetryable(System.Net.HttpStatusCode statusCode) =>
        statusCode is System.Net.HttpStatusCode.RequestTimeout
            or System.Net.HttpStatusCode.TooManyRequests
            or System.Net.HttpStatusCode.BadGateway
            or System.Net.HttpStatusCode.ServiceUnavailable
            or System.Net.HttpStatusCode.GatewayTimeout
        || (int)statusCode >= 500;

    private static string NormalizeProduct(string productKey) =>
        productKey.Trim().ToLowerInvariant() switch
        {
            "compliance-core" or "compliance_core" => "compliancecore",
            "field-companion" or "field_companion" => "fieldcompanion",
            var normalized => normalized
        };
}
