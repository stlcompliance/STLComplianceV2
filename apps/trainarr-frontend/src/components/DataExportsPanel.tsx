import { useMutation, useQuery } from '@tanstack/react-query'
import {
  exportQualificationIssuesCsv,
  exportTrainingAssignmentsCsv,
  exportTrainingDefinitionsCsv,
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
    queryKey: ['trainarr-entity-export-manifest', accessToken],
    queryFn: () => getEntityExportManifest(accessToken),
    enabled: canExport,
  })

  const assignmentsExportMutation = useMutation({
    mutationFn: () => exportTrainingAssignmentsCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(blob, `trainarr-training-assignments-${new Date().toISOString().slice(0, 10)}.csv`)
    },
  })

  const qualificationsExportMutation = useMutation({
    mutationFn: () => exportQualificationIssuesCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(blob, `trainarr-qualification-issues-${new Date().toISOString().slice(0, 10)}.csv`)
    },
  })

  const definitionsExportMutation = useMutation({
    mutationFn: () => exportTrainingDefinitionsCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(blob, `trainarr-training-definitions-${new Date().toISOString().slice(0, 10)}.csv`)
    },
  })

  if (!canExport) {
    return (
      <section
        className="mt-6 rounded-xl border border-border bg-card p-5"
        data-testid="data-exports-panel"
      >
        <h2 className="text-lg font-semibold text-foreground">Data exports</h2>
        <p className="mt-2 text-sm text-muted-foreground">
          Bulk CSV exports require tenant admin or TrainArr admin role.
        </p>
      </section>
    )
  }

  const entities = manifestQuery.data?.entities ?? []

  return (
    <section
      className="mt-6 rounded-xl border border-border bg-card p-5"
      data-testid="data-exports-panel"
    >
      <header>
        <h2 className="text-lg font-semibold text-foreground">Data exports</h2>
        <p className="mt-1 text-sm text-muted-foreground">
          Bulk CSV exports for training assignments, qualifications, and definitions.
        </p>
      </header>

      {manifestQuery.isLoading && (
        <p className="mt-3 text-sm text-muted-foreground">Loading export manifest…</p>
      )}

      {manifestQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load export manifest.</p>
      )}

      {manifestQuery.data && (
        <div className="mt-4 space-y-3">
          {entities.map((entity) => (
            <div
              key={entity.entityKey}
              className="flex flex-wrap items-center justify-between gap-3 rounded-md border border-border bg-background px-3 py-3"
            >
              <div>
                <p className="font-medium text-foreground">{entity.displayName}</p>
                <p className="text-sm text-muted-foreground">{entity.description}</p>
              </div>
              <button
                type="button"
                className="rounded-md border border-border px-3 py-1.5 text-sm font-medium hover:bg-muted disabled:opacity-50"
                disabled={
                  (entity.entityKey === 'training_assignments' && assignmentsExportMutation.isPending)
                  || (entity.entityKey === 'qualification_issues' && qualificationsExportMutation.isPending)
                  || (entity.entityKey === 'training_definitions' && definitionsExportMutation.isPending)
                }
                onClick={() => {
                  if (entity.entityKey === 'training_assignments') {
                    assignmentsExportMutation.mutate()
                  } else if (entity.entityKey === 'qualification_issues') {
                    qualificationsExportMutation.mutate()
                  } else if (entity.entityKey === 'training_definitions') {
                    definitionsExportMutation.mutate()
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
