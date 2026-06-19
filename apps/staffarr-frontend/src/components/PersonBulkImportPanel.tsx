import { useMutation } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useState } from 'react'

import { importPeopleBulk } from '../api/client'
import type { BulkPersonImportResponse } from '../api/types'

interface PersonBulkImportPanelProps {
  accessToken: string
  canImport: boolean
  onComplete?: () => void
}

const CSV_TEMPLATE = `givenName,familyName,primaryEmail,employmentStatus,jobTitle,managerEmail
Jane,Doe,jane.doe@example.com,active,Technician,
John,Smith,john.smith@example.com,active,Lead,jane.doe@example.com`

function parseCsvImportRows(csvText: string) {
  const lines = csvText
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean)

  if (lines.length < 2) {
    throw new Error('CSV must include a header row and at least one person row.')
  }

  const headers = lines[0].split(',').map((header) => header.trim().toLowerCase())
  const requiredHeaders = ['givenname', 'familyname', 'primaryemail']
  for (const requiredHeader of requiredHeaders) {
    if (!headers.includes(requiredHeader)) {
      throw new Error(`CSV header must include ${requiredHeader}.`)
    }
  }

  return lines.slice(1).map((line) => {
    const values = line.split(',').map((value) => value.trim())
    const row: Record<string, string> = {}
    headers.forEach((header, index) => {
      row[header] = values[index] ?? ''
    })

    return {
      givenName: row.givenname,
      familyName: row.familyname,
      primaryEmail: row.primaryemail,
      employmentStatus: row.employmentstatus || 'active',
      jobTitle: row.jobtitle || null,
      managerEmail: row.manageremail || null,
      primaryOrgUnitId: null,
      managerPersonId: null,
    }
  })
}

export function PersonBulkImportPanel({ accessToken, canImport, onComplete }: PersonBulkImportPanelProps) {
  const [csvText, setCsvText] = useState(CSV_TEMPLATE)
  const [dryRun, setDryRun] = useState(true)
  const [parseError, setParseError] = useState<string | null>(null)
  const [lastResult, setLastResult] = useState<BulkPersonImportResponse | null>(null)

  const importMutation = useMutation({
    mutationFn: async () => {
      const people = parseCsvImportRows(csvText)
      return importPeopleBulk(accessToken, { people, dryRun })
    },
    onSuccess: (result) => {
      setLastResult(result)
      setParseError(null)
      if (!result.dryRun && result.createdCount > 0) {
        onComplete?.()
      }
    },
    onError: (error) => {
      if (error instanceof Error) {
        setParseError(error.message)
      }
    },
  })

  const handleSubmit = () => {
    setParseError(null)
    setLastResult(null)
    try {
      parseCsvImportRows(csvText)
      importMutation.mutate()
    } catch (error) {
      setParseError(error instanceof Error ? error.message : 'Unable to parse CSV.')
    }
  }

  return (
    <section className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Bulk person onboarding import</h2>
        <p className="mt-1 text-sm text-slate-400">
          Import up to 100 people per batch. Place managers before direct reports when using managerEmail.
        </p>
      </header>

      {canImport ? (
        <>
          <label htmlFor="person-bulk-import-csv" className="block text-sm text-slate-300">
            CSV rows
            <textarea
              id="person-bulk-import-csv"
              value={csvText}
              onChange={(event) => setCsvText(event.target.value)}
              rows={8}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs text-slate-100"
            />
          </label>

          <label htmlFor="person-bulk-import-dry-run" className="flex items-center gap-2 text-sm text-slate-300">
            <input
              id="person-bulk-import-dry-run"
              type="checkbox"
              data-testid="person-bulk-import-dry-run"
              checked={dryRun}
              onChange={(event) => setDryRun(event.target.checked)}
            />
            Dry run (validate only)
          </label>

          <button
            type="button"
            disabled={importMutation.isPending}
            onClick={handleSubmit}
            className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            {importMutation.isPending ? 'Importing…' : dryRun ? 'Validate import' : 'Run import'}
          </button>
        </>
      ) : (
        <p className="text-sm text-[var(--color-text-muted)]">
          Bulk import requires tenant admin, StaffArr admin, or HR admin role.
        </p>
      )}

      {parseError ? (
        <ApiErrorCallout title="Bulk import failed" message={parseError} />
      ) : null}
      {importMutation.error instanceof Error ? (
        <ApiErrorCallout
          title="Bulk import failed"
          message={getErrorMessage(importMutation.error, 'Bulk import failed.')}
        />
      ) : null}

      {lastResult ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            {lastResult.dryRun ? 'Validated' : 'Created'} {lastResult.dryRun ? lastResult.validatedCount : lastResult.createdCount} of{' '}
            {lastResult.totalRows} rows ({lastResult.errorCount} errors)
          </p>
          <ul className="mt-3 space-y-1 font-mono text-xs">
            {lastResult.results.map((row) => (
              <li key={`${row.rowIndex}-${row.primaryEmail}`}>
                #{row.rowIndex + 1} {row.primaryEmail} — {row.status}
                {row.message ? `: ${row.message}` : ''}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </section>
  )
}
