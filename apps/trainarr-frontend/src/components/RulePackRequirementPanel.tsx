import type { TrainingRulePackRequirementResponse } from '../api/types'

interface RulePackRequirementPanelProps {
  title: string
  requirements: TrainingRulePackRequirementResponse[]
  rulePackKeyInput: string
  onRulePackKeyChange: (value: string) => void
  onSave: () => void
  onRemove: (requirementId: string) => void
  isSaving: boolean
  isRemovingId: string | null
  canManage: boolean
  validateWithComplianceCore: boolean
  onValidateWithComplianceCoreChange: (value: boolean) => void
}

export function RulePackRequirementPanel({
  title,
  requirements,
  rulePackKeyInput,
  onRulePackKeyChange,
  onSave,
  onRemove,
  isSaving,
  isRemovingId,
  canManage,
  validateWithComplianceCore,
  onValidateWithComplianceCoreChange,
}: RulePackRequirementPanelProps) {
  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">{title}</h2>
      <p className="mt-1 text-xs text-slate-500">
        References Compliance Core rule pack keys only — TrainArr does not own rule packs.
      </p>

      {canManage ? (
        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <label className="block text-xs text-slate-400 sm:col-span-2">
            Rule pack key
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 font-mono text-sm text-slate-100"
              value={rulePackKeyInput}
              onChange={(e) => onRulePackKeyChange(e.target.value)}
              placeholder="driver_qualification"
            />
          </label>
          <label className="flex items-center gap-2 text-xs text-slate-400 sm:col-span-2">
            <input
              type="checkbox"
              checked={validateWithComplianceCore}
              onChange={(e) => onValidateWithComplianceCoreChange(e.target.checked)}
            />
            Validate against Compliance Core when saving
          </label>
          <button
            type="button"
            className="rounded bg-violet-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-600 disabled:opacity-50 sm:col-span-2 sm:justify-self-start"
            disabled={isSaving || !rulePackKeyInput.trim()}
            onClick={onSave}
          >
            {isSaving ? 'Saving…' : 'Save rule pack requirement'}
          </button>
        </div>
      ) : (
        <p className="mt-3 text-sm text-slate-400">Rule pack requirements require trainarr admin access.</p>
      )}

      {requirements.length === 0 ? (
        <p className="mt-4 text-sm text-slate-500">No rule pack requirements linked.</p>
      ) : (
        <ul className="mt-4 space-y-2">
          {requirements.map((requirement) => (
            <li
              key={requirement.requirementId}
              className="rounded-lg border border-slate-700 bg-slate-950/40 p-3 text-sm"
            >
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-mono font-medium text-slate-100">{requirement.rulePackKey}</p>
                  {requirement.metadata ? (
                    <>
                      <p className="mt-1 text-slate-300">{requirement.metadata.label}</p>
                      <p className="mt-1 text-xs text-slate-400">
                        {requirement.metadata.regulatoryProgramLabel} · v{requirement.metadata.versionNumber} ·{' '}
                        {requirement.metadata.status}
                      </p>
                    </>
                  ) : null}
                </div>
                {canManage ? (
                  <button
                    type="button"
                    className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-300 hover:bg-slate-800 disabled:opacity-50"
                    disabled={isRemovingId === requirement.requirementId}
                    onClick={() => onRemove(requirement.requirementId)}
                  >
                    {isRemovingId === requirement.requirementId ? 'Removing…' : 'Remove'}
                  </button>
                ) : null}
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
