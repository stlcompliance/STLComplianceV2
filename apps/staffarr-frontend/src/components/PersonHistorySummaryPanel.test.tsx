import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PersonHistorySummaryPanel } from './PersonHistorySummaryPanel'

describe('PersonHistorySummaryPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows retryable error callout when summary query fails', () => {
    const onRetryRead = vi.fn()

    render(
      <PersonHistorySummaryPanel
        personDisplayName="Alex Example"
        summary={null}
        isLoading={false}
        isError
        readErrorMessage="history rollup request failed"
        onRetryRead={onRetryRead}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('History summary unavailable')).toBeTruthy()
    expect(screen.getByText('history rollup request failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry history summary' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })

  it('shows unavailable callout when summary payload is missing', () => {
    render(
      <PersonHistorySummaryPanel personDisplayName="Alex Example" summary={null} isLoading={false} />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('History summary unavailable')).toBeTruthy()
    expect(screen.getByText('History summary has not been generated yet.')).toBeTruthy()
  })

  it('shows pending message when rollup is not materialized', () => {
    render(
      <PersonHistorySummaryPanel
        personDisplayName="Alex Example"
        summary={{
          personId: '00000000-0000-0000-0000-000000000001',
          eventCount: 0,
          incidentCount: 0,
          certificationCount: 0,
          permissionCount: 0,
          readinessCount: 0,
          trainingBlockerCount: 0,
          personnelNoteCount: 0,
          personnelDocumentCount: 0,
          lastEventAt: null,
          computedAt: '1970-01-01T00:00:00.000Z',
          isMaterialized: false,
        }}
        isLoading={false}
      />,
    )

    expect(screen.getByText(/has not been computed yet/i)).toBeTruthy()
  })

  it('shows category counts when materialized', () => {
    render(
      <PersonHistorySummaryPanel
        personDisplayName="Alex Example"
        summary={{
          personId: '00000000-0000-0000-0000-000000000001',
          eventCount: 3,
          incidentCount: 1,
          certificationCount: 2,
          permissionCount: 0,
          readinessCount: 0,
          trainingBlockerCount: 0,
          personnelNoteCount: 0,
          personnelDocumentCount: 0,
          lastEventAt: '2026-05-28T12:00:00.000Z',
          computedAt: '2026-05-28T12:05:00.000Z',
          isMaterialized: true,
        }}
        isLoading={false}
      />,
    )

    expect(screen.getByText('3')).toBeTruthy()
    expect(screen.getByText('2')).toBeTruthy()
  })
})
