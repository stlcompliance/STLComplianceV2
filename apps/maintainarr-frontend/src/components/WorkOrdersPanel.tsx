import { CirclePlus } from 'lucide-react'
import { Link } from 'react-router-dom'
import type {
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
  workOrders: WorkOrderSummaryResponse[]
  selectedWorkOrder: WorkOrderDetailResponse | null
  selectedWorkOrderId: string
  statusFilter: string
  isLoading: boolean
  isDetailLoading: boolean
  isUpdatingStatus: boolean
  onSelectedWorkOrderIdChange: (value: string) => void
  onStatusFilterChange: (value: string) => void
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
  'draft',
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

export function WorkOrdersPanel({
  canCreate,
  canPerform,
  canClose,
  viewAllWorkOrders,
  sessionPersonId,
  technicianRefs,
  workOrders,
  selectedWorkOrder,
  selectedWorkOrderId,
  statusFilter,
  isLoading,
  isDetailLoading,
  isUpdatingStatus,
  onSelectedWorkOrderIdChange,
  onStatusFilterChange,
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
        <div className="mb-6 flex flex-wrap items-center justify-between gap-3 rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <div>
            <h3 className="text-sm font-semibold text-white">Create work order</h3>
            <p className="mt-1 text-xs text-slate-400">
              The guided create wizard now lives on its own page so draft planning, preview, and final actions stay in one flow.
            </p>
          </div>
          <Link
            to="/work-orders/create"
            className="inline-flex items-center gap-2 rounded-lg bg-sky-700 px-4 py-2 text-sm font-medium text-white transition hover:bg-sky-600"
          >
            <CirclePlus className="h-4 w-4" />
            Open create wizard
          </Link>
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
                        <span className="text-[var(--color-text-muted)]">—</span>
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
                <dt className="text-[var(--color-text-muted)]">Number</dt>
                <dd className="font-mono text-white">{selectedWorkOrder.workOrderNumber}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Asset</dt>
                <dd>
                  {selectedWorkOrder.assetTag} — {selectedWorkOrder.assetName}
                </dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Work type</dt>
                <dd>{selectedWorkOrder.workOrderType ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Source product</dt>
                <dd>{selectedWorkOrder.sourceProduct ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Origin</dt>
                <dd>
                  {(selectedWorkOrder.originType ?? '—')
                    + (selectedWorkOrder.originRef ? ` · ${selectedWorkOrder.originRef}` : '')}
                </dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Source ref</dt>
                <dd>{selectedWorkOrder.sourceObjectRef ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Location</dt>
                <dd>{selectedWorkOrder.staffarrLocationId ?? '—'}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Site</dt>
                <dd>{selectedWorkOrder.staffarrSiteId ?? '—'}</dd>
              </div>
              <div className="md:col-span-2">
                <dt className="text-[var(--color-text-muted)]">Description</dt>
                <dd>{selectedWorkOrder.description || '—'}</dd>
              </div>
              {selectedWorkOrder.defectTitle ? (
                <div className="md:col-span-2">
                  <dt className="text-[var(--color-text-muted)]">Linked defect</dt>
                  <dd>{selectedWorkOrder.defectTitle}</dd>
                </div>
              ) : null}
              {selectedWorkOrder.pmScheduleName ? (
                <div>
                  <dt className="text-[var(--color-text-muted)]">PM schedule</dt>
                  <dd>{selectedWorkOrder.pmScheduleName}</dd>
                </div>
              ) : null}
              <div>
                <dt className="text-[var(--color-text-muted)]">Assigned technician</dt>
                <dd>{selectedWorkOrder.assignedTechnicianPersonId ?? 'Unassigned'}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Assigned tech IDs</dt>
                <dd>{selectedWorkOrder.assignedTechnicianPersonIds?.join(', ') || '—'}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Required qualifications</dt>
                <dd>{selectedWorkOrder.requiredQualificationRefs?.join(', ') || '—'}</dd>
              </div>
              <div className="md:col-span-2">
                <dt className="text-[var(--color-text-muted)]">Vendor work refs</dt>
                <dd>{selectedWorkOrder.vendorWorkRefs?.join(', ') || '—'}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Updated</dt>
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
