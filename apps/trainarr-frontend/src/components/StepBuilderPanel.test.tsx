import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { StepBuilderPanel } from './StepBuilderPanel'

const definition = {
  trainingDefinitionId: 'def-1',
  definitionKey: 'hazmat',
  name: 'Hazmat awareness',
  description: 'Required hazmat training',
  qualificationKey: 'hazmat',
  qualificationName: 'Hazmat',
  status: 'active',
  createdAt: '2026-01-01T00:00:00Z',
}

describe('StepBuilderPanel', () => {
  it('creates a step when form is submitted', () => {
    const onCreateStep = vi.fn().mockResolvedValue(undefined)

    render(
      <StepBuilderPanel
        definitions={[definition]}
        selectedDefinitionId="def-1"
        steps={[]}
        isLoading={false}
        canManage
        isSubmitting={false}
        onSelectDefinition={vi.fn()}
        onCreateStep={onCreateStep}
        onDeleteStep={vi.fn()}
      />,
    )

    fireEvent.change(screen.getByLabelText(/^Name/i), { target: { value: 'intro' } })
    fireEvent.change(screen.getByLabelText(/Description/i), { target: { value: 'Read the safety overview.' } })
    fireEvent.click(screen.getByRole('button', { name: /Add step/i }))

    expect(onCreateStep).toHaveBeenCalledWith(
      expect.objectContaining({
        stepKey: 'intro',
        name: 'intro',
        stepType: 'content',
      }),
    )
  })
})
