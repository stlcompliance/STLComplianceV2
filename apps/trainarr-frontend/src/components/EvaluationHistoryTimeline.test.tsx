import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { EvaluationHistoryTimeline } from './EvaluationHistoryTimeline'

describe('EvaluationHistoryTimeline', () => {
  it('renders empty message when no history', () => {
    render(<EvaluationHistoryTimeline items={[]} />)
    expect(screen.getByText(/no evaluation history yet/i)).toBeInTheDocument()
  })

  it('renders current and superseded entries', () => {
    render(
      <EvaluationHistoryTimeline
        items={[
          {
            entryId: 'current',
            trainingAssignmentId: 'a1',
            result: 'pass',
            score: 95,
            notes: 'Ready',
            evaluatorUserId: 'u1',
            evaluatedAt: '2026-05-28T12:00:00Z',
            isCurrent: true,
            supersededAt: null,
          },
          {
            entryId: 'old',
            trainingAssignmentId: 'a1',
            result: 'fail',
            score: 50,
            notes: 'First attempt',
            evaluatorUserId: 'u1',
            evaluatedAt: '2026-05-27T12:00:00Z',
            isCurrent: false,
            supersededAt: '2026-05-28T11:00:00Z',
          },
        ]}
      />,
    )

    expect(screen.getByText('Current')).toBeInTheDocument()
    expect(screen.getByText('Superseded')).toBeInTheDocument()
    expect(screen.getByText('Ready')).toBeInTheDocument()
    expect(screen.getByText('First attempt')).toBeInTheDocument()
  })
})
