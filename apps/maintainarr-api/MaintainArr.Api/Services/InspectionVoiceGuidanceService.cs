using MaintainArr.Api.Contracts;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class InspectionVoiceGuidanceService(
    InspectionRunService inspectionRunService)
{
    public async Task<InspectionVoiceGuidanceResponse> GetGuidanceAsync(
        Guid tenantId,
        Guid inspectionRunId,
        CancellationToken cancellationToken = default)
    {
        var detail = await inspectionRunService.GetAsync(tenantId, inspectionRunId, cancellationToken);
        if (!string.Equals(detail.Status, InspectionRunStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inspection_voice.run_not_in_progress",
                "Voice guidance is available only for in-progress inspection runs.",
                400);
        }

        var answeredIds = detail.Answers
            .Select(x => x.ChecklistItemId)
            .ToHashSet();

        var prompts = detail.ChecklistItems
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.ItemKey)
            .Select(item => BuildPrompt(item, answeredIds.Contains(item.ChecklistItemId)))
            .ToList();

        var nextIndex = prompts.FindIndex(x => !x.IsAnswered);
        if (nextIndex < 0)
        {
            nextIndex = prompts.Count > 0 ? prompts.Count - 1 : 0;
        }

        return new InspectionVoiceGuidanceResponse(inspectionRunId, prompts, nextIndex);
    }

    public NormalizeVoiceNumericResult NormalizeNumeric(string? transcript) =>
        VoiceNumericNormalizer.Normalize(transcript);

    private static InspectionVoicePromptResponse BuildPrompt(
        InspectionRunChecklistItemSnapshot item,
        bool isAnswered)
    {
        var voiceHint = item.ItemType switch
        {
            InspectionChecklistItemTypes.PassFail => "Say pass, fail, or N A.",
            InspectionChecklistItemTypes.YesNo => "Say yes or no.",
            InspectionChecklistItemTypes.Numeric => "Say a number, for example twelve point five.",
            InspectionChecklistItemTypes.Select => FormatOptionsHint(item.ControlledOptions, allowMultiple: false),
            InspectionChecklistItemTypes.MultiSelect => FormatOptionsHint(item.ControlledOptions, allowMultiple: true),
            InspectionChecklistItemTypes.Photo => "Use the inspection evidence panel to upload a photo.",
            InspectionChecklistItemTypes.Signature => "Use the inspection evidence panel to upload a signature.",
            InspectionChecklistItemTypes.Text => "Say your observation.",
            _ => "Provide your answer.",
        };

        var requiredSuffix = item.IsRequired ? " This item is required." : string.Empty;
        var ttsPrompt = $"{item.Prompt}.{requiredSuffix} {voiceHint}";

        return new InspectionVoicePromptResponse(
            item.ChecklistItemId,
            item.ItemKey,
            item.Prompt,
            item.ItemType,
            item.ControlledOptions,
            ttsPrompt.Trim(),
            voiceHint,
            item.SortOrder,
            isAnswered);
    }

    private static string FormatOptionsHint(IReadOnlyList<string> controlledOptions, bool allowMultiple)
    {
        if (controlledOptions.Count == 0)
        {
            return allowMultiple
                ? "Say one or more of the listed options."
                : "Say one of the listed options.";
        }

        var preview = string.Join(", ", controlledOptions.Take(8));
        var suffix = controlledOptions.Count > 8 ? ", and more" : string.Empty;
        return allowMultiple
            ? $"Say one or more of: {preview}{suffix}."
            : $"Say one of: {preview}{suffix}.";
    }
}
