import type { PickerOption } from '@stl/shared-ui'

import type {
  QualificationCheckHistoryItemResponse,
  QualificationCheckResponse,
  TrainingDefinitionResponse,
} from '../api/types'
import { QualificationCheckPanel } from './QualificationCheckPanel'

interface AuthorizationCheckOperationsPanelProps {
  definitions: TrainingDefinitionResponse[]
  history: QualificationCheckHistoryItemResponse[]
  isLoadingHistory: boolean
  check: QualificationCheckResponse | null
  isChecking: boolean
  canRun: boolean
  staffarrPersonId: string
  onStaffarrPersonIdChange: (value: string) => void
  selectedDefinitionId: string
  onSelectDefinition: (definitionId: string) => void
  rulePackKey: string
  onRulePackKeyChange: (value: string) => void
  rulePackOptions: PickerOption[]
  onRunCheck: () => void
}

function outcomeClass(outcome: string): string {
  switch (outcome) {
    case 'allow':
      return 'text-emerald-300'
    case 'warn':
      return 'text-amber-300'
    case 'block':
      return 'text-rose-300'
    default:
      return 'text-slate-300'
  }
}

export function AuthorizationCheckOperationsPanel({
  definitions,
  history,
  isLoadingHistory,
  check,
  isChecking,
  canRun,
  staffarrPersonId,
  onStaffarrPersonIdChange,
  selectedDefinitionId,
  onSelectDefinition,
  rulePackKey,
  onRulePackKeyChange,
  rulePackOptions,
  onRunCheck,
}: AuthorizationCheckOperationsPanelProps) {
  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
        Authorization check operations
      </h2>
      <p className="mt-1 text-xs text-slate-500">
        Run ad-hoc qualification authorization checks and review recent outcomes before assignment or dispatch
        gating decisions.
      </p>

      <div className="mt-4 grid gap-3 sm:grid-cols-2">
        <label className="block text-xs text-slate-400">
          StaffArr person ID
          <input
            type="text"
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-2 font-mono text-xs text-slate-100"
            value={staffarrPersonId}
            onChange={(e) => onStaffarrPersonIdChange(e.target.value)}
            placeholder="00000000-0000-0000-0000-000000000001"
          />
        </label>
        <label className="block text-xs text-slate-400">
          Training definition
          <select
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-2 text-sm text-slate-100"
            value={selectedDefinitionId}
            onChange={(e) => onSelectDefinition(e.target.value)}
          >
            <option value="">Select definition…</option>
            {definitions.map((definition) => (
              <option key={definition.trainingDefinitionId} value={definition.trainingDefinitionId}>
                {definition.name} · {definition.qualificationKey}
              </option>
            ))}
          </select>
        </label>
      </div>

      <QualificationCheckPanel
        check={check}
        isChecking={isChecking}
        onRunCheck={onRunCheck}
        canRun={canRun}
        rulePackKey={rulePackKey}
        onRulePackKeyChange={onRulePackKeyChange}
        rulePackOptions={rulePackOptions}
      />

      <div className="mt-4">
        <p className="text-xs font-medium uppercase tracking-wide text-slate-500">Recent checks</p>
        {isLoadingHistory ? (
          <p className="mt-2 text-sm text-slate-400">Loading check history…</p>
        ) : history.length === 0 ? (
          <p className="mt-2 text-sm text-slate-400">No authorization checks recorded yet.</p>
        ) : (
          <ul className="mt-2 max-h-56 space-y-2 overflow-y-auto">
            {history.map((item) => (
              <li
                key={item.checkId}
                className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm"
              >
                <p className={`font-semibold uppercase ${outcomeClass(item.outcome)}`}>{item.outcome}</p>
                <p className="mt-1 font-mono text-xs text-slate-500">{item.staffarrPersonId}</p>
                <p className="mt-1 text-xs text-slate-400">
                  {item.qualificationKey} · {new Date(item.checkedAt).toLocaleString()}
                </p>
                <p className="mt-1 text-xs text-slate-500">{item.message}</p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
