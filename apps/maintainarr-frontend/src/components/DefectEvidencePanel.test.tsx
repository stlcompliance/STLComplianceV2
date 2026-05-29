import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { DefectEvidencePanel } from './DefectEvidencePanel'

describe('DefectEvidencePanel', () => {
  it('shows empty state when no defect selected', () => {
    render(
      <DefectEvidencePanel
        defectId={null}
        defectTitle={null}
        defectStatus={null}
        evidence={[]}
        canUpload
        evidenceTypeKey="defect_photo"
        evidenceNotes=""
        selectedFileName={null}
        onEvidenceTypeKeyChange={vi.fn()}
        onEvidenceNotesChange={vi.fn()}
        onSelectFile={vi.fn()}
        onUploadEvidence={vi.fn()}
        isUploadingEvidence={false}
        isLoading={false}
      />,
    )

    expect(screen.getByTestId('defect-evidence-empty')).toBeInTheDocument()
  })

  it('lists evidence and upload form for open defect', () => {
    render(
      <DefectEvidencePanel
        defectId="33333333-3333-3333-3333-333333333333"
        defectTitle="Failed brakes"
        defectStatus="open"
        evidence={[
          {
            evidenceId: '77777777-7777-7777-7777-777777777777',
            defectId: '33333333-3333-3333-3333-333333333333',
            evidenceTypeKey: 'defect_photo',
            fileName: 'brake.jpg',
            contentType: 'image/jpeg',
            sizeBytes: 2048,
            notes: 'Visible wear',
            uploadedByUserId: '66666666-6666-6666-6666-666666666666',
            createdAt: '2026-05-27T12:00:00Z',
          },
        ]}
        canUpload
        evidenceTypeKey="defect_photo"
        evidenceNotes=""
        selectedFileName={null}
        onEvidenceTypeKeyChange={vi.fn()}
        onEvidenceNotesChange={vi.fn()}
        onSelectFile={vi.fn()}
        onUploadEvidence={vi.fn()}
        isUploadingEvidence={false}
        isLoading={false}
      />,
    )

    expect(screen.getByText(/Failed brakes/)).toBeInTheDocument()
    expect(screen.getByText('brake.jpg')).toBeInTheDocument()
    expect(screen.getByTestId('defect-evidence-upload')).toBeInTheDocument()
  })
})
