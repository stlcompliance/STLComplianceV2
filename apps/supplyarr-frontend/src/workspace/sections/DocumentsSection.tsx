import { AuditHistoryPanel } from '../../components/AuditHistoryPanel'
import { VendorEmailInboxPanel } from '../../components/VendorEmailInboxPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function DocumentsSection({ state: s }: Props) {
  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <VendorEmailInboxPanel
        accessToken={s.accessToken}
        canManage={s.canCreatePr || s.canApprovePr || s.canCreatePo}
      />
      <AuditHistoryPanel accessToken={s.accessToken} canRead={s.canReadAuditHistory} />

      <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5 lg:col-span-2">
        <h2 className="text-lg font-semibold text-white">Record handoff boundary</h2>
        <p className="mt-1 text-sm text-slate-400">
          Track suppliers and procurement documents together. Actual file storage, versions, retention, and document lifecycle are handled separately.
        </p>
        <p className="mt-3 text-sm text-slate-300">
          Use this surface for email-linked evidence, attachment intake, and audit traceability
          when buying activity needs supporting records.
        </p>
      </section>
    </div>
  )
}
