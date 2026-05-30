import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { BatchQualificationCheckPanel } from './BatchQualificationCheckPanel'

const rulePackOptions = [{ value: 'driver_qualification', label: 'driver_qualification' }]
const qualificationOptions = [{ value: 'hazmat_endorsement', label: 'Hazmat endorsement (hazmat_endorsement)' }]

describe('BatchQualificationCheckPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders supervisor batch form controls', () => {
    render(
      <BatchQualificationCheckPanel
        batch={null}
        isChecking={false}
        onRunBatch={vi.fn()}
        canRun
        qualificationKey="hazmat_endorsement"
        onQualificationKeyChange={vi.fn()}
        qualificationOptions={qualificationOptions}
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={rulePackOptions}
        selectedPersonIds={['person-1']}
        onSelectedPersonIdsChange={vi.fn()}
        personPickerOptions={[{ value: 'person-1', label: 'Person 1' }]}
        selectedRemediationPersonIds={[]}
        onToggleRemediationPerson={vi.fn()}
        remediationPersonOptions={[]}
      />,
    )

    expect(screen.getByRole('button', { name: /run batch qualification check/i })).toBeEnabled()
    expect(screen.getByTestId('batch-qualification-key')).toBeInTheDocument()
    expect(screen.getByTestId('batch-qualification-rule-pack')).toBeInTheDocument()
  })

  it('shows batch summary and per-person outcomes', () => {
    render(
      <BatchQualificationCheckPanel
        batch={{
          batchId: 'batch-1',
          qualificationKey: 'hazmat_endorsement',
          summary: { total: 2, allowCount: 1, warnCount: 1, blockCount: 0 },
          results: [
            {
              checkId: 'c1',
              staffarrPersonId: 'person-a',
              qualificationKey: 'hazmat_endorsement',
              outcome: 'allow',
              reasonCode: 'local_issued',
              message: 'ok',
              localQualification: null,
              complianceCore: null,
            },
            {
              checkId: 'c2',
              staffarrPersonId: 'person-b',
              qualificationKey: 'hazmat_endorsement',
              outcome: 'warn',
              reasonCode: 'local_no_qualification',
              message: 'warn',
              localQualification: null,
              complianceCore: null,
            },
          ],
        }}
        isChecking={false}
        onRunBatch={vi.fn()}
        canRun
        qualificationKey="hazmat_endorsement"
        onQualificationKeyChange={vi.fn()}
        qualificationOptions={qualificationOptions}
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={rulePackOptions}
        selectedPersonIds={[]}
        onSelectedPersonIdsChange={vi.fn()}
        personPickerOptions={[]}
        selectedRemediationPersonIds={[]}
        onToggleRemediationPerson={vi.fn()}
        remediationPersonOptions={[]}
      />,
    )

    expect(screen.getByText(/1 allow/i)).toBeInTheDocument()
    expect(screen.getByText('person-a')).toBeInTheDocument()
    expect(screen.getByText('person-b')).toBeInTheDocument()
  })

  it('invokes onRunBatch when the button is clicked', () => {
    const onRunBatch = vi.fn()
    render(
      <BatchQualificationCheckPanel
        batch={null}
        isChecking={false}
        onRunBatch={onRunBatch}
        canRun
        qualificationKey="hazmat_endorsement"
        onQualificationKeyChange={vi.fn()}
        qualificationOptions={qualificationOptions}
        rulePackKey=""
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={rulePackOptions}
        selectedPersonIds={['person-1']}
        onSelectedPersonIdsChange={vi.fn()}
        personPickerOptions={[{ value: 'person-1', label: 'Person 1' }]}
        selectedRemediationPersonIds={[]}
        onToggleRemediationPerson={vi.fn()}
        remediationPersonOptions={[]}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: /run batch qualification check/i }))
    expect(onRunBatch).toHaveBeenCalledTimes(1)
  })
})
