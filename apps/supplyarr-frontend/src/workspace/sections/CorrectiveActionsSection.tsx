import { AuditHistoryPanel } from '../../components/AuditHistoryPanel'
import { ProcurementExceptionsPanel } from '../../components/ProcurementExceptionsPanel'
import { SupplierIncidentsPanel } from '../../components/SupplierIncidentsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

function uniqueParties(state: SupplyArrWorkspaceState) {
  return state.supplierDirectory
}

export function CorrectiveActionsSection({ state: s }: Props) {
  const parties = uniqueParties(s)

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <SupplierIncidentsPanel
        accessToken={s.accessToken}
        canManage={s.canManage}
        incidentParties={parties}
      />
      <ProcurementExceptionsPanel
        accessToken={s.accessToken}
        currentUserId={s.session?.userId ?? ''}
        canManage={s.canCreatePr}
        canApprove={s.canApprovePr}
        purchaseRequests={s.purchaseRequestsQuery.data ?? []}
        purchaseOrders={s.purchaseOrdersQuery.data ?? []}
      />
      <AuditHistoryPanel accessToken={s.accessToken} canRead={s.canReadAuditHistory} />

      <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5 lg:col-span-2">
        <h2 className="text-lg font-semibold text-white">Corrective workflow</h2>
        <p className="mt-1 text-sm text-slate-400">
          Supplier incidents and procurement exceptions are the operational trigger points for
          holds, root cause follow-up, and recovery actions. Quality disposition and supplier-side communication are handled in their own workflows.
        </p>
      </section>
    </div>
  )
}
