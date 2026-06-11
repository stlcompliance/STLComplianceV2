import { CheckCircle2, FileUp, RefreshCw, Upload } from 'lucide-react'
import { useState } from 'react'
import type { ChangeEvent, FormEvent } from 'react'

export type SmartImportBatchRow = {
  batchId: string
  status: string
  destinationProductHint: string
  sourceLabel: string
  proposedRecordCount: number
  updatedAt: string
  errorMessage?: string | null
}

export type SmartImportProposedRecordRow = {
  proposedRecordId: string
  destinationProduct: string
  entityType: string
  operation: string
  confidence: number
  reviewStatus: string
  requiresReview: boolean
  reviewReasons: string[]
  proposedPayload: unknown
}

export type SmartImportBatchDetail = {
  batch: SmartImportBatchRow
  proposedRecords: SmartImportProposedRecordRow[]
}

export type SmartImportReviewWorkspaceProps = {
  batches: SmartImportBatchRow[]
  selectedBatch?: SmartImportBatchDetail | null
  isLoading?: boolean
  onRefresh: () => Promise<void> | void
  onSelectBatch: (batchId: string) => Promise<void> | void
  onUpload: (file: File, destinationProduct: string) => Promise<void> | void
  onReview: (proposedRecordId: string, decision: 'approved' | 'rejected' | 'needs_changes') => Promise<void> | void
  onCreateCommitPlan: (batchId: string) => Promise<void> | void
  initialDestinationProduct?: string
}

const destinationOptions = [
  'staffarr',
  'trainarr',
  'maintainarr',
  'routarr',
  'supplyarr',
  'compliancecore',
  'loadarr',
  'recordarr',
  'reportarr',
  'assurarr',
]

export function SmartImportReviewWorkspace({
  batches,
  selectedBatch,
  isLoading = false,
  onRefresh,
  onSelectBatch,
  onUpload,
  onReview,
  onCreateCommitPlan,
  initialDestinationProduct,
}: SmartImportReviewWorkspaceProps) {
  const [file, setFile] = useState<File | null>(null)
  const [destinationProduct, setDestinationProduct] = useState(
    initialDestinationProduct && destinationOptions.includes(initialDestinationProduct)
      ? initialDestinationProduct
      : destinationOptions[0],
  )

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    setFile(event.target.files?.[0] ?? null)
  }

  const handleUpload = async (event: FormEvent) => {
    event.preventDefault()
    if (!file) return
    await onUpload(file, destinationProduct)
    setFile(null)
  }

  return (
    <div className="grid gap-4 xl:grid-cols-[360px_minmax(0,1fr)]">
      <section className="rounded-md border border-slate-700 bg-slate-900/70">
        <div className="flex items-center justify-between border-b border-slate-700 px-4 py-3">
          <h2 className="text-sm font-semibold text-white">Smart Import</h2>
          <button
            type="button"
            title="Refresh"
            aria-label="Refresh"
            onClick={() => void onRefresh()}
            className="inline-flex h-8 w-8 items-center justify-center rounded-md text-slate-300 hover:bg-slate-800 hover:text-white"
          >
            <RefreshCw className="h-4 w-4" aria-hidden />
          </button>
        </div>

        <form onSubmit={handleUpload} className="space-y-3 border-b border-slate-700 p-4">
          <label className="block text-xs font-medium text-slate-300" htmlFor="smart-import-destination">
            Destination
          </label>
          <select
            id="smart-import-destination"
            value={destinationProduct}
            onChange={(event) => setDestinationProduct(event.target.value)}
            className="w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
          >
            {destinationOptions.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>

          <label className="flex cursor-pointer items-center gap-2 rounded-md border border-dashed border-slate-600 bg-slate-950 px-3 py-3 text-sm text-slate-200 hover:border-teal-400/60">
            <FileUp className="h-4 w-4 text-teal-300" aria-hidden />
            <span className="truncate">{file?.name ?? 'Choose source file'}</span>
            <input className="sr-only" type="file" onChange={handleFileChange} />
          </label>

          <button
            type="submit"
            disabled={!file}
            className="inline-flex w-full items-center justify-center gap-2 rounded-md bg-teal-500 px-3 py-2 text-sm font-semibold text-slate-950 hover:bg-teal-400 disabled:cursor-not-allowed disabled:bg-slate-700 disabled:text-slate-400"
          >
            <Upload className="h-4 w-4" aria-hidden />
            Upload
          </button>
        </form>

        <div className="max-h-[520px] overflow-auto p-2">
          {batches.map((batch) => (
            <button
              key={batch.batchId}
              type="button"
              onClick={() => void onSelectBatch(batch.batchId)}
              className="mb-2 block w-full rounded-md border border-slate-700 bg-slate-950 p-3 text-left hover:border-teal-400/50"
            >
              <div className="flex items-center justify-between gap-2">
                <p className="truncate text-sm font-medium text-white">{batch.sourceLabel}</p>
                <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-200">{batch.status}</span>
              </div>
              <p className="mt-1 text-xs text-slate-400">
                {batch.destinationProductHint} · {batch.proposedRecordCount} proposed
              </p>
            </button>
          ))}
        </div>
      </section>

      <section className="rounded-md border border-slate-700 bg-slate-900/70">
        <div className="flex items-center justify-between border-b border-slate-700 px-4 py-3">
          <div className="min-w-0">
            <h2 className="truncate text-sm font-semibold text-white">
              {selectedBatch?.batch.sourceLabel ?? 'Review queue'}
            </h2>
            <p className="text-xs text-slate-400">{isLoading ? 'Loading' : selectedBatch?.batch.status ?? 'No batch selected'}</p>
          </div>
          {selectedBatch ? (
            <button
              type="button"
              onClick={() => void onCreateCommitPlan(selectedBatch.batch.batchId)}
              className="inline-flex items-center gap-2 rounded-md border border-teal-400/50 bg-teal-500/10 px-3 py-2 text-sm text-teal-100 hover:bg-teal-500/20"
            >
              <CheckCircle2 className="h-4 w-4" aria-hidden />
              Plan commit
            </button>
          ) : null}
        </div>

        <div className="space-y-3 p-4">
          {selectedBatch?.proposedRecords.map((record) => (
            <article key={record.proposedRecordId} className="rounded-md border border-slate-700 bg-slate-950 p-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div>
                  <h3 className="text-sm font-semibold text-white">
                    {record.destinationProduct} · {record.entityType}
                  </h3>
                  <p className="text-xs text-slate-400">
                    {record.operation} · {record.confidence}% · {record.reviewStatus}
                  </p>
                </div>
                <div className="flex gap-2">
                  <button
                    type="button"
                    onClick={() => void onReview(record.proposedRecordId, 'approved')}
                    className="rounded-md bg-emerald-500 px-3 py-1.5 text-xs font-semibold text-slate-950 hover:bg-emerald-400"
                  >
                    Approve
                  </button>
                  <button
                    type="button"
                    onClick={() => void onReview(record.proposedRecordId, 'rejected')}
                    className="rounded-md border border-slate-600 px-3 py-1.5 text-xs text-slate-200 hover:bg-slate-800"
                  >
                    Reject
                  </button>
                </div>
              </div>
              <div className="mt-3 flex flex-wrap gap-2">
                {record.reviewReasons.map((reason) => (
                  <span key={reason} className="rounded bg-amber-500/10 px-2 py-1 text-xs text-amber-200">
                    {reason}
                  </span>
                ))}
              </div>
            </article>
          ))}
        </div>
      </section>
    </div>
  )
}
