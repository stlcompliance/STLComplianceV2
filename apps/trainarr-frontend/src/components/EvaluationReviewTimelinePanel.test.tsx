import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { EvaluationReviewTimelinePanel } from './EvaluationReviewTimelinePanel'

vi.mock('../api/client', () => ({
  getTrainingEvaluationReviewTimeline: vi.fn().mockResolvedValue({
    items: [],
    totalCount: 0,
    limit: 25,
  }),
}))

describe('EvaluationReviewTimelinePanel', () => {
  it('renders empty timeline state for reviewers', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <EvaluationReviewTimelinePanel
          accessToken="token"
          canReview={true}
          selectedAssignmentId={null}
          onSelectAssignment={() => undefined}
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/No evaluations recorded yet/i)).toBeInTheDocument()
  })

  it('shows retryable error callout when timeline query fails', async () => {
    vi.mocked(client.getTrainingEvaluationReviewTimeline).mockRejectedValueOnce(
      new Error('timeline unavailable'),
    )
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <EvaluationReviewTimelinePanel
          accessToken="token"
          canReview={true}
          selectedAssignmentId={null}
          onSelectAssignment={() => undefined}
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('timeline unavailable')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Retry evaluation timeline' }))
    expect(client.getTrainingEvaluationReviewTimeline).toHaveBeenCalled()
  })
})
