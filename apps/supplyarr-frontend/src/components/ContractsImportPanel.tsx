import { useMutation } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { Upload } from 'lucide-react'

import { importContractsCsv } from '../api/client'
import type { ContractsCsvImportResponse } from '../api/types'

interface ContractsImportPanelProps {
  accessToken: string
  canManage: boolean
}

const TEMPLATE = `vendor_party_key,contract_key,contract_type,title,effective_at,expires_at,renewal_at,payment_terms,freight_terms,warranty_terms,minimum_spend,service_level_agreement,approval_status,status,notes
SUP-2048,SC-2048,master_supply_agreement,Supply Agreement 2026,2026-01-15T00:00:00Z,2026-12-31T00:00:00Z,2026-11-01T00:00:00Z,Net 30,FOB destination,12 months from receipt,25000,95% on-time shipment rate,approved,active,Priority partner contract`

function formatHeaderValue(value: number): string {
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: 0 }).format(value)
}

export function ContractsImportPanel({ accessToken, canManage }: ContractsImportPanelProps) {
  const [csv, setCsv] = useState('')
  const [fileName, setFileName] = useState<string | null>(null)
  const [dryRun, setDryRun] = useState(true)
  const [result, setResult] = useState<ContractsCsvImportResponse | null>(null)

  const importMutation = useMutation({
    mutationFn: () =>
      importContractsCsv(accessToken, {
        csv,
        dryRun,
        fileName,
      }),
    onSuccess: (response) => {
      setResult(response)
    },
  })

  const fileSummary = useMemo(() => {
    if (!fileName) return 'No file selected'
    return fileName
  }, [fileName])

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="supplyarr-contract-import-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex gap-3">
          <Upload className="mt-0.5 h-5 w-5 text-sky-400" aria-hidden />
          <div>
            <h2 className="text-lg font-semibold text-slate-50">Contract import</h2>
            <p className="mt-1 text-sm text-slate-400">
              Paste or upload a contracts CSV, validate it, and import vendor agreements with dry-run support.
            </p>
          </div>
        </div>
        <span className="rounded-md bg-slate-800 px-3 py-1.5 text-xs text-slate-300">
          Template rows: {formatHeaderValue(15)}
        </span>
      </div>

      <div className="mt-4 grid gap-4 lg:grid-cols-[1.4fr_0.6fr]">
        <div>
          <label htmlFor="contracts-import-csv" className="block text-sm font-medium text-slate-200">
            CSV data
          </label>
          <textarea
            id="contracts-import-csv"
            className="mt-2 min-h-64 w-full rounded-md border border-slate-700 bg-slate-950/60 px-3 py-2 font-mono text-sm text-slate-100 placeholder:text-[var(--color-text-muted)]"
            placeholder={TEMPLATE}
            value={csv}
            onChange={(event) => setCsv(event.target.value)}
          />
          <p className="mt-2 text-xs text-[var(--color-text-muted)]">
            Required headers: vendor_party_key, contract_key, contract_type, title, effective_at, expires_at, renewal_at,
            payment_terms, freight_terms, warranty_terms, minimum_spend, service_level_agreement, approval_status,
            status, notes.
          </p>
        </div>

        <div className="space-y-4">
          <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-4">
            <label htmlFor="contracts-import-file" className="block text-sm font-medium text-slate-200">
              Upload CSV file
            </label>
            <input
              id="contracts-import-file"
              type="file"
              accept=".csv,text/csv"
              className="mt-2 block w-full text-sm text-slate-300 file:mr-3 file:rounded-md file:border-0 file:bg-slate-700 file:px-3 file:py-2 file:text-slate-100 hover:file:bg-slate-600"
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
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">Selected file: {fileSummary}</p>
          </div>

          <label htmlFor="contracts-import-dry-run" className="flex items-center gap-2 text-sm text-slate-200">
            <input
              id="contracts-import-dry-run"
              type="checkbox"
              checked={dryRun}
              onChange={(event) => setDryRun(event.target.checked)}
            />
            Validate only (dry run)
          </label>

          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              className="rounded-md bg-sky-700 px-4 py-2 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
              disabled={importMutation.isPending || !csv.trim()}
              onClick={() => importMutation.mutate()}
            >
              {importMutation.isPending ? 'Importing…' : dryRun ? 'Run validation' : 'Import contracts'}
            </button>
            <button
              type="button"
              className="rounded-md border border-slate-700 px-4 py-2 text-sm font-medium text-slate-200 hover:border-slate-500"
              onClick={() => {
                setCsv(TEMPLATE)
                setFileName('supplyarr-contracts-template.csv')
                setResult(null)
              }}
            >
              Load template
            </button>
          </div>
        </div>
      </div>

      {importMutation.isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Contract import failed"
            message={getErrorMessage(importMutation.error, 'Failed to import contracts CSV.')}
          />
        </div>
      ) : null}

      {result ? (
        <div className="mt-4 space-y-4 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <div className="flex flex-wrap gap-2 text-xs">
            <span className="rounded-md bg-slate-800 px-2 py-1 text-slate-300">
              Rows read: {result.rowsRead}
            </span>
            <span className="rounded-md bg-slate-800 px-2 py-1 text-slate-300">
              Accepted: {result.contractsAccepted}
            </span>
            <span className="rounded-md bg-slate-800 px-2 py-1 text-slate-300">
              Created: {result.contractsCreated}
            </span>
            <span className={`rounded-md px-2 py-1 ${result.succeeded ? 'bg-emerald-950/60 text-emerald-300' : 'bg-rose-950/60 text-rose-300'}`}>
              {result.succeeded ? 'Import ready' : 'Import issues found'}
            </span>
          </div>

          {result.issues.length > 0 ? (
            <div>
              <h3 className="text-sm font-semibold text-slate-100">Validation issues</h3>
              <ul className="mt-2 space-y-2 text-sm text-slate-300">
                {result.issues.map((issue, index) => (
                  <li key={`${issue.lineNumber}-${issue.code}-${index}`} className="rounded-md border border-slate-800 px-3 py-2">
                    <span className="font-medium text-slate-100">Line {issue.lineNumber}</span>
                    {' '}
                    <span className="text-[var(--color-text-muted)]">({issue.code})</span>
                    <div className="mt-1">{issue.message}</div>
                  </li>
                ))}
              </ul>
            </div>
          ) : (
            <p className="text-sm text-slate-400">
              {result.dryRun
                ? 'Dry run completed without validation issues.'
                : 'Contracts imported successfully.'}
            </p>
          )}
        </div>
      ) : null}
    </section>
  )
}
