import { Link } from 'react-router-dom'

import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

function PortalCard({
  title,
  description,
  actionLabel,
  to,
}: {
  title: string
  description: string
  actionLabel: string
  to: string
}) {
  return (
    <article className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4">
      <h3 className="text-base font-semibold text-white">{title}</h3>
      <p className="mt-2 text-sm text-slate-400">{description}</p>
      <Link
        to={to}
        className="mt-4 inline-flex rounded-lg border border-slate-700 px-3 py-2 text-sm font-medium text-slate-100 hover:border-sky-600"
      >
        {actionLabel}
      </Link>
    </article>
  )
}

export function SupplierPortalSection({ state: s }: Props) {
  return (
    <div className="space-y-6">
      <section className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6 shadow-2xl shadow-sky-950/20">
        <h2 className="text-3xl font-semibold text-white">Supplier portal operations</h2>
        <p className="mt-3 max-w-3xl text-sm text-slate-300">
          SupplyArr supports supplier-facing collaboration, but portal identity and tenant access
          remain governed by NexArr. Use the portal for onboarding, quote submission, PO
          acknowledgment, ASN updates, and corrective response intake.
        </p>
      </section>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <PortalCard
          title="Vendor portal"
          description="Supplier-side onboarding, quote submission, and message exchange live in the vendor portal experience."
          actionLabel="Open portal"
          to="/vendor-portal"
        />
        <PortalCard
          title="Vendor order portal"
          description="Magic-link order status for suppliers and carriers is handled on the vendor order portal surfaces."
          actionLabel="Review vendor orders"
          to="/purchasing/vendor-orders"
        />
        <PortalCard
          title="Supplier onboarding"
          description="Qualification packets, compliance documents, and review queues stay anchored in SupplyArr."
          actionLabel="Open onboarding"
          to="/onboarding"
        />
      </div>

      <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
        <h3 className="text-lg font-semibold text-white">Portal scope</h3>
        <p className="mt-2 text-sm text-slate-400">
          Supplier portal access is read and write only for supplier-specific workflows. It does
          not replace internal SupplyArr ownership of supplier records, approval state, or
          procurement truth.
        </p>
        <p className="mt-3 text-sm text-slate-300">
          Current tenant: {s.me.tenantId}. Current role: {s.me.tenantRoleKey}.
        </p>
      </section>
    </div>
  )
}
