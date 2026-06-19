import { buildProductLaunchUrlMap } from '@stl/shared-ui'
import { ApiErrorCallout } from '@stl/shared-ui'
import { useState } from 'react'
import { createLaunchHandoff } from '../api/client'
import type { WorkforceOnboardingJourneyResponse } from '../api/types'

type Props = {
  accessToken: string
  personDisplayName: string
  journey: WorkforceOnboardingJourneyResponse | null
  isLoading: boolean
  isError: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
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

function statusTone(status: string) {
  switch (status) {
    case 'complete':
      return 'success'
    case 'blocked':
      return 'danger'
    case 'unavailable':
      return 'warning'
    default:
      return 'pending'
  }
}

export function WorkforceOnboardingJourneyPanel({
  accessToken,
  personDisplayName,
  journey,
  isLoading,
  isError,
  readErrorMessage,
  onRetryRead,
}: Props) {
  const launchUrls = buildProductLaunchUrlMap(import.meta.env)
  const trainarrLaunchUrl = launchUrls.trainarr
  const [launchError, setLaunchError] = useState<string | null>(null)
  const [isLaunchingTrainarr, setIsLaunchingTrainarr] = useState(false)

  async function handleOpenTrainArr() {
    if (isLaunchingTrainarr) {
      return
    }

    setLaunchError(null)
    setIsLaunchingTrainarr(true)
    try {
      const callbackUrl = window.location.href
      const handoff = await createLaunchHandoff(accessToken, 'trainarr', callbackUrl)
      window.location.assign(handoff.launchUrl)
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to launch TrainArr via suite handoff.'
      setLaunchError(message)
      setIsLaunchingTrainarr(false)
    }
  }

  return (
    <section
      className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6"
      data-testid="workforce-onboarding-journey-panel"
    >
      <header>
        <h2 className="text-sm font-medium text-slate-300">New employee → qualified worker</h2>
        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
          docs/23 cross-product journey for {personDisplayName}. StaffArr owns profile, org, permissions, and
          readiness; TrainArr owns training workflow truth.
        </p>
      </header>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading onboarding journey…</p>
      ) : null}

      {isError ? (
        <ApiErrorCallout
          className="mt-4"
          message={readErrorMessage ?? 'Failed to load workforce onboarding journey.'}
          onRetry={onRetryRead}
          retryLabel="Retry journey"
        />
      ) : null}

      {launchError ? (
        <ApiErrorCallout className="mt-4" message={launchError} />
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
                    <p className="mt-1 text-xs text-[var(--color-text-muted)]">{step.detail}</p>
                  </div>
                  <span
                    className="stl-tone-badge rounded border px-2 py-0.5 text-xs font-medium uppercase tracking-wide"
                    data-tone={statusTone(step.status)}
                  >
                    {statusLabel(step.status)}
                  </span>
                </div>
                {step.statusReason ? (
                  <p className="mt-2 text-xs text-slate-400">{step.statusReason}</p>
                ) : null}
                {step.stepKey.startsWith('trainarr_') && trainarrLaunchUrl ? (
                  <button
                    type="button"
                    onClick={() => void handleOpenTrainArr()}
                    disabled={isLaunchingTrainarr}
                    className="mt-2 inline-block text-xs text-sky-400 hover:text-sky-300 disabled:cursor-not-allowed disabled:text-[var(--color-text-muted)]"
                  >
                    {isLaunchingTrainarr ? 'Opening TrainArr…' : 'Open TrainArr (suite handoff)'}
                  </button>
                ) : null}
              </li>
            ))}
          </ol>
        </>
      ) : null}
    </section>
  )
}
