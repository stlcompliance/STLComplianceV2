import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getMaintenanceParts, getSessionBootstrap } from '../api/client'
import { canCreateParts, loadSession } from '../auth/sessionStorage'
import type { MaintenancePartResponse } from '../api/types'

function statusTone(status: string): string {
  switch (status) {
    case 'active':
      return 'bg-emerald-500/15 text-emerald-300 ring-1 ring-inset ring-emerald-400/30'
    case 'draft':
      return 'bg-sky-500/15 text-sky-300 ring-1 ring-inset ring-sky-400/30'
    case 'discontinued':
      return 'bg-rose-500/15 text-rose-300 ring-1 ring-inset ring-rose-400/30'
    default:
      return 'bg-slate-700/70 text-slate-200 ring-1 ring-inset ring-slate-500/50'
  }
}

function renderComplianceSummary(part: MaintenancePartResponse): string {
  const segments: string[] = []
  if (part.sdsDocumentId) {
    segments.push(`SDS ${part.sdsDocumentId}`)
  }
  if (part.complianceCoreMaterialKey) {
    segments.push(`Material ${part.complianceCoreMaterialKey}`)
  }
  if (part.complianceCoreHazardKeys.length > 0) {
    segments.push(`${part.complianceCoreHazardKeys.length} hazard key${part.complianceCoreHazardKeys.length === 1 ? '' : 's'}`)
  }
  return segments.length > 0 ? segments.join(' • ') : 'No compliance snapshot linked'
}

export function MaintenancePartsPanel() {
  const session = loadSession()
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('all')

  const bootstrapQuery = useQuery({
    queryKey: ['maintainarr-session-bootstrap', session?.accessToken],
    enabled: !!session,
    queryFn: () => getSessionBootstrap(session!.accessToken),
  })

  const partsQuery = useQuery({
    queryKey: ['maintainarr-parts', session?.accessToken, search, status],
    enabled: !!session,
    queryFn: () =>
      getMaintenanceParts(session!.accessToken, {
        search: search.trim() || undefined,
        status: status === 'all' ? undefined : status,
      }),
  })

  const canCreate = bootstrapQuery.data
    ? canCreateParts(bootstrapQuery.data.tenantRoleKey, bootstrapQuery.data.isPlatformAdmin)
    : false

  const parts = useMemo(() => partsQuery.data ?? [], [partsQuery.data])

  if (!session) {
    return <p className="text-sm text-slate-400">Launch MaintainArr again to load maintenance parts.</p>
  }

  return (
    <section className="space-y-6" data-testid="maintenance-parts-panel">
      <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6 shadow-lg shadow-slate-950/30">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div className="space-y-3">
            <div>
              <h2 className="text-xl font-semibold text-white">Maintenance part profiles</h2>
              <p className="mt-2 max-w-3xl text-sm text-slate-300">
                Use maintenance part profiles for work orders, approvals, and service planning.
              </p>
            </div>
            <div className="rounded-xl border border-sky-500/20 bg-sky-500/10 p-4 text-sm text-sky-100">
              Profiles here may include an external part reference when you need to match vendor data.
            </div>
          </div>
          {canCreate ? (
            <Link
              to="/parts/create"
              className="inline-flex items-center justify-center rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500"
            >
              Create part profile
            </Link>
          ) : null}
        </div>
      </div>

      <div className="grid gap-4 rounded-2xl border border-slate-800 bg-slate-950/70 p-6 md:grid-cols-[minmax(0,1fr)_220px]">
        <label className="space-y-2 text-sm text-slate-200">
          <span>Search parts</span>
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Part number, display name, or manufacturer part number"
            className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-white outline-none ring-0 placeholder:text-[var(--color-text-muted)] focus:border-sky-500"
          />
        </label>
        <label className="space-y-2 text-sm text-slate-200">
          <span>Status</span>
          <select
            value={status}
            onChange={(event) => setStatus(event.target.value)}
            className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-white outline-none focus:border-sky-500"
          >
            <option value="all">All statuses</option>
            <option value="active">Active</option>
            <option value="draft">Draft</option>
            <option value="inactive">Inactive</option>
            <option value="discontinued">Discontinued</option>
          </select>
        </label>
      </div>

      <div className="space-y-4">
        {partsQuery.isLoading ? (
          <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6 text-sm text-slate-400">
            Loading maintenance part profiles…
          </div>
        ) : null}

        {partsQuery.isError ? (
          <div className="rounded-2xl border border-rose-500/30 bg-rose-500/10 p-6 text-sm text-rose-200">
            {partsQuery.error instanceof Error
              ? partsQuery.error.message
              : 'Failed to load maintenance part profiles.'}
          </div>
        ) : null}

        {!partsQuery.isLoading && !partsQuery.isError && parts.length === 0 ? (
          <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6 text-sm text-slate-300">
            No maintenance part profiles matched the current filters.
          </div>
        ) : null}

        {parts.map((part) => (
          <Link
            key={part.partId}
            to={`/parts/${part.partId}`}
            className="block rounded-2xl border border-slate-800 bg-slate-950/70 p-6 transition hover:border-sky-500/40 hover:bg-slate-950"
          >
            <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
              <div className="space-y-3">
                <div className="flex flex-wrap items-center gap-3">
                  <h3 className="text-lg font-semibold text-white">{part.displayName}</h3>
                  <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${statusTone(part.status)}`}>
                    {part.status}
                  </span>
                  <span className="inline-flex rounded-full bg-slate-800 px-2.5 py-1 text-xs font-medium text-slate-200">
                    {part.sourceType === 'supplyarr_snapshot' ? 'SupplyArr snapshot' : 'MaintainArr profile'}
                  </span>
                </div>
                <div className="flex flex-wrap gap-x-6 gap-y-2 text-sm text-slate-300">
                  <span className="font-mono text-sky-300">{part.partNumber}</span>
                  <span>{part.categoryKey}</span>
                  <span>{part.unitOfMeasure}</span>
                  {part.manufacturerPartNumber ? <span>MFG {part.manufacturerPartNumber}</span> : null}
                </div>
                <p className="text-sm text-slate-400">
                  {part.description || 'No description captured yet.'}
                </p>
              </div>
              <div className="grid gap-2 text-sm text-slate-300 lg:min-w-[320px]">
                <div>
                  <p className="text-xs uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Source of truth</p>
                  <p className="mt-1">{part.sourceLabel}</p>
                </div>
                <div>
                  <p className="text-xs uppercase tracking-[0.18em] text-[var(--color-text-muted)]">SupplyArr reference</p>
                  <p className="mt-1 font-mono text-slate-200">
                    {part.supplyArrPartId ?? 'No linked SupplyArr part ID'}
                  </p>
                </div>
                <div>
                  <p className="text-xs uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Compliance snapshot</p>
                  <p className="mt-1">{renderComplianceSummary(part)}</p>
                </div>
              </div>
            </div>
          </Link>
        ))}
      </div>
    </section>
  )
}
