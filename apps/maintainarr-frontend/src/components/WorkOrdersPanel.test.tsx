import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { WorkOrdersPanel } from './WorkOrdersPanel'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const mod = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...mod,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      testId,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      options: { value: string; label: string }[]
      testId?: string
    }) => (
      <label htmlFor={testId ?? 'mock-static-search-picker'}>
        {label}
        <input
          id={testId ?? 'mock-static-search-picker'}
          aria-label={label}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <ul>
          {options.map((option) => (
            <li key={option.value}>{option.label}</li>
          ))}
        </ul>
      </label>
    ),
  }
})

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

describe('WorkOrdersPanel', () => {
  it('renders work order list and create form', () => {
    render(
      <WorkOrdersPanel
        canCreate
        canPerform
        canClose
        viewAllWorkOrders
        sessionPersonId="bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
        technicianRefs={[
          {
            personId: 'person-tech-001',
            displayName: 'Alex Technician',
            activeStatus: 'active',
            primarySite: 'yard-a',
            lastSeenAt: '2026-05-28T00:00:00Z',
          },
        ]}
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
        workOrders={[
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
            status: 'open',
            source: 'defect',
            assignedTechnicianPersonId: null,
            createdByUserId: '55555555-5555-5555-5555-555555555555',
            createdAt: '2026-05-27T12:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
            startedAt: null,
            completedAt: null,
            cancelledAt: null,
          },
        ]}
        selectedWorkOrder={null}
        selectedWorkOrderId=""
        selectedAssetId=""
        workOrderTitle=""
        workOrderDescription=""
        workOrderPriority="medium"
        assignedPersonId=""
        statusFilter=""
        isLoading={false}
        isDetailLoading={false}
        isCreating={false}
        isUpdatingStatus={false}
        onSelectedWorkOrderIdChange={vi.fn()}
        onSelectedAssetIdChange={vi.fn()}
        onWorkOrderTitleChange={vi.fn()}
        onWorkOrderDescriptionChange={vi.fn()}
        onWorkOrderPriorityChange={vi.fn()}
        onAssignedPersonIdChange={vi.fn()}
        onStatusFilterChange={vi.fn()}
        onCreateWorkOrder={vi.fn()}
        onUpdateStatus={vi.fn()}
        {...laborEvidenceProps}
        {...partsDemandProps}
      />,
    )

    expect(screen.getByText('Work orders')).toBeInTheDocument()
    expect(screen.getByText('Repair: Hydraulic leak')).toBeInTheDocument()
    expect(screen.getByText('Defect')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create work order' })).toBeInTheDocument()
  })

  it('uses searchable pickers for asset and technician selection', () => {
    render(
      <WorkOrdersPanel
        canCreate
        canPerform={false}
        canClose={false}
        viewAllWorkOrders={false}
        sessionPersonId="bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
        technicianRefs={[
          {
            personId: 'person-tech-001',
            displayName: 'Alex Technician',
            activeStatus: 'active',
            primarySite: 'yard-a',
            lastSeenAt: '2026-05-28T00:00:00Z',
          },
        ]}
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
        workOrders={[]}
        selectedWorkOrder={null}
        selectedWorkOrderId=""
        selectedAssetId=""
        workOrderTitle="Repair"
        workOrderDescription=""
        workOrderPriority="medium"
        assignedPersonId=""
        statusFilter=""
        isLoading={false}
        isDetailLoading={false}
        isCreating={false}
        isUpdatingStatus={false}
        onSelectedWorkOrderIdChange={vi.fn()}
        onSelectedAssetIdChange={vi.fn()}
        onWorkOrderTitleChange={vi.fn()}
        onWorkOrderDescriptionChange={vi.fn()}
        onWorkOrderPriorityChange={vi.fn()}
        onAssignedPersonIdChange={vi.fn()}
        onStatusFilterChange={vi.fn()}
        onCreateWorkOrder={vi.fn()}
        onUpdateStatus={vi.fn()}
        {...laborEvidenceProps}
        {...partsDemandProps}
      />,
    )

    const assetPicker = screen.getAllByTestId('work-order-create-asset-picker')[0]
    const technicianPicker = screen.getAllByTestId('work-order-create-assigned-technician-picker')[0]

    expect(assetPicker).toBeInTheDocument()
    expect(technicianPicker).toBeInTheDocument()
    expect(screen.getAllByText('FL-100 — Forklift 100').length).toBeGreaterThan(0)
    expect(screen.getAllByText('Alex Technician · active · yard-a').length).toBeGreaterThan(0)
  })

  it('shows empty state when no work orders', () => {
    render(
      <WorkOrdersPanel
        canCreate={false}
        canPerform={false}
        canClose={false}
        viewAllWorkOrders={false}
        sessionPersonId=""
        technicianRefs={[]}
        assets={[]}
        workOrders={[]}
        selectedWorkOrder={null}
        selectedWorkOrderId=""
        selectedAssetId=""
        workOrderTitle=""
        workOrderDescription=""
        workOrderPriority="medium"
        assignedPersonId=""
        statusFilter="open"
        isLoading={false}
        isDetailLoading={false}
        isCreating={false}
        isUpdatingStatus={false}
        onSelectedWorkOrderIdChange={vi.fn()}
        onSelectedAssetIdChange={vi.fn()}
        onWorkOrderTitleChange={vi.fn()}
        onWorkOrderDescriptionChange={vi.fn()}
        onWorkOrderPriorityChange={vi.fn()}
        onAssignedPersonIdChange={vi.fn()}
        onStatusFilterChange={vi.fn()}
        onCreateWorkOrder={vi.fn()}
        onUpdateStatus={vi.fn()}
        {...laborEvidenceProps}
        {...partsDemandProps}
      />,
    )

    expect(screen.getAllByText('No work orders match the current filter.').length).toBeGreaterThan(0)
  })
})
