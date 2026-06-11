namespace STLCompliance.OpenApi.Tests;

public sealed class AiProviderConfigurationTests
{
    [Fact]
    public void Render_ai_provider_env_group_is_only_attached_to_backend_processor_services()
    {
        var renderYaml = File.ReadAllText(Path.Combine(FindRepoRoot(), "render.yaml"));

        Assert.Contains("name: stl-ai-provider", renderYaml, StringComparison.Ordinal);
        Assert.Contains("key: OPENAI_API_KEY", renderYaml, StringComparison.Ordinal);
        Assert.Contains("sync: false", renderYaml, StringComparison.Ordinal);

        var services = SplitRenderServiceBlocks(renderYaml);
        var consumers = services
            .Where(block => block.Contains("fromGroup: stl-ai-provider", StringComparison.Ordinal))
            .Select(ExtractServiceName)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["nexarr-api", "shared-worker"], consumers);
    }

    [Fact]
    public void Frontend_sources_do_not_reference_openai_api_key()
    {
        var root = FindRepoRoot();
        var frontendPaths = new[]
        {
            Path.Combine(root, "apps", "suite-frontend", "src"),
            Path.Combine(root, "packages", "shared-ui", "src"),
            Path.Combine(root, "apps", "suite-frontend", ".env.example"),
            Path.Combine(root, "apps", "suite-frontend", ".env.development")
        };

        foreach (var path in frontendPaths)
        {
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    Assert.DoesNotContain("OPENAI_API_KEY", File.ReadAllText(file), StringComparison.Ordinal);
                }
            }
            else if (File.Exists(path))
            {
                Assert.DoesNotContain("OPENAI_API_KEY", File.ReadAllText(path), StringComparison.Ordinal);
            }
        }
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "render.yaml")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }

    private static IReadOnlyList<string> SplitRenderServiceBlocks(string renderYaml)
    {
        var blocks = new List<string>();
        var current = new List<string>();
        foreach (var line in renderYaml.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            if (line.StartsWith("  - type: ", StringComparison.Ordinal) && current.Count > 0)
            {
                blocks.Add(string.Join(Environment.NewLine, current));
                current.Clear();
            }

            if (current.Count > 0 || line.StartsWith("  - type: ", StringComparison.Ordinal))
            {
                current.Add(line);
            }
        }

        if (current.Count > 0)
        {
            blocks.Add(string.Join(Environment.NewLine, current));
        }

        return blocks;
    }

    private static string ExtractServiceName(string block)
    {
        var line = block
            .Split(["\r\n", "\n"], StringSplitOptions.None)
            .FirstOrDefault(value => value.TrimStart().StartsWith("name: ", StringComparison.Ordinal));

        return line?.Split(':', 2)[1].Trim() ?? string.Empty;
    }
}
