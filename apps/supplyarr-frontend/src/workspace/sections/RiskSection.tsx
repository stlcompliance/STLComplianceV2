import { ProcurementExceptionsPanel } from '../../components/ProcurementExceptionsPanel'
import { SupplierIncidentsPanel } from '../../components/SupplierIncidentsPanel'
import { SupplyReadinessCheckPanel } from '../../components/SupplyReadinessCheckPanel'
import { SupplyReadinessDashboardPanel } from '../../components/SupplyReadinessDashboardPanel'
import { SupplierRestrictionsPanel } from '../../components/SupplierRestrictionsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

function uniqueSuppliers(state: SupplyArrWorkspaceState) {
  return state.supplierDirectory
}

export function RiskSection({ state: s }: Props) {
  const suppliers = uniqueSuppliers(s)

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <SupplyReadinessDashboardPanel
        accessToken={s.accessToken}
        canRead={s.canReadSupplyReadiness}
      />
      <SupplyReadinessCheckPanel
        accessToken={s.accessToken}
        canRead={s.canReadSupplyReadiness}
        parts={s.partsQuery.data ?? []}
        suppliers={suppliers}
      />
      <SupplierRestrictionsPanel
        accessToken={s.accessToken}
        canManage={s.canManage}
        restrictableSuppliers={suppliers}
      />
      <SupplierIncidentsPanel
        accessToken={s.accessToken}
        canManage={s.canManage}
        supplierUnits={suppliers}
      />
      <ProcurementExceptionsPanel
        accessToken={s.accessToken}
        currentUserId={s.session?.userId ?? ''}
        canManage={s.canCreatePr}
        canApprove={s.canApprovePr}
        purchaseRequests={s.purchaseRequestsQuery.data ?? []}
        purchaseOrders={s.purchaseOrdersQuery.data ?? []}
      />

      <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5 lg:col-span-2">
        <h2 className="text-lg font-semibold text-white">Risk posture</h2>
        <p className="mt-1 text-sm text-slate-400">
          Use this page to review supplier holds, procurement exceptions, and readiness blockers
          before quotes or purchase orders are released.
        </p>
      </section>
    </div>
  )
}
