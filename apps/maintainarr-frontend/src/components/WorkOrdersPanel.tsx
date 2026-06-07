import { useMemo } from 'react'
import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import type {
  AssetResponse,
  TechnicianRefResponse,
  WorkOrderDetailResponse,
  WorkOrderEvidenceResponse,
  WorkOrderLaborEntryResponse,
  WorkOrderPartsDemandLineResponse,
  WorkOrderPartsDemandStatusEventResponse,
  WorkOrderSupplyReadinessResponse,
  WorkOrderSummaryResponse,
  WorkOrderTaskLineResponse,
} from '../api/types'
import { WorkOrderLaborEvidencePanel } from './WorkOrderLaborEvidencePanel'
import { WorkOrderLifecyclePanel } from './WorkOrderLifecyclePanel'
import { WorkOrderPartsDemandPanel } from './WorkOrderPartsDemandPanel'
import { WorkOrderSupplyReadinessPanel } from './WorkOrderSupplyReadinessPanel'

interface WorkOrdersPanelProps {
  canCreate: boolean
  canPerform: boolean
  canClose: boolean
  viewAllWorkOrders: boolean
  sessionPersonId: string
  technicianRefs: TechnicianRefResponse[]
  assets: AssetResponse[]
  workOrders: WorkOrderSummaryResponse[]
  selectedWorkOrder: WorkOrderDetailResponse | null
  selectedWorkOrderId: string
  selectedAssetId: string
  workOrderTitle: string
  workOrderDescription: string
  workOrderPriority: string
  assignedPersonId: string
  statusFilter: string
  isLoading: boolean
  isDetailLoading: boolean
  isCreating: boolean
  isUpdatingStatus: boolean
  onSelectedWorkOrderIdChange: (value: string) => void
  onSelectedAssetIdChange: (value: string) => void
  onWorkOrderTitleChange: (value: string) => void
  onWorkOrderDescriptionChange: (value: string) => void
  onWorkOrderPriorityChange: (value: string) => void
  onAssignedPersonIdChange: (value: string) => void
  onStatusFilterChange: (value: string) => void
  onCreateWorkOrder: () => void
  onUpdateStatus: (workOrderId: string, status: string) => void
  tasks: WorkOrderTaskLineResponse[]
  labor: WorkOrderLaborEntryResponse[]
  evidence: WorkOrderEvidenceResponse[]
  taskTitle: string
  laborHours: string
  laborTypeKey: string
  laborPersonId: string
  selectedTaskLineId: string
  evidenceTypeKey: string
  evidenceNotes: string
  selectedEvidenceFileName: string | null
  onTaskTitleChange: (value: string) => void
  onLaborHoursChange: (value: string) => void
  onLaborTypeKeyChange: (value: string) => void
  onLaborPersonIdChange: (value: string) => void
  onSelectedTaskLineIdChange: (value: string) => void
  onEvidenceTypeKeyChange: (value: string) => void
  onEvidenceNotesChange: (value: string) => void
  onSelectEvidenceFile: (file: File | null) => void
  onAddTask: () => void
  onLogLabor: () => void
  onUploadEvidence: () => void
  isAddingTask: boolean
  isLoggingLabor: boolean
  isUploadingEvidence: boolean
  partsDemand: WorkOrderPartsDemandLineResponse[]
  partsDemandStatusEvents: WorkOrderPartsDemandStatusEventResponse[]
  supplyReadiness: WorkOrderSupplyReadinessResponse | null
  isSupplyReadinessLoading: boolean
  demandPartNumber: string
  demandSupplyarrPartId: string
  demandQuantity: string
  demandUnitOfMeasure: string
  demandNotes: string
  createPurchaseRequestDraft: boolean
  onDemandPartNumberChange: (value: string) => void
  onDemandSupplyarrPartIdChange: (value: string) => void
  onDemandQuantityChange: (value: string) => void
  onDemandUnitOfMeasureChange: (value: string) => void
  onDemandNotesChange: (value: string) => void
  onCreatePurchaseRequestDraftChange: (value: boolean) => void
  onAddPartsDemandLine: () => void
  onPublishPartsDemand: () => void
  isAddingPartsDemand: boolean
  isPublishingPartsDemand: boolean
}

function formatSource(source: string): string {
  if (source === 'defect') {
    return 'Defect'
  }
  if (source === 'pm_schedule') {
    return 'PM schedule'
  }
  return 'Manual'
}

const WORK_ORDER_STATUS_FLOW = [
  'open',
  'requested',
  'triage',
  'rejected',
  'approved',
  'planned',
  'waiting_parts',
  'waiting_labor',
  'waiting_vendor',
  'waiting_approval',
  'waiting_compliance',
  'scheduled',
  'assigned',
  'in_progress',
  'paused',
  'blocked',
  'completed_pending_review',
  'completed',
  'closed',
  'cancelled',
  'canceled',
] as const

function humanizeStatus(status: string): string {
  const words = status.replaceAll('_', ' ')
  return words.charAt(0).toUpperCase() + words.slice(1)
}

function isTerminalStatus(status: string): boolean {
  return ['completed', 'closed', 'cancelled', 'canceled'].includes(status)
}

function statusOptionsFor(currentStatus: string, canClose: boolean): string[] {
  if (isTerminalStatus(currentStatus)) {
    return [currentStatus]
  }

  const options = Array.from(new Set([
    currentStatus,
    ...WORK_ORDER_STATUS_FLOW.filter((status) => status !== 'rejected'),
  ]))

  if (canClose) {
    options.push('closed')
    options.push('cancelled')
  }

  return Array.from(new Set(options))
}

function assetToOption(asset: AssetResponse): PickerOption {
  return {
    value: asset.assetId,
    label: `${asset.assetTag} — ${asset.name}`,
  }
}

function technicianToOption(ref: TechnicianRefResponse): PickerOption {
  const statusLabel = ref.activeStatus ? ` · ${ref.activeStatus}` : ''
  const siteLabel = ref.primarySite ? ` · ${ref.primarySite}` : ''
  return {
    value: ref.personId,
    label: `${ref.displayName}${statusLabel}${siteLabel}`,
  }
}

export function WorkOrdersPanel({
  canCreate,
  canPerform,
  canClose,
  viewAllWorkOrders,
  sessionPersonId,
  technicianRefs,
  assets,
  workOrders,
  selectedWorkOrder,
  selectedWorkOrderId,
  selectedAssetId,
  workOrderTitle,
  workOrderDescription,
  workOrderPriority,
  assignedPersonId,
  statusFilter,
  isLoading,
  isDetailLoading,
  isCreating,
  isUpdatingStatus,
  onSelectedWorkOrderIdChange,
  onSelectedAssetIdChange,
  onWorkOrderTitleChange,
  onWorkOrderDescriptionChange,
  onWorkOrderPriorityChange,
  onAssignedPersonIdChange,
  onStatusFilterChange,
  onCreateWorkOrder,
  onUpdateStatus,
  tasks,
  labor,
  evidence,
  taskTitle,
  laborHours,
  laborTypeKey,
  laborPersonId,
  selectedTaskLineId,
  evidenceTypeKey,
  evidenceNotes,
  selectedEvidenceFileName,
  onTaskTitleChange,
  onLaborHoursChange,
  onLaborTypeKeyChange,
  onLaborPersonIdChange,
  onSelectedTaskLineIdChange,
  onEvidenceTypeKeyChange,
  onEvidenceNotesChange,
  onSelectEvidenceFile,
  onAddTask,
  onLogLabor,
  onUploadEvidence,
  isAddingTask,
  isLoggingLabor,
  isUploadingEvidence,
  partsDemand,
  partsDemandStatusEvents,
  demandPartNumber,
  demandSupplyarrPartId,
  demandQuantity,
  demandUnitOfMeasure,
  demandNotes,
  createPurchaseRequestDraft,
  onDemandPartNumberChange,
  onDemandSupplyarrPartIdChange,
  onDemandQuantityChange,
  onDemandUnitOfMeasureChange,
  onDemandNotesChange,
  onCreatePurchaseRequestDraftChange,
  onAddPartsDemandLine,
  onPublishPartsDemand,
  isAddingPartsDemand,
  isPublishingPartsDemand,
  supplyReadiness,
  isSupplyReadinessLoading,
}: WorkOrdersPanelProps) {
  const assetOptions = useMemo(() => assets.map(assetToOption), [assets])
  const selectedAssetOption = useMemo(
    () => assetOptions.find((option) => option.value === selectedAssetId),
    [assetOptions, selectedAssetId],
  )
  const technicianOptions = useMemo(() => technicianRefs.map(technicianToOption), [technicianRefs])
  const selectedTechnicianOption = useMemo(
    () => technicianOptions.find((option) => option.value === assignedPersonId),
    [assignedPersonId, technicianOptions],
  )

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-6"
      data-testid="work-orders-panel"
    >
      <header className="mb-4">
        <h2 className="text-lg font-semibold text-white">Work orders</h2>
        <p className="mt-1 text-sm text-slate-400">
          Track corrective maintenance from open through completion.
          {viewAllWorkOrders
            ? ' Managers see all tenant work orders.'
            : ' Technicians see work orders they created or are assigned to.'}
        </p>
      </header>

      <div className="mb-4 flex flex-wrap gap-3">
        <label className="block text-sm" htmlFor="work-orders-status-filter">
          <span className="text-slate-300">Status filter</span>
          <select
            id="work-orders-status-filter"
            className="mt-1 rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
            value={statusFilter}
            onChange={(event) => onStatusFilterChange(event.target.value)}
          >
            <option value="">All statuses</option>
            {WORK_ORDER_STATUS_FLOW.map((status) => (
              <option key={status} value={status}>
                {humanizeStatus(status)}
              </option>
            ))}
          </select>
        </label>
      </div>

      {canCreate ? (
        <div className="mb-6 grid gap-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4 md:grid-cols-2">
          <StaticSearchPicker
            id="work-order-create-asset"
            label="Asset for work order"
            value={selectedAssetId}
            onChange={onSelectedAssetIdChange}
            options={assetOptions}
            placeholder="Search assets…"
            testId="work-order-create-asset-picker"
            selectedOption={selectedAssetOption}
          />

          <label className="block text-sm md:col-span-2" htmlFor="work-order-create-title">
            <span className="text-slate-300">Work order title</span>
            <input
              id="work-order-create-title"
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              value={workOrderTitle}
              onChange={(event) => onWorkOrderTitleChange(event.target.value)}
            />
          </label>

          <label className="block text-sm md:col-span-2" htmlFor="work-order-create-description">
            <span className="text-slate-300">Work order description</span>
            <textarea
              id="work-order-create-description"
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              rows={2}
              value={workOrderDescription}
              onChange={(event) => onWorkOrderDescriptionChange(event.target.value)}
            />
          </label>

          <label className="block text-sm" htmlFor="work-order-create-priority">
            <span className="text-slate-300">Work order priority</span>
            <select
              id="work-order-create-priority"
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              value={workOrderPriority}
              onChange={(event) => onWorkOrderPriorityChange(event.target.value)}
            >
              <option value="low">Low</option>
              <option value="medium">Medium</option>
              <option value="high">High</option>
              <option value="urgent">Urgent</option>
            </select>
          </label>

          <StaticSearchPicker
            id="work-order-create-assigned-technician"
            label="Assigned technician"
            value={assignedPersonId}
            onChange={onAssignedPersonIdChange}
            options={[
              { value: '', label: 'Unassigned' },
              { value: sessionPersonId, label: `Me (${sessionPersonId})` },
              ...technicianOptions.filter((option) => option.value !== sessionPersonId),
            ]}
            placeholder="Search technicians…"
            testId="work-order-create-assigned-technician-picker"
            selectedOption={selectedTechnicianOption}
          />

          <div className="flex items-end md:col-span-2">
            <button
              type="button"
              className="rounded-lg bg-sky-700 px-4 py-2 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
              disabled={!selectedAssetId || !workOrderTitle.trim() || isCreating}
              onClick={onCreateWorkOrder}
            >
              {isCreating ? 'Creating…' : 'Create work order'}
            </button>
          </div>
        </div>
      ) : null}

      {isLoading ? (
        <p className="text-sm text-slate-400">Loading work orders…</p>
      ) : workOrders.length === 0 ? (
        <p className="text-sm text-slate-400">No work orders match the current filter.</p>
      ) : (
        <div className="overflow-x-auto" data-testid="work-order-list">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-slate-700 text-slate-400">
              <tr>
                <th className="px-3 py-2 font-medium">Number</th>
                <th className="px-3 py-2 font-medium">Asset</th>
                <th className="px-3 py-2 font-medium">Title</th>
                <th className="px-3 py-2 font-medium">Priority</th>
                <th className="px-3 py-2 font-medium">Status</th>
                <th className="px-3 py-2 font-medium">Source</th>
                <th className="px-3 py-2 font-medium">Detail</th>
                {canPerform ? <th className="px-3 py-2 font-medium">Actions</th> : null}
              </tr>
            </thead>
            <tbody>
              {workOrders.map((workOrder) => (
                <tr key={workOrder.workOrderId} className="border-b border-slate-800 text-slate-200">
                  <td className="px-3 py-2 font-mono text-xs">{workOrder.workOrderNumber}</td>
                  <td className="px-3 py-2">
                    <div className="font-medium">{workOrder.assetTag}</div>
                    <div className="text-xs text-slate-400">{workOrder.assetName}</div>
                  </td>
                  <td className="px-3 py-2">{workOrder.title}</td>
                  <td className="px-3 py-2">{workOrder.priority}</td>
                  <td className="px-3 py-2">{workOrder.status}</td>
                  <td className="px-3 py-2">{formatSource(workOrder.source)}</td>
                  <td className="px-3 py-2">
                    <button
                      type="button"
                      className="text-sky-400 hover:text-sky-300"
                      data-testid={`work-order-select-${workOrder.workOrderId}`}
                      onClick={() => onSelectedWorkOrderIdChange(workOrder.workOrderId)}
                    >
                      {selectedWorkOrderId === workOrder.workOrderId ? 'Selected' : 'View'}
                    </button>
                  </td>
                  {canPerform ? (
                    <td className="px-3 py-2">
                      {!isTerminalStatus(workOrder.status) ? (
                        <select
                          id={`work-order-status-${workOrder.workOrderId}`}
                          className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-white"
                          value={workOrder.status}
                          disabled={isUpdatingStatus}
                          aria-label={`Work order status for ${workOrder.title}`}
                          onChange={(event) => onUpdateStatus(workOrder.workOrderId, event.target.value)}
                        >
                          {statusOptionsFor(workOrder.status, canClose).map((status) => (
                            <option key={status} value={status}>
                              {humanizeStatus(status)}
                            </option>
                          ))}
                        </select>
                      ) : (
                        <span className="text-slate-500">—</span>
                      )}
                    </td>
                  ) : null}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {selectedWorkOrderId ? (
        <div className="mt-6 rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <h3 className="text-sm font-semibold text-white">Work order detail</h3>
          {isDetailLoading ? (
            <p className="mt-2 text-sm text-slate-400">Loading detail…</p>
          ) : selectedWorkOrder ? (
            <>
            <dl className="mt-3 grid gap-2 text-sm text-slate-300 md:grid-cols-2">
              <div>
                <dt className="text-slate-500">Number</dt>
                <dd className="font-mono text-white">{selectedWorkOrder.workOrderNumber}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Asset</dt>
                <dd>
                  {selectedWorkOrder.assetTag} — {selectedWorkOrder.assetName}
                </dd>
              </div>
              <div>
                <dt className="text-slate-500">Work type</dt>
                <dd>{selectedWorkOrder.workOrderType ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Source product</dt>
                <dd>{selectedWorkOrder.sourceProduct ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Origin</dt>
                <dd>
                  {(selectedWorkOrder.originType ?? '—')
                    + (selectedWorkOrder.originRef ? ` · ${selectedWorkOrder.originRef}` : '')}
                </dd>
              </div>
              <div>
                <dt className="text-slate-500">Source ref</dt>
                <dd>{selectedWorkOrder.sourceObjectRef ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Location</dt>
                <dd>{selectedWorkOrder.staffarrLocationId ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Site</dt>
                <dd>{selectedWorkOrder.staffarrSiteId ?? '—'}</dd>
              </div>
              <div className="md:col-span-2">
                <dt className="text-slate-500">Description</dt>
                <dd>{selectedWorkOrder.description || '—'}</dd>
              </div>
              {selectedWorkOrder.defectTitle ? (
                <div className="md:col-span-2">
                  <dt className="text-slate-500">Linked defect</dt>
                  <dd>{selectedWorkOrder.defectTitle}</dd>
                </div>
              ) : null}
              {selectedWorkOrder.pmScheduleName ? (
                <div>
                  <dt className="text-slate-500">PM schedule</dt>
                  <dd>{selectedWorkOrder.pmScheduleName}</dd>
                </div>
              ) : null}
              <div>
                <dt className="text-slate-500">Assigned technician</dt>
                <dd>{selectedWorkOrder.assignedTechnicianPersonId ?? 'Unassigned'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Assigned tech IDs</dt>
                <dd>{selectedWorkOrder.assignedTechnicianPersonIds?.join(', ') || '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Required qualifications</dt>
                <dd>{selectedWorkOrder.requiredQualificationRefs?.join(', ') || '—'}</dd>
              </div>
              <div className="md:col-span-2">
                <dt className="text-slate-500">Vendor work refs</dt>
                <dd>{selectedWorkOrder.vendorWorkRefs?.join(', ') || '—'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Updated</dt>
                <dd>{new Date(selectedWorkOrder.updatedAt).toLocaleString()}</dd>
              </div>
            </dl>
            <WorkOrderLifecyclePanel
              workOrder={selectedWorkOrder}
              tasks={tasks}
              labor={labor}
              evidence={evidence}
              isDetailLoading={isDetailLoading}
            />
            <WorkOrderLaborEvidencePanel
              workOrder={selectedWorkOrder}
              tasks={tasks}
              labor={labor}
              evidence={evidence}
              canPerform={canPerform}
              canApprove={canClose}
              sessionPersonId={sessionPersonId}
              technicianRefs={technicianRefs}
              taskTitle={taskTitle}
              laborHours={laborHours}
              laborTypeKey={laborTypeKey}
              laborPersonId={laborPersonId}
              selectedTaskLineId={selectedTaskLineId}
              evidenceTypeKey={evidenceTypeKey}
              evidenceNotes={evidenceNotes}
              selectedFileName={selectedEvidenceFileName}
              onTaskTitleChange={onTaskTitleChange}
              onLaborHoursChange={onLaborHoursChange}
              onLaborTypeKeyChange={onLaborTypeKeyChange}
              onLaborPersonIdChange={onLaborPersonIdChange}
              onSelectedTaskLineIdChange={onSelectedTaskLineIdChange}
              onEvidenceTypeKeyChange={onEvidenceTypeKeyChange}
              onEvidenceNotesChange={onEvidenceNotesChange}
              onSelectFile={onSelectEvidenceFile}
              onAddTask={onAddTask}
              onLogLabor={onLogLabor}
              onUploadEvidence={onUploadEvidence}
              isAddingTask={isAddingTask}
              isLoggingLabor={isLoggingLabor}
              isUploadingEvidence={isUploadingEvidence}
            />
            <WorkOrderPartsDemandPanel
              workOrder={selectedWorkOrder}
              demandLines={partsDemand}
              statusEvents={partsDemandStatusEvents}
              canPerform={canPerform}
              partNumber={demandPartNumber}
              supplyarrPartId={demandSupplyarrPartId}
              quantityRequested={demandQuantity}
              unitOfMeasure={demandUnitOfMeasure}
              notes={demandNotes}
              createPurchaseRequestDraft={createPurchaseRequestDraft}
              onPartNumberChange={onDemandPartNumberChange}
              onSupplyarrPartIdChange={onDemandSupplyarrPartIdChange}
              onQuantityRequestedChange={onDemandQuantityChange}
              onUnitOfMeasureChange={onDemandUnitOfMeasureChange}
              onNotesChange={onDemandNotesChange}
              onCreatePurchaseRequestDraftChange={onCreatePurchaseRequestDraftChange}
              onAddDemandLine={onAddPartsDemandLine}
              onPublishDemand={onPublishPartsDemand}
              isAdding={isAddingPartsDemand}
              isPublishing={isPublishingPartsDemand}
            />
            <WorkOrderSupplyReadinessPanel
              readiness={supplyReadiness}
              isLoading={isSupplyReadinessLoading}
            />
            </>
          ) : (
            <p className="mt-2 text-sm text-slate-400">Could not load work order detail.</p>
          )}
        </div>
      ) : null}
    </section>
  )
}
