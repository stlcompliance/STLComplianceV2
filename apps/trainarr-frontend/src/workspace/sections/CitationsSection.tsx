import {
  removeTrainingDefinitionCitation,
  removeTrainingProgramCitation,
} from '../../api/client'
import { CitationAttachmentPanel } from '../../components/CitationAttachmentPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function CitationsSection({ state }: Props) {
  const s = state
  const citationOptionsById = new Map<string, { citationId: string; citationKey: string; label: string }>()
  for (const citation of s.definitionCitationsQuery.data ?? []) {
    citationOptionsById.set(citation.complianceCoreCitationId, {
      citationId: citation.complianceCoreCitationId,
      citationKey: citation.citationKey,
      label: citation.metadata?.label
        ? `${citation.metadata.label} (${citation.citationKey})`
        : citation.citationKey,
    })
  }
  for (const citation of s.programCitationsQuery.data ?? []) {
    if (!citationOptionsById.has(citation.complianceCoreCitationId)) {
      citationOptionsById.set(citation.complianceCoreCitationId, {
        citationId: citation.complianceCoreCitationId,
        citationKey: citation.citationKey,
        label: citation.metadata?.label
          ? `${citation.metadata.label} (${citation.citationKey})`
          : citation.citationKey,
      })
    }
  }

  if (
    s.citationIdInput.trim().length > 0 &&
    s.citationKeyInput.trim().length > 0 &&
    !citationOptionsById.has(s.citationIdInput.trim())
  ) {
    citationOptionsById.set(s.citationIdInput.trim(), {
      citationId: s.citationIdInput.trim(),
      citationKey: s.citationKeyInput.trim(),
      label: s.citationKeyInput.trim(),
    })
  }

  const citationOptions = [...citationOptionsById.values()].sort((left, right) =>
    left.label.localeCompare(right.label),
  )

  return (
    <>
      {s.selectedDefinitionIdForCitations ? (
        <CitationAttachmentPanel
          title="Training definition citations"
          citations={s.definitionCitationsQuery.data ?? []}
          citationOptions={citationOptions}
          citationIdInput={s.citationIdInput}
          citationKeyInput={s.citationKeyInput}
          onCitationSelectionChange={(value) => {
            s.setCitationIdInput(value?.citationId ?? '')
            s.setCitationKeyInput(value?.citationKey ?? '')
          }}
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
          citationOptions={citationOptions}
          citationIdInput={s.citationIdInput}
          citationKeyInput={s.citationKeyInput}
          onCitationSelectionChange={(value) => {
            s.setCitationIdInput(value?.citationId ?? '')
            s.setCitationKeyInput(value?.citationKey ?? '')
          }}
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
