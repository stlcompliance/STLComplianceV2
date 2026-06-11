import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach } from 'vitest'
import { describe, expect, it, vi } from 'vitest'

import { FindingsWorkflowGatesPanel } from './FindingsWorkflowGatesPanel'

describe('FindingsWorkflowGatesPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders empty findings state and gate list', () => {
    render(
      <FindingsWorkflowGatesPanel
        factDefinitions={[
          {
            factDefinitionId: 'fact-1',
            factKey: 'driver_license_valid',
            label: 'Valid license',
            description: '',
            valueType: 'boolean',
            isActive: true,
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
          },
        ]}
        rulePackContent={{
          schemaVersion: 1,
          logic: 'all',
          rules: [
            {
              ruleKey: 'license_valid',
              label: 'Valid license',
              type: 'fact_boolean',
              factKey: 'driver_license_valid',
              expectedValue: true,
            },
          ],
        }}
        findings={[]}
        workflowGates={[
          {
            workflowGateId: 'gate-1',
            gateKey: 'driver_assignment',
            label: 'Driver assignment',
            description: 'Gate before assignment',
            rulePackId: 'pack-1',
            packKey: 'driver_qualification',
            isActive: true,
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
          },
        ]}
        onCheckGate={vi.fn()}
        isCheckingGate={false}
        lastGateCheck={null}
        onCheckGateBatch={vi.fn()}
        isCheckingGateBatch={false}
        lastGateBatch={null}
      />,
    )

    expect(screen.getByTestId('findings-workflow-gates-panel')).toBeTruthy()
    expect(screen.queryByTestId('findings-workflow-gate-seed')).not.toBeInTheDocument()
    expect(screen.getByTestId('findings-workflow-gate-select')).toBeTruthy()
    expect(screen.getByTestId('findings-workflow-gate-emit-findings')).toBeTruthy()
    expect(screen.getByTestId('findings-workflow-gate-findings-section')).toBeTruthy()
    expect(screen.getByText(/No findings yet/i)).toBeInTheDocument()
    expect(screen.getByText(/Workflow gates \(1\)/i)).toBeInTheDocument()
  })

  it('shows findings and last gate check outcome', () => {
    const onCheckGate = vi.fn()

    render(
      <FindingsWorkflowGatesPanel
        factDefinitions={[]}
        rulePackContent={null}
        findings={[
          {
            findingId: 'finding-1',
            rulePackId: 'pack-1',
            packKey: 'driver_qualification',
            ruleEvaluationRunId: 'run-1',
            findingKey: 'dq_license_fail',
            severity: 'block',
            status: 'open',
            ruleKey: 'license_valid',
            factKey: null,
            title: 'Valid license',
            message: 'License failed',
            reasonCode: 'rule_failed',
            createdAt: '2026-01-01T00:00:00Z',
          },
        ]}
        workflowGates={[
          {
            workflowGateId: 'gate-1',
            gateKey: 'driver_assignment',
            label: 'Driver assignment',
            description: 'Gate',
            rulePackId: 'pack-1',
            packKey: 'driver_qualification',
            isActive: true,
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
          },
        ]}
        onCheckGate={onCheckGate}
        isCheckingGate={false}
        lastGateCheck={{
          checkResultId: 'check-1',
          gateKey: 'driver_assignment',
          gateLabel: 'Driver assignment',
          rulePackId: 'pack-1',
          packKey: 'driver_qualification',
          outcome: 'block',
          reasonCode: 'rule_evaluation_failed',
          message: 'Failed',
          ruleEvaluationRunId: 'run-1',
          reasons: [{ code: 'rule_failed', message: 'License invalid', ruleKey: 'license_valid', factKey: null }],
          findingsEmitted: [],
          checkedAt: '2026-01-01T00:00:00Z',
        }}
        onCheckGateBatch={vi.fn()}
        isCheckingGateBatch={false}
        lastGateBatch={null}
      />,
    )

    expect(screen.getByText(/License failed/i)).toBeInTheDocument()
    expect(screen.getByText(/Findings \(1\)/i)).toBeInTheDocument()

    fireEvent.change(screen.getByTestId('findings-workflow-gate-select'), {
      target: { value: 'driver_assignment' },
    })
    fireEvent.click(screen.getByTestId('findings-workflow-gate-check'))
    expect(onCheckGate).toHaveBeenCalledWith('driver_assignment', {}, true)
  })
})
