import { useEffect, type ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
  Activity,
  ClipboardCheck,
  FileCheck2,
  LayoutDashboard,
  PackageCheck,
  Settings,
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
import { clearSession, loadSession, type StoredOrdArrSession } from './auth/sessionStorage'
import {
  getDashboard,
  getSessionBootstrap,
  listCompletionPackets,
  listHandoffs,
  listOrders,
  type OrdArrCompletionPacket,
  type OrdArrDashboardResponse,
  type OrdArrHandoff,
  type OrdArrOrderSummary,
} from './api/client'
import { LaunchPage } from './LaunchPage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_ORDARR_API_BASE ?? ''

const navItems: ProductNavItem[] = [
  { label: 'Dashboard', to: '/dashboard', icon: LayoutDashboard as ProductNavItem['icon'] },
  { label: 'Orders', to: '/orders', icon: ClipboardCheck as ProductNavItem['icon'] },
  { label: 'Handoffs', to: '/handoffs', icon: Activity as ProductNavItem['icon'] },
  { label: 'Completion', to: '/completion', icon: FileCheck2 as ProductNavItem['icon'] },
  { label: 'Settings', to: '/settings', icon: Settings as ProductNavItem['icon'], sectionBreakBefore: true },
]

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
        <p className="ordarr-label">{eyebrow}</p>
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
    <section className="ordarr-panel">
      <div className="ordarr-panel-inner space-y-3">
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
    <div className="ordarr-panel">
      <div className="ordarr-panel-inner">
        <p className="ordarr-label">{label}</p>
        <p className="mt-2 text-3xl font-semibold text-slate-50">{value}</p>
        <p className="mt-2 text-sm text-slate-300">{hint}</p>
      </div>
    </div>
  )
}

function EmptyState({ title }: { title: string }) {
  return <div className="rounded-lg border border-dashed border-slate-700/80 p-4 text-sm text-slate-400">{title}</div>
}

function DashboardPage({
  accessToken,
}: {
  accessToken: string
}) {
  const dashboardQuery = useQuery({
    queryKey: ['ordarr', 'dashboard', accessToken],
    queryFn: () => getDashboard(accessToken),
    enabled: Boolean(accessToken),
  })
  const dashboard = dashboardQuery.data

  return (
    <div className="ordarr-page">
      <PageHeader
        eyebrow="OrdArr"
        title="Order orchestration"
        description="Track order/request lifecycle, product handoff state, completion packets, and finance-ready packet readiness from the OrdArr source record."
        action={dashboard ? <span className="ordarr-pill">Updated {formatDate(dashboard.generatedAt)}</span> : null}
      />
      {dashboardQuery.isError ? (
        <ApiErrorCallout title="Unable to load dashboard" message={getErrorMessage(dashboardQuery.error, 'Failed to load OrdArr dashboard.')} />
      ) : null}
      {dashboard ? <DashboardMetrics dashboard={dashboard} /> : <EmptyState title="No OrdArr dashboard response is available." />}
      {dashboard ? (
        <Panel title="Recent activity" icon={<Activity className="h-4 w-4 text-sky-300" />}>
          <div className="space-y-3">
            {dashboard.recentActivity.map((item) => (
              <div key={item.activityId} className="rounded-lg border border-slate-700/70 bg-slate-900/70 p-3">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <strong className="text-sm text-slate-50">{item.orderNumber}</strong>
                  <span className="ordarr-pill">{item.eventType}</span>
                </div>
                <p className="mt-2 text-sm text-slate-300">{item.message}</p>
                <p className="mt-2 text-xs text-slate-400">{formatDate(item.occurredAt)}</p>
              </div>
            ))}
            {dashboard.recentActivity.length === 0 ? <EmptyState title="No OrdArr activity has been recorded." /> : null}
          </div>
        </Panel>
      ) : null}
    </div>
  )
}

function DashboardMetrics({ dashboard }: { dashboard: OrdArrDashboardResponse }) {
  return (
    <div className="ordarr-grid cols-3">
      <Metric label="Orders" value={dashboard.orderCount} hint="Canonical OrdArr order/request records" />
      <Metric label="Requests" value={dashboard.requestCount} hint="Request-type order orchestration records" />
      <Metric label="Handoffs" value={dashboard.activeHandoffCount} hint="Open product handoffs awaiting state changes" />
      <Metric label="Completion" value={dashboard.completionPacketCount} hint="Completion packet records assembled by OrdArr" />
      <Metric label="Invoice ready" value={dashboard.invoiceReadyPacketCount} hint="Customer invoice-ready packets" />
      <Metric label="Bill ready" value={dashboard.billReadyPacketCount} hint="Vendor bill-ready packets" />
    </div>
  )
}

function OrdersPage({ accessToken }: { accessToken: string }) {
  const ordersQuery = useQuery({
    queryKey: ['ordarr', 'orders', accessToken],
    queryFn: () => listOrders(accessToken),
    enabled: Boolean(accessToken),
  })
  const orders = ordersQuery.data ?? []

  return (
    <div className="ordarr-page">
      <PageHeader
        eyebrow="Orders"
        title="Order/request register"
        description="OrdArr owns the parent orchestration record. Customer truth remains a CustomArr reference and execution truth remains with the target product."
      />
      {ordersQuery.isError ? <ApiErrorCallout title="Unable to load orders" message={getErrorMessage(ordersQuery.error, 'Failed to load orders.')} /> : null}
      <div className="ordarr-grid cols-2">
        {orders.map((order) => (
          <OrderCard key={order.orderId} order={order} />
        ))}
      </div>
      {orders.length === 0 ? <EmptyState title="No OrdArr orders or requests have been created." /> : null}
    </div>
  )
}

function OrderCard({ order }: { order: OrdArrOrderSummary }) {
  return (
    <div className="ordarr-panel">
      <div className="ordarr-panel-inner space-y-3">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <p className="ordarr-label">{order.orderNumber}</p>
            <h2 className="mt-1 text-lg font-semibold text-slate-50">{order.customerName}</h2>
          </div>
          <span className="ordarr-pill">{titleize(order.lifecycleStatus)}</span>
        </div>
        <p className="text-sm text-slate-300">{order.summary}</p>
        <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-2">
          <p><strong className="text-slate-100">Request:</strong> {titleize(order.requestType)}</p>
          <p><strong className="text-slate-100">Customer ref:</strong> {order.customerRef.productKey}:{order.customerRef.objectType}:{order.customerRef.objectId}</p>
          <p><strong className="text-slate-100">Handoff:</strong> {titleize(order.handoffState)}</p>
          <p><strong className="text-slate-100">Finance packet:</strong> {titleize(order.financialPacketState)}</p>
        </div>
      </div>
    </div>
  )
}

function HandoffsPage({ accessToken }: { accessToken: string }) {
  const handoffsQuery = useQuery({
    queryKey: ['ordarr', 'handoffs', accessToken],
    queryFn: () => listHandoffs(accessToken),
    enabled: Boolean(accessToken),
  })
  const handoffs = handoffsQuery.data ?? []

  return (
    <div className="ordarr-page">
      <PageHeader
        eyebrow="Handoffs"
        title="Product handoff queue"
        description="OrdArr requests work through explicit target-product handoffs; acceptance, blocking, and execution status remain with the target product."
      />
      {handoffsQuery.isError ? <ApiErrorCallout title="Unable to load handoffs" message={getErrorMessage(handoffsQuery.error, 'Failed to load handoffs.')} /> : null}
      <div className="ordarr-grid cols-2">
        {handoffs.map((handoff) => (
          <HandoffCard key={handoff.handoffId} handoff={handoff} />
        ))}
      </div>
      {handoffs.length === 0 ? <EmptyState title="No OrdArr handoffs have been created." /> : null}
    </div>
  )
}

function HandoffCard({ handoff }: { handoff: OrdArrHandoff }) {
  return (
    <div className="ordarr-panel">
      <div className="ordarr-panel-inner space-y-2">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <strong className="text-slate-50">{handoff.orderNumber ?? handoff.handoffId}</strong>
          <span className="ordarr-pill">{titleize(handoff.state)}</span>
        </div>
        <p className="text-sm text-slate-300">{handoff.summary}</p>
        <p className="text-sm text-slate-400">
          {handoff.targetProductKey}.{handoff.handoffType} · requested {formatDate(handoff.requestedAt)}
        </p>
      </div>
    </div>
  )
}

function CompletionPage({ accessToken }: { accessToken: string }) {
  const packetsQuery = useQuery({
    queryKey: ['ordarr', 'completion-packets', accessToken],
    queryFn: () => listCompletionPackets(accessToken),
    enabled: Boolean(accessToken),
  })
  const packets = packetsQuery.data ?? []

  return (
    <div className="ordarr-page">
      <PageHeader
        eyebrow="Completion"
        title="Completion and finance packets"
        description="OrdArr assembles completion, invoice-ready, and bill-ready packet state while RecordArr owns persistent files, packages, retention, and audit trail."
      />
      {packetsQuery.isError ? <ApiErrorCallout title="Unable to load packets" message={getErrorMessage(packetsQuery.error, 'Failed to load completion packets.')} /> : null}
      <div className="ordarr-grid cols-2">
        {packets.map((packet) => (
          <PacketCard key={packet.packetId} packet={packet} />
        ))}
      </div>
      {packets.length === 0 ? <EmptyState title="No OrdArr completion or finance packets have been created." /> : null}
    </div>
  )
}

function PacketCard({ packet }: { packet: OrdArrCompletionPacket }) {
  return (
    <div className="ordarr-panel">
      <div className="ordarr-panel-inner space-y-2">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <strong className="text-slate-50">{packet.orderNumber ?? packet.packetId}</strong>
          <span className="ordarr-pill">{titleize(packet.status)}</span>
        </div>
        <p className="text-sm text-slate-300">{titleize(packet.packetType)}</p>
        <p className="text-xs text-slate-400">{packet.recordRefs.length} RecordArr reference(s)</p>
      </div>
    </div>
  )
}

function SettingsPage({
  accessToken,
  session,
}: {
  accessToken: string
  session: StoredOrdArrSession | null
}) {
  return (
    <div className="ordarr-page">
      <PageHeader
        eyebrow="Settings"
        title="Workspace wiring"
        description="Runtime launch, API, and ownership context for the OrdArr product surface."
        action={<span className="ordarr-pill">Live only</span>}
      />
      <div className="ordarr-grid cols-2">
        <Panel title="Runtime" icon={<PackageCheck className="h-4 w-4 text-sky-300" />}>
          <div className="space-y-2 text-sm text-slate-300">
            <p><strong className="text-slate-100">API base:</strong> <span className="ordarr-pill">{apiBase || '/api proxy'}</span></p>
            <p><strong className="text-slate-100">Frontend port:</strong> <span className="ordarr-pill">5187</span></p>
            <p><strong className="text-slate-100">API port:</strong> <span className="ordarr-pill">5112</span></p>
            <p><strong className="text-slate-100">Suite home:</strong> <span className="ordarr-pill">{suiteHomeUrl}</span></p>
            <p><strong className="text-slate-100">Access token present:</strong> <span className="ordarr-pill">{accessToken ? 'yes' : 'no'}</span></p>
            <p><strong className="text-slate-100">Current tenant:</strong> {session?.tenantDisplayName ?? 'n/a'}</p>
          </div>
        </Panel>
        <Panel title="Boundaries" icon={<ClipboardCheck className="h-4 w-4 text-sky-300" />}>
          <div className="space-y-2 text-sm text-slate-300">
            <p>OrdArr owns order/request orchestration, lifecycle, product handoffs, completion packets, invoice-ready packets, and bill-ready packets.</p>
            <p>Customer truth remains in CustomArr, execution truth remains in execution products, files remain in RecordArr, and accounting remains in external finance systems.</p>
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
    queryKey: ['ordarr', 'session', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const launchCatalogQuery = useQuery({
    queryKey: ['ordarr', 'launch-catalog', session?.accessToken],
    queryFn: () => getLaunchCatalog(apiBase, session!.accessToken, 'ordarr'),
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
    currentProductKey: 'ordarr',
    suiteHomeUrl,
    productLaunchUrls,
  })

  if (location.pathname === '/launch' || location.pathname === '/handoff') {
    return <LaunchPage />
  }

  return (
    <ProductWorkspaceFrame
      productName="OrdArr"
      productKey="ordarr"
      workspaceSubtitle="Order and request orchestration"
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
        <Route path="/orders" element={<OrdersPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/handoffs" element={<HandoffsPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/completion" element={<CompletionPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/settings" element={<SettingsPage accessToken={session?.accessToken ?? ''} session={session} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </ProductWorkspaceFrame>
  )
}
