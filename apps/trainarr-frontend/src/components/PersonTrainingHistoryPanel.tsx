import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'

import { getPersonTrainingHistory } from '../api/client'

interface PersonTrainingHistoryPanelProps {
  accessToken: string
  defaultStaffarrPersonId?: string
}

export function PersonTrainingHistoryPanel({
  accessToken,
  defaultStaffarrPersonId = '',
}: PersonTrainingHistoryPanelProps) {
  const [staffarrPersonId, setStaffarrPersonId] = useState(defaultStaffarrPersonId)

  const historyQuery = useQuery({
    queryKey: ['trainarr-person-training-history', accessToken, staffarrPersonId],
    queryFn: () => getPersonTrainingHistory(accessToken, staffarrPersonId, 25),
    enabled: staffarrPersonId.length > 0,
  })

  return (
    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid="person-training-history-panel"
    >
      <h2 className="text-lg font-semibold text-foreground">Person training history</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Materialized training timeline for a StaffArr person, built from processed domain events.
      </p>

      <label className="mt-4 block text-sm">
        StaffArr person ID
        <input
          className="mt-1 w-full rounded border border-border px-2 py-1 font-mono text-xs"
          value={staffarrPersonId}
          onChange={(event) => setStaffarrPersonId(event.target.value.trim())}
          placeholder="00000000-0000-0000-0000-000000000000"
          data-testid="person-training-history-person-id"
        />
      </label>

      {staffarrPersonId.length === 0 && (
        <p className="mt-3 text-sm text-muted-foreground">Enter a person ID to load training history.</p>
      )}

      {historyQuery.isLoading && staffarrPersonId.length > 0 && (
        <p className="mt-3 text-sm text-muted-foreground">Loading history…</p>
      )}

      {historyQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load person training history.</p>
      )}

      {historyQuery.data && historyQuery.data.items.length === 0 && (
        <p className="mt-3 text-sm text-muted-foreground" data-testid="person-training-history-empty">
          No training history entries for this person yet.
        </p>
      )}

      {historyQuery.data && historyQuery.data.items.length > 0 && (
        <ul className="mt-4 space-y-2" data-testid="person-training-history-list">
          {historyQuery.data.items.map((entry) => (
            <li key={entry.entryId} className="rounded border border-border px-3 py-2 text-sm">
              <div className="font-medium text-foreground">{entry.summary}</div>
              <div className="mt-1 text-xs text-muted-foreground">
                {entry.eventKind} · {new Date(entry.occurredAt).toLocaleString()}
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
