import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { EvidenceCapturePanel } from './EvidenceCapturePanel'
import type { TrainingAssignmentDetailResponse, TrainingEvidenceResponse } from '../api/types'

const assignment: TrainingAssignmentDetailResponse = {
  assignmentId: 'a1',
  staffarrPersonId: '11111111-1111-1111-1111-111111111111',
  trainingDefinitionId: 'd1',
  trainingDefinitionName: 'Annual compliance refresher',
  trainingDefinitionKey: 'annual_compliance',
  qualificationKey: 'annual_compliance',
  qualificationName: 'Annual Compliance',
  staffarrIncidentRemediationId: null,
  assignmentReason: 'manual',
  status: 'in_progress',
  dueAt: null,
  assignedByUserId: null,
  blockerPublicationId: null,
  completedAt: null,
  completedByUserId: null,
  createdAt: '2026-05-27T12:00:00Z',
  updatedAt: '2026-05-27T12:00:00Z',
  evidenceCount: 1,
  evaluation: null,
  signoffs: [],
  completionRequirementsMet: false,
  qualificationIssue: null,
}

const evidence: TrainingEvidenceResponse = {
  evidenceId: 'e1',
  trainingAssignmentId: 'a1',
  evidenceTypeKey: 'completion_certificate',
  fileName: 'certificate.pdf',
  contentType: 'application/pdf',
  sizeBytes: 2048,
  notes: 'Signed by trainer',
  uploadedByUserId: 'u1',
  createdAt: '2026-05-27T12:30:00Z',
}

describe('EvidenceCapturePanel', () => {
  it('renders empty selection state', () => {
    render(
      <EvidenceCapturePanel
        assignment={null}
        evidence={[]}
        evidenceTypeKey="completion_certificate"
        notes=""
        selectedFileName={null}
        onEvidenceTypeKeyChange={vi.fn()}
        onNotesChange={vi.fn()}
        onSelectFile={vi.fn()}
        onUploadEvidence={vi.fn()}
        isUploading={false}
        canUpload={false}
      />,
    )
    expect(screen.getByText(/select an assignment to capture evidence/i)).toBeInTheDocument()
  })

  it('renders evidence list and upload controls', () => {
    render(
      <EvidenceCapturePanel
        assignment={assignment}
        evidence={[evidence]}
        evidenceTypeKey="completion_certificate"
        notes=""
        selectedFileName="certificate.pdf"
        onEvidenceTypeKeyChange={vi.fn()}
        onNotesChange={vi.fn()}
        onSelectFile={vi.fn()}
        onUploadEvidence={vi.fn()}
        isUploading={false}
        canUpload
      />,
    )
    expect(screen.getAllByText('certificate.pdf').length).toBeGreaterThanOrEqual(1)
    expect(screen.getByRole('button', { name: /upload evidence/i })).toBeInTheDocument()
  })
})
