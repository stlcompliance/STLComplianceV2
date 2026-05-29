import type {
  AssetResponse,
  InspectionRunDetailResponse,
  InspectionRunEvidenceResponse,
  InspectionRunSummaryResponse,
  InspectionTemplateSummaryResponse,
  InspectionVoicePromptResponse,
} from '../api/types'
import { InspectionRunEvidencePanel } from './InspectionRunEvidencePanel'

interface InspectionRunnerPanelProps {
  canExecute: boolean
  viewAllRuns: boolean
  assets: AssetResponse[]
  activeTemplates: InspectionTemplateSummaryResponse[]
  runs: InspectionRunSummaryResponse[]
  activeRun: InspectionRunDetailResponse | null
  selectedAssetId: string
  selectedTemplateId: string
  selectedRunId: string
  answerDrafts: Record<string, { passFailValue?: string; numericValue?: string; textValue?: string }>
  isLoading: boolean
  isRunLoading: boolean
  isStarting: boolean
  isSubmitting: boolean
  isCompleting: boolean
  isCreatingDefects: boolean
  voiceGuidanceEnabled: boolean
  voiceGuidanceSupported: boolean
  voiceGuidanceLoading: boolean
  currentVoicePrompt: InspectionVoicePromptResponse | null
  voiceStatusMessage: string | null
  isVoiceListening: boolean
  onVoiceGuidanceEnabledChange: (enabled: boolean) => void
  onReadCurrentPrompt: () => void
  onListenForAnswer: () => void
  onSelectedAssetIdChange: (value: string) => void
  onSelectedTemplateIdChange: (value: string) => void
  onSelectedRunIdChange: (value: string) => void
  onAnswerDraftChange: (
    checklistItemId: string,
    field: 'passFailValue' | 'numericValue' | 'textValue',
    value: string,
  ) => void
  onStartRun: () => void
  onSubmitAnswers: () => void
  onCompleteRun: () => void
  onCreateDefectsFromRun: () => void
  runEvidence: InspectionRunEvidenceResponse[]
  evidenceChecklistItemId: string
  evidenceTypeKey: string
  evidenceNotes: string
  selectedEvidenceFileName: string | null
  isEvidenceLoading: boolean
  isUploadingEvidence: boolean
  onEvidenceChecklistItemIdChange: (value: string) => void
  onEvidenceTypeKeyChange: (value: string) => void
  onEvidenceNotesChange: (value: string) => void
  onSelectEvidenceFile: (file: File | null) => void
  onUploadEvidence: () => void
}

function formatResult(result: string | null): string {
  if (!result) {
    return '—'
  }
  if (result === 'passed') {
    return 'Passed'
  }
  if (result === 'failed') {
    return 'Failed'
  }
  return result
}

export function InspectionRunnerPanel({
  canExecute,
  viewAllRuns,
  assets,
  activeTemplates,
  runs,
  activeRun,
  selectedAssetId,
  selectedTemplateId,
  selectedRunId,
  answerDrafts,
  isLoading,
  isRunLoading,
  isStarting,
  isSubmitting,
  isCompleting,
  isCreatingDefects,
  voiceGuidanceEnabled,
  voiceGuidanceSupported,
  voiceGuidanceLoading,
  currentVoicePrompt,
  voiceStatusMessage,
  isVoiceListening,
  onVoiceGuidanceEnabledChange,
  onReadCurrentPrompt,
  onListenForAnswer,
  onSelectedAssetIdChange,
  onSelectedTemplateIdChange,
  onSelectedRunIdChange,
  onAnswerDraftChange,
  onStartRun,
  onSubmitAnswers,
  onCompleteRun,
  onCreateDefectsFromRun,
  runEvidence,
  evidenceChecklistItemId,
  evidenceTypeKey,
  evidenceNotes,
  selectedEvidenceFileName,
  isEvidenceLoading,
  isUploadingEvidence,
  onEvidenceChecklistItemIdChange,
  onEvidenceTypeKeyChange,
  onEvidenceNotesChange,
  onSelectEvidenceFile,
  onUploadEvidence,
}: InspectionRunnerPanelProps) {
  const inProgressRun = activeRun?.status === 'in_progress'
  const failedCompletedRun = activeRun?.status === 'completed' && activeRun?.result === 'failed'
  const checklistOptions =
    activeRun?.checklistItems.map((item) => ({
      value: item.checklistItemId,
      label: item.prompt,
    })) ?? []

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <header className="mb-4">
        <h2 className="text-lg font-semibold text-white">Run inspection</h2>
        <p className="mt-1 text-sm text-slate-400">
          Select an asset and active template, answer the checklist, and complete the run.
          {viewAllRuns ? ' Managers see all tenant runs.' : ' Technicians see runs they started.'}
        </p>
      </header>

      {isLoading ? (
        <p className="text-sm text-slate-400">Loading inspection runner…</p>
      ) : (
        <>
          {canExecute ? (
            <div className="mb-6 grid gap-4 md:grid-cols-2">
              <label className="block text-sm" htmlFor="inspection-runner-asset">
                <span className="text-slate-300">Asset for inspection</span>
                <select
                  id="inspection-runner-asset"
                  className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
                  value={selectedAssetId}
                  onChange={(event) => onSelectedAssetIdChange(event.target.value)}
                >
                  <option value="">Select asset…</option>
                  {assets.map((asset) => (
                    <option key={asset.assetId} value={asset.assetId}>
                      {asset.assetTag} — {asset.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="block text-sm" htmlFor="inspection-runner-template">
                <span className="text-slate-300">Active inspection template</span>
                <select
                  id="inspection-runner-template"
                  className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
                  value={selectedTemplateId}
                  onChange={(event) => onSelectedTemplateIdChange(event.target.value)}
                >
                  <option value="">Select template…</option>
                  {activeTemplates.map((template) => (
                    <option key={template.inspectionTemplateId} value={template.inspectionTemplateId}>
                      {template.name} (v{template.version})
                    </option>
                  ))}
                </select>
              </label>

              <div className="md:col-span-2">
                <button
                  type="button"
                  className="rounded-lg bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
                  disabled={!selectedAssetId || !selectedTemplateId || isStarting}
                  onClick={onStartRun}
                >
                  {isStarting ? 'Starting…' : 'Start inspection run'}
                </button>
              </div>
            </div>
          ) : (
            <p className="mb-6 text-sm text-slate-400">Inspection execution requires a technician role.</p>
          )}

          {isRunLoading ? (
            <p className="mb-6 text-sm text-slate-400">Loading active run…</p>
          ) : activeRun ? (
            <div className="mb-6 rounded-lg border border-slate-700 bg-slate-950/50 p-4">
              <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
                <div>
                  <h3 className="font-medium text-white">
                    {activeRun.assetTag} — {activeRun.templateName}
                  </h3>
                  <p className="text-xs text-slate-400">
                    Run {activeRun.inspectionRunId.slice(0, 8)} · v{activeRun.templateVersion} · {activeRun.status}
                    {activeRun.result ? ` · ${formatResult(activeRun.result)}` : ''}
                  </p>
                </div>
                {inProgressRun && canExecute ? (
                  <div className="flex flex-wrap gap-2">
                    <button
                      type="button"
                      className="rounded-lg border border-slate-600 px-3 py-1.5 text-sm text-slate-200 hover:bg-slate-800 disabled:opacity-50"
                      disabled={isSubmitting}
                      onClick={onSubmitAnswers}
                    >
                      {isSubmitting ? 'Saving…' : 'Save answers'}
                    </button>
                    <button
                      type="button"
                      className="rounded-lg bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
                      disabled={isCompleting}
                      onClick={onCompleteRun}
                    >
                      {isCompleting ? 'Completing…' : 'Complete run'}
                    </button>
                  </div>
                ) : null}
                {failedCompletedRun && canExecute ? (
                  <div className="flex flex-col items-end gap-1">
                    <p className="text-xs text-amber-200">Failed items auto-create defects on completion.</p>
                    <button
                      type="button"
                      className="rounded-lg bg-amber-800 px-3 py-1.5 text-sm font-medium text-white hover:bg-amber-700 disabled:opacity-50"
                      disabled={isCreatingDefects}
                      onClick={onCreateDefectsFromRun}
                    >
                      {isCreatingDefects ? 'Capturing…' : 'Capture defects from run'}
                    </button>
                  </div>
                ) : null}
              </div>

              {inProgressRun && canExecute ? (
                <div className="mb-4 rounded-lg border border-violet-900/60 bg-violet-950/20 p-3">
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <label className="flex items-center gap-2 text-sm text-slate-200" htmlFor="inspection-runner-voice-guidance">
                      <input
                        id="inspection-runner-voice-guidance"
                        type="checkbox"
                        checked={voiceGuidanceEnabled}
                        disabled={!voiceGuidanceSupported}
                        onChange={(event) => onVoiceGuidanceEnabledChange(event.target.checked)}
                      />
                      Voice-guided inspection
                    </label>
                    {voiceGuidanceEnabled ? (
                      <div className="flex flex-wrap gap-2">
                        <button
                          type="button"
                          className="rounded border border-violet-700 px-3 py-1 text-sm text-violet-100 hover:bg-violet-900/40 disabled:opacity-50"
                          disabled={!currentVoicePrompt || voiceGuidanceLoading}
                          onClick={onReadCurrentPrompt}
                        >
                          Read prompt
                        </button>
                        <button
                          type="button"
                          className="rounded bg-violet-800 px-3 py-1 text-sm text-white hover:bg-violet-700 disabled:opacity-50"
                          disabled={!currentVoicePrompt || isVoiceListening || voiceGuidanceLoading}
                          onClick={onListenForAnswer}
                        >
                          {isVoiceListening ? 'Listening…' : 'Listen for answer'}
                        </button>
                      </div>
                    ) : null}
                  </div>
                  {!voiceGuidanceSupported ? (
                    <p className="mt-2 text-xs text-slate-400">
                      Voice guidance requires browser speech synthesis and recognition support.
                    </p>
                  ) : null}
                  {voiceGuidanceEnabled && currentVoicePrompt ? (
                    <div className="mt-3 text-sm">
                      <p className="font-medium text-violet-100">{currentVoicePrompt.prompt}</p>
                      <p className="mt-1 text-xs text-violet-200/80">{currentVoicePrompt.voiceAnswerHint}</p>
                    </div>
                  ) : null}
                  {voiceGuidanceEnabled && voiceGuidanceLoading ? (
                    <p className="mt-2 text-xs text-slate-400">Loading voice prompts…</p>
                  ) : null}
                  {voiceStatusMessage ? (
                    <p className="mt-2 text-xs text-emerald-300">{voiceStatusMessage}</p>
                  ) : null}
                </div>
              ) : null}

              <ul className="space-y-3">
                {activeRun.checklistItems.map((item) => {
                  const existing = activeRun.answers.find((a) => a.checklistItemId === item.checklistItemId)
                  const draft = answerDrafts[item.checklistItemId] ?? {}

                  return (
                    <li
                      key={item.checklistItemId}
                      className="rounded-lg border border-slate-800 bg-slate-900/80 p-3 text-sm"
                    >
                      <div
                        id={`inspection-item-prompt-${item.checklistItemId}`}
                        className="mb-2 font-medium text-slate-100"
                      >
                        {item.prompt}
                        {item.isRequired ? <span className="ml-1 text-red-300">*</span> : null}
                      </div>
                      <p className="mb-2 text-xs text-slate-500">
                        {item.itemKey}
                        {item.categoryKey ? ` · ${item.categoryKey}` : ''}
                      </p>

                      {inProgressRun && canExecute ? (
                        item.itemType === 'pass_fail' ? (
                          <select
                            id={`inspection-answer-pass-fail-${item.checklistItemId}`}
                            className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-white"
                            value={draft.passFailValue ?? existing?.passFailValue ?? ''}
                            aria-labelledby={`inspection-item-prompt-${item.checklistItemId}`}
                            onChange={(event) =>
                              onAnswerDraftChange(item.checklistItemId, 'passFailValue', event.target.value)
                            }
                          >
                            <option value="">Select…</option>
                            <option value="pass">Pass</option>
                            <option value="fail">Fail</option>
                            <option value="na">N/A</option>
                          </select>
                        ) : item.itemType === 'numeric' ? (
                          <input
                            id={`inspection-answer-numeric-${item.checklistItemId}`}
                            type="number"
                            className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-white"
                            value={draft.numericValue ?? (existing?.numericValue?.toString() ?? '')}
                            aria-labelledby={`inspection-item-prompt-${item.checklistItemId}`}
                            onChange={(event) =>
                              onAnswerDraftChange(item.checklistItemId, 'numericValue', event.target.value)
                            }
                          />
                        ) : (
                          <textarea
                            id={`inspection-answer-text-${item.checklistItemId}`}
                            className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-white"
                            rows={2}
                            value={draft.textValue ?? existing?.textValue ?? ''}
                            aria-labelledby={`inspection-item-prompt-${item.checklistItemId}`}
                            onChange={(event) =>
                              onAnswerDraftChange(item.checklistItemId, 'textValue', event.target.value)
                            }
                          />
                        )
                      ) : (
                        <p className="text-slate-300">
                          {existing?.passFailValue ??
                            existing?.numericValue?.toString() ??
                            existing?.textValue ??
                            'No answer'}
                        </p>
                      )}
                    </li>
                  )
                })}
              </ul>
            </div>
          ) : null}

          <InspectionRunEvidencePanel
            inspectionRunId={selectedRunId || null}
            runStatus={activeRun?.status ?? null}
            evidence={runEvidence}
            checklistItemId={evidenceChecklistItemId}
            checklistOptions={checklistOptions}
            canUpload={canExecute}
            evidenceTypeKey={evidenceTypeKey}
            evidenceNotes={evidenceNotes}
            selectedFileName={selectedEvidenceFileName}
            onChecklistItemIdChange={onEvidenceChecklistItemIdChange}
            onEvidenceTypeKeyChange={onEvidenceTypeKeyChange}
            onEvidenceNotesChange={onEvidenceNotesChange}
            onSelectFile={onSelectEvidenceFile}
            onUploadEvidence={onUploadEvidence}
            isUploadingEvidence={isUploadingEvidence}
            isLoading={isEvidenceLoading}
          />

          <div>
            <h3 className="mb-2 text-sm font-medium text-slate-300">Inspection runs</h3>
            {runs.length === 0 ? (
              <p className="text-sm text-slate-400">No inspection runs yet.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="min-w-full text-left text-sm">
                  <thead className="border-b border-slate-700 text-slate-400">
                    <tr>
                      <th className="px-3 py-2 font-medium">Asset</th>
                      <th className="px-3 py-2 font-medium">Template</th>
                      <th className="px-3 py-2 font-medium">Status</th>
                      <th className="px-3 py-2 font-medium">Result</th>
                      <th className="px-3 py-2 font-medium">Started</th>
                      <th className="px-3 py-2 font-medium" />
                    </tr>
                  </thead>
                  <tbody>
                    {runs.map((run) => (
                      <tr key={run.inspectionRunId} className="border-b border-slate-800 text-slate-200">
                        <td className="px-3 py-2">
                          <div className="font-medium">{run.assetTag}</div>
                          <div className="text-xs text-slate-400">{run.assetName}</div>
                        </td>
                        <td className="px-3 py-2">
                          <div className="font-medium">{run.templateName}</div>
                          <div className="text-xs text-slate-400">v{run.templateVersion}</div>
                        </td>
                        <td className="px-3 py-2">{run.status}</td>
                        <td className="px-3 py-2">{formatResult(run.result)}</td>
                        <td className="px-3 py-2 text-slate-300">
                          {new Date(run.startedAt).toLocaleString()}
                        </td>
                        <td className="px-3 py-2">
                          <button
                            type="button"
                            className="text-sky-300 hover:text-sky-200"
                            onClick={() => onSelectedRunIdChange(run.inspectionRunId)}
                          >
                            {selectedRunId === run.inspectionRunId ? 'Viewing' : 'Open'}
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </>
      )}
    </section>
  )
}
