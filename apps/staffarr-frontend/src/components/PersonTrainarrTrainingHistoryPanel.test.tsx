import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { PersonTrainarrTrainingHistoryPanel } from './PersonTrainarrTrainingHistoryPanel'

describe('PersonTrainarrTrainingHistoryPanel', () => {
  it('renders TrainArr history entries', () => {
    render(
      <PersonTrainarrTrainingHistoryPanel
        personDisplayName="Alex Driver"
        isLoading={false}
        isError={false}
        history={{
          personId: 'person-1',
          sourceProduct: 'trainarr',
          sourceNote: 'Read-through from TrainArr.',
          totalCount: 1,
          items: [
            {
              entryId: 'entry-1',
              eventKind: 'assignment_created',
              summary: 'Training assignment created',
              relatedEntityType: 'training_assignment',
              relatedEntityId: 'assign-1',
              occurredAt: new Date().toISOString(),
            },
          ],
        }}
      />,
    )

    expect(screen.getByTestId('person-trainarr-training-history-panel')).toBeTruthy()
    expect(screen.getByText('Training assignment created')).toBeTruthy()
  })

  it('shows empty state when no entries', () => {
    render(
      <PersonTrainarrTrainingHistoryPanel
        personDisplayName="Alex Driver"
        isLoading={false}
        isError={false}
        history={{
          personId: 'person-1',
          sourceProduct: 'trainarr',
          sourceNote: 'Read-through from TrainArr.',
          totalCount: 0,
          items: [],
        }}
      />,
    )

    expect(screen.getByTestId('person-trainarr-training-history-empty')).toBeTruthy()
  })

  it('shows retryable error callout when history fails', () => {
    const onRetryRead = vi.fn()
    render(
      <PersonTrainarrTrainingHistoryPanel
        personDisplayName="Alex Driver"
        isLoading={false}
        isError
        readErrorMessage="history unavailable"
        onRetryRead={onRetryRead}
        history={null}
      />,
    )

    expect(screen.getByText('history unavailable')).toBeTruthy()
    screen.getByRole('button', { name: 'Retry history' }).click()
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })
})
