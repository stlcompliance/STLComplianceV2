import { useEffect, useState, type FormEvent, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  AlertTriangle,
  ArrowRightLeft,
  BadgeDollarSign,
  CircleDollarSign,
  FileChartColumn,
  Landmark,
  Receipt,
  ShieldCheck,
} from 'lucide-react'
import { Navigate, Route, Routes, useLocation } from 'react-router-dom'
import {
  ApiErrorCallout,
  ProductWorkspaceFrame,
  buildProductLaunchUrlMap,
  formatProductLaunchError,
  getErrorMessage,
  getLaunchCatalog,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  useProductWorkspaceLaunch,
} from '@stl/shared-ui'
import {
  clearSession,
  loadSession,
  type StoredLedgArrSession,
} from './auth/sessionStorage'
import {
  approveBillableEvent,
  createBankAccount,
  createBankReconciliation,
  createBankTransaction,
  createBillableEventFromPacket,
  createIntercompanyTransaction,
  createTaxAdjustment,
  closeFiscalPeriod,
  getApAging,
  getArAging,
  getDashboard,
  getReportSummary,
  getSessionBootstrap,
  getTrialBalance,
  generateBillableEventInvoiceDraft,
  holdBillableEvent,
  lockFiscalPeriod,
  listBankAccounts,
  listBankReconciliations,
  listBankTransactions,
  listBillableEvents,
  listBudgets,
  listCustomerInvoices,
  listCustomerPayments,
  listExternalFinanceSystems,
  listExternalPostingBatches,
  listFinancialLegalEntities,
  listFinancialLegalEntityRelationships,
  listFixedAssets,
  listFixedAssetDepreciationSchedules,
  listFiscalPeriods,
  listGLAccounts,
  listIntercompanyBalances,
  listIntercompanyTransactions,
  listJournals,
  listPackets,
  listPaymentRuns,
  listTaxCodes,
  listTaxAdjustments,
  listTaxLiabilitySummaries,
  listVendorBills,
  reopenFiscalPeriod,
  settleIntercompanyTransaction,
  type AgingBucket,
  type BankAccountSummary,
  type BankReconciliationSummary,
  type BankTransactionSummary,
  type BillableEventSummary,
  type BudgetSummary,
  type CustomerInvoice,
  type CustomerPaymentSummary,
  type ExternalFinanceSystem,
  type ExternalPostingBatchSummary,
  type FinancialLegalEntity,
  type FinancialLegalEntityRelationshipSummary,
  type FinancialPacket,
  type FixedAssetSummary,
  type FiscalPeriod,
  type GLAccount,
  type IntercompanyBalanceSummary,
  type IntercompanyTransactionSummary,
  type JournalEntry,
  type PaymentRunSummary,
  type ReportSummaryResponse,
  type TaxCode,
  type TaxAdjustmentSummary,
  type TaxLiabilitySummary,
  type TrialBalanceRow,
  type VendorBill,
} from './api/client'
import { LaunchPage } from './LaunchPage'
import { ledgArrNavItems } from './navigation/ledgarrNav'
import { PayrollPage } from './pages/PayrollPage'
import { LedgArrSettingsPage } from './pages/settings/LedgArrSettingsPage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_LEDGARR_API_BASE ?? ''

function formatMoney(value: number): string {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(value)
}

function formatDate(value: string | null | undefined): string {
  if (!value) {
    return 'n/a'
  }

  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleString(undefined, {
        month: 'short',
        day: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
      })
}

function formatDateOnly(value: string | null | undefined): string {
  if (!value) {
    return 'n/a'
  }

  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleDateString(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
      })
}

function titleize(value: string): string {
  return value
    .split(/[_-]/g)
    .filter(Boolean)
    .map((part) => part.slice(0, 1).toUpperCase() + part.slice(1))
    .join(' ')
}

function sourceProductTone(productKey: string): string {
  switch (productKey.toLowerCase()) {
    case 'customarr':
      return 'bg-sky-500/15 text-sky-200 border-sky-400/30'
    case 'supplyarr':
      return 'bg-emerald-500/15 text-emerald-200 border-emerald-400/30'
    case 'ordarr':
      return 'bg-indigo-500/15 text-indigo-200 border-indigo-400/30'
    case 'routarr':
      return 'bg-amber-500/15 text-amber-200 border-amber-400/30'
    case 'loadarr':
      return 'bg-cyan-500/15 text-cyan-200 border-cyan-400/30'
    case 'maintainarr':
      return 'bg-fuchsia-500/15 text-fuchsia-200 border-fuchsia-400/30'
    case 'assurarr':
      return 'bg-rose-500/15 text-rose-200 border-rose-400/30'
    default:
      return 'bg-slate-700/70 text-slate-200 border-slate-600/70'
  }
}

function PageHeader({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow: string
  title: string
  description: string
  action?: ReactNode
}) {
  return (
    <div className="flex flex-col gap-3 border-b border-slate-700/70 pb-4 lg:flex-row lg:items-end lg:justify-between">
      <div className="space-y-2">
        <p className="ledgarr-label">{eyebrow}</p>
        <h1 className="text-2xl font-semibold text-slate-50">{title}</h1>
        <p className="max-w-4xl text-sm text-slate-300">{description}</p>
      </div>
      {action}
    </div>
  )
}

function Panel({
  title,
  icon,
  children,
}: {
  title: string
  icon: ReactNode
  children: ReactNode
}) {
  return (
    <section className="ledgarr-panel">
      <div className="ledgarr-panel-inner space-y-3">
        <div className="flex items-center gap-2">
          {icon}
          <h2 className="text-base font-semibold text-slate-50">{title}</h2>
        </div>
        {children}
      </div>
    </section>
  )
}

function Metric({
  label,
  value,
  hint,
}: {
  label: string
  value: string | number
  hint: string
}) {
  return (
    <div className="ledgarr-panel">
      <div className="ledgarr-panel-inner">
        <p className="ledgarr-label">{label}</p>
        <p className="mt-2 text-3xl font-semibold text-slate-50">{value}</p>
        <p className="mt-2 text-sm text-slate-300">{hint}</p>
      </div>
    </div>
  )
}

function EmptyState({ title, detail }: { title: string; detail?: string }) {
  return (
    <div className="rounded-lg border border-dashed border-slate-700/80 p-4 text-sm text-slate-400">
      <p>{title}</p>
      {detail ? <p className="mt-2 text-slate-500">{detail}</p> : null}
    </div>
  )
}

function StatusPill({ status }: { status: string }) {
  const lowered = status.toLowerCase()
  const className =
    lowered.includes('failed') ||
    lowered.includes('variance') ||
    lowered.includes('blocked') ||
    lowered.includes('overdue') ||
    lowered.includes('locked') ||
    lowered.includes('closed')
      ? 'ledgarr-pill warning'
      : 'ledgarr-pill'
  return <span className={className}>{titleize(status)}</span>
}

function SourceProductBadge({ productKey }: { productKey: string }) {
  return (
    <span
      className={`inline-flex items-center rounded-full border px-2 py-1 text-xs font-medium ${sourceProductTone(
        productKey,
      )}`}
    >
      {titleize(productKey)}
    </span>
  )
}

function ScopeNote({ children }: { children: ReactNode }) {
  return (
    <div className="rounded-xl border border-slate-700/80 bg-slate-950/70 px-4 py-3 text-sm text-slate-300">
      <span className="font-medium text-slate-100">Dashboard scope:</span> {children}
    </div>
  )
}

function TabStrip({
  tabs,
  activeTab,
  onChange,
}: {
  tabs: readonly string[]
  activeTab: string
  onChange: (tab: string) => void
}) {
  return (
    <div className="flex flex-wrap gap-2">
      {tabs.map((tab) => {
        const active = tab === activeTab
        return (
          <button
            key={tab}
            type="button"
            onClick={() => onChange(tab)}
            className={`rounded-full border px-3 py-1.5 text-sm transition ${
              active
                ? 'border-teal-400/50 bg-teal-400/12 text-teal-100'
                : 'border-slate-700 bg-slate-900/70 text-slate-300 hover:border-slate-600 hover:text-slate-100'
            }`}
          >
            {tab}
          </button>
        )
      })}
    </div>
  )
}

function AttentionList({
  items,
}: {
  items: Array<{ title: string; detail: string; severity: string }>
}) {
  if (items.length === 0) {
    return <EmptyState title="No LedgArr exceptions need review right now." />
  }

  return (
    <div className="space-y-3">
      {items.map((item) => (
        <div key={`${item.severity}-${item.title}`} className="rounded-lg border border-slate-700/70 bg-slate-900/70 p-3">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <strong className="text-sm text-slate-50">{item.title}</strong>
            <StatusPill status={item.severity} />
          </div>
          <p className="mt-2 text-sm text-slate-300">{item.detail}</p>
        </div>
      ))}
    </div>
  )
}

function AgingCard({ title, buckets }: { title: string; buckets: AgingBucket[] }) {
  return (
    <Panel title={title} icon={<CircleDollarSign className="h-4 w-4 text-teal-300" />}>
      {buckets.length === 0 ? (
        <EmptyState title="No aging balances are available." />
      ) : (
        <div className="ledgarr-grid cols-4">
          {buckets.map((bucket) => (
            <Metric
              key={bucket.bucket}
              label={titleize(bucket.bucket)}
              value={formatMoney(bucket.amount)}
              hint="Tenant-scoped financial aging"
            />
          ))}
        </div>
      )}
    </Panel>
  )
}

function JournalTable({ journals }: { journals: JournalEntry[] }) {
  if (journals.length === 0) {
    return <EmptyState title="No journals have been created." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Journal</th>
            <th>Date</th>
            <th>Description</th>
            <th>Debits</th>
            <th>Credits</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {journals.map((journal) => (
            <tr key={journal.id}>
              <td className="font-semibold text-slate-50">{journal.journalNumber}</td>
              <td>{formatDateOnly(journal.accountingDate)}</td>
              <td>{journal.description}</td>
              <td>{formatMoney(journal.totalDebits)}</td>
              <td>{formatMoney(journal.totalCredits)}</td>
              <td>
                <StatusPill status={journal.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function PacketTable({ packets }: { packets: FinancialPacket[] }) {
  if (packets.length === 0) {
    return <EmptyState title="No financial packets have been received." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Source</th>
            <th>Packet</th>
            <th>Accounting Date</th>
            <th>Amount</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {packets.map((packet) => (
            <tr key={packet.id}>
              <td>
                <div className="flex flex-col gap-2">
                  <strong className="text-slate-50">{packet.sourceRecordDisplayName}</strong>
                  <SourceProductBadge productKey={packet.sourceProductKey} />
                </div>
              </td>
              <td>{titleize(packet.packetType)}</td>
              <td>{formatDateOnly(packet.accountingDate)}</td>
              <td>{formatMoney(packet.sourceTotalAmount)}</td>
              <td>
                <StatusPill status={packet.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function BillableEventTable({
  events,
  onApprove,
  onHold,
  onGenerateDraft,
}: {
  events: BillableEventSummary[]
  onApprove: (billableEventId: string) => void
  onHold: (billableEventId: string) => void
  onGenerateDraft: (billableEventId: string) => void
}) {
  if (events.length === 0) {
    return <EmptyState title="No LedgArr billable events have been created yet." detail="Create billable events from source packets to start the billing review workflow." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Event</th>
            <th>Source</th>
            <th>Charge type</th>
            <th>Amount</th>
            <th>Approval</th>
            <th>Invoice</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {events.map((billableEvent) => (
            <tr key={billableEvent.id}>
              <td>
                <div className="font-semibold text-slate-50">{billableEvent.eventNumber}</div>
                <div className="text-xs text-slate-400">{billableEvent.financialLegalEntityDisplayName}</div>
              </td>
              <td>
                <div className="flex flex-col gap-2">
                  <strong className="text-slate-50">{billableEvent.sourceRecordDisplayName}</strong>
                  <SourceProductBadge productKey={billableEvent.sourceProductKey} />
                </div>
              </td>
              <td>
                <div>{titleize(billableEvent.chargeType)}</div>
                <div className="text-xs text-slate-400">{billableEvent.customerDisplayName}</div>
              </td>
              <td>
                <div>{formatMoney(billableEvent.amount)}</div>
                <div className="text-xs text-slate-400">{formatDateOnly(billableEvent.accountingDate)}</div>
              </td>
              <td>
                <StatusPill status={billableEvent.approvalStatus} />
                {billableEvent.holdReason ? <div className="mt-2 text-xs text-slate-400">{billableEvent.holdReason}</div> : null}
              </td>
              <td>
                <StatusPill status={billableEvent.invoiceStatus} />
                {billableEvent.exceptionReason ? <div className="mt-2 text-xs text-amber-200">{titleize(billableEvent.exceptionReason)}</div> : null}
              </td>
              <td>
                <div className="flex flex-wrap gap-2">
                  <button
                    type="button"
                    onClick={() => onApprove(billableEvent.id)}
                    className="rounded-full border border-slate-700 bg-slate-900/70 px-3 py-1 text-xs font-medium text-slate-300 transition hover:border-slate-600 hover:text-slate-100"
                  >
                    Approve
                  </button>
                  <button
                    type="button"
                    onClick={() => onHold(billableEvent.id)}
                    className="rounded-full border border-slate-700 bg-slate-900/70 px-3 py-1 text-xs font-medium text-slate-300 transition hover:border-slate-600 hover:text-slate-100"
                  >
                    Hold
                  </button>
                  <button
                    type="button"
                    onClick={() => onGenerateDraft(billableEvent.id)}
                    className="rounded-full border border-teal-400/40 bg-teal-400/12 px-3 py-1 text-xs font-medium text-teal-100 transition hover:border-teal-300 hover:bg-teal-400/18"
                  >
                    Draft Invoice
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function VendorBillTable({ bills }: { bills: VendorBill[] }) {
  if (bills.length === 0) {
    return <EmptyState title="No vendor bills have been created." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Vendor</th>
            <th>Bill</th>
            <th>Due</th>
            <th>Amount</th>
            <th>Match</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {bills.map((bill) => (
            <tr key={bill.id}>
              <td className="font-semibold text-slate-50">{bill.vendorDisplayName}</td>
              <td>{bill.vendorInvoiceNumber}</td>
              <td>{formatDateOnly(bill.dueDate)}</td>
              <td>{formatMoney(bill.totalAmount)}</td>
              <td>
                <StatusPill status={bill.matchStatus} />
              </td>
              <td>
                <StatusPill status={bill.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function CustomerInvoiceTable({ invoices }: { invoices: CustomerInvoice[] }) {
  if (invoices.length === 0) {
    return <EmptyState title="No customer invoices have been created." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Customer</th>
            <th>Invoice</th>
            <th>Due</th>
            <th>Amount</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {invoices.map((invoice) => (
            <tr key={invoice.id}>
              <td className="font-semibold text-slate-50">{invoice.customerDisplayName}</td>
              <td>{invoice.invoiceNumber}</td>
              <td>{formatDateOnly(invoice.dueDate)}</td>
              <td>{formatMoney(invoice.totalAmount)}</td>
              <td>
                <StatusPill status={invoice.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function LegalEntityTable({ entities }: { entities: FinancialLegalEntity[] }) {
  if (entities.length === 0) {
    return <EmptyState title="No LedgArr legal entities are configured yet." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Entity</th>
            <th>Code</th>
            <th>Type</th>
            <th>Currency</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {entities.map((entity) => (
            <tr key={entity.id}>
              <td className="font-semibold text-slate-50">{entity.displayName}</td>
              <td>{entity.entityCode}</td>
              <td>{titleize(entity.entityType)}</td>
              <td>{entity.baseCurrencyCode}</td>
              <td>
                <StatusPill status={entity.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function PaymentRunTable({ runs }: { runs: PaymentRunSummary[] }) {
  if (runs.length === 0) {
    return <EmptyState title="No vendor payment runs have been created yet." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Payment run</th>
            <th>Status</th>
            <th>Total</th>
            <th>Latest export</th>
          </tr>
        </thead>
        <tbody>
          {runs.map((run) => (
            <tr key={run.id}>
              <td className="font-semibold text-slate-50">{run.paymentRunNumber}</td>
              <td><StatusPill status={run.status} /></td>
              <td>{formatMoney(run.totalAmount)}</td>
              <td>{run.latestExportedAt ? `${titleize(run.latestExportStatus ?? 'unknown')} · ${formatDate(run.latestExportedAt)}` : 'Not exported'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function CustomerPaymentTable({ payments }: { payments: CustomerPaymentSummary[] }) {
  if (payments.length === 0) {
    return <EmptyState title="No customer payments have been recorded yet." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Payment</th>
            <th>Customer ref</th>
            <th>Amount</th>
            <th>Applied</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {payments.map((payment) => (
            <tr key={payment.id}>
              <td className="font-semibold text-slate-50">{payment.paymentNumber}</td>
              <td>{payment.customerRefId}</td>
              <td>{formatMoney(payment.amount)}</td>
              <td>{formatMoney(payment.appliedAmount)}</td>
              <td><StatusPill status={payment.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function BankAccountTable({ accounts }: { accounts: BankAccountSummary[] }) {
  if (accounts.length === 0) {
    return <EmptyState title="No bank accounts are configured in LedgArr yet." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Account</th>
            <th>Bank</th>
            <th>Entity</th>
            <th>Cash account</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {accounts.map((account) => (
            <tr key={account.id}>
              <td>
                <div className="font-semibold text-slate-50">{account.accountDisplayName}</div>
                <div className="text-xs text-slate-400">{account.maskedAccountNumber} · {titleize(account.accountType)}</div>
              </td>
              <td>{account.bankName}</td>
              <td>{account.financialLegalEntityDisplayName}</td>
              <td>{account.glCashAccountCode}</td>
              <td><StatusPill status={account.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function BankTransactionTable({ transactions }: { transactions: BankTransactionSummary[] }) {
  if (transactions.length === 0) {
    return <EmptyState title="No bank transactions have been imported or recorded yet." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Date</th>
            <th>Account</th>
            <th>Description</th>
            <th>Amount</th>
            <th>Match</th>
            <th>Reconciliation</th>
          </tr>
        </thead>
        <tbody>
          {transactions.map((transaction) => (
            <tr key={transaction.id}>
              <td>{formatDateOnly(transaction.transactionDate)}</td>
              <td>{transaction.bankAccountDisplayName}</td>
              <td>
                <div className="font-semibold text-slate-50">{transaction.description}</div>
                <div className="text-xs text-slate-400">{titleize(transaction.sourceType)} · {titleize(transaction.direction)}</div>
              </td>
              <td>{formatMoney(transaction.amount)}</td>
              <td><StatusPill status={transaction.matchStatus} /></td>
              <td><StatusPill status={transaction.reconciliationStatus} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function BankReconciliationTable({ reconciliations }: { reconciliations: BankReconciliationSummary[] }) {
  if (reconciliations.length === 0) {
    return <EmptyState title="No bank reconciliations have been created yet." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Account</th>
            <th>Statement date</th>
            <th>Beginning</th>
            <th>Ending</th>
            <th>Exceptions</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {reconciliations.map((reconciliation) => (
            <tr key={reconciliation.id}>
              <td className="font-semibold text-slate-50">{reconciliation.bankAccountDisplayName}</td>
              <td>{formatDateOnly(reconciliation.statementDate)}</td>
              <td>{formatMoney(reconciliation.beginningBalance)}</td>
              <td>{formatMoney(reconciliation.endingBalance)}</td>
              <td>{reconciliation.exceptionCount}</td>
              <td>
                <div className="flex flex-wrap gap-2">
                  <StatusPill status={reconciliation.approvalStatus} />
                  <StatusPill status={reconciliation.lockStatus} />
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function BudgetTable({ budgets }: { budgets: BudgetSummary[] }) {
  if (budgets.length === 0) {
    return <EmptyState title="No LedgArr budgets have been created yet." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Budget</th>
            <th>Name</th>
            <th>Lines</th>
            <th>Total budget</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {budgets.map((budget) => (
            <tr key={budget.id}>
              <td className="font-semibold text-slate-50">{budget.budgetNumber}</td>
              <td>{budget.name}</td>
              <td>{budget.lineCount}</td>
              <td>{formatMoney(budget.totalBudgetAmount)}</td>
              <td><StatusPill status={budget.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function FixedAssetTable({ assets }: { assets: FixedAssetSummary[] }) {
  if (assets.length === 0) {
    return <EmptyState title="No LedgArr fixed asset accounting records exist yet." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Asset</th>
            <th>Class</th>
            <th>Capitalized</th>
            <th>Book value</th>
            <th>Next depreciation</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {assets.map((asset) => (
            <tr key={asset.id}>
              <td>
                <div className="font-semibold text-slate-50">{asset.assetNumber}</div>
                <div className="text-xs text-slate-400">MaintainArr ref {asset.maintainArrAssetRefId}</div>
              </td>
              <td>{titleize(asset.assetClass)}</td>
              <td>{formatMoney(asset.capitalizedCost)}</td>
              <td>{formatMoney(asset.bookValue)}</td>
              <td>{asset.nextDepreciationDate ? `${formatDateOnly(asset.nextDepreciationDate)} (${asset.remainingScheduleCount} left)` : 'Schedule complete'}</td>
              <td><StatusPill status={asset.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function TaxCodeTable({ codes }: { codes: TaxCode[] }) {
  if (codes.length === 0) {
    return <EmptyState title="No tax codes are configured in LedgArr yet." detail="Tax accounting can still exist as financial policy, but code-level setup has not been created for this tenant." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Tax code</th>
            <th>Display name</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {codes.map((code) => (
            <tr key={code.id}>
              <td className="font-semibold text-slate-50">{code.taxCodeKey}</td>
              <td>{code.displayName}</td>
              <td><StatusPill status={code.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function TaxAdjustmentTable({ adjustments }: { adjustments: TaxAdjustmentSummary[] }) {
  if (adjustments.length === 0) {
    return (
      <EmptyState
        title="No tax adjustments have been recorded yet."
        detail="Create a LedgArr tax adjustment when finance needs to correct or recognize tax liability outside the normal source-event flow."
      />
    )
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Adjustment</th>
            <th>Entity</th>
            <th>Tax code</th>
            <th>Date</th>
            <th>Amount</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {adjustments.map((adjustment) => (
            <tr key={adjustment.id}>
              <td>
                <div className="font-semibold text-slate-50">{adjustment.adjustmentNumber}</div>
                <div className="text-xs text-slate-400">{adjustment.reason}</div>
              </td>
              <td>{adjustment.financialLegalEntityDisplayName}</td>
              <td>
                <div className="font-medium text-slate-100">{adjustment.taxCodeKey}</div>
                <div className="text-xs text-slate-400">{adjustment.taxCodeDisplayName}</div>
              </td>
              <td>{formatDateOnly(adjustment.adjustmentDate)}</td>
              <td>{formatMoney(adjustment.amount)}</td>
              <td><StatusPill status={adjustment.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function TaxLiabilitySummaryTable({ summaries }: { summaries: TaxLiabilitySummary[] }) {
  if (summaries.length === 0) {
    return <EmptyState title="No tax liability summary is available yet." detail="Tax liability summary will appear as LedgArr tax adjustments and postings accumulate." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Entity</th>
            <th>Tax code</th>
            <th>Currency</th>
            <th>Liability</th>
            <th>Adjustments</th>
            <th>Latest activity</th>
          </tr>
        </thead>
        <tbody>
          {summaries.map((summary) => (
            <tr key={`${summary.financialLegalEntityId}-${summary.taxCodeId}-${summary.currencyCode}`}>
              <td>{summary.financialLegalEntityDisplayName}</td>
              <td>
                <div className="font-medium text-slate-100">{summary.taxCodeKey}</div>
                <div className="text-xs text-slate-400">{summary.taxCodeDisplayName}</div>
              </td>
              <td>{summary.currencyCode}</td>
              <td>{formatMoney(summary.liabilityAmount)}</td>
              <td>{summary.adjustmentCount}</td>
              <td>{formatDateOnly(summary.latestAdjustmentDate)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function RelationshipTable({ relationships }: { relationships: FinancialLegalEntityRelationshipSummary[] }) {
  if (relationships.length === 0) {
    return <EmptyState title="No intercompany legal-entity relationships have been configured." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Parent</th>
            <th>Child</th>
            <th>Relationship</th>
            <th>Ownership</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {relationships.map((relationship) => (
            <tr key={relationship.id}>
              <td className="font-semibold text-slate-50">{relationship.parentDisplayName}</td>
              <td>{relationship.childDisplayName}</td>
              <td>{titleize(relationship.relationshipType)}</td>
              <td>{(relationship.ownershipPercentage * 100).toFixed(0)}%</td>
              <td><StatusPill status={relationship.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function IntercompanyTransactionTable({
  transactions,
  onSelectSettle,
  settlingTransactionId,
}: {
  transactions: IntercompanyTransactionSummary[]
  onSelectSettle: (transaction: IntercompanyTransactionSummary) => void
  settlingTransactionId?: string | null
}) {
  if (transactions.length === 0) {
    return <EmptyState title="No intercompany transactions have been recorded yet." detail="Create due-to / due-from activity here once LedgArr entity relationships are established." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Transaction</th>
            <th>From</th>
            <th>To</th>
            <th>Date</th>
            <th>Amount</th>
            <th>Settlement</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          {transactions.map((transaction) => (
            <tr key={transaction.id}>
              <td>
                <div className="font-semibold text-slate-50">{transaction.transactionNumber}</div>
                <div className="text-xs text-slate-400">{transaction.description}</div>
              </td>
              <td>{transaction.fromFinancialLegalEntityDisplayName}</td>
              <td>{transaction.toFinancialLegalEntityDisplayName}</td>
              <td>
                <div>{formatDateOnly(transaction.transactionDate)}</div>
                <div className="text-xs text-slate-400">{transaction.dueDate ? `Due ${formatDateOnly(transaction.dueDate)}` : 'No due date'}</div>
              </td>
              <td>{formatMoney(transaction.amount)}</td>
              <td><StatusPill status={transaction.settlementStatus} /></td>
              <td>
                {transaction.settlementStatus !== 'settled' ? (
                  <button
                    type="button"
                    onClick={() => onSelectSettle(transaction)}
                    className={`rounded-full border px-3 py-1 text-xs font-medium transition ${
                      settlingTransactionId === transaction.id
                        ? 'border-teal-400/50 bg-teal-400/12 text-teal-100'
                        : 'border-slate-700 bg-slate-900/70 text-slate-300 hover:border-slate-600 hover:text-slate-100'
                    }`}
                  >
                    Settle
                  </button>
                ) : (
                  <span className="text-xs text-slate-500">Settled</span>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function IntercompanyBalanceTable({ balances }: { balances: IntercompanyBalanceSummary[] }) {
  if (balances.length === 0) {
    return <EmptyState title="No open due to / due from balances are outstanding." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>From entity</th>
            <th>To entity</th>
            <th>Currency</th>
            <th>Open amount</th>
            <th>Transactions</th>
            <th>Window</th>
          </tr>
        </thead>
        <tbody>
          {balances.map((balance) => (
            <tr key={`${balance.fromFinancialLegalEntityId}-${balance.toFinancialLegalEntityId}-${balance.currencyCode}`}>
              <td>{balance.fromFinancialLegalEntityDisplayName}</td>
              <td>{balance.toFinancialLegalEntityDisplayName}</td>
              <td>{balance.currencyCode}</td>
              <td>{formatMoney(balance.openAmount)}</td>
              <td>{balance.openTransactionCount}</td>
              <td>
                <div>{formatDateOnly(balance.oldestTransactionDate)}</div>
                <div className="text-xs text-slate-400">through {formatDateOnly(balance.latestTransactionDate)}</div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function ExternalFinanceSystemTable({ systems }: { systems: ExternalFinanceSystem[] }) {
  if (systems.length === 0) {
    return <EmptyState title="No external finance systems are configured for this tenant." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>System</th>
            <th>Key</th>
            <th>Mode</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {systems.map((system) => (
            <tr key={system.id}>
              <td className="font-semibold text-slate-50">{system.displayName}</td>
              <td>{system.systemKey}</td>
              <td>{titleize(system.mode)}</td>
              <td><StatusPill status={system.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function ExternalPostingBatchTable({ batches }: { batches: ExternalPostingBatchSummary[] }) {
  if (batches.length === 0) {
    return <EmptyState title="No external posting batches have been created yet." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Batch</th>
            <th>System</th>
            <th>Journals</th>
            <th>Created</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {batches.map((batch) => (
            <tr key={batch.id}>
              <td className="font-semibold text-slate-50">{batch.batchNumber}</td>
              <td>{batch.externalFinanceSystemDisplayName}</td>
              <td>{batch.journalCount}</td>
              <td>{formatDate(batch.createdAt)}</td>
              <td><StatusPill status={batch.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function FiscalPeriodTable({ periods }: { periods: FiscalPeriod[] }) {
  if (periods.length === 0) {
    return <EmptyState title="No fiscal periods are available." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Period</th>
            <th>Name</th>
            <th>Start</th>
            <th>End</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {periods.map((period) => (
            <tr key={period.id}>
              <td className="font-semibold text-slate-50">{period.periodKey}</td>
              <td>{period.name}</td>
              <td>{formatDateOnly(period.startDate)}</td>
              <td>{formatDateOnly(period.endDate)}</td>
              <td>
                <StatusPill status={period.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

type PeriodControlAction = 'close' | 'reopen' | 'lock'

function getAvailablePeriodActions(period: FiscalPeriod): PeriodControlAction[] {
  switch (period.status.toLowerCase()) {
    case 'open':
      return ['close']
    case 'closed':
      return ['reopen', 'lock']
    case 'locked':
      return ['reopen']
    default:
      return []
  }
}

function getPeriodActionLabel(action: PeriodControlAction): string {
  switch (action) {
    case 'close':
      return 'Soft close'
    case 'reopen':
      return 'Reopen'
    case 'lock':
      return 'Hard lock'
  }
}

function FiscalPeriodControlTable({
  periods,
  selectedPeriodId,
  selectedAction,
  onSelectAction,
}: {
  periods: FiscalPeriod[]
  selectedPeriodId: string | null
  selectedAction: PeriodControlAction | null
  onSelectAction: (periodId: string, action: PeriodControlAction) => void
}) {
  if (periods.length === 0) {
    return <EmptyState title="No fiscal periods are available." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Period</th>
            <th>Name</th>
            <th>Start</th>
            <th>End</th>
            <th>Status</th>
            <th>Controls</th>
          </tr>
        </thead>
        <tbody>
          {periods.map((period) => {
            const availableActions = getAvailablePeriodActions(period)
            const selected = selectedPeriodId === period.id

            return (
              <tr key={period.id} className={selected ? 'bg-slate-900/50' : undefined}>
                <td className="font-semibold text-slate-50">{period.periodKey}</td>
                <td>{period.name}</td>
                <td>{formatDateOnly(period.startDate)}</td>
                <td>{formatDateOnly(period.endDate)}</td>
                <td>
                  <StatusPill status={period.status} />
                </td>
                <td>
                  {availableActions.length > 0 ? (
                    <div className="flex flex-wrap gap-2">
                      {availableActions.map((action) => {
                        const isActive = selected && selectedAction === action

                        return (
                          <button
                            key={`${period.id}-${action}`}
                            type="button"
                            onClick={() => onSelectAction(period.id, action)}
                            className={`rounded-full border px-3 py-1 text-xs font-medium transition ${
                              isActive
                                ? 'border-teal-400/50 bg-teal-400/12 text-teal-100'
                                : 'border-slate-700 bg-slate-900/70 text-slate-300 hover:border-slate-600 hover:text-slate-100'
                            }`}
                          >
                            {getPeriodActionLabel(action)}
                          </button>
                        )
                      })}
                    </div>
                  ) : (
                    <span className="text-xs text-slate-500">No manual controls available</span>
                  )}
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}

function GLAccountTable({ accounts }: { accounts: GLAccount[] }) {
  if (accounts.length === 0) {
    return <EmptyState title="No chart accounts are available." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Account</th>
            <th>Name</th>
            <th>Type</th>
            <th>Normal balance</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {accounts.map((account) => (
            <tr key={account.id}>
              <td className="font-semibold text-slate-50">{account.accountCode}</td>
              <td>{account.name}</td>
              <td>{titleize(account.accountType)}</td>
              <td>{titleize(account.normalBalance)}</td>
              <td>
                <StatusPill status={account.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function TrialBalanceTable({ rows }: { rows: TrialBalanceRow[] }) {
  if (rows.length === 0) {
    return <EmptyState title="No posted ledger balances are available." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Account</th>
            <th>Name</th>
            <th>Debits</th>
            <th>Credits</th>
            <th>Balance</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.accountCode}>
              <td className="font-semibold text-slate-50">{row.accountCode}</td>
              <td>{row.accountName}</td>
              <td>{formatMoney(row.debits)}</td>
              <td>{formatMoney(row.credits)}</td>
              <td>{formatMoney(row.balance)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function SummaryReportCard({
  title,
  summary,
  unavailableMessage,
}: {
  title: string
  summary?: ReportSummaryResponse
  unavailableMessage?: string
}) {
  return (
    <div className="ledgarr-panel">
      <div className="ledgarr-panel-inner">
        <p className="ledgarr-label">{title}</p>
        {summary ? (
          <>
            <p className="mt-2 text-lg font-semibold text-slate-50">
              {formatMoney(summary.totalDebits)} / {formatMoney(summary.totalCredits)}
            </p>
            <p className="mt-2 text-sm text-slate-300">
              Generated {formatDate(summary.generatedAt)} with {summary.mappingExceptionCount} mapping exceptions.
            </p>
          </>
        ) : (
          <p className="mt-2 text-sm text-slate-400">{unavailableMessage ?? 'This summary is not available yet.'}</p>
        )}
      </div>
    </div>
  )
}

function DashboardPage({ accessToken }: { accessToken: string }) {
  const dashboardQuery = useQuery({
    queryKey: ['ledgarr', 'dashboard', accessToken],
    queryFn: () => getDashboard(accessToken),
    enabled: Boolean(accessToken),
  })
  const packetsQuery = useQuery({
    queryKey: ['ledgarr', 'packets', accessToken, 'dashboard'],
    queryFn: () => listPackets(accessToken),
    enabled: Boolean(accessToken),
  })

  const dashboard = dashboardQuery.data
  const packets = packetsQuery.data ?? []
  const attentionItems =
    dashboard
      ? [
          dashboard.openPacketCount > 0
            ? {
                title: 'Posting queue needs review',
                detail: `${dashboard.openPacketCount} source packets still need mapping, validation, approval, or posting.`,
                severity: 'review',
              }
            : null,
          dashboard.openApAmount > 0
            ? {
                title: 'Open payables require action',
                detail: `${formatMoney(dashboard.openApAmount)} remains in unpaid vendor obligations.`,
                severity: 'watch',
              }
            : null,
          dashboard.openArAmount > 0
            ? {
                title: 'Open receivables remain outstanding',
                detail: `${formatMoney(dashboard.openArAmount)} remains in customer receivables exposure.`,
                severity: 'watch',
              }
            : null,
        ].filter((item): item is { title: string; detail: string; severity: string } => item !== null)
      : []

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="LedgArr"
        title="Financial control center"
        description="Cash, obligations, posting readiness, period control, source exceptions, and audit activity for LedgArr-owned financial truth."
        action={dashboard ? <span className="ledgarr-pill">Updated {formatDate(dashboard.generatedAt)}</span> : null}
      />
      {dashboardQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load dashboard"
          message={getErrorMessage(dashboardQuery.error, 'Failed to load LedgArr dashboard.')}
        />
      ) : null}
      {dashboard ? (
        <>
          <div className="ledgarr-grid cols-4">
            <Metric label="Legal entities" value={dashboard.financialLegalEntityCount} hint="LedgArr-owned financial reporting entities" />
            <Metric label="Open AR" value={formatMoney(dashboard.openArAmount)} hint="Customer receivables awaiting settlement" />
            <Metric label="Open AP" value={formatMoney(dashboard.openApAmount)} hint="Vendor obligations awaiting payment" />
            <Metric label="Unposted queue" value={dashboard.openPacketCount} hint="Packets still awaiting posting control" />
            <Metric label="Posted journals" value={dashboard.postedJournalCount} hint="Immutable ledger entries already posted" />
            <Metric label="Open periods" value={dashboard.openPeriodCount} hint="Periods currently accepting normal activity" />
            <Metric label="Closed periods" value={dashboard.closedPeriodCount} hint="Periods protected by close controls" />
            <Metric label="Posted debit volume" value={formatMoney(dashboard.postedDebitVolume)} hint="Current posted debit volume in LedgArr" />
          </div>
          <div className="ledgarr-grid cols-2">
            <Panel title="Attention required" icon={<AlertTriangle className="h-4 w-4 text-amber-300" />}>
              <AttentionList items={attentionItems} />
            </Panel>
            <Panel title="Source exceptions" icon={<ArrowRightLeft className="h-4 w-4 text-teal-300" />}>
              {packets.length === 0 ? (
                <EmptyState title="No cross-product financial packets are waiting in the dashboard queue." />
              ) : (
                <div className="space-y-3">
                  {packets.slice(0, 5).map((packet) => (
                    <div key={packet.id} className="rounded-lg border border-slate-700/70 bg-slate-900/70 p-3">
                      <div className="flex flex-wrap items-center gap-2">
                        <SourceProductBadge productKey={packet.sourceProductKey} />
                        <StatusPill status={packet.status} />
                      </div>
                      <p className="mt-2 text-sm font-medium text-slate-100">{packet.sourceRecordDisplayName}</p>
                      <p className="mt-1 text-sm text-slate-300">
                        {titleize(packet.packetType)} for {formatMoney(packet.sourceTotalAmount)} on {formatDateOnly(packet.accountingDate)}.
                      </p>
                    </div>
                  ))}
                </div>
              )}
            </Panel>
          </div>
          <Panel title="Recent financial audit activity" icon={<Landmark className="h-4 w-4 text-teal-300" />}>
            <div className="space-y-3">
              {dashboard.recentActivity.map((item) => (
                <div key={item.activityId} className="rounded-md border border-slate-700/70 bg-slate-900/70 p-3">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <strong className="text-sm text-slate-50">{titleize(item.action)}</strong>
                    <StatusPill status={item.targetType} />
                  </div>
                  <p className="mt-2 text-sm text-slate-300">{item.summary}</p>
                  <p className="mt-2 text-xs text-slate-400">{formatDate(item.occurredAt)}</p>
                </div>
              ))}
              {dashboard.recentActivity.length === 0 ? (
                <EmptyState title="No LedgArr audit activity has been recorded." />
              ) : null}
            </div>
          </Panel>
          <ScopeNote>
            LedgArr owns legal entities, journals, subledgers, reporting, and close controls. Customer, vendor,
            document, people, warehouse, maintenance, transportation, and order records remain owned by their source
            products and are surfaced here as references only.
          </ScopeNote>
        </>
      ) : (
        <EmptyState title="No LedgArr dashboard response is available." />
      )}
    </div>
  )
}

function GeneralLedgerPage({ accessToken }: { accessToken: string }) {
  const [activeTab, setActiveTab] = useState('Journal Entries')
  const journalsQuery = useQuery({
    queryKey: ['ledgarr', 'journals', accessToken],
    queryFn: () => listJournals(accessToken),
    enabled: Boolean(accessToken),
  })
  const accountsQuery = useQuery({
    queryKey: ['ledgarr', 'gl-accounts', accessToken],
    queryFn: () => listGLAccounts(accessToken),
    enabled: Boolean(accessToken),
  })
  const periodsQuery = useQuery({
    queryKey: ['ledgarr', 'fiscal-periods', accessToken],
    queryFn: () => listFiscalPeriods(accessToken),
    enabled: Boolean(accessToken),
  })
  const packetsQuery = useQuery({
    queryKey: ['ledgarr', 'packets', accessToken, 'gl'],
    queryFn: () => listPackets(accessToken),
    enabled: Boolean(accessToken),
  })

  const journals = (journalsQuery.data ?? []).map((item) => item.journal)
  const tabs = ['Journal Entries', 'Chart Accounts', 'Fiscal Periods', 'Posting Queue'] as const

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="General ledger"
        title="Posting core and accounting structure"
        description="Manual journals, chart accounts, period control, and source posting queues stay inside LedgArr because they define financial truth."
      />
      <TabStrip tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />
      {journalsQuery.isError || accountsQuery.isError || periodsQuery.isError || packetsQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load general ledger data"
          message={
            getErrorMessage(journalsQuery.error, '') ||
            getErrorMessage(accountsQuery.error, '') ||
            getErrorMessage(periodsQuery.error, '') ||
            getErrorMessage(packetsQuery.error, 'Failed to load LedgArr general ledger data.')
          }
        />
      ) : null}
      {activeTab === 'Journal Entries' ? <JournalTable journals={journals} /> : null}
      {activeTab === 'Chart Accounts' ? <GLAccountTable accounts={accountsQuery.data ?? []} /> : null}
      {activeTab === 'Fiscal Periods' ? <FiscalPeriodTable periods={periodsQuery.data ?? []} /> : null}
      {activeTab === 'Posting Queue' ? <PacketTable packets={packetsQuery.data ?? []} /> : null}
      <ScopeNote>
        Period locks, journal balance rules, and chart-of-accounts structure are LedgArr-owned controls. Source
        operational products may contribute packets, but they do not post directly into the ledger.
      </ScopeNote>
    </div>
  )
}

function PayablesPage({ accessToken }: { accessToken: string }) {
  const [activeTab, setActiveTab] = useState('Vendor Bills')
  const billsQuery = useQuery({
    queryKey: ['ledgarr', 'ap', accessToken],
    queryFn: () => listVendorBills(accessToken),
    enabled: Boolean(accessToken),
  })
  const agingQuery = useQuery({
    queryKey: ['ledgarr', 'ap-aging', accessToken],
    queryFn: () => getApAging(accessToken),
    enabled: Boolean(accessToken),
  })
  const packetsQuery = useQuery({
    queryKey: ['ledgarr', 'packets', accessToken, 'ap'],
    queryFn: () => listPackets(accessToken),
    enabled: Boolean(accessToken),
  })

  const exceptions = (packetsQuery.data ?? []).filter((packet) =>
    ['supplyarr', 'loadarr', 'routarr', 'maintainarr'].includes(packet.sourceProductKey.toLowerCase()),
  )
  const tabs = ['Vendor Bills', 'Aging', 'Source Exceptions'] as const

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Payables"
        title="Vendor obligations and payment control"
        description="SupplyArr owns vendor truth and procurement context. LedgArr owns the payable obligation, matching state, posting, and payment control."
      />
      <TabStrip tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />
      {billsQuery.isError || agingQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load payables data"
          message={
            getErrorMessage(billsQuery.error, '') ||
            getErrorMessage(agingQuery.error, 'Failed to load LedgArr payables data.')
          }
        />
      ) : null}
      {activeTab === 'Vendor Bills' ? <VendorBillTable bills={billsQuery.data ?? []} /> : null}
      {activeTab === 'Aging' ? <AgingCard title="AP aging" buckets={agingQuery.data ?? []} /> : null}
      {activeTab === 'Source Exceptions' ? <PacketTable packets={exceptions} /> : null}
      <ScopeNote>
        Vendors, POs, receipts, and carrier operations stay in their owning products. LedgArr stores only the payable,
        its source references, and the financial correction workflow.
      </ScopeNote>
    </div>
  )
}

function ReceivablesPage({ accessToken }: { accessToken: string }) {
  const [activeTab, setActiveTab] = useState('Customer Invoices')
  const invoicesQuery = useQuery({
    queryKey: ['ledgarr', 'ar', accessToken],
    queryFn: () => listCustomerInvoices(accessToken),
    enabled: Boolean(accessToken),
  })
  const agingQuery = useQuery({
    queryKey: ['ledgarr', 'ar-aging', accessToken],
    queryFn: () => getArAging(accessToken),
    enabled: Boolean(accessToken),
  })
  const packetsQuery = useQuery({
    queryKey: ['ledgarr', 'packets', accessToken, 'ar'],
    queryFn: () => listPackets(accessToken),
    enabled: Boolean(accessToken),
  })

  const billingFeed = (packetsQuery.data ?? []).filter((packet) =>
    ['ordarr', 'customarr', 'routarr', 'loadarr', 'maintainarr', 'assurarr'].includes(
      packet.sourceProductKey.toLowerCase(),
    ),
  )
  const tabs = ['Customer Invoices', 'Aging', 'Invoice Feed'] as const

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Receivables"
        title="Customer invoice and collection control"
        description="CustomArr owns customer identity and OrdArr or source products own operational commercial context. LedgArr owns the invoice, AR balance, posting, and payment application."
      />
      <TabStrip tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />
      {invoicesQuery.isError || agingQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load receivables data"
          message={
            getErrorMessage(invoicesQuery.error, '') ||
            getErrorMessage(agingQuery.error, 'Failed to load LedgArr receivables data.')
          }
        />
      ) : null}
      {activeTab === 'Customer Invoices' ? <CustomerInvoiceTable invoices={invoicesQuery.data ?? []} /> : null}
      {activeTab === 'Aging' ? <AgingCard title="AR aging" buckets={agingQuery.data ?? []} /> : null}
      {activeTab === 'Invoice Feed' ? <PacketTable packets={billingFeed} /> : null}
      <ScopeNote>
        LedgArr never becomes the customer master. It carries customer references, invoice snapshots, and posted
        receivable consequences only.
      </ScopeNote>
    </div>
  )
}

function BillingPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const [activeTab, setActiveTab] = useState('Billing Queue')
  const [selectedPacketId, setSelectedPacketId] = useState<string | null>(null)
  const [selectedBillableEventId, setSelectedBillableEventId] = useState<string | null>(null)
  const [holdReason, setHoldReason] = useState('')
  const packetsQuery = useQuery({
    queryKey: ['ledgarr', 'packets', accessToken, 'billing'],
    queryFn: () => listPackets(accessToken),
    enabled: Boolean(accessToken),
  })
  const billableEventsQuery = useQuery({
    queryKey: ['ledgarr', 'billable-events', accessToken],
    queryFn: () => listBillableEvents(accessToken),
    enabled: Boolean(accessToken),
  })
  const invoicesQuery = useQuery({
    queryKey: ['ledgarr', 'ar', accessToken, 'billing'],
    queryFn: () => listCustomerInvoices(accessToken),
    enabled: Boolean(accessToken),
  })
  const createBillableEventMutation = useMutation({
    mutationFn: async (packetId: string) => createBillableEventFromPacket(accessToken, packetId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['ledgarr', 'billable-events', accessToken] })
    },
  })
  const approveBillableEventMutation = useMutation({
    mutationFn: async (billableEventId: string) => approveBillableEvent(accessToken, billableEventId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['ledgarr', 'billable-events', accessToken] })
    },
  })
  const holdBillableEventMutation = useMutation({
    mutationFn: async ({ billableEventId, reason }: { billableEventId: string; reason: string }) =>
      holdBillableEvent(accessToken, billableEventId, { reason }),
    onSuccess: async () => {
      setHoldReason('')
      setSelectedBillableEventId(null)
      await queryClient.invalidateQueries({ queryKey: ['ledgarr', 'billable-events', accessToken] })
    },
  })
  const generateDraftMutation = useMutation({
    mutationFn: async (billableEventId: string) => generateBillableEventInvoiceDraft(accessToken, billableEventId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['ledgarr', 'billable-events', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['ledgarr', 'ar', accessToken, 'billing'] }),
      ])
    },
  })

  const billablePackets = (packetsQuery.data ?? []).filter((packet) =>
    ['ordarr', 'customarr', 'routarr', 'loadarr', 'maintainarr', 'assurarr'].includes(
      packet.sourceProductKey.toLowerCase(),
    ),
  )
  const draftReadyCount = (billableEventsQuery.data ?? []).filter((billableEvent) => billableEvent.invoiceStatus === 'draft_generated').length
  const tabs = ['Billing Queue', 'Billable Events', 'Draft Signals'] as const

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Billing"
        title="Billable event intake"
        description="Billing remains separate from AR because source products contribute invoice-ready and charge-ready events that LedgArr must review before financialization."
      />
      <TabStrip tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />
      <div className="ledgarr-grid cols-3">
        <Metric label="Billable source events" value={billablePackets.length} hint="Operational packets awaiting review or invoice treatment" />
        <Metric label="LedgArr billable events" value={billableEventsQuery.data?.length ?? 0} hint="Billing-review records now owned by LedgArr" />
        <Metric label="Draft invoice signals" value={draftReadyCount} hint="Approved billable events marked ready for invoice draft generation" />
        <Metric label="Owning products" value="OrdArr / RoutArr / LoadArr" hint="LedgArr consumes source references instead of owning execution" />
      </div>
      {packetsQuery.isError || billableEventsQuery.isError || invoicesQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load billing workflow"
          message={
            getErrorMessage(packetsQuery.error, '') ||
            getErrorMessage(billableEventsQuery.error, '') ||
            getErrorMessage(invoicesQuery.error, 'Failed to load LedgArr billing workflow.')
          }
        />
      ) : null}
      {activeTab === 'Billing Queue' ? (
        <div className="ledgarr-grid cols-2">
          <Panel title="Operational billing intake" icon={<Receipt className="h-4 w-4 text-teal-300" />}>
            <PacketTable packets={billablePackets} />
          </Panel>
          <Panel title="Create LedgArr billable event" icon={<BadgeDollarSign className="h-4 w-4 text-teal-300" />}>
            <div className="space-y-4">
              <label className="block space-y-2">
                <span className="ledgarr-label">Source packet</span>
                <select
                  value={selectedPacketId ?? ''}
                  onChange={(event) => setSelectedPacketId(event.target.value || null)}
                  className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60"
                >
                  <option value="">Select billing packet</option>
                  {billablePackets.map((packet) => (
                    <option key={packet.id} value={packet.id}>
                      {packet.sourceRecordDisplayName} · {titleize(packet.packetType)} · {formatMoney(packet.sourceTotalAmount)}
                    </option>
                  ))}
                </select>
              </label>
              {createBillableEventMutation.isError ? (
                <ApiErrorCallout
                  title="Unable to create billable event"
                  message={getErrorMessage(createBillableEventMutation.error, 'Failed to create LedgArr billable event.')}
                />
              ) : null}
              <button
                type="button"
                onClick={async () => {
                  if (!selectedPacketId) {
                    return
                  }
                  await createBillableEventMutation.mutateAsync(selectedPacketId)
                }}
                disabled={!selectedPacketId || createBillableEventMutation.isPending}
                className="rounded-full border border-teal-400/40 bg-teal-400/12 px-4 py-2 text-sm font-medium text-teal-100 transition hover:border-teal-300 hover:bg-teal-400/18 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900/60 disabled:text-slate-500"
              >
                {createBillableEventMutation.isPending ? 'Creating billable event...' : 'Create billable event from packet'}
              </button>
            </div>
          </Panel>
        </div>
      ) : null}
      {activeTab === 'Billable Events' ? (
        <div className="ledgarr-grid cols-2">
          <Panel title="Billable event workflow" icon={<BadgeDollarSign className="h-4 w-4 text-teal-300" />}>
            <BillableEventTable
              events={billableEventsQuery.data ?? []}
              onApprove={async (billableEventId) => {
                await approveBillableEventMutation.mutateAsync(billableEventId)
              }}
              onHold={(billableEventId) => setSelectedBillableEventId(billableEventId)}
              onGenerateDraft={async (billableEventId) => {
                await generateDraftMutation.mutateAsync(billableEventId)
              }}
            />
          </Panel>
          <Panel title="Hold selected event" icon={<AlertTriangle className="h-4 w-4 text-amber-300" />}>
            <div className="space-y-4">
              <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-4 text-sm text-slate-300">
                {selectedBillableEventId
                  ? ((billableEventsQuery.data ?? []).find((item) => item.id === selectedBillableEventId)?.eventNumber ?? 'Selected event')
                  : 'Select Hold on a billable event to capture a billing hold reason.'}
              </div>
              <textarea
                value={holdReason}
                onChange={(event) => setHoldReason(event.target.value)}
                rows={4}
                placeholder="Explain the billing hold, exception, or missing source condition."
                className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60"
              />
              {holdBillableEventMutation.isError || approveBillableEventMutation.isError || generateDraftMutation.isError ? (
                <ApiErrorCallout
                  title="Billing action failed"
                  message={
                    getErrorMessage(holdBillableEventMutation.error, '') ||
                    getErrorMessage(approveBillableEventMutation.error, '') ||
                    getErrorMessage(generateDraftMutation.error, 'Failed to update LedgArr billing workflow.')
                  }
                />
              ) : null}
              <button
                type="button"
                onClick={async () => {
                  if (!selectedBillableEventId || !holdReason.trim()) {
                    return
                  }
                  await holdBillableEventMutation.mutateAsync({ billableEventId: selectedBillableEventId, reason: holdReason.trim() })
                }}
                disabled={!selectedBillableEventId || !holdReason.trim() || holdBillableEventMutation.isPending}
                className="rounded-full border border-teal-400/40 bg-teal-400/12 px-4 py-2 text-sm font-medium text-teal-100 transition hover:border-teal-300 hover:bg-teal-400/18 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900/60 disabled:text-slate-500"
              >
                {holdBillableEventMutation.isPending ? 'Holding billable event...' : 'Apply hold reason'}
              </button>
            </div>
          </Panel>
        </div>
      ) : null}
      {activeTab === 'Draft Signals' ? (
        <Panel title="Invoice draft readiness" icon={<FileChartColumn className="h-4 w-4 text-teal-300" />}>
          <BillableEventTable
            events={(billableEventsQuery.data ?? []).filter((billableEvent) => billableEvent.invoiceStatus === 'draft_generated')}
            onApprove={() => undefined}
            onHold={() => undefined}
            onGenerateDraft={() => undefined}
          />
        </Panel>
      ) : null}
      <ScopeNote>
        Billable events still come from the source product with source badges, snapshots, and references. LedgArr now
        owns the billing review record and invoice-draft state without taking over the operational record itself.
      </ScopeNote>
    </div>
  )
}

function BankingPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const [activeTab, setActiveTab] = useState('Bank Accounts')
  const [accountEntityId, setAccountEntityId] = useState('')
  const [bankName, setBankName] = useState('')
  const [accountDisplayName, setAccountDisplayName] = useState('')
  const [accountType, setAccountType] = useState('checking')
  const [maskedAccountNumber, setMaskedAccountNumber] = useState('')
  const [currencyCode, setCurrencyCode] = useState('USD')
  const [glCashAccountId, setGlCashAccountId] = useState('')
  const [reconciliationEnabled, setReconciliationEnabled] = useState(true)
  const [transactionBankAccountId, setTransactionBankAccountId] = useState('')
  const [transactionDate, setTransactionDate] = useState(new Date().toISOString().slice(0, 10))
  const [transactionDescription, setTransactionDescription] = useState('')
  const [transactionAmount, setTransactionAmount] = useState('')
  const [transactionDirection, setTransactionDirection] = useState('debit')
  const [reconciliationBankAccountId, setReconciliationBankAccountId] = useState('')
  const [reconciliationStartDate, setReconciliationStartDate] = useState(new Date().toISOString().slice(0, 10))
  const [reconciliationEndDate, setReconciliationEndDate] = useState(new Date().toISOString().slice(0, 10))
  const [statementDate, setStatementDate] = useState(new Date().toISOString().slice(0, 10))
  const [beginningBalance, setBeginningBalance] = useState('')
  const [endingBalance, setEndingBalance] = useState('')
  const [adjustmentTotal, setAdjustmentTotal] = useState('0')
  const [selectedReconciliationTransactionIds, setSelectedReconciliationTransactionIds] = useState<string[]>([])
  const bankAccountsQuery = useQuery({
    queryKey: ['ledgarr', 'bank-accounts', accessToken],
    queryFn: () => listBankAccounts(accessToken),
    enabled: Boolean(accessToken),
  })
  const entitiesQuery = useQuery({
    queryKey: ['ledgarr', 'financial-legal-entities', accessToken, 'banking'],
    queryFn: () => listFinancialLegalEntities(accessToken),
    enabled: Boolean(accessToken),
  })
  const accountsQuery = useQuery({
    queryKey: ['ledgarr', 'gl-accounts', accessToken, 'banking'],
    queryFn: () => listGLAccounts(accessToken),
    enabled: Boolean(accessToken),
  })
  const bankTransactionsQuery = useQuery({
    queryKey: ['ledgarr', 'bank-transactions', accessToken],
    queryFn: () => listBankTransactions(accessToken),
    enabled: Boolean(accessToken),
  })
  const reconciliationsQuery = useQuery({
    queryKey: ['ledgarr', 'bank-reconciliations', accessToken],
    queryFn: () => listBankReconciliations(accessToken),
    enabled: Boolean(accessToken),
  })
  const paymentRunsQuery = useQuery({
    queryKey: ['ledgarr', 'payment-runs', accessToken],
    queryFn: () => listPaymentRuns(accessToken),
    enabled: Boolean(accessToken),
  })
  const customerPaymentsQuery = useQuery({
    queryKey: ['ledgarr', 'customer-payments', accessToken],
    queryFn: () => listCustomerPayments(accessToken),
    enabled: Boolean(accessToken),
  })
  const createBankAccountMutation = useMutation({
    mutationFn: async () =>
      createBankAccount(accessToken, {
        financialLegalEntityId: accountEntityId,
        bankName,
        accountDisplayName,
        accountType,
        maskedAccountNumber,
        currencyCode,
        glCashAccountId,
        reconciliationEnabled,
      }),
    onSuccess: async () => {
      setBankName('')
      setAccountDisplayName('')
      setMaskedAccountNumber('')
      await queryClient.invalidateQueries({ queryKey: ['ledgarr', 'bank-accounts', accessToken] })
    },
  })
  const createBankTransactionMutation = useMutation({
    mutationFn: async () =>
      createBankTransaction(accessToken, {
        bankAccountId: transactionBankAccountId,
        transactionDate,
        description: transactionDescription,
        amount: Number(transactionAmount),
        direction: transactionDirection,
        sourceType: 'manual',
        matchStatus: 'unmatched',
      }),
    onSuccess: async () => {
      setTransactionDescription('')
      setTransactionAmount('')
      await queryClient.invalidateQueries({ queryKey: ['ledgarr', 'bank-transactions', accessToken] })
    },
  })
  const createBankReconciliationMutation = useMutation({
    mutationFn: async () =>
      createBankReconciliation(accessToken, {
        bankAccountId: reconciliationBankAccountId,
        periodStartDate: reconciliationStartDate,
        periodEndDate: reconciliationEndDate,
        beginningBalance: Number(beginningBalance),
        endingBalance: Number(endingBalance),
        statementDate,
        adjustmentTotal: Number(adjustmentTotal),
        bankTransactionIds: selectedReconciliationTransactionIds,
      }),
    onSuccess: async () => {
      setBeginningBalance('')
      setEndingBalance('')
      setAdjustmentTotal('0')
      setSelectedReconciliationTransactionIds([])
      await queryClient.invalidateQueries({ queryKey: ['ledgarr', 'bank-reconciliations', accessToken] })
    },
  })

  useEffect(() => {
    if (!accountEntityId && entitiesQuery.data?.[0]?.id) {
      setAccountEntityId(entitiesQuery.data[0].id)
    }
  }, [accountEntityId, entitiesQuery.data])

  useEffect(() => {
    if (!glCashAccountId && accountsQuery.data?.[0]?.id) {
      setGlCashAccountId(accountsQuery.data[0].id)
    }
  }, [accountsQuery.data, glCashAccountId])

  useEffect(() => {
    if (!transactionBankAccountId && bankAccountsQuery.data?.[0]?.id) {
      setTransactionBankAccountId(bankAccountsQuery.data[0].id)
    }
    if (!reconciliationBankAccountId && bankAccountsQuery.data?.[0]?.id) {
      setReconciliationBankAccountId(bankAccountsQuery.data[0].id)
    }
  }, [bankAccountsQuery.data, reconciliationBankAccountId, transactionBankAccountId])

  const paymentRuns = paymentRunsQuery.data ?? []
  const customerPayments = customerPaymentsQuery.data ?? []
  const tabs = ['Bank Accounts', 'Bank Transactions', 'Reconciliations', 'Cash Activity'] as const
  const reconciliationCandidateTransactions = (bankTransactionsQuery.data ?? []).filter(
    (transaction) => transaction.bankAccountId === reconciliationBankAccountId,
  )

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Banking"
        title="Banking and reconciliation control"
        description="LedgArr now owns bank-account setup, imported or manual transaction visibility, reconciliation workflows, and the related payment activity feeding cash control."
      />
      <TabStrip tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />
      <div className="ledgarr-grid cols-3">
        <Metric label="Bank accounts" value={bankAccountsQuery.data?.length ?? 0} hint="LedgArr-owned bank account records" />
        <Metric label="Transactions" value={bankTransactionsQuery.data?.length ?? 0} hint="Imported or manual bank activity visible in LedgArr" />
        <Metric label="Reconciliations" value={reconciliationsQuery.data?.length ?? 0} hint="Statement reconciliation packages created in LedgArr" />
      </div>
      {bankAccountsQuery.isError || bankTransactionsQuery.isError || reconciliationsQuery.isError || paymentRunsQuery.isError || customerPaymentsQuery.isError || entitiesQuery.isError || accountsQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load banking activity"
          message={
            getErrorMessage(bankAccountsQuery.error, '') ||
            getErrorMessage(bankTransactionsQuery.error, '') ||
            getErrorMessage(reconciliationsQuery.error, '') ||
            getErrorMessage(paymentRunsQuery.error, '') ||
            getErrorMessage(customerPaymentsQuery.error, '') ||
            getErrorMessage(entitiesQuery.error, '') ||
            getErrorMessage(accountsQuery.error, 'Failed to load LedgArr banking activity.')
          }
        />
      ) : null}
      {activeTab === 'Bank Accounts' ? (
        <div className="ledgarr-grid cols-2">
          <Panel title="Create bank account" icon={<Landmark className="h-4 w-4 text-teal-300" />}>
            <form className="space-y-4" onSubmit={async (event) => {
              event.preventDefault()
              await createBankAccountMutation.mutateAsync()
            }}>
              <label className="block space-y-2">
                <span className="ledgarr-label">Financial legal entity</span>
                <select value={accountEntityId} onChange={(event) => setAccountEntityId(event.target.value)} className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60">
                  {(entitiesQuery.data ?? []).map((entity) => (
                    <option key={entity.id} value={entity.id}>{entity.entityCode} · {entity.displayName}</option>
                  ))}
                </select>
              </label>
              <div className="grid gap-4 sm:grid-cols-2">
                <input value={bankName} onChange={(event) => setBankName(event.target.value)} placeholder="Bank name" className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60" />
                <input value={accountDisplayName} onChange={(event) => setAccountDisplayName(event.target.value)} placeholder="Account display name" className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60" />
                <input value={maskedAccountNumber} onChange={(event) => setMaskedAccountNumber(event.target.value)} placeholder="Masked account number" className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60" />
                <input value={currencyCode} onChange={(event) => setCurrencyCode(event.target.value)} placeholder="Currency" className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60" />
              </div>
              <div className="grid gap-4 sm:grid-cols-2">
                <select value={accountType} onChange={(event) => setAccountType(event.target.value)} className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60">
                  <option value="checking">Checking</option>
                  <option value="savings">Savings</option>
                  <option value="credit">Credit</option>
                </select>
                <select value={glCashAccountId} onChange={(event) => setGlCashAccountId(event.target.value)} className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60">
                  {(accountsQuery.data ?? []).map((account) => (
                    <option key={account.id} value={account.id}>{account.accountCode} · {account.name}</option>
                  ))}
                </select>
              </div>
              <label className="flex items-center gap-2 text-sm text-slate-300">
                <input type="checkbox" checked={reconciliationEnabled} onChange={(event) => setReconciliationEnabled(event.target.checked)} />
                Reconciliation enabled
              </label>
              {createBankAccountMutation.isError ? <ApiErrorCallout title="Unable to create bank account" message={getErrorMessage(createBankAccountMutation.error, 'Failed to create bank account.')} /> : null}
              <button type="submit" disabled={!accountEntityId || !bankName.trim() || !accountDisplayName.trim() || !maskedAccountNumber.trim() || !glCashAccountId || createBankAccountMutation.isPending} className="rounded-full border border-teal-400/40 bg-teal-400/12 px-4 py-2 text-sm font-medium text-teal-100 transition hover:border-teal-300 hover:bg-teal-400/18 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900/60 disabled:text-slate-500">
                {createBankAccountMutation.isPending ? 'Creating bank account...' : 'Create bank account'}
              </button>
            </form>
          </Panel>
          <Panel title="Bank accounts" icon={<Landmark className="h-4 w-4 text-teal-300" />}>
            <BankAccountTable accounts={bankAccountsQuery.data ?? []} />
          </Panel>
        </div>
      ) : null}
      {activeTab === 'Bank Transactions' ? (
        <div className="ledgarr-grid cols-2">
          <Panel title="Create bank transaction" icon={<CircleDollarSign className="h-4 w-4 text-teal-300" />}>
            <form className="space-y-4" onSubmit={async (event) => {
              event.preventDefault()
              await createBankTransactionMutation.mutateAsync()
            }}>
              <select value={transactionBankAccountId} onChange={(event) => setTransactionBankAccountId(event.target.value)} className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60">
                {(bankAccountsQuery.data ?? []).map((account) => (
                  <option key={account.id} value={account.id}>{account.accountDisplayName} · {account.bankName}</option>
                ))}
              </select>
              <div className="grid gap-4 sm:grid-cols-2">
                <input type="date" value={transactionDate} onChange={(event) => setTransactionDate(event.target.value)} className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60" />
                <select value={transactionDirection} onChange={(event) => setTransactionDirection(event.target.value)} className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60">
                  <option value="debit">Debit</option>
                  <option value="credit">Credit</option>
                </select>
              </div>
              <input value={transactionDescription} onChange={(event) => setTransactionDescription(event.target.value)} placeholder="Transaction description" className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60" />
              <input type="number" step="0.01" value={transactionAmount} onChange={(event) => setTransactionAmount(event.target.value)} placeholder="Amount" className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60" />
              {createBankTransactionMutation.isError ? <ApiErrorCallout title="Unable to create bank transaction" message={getErrorMessage(createBankTransactionMutation.error, 'Failed to create bank transaction.')} /> : null}
              <button type="submit" disabled={!transactionBankAccountId || !transactionDescription.trim() || !transactionAmount || createBankTransactionMutation.isPending} className="rounded-full border border-teal-400/40 bg-teal-400/12 px-4 py-2 text-sm font-medium text-teal-100 transition hover:border-teal-300 hover:bg-teal-400/18 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900/60 disabled:text-slate-500">
                {createBankTransactionMutation.isPending ? 'Creating bank transaction...' : 'Create bank transaction'}
              </button>
            </form>
          </Panel>
          <Panel title="Bank transactions" icon={<CircleDollarSign className="h-4 w-4 text-teal-300" />}>
            <BankTransactionTable transactions={bankTransactionsQuery.data ?? []} />
          </Panel>
        </div>
      ) : null}
      {activeTab === 'Reconciliations' ? (
        <div className="ledgarr-grid cols-2">
          <Panel title="Create reconciliation" icon={<FileChartColumn className="h-4 w-4 text-teal-300" />}>
            <form className="space-y-4" onSubmit={async (event) => {
              event.preventDefault()
              await createBankReconciliationMutation.mutateAsync()
            }}>
              <select value={reconciliationBankAccountId} onChange={(event) => {
                setReconciliationBankAccountId(event.target.value)
                setSelectedReconciliationTransactionIds([])
              }} className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60">
                {(bankAccountsQuery.data ?? []).map((account) => (
                  <option key={account.id} value={account.id}>{account.accountDisplayName} · {account.bankName}</option>
                ))}
              </select>
              <div className="grid gap-4 sm:grid-cols-3">
                <input type="date" value={reconciliationStartDate} onChange={(event) => setReconciliationStartDate(event.target.value)} className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60" />
                <input type="date" value={reconciliationEndDate} onChange={(event) => setReconciliationEndDate(event.target.value)} className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60" />
                <input type="date" value={statementDate} onChange={(event) => setStatementDate(event.target.value)} className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60" />
              </div>
              <div className="grid gap-4 sm:grid-cols-3">
                <input type="number" step="0.01" value={beginningBalance} onChange={(event) => setBeginningBalance(event.target.value)} placeholder="Beginning balance" className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60" />
                <input type="number" step="0.01" value={endingBalance} onChange={(event) => setEndingBalance(event.target.value)} placeholder="Ending balance" className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60" />
                <input type="number" step="0.01" value={adjustmentTotal} onChange={(event) => setAdjustmentTotal(event.target.value)} placeholder="Adjustment total" className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60" />
              </div>
              <div className="space-y-2">
                <span className="ledgarr-label">Include transactions</span>
                <div className="max-h-48 overflow-auto rounded-lg border border-slate-800 bg-slate-950/60 p-3">
                  {reconciliationCandidateTransactions.length > 0 ? reconciliationCandidateTransactions.map((transaction) => {
                    const checked = selectedReconciliationTransactionIds.includes(transaction.id)
                    return (
                      <label key={transaction.id} className="mb-2 flex items-start gap-2 text-sm text-slate-300">
                        <input type="checkbox" checked={checked} onChange={(event) => {
                          setSelectedReconciliationTransactionIds((current) =>
                            event.target.checked ? [...current, transaction.id] : current.filter((id) => id !== transaction.id),
                          )
                        }} />
                        <span>{formatDateOnly(transaction.transactionDate)} · {transaction.description} · {formatMoney(transaction.amount)}</span>
                      </label>
                    )
                  }) : <div className="text-sm text-slate-500">No transactions are available for the selected bank account.</div>}
                </div>
              </div>
              {createBankReconciliationMutation.isError ? <ApiErrorCallout title="Unable to create reconciliation" message={getErrorMessage(createBankReconciliationMutation.error, 'Failed to create bank reconciliation.')} /> : null}
              <button type="submit" disabled={!reconciliationBankAccountId || !beginningBalance || !endingBalance || selectedReconciliationTransactionIds.length === 0 || createBankReconciliationMutation.isPending} className="rounded-full border border-teal-400/40 bg-teal-400/12 px-4 py-2 text-sm font-medium text-teal-100 transition hover:border-teal-300 hover:bg-teal-400/18 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900/60 disabled:text-slate-500">
                {createBankReconciliationMutation.isPending ? 'Creating reconciliation...' : 'Create bank reconciliation'}
              </button>
            </form>
          </Panel>
          <Panel title="Bank reconciliations" icon={<FileChartColumn className="h-4 w-4 text-teal-300" />}>
            <BankReconciliationTable reconciliations={reconciliationsQuery.data ?? []} />
          </Panel>
        </div>
      ) : null}
      {activeTab === 'Cash Activity' ? (
        <>
          <Panel title="Vendor payment runs" icon={<Landmark className="h-4 w-4 text-teal-300" />}>
            <PaymentRunTable runs={paymentRuns} />
          </Panel>
          <Panel title="Customer payments" icon={<CircleDollarSign className="h-4 w-4 text-teal-300" />}>
            <CustomerPaymentTable payments={customerPayments} />
          </Panel>
        </>
      ) : null}
      <ScopeNote>
        Operational bank truth still comes from statement imports or external institutions, but LedgArr now owns the
        tenant bank-account records, reconciliation workflow state, and cash control actions built on top of them.
      </ScopeNote>
    </div>
  )
}

function BudgetsPage({ accessToken }: { accessToken: string }) {
  const budgetsQuery = useQuery({
    queryKey: ['ledgarr', 'budgets', accessToken],
    queryFn: () => listBudgets(accessToken),
    enabled: Boolean(accessToken),
  })

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Budgets"
        title="Budget and spend guardrails"
        description="Budgets and budget thresholds belong in LedgArr because they directly influence whether financial actions are allowed, warned, or blocked."
      />
      <div className="ledgarr-grid cols-3">
        <Metric label="Budget versions" value={budgetsQuery.data?.length ?? 0} hint="Approved budget headers currently configured in LedgArr" />
        <Metric label="Budgeted amount" value={formatMoney((budgetsQuery.data ?? []).reduce((sum, budget) => sum + budget.totalBudgetAmount, 0))} hint="Total tracked budget volume across configured budget lines" />
        <Metric label="Ownership" value="LedgArr" hint="Budget enforcement is finance-owned even when dimensions reference other products" />
      </div>
      {budgetsQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load budgets"
          message={getErrorMessage(budgetsQuery.error, 'Failed to load LedgArr budgets.')}
        />
      ) : null}
      <BudgetTable budgets={budgetsQuery.data ?? []} />
      <ScopeNote>
        Sites, departments, and other operational dimensions are still referenced from owning products. LedgArr owns the
        budget, the thresholds, and the financial approval consequences.
      </ScopeNote>
    </div>
  )
}

function FixedAssetsPage({ accessToken }: { accessToken: string }) {
  const assetsQuery = useQuery({
    queryKey: ['ledgarr', 'fixed-assets', accessToken],
    queryFn: () => listFixedAssets(accessToken),
    enabled: Boolean(accessToken),
  })
  const firstAssetId = assetsQuery.data?.[0]?.id
  const schedulesQuery = useQuery({
    queryKey: ['ledgarr', 'fixed-assets', firstAssetId, 'schedule', accessToken],
    queryFn: () => listFixedAssetDepreciationSchedules(accessToken, firstAssetId!),
    enabled: Boolean(accessToken && firstAssetId),
  })

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Fixed assets"
        title="Capitalize and depreciate maintained assets"
        description="MaintainArr owns the physical asset record. LedgArr owns the asset accounting record, book value, and depreciation schedule."
      />
      <div className="ledgarr-grid cols-3">
        <Metric label="Capitalized assets" value={assetsQuery.data?.length ?? 0} hint="MaintainArr assets with LedgArr accounting records" />
        <Metric label="Capitalized value" value={formatMoney((assetsQuery.data ?? []).reduce((sum, asset) => sum + asset.capitalizedCost, 0))} hint="Original cost recognized by LedgArr" />
        <Metric label="Current book value" value={formatMoney((assetsQuery.data ?? []).reduce((sum, asset) => sum + asset.bookValue, 0))} hint="Remaining book value across active records" />
      </div>
      {assetsQuery.isError || schedulesQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load fixed assets"
          message={
            getErrorMessage(assetsQuery.error, '') ||
            getErrorMessage(schedulesQuery.error, 'Failed to load LedgArr fixed asset data.')
          }
        />
      ) : null}
      <FixedAssetTable assets={assetsQuery.data ?? []} />
      <Panel title="Example depreciation schedule" icon={<FileChartColumn className="h-4 w-4 text-teal-300" />}>
        {firstAssetId ? (
          schedulesQuery.data && schedulesQuery.data.length > 0 ? (
            <div className="ledgarr-panel overflow-hidden">
              <table className="ledgarr-table">
                <thead>
                  <tr>
                    <th>Sequence</th>
                    <th>Date</th>
                    <th>Amount</th>
                    <th>Accumulated</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {schedulesQuery.data.slice(0, 6).map((schedule) => (
                    <tr key={schedule.id}>
                      <td className="font-semibold text-slate-50">{schedule.sequenceNumber}</td>
                      <td>{formatDateOnly(schedule.depreciationDate)}</td>
                      <td>{formatMoney(schedule.depreciationAmount)}</td>
                      <td>{formatMoney(schedule.accumulatedDepreciation)}</td>
                      <td><StatusPill status={schedule.status} /></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <EmptyState title="No depreciation schedule lines are available for the first asset." />
          )
        ) : (
          <EmptyState title="Create or capitalize a fixed asset accounting record to see depreciation schedules here." />
        )}
      </Panel>
      <ScopeNote>
        This page deliberately keeps the MaintainArr asset as a reference. The accounting record, depreciation, and
        correction workflow remain LedgArr-owned.
      </ScopeNote>
    </div>
  )
}

function TaxPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const [activeTab, setActiveTab] = useState('Tax Codes')
  const [selectedEntityId, setSelectedEntityId] = useState('')
  const [selectedTaxCodeId, setSelectedTaxCodeId] = useState('')
  const [adjustmentDate, setAdjustmentDate] = useState(new Date().toISOString().slice(0, 10))
  const [adjustmentAmount, setAdjustmentAmount] = useState('')
  const [adjustmentReason, setAdjustmentReason] = useState('')

  const taxCodesQuery = useQuery({
    queryKey: ['ledgarr', 'tax-codes', accessToken],
    queryFn: () => listTaxCodes(accessToken),
    enabled: Boolean(accessToken),
  })
  const adjustmentsQuery = useQuery({
    queryKey: ['ledgarr', 'tax-adjustments', accessToken],
    queryFn: () => listTaxAdjustments(accessToken),
    enabled: Boolean(accessToken),
  })
  const liabilitySummaryQuery = useQuery({
    queryKey: ['ledgarr', 'tax-liability-summary', accessToken],
    queryFn: () => listTaxLiabilitySummaries(accessToken),
    enabled: Boolean(accessToken),
  })
  const entitiesQuery = useQuery({
    queryKey: ['ledgarr', 'financial-legal-entities', accessToken, 'tax'],
    queryFn: () => listFinancialLegalEntities(accessToken),
    enabled: Boolean(accessToken),
  })
  const adjustmentMutation = useMutation({
    mutationFn: async () =>
      createTaxAdjustment(accessToken, {
        financialLegalEntityId: selectedEntityId,
        taxCodeId: selectedTaxCodeId,
        adjustmentDate,
        amount: Number(adjustmentAmount),
        currencyCode: 'USD',
        reason: adjustmentReason.trim(),
        status: 'posted',
      }),
    onSuccess: async () => {
      setAdjustmentAmount('')
      setAdjustmentReason('')
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['ledgarr', 'tax-adjustments', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['ledgarr', 'tax-liability-summary', accessToken] }),
      ])
    },
  })

  useEffect(() => {
    if (!selectedEntityId && entitiesQuery.data?.[0]?.id) {
      setSelectedEntityId(entitiesQuery.data[0].id)
    }
  }, [entitiesQuery.data, selectedEntityId])

  useEffect(() => {
    if (!selectedTaxCodeId && taxCodesQuery.data?.[0]?.id) {
      setSelectedTaxCodeId(taxCodesQuery.data[0].id)
    }
  }, [selectedTaxCodeId, taxCodesQuery.data])

  const tabs = ['Tax Codes', 'Adjustments', 'Liability Summary'] as const
  const totalLiability = (liabilitySummaryQuery.data ?? []).reduce((sum, item) => sum + item.liabilityAmount, 0)

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Tax"
        title="Tax accounting setup"
        description="Compliance Core still owns regulatory meaning. LedgArr owns the financial tax codes, liabilities, and accounting-side tax setup shown here."
      />
      <TabStrip tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />
      <div className="ledgarr-grid cols-3">
        <Metric label="Tax codes" value={taxCodesQuery.data?.length ?? 0} hint="LedgArr tax accounting codes configured for this tenant" />
        <Metric label="Tax adjustments" value={adjustmentsQuery.data?.length ?? 0} hint="Finance-authored adjustments recorded directly in LedgArr" />
        <Metric label="Tracked liability" value={formatMoney(totalLiability)} hint="Current tax liability reflected by LedgArr adjustment activity" />
        <Metric label="Rule owner" value="Compliance Core" hint="Applicability and regulatory interpretation remain outside LedgArr" />
        <Metric label="Finance owner" value="LedgArr" hint="Financial tax setup, adjustments, and liabilities stay within LedgArr" />
      </div>
      {taxCodesQuery.isError || adjustmentsQuery.isError || liabilitySummaryQuery.isError || entitiesQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load tax workspace"
          message={
            getErrorMessage(taxCodesQuery.error, '') ||
            getErrorMessage(adjustmentsQuery.error, '') ||
            getErrorMessage(liabilitySummaryQuery.error, '') ||
            getErrorMessage(entitiesQuery.error, 'Failed to load LedgArr tax workspace.')
          }
        />
      ) : null}
      {activeTab === 'Tax Codes' ? <TaxCodeTable codes={taxCodesQuery.data ?? []} /> : null}
      {activeTab === 'Adjustments' ? (
        <div className="ledgarr-grid cols-2">
          <Panel title="Create tax adjustment" icon={<BadgeDollarSign className="h-4 w-4 text-teal-300" />}>
            <form
              className="space-y-4"
              onSubmit={async (event) => {
                event.preventDefault()
                await adjustmentMutation.mutateAsync()
              }}
            >
              <label className="block space-y-2">
                <span className="ledgarr-label">Financial legal entity</span>
                <select
                  value={selectedEntityId}
                  onChange={(event) => setSelectedEntityId(event.target.value)}
                  className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60"
                >
                  {(entitiesQuery.data ?? []).map((entity) => (
                    <option key={entity.id} value={entity.id}>
                      {entity.entityCode} · {entity.displayName}
                    </option>
                  ))}
                </select>
              </label>
              <label className="block space-y-2">
                <span className="ledgarr-label">Tax code</span>
                <select
                  value={selectedTaxCodeId}
                  onChange={(event) => setSelectedTaxCodeId(event.target.value)}
                  className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60"
                >
                  {(taxCodesQuery.data ?? []).map((code) => (
                    <option key={code.id} value={code.id}>
                      {code.taxCodeKey} · {code.displayName}
                    </option>
                  ))}
                </select>
              </label>
              <div className="grid gap-4 sm:grid-cols-2">
                <label className="block space-y-2">
                  <span className="ledgarr-label">Adjustment date</span>
                  <input
                    type="date"
                    value={adjustmentDate}
                    onChange={(event) => setAdjustmentDate(event.target.value)}
                    className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60"
                  />
                </label>
                <label className="block space-y-2">
                  <span className="ledgarr-label">Amount</span>
                  <input
                    type="number"
                    step="0.01"
                    value={adjustmentAmount}
                    onChange={(event) => setAdjustmentAmount(event.target.value)}
                    placeholder="0.00"
                    className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60"
                  />
                </label>
              </div>
              <label className="block space-y-2">
                <span className="ledgarr-label">Reason</span>
                <textarea
                  value={adjustmentReason}
                  onChange={(event) => setAdjustmentReason(event.target.value)}
                  rows={4}
                  placeholder="Explain the tax correction, accrual, or adjustment being recognized."
                  className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60"
                />
              </label>
              {adjustmentMutation.isError ? (
                <ApiErrorCallout
                  title="Unable to create tax adjustment"
                  message={getErrorMessage(adjustmentMutation.error, 'Failed to create LedgArr tax adjustment.')}
                />
              ) : null}
              <button
                type="submit"
                disabled={!selectedEntityId || !selectedTaxCodeId || !adjustmentAmount || !adjustmentReason.trim() || adjustmentMutation.isPending}
                className="rounded-full border border-teal-400/40 bg-teal-400/12 px-4 py-2 text-sm font-medium text-teal-100 transition hover:border-teal-300 hover:bg-teal-400/18 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900/60 disabled:text-slate-500"
              >
                {adjustmentMutation.isPending ? 'Recording adjustment...' : 'Create tax adjustment'}
              </button>
            </form>
          </Panel>
          <Panel title="Recent tax adjustments" icon={<Receipt className="h-4 w-4 text-teal-300" />}>
            <TaxAdjustmentTable adjustments={adjustmentsQuery.data ?? []} />
          </Panel>
        </div>
      ) : null}
      {activeTab === 'Liability Summary' ? <TaxLiabilitySummaryTable summaries={liabilitySummaryQuery.data ?? []} /> : null}
      <ScopeNote>
        Tax code configuration here is financial setup only. Governing bodies, citations, and compliance rule meaning
        remain owned by Compliance Core, while LedgArr now owns the accounting-side tax adjustments and liability view.
      </ScopeNote>
    </div>
  )
}

function IntercompanyPage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const [activeTab, setActiveTab] = useState('Transactions')
  const [selectedRelationshipId, setSelectedRelationshipId] = useState('')
  const [transactionDate, setTransactionDate] = useState(new Date().toISOString().slice(0, 10))
  const [dueDate, setDueDate] = useState('')
  const [amount, setAmount] = useState('')
  const [description, setDescription] = useState('')
  const [settleTransactionId, setSettleTransactionId] = useState<string | null>(null)
  const [settlementReason, setSettlementReason] = useState('')

  const relationshipsQuery = useQuery({
    queryKey: ['ledgarr', 'intercompany-relationships', accessToken],
    queryFn: () => listFinancialLegalEntityRelationships(accessToken),
    enabled: Boolean(accessToken),
  })
  const transactionsQuery = useQuery({
    queryKey: ['ledgarr', 'intercompany-transactions', accessToken],
    queryFn: () => listIntercompanyTransactions(accessToken),
    enabled: Boolean(accessToken),
  })
  const balancesQuery = useQuery({
    queryKey: ['ledgarr', 'intercompany-balances', accessToken],
    queryFn: () => listIntercompanyBalances(accessToken),
    enabled: Boolean(accessToken),
  })
  const systemsQuery = useQuery({
    queryKey: ['ledgarr', 'external-finance-systems', accessToken],
    queryFn: () => listExternalFinanceSystems(accessToken),
    enabled: Boolean(accessToken),
  })
  const batchesQuery = useQuery({
    queryKey: ['ledgarr', 'external-posting-batches', accessToken],
    queryFn: () => listExternalPostingBatches(accessToken),
    enabled: Boolean(accessToken),
  })
  const createTransactionMutation = useMutation({
    mutationFn: async () => {
      const relationship = (relationshipsQuery.data ?? []).find((item) => item.id === selectedRelationshipId)
      if (!relationship) {
        throw new Error('Select an intercompany relationship first.')
      }

      return createIntercompanyTransaction(accessToken, {
        relationshipId: relationship.id,
        fromFinancialLegalEntityId: relationship.parentFinancialLegalEntityId,
        toFinancialLegalEntityId: relationship.childFinancialLegalEntityId,
        transactionDate,
        dueDate: dueDate || null,
        amount: Number(amount),
        currencyCode: 'USD',
        description: description.trim(),
        transactionType: 'due_to_due_from',
        status: 'posted',
        settlementStatus: 'open',
      })
    },
    onSuccess: async () => {
      setAmount('')
      setDescription('')
      setDueDate('')
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['ledgarr', 'intercompany-transactions', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['ledgarr', 'intercompany-balances', accessToken] }),
      ])
    },
  })
  const settleTransactionMutation = useMutation({
    mutationFn: async () => {
      if (!settleTransactionId) {
        throw new Error('Select an intercompany transaction to settle first.')
      }

      return settleIntercompanyTransaction(accessToken, settleTransactionId, {
        reason: settlementReason.trim(),
      })
    },
    onSuccess: async () => {
      setSettleTransactionId(null)
      setSettlementReason('')
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['ledgarr', 'intercompany-transactions', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['ledgarr', 'intercompany-balances', accessToken] }),
      ])
    },
  })

  useEffect(() => {
    if (!selectedRelationshipId && relationshipsQuery.data?.[0]?.id) {
      setSelectedRelationshipId(relationshipsQuery.data[0].id)
    }
  }, [relationshipsQuery.data, selectedRelationshipId])

  const selectedRelationship =
    (relationshipsQuery.data ?? []).find((item) => item.id === selectedRelationshipId) ?? null
  const openTransactionCount = (transactionsQuery.data ?? []).filter((item) => item.settlementStatus !== 'settled').length
  const tabs = ['Transactions', 'Balances', 'Relationships', 'External Bridge'] as const

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Intercompany"
        title="Entity relationships and finance bridge state"
        description="Intercompany flows connect LedgArr-owned legal entities. This page now tracks due to / due from transactions and still shows external finance bridge state because both areas sit at the legal-entity and consolidation boundary."
      />
      <TabStrip tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />
      <div className="ledgarr-grid cols-3">
        <Metric label="Entity relationships" value={relationshipsQuery.data?.length ?? 0} hint="Configured LedgArr intercompany entity links" />
        <Metric label="Open intercompany items" value={openTransactionCount} hint="Transactions still outstanding between LedgArr entities" />
        <Metric label="Finance systems" value={systemsQuery.data?.length ?? 0} hint="External finance systems configured for export or bridge modes" />
        <Metric label="Posting batches" value={batchesQuery.data?.length ?? 0} hint="External posting history created from posted journals" />
      </div>
      {relationshipsQuery.isError || transactionsQuery.isError || balancesQuery.isError || systemsQuery.isError || batchesQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load intercompany and integration data"
          message={
            getErrorMessage(relationshipsQuery.error, '') ||
            getErrorMessage(transactionsQuery.error, '') ||
            getErrorMessage(balancesQuery.error, '') ||
            getErrorMessage(systemsQuery.error, '') ||
            getErrorMessage(batchesQuery.error, 'Failed to load LedgArr intercompany data.')
          }
        />
      ) : null}
      {activeTab === 'Transactions' ? (
        <div className="ledgarr-grid cols-2">
          <Panel title="Create intercompany transaction" icon={<ArrowRightLeft className="h-4 w-4 text-teal-300" />}>
            <form
              className="space-y-4"
              onSubmit={async (event) => {
                event.preventDefault()
                await createTransactionMutation.mutateAsync()
              }}
            >
              <label className="block space-y-2">
                <span className="ledgarr-label">Relationship</span>
                <select
                  value={selectedRelationshipId}
                  onChange={(event) => setSelectedRelationshipId(event.target.value)}
                  className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60"
                >
                  {(relationshipsQuery.data ?? []).map((relationship) => (
                    <option key={relationship.id} value={relationship.id}>
                      {relationship.parentDisplayName} to {relationship.childDisplayName}
                    </option>
                  ))}
                </select>
              </label>
              {selectedRelationship ? (
                <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-4 text-sm text-slate-300">
                  <div className="font-medium text-slate-100">
                    {selectedRelationship.parentDisplayName} to {selectedRelationship.childDisplayName}
                  </div>
                  <div className="mt-2 text-xs text-slate-400">
                    {titleize(selectedRelationship.relationshipType)} · {(selectedRelationship.ownershipPercentage * 100).toFixed(0)}% ownership
                  </div>
                </div>
              ) : null}
              <div className="grid gap-4 sm:grid-cols-3">
                <label className="block space-y-2">
                  <span className="ledgarr-label">Transaction date</span>
                  <input
                    type="date"
                    value={transactionDate}
                    onChange={(event) => setTransactionDate(event.target.value)}
                    className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60"
                  />
                </label>
                <label className="block space-y-2">
                  <span className="ledgarr-label">Due date</span>
                  <input
                    type="date"
                    value={dueDate}
                    onChange={(event) => setDueDate(event.target.value)}
                    className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition focus:border-teal-400/60"
                  />
                </label>
                <label className="block space-y-2">
                  <span className="ledgarr-label">Amount</span>
                  <input
                    type="number"
                    step="0.01"
                    value={amount}
                    onChange={(event) => setAmount(event.target.value)}
                    placeholder="0.00"
                    className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60"
                  />
                </label>
              </div>
              <label className="block space-y-2">
                <span className="ledgarr-label">Description</span>
                <textarea
                  value={description}
                  onChange={(event) => setDescription(event.target.value)}
                  rows={4}
                  placeholder="Describe the intercompany charge, recharge, or settlement item being recognized."
                  className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60"
                />
              </label>
              {createTransactionMutation.isError ? (
                <ApiErrorCallout
                  title="Unable to create intercompany transaction"
                  message={getErrorMessage(createTransactionMutation.error, 'Failed to create LedgArr intercompany transaction.')}
                />
              ) : null}
              <button
                type="submit"
                disabled={!selectedRelationshipId || !amount || !description.trim() || createTransactionMutation.isPending}
                className="rounded-full border border-teal-400/40 bg-teal-400/12 px-4 py-2 text-sm font-medium text-teal-100 transition hover:border-teal-300 hover:bg-teal-400/18 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900/60 disabled:text-slate-500"
              >
                {createTransactionMutation.isPending ? 'Recording transaction...' : 'Create intercompany transaction'}
              </button>
            </form>
          </Panel>
          <Panel title="Settle open transaction" icon={<BadgeDollarSign className="h-4 w-4 text-teal-300" />}>
            <div className="space-y-4">
              <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-4 text-sm text-slate-300">
                {settleTransactionId ? (
                  (() => {
                    const selected = (transactionsQuery.data ?? []).find((transaction) => transaction.id === settleTransactionId)
                    return selected ? (
                      <>
                        <div className="font-medium text-slate-100">{selected.transactionNumber}</div>
                        <div className="mt-2">
                          {selected.fromFinancialLegalEntityDisplayName} to {selected.toFinancialLegalEntityDisplayName}
                        </div>
                        <div className="mt-1 text-xs text-slate-400">{selected.description}</div>
                      </>
                    ) : (
                      <span>Select an open transaction from the table to settle it.</span>
                    )
                  })()
                ) : (
                  <span>Select an open transaction from the table to settle it.</span>
                )}
              </div>
              <label className="block space-y-2">
                <span className="ledgarr-label">Settlement reason</span>
                <textarea
                  value={settlementReason}
                  onChange={(event) => setSettlementReason(event.target.value)}
                  rows={4}
                  placeholder="Capture why this intercompany item is being settled."
                  className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60"
                />
              </label>
              {settleTransactionMutation.isError ? (
                <ApiErrorCallout
                  title="Unable to settle intercompany transaction"
                  message={getErrorMessage(settleTransactionMutation.error, 'Failed to settle LedgArr intercompany transaction.')}
                />
              ) : null}
              <button
                type="button"
                onClick={async () => {
                  await settleTransactionMutation.mutateAsync()
                }}
                disabled={!settleTransactionId || !settlementReason.trim() || settleTransactionMutation.isPending}
                className="rounded-full border border-teal-400/40 bg-teal-400/12 px-4 py-2 text-sm font-medium text-teal-100 transition hover:border-teal-300 hover:bg-teal-400/18 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900/60 disabled:text-slate-500"
              >
                {settleTransactionMutation.isPending ? 'Settling transaction...' : 'Settle selected transaction'}
              </button>
            </div>
          </Panel>
          <Panel title="Intercompany transactions" icon={<ArrowRightLeft className="h-4 w-4 text-teal-300" />}>
            <IntercompanyTransactionTable
              transactions={transactionsQuery.data ?? []}
              onSelectSettle={(transaction) => setSettleTransactionId(transaction.id)}
              settlingTransactionId={settleTransactionId}
            />
          </Panel>
        </div>
      ) : null}
      {activeTab === 'Balances' ? (
        <Panel title="Due to / due from balances" icon={<BadgeDollarSign className="h-4 w-4 text-teal-300" />}>
          <IntercompanyBalanceTable balances={balancesQuery.data ?? []} />
        </Panel>
      ) : null}
      {activeTab === 'Relationships' ? (
        <Panel title="Financial legal entity relationships" icon={<ArrowRightLeft className="h-4 w-4 text-teal-300" />}>
          <RelationshipTable relationships={relationshipsQuery.data ?? []} />
        </Panel>
      ) : null}
      {activeTab === 'External Bridge' ? (
        <>
          <Panel title="External finance systems" icon={<ShieldCheck className="h-4 w-4 text-teal-300" />}>
            <ExternalFinanceSystemTable systems={systemsQuery.data ?? []} />
          </Panel>
          <Panel title="External posting batches" icon={<FileChartColumn className="h-4 w-4 text-teal-300" />}>
            <ExternalPostingBatchTable batches={batchesQuery.data ?? []} />
          </Panel>
        </>
      ) : null}
      <ScopeNote>
        Intercompany ownership stays with LedgArr legal entities. Operational products never own these balances, and
        external systems shown here are still bridge targets only unless the tenant deliberately configures an
        external-master mode.
      </ScopeNote>
    </div>
  )
}

function ClosePage({ accessToken }: { accessToken: string }) {
  const queryClient = useQueryClient()
  const [selectedPeriodId, setSelectedPeriodId] = useState<string | null>(null)
  const [selectedAction, setSelectedAction] = useState<PeriodControlAction | null>(null)
  const [reason, setReason] = useState('')

  const dashboardQuery = useQuery({
    queryKey: ['ledgarr', 'dashboard', accessToken, 'close'],
    queryFn: () => getDashboard(accessToken),
    enabled: Boolean(accessToken),
  })
  const periodsQuery = useQuery({
    queryKey: ['ledgarr', 'fiscal-periods', accessToken, 'close'],
    queryFn: () => listFiscalPeriods(accessToken),
    enabled: Boolean(accessToken),
  })
  const selectedPeriod =
    (periodsQuery.data ?? []).find((period) => period.id === selectedPeriodId) ?? null
  const lockedPeriodCount = (periodsQuery.data ?? []).filter((period) => period.status.toLowerCase() === 'locked').length
  const actionablePeriods = (periodsQuery.data ?? []).filter(
    (period) => getAvailablePeriodActions(period).length > 0,
  ).length

  const periodActionMutation = useMutation({
    mutationFn: async ({
      periodId,
      action,
      actionReason,
    }: {
      periodId: string
      action: PeriodControlAction
      actionReason: string
    }) => {
      switch (action) {
        case 'close':
          return closeFiscalPeriod(accessToken, periodId, { reason: actionReason })
        case 'reopen':
          return reopenFiscalPeriod(accessToken, periodId, { reason: actionReason })
        case 'lock':
          return lockFiscalPeriod(accessToken, periodId, { reason: actionReason })
      }
    },
    onSuccess: async () => {
      setSelectedPeriodId(null)
      setSelectedAction(null)
      setReason('')
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['ledgarr', 'dashboard', accessToken, 'close'] }),
        queryClient.invalidateQueries({ queryKey: ['ledgarr', 'fiscal-periods', accessToken, 'close'] }),
      ])
    },
  })

  function handleSelectAction(periodId: string, action: PeriodControlAction) {
    setSelectedPeriodId(periodId)
    setSelectedAction(action)
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!selectedPeriodId || !selectedAction) {
      return
    }

    await periodActionMutation.mutateAsync({
      periodId: selectedPeriodId,
      action: selectedAction,
      actionReason: reason.trim(),
    })
  }

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Close"
        title="Period close and control state"
        description="Close, reopen, and lock decisions are LedgArr controls because they govern whether financial records may still change."
      />
      <div className="ledgarr-grid cols-3">
        <Metric
          label="Open periods"
          value={dashboardQuery.data?.openPeriodCount ?? '0'}
          hint="Available for normal LedgArr posting"
        />
        <Metric
          label="Closed periods"
          value={dashboardQuery.data?.closedPeriodCount ?? '0'}
          hint="Protected by close workflow controls"
        />
        <Metric
          label="Locked periods"
          value={lockedPeriodCount}
          hint="Hard-locked against financial change until authorized reopen"
        />
        <Metric
          label="Unposted queue"
          value={dashboardQuery.data?.openPacketCount ?? '0'}
          hint="Backlog to resolve before hard close"
        />
      </div>
      {dashboardQuery.isError || periodsQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load close data"
          message={
            getErrorMessage(dashboardQuery.error, '') ||
            getErrorMessage(periodsQuery.error, 'Failed to load LedgArr close data.')
          }
        />
      ) : null}
      <div className="ledgarr-grid cols-2">
        <Panel title="Close control panel" icon={<ShieldCheck className="h-4 w-4 text-teal-300" />}>
          <form className="space-y-4" onSubmit={handleSubmit}>
            <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-4">
              <p className="ledgarr-label">Selected period</p>
              <p className="mt-2 text-base font-semibold text-slate-50">
                {selectedPeriod ? `${selectedPeriod.periodKey} · ${selectedPeriod.name}` : 'Choose a period action below'}
              </p>
              <p className="mt-2 text-sm text-slate-300">
                {selectedAction
                  ? `${getPeriodActionLabel(selectedAction)} will be applied with an audit reason.`
                  : 'Only valid status transitions are offered to keep close controls disciplined.'}
              </p>
              {selectedPeriod ? (
                <div className="mt-3 flex flex-wrap items-center gap-2 text-xs text-slate-400">
                  <StatusPill status={selectedPeriod.status} />
                  <span>
                    {formatDateOnly(selectedPeriod.startDate)} to {formatDateOnly(selectedPeriod.endDate)}
                  </span>
                </div>
              ) : null}
            </div>
            <label className="block space-y-2">
              <span className="ledgarr-label">Reason</span>
              <textarea
                value={reason}
                onChange={(event) => setReason(event.target.value)}
                placeholder="Capture why this close, reopen, or lock action is being performed."
                rows={4}
                className="w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-teal-400/60"
              />
            </label>
            {periodActionMutation.isError ? (
              <ApiErrorCallout
                title="Period control action failed"
                message={getErrorMessage(periodActionMutation.error, 'Failed to update fiscal period status.')}
              />
            ) : null}
            <div className="flex flex-wrap items-center gap-3">
              <button
                type="submit"
                disabled={!selectedPeriod || !selectedAction || !reason.trim() || periodActionMutation.isPending}
                className="rounded-full border border-teal-400/40 bg-teal-400/12 px-4 py-2 text-sm font-medium text-teal-100 transition hover:border-teal-300 hover:bg-teal-400/18 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900/60 disabled:text-slate-500"
              >
                {periodActionMutation.isPending
                  ? 'Saving control...'
                  : selectedAction
                    ? `${getPeriodActionLabel(selectedAction)} selected period`
                    : 'Select a period action'}
              </button>
              <button
                type="button"
                onClick={() => {
                  setSelectedPeriodId(null)
                  setSelectedAction(null)
                  setReason('')
                }}
                className="rounded-full border border-slate-700 bg-slate-900/70 px-4 py-2 text-sm font-medium text-slate-300 transition hover:border-slate-600 hover:text-slate-100"
              >
                Reset
              </button>
            </div>
          </form>
        </Panel>
        <Panel title="Close readiness snapshot" icon={<AlertTriangle className="h-4 w-4 text-amber-300" />}>
          <div className="space-y-4 text-sm text-slate-300">
            <p>
              LedgArr close is staged: an <span className="font-medium text-slate-100">open</span> period may be soft
              closed, a <span className="font-medium text-slate-100">closed</span> period may be hard locked, and only
              closed or locked periods may be reopened with explicit reason capture.
            </p>
            <div className="grid gap-3 sm:grid-cols-2">
              <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-4">
                <p className="ledgarr-label">Actionable periods</p>
                <p className="mt-2 text-2xl font-semibold text-slate-50">{actionablePeriods}</p>
                <p className="mt-2 text-sm text-slate-400">Periods with a valid manual control still available.</p>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-4">
                <p className="ledgarr-label">Open posting queue</p>
                <p className="mt-2 text-2xl font-semibold text-slate-50">
                  {dashboardQuery.data?.openPacketCount ?? 0}
                </p>
                <p className="mt-2 text-sm text-slate-400">Resolve packet backlog before moving to hard lock.</p>
              </div>
            </div>
          </div>
        </Panel>
      </div>
      <FiscalPeriodControlTable
        periods={periodsQuery.data ?? []}
        selectedPeriodId={selectedPeriodId}
        selectedAction={selectedAction}
        onSelectAction={handleSelectAction}
      />
      <ScopeNote>
        Close is a LedgArr control surface. Reopen, lock, and correction workflows belong here even when the source event
        originated in another product.
      </ScopeNote>
    </div>
  )
}

function ReportsPage({ accessToken }: { accessToken: string }) {
  const trialBalanceQuery = useQuery({
    queryKey: ['ledgarr', 'trial-balance', accessToken],
    queryFn: () => getTrialBalance(accessToken),
    enabled: Boolean(accessToken),
  })
  const pnlSummaryQuery = useQuery({
    queryKey: ['ledgarr', 'report-summary', accessToken, 'profit-and-loss'],
    queryFn: () => getReportSummary(accessToken, 'profit-and-loss'),
    enabled: Boolean(accessToken),
  })
  const balanceSheetSummaryQuery = useQuery({
    queryKey: ['ledgarr', 'report-summary', accessToken, 'balance-sheet'],
    queryFn: () => getReportSummary(accessToken, 'balance-sheet'),
    enabled: Boolean(accessToken),
  })
  const cashFlowSummaryQuery = useQuery({
    queryKey: ['ledgarr', 'report-summary', accessToken, 'cash-flow'],
    queryFn: () => getReportSummary(accessToken, 'cash-flow'),
    enabled: Boolean(accessToken),
  })

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Reports"
        title="Canonical financial reporting"
        description="LedgArr owns the authoritative financial statements. ReportArr may consume these datasets later, but it does not replace this source of truth."
        action={
          trialBalanceQuery.data ? (
            <span className="ledgarr-pill">
              {formatMoney(trialBalanceQuery.data.totalDebits)} / {formatMoney(trialBalanceQuery.data.totalCredits)}
            </span>
          ) : null
        }
      />
      <div className="ledgarr-grid cols-3">
        <SummaryReportCard
          title="Profit and Loss"
          summary={pnlSummaryQuery.data}
          unavailableMessage="Profit and Loss summary will appear here as report datasets expand."
        />
        <SummaryReportCard
          title="Balance Sheet"
          summary={balanceSheetSummaryQuery.data}
          unavailableMessage="Balance Sheet summary will appear here as report datasets expand."
        />
        <SummaryReportCard
          title="Cash Flow"
          summary={cashFlowSummaryQuery.data}
          unavailableMessage="Cash Flow summary will appear here as report datasets expand."
        />
      </div>
      {trialBalanceQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load reports"
          message={getErrorMessage(trialBalanceQuery.error, 'Failed to load LedgArr reports.')}
        />
      ) : null}
      <Panel title="Trial balance" icon={<FileChartColumn className="h-4 w-4 text-teal-300" />}>
        <TrialBalanceTable rows={trialBalanceQuery.data?.rows ?? []} />
      </Panel>
      <ScopeNote>
        ReportArr can consume these LedgArr datasets for cross-suite analytics, but LedgArr remains the source of truth
        for the financial statements themselves.
      </ScopeNote>
    </div>
  )
}

function LegalEntitiesLandingPage({ accessToken }: { accessToken: string }) {
  const entitiesQuery = useQuery({
    queryKey: ['ledgarr', 'financial-legal-entities', accessToken],
    queryFn: () => listFinancialLegalEntities(accessToken),
    enabled: Boolean(accessToken),
  })

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Settings / Legal entities"
        title="LedgArr legal entities"
        description="These are LedgArr-owned accounting entities only. They must never model Compliance Core governing bodies or regulators."
      />
      {entitiesQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load legal entities"
          message={getErrorMessage(entitiesQuery.error, 'Failed to load legal entities.')}
        />
      ) : null}
      <LegalEntityTable entities={entitiesQuery.data ?? []} />
      <ScopeNote>
        Legal entities here are accounting/reporting entities only. Governing bodies, agencies, and law catalogs remain
        owned by Compliance Core.
      </ScopeNote>
    </div>
  )
}

function HomePage({
  accessToken,
  session,
}: {
  accessToken: string
  session: StoredLedgArrSession | null
}) {
  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Launch status"
        title="LedgArr workspace bootstrap"
        description="LedgArr owns financial truth, ledger posting, payables, receivables, reporting, close controls, and external finance handoff status."
        action={<span className="ledgarr-pill">Financial SOT</span>}
      />
      <div className="ledgarr-grid cols-2">
        <Panel title="Workspace session" icon={<ShieldCheck className="h-4 w-4 text-teal-300" />}>
          <div className="space-y-2 text-sm text-slate-300">
            <p>
              <strong className="text-slate-100">API base:</strong>{' '}
              <span className="ledgarr-pill">{apiBase || '/api proxy'}</span>
            </p>
            <p>
              <strong className="text-slate-100">Frontend port:</strong>{' '}
              <span className="ledgarr-pill">5188</span>
            </p>
            <p>
              <strong className="text-slate-100">Access token present:</strong>{' '}
              <span className="ledgarr-pill">{accessToken ? 'yes' : 'no'}</span>
            </p>
            <p>
              <strong className="text-slate-100">Tenant:</strong>{' '}
              <span>{session?.tenantDisplayName ?? 'No active LedgArr handoff session'}</span>
            </p>
          </div>
        </Panel>
        <Panel title="Ownership" icon={<BadgeDollarSign className="h-4 w-4 text-teal-300" />}>
          <div className="space-y-2 text-sm text-slate-300">
            <p>LedgArr owns legal entities, journals, AP, AR, tax accounting, budgets, close controls, and financial reports.</p>
            <p>Customers remain owned by CustomArr. Vendors remain owned by SupplyArr. Documents remain owned by RecordArr.</p>
            <p>Operational orders, trips, receipts, work orders, and assets remain owned by their source products and enter LedgArr as references or finance packets.</p>
          </div>
        </Panel>
      </div>
    </div>
  )
}

export default function App() {
  const location = useLocation()
  const session = loadSession()

  const sessionQuery = useQuery({
    queryKey: ['ledgarr', 'session', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const launchCatalogQuery = useQuery({
    queryKey: ['ledgarr', 'launch-catalog', session?.accessToken],
    queryFn: () => getLaunchCatalog(apiBase, session!.accessToken, 'ledgarr'),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  useEffect(() => {
    if (sessionQuery.isError && resolveProductWorkspaceBootstrapError(sessionQuery.error)) {
      clearSession()
    }
  }, [sessionQuery.error, sessionQuery.isError])

  useEffect(() => {
    if (launchCatalogQuery.isError && resolveProductWorkspaceBootstrapError(launchCatalogQuery.error)) {
      clearSession()
    }
  }, [launchCatalogQuery.error, launchCatalogQuery.isError])

  const bootstrapError = sessionQuery.isError
    ? resolveProductWorkspaceBootstrapError(sessionQuery.error)
    : launchCatalogQuery.isError
      ? resolveProductWorkspaceBootstrapError(launchCatalogQuery.error)
      : null

  const workspaceSession =
    session && sessionQuery.data && !bootstrapError
      ? {
          userId: session.userId,
          tenantId: session.tenantId,
          userDisplayName: session.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
        }
      : null

  const switcherEntitlements =
    launchCatalogQuery.data?.products.map((product) => product.productKey) ??
    sessionQuery.data?.entitlements ??
    []

  const launch = useProductWorkspaceLaunch({
    apiBase,
    accessToken: session?.accessToken ?? '',
    currentProductKey: 'ledgarr',
    suiteHomeUrl,
    productLaunchUrls,
  })

  if (location.pathname === '/launch' || location.pathname === '/handoff') {
    return <LaunchPage />
  }

  return (
    <ProductWorkspaceFrame
      productName="LedgArr"
      productKey="ledgarr"
      workspaceSubtitle="Financial ledger, subledgers, and ERP control"
      navItems={ledgArrNavItems}
      entitlements={switcherEntitlements}
      suiteHomeUrl={suiteHomeUrl}
      productLaunchUrls={productLaunchUrls}
      onSelectProduct={
        session?.accessToken
          ? (productKey) => {
              void launch.mutate(productKey)
            }
          : undefined
      }
      onSignOut={
        session
          ? () => {
              clearSession()
              window.location.assign(suiteHomeUrl)
            }
          : undefined
      }
      isProductLaunchPending={launch.isPending}
      productLaunchError={launch.isError ? formatProductLaunchError(launch.error) : null}
      aiAssistance={session?.accessToken ? { apiBase, accessToken: session.accessToken } : undefined}
      workspaceSession={workspaceSession}
      isBootstrapping={Boolean(session?.accessToken) && (sessionQuery.isLoading || launchCatalogQuery.isLoading)}
      bootstrapError={bootstrapError}
    >
      <Routes>
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<DashboardPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/general-ledger" element={<GeneralLedgerPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/payables" element={<PayablesPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/receivables" element={<ReceivablesPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/billing" element={<BillingPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/banking" element={<BankingPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/budgets" element={<BudgetsPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/fixed-assets" element={<FixedAssetsPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/tax" element={<TaxPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/intercompany" element={<IntercompanyPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/close" element={<ClosePage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/reports" element={<ReportsPage accessToken={session?.accessToken ?? ''} />} />
        <Route
          path="/settings"
          element={<LedgArrSettingsPage accessToken={session?.accessToken ?? ''} canManage={session?.tenantRoleKey === 'tenant_admin'} />}
        />
        <Route path="/settings/legal-entities" element={<LegalEntitiesLandingPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/payroll" element={<PayrollPage />} />
        <Route path="/payroll/calendars" element={<PayrollPage />} />
        <Route path="/payroll/code-mappings" element={<PayrollPage />} />
        <Route path="/payroll/batches" element={<PayrollPage />} />
        <Route path="/payroll/batches/:id" element={<PayrollPage />} />
        <Route path="/payroll/batches/:id/lines" element={<PayrollPage />} />
        <Route path="/payroll/batches/:id/export" element={<PayrollPage />} />
        <Route path="/payroll/batches/:id/journal" element={<PayrollPage />} />
        <Route path="/packets" element={<Navigate to="/billing" replace />} />
        <Route path="/journals" element={<Navigate to="/general-ledger" replace />} />
        <Route path="/ap" element={<Navigate to="/payables" replace />} />
        <Route path="/ar" element={<Navigate to="/receivables" replace />} />
        <Route path="/valuation" element={<Navigate to="/fixed-assets" replace />} />
        <Route path="/home" element={<HomePage accessToken={session?.accessToken ?? ''} session={session} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </ProductWorkspaceFrame>
  )
}
