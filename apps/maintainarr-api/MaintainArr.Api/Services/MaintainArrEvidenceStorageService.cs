using Microsoft.Extensions.Options;
using MaintainArr.Api.Options;

namespace MaintainArr.Api.Services;

public sealed class MaintainArrEvidenceStorageService(IHostEnvironment environment, IOptions<EvidenceStorageOptions> options)
{
    private readonly string _rootPath = ResolveRootPath(environment.ContentRootPath, options.Value.RootPath);

    public Task<string> SaveWorkOrderEvidenceAsync(
        Guid tenantId,
        Guid workOrderId,
        Guid evidenceId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default) =>
        SaveAsync(tenantId, "work-orders", workOrderId, evidenceId, fileName, content, cancellationToken);

    public Task<string> SaveDefectEvidenceAsync(
        Guid tenantId,
        Guid defectId,
        Guid evidenceId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default) =>
        SaveAsync(tenantId, "defects", defectId, evidenceId, fileName, content, cancellationToken);

    public Task<string> SaveInspectionRunEvidenceAsync(
        Guid tenantId,
        Guid inspectionRunId,
        Guid evidenceId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default) =>
        SaveAsync(tenantId, "inspection-runs", inspectionRunId, evidenceId, fileName, content, cancellationToken);

    public async Task<string> SaveAsync(
        Guid tenantId,
        string scope,
        Guid scopeId,
        Guid evidenceId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var safeFileName = SanitizeFileName(fileName);
        var relativeKey = Path.Combine(
            tenantId.ToString("N"),
            scope,
            scopeId.ToString("N"),
            $"{evidenceId:N}_{safeFileName}");
        var absolutePath = Path.Combine(_rootPath, relativeKey);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var fileStream = File.Create(absolutePath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return relativeKey.Replace('\\', '/');
    }

    public bool TryOpenReadStream(string storageKey, out FileStream? stream)
    {
        var absolutePath = Path.Combine(_rootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(absolutePath))
        {
            stream = null;
            return false;
        }

        stream = File.OpenRead(absolutePath);
        return true;
    }

    private static string ResolveRootPath(string contentRoot, string configuredRoot) =>
        Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(contentRoot, configuredRoot);

    private static string SanitizeFileName(string fileName)
    {
        var trimmed = Path.GetFileName(fileName.Trim());
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return "evidence.bin";
        }

        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            trimmed = trimmed.Replace(invalid, '_');
        }

        return trimmed.Length > 200 ? trimmed[..200] : trimmed;
    }
}
