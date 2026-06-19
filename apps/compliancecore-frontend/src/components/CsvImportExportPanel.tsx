import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useRef, useState } from 'react'

import { exportCsvBundleZip, getCsvBundleManifest, getRegulatoryPrograms, importCsvBundle } from '../api/client'
import type { CsvImportResultResponse } from '../api/types'

interface CsvImportExportPanelProps {
  accessToken: string
  canManage: boolean
}

export function CsvImportExportPanel({ accessToken, canManage }: CsvImportExportPanelProps) {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [dryRun, setDryRun] = useState(true)
  const [lastResult, setLastResult] = useState<CsvImportResultResponse | null>(null)
  const [regulatorySpineMode, setRegulatorySpineMode] = useState('strict')
  const [governingBodyKey, setGoverningBodyKey] = useState('')
  const [governingBodyLabel, setGoverningBodyLabel] = useState('')
  const [governingBodyDescription, setGoverningBodyDescription] = useState('')
  const [jurisdictionKey, setJurisdictionKey] = useState('')
  const [jurisdictionLabel, setJurisdictionLabel] = useState('')
  const [jurisdictionDescription, setJurisdictionDescription] = useState('')
  const [mappingSourceKey, setMappingSourceKey] = useState('')
  const [mappingTargetKey, setMappingTargetKey] = useState('')
  const [programMappings, setProgramMappings] = useState<Array<{ sourceKey: string; targetKey: string }>>([])

  const manifestQuery = useQuery({
    queryKey: ['compliancecore-csv-manifest', accessToken],
    queryFn: () => getCsvBundleManifest(accessToken),
  })

  const programsQuery = useQuery({
    queryKey: ['compliancecore-regulatory-programs-import', accessToken],
    queryFn: () => getRegulatoryPrograms(accessToken),
    enabled: canManage,
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
    mutationFn: (files: FileList) =>
      importCsvBundle(accessToken, files, dryRun, {
        regulatorySpineMode,
        governingBodyKey,
        governingBodyLabel,
        governingBodyDescription,
        jurisdictionKey,
        jurisdictionLabel,
        jurisdictionDescription,
        programMappings: Object.fromEntries(
          programMappings
            .map((mapping) => [mapping.sourceKey.trim(), mapping.targetKey.trim()])
            .filter(([sourceKey, targetKey]) => sourceKey && targetKey),
        ),
      }),
    onSuccess: (result) => {
      setLastResult(result)
    },
  })

  const showCreateFields = regulatorySpineMode === 'create_missing' || regulatorySpineMode === 'create_or_map'
  const existingPrograms = programsQuery.data ?? []

  return (
    <section
      data-testid="csv-import-export-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">10-CSV import / export</h2>
        <p className="mt-1 text-sm text-slate-400">
          Bundle covers controlled vocabulary, keys, rule packs, fact requirements, mappings, SDS references, and
          legal exception/exemption records per Compliance Core featureset.
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
              <span className="text-[var(--color-text-muted)]"> — {file.headers.join(', ')}</span>
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
          <label htmlFor="csv-import-resolution-mode" className="block text-sm text-slate-300">
            Registry resolution
            <select
              id="csv-import-resolution-mode"
              value={regulatorySpineMode}
              onChange={(event) => setRegulatorySpineMode(event.target.value)}
              className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
            >
              <option value="strict">Strict match</option>
              <option value="create_missing">Create missing</option>
              <option value="map_existing">Map existing</option>
              <option value="create_or_map">Create or map</option>
            </select>
          </label>
          {showCreateFields ? (
            <div className="grid gap-3 md:grid-cols-2">
              <TextInput label="Governing body key" value={governingBodyKey} onChange={setGoverningBodyKey} />
              <TextInput label="Governing body label" value={governingBodyLabel} onChange={setGoverningBodyLabel} />
              <TextInput
                label="Governing body description"
                value={governingBodyDescription}
                onChange={setGoverningBodyDescription}
              />
              <TextInput label="Jurisdiction key" value={jurisdictionKey} onChange={setJurisdictionKey} />
              <TextInput label="Jurisdiction label" value={jurisdictionLabel} onChange={setJurisdictionLabel} />
              <TextInput
                label="Jurisdiction description"
                value={jurisdictionDescription}
                onChange={setJurisdictionDescription}
              />
            </div>
          ) : null}
          <div className="space-y-2 rounded-md border border-slate-800 p-3">
            <div className="grid gap-2 md:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_auto]">
              <TextInput label="Imported program key" value={mappingSourceKey} onChange={setMappingSourceKey} />
              <label className="block text-sm text-slate-300">
                Existing program
                <select
                  value={mappingTargetKey}
                  onChange={(event) => setMappingTargetKey(event.target.value)}
                  className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
                >
                  <option value="">Select program</option>
                  {existingPrograms.map((program) => (
                    <option key={program.regulatoryProgramId} value={program.programKey}>
                      {program.programKey}
                    </option>
                  ))}
                </select>
              </label>
              <button
                type="button"
                onClick={() => {
                  if (!mappingSourceKey.trim() || !mappingTargetKey.trim()) {
                    return
                  }
                  setProgramMappings((current) => [
                    ...current.filter((mapping) => mapping.sourceKey !== mappingSourceKey.trim()),
                    { sourceKey: mappingSourceKey.trim(), targetKey: mappingTargetKey.trim() },
                  ])
                  setMappingSourceKey('')
                  setMappingTargetKey('')
                }}
                className="self-end rounded-md bg-slate-700 px-3 py-2 text-sm font-medium text-white hover:bg-slate-600"
              >
                Add mapping
              </button>
            </div>
            {programMappings.length > 0 ? (
              <ul className="space-y-1 text-xs text-slate-300">
                {programMappings.map((mapping) => (
                  <li key={mapping.sourceKey} className="flex items-center justify-between gap-2 rounded bg-slate-900 px-2 py-1">
                    <span>
                      <span className="font-mono text-slate-100">{mapping.sourceKey}</span>
                      <span className="text-[var(--color-text-muted)]"> -&gt; </span>
                      <span className="font-mono text-emerald-200">{mapping.targetKey}</span>
                    </span>
                    <button
                      type="button"
                      onClick={() =>
                        setProgramMappings((current) => current.filter((item) => item.sourceKey !== mapping.sourceKey))
                      }
                      className="text-slate-400 hover:text-slate-100"
                    >
                      Remove
                    </button>
                  </li>
                ))}
              </ul>
            ) : null}
          </div>
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
        <p className="text-sm text-[var(--color-text-muted)]">CSV import requires compliance admin or tenant admin role.</p>
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
        <ApiErrorCallout title="Import failed" message={getErrorMessage(importMutation.error, 'Import failed.')} />
      ) : null}
      {exportMutation.isError ? (
        <ApiErrorCallout title="Export failed" message={getErrorMessage(exportMutation.error, 'Export failed.')} />
      ) : null}
    </section>
  )
}

function TextInput({
  label,
  value,
  onChange,
}: {
  label: string
  value: string
  onChange: (value: string) => void
}) {
  const id = `csv-import-${label.toLowerCase().replaceAll(' ', '-')}`
  return (
    <label htmlFor={id} className="block text-sm text-slate-300">
      {label}
      <input
        id={id}
        type="text"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
      />
    </label>
  )
}
