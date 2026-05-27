import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { AssignmentsPanel } from './AssignmentsPanel'
import type { TrainingAssignmentSummaryResponse } from '../api/types'

const sampleAssignment: TrainingAssignmentSummaryResponse = {
  assignmentId: 'a1',
  staffarrPersonId: '11111111-1111-1111-1111-111111111111',
  trainingDefinitionId: 'd1',
  trainingDefinitionName: 'Annual compliance refresher',
  qualificationKey: 'annual_compliance',
  staffarrIncidentRemediationId: 'r1',
  assignmentReason: 'incident_remediation',
  status: 'assigned',
  dueAt: null,
  createdAt: '2026-05-27T12:00:00Z',
}

describe('AssignmentsPanel', () => {
  it('renders empty state when no assignments', () => {
    render(
      <AssignmentsPanel
        assignments={[]}
        selectedAssignmentId={null}
        onSelectAssignment={vi.fn()}
        canManage={false}
      />,
    )
    expect(screen.getByText(/no training assignments yet/i)).toBeInTheDocument()
  })

  it('renders assignment summary and complete action', () => {
    render(
      <AssignmentsPanel
        assignments={[sampleAssignment]}
        selectedAssignmentId="a1"
        onSelectAssignment={vi.fn()}
        canManage
        onComplete={vi.fn()}
      />,
    )
    expect(screen.getByText('Annual compliance refresher')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /mark complete/i })).toBeInTheDocument()
    expect(screen.getByText(/linked remediation/i)).toBeInTheDocument()
  })
})
