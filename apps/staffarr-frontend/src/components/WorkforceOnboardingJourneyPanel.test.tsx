import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { WorkforceOnboardingJourneyPanel } from './WorkforceOnboardingJourneyPanel'

describe('WorkforceOnboardingJourneyPanel', () => {
  it('renders journey steps and overall summary', () => {
    render(
      <WorkforceOnboardingJourneyPanel
        personDisplayName="Ada Lovelace"
        isLoading={false}
        isError={false}
        journey={{
          personId: '11111111-1111-1111-1111-111111111111',
          journeyKey: 'new_employee_to_qualified_worker',
          overallStatus: 'in_progress',
          overallSummary: 'Onboarding is in progress.',
          trainarrIntegrationNote: null,
          steps: [
            {
              stepKey: 'workforce_profile',
              title: 'Workforce profile',
              detail: 'Profile detail',
              status: 'complete',
              statusReason: null,
            },
            {
              stepKey: 'trainarr_training_assigned',
              title: 'TrainArr programs assigned',
              detail: 'Training detail',
              status: 'pending',
              statusReason: 'No training assignment recorded yet.',
            },
          ],
        }}
      />,
    )

    expect(screen.getByTestId('workforce-onboarding-journey-summary').textContent).toMatch(/in progress/i)
    expect(screen.getByTestId('workforce-onboarding-step-workforce_profile')).toBeTruthy()
    expect(screen.getByText(/No training assignment recorded yet/i)).toBeTruthy()
  })

  it('shows loading state', () => {
    render(
      <WorkforceOnboardingJourneyPanel
        personDisplayName="Ada Lovelace"
        journey={null}
        isLoading
        isError={false}
      />,
    )

    expect(screen.getByText(/Loading onboarding journey/i)).toBeTruthy()
  })

  it('shows retryable error callout when journey fails to load', () => {
    const onRetryRead = vi.fn()
    render(
      <WorkforceOnboardingJourneyPanel
        personDisplayName="Ada Lovelace"
        journey={null}
        isLoading={false}
        isError
        readErrorMessage="journey unavailable"
        onRetryRead={onRetryRead}
      />,
    )

    expect(screen.getByText('journey unavailable')).toBeTruthy()
    screen.getByRole('button', { name: 'Retry journey' }).click()
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })
})
