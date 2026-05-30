import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { StepBranchBuilderPanel } from './StepBranchBuilderPanel'

const definition = {
  trainingDefinitionId: 'def-1',
  definitionKey: 'pit_operator',
  name: 'PIT operator',
  description: 'Forklift operator qualification',
  qualificationKey: 'pit_operator',
  qualificationName: 'PIT Operator',
  status: 'active',
  createdAt: '2026-01-01T00:00:00Z',
}

const steps = [
  {
    stepId: 'step-quiz',
    trainingDefinitionId: 'def-1',
    stepKey: 'safety-quiz',
    name: 'Safety quiz',
    description: 'Quiz step',
    stepType: 'quiz' as const,
    configJson: '{}',
    sortOrder: 0,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
  },
]

const catalog = [
  {
    branchType: 'quiz_failed_remediation',
    label: 'Quiz failed remediation',
    description: 'Unlock remediation step on quiz fail.',
    defaultConfigJson: '{"targetStepKey":"remediation-review"}',
  },
  {
    branchType: 'step_visibility',
    label: 'Conditional visibility',
    description: 'Show when dependency step reaches status.',
    defaultConfigJson: '{"dependsOnStepKey":"intro","requiredStatus":"completed"}',
  },
]

describe('StepBranchBuilderPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('creates a branch rule when form is submitted', () => {
    const onCreateBranch = vi.fn().mockResolvedValue(undefined)

    render(
      <StepBranchBuilderPanel
        definitions={[definition]}
        selectedDefinitionId="def-1"
        steps={steps}
        selectedStepId="step-quiz"
        catalog={catalog}
        branches={[]}
        isLoading={false}
        canManage
        isSubmitting={false}
        onSelectDefinition={vi.fn()}
        onSelectStep={vi.fn()}
        onCreateBranch={onCreateBranch}
        onDeleteBranch={vi.fn()}
      />,
    )

    fireEvent.change(screen.getByLabelText(/^Label/i), { target: { value: 'Unlock remediation' } })
    fireEvent.click(screen.getByRole('button', { name: /Add branch rule/i }))

    expect(onCreateBranch).toHaveBeenCalledWith(
      expect.objectContaining({
        branchKey: 'train.branch.unlockremediation',
        branchType: 'quiz_failed_remediation',
        label: 'Unlock remediation',
      }),
    )
  })

  it('lists configured branch rules', () => {
    render(
      <StepBranchBuilderPanel
        definitions={[definition]}
        selectedDefinitionId="def-1"
        steps={steps}
        selectedStepId="step-quiz"
        catalog={catalog}
        branches={[
          {
            branchId: 'branch-1',
            trainingDefinitionStepId: 'step-quiz',
            branchKey: 'quiz_fail',
            branchType: 'quiz_failed_remediation',
            label: 'Quiz fail remediation',
            configJson: '{"targetStepKey":"remediation-review"}',
            sortOrder: 0,
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
          },
        ]}
        isLoading={false}
        canManage={false}
        isSubmitting={false}
        onSelectDefinition={vi.fn()}
        onSelectStep={vi.fn()}
        onCreateBranch={vi.fn()}
        onDeleteBranch={vi.fn()}
      />,
    )

    expect(screen.getByText('Quiz fail remediation')).toBeInTheDocument()
    expect(screen.getByText('quiz_failed_remediation')).toBeInTheDocument()
  })
})
