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
            InspectionChecklistItemTypes.Numeric => "Say a number, for example twelve point five.",
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
            ttsPrompt.Trim(),
            voiceHint,
            item.SortOrder,
            isAnswered);
    }
}
