import { useMutation, useQuery } from '@tanstack/react-query'
import {
  exportAssetsCsv,
  exportInspectionRunsCsv,
  exportWorkOrdersCsv,
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
    queryKey: ['maintainarr-entity-export-manifest', accessToken],
    queryFn: () => getEntityExportManifest(accessToken),
    enabled: canExport,
  })

  const assetsExportMutation = useMutation({
    mutationFn: () => exportAssetsCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(blob, `maintainarr-assets-${new Date().toISOString().slice(0, 10)}.csv`)
    },
  })

  const workOrdersExportMutation = useMutation({
    mutationFn: () => exportWorkOrdersCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(
        blob,
        `maintainarr-work-orders-${new Date().toISOString().slice(0, 10)}.csv`,
      )
    },
  })

  const inspectionsExportMutation = useMutation({
    mutationFn: () => exportInspectionRunsCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(
        blob,
        `maintainarr-inspection-runs-${new Date().toISOString().slice(0, 10)}.csv`,
      )
    },
  })

  if (!canExport) {
    return (
      <section
        className="mt-8 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
        data-testid="data-exports-panel"
      >
        <h2 className="text-lg font-semibold text-slate-50">Data exports</h2>
        <p className="mt-2 text-sm text-slate-500">
          Bulk CSV exports require tenant admin, MaintainArr admin, or manager role.
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
          Download tenant-scoped entity CSV files. Report rollups export from the panels above;
          full audit packages are available under Settings.
        </p>
      </header>

      <ul className="mt-4 space-y-3">
        {entities.map((entity) => {
          const pending =
            (entity.entityKey === 'assets' && assetsExportMutation.isPending) ||
            (entity.entityKey === 'work_orders' && workOrdersExportMutation.isPending) ||
            (entity.entityKey === 'inspection_runs' && inspectionsExportMutation.isPending)

          const onExport = () => {
            if (entity.entityKey === 'assets') {
              assetsExportMutation.mutate()
            } else if (entity.entityKey === 'work_orders') {
              workOrdersExportMutation.mutate()
            } else if (entity.entityKey === 'inspection_runs') {
              inspectionsExportMutation.mutate()
            }
          }

          return (
            <li
              key={entity.entityKey}
              className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-slate-800 bg-slate-950/50 px-3 py-2"
            >
              <div>
                <p className="font-medium text-slate-200">{entity.label}</p>
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

      {manifestQuery.data?.reportExports.length ? (
        <p className="mt-4 text-xs text-slate-500">
          Report CSV endpoints:{' '}
          {manifestQuery.data.reportExports.map((r) => r.label).join(' · ')}
        </p>
      ) : null}
    </section>
  )
}
