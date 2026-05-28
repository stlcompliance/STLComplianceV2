import type { PersonTimelineEntryResponse } from '../api/types'

interface PersonTimelinePanelProps {
  personDisplayName: string
  entries: PersonTimelineEntryResponse[]
  totalCount: number
  isLoading: boolean
}

function categoryLabel(category: PersonTimelineEntryResponse['category']): string {
  switch (category) {
    case 'incident':
      return 'Incident'
    case 'incident_routing':
      return 'TrainArr routing'
    case 'readiness':
      return 'Readiness'
    case 'certification':
      return 'Certification'
    case 'permission':
      return 'Permission'
    case 'training_blocker':
      return 'Training blocker'
    case 'personnel_note':
      return 'Personnel note'
    case 'personnel_document':
      return 'Personnel document'
    default:
      return category
  }
}

function eventTypeLabel(eventType: string): string {
  switch (eventType) {
    case 'incident_reported':
      return 'Incident reported'
    case 'incident_routed_trainarr':
      return 'Routed to TrainArr'
    case 'readiness_override_granted':
      return 'Readiness override granted'
    case 'readiness_override_cleared':
      return 'Readiness override cleared'
    case 'certification_granted':
      return 'Certification granted'
    case 'training_blocker_published':
      return 'Training blocker published'
    case 'training_blocker_cleared':
      return 'Training blocker cleared'
    case 'personnel_note_created':
      return 'Personnel note created'
    case 'personnel_document_uploaded':
      return 'Personnel document uploaded'
    case 'assignment_created':
      return 'Role assignment created'
    case 'assignment_status_updated':
      return 'Role assignment updated'
    case 'role_template_permissions_updated':
      return 'Role template permissions updated'
    default:
      return eventType.replaceAll('_', ' ')
  }
}

export function PersonTimelinePanel({
  personDisplayName,
  entries,
  totalCount,
  isLoading,
}: PersonTimelinePanelProps) {
  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <h2 className="text-sm font-medium text-slate-300">Person history timeline</h2>
      <p className="mt-2 text-xs text-slate-500">
        Unified workforce history for {personDisplayName} from incidents, readiness, certifications, permissions,
        training blockers, personnel notes, and documents.
      </p>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading timeline…</p>
      ) : entries.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">No timeline events recorded yet.</p>
      ) : (
        <>
          <p className="mt-3 text-xs text-slate-500">{totalCount} event{totalCount === 1 ? '' : 's'} total</p>
          <ul className="mt-3 divide-y divide-slate-700 text-sm">
            {entries.map((entry) => (
              <li key={entry.entryId} className="py-3">
                <div className="flex flex-wrap items-baseline gap-2">
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-300">
                    {categoryLabel(entry.category)}
                  </span>
                  <p className="text-white">{entry.title}</p>
                </div>
                <p className="mt-1 text-xs text-slate-400">{eventTypeLabel(entry.eventType)}</p>
                {entry.detail ? <p className="mt-1 text-xs text-slate-500">{entry.detail}</p> : null}
                {entry.externalReferenceId ? (
                  <p className="mt-1 text-xs text-slate-500">External ref: {entry.externalReferenceId}</p>
                ) : null}
                <p className="mt-1 text-xs text-slate-500">{new Date(entry.occurredAt).toLocaleString()}</p>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  )
}
