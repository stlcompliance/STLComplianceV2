import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { commitAssetImport, validateAssetImport } from '../api/client'
import type { AssetBulkImportResponse, AssetImportRowRequest } from '../api/types'

interface AssetBulkImportPanelProps {
  accessToken: string
  canImport: boolean
  onComplete?: () => void
}

const CSV_TEMPLATE = `assetClassKey,assetTypeKey,assetTag,name,description,siteRef,lifecycleStatus
vehicles,forklift,FLT-101,Forklift 101,Main shop forklift,yard-a,active
vehicles,forklift,FLT-102,Forklift 102,Backup unit,yard-b,active`

function parseCsvImportRows(csvText: string): AssetImportRowRequest[] {
  const lines = csvText
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean)

  if (lines.length < 2) {
    throw new Error('CSV must include a header row and at least one asset row.')
  }

  const headers = lines[0].split(',').map((header) => header.trim().toLowerCase())
  const required = ['assetclasskey', 'assettypekey', 'assettag', 'name']
  for (const key of required) {
    if (!headers.includes(key)) {
      throw new Error(`CSV header must include ${key}.`)
    }
  }

  return lines.slice(1).map((line) => {
    const values = line.split(',').map((value) => value.trim())
    const row: Record<string, string> = {}
    headers.forEach((header, index) => {
      row[header] = values[index] ?? ''
    })

    return {
      assetClassKey: row.assetclasskey,
      assetTypeKey: row.assettypekey,
      assetTag: row.assettag,
      name: row.name,
      description: row.description ?? '',
      siteRef: row.siteref || null,
      lifecycleStatus: row.lifecyclestatus || 'active',
    }
  })
}

export function AssetBulkImportPanel({ accessToken, canImport, onComplete }: AssetBulkImportPanelProps) {
  const [csvText, setCsvText] = useState(CSV_TEMPLATE)
  const [parseError, setParseError] = useState<string | null>(null)
  const [lastResult, setLastResult] = useState<AssetBulkImportResponse | null>(null)

  const validateMutation = useMutation({
    mutationFn: async () => {
      const assets = parseCsvImportRows(csvText)
      return validateAssetImport(accessToken, { assets })
    },
    onSuccess: (result) => {
      setLastResult(result)
      setParseError(null)
    },
    onError: (error) => {
      setParseError(error instanceof Error ? error.message : 'Validation failed.')
    },
  })

  const commitMutation = useMutation({
    mutationFn: async () => {
      const assets = parseCsvImportRows(csvText)
      return commitAssetImport(accessToken, { assets })
    },
    onSuccess: (result) => {
      setLastResult(result)
      setParseError(null)
      if (result.successCount > 0) {
        onComplete?.()
      }
    },
    onError: (error) => {
      setParseError(error instanceof Error ? error.message : 'Import commit failed.')
    },
  })

  const runValidate = () => {
    setParseError(null)
    setLastResult(null)
    try {
      parseCsvImportRows(csvText)
      validateMutation.mutate()
    } catch (error) {
      setParseError(error instanceof Error ? error.message : 'Unable to parse CSV.')
    }
  }

  const runCommit = () => {
    setParseError(null)
    setLastResult(null)
    try {
      parseCsvImportRows(csvText)
      commitMutation.mutate()
    } catch (error) {
      setParseError(error instanceof Error ? error.message : 'Unable to parse CSV.')
    }
  }

  const isPending = validateMutation.isPending || commitMutation.isPending

  return (
    <section
      className="rounded-xl border border-amber-800/40 bg-amber-950/20 p-5"
      data-testid="asset-bulk-import-panel"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Bulk asset import</h2>
        <p className="mt-1 text-sm text-slate-400">
          Import up to 100 assets per batch. Asset class and type keys must already exist and be active.
          Validate first, then commit.
        </p>
      </header>

      {canImport ? (
        <>
          <label className="mt-4 block text-sm text-slate-300" htmlFor="asset-bulk-import-csv">
            Asset import CSV
            <textarea
              id="asset-bulk-import-csv"
              className="mt-1 h-40 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-sm text-slate-100"
              value={csvText}
              onChange={(event) => setCsvText(event.target.value)}
              spellCheck={false}
            />
          </label>

          <div className="mt-3 flex flex-wrap gap-2">
            <button
              type="button"
              className="rounded-md bg-slate-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-slate-600 disabled:opacity-50"
              disabled={isPending}
              onClick={runValidate}
            >
              {validateMutation.isPending ? 'Validating…' : 'Validate'}
            </button>
            <button
              type="button"
              className="rounded-md bg-amber-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-amber-600 disabled:opacity-50"
              disabled={isPending}
              onClick={runCommit}
            >
              {commitMutation.isPending ? 'Committing…' : 'Commit import'}
            </button>
          </div>

          {parseError ? <p className="mt-3 text-sm text-rose-400">{parseError}</p> : null}

          {lastResult ? (
            <div className="mt-4 rounded-lg border border-slate-700 bg-slate-950/60 p-3 text-sm text-slate-300">
              <p>
                Batch {lastResult.importBatchId} · {lastResult.phase} · {lastResult.successCount} ok /{' '}
                {lastResult.errorCount} errors / {lastResult.totalRows} rows
              </p>
              <ul className="mt-2 max-h-40 space-y-1 overflow-y-auto">
                {lastResult.results.map((row) => (
                  <li key={row.rowIndex}>
                    {row.assetTag}: {row.status}
                    {row.message ? ` — ${row.message}` : ''}
                  </li>
                ))}
              </ul>
            </div>
          ) : null}
        </>
      ) : (
        <p className="mt-3 text-sm text-slate-500">
          Bulk import requires tenant admin, MaintainArr admin, or manager role.
        </p>
      )}
    </section>
  )
}
