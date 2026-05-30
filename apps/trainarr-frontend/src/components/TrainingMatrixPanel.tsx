import { buildSemanticKey, GeneratedKeyField } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

import type {
  CreateTrainingMatrixEntryRequest,
  TrainingDefinitionResponse,
  TrainingMatrixEntryResponse,
  TrainingProgramSummaryResponse,
} from '../api/types'

interface TrainingMatrixPanelProps {
  entries: TrainingMatrixEntryResponse[]
  programs: TrainingProgramSummaryResponse[]
  definitions: TrainingDefinitionResponse[]
  applicabilityKey: string
  applicabilityLabel: string
  targetType: 'program' | 'definition'
  targetId: string
  requirementLevel: string
  sortOrder: string
  onApplicabilityKeyChange: (value: string) => void
  onApplicabilityLabelChange: (value: string) => void
  onTargetTypeChange: (value: 'program' | 'definition') => void
  onTargetIdChange: (value: string) => void
  onRequirementLevelChange: (value: string) => void
  onSortOrderChange: (value: string) => void
  onCreateEntry: () => void
  onDeleteEntry: (matrixEntryId: string) => void
  isCreating: boolean
  deletingEntryId: string | null
  canManage: boolean
}

export function TrainingMatrixPanel({
  entries,
  programs,
  definitions,
  applicabilityKey,
  applicabilityLabel,
  targetType,
  targetId,
  requirementLevel,
  sortOrder,
  onApplicabilityKeyChange,
  onApplicabilityLabelChange,
  onTargetTypeChange,
  onTargetIdChange,
  onRequirementLevelChange,
  onSortOrderChange,
  onCreateEntry,
  onDeleteEntry,
  isCreating,
  deletingEntryId,
  canManage,
}: TrainingMatrixPanelProps) {
  const [showApplicabilityKeyPolicy, setShowApplicabilityKeyPolicy] = useState(false)
  const generatedApplicabilityKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'train',
        kind: 'applicability',
        title: applicabilityLabel.trim(),
        existingKeys: entries.map((entry) => entry.applicabilityKey),
        maxLength: 128,
      }),
    [applicabilityLabel, entries],
  )

  useEffect(() => {
    onApplicabilityKeyChange(generatedApplicabilityKey)
  }, [generatedApplicabilityKey, onApplicabilityKeyChange])

  const targets =
    targetType === 'program'
      ? programs.map((p) => ({ id: p.programId, label: p.name }))
      : definitions.map((d) => ({ id: d.trainingDefinitionId, label: d.name }))

  const grouped = entries.reduce<Record<string, TrainingMatrixEntryResponse[]>>((acc, entry) => {
    const key = entry.applicabilityKey
    acc[key] = acc[key] ?? []
    acc[key].push(entry)
    return acc
  }, {})

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4" data-testid="training-matrix-panel">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Training matrix</h2>
      <p className="mt-1 text-xs text-slate-500">
        Map role or position applicability keys to required programs or definitions (StaffArr owns org truth; keys
        are local references).
      </p>

      {!canManage ? (
        <p className="mt-3 text-sm text-slate-400">Matrix editing requires trainarr admin access.</p>
      ) : (
        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <div className="space-y-1">
            <GeneratedKeyField
              sourceLabel={applicabilityLabel.trim()}
              generatedKey={generatedApplicabilityKey}
              confirmedKey={applicabilityKey}
              manualOverride=""
              onManualOverrideChange={() => {}}
              showAdvancedKey={showApplicabilityKeyPolicy}
              disabled={isCreating}
              label="Applicability key"
            />
            {!showApplicabilityKeyPolicy ? (
              <button
                type="button"
                className="text-xs text-slate-500 underline-offset-2 hover:text-slate-300 hover:underline"
                onClick={() => setShowApplicabilityKeyPolicy(true)}
                disabled={isCreating}
              >
                Key policy
              </button>
            ) : null}
          </div>
          <label htmlFor="training-matrix-label" className="block text-xs text-slate-400">
            Label
            <input
              id="training-matrix-label"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              value={applicabilityLabel}
              onChange={(e) => onApplicabilityLabelChange(e.target.value)}
            />
          </label>
          <label htmlFor="training-matrix-target-type" className="block text-xs text-slate-400">
            Target type
            <select
              id="training-matrix-target-type"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              value={targetType}
              onChange={(e) => onTargetTypeChange(e.target.value as 'program' | 'definition')}
            >
              <option value="program">Program</option>
              <option value="definition">Definition</option>
            </select>
          </label>
          <label htmlFor="training-matrix-target" className="block text-xs text-slate-400">
            Target
            <select
              id="training-matrix-target"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              value={targetId}
              onChange={(e) => onTargetIdChange(e.target.value)}
            >
              <option value="">Select…</option>
              {targets.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.label}
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="training-matrix-requirement-level" className="block text-xs text-slate-400">
            Requirement
            <select
              id="training-matrix-requirement-level"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              value={requirementLevel}
              onChange={(e) => onRequirementLevelChange(e.target.value)}
            >
              <option value="required">Required</option>
              <option value="recommended">Recommended</option>
            </select>
          </label>
          <label htmlFor="training-matrix-sort-order" className="block text-xs text-slate-400">
            Sort order
            <input
              id="training-matrix-sort-order"
              type="number"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              value={sortOrder}
              onChange={(e) => onSortOrderChange(e.target.value)}
            />
          </label>
          <div className="sm:col-span-2">
            <button
              type="button"
              className="rounded bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
              disabled={isCreating || !applicabilityKey.trim() || !targetId}
              onClick={onCreateEntry}
            >
              {isCreating ? 'Adding…' : 'Add matrix row'}
            </button>
          </div>
        </div>
      )}

      <div className="mt-6 border-t border-slate-700 pt-4">
        <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Matrix rows</h3>
        {entries.length === 0 ? (
          <p className="mt-2 text-sm text-slate-500">No matrix rows yet.</p>
        ) : (
          <ul className="mt-2 space-y-4">
            {Object.entries(grouped).map(([key, rows]) => (
              <li key={key}>
                <p className="text-xs font-medium uppercase tracking-wide text-violet-300">{key}</p>
                <ul className="mt-1 space-y-1">
                  {rows.map((row) => (
                    <li
                      key={row.matrixEntryId}
                      className="flex flex-wrap items-center justify-between gap-2 rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm"
                    >
                      <span className="text-slate-200">
                        {row.applicabilityLabel} ·{' '}
                        {row.trainingProgramName ?? row.trainingDefinitionName ?? '—'} · {row.requirementLevel}
                      </span>
                      {canManage ? (
                        <button
                          type="button"
                          className="text-xs text-red-300 hover:text-red-200 disabled:opacity-50"
                          disabled={deletingEntryId === row.matrixEntryId}
                          onClick={() => onDeleteEntry(row.matrixEntryId)}
                        >
                          Remove
                        </button>
                      ) : null}
                    </li>
                  ))}
                </ul>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}

export function buildMatrixCreatePayload(
  applicabilityKey: string,
  applicabilityLabel: string,
  targetType: 'program' | 'definition',
  targetId: string,
  requirementLevel: string,
  sortOrder: string,
): CreateTrainingMatrixEntryRequest {
  return {
    applicabilityKey: applicabilityKey.trim(),
    applicabilityLabel: applicabilityLabel.trim(),
    trainingProgramId: targetType === 'program' ? targetId : null,
    trainingDefinitionId: targetType === 'definition' ? targetId : null,
    requirementLevel,
    sortOrder: Number.parseInt(sortOrder, 10) || 0,
  }
}
