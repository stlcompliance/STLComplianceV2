import { type FormEvent, useState } from 'react'
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
  selectedNote: PersonnelNoteDetailResponse | null
  isLoading: boolean
  isLoadingDetail: boolean
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
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
  selectedNote,
  isLoading,
  isLoadingDetail,
  canManage,
  isSubmitting,
  errorMessage,
  onSelectNote,
  onCreateNote,
}: PersonnelNotesPanelProps) {
  const [categoryKey, setCategoryKey] = useState<PersonnelNoteCategoryKey>('general')
  const [visibilityKey, setVisibilityKey] = useState<PersonnelNoteVisibilityKey>('hr_only')
  const [subject, setSubject] = useState('')
  const [body, setBody] = useState('')

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
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Personnel notes</h2>
          <p className="mt-1 text-sm text-slate-400">
            HR personnel notes for {personDisplayName}. StaffArr owns note history with visibility controls.
          </p>
        </div>
        {canManage ? (
          <span className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-300 ring-1 ring-slate-600">
            staffarr.notes.manage
          </span>
        ) : null}
      </div>

      {errorMessage ? (
        <p className="mt-4 rounded-lg border border-rose-500/40 bg-rose-950/40 px-3 py-2 text-sm text-rose-200">
          {errorMessage}
        </p>
      ) : null}

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading notes…</p>
      ) : notes.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">No personnel notes recorded for this person yet.</p>
      ) : (
        <ul className="mt-4 space-y-2">
          {notes.map((note) => (
            <li key={note.noteId}>
              <button
                type="button"
                onClick={() => onSelectNote(note.noteId)}
                className={`w-full rounded-lg border px-3 py-2 text-left transition ${
                  selectedNote?.noteId === note.noteId
                    ? 'border-sky-500/60 bg-sky-950/30'
                    : 'border-slate-700 bg-slate-950/40 hover:border-slate-500'
                }`}
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium text-slate-100">{note.subject}</span>
                  <span className="rounded-full bg-slate-800 px-2 py-0.5 text-xs text-slate-300 ring-1 ring-slate-600">
                    {formatVisibilityLabel(note.visibilityKey)}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  {formatCategoryLabel(note.categoryKey)} · {new Date(note.createdAt).toLocaleString()}
                </p>
              </button>
            </li>
          ))}
        </ul>
      )}

      {selectedNote ? (
        <div className="mt-4 rounded-lg border border-slate-700 bg-slate-950/50 p-4">
          <h3 className="text-sm font-medium text-slate-200">Note detail</h3>
          {isLoadingDetail ? (
            <p className="mt-2 text-sm text-slate-400">Loading detail…</p>
          ) : (
            <>
              <p className="mt-2 whitespace-pre-wrap text-sm text-slate-300">{selectedNote.body}</p>
              <dl className="mt-3 grid gap-2 text-xs text-slate-400 sm:grid-cols-2">
                <div>
                  <dt className="uppercase tracking-wide">Category</dt>
                  <dd className="text-slate-200">{formatCategoryLabel(selectedNote.categoryKey)}</dd>
                </div>
                <div>
                  <dt className="uppercase tracking-wide">Visibility</dt>
                  <dd className="text-slate-200">{formatVisibilityLabel(selectedNote.visibilityKey)}</dd>
                </div>
              </dl>
            </>
          )}
        </div>
      ) : null}

      {canManage ? (
        <form onSubmit={handleSubmit} className="mt-6 space-y-3 border-t border-slate-700 pt-4">
          <h3 className="text-sm font-medium text-slate-200">Add personnel note</h3>
          <div className="grid gap-3 sm:grid-cols-2">
            <label className="block text-xs text-slate-400">
              Category
              <select
                value={categoryKey}
                onChange={(event) => setCategoryKey(event.target.value as PersonnelNoteCategoryKey)}
                className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              >
                {categoryOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
            <label className="block text-xs text-slate-400">
              Visibility
              <select
                value={visibilityKey}
                onChange={(event) => setVisibilityKey(event.target.value as PersonnelNoteVisibilityKey)}
                className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              >
                {visibilityOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
          </div>
          <label className="block text-xs text-slate-400">
            Subject
            <input
              value={subject}
              onChange={(event) => setSubject(event.target.value)}
              required
              minLength={4}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-xs text-slate-400">
            Body
            <textarea
              value={body}
              onChange={(event) => setBody(event.target.value)}
              required
              minLength={8}
              rows={4}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
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
