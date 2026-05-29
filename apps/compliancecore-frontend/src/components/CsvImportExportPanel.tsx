import { useMutation, useQuery } from '@tanstack/react-query'
import { useRef, useState } from 'react'

import { exportCsvBundleZip, getCsvBundleManifest, importCsvBundle } from '../api/client'
import type { CsvImportResultResponse } from '../api/types'

interface CsvImportExportPanelProps {
  accessToken: string
  canManage: boolean
}

export function CsvImportExportPanel({ accessToken, canManage }: CsvImportExportPanelProps) {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [dryRun, setDryRun] = useState(true)
  const [lastResult, setLastResult] = useState<CsvImportResultResponse | null>(null)

  const manifestQuery = useQuery({
    queryKey: ['compliancecore-csv-manifest', accessToken],
    queryFn: () => getCsvBundleManifest(accessToken),
  })

  const exportMutation = useMutation({
    mutationFn: () => exportCsvBundleZip(accessToken),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-csv-bundle-${new Date().toISOString().slice(0, 10)}.zip`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const importMutation = useMutation({
    mutationFn: (files: FileList) => importCsvBundle(accessToken, files, dryRun),
    onSuccess: (result) => {
      setLastResult(result)
    },
  })

  return (
    <section
      data-testid="csv-import-export-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">9-CSV import / export</h2>
        <p className="mt-1 text-sm text-slate-400">
          Bundle covers controlled vocabulary, keys, rule packs, citations, fact requirements, mappings, and SDS
          references per Compliance Core featureset.
        </p>
      </header>

      <div
        className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
        data-testid="csv-import-export-manifest"
      >
        <h3 className="text-sm font-medium text-slate-200">Bundle files</h3>
        <ul className="mt-2 list-inside list-disc text-sm text-slate-400">
          {(manifestQuery.data?.files ?? []).map((file) => (
            <li key={file.fileName}>
              <span className="font-mono text-slate-300">{file.fileName}</span>
              <span className="text-slate-500"> — {file.headers.join(', ')}</span>
            </li>
          ))}
        </ul>
      </div>

      <div className="flex flex-wrap gap-3">
        <button
          type="button"
          onClick={() => exportMutation.mutate()}
          disabled={exportMutation.isPending}
          data-testid="csv-import-export-download"
          className="rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
        >
          {exportMutation.isPending ? 'Exporting…' : 'Download ZIP export'}
        </button>
      </div>

      {canManage ? (
        <div className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <h3 className="text-sm font-medium text-slate-200">Import</h3>
          <label htmlFor="csv-import-export-dry-run" className="flex items-center gap-2 text-sm text-slate-300">
            <input
              id="csv-import-export-dry-run"
              type="checkbox"
              checked={dryRun}
              onChange={(event) => setDryRun(event.target.checked)}
              className="rounded border-slate-600"
            />
            Dry run (validate only)
          </label>
          <label htmlFor="csv-import-export-files" className="block text-sm text-slate-300">
            CSV or ZIP bundle files
            <input
              id="csv-import-export-files"
              ref={fileInputRef}
              type="file"
              accept=".csv,.zip"
              multiple
              className="mt-1 block w-full text-sm text-slate-300 file:mr-3 file:rounded-md file:border-0 file:bg-slate-700 file:px-3 file:py-1.5 file:text-sm file:text-slate-100"
            />
          </label>
          <button
            type="button"
            onClick={() => {
              const files = fileInputRef.current?.files
              if (!files?.length) {
                return
              }
              importMutation.mutate(files)
            }}
            disabled={importMutation.isPending}
            className="rounded-md bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
          >
            {importMutation.isPending ? 'Importing…' : dryRun ? 'Validate import' : 'Apply import'}
          </button>
        </div>
      ) : (
        <p className="text-sm text-slate-500">CSV import requires compliance admin or tenant admin role.</p>
      )}

      {lastResult ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            {lastResult.dryRun ? 'Dry run' : lastResult.applied ? 'Import applied' : 'Import not applied'}
            {lastResult.issues.length > 0 ? ` — ${lastResult.issues.length} issue(s)` : ' — no issues'}
          </p>
          {lastResult.files.length > 0 ? (
            <ul className="mt-2 space-y-1">
              {lastResult.files.map((file) => (
                <li key={file.fileName}>
                  {file.fileName}: {file.rowCount} rows ({file.created} created, {file.updated} updated)
                </li>
              ))}
            </ul>
          ) : null}
          {lastResult.issues.length > 0 ? (
            <ul className="mt-2 space-y-1 text-amber-200">
              {lastResult.issues.map((issue, index) => (
                <li key={`${issue.fileName}-${issue.lineNumber}-${index}`}>
                  {issue.fileName}
                  {issue.lineNumber ? `:${issue.lineNumber}` : ''} — {issue.message}
                </li>
              ))}
            </ul>
          ) : null}
        </div>
      ) : null}

      {importMutation.isError ? (
        <p className="text-sm text-red-300">
          {importMutation.error instanceof Error ? importMutation.error.message : 'Import failed'}
        </p>
      ) : null}
      {exportMutation.isError ? (
        <p className="text-sm text-red-300">
          {exportMutation.error instanceof Error ? exportMutation.error.message : 'Export failed'}
        </p>
      ) : null}
    </section>
  )
}
