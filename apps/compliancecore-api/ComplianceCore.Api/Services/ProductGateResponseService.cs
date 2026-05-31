using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ProductGateResponseService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public const string RecordResponseActionScope = "compliancecore.product_gates.respond";

    public async Task<ProductGateResponseItemResponse> CreateAsync(
        CreateProductGateResponseRequest request,
        string sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new StlApiException("product_gate_response.validation", "Tenant id is required.", 400);
        }

        if (request.CheckResultId == Guid.Empty)
        {
            throw new StlApiException("product_gate_response.validation", "Check result id is required.", 400);
        }

        var responseOutcome = NormalizeRequiredKey(request.ResponseOutcome, "Response outcome");
        var checkExists = await db.WorkflowGateCheckResults
            .AnyAsync(
                x => x.TenantId == request.TenantId && x.Id == request.CheckResultId,
                cancellationToken);
        if (!checkExists)
        {
            throw new StlApiException(
                "product_gate_response.check_not_found",
                "Workflow gate check result was not found.",
                404);
        }

        var payload = request.ResponsePayload ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var persisted = new ProductGateResponse
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            WorkflowGateCheckResultId = request.CheckResultId,
            SourceProduct = NormalizeRequiredKey(sourceProductKey, "Source product"),
            ResponseOutcome = responseOutcome,
            ResponseCode = NormalizeOptional(request.ResponseCode),
            ResponseMessage = NormalizeOptional(request.ResponseMessage),
            ResponsePayloadJson = JsonSerializer.Serialize(payload),
            RespondedAt = DateTimeOffset.UtcNow,
        };

        db.ProductGateResponses.Add(persisted);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "product_gates.response.recorded",
            request.TenantId,
            actorUserId: null,
            "workflow_gate_check_result",
            request.CheckResultId.ToString(),
            persisted.ResponseOutcome,
            reasonCode: persisted.SourceProduct,
            cancellationToken: cancellationToken);

        return MapResponse(persisted);
    }

    public async Task<ProductGateResponseListResponse> ListAsync(
        Guid tenantId,
        Guid checkResultId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty || checkResultId == Guid.Empty)
        {
            throw new StlApiException(
                "product_gate_response.validation",
                "Tenant id and check result id are required.",
                400);
        }

        var items = await db.ProductGateResponses
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkflowGateCheckResultId == checkResultId)
            .OrderByDescending(x => x.RespondedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return new ProductGateResponseListResponse(
            checkResultId,
            items.Count,
            items.Select(MapResponse).ToList());
    }

    private static ProductGateResponseItemResponse MapResponse(ProductGateResponse entity)
    {
        var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.ResponsePayloadJson)
            ?? [];
        return new ProductGateResponseItemResponse(
            entity.Id,
            entity.TenantId,
            entity.WorkflowGateCheckResultId,
            entity.SourceProduct,
            entity.ResponseOutcome,
            entity.ResponseCode,
            entity.ResponseMessage,
            payload,
            entity.RespondedAt);
    }

    private static string NormalizeRequiredKey(string value, string label)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length == 0)
        {
            throw new StlApiException("product_gate_response.validation", $"{label} is required.", 400);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
