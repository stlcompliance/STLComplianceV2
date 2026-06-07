import { WorkOrdersPanel } from '../../components/WorkOrdersPanel'
import { useLocation } from 'react-router-dom'
import { WorkOrderProfile } from './MaintenanceDetailProfiles'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function WorkOrdersSection({ state }: Props) {
  const s = state
  const location = useLocation()
  if (location.pathname.startsWith('/work-orders/details')) {
    return <WorkOrderProfile state={s} />
  }

  return (
    <div className="mb-8" data-testid="maintainarr-work-orders-workspace">
      <WorkOrdersPanel
        canCreate={s.canCreateWorkOrder}
        canPerform={s.canExecuteInspections}
        canClose={s.canCloseWorkOrder}
        viewAllWorkOrders={s.viewAllWorkOrders}
        sessionPersonId={s.session.personId}
        technicianRefs={s.technicianRefs}
        workOrders={s.workOrdersQuery.data ?? []}
        selectedWorkOrder={s.workOrderDetailQuery.data ?? null}
        selectedWorkOrderId={s.selectedWorkOrderId}
        statusFilter={s.workOrderStatusFilter}
        isLoading={s.workOrdersQuery.isLoading}
        isDetailLoading={s.workOrderDetailQuery.isLoading}
        isUpdatingStatus={s.updateWorkOrderStatusMutation.isPending}
        onSelectedWorkOrderIdChange={s.setSelectedWorkOrderId}
        onStatusFilterChange={s.setWorkOrderStatusFilter}
        onUpdateStatus={(workOrderId, status) =>
          s.updateWorkOrderStatusMutation.mutate({ workOrderId, status })
        }
        tasks={s.workOrderTasksQuery.data ?? []}
        labor={s.workOrderLaborQuery.data ?? []}
        evidence={s.workOrderEvidenceQuery.data ?? []}
        taskTitle={s.woTaskTitle}
        laborHours={s.woLaborHours}
        laborTypeKey={s.woLaborTypeKey}
        laborPersonId={s.woLaborPersonId}
        selectedTaskLineId={s.woSelectedTaskLineId}
        evidenceTypeKey={s.woEvidenceTypeKey}
        evidenceNotes={s.woEvidenceNotes}
        selectedEvidenceFileName={s.woEvidenceFile?.name ?? null}
        onTaskTitleChange={s.setWoTaskTitle}
        onLaborHoursChange={s.setWoLaborHours}
        onLaborTypeKeyChange={s.setWoLaborTypeKey}
        onLaborPersonIdChange={s.setWoLaborPersonId}
        onSelectedTaskLineIdChange={s.setWoSelectedTaskLineId}
        onEvidenceTypeKeyChange={s.setWoEvidenceTypeKey}
        onEvidenceNotesChange={s.setWoEvidenceNotes}
        onSelectEvidenceFile={s.setWoEvidenceFile}
        onAddTask={() => s.addWorkOrderTaskMutation.mutate()}
        onLogLabor={() => s.logWorkOrderLaborMutation.mutate()}
        onUploadEvidence={() => s.uploadWorkOrderEvidenceMutation.mutate()}
        isAddingTask={s.addWorkOrderTaskMutation.isPending}
        isLoggingLabor={s.logWorkOrderLaborMutation.isPending}
        isUploadingEvidence={s.uploadWorkOrderEvidenceMutation.isPending}
        partsDemand={s.workOrderPartsDemandQuery.data ?? []}
        partsDemandStatusEvents={s.workOrderPartsDemandStatusEventsQuery.data ?? []}
        demandPartNumber={s.demandPartNumber}
        demandSupplyarrPartId={s.demandSupplyarrPartId}
        demandQuantity={s.demandQuantity}
        demandUnitOfMeasure={s.demandUnitOfMeasure}
        demandNotes={s.demandNotes}
        createPurchaseRequestDraft={s.createPurchaseRequestDraft}
        onDemandPartNumberChange={s.setDemandPartNumber}
        onDemandSupplyarrPartIdChange={s.setDemandSupplyarrPartId}
        onDemandQuantityChange={s.setDemandQuantity}
        onDemandUnitOfMeasureChange={s.setDemandUnitOfMeasure}
        onDemandNotesChange={s.setDemandNotes}
        onCreatePurchaseRequestDraftChange={s.setCreatePurchaseRequestDraft}
        onAddPartsDemandLine={() => s.addWorkOrderPartsDemandMutation.mutate()}
        onPublishPartsDemand={() => s.publishWorkOrderPartsDemandMutation.mutate()}
        isAddingPartsDemand={s.addWorkOrderPartsDemandMutation.isPending}
        isPublishingPartsDemand={s.publishWorkOrderPartsDemandMutation.isPending}
        supplyReadiness={s.workOrderSupplyReadinessQuery.data ?? null}
        isSupplyReadinessLoading={s.workOrderSupplyReadinessQuery.isLoading}
      />
    </div>
  )
}
