import { AuditHistoryPanel } from '../../components/AuditHistoryPanel'
import { SupplierEmailInboxPanel } from '../../components/SupplierEmailInboxPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function DocumentsSection({ state: s }: Props) {
  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <SupplierEmailInboxPanel
        accessToken={s.accessToken}
        canManage={s.canCreatePr || s.canApprovePr || s.canCreatePo}
      />
      <AuditHistoryPanel accessToken={s.accessToken} canRead={s.canReadAuditHistory} />

      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 lg:col-span-2">
        <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Record handoff boundary</h2>
        <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
          Track suppliers and procurement documents together. Actual file storage, versions, retention, and document lifecycle are handled separately.
        </p>
        <p className="mt-3 text-sm text-[var(--color-text-secondary)]">
          Use this surface for email-linked evidence, attachment intake, and audit traceability
          when buying activity needs supporting records.
        </p>
      </section>
    </div>
  )
}
