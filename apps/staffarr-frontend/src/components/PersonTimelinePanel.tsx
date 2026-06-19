import { ApiErrorCallout } from '@stl/shared-ui'
import type { PersonTimelineEntryResponse } from '../api/types'

export type PersonTimelineCategoryFilter =
  | ''
  | PersonTimelineEntryResponse['category']

export const PERSON_TIMELINE_CATEGORY_OPTIONS: Array<{
  value: PersonTimelineCategoryFilter
  label: string
}> = [
  { value: '', label: 'All categories' },
  { value: 'incident', label: 'Incidents' },
  { value: 'incident_routing', label: 'TrainArr routing' },
  { value: 'readiness', label: 'Readiness' },
  { value: 'certification', label: 'Certifications' },
  { value: 'permission', label: 'Permissions' },
  { value: 'training_blocker', label: 'Training blockers' },
  { value: 'personnel_note', label: 'Personnel notes' },
  { value: 'personnel_document', label: 'Personnel documents' },
  { value: 'recruiting', label: 'Recruiting' },
]

export const PERSON_TIMELINE_PAGE_SIZE_OPTIONS = [10, 25, 50] as const

interface PersonTimelinePanelProps {
  personDisplayName: string
  entries: PersonTimelineEntryResponse[]
  totalCount: number
  page: number
  pageSize: number
  hasNextPage: boolean
  categoryFilter: PersonTimelineCategoryFilter
  isLoading: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
  onCategoryFilterChange: (value: PersonTimelineCategoryFilter) => void
  onPageChange: (page: number) => void
  onPageSizeChange: (pageSize: number) => void
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
    case 'recruiting':
      return 'Recruiting'
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
  page,
  pageSize,
  hasNextPage,
  categoryFilter,
  isLoading,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
  onCategoryFilterChange,
  onPageChange,
  onPageSizeChange,
}: PersonTimelinePanelProps) {
  const totalPages = totalCount === 0 ? 0 : Math.ceil(totalCount / pageSize)
  const showingFrom = totalCount === 0 ? 0 : (page - 1) * pageSize + 1
  const showingTo = totalCount === 0 ? 0 : Math.min(page * pageSize, totalCount)

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <h2 className="text-sm font-medium text-slate-300">Person history timeline</h2>
      <p className="mt-2 text-xs text-slate-500">
        Unified workforce history for {personDisplayName} from incidents, readiness, certifications, permissions,
        training blockers, personnel notes, recruiting, and documents.
      </p>

      <div className="mt-4 flex flex-wrap items-end gap-3">
        <label htmlFor="person-timeline-category-filter" className="block text-sm text-slate-400">
          Category
          <select
            id="person-timeline-category-filter"
            className="mt-1 block w-full min-w-[12rem] rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
            value={categoryFilter}
            onChange={(e) => onCategoryFilterChange(e.target.value as PersonTimelineCategoryFilter)}
            data-testid="person-timeline-category-filter"
          >
            {PERSON_TIMELINE_CATEGORY_OPTIONS.map((option) => (
              <option key={option.value || 'all'} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
        <label htmlFor="person-timeline-page-size" className="block text-sm text-slate-400">
          Page size
          <select
            id="person-timeline-page-size"
            className="mt-1 block w-full min-w-[6rem] rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            data-testid="person-timeline-page-size"
          >
            {PERSON_TIMELINE_PAGE_SIZE_OPTIONS.map((size) => (
              <option key={size} value={size}>
                {size}
              </option>
            ))}
          </select>
        </label>
      </div>

      {isError ? (
        <ApiErrorCallout
          className="mt-4"
          title="Timeline unavailable"
          message={readErrorMessage ?? 'Failed to load person timeline events.'}
          retryLabel={onRetryRead ? 'Retry timeline' : undefined}
          onRetry={onRetryRead}
        />
      ) : isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading timeline…</p>
      ) : entries.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">
          {categoryFilter
            ? 'No timeline events match this category filter.'
            : 'No timeline events recorded yet.'}
        </p>
      ) : (
        <>
          <p className="mt-3 text-xs text-slate-500" data-testid="person-timeline-range">
            Showing {showingFrom}–{showingTo} of {totalCount} event{totalCount === 1 ? '' : 's'}
            {categoryFilter ? ` (${categoryLabel(categoryFilter)})` : ''}
          </p>
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

      {totalCount > 0 ? (
        <div className="mt-4 flex flex-wrap items-center gap-3">
          <button
            type="button"
            className="rounded-lg border border-slate-700 px-3 py-1.5 text-sm text-slate-200 disabled:cursor-not-allowed disabled:opacity-40"
            disabled={page <= 1 || isLoading}
            onClick={() => onPageChange(page - 1)}
            data-testid="person-timeline-prev-page"
          >
            Previous
          </button>
          <span className="text-xs text-slate-500" data-testid="person-timeline-page-indicator">
            Page {page} of {totalPages}
          </span>
          <button
            type="button"
            className="rounded-lg border border-slate-700 px-3 py-1.5 text-sm text-slate-200 disabled:cursor-not-allowed disabled:opacity-40"
            disabled={!hasNextPage || isLoading}
            onClick={() => onPageChange(page + 1)}
            data-testid="person-timeline-next-page"
          >
            Next
          </button>
        </div>
      ) : null}
    </section>
  )
}
