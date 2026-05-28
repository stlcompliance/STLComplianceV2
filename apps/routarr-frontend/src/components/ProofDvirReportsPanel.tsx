import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'

import {
  exportProofDvirReportSummaryCsv,
  getProofDvirReportDvirDetail,
  getProofDvirReportProofDetail,
  getProofDvirReportSummary,
  getProofDvirReportTripDetail,
} from '../api/client'

type Props = {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-700 bg-slate-950/60 px-3 py-2">
      <p className="text-xs text-slate-500">{label}</p>
      <p className="text-lg font-semibold text-slate-100">{value}</p>
    </div>
  )
}

export function ProofDvirReportsPanel({ accessToken, canRead, canExport }: Props) {
  const [scope, setScope] = useState<'daily' | 'weekly'>('daily')
  const [selectedTripId, setSelectedTripId] = useState<string | null>(null)
  const [selectedProofId, setSelectedProofId] = useState<string | null>(null)
  const [selectedDvirId, setSelectedDvirId] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['routarr-proof-dvir-report-summary', accessToken, scope],
    queryFn: () => getProofDvirReportSummary(accessToken, { scope }),
    enabled: canRead,
  })

  const tripDetailQuery = useQuery({
    queryKey: ['routarr-proof-dvir-report-trip', accessToken, selectedTripId],
    queryFn: () => getProofDvirReportTripDetail(accessToken, selectedTripId!),
    enabled: canRead && Boolean(selectedTripId),
  })

  const proofDetailQuery = useQuery({
    queryKey: ['routarr-proof-dvir-report-proof', accessToken, selectedProofId],
    queryFn: () => getProofDvirReportProofDetail(accessToken, selectedProofId!),
    enabled: canRead && Boolean(selectedProofId),
  })

  const dvirDetailQuery = useQuery({
    queryKey: ['routarr-proof-dvir-report-dvir', accessToken, selectedDvirId],
    queryFn: () => getProofDvirReportDvirDetail(accessToken, selectedDvirId!),
    enabled: canRead && Boolean(selectedDvirId),
  })

  const exportMutation = useMutation({
    mutationFn: () => exportProofDvirReportSummaryCsv(accessToken, { scope }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `routarr-proof-dvir-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="mt-8 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="proof-dvir-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Proof &amp; DVIR reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Pickup/delivery proof and pre/post-trip DVIR rollups from RoutArr-owned execution
            tables.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <label className="mt-4 flex items-center gap-2 text-sm text-slate-300">
        Scope
        <select
          className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          value={scope}
          onChange={(e) => {
            setScope(e.target.value as 'daily' | 'weekly')
            setSelectedTripId(null)
            setSelectedProofId(null)
            setSelectedDvirId(null)
          }}
        >
          <option value="daily">Daily</option>
          <option value="weekly">Weekly</option>
        </select>
      </label>

      {summaryQuery.isLoading ? (
        <p className="mt-3 text-sm text-slate-500">Loading proof/DVIR report summary…</p>
      ) : null}

      {summaryQuery.isError ? (
        <p className="mt-3 text-sm text-rose-400">Failed to load proof/DVIR report summary.</p>
      ) : null}

      {summaryQuery.data ? (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Proof records" value={String(summaryQuery.data.totalProofCount)} />
            <MetricCard label="DVIR inspections" value={String(summaryQuery.data.totalDvirCount)} />
            <MetricCard
              label="Trips with activity"
              value={String(summaryQuery.data.tripWithProofOrDvirCount)}
            />
            <MetricCard
              label="Fail/conditional DVIR"
              value={String(summaryQuery.data.failOrConditionalDvirCount)}
            />
          </div>

          <div className="mt-6">
            <h3 className="text-sm font-semibold text-slate-200">Trips</h3>
            {summaryQuery.data.trips.length === 0 ? (
              <p className="mt-2 text-xs text-slate-500">No proof or DVIR activity in this window.</p>
            ) : (
              <ul className="mt-2 max-h-64 space-y-1 overflow-y-auto text-sm">
                {summaryQuery.data.trips.map((trip) => (
                  <li key={trip.tripId}>
                    <button
                      type="button"
                      className={`w-full rounded px-2 py-1 text-left hover:bg-slate-800 ${
                        selectedTripId === trip.tripId ? 'bg-slate-800' : ''
                      }`}
                      onClick={() => {
                        setSelectedTripId(trip.tripId)
                        setSelectedProofId(null)
                        setSelectedDvirId(null)
                      }}
                    >
                      {trip.tripNumber} — {trip.title}
                      <span className="ml-2 text-xs text-slate-500">
                        {trip.proofCount} proof · pre {trip.hasPreTripDvir ? 'yes' : 'no'} · post{' '}
                        {trip.hasPostTripDvir ? 'yes' : 'no'}
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className="mt-6 grid gap-6 lg:grid-cols-2">
            <div>
              <h3 className="text-sm font-semibold text-slate-200">Recent proof</h3>
              {summaryQuery.data.recentProofs.length === 0 ? (
                <p className="mt-2 text-xs text-slate-500">No proof in this window.</p>
              ) : (
                <ul className="mt-2 max-h-48 space-y-1 overflow-y-auto text-sm">
                  {summaryQuery.data.recentProofs.map((proof) => (
                    <li key={proof.proofId}>
                      <button
                        type="button"
                        className={`w-full rounded px-2 py-1 text-left hover:bg-slate-800 ${
                          selectedProofId === proof.proofId ? 'bg-slate-800' : ''
                        }`}
                        onClick={() => {
                          setSelectedProofId(proof.proofId)
                          setSelectedTripId(null)
                          setSelectedDvirId(null)
                        }}
                      >
                        {proof.tripNumber} — {proof.proofType}
                        {proof.referenceKey ? ` · ${proof.referenceKey}` : ''}
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </div>

            <div>
              <h3 className="text-sm font-semibold text-slate-200">Recent DVIR</h3>
              {summaryQuery.data.recentDvirInspections.length === 0 ? (
                <p className="mt-2 text-xs text-slate-500">No DVIR in this window.</p>
              ) : (
                <ul className="mt-2 max-h-48 space-y-1 overflow-y-auto text-sm">
                  {summaryQuery.data.recentDvirInspections.map((dvir) => (
                    <li key={dvir.dvirId}>
                      <button
                        type="button"
                        className={`w-full rounded px-2 py-1 text-left hover:bg-slate-800 ${
                          selectedDvirId === dvir.dvirId ? 'bg-slate-800' : ''
                        }`}
                        onClick={() => {
                          setSelectedDvirId(dvir.dvirId)
                          setSelectedTripId(null)
                          setSelectedProofId(null)
                        }}
                      >
                        {dvir.tripNumber} — {dvir.phase.replace('_', ' ')} · {dvir.result}
                      </button>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>
        </>
      ) : null}

      {tripDetailQuery.data ? (
        <div
          className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
          data-testid="proof-dvir-report-trip-detail"
        >
          <h3 className="font-semibold text-slate-100">Trip detail</h3>
          <p className="mt-1 text-slate-300">
            {tripDetailQuery.data.tripNumber} — {tripDetailQuery.data.title}
          </p>
          <p className="text-xs text-slate-500">
            {tripDetailQuery.data.proofCount} proof · pre DVIR{' '}
            {tripDetailQuery.data.hasPreTripDvir ? 'yes' : 'no'} · post DVIR{' '}
            {tripDetailQuery.data.hasPostTripDvir ? 'yes' : 'no'}
          </p>
        </div>
      ) : null}

      {proofDetailQuery.data ? (
        <div
          className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
          data-testid="proof-dvir-report-proof-detail"
        >
          <h3 className="font-semibold text-slate-100">Proof detail</h3>
          <p className="mt-1 text-slate-300">
            {proofDetailQuery.data.tripNumber} — {proofDetailQuery.data.proofType}
          </p>
          <p className="text-xs text-slate-500">
            {proofDetailQuery.data.referenceKey}
            {proofDetailQuery.data.notes ? ` · ${proofDetailQuery.data.notes}` : ''}
          </p>
        </div>
      ) : null}

      {dvirDetailQuery.data ? (
        <div
          className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
          data-testid="proof-dvir-report-dvir-detail"
        >
          <h3 className="font-semibold text-slate-100">DVIR detail</h3>
          <p className="mt-1 text-slate-300">
            {dvirDetailQuery.data.tripNumber} — {dvirDetailQuery.data.phase.replace('_', ' ')} ·{' '}
            {dvirDetailQuery.data.result}
          </p>
          {dvirDetailQuery.data.defectNotes ? (
            <p className="text-xs text-slate-500">{dvirDetailQuery.data.defectNotes}</p>
          ) : null}
        </div>
      ) : null}
    </section>
  )
}
