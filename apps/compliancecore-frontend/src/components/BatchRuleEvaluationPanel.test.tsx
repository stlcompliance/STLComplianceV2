import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach } from 'vitest'
import { describe, expect, it, vi } from 'vitest'

import { BatchRuleEvaluationPanel } from './BatchRuleEvaluationPanel'

const samplePacks = [
  {
    rulePackId: 'pack-1',
    regulatoryProgramId: 'prog-1',
    regulatoryProgramKey: 'driver_compliance',
    regulatoryProgramLabel: 'Driver Compliance',
    packKey: 'driver_qualification',
    label: 'Driver Qualification',
    description: 'Driver rules',
    versionNumber: 1,
    status: 'draft',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
  },
  {
    rulePackId: 'pack-2',
    regulatoryProgramId: 'prog-1',
    regulatoryProgramKey: 'driver_compliance',
    regulatoryProgramLabel: 'Driver Compliance',
    packKey: 'vehicle_inspection',
    label: 'Vehicle Inspection',
    description: 'Vehicle rules',
    versionNumber: 1,
    status: 'draft',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
  },
]

describe('BatchRuleEvaluationPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders pack selection and disables batch until packs selected', () => {
    const onRunBatch = vi.fn()
    render(
      <BatchRuleEvaluationPanel
        rulePacks={samplePacks}
        factKeys={['driver_license_valid']}
        batch={null}
        isEvaluating={false}
        onRunBatch={onRunBatch}
      />,
    )

    expect(screen.getByText(/Batch rule evaluation/i)).toBeInTheDocument()
    expect(screen.getByTestId('batch-rule-evaluation-panel')).toBeTruthy()
    const runButton = screen.getByTestId('batch-rule-evaluation-run')
    expect(runButton).toBeDisabled()

    fireEvent.click(screen.getByLabelText(/Driver Qualification/i))
    expect(screen.getByTestId('batch-rule-evaluation-run')).not.toBeDisabled()
  })

  it('invokes onRunBatch with selected packs and facts', () => {
    const onRunBatch = vi.fn()
    render(
      <BatchRuleEvaluationPanel
        rulePacks={samplePacks}
        factKeys={['driver_license_valid', 'medical_cert_on_file']}
        batch={null}
        isEvaluating={false}
        onRunBatch={onRunBatch}
      />,
    )

    fireEvent.click(screen.getByLabelText(/Driver Qualification/i))
    fireEvent.click(screen.getByLabelText(/Vehicle Inspection/i))
    fireEvent.click(screen.getByLabelText('driver_license_valid'))
    fireEvent.click(screen.getByTestId('batch-rule-evaluation-run'))

    expect(onRunBatch).toHaveBeenCalledWith(
      ['driver_qualification', 'vehicle_inspection'],
      { driver_license_valid: true },
      false,
    )
  })

  it('shows batch summary when results are provided', () => {
    render(
      <BatchRuleEvaluationPanel
        rulePacks={samplePacks}
        factKeys={[]}
        batch={{
          batchId: 'batch-1',
          results: [
            {
              rulePackKey: 'driver_qualification',
              rulePackId: 'pack-1',
              packLabel: 'Driver Qualification',
              outcome: 'allow',
              reasonCode: 'rule_evaluation_passed',
              message: 'All rule checks passed for the supplied facts.',
              overallResult: 'pass',
              evaluationRunId: 'run-1',
              ruleResults: [],
              findingsEmitted: [],
            },
          ],
          summary: { total: 1, allowCount: 1, warnCount: 0, blockCount: 0 },
        }}
        isEvaluating={false}
        onRunBatch={vi.fn()}
      />,
    )

    expect(screen.getByTestId('batch-rule-evaluation-latest-result')).toBeTruthy()
    expect(screen.getByText(/1 allow/i)).toBeInTheDocument()
    expect(screen.getByText(/All rule checks passed/i)).toBeInTheDocument()
  })
})
