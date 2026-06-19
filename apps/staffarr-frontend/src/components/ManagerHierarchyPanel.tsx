import { type FormEvent, useMemo, useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import type {
  ManagerChainEntryResponse,
  StaffPersonSummaryResponse,
  SubordinateSummaryResponse,
} from '../api/types'

interface ManagerHierarchyPanelProps {
  selectedPersonId: string
  selectedPersonDisplayName: string
  people: StaffPersonSummaryResponse[]
  managerChain: ManagerChainEntryResponse[]
  subordinates: SubordinateSummaryResponse[]
  selectedSubordinateId?: string | null
  selectedSubordinate: SubordinateSummaryResponse | null
  isLoading?: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
  isLoadingSubordinateDetail?: boolean
  isSubordinateDetailError?: boolean
  subordinateDetailErrorMessage?: string | null
  onRetrySubordinateDetail?: () => void
  canManage: boolean
  isSubmitting: boolean
  actionErrorMessage: string | null
  onSelectSubordinate: (subordinatePersonId: string) => void
  onUpdateManager: (managerPersonId: string | null) => Promise<void>
}

export function formatManagerMutationError(errorMessage: string | null): string | null {
  if (!errorMessage) {
    return null
  }

  const normalized = errorMessage.toLowerCase()
  if (normalized.includes('"status":403') || normalized.includes('forbidden')) {
    return `Forbidden: ${errorMessage}`
  }

  if (normalized.includes('"status":409') || normalized.includes('cycle') || normalized.includes('conflict')) {
    return `Conflict: ${errorMessage}`
  }

  if (normalized.includes('"status":400') || normalized.includes('validation')) {
    return `Validation: ${errorMessage}`
  }

  return errorMessage
}

function indentClass(depth: number): string {
  if (depth <= 1) {
    return 'pl-0'
  }

  if (depth === 2) {
    return 'pl-4'
  }

  return 'pl-8'
}

export function ManagerHierarchyPanel({
  selectedPersonId,
  selectedPersonDisplayName,
  people,
  managerChain,
  subordinates,
  selectedSubordinateId = null,
  selectedSubordinate,
  isLoading = false,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
  isLoadingSubordinateDetail = false,
  isSubordinateDetailError = false,
  subordinateDetailErrorMessage = null,
  onRetrySubordinateDetail,
  canManage,
  isSubmitting,
  actionErrorMessage,
  onSelectSubordinate,
  onUpdateManager,
}: ManagerHierarchyPanelProps) {
  const [managerPersonId, setManagerPersonId] = useState<string>('')
  const normalizedError = formatManagerMutationError(actionErrorMessage)

  const managerCandidates = useMemo(
    () =>
      people
        .filter((person) => person.personId !== selectedPersonId)
        .sort((left, right) => left.displayName.localeCompare(right.displayName)),
    [people, selectedPersonId],
  )
  const managerOptions = useMemo<PickerOption[]>(
    () => managerCandidates.map((candidate) => ({ value: candidate.personId, label: candidate.displayName })),
    [managerCandidates],
  )
  const selectedManagerOption = managerOptions.find((option) => option.value === managerPersonId)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onUpdateManager(managerPersonId || null)
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-medium text-slate-300">Manager and subordinates</h2>
        <span className={`text-xs ${canManage ? 'text-emerald-300' : 'text-[var(--color-text-muted)]'}`}>
          {canManage ? 'Write enabled' : 'Read only'}
        </span>
      </div>
      <p className="mt-2 text-xs text-[var(--color-text-muted)]">Hierarchy workspace for {selectedPersonDisplayName}</p>
      {normalizedError ? (
        <div className="mt-3">
          <ApiErrorCallout title="Manager update failed" message={normalizedError} />
        </div>
      ) : null}
      {isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Manager hierarchy unavailable"
            message={readErrorMessage ?? 'Failed to load manager chain and subordinate hierarchy.'}
            onRetry={onRetryRead}
            retryLabel="Retry hierarchy"
          />
        </div>
      ) : null}

      <div className="mt-5 grid gap-6 lg:grid-cols-2">
        <div>
          <h3 className="text-sm font-medium text-slate-300">Manager chain</h3>
          {isLoading ? (
            <p className="mt-3 text-sm text-slate-400">Loading manager chain…</p>
          ) : !isError && managerChain.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No manager assigned above this person.</p>
          ) : !isError ? (
            <ol className="mt-3 space-y-2 text-sm text-slate-200">
              {managerChain.map((entry) => (
                <li key={entry.personId}>
                  <span className="font-medium text-white">{entry.displayName}</span>
                  <span className="text-slate-400"> ({entry.jobTitle ?? 'No title'})</span>
                </li>
              ))}
            </ol>
          ) : null}
        </div>

        <div>
          <h3 className="text-sm font-medium text-slate-300">Subordinates (tree view)</h3>
          {isLoading ? (
            <p className="mt-3 text-sm text-slate-400">Loading subordinates…</p>
          ) : !isError && subordinates.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No subordinates found for this person.</p>
          ) : !isError ? (
            <ul className="mt-3 divide-y divide-slate-700 text-sm">
              {subordinates.map((subordinate) => (
                <li key={subordinate.personId} className={`py-2 ${indentClass(subordinate.depth)}`}>
                  <button
                    type="button"
                    className="text-left text-white hover:text-sky-300"
                    onClick={() => onSelectSubordinate(subordinate.personId)}
                  >
                    {subordinate.displayName}
                  </button>
                  <p className="text-xs text-slate-400">
                    {subordinate.jobTitle ?? 'No title'} · reports: {subordinate.directReportCount}
                  </p>
                </li>
              ))}
            </ul>
          ) : null}
        </div>
      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-2">
        <div>
          <h3 className="text-sm font-medium text-slate-300">Subordinate detail</h3>
          {!selectedSubordinateId ? (
            <p className="mt-3 text-sm text-slate-400">Select a subordinate to view detail.</p>
          ) : isLoadingSubordinateDetail ? (
            <p className="mt-3 text-sm text-slate-400">Loading subordinate detail…</p>
          ) : isSubordinateDetailError ? (
            <div className="mt-3">
              <ApiErrorCallout
                title="Subordinate detail unavailable"
                message={subordinateDetailErrorMessage ?? 'Failed to load subordinate detail.'}
                onRetry={onRetrySubordinateDetail}
                retryLabel="Retry subordinate detail"
              />
            </div>
          ) : !selectedSubordinate ? (
            <p className="mt-3 text-sm text-slate-400">Subordinate detail is unavailable.</p>
          ) : (
            <dl className="mt-3 grid gap-2 text-sm">
              <div className="flex justify-between gap-3">
                <dt className="text-[var(--color-text-muted)]">Name</dt>
                <dd className="text-white">{selectedSubordinate.displayName}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="text-[var(--color-text-muted)]">Email</dt>
                <dd className="text-white">{selectedSubordinate.primaryEmail}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="text-[var(--color-text-muted)]">Manager</dt>
                <dd className="text-white">{selectedSubordinate.managerDisplayName ?? 'None'}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="text-[var(--color-text-muted)]">Assignment</dt>
                <dd className="text-right text-white">{selectedSubordinate.activeAssignmentPath ?? 'Unassigned'}</dd>
              </div>
            </dl>
          )}
        </div>

        {canManage ? (
          <form className="space-y-3" onSubmit={handleSubmit}>
            <h3 className="text-sm font-medium text-slate-300">Update selected person manager</h3>
            <label className="block text-sm text-slate-300">
              Manager
              <div className="mt-1">
                <StaticSearchPicker
                  value={managerPersonId}
                  onChange={setManagerPersonId}
                  options={managerOptions}
                  selectedOption={selectedManagerOption}
                  placeholder="No manager"
                  testId="manager-hierarchy-manager"
                />
              </div>
            </label>
            <button
              type="submit"
              className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Saving…' : 'Update manager'}
            </button>
          </form>
        ) : (
          <p className="mt-2 text-xs text-[var(--color-text-muted)]">
            Your role does not include manager hierarchy write permission.
          </p>
        )}
      </div>
    </section>
  )
}
