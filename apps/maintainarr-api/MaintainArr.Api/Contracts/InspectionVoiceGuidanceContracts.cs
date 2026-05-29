namespace MaintainArr.Api.Contracts;

public sealed record InspectionVoicePromptResponse(
    Guid ChecklistItemId,
    string ItemKey,
    string Prompt,
    string ItemType,
    string TtsPrompt,
    string VoiceAnswerHint,
    int SortOrder,
    bool IsAnswered);

public sealed record InspectionVoiceGuidanceResponse(
    Guid InspectionRunId,
    IReadOnlyList<InspectionVoicePromptResponse> Prompts,
    int NextUnansweredIndex);

public sealed record NormalizeVoiceNumericRequest(string Transcript);

public sealed record NormalizeVoiceNumericResponse(
    decimal? Value,
    string? NormalizedText,
    bool Understood);
