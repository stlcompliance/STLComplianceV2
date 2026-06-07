import { render, screen } from '@testing-library/react'
import type { ComponentProps } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { WorkOrdersPanel } from './WorkOrdersPanel'

const laborEvidenceProps = {
  tasks: [],
  labor: [],
  evidence: [],
  taskTitle: '',
  laborHours: '1',
  laborTypeKey: 'regular',
  laborPersonId: '',
  selectedTaskLineId: '',
  evidenceTypeKey: 'completion_photo',
  evidenceNotes: '',
  selectedEvidenceFileName: null,
  onTaskTitleChange: vi.fn(),
  onLaborHoursChange: vi.fn(),
  onLaborTypeKeyChange: vi.fn(),
  onLaborPersonIdChange: vi.fn(),
  onSelectedTaskLineIdChange: vi.fn(),
  onEvidenceTypeKeyChange: vi.fn(),
  onEvidenceNotesChange: vi.fn(),
  onSelectEvidenceFile: vi.fn(),
  onAddTask: vi.fn(),
  onLogLabor: vi.fn(),
  onUploadEvidence: vi.fn(),
  isAddingTask: false,
  isLoggingLabor: false,
  isUploadingEvidence: false,
}

const partsDemandProps = {
  partsDemand: [],
  partsDemandStatusEvents: [],
  demandPartNumber: '',
  demandSupplyarrPartId: '',
  demandQuantity: '1',
  demandUnitOfMeasure: 'each',
  demandNotes: '',
  createPurchaseRequestDraft: false,
  onDemandPartNumberChange: vi.fn(),
  onDemandSupplyarrPartIdChange: vi.fn(),
  onDemandQuantityChange: vi.fn(),
  onDemandUnitOfMeasureChange: vi.fn(),
  onDemandNotesChange: vi.fn(),
  onCreatePurchaseRequestDraftChange: vi.fn(),
  onAddPartsDemandLine: vi.fn(),
  onPublishPartsDemand: vi.fn(),
  isAddingPartsDemand: false,
  isPublishingPartsDemand: false,
  supplyReadiness: null,
  isSupplyReadinessLoading: false,
}

function renderPanel(overrides: Partial<ComponentProps<typeof WorkOrdersPanel>> = {}) {
  const props: ComponentProps<typeof WorkOrdersPanel> = {
    canCreate: true,
    canPerform: true,
    canClose: true,
    viewAllWorkOrders: true,
    sessionPersonId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    technicianRefs: [
      {
        personId: 'person-tech-001',
        displayName: 'Alex Technician',
        activeStatus: 'active',
        primarySite: 'yard-a',
        lastSeenAt: '2026-05-28T00:00:00Z',
      },
    ],
    workOrders: [
      {
        workOrderId: '33333333-3333-3333-3333-333333333333',
        workOrderNumber: 'WO-20260527-AB12CD34',
        assetId: '11111111-1111-1111-1111-111111111111',
        assetTag: 'FL-100',
        assetName: 'Forklift 100',
        defectId: '44444444-4444-4444-4444-444444444444',
        pmScheduleId: null,
        title: 'Repair: Hydraulic leak',
        priority: 'high',
        status: 'draft',
        source: 'defect',
        assignedTechnicianPersonId: null,
        createdByUserId: '55555555-5555-5555-5555-555555555555',
        createdAt: '2026-05-27T12:00:00Z',
        updatedAt: '2026-05-27T12:00:00Z',
        startedAt: null,
        completedAt: null,
        cancelledAt: null,
      },
    ],
    selectedWorkOrder: null,
    selectedWorkOrderId: '',
    statusFilter: '',
    isLoading: false,
    isDetailLoading: false,
    isUpdatingStatus: false,
    onSelectedWorkOrderIdChange: vi.fn(),
    onStatusFilterChange: vi.fn(),
    onUpdateStatus: vi.fn(),
    ...laborEvidenceProps,
    ...partsDemandProps,
    ...overrides,
  }

  return render(
    <MemoryRouter>
      <WorkOrdersPanel {...props} />
    </MemoryRouter>,
  )
}

describe('WorkOrdersPanel', () => {
  it('renders work order list and create wizard launcher', () => {
    renderPanel()

    expect(screen.getByText('Work orders')).toBeInTheDocument()
    expect(screen.getByText('Repair: Hydraulic leak')).toBeInTheDocument()
    expect(screen.getAllByText('Draft').length).toBeGreaterThan(0)
    expect(screen.getByRole('link', { name: 'Open create wizard' })).toBeInTheDocument()
  })

  it('shows the draft status in the filter flow', () => {
    renderPanel({ workOrders: [] })

    expect(screen.getAllByRole('option', { name: 'Draft' }).length).toBeGreaterThan(0)
  })

  it('shows empty state when no work orders', () => {
    renderPanel({
      canCreate: false,
      workOrders: [],
      selectedWorkOrder: null,
      selectedWorkOrderId: '',
      sessionPersonId: '',
      technicianRefs: [],
      statusFilter: 'open',
    })

    expect(screen.getAllByText('No work orders match the current filter.').length).toBeGreaterThan(0)
  })
})
