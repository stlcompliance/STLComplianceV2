import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { CompletionRuleBuilderPanel } from './CompletionRuleBuilderPanel'

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

const catalog = [
  {
    ruleType: 'required_evaluator_pass',
    label: 'Passing evaluation',
    description: 'Evaluator pass required.',
    defaultConfigJson: '{}',
  },
  {
    ruleType: 'required_signoff',
    label: 'Required signoff',
    description: 'Signoff role required.',
    defaultConfigJson: '{"signoffRole":"trainer"}',
  },
]

describe('CompletionRuleBuilderPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('creates a completion rule when form is submitted', () => {
    const onCreateRule = vi.fn().mockResolvedValue(undefined)

    render(
      <CompletionRuleBuilderPanel
        definitions={[definition]}
        selectedDefinitionId="def-1"
        catalog={catalog}
        rules={[]}
        isLoading={false}
        canManage
        isSubmitting={false}
        onSelectDefinition={vi.fn()}
        onCreateRule={onCreateRule}
        onDeleteRule={vi.fn()}
      />,
    )

    fireEvent.change(screen.getByLabelText(/^Label/i), { target: { value: 'Evaluator pass only' } })
    fireEvent.click(screen.getByRole('button', { name: /Add completion rule/i }))

    expect(onCreateRule).toHaveBeenCalledWith(
      expect.objectContaining({
        ruleKey: 'evaluator-pass-only',
        ruleType: 'required_evaluator_pass',
        label: 'Evaluator pass only',
      }),
    )
  })

  it('lists configured rules', () => {
    render(
      <CompletionRuleBuilderPanel
        definitions={[definition]}
        selectedDefinitionId="def-1"
        catalog={catalog}
        rules={[
          {
            completionRuleId: 'rule-1',
            trainingDefinitionId: 'def-1',
            ruleKey: 'evaluator_pass',
            ruleType: 'required_evaluator_pass',
            label: 'Passing evaluation',
            configJson: '{}',
            sortOrder: 0,
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
          },
        ]}
        isLoading={false}
        canManage={false}
        isSubmitting={false}
        onSelectDefinition={vi.fn()}
        onCreateRule={vi.fn()}
        onDeleteRule={vi.fn()}
      />,
    )

    expect(screen.getByText('evaluator_pass')).toBeInTheDocument()
    expect(screen.getByText('required_evaluator_pass')).toBeInTheDocument()
  })
})
