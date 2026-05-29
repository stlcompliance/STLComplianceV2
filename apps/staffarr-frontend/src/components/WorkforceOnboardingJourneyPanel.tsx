import { buildProductLaunchUrlMap } from '@stl/shared-ui'
import type { WorkforceOnboardingJourneyResponse } from '../api/types'

type Props = {
  personDisplayName: string
  journey: WorkforceOnboardingJourneyResponse | null
  isLoading: boolean
  isError: boolean
}

function statusLabel(status: string) {
  switch (status) {
    case 'complete':
      return 'Complete'
    case 'blocked':
      return 'Blocked'
    case 'unavailable':
      return 'Unavailable'
    case 'in_progress':
      return 'In progress'
    default:
      return 'Pending'
  }
}

function statusClass(status: string) {
  switch (status) {
    case 'complete':
      return 'bg-emerald-900/40 text-emerald-200'
    case 'blocked':
      return 'bg-rose-900/40 text-rose-200'
    case 'unavailable':
      return 'bg-amber-900/40 text-amber-200'
    default:
      return 'bg-slate-800 text-slate-300'
  }
}

export function WorkforceOnboardingJourneyPanel({
  personDisplayName,
  journey,
  isLoading,
  isError,
}: Props) {
  const launchUrls = buildProductLaunchUrlMap(import.meta.env)
  const trainarrLaunchUrl = launchUrls.trainarr

  return (
    <section
      className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6"
      data-testid="workforce-onboarding-journey-panel"
    >
      <header>
        <h2 className="text-sm font-medium text-slate-300">New employee → qualified worker</h2>
        <p className="mt-1 text-xs text-slate-500">
          docs/23 cross-product journey for {personDisplayName}. StaffArr owns profile, org, permissions, and
          readiness; TrainArr owns training workflow truth.
        </p>
      </header>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading onboarding journey…</p>
      ) : null}

      {isError ? (
        <p className="mt-4 text-sm text-rose-400" role="alert">
          Failed to load workforce onboarding journey.
        </p>
      ) : null}

      {journey ? (
        <>
          <p
            className="mt-4 text-sm text-slate-200"
            data-testid="workforce-onboarding-journey-summary"
          >
            <span className="font-medium capitalize">{journey.overallStatus.replaceAll('_', ' ')}</span>
            {' — '}
            {journey.overallSummary}
          </p>

          {journey.trainarrIntegrationNote ? (
            <p className="mt-2 text-xs text-amber-300">{journey.trainarrIntegrationNote}</p>
          ) : null}

          <ol className="mt-4 space-y-3" data-testid="workforce-onboarding-journey-steps">
            {journey.steps.map((step) => (
              <li
                key={step.stepKey}
                className="rounded-md border border-slate-800 bg-slate-950/50 px-3 py-3"
                data-testid={`workforce-onboarding-step-${step.stepKey}`}
              >
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <p className="text-sm font-medium text-slate-100">{step.title}</p>
                    <p className="mt-1 text-xs text-slate-500">{step.detail}</p>
                  </div>
                  <span
                    className={`rounded px-2 py-0.5 text-xs font-medium uppercase tracking-wide ${statusClass(step.status)}`}
                  >
                    {statusLabel(step.status)}
                  </span>
                </div>
                {step.statusReason ? (
                  <p className="mt-2 text-xs text-slate-400">{step.statusReason}</p>
                ) : null}
                {step.stepKey.startsWith('trainarr_') && trainarrLaunchUrl ? (
                  <a
                    href={trainarrLaunchUrl}
                    className="mt-2 inline-block text-xs text-sky-400 hover:text-sky-300"
                  >
                    Open TrainArr (suite handoff)
                  </a>
                ) : null}
              </li>
            ))}
          </ol>
        </>
      ) : null}
    </section>
  )
}
