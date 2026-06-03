import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      options,
      onChange,
      testId,
    }: {
      label?: string
      value: string
      options: { value: string; label: string }[]
      onChange: (value: string) => void
      testId?: string
    }) => (
      <label htmlFor={testId ?? 'mock-step-picker'}>
        {label ? <span>{label}</span> : null}
        <select
          id={testId ?? 'mock-step-picker'}
          aria-label={label ?? 'Static search picker'}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">Select…</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    ),
  }
})
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
  afterEach(() => {
    cleanup()
  })

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
        stepKey: 'train.step.intro',
        name: 'intro',
        stepType: 'content',
      }),
    )
  })

  it('uses a searchable picker for training definitions', () => {
    const onSelectDefinition = vi.fn()

    render(
      <StepBuilderPanel
        definitions={[definition]}
        selectedDefinitionId=""
        steps={[]}
        isLoading={false}
        canManage
        isSubmitting={false}
        onSelectDefinition={onSelectDefinition}
        onCreateStep={vi.fn()}
        onDeleteStep={vi.fn()}
      />,
    )

    const definitionPicker = screen.getByLabelText('Training definition')
    fireEvent.change(definitionPicker, { target: { value: 'def-1' } })

    expect(onSelectDefinition).toHaveBeenCalledWith('def-1')
  })
})
