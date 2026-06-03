using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class TrainingProgramDraftService(TrainArrDbContext db, ITrainArrAuditService audit)
{
    private static readonly Regex TokenRegex = new("[a-z0-9]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "and", "ai", "build", "create", "course", "draft", "for", "from", "generate", "into", "make",
        "of", "on", "program", "programming", "the", "to", "training", "with", "new", "newhire", "new-hires",
        "assistant", "help", "please", "drafting"
    };

    public async Task<TrainingProgramDraftResponse> GenerateAsync(
        Guid tenantId,
        Guid actorUserId,
        GenerateTrainingProgramDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        var prompt = NormalizePrompt(request.Prompt);
        var definitions = await db.TrainingDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "active")
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        if (definitions.Count == 0)
        {
            throw new StlApiException(
                "training_program_drafts.no_definitions",
                "Create training definitions before generating an AI-assisted draft.",
                400);
        }

        var promptTokens = TokenRegex
            .Matches(prompt.ToLowerInvariant())
            .Select(match => match.Value)
            .Where(token => !StopWords.Contains(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var scoredDefinitions = definitions
            .Select(definition => ScoreDefinition(definition, prompt, promptTokens))
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Definition.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var selectedMatches = scoredDefinitions
            .Where(item => item.Score > 0)
            .Take(4)
            .ToList();

        if (selectedMatches.Count == 0)
        {
            selectedMatches = scoredDefinitions.Take(3).ToList();
        }

        var selectedIds = selectedMatches.Select(item => item.Definition.Id).ToList();
        var name = BuildProgramName(prompt, selectedMatches);
        var description = BuildProgramDescription(prompt, selectedMatches, name);
        var summary = BuildSummary(selectedMatches, definitions.Count);
        var generatedAt = DateTimeOffset.UtcNow;

        await audit.WriteAsync(
            "training_program.draft_generate",
            tenantId,
            actorUserId,
            "training_program_draft",
            name,
            "Succeeded",
            cancellationToken: cancellationToken);

        return new TrainingProgramDraftResponse(
            generatedAt,
            prompt,
            name,
            description,
            selectedIds,
            selectedMatches
                .Select(match => new TrainingProgramDraftMatchResponse(
                    match.Definition.Id,
                    match.Definition.DefinitionKey,
                    match.Definition.Name,
                    match.Definition.QualificationKey,
                    match.Definition.QualificationName,
                    match.Score,
                    match.MatchReason))
                .ToList(),
            summary);
    }

    private static DraftMatch ScoreDefinition(TrainingDefinition definition, string prompt, string[] promptTokens)
    {
        var score = 0;
        var reasons = new List<string>();

        foreach (var token in promptTokens)
        {
            if (Contains(definition.Name, token))
            {
                score += 8;
                reasons.Add($"name matches '{token}'");
            }

            if (Contains(definition.Description, token))
            {
                score += 4;
                reasons.Add($"description matches '{token}'");
            }

            if (Contains(definition.QualificationName, token))
            {
                score += 6;
                reasons.Add($"qualification name matches '{token}'");
            }

            if (Contains(definition.QualificationKey, token))
            {
                score += 6;
                reasons.Add($"qualification key matches '{token}'");
            }
        }

        if (Contains(definition.Name, prompt))
        {
            score += 12;
            reasons.Add("name matches the full prompt");
        }

        if (Contains(definition.QualificationName, prompt))
        {
            score += 10;
            reasons.Add("qualification name matches the full prompt");
        }

        if (Contains(definition.Description, prompt))
        {
            score += 6;
            reasons.Add("description matches the full prompt");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("No direct keyword match; used active definitions as safe draft starters.");
        }

        return new DraftMatch(definition, score, string.Join("; ", reasons.Distinct(StringComparer.OrdinalIgnoreCase)));
    }

    private static string BuildProgramName(string prompt, IReadOnlyList<DraftMatch> matches)
    {
        var promptWords = TokenRegex
            .Matches(prompt)
            .Select(match => match.Value)
            .Where(token => !StopWords.Contains(token))
            .Select(TitleCase)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToList();

        if (promptWords.Count == 0 && matches.Count > 0)
        {
            promptWords.AddRange(
                TokenRegex.Matches(matches[0].Definition.Name)
                    .Select(match => match.Value)
                    .Where(token => !StopWords.Contains(token))
                    .Select(TitleCase)
                    .Take(3));
        }

        var core = string.Join(" ", promptWords).Trim();
        if (string.IsNullOrWhiteSpace(core))
        {
            core = "Training";
        }

        if (!core.Contains("program", StringComparison.OrdinalIgnoreCase) &&
            !core.Contains("training", StringComparison.OrdinalIgnoreCase))
        {
            core = $"{core} Training";
        }

        return $"{core} Program";
    }

    private static string BuildProgramDescription(string prompt, IReadOnlyList<DraftMatch> matches, string name)
    {
        var recommended = matches.Select(match => match.Definition.Name).Take(3).ToList();
        var rationale = recommended.Count == 0
            ? "No active definitions matched directly, so the draft uses the current catalog as a starting point."
            : $"Recommended definitions: {string.Join(", ", recommended)}.";

        return $"AI-assisted draft for \"{prompt}\". {rationale} Generated as {name}.";
    }

    private static string BuildSummary(IReadOnlyList<DraftMatch> matches, int activeDefinitionCount)
    {
        if (matches.Count == 0)
        {
            return $"No direct keyword matches were found. Suggested from {activeDefinitionCount} active definitions.";
        }

        var top = matches.Take(3).Select(match => match.Definition.Name).ToList();
        return $"Suggested {matches.Count} definition(s) from {activeDefinitionCount} active definitions, led by {string.Join(", ", top)}.";
    }

    private static string NormalizePrompt(string prompt)
    {
        var trimmed = prompt.Trim();
        if (trimmed.Length < 8 || trimmed.Length > 1000)
        {
            throw new StlApiException(
                "training_program_drafts.validation",
                "Prompt must be between 8 and 1000 characters.",
                400);
        }

        return trimmed;
    }

    private static bool Contains(string source, string token) =>
        source.Contains(token, StringComparison.OrdinalIgnoreCase);

    private static string TitleCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }

    private sealed record DraftMatch(TrainingDefinition Definition, int Score, string MatchReason);
}
