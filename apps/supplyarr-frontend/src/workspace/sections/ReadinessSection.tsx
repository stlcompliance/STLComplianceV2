import { LoadArrHandoffPanel } from '../../components/LoadArrHandoffPanel'
import { SupplyReadinessCheckPanel } from '../../components/SupplyReadinessCheckPanel'
import { SupplyReadinessDashboardPanel } from '../../components/SupplyReadinessDashboardPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function ReadinessSection({ state: s }: Props) {
  return (
    <div className="space-y-6">
      <LoadArrHandoffPanel
        accessToken={s.accessToken}
        title="Receiving, reservations, and stock status"
        description="Use this view to understand supplier, approval, and procurement blockers. Use the receiving workspace for completion, stock adjustments, holds, reservations, and inventory activity."
        metrics={[
          {
            label: 'Issued POs',
            value: s.issuedPurchaseOrders.length,
          },
          {
            label: 'Backorders',
            value: s.backordersQuery.data?.length ?? 0,
          },
          {
            label: 'Supplier returns',
            value: s.vendorReturnsQuery.data?.length ?? 0,
          },
        ]}
      />

      <div className="grid gap-6 lg:grid-cols-2">
        <SupplyReadinessDashboardPanel accessToken={s.accessToken} canRead={s.canReadSupplyReadiness} />
        <SupplyReadinessCheckPanel
          accessToken={s.accessToken}
          canRead={s.canReadSupplyReadiness}
          parts={s.partsQuery.data ?? []}
          vendors={s.supplierDirectory}
        />
      </div>
    </div>
  )
}
