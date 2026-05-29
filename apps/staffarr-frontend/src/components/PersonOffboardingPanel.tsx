import { type FormEvent, useEffect, useState } from 'react'
import type { PersonOffboardingResponse } from '../api/types'

interface PersonOffboardingPanelProps {
  personId: string
  personDisplayName: string
  peopleOptions: Array<{ personId: string; displayName: string }>
  offboarding: PersonOffboardingResponse | null
  isLoading: boolean
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
  onStart: (request: {
    separationDate: string
    separationReason: string | null
    targetEmploymentStatus: string
    disableLoginRequested: boolean
    newManagerPersonIdForReports: string | null
  }) => Promise<void>
  onExecute: (request: { newManagerPersonIdForReports: string | null }) => Promise<void>
}

function statusClass(status: string) {
  switch (status) {
    case 'complete':
      return 'bg-emerald-900/40 text-emerald-200'
    case 'blocked':
      return 'bg-rose-900/40 text-rose-200'
    case 'skipped':
      return 'bg-slate-800 text-slate-400'
    default:
      return 'bg-slate-800 text-slate-300'
  }
}

export function PersonOffboardingPanel({
  personId,
  personDisplayName,
  peopleOptions,
  offboarding,
  isLoading,
  canManage,
  isSubmitting,
  errorMessage,
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
        <p className="mt-1 text-xs text-slate-500">
          Coordinate separation for {personDisplayName}: checklist, permission removal, and inactive status while
          preserving personnel history.
        </p>
      </header>

      {isLoading ? <p className="mt-4 text-sm text-slate-400">Loading offboarding state…</p> : null}

      {errorMessage ? (
        <p className="mt-4 text-sm text-rose-400" role="alert">
          {errorMessage}
        </p>
      ) : null}

      {offboarding ? (
        <div className="mt-4 space-y-4">
          <dl className="grid gap-3 text-sm md:grid-cols-2">
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Status</dt>
              <dd className="text-right uppercase tracking-wide text-slate-200">{offboarding.status}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Separation date</dt>
              <dd className="text-right text-slate-200">
                {new Date(offboarding.separationDate).toLocaleDateString()}
              </dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Target status</dt>
              <dd className="text-right uppercase tracking-wide text-slate-200">
                {offboarding.targetEmploymentStatus}
              </dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Active direct reports</dt>
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
                    <p className="mt-1 text-xs text-slate-500">{step.detail}</p>
                    {step.blockerDetail ? (
                      <p className="mt-1 text-xs text-amber-300">{step.blockerDetail}</p>
                    ) : null}
                  </div>
                  <span
                    className={`rounded px-2 py-0.5 text-xs font-medium uppercase tracking-wide ${statusClass(step.status)}`}
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
                <label htmlFor="execute-offboarding-manager" className="block text-sm text-slate-300 md:col-span-2">
                  Replacement manager for direct reports
                  <select
                    id="execute-offboarding-manager"
                    value={executeManagerPersonId}
                    onChange={(event) => setExecuteManagerPersonId(event.target.value)}
                    className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    required={!offboarding.newManagerPersonIdForReports}
                  >
                    <option value="">
                      {offboarding.newManagerPersonIdForReports ? 'Use manager from start request' : 'Select manager'}
                    </option>
                    {managerChoices.map((person) => (
                      <option key={person.personId} value={person.personId}>
                        {person.displayName}
                      </option>
                    ))}
                  </select>
                </label>
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
      ) : canManage ? (
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
          <label htmlFor="offboarding-manager" className="block text-sm text-slate-300 md:col-span-2">
            Replacement manager for direct reports (optional at start)
            <select
              id="offboarding-manager"
              value={newManagerPersonId}
              onChange={(event) => setNewManagerPersonId(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">Assign during execution if needed</option>
              {managerChoices.map((person) => (
                <option key={person.personId} value={person.personId}>
                  {person.displayName}
                </option>
              ))}
            </select>
          </label>
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
              className="rounded-md bg-slate-100 px-4 py-2 text-sm font-medium text-slate-900 hover:bg-white disabled:opacity-50"
            >
              {isSubmitting ? 'Starting…' : 'Start offboarding'}
            </button>
          </div>
        </form>
      ) : (
        <p className="mt-4 text-sm text-slate-400">No offboarding record exists for this person.</p>
      )}
    </section>
  )
}
