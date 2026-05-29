import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { DefectsPanel } from './DefectsPanel'

describe('DefectsPanel', () => {
  it('renders defect list and manual create form', () => {
    render(
      <DefectsPanel
        canCreate
        canCreateWorkOrder
        canManageStatus
        canUploadEvidence
        viewAllDefects
        assets={[
          {
            assetId: '11111111-1111-1111-1111-111111111111',
            assetTypeId: '22222222-2222-2222-2222-222222222222',
            typeKey: 'forklift',
            typeName: 'Forklift',
            classKey: 'vehicles',
            className: 'Vehicles',
            assetTag: 'FL-100',
            name: 'Forklift 100',
            description: '',
            lifecycleStatus: 'active',
            siteRef: null,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        defects={[
          {
            defectId: '33333333-3333-3333-3333-333333333333',
            assetId: '11111111-1111-1111-1111-111111111111',
            assetTag: 'FL-100',
            assetName: 'Forklift 100',
            inspectionRunId: '44444444-4444-4444-4444-444444444444',
            checklistItemId: '55555555-5555-5555-5555-555555555555',
            checklistItemKey: 'brakes-ok',
            title: 'Failed: Brakes operate correctly',
            severity: 'medium',
            status: 'open',
            source: 'inspection_auto',
            reportedByUserId: '66666666-6666-6666-6666-666666666666',
            createdAt: '2026-05-27T12:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
            resolvedAt: null,
            evidenceCount: 0,
          },
        ]}
        selectedDefectId=""
        selectedDefect={null}
        defectEvidence={[]}
        selectedAssetId=""
        defectTitle=""
        defectDescription=""
        defectSeverity="medium"
        statusFilter=""
        evidenceTypeKey="defect_photo"
        evidenceNotes=""
        selectedEvidenceFileName={null}
        isLoading={false}
        isEvidenceLoading={false}
        isCreating={false}
        isUpdatingStatus={false}
        isUploadingEvidence={false}
        onSelectedDefectIdChange={vi.fn()}
        onSelectedAssetIdChange={vi.fn()}
        onDefectTitleChange={vi.fn()}
        onDefectDescriptionChange={vi.fn()}
        onDefectSeverityChange={vi.fn()}
        onStatusFilterChange={vi.fn()}
        onEvidenceTypeKeyChange={vi.fn()}
        onEvidenceNotesChange={vi.fn()}
        onSelectEvidenceFile={vi.fn()}
        onUploadEvidence={vi.fn()}
        onCreateDefect={vi.fn()}
        onCreateWorkOrderFromDefect={vi.fn()}
        onUpdateStatus={vi.fn()}
      />,
    )

    expect(screen.getByText('Defects')).toBeInTheDocument()
    expect(screen.getByText('Failed: Brakes operate correctly')).toBeInTheDocument()
    expect(screen.getByText('Inspection (auto)')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Report defect' })).toBeInTheDocument()
    expect(screen.getByTestId('defect-evidence-panel')).toBeInTheDocument()
  })

  it('shows empty state when no defects', () => {
    render(
      <DefectsPanel
        canCreate={false}
        canCreateWorkOrder={false}
        canManageStatus={false}
        canUploadEvidence={false}
        viewAllDefects={false}
        assets={[]}
        defects={[]}
        selectedDefectId=""
        selectedDefect={null}
        defectEvidence={[]}
        selectedAssetId=""
        defectTitle=""
        defectDescription=""
        defectSeverity="medium"
        statusFilter="open"
        evidenceTypeKey="defect_photo"
        evidenceNotes=""
        selectedEvidenceFileName={null}
        isLoading={false}
        isEvidenceLoading={false}
        isCreating={false}
        isUpdatingStatus={false}
        isUploadingEvidence={false}
        onSelectedDefectIdChange={vi.fn()}
        onSelectedAssetIdChange={vi.fn()}
        onDefectTitleChange={vi.fn()}
        onDefectDescriptionChange={vi.fn()}
        onDefectSeverityChange={vi.fn()}
        onStatusFilterChange={vi.fn()}
        onEvidenceTypeKeyChange={vi.fn()}
        onEvidenceNotesChange={vi.fn()}
        onSelectEvidenceFile={vi.fn()}
        onUploadEvidence={vi.fn()}
        onCreateDefect={vi.fn()}
        onCreateWorkOrderFromDefect={vi.fn()}
        onUpdateStatus={vi.fn()}
      />,
    )

    expect(screen.getByText('No defects match the current filter.')).toBeInTheDocument()
  })
})
