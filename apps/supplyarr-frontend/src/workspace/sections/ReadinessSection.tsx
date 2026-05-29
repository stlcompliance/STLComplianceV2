import { SupplyReadinessCheckPanel } from '../../components/SupplyReadinessCheckPanel'
import { SupplyReadinessDashboardPanel } from '../../components/SupplyReadinessDashboardPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function ReadinessSection({ state: s }: Props) {
  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <SupplyReadinessDashboardPanel accessToken={s.accessToken} canRead={s.canReadSupplyReadiness} />
      <SupplyReadinessCheckPanel
        accessToken={s.accessToken}
        canRead={s.canReadSupplyReadiness}
        parts={s.partsQuery.data ?? []}
        vendors={s.vendors}
      />
    </div>
  )
}
