import { LoadArrHandoffPanel } from '../../components/LoadArrHandoffPanel'
import { DemandRefsPanel } from '../../components/DemandRefsPanel'
import { ReorderEvaluationPanel } from '../../components/ReorderEvaluationPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function PlanningSection({ state: s }: Props) {
  return (
    <div className="space-y-6">
      <LoadArrHandoffPanel
        accessToken={s.accessToken}
        title="Warehouse execution"
        description="Keep procurement, supplier, item, and planning context here. Inventory balances, reservations, picks, transfers, receiving completion, and warehouse movement are handled separately."
        metrics={[
          {
            label: 'Availability snapshots',
            value: s.availabilitySnapshotsQuery.data?.length ?? 0,
          },
          {
            label: 'Demand references',
            value: s.demandRefsQuery.data?.length ?? 0,
          },
          {
            label: 'Reorder suggestions',
            value: s.reorderEvaluationQuery.data?.suggestions.length ?? 0,
          },
        ]}
      />

      <div className="grid gap-6 lg:grid-cols-2">
        <ReorderEvaluationPanel
          suggestions={s.reorderEvaluationQuery.data?.suggestions ?? []}
          parts={s.partsQuery.data ?? []}
          canManagePolicy={s.canManageInv}
          canCreatePurchaseRequest={s.canCreatePr}
          isLoading={s.reorderEvaluationQuery.isLoading}
          selectedPartId={s.reorderPolicyPartId}
          reorderPoint={s.reorderPoint}
          reorderQuantity={s.reorderQuantity}
          selectedSuggestionPartIds={s.selectedReorderPartIds}
          prRequestKey={s.reorderPrRequestKey}
          prTitle={s.reorderPrTitle}
          prNotes={s.reorderPrNotes}
          onSelectedPartIdChange={s.setReorderPolicyPartId}
          onReorderPointChange={s.setReorderPoint}
          onReorderQuantityChange={s.setReorderQuantity}
          onSelectedSuggestionPartIdsChange={s.setSelectedReorderPartIds}
          onPrRequestKeyChange={s.setReorderPrRequestKey}
          onPrTitleChange={s.setReorderPrTitle}
          onPrNotesChange={s.setReorderPrNotes}
          onSavePolicy={() => s.upsertReorderPolicyMutation.mutate()}
          onRefreshEvaluation={() => s.reorderEvaluationQuery.refetch()}
          onCreatePurchaseRequest={() => s.createPurchaseRequestFromReorderMutation.mutate()}
          isSavingPolicy={s.upsertReorderPolicyMutation.isPending}
          isCreatingPurchaseRequest={s.createPurchaseRequestFromReorderMutation.isPending}
        />
        <DemandRefsPanel
          demandRefs={s.demandRefsQuery.data ?? []}
          parts={s.partsQuery.data ?? []}
          canCreatePurchaseRequest={s.canCreatePr}
          isLoading={s.demandRefsQuery.isLoading}
          selectedDemandRefId={s.selectedDemandRefId}
          prRequestKey={s.demandPrRequestKey}
          prTitle={s.demandPrTitle}
          prNotes={s.demandPrNotes}
          onSelectedDemandRefIdChange={s.setSelectedDemandRefId}
          onPrRequestKeyChange={s.setDemandPrRequestKey}
          onPrTitleChange={s.setDemandPrTitle}
          onPrNotesChange={s.setDemandPrNotes}
          onCreatePurchaseRequest={() => s.createPurchaseRequestFromDemandRefMutation.mutate()}
          isCreatingPurchaseRequest={s.createPurchaseRequestFromDemandRefMutation.isPending}
        />
      </div>
    </div>
  )
}
