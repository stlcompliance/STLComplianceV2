import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportVendorReportSummaryCsv,
  getCompliancePartyDetail,
  getVendorReportDetail,
  getVendorReportSummary,
  getRfqs,
  getVendorReturns,
  listWarrantyClaims,
} from '../api/client'

interface VendorReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

export function VendorReportsPanel({ accessToken, canRead, canExport }: VendorReportsPanelProps) {
  const [approvalFilter, setApprovalFilter] = useState('')
  const [activeOnly, setActiveOnly] = useState(false)
  const [selectedVendorId, setSelectedVendorId] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['supplyarr-vendor-report-summary', accessToken, approvalFilter, activeOnly],
    queryFn: () =>
      getVendorReportSummary(accessToken, {
        approvalStatus: approvalFilter || undefined,
        activeOnly: activeOnly || undefined,
      }),
    enabled: canRead,
  })

  const detailQuery = useQuery({
    queryKey: ['supplyarr-vendor-report-detail', accessToken, selectedVendorId],
    queryFn: () => getVendorReportDetail(accessToken, selectedVendorId!),
    enabled: canRead && Boolean(selectedVendorId),
  })

  const rfqsQuery = useQuery({
    queryKey: ['supplyarr-vendor-report-rfqs', accessToken, selectedVendorId],
    queryFn: () => getRfqs(accessToken),
    enabled: canRead && Boolean(selectedVendorId),
  })

  const complianceDetailQuery = useQuery({
    queryKey: ['supplyarr-vendor-compliance-detail', accessToken, selectedVendorId],
    queryFn: () => getCompliancePartyDetail(accessToken, selectedVendorId!),
    enabled: canRead && Boolean(selectedVendorId),
  })

  const vendorReturnsQuery = useQuery({
    queryKey: ['supplyarr-vendor-report-returns', accessToken, selectedVendorId],
    queryFn: () => getVendorReturns(accessToken, { vendorPartyId: selectedVendorId! }),
    enabled: canRead && Boolean(selectedVendorId),
  })

  const warrantyClaimsQuery = useQuery({
    queryKey: ['supplyarr-vendor-report-warranty-claims', accessToken, selectedVendorId],
    queryFn: () => listWarrantyClaims(accessToken, { vendorPartyId: selectedVendorId! }),
    enabled: canRead && Boolean(selectedVendorId),
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportVendorReportSummaryCsv(accessToken, {
        approvalStatus: approvalFilter || undefined,
        activeOnly: activeOnly || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `supplyarr-vendor-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const scorecard = detailQuery.data
    ? (() => {
        const summary = detailQuery.data.summary
        const returnCount = vendorReturnsQuery.data?.length ?? 0
        const warrantyClaimCount = warrantyClaimsQuery.data?.length ?? 0
        const averageLeadTimeDays = summary.averageLeadTimeDays
        const onTimeDeliveryRate = summary.onTimeDeliveryRate
        const leadTimeCoverage =
          summary.partVendorLinkCount > 0
            ? Math.round((summary.leadTimeSampleCount / summary.partVendorLinkCount) * 100)
            : 0
        const vendorRfqs = (rfqsQuery.data ?? []).filter(
          (rfq) =>
            rfq.invitations.some((invite) => invite.vendorPartyId === selectedVendorId) ||
            rfq.quotes.some((quote) => quote.vendorPartyId === selectedVendorId),
        )
        const vendorQuotes = vendorRfqs.flatMap((rfq) =>
          rfq.quotes
            .filter((quote) => quote.vendorPartyId === selectedVendorId)
            .map((quote) => {
              const invitation = rfq.invitations.find((invite) => invite.vendorPartyId === selectedVendorId)
              return {
                rfqId: rfq.rfqId,
                rfqKey: rfq.rfqKey,
                quote,
                invitation,
              }
            }),
        )
        const quoteResponseDurations = vendorQuotes
          .filter((entry) => entry.invitation?.invitedAt && entry.quote.submittedAt)
          .map((entry) => {
            const invitedAt = new Date(entry.invitation!.invitedAt).getTime()
            const submittedAt = new Date(entry.quote.submittedAt!).getTime()
            return Math.max(0, (submittedAt - invitedAt) / (1000 * 60 * 60 * 24))
          })
        const averageQuoteResponseDays =
          quoteResponseDurations.length > 0
            ? Math.round((quoteResponseDurations.reduce((total, days) => total + days, 0) / quoteResponseDurations.length) * 10) / 10
            : null
        const quoteCompetitiveness =
          vendorQuotes.length > 0
            ? Math.round(
                (vendorQuotes.filter((entry) => {
                  const submittedQuotes = (rfqsQuery.data ?? [])
                    .find((rfq) => rfq.rfqId === entry.rfqId)
                    ?.quotes.filter((quote) => quote.status === 'submitted')
                  const lowestAmount = Math.min(
                    ...(submittedQuotes ?? []).map((quote) => quote.totalAmount ?? Number.POSITIVE_INFINITY),
                  )
                  return entry.quote.totalAmount != null && entry.quote.totalAmount === lowestAmount
                }).length /
                  vendorQuotes.length) *
                  100,
              )
            : null
        const complianceDocuments = complianceDetailQuery.data?.documents ?? []
        const expiredDocumentCount = complianceDocuments.filter((doc) => doc.isExpired).length
        const expiringSoonDocumentCount = complianceDocuments.filter((doc) => doc.isExpiringSoon).length
        const approvedDocumentCount = complianceDocuments.filter(
          (doc) => doc.effectiveStatus === 'approved',
        ).length
        const reviewPendingDocumentCount = complianceDocuments.filter(
          (doc) => doc.effectiveStatus === 'pending_review',
        ).length
        const linkCoverage =
          summary.partVendorLinkCount > 0
            ? Math.round((summary.preferredPartLinkCount / summary.partVendorLinkCount) * 100)
            : 0
        const recentOrders = detailQuery.data.recentPurchaseOrders
        const recentQuantityOrdered = recentOrders.reduce((total, order) => total + order.quantityOrdered, 0)
        const recentQuantityReceived = recentOrders.reduce((total, order) => total + order.quantityReceived, 0)
        const fullyReceivedOrderCount = recentOrders.filter(
          (order) => order.quantityOrdered > 0 && order.quantityReceived >= order.quantityOrdered,
        ).length
        const recentFillRate =
          recentQuantityOrdered > 0 ? Math.round((recentQuantityReceived / recentQuantityOrdered) * 100) : null
        const fullyReceivedOrderRate =
          recentOrders.length > 0 ? Math.round((fullyReceivedOrderCount / recentOrders.length) * 100) : null
        const returnRate =
          summary.postedReceivingReceiptCount > 0
            ? Math.round((returnCount / summary.postedReceivingReceiptCount) * 100)
            : null
        const warrantyClaimRate =
          summary.postedReceivingReceiptCount > 0
            ? Math.round((warrantyClaimCount / summary.postedReceivingReceiptCount) * 100)
            : null
        const activityRecencyDays = (() => {
          const recentAt = summary.lastReceivingPostedAt ?? summary.lastPurchaseOrderAt
          if (!recentAt) return null
          const diffMs = Date.now() - new Date(recentAt).getTime()
          return Math.max(0, Math.floor(diffMs / (1000 * 60 * 60 * 24)))
        })()
        const openCommitments = summary.openPurchaseRequestCount + summary.openPurchaseOrderCount
        const approvalHealth =
          summary.approvalStatus === 'approved'
            ? summary.status === 'active'
              ? 'Approved and active'
              : 'Approved'
            : summary.approvalStatus === 'restricted'
              ? 'Restricted'
              : summary.approvalStatus
        const documentPosture =
          complianceDetailQuery.data?.summary.compliancePosture ??
          (expiredDocumentCount > 0
            ? 'expired'
            : expiringSoonDocumentCount > 0
              ? 'expiring_soon'
              : reviewPendingDocumentCount > 0
                ? 'pending_review'
                : approvedDocumentCount > 0
                  ? 'approved'
                  : 'unknown')
        const activityBonus =
          activityRecencyDays == null ? 0 : Math.max(0, 10 - Math.floor(activityRecencyDays / 14))
        const score = Math.max(
          0,
          Math.min(
            100,
            45 +
              Math.min(20, Math.floor(linkCoverage / 5)) +
              Math.min(20, recentFillRate ?? 10) +
              Math.min(10, fullyReceivedOrderRate ?? 5) +
              Math.min(10, quoteCompetitiveness ?? 5) +
              Math.min(8, Math.max(0, 10 - (averageQuoteResponseDays ?? 10))) +
              Math.min(10, activityBonus) +
              Math.min(10, summary.postedReceivingReceiptCount * 2) -
              Math.min(10, Math.max(0, 15 - (averageLeadTimeDays ?? 15))) +
              Math.min(10, Math.floor((onTimeDeliveryRate ?? 0) / 10)) +
              Math.min(5, Math.floor(leadTimeCoverage / 20)) -
              summary.openBackorderCount * 6 -
              returnCount * 4 -
              warrantyClaimCount * 5 -
              expiredDocumentCount * 6 -
              expiringSoonDocumentCount * 2 -
              openCommitments * 2 -
              (summary.approvalStatus === 'approved' && summary.status === 'active' ? 0 : 8),
          ),
        )
        const status =
          score >= 80 ? 'Healthy' : score >= 55 ? 'Watch' : 'At risk'
        return {
          score,
          status,
          catalogCoverage: linkCoverage,
          recentFillRate,
          fullyReceivedOrderRate,
          returnRate,
          warrantyClaimRate,
          averageLeadTimeDays,
          onTimeDeliveryRate,
          leadTimeCoverage,
          quoteCompetitiveness,
          averageQuoteResponseDays,
          openCommitments,
          openBackorders: summary.openBackorderCount,
          approvalHealth,
          documentPosture,
          expiredDocumentCount,
          expiringSoonDocumentCount,
          approvedDocumentCount,
          reviewPendingDocumentCount,
          activityRecencyDays,
        }
      })()
    : null

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="vendor-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Vendor reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Procurement activity, catalog links, lead time, and LoadArr receipt rollups per
            vendor.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-3 text-sm">
        <label htmlFor="vendor-report-approval-filter" className="flex items-center gap-2 text-slate-300">
          Vendor approval filter
          <select
            id="vendor-report-approval-filter"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={approvalFilter}
            onChange={(event) => setApprovalFilter(event.target.value)}
          >
            <option value="">All</option>
            <option value="approved">Approved</option>
            <option value="pending">Pending</option>
            <option value="restricted">Restricted</option>
          </select>
        </label>
        <label htmlFor="vendor-report-active-only" className="flex items-center gap-2 text-slate-300">
          <input
            id="vendor-report-active-only"
            type="checkbox"
            checked={activeOnly}
            onChange={(event) => setActiveOnly(event.target.checked)}
          />
          Active vendors only
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-500">Loading vendor report summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Vendor report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load vendor report summary.')}
            retryLabel="Retry summary"
            onRetry={() => {
              void summaryQuery.refetch()
            }}
          />
        </div>
      )}

      {exportMutation.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="CSV export failed"
            message={getErrorMessage(exportMutation.error, 'Unable to export vendor report CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 flex flex-wrap gap-2 text-xs">
            {summaryQuery.data.approvalStatusCounts.map((item) => (
              <span
                key={item.approvalStatus}
                className="rounded-md bg-slate-800 px-2 py-1 text-slate-300"
              >
                {item.approvalStatus}: {item.count}
              </span>
            ))}
          </div>

          {summaryQuery.data.vendors.length === 0 ? (
            <p className="mt-4 text-sm text-slate-500">No vendors match the current filters.</p>
          ) : (
            <ul className="mt-4 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
              {summaryQuery.data.vendors.map((vendor) => (
                <li key={vendor.vendorPartyId} className="px-3 py-3">
                  <button
                    type="button"
                    className="w-full text-left"
                    onClick={() => setSelectedVendorId(vendor.vendorPartyId)}
                  >
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <div className="font-medium text-slate-100">
                          {vendor.partyKey} · {vendor.displayName}
                        </div>
                        <div className="text-xs text-slate-500">
                          {vendor.approvalStatus} · {vendor.status}
                        </div>
                      </div>
                      <span className="text-xs text-slate-400">
                        {vendor.partVendorLinkCount} catalog links
                      </span>
                    </div>
                    <p className="mt-2 text-xs text-slate-400">
                      Open PR {vendor.openPurchaseRequestCount} · Open PO {vendor.openPurchaseOrderCount}{' '}
                      · Issued PO {vendor.issuedPurchaseOrderCount} · Backorders {vendor.openBackorderCount}
                    </p>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </>
      )}

      {selectedVendorId && detailQuery.data && (
        <div className="mt-6 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <h3 className="text-sm font-semibold text-slate-100">
            Detail · {detailQuery.data.summary.displayName}
          </h3>
          <p className="mt-1 text-xs text-slate-500">
            LoadArr receipts posted {detailQuery.data.summary.postedReceivingReceiptCount} · Open
            line qty {detailQuery.data.summary.openPurchaseOrderLineQuantity}
          </p>

          {scorecard ? (
            <div className="mt-4 rounded-lg border border-slate-800 bg-slate-900/70 p-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                    Vendor scorecard
                  </h4>
                  <p className="mt-1 text-sm text-slate-200">
                    {scorecard.status} · {scorecard.score}/100
                  </p>
                  <p className="mt-1 text-xs text-slate-500">
                    Built from catalog coverage, recent fulfillment, return, warranty, and approval signals.
                  </p>
                </div>
                <span
                  className={`rounded-full px-3 py-1 text-xs font-semibold uppercase tracking-wide ${
                    scorecard.status === 'Healthy'
                      ? 'bg-emerald-500/20 text-emerald-300'
                      : scorecard.status === 'Watch'
                        ? 'bg-amber-500/20 text-amber-300'
                        : 'bg-rose-500/20 text-rose-300'
                  }`}
                >
                  {scorecard.status}
                </span>
              </div>

              <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                <Metric label="Catalog coverage" value={`${scorecard.catalogCoverage}%`} />
                <Metric label="Recent fill rate" value={formatPercent(scorecard.recentFillRate)} />
                <Metric label="Fully received PO rate" value={formatPercent(scorecard.fullyReceivedOrderRate)} />
                <Metric label="Average lead time" value={formatDays(scorecard.averageLeadTimeDays)} />
                <Metric label="On-time delivery" value={formatPercent(scorecard.onTimeDeliveryRate)} />
                <Metric label="Lead-time coverage" value={formatPercent(scorecard.leadTimeCoverage)} />
                <Metric label="Quote competitiveness" value={formatPercent(scorecard.quoteCompetitiveness)} />
                <Metric label="Avg quote response" value={formatDays(scorecard.averageQuoteResponseDays)} />
                <Metric label="Return rate" value={formatPercent(scorecard.returnRate)} />
                <Metric label="Warranty claim rate" value={formatPercent(scorecard.warrantyClaimRate)} />
                <Metric label="Document posture" value={scorecard.documentPosture.replace(/_/g, ' ')} />
                <Metric label="Expired docs" value={String(scorecard.expiredDocumentCount)} />
                <Metric label="Expiring docs" value={String(scorecard.expiringSoonDocumentCount)} />
                <Metric label="Open commitments" value={String(scorecard.openCommitments)} />
                <Metric label="Open backorders" value={String(scorecard.openBackorders)} />
                <Metric label="Approval health" value={scorecard.approvalHealth} />
              </div>

              {complianceDetailQuery.data?.documents.length ? (
                <div className="mt-4">
                  <h5 className="text-xs font-medium uppercase tracking-wide text-slate-500">
                    Compliance documents
                  </h5>
                  <ul className="mt-2 space-y-2 text-xs text-slate-300">
                    {complianceDetailQuery.data.documents.slice(0, 4).map((doc) => (
                      <li key={doc.documentId} className="rounded-md border border-slate-800 bg-slate-950/50 px-3 py-2">
                        <div className="font-medium text-slate-100">
                          {doc.documentKey} · {doc.title}
                        </div>
                        <div className="mt-1 text-slate-500">
                          {doc.documentTypeKey} · {doc.effectiveStatus}
                          {doc.expiresAt ? ` · expires ${new Date(doc.expiresAt).toLocaleDateString()}` : ''}
                        </div>
                      </li>
                    ))}
                  </ul>
                </div>
              ) : (
                <p className="mt-4 text-xs text-slate-500">No linked compliance documents found for this vendor.</p>
              )}

              <p className="mt-3 text-xs text-slate-500">
                Activity recency:{' '}
                {scorecard.activityRecencyDays == null ? 'No recent activity' : `${scorecard.activityRecencyDays} day(s)`}
              </p>
            </div>
          ) : null}

          {detailQuery.data.recentPurchaseOrders.length > 0 && (
            <div className="mt-4">
              <h4 className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Recent purchase orders
              </h4>
              <ul className="mt-2 space-y-1 text-sm text-slate-300">
                {detailQuery.data.recentPurchaseOrders.map((row) => (
                  <li key={row.purchaseOrderId}>
                    {row.orderKey} · {row.status} · {row.quantityReceived}/{row.quantityOrdered} received
                  </li>
                ))}
              </ul>
            </div>
          )}

          {detailQuery.data.partLinks.length > 0 && (
            <div className="mt-4">
              <h4 className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Catalog links
              </h4>
              <ul className="mt-2 space-y-1 text-sm text-slate-300">
                {detailQuery.data.partLinks.map((row) => (
                  <li key={row.partVendorLinkId}>
                    {row.partKey} · {row.vendorPartNumber}
                    {row.isPreferred ? ' (preferred)' : ''}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}
    </section>
  )
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-800 bg-slate-950/60 px-3 py-2">
      <div className="text-[11px] uppercase tracking-wide text-slate-500">{label}</div>
      <div className="mt-1 text-sm font-semibold text-slate-100">{value}</div>
    </div>
  )
}

function formatPercent(value: number | null): string {
  return value == null ? 'n/a' : `${value}%`
}

function formatDays(value: number | null): string {
  return value == null ? 'n/a' : `${value} day${value === 1 ? '' : 's'}`
}
