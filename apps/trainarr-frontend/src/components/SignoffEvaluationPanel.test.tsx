import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { SignoffEvaluationPanel } from './SignoffEvaluationPanel'
import type { TrainingAssignmentDetailResponse } from '../api/types'

const assignment: TrainingAssignmentDetailResponse = {
  assignmentId: 'a1',
  staffarrPersonId: '11111111-1111-1111-1111-111111111111',
  trainingDefinitionId: 'd1',
  trainingDefinitionName: 'Forklift practical',
  trainingDefinitionKey: 'forklift_practical',
  qualificationKey: 'forklift_ops',
  qualificationName: 'Forklift Operations',
  staffarrIncidentRemediationId: null,
  sourceQualificationIssueId: null,
  assignmentReason: 'manual',
  status: 'in_progress',
  dueAt: null,
  assignedByUserId: null,
  blockerPublicationId: null,
  completedAt: null,
  completedByUserId: null,
  createdAt: '2026-05-27T12:00:00Z',
  updatedAt: '2026-05-27T12:00:00Z',
  evidenceCount: 0,
  evaluation: null,
  signoffs: [],
  completionRequirementsMet: false,
  qualificationIssue: null,
}

const baseProps = {
  evaluationResult: 'pass',
  evaluationScore: '95',
  evaluationNotes: '',
  signoffNotes: '',
  onEvaluationResultChange: vi.fn(),
  onEvaluationScoreChange: vi.fn(),
  onEvaluationNotesChange: vi.fn(),
  onSignoffNotesChange: vi.fn(),
  onSubmitEvaluation: vi.fn(),
  onSubmitTraineeSignoff: vi.fn(),
  onSubmitTrainerSignoff: vi.fn(),
  isSubmittingEvaluation: false,
  isSubmittingTraineeSignoff: false,
  isSubmittingTrainerSignoff: false,
  canSubmitEvaluation: true,
  canSubmitTraineeSignoff: false,
  canSubmitTrainerSignoff: true,
}

describe('SignoffEvaluationPanel', () => {
  it('renders empty selection state', () => {
    render(<SignoffEvaluationPanel assignment={null} {...baseProps} />)
    expect(screen.getByText(/select an assignment to record evaluation/i)).toBeInTheDocument()
  })

  it('renders evaluation form and trainer signoff when permitted', () => {
    render(<SignoffEvaluationPanel assignment={assignment} {...baseProps} />)
    expect(screen.getByRole('button', { name: /submit evaluation/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /trainer signoff/i })).toBeInTheDocument()
    expect(screen.queryByText(/requirements met/i)).not.toBeInTheDocument()
  })

  it('shows requirements met when assignment is ready', () => {
    render(
      <SignoffEvaluationPanel
        assignment={{
          ...assignment,
          evaluation: {
            evaluationId: 'e1',
            trainingAssignmentId: 'a1',
            result: 'pass',
            score: 100,
            notes: null,
            evaluatorUserId: 'u1',
            evaluatedAt: '2026-05-27T13:00:00Z',
          },
          signoffs: [
            {
              signoffId: 's1',
              trainingAssignmentId: 'a1',
              signoffRole: 'trainee',
              signedByUserId: 'p1',
              notes: null,
              signedAt: '2026-05-27T13:05:00Z',
            },
            {
              signoffId: 's2',
              trainingAssignmentId: 'a1',
              signoffRole: 'trainer',
              signedByUserId: 'u2',
              notes: null,
              signedAt: '2026-05-27T13:10:00Z',
            },
          ],
          completionRequirementsMet: true,
          qualificationIssue: null,
        }}
        {...baseProps}
        canSubmitEvaluation={false}
        canSubmitTrainerSignoff={false}
      />,
    )
    expect(screen.getByText(/requirements met/i)).toBeInTheDocument()
    expect(screen.getByText(/result: pass/i)).toBeInTheDocument()
  })
})
