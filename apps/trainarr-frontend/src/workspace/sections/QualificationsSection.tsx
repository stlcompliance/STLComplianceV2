import { BatchQualificationCheckPanel } from '../../components/BatchQualificationCheckPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

const personIdPattern =
  /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi

function parsePersonIdsFromText(text: string): string[] {
  const matches = text.match(personIdPattern) ?? []
  return [...new Set(matches.map((id) => id.toLowerCase()))]
}

type Props = { state: TrainArrWorkspaceState }

export function QualificationsSection({ state }: Props) {
  const s = state
  if (!s.canBatchQualification) {
    return <p className="text-sm text-slate-400">You do not have permission to run batch qualification checks.</p>
  }

  return (
    <BatchQualificationCheckPanel
      batch={s.batchQualificationCheck}
      isChecking={s.batchQualificationCheckMutation.isPending}
      onRunBatch={() => s.batchQualificationCheckMutation.mutate()}
      canRun={
        Boolean(s.batchQualificationKey.trim()) &&
        (parsePersonIdsFromText(s.batchPersonIdsText).length > 0 || s.selectedBatchRemediationPersonIds.length > 0)
      }
      qualificationKey={s.batchQualificationKey}
      onQualificationKeyChange={s.setBatchQualificationKey}
      rulePackKey={s.rulePackKey}
      onRulePackKeyChange={s.setRulePackKey}
      personIdsText={s.batchPersonIdsText}
      onPersonIdsTextChange={s.setBatchPersonIdsText}
      selectedRemediationPersonIds={s.selectedBatchRemediationPersonIds}
      onToggleRemediationPerson={(personId) => {
        s.setSelectedBatchRemediationPersonIds((current) =>
          current.includes(personId) ? current.filter((id) => id !== personId) : [...current, personId],
        )
        s.setBatchQualificationCheck(null)
      }}
      remediationPersonOptions={(s.remediationsQuery.data ?? []).map((remediation) => ({
        remediationId: remediation.remediationId,
        staffarrPersonId: remediation.staffarrPersonId,
        label: `${remediation.reasonCategoryKey} · ${remediation.remediationId.slice(0, 8)}`,
      }))}
    />
  )
}
