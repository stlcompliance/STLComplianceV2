import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportSupplierReportSummaryCsv,
  getComplianceSupplierDetail,
  getSupplierReportDetail,
  getSupplierReportSummary,
  getSupplierReturns,
  getRfqs,
  listSupplierWarrantyClaims,
} from '../api/client'
import {
  formatSupplierIdentitySummary,
  formatSupplierServiceTypes,
  humanizeSupplierUnitKind,
} from '../utils/supplierPresentation'

interface SupplierReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

export function SupplierReportsPanel({ accessToken, canRead, canExport }: SupplierReportsPanelProps) {
  const [approvalFilter, setApprovalFilter] = useState('')
  const [activeOnly, setActiveOnly] = useState(false)
  const [selectedSupplierId, setSelectedSupplierId] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['supplyarr-supplier-report-summary', accessToken, approvalFilter, activeOnly],
    queryFn: () =>
      getSupplierReportSummary(accessToken, {
        approvalStatus: approvalFilter || undefined,
        activeOnly: activeOnly || undefined,
      }),
    enabled: canRead,
  })

  const detailQuery = useQuery({
    queryKey: ['supplyarr-supplier-report-detail', accessToken, selectedSupplierId],
    queryFn: () => getSupplierReportDetail(accessToken, selectedSupplierId!),
    enabled: canRead && Boolean(selectedSupplierId),
  })

  const rfqsQuery = useQuery({
    queryKey: ['supplyarr-supplier-report-rfqs', accessToken, selectedSupplierId],
    queryFn: () => getRfqs(accessToken),
    enabled: canRead && Boolean(selectedSupplierId),
  })

  const complianceDetailQuery = useQuery({
    queryKey: ['supplyarr-supplier-compliance-detail', accessToken, selectedSupplierId],
    queryFn: () => getComplianceSupplierDetail(accessToken, selectedSupplierId!),
    enabled: canRead && Boolean(selectedSupplierId),
  })

  const supplierReturnsQuery = useQuery({
    queryKey: ['supplyarr-supplier-report-returns', accessToken, selectedSupplierId],
    queryFn: () => getSupplierReturns(accessToken, { supplierId: selectedSupplierId! }),
    enabled: canRead && Boolean(selectedSupplierId),
  })

  const warrantyClaimsQuery = useQuery({
    queryKey: ['supplyarr-supplier-report-warranty-claims', accessToken, selectedSupplierId],
    queryFn: () => listSupplierWarrantyClaims(accessToken, { supplierId: selectedSupplierId! }),
    enabled: canRead && Boolean(selectedSupplierId),
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportSupplierReportSummaryCsv(accessToken, {
        approvalStatus: approvalFilter || undefined,
        activeOnly: activeOnly || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `supplyarr-supplier-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const scorecard = detailQuery.data
    ? (() => {
        const summary = detailQuery.data.summary
        const returnCount = supplierReturnsQuery.data?.length ?? 0
        const warrantyClaimCount = warrantyClaimsQuery.data?.length ?? 0
        const averageLeadTimeDays = summary.averageLeadTimeDays
        const onTimeDeliveryRate = summary.onTimeDeliveryRate
        const leadTimeCoverage =
          summary.partSupplierLinkCount > 0
            ? Math.round((summary.leadTimeSampleCount / summary.partSupplierLinkCount) * 100)
            : 0
        const supplierRfqs = (rfqsQuery.data ?? []).filter(
          (rfq) =>
            rfq.invitations.some((invite) => invite.supplierId === selectedSupplierId) ||
            rfq.quotes.some((quote) => quote.supplierId === selectedSupplierId),
        )
        const supplierQuotes = supplierRfqs.flatMap((rfq) =>
          rfq.quotes
            .filter((quote) => quote.supplierId === selectedSupplierId)
            .map((quote) => {
              const invitation = rfq.invitations.find(
                (invite) => invite.supplierId === selectedSupplierId,
              )
              return {
                rfqId: rfq.rfqId,
                rfqKey: rfq.rfqKey,
                quote,
                invitation,
              }
            }),
        )
        const quoteResponseDurations = supplierQuotes
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
          supplierQuotes.length > 0
            ? Math.round(
                (supplierQuotes.filter((entry) => {
                  const submittedQuotes = (rfqsQuery.data ?? [])
                    .find((rfq) => rfq.rfqId === entry.rfqId)
                    ?.quotes.filter((quote) => quote.status === 'submitted')
                  const lowestAmount = Math.min(
                    ...(submittedQuotes ?? []).map((quote) => quote.totalAmount ?? Number.POSITIVE_INFINITY),
                  )
                  return entry.quote.totalAmount != null && entry.quote.totalAmount === lowestAmount
                }).length /
                  supplierQuotes.length) *
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
          summary.partSupplierLinkCount > 0
            ? Math.round((summary.preferredPartSupplierLinkCount / summary.partSupplierLinkCount) * 100)
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
      className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 shadow-[var(--shadow-surface)] lg:col-span-2"
      data-testid="supplier-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Supplier reports</h2>
          <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
            Procurement activity, catalog links, lead time, and receipt rollups per supplier identity or sub-unit.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-[var(--color-accent)] px-3 py-1.5 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-3 text-sm">
        <label htmlFor="supplier-report-approval-filter" className="flex items-center gap-2 text-[var(--color-text-secondary)]">
          Supplier approval filter
          <select
            id="supplier-report-approval-filter"
            className="rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-2 py-1 text-[var(--color-text-primary)]"
            value={approvalFilter}
            onChange={(event) => setApprovalFilter(event.target.value)}
          >
            <option value="">All</option>
            <option value="approved">Approved</option>
            <option value="pending">Pending</option>
            <option value="restricted">Restricted</option>
          </select>
        </label>
        <label htmlFor="supplier-report-active-only" className="flex items-center gap-2 text-[var(--color-text-secondary)]">
          <input
            id="supplier-report-active-only"
            type="checkbox"
            checked={activeOnly}
            onChange={(event) => setActiveOnly(event.target.checked)}
          />
          Active suppliers only
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading supplier report summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Supplier report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load supplier report summary.')}
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
            message={getErrorMessage(exportMutation.error, 'Unable to export supplier report CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 flex flex-wrap gap-2 text-xs">
            {summaryQuery.data.approvalStatusCounts.map((item) => (
              <span
                key={item.approvalStatus}
                className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-2 py-1 text-[var(--color-text-secondary)]"
              >
                {item.approvalStatus}: {item.count}
              </span>
            ))}
          </div>

          {summaryQuery.data.suppliers.length === 0 ? (
            <p className="mt-4 text-sm text-[var(--color-text-muted)]">No suppliers match the current filters.</p>
          ) : (
            <ul className="mt-4 divide-y divide-[var(--color-border-subtle)] rounded-md border border-[var(--color-border-subtle)] text-sm">
              {summaryQuery.data.suppliers.map((supplier) => (
                <li
                  key={supplier.supplierId}
                  className={`px-3 py-3 transition ${
                    selectedSupplierId === supplier.supplierId ? 'bg-[var(--color-bg-control-hover)]' : ''
                  }`}
                >
                  <button
                    type="button"
                    className="w-full text-left"
                    onClick={() => setSelectedSupplierId(supplier.supplierId)}
                  >
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <div className="font-medium text-[var(--color-text-primary)]">
                          {formatSupplierIdentitySummary({
                            supplierDisplayName: supplier.supplierDisplayName,
                            supplierKey: supplier.supplierKey,
                            parentSupplierDisplayName: supplier.parentSupplierDisplayName,
                            supplierUnitKind: supplier.supplierUnitKind,
                          })}
                        </div>
                        <div className="text-xs text-[var(--color-text-muted)]">
                          {humanizeSupplierUnitKind(supplier.supplierUnitKind)} · {supplier.approvalStatus} · {supplier.status}
                        </div>
                        {supplier.supplierServiceTypes?.length ? (
                          <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                            {formatSupplierServiceTypes(supplier.supplierServiceTypes)}
                          </div>
                        ) : null}
                      </div>
                      <span className="text-xs text-[var(--color-text-muted)]">
                          {supplier.partSupplierLinkCount} source links
                      </span>
                    </div>
                    <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                      Open PR {supplier.openPurchaseRequestCount} · Open PO {supplier.openPurchaseOrderCount}{' '}
                      · Issued PO {supplier.issuedPurchaseOrderCount} · Backorders {supplier.openBackorderCount}
                    </p>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </>
      )}

      {selectedSupplierId && detailQuery.data && (
        <div className="mt-6 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
          <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">
            Detail · {formatSupplierIdentitySummary({
              supplierDisplayName:
                detailQuery.data.summary.supplierDisplayName
                ?? detailQuery.data.summary.displayName
                ?? 'Supplier',
              supplierKey: detailQuery.data.summary.supplierKey,
              parentSupplierDisplayName: detailQuery.data.summary.parentSupplierDisplayName,
              supplierUnitKind: detailQuery.data.summary.supplierUnitKind,
            })}
          </h3>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            {humanizeSupplierUnitKind(detailQuery.data.summary.supplierUnitKind)} · Receipts posted {detailQuery.data.summary.postedReceivingReceiptCount} · Open line qty {detailQuery.data.summary.openPurchaseOrderLineQuantity}
          </p>
          {detailQuery.data.summary.supplierServiceTypes?.length ? (
            <p className="mt-1 text-xs text-[var(--color-text-muted)]">
              Services provided: {formatSupplierServiceTypes(detailQuery.data.summary.supplierServiceTypes)}
            </p>
          ) : null}

          {scorecard ? (
            <div className="mt-4 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h4 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
                    Supplier scorecard
                  </h4>
                  <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
                    {scorecard.status} · {scorecard.score}/100
                  </p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    Built from source coverage, recent fulfillment, return, warranty, and approval signals.
                  </p>
                </div>
                <span
                  className={`rounded-full border px-3 py-1 text-xs font-semibold uppercase tracking-wide ${
                    scorecard.status === 'Healthy'
                      ? 'border-[var(--color-success-border)] bg-[var(--color-success-bg)] text-[var(--color-success-text)]'
                      : scorecard.status === 'Watch'
                        ? 'border-[var(--color-warning-border)] bg-[var(--color-warning-bg)] text-[var(--color-warning-text)]'
                        : 'border-[var(--tone-danger-border)] bg-[var(--tone-danger-bg)] text-[var(--tone-danger-text)]'
                  }`}
                >
                  {scorecard.status}
                </span>
              </div>

              <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                <Metric label="Source coverage" value={`${scorecard.catalogCoverage}%`} />
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
                  <h5 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                    Compliance documents
                  </h5>
                  <ul className="mt-2 space-y-2 text-xs text-[var(--color-text-secondary)]">
                    {complianceDetailQuery.data.documents.slice(0, 4).map((doc) => (
                      <li
                        key={doc.documentId}
                        className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2"
                      >
                        <div className="font-medium text-[var(--color-text-primary)]">
                          {doc.documentKey} · {doc.title}
                        </div>
                        <div className="mt-1 text-[var(--color-text-muted)]">
                          {doc.documentTypeKey} · {doc.effectiveStatus}
                          {doc.expiresAt ? ` · expires ${new Date(doc.expiresAt).toLocaleDateString()}` : ''}
                        </div>
                      </li>
                    ))}
                  </ul>
                </div>
              ) : (
                <p className="mt-4 text-xs text-[var(--color-text-muted)]">No linked compliance documents found for this supplier.</p>
              )}

              <p className="mt-3 text-xs text-[var(--color-text-muted)]">
                Activity recency:{' '}
                {scorecard.activityRecencyDays == null ? 'No recent activity' : `${scorecard.activityRecencyDays} day(s)`}
              </p>
            </div>
          ) : null}

          {detailQuery.data.recentPurchaseOrders.length > 0 && (
            <div className="mt-4">
              <h4 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                Recent purchase orders
              </h4>
              <ul className="mt-2 space-y-1 text-sm text-[var(--color-text-secondary)]">
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
              <h4 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                Supplier source links
              </h4>
              <ul className="mt-2 space-y-1 text-sm text-[var(--color-text-secondary)]">
                {detailQuery.data.partLinks.map((row) => (
                  <li key={row.partSupplierLinkId}>
                    {row.partKey} · {row.supplierPartNumber}
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
    <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2">
      <div className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">{label}</div>
      <div className="mt-1 text-sm font-semibold text-[var(--color-text-primary)]">{value}</div>
    </div>
  )
}

function formatPercent(value: number | null): string {
  return value == null ? 'n/a' : `${value}%`
}

function formatDays(value: number | null): string {
  return value == null ? 'n/a' : `${value} day${value === 1 ? '' : 's'}`
}
