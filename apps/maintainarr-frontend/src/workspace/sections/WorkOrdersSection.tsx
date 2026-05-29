import { WorkOrdersPanel } from '../../components/WorkOrdersPanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function WorkOrdersSection({ state }: Props) {
  const s = state
  return (
    <div className="mb-8">
      <WorkOrdersPanel
        canCreate={s.canCreateWorkOrder}
        canPerform={s.canExecuteInspections}
        canClose={s.canCloseWorkOrder}
        viewAllWorkOrders={s.viewAllWorkOrders}
        sessionPersonId={s.session.personId}
        technicianRefs={s.technicianRefs}
        assets={s.assetsQuery.data ?? []}
        workOrders={s.workOrdersQuery.data ?? []}
        selectedWorkOrder={s.workOrderDetailQuery.data ?? null}
        selectedWorkOrderId={s.selectedWorkOrderId}
        selectedAssetId={s.workOrderAssetId}
        workOrderTitle={s.workOrderTitle}
        workOrderDescription={s.workOrderDescription}
        workOrderPriority={s.workOrderPriority}
        assignedPersonId={s.assignedPersonId}
        statusFilter={s.workOrderStatusFilter}
        isLoading={s.workOrdersQuery.isLoading}
        isDetailLoading={s.workOrderDetailQuery.isLoading}
        isCreating={s.createWorkOrderMutation.isPending}
        isUpdatingStatus={s.updateWorkOrderStatusMutation.isPending}
        onSelectedWorkOrderIdChange={s.setSelectedWorkOrderId}
        onSelectedAssetIdChange={s.setWorkOrderAssetId}
        onWorkOrderTitleChange={s.setWorkOrderTitle}
        onWorkOrderDescriptionChange={s.setWorkOrderDescription}
        onWorkOrderPriorityChange={s.setWorkOrderPriority}
        onAssignedPersonIdChange={s.setAssignedPersonId}
        onStatusFilterChange={s.setWorkOrderStatusFilter}
        onCreateWorkOrder={() => s.createWorkOrderMutation.mutate()}
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
      />
    </div>
  )
}
