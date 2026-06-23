import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { Upload } from 'lucide-react'
import { ApiErrorCallout, getErrorMessage, type ProductImportManifest } from '@stl/shared-ui'
import { runGenericCsvImport, type GenericCsvImportResponse } from '../api/client'

interface GenericCsvImportPanelProps {
  accessToken: string
  manifest: ProductImportManifest
  canManage: boolean
  onComplete?: () => void
}

function humanizeMetricKey(value: string): string {
  return value
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/_/g, ' ')
    .replace(/\b\w/g, (character) => character.toUpperCase())
}

export function GenericCsvImportPanel({
  accessToken,
  manifest,
  canManage,
  onComplete,
}: GenericCsvImportPanelProps) {
  const [csv, setCsv] = useState('')
  const [fileName, setFileName] = useState<string | null>(null)
  const [dryRun, setDryRun] = useState(true)
  const [result, setResult] = useState<GenericCsvImportResponse | null>(null)

  const importMutation = useMutation({
    mutationFn: () =>
      runGenericCsvImport(accessToken, manifest.importTypeKey, {
        csv,
        dryRun,
        fileName,
      }),
    onSuccess: (response) => {
      setResult(response)
      if (!response.dryRun && response.succeeded) {
        onComplete?.()
      }
    },
  })

  if (!canManage) {
    return (
      <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4 text-sm text-[var(--color-text-muted)]">
        This import requires import and manage permission for this tenant.
      </div>
    )
  }

  return (
    <section className="space-y-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex gap-3">
          <span className="rounded-xl border border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] p-2 text-[var(--color-accent)]">
            <Upload className="h-4 w-4" aria-hidden />
          </span>
          <div>
            <h3 className="text-base font-semibold text-[var(--color-text-primary)]">
              {manifest.displayName}
            </h3>
            <p className="mt-1 text-sm text-[var(--color-text-muted)]">
              {manifest.description}
            </p>
          </div>
        </div>
        <span className="rounded-full border border-[var(--color-border-subtle)] px-3 py-1 text-xs uppercase tracking-wide text-[var(--color-text-secondary)]">
          {manifest.supportedFileTypes.join(', ')}
        </span>
      </div>

      <div className="grid gap-4 xl:grid-cols-[minmax(0,1.35fr)_minmax(18rem,0.75fr)]">
        <div>
          <label htmlFor="generic-import-csv" className="block text-sm font-medium text-[var(--color-text-primary)]">
            CSV data
          </label>
          <textarea
            id="generic-import-csv"
            className="mt-2 min-h-72 w-full rounded-lg border border-[var(--color-border-default)] bg-[var(--color-field-bg)] px-3 py-2 font-mono text-sm text-[var(--color-text-primary)]"
            placeholder={manifest.requiredColumns.join(',')}
            value={csv}
            onChange={(event) => setCsv(event.target.value)}
            spellCheck={false}
          />
          <p className="mt-2 text-xs text-[var(--color-text-muted)]">
            Required columns: {manifest.requiredColumns.join(', ')}
          </p>
        </div>

        <div className="space-y-4">
          <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
            <label htmlFor="generic-import-file" className="block text-sm font-medium text-[var(--color-text-primary)]">
              Upload CSV file
            </label>
            <input
              id="generic-import-file"
              type="file"
              accept=".csv,text/csv"
              className="mt-2 block w-full text-sm text-[var(--color-text-secondary)] file:mr-3 file:rounded-md file:border-0 file:bg-[var(--color-bg-control-hover)] file:px-3 file:py-2 file:text-[var(--color-text-primary)]"
              onChange={(event) => {
                const file = event.target.files?.[0] ?? null
                setFileName(file?.name ?? null)
                if (!file) {
                  return
                }

                const reader = new FileReader()
                reader.onload = () => {
                  setCsv(typeof reader.result === 'string' ? reader.result : '')
                }
                reader.readAsText(file)
              }}
            />
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">
              Selected file: {fileName ?? 'No file selected'}
            </p>
          </div>

          <label htmlFor="generic-import-dry-run" className="flex items-center gap-2 text-sm text-[var(--color-text-primary)]">
            <input
              id="generic-import-dry-run"
              type="checkbox"
              checked={dryRun}
              onChange={(event) => setDryRun(event.target.checked)}
            />
            Validate only
          </label>

          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-slate-950 transition hover:bg-[var(--color-accent-hover)] disabled:cursor-not-allowed disabled:opacity-60"
              disabled={importMutation.isPending || !csv.trim()}
              onClick={() => importMutation.mutate()}
            >
              {importMutation.isPending ? 'Running import…' : dryRun ? 'Validate import' : 'Commit import'}
            </button>
            <button
              type="button"
              className="rounded-md border border-[var(--color-border-subtle)] px-4 py-2 text-sm font-medium text-[var(--color-text-secondary)] transition hover:border-[var(--color-accent-border)] hover:text-[var(--color-text-primary)]"
              onClick={() => {
                setCsv('')
                setFileName(null)
                setResult(null)
              }}
            >
              Clear
            </button>
          </div>
        </div>
      </div>

      {importMutation.isError ? (
        <ApiErrorCallout
          title="Import failed"
          message={getErrorMessage(importMutation.error, 'Failed to run import.')}
        />
      ) : null}

      {result ? (
        <div className="space-y-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
          <div className="flex flex-wrap gap-2 text-xs">
            <span className="rounded-full border border-[var(--color-border-subtle)] px-3 py-1 text-[var(--color-text-secondary)]">
              Rows read: {result.rowsRead}
            </span>
            <span
              className={`rounded-full border px-3 py-1 ${
                result.succeeded
                  ? 'stl-tone-badge'
                  : 'border-[var(--tone-danger-border)] bg-[var(--tone-danger-bg)] text-[var(--tone-danger-text)]'
              }`}
              data-tone={result.succeeded ? 'success' : 'danger'}
            >
              {result.succeeded ? (result.dryRun ? 'Validation ready' : 'Import complete') : 'Issues found'}
            </span>
            {result.metrics.map((metric) => (
              <span
                key={metric.key}
                className="rounded-full border border-[var(--color-border-subtle)] px-3 py-1 text-[var(--color-text-secondary)]"
              >
                {humanizeMetricKey(metric.key)}: {metric.value}
              </span>
            ))}
          </div>

          {result.issues.length > 0 ? (
            <div>
              <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">
                Validation issues
              </h4>
              <ul className="mt-2 space-y-2 text-sm text-[var(--color-text-secondary)]">
                {result.issues.map((issue, index) => (
                  <li
                    key={`${issue.lineNumber}-${issue.code}-${index}`}
                    className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2"
                  >
                    <span className="font-medium text-[var(--color-text-primary)]">
                      Line {issue.lineNumber}
                    </span>{' '}
                    <span className="text-[var(--color-text-muted)]">({issue.code})</span>
                    <div className="mt-1">{issue.message}</div>
                  </li>
                ))}
              </ul>
            </div>
          ) : (
            <p className="text-sm text-[var(--color-text-muted)]">
              {result.dryRun
                ? 'Validation completed without blocking issues.'
                : 'Import completed without blocking issues.'}
            </p>
          )}
        </div>
      ) : null}
    </section>
  )
}
