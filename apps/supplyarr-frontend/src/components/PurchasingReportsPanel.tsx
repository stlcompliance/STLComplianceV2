import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ShoppingCart } from 'lucide-react'

import {
  exportPurchasingReportSummaryCsv,
  getPurchasingPurchaseOrderDetail,
  getPurchasingPurchaseRequestDetail,
  getPurchasingReportSummary,
} from '../api/client'

interface PurchasingReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function formatDocumentType(type: string): string {
  return type === 'purchase_request' ? 'PR' : 'PO'
}

export function PurchasingReportsPanel({
  accessToken,
  canRead,
  canExport,
}: PurchasingReportsPanelProps) {
  const [openDocumentsOnly, setOpenDocumentsOnly] = useState(false)
  const [selectedPrId, setSelectedPrId] = useState<string | null>(null)
  const [selectedPoId, setSelectedPoId] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['supplyarr-purchasing-report-summary', accessToken, openDocumentsOnly],
    queryFn: () =>
      getPurchasingReportSummary(accessToken, {
        openDocumentsOnly: openDocumentsOnly || undefined,
      }),
    enabled: canRead,
  })

  const prDetailQuery = useQuery({
    queryKey: ['supplyarr-purchasing-pr-detail', accessToken, selectedPrId],
    queryFn: () => getPurchasingPurchaseRequestDetail(accessToken, selectedPrId!),
    enabled: canRead && Boolean(selectedPrId),
  })

  const poDetailQuery = useQuery({
    queryKey: ['supplyarr-purchasing-po-detail', accessToken, selectedPoId],
    queryFn: () => getPurchasingPurchaseOrderDetail(accessToken, selectedPoId!),
    enabled: canRead && Boolean(selectedPoId),
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportPurchasingReportSummaryCsv(accessToken, {
        openDocumentsOnly: openDocumentsOnly || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `supplyarr-purchasing-${new Date().toISOString().slice(0, 10)}.csv`
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
      data-testid="purchasing-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex gap-3">
          <ShoppingCart className="mt-0.5 h-5 w-5 text-sky-400" aria-hidden />
          <div>
            <h2 className="text-lg font-semibold text-slate-50">Purchasing reports</h2>
            <p className="mt-1 text-sm text-slate-400">
              Purchase requests, orders, receiving, and backorder pipeline rollups.
            </p>
          </div>
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
        <label className="flex items-center gap-2 text-slate-300">
          <input
            type="checkbox"
            checked={openDocumentsOnly}
            onChange={(event) => setOpenDocumentsOnly(event.target.checked)}
          />
          Open documents only
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-500">Loading purchasing summary…</p>
      )}

      {summaryQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load purchasing report.</p>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 flex flex-wrap gap-2 text-xs">
            <span className="rounded-md bg-slate-800 px-2 py-1 text-slate-300">
              PRs: {summaryQuery.data.totals.openPurchaseRequestCount} open /{' '}
              {summaryQuery.data.totals.purchaseRequestCount} total
            </span>
            <span className="rounded-md bg-slate-800 px-2 py-1 text-slate-300">
              POs: {summaryQuery.data.totals.issuedPurchaseOrderCount} issued ·{' '}
              {summaryQuery.data.totals.openPurchaseOrderCount} open
            </span>
            <span className="rounded-md bg-slate-800 px-2 py-1 text-slate-300">
              Receiving: {summaryQuery.data.totals.postedReceivingReceiptCount} posted
            </span>
            <span className="rounded-md bg-amber-950 px-2 py-1 text-amber-200">
              Backorders: {summaryQuery.data.totals.openBackorderCount}
            </span>
          </div>

          {summaryQuery.data.documents.length === 0 ? (
            <p className="mt-4 text-sm text-slate-500">No documents match the current filters.</p>
          ) : (
            <ul className="mt-4 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
              {summaryQuery.data.documents.map((doc) => (
                <li key={`${doc.documentType}-${doc.documentId}`} className="px-3 py-3">
                  <button
                    type="button"
                    className="w-full text-left"
                    onClick={() => {
                      if (doc.documentType === 'purchase_request') {
                        setSelectedPrId(doc.documentId)
                        setSelectedPoId(null)
                      } else {
                        setSelectedPoId(doc.documentId)
                        setSelectedPrId(null)
                      }
                    }}
                  >
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <div className="font-medium text-slate-100">
                          {formatDocumentType(doc.documentType)} {doc.documentKey} · {doc.title}
                        </div>
                        <div className="text-xs text-slate-500">
                          {doc.vendorDisplayName || 'No vendor'} · {doc.status}
                        </div>
                      </div>
                      <span className="text-xs text-slate-400">{doc.lineCount} lines</span>
                    </div>
                    <p className="mt-2 text-xs text-slate-400">
                      Ordered {doc.quantityOrdered} · Received {doc.quantityReceived}
                    </p>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </>
      )}

      {selectedPrId && prDetailQuery.data && (
        <div className="mt-6 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <h3 className="text-sm font-semibold text-slate-100">
            PR detail · {prDetailQuery.data.summary.documentKey}
          </h3>
          {prDetailQuery.data.linkedPurchaseOrderKey && (
            <p className="mt-1 text-xs text-slate-500">
              Linked PO: {prDetailQuery.data.linkedPurchaseOrderKey}
            </p>
          )}
          {prDetailQuery.data.lines.length > 0 && (
            <ul className="mt-3 space-y-1 text-sm text-slate-300">
              {prDetailQuery.data.lines.map((line) => (
                <li key={line.lineId}>
                  {line.partKey}: {line.quantityRequested} {line.unitOfMeasure}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {selectedPoId && poDetailQuery.data && (
        <div className="mt-6 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <h3 className="text-sm font-semibold text-slate-100">
            PO detail · {poDetailQuery.data.summary.documentKey}
          </h3>
          {poDetailQuery.data.lines.length > 0 && (
            <ul className="mt-3 space-y-1 text-sm text-slate-300">
              {poDetailQuery.data.lines.map((line) => (
                <li key={line.lineId}>
                  {line.partKey}: received {line.quantityReceived} of {line.quantityOrdered}
                </li>
              ))}
            </ul>
          )}
          {poDetailQuery.data.receivingReceipts.length > 0 && (
            <p className="mt-2 text-xs text-slate-500">
              {poDetailQuery.data.receivingReceipts.length} receiving receipt(s)
            </p>
          )}
          {poDetailQuery.data.backorders.length > 0 && (
            <p className="mt-1 text-xs text-amber-300">
              {poDetailQuery.data.backorders.length} backorder(s) on this PO
            </p>
          )}
        </div>
      )}
    </section>
  )
}
