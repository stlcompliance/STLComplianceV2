using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class QualificationWalletService(
    TrainArrDbContext db,
    QualificationReportService reportService,
    IOptions<StlJwtOptions> options,
    IConfiguration configuration)
{
    private const string WalletAudience = "trainarr-qualification-wallet";
    private const string WalletCredentialType = "qualification_wallet_credential";

    public async Task<QualificationWalletCredentialResponse> GetCredentialAsync(
        Guid tenantId,
        Guid qualificationIssueId,
        HttpRequest request,
        CancellationToken cancellationToken = default)
    {
        var issue = await LoadIssueAsync(tenantId, qualificationIssueId, cancellationToken);
        var generatedAt = DateTimeOffset.UtcNow;
        var token = CreateCredentialToken(issue, generatedAt);
        return BuildCredentialResponse(issue, generatedAt, GetBaseUrl(request), token);
    }

    public async Task<QualificationWalletVerificationResponse> VerifyCredentialAsync(
        Guid tenantId,
        string credentialToken,
        HttpRequest request,
        CancellationToken cancellationToken = default)
    {
        var verifiedAt = DateTimeOffset.UtcNow;
        var token = credentialToken.Trim();
        if (token.Length == 0)
        {
            return new QualificationWalletVerificationResponse(
                verifiedAt,
                false,
                "Credential token is required.",
                null,
                null);
        }

        ClaimsPrincipal principal;
        try
        {
            principal = ValidateToken(token);
        }
        catch (SecurityTokenException ex)
        {
            throw new StlApiException(
                "qualification_wallet.invalid_token",
                ex.Message,
                400);
        }
        catch (ArgumentException ex)
        {
            throw new StlApiException(
                "qualification_wallet.invalid_token",
                ex.Message,
                400);
        }

        var qualificationIssueId = ExtractGuidClaim(principal, JwtRegisteredClaimNames.Sub);
        var tokenTenantId = ExtractGuidClaim(principal, "tenantId");
        if (tokenTenantId != tenantId)
        {
            return new QualificationWalletVerificationResponse(
                verifiedAt,
                false,
                "Credential was issued for a different tenant.",
                null,
                null);
        }

        var issue = await LoadIssueAsync(tenantId, qualificationIssueId, cancellationToken);
        var credential = BuildCredentialResponse(issue, verifiedAt, GetBaseUrl(request), token);
        var report = await reportService.GetPointInTimeReportAsync(
            tenantId,
            issue.StaffarrPersonId,
            issue.QualificationKey,
            "smart badge verification",
            verifiedAt,
            cancellationToken);

        var isValid = report.IsQualified;
        var message = isValid
            ? $"Credential is valid for {report.QualificationName}."
            : report.QualificationMessage;

        return new QualificationWalletVerificationResponse(
            verifiedAt,
            isValid,
            message,
            credential,
            report);
    }

    private async Task<QualificationIssue> LoadIssueAsync(
        Guid tenantId,
        Guid qualificationIssueId,
        CancellationToken cancellationToken)
    {
        var issue = await db.QualificationIssues.AsNoTracking().FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == qualificationIssueId,
            cancellationToken);
        if (issue is null)
        {
            throw new StlApiException(
                "qualification_wallet.not_found",
                "Qualification credential was not found.",
                404);
        }

        return issue;
    }

    private string CreateCredentialToken(QualificationIssue issue, DateTimeOffset generatedAt)
    {
        var signingKey = configuration["AUTH_SIGNING_KEY"] ?? options.Value.SigningKey;
        var jwtOptions = options.Value;
        var expiresAt = generatedAt.AddYears(10);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, issue.Id.ToString()),
            new("tenantId", issue.TenantId.ToString()),
            new("personId", issue.StaffarrPersonId.ToString()),
            new("qualificationKey", issue.QualificationKey),
            new("qualificationName", issue.QualificationName),
            new("status", issue.Status),
            new("issuedAt", issue.IssuedAt.ToString("O")),
            new("expiresAt", issue.ExpiresAt?.ToString("O") ?? string.Empty),
            new("grantPublicationId", issue.GrantPublicationId.ToString()),
            new("credentialType", WalletCredentialType),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: WalletAudience,
            claims: claims,
            notBefore: generatedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal ValidateToken(string credentialToken)
    {
        var signingKey = configuration["AUTH_SIGNING_KEY"] ?? options.Value.SigningKey;
        var jwtOptions = options.Value;
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = WalletAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
        };

        var handler = new JwtSecurityTokenHandler();
        handler.MapInboundClaims = false;
        var principal = handler.ValidateToken(credentialToken, parameters, out _);
        var credentialType = principal.FindFirst("credentialType")?.Value;
        if (!string.Equals(credentialType, WalletCredentialType, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "qualification_wallet.invalid_token",
                "Credential token is not a TrainArr qualification wallet credential.",
                400);
        }

        return principal;
    }

    private static QualificationWalletCredentialResponse BuildCredentialResponse(
        QualificationIssue issue,
        DateTimeOffset generatedAt,
        string baseUrl,
        string credentialToken)
    {
        return new QualificationWalletCredentialResponse(
            issue.Id,
            issue.StaffarrPersonId,
            issue.QualificationKey,
            issue.QualificationName,
            issue.Status,
            issue.IssuedAt,
            issue.ExpiresAt,
            generatedAt,
            credentialToken,
            $"{baseUrl}/api/v1/qualifications/wallet/verify",
            $"{issue.QualificationName} credential");
    }

    private static Guid ExtractGuidClaim(ClaimsPrincipal principal, string claimType)
    {
        var value = principal.FindFirst(claimType)?.Value;
        if (!Guid.TryParse(value, out var guid))
        {
            throw new StlApiException(
                "qualification_wallet.invalid_token",
                $"Credential token is missing the '{claimType}' claim.",
                400);
        }

        return guid;
    }

    private static string GetBaseUrl(HttpRequest request)
    {
        var scheme = request.Scheme;
        var host = request.Host.HasValue ? request.Host.Value : "localhost";
        return $"{scheme}://{host}";
    }
}
