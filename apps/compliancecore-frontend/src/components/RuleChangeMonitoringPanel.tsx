import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ControlledSelect } from '@stl/shared-ui'

import { getRuleChangeSummary, listRuleChangeEvents } from '../api/client'

interface RuleChangeMonitoringPanelProps {
  accessToken: string
}

const CHANGE_TYPES = [
  { value: '', label: 'All types' },
  { value: 'version_created', label: 'Version created' },
  { value: 'status_changed', label: 'Status changed' },
  { value: 'content_updated', label: 'Content updated' },
  { value: 'scan_detected', label: 'Scan detected' },
]

export function RuleChangeMonitoringPanel({ accessToken }: RuleChangeMonitoringPanelProps) {
  const [changeType, setChangeType] = useState('')
  const [packKey, setPackKey] = useState('')

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-rule-change-summary', accessToken],
    queryFn: () => getRuleChangeSummary(accessToken),
  })

  const packKeysQuery = useQuery({
    queryKey: ['compliancecore-rule-change-pack-keys', accessToken],
    queryFn: () => listRuleChangeEvents(accessToken, { limit: 200 }),
  })

  const eventsQuery = useQuery({
    queryKey: ['compliancecore-rule-change-events', accessToken, changeType, packKey],
    queryFn: () =>
      listRuleChangeEvents(accessToken, {
        changeType: changeType || undefined,
        packKey: packKey.trim() || undefined,
        limit: 50,
      }),
  })

  const summary = summaryQuery.data
  const packKeyOptions = [
    ...new Set([
      ...(packKeysQuery.data ?? []).map((event) => event.packKey).filter((value) => value.trim().length > 0),
      ...((eventsQuery.data ?? []).map((event) => event.packKey).filter((value) => value.trim().length > 0)),
      ...(packKey.trim().length > 0 ? [packKey.trim()] : []),
    ]),
  ]
    .sort((left, right) => left.localeCompare(right))
    .map((value) => ({ value, label: value }))

  return (
    <section
      data-testid="rule-change-monitoring-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Rule change monitoring</h2>
        <p className="mt-1 text-sm text-slate-400">
          Tracks rule pack version creation, status transitions, content updates, and periodic worker scans
          against monitor snapshots.
        </p>
      </header>

      {summary && (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Last 24 hours</p>
            <p className="mt-1 text-2xl font-semibold text-slate-100">{summary.eventsLast24Hours}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Last 7 days</p>
            <p className="mt-1 text-2xl font-semibold text-slate-100">{summary.eventsLast7Days}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Published transitions</p>
            <p className="mt-1 text-2xl font-semibold text-violet-300">{summary.statusChangedCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Worker scan hits</p>
            <p className="mt-1 text-2xl font-semibold text-amber-300">{summary.scanDetectedCount}</p>
          </div>
        </div>
      )}

      <div className="flex flex-wrap gap-3">
        <label htmlFor="rule-change-filter-type" className="flex flex-col gap-1 text-sm text-slate-400">
          Rule change type
          <select
            id="rule-change-filter-type"
            value={changeType}
            onChange={(event) => setChangeType(event.target.value)}
            className="rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-slate-200"
          >
            {CHANGE_TYPES.map((option) => (
              <option key={option.value || 'all'} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
        <ControlledSelect
          id="rule-change-filter-pack-key"
          label="Rule pack key filter"
          value={packKey}
          onChange={setPackKey}
          options={packKeyOptions}
          emptyLabel="All rule packs"
          testId="rule-change-filter-pack-key"
          className="rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 font-mono text-sm text-slate-200"
        />
      </div>

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Recent change events</h3>
        {(eventsQuery.data ?? []).length === 0 ? (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]" data-testid="rule-change-events-empty">
            No rule change events match the current filters.
          </p>
        ) : (
          <ul
            className="mt-3 max-h-80 space-y-2 overflow-y-auto"
            data-testid="rule-change-events-list"
          >
            {(eventsQuery.data ?? []).map((event) => (
              <li
                key={event.eventId}
                className="rounded border border-slate-800 px-3 py-2 text-sm text-slate-300"
              >
                <div className="flex flex-wrap items-center gap-2">
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-violet-300">
                    {event.changeType}
                  </span>
                  <span className="font-mono text-xs text-sky-300">{event.packKey}</span>
                  <span className="text-xs text-[var(--color-text-muted)]">{event.programKey}</span>
                  <span className="text-xs text-[var(--color-text-muted)]">{event.source}</span>
                </div>
                <p className="mt-1 text-slate-400">{event.summary}</p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  {new Date(event.detectedAt).toLocaleString()}
                </p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
