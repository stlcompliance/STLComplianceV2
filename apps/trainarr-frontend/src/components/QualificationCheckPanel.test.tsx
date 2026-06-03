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
      placeholder,
      testId,
    }: {
      label?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
      placeholder?: string
      testId?: string
    }) => (
      <label>
        {label ? <span>{label}</span> : null}
        <select
          aria-label={label ?? placeholder ?? 'Static search picker'}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{placeholder ?? 'Select…'}</option>
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

import { QualificationCheckPanel } from './QualificationCheckPanel'

describe('QualificationCheckPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders empty state without a prior check', () => {
    render(
      <QualificationCheckPanel
        check={null}
        isChecking={false}
        onRunCheck={vi.fn()}
        canRun
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
      />,
    )

    expect(screen.getByText(/authorization check/i)).toBeInTheDocument()
    expect(screen.getByLabelText('Compliance Core rule pack key')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /run qualification check/i })).toBeEnabled()
  })

  it('shows allow outcome details', () => {
    render(
      <QualificationCheckPanel
        check={{
          checkId: 'check-1',
          staffarrPersonId: 'person-1',
          qualificationKey: 'hazmat',
          outcome: 'allow',
          reasonCode: 'rule_evaluation_passed',
          message: 'Authorization check passed.',
          localQualification: {
            qualificationIssueId: 'issue-1',
            status: 'issued',
            message: 'Active qualification issued.',
          },
          complianceCore: {
            rulePackKey: 'driver_qualification',
            outcome: 'allow',
            reasonCode: 'rule_evaluation_passed',
            message: 'All rule checks passed.',
            evaluationResult: 'pass',
            unresolvedFactKeys: [],
          },
        }}
        isChecking={false}
        onRunCheck={vi.fn()}
        canRun
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
      />,
    )

    expect(screen.getByText('allow')).toBeInTheDocument()
    expect(screen.getByText(/authorization check passed/i)).toBeInTheDocument()
  })

  it('invokes run check handler', () => {
    const onRunCheck = vi.fn()

    render(
      <QualificationCheckPanel
        check={null}
        isChecking={false}
        onRunCheck={onRunCheck}
        canRun
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: /run qualification check/i }))
    expect(onRunCheck).toHaveBeenCalledOnce()
  })
})
