import { useEffect, type ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
  Banknote,
  BookOpenCheck,
  Boxes,
  FileChartColumn,
  Landmark,
  LayoutDashboard,
  Receipt,
  Settings,
  WalletCards,
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
  type ProductNavItem,
} from '@stl/shared-ui'
import { clearSession, loadSession, type StoredLedgArrSession } from './auth/sessionStorage'
import {
  getDashboard,
  getSessionBootstrap,
  getTrialBalance,
  listCustomerInvoices,
  listJournals,
  listPackets,
  listVendorBills,
  type CustomerInvoice,
  type FinancialPacket,
  type JournalEntry,
  type LedgArrDashboardResponse,
  type TrialBalanceRow,
  type VendorBill,
} from './api/client'
import { LaunchPage } from './LaunchPage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_LEDGARR_API_BASE ?? ''

const navItems: ProductNavItem[] = [
  { label: 'Dashboard', to: '/dashboard', icon: LayoutDashboard as ProductNavItem['icon'] },
  { label: 'Packets', to: '/packets', icon: Receipt as ProductNavItem['icon'] },
  { label: 'Journals', to: '/journals', icon: BookOpenCheck as ProductNavItem['icon'] },
  { label: 'AP', to: '/ap', icon: WalletCards as ProductNavItem['icon'] },
  { label: 'AR', to: '/ar', icon: Banknote as ProductNavItem['icon'] },
  { label: 'Valuation', to: '/valuation', icon: Boxes as ProductNavItem['icon'] },
  { label: 'Reports', to: '/reports', icon: FileChartColumn as ProductNavItem['icon'] },
  { label: 'Settings', to: '/settings', icon: Settings as ProductNavItem['icon'], sectionBreakBefore: true },
]

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

function titleize(value: string): string {
  return value
    .split(/[_-]/g)
    .filter(Boolean)
    .map((part) => part.slice(0, 1).toUpperCase() + part.slice(1))
    .join(' ')
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
        <p className="max-w-3xl text-sm text-slate-300">{description}</p>
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

function EmptyState({ title }: { title: string }) {
  return <div className="rounded-lg border border-dashed border-slate-700/80 p-4 text-sm text-slate-400">{title}</div>
}

function StatusPill({ status }: { status: string }) {
  const className = status.includes('failed') || status.includes('variance') || status.includes('needs')
    ? 'ledgarr-pill warning'
    : 'ledgarr-pill'
  return <span className={className}>{titleize(status)}</span>
}

function DashboardPage({ accessToken }: { accessToken: string }) {
  const dashboardQuery = useQuery({
    queryKey: ['ledgarr', 'dashboard', accessToken],
    queryFn: () => getDashboard(accessToken),
    enabled: Boolean(accessToken),
  })
  const dashboard = dashboardQuery.data

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="LedgArr"
        title="Financial control center"
        description="Financial legal entities, packets, postings, close state, subledger exposure, and audit activity."
        action={dashboard ? <span className="ledgarr-pill">Updated {formatDate(dashboard.generatedAt)}</span> : null}
      />
      {dashboardQuery.isError ? (
        <ApiErrorCallout title="Unable to load dashboard" message={getErrorMessage(dashboardQuery.error, 'Failed to load LedgArr dashboard.')} />
      ) : null}
      {dashboard ? <DashboardMetrics dashboard={dashboard} /> : <EmptyState title="No LedgArr dashboard response is available." />}
      {dashboard ? (
        <Panel title="Audit activity" icon={<Landmark className="h-4 w-4 text-teal-300" />}>
          <div className="space-y-3">
            {dashboard.recentActivity.map((item) => (
              <div key={item.activityId} className="rounded-md border border-slate-700/70 bg-slate-900/70 p-3">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <strong className="text-sm text-slate-50">{titleize(item.action)}</strong>
                  <span className="ledgarr-pill">{item.targetType}</span>
                </div>
                <p className="mt-2 text-sm text-slate-300">{item.summary}</p>
                <p className="mt-2 text-xs text-slate-400">{formatDate(item.occurredAt)}</p>
              </div>
            ))}
            {dashboard.recentActivity.length === 0 ? <EmptyState title="No LedgArr audit activity has been recorded." /> : null}
          </div>
        </Panel>
      ) : null}
    </div>
  )
}

function DashboardMetrics({ dashboard }: { dashboard: LedgArrDashboardResponse }) {
  return (
    <div className="ledgarr-grid cols-4">
      <Metric label="Entities" value={dashboard.financialLegalEntityCount} hint="Active Financial Legal Entities" />
      <Metric label="Open packets" value={dashboard.openPacketCount} hint="Received, failed, or mapping-needed packets" />
      <Metric label="Posted journals" value={dashboard.postedJournalCount} hint="Immutable general ledger postings" />
      <Metric label="Open periods" value={dashboard.openPeriodCount} hint="Fiscal periods accepting normal postings" />
      <Metric label="Debit volume" value={formatMoney(dashboard.postedDebitVolume)} hint="Posted journal debit total" />
      <Metric label="AP exposure" value={formatMoney(dashboard.openApAmount)} hint="Open vendor bill amount" />
      <Metric label="AR exposure" value={formatMoney(dashboard.openArAmount)} hint="Open customer invoice amount" />
      <Metric label="Closed periods" value={dashboard.closedPeriodCount} hint="Periods closed by LedgArr controls" />
    </div>
  )
}

function PacketsPage({ accessToken }: { accessToken: string }) {
  const query = useQuery({
    queryKey: ['ledgarr', 'packets', accessToken],
    queryFn: () => listPackets(accessToken),
    enabled: Boolean(accessToken),
  })
  const packets = query.data ?? []

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Packets"
        title="Financial packet inbox"
        description="Finance-ready events received from owning products and mapped into LedgArr posting workflows."
      />
      {query.isError ? <ApiErrorCallout title="Unable to load packets" message={getErrorMessage(query.error, 'Failed to load packets.')} /> : null}
      <PacketTable packets={packets} />
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
                <strong className="text-slate-50">{packet.sourceRecordDisplayName}</strong>
                <p className="text-xs text-slate-400">{packet.sourceProductKey}</p>
              </td>
              <td>{titleize(packet.packetType)}</td>
              <td>{packet.accountingDate}</td>
              <td>{formatMoney(packet.sourceTotalAmount)}</td>
              <td><StatusPill status={packet.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function JournalsPage({ accessToken }: { accessToken: string }) {
  const query = useQuery({
    queryKey: ['ledgarr', 'journals', accessToken],
    queryFn: () => listJournals(accessToken),
    enabled: Boolean(accessToken),
  })
  const journals = (query.data ?? []).map((item) => item.journal)

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Journals"
        title="General ledger journal register"
        description="Balanced journal entries, posting state, reversals, and source document accounting dates."
      />
      {query.isError ? <ApiErrorCallout title="Unable to load journals" message={getErrorMessage(query.error, 'Failed to load journals.')} /> : null}
      <JournalTable journals={journals} />
    </div>
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
              <td>{journal.accountingDate}</td>
              <td>{journal.description}</td>
              <td>{formatMoney(journal.totalDebits)}</td>
              <td>{formatMoney(journal.totalCredits)}</td>
              <td><StatusPill status={journal.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function APPage({ accessToken }: { accessToken: string }) {
  const query = useQuery({
    queryKey: ['ledgarr', 'ap', accessToken],
    queryFn: () => listVendorBills(accessToken),
    enabled: Boolean(accessToken),
  })
  const bills = query.data ?? []

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Accounts payable"
        title="Vendor bill queue"
        description="AP bills, match state, approvals, posting state, and payment export readiness."
      />
      {query.isError ? <ApiErrorCallout title="Unable to load AP" message={getErrorMessage(query.error, 'Failed to load vendor bills.')} /> : null}
      <VendorBillTable bills={bills} />
    </div>
  )
}

function VendorBillTable({ bills }: { bills: VendorBill[] }) {
  if (bills.length === 0) {
    return <EmptyState title="No AP vendor bills have been created." />
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
              <td>{bill.dueDate}</td>
              <td>{formatMoney(bill.totalAmount)}</td>
              <td><StatusPill status={bill.matchStatus} /></td>
              <td><StatusPill status={bill.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function ARPage({ accessToken }: { accessToken: string }) {
  const query = useQuery({
    queryKey: ['ledgarr', 'ar', accessToken],
    queryFn: () => listCustomerInvoices(accessToken),
    enabled: Boolean(accessToken),
  })
  const invoices = query.data ?? []

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Accounts receivable"
        title="Customer invoice queue"
        description="AR invoices, issue state, posting state, payment application, and aging exposure."
      />
      {query.isError ? <ApiErrorCallout title="Unable to load AR" message={getErrorMessage(query.error, 'Failed to load customer invoices.')} /> : null}
      <CustomerInvoiceTable invoices={invoices} />
    </div>
  )
}

function CustomerInvoiceTable({ invoices }: { invoices: CustomerInvoice[] }) {
  if (invoices.length === 0) {
    return <EmptyState title="No AR customer invoices have been created." />
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
              <td>{invoice.dueDate}</td>
              <td>{formatMoney(invoice.totalAmount)}</td>
              <td><StatusPill status={invoice.status} /></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function ValuationPage() {
  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Valuation"
        title="Inventory valuation"
        description="Inventory financial value, cost layers, COGS posting, and reconciliation exceptions separate from LoadArr stock execution."
      />
      <div className="ledgarr-grid cols-3">
        <Metric label="Cost methods" value="FIFO / WAC / Standard" hint="Configured through LedgArr item cost profiles" />
        <Metric label="Stock owner" value="LoadArr" hint="Operational balances remain outside LedgArr" />
        <Metric label="Finance owner" value="LedgArr" hint="Valuation, layers, COGS, and adjustments" />
      </div>
    </div>
  )
}

function ReportsPage({ accessToken }: { accessToken: string }) {
  const query = useQuery({
    queryKey: ['ledgarr', 'trial-balance', accessToken],
    queryFn: () => getTrialBalance(accessToken),
    enabled: Boolean(accessToken),
  })
  const rows = query.data?.rows ?? []

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Reports"
        title="Trial balance"
        description="Ledger-sourced balances with equal debit and credit totals."
        action={query.data ? <span className="ledgarr-pill">{formatMoney(query.data.totalDebits)} / {formatMoney(query.data.totalCredits)}</span> : null}
      />
      {query.isError ? <ApiErrorCallout title="Unable to load reports" message={getErrorMessage(query.error, 'Failed to load trial balance.')} /> : null}
      <TrialBalanceTable rows={rows} />
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

function SettingsPage({
  accessToken,
  session,
}: {
  accessToken: string
  session: StoredLedgArrSession | null
}) {
  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Settings"
        title="Finance setup"
        description="Accounting entity setup, fiscal controls, chart configuration, subledger controls, and external ERP bridge mode."
        action={<span className="ledgarr-pill">Financial SOT</span>}
      />
      <div className="ledgarr-grid cols-2">
        <Panel title="Runtime" icon={<Landmark className="h-4 w-4 text-teal-300" />}>
          <div className="space-y-2 text-sm text-slate-300">
            <p><strong className="text-slate-100">API base:</strong> <span className="ledgarr-pill">{apiBase || '/api proxy'}</span></p>
            <p><strong className="text-slate-100">Frontend port:</strong> <span className="ledgarr-pill">5188</span></p>
            <p><strong className="text-slate-100">API port:</strong> <span className="ledgarr-pill">5113</span></p>
            <p><strong className="text-slate-100">Access token present:</strong> <span className="ledgarr-pill">{accessToken ? 'yes' : 'no'}</span></p>
            <p><strong className="text-slate-100">Current tenant:</strong> {session?.tenantDisplayName ?? 'n/a'}</p>
          </div>
        </Panel>
        <Panel title="Boundaries" icon={<BookOpenCheck className="h-4 w-4 text-amber-300" />}>
          <div className="space-y-2 text-sm text-slate-300">
            <p>LedgArr owns financial truth, posting rules, subledgers, close controls, valuation, and external finance export history.</p>
            <p>Compliance Core keeps governing bodies, citations, regulatory vocabulary, and rulepack authority metadata.</p>
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
      workspaceSubtitle="Financial ledger and ERP bridge"
      navItems={navItems}
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
        <Route path="/packets" element={<PacketsPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/journals" element={<JournalsPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/ap" element={<APPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/ar" element={<ARPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/valuation" element={<ValuationPage />} />
        <Route path="/reports" element={<ReportsPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/settings" element={<SettingsPage accessToken={session?.accessToken ?? ''} session={session} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </ProductWorkspaceFrame>
  )
}
