using Microsoft.Extensions.Options;
using RecordArr.Api.Options;

namespace RecordArr.Api.Services;

public sealed class RecordArrDocumentStorageService(IHostEnvironment environment, IOptions<DocumentStorageOptions> options)
{
    private readonly string _rootPath = ResolveRootPath(environment.ContentRootPath, options.Value.RootPath);

    public async Task<string> SaveAsync(
        Guid tenantId,
        Guid importBatchId,
        string checksum,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var safeChecksum = SanitizePathSegment(checksum, "checksum");
        var safeFileName = SanitizeFileName(fileName);
        var relativeKey = Path.Combine(
            tenantId.ToString("N"),
            importBatchId.ToString("N"),
            safeChecksum,
            safeFileName);

        var absolutePath = Path.Combine(_rootPath, relativeKey);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var fileStream = File.Create(absolutePath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return relativeKey.Replace('\\', '/');
    }

    public bool TryOpenReadStream(string storageKey, out FileStream? stream)
    {
        var absolutePath = Path.GetFullPath(Path.Combine(_rootPath, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        var rootPath = Path.GetFullPath(_rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!absolutePath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase) || !File.Exists(absolutePath))
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

    private static string SanitizePathSegment(string value, string fallback)
    {
        var trimmed = Path.GetFileName(value.Trim());
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return fallback;
        }

        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            trimmed = trimmed.Replace(invalid, '_');
        }

        return trimmed.Length > 200 ? trimmed[..200] : trimmed;
    }

    private static string SanitizeFileName(string fileName) =>
        SanitizePathSegment(fileName, "document.bin");
}
