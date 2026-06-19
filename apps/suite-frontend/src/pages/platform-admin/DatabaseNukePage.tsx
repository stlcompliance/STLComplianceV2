import { useMutation, useQuery } from '@tanstack/react-query'
import { AlertTriangle, Database, RefreshCw, ShieldCheck, Trash2 } from 'lucide-react'
import { useMemo, useState, type FormEvent } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import type { DatabaseNukeTargetPreview } from '../../api/types'
import {
  PlatformAdminKpiCard,
  PlatformAdminPageHeader,
  PlatformAdminScopeNote,
  PlatformAdminSection,
} from '../../components/platform-admin/PlatformAdminPageChrome'
import { useToast } from '../../feedback'

const numberFormatter = new Intl.NumberFormat()

export function DatabaseNukePage() {
  const { pushToast } = useToast()
  const [confirmationPhrase, setConfirmationPhrase] = useState('')
  const [reason, setReason] = useState('')

  const previewQuery = useQuery({
    queryKey: ['platform-admin-database-nuke-preview'],
    queryFn: () => nexarr.getDatabaseNukePreview(),
  })

  const executeMutation = useMutation({
    mutationFn: () =>
      nexarr.executeDatabaseNuke({
        confirmationPhrase,
        reason,
      }),
    onSuccess: (result) => {
      pushToast({
        variant: result.targets.some((target) => target.status === 'error') ? 'error' : 'success',
        message: `Database nuke run ${result.runId} completed.`,
      })
      setConfirmationPhrase('')
      setReason('')
      void previewQuery.refetch()
    },
    onError: (error) => {
      pushToast({
        variant: 'error',
        message: getErrorMessage(error, 'Database nuke failed.'),
      })
    },
  })

  const preview = previewQuery.data
  const summary = useMemo(() => summarizeTargets(preview?.targets ?? []), [preview?.targets])
  const canExecute =
    Boolean(preview?.isEnabled) &&
    summary.readyTargets > 0 &&
    summary.truncateTableCount > 0 &&
    summary.errorTargets === 0 &&
    confirmationPhrase.trim() === preview?.confirmationPhrase &&
    reason.trim().length >= 10 &&
    !executeMutation.isPending

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!canExecute) {
      return
    }
    executeMutation.mutate()
  }

  if (previewQuery.isLoading) {
    return <p className="text-sm text-slate-500">Loading database nuke preview...</p>
  }

  if (previewQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(previewQuery.error, 'Failed to load database nuke preview.')}
        onRetry={() => void previewQuery.refetch()}
        retryLabel="Retry preview"
      />
    )
  }

  return (
    <div className="space-y-6">
      <PlatformAdminPageHeader
        title="Database nuke"
        summary="Platform-owner reset for product data tables across configured product databases. Reference data, NexArr control-plane records, schema history, and audit trails are preserved."
        badge="Critical action"
        updatedAt={preview ? new Date(preview.generatedAt).toLocaleString() : null}
        actions={
          <button
            type="button"
            onClick={() => void previewQuery.refetch()}
            className="inline-flex items-center gap-2 rounded-md border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
          >
            <RefreshCw className="h-4 w-4" aria-hidden />
            Refresh
          </button>
        }
      />

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <PlatformAdminKpiCard
          label="Ready databases"
          value={summary.readyTargets}
          hint={`${summary.missingTargets} target databases are not configured for NexArr access.`}
          tone={summary.errorTargets ? 'bad' : summary.readyTargets ? 'warn' : 'neutral'}
        />
        <PlatformAdminKpiCard
          label="Tables to wipe"
          value={summary.truncateTableCount}
          hint={`${numberFormatter.format(summary.estimatedRowsToDelete)} estimated rows are in scope.`}
          tone={summary.truncateTableCount ? 'bad' : 'neutral'}
        />
        <PlatformAdminKpiCard
          label="Tables preserved"
          value={summary.preserveTableCount}
          hint={`${numberFormatter.format(summary.estimatedRowsPreserved)} estimated rows remain untouched.`}
          tone="good"
        />
        <PlatformAdminKpiCard
          label="Preview errors"
          value={summary.errorTargets}
          hint="Configured databases with preview errors block execution."
          tone={summary.errorTargets ? 'bad' : 'good'}
        />
      </div>

      {!preview?.isEnabled ? (
        <div className="flex items-start gap-3 rounded-lg border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">
          <ShieldCheck className="mt-0.5 h-5 w-5 shrink-0" aria-hidden />
          <p>Database nuke is disabled in this environment.</p>
        </div>
      ) : null}

      <PlatformAdminSection
        title="Impact preview"
        description="NexArr will truncate product data tables for ready targets only; missing targets are skipped and preview errors block execution."
      >
        <div className="overflow-x-auto">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th className="px-3 py-2">Database</th>
                <th className="px-3 py-2">Status</th>
                <th className="px-3 py-2">Wipe tables</th>
                <th className="px-3 py-2">Preserved</th>
                <th className="px-3 py-2">Rows in scope</th>
                <th className="px-3 py-2">Notes</th>
              </tr>
            </thead>
            <tbody>
              {(preview?.targets ?? []).map((target) => (
                <tr key={target.productDatabase} className="border-b border-slate-100">
                  <td className="px-3 py-2 font-mono text-xs text-stl-navy">
                    {target.productDatabase}
                  </td>
                  <td className="px-3 py-2">
                    <StatusPill status={target.status} />
                  </td>
                  <td className="px-3 py-2">{target.truncateTableCount}</td>
                  <td className="px-3 py-2">{target.preserveTableCount}</td>
                  <td className="px-3 py-2">
                    {numberFormatter.format(target.estimatedRowsToDelete)}
                  </td>
                  <td className="px-3 py-2 text-xs text-slate-600">
                    {target.errorMessage ?? previewNote(target)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </PlatformAdminSection>

      <div className="grid gap-4 xl:grid-cols-2">
        <TableDispositionPanel title="Tables to wipe" targets={preview?.targets ?? []} kind="wipe" />
        <TableDispositionPanel
          title="Tables preserved"
          targets={preview?.targets ?? []}
          kind="preserve"
        />
      </div>

      <PlatformAdminSection
        title="Execute"
        description="This action clears configured product data tables and cannot be undone from NexArr."
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          {preview?.isEnabled && summary.readyTargets > 0 && summary.truncateTableCount === 0 ? (
            <div className="flex items-start gap-3 rounded-lg border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">
              <ShieldCheck className="mt-0.5 h-5 w-5 shrink-0" aria-hidden />
              <p>
                The preview did not find any product tables to truncate, so the nuke action is
                disabled until the database layout changes.
              </p>
            </div>
          ) : null}

          <div className="flex items-start gap-3 rounded-lg border border-rose-200 bg-rose-50 p-4 text-sm text-rose-800">
            <AlertTriangle className="mt-0.5 h-5 w-5 shrink-0" aria-hidden />
            <p>
              The run will reset identities on truncated tables. Take a backup before executing in
              a shared or production environment.
            </p>
          </div>

          <label className="block text-sm font-medium text-slate-700">
            Reason
            <textarea
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              rows={3}
              className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-800 shadow-sm focus:border-stl-teal focus:outline-none focus:ring-2 focus:ring-stl-teal/20"
              placeholder="Resetting seeded demo data after validation"
            />
          </label>

          <label className="block text-sm font-medium text-slate-700">
            Confirmation phrase
            <input
              value={confirmationPhrase}
              onChange={(event) => setConfirmationPhrase(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 font-mono text-sm text-slate-800 shadow-sm focus:border-stl-teal focus:outline-none focus:ring-2 focus:ring-stl-teal/20"
              placeholder={preview?.confirmationPhrase}
            />
          </label>

          {executeMutation.isError ? (
            <ApiErrorCallout
              message={getErrorMessage(executeMutation.error, 'Database nuke failed.')}
            />
          ) : null}

          <button
            type="submit"
            disabled={!canExecute}
            className="inline-flex min-h-10 items-center gap-2 rounded-md bg-rose-700 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-rose-800 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
          >
            <Trash2 className="h-4 w-4" aria-hidden />
            {executeMutation.isPending ? 'Running...' : 'Run database nuke'}
          </button>
        </form>
      </PlatformAdminSection>

      <PlatformAdminScopeNote>
        Ownership scope: NexArr coordinates the reset as the platform control plane. Product
        databases remain product-owned; reference datasets, control-plane records, schema
        infrastructure, and audit evidence are preserved.
      </PlatformAdminScopeNote>
    </div>
  )
}

function summarizeTargets(targets: DatabaseNukeTargetPreview[]) {
  return targets.reduce(
    (summary, target) => {
      if (target.status === 'ready') {
        summary.readyTargets += 1
      }
      if (target.status === 'missing_connection') {
        summary.missingTargets += 1
      }
      if (target.status === 'error') {
        summary.errorTargets += 1
      }
      summary.truncateTableCount += target.truncateTableCount
      summary.preserveTableCount += target.preserveTableCount
      summary.estimatedRowsToDelete += target.estimatedRowsToDelete
      summary.estimatedRowsPreserved += target.estimatedRowsPreserved
      return summary
    },
    {
      readyTargets: 0,
      missingTargets: 0,
      errorTargets: 0,
      truncateTableCount: 0,
      preserveTableCount: 0,
      estimatedRowsToDelete: 0,
      estimatedRowsPreserved: 0,
    },
  )
}

function StatusPill({ status }: { status: string }) {
  const classes =
    status === 'ready'
      ? 'border-amber-200 bg-amber-50 text-amber-700'
      : status === 'missing_connection'
        ? 'border-slate-200 bg-slate-50 text-slate-600'
        : 'border-rose-200 bg-rose-50 text-rose-700'
  const label =
    status === 'ready' ? 'Ready' : status === 'missing_connection' ? 'Not configured' : 'Error'
  return (
    <span className={`inline-flex rounded-full border px-2.5 py-0.5 text-xs font-medium ${classes}`}>
      {label}
    </span>
  )
}

function previewNote(target: DatabaseNukeTargetPreview) {
  if (target.status === 'ready') {
    return target.truncateTableCount > 0 ? 'Ready for truncation.' : 'No product data tables found.'
  }
  if (target.status === 'missing_connection') {
    return 'No connection string configured.'
  }
  return target.errorCode ?? 'Preview failed.'
}

function TableDispositionPanel({
  title,
  targets,
  kind,
}: {
  title: string
  targets: DatabaseNukeTargetPreview[]
  kind: 'wipe' | 'preserve'
}) {
  return (
    <section className="rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex items-center gap-2">
        <Database className="h-4 w-4 text-slate-500" aria-hidden />
        <h3 className="text-sm font-semibold text-stl-navy">{title}</h3>
      </div>
      <div className="mt-3 space-y-2">
        {targets.map((target) => {
          const tables = kind === 'wipe' ? target.tablesToTruncate : target.preservedTables
          return (
            <details key={`${kind}-${target.productDatabase}`} className="border-t border-slate-100 pt-2">
              <summary className="cursor-pointer text-sm font-medium text-slate-700">
                {target.productDatabase} ({tables.length})
              </summary>
              {tables.length ? (
                <ul className="mt-2 max-h-52 space-y-1 overflow-auto text-xs text-slate-600">
                  {tables.map((table) => (
                    <li
                      key={`${target.productDatabase}-${table.schema}-${table.table}`}
                      className="grid gap-1 rounded-md bg-slate-50 px-2 py-1 sm:grid-cols-[minmax(0,1fr)_8rem]"
                    >
                      <span className="break-all font-mono">
                        {table.schema}.{table.table}
                      </span>
                      <span className="text-slate-500">
                        {table.reason} · {numberFormatter.format(table.estimatedRows)}
                      </span>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="mt-2 text-xs text-slate-500">None</p>
              )}
            </details>
          )
        })}
      </div>
    </section>
  )
}
