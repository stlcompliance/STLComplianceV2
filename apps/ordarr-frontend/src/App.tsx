import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Activity,
  ArrowRight,
  Clock3,
  ClipboardCheck,
  FileCheck2,
  LayoutDashboard,
  LoaderCircle,
  PackageCheck,
  Plus,
  Settings,
  ShieldAlert,
  SquarePen,
  Truck,
} from 'lucide-react'
import { Link, Navigate, Route, Routes, useLocation, useNavigate, useParams } from 'react-router-dom'
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
  addHold,
  addOrderLine,
  approveOrder,
  cancelOrder,
  createOrder,
  createReturn,
  getDashboard,
  getOrder,
  getReportSummary,
  getSessionBootstrap,
  listCompletionPackets,
  listHandoffs,
  listOrders,
  releaseHold,
  submitOrder,
  type OrdArrCreateOrderRequest,
  type OrdArrHandoff,
  type OrdArrOrderLineRequest,
  type OrdArrReturnRequest,
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
  { label: 'Reports', to: '/reports', icon: Truck as ProductNavItem['icon'] },
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

function badgeTone(value: string): string {
  const lower = value.toLowerCase()
  if (lower.includes('hold') || lower.includes('blocked') || lower.includes('late') || lower.includes('cancel')) {
    return 'ordarr-pill danger'
  }
  if (lower.includes('ready') || lower.includes('accepted') || lower.includes('fulfilled') || lower.includes('closed')) {
    return 'ordarr-pill success'
  }
  if (lower.includes('draft') || lower.includes('submitted') || lower.includes('pending') || lower.includes('review')) {
    return 'ordarr-pill warn'
  }
  return 'ordarr-pill'
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
    <div className="flex flex-col gap-4 border-b border-slate-700/70 pb-5 lg:flex-row lg:items-end lg:justify-between">
      <div className="space-y-2">
        <p className="ordarr-label">{eyebrow}</p>
        <h1 className="text-2xl font-semibold text-slate-50 lg:text-3xl">{title}</h1>
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
  action,
}: {
  title: string
  icon?: ReactNode
  children: ReactNode
  action?: ReactNode
}) {
  return (
    <section className="ordarr-panel">
      <div className="ordarr-panel-inner space-y-4">
        <div className="flex items-center justify-between gap-3">
          <div className="flex items-center gap-2">
            {icon}
            <h2 className="text-base font-semibold text-slate-50">{title}</h2>
          </div>
          {action}
        </div>
        {children}
      </div>
    </section>
  )
}

function MetricCard({
  label,
  value,
  hint,
  tone = 'neutral',
}: {
  label: string
  value: string | number
  hint: string
  tone?: 'neutral' | 'good' | 'warn' | 'bad'
}) {
  return (
    <div className={`ordarr-panel ordarr-metric ${tone}`}>
      <div className="ordarr-panel-inner">
        <p className="ordarr-label">{label}</p>
        <p className="mt-2 text-3xl font-semibold text-slate-50">{value}</p>
        <p className="mt-2 text-sm text-slate-300">{hint}</p>
      </div>
    </div>
  )
}

function EmptyState({ title, detail }: { title: string; detail?: string }) {
  return (
    <div className="rounded-lg border border-dashed border-slate-700/80 bg-slate-950/40 p-4 text-sm text-slate-400">
      <p className="font-medium text-slate-200">{title}</p>
      {detail ? <p className="mt-1 text-slate-400">{detail}</p> : null}
    </div>
  )
}

function DashboardPage({ accessToken }: { accessToken: string }) {
  const dashboardQuery = useQuery({
    queryKey: ['ordarr', 'dashboard', accessToken],
    queryFn: () => getDashboard(accessToken),
    enabled: Boolean(accessToken),
  })
  const reportQuery = useQuery({
    queryKey: ['ordarr', 'report-summary', accessToken],
    queryFn: () => getReportSummary(accessToken),
    enabled: Boolean(accessToken),
  })

  const dashboard = dashboardQuery.data
  const report = reportQuery.data

  const attentionItems = useMemo(() => {
    const items: Array<{ title: string; detail: string; tone: 'bad' | 'warn' | 'neutral' }> = []
    if (dashboard?.blockedOrderCount) {
      items.push({
        title: `${dashboard.blockedOrderCount} blocked order(s)`,
        detail: 'Open holds, compliance blockers, or target handoffs are preventing release.',
        tone: 'bad',
      })
    }
    if (dashboard?.lateOrderCount) {
      items.push({
        title: `${dashboard.lateOrderCount} late order(s)`,
        detail: 'Promised windows are in the past for these open orders.',
        tone: 'warn',
      })
    }
    if (dashboard?.openHoldCount) {
      items.push({
        title: `${dashboard.openHoldCount} open hold(s)`,
        detail: 'Release permissions and comments should be reviewed before downstream release.',
        tone: 'warn',
      })
    }
    return items
  }, [dashboard?.blockedOrderCount, dashboard?.lateOrderCount, dashboard?.openHoldCount])

  return (
    <div className="ordarr-page">
      <PageHeader
        eyebrow="OrdArr"
        title="Order orchestration console"
        description="Track order lifecycle, holds, handoffs, completion packets, returns, and finance handoff readiness without owning customer or operational execution truth."
        action={
          <Link to="/orders/new" className="ordarr-button">
            <Plus className="h-4 w-4" />
            New order
          </Link>
        }
      />
      {dashboardQuery.isError ? (
        <ApiErrorCallout title="Unable to load dashboard" message={getErrorMessage(dashboardQuery.error, 'Failed to load OrdArr dashboard.')} />
      ) : null}

      <div className="grid gap-4 lg:grid-cols-6">
        <MetricCard label="Open orders" value={dashboard?.openOrderCount ?? '—'} hint="Orders still in active orchestration" />
        <MetricCard label="Open holds" value={dashboard?.openHoldCount ?? '—'} hint="Holds awaiting release" />
        <MetricCard label="Blocked" value={dashboard?.blockedOrderCount ?? '—'} hint="Orders waiting on blockers" tone="bad" />
        <MetricCard label="Late" value={dashboard?.lateOrderCount ?? '—'} hint="Orders past promised windows" tone="warn" />
        <MetricCard label="Returns" value={dashboard?.returnCount ?? '—'} hint="RMA or return records in scope" />
        <MetricCard label="Handoffs" value={dashboard?.activeHandoffCount ?? '—'} hint="Open execution handoffs" />
      </div>

      <div className="grid gap-4 xl:grid-cols-3">
        <Panel title="Primary operational view" icon={<LayoutDashboard className="h-4 w-4 text-sky-300" />}>
          {dashboard ? (
            <div className="space-y-3">
              <div className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-4">
                <div className="flex flex-wrap items-center gap-2">
                  <span className="ordarr-pill">Live</span>
                  <span className="ordarr-pill">{formatDate(dashboard.generatedAt)}</span>
                  <span className="ordarr-pill">{dashboard.orderCount} order(s) tracked</span>
                </div>
                <p className="mt-3 text-sm text-slate-300">
                  OrdArr owns the commercial order promise, status coordination, and handoff choreography.
                </p>
              </div>
              <div className="space-y-2">
                {dashboard.featuredOrders.slice(0, 4).map((order) => (
                  <Link key={order.orderId} to={`/orders/${order.orderId}`} className="ordarr-list-row">
                    <div>
                      <p className="font-medium text-slate-50">{order.orderNumber}</p>
                      <p className="text-xs text-slate-400">{order.customerName}</p>
                    </div>
                    <span className={badgeTone(order.lifecycleStatus)}>{titleize(order.lifecycleStatus)}</span>
                  </Link>
                ))}
                {dashboard.featuredOrders.length === 0 ? (
                  <EmptyState title="No orders yet." detail="Create an order to start orchestrating a commercial request." />
                ) : null}
              </div>
            </div>
          ) : (
            <EmptyState title="Dashboard unavailable." detail="No dashboard response returned from OrdArr." />
          )}
        </Panel>

        <Panel title="Attention required" icon={<ShieldAlert className="h-4 w-4 text-amber-300" />}>
          <div className="space-y-3">
            {attentionItems.length > 0 ? (
              attentionItems.map((item) => (
                <div key={item.title} className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-4">
                  <p className={`text-sm font-semibold ${item.tone === 'bad' ? 'text-red-200' : 'text-amber-100'}`}>{item.title}</p>
                  <p className="mt-1 text-sm text-slate-300">{item.detail}</p>
                </div>
              ))
            ) : (
              <EmptyState title="No blockers detected." detail="OrdArr has no current hold or lateness alerts." />
            )}
          </div>
        </Panel>

        <Panel title="Reporting snapshot" icon={<Truck className="h-4 w-4 text-sky-300" />}>
          {report ? (
            <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-1">
              <MetricCard label="Fill rate" value={`${report.fillRatePercent}%`} hint="Closed orders over total orders" tone="good" />
              <MetricCard label="On-time" value={`${report.onTimePercent}%`} hint="Orders within promised windows" tone="good" />
              <MetricCard label="Lines" value={report.lineCount} hint="Tracked order lines across active records" />
              <MetricCard label="Open handoffs" value={report.activeHandoffCount} hint="Requests waiting on target products" />
            </div>
          ) : (
            <EmptyState title="Report summary unavailable." detail="The report endpoint did not return data." />
          )}
        </Panel>
      </div>

      <Panel title="Recent activity" icon={<Activity className="h-4 w-4 text-sky-300" />}>
        {dashboard?.recentActivity.length ? (
          <div className="space-y-3">
            {dashboard.recentActivity.map((item) => (
              <div key={item.activityId} className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <strong className="text-sm text-slate-50">{item.orderNumber}</strong>
                  <span className="ordarr-pill">{titleize(item.eventType)}</span>
                </div>
                <p className="mt-2 text-sm text-slate-300">{item.message}</p>
                <p className="mt-2 text-xs text-slate-400">{formatDate(item.occurredAt)}</p>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState title="No recent activity yet." detail="Activity appears once orders are created or updated." />
        )}
      </Panel>
    </div>
  )
}

function OrdersPage({ accessToken }: { accessToken: string }) {
  const [searchParams, setSearchParams] = useState(() => new URLSearchParams(window.location.search))
  const status = searchParams.get('status') ?? ''
  const ordersQuery = useQuery({
    queryKey: ['ordarr', 'orders', accessToken, status],
    queryFn: () => listOrders(accessToken),
    enabled: Boolean(accessToken),
  })
  const orders = ordersQuery.data ?? []
  const visibleOrders = status ? orders.filter((order) => order.lifecycleStatus === status) : orders

  return (
    <div className="ordarr-page">
      <PageHeader
        eyebrow="Orders"
        title="Order register"
        description="Review the parent request record, its lifecycle state, and the latest handoff and hold status."
        action={
          <Link to="/orders/new" className="ordarr-button">
            <Plus className="h-4 w-4" />
            Create order
          </Link>
        }
      />
      {ordersQuery.isError ? <ApiErrorCallout title="Unable to load orders" message={getErrorMessage(ordersQuery.error, 'Failed to load orders.')} /> : null}
      <div className="ordarr-filterbar">
        <label className="ordarr-field">
          <span>Status</span>
          <select
            value={status}
            onChange={(event) => {
              const next = new URLSearchParams(searchParams)
              if (event.target.value) {
                next.set('status', event.target.value)
              } else {
                next.delete('status')
              }
              setSearchParams(next)
              window.history.replaceState(null, '', `${window.location.pathname}${next.toString() ? `?${next.toString()}` : ''}`)
            }}
          >
            <option value="">All statuses</option>
            {Array.from(new Set(orders.map((order) => order.lifecycleStatus))).map((value) => (
              <option key={value} value={value}>
                {titleize(value)}
              </option>
            ))}
          </select>
        </label>
        <div className="text-xs text-slate-400">
          {visibleOrders.length} of {orders.length} order(s) shown
        </div>
      </div>

      <Panel title="Orders" icon={<ClipboardCheck className="h-4 w-4 text-sky-300" />}>
        {visibleOrders.length ? (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="text-slate-400">
                <tr>
                  <th className="px-4 py-3">Order</th>
                  <th className="px-4 py-3">Customer</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3">Holds</th>
                  <th className="px-4 py-3">Updated</th>
                  <th className="px-4 py-3 text-right">Open</th>
                </tr>
              </thead>
              <tbody>
                {visibleOrders.map((order) => (
                  <tr key={order.orderId} className="border-t border-slate-800 align-top">
                    <td className="px-4 py-4">
                      <p className="font-medium text-slate-50">{order.orderNumber}</p>
                      <p className="mt-1 text-xs text-slate-400">{titleize(order.orderType)} via {titleize(order.sourceChannel)}</p>
                    </td>
                    <td className="px-4 py-4 text-slate-300">
                      <p>{order.customerName}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">{order.customerRef.productKey}:{order.customerRef.objectType}:{order.customerRef.objectId}</p>
                    </td>
                    <td className="px-4 py-4">
                      <div className="space-y-2">
                        <span className={badgeTone(order.lifecycleStatus)}>{titleize(order.lifecycleStatus)}</span>
                        <p className="text-xs text-slate-400">{titleize(order.nextAction)}</p>
                      </div>
                    </td>
                    <td className="px-4 py-4 text-slate-300">{order.holdCount}</td>
                    <td className="px-4 py-4 text-slate-400">{formatDate(order.updatedAt)}</td>
                    <td className="px-4 py-4 text-right">
                      <Link className="ordarr-link" to={`/orders/${order.orderId}`}>
                        Open detail
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <EmptyState title="No orders match the current filter." detail="Create a new order or clear the filter to see all records." />
        )}
      </Panel>
    </div>
  )
}

export function OrderDetailPage({ accessToken }: { accessToken: string }) {
  const { orderId } = useParams<{ orderId: string }>()
  const queryClient = useQueryClient()
  const [lineDraft, setLineDraft] = useState<OrdArrOrderLineRequest>({
    lineType: 'item',
    description: '',
    quantity: 1,
    unitOfMeasure: 'ea',
    targetProductKey: 'loadarr',
    taxable: true,
    allowSubstitution: true,
    canCancel: true,
    canReturn: true,
  })
  const [holdDraft, setHoldDraft] = useState({
    holdType: 'compliance',
    reason: '',
    ownerProductKey: 'compliancecore',
    releasePermission: 'ordarr.order_requests.update',
    comment: '',
    ownerPersonId: '',
  })
  const [returnDraft, setReturnDraft] = useState<OrdArrReturnRequest>({
    returnType: 'rma',
    reason: '',
    quantity: 1,
    orderLineIds: [],
    notes: '',
    sourceReference: '',
  })

  const orderQuery = useQuery({
    queryKey: ['ordarr', 'order', orderId, accessToken],
    queryFn: () => getOrder(accessToken, orderId!),
    enabled: Boolean(accessToken && orderId),
  })
  const order = orderQuery.data

  const refresh = async () => {
    await queryClient.invalidateQueries({ queryKey: ['ordarr'] })
  }

  const submitMutation = useMutation({
    mutationFn: async () => {
      if (!orderId) {
        return null
      }
      return submitOrder(accessToken, orderId, { comment: 'Submitted from OrdArr detail' }, crypto.randomUUID())
    },
    onSuccess: refresh,
  })
  const approveMutation = useMutation({
    mutationFn: async () => {
      if (!orderId) {
        return null
      }
      return approveOrder(
        accessToken,
        orderId,
        {
          promisedWindowStart: order?.promisedWindowStart ?? undefined,
          promisedWindowEnd: order?.promisedWindowEnd ?? undefined,
          fulfillmentProductKeys: ['loadarr', 'routarr'],
          reason: 'Approved in OrdArr workspace',
        },
        crypto.randomUUID(),
      )
    },
    onSuccess: refresh,
  })
  const holdMutation = useMutation({
    mutationFn: async (draft: typeof holdDraft) => {
      if (!orderId) {
        return null
      }
      return addHold(accessToken, orderId, draft, crypto.randomUUID())
    },
    onSuccess: refresh,
  })
  const releaseMutation = useMutation({
    mutationFn: async (holdId: string) => {
      if (!orderId) {
        return null
      }
      return releaseHold(accessToken, orderId, holdId, { comment: 'Released from OrdArr workspace' }, crypto.randomUUID())
    },
    onSuccess: refresh,
  })
  const lineMutation = useMutation({
    mutationFn: async () => {
      if (!orderId) {
        return null
      }
      return addOrderLine(accessToken, orderId, lineDraft, crypto.randomUUID())
    },
    onSuccess: refresh,
  })
  const returnMutation = useMutation({
    mutationFn: async () => {
      if (!orderId) {
        return null
      }
      return createReturn(
        accessToken,
        orderId,
        {
          ...returnDraft,
          orderLineIds: returnDraft.orderLineIds?.filter(Boolean) ?? [],
        },
        crypto.randomUUID(),
      )
    },
    onSuccess: refresh,
  })
  const cancelMutation = useMutation({
    mutationFn: async () => {
      if (!orderId) {
        return null
      }
      return cancelOrder(accessToken, orderId, { reason: 'Cancelled from OrdArr workspace' }, crypto.randomUUID())
    },
    onSuccess: refresh,
  })

  if (!orderId) {
    return <Navigate to="/orders" replace />
  }

  if (orderQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading order detail…</p>
  }

  if (orderQuery.isError || !order) {
    return <ApiErrorCallout title="Unable to load order" message={getErrorMessage(orderQuery.error, 'Failed to load the selected order.')} />
  }

  return (
    <div className="ordarr-page">
      <PageHeader
        eyebrow="Order detail"
        title={order.orderNumber}
        description={order.summary}
        action={
          <div className="flex flex-wrap gap-2">
            <button type="button" className="ordarr-button" onClick={() => submitMutation.mutate()} disabled={submitMutation.isPending}>
              {submitMutation.isPending ? <LoaderCircle className="h-4 w-4 animate-spin" /> : <ArrowRight className="h-4 w-4" />}
              Submit
            </button>
            <button type="button" className="ordarr-button" onClick={() => approveMutation.mutate()} disabled={approveMutation.isPending}>
              <SquarePen className="h-4 w-4" />
              Approve
            </button>
            <button
              type="button"
              className="ordarr-button"
              onClick={() => holdMutation.mutate({ ...holdDraft, reason: holdDraft.reason || 'Manual hold from OrdArr workspace' })}
              disabled={holdMutation.isPending}
            >
              <ShieldAlert className="h-4 w-4" />
              Hold
            </button>
            <button type="button" className="ordarr-button danger" onClick={() => cancelMutation.mutate()} disabled={cancelMutation.isPending}>
              Cancel
            </button>
          </div>
        }
      />

      <div className="grid gap-4 lg:grid-cols-4">
        <MetricCard label="Lifecycle" value={titleize(order.lifecycleStatus)} hint="OrdArr-owned lifecycle state" />
        <MetricCard label="Approval" value={titleize(order.approvalState)} hint="Approval and release position" />
        <MetricCard label="Lines" value={order.lineCount} hint="Order lines attached to this request" />
        <MetricCard label="Holds" value={order.holdCount} hint="Open holds blocking release" tone={order.holdCount ? 'warn' : 'good'} />
      </div>

      <div className="grid gap-4 xl:grid-cols-2">
        <Panel title="Header" icon={<ClipboardCheck className="h-4 w-4 text-sky-300" />}>
          <div className="grid gap-3 md:grid-cols-2">
            <DetailField label="Customer" value={order.customerName} />
            <DetailField label="Customer ref" value={`${order.customerRef.productKey}:${order.customerRef.objectType}:${order.customerRef.objectId}`} />
            <DetailField label="Source" value={`${titleize(order.sourceChannel)} / ${titleize(order.orderType)}`} />
            <DetailField label="Priority" value={titleize(order.priority)} />
            <DetailField label="Requested" value={formatDate(order.requestedAt)} />
            <DetailField label="Updated" value={formatDate(order.updatedAt)} />
            <DetailField label="Payment terms" value={order.paymentTerms ?? 'n/a'} />
            <DetailField label="Shipping preference" value={order.shippingMethodPreference ?? 'n/a'} />
          </div>
          {order.customerNotes ? <p className="mt-3 text-sm text-slate-300">{order.customerNotes}</p> : null}
          {order.internalNotes ? <p className="mt-2 text-xs text-slate-400">Internal: {order.internalNotes}</p> : null}
        </Panel>

        <Panel title="Timeline" icon={<Clock3 className="h-4 w-4 text-sky-300" />}>
          <div className="space-y-3">
            {order.timeline.map((entry) => (
              <div key={entry.timelineId} className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className={badgeTone(entry.status)}>{titleize(entry.status)}</span>
                  <span className="ordarr-pill">{titleize(entry.eventType)}</span>
                </div>
                <p className="mt-2 text-sm text-slate-300">{entry.message}</p>
                <p className="mt-2 text-xs text-slate-400">{formatDate(entry.occurredAt)} · {entry.sourceProductKey}</p>
              </div>
            ))}
            {order.timeline.length === 0 ? <EmptyState title="No timeline entries yet." /> : null}
          </div>
        </Panel>
      </div>

      <div className="grid gap-4 xl:grid-cols-3">
        <Panel title="Lines" icon={<ClipboardCheck className="h-4 w-4 text-sky-300" />}>
          <div className="space-y-3">
            {order.lines.map((line) => (
              <div key={line.orderLineId} className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <strong className="text-sm text-slate-50">
                    #{line.lineNumber} {line.description}
                  </strong>
                  <span className={badgeTone(line.fulfillmentStatus)}>{titleize(line.fulfillmentStatus)}</span>
                </div>
                <p className="mt-2 text-sm text-slate-300">
                  {line.quantity} {line.unitOfMeasure} · {titleize(line.lineType)} · {line.targetProductKey ?? 'no target'}
                </p>
                {line.complianceFlag ? <p className="mt-2 text-xs text-amber-200">Compliance: {line.complianceFlag}</p> : null}
              </div>
            ))}
            {order.lines.length === 0 ? <EmptyState title="No lines yet." detail="Use the add-line form to attach order demand." /> : null}
          </div>
          <div className="ordarr-form">
            <MiniField label="Line type" value={lineDraft.lineType} onChange={(value) => setLineDraft((draft) => ({ ...draft, lineType: value }))} />
            <MiniField label="Description" value={lineDraft.description} onChange={(value) => setLineDraft((draft) => ({ ...draft, description: value }))} />
            <div className="grid gap-3 md:grid-cols-2">
              <MiniField label="Qty" value={String(lineDraft.quantity)} onChange={(value) => setLineDraft((draft) => ({ ...draft, quantity: Number(value) || 1 }))} />
              <MiniField label="UOM" value={lineDraft.unitOfMeasure} onChange={(value) => setLineDraft((draft) => ({ ...draft, unitOfMeasure: value }))} />
            </div>
            <MiniField label="Target product" value={lineDraft.targetProductKey ?? ''} onChange={(value) => setLineDraft((draft) => ({ ...draft, targetProductKey: value || null }))} />
            <button type="button" className="ordarr-button" onClick={() => lineMutation.mutate()} disabled={lineMutation.isPending}>
              <Plus className="h-4 w-4" />
              Add line
            </button>
          </div>
        </Panel>

        <Panel title="Holds" icon={<ShieldAlert className="h-4 w-4 text-amber-300" />}>
          <div className="space-y-3">
            {order.holds.map((hold) => (
              <div key={hold.holdId} className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className={badgeTone(hold.status)}>{titleize(hold.status)}</span>
                  <span className="ordarr-pill">{titleize(hold.holdType)}</span>
                </div>
                <p className="mt-2 text-sm text-slate-300">{hold.reason}</p>
                <p className="mt-2 text-xs text-slate-400">Owner: {hold.ownerProductKey} · Release: {hold.releasePermission}</p>
                {hold.status === 'open' ? (
                  <button type="button" className="mt-3 ordarr-button" onClick={() => releaseMutation.mutate(hold.holdId)} disabled={releaseMutation.isPending}>
                    Release hold
                  </button>
                ) : null}
              </div>
            ))}
            {order.holds.length === 0 ? <EmptyState title="No holds recorded." detail="Add a hold if the order must pause for review." /> : null}
          </div>
          <div className="ordarr-form">
            <MiniField label="Hold type" value={holdDraft.holdType} onChange={(value) => setHoldDraft((draft) => ({ ...draft, holdType: value }))} />
            <MiniField label="Reason" value={holdDraft.reason} onChange={(value) => setHoldDraft((draft) => ({ ...draft, reason: value }))} />
            <MiniField label="Owner product" value={holdDraft.ownerProductKey} onChange={(value) => setHoldDraft((draft) => ({ ...draft, ownerProductKey: value }))} />
            <button type="button" className="ordarr-button" onClick={() => holdMutation.mutate(holdDraft)} disabled={holdMutation.isPending}>
              <ShieldAlert className="h-4 w-4" />
              Add hold
            </button>
          </div>
        </Panel>

        <Panel title="Returns / RMAs" icon={<FileCheck2 className="h-4 w-4 text-sky-300" />}>
          <div className="space-y-3">
            {order.returns.map((item) => (
              <div key={item.returnId} className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <strong className="text-sm text-slate-50">{item.returnNumber}</strong>
                  <span className={badgeTone(item.status)}>{titleize(item.status)}</span>
                </div>
                <p className="mt-2 text-sm text-slate-300">{item.reason}</p>
                <p className="mt-2 text-xs text-slate-400">{item.quantity} line(s) · {titleize(item.returnType)}</p>
              </div>
            ))}
            {order.returns.length === 0 ? <EmptyState title="No returns yet." detail="Capture a basic return/RMA record here." /> : null}
          </div>
          <div className="ordarr-form">
            <MiniField label="Return type" value={returnDraft.returnType} onChange={(value) => setReturnDraft((draft) => ({ ...draft, returnType: value }))} />
            <MiniField label="Reason" value={returnDraft.reason} onChange={(value) => setReturnDraft((draft) => ({ ...draft, reason: value }))} />
            <MiniField label="Quantity" value={String(returnDraft.quantity)} onChange={(value) => setReturnDraft((draft) => ({ ...draft, quantity: Number(value) || 1 }))} />
            <button type="button" className="ordarr-button" onClick={() => returnMutation.mutate()} disabled={returnMutation.isPending}>
              <FileCheck2 className="h-4 w-4" />
              Create return
            </button>
          </div>
        </Panel>
      </div>

      <Panel title="Handoffs" icon={<Truck className="h-4 w-4 text-sky-300" />}>
        {order.handoffs.length ? (
          <div className="space-y-3">
            {order.handoffs.map((handoff) => (
              <div key={handoff.handoffId} className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-4">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <strong className="text-sm text-slate-50">{handoff.targetProductKey}</strong>
                  <span className={badgeTone(handoff.state)}>{titleize(handoff.state)}</span>
                </div>
                <p className="mt-2 text-sm text-slate-300">{handoff.summary}</p>
                <p className="mt-2 text-xs text-slate-400">{handoff.handoffType} · requested {formatDate(handoff.requestedAt)}</p>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState title="No handoffs yet." detail="Approve the order to create downstream execution handoffs." />
        )}
      </Panel>
    </div>
  )
}

function CreateOrderPage({ accessToken }: { accessToken: string }) {
  const navigate = useNavigate()
  const [form, setForm] = useState<OrdArrCreateOrderRequest>({
    customerRef: { productKey: 'customarr', objectType: 'customer', objectId: '', objectNumber: '' },
    customerName: '',
    requestType: 'customer_order',
    ownerPersonId: '',
    summary: '',
    requestedWindowStart: '',
    requestedWindowEnd: '',
    promisedWindowStart: '',
    promisedWindowEnd: '',
    sourceChannel: 'manual_entry',
    orderType: 'customer_order',
    priority: 'normal',
    lines: [
      {
        lineType: 'item',
        description: '',
        quantity: 1,
        unitOfMeasure: 'ea',
      },
    ],
  })
  const createMutation = useMutation({
    mutationFn: () => createOrder(accessToken, form, crypto.randomUUID()),
    onSuccess: async (order) => {
      await navigate(`/orders/${order.orderId}`)
    },
  })

  return (
    <div className="ordarr-page">
      <PageHeader
        eyebrow="Create"
        title="Create order"
        description="Start an OrdArr order from a customer reference, a source channel, and one or more order lines."
      />
      <Panel title="Order intake" icon={<Plus className="h-4 w-4 text-sky-300" />}>
        <div className="grid gap-4 md:grid-cols-2">
          <MiniField label="Customer name" value={form.customerName} onChange={(value) => setForm((draft) => ({ ...draft, customerName: value }))} />
          <MiniField label="Customer id" value={form.customerRef.objectId} onChange={(value) => setForm((draft) => ({ ...draft, customerRef: { ...draft.customerRef, objectId: value } }))} />
          <MiniField label="Buyer PO" value={form.buyerPoNumber ?? ''} onChange={(value) => setForm((draft) => ({ ...draft, buyerPoNumber: value || null }))} />
          <MiniField label="Owner person" value={form.ownerPersonId} onChange={(value) => setForm((draft) => ({ ...draft, ownerPersonId: value }))} />
          <MiniField label="Source channel" value={form.sourceChannel ?? 'manual_entry'} onChange={(value) => setForm((draft) => ({ ...draft, sourceChannel: value }))} />
          <MiniField label="Priority" value={form.priority ?? 'normal'} onChange={(value) => setForm((draft) => ({ ...draft, priority: value }))} />
        </div>
        <MiniField label="Summary" value={form.summary} onChange={(value) => setForm((draft) => ({ ...draft, summary: value }))} />
        <div className="grid gap-4 md:grid-cols-2">
          <MiniField label="Requested window start" value={form.requestedWindowStart ?? ''} onChange={(value) => setForm((draft) => ({ ...draft, requestedWindowStart: value || null }))} />
          <MiniField label="Requested window end" value={form.requestedWindowEnd ?? ''} onChange={(value) => setForm((draft) => ({ ...draft, requestedWindowEnd: value || null }))} />
          <MiniField label="Promised window start" value={form.promisedWindowStart ?? ''} onChange={(value) => setForm((draft) => ({ ...draft, promisedWindowStart: value || null }))} />
          <MiniField label="Promised window end" value={form.promisedWindowEnd ?? ''} onChange={(value) => setForm((draft) => ({ ...draft, promisedWindowEnd: value || null }))} />
        </div>
        <div className="space-y-3 rounded-xl border border-slate-700/70 bg-slate-950/60 p-4">
          <p className="text-sm font-semibold text-slate-100">Initial line</p>
          <MiniField
            label="Description"
            value={form.lines?.[0]?.description ?? ''}
            onChange={(value) =>
              setForm((draft) => ({
                ...draft,
                lines: [{ ...(draft.lines?.[0] ?? { lineType: 'item', quantity: 1, unitOfMeasure: 'ea' }), description: value }],
              }))
            }
          />
          <div className="grid gap-4 md:grid-cols-3">
            <MiniField
              label="Line type"
              value={form.lines?.[0]?.lineType ?? 'item'}
              onChange={(value) =>
                setForm((draft) => ({
                  ...draft,
                  lines: [{ ...(draft.lines?.[0] ?? { description: '', quantity: 1, unitOfMeasure: 'ea' }), lineType: value }],
                }))
              }
            />
            <MiniField
              label="Quantity"
              value={String(form.lines?.[0]?.quantity ?? 1)}
              onChange={(value) =>
                setForm((draft) => ({
                  ...draft,
                  lines: [{ ...(draft.lines?.[0] ?? { description: '', lineType: 'item', unitOfMeasure: 'ea' }), quantity: Number(value) || 1 }],
                }))
              }
            />
            <MiniField
              label="UOM"
              value={form.lines?.[0]?.unitOfMeasure ?? 'ea'}
              onChange={(value) =>
                setForm((draft) => ({
                  ...draft,
                  lines: [{ ...(draft.lines?.[0] ?? { description: '', lineType: 'item', quantity: 1 }), unitOfMeasure: value }],
                }))
              }
            />
          </div>
          <button type="button" className="ordarr-button" onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
            {createMutation.isPending ? <LoaderCircle className="h-4 w-4 animate-spin" /> : <Plus className="h-4 w-4" />}
            Create order
          </button>
        </div>
      </Panel>
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
        description="OrdArr requests downstream execution, but the target products own acceptance and execution truth."
      />
      {handoffsQuery.isError ? <ApiErrorCallout title="Unable to load handoffs" message={getErrorMessage(handoffsQuery.error, 'Failed to load handoffs.')} /> : null}
      <Panel title="Handoffs" icon={<Truck className="h-4 w-4 text-sky-300" />}>
        <div className="space-y-3">
          {handoffs.map((handoff: OrdArrHandoff) => (
            <div key={handoff.handoffId} className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <strong className="text-slate-50">{handoff.orderNumber ?? handoff.handoffId}</strong>
                <span className={badgeTone(handoff.state)}>{titleize(handoff.state)}</span>
              </div>
              <p className="mt-2 text-sm text-slate-300">{handoff.summary}</p>
              <p className="mt-2 text-xs text-slate-400">
                {handoff.targetProductKey}.{handoff.handoffType} · requested {formatDate(handoff.requestedAt)}
              </p>
            </div>
          ))}
          {handoffs.length === 0 ? <EmptyState title="No handoffs yet." detail="Approve an order to generate downstream demand." /> : null}
        </div>
      </Panel>
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
        description="OrdArr assembles operational closeout artifacts while RecordArr stores the retained files and external finance systems own the money truth."
      />
      {packetsQuery.isError ? <ApiErrorCallout title="Unable to load packets" message={getErrorMessage(packetsQuery.error, 'Failed to load completion packets.')} /> : null}
      <Panel title="Packets" icon={<FileCheck2 className="h-4 w-4 text-sky-300" />}>
        <div className="space-y-3">
          {packets.map((packet) => (
            <div key={packet.packetId} className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <strong className="text-slate-50">{packet.orderNumber ?? packet.packetId}</strong>
                <span className={badgeTone(packet.status)}>{titleize(packet.status)}</span>
              </div>
              <p className="mt-2 text-sm text-slate-300">{titleize(packet.packetType)}</p>
              <p className="mt-2 text-xs text-slate-400">{packet.recordRefs.length} RecordArr reference(s)</p>
            </div>
          ))}
          {packets.length === 0 ? <EmptyState title="No packets yet." detail="Completion packet data appears once orders advance." /> : null}
        </div>
      </Panel>
    </div>
  )
}

function ReportsPage({ accessToken }: { accessToken: string }) {
  const reportQuery = useQuery({
    queryKey: ['ordarr', 'reports', accessToken],
    queryFn: () => getReportSummary(accessToken),
    enabled: Boolean(accessToken),
  })
  const report = reportQuery.data

  return (
    <div className="ordarr-page">
      <PageHeader
        eyebrow="Reports"
        title="Operational reporting"
        description="Use OrdArr reporting for backlog, open orders, holds, returns, and lifecycle visibility. Corrections still happen in the owning product."
      />
      {reportQuery.isError ? <ApiErrorCallout title="Unable to load reporting" message={getErrorMessage(reportQuery.error, 'Failed to load OrdArr reporting.')} /> : null}
      {report ? (
        <div className="grid gap-4 lg:grid-cols-3">
          <MetricCard label="Orders" value={report.orderCount} hint="All tracked order/request records" />
          <MetricCard label="Open orders" value={report.openOrderCount} hint="Orders still in active orchestration" />
          <MetricCard label="Closed orders" value={report.closedOrderCount} hint="Orders in closed lifecycle state" />
          <MetricCard label="Blocked" value={report.blockedOrderCount} hint="Orders that cannot progress yet" tone="bad" />
          <MetricCard label="Late" value={report.lateOrderCount} hint="Orders past promised dates" tone="warn" />
          <MetricCard label="Fill rate" value={`${report.fillRatePercent}%`} hint="Closed orders vs total orders" tone="good" />
        </div>
      ) : (
        <EmptyState title="No report summary available." detail="The reporting endpoint returned no data." />
      )}
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
      <div className="grid gap-4 lg:grid-cols-2">
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
            <p>OrdArr owns order/request orchestration, lifecycle, handoffs, returns basics, and completion packet coordination.</p>
            <p>Customer truth remains in CustomArr, inventory truth remains in LoadArr, execution truth remains in execution products, files remain in RecordArr, and accounting remains in LedgArr.</p>
          </div>
        </Panel>
      </div>
    </div>
  )
}

function MiniField({
  label,
  value,
  onChange,
}: {
  label: string
  value: string
  onChange: (value: string) => void
}) {
  return (
    <label className="ordarr-field">
      <span>{label}</span>
      <input value={value} onChange={(event) => onChange(event.target.value)} />
    </label>
  )
}

function DetailField({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-slate-700/70 bg-slate-950/70 p-3">
      <p className="text-xs uppercase tracking-[0.12em] text-sky-200/80">{label}</p>
      <p className="mt-2 text-sm text-slate-100">{value}</p>
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
          userId: session.userId,
          tenantId: session.tenantId,
          userDisplayName: session.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
        }
      : null

  const launch = useProductWorkspaceLaunch({
    apiBase,
    accessToken: session?.accessToken ?? '',
    currentProductKey: 'ordarr',
    suiteHomeUrl,
    productLaunchUrls,
  })

  if (location.pathname === '/handoff') {
    return <Navigate replace to={{ pathname: '/launch', search: location.search }} />
  }

  if (location.pathname === '/launch') {
    return <LaunchPage />
  }

  return (
    <ProductWorkspaceFrame
      productName="OrdArr"
      productKey="ordarr"
      workspaceSubtitle="Order and request orchestration"
      navItems={navItems}
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
        <Route path="/orders/new" element={<CreateOrderPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/orders/:orderId" element={<OrderDetailPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/handoffs" element={<HandoffsPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/completion" element={<CompletionPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/reports" element={<ReportsPage accessToken={session?.accessToken ?? ''} />} />
        <Route path="/settings" element={<SettingsPage accessToken={session?.accessToken ?? ''} session={session} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </ProductWorkspaceFrame>
  )
}
