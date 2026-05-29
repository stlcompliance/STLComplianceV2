import { useMutation, useQuery } from '@tanstack/react-query'
import {
  exportBulkPeopleCsv,
  exportBulkPersonCertificationsCsv,
  exportBulkPersonnelIncidentsCsv,
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
    queryKey: ['staffarr-entity-export-manifest', accessToken],
    queryFn: () => getEntityExportManifest(accessToken),
    enabled: canExport,
  })

  const peopleExportMutation = useMutation({
    mutationFn: () => exportBulkPeopleCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(blob, `staffarr-people-${new Date().toISOString().slice(0, 10)}.csv`)
    },
  })

  const incidentsExportMutation = useMutation({
    mutationFn: () => exportBulkPersonnelIncidentsCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(
        blob,
        `staffarr-personnel-incidents-${new Date().toISOString().slice(0, 10)}.csv`,
      )
    },
  })

  const certificationsExportMutation = useMutation({
    mutationFn: () => exportBulkPersonCertificationsCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(
        blob,
        `staffarr-person-certifications-${new Date().toISOString().slice(0, 10)}.csv`,
      )
    },
  })

  if (!canExport) {
    return (
      <section
        className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
        data-testid="data-exports-panel"
      >
        <h2 className="text-lg font-semibold text-slate-50">Data exports</h2>
        <p className="mt-2 text-sm text-slate-400">
          Bulk CSV exports require tenant admin, StaffArr admin, or HR admin role.
        </p>
      </section>
    )
  }

  const entities = manifestQuery.data?.entities ?? []

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="data-exports-panel"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Data exports</h2>
        <p className="mt-1 text-sm text-slate-400">
          Bulk CSV exports for people, personnel incidents, and person certifications.
        </p>
      </header>

      {manifestQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading export manifest…</p>
      )}

      {manifestQuery.isError && (
        <p className="mt-3 text-sm text-red-300">Failed to load export manifest.</p>
      )}

      {manifestQuery.data && (
        <div className="mt-4 space-y-3">
          {entities.map((entity) => (
            <div
              key={entity.entityKey}
              className="flex flex-wrap items-center justify-between gap-3 rounded-md border border-slate-700 bg-slate-950 px-3 py-3"
            >
              <div>
                <p className="font-medium text-slate-100">{entity.displayName}</p>
                <p className="text-sm text-slate-400">{entity.description}</p>
              </div>
              <button
                type="button"
                className="rounded-md border border-slate-600 px-3 py-1.5 text-sm font-medium text-slate-100 hover:bg-slate-800 disabled:opacity-50"
                disabled={
                  peopleExportMutation.isPending ||
                  incidentsExportMutation.isPending ||
                  certificationsExportMutation.isPending
                }
                onClick={() => {
                  if (entity.entityKey === 'people') {
                    peopleExportMutation.mutate()
                  } else if (entity.entityKey === 'personnel_incidents') {
                    incidentsExportMutation.mutate()
                  } else if (entity.entityKey === 'person_certifications') {
                    certificationsExportMutation.mutate()
                  }
                }}
              >
                Download CSV
              </button>
            </div>
          ))}
        </div>
      )}
    </section>
  )
}
