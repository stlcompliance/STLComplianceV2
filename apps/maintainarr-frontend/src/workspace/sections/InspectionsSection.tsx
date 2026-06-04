import { InspectionRunnerPanel } from '../../components/InspectionRunnerPanel'
import { useLocation } from 'react-router-dom'
import { InspectionRunProfile } from './MaintenanceDetailProfiles'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function InspectionsSection({ state }: Props) {
  const s = state
  const location = useLocation()
  if (location.pathname.startsWith('/inspections/details')) {
    return <InspectionRunProfile state={s} />
  }

  return (
    <div className="mb-8" data-testid="maintainarr-inspections-workspace">
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
            [checklistItemId]: {
              ...current[checklistItemId],
              [field]:
                field === 'selectedOptions'
                  ? Array.isArray(value)
                    ? value
                    : []
                  : value,
            },
          }))
        }
        onStartRun={() => s.startRunMutation.mutate()}
        onSubmitAnswers={() => s.submitAnswersMutation.mutate()}
        onCompleteRun={() => s.completeRunMutation.mutate()}
        onCreateDefectsFromRun={() => s.createDefectsFromRunMutation.mutate()}
        runEvidence={s.inspectionRunEvidenceQuery.data ?? []}
        evidenceChecklistItemId={s.inspectionEvidenceChecklistItemId}
        evidenceTypeKey={s.inspectionEvidenceTypeKey}
        evidenceNotes={s.inspectionEvidenceNotes}
        selectedEvidenceFileName={s.inspectionEvidenceFile?.name ?? null}
        isEvidenceLoading={s.inspectionRunEvidenceQuery.isLoading}
        isUploadingEvidence={s.uploadInspectionRunEvidenceMutation.isPending}
        onEvidenceChecklistItemIdChange={s.setInspectionEvidenceChecklistItemId}
        onEvidenceTypeKeyChange={s.setInspectionEvidenceTypeKey}
        onEvidenceNotesChange={s.setInspectionEvidenceNotes}
        onSelectEvidenceFile={s.setInspectionEvidenceFile}
        onUploadEvidence={() => s.uploadInspectionRunEvidenceMutation.mutate()}
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
