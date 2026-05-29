import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach } from 'vitest'
import { describe, expect, it, vi } from 'vitest'

import { BatchWorkflowGateCheckPanel } from './BatchWorkflowGateCheckPanel'

const sampleGates = [
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
  {
    workflowGateId: 'gate-2',
    gateKey: 'driver_clearance',
    label: 'Driver clearance',
    description: 'Gate before clearance',
    rulePackId: 'pack-1',
    packKey: 'driver_qualification',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
  },
]

describe('BatchWorkflowGateCheckPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders gate selection and disables batch until gates selected', () => {
    const onRunBatch = vi.fn()
    render(
      <BatchWorkflowGateCheckPanel
        workflowGates={sampleGates}
        factKeys={['driver_license_valid']}
        batch={null}
        isChecking={false}
        onRunBatch={onRunBatch}
      />,
    )

    expect(screen.getByTestId('batch-workflow-gate-check-panel')).toBeInTheDocument()
    const runButton = screen.getByTestId('batch-workflow-gate-run')
    expect(runButton).toBeDisabled()
    expect(runButton).toHaveTextContent(/0 gates/i)

    fireEvent.click(screen.getByTestId('batch-workflow-gate-gate-driver_assignment'))
    expect(screen.getByTestId('batch-workflow-gate-run')).not.toBeDisabled()
    expect(screen.getByTestId('batch-workflow-gate-run')).toHaveTextContent(/1 gate/i)
  })

  it('invokes onRunBatch with selected gates and facts', () => {
    const onRunBatch = vi.fn()
    render(
      <BatchWorkflowGateCheckPanel
        workflowGates={sampleGates}
        factKeys={['driver_license_valid', 'medical_cert_on_file']}
        batch={null}
        isChecking={false}
        onRunBatch={onRunBatch}
      />,
    )

    fireEvent.click(screen.getByTestId('batch-workflow-gate-gate-driver_assignment'))
    fireEvent.click(screen.getByTestId('batch-workflow-gate-gate-driver_clearance'))
    const licenseCheckbox = screen.getByLabelText('driver_license_valid')
    fireEvent.click(licenseCheckbox)
    fireEvent.click(screen.getByTestId('batch-workflow-gate-run'))

    expect(onRunBatch).toHaveBeenCalledWith(
      ['driver_assignment', 'driver_clearance'],
      { driver_license_valid: true },
      false,
    )
  })

  it('shows batch summary when results are provided', () => {
    render(
      <BatchWorkflowGateCheckPanel
        workflowGates={sampleGates}
        factKeys={[]}
        batch={{
          batchId: 'batch-1',
          results: [
            {
              checkResultId: 'check-1',
              gateKey: 'driver_assignment',
              gateLabel: 'Driver assignment',
              rulePackId: 'pack-1',
              packKey: 'driver_qualification',
              outcome: 'allow',
              reasonCode: 'rules_passed',
              message: 'All rules passed.',
              ruleEvaluationRunId: 'run-1',
              reasons: [],
              findingsEmitted: [],
              checkedAt: '2026-01-01T00:00:00Z',
            },
          ],
          summary: { total: 1, allowCount: 1, warnCount: 0, blockCount: 0 },
        }}
        isChecking={false}
        onRunBatch={vi.fn()}
      />,
    )

    const result = screen.getByTestId('batch-workflow-gate-latest-result')
    expect(result).toHaveAttribute('data-allow-count', '1')
    expect(result).toHaveAttribute('data-block-count', '0')
    expect(screen.getByText(/1 allow/i)).toBeInTheDocument()
    expect(screen.getByText(/All rules passed/i)).toBeInTheDocument()
  })
})
