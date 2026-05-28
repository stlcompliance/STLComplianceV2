import { useMutation, useQuery } from '@tanstack/react-query'

import {
  exportDispatchExceptionsCsv,
  exportRoutesCsv,
  exportTripsCsv,
  getEntityExportManifest,
} from '../api/client'

type Props = {
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

export function DataExportsPanel({ accessToken, canExport }: Props) {
  const manifestQuery = useQuery({
    queryKey: ['routarr-entity-export-manifest', accessToken],
    queryFn: () => getEntityExportManifest(accessToken),
    enabled: canExport,
  })

  const tripsExportMutation = useMutation({
    mutationFn: () => exportTripsCsv(accessToken),
    onSuccess: (blob) => triggerBlobDownload(blob, `routarr-trips-${new Date().toISOString().slice(0, 10)}.csv`),
  })

  const routesExportMutation = useMutation({
    mutationFn: () => exportRoutesCsv(accessToken),
    onSuccess: (blob) => triggerBlobDownload(blob, `routarr-routes-${new Date().toISOString().slice(0, 10)}.csv`),
  })

  const exceptionsExportMutation = useMutation({
    mutationFn: () => exportDispatchExceptionsCsv(accessToken),
    onSuccess: (blob) =>
      triggerBlobDownload(blob, `routarr-dispatch-exceptions-${new Date().toISOString().slice(0, 10)}.csv`),
  })

  if (!canExport) {
    return (
      <section
        className="mt-8 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
        data-testid="data-exports-panel"
      >
        <h2 className="text-lg font-semibold text-slate-50">Data exports</h2>
        <p className="mt-2 text-sm text-slate-500">
          Bulk CSV exports require RoutArr manager, admin, or tenant admin role.
        </p>
      </section>
    )
  }

  const entities = manifestQuery.data?.entities ?? []

  return (
    <section
      className="mt-8 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="data-exports-panel"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Data exports</h2>
        <p className="mt-1 text-sm text-slate-400">
          Download tenant-scoped trip, route, and dispatch exception CSV files. Report rollups export
          from the panels above.
        </p>
      </header>

      {manifestQuery.isLoading ? (
        <p className="mt-3 text-sm text-slate-500">Loading export manifest…</p>
      ) : null}

      <ul className="mt-4 space-y-3">
        {entities.map((entity) => {
          const pending =
            (entity.entityKey === 'trips' && tripsExportMutation.isPending) ||
            (entity.entityKey === 'routes' && routesExportMutation.isPending) ||
            (entity.entityKey === 'dispatch_exceptions' && exceptionsExportMutation.isPending)

          const onExport = () => {
            if (entity.entityKey === 'trips') {
              tripsExportMutation.mutate()
            } else if (entity.entityKey === 'routes') {
              routesExportMutation.mutate()
            } else if (entity.entityKey === 'dispatch_exceptions') {
              exceptionsExportMutation.mutate()
            }
          }

          return (
            <li
              key={entity.entityKey}
              className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-slate-700 bg-slate-950/50 px-3 py-2"
            >
              <div>
                <p className="text-sm font-medium text-slate-100">{entity.label}</p>
                <p className="text-xs text-slate-500">{entity.description}</p>
              </div>
              <button
                type="button"
                className="rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
                disabled={pending}
                onClick={onExport}
              >
                {pending ? 'Exporting…' : 'Download CSV'}
              </button>
            </li>
          )
        })}
      </ul>
    </section>
  )
}
