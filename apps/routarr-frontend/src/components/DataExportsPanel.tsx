import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportDispatchExceptionsCsv,
  exportRoutesCsv,
  exportTripsCsv,
  getEntityExportManifest,
} from '../api/client'

interface DataExportsPanelProps {
  accessToken: string
  canExport: boolean
}

function triggerBlobDownload(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  anchor.click()
  URL.revokeObjectURL(url)
}

export function DataExportsPanel({ accessToken, canExport }: DataExportsPanelProps) {
  const manifestQuery = useQuery({
    queryKey: ['routarr-entity-export-manifest', accessToken],
    queryFn: () => getEntityExportManifest(accessToken),
    enabled: canExport,
  })

  const tripsExportMutation = useMutation({
    mutationFn: () => exportTripsCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(blob, `routarr-trips-${new Date().toISOString().slice(0, 10)}.csv`)
    },
  })

  const routesExportMutation = useMutation({
    mutationFn: () => exportRoutesCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(blob, `routarr-routes-${new Date().toISOString().slice(0, 10)}.csv`)
    },
  })

  const exceptionsExportMutation = useMutation({
    mutationFn: () => exportDispatchExceptionsCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(
        blob,
        `routarr-dispatch-exceptions-${new Date().toISOString().slice(0, 10)}.csv`,
      )
    },
  })

  const exportError =
    tripsExportMutation.error ?? routesExportMutation.error ?? exceptionsExportMutation.error

  if (!canExport) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="data-exports-panel">
        <h2 className="text-lg font-semibold text-slate-50">Data exports</h2>
        <p className="mt-2 text-sm text-slate-400">
          Bulk CSV exports require tenant admin, RoutArr admin, or dispatch admin access.
        </p>
      </section>
    )
  }

  const entities = manifestQuery.data?.entities ?? []

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="data-exports-panel">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Data exports</h2>
        <p className="mt-1 text-sm text-slate-400">
          Bulk CSV exports for trips, routes, and dispatch exceptions.
        </p>
      </header>

      {manifestQuery.isLoading ? (
        <p className="mt-3 text-sm text-slate-400">Loading export manifest…</p>
      ) : null}

      {manifestQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Export manifest unavailable"
            message={getErrorMessage(manifestQuery.error, 'Failed to load export manifest.')}
            retryLabel="Retry manifest"
            onRetry={() => {
              void manifestQuery.refetch()
            }}
          />
        </div>
      ) : null}

      {exportError ? (
        <div className="mt-3">
          <ApiErrorCallout title="CSV export failed" message={getErrorMessage(exportError, 'Unable to export CSV.')} />
        </div>
      ) : null}

      <div className="mt-4 space-y-3">
        {entities.map((entity) => (
          <div
            key={entity.entityKey}
            className="flex flex-wrap items-center justify-between gap-3 rounded-md border border-slate-700 bg-slate-950 px-3 py-3"
          >
            <div>
              <p className="font-medium text-slate-100">{entity.label}</p>
              <p className="text-sm text-slate-400">{entity.description}</p>
            </div>
            <button
              type="button"
              className="rounded-md border border-slate-600 px-3 py-1.5 text-sm font-medium text-slate-100 hover:bg-slate-800 disabled:opacity-50"
              disabled={
                tripsExportMutation.isPending ||
                routesExportMutation.isPending ||
                exceptionsExportMutation.isPending
              }
              onClick={() => {
                if (entity.entityKey === 'trips') {
                  tripsExportMutation.mutate()
                } else if (entity.entityKey === 'routes') {
                  routesExportMutation.mutate()
                } else if (entity.entityKey === 'dispatch_exceptions') {
                  exceptionsExportMutation.mutate()
                }
              }}
            >
              Download CSV
            </button>
          </div>
        ))}
      </div>
    </section>
  )
}
