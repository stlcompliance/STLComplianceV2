import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AssignmentStepsPanel } from './AssignmentStepsPanel'

const steps = [
  {
    progressId: 'progress-1',
    trainingAssignmentId: 'assignment-1',
    stepId: 'step-1',
    stepKey: 'lesson-1',
    name: 'Lesson 1',
    description: 'Read and acknowledge the lesson.',
    stepType: 'content' as const,
    configJson: JSON.stringify(
      {
        title: 'Lesson overview',
        body: 'Review the assigned material and acknowledge the lesson.',
        externalUrl: 'https://example.com/lesson',
        requireAcknowledgement: true,
      },
      null,
      2,
    ),
    sortOrder: 0,
    status: 'pending',
    isVisible: true,
    quizScorePercent: null,
    responseJson: null,
    completedAt: null,
  },
]

describe('AssignmentStepsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders a content lesson and submits acknowledgement', () => {
    const onSubmitStep = vi.fn().mockResolvedValue(undefined)

    render(
      <AssignmentStepsPanel
        steps={steps}
        isLoading={false}
        canComplete
        canEvaluate={false}
        isSubmitting={false}
        onSubmitStep={onSubmitStep}
      />,
    )

    expect(screen.getByText('Lesson overview')).toBeInTheDocument()
    expect(screen.getByText('Lesson overview')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /open reference link/i })).toHaveAttribute(
      'href',
      'https://example.com/lesson',
    )

    fireEvent.click(screen.getByLabelText(/I have reviewed and acknowledge this lesson/i))
    fireEvent.click(screen.getByRole('button', { name: /submit step/i }))

    expect(onSubmitStep).toHaveBeenCalledWith(
      'step-1',
      expect.objectContaining({
        contentAcknowledged: true,
        practicalResult: 'pass',
      }),
    )
  })
})
