import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  exportBulkEvaluationsCsv,
  exportBulkFindingsCsv,
  exportBulkRulePacksCsv,
  exportBulkWorkflowGateChecksCsv,
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
    queryKey: ['compliancecore-entity-export-manifest', accessToken],
    queryFn: () => getEntityExportManifest(accessToken),
    enabled: canExport,
  })

  const findingsExportMutation = useMutation({
    mutationFn: () => exportBulkFindingsCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(blob, `compliancecore-findings-${new Date().toISOString().slice(0, 10)}.csv`)
    },
  })

  const evaluationsExportMutation = useMutation({
    mutationFn: () => exportBulkEvaluationsCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(
        blob,
        `compliancecore-evaluations-${new Date().toISOString().slice(0, 10)}.csv`,
      )
    },
  })

  const rulePacksExportMutation = useMutation({
    mutationFn: () => exportBulkRulePacksCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(
        blob,
        `compliancecore-rule-packs-${new Date().toISOString().slice(0, 10)}.csv`,
      )
    },
  })

  const workflowGateChecksExportMutation = useMutation({
    mutationFn: () => exportBulkWorkflowGateChecksCsv(accessToken),
    onSuccess: (blob) => {
      triggerBlobDownload(
        blob,
        `compliancecore-workflow-gate-checks-${new Date().toISOString().slice(0, 10)}.csv`,
      )
    },
  })

  const exportError =
    findingsExportMutation.error ??
    evaluationsExportMutation.error ??
    rulePacksExportMutation.error ??
    workflowGateChecksExportMutation.error

  if (!canExport) {
    return (
      <section
        className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
        data-testid="data-exports-panel"
      >
        <h2 className="text-lg font-semibold text-slate-50">Data exports</h2>
        <p className="mt-2 text-sm text-slate-400">
          Bulk CSV exports require tenant admin, compliance admin, or compliance reviewer role.
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
          Bulk CSV exports for findings, rule evaluation runs, workflow gate checks, and rule packs.
        </p>
      </header>

      {manifestQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading export manifest…</p>
      )}

      {manifestQuery.isError && (
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
      )}

      {exportError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="CSV export failed"
            message={getErrorMessage(exportError, 'Unable to export CSV.')}
          />
        </div>
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
                className="rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
                disabled={
                  (entity.entityKey === 'findings' && findingsExportMutation.isPending) ||
                  (entity.entityKey === 'evaluations' && evaluationsExportMutation.isPending) ||
                  (entity.entityKey === 'workflow_gate_checks' &&
                    workflowGateChecksExportMutation.isPending) ||
                  (entity.entityKey === 'rule_packs' && rulePacksExportMutation.isPending)
                }
                onClick={() => {
                  if (entity.entityKey === 'findings') {
                    findingsExportMutation.mutate()
                  } else if (entity.entityKey === 'evaluations') {
                    evaluationsExportMutation.mutate()
                  } else if (entity.entityKey === 'workflow_gate_checks') {
                    workflowGateChecksExportMutation.mutate()
                  } else if (entity.entityKey === 'rule_packs') {
                    rulePacksExportMutation.mutate()
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
