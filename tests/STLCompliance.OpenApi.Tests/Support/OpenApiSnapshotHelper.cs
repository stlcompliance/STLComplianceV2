using System.Text.Json;
using System.Text.Json.Nodes;

namespace STLCompliance.OpenApi.Tests.Support;

internal static class OpenApiSnapshotHelper
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    public static string Normalize(string openApiJson)
    {
        var root = JsonNode.Parse(openApiJson)
            ?? throw new InvalidOperationException("OpenAPI document is empty.");

        if (root["info"] is JsonObject info)
        {
            info.Remove("version");
        }

        if (root["paths"] is JsonObject paths)
        {
            var sortedPaths = new JsonObject();
            foreach (var path in paths.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                sortedPaths[path.Key] = path.Value?.DeepClone();
            }

            root["paths"] = sortedPaths;
        }

        return root.ToJsonString(WriteOptions);
    }

    public static string SnapshotPath(string productKey)
    {
        var baseDirectory = AppContext.BaseDirectory;
        return Path.Combine(baseDirectory, "snapshots", $"{productKey}.openapi.json");
    }

    public static string RepoSnapshotPath(string productKey)
    {
        var repoRoot = FindRepoRoot();
        return Path.Combine(repoRoot, "tests", "STLCompliance.OpenApi.Tests", "snapshots", $"{productKey}.openapi.json");
    }

    public static void AssertMatchesSnapshot(string productKey, string actualNormalized)
    {
        var updateSnapshots = string.Equals(
            Environment.GetEnvironmentVariable("OPENAPI_UPDATE_SNAPSHOTS"),
            "1",
            StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                Environment.GetEnvironmentVariable("OPENAPI_UPDATE_SNAPSHOTS"),
                "true",
                StringComparison.OrdinalIgnoreCase);

        var repoSnapshotPath = RepoSnapshotPath(productKey);
        if (updateSnapshots)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(repoSnapshotPath)!);
            File.WriteAllText(repoSnapshotPath, actualNormalized);
            return;
        }

        Assert.True(
            File.Exists(repoSnapshotPath),
            $"Missing OpenAPI snapshot for {productKey}. Run with OPENAPI_UPDATE_SNAPSHOTS=1 to generate {repoSnapshotPath}.");

        var expected = Normalize(File.ReadAllText(repoSnapshotPath));
        Assert.Equal(expected, actualNormalized);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "STLCompliance.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from test output directory.");
    }
}
