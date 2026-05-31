import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportVendorReportSummaryCsv,
  getVendorReportDetail,
  getVendorReportSummary,
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
            Procurement activity, catalog links, and receiving rollups per vendor.
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
            Posted receipts {detailQuery.data.summary.postedReceivingReceiptCount} · Open line qty{' '}
            {detailQuery.data.summary.openPurchaseOrderLineQuantity}
          </p>

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
