import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AuthorizationCheckOperationsPanel } from './AuthorizationCheckOperationsPanel'

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

describe('AuthorizationCheckOperationsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  const definitions = [
    {
      trainingDefinitionId: 'def-1',
      definitionKey: 'hazmat',
      name: 'Hazmat Endorsement',
      description: '',
      qualificationKey: 'hazmat_endorsement',
      qualificationName: 'Hazmat',
      status: 'active',
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
  ]

  it('renders operations panel and recent history', () => {
    render(
      <AuthorizationCheckOperationsPanel
        definitions={definitions}
        history={[
          {
            checkId: 'check-1',
            staffarrPersonId: 'person-1',
            qualificationKey: 'hazmat_endorsement',
            outcome: 'warn',
            reasonCode: 'local_no_qualification',
            message: 'Authorization check returned warnings.',
            rulePackKey: 'driver_qualification',
            trainingDefinitionId: 'def-1',
            batchId: null,
            checkedAt: '2026-05-28T12:00:00Z',
          },
        ]}
        isLoadingHistory={false}
        check={null}
        isChecking={false}
        canRun
        staffarrPersonId="person-1"
        onStaffarrPersonIdChange={vi.fn()}
        selectedDefinitionId="def-1"
        onSelectDefinition={vi.fn()}
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
        personPickerOptions={[{ value: 'person-1', label: 'Person 1' }]}
        onRunCheck={vi.fn()}
      />,
    )

    expect(screen.getByLabelText(/StaffArr person/i)).toBeInTheDocument()
    expect(screen.getByText(/authorization check operations/i)).toBeInTheDocument()
    expect(screen.getByText('warn')).toBeInTheDocument()
  })

  it('invokes run check handler', () => {
    const onRunCheck = vi.fn()

    render(
      <AuthorizationCheckOperationsPanel
        definitions={definitions}
        history={[]}
        isLoadingHistory={false}
        check={null}
        isChecking={false}
        canRun
        staffarrPersonId="person-1"
        onStaffarrPersonIdChange={vi.fn()}
        selectedDefinitionId="def-1"
        onSelectDefinition={vi.fn()}
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
        personPickerOptions={[{ value: 'person-1', label: 'Person 1' }]}
        onRunCheck={onRunCheck}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: /run qualification check/i }))
    expect(onRunCheck).toHaveBeenCalledOnce()
  })

  it('uses a searchable picker for training definitions', () => {
    const onSelectDefinition = vi.fn()

    render(
      <AuthorizationCheckOperationsPanel
        definitions={definitions}
        history={[]}
        isLoadingHistory={false}
        check={null}
        isChecking={false}
        canRun
        staffarrPersonId="person-1"
        onStaffarrPersonIdChange={vi.fn()}
        selectedDefinitionId=""
        onSelectDefinition={onSelectDefinition}
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
        personPickerOptions={[{ value: 'person-1', label: 'Person 1' }]}
        onRunCheck={vi.fn()}
      />,
    )

    const definitionPicker = screen.getByText('Training definition').closest('label')?.querySelector('input')
    expect(definitionPicker).toBeTruthy()
    fireEvent.change(definitionPicker as HTMLInputElement, { target: { value: 'def-1' } })
    expect(onSelectDefinition).toHaveBeenCalledWith('def-1')
  })
})
