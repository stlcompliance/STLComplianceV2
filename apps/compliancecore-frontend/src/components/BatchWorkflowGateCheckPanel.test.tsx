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

    expect(screen.getByText(/Batch workflow gate check/i)).toBeInTheDocument()
    const runButton = screen.getByRole('button', { name: /Run batch check \(0 gates\)/i })
    expect(runButton).toBeDisabled()

    fireEvent.click(screen.getByLabelText(/Driver assignment/i))
    expect(screen.getByRole('button', { name: /Run batch check \(1 gate\)/i })).not.toBeDisabled()
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

    fireEvent.click(screen.getByLabelText(/Driver assignment/i))
    fireEvent.click(screen.getByLabelText(/Driver clearance/i))
    const licenseCheckbox = screen.getByLabelText('driver_license_valid')
    fireEvent.click(licenseCheckbox)
    fireEvent.click(screen.getByRole('button', { name: /Run batch check \(2 gates\)/i }))

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

    expect(screen.getByText(/1 allow/i)).toBeInTheDocument()
    expect(screen.getByText(/All rules passed/i)).toBeInTheDocument()
  })
})
