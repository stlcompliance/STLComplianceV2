import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { TrainingAcknowledgementsPanel } from './TrainingAcknowledgementsPanel'

vi.mock('../api/client', () => ({
  listTrainingAcknowledgements: vi.fn().mockResolvedValue([
    {
      acknowledgementId: 'ack-1',
      personId: 'person-1',
      trainingAssignmentId: 'assignment-1',
      trainingTitle: 'Hazmat Refresher',
      summary: 'Complete annual hazmat refresher.',
      status: 'pending',
      assignmentReason: 'annual_recertification',
      requestedAt: '2026-05-28T12:00:00.000Z',
      dueAt: null,
      acknowledgedAt: null,
    },
  ]),
  acknowledgeTrainingAssignment: vi.fn().mockResolvedValue(undefined),
}))

describe('TrainingAcknowledgementsPanel', () => {
  it('renders pending acknowledgements', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <TrainingAcknowledgementsPanel
          accessToken="token"
          personId="person-1"
          displayName="Alex Operator"
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('training-acknowledgements-panel')).toBeTruthy()
    expect(screen.getByText('Hazmat Refresher')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Acknowledge assignment' })).toBeTruthy()
  })

  it('shows retryable error callout when acknowledgements query fails', async () => {
    vi.mocked(client.listTrainingAcknowledgements).mockRejectedValueOnce(
      new Error('acknowledgements unavailable'),
    )
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <TrainingAcknowledgementsPanel
          accessToken="token"
          personId="person-1"
          displayName="Alex Operator"
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('acknowledgements unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry acknowledgements' })).toBeTruthy()
  })
})
