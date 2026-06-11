import { useMutation, useQuery } from '@tanstack/react-query'
import {
  AlertTriangle,
  ArrowRightLeft,
  CheckCircle2,
  ExternalLink,
  FileText,
  PackageCheck,
  Route,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import { Navigate, useParams } from 'react-router-dom'
import { DetailBadge, DetailEmptyState, ProfileDetailsLayout, type DetailRailSectionConfig } from '@stl/shared-ui'
import type { RegisterVendorOrderDocumentRequest, UpdateVendorOrderStatusRequest } from '../../api/types'
import {
  createVendorOrderBrokerDecision,
  createVendorOrderMagicLink,
  getVendorOrder,
  getVendorOrders,
  getVendorOrderHistory,
  registerVendorOrderDocument,
  sendVendorOrderToVendor,
  splitVendorOrder,
  submitVendorOrderStatus,
} from '../../api/vendorOrderClient'
import { listRoutArrTripsByVendorOrder } from '../../api/routarrReferenceClient'
import { useSupplyArrPageAccess } from './useSupplyArrPageAccess'
import {
  formatVendorOrderDateTime,
  humanizeVendorOrderValue,
  quantitySummary,
  vendorOrderStatusTone,
} from './vendorOrderUi'

const INTERNAL_STATUS_OPTIONS = [
  'draft',
  'sent_to_vendor',
  'pending_vendor_acknowledgment',
  'acknowledged',
  'in_progress',
  'partially_ready',
  'completed_ready_for_dispatch',
  'unable_to_fulfill',
  'cancelled',
  'closed',
]

const DOCUMENT_TYPES = [
  'photo',
  'packing_slip',
  'bill_of_lading',
  'scale_ticket',
  'proof_of_readiness',
  'other',
]

export function VendorOrderDetailPage() {
  const { vendorOrderId } = useParams<{ vendorOrderId: string }>()
  const { session, meQuery, canReadVendorOrders, canUpdateVendorOrders } = useSupplyArrPageAccess()
  const [latestMagicLink, setLatestMagicLink] = useState<{
    url: string
    expiresAt?: string | null
    description?: string
  } | null>(null)
  const [statusDraft, setStatusDraft] = useState<UpdateVendorOrderStatusRequest>({
    newStatus: 'in_progress',
    quantityReady: null,
    estimatedReadyAt: null,
    confirmedReadyAt: null,
    pickupWindowStart: null,
    pickupWindowEnd: null,
    note: null,
    exceptionReason: null,
    readyForPickupConfirmed: false,
  })
  const [documentDraft, setDocumentDraft] = useState<RegisterVendorOrderDocumentRequest>({
    documentType: 'photo',
    fileName: '',
    contentType: '',
    storageProvider: 'shared_blob',
    storageKey: '',
    sizeBytes: null,
  })
  const [decisionType, setDecisionType] = useState('wait_full')
  const [decisionQuantity, setDecisionQuantity] = useState('')
  const [decisionTripId, setDecisionTripId] = useState('')
  const [decisionNote, setDecisionNote] = useState('')
  const [splitReason, setSplitReason] = useState('Split remaining quantity')
  const [splitTripId, setSplitTripId] = useState('')
  const [splitExpectedReadyAt, setSplitExpectedReadyAt] = useState('')
  const [splitPickupWindowStart, setSplitPickupWindowStart] = useState('')
  const [splitPickupWindowEnd, setSplitPickupWindowEnd] = useState('')

  if (!session) {
    return <p className="text-sm text-slate-400">Loading vendor order…</p>
  }

  if (!vendorOrderId) {
    return <Navigate to="/purchasing/vendor-orders" replace />
  }

  if (meQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading vendor-order access…</p>
  }

  if (!canReadVendorOrders) {
    return (
      <section className="rounded-3xl border border-slate-800 bg-slate-950/70 p-8">
        <h1 className="text-2xl font-bold text-white">Vendor-order detail</h1>
        <p className="mt-3 text-sm text-slate-400">
          You do not have permission to view SupplyArr vendor orders.
        </p>
      </section>
    )
  }

  const vendorOrderQuery = useQuery({
    queryKey: ['supplyarr-vendor-order', session.accessToken, vendorOrderId],
    queryFn: () => getVendorOrder(session.accessToken, vendorOrderId),
  })

  const historyQuery = useQuery({
    queryKey: ['supplyarr-vendor-order-history', session.accessToken, vendorOrderId],
    queryFn: () => getVendorOrderHistory(session.accessToken, vendorOrderId),
    enabled: vendorOrderQuery.isSuccess,
  })

  const relatedTripsQuery = useQuery({
    queryKey: ['supplyarr-related-routarr-trips', session.accessToken, vendorOrderId],
    queryFn: () => listRoutArrTripsByVendorOrder(session.accessToken, vendorOrderId),
    enabled: vendorOrderQuery.isSuccess,
  })

  const lineageQuery = useQuery({
    queryKey: ['supplyarr-vendor-order-lineage', session.accessToken, vendorOrderQuery.data?.vendorId],
    queryFn: () => getVendorOrders(session.accessToken, { vendorId: vendorOrderQuery.data?.vendorId }),
    enabled: Boolean(vendorOrderQuery.data?.vendorId),
  })

  const refreshAll = async () => {
    await vendorOrderQuery.refetch()
    await historyQuery.refetch()
    await relatedTripsQuery.refetch()
    await lineageQuery.refetch()
  }

  const sendMutation = useMutation({
    mutationFn: () => sendVendorOrderToVendor(session.accessToken, vendorOrderId),
    onSuccess: async (result) => {
      setLatestMagicLink({
        url: result.magicLinkUrl,
        expiresAt: result.expiresAt,
        description: 'Vendor magic link issued from send flow.',
      })
      await refreshAll()
    },
  })

  const magicLinkMutation = useMutation({
    mutationFn: () => createVendorOrderMagicLink(session.accessToken, vendorOrderId),
    onSuccess: async (result) => {
      setLatestMagicLink({
        url: result.url,
        expiresAt: result.expiresAt,
        description: 'Vendor magic link regenerated.',
      })
      await refreshAll()
    },
  })

  const statusMutation = useMutation({
    mutationFn: () => submitVendorOrderStatus(session.accessToken, vendorOrderId, statusDraft),
    onSuccess: async () => {
      await refreshAll()
    },
  })

  const documentMutation = useMutation({
    mutationFn: () => registerVendorOrderDocument(session.accessToken, vendorOrderId, documentDraft),
    onSuccess: async () => {
      setDocumentDraft({
        documentType: 'photo',
        fileName: '',
        contentType: '',
        storageProvider: 'shared_blob',
        storageKey: '',
        sizeBytes: null,
      })
      await refreshAll()
    },
  })

  const decisionMutation = useMutation({
    mutationFn: () =>
      createVendorOrderBrokerDecision(session.accessToken, vendorOrderId, {
        decisionType,
        authorizedQuantity: decisionQuantity ? Number(decisionQuantity) : null,
        selectedTripId: decisionTripId || null,
        note: decisionNote || null,
      }),
    onSuccess: async () => {
      setDecisionNote('')
      setDecisionQuantity('')
      await refreshAll()
    },
  })

  const splitMutation = useMutation({
    mutationFn: () =>
      splitVendorOrder(session.accessToken, vendorOrderId, {
        selectedTripId: splitTripId || null,
        splitReason: splitReason || null,
        remainingExpectedReadyAt: splitExpectedReadyAt || null,
        remainingPickupWindowStart: splitPickupWindowStart || null,
        remainingPickupWindowEnd: splitPickupWindowEnd || null,
      }),
    onSuccess: async (result) => {
      setLatestMagicLink({
        url: result.remainingVendorOrderUrl,
        expiresAt: null,
        description: 'New remaining-child vendor link issued after split.',
      })
      await refreshAll()
    },
  })

  useEffect(() => {
    if (!vendorOrderQuery.data) {
      return
    }

    setStatusDraft((current) => ({
      ...current,
      quantityReady: vendorOrderQuery.data.quantityReady,
      estimatedReadyAt: vendorOrderQuery.data.expectedReadyAt,
      pickupWindowStart: vendorOrderQuery.data.pickupWindowStart,
      pickupWindowEnd: vendorOrderQuery.data.pickupWindowEnd,
    }))
  }, [vendorOrderQuery.data])

  if (vendorOrderQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading vendor order…</p>
  }

  if (vendorOrderQuery.isError || !vendorOrderQuery.data) {
    return (
      <section className="rounded-3xl border border-slate-800 bg-slate-950/70 p-8">
        <h1 className="text-2xl font-bold text-white">Vendor-order detail unavailable</h1>
        <p className="mt-3 text-sm text-red-300">
          Unable to load this vendor order right now.
        </p>
      </section>
    )
  }

  const order = vendorOrderQuery.data
  const splitChildren =
    (lineageQuery.data ?? []).filter((item) => item.parentVendorOrderId === order.vendorOrderId) ?? []
  const activeVendorBlockTrips =
    (relatedTripsQuery.data ?? []).filter((trip) =>
      (trip.dispatchBlocks ?? []).some(
        (block) => block.blockType === 'vendor_readiness' && block.status === 'active',
      ),
    ) ?? []
  const readyTripCount = (relatedTripsQuery.data ?? []).length - activeVendorBlockTrips.length
  const decisionSummary =
    order.status === 'completed_ready_for_dispatch'
      ? 'Vendor released this order for dispatch.'
      : order.status === 'partially_ready'
        ? 'Broker decision required for partial readiness.'
        : order.status === 'unable_to_fulfill'
          ? 'Vendor reported that this order cannot be fulfilled.'
          : 'Waiting on vendor readiness confirmation.'

  const decisionDetail =
    order.status === 'completed_ready_for_dispatch'
      ? 'SupplyArr has the vendor-ready confirmation. RoutArr dispatch can proceed once the linked trip is unblocked and manually released.'
      : order.status === 'partially_ready'
        ? 'The vendor reported a partial quantity. Decide whether to wait, authorize a partial dispatch, or split the remaining quantity.'
        : order.status === 'unable_to_fulfill'
          ? 'Keep linked RoutArr work blocked and follow the vendor exception path until the broker resolves the order.'
          : 'SupplyArr owns the vendor-facing workflow and will publish readiness events when the vendor confirms pickup readiness.'

  const railSections: DetailRailSectionConfig[] = [
    {
      title: 'Related RoutArr trips',
      icon: <Route className="h-5 w-5" />,
      content:
        relatedTripsQuery.data && relatedTripsQuery.data.length > 0 ? (
          <div className="space-y-3">
            {relatedTripsQuery.data.map((trip) => {
              const activeBlock = (trip.dispatchBlocks ?? []).find(
                (block) => block.blockType === 'vendor_readiness' && block.status === 'active',
              )
              return (
                <div key={trip.tripId} className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-white">{trip.title}</p>
                      <p className="mt-1 text-xs text-slate-400">
                        {trip.tripNumber} · {humanizeVendorOrderValue(trip.dispatchStatus)}
                      </p>
                    </div>
                    <DetailBadge
                      label={activeBlock ? 'Dispatch blocked' : 'Dispatch ready'}
                      tone={activeBlock ? 'bad' : 'good'}
                    />
                  </div>
                  <p className="mt-3 text-xs text-slate-300">
                    Vendor status snapshot: {humanizeVendorOrderValue(trip.vendorReadinessStatusSnapshot)}
                  </p>
                  <p className="mt-1 text-xs text-slate-400">
                    {quantitySummary(
                      trip.vendorOrderedQuantitySnapshot ?? 0,
                      trip.vendorQuantityReadySnapshot ?? 0,
                      Math.max(
                        0,
                        (trip.vendorOrderedQuantitySnapshot ?? 0) - (trip.vendorQuantityReadySnapshot ?? 0),
                      ),
                      order.quantityUom,
                    )}
                  </p>
                  <p className="mt-1 text-xs text-slate-400">
                    Ready confirmation: {formatVendorOrderDateTime(trip.vendorConfirmedReadyAtSnapshot)}
                  </p>
                  {activeBlock ? (
                    <p className="mt-2 text-xs text-amber-300">
                      Block reason: {humanizeVendorOrderValue(activeBlock.blockReason)}
                    </p>
                  ) : null}
                  {trip.dispatchOverrideReason ? (
                    <p className="mt-2 text-xs text-red-200">
                      Override reason: {trip.dispatchOverrideReason}
                    </p>
                  ) : null}
                </div>
              )
            })}
          </div>
        ) : relatedTripsQuery.isLoading ? (
          <p className="text-sm text-slate-400">Loading related RoutArr trips…</p>
        ) : (
          <DetailEmptyState text="No RoutArr trips are linked to this vendor order yet." />
        ),
    },
    {
      title: 'Documents',
      icon: <FileText className="h-5 w-5" />,
      content: (
        <div className="space-y-4">
          {order.documents.length > 0 ? (
            <ul className="space-y-3">
              {order.documents.map((document) => (
                <li key={document.documentId} className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                  <p className="font-medium text-white">{document.fileName}</p>
                  <p className="mt-1 text-xs text-slate-400">
                    {humanizeVendorOrderValue(document.documentType)} · {document.contentType}
                  </p>
                  <p className="mt-1 text-xs text-slate-500">
                    RecordArr {document.recordArrRecordNumberSnapshot} · uploaded {formatVendorOrderDateTime(document.uploadedAt)}
                  </p>
                </li>
              ))}
            </ul>
          ) : (
            <DetailEmptyState text="No vendor-order documents have been registered yet." />
          )}

          {canUpdateVendorOrders ? (
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
              <h3 className="font-medium text-white">Register vendor-order document</h3>
              <p className="mt-1 text-xs text-slate-400">
                This records canonical RecordArr metadata. Shared blob storage stays a provider/key reference only in v1.
              </p>
              <div className="mt-3 grid gap-3">
                <label className="text-sm text-slate-300">
                  File
                  <input
                    type="file"
                    className="mt-1 block w-full text-sm text-slate-300"
                    onChange={(event) => {
                      const file = event.target.files?.[0]
                      if (!file) {
                        return
                      }

                      setDocumentDraft((current) => ({
                        ...current,
                        fileName: file.name,
                        contentType: file.type || 'application/octet-stream',
                        sizeBytes: file.size,
                        storageKey: current.storageKey || `vendor-order/${file.name}`,
                      }))
                    }}
                  />
                </label>
                <div className="grid gap-3 md:grid-cols-2">
                  <label className="text-sm text-slate-300">
                    Document type
                    <select
                      className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      value={documentDraft.documentType}
                      onChange={(event) => setDocumentDraft({ ...documentDraft, documentType: event.target.value })}
                    >
                      {DOCUMENT_TYPES.map((documentType) => (
                        <option key={documentType} value={documentType}>
                          {humanizeVendorOrderValue(documentType)}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label className="text-sm text-slate-300">
                    Content type
                    <input
                      className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      value={documentDraft.contentType}
                      onChange={(event) => setDocumentDraft({ ...documentDraft, contentType: event.target.value })}
                    />
                  </label>
                </div>
                <label className="text-sm text-slate-300">
                  File name
                  <input
                    className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                    value={documentDraft.fileName}
                    onChange={(event) => setDocumentDraft({ ...documentDraft, fileName: event.target.value })}
                  />
                </label>
                <div className="grid gap-3 md:grid-cols-2">
                  <label className="text-sm text-slate-300">
                    Storage provider
                    <input
                      className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      value={documentDraft.storageProvider ?? ''}
                      onChange={(event) => setDocumentDraft({ ...documentDraft, storageProvider: event.target.value })}
                    />
                  </label>
                  <label className="text-sm text-slate-300">
                    Storage key
                    <input
                      className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      value={documentDraft.storageKey ?? ''}
                      onChange={(event) => setDocumentDraft({ ...documentDraft, storageKey: event.target.value })}
                    />
                  </label>
                </div>
                <button
                  type="button"
                  className="rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-sky-400 disabled:opacity-50"
                  disabled={
                    documentMutation.isPending ||
                    !documentDraft.fileName.trim() ||
                    !documentDraft.contentType.trim()
                  }
                  onClick={() => documentMutation.mutate()}
                >
                  {documentMutation.isPending ? 'Registering…' : 'Register document'}
                </button>
                {documentMutation.error instanceof Error ? (
                  <p className="text-xs text-red-300">{documentMutation.error.message}</p>
                ) : null}
              </div>
            </div>
          ) : null}
        </div>
      ),
    },
    {
      title: 'Status history',
      icon: <ArrowRightLeft className="h-5 w-5" />,
      content:
        historyQuery.data && historyQuery.data.length > 0 ? (
          <ol className="space-y-3">
            {historyQuery.data.map((entry) => (
              <li key={entry.statusUpdateId} className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="font-medium text-white">
                      {humanizeVendorOrderValue(entry.previousStatus ?? 'new')} to {humanizeVendorOrderValue(entry.newStatus)}
                    </p>
                    <p className="mt-1 text-xs text-slate-400">
                      {humanizeVendorOrderValue(entry.source)} · {formatVendorOrderDateTime(entry.createdAt)}
                    </p>
                  </div>
                  <DetailBadge label={humanizeVendorOrderValue(entry.newStatus)} tone={vendorOrderStatusTone(entry.newStatus)} />
                </div>
                <p className="mt-2 text-xs text-slate-300">
                  {quantitySummary(
                    entry.orderedQuantitySnapshot,
                    entry.quantityReady,
                    entry.quantityRemaining,
                    order.quantityUom,
                  )}
                </p>
                {entry.note ? <p className="mt-2 text-xs text-slate-300">{entry.note}</p> : null}
                {entry.exceptionReason ? (
                  <p className="mt-2 text-xs text-red-200">{entry.exceptionReason}</p>
                ) : null}
              </li>
            ))}
          </ol>
        ) : (
          <DetailEmptyState text="No immutable vendor-order status updates have been recorded yet." />
        ),
    },
  ]

  return (
    <ProfileDetailsLayout
      testId="supplyarr-vendor-order-detail"
      backLabel="Vendor orders"
      backTo="/purchasing/vendor-orders"
      breadcrumbs={['Vendor orders', order.itemDescription]}
      icon={<PackageCheck className="h-9 w-9" />}
      title={order.itemDescription}
      subtitle={
        <span className="flex flex-wrap items-center gap-2">
          <span>{order.vendorNameSnapshot}</span>
          <span className="text-slate-600">/</span>
          <span>{order.vendorOrderId}</span>
        </span>
      }
      badges={[
        { label: 'SupplyArr vendor order', tone: 'info' },
        { label: humanizeVendorOrderValue(order.status), tone: vendorOrderStatusTone(order.status) },
        { label: order.brokerOrderNumberSnapshot ?? 'OrdArr ref pending', tone: order.brokerOrderNumberSnapshot ? 'neutral' : 'warn' },
      ]}
      actions={
        canUpdateVendorOrders ? (
          <>
            <button
              type="button"
              className="inline-flex rounded-xl bg-sky-500 px-4 py-3 text-sm font-semibold text-slate-950 hover:bg-sky-400 disabled:opacity-50"
              disabled={sendMutation.isPending}
              onClick={() => sendMutation.mutate()}
            >
              {sendMutation.isPending ? 'Sending…' : 'Send to vendor'}
            </button>
            <button
              type="button"
              className="inline-flex rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
              disabled={magicLinkMutation.isPending}
              onClick={() => magicLinkMutation.mutate()}
            >
              {magicLinkMutation.isPending ? 'Generating…' : 'Generate magic link'}
            </button>
          </>
        ) : null
      }
      metrics={[
        {
          label: 'Status',
          value: humanizeVendorOrderValue(order.status),
          hint: `Updated ${formatVendorOrderDateTime(order.updatedAt)}`,
          icon: <PackageCheck className="h-5 w-5" />,
          tone: vendorOrderStatusTone(order.status),
        },
        {
          label: 'Quantity',
          value: `${order.quantityReady}/${order.orderedQuantity}`,
          hint: `${order.quantityRemaining} remaining ${order.quantityUom}`,
          icon: <CheckCircle2 className="h-5 w-5" />,
          tone: order.status === 'completed_ready_for_dispatch' ? 'good' : order.status === 'partially_ready' ? 'warn' : 'neutral',
        },
        {
          label: 'Linked trips',
          value: String(relatedTripsQuery.data?.length ?? 0),
          hint: `${activeVendorBlockTrips.length} blocked · ${readyTripCount} ready`,
          icon: <Route className="h-5 w-5" />,
          tone: activeVendorBlockTrips.length > 0 ? 'warn' : 'good',
        },
        {
          label: 'Documents',
          value: String(order.documents.length),
          hint: `${order.statusHistory.length} immutable status events`,
          icon: <FileText className="h-5 w-5" />,
          tone: order.documents.length > 0 ? 'good' : 'neutral',
        },
      ]}
      tabs={['Overview', 'Readiness', 'Related records', 'Documents', 'History']}
      snapshotTitle="Vendor-order snapshot"
      snapshotSubtitle="SupplyArr-owned vendor readiness, pickup snapshots, and cross-product reference labels."
      snapshotFields={[
        { label: 'Vendor order ID', value: order.vendorOrderId, source: 'SupplyArr source of truth' },
        { label: 'Vendor', value: order.vendorNameSnapshot, source: 'SupplyArr vendor reference' },
        { label: 'Broker order snapshot', value: order.brokerOrderNumberSnapshot ?? 'Not recorded', source: 'OrdArr snapshot' },
        { label: 'Pickup location snapshot', value: order.pickupLocationNameSnapshot ?? 'Not recorded', source: 'Vendor snapshot' },
        { label: 'Pickup address snapshot', value: order.pickupAddressSnapshot, source: 'Vendor snapshot' },
        { label: 'Destination summary snapshot', value: order.deliveryLocationNameSnapshot ?? 'Hidden or not recorded', source: 'Customer snapshot' },
        { label: 'Destination address snapshot', value: order.deliveryAddressSnapshot ?? 'Hidden or not recorded', source: 'Customer snapshot' },
        { label: 'Expected ready', value: formatVendorOrderDateTime(order.expectedReadyAt), source: 'SupplyArr vendor workflow' },
        { label: 'Confirmed ready', value: formatVendorOrderDateTime(order.confirmedReadyAt), source: 'SupplyArr vendor workflow' },
        { label: 'Pickup window start', value: formatVendorOrderDateTime(order.pickupWindowStart), source: 'Pickup schedule snapshot' },
        { label: 'Pickup window end', value: formatVendorOrderDateTime(order.pickupWindowEnd), source: 'Pickup schedule snapshot' },
        { label: 'Created by person', value: order.createdByPersonId ?? 'Not recorded', source: 'StaffArr personId' },
      ]}
      mainContent={
        <div className="space-y-5">
          {latestMagicLink ? (
            <section className="rounded-2xl border border-sky-700/30 bg-sky-950/20 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="text-sm font-semibold text-white">Latest vendor access link</h3>
                  {latestMagicLink.description ? (
                    <p className="mt-1 text-xs text-sky-200/80">{latestMagicLink.description}</p>
                  ) : null}
                  <p className="mt-1 break-all text-xs text-sky-200">{latestMagicLink.url}</p>
                  {latestMagicLink.expiresAt ? (
                    <p className="mt-1 text-xs text-slate-400">
                      Expires {formatVendorOrderDateTime(latestMagicLink.expiresAt)}
                    </p>
                  ) : (
                    <p className="mt-1 text-xs text-slate-400">
                      Expiry is tracked on the newly issued remaining-child order link.
                    </p>
                  )}
                </div>
                <ExternalLink className="h-4 w-4 text-sky-300" />
              </div>
            </section>
          ) : null}

          <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Broker actions</h3>
            <p className="mt-2 text-sm text-slate-400">
              Record internal status decisions separately from RoutArr execution. SupplyArr owns vendor readiness and the vendor-safe portal workflow.
            </p>
            {canUpdateVendorOrders ? (
              <div className="mt-4 grid gap-5 lg:grid-cols-2">
                <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
                  <h4 className="font-semibold text-white">Internal status update</h4>
                  <div className="mt-3 grid gap-3">
                    <label className="text-sm text-slate-300">
                      New status
                      <select
                        className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                        value={statusDraft.newStatus}
                        onChange={(event) => setStatusDraft({ ...statusDraft, newStatus: event.target.value })}
                      >
                        {INTERNAL_STATUS_OPTIONS.map((status) => (
                          <option key={status} value={status}>
                            {humanizeVendorOrderValue(status)}
                          </option>
                        ))}
                      </select>
                    </label>
                    <div className="grid gap-3 md:grid-cols-2">
                      <label className="text-sm text-slate-300">
                        Quantity ready
                        <input
                          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                          inputMode="decimal"
                          value={statusDraft.quantityReady?.toString() ?? ''}
                          onChange={(event) =>
                            setStatusDraft({
                              ...statusDraft,
                              quantityReady: event.target.value ? Number(event.target.value) : null,
                            })
                          }
                        />
                      </label>
                      <label className="text-sm text-slate-300">
                        Estimated ready at
                        <input
                          type="datetime-local"
                          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                          value={toDatetimeLocalValue(statusDraft.estimatedReadyAt)}
                          onChange={(event) =>
                            setStatusDraft({ ...statusDraft, estimatedReadyAt: fromDatetimeLocalValue(event.target.value) })
                          }
                        />
                      </label>
                    </div>
                    <div className="grid gap-3 md:grid-cols-2">
                      <label className="text-sm text-slate-300">
                        Pickup window start
                        <input
                          type="datetime-local"
                          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                          value={toDatetimeLocalValue(statusDraft.pickupWindowStart)}
                          onChange={(event) =>
                            setStatusDraft({ ...statusDraft, pickupWindowStart: fromDatetimeLocalValue(event.target.value) })
                          }
                        />
                      </label>
                      <label className="text-sm text-slate-300">
                        Pickup window end
                        <input
                          type="datetime-local"
                          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                          value={toDatetimeLocalValue(statusDraft.pickupWindowEnd)}
                          onChange={(event) =>
                            setStatusDraft({ ...statusDraft, pickupWindowEnd: fromDatetimeLocalValue(event.target.value) })
                          }
                        />
                      </label>
                    </div>
                    <label className="text-sm text-slate-300">
                      Note
                      <textarea
                        className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                        rows={3}
                        value={statusDraft.note ?? ''}
                        onChange={(event) => setStatusDraft({ ...statusDraft, note: event.target.value || null })}
                      />
                    </label>
                    <label className="text-sm text-slate-300">
                      Exception reason
                      <textarea
                        className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                        rows={2}
                        value={statusDraft.exceptionReason ?? ''}
                        onChange={(event) =>
                          setStatusDraft({ ...statusDraft, exceptionReason: event.target.value || null })
                        }
                      />
                    </label>
                    <label className="flex items-start gap-3 rounded-xl border border-slate-800 bg-slate-950/70 p-3 text-sm text-slate-300">
                      <input
                        type="checkbox"
                        className="mt-1"
                        checked={statusDraft.readyForPickupConfirmed}
                        onChange={(event) =>
                          setStatusDraft({ ...statusDraft, readyForPickupConfirmed: event.target.checked })
                        }
                      />
                      <span>I confirm this order is complete, staged, and ready for pickup.</span>
                    </label>
                    <button
                      type="button"
                      className="rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-sky-400 disabled:opacity-50"
                      disabled={statusMutation.isPending}
                      onClick={() => statusMutation.mutate()}
                    >
                      {statusMutation.isPending ? 'Saving…' : 'Save internal status update'}
                    </button>
                    {statusMutation.error instanceof Error ? (
                      <p className="text-xs text-red-300">{statusMutation.error.message}</p>
                    ) : null}
                  </div>
                </div>

                <div className="space-y-5">
                  <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
                    <h4 className="font-semibold text-white">Partial readiness decision</h4>
                    <p className="mt-1 text-xs text-slate-400">
                      Keep the vendor order blocked, authorize a partial dispatch, or prepare the split flow.
                    </p>
                    <div className="mt-3 grid gap-3">
                      <label className="text-sm text-slate-300">
                        Decision
                        <select
                          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                          value={decisionType}
                          onChange={(event) => setDecisionType(event.target.value)}
                        >
                          <option value="wait_full">Wait full quantity</option>
                          <option value="dispatch_partial">Dispatch partial quantity</option>
                        </select>
                      </label>
                      <div className="grid gap-3 md:grid-cols-2">
                        <label className="text-sm text-slate-300">
                          Authorized quantity
                          <input
                            className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                            inputMode="decimal"
                            value={decisionQuantity}
                            onChange={(event) => setDecisionQuantity(event.target.value)}
                          />
                        </label>
                        <label className="text-sm text-slate-300">
                          Selected trip ID
                          <input
                            className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                            value={decisionTripId}
                            onChange={(event) => setDecisionTripId(event.target.value)}
                          />
                        </label>
                      </div>
                      <label className="text-sm text-slate-300">
                        Note
                        <textarea
                          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                          rows={3}
                          value={decisionNote}
                          onChange={(event) => setDecisionNote(event.target.value)}
                        />
                      </label>
                      <button
                        type="button"
                        className="rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-sky-400 disabled:opacity-50"
                        disabled={decisionMutation.isPending}
                        onClick={() => decisionMutation.mutate()}
                      >
                        {decisionMutation.isPending ? 'Saving…' : 'Record broker decision'}
                      </button>
                      {decisionMutation.error instanceof Error ? (
                        <p className="text-xs text-red-300">{decisionMutation.error.message}</p>
                      ) : null}
                    </div>
                  </div>

                  <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
                    <h4 className="font-semibold text-white">Split remaining quantity</h4>
                    <p className="mt-1 text-xs text-slate-400">
                      Create ready and remaining child orders, revoke the old token, and rotate remaining-child access.
                    </p>
                    <div className="mt-3 grid gap-3">
                      <label className="text-sm text-slate-300">
                        Split reason
                        <input
                          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                          value={splitReason}
                          onChange={(event) => setSplitReason(event.target.value)}
                        />
                      </label>
                      <label className="text-sm text-slate-300">
                        Selected ready-trip ID
                        <input
                          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                          value={splitTripId}
                          onChange={(event) => setSplitTripId(event.target.value)}
                        />
                      </label>
                      <div className="grid gap-3 md:grid-cols-2">
                        <label className="text-sm text-slate-300">
                          Remaining expected ready at
                          <input
                            type="datetime-local"
                            className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                            value={splitExpectedReadyAt}
                            onChange={(event) => setSplitExpectedReadyAt(event.target.value)}
                          />
                        </label>
                        <label className="text-sm text-slate-300">
                          Remaining pickup window start
                          <input
                            type="datetime-local"
                            className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                            value={splitPickupWindowStart}
                            onChange={(event) => setSplitPickupWindowStart(event.target.value)}
                          />
                        </label>
                      </div>
                      <label className="text-sm text-slate-300">
                        Remaining pickup window end
                        <input
                          type="datetime-local"
                          className="mt-1 block w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                          value={splitPickupWindowEnd}
                          onChange={(event) => setSplitPickupWindowEnd(event.target.value)}
                        />
                      </label>
                      <button
                        type="button"
                        className="rounded-xl bg-amber-400 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-amber-300 disabled:opacity-50"
                        disabled={splitMutation.isPending}
                        onClick={() => splitMutation.mutate()}
                      >
                        {splitMutation.isPending ? 'Splitting…' : 'Split remaining quantity'}
                      </button>
                      {splitMutation.error instanceof Error ? (
                        <p className="text-xs text-red-300">{splitMutation.error.message}</p>
                      ) : null}
                    </div>
                  </div>
                </div>
              </div>
            ) : (
              <p className="mt-4 text-sm text-slate-400">You can view this record, but update actions are restricted.</p>
            )}
          </section>

          <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Split lineage</h3>
            <div className="mt-4 grid gap-4 md:grid-cols-2">
              <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
                <p className="text-xs uppercase tracking-wide text-slate-500">Parent vendor order</p>
                <p className="mt-2 text-sm font-medium text-white">{order.parentVendorOrderId ?? 'This is the parent order.'}</p>
                {order.splitReason ? (
                  <p className="mt-2 text-xs text-slate-400">Split reason: {order.splitReason}</p>
                ) : null}
              </div>
              <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
                <p className="text-xs uppercase tracking-wide text-slate-500">Child vendor orders</p>
                {splitChildren.length > 0 ? (
                  <ul className="mt-2 space-y-2">
                    {splitChildren.map((child) => (
                      <li key={child.vendorOrderId} className="rounded-xl border border-slate-800 bg-slate-950/70 p-3">
                        <p className="text-sm font-medium text-white">{child.itemDescription}</p>
                        <p className="mt-1 text-xs text-slate-400">
                          {child.vendorOrderId} · {humanizeVendorOrderValue(child.status)}
                        </p>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="mt-2 text-sm text-slate-400">No child vendor orders have been created from this parent yet.</p>
                )}
              </div>
            </div>
          </section>
        </div>
      }
      decisionTitle="Readiness decision"
      decisionBadge={{
        label:
          order.status === 'completed_ready_for_dispatch'
            ? 'Dispatch releasable'
            : order.status === 'partially_ready'
              ? 'Broker decision required'
              : order.status === 'unable_to_fulfill'
                ? 'Blocked by vendor'
                : 'Waiting on vendor',
        tone:
          order.status === 'completed_ready_for_dispatch'
            ? 'good'
            : order.status === 'unable_to_fulfill'
              ? 'bad'
              : 'warn',
      }}
      decisionIcon={
        order.status === 'completed_ready_for_dispatch' ? (
          <CheckCircle2 className="h-5 w-5 text-emerald-300" />
        ) : (
          <AlertTriangle className="h-5 w-5 text-amber-300" />
        )
      }
      decisionSummary={decisionSummary}
      decisionDetail={decisionDetail}
      allowedChecks={[
        order.status === 'completed_ready_for_dispatch',
        readyTripCount > 0,
        order.documents.length > 0,
      ].filter(Boolean).length}
      blockedChecks={[
        order.status !== 'completed_ready_for_dispatch',
        activeVendorBlockTrips.length > 0,
        order.status === 'unable_to_fulfill',
      ].filter(Boolean).length}
      railSections={railSections}
    />
  )
}

function fromDatetimeLocalValue(value: string): string | null {
  if (!value) {
    return null
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return null
  }

  return date.toISOString()
}

function toDatetimeLocalValue(value: string | null | undefined): string {
  if (!value) {
    return ''
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }

  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  const hours = `${date.getHours()}`.padStart(2, '0')
  const minutes = `${date.getMinutes()}`.padStart(2, '0')
  return `${year}-${month}-${day}T${hours}:${minutes}`
}
