import { ArrowRightLeft, BellRing, CheckCircle2, CircleAlert, ExternalLink, RefreshCw, ShieldCheck, TimerReset, Wrench } from 'lucide-react'

import type {
  AssetComplaintSignalResponse,
  AssetEnrichmentSnapshotResponse,
  AssetExternalIntelligenceOverviewResponse,
  ExternalIntelligenceProviderSummaryResponse,
} from '../api/types'

interface AssetExternalIntelligencePanelProps {
  overview: AssetExternalIntelligenceOverviewResponse | null
  isLoading: boolean
  isRefreshing?: boolean
  onRefresh?: () => void
  onAcceptSuggestion?: (suggestionId: string) => void
  onRejectSuggestion?: (suggestionId: string) => void
  onCreateRecallWorkOrder?: (recallId: string) => void
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleString()
}

function statusTone(status: string): string {
  const normalized = status.toLowerCase()
  if (['healthy', 'active', 'ready', 'accepted', 'verified'].includes(normalized)) {
    return 'border-emerald-500/30 bg-emerald-500/10 text-emerald-200'
  }
  if (['warn', 'warning', 'planned', 'pending'].includes(normalized)) {
    return 'border-amber-500/30 bg-amber-500/10 text-amber-100'
  }
  if (['disabled', 'error', 'rejected', 'resolved'].includes(normalized)) {
    return 'border-rose-500/30 bg-rose-500/10 text-rose-100'
  }
  return 'border-slate-700 bg-slate-900/70 text-slate-200'
}

function summaryTone(count: number): string {
  if (count > 0) {
    return 'text-white'
  }
  return 'text-slate-300'
}

function renderProviderSupport(provider: ExternalIntelligenceProviderSummaryResponse): string {
  const supports = [
    provider.supportsVinDecode ? 'VIN' : null,
    provider.supportsRecallLookup ? 'Recalls' : null,
    provider.supportsComplaintLookup ? 'Complaints' : null,
    provider.supportsReferenceLookups ? 'References' : null,
    provider.supportsEquipmentReferences ? 'Equipment' : null,
  ].filter(Boolean)
  return supports.length > 0 ? supports.join(' · ') : 'No active capabilities'
}

function renderSnapshotTitle(snapshot: AssetEnrichmentSnapshotResponse): string {
  return snapshot.snapshotType.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function renderComplaintFlags(complaint: AssetComplaintSignalResponse): string {
  const flags = [
    complaint.crash ? 'Crash' : null,
    complaint.fire ? 'Fire' : null,
  ].filter(Boolean)
  return flags.length > 0 ? flags.join(' · ') : 'No crash/fire flags'
}

export function AssetExternalIntelligencePanel({
  overview,
  isLoading,
  isRefreshing = false,
  onRefresh,
  onAcceptSuggestion,
  onRejectSuggestion,
  onCreateRecallWorkOrder,
}: AssetExternalIntelligencePanelProps) {
  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading external intelligence…</p>
  }

  if (!overview) {
    return (
      <div className="space-y-3">
        <p className="text-sm text-slate-400">No external intelligence has been collected yet for this asset.</p>
        {onRefresh ? (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-xs font-medium text-slate-100 hover:bg-slate-800 disabled:opacity-50"
            disabled={isRefreshing}
            onClick={onRefresh}
          >
            <RefreshCw className={`h-4 w-4 ${isRefreshing ? 'animate-spin' : ''}`} />
            Refresh intelligence
          </button>
        ) : null}
      </div>
    )
  }

  const latestSnapshot = overview.snapshots[0] ?? null
  const latestRecall = overview.recalls[0] ?? null
  const latestSuggestion = overview.suggestions[0] ?? null

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className="text-sm font-medium text-white">NHTSA / external intelligence</p>
          <p className="mt-1 text-xs text-slate-500">
            VIN decode, recall, complaint, and reference snapshots for this asset.
          </p>
        </div>
        {onRefresh ? (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-xs font-medium text-slate-100 hover:bg-slate-800 disabled:opacity-50"
            disabled={isRefreshing}
            onClick={onRefresh}
          >
            <RefreshCw className={`h-4 w-4 ${isRefreshing ? 'animate-spin' : ''}`} />
            Refresh
          </button>
        ) : null}
      </div>

      <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-5">
        {[
          ['Identifiers', overview.summary.identifierCount],
          ['Snapshots', overview.summary.snapshotCount],
          ['Suggestions', overview.summary.suggestionCount],
          ['Active recalls', overview.summary.activeRecallCount],
          ['Complaints', overview.summary.complaintCount],
        ].map(([label, count]) => (
          <div key={label} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <div className="text-xs uppercase tracking-wide text-slate-500">{label}</div>
            <div className={`mt-1 text-lg font-semibold ${summaryTone(Number(count))}`}>{count}</div>
          </div>
        ))}
      </div>

      <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
        <div className="flex flex-wrap items-center gap-2 text-xs text-slate-400">
          <ShieldCheck className="h-4 w-4" />
          <span>VIN: {overview.vin ?? 'Not recorded'}</span>
          <span className="text-slate-600">·</span>
          <span>Last refreshed: {formatDateTime(overview.summary.lastRefreshedAt)}</span>
        </div>
      </div>

      <div className="space-y-3">
        <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-slate-500">
          <ArrowRightLeft className="h-4 w-4" />
          Providers
        </div>
        <div className="grid gap-3 xl:grid-cols-2">
          {overview.providers.map((provider) => (
            <div key={provider.providerKey} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <div className="text-sm font-medium text-white">{provider.displayName}</div>
                  <div className="mt-1 text-xs text-slate-500">{provider.description}</div>
                </div>
                <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${statusTone(provider.status)}`}>
                  {provider.status}
                </span>
              </div>
              <div className="mt-2 text-xs text-slate-400">{renderProviderSupport(provider)}</div>
              <div className="mt-2 text-[11px] text-slate-500">
                Source: {provider.sourceOfTruth}
                {provider.lastSuccessfulAt ? ` · Last success ${formatDateTime(provider.lastSuccessfulAt)}` : ''}
              </div>
              {provider.lastError ? (
                <p className="mt-2 rounded-md border border-rose-500/20 bg-rose-500/10 p-2 text-xs text-rose-100">
                  {provider.lastError}
                </p>
              ) : null}
            </div>
          ))}
        </div>
      </div>

      <div className="grid gap-4 xl:grid-cols-2">
        <div className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/60 p-3">
          <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-slate-500">
            <BellRing className="h-4 w-4" />
            Suggestions
          </div>
          {overview.suggestions.length === 0 ? (
            <p className="text-sm text-slate-400">No enrichment suggestions are available.</p>
          ) : (
            <ul className="space-y-2">
              {overview.suggestions.slice(0, 3).map((suggestion) => (
                <li key={suggestion.suggestionId} className="rounded-lg border border-slate-800 bg-slate-950/70 p-3">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <div className="text-sm font-medium text-white">{suggestion.fieldLabel}</div>
                      <div className="mt-1 text-xs text-slate-500">
                        {suggestion.fieldKey} · {Math.round(suggestion.confidence * 100)}% confidence
                      </div>
                    </div>
                    <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${statusTone(suggestion.status)}`}>
                      {suggestion.status}
                    </span>
                  </div>
                  <div className="mt-2 text-xs text-slate-300">
                    <span className="text-slate-500">Current:</span> {suggestion.currentValue ?? 'Not recorded'}
                    <span className="mx-2 text-slate-600">·</span>
                    <span className="text-slate-500">Proposed:</span> {suggestion.proposedValue ?? 'Not proposed'}
                  </div>
                  <p className="mt-2 text-xs leading-5 text-slate-400">{suggestion.reason}</p>
                  {onAcceptSuggestion || onRejectSuggestion ? (
                    <div className="mt-3 flex flex-wrap gap-2">
                      {onAcceptSuggestion ? (
                        <button
                          type="button"
                          className="inline-flex items-center gap-2 rounded-lg bg-emerald-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-emerald-500 disabled:opacity-50"
                          onClick={() => onAcceptSuggestion(suggestion.suggestionId)}
                        >
                          <CheckCircle2 className="h-4 w-4" />
                          Accept
                        </button>
                      ) : null}
                      {onRejectSuggestion ? (
                        <button
                          type="button"
                          className="inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-xs font-semibold text-slate-200 hover:bg-slate-800 disabled:opacity-50"
                          onClick={() => onRejectSuggestion(suggestion.suggestionId)}
                        >
                          <CircleAlert className="h-4 w-4" />
                          Reject
                        </button>
                      ) : null}
                    </div>
                  ) : null}
                </li>
              ))}
            </ul>
          )}

          {latestSuggestion ? (
            <p className="text-xs text-slate-500">
              Latest suggestion: {latestSuggestion.fieldLabel} · {formatDateTime(latestSuggestion.updatedAt)}
            </p>
          ) : null}
        </div>

        <div className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/60 p-3">
          <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-slate-500">
            <TimerReset className="h-4 w-4" />
            Recalls and complaints
          </div>
          {overview.recalls.length === 0 ? (
            <p className="text-sm text-slate-400">No active recall snapshots are available.</p>
          ) : (
            <ul className="space-y-2">
              {overview.recalls.slice(0, 3).map((recall) => (
                <li key={recall.recallId} className="rounded-lg border border-slate-800 bg-slate-950/70 p-3">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <div className="text-sm font-medium text-white">{recall.campaignNumber}</div>
                      <div className="mt-1 text-xs text-slate-500">
                        {recall.component} · {recall.manufacturer}
                      </div>
                    </div>
                    <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${statusTone(recall.status)}`}>
                      {recall.status}
                    </span>
                  </div>
                  <p className="mt-2 text-xs leading-5 text-slate-400">{recall.summary}</p>
                  {onCreateRecallWorkOrder ? (
                    <button
                      type="button"
                      className="mt-3 inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-xs font-semibold text-slate-100 hover:bg-slate-800 disabled:opacity-50"
                      onClick={() => onCreateRecallWorkOrder(recall.recallId)}
                    >
                      <Wrench className="h-4 w-4" />
                      Create recall work order
                    </button>
                  ) : null}
                </li>
              ))}
            </ul>
          )}

          {overview.complaints.length > 0 ? (
            <div className="space-y-2">
              <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-slate-500">
                <ExternalLink className="h-4 w-4" />
                Complaints
              </div>
              <ul className="space-y-2">
                {overview.complaints.slice(0, 3).map((complaint) => (
                  <li key={complaint.odiNumber} className="rounded-lg border border-slate-800 bg-slate-950/70 p-3">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                      <div>
                        <div className="text-sm font-medium text-white">{complaint.odiNumber}</div>
                        <div className="mt-1 text-xs text-slate-500">
                          {complaint.manufacturer ?? 'Unknown manufacturer'} · {renderComplaintFlags(complaint)}
                        </div>
                      </div>
                      <span className="rounded-full border border-slate-700 px-2 py-0.5 text-[11px] uppercase tracking-wide text-slate-200">
                        Complaint
                      </span>
                    </div>
                    <p className="mt-2 text-xs leading-5 text-slate-400">{complaint.summary}</p>
                  </li>
                ))}
              </ul>
            </div>
          ) : (
            <p className="text-sm text-slate-400">No complaint signal snapshot is available.</p>
          )}

          {latestRecall ? (
            <p className="text-xs text-slate-500">
              Latest recall: {latestRecall.campaignNumber} · {formatDateTime(latestRecall.capturedAt)}
            </p>
          ) : null}
        </div>
      </div>

      <div className="space-y-2 rounded-lg border border-slate-800 bg-slate-950/60 p-3">
        <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-slate-500">
          <TimerReset className="h-4 w-4" />
          Identifiers
        </div>
        {overview.identifiers.length === 0 ? (
          <p className="text-sm text-slate-400">No identifiers stored yet.</p>
        ) : (
          <ul className="space-y-2">
            {overview.identifiers.slice(0, 4).map((identifier) => (
              <li key={identifier.identifierId} className="rounded-lg border border-slate-800 bg-slate-950/70 p-3 text-xs text-slate-300">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <div className="font-medium text-white">{identifier.identifierType}</div>
                    <div className="mt-1 text-slate-500">{identifier.sourceSystem}</div>
                  </div>
                  <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${identifier.isPrimary ? 'border-sky-500/30 bg-sky-500/10 text-sky-100' : 'border-slate-700 bg-slate-900 text-slate-300'}`}>
                    {identifier.isPrimary ? 'Primary' : 'Secondary'}
                  </span>
                </div>
                <div className="mt-2 break-all font-mono text-sm text-sky-100">{identifier.identifierValue}</div>
                {identifier.metadata ? (
                  <pre className="mt-2 overflow-x-auto rounded-md border border-slate-800 bg-slate-900/80 p-2 text-[11px] text-slate-400">
                    {JSON.stringify(identifier.metadata, null, 2)}
                  </pre>
                ) : null}
              </li>
            ))}
          </ul>
        )}
      </div>

      {latestSnapshot ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3 text-xs text-slate-400">
          <div className="font-medium text-white">{renderSnapshotTitle(latestSnapshot)}</div>
          <div className="mt-1">{latestSnapshot.summary}</div>
          <div className="mt-2 text-slate-500">
            Captured {formatDateTime(latestSnapshot.capturedAt)}
            {latestSnapshot.sourceObjectRef ? ` · ${latestSnapshot.sourceObjectRef}` : ''}
          </div>
          {latestSnapshot.details ? (
            <pre className="mt-2 overflow-x-auto rounded-md border border-slate-800 bg-slate-900/80 p-2 text-[11px] text-slate-400">
              {JSON.stringify(latestSnapshot.details, null, 2)}
            </pre>
          ) : null}
        </div>
      ) : null}
    </div>
  )
}
