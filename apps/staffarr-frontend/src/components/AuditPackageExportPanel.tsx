import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'

import {
  exportAuditPackageJson,
  exportAuditPackageZip,
  getAuditPackageManifest,
  getAuditPackageTimeline,
} from '../api/client'
import type { AuditPackageExportResponse } from '../api/types'

interface AuditPackageExportPanelProps {
  accessToken: string
  canExport: boolean
}

export function AuditPackageExportPanel({ accessToken, canExport }: AuditPackageExportPanelProps) {
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [lastJsonExport, setLastJsonExport] = useState<AuditPackageExportResponse | null>(null)

  const manifestQuery = useQuery({
    queryKey: ['staffarr-audit-package-manifest', accessToken],
    queryFn: () => getAuditPackageManifest(accessToken),
  })

  const timelineQuery = useQuery({
    queryKey: ['staffarr-audit-package-timeline', accessToken, fromDate, toDate],
    queryFn: () =>
      getAuditPackageTimeline(accessToken, {
        from: fromDate || undefined,
        to: toDate || undefined,
        page: 1,
        pageSize: 15,
      }),
  })

  const zipExportMutation = useMutation({
    mutationFn: () =>
      exportAuditPackageZip(accessToken, {
        from: fromDate || undefined,
        to: toDate || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `staffarr-audit-package-${new Date().toISOString().slice(0, 10)}.zip`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const jsonExportMutation = useMutation({
    mutationFn: () =>
      exportAuditPackageJson(accessToken, {
        from: fromDate || undefined,
        to: toDate || undefined,
      }),
    onSuccess: (result) => {
      setLastJsonExport(result)
    },
  })

  return (
    <section className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Audit package export</h2>
        <p className="mt-1 text-sm text-slate-400">
          Export tenant workforce audit events, people, permission history, certifications, incidents,
          readiness overrides, and training blockers for compliance review.
        </p>
      </header>

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Package sections</h3>
        <ul className="mt-2 list-inside list-disc text-sm text-slate-400">
          {(manifestQuery.data?.sections ?? []).map((section) => (
            <li key={section.key}>
              <span className="font-mono text-slate-300">{section.fileName}</span>
              <span className="text-slate-500"> — {section.label}</span>
            </li>
          ))}
        </ul>
      </div>

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Audit timeline preview</h3>
        <p className="mt-1 text-xs text-slate-500">
          Recent tenant audit events matching the selected date range (newest first).
        </p>
        {timelineQuery.isLoading ? (
          <p className="mt-3 text-sm text-slate-500">Loading audit timeline…</p>
        ) : timelineQuery.isError ? (
          <p className="mt-3 text-sm text-rose-400">Failed to load audit timeline.</p>
        ) : timelineQuery.data && timelineQuery.data.items.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No audit events in this range.</p>
        ) : timelineQuery.data ? (
          <>
            <ul className="mt-3 divide-y divide-slate-800 text-sm">
              {timelineQuery.data.items.map((item) => (
                <li key={item.auditEventId} className="py-2">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <span className="font-mono text-sky-300">{item.action}</span>
                    <span className="text-xs text-slate-500">
                      {new Date(item.occurredAt).toLocaleString()}
                    </span>
                  </div>
                  <p className="text-xs text-slate-400">
                    {item.targetType}
                    {item.targetId ? ` · ${item.targetId}` : ''} · {item.result}
                    {item.reasonCode ? ` · ${item.reasonCode}` : ''}
                  </p>
                </li>
              ))}
            </ul>
            <p className="mt-2 text-xs text-slate-500">
              Showing {timelineQuery.data.items.length} of {timelineQuery.data.totalCount} events.
            </p>
          </>
        ) : null}
      </div>

      <div className="grid gap-3 sm:grid-cols-2">
        <label className="block text-sm text-slate-300">
          From (optional)
          <input
            type="date"
            value={fromDate}
            onChange={(event) => setFromDate(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
        <label className="block text-sm text-slate-300">
          To (optional)
          <input
            type="date"
            value={toDate}
            onChange={(event) => setToDate(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
      </div>

      {canExport ? (
        <div className="flex flex-wrap gap-3">
          <button
            type="button"
            onClick={() => zipExportMutation.mutate()}
            disabled={zipExportMutation.isPending}
            className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            {zipExportMutation.isPending ? 'Exporting…' : 'Download ZIP package'}
          </button>
          <button
            type="button"
            onClick={() => jsonExportMutation.mutate()}
            disabled={jsonExportMutation.isPending}
            className="rounded-md bg-slate-700 px-4 py-2 text-sm font-medium text-slate-100 hover:bg-slate-600 disabled:opacity-50"
          >
            {jsonExportMutation.isPending ? 'Loading…' : 'Preview JSON export'}
          </button>
        </div>
      ) : (
        <p className="text-sm text-slate-500">
          Audit package export requires tenant admin, StaffArr admin, or HR admin role.
        </p>
      )}

      {lastJsonExport ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Package <span className="font-mono text-sky-300">{lastJsonExport.packageId}</span> generated at{' '}
            {new Date(lastJsonExport.generatedAt).toLocaleString()}.
          </p>
          <p className="mt-2">
            Counts: {lastJsonExport.counts.auditEvents} audit events, {lastJsonExport.counts.people} people,{' '}
            {lastJsonExport.counts.permissionHistory} permission events,{' '}
            {lastJsonExport.counts.personCertifications} certifications,{' '}
            {lastJsonExport.counts.personnelIncidents} incidents,{' '}
            {lastJsonExport.counts.readinessOverrides} readiness overrides,{' '}
            {lastJsonExport.counts.trainingBlockers} training blockers.
          </p>
        </div>
      ) : null}
    </section>
  )
}
