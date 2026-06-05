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
        title="Receiving, reservations, and stock status now hand off to LoadArr"
        description="Use SupplyArr readiness to understand supplier, approval, and procurement blockers. Use LoadArr for receiving completion, stock adjustments, holds, reservations, and inventory execution history."
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
            label: 'Vendor returns',
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
          vendors={s.vendors}
        />
      </div>
    </div>
  )
}
