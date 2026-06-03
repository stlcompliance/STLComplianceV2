import { BatchQualificationCheckPanel } from '../../components/BatchQualificationCheckPanel'
import { AuthorizationCheckOperationsPanel } from '../../components/AuthorizationCheckOperationsPanel'
import { PersonTrainingHistoryPanel } from '../../components/PersonTrainingHistoryPanel'
import { QualificationManagementPanel } from '../../components/QualificationManagementPanel'
import { QualificationWalletPanel } from '../../components/QualificationWalletPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function QualificationsSection({ state }: Props) {
  const s = state
  const qualificationOptionsByKey = new Map<string, { value: string; label: string }>()
  for (const definition of s.definitionsQuery.data ?? []) {
    const qualificationKey = definition.qualificationKey.trim()
    if (!qualificationKey || qualificationOptionsByKey.has(qualificationKey)) {
      continue
    }
    qualificationOptionsByKey.set(qualificationKey, {
      value: qualificationKey,
      label: `${definition.qualificationName} (${qualificationKey})`,
    })
  }

  if (s.batchQualificationKey.trim() && !qualificationOptionsByKey.has(s.batchQualificationKey.trim())) {
    qualificationOptionsByKey.set(s.batchQualificationKey.trim(), {
      value: s.batchQualificationKey.trim(),
      label: s.batchQualificationKey.trim(),
    })
  }

  const qualificationOptions = [...qualificationOptionsByKey.values()].sort((left, right) =>
    left.label.localeCompare(right.label),
  )

  return (
    <div className="space-y-6">
      <QualificationManagementPanel
        issues={s.qualificationIssuesQuery.data ?? []}
        statusFilter={s.qualificationStatusFilter}
        lifecycleReason={s.lifecycleReason}
        selectedIssueId={s.selectedQualificationIssueId}
        onStatusFilterChange={(value) => {
          s.setQualificationStatusFilter(value)
          s.setSelectedQualificationIssueId(null)
        }}
        onLifecycleReasonChange={s.setLifecycleReason}
        onSelectIssue={s.setSelectedQualificationIssueId}
        onSuspend={(id) => s.suspendQualificationMutation.mutate(id)}
        onRevoke={(id) => s.revokeQualificationMutation.mutate(id)}
        onExpire={(id) => s.expireQualificationMutation.mutate(id)}
        isSuspending={s.suspendQualificationMutation.isPending}
        isRevoking={s.revokeQualificationMutation.isPending}
        isExpiring={s.expireQualificationMutation.isPending}
        canManage={s.canQualifications}
        history={s.qualificationIssueHistoryQuery.data ?? []}
        isLoadingHistory={s.qualificationIssueHistoryQuery.isLoading}
      />

      <QualificationWalletPanel
        accessToken={s.accessToken}
        issues={s.qualificationIssuesQuery.data ?? []}
        selectedIssueId={s.selectedQualificationIssueId}
        canManage={s.canQualifications}
      />

      <PersonTrainingHistoryPanel
        accessToken={s.accessToken}
        defaultStaffarrPersonId={s.me.personId}
        personOptions={s.personPickerOptions}
      />

      <AuthorizationCheckOperationsPanel
        definitions={s.definitionsQuery.data ?? []}
        history={s.qualificationCheckHistoryQuery.data ?? []}
        isLoadingHistory={s.qualificationCheckHistoryQuery.isLoading}
        check={s.operationsQualificationCheck}
        isChecking={s.operationsQualificationCheckMutation.isPending}
        canRun={Boolean(s.operationsCheckPersonId.trim() && s.operationsCheckDefinitionId)}
        staffarrPersonId={s.operationsCheckPersonId}
        onStaffarrPersonIdChange={(value) => {
          s.setOperationsCheckPersonId(value)
          s.setOperationsQualificationCheck(null)
        }}
        selectedDefinitionId={s.operationsCheckDefinitionId}
        onSelectDefinition={(value) => {
          s.setOperationsCheckDefinitionId(value)
          s.setOperationsQualificationCheck(null)
        }}
        rulePackKey={s.rulePackKey}
        onRulePackKeyChange={s.setRulePackKey}
        rulePackOptions={s.rulePackOptions}
        personPickerOptions={s.personPickerOptions}
        onRunCheck={() => s.operationsQualificationCheckMutation.mutate()}
      />

      {s.canBatchQualification ? (
        <BatchQualificationCheckPanel
          batch={s.batchQualificationCheck}
          isChecking={s.batchQualificationCheckMutation.isPending}
          onRunBatch={() => s.batchQualificationCheckMutation.mutate()}
          canRun={
            Boolean(s.batchQualificationKey.trim()) &&
            (s.selectedBatchPersonIds.length > 0 || s.selectedBatchRemediationPersonIds.length > 0)
          }
          qualificationKey={s.batchQualificationKey}
          onQualificationKeyChange={s.setBatchQualificationKey}
          qualificationOptions={qualificationOptions}
          rulePackKey={s.rulePackKey}
          onRulePackKeyChange={s.setRulePackKey}
          rulePackOptions={s.rulePackOptions}
          selectedPersonIds={s.selectedBatchPersonIds}
          onSelectedPersonIdsChange={(values) => {
            s.setSelectedBatchPersonIds(values)
            s.setBatchQualificationCheck(null)
          }}
          personPickerOptions={s.personPickerOptions}
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
      ) : (
        <p className="text-sm text-slate-400">You do not have permission to run batch qualification checks.</p>
      )}
    </div>
  )
}
