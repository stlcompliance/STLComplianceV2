import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  commitFactSourceIngestion,
  listSourceIngestionBatches,
  validateFactSourceIngestion,
} from '../api/client'
import type { FactSourceBulkIngestionRequest, SourceIngestionBatchResponse } from '../api/types'

const SAMPLE_PAYLOAD: FactSourceBulkIngestionRequest = {
  sources: [
    {
      factDefinitionId: '00000000-0000-0000-0000-000000000000',
      sourceKey: 'sample_static_source',
      sourceType: 'static_config',
      label: 'Sample static source',
      description: 'Replace factDefinitionId with a catalog fact id before commit.',
      configJson: '{"booleanValue":true}',
      priority: 0,
    },
  ],
}

interface SourceIngestionPanelProps {
  accessToken: string
  canManage: boolean
}

export function SourceIngestionPanel({ accessToken, canManage }: SourceIngestionPanelProps) {
  const queryClient = useQueryClient()
  const [jsonText, setJsonText] = useState(() => JSON.stringify(SAMPLE_PAYLOAD, null, 2))
  const [lastResult, setLastResult] = useState<SourceIngestionBatchResponse | null>(null)
  const [parseError, setParseError] = useState<string | null>(null)

  const batchesQuery = useQuery({
    queryKey: ['compliancecore-source-ingestion-batches', accessToken],
    queryFn: () => listSourceIngestionBatches(accessToken, 'fact_sources'),
  })

  const validateMutation = useMutation({
    mutationFn: (payload: FactSourceBulkIngestionRequest) =>
      validateFactSourceIngestion(accessToken, payload),
    onSuccess: (result) => {
      setLastResult(result)
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-source-ingestion-batches'] })
    },
  })

  const commitMutation = useMutation({
    mutationFn: (payload: FactSourceBulkIngestionRequest) =>
      commitFactSourceIngestion(accessToken, payload),
    onSuccess: (result) => {
      setLastResult(result)
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-source-ingestion-batches'] })
    },
  })

  function parsePayload(): FactSourceBulkIngestionRequest | null {
    try {
      const parsed = JSON.parse(jsonText) as FactSourceBulkIngestionRequest
      if (!Array.isArray(parsed.sources) || parsed.sources.length === 0) {
        setParseError('JSON must include a non-empty sources array.')
        return null
      }
      setParseError(null)
      return parsed
    } catch {
      setParseError('JSON is not valid.')
      return null
    }
  }

  function runValidate() {
    const payload = parsePayload()
    if (!payload) {
      return
    }
    validateMutation.mutate(payload)
  }

  function runCommit() {
    const payload = parsePayload()
    if (!payload) {
      return
    }
    commitMutation.mutate(payload)
  }

  const pending = validateMutation.isPending || commitMutation.isPending

  return (
    <section className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Source ingestion</h2>
        <p className="mt-1 text-sm text-slate-400">
          Validate then commit fact source batches (max 50 rows). Product fact batches use the service-token
          integration API with scope compliancecore.sources.ingest.
        </p>
      </header>

      {canManage ? (
        <div className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <label className="block text-sm font-medium text-slate-200" htmlFor="source-ingestion-json">
            Fact sources JSON
          </label>
          <textarea
            id="source-ingestion-json"
            value={jsonText}
            onChange={(event) => setJsonText(event.target.value)}
            rows={12}
            className="w-full rounded-md border border-slate-700 bg-slate-950 font-mono text-xs text-slate-200"
          />
          {parseError && <p className="text-sm text-rose-400">{parseError}</p>}
          <div className="flex flex-wrap gap-3">
            <button
              type="button"
              onClick={runValidate}
              disabled={pending}
              className="rounded-md bg-slate-700 px-4 py-2 text-sm font-medium text-white hover:bg-slate-600 disabled:opacity-50"
            >
              {validateMutation.isPending ? 'Validating…' : 'Validate batch'}
            </button>
            <button
              type="button"
              onClick={runCommit}
              disabled={pending}
              className="rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
            >
              {commitMutation.isPending ? 'Committing…' : 'Commit batch'}
            </button>
          </div>
        </div>
      ) : (
        <p className="text-sm text-slate-400">Source ingestion management requires compliance admin or tenant admin.</p>
      )}

      {lastResult && (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Batch <span className="font-mono text-violet-300">{lastResult.batchId}</span> — {lastResult.status} (
            {lastResult.successCount} ok, {lastResult.errorCount} errors, {lastResult.skippedCount} skipped)
          </p>
          <ul className="mt-2 max-h-40 space-y-1 overflow-y-auto">
            {lastResult.jobs.map((job) => (
              <li key={`${job.rowIndex}-${job.jobKey}`} className="font-mono text-xs">
                {job.jobKey}: {job.status}
                {job.errorCode ? ` (${job.errorCode})` : ''}
              </li>
            ))}
          </ul>
        </div>
      )}

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Recent batches</h3>
        {(batchesQuery.data ?? []).length === 0 ? (
          <p className="mt-2 text-sm text-slate-500">No ingestion batches yet.</p>
        ) : (
          <ul className="mt-2 space-y-2">
            {(batchesQuery.data ?? []).map((batch) => (
              <li key={batch.batchId} className="rounded border border-slate-800 px-3 py-2 text-xs text-slate-400">
                <span className="font-mono text-slate-300">{batch.batchId.slice(0, 8)}…</span> — {batch.phase} /{' '}
                {batch.status} — {batch.successCount}/{batch.totalJobs} at{' '}
                {new Date(batch.createdAt).toLocaleString()}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
