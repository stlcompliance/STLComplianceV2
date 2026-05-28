import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'

import {
  exportPeopleCsv,
  exportPeopleJson,
  exportPeopleZip,
  getOrgUnits,
  getPeopleExportManifest,
} from '../api/client'
import type { PersonExportFilters, PersonExportResponse } from '../api/types'

interface PersonExportPanelProps {
  accessToken: string
  canExport: boolean
}

export function PersonExportPanel({ accessToken, canExport }: PersonExportPanelProps) {
  const [employmentStatus, setEmploymentStatus] = useState('')
  const [orgUnitId, setOrgUnitId] = useState('')
  const [lastJsonExport, setLastJsonExport] = useState<PersonExportResponse | null>(null)

  const manifestQuery = useQuery({
    queryKey: ['staffarr-people-export-manifest', accessToken],
    queryFn: () => getPeopleExportManifest(accessToken),
    enabled: canExport,
  })

  const orgUnitsQuery = useQuery({
    queryKey: ['staffarr-org-units', accessToken],
    queryFn: () => getOrgUnits(accessToken),
    enabled: canExport,
  })

  const filters = {
    employmentStatus: employmentStatus || undefined,
    orgUnitId: orgUnitId || undefined,
  }

  const csvExportMutation = useMutation({
    mutationFn: (exportFilters: PersonExportFilters) => exportPeopleCsv(accessToken, exportFilters),
    onSuccess: (csv) => {
      const blob = new Blob([csv], { type: 'text/csv' })
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `staffarr-people-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const zipExportMutation = useMutation({
    mutationFn: (exportFilters: PersonExportFilters) => exportPeopleZip(accessToken, exportFilters),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `staffarr-people-export-${new Date().toISOString().slice(0, 10)}.zip`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const jsonExportMutation = useMutation({
    mutationFn: (exportFilters: PersonExportFilters) => exportPeopleJson(accessToken, exportFilters),
    onSuccess: (result) => {
      setLastJsonExport(result)
    },
  })

  return (
    <section className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Person export bundle</h2>
        <p className="mt-1 text-sm text-slate-400">
          Export workforce directory CSV compatible with bulk import, plus JSON preview and ZIP bundle.
        </p>
      </header>

      {canExport ? (
        <>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
            <p className="font-mono text-xs text-slate-400">
              {manifestQuery.data?.csvHeader ?? PeopleExportServiceFallbackHeader}
            </p>
          </div>

          <label className="block text-sm text-slate-300">
            Employment status filter (optional)
            <select
              value={employmentStatus}
              onChange={(event) => setEmploymentStatus(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">All statuses</option>
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
              <option value="terminated">Terminated</option>
            </select>
          </label>

          <label className="block text-sm text-slate-300">
            Primary org unit filter (optional)
            <select
              value={orgUnitId}
              onChange={(event) => setOrgUnitId(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">All org units</option>
              {(orgUnitsQuery.data ?? [])
                .filter((unit) => unit.status === 'active')
                .map((unit) => (
                  <option key={unit.orgUnitId} value={unit.orgUnitId}>
                    {unit.unitType} · {unit.name}
                  </option>
                ))}
            </select>
          </label>

          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              disabled={csvExportMutation.isPending}
              onClick={() => csvExportMutation.mutate(filters)}
              className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            >
              Download CSV
            </button>
            <button
              type="button"
              disabled={zipExportMutation.isPending}
              onClick={() => zipExportMutation.mutate(filters)}
              className="rounded-md border border-slate-600 px-4 py-2 text-sm text-slate-100 hover:bg-slate-800 disabled:opacity-50"
            >
              Download ZIP bundle
            </button>
            <button
              type="button"
              disabled={jsonExportMutation.isPending}
              onClick={() => jsonExportMutation.mutate(filters)}
              className="rounded-md border border-slate-600 px-4 py-2 text-sm text-slate-100 hover:bg-slate-800 disabled:opacity-50"
            >
              Preview JSON export
            </button>
          </div>
        </>
      ) : (
        <p className="text-sm text-slate-500">
          Person export requires tenant admin, StaffArr admin, or HR admin role.
        </p>
      )}

      {lastJsonExport ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Exported {lastJsonExport.personCount} people at{' '}
            {new Date(lastJsonExport.generatedAt).toLocaleString()}
          </p>
        </div>
      ) : null}
    </section>
  )
}

const PeopleExportServiceFallbackHeader =
  'givenName,familyName,primaryEmail,employmentStatus,jobTitle,managerEmail,primaryOrgUnitId,personId'
