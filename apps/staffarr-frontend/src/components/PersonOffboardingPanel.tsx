import { type FormEvent, useEffect, useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker } from '@stl/shared-ui'
import type { PersonOffboardingResponse } from '../api/types'

interface PersonOffboardingPanelProps {
  personId: string
  personDisplayName: string
  peopleOptions: Array<{ personId: string; displayName: string }>
  offboarding: PersonOffboardingResponse | null
  isLoading: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
  canManage: boolean
  isSubmitting: boolean
  actionErrorMessage?: string | null
  onStart: (request: {
    separationDate: string
    separationReason: string | null
    targetEmploymentStatus: string
    disableLoginRequested: boolean
    newManagerPersonIdForReports: string | null
  }) => Promise<void>
  onExecute: (request: { newManagerPersonIdForReports: string | null }) => Promise<void>
}

function statusTone(status: string) {
  switch (status) {
    case 'complete':
      return 'success'
    case 'blocked':
      return 'danger'
    case 'skipped':
      return 'inactive'
    default:
      return 'pending'
  }
}

export function PersonOffboardingPanel({
  personId,
  personDisplayName,
  peopleOptions,
  offboarding,
  isLoading,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
  canManage,
  isSubmitting,
  actionErrorMessage = null,
  onStart,
  onExecute,
}: PersonOffboardingPanelProps) {
  const [separationDate, setSeparationDate] = useState('')
  const [separationReason, setSeparationReason] = useState('')
  const [targetEmploymentStatus, setTargetEmploymentStatus] = useState('inactive')
  const [disableLoginRequested, setDisableLoginRequested] = useState(true)
  const [newManagerPersonId, setNewManagerPersonId] = useState('')
  const [executeManagerPersonId, setExecuteManagerPersonId] = useState('')

  useEffect(() => {
    setSeparationDate('')
    setSeparationReason('')
    setTargetEmploymentStatus('inactive')
    setDisableLoginRequested(true)
    setNewManagerPersonId('')
    setExecuteManagerPersonId('')
  }, [personId])

  const managerChoices = peopleOptions.filter((person) => person.personId !== personId)
  const managerOptions = managerChoices.map((person) => ({ value: person.personId, label: person.displayName }))
  const selectedNewManagerOption = managerOptions.find((option) => option.value === newManagerPersonId)
  const selectedExecuteManagerOption = managerOptions.find((option) => option.value === executeManagerPersonId)
  const inProgress = offboarding?.status === 'in_progress'

  const handleStart = async (event: FormEvent) => {
    event.preventDefault()
    await onStart({
      separationDate: new Date(separationDate).toISOString(),
      separationReason: separationReason.trim() || null,
      targetEmploymentStatus,
      disableLoginRequested,
      newManagerPersonIdForReports: newManagerPersonId || null,
    })
  }

  const handleExecute = async (event: FormEvent) => {
    event.preventDefault()
    if (!offboarding) {
      return
    }

    await onExecute({
      newManagerPersonIdForReports: executeManagerPersonId || null,
    })
  }

  return (
    <section
      className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6"
      data-testid="person-offboarding-panel"
    >
      <header>
        <h2 className="text-sm font-medium text-slate-300">Workforce offboarding</h2>
        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
          Coordinate separation for {personDisplayName}: checklist, permission removal, and inactive status while
          preserving personnel history.
        </p>
      </header>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading offboarding state…</p>
      ) : isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Offboarding workflow unavailable"
            message={readErrorMessage ?? 'Failed to load offboarding workflow state.'}
            onRetry={onRetryRead}
            retryLabel="Retry offboarding"
          />
        </div>
      ) : null}

      {actionErrorMessage ? (
        <div className="mt-4">
          <ApiErrorCallout title="Offboarding action failed" message={actionErrorMessage} />
        </div>
      ) : null}

      {!isError && offboarding ? (
        <div className="mt-4 space-y-4">
          <dl className="grid gap-3 text-sm md:grid-cols-2">
            <div className="flex justify-between gap-4">
              <dt className="text-[var(--color-text-muted)]">Status</dt>
              <dd className="text-right uppercase tracking-wide text-slate-200">{offboarding.status}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-[var(--color-text-muted)]">Separation date</dt>
              <dd className="text-right text-slate-200">
                {new Date(offboarding.separationDate).toLocaleDateString()}
              </dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-[var(--color-text-muted)]">Target status</dt>
              <dd className="text-right uppercase tracking-wide text-slate-200">
                {offboarding.targetEmploymentStatus}
              </dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-[var(--color-text-muted)]">Active direct reports</dt>
              <dd className="text-right text-slate-200">{offboarding.activeDirectReportCount}</dd>
            </div>
          </dl>

          <ol className="space-y-3" data-testid="person-offboarding-steps">
            {offboarding.steps.map((step) => (
              <li
                key={step.stepKey}
                className="rounded-md border border-slate-800 bg-slate-950/50 px-3 py-3"
                data-testid={`person-offboarding-step-${step.stepKey}`}
              >
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <p className="text-sm font-medium text-slate-100">{step.title}</p>
                    <p className="mt-1 text-xs text-[var(--color-text-muted)]">{step.detail}</p>
                    {step.blockerDetail ? (
                      <p className="mt-1 text-xs text-amber-300">{step.blockerDetail}</p>
                    ) : null}
                  </div>
                  <span
                    className="stl-tone-badge rounded border px-2 py-0.5 text-xs font-medium uppercase tracking-wide"
                    data-tone={statusTone(step.status)}
                  >
                    {step.status}
                  </span>
                </div>
              </li>
            ))}
          </ol>

          {canManage && inProgress ? (
            <form className="grid gap-4 md:grid-cols-2" onSubmit={handleExecute}>
              {offboarding.activeDirectReportCount > 0 ? (
                <StaticSearchPicker
                  id="execute-offboarding-manager"
                  label="Replacement manager for direct reports"
                  value={executeManagerPersonId}
                  onChange={setExecuteManagerPersonId}
                  options={managerOptions}
                  placeholder="Search replacement managers…"
                  testId="execute-offboarding-manager-picker"
                  selectedOption={selectedExecuteManagerOption}
                />
              ) : null}
              <div className="md:col-span-2">
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="rounded-md bg-rose-700 px-4 py-2 text-sm font-medium text-white hover:bg-rose-600 disabled:opacity-50"
                >
                  {isSubmitting ? 'Executing…' : 'Execute offboarding'}
                </button>
              </div>
            </form>
          ) : null}
        </div>
      ) : !isError && canManage ? (
        <form className="mt-4 grid gap-4 md:grid-cols-2" onSubmit={handleStart}>
          <label htmlFor="offboarding-separation-date" className="block text-sm text-slate-300">
            Separation date
            <input
              id="offboarding-separation-date"
              type="date"
              value={separationDate}
              onChange={(event) => setSeparationDate(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              required
            />
          </label>
          <label htmlFor="offboarding-target-status" className="block text-sm text-slate-300">
            Target employment status
            <select
              id="offboarding-target-status"
              value={targetEmploymentStatus}
              onChange={(event) => setTargetEmploymentStatus(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="inactive">Inactive</option>
              <option value="terminated">Terminated</option>
            </select>
          </label>
          <label htmlFor="offboarding-reason" className="block text-sm text-slate-300 md:col-span-2">
            Separation reason
            <input
              id="offboarding-reason"
              value={separationReason}
              onChange={(event) => setSeparationReason(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              placeholder="Optional separation reason"
            />
          </label>
          <StaticSearchPicker
            id="offboarding-manager"
            label="Replacement manager for direct reports (optional at start)"
            value={newManagerPersonId}
            onChange={setNewManagerPersonId}
            options={managerOptions}
            placeholder="Search replacement managers…"
            testId="offboarding-manager-picker"
            selectedOption={selectedNewManagerOption}
          />
          <label className="flex items-center gap-2 text-sm text-slate-300 md:col-span-2">
            <input
              type="checkbox"
              checked={disableLoginRequested}
              onChange={(event) => setDisableLoginRequested(event.target.checked)}
            />
            Request NexArr login disable when appropriate
          </label>
          <div className="md:col-span-2">
            <button
              type="submit"
              disabled={isSubmitting}
              className="rounded-md bg-[var(--color-bg-control-hover)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-surface)] disabled:opacity-50"
            >
              {isSubmitting ? 'Starting…' : 'Start offboarding'}
            </button>
          </div>
        </form>
      ) : !isError ? (
        <p className="mt-4 text-sm text-slate-400">No offboarding record exists for this person.</p>
      ) : null}
    </section>
  )
}
