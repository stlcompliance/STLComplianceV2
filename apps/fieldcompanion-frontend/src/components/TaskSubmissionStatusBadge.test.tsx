import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { TaskSubmissionStatusBadge } from './TaskSubmissionStatusBadge'

describe('TaskSubmissionStatusBadge', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders submission chips with the expected tone mapping', () => {
    render(
      <TaskSubmissionStatusBadge
        chips={[
          {
            kind: 'acknowledge',
            label: 'Acknowledgment queued',
            tone: 'pending',
            detail: 'Queued locally for sync.',
          },
          {
            kind: 'evidence',
            label: 'Evidence submitted',
            tone: 'success',
            detail: 'Uploaded to the owning product.',
          },
          {
            kind: 'dvir',
            label: 'DVIR failed',
            tone: 'error',
          },
        ]}
      />,
    )

    expect(screen.getByTestId('fieldcompanion-task-submission-status')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-submission-chip-acknowledge')).toHaveAttribute(
      'data-tone',
      'pending',
    )
    expect(screen.getByTestId('fieldcompanion-submission-chip-acknowledge')).toHaveAttribute(
      'title',
      'Queued locally for sync.',
    )
    expect(screen.getByTestId('fieldcompanion-submission-chip-evidence')).toHaveAttribute(
      'data-tone',
      'success',
    )
    expect(screen.getByTestId('fieldcompanion-submission-chip-dvir')).toHaveAttribute(
      'data-tone',
      'danger',
    )
    expect(screen.getByTestId('fieldcompanion-submission-chip-dvir')).not.toHaveAttribute('title')
  })

  it('renders nothing when there are no chips', () => {
    render(<TaskSubmissionStatusBadge chips={[]} />)

    expect(screen.queryByTestId('fieldcompanion-task-submission-status')).toBeNull()
  })
})
