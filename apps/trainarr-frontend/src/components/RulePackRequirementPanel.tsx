import { useMemo } from 'react'
import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useEffect, useState } from 'react'

import type { TrainingRulePackRequirementResponse } from '../api/types'

interface RulePackRequirementPanelProps {
  mode: 'drawer' | 'details' | 'create'
  title: string
  requirements: TrainingRulePackRequirementResponse[]
  rulePackKeyInput: string
  rulePackOptions: PickerOption[]
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
  mode,
  title,
  requirements,
  rulePackKeyInput,
  rulePackOptions,
  onRulePackKeyChange,
  onSave,
  onRemove,
  isSaving,
  isRemovingId,
  canManage,
  validateWithComplianceCore,
  onValidateWithComplianceCoreChange,
}: RulePackRequirementPanelProps) {
  type RulePackColumnKey = 'label' | 'program' | 'version' | 'status'
  const storageKey = `trainarr.rulepacks.drawer.columns.v1.${title.toLowerCase().replace(/\s+/g, '-')}`
  const allColumns: Array<{ key: RulePackColumnKey; label: string }> = [
    { key: 'label', label: 'Rule pack' },
    { key: 'program', label: 'Regulatory program' },
    { key: 'version', label: 'Version' },
    { key: 'status', label: 'Status' },
  ]
  const [selectedColumns, setSelectedColumns] = useState<RulePackColumnKey[]>(['label', 'program', 'version', 'status'])
  const selectedRulePackOption = useMemo<PickerOption | undefined>(
    () =>
      rulePackOptions.find((option) => option.value === rulePackKeyInput) ??
      (rulePackKeyInput ? { value: rulePackKeyInput, label: rulePackKeyInput } : undefined),
    [rulePackKeyInput, rulePackOptions],
  )
  useEffect(() => {
    try {
      const raw = window.localStorage.getItem(storageKey)
      if (!raw) return
      const parsed = JSON.parse(raw) as RulePackColumnKey[]
      const valid = parsed.filter((column) => allColumns.some((candidate) => candidate.key === column)).slice(0, 5)
      if (valid.length > 0) setSelectedColumns(valid)
    } catch {}
  }, [storageKey])
  useEffect(() => {
    window.localStorage.setItem(storageKey, JSON.stringify(selectedColumns))
  }, [selectedColumns, storageKey])

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">{title}</h2>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        References Compliance Core rule pack keys only — TrainArr does not own rule packs.
      </p>

      {mode === 'create' && canManage ? (
        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <StaticSearchPicker
              id="rule-pack-requirement-key"
              label="Rule pack"
              value={rulePackKeyInput}
              onChange={onRulePackKeyChange}
              options={rulePackOptions}
              selectedOption={selectedRulePackOption}
              placeholder="Search rule packs…"
              testId="rule-pack-requirement-key"
            />
          </div>
          <label htmlFor="rule-pack-requirement-validate-compliance-core" className="flex items-center gap-2 text-xs text-slate-400 sm:col-span-2">
            <input
              id="rule-pack-requirement-validate-compliance-core"
              type="checkbox"
              data-testid="rule-pack-requirement-validate-compliance-core"
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
        <p className="mt-4 text-sm text-[var(--color-text-muted)]">No rule pack requirements linked.</p>
      ) : (
        <div className="mt-4 space-y-3">
          <div className="rounded-md border border-slate-700 p-2">
            <p className="text-xs text-slate-400">Visible columns (max 5)</p>
            <div className="mt-2 flex flex-wrap gap-3">
              {allColumns.map((column) => (
                <label key={column.key} className="inline-flex items-center gap-2 text-xs text-slate-300">
                  <input
                    type="checkbox"
                    checked={selectedColumns.includes(column.key)}
                    onChange={() => {
                      if (selectedColumns.includes(column.key)) {
                        const next = selectedColumns.filter((item) => item !== column.key)
                        if (next.length > 0) setSelectedColumns(next)
                      } else if (selectedColumns.length < 5) {
                        setSelectedColumns([...selectedColumns, column.key])
                      }
                    }}
                  />
                  {column.label}
                </label>
              ))}
            </div>
          </div>
          <div className="overflow-x-auto rounded-md border border-slate-700">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-950/70">
                <tr>
                  {selectedColumns.map((column) => (
                    <th key={column} className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-slate-400">
                      {allColumns.find((item) => item.key === column)?.label}
                    </th>
                  ))}
                  {mode !== 'drawer' && canManage ? <th className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-slate-400">Actions</th> : null}
                </tr>
              </thead>
              <tbody>
          {requirements.map((requirement) => (
                <tr
              key={requirement.requirementId}
              className="border-t border-slate-800"
            >
                  {selectedColumns.map((column) => (
                    <td key={`${requirement.requirementId}-${column}`} className="px-3 py-2 text-slate-200">
                      {column === 'label' ? requirement.metadata?.label ?? 'Rule pack' : null}
                      {column === 'program' ? requirement.metadata?.regulatoryProgramLabel ?? '—' : null}
                      {column === 'version' ? (requirement.metadata ? `v${requirement.metadata.versionNumber}` : '—') : null}
                      {column === 'status' ? requirement.metadata?.status ?? '—' : null}
                    </td>
                  ))}
                {mode !== 'drawer' && canManage ? (
                    <td className="px-3 py-2">
                    <button
                    type="button"
                    className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-300 hover:bg-slate-800 disabled:opacity-50"
                    disabled={isRemovingId === requirement.requirementId}
                    onClick={() => onRemove(requirement.requirementId)}
                  >
                    {isRemovingId === requirement.requirementId ? 'Removing…' : 'Remove'}
                  </button>
                    </td>
                ) : null}
                </tr>
          ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </section>
  )
}
