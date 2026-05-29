import { InspectionRunnerPanel } from '../../components/InspectionRunnerPanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function InspectionsSection({ state }: Props) {
  const s = state
  return (
    <div className="mb-8">
      <InspectionRunnerPanel
        canExecute={s.canExecuteInspections}
        viewAllRuns={s.viewAllRuns}
        assets={s.assetsQuery.data ?? []}
        activeTemplates={s.activeTemplates}
        runs={s.inspectionRunsQuery.data ?? []}
        activeRun={s.inspectionRunQuery.data ?? null}
        selectedAssetId={s.runAssetId}
        selectedTemplateId={s.runTemplateId}
        selectedRunId={s.selectedRunId}
        answerDrafts={s.answerDrafts}
        isLoading={s.assetsQuery.isLoading || s.templatesQuery.isLoading || s.inspectionRunsQuery.isLoading}
        isRunLoading={s.inspectionRunQuery.isLoading}
        isStarting={s.startRunMutation.isPending}
        isSubmitting={s.submitAnswersMutation.isPending}
        isCompleting={s.completeRunMutation.isPending}
        isCreatingDefects={s.createDefectsFromRunMutation.isPending}
        onSelectedAssetIdChange={s.setRunAssetId}
        onSelectedTemplateIdChange={s.setRunTemplateId}
        onSelectedRunIdChange={s.setSelectedRunId}
        onAnswerDraftChange={(checklistItemId, field, value) =>
          s.setAnswerDrafts((current) => ({
            ...current,
            [checklistItemId]: { ...current[checklistItemId], [field]: value },
          }))
        }
        onStartRun={() => s.startRunMutation.mutate()}
        onSubmitAnswers={() => s.submitAnswersMutation.mutate()}
        onCompleteRun={() => s.completeRunMutation.mutate()}
        onCreateDefectsFromRun={() => s.createDefectsFromRunMutation.mutate()}
        voiceGuidanceEnabled={s.voiceGuidanceEnabled}
        voiceGuidanceSupported={s.voiceGuidanceSupported}
        voiceGuidanceLoading={s.voiceGuidanceLoading}
        currentVoicePrompt={s.currentVoicePrompt}
        voiceStatusMessage={s.voiceStatusMessage}
        isVoiceListening={s.isVoiceListening}
        onVoiceGuidanceEnabledChange={s.setVoiceGuidanceEnabled}
        onReadCurrentPrompt={s.handleReadCurrentPrompt}
        onListenForAnswer={() => {
          void s.handleListenForAnswer()
        }}
      />
    </div>
  )
}
