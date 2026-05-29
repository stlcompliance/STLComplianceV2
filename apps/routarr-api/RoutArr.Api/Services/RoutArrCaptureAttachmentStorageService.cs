using Microsoft.Extensions.Options;
using RoutArr.Api.Options;

namespace RoutArr.Api.Services;

public sealed class RoutArrCaptureAttachmentStorageService(
    IHostEnvironment environment,
    IOptions<CaptureAttachmentStorageOptions> options)
{
    private readonly string _rootPath = ResolveRootPath(environment.ContentRootPath, options.Value.RootPath);

    public async Task<string> SaveAsync(
        Guid tenantId,
        Guid tripId,
        Guid attachmentId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var safeFileName = SanitizeFileName(fileName);
        var relativeKey = Path.Combine(
            tenantId.ToString("N"),
            tripId.ToString("N"),
            $"{attachmentId:N}_{safeFileName}");
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

    public long TryDelete(string storageKey)
    {
        var absolutePath = Path.Combine(_rootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(absolutePath))
        {
            return 0;
        }

        var length = new FileInfo(absolutePath).Length;
        File.Delete(absolutePath);
        return length;
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
            return "attachment.bin";
        }

        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            trimmed = trimmed.Replace(invalid, '_');
        }

        return trimmed.Length > 200 ? trimmed[..200] : trimmed;
    }
}
