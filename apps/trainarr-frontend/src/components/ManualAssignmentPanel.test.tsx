import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ManualAssignmentPanel } from './ManualAssignmentPanel'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const mod = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...mod,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      testId,
    }: {
      label?: string
      value: string
      onChange: (v: string) => void
      options: { value: string; label: string }[]
      testId?: string
    }) => (
      <label htmlFor={testId ?? 'mock-static-search-picker'}>
        {label}
        <input
          id={testId ?? 'mock-static-search-picker'}
          aria-label={label}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <ul>
          {options.map((option) => (
            <li key={option.value}>{option.label}</li>
          ))}
        </ul>
      </label>
    ),
  }
})

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const mod = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...mod,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
    }: {
      label?: string
      value: string
      onChange: (v: string) => void
    }) => (
      <label htmlFor="mock-static-search-picker">
        {label}
        <input
          id="mock-static-search-picker"
          aria-label={label}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
      </label>
    ),
  }
})

describe('ManualAssignmentPanel', () => {
  afterEach(() => {
    cleanup()
  })

  const definitions = [
    {
      trainingDefinitionId: 'def-1',
      definitionKey: 'annual',
      name: 'Annual Compliance',
      description: '',
      qualificationKey: 'annual_compliance',
      qualificationName: 'Annual Compliance',
      status: 'active',
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
  ]

  it('requires authorization check before create', () => {
    render(
      <ManualAssignmentPanel
        definitions={definitions}
        staffarrPersonId="person-1"
        onStaffarrPersonIdChange={vi.fn()}
        selectedDefinitionId="def-1"
        onSelectDefinition={vi.fn()}
        qualificationCheck={null}
        isCheckingQualification={false}
        onRunQualificationCheck={vi.fn()}
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
        personPickerOptions={[{ value: 'person-1', label: 'Person 1' }]}
        onCreateAssignment={vi.fn()}
        isCreating={false}
        canManage
      />,
    )

    expect(screen.getByLabelText(/^Person$/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /create manual assignment/i })).toBeDisabled()
    expect(screen.getByText(/run an authorization check before creating/i)).toBeInTheDocument()
  })

  it('enables create when check outcome is allow', () => {
    const onCreateAssignment = vi.fn()

    render(
      <ManualAssignmentPanel
        definitions={definitions}
        staffarrPersonId="person-1"
        onStaffarrPersonIdChange={vi.fn()}
        selectedDefinitionId="def-1"
        onSelectDefinition={vi.fn()}
        qualificationCheck={{
          checkId: 'check-1',
          staffarrPersonId: 'person-1',
          qualificationKey: 'annual_compliance',
          outcome: 'allow',
          reasonCode: 'local_issued',
          message: 'Authorization check passed.',
          localQualification: null,
          complianceCore: null,
        }}
        isCheckingQualification={false}
        onRunQualificationCheck={vi.fn()}
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
        personPickerOptions={[{ value: 'person-1', label: 'Person 1' }]}
        onCreateAssignment={onCreateAssignment}
        isCreating={false}
        canManage
      />,
    )

    const button = screen.getByRole('button', { name: /create manual assignment/i })
    expect(button).toBeEnabled()
    fireEvent.click(button)
    expect(onCreateAssignment).toHaveBeenCalledOnce()
  })

  it('uses a searchable picker for training definitions', () => {
    const onSelectDefinition = vi.fn()

    render(
      <ManualAssignmentPanel
        definitions={definitions}
        staffarrPersonId="person-1"
        onStaffarrPersonIdChange={vi.fn()}
        selectedDefinitionId=""
        onSelectDefinition={onSelectDefinition}
        qualificationCheck={null}
        isCheckingQualification={false}
        onRunQualificationCheck={vi.fn()}
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
        personPickerOptions={[{ value: 'person-1', label: 'Person 1' }]}
        onCreateAssignment={vi.fn()}
        isCreating={false}
        canManage
      />,
    )

    const definitionPicker = screen.getByText('Training definition').closest('label')?.querySelector('input')
    expect(definitionPicker).toBeTruthy()
    fireEvent.change(definitionPicker as HTMLInputElement, { target: { value: 'def-1' } })
    expect(onSelectDefinition).toHaveBeenCalledWith('def-1')
  })
})
