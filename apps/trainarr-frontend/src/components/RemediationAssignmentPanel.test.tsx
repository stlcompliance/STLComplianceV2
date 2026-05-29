import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { RemediationAssignmentPanel } from './RemediationAssignmentPanel'

describe('RemediationAssignmentPanel', () => {
  it('hides panel when user cannot manage assignments', () => {
    const { container } = render(
      <RemediationAssignmentPanel
        remediations={[]}
        definitions={[]}
        selectedRemediationId={null}
        selectedDefinitionId=""
        onSelectRemediation={vi.fn()}
        onSelectDefinition={vi.fn()}
        onCreateAssignment={vi.fn()}
        isCreating={false}
        canManage={false}
        qualificationCheck={null}
        isCheckingQualification={false}
        onRunQualificationCheck={vi.fn()}
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
      />,
    )
    expect(container).toBeEmptyDOMElement()
  })

  it('shows pending remediations workflow', () => {
    render(
      <RemediationAssignmentPanel
        remediations={[
          {
            remediationId: 'r1',
            tenantId: 't1',
            staffarrIncidentId: 'i1',
            staffarrPersonId: '11111111-1111-1111-1111-111111111111',
            reasonCategoryKey: 'training_compliance',
            status: 'intake_received',
            createdAt: '2026-05-27T12:00:00Z',
          },
        ]}
        definitions={[
          {
            trainingDefinitionId: 'd1',
            definitionKey: 'annual_compliance',
            name: 'Annual compliance refresher',
            description: 'Required annual training',
            qualificationKey: 'annual_compliance',
            qualificationName: 'Annual Compliance',
            status: 'active',
            createdAt: '2026-05-27T12:00:00Z',
          },
        ]}
        selectedRemediationId={null}
        selectedDefinitionId=""
        onSelectRemediation={vi.fn()}
        onSelectDefinition={vi.fn()}
        onCreateAssignment={vi.fn()}
        isCreating={false}
        canManage
        qualificationCheck={null}
        isCheckingQualification={false}
        onRunQualificationCheck={vi.fn()}
        rulePackKey="driver_qualification"
        onRulePackKeyChange={vi.fn()}
        rulePackOptions={[{ value: 'driver_qualification', label: 'driver_qualification' }]}
      />,
    )
    expect(screen.getByText(/remediation → assignment/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /create assignment from remediation/i })).toBeDisabled()
  })
})
