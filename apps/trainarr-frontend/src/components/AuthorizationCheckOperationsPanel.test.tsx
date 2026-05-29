import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AuthorizationCheckOperationsPanel } from './AuthorizationCheckOperationsPanel'

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
        onRunCheck={vi.fn()}
      />,
    )

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
        onRunCheck={onRunCheck}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: /run qualification check/i }))
    expect(onRunCheck).toHaveBeenCalledOnce()
  })
})
