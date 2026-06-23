import { ContractsImportPanel } from '../../components/ContractsImportPanel'
import { AuditHistoryPanel } from '../../components/AuditHistoryPanel'
import type { SupplyContractResponse } from '../../api/types'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

function formatDate(value: string | null): string {
  if (!value) {
    return 'Open-ended'
  }
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? value : parsed.toLocaleDateString()
}

function ContractCard({ contract }: { contract: SupplyContractResponse }) {
  return (
    <article className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className="text-sm font-semibold text-white">{contract.contractKey}</p>
          <p className="mt-1 text-sm text-slate-300">{contract.title}</p>
          <p className="mt-2 text-xs text-slate-400">
            {contract.vendorDisplayName} · {contract.contractType.replaceAll('_', ' ')}
          </p>
        </div>
        <span className="rounded-full bg-slate-800 px-3 py-1 text-xs uppercase tracking-wide text-slate-300">
          {contract.status}
        </span>
      </div>

      <dl className="mt-4 grid gap-3 sm:grid-cols-2">
        <Field label="Effective" value={formatDate(contract.effectiveAt)} />
        <Field label="Expires" value={formatDate(contract.expiresAt)} />
        <Field label="Renewal" value={formatDate(contract.renewalAt)} />
        <Field label="Approval" value={contract.approvalStatus.replaceAll('_', ' ')} />
        <Field label="Payment terms" value={contract.paymentTerms} />
        <Field label="Freight terms" value={contract.freightTerms} />
      </dl>
      <p className="mt-4 text-xs text-slate-400">
        Warranty: {contract.warrantyTerms || 'Not recorded'} · Minimum spend:{' '}
        {contract.minimumSpend == null ? 'Not recorded' : `$${contract.minimumSpend.toLocaleString()}`}
      </p>
    </article>
  )
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-[11px] uppercase tracking-wide text-slate-400">{label}</dt>
      <dd className="mt-1 text-sm text-slate-100">{value}</dd>
    </div>
  )
}

export function ContractsSection({ state: s }: Props) {
  const contracts = s.contractsQuery.data ?? []

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <ContractsImportPanel
        accessToken={s.accessToken}
        canManage={s.canCreatePr || s.canApprovePr || s.canCreatePo}
      />
      <AuditHistoryPanel accessToken={s.accessToken} canRead={s.canReadAuditHistory} />

      <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5 lg:col-span-2">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-white">Contract register</h2>
            <p className="mt-1 text-sm text-slate-400">
              Keep agreement metadata, renewal dates, terms, and supplier links in one place. Document files are managed separately.
            </p>
          </div>
          <span className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-300">
            {contracts.length} record{contracts.length === 1 ? '' : 's'}
          </span>
        </div>

        {contracts.length === 0 ? (
          <p className="mt-4 text-sm text-slate-400">No contract records are available yet.</p>
        ) : (
          <div className="mt-4 grid gap-4 xl:grid-cols-2">
            {contracts.slice(0, 6).map((contract) => (
              <ContractCard key={contract.contractId} contract={contract} />
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
