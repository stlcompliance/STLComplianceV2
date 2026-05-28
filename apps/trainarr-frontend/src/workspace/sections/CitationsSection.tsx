import {
  removeTrainingDefinitionCitation,
  removeTrainingProgramCitation,
} from '../../api/client'
import { CitationAttachmentPanel } from '../../components/CitationAttachmentPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function CitationsSection({ state }: Props) {
  const s = state
  return (
    <>
      {s.selectedDefinitionIdForCitations ? (
        <CitationAttachmentPanel
          title="Training definition citations"
          citations={s.definitionCitationsQuery.data ?? []}
          citationIdInput={s.citationIdInput}
          citationKeyInput={s.citationKeyInput}
          onCitationIdChange={s.setCitationIdInput}
          onCitationKeyChange={s.setCitationKeyInput}
          onAttach={() => s.attachDefinitionCitationMutation.mutate()}
          onRemove={async (attachmentId) => {
            s.setRemovingCitationId(attachmentId)
            try {
              await removeTrainingDefinitionCitation(
                s.accessToken,
                s.selectedDefinitionIdForCitations!,
                attachmentId,
              )
              await s.queryClient.invalidateQueries({
                queryKey: ['trainarr-definition-citations', s.session.accessToken, s.selectedDefinitionIdForCitations],
              })
            } finally {
              s.setRemovingCitationId(null)
            }
          }}
          isAttaching={s.attachDefinitionCitationMutation.isPending}
          isRemovingId={s.removingCitationId}
          canManage={s.canPrograms}
          validateWithComplianceCore={s.validateCitationWithComplianceCore}
          onValidateWithComplianceCoreChange={s.setValidateCitationWithComplianceCore}
        />
      ) : (
        <p className="text-sm text-slate-400">Select a training definition on the Programs page to manage citations.</p>
      )}

      {s.selectedProgramId ? (
        <CitationAttachmentPanel
          title="Training program citations"
          citations={s.programCitationsQuery.data ?? []}
          citationIdInput={s.citationIdInput}
          citationKeyInput={s.citationKeyInput}
          onCitationIdChange={s.setCitationIdInput}
          onCitationKeyChange={s.setCitationKeyInput}
          onAttach={() => s.attachProgramCitationMutation.mutate()}
          onRemove={async (attachmentId) => {
            s.setRemovingCitationId(attachmentId)
            try {
              await removeTrainingProgramCitation(s.accessToken, s.selectedProgramId!, attachmentId)
              await s.queryClient.invalidateQueries({
                queryKey: ['trainarr-program-citations', s.session.accessToken, s.selectedProgramId],
              })
            } finally {
              s.setRemovingCitationId(null)
            }
          }}
          isAttaching={s.attachProgramCitationMutation.isPending}
          isRemovingId={s.removingCitationId}
          canManage={s.canPrograms}
          validateWithComplianceCore={s.validateCitationWithComplianceCore}
          onValidateWithComplianceCoreChange={s.setValidateCitationWithComplianceCore}
        />
      ) : null}
    </>
  )
}
