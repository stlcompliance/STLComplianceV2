import { type FormEvent, useState } from 'react'
import { ApiErrorCallout } from '@stl/shared-ui'
import type {
  CreatePersonnelNoteRequest,
  PersonnelNoteCategoryKey,
  PersonnelNoteDetailResponse,
  PersonnelNoteSummaryResponse,
  PersonnelNoteVisibilityKey,
} from '../api/types'

interface PersonnelNotesPanelProps {
  personId: string
  personDisplayName: string
  notes: PersonnelNoteSummaryResponse[]
  selectedNoteId?: string | null
  selectedNote: PersonnelNoteDetailResponse | null
  isLoading: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
  isLoadingDetail: boolean
  isDetailError?: boolean
  detailErrorMessage?: string | null
  onRetryDetail?: () => void
  canManage: boolean
  isSubmitting: boolean
  actionErrorMessage: string | null
  onSelectNote: (noteId: string) => void
  onCreateNote: (request: CreatePersonnelNoteRequest) => Promise<void>
}

const categoryOptions: { value: PersonnelNoteCategoryKey; label: string }[] = [
  { value: 'general', label: 'General' },
  { value: 'performance', label: 'Performance' },
  { value: 'coaching', label: 'Coaching' },
  { value: 'disciplinary', label: 'Disciplinary' },
  { value: 'medical', label: 'Medical' },
  { value: 'other', label: 'Other' },
]

const visibilityOptions: { value: PersonnelNoteVisibilityKey; label: string }[] = [
  { value: 'hr_only', label: 'HR only' },
  { value: 'management', label: 'Management' },
  { value: 'personnel_visible', label: 'Personnel visible' },
]

export function canManagePersonnelNotes(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) {
    return true
  }

  return ['tenant_admin', 'staffarr_admin', 'hr_admin'].includes(tenantRoleKey)
}

function formatCategoryLabel(key: string): string {
  const match = categoryOptions.find((option) => option.value === key)
  return match?.label ?? key
}

function formatVisibilityLabel(key: string): string {
  const match = visibilityOptions.find((option) => option.value === key)
  return match?.label ?? key
}

export function PersonnelNotesPanel({
  personId: _personId,
  personDisplayName,
  notes,
  selectedNoteId = null,
  selectedNote,
  isLoading,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
  isLoadingDetail,
  isDetailError = false,
  detailErrorMessage = null,
  onRetryDetail,
  canManage,
  isSubmitting,
  actionErrorMessage,
  onSelectNote,
  onCreateNote,
}: PersonnelNotesPanelProps) {
  const [categoryKey, setCategoryKey] = useState<PersonnelNoteCategoryKey>('general')
  const [visibilityKey, setVisibilityKey] = useState<PersonnelNoteVisibilityKey>('hr_only')
  const [subject, setSubject] = useState('')
  const [body, setBody] = useState('')
  const panelClassName =
    'mt-6 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6'
  const panelHeadingClassName = 'text-sm font-medium text-[var(--color-text-secondary)]'
  const panelCopyClassName = 'text-sm text-[var(--color-text-muted)]'
  const secondaryTextClassName = 'text-[var(--color-text-secondary)]'
  const primaryTextClassName = 'text-[var(--color-text-primary)]'

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onCreateNote({
      categoryKey,
      visibilityKey,
      subject,
      body,
    })
    setSubject('')
    setBody('')
  }

  return (
    <section className={panelClassName}>
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className={panelHeadingClassName}>Personnel notes</h2>
          <p className={`mt-1 ${panelCopyClassName}`}>
            HR personnel notes for {personDisplayName}. Review note history with visibility controls.
          </p>
        </div>
        {canManage ? (
          <span className="rounded-full bg-[var(--color-bg-control)] px-3 py-1 text-xs text-[var(--color-text-secondary)] ring-1 ring-[var(--color-border-subtle)]">
            staffarr.notes.manage
          </span>
        ) : null}
      </div>

      {actionErrorMessage ? (
        <div className="mt-4">
          <ApiErrorCallout title="Personnel notes action failed" message={actionErrorMessage} />
        </div>
      ) : null}

      {isLoading ? (
        <p className={`mt-4 ${panelCopyClassName}`}>Loading notes…</p>
      ) : isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Personnel notes unavailable"
            message={readErrorMessage ?? 'Failed to load personnel notes.'}
            onRetry={onRetryRead}
            retryLabel="Retry notes"
          />
        </div>
      ) : notes.length === 0 ? (
        <p className={`mt-4 ${panelCopyClassName}`}>No personnel notes recorded for this person yet.</p>
      ) : (
        <ul className="mt-4 space-y-2">
          {notes.map((note) => (
            <li key={note.noteId}>
              <button
                type="button"
                onClick={() => onSelectNote(note.noteId)}
                className={`w-full rounded-lg border px-3 py-2 text-left transition ${
                  selectedNote?.noteId === note.noteId
                    ? 'border-sky-500/60 bg-sky-500/10'
                    : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] hover:border-[var(--color-border-strong)]'
                }`}
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className={`font-medium ${primaryTextClassName}`}>{note.subject}</span>
                  <span className="rounded-full bg-[var(--color-bg-control)] px-2 py-0.5 text-xs text-[var(--color-text-secondary)] ring-1 ring-[var(--color-border-subtle)]">
                    {formatVisibilityLabel(note.visibilityKey)}
                  </span>
                </div>
                <p className={`mt-1 text-xs ${panelCopyClassName}`}>
                  {formatCategoryLabel(note.categoryKey)} · {new Date(note.createdAt).toLocaleString()}
                </p>
              </button>
            </li>
          ))}
        </ul>
      )}

      {selectedNoteId ? (
        <div className="mt-4 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
          <h3 className={`text-sm font-medium ${secondaryTextClassName}`}>Note detail</h3>
          {isLoadingDetail ? (
            <p className={`mt-2 ${panelCopyClassName}`}>Loading detail…</p>
          ) : isDetailError ? (
            <div className="mt-2">
              <ApiErrorCallout
                title="Note detail unavailable"
                message={detailErrorMessage ?? 'Failed to load note detail.'}
                onRetry={onRetryDetail}
                retryLabel="Retry note detail"
              />
            </div>
          ) : (
            <>
              {selectedNote ? (
                <>
                  <p className={`mt-2 whitespace-pre-wrap text-sm ${secondaryTextClassName}`}>{selectedNote.body}</p>
                  <dl className={`mt-3 grid gap-2 text-xs ${panelCopyClassName} sm:grid-cols-2`}>
                    <div>
                      <dt className="uppercase tracking-wide">Category</dt>
                      <dd className={primaryTextClassName}>{formatCategoryLabel(selectedNote.categoryKey)}</dd>
                    </div>
                    <div>
                      <dt className="uppercase tracking-wide">Visibility</dt>
                      <dd className={primaryTextClassName}>{formatVisibilityLabel(selectedNote.visibilityKey)}</dd>
                    </div>
                  </dl>
                </>
              ) : (
                <p className={`mt-2 ${panelCopyClassName}`}>Note detail is unavailable.</p>
              )}
            </>
          )}
        </div>
      ) : null}

      {canManage ? (
        <form onSubmit={handleSubmit} className="mt-6 space-y-3 border-t border-[var(--color-border-subtle)] pt-4">
          <h3 className={`text-sm font-medium ${secondaryTextClassName}`}>Add personnel note</h3>
          <div className="grid gap-3 sm:grid-cols-2">
            <label htmlFor="personnel-note-category" className={`block text-xs ${secondaryTextClassName}`}>
              Category
              <select
                id="personnel-note-category"
                value={categoryKey}
                onChange={(event) => setCategoryKey(event.target.value as PersonnelNoteCategoryKey)}
                className="mt-1 w-full rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
              >
                {categoryOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="personnel-note-visibility" className={`block text-xs ${secondaryTextClassName}`}>
              Visibility
              <select
                id="personnel-note-visibility"
                value={visibilityKey}
                onChange={(event) => setVisibilityKey(event.target.value as PersonnelNoteVisibilityKey)}
                className="mt-1 w-full rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
              >
                {visibilityOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
          </div>
          <label htmlFor="personnel-note-subject" className={`block text-xs ${secondaryTextClassName}`}>
            Subject
            <input
              id="personnel-note-subject"
              value={subject}
              onChange={(event) => setSubject(event.target.value)}
              required
              minLength={4}
              className="mt-1 w-full rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="personnel-note-body" className={`block text-xs ${secondaryTextClassName}`}>
            Body
            <textarea
              id="personnel-note-body"
              value={body}
              onChange={(event) => setBody(event.target.value)}
              required
              minLength={8}
              rows={4}
              className="mt-1 w-full rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            {isSubmitting ? 'Saving…' : 'Save note'}
          </button>
        </form>
      ) : null}
    </section>
  )
}
