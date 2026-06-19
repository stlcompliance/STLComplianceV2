import { type FormEvent, useState } from 'react'
import { ApiErrorCallout } from '@stl/shared-ui'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import type {
  CreateIncidentAttachmentRequest,
  CreateIncidentNoteRequest,
  CreatePersonnelIncidentRequest,
  PersonnelIncidentDetailResponse,
  PersonnelIncidentReasonCategory,
  PersonnelIncidentSeverity,
  PersonnelIncidentSummaryResponse,
  UpdateIncidentNoteStatusRequest,
} from '../api/types'

type ControlledOption<T extends string = string> = { value: T; label: string }

interface IncidentsPanelProps {
  personId: string
  personDisplayName: string
  incidents: PersonnelIncidentSummaryResponse[]
  reasonCategoryOptions: Array<ControlledOption<PersonnelIncidentReasonCategory>>
  severityOptions: Array<ControlledOption<PersonnelIncidentSeverity>>
  selectedIncidentId?: string | null
  selectedIncident: PersonnelIncidentDetailResponse | null
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
  onSelectIncident: (incidentId: string) => void
  onCreateIncident: (request: CreatePersonnelIncidentRequest) => Promise<void>
  onRouteToTrainarr?: (incidentId: string) => Promise<void>
  onUpdateIncidentStatus?: (incidentId: string, status: 'open' | 'closed') => Promise<void>
  onCreateIncidentNote?: (incidentId: string, request: CreateIncidentNoteRequest) => Promise<void>
  onUpdateIncidentNoteStatus?: (
    incidentId: string,
    noteId: string,
    request: UpdateIncidentNoteStatusRequest,
  ) => Promise<void>
  onCreateIncidentAttachment?: (
    incidentId: string,
    request: CreateIncidentAttachmentRequest,
  ) => Promise<void>
  onDownloadIncidentAttachment?: (incidentId: string, attachmentId: string) => Promise<void>
  isRouting?: boolean
  isUpdatingStatus?: boolean
  isCreatingIncidentNote?: boolean
  isUpdatingIncidentNoteStatus?: boolean
  isCreatingIncidentAttachment?: boolean
}

export function isIncidentRoutableToTrainarr(reasonCategoryKey: string): boolean {
  return reasonCategoryKey === 'training_compliance'
}

export function canManageIncidents(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) {
    return true
  }

  return ['tenant_admin', 'staffarr_admin', 'hr_admin'].includes(tenantRoleKey)
}

function humanizeKey(value: string): string {
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (character) => character.toUpperCase())
}

function formatCategoryLabel(key: string, options: Array<ControlledOption>): string {
  const match = options.find((option) => option.value === key)
  return match?.label ?? humanizeKey(key)
}

function severityBadgeClass(severity: string): string {
  switch (severity) {
    case 'critical':
      return 'bg-rose-500/20 text-rose-200 ring-rose-500/40'
    case 'high':
      return 'bg-orange-500/20 text-orange-200 ring-orange-500/40'
    case 'low':
      return 'bg-slate-500/20 text-slate-200 ring-slate-500/40'
    default:
      return 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
  }
}

function noteTypeLabel(noteTypeKey: string): string {
  return noteTypeKey === 'corrective_action' ? 'Corrective action' : 'Note'
}

async function fileToBase64(file: File): Promise<string> {
  const result = await new Promise<string>((resolve, reject) => {
    const reader = new FileReader()
    reader.onerror = () => reject(reader.error ?? new Error('Failed to read file.'))
    reader.onload = () => resolve(String(reader.result ?? ''))
    reader.readAsDataURL(file)
  })

  return result.includes(',') ? result.split(',')[1] ?? '' : result
}

export function IncidentsPanel({
  personId,
  personDisplayName,
  incidents,
  reasonCategoryOptions,
  severityOptions,
  selectedIncidentId = null,
  selectedIncident,
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
  onSelectIncident,
  onCreateIncident,
  onRouteToTrainarr,
  onUpdateIncidentStatus,
  onCreateIncidentNote,
  onUpdateIncidentNoteStatus,
  onCreateIncidentAttachment,
  onDownloadIncidentAttachment,
  isRouting = false,
  isUpdatingStatus = false,
  isCreatingIncidentNote = false,
  isUpdatingIncidentNoteStatus = false,
  isCreatingIncidentAttachment = false,
}: IncidentsPanelProps) {
  const [reasonCategoryKey, setReasonCategoryKey] = useState<PersonnelIncidentReasonCategory | ''>('')
  const [severity, setSeverity] = useState<PersonnelIncidentSeverity | ''>('')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [occurredAt, setOccurredAt] = useState(() => new Date().toISOString().slice(0, 16))
  const [noteTypeKey, setNoteTypeKey] = useState<'note' | 'corrective_action'>('note')
  const [noteSubject, setNoteSubject] = useState('')
  const [noteBody, setNoteBody] = useState('')
  const [noteDueAt, setNoteDueAt] = useState('')
  const [attachmentTitle, setAttachmentTitle] = useState('')
  const [attachmentDescription, setAttachmentDescription] = useState('')
  const [attachmentFileName, setAttachmentFileName] = useState('')
  const [attachmentContentType, setAttachmentContentType] = useState('application/octet-stream')
  const [attachmentContentBase64, setAttachmentContentBase64] = useState('')
  const location = useLocation()
  const navigate = useNavigate()
  const isCreateDrawerOpen = location.pathname.startsWith('/incidents/drawer')

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!reasonCategoryKey || !severity) {
      return
    }

    await onCreateIncident({
      personId,
      reasonCategoryKey,
      severity,
      title,
      description,
      occurredAt: new Date(occurredAt).toISOString(),
    })
    setTitle('')
    setDescription('')
  }

  const handleNoteSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!onCreateIncidentNote || !selectedIncident) {
      return
    }

    await onCreateIncidentNote(selectedIncident.incidentId, {
      noteTypeKey,
      subject: noteSubject,
      body: noteBody,
      dueAt: noteDueAt ? new Date(noteDueAt).toISOString() : null,
    })

    setNoteSubject('')
    setNoteBody('')
    setNoteDueAt('')
    setNoteTypeKey('note')
  }

  const handleAttachmentFileChange = async (file: File | null) => {
    if (!file) {
      setAttachmentFileName('')
      setAttachmentContentType('application/octet-stream')
      setAttachmentContentBase64('')
      return
    }

    setAttachmentFileName(file.name)
    setAttachmentContentType(file.type || 'application/octet-stream')
    setAttachmentContentBase64(await fileToBase64(file))
  }

  const handleAttachmentSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!onCreateIncidentAttachment || !selectedIncident) {
      return
    }

    await onCreateIncidentAttachment(selectedIncident.incidentId, {
      title: attachmentTitle,
      fileName: attachmentFileName || attachmentTitle || 'attachment.bin',
      contentType: attachmentContentType,
      contentBase64: attachmentContentBase64,
      description: attachmentDescription || null,
    })

    setAttachmentTitle('')
    setAttachmentDescription('')
    setAttachmentFileName('')
    setAttachmentContentType('application/octet-stream')
    setAttachmentContentBase64('')
  }

  const closeCreateDrawer = () => {
    navigate('/incidents', { replace: true })
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Personnel incidents</h2>
          <p className="mt-1 text-sm text-slate-400">
            Intake records for {personDisplayName}. StaffArr owns incident history for workforce personnel.
          </p>
        </div>
        {canManage ? (
          <div className="flex flex-wrap items-center gap-2">
            <Link
              to="/incidents/drawer"
              className="rounded-lg bg-sky-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-sky-500"
            >
              Create incident
            </Link>
            <span className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-300 ring-1 ring-slate-600">
              staffarr.incidents.manage
            </span>
          </div>
        ) : null}
      </div>

      {actionErrorMessage ? (
        <div className="mt-4">
          <ApiErrorCallout title="Personnel incident action failed" message={actionErrorMessage} />
        </div>
      ) : null}

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading incidents…</p>
      ) : isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Personnel incidents unavailable"
            message={readErrorMessage ?? 'Failed to load personnel incidents.'}
            onRetry={onRetryRead}
            retryLabel="Retry incidents"
          />
        </div>
      ) : incidents.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">No incidents recorded for this person yet.</p>
      ) : (
        <ul className="mt-4 space-y-2">
          {incidents.map((incident) => (
            <li key={incident.incidentId}>
              <button
                type="button"
                onClick={() => onSelectIncident(incident.incidentId)}
                className={`w-full rounded-lg border px-3 py-2 text-left transition ${
                  selectedIncident?.incidentId === incident.incidentId
                    ? 'border-sky-500/60 bg-sky-950/30'
                    : 'border-slate-700 bg-slate-950/40 hover:border-slate-500'
                }`}
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium text-slate-100">{incident.title}</span>
                  <span
                    className={`rounded-full px-2 py-0.5 text-xs ring-1 ${severityBadgeClass(incident.severity)}`}
                  >
                    {incident.severity}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  {formatCategoryLabel(incident.reasonCategoryKey, reasonCategoryOptions)} · {incident.status} · reported{' '}
                  {new Date(incident.reportedAt).toLocaleString()}
                  {incident.trainarrRouting ? (
                    <span className="ml-1 text-violet-300">· routed to TrainArr</span>
                  ) : null}
                </p>
              </button>
            </li>
          ))}
        </ul>
      )}

      {selectedIncidentId ? (
        <div className="mt-4 rounded-lg border border-slate-700 bg-slate-950/50 p-4">
          <h3 className="text-sm font-medium text-slate-200">Incident detail</h3>
          {isLoadingDetail ? (
            <p className="mt-2 text-sm text-slate-400">Loading detail…</p>
          ) : isDetailError ? (
            <div className="mt-2">
              <ApiErrorCallout
                title="Incident detail unavailable"
                message={detailErrorMessage ?? 'Failed to load incident detail.'}
                onRetry={onRetryDetail}
                retryLabel="Retry incident detail"
              />
            </div>
          ) : (
            <>
              {selectedIncident ? (
                <>
                  <p className="mt-2 text-sm text-slate-300">{selectedIncident.description}</p>
                  <dl className="mt-3 grid gap-2 text-xs text-slate-400 sm:grid-cols-2">
                    <div>
                      <dt className="uppercase tracking-wide">Occurred</dt>
                      <dd className="text-slate-200">{new Date(selectedIncident.occurredAt).toLocaleString()}</dd>
                    </div>
                    <div>
                      <dt className="uppercase tracking-wide">Reported</dt>
                      <dd className="text-slate-200">{new Date(selectedIncident.reportedAt).toLocaleString()}</dd>
                    </div>
                    <div>
                      <dt className="uppercase tracking-wide">Status</dt>
                      <dd className="text-slate-200">{selectedIncident.status}</dd>
                    </div>
                    {selectedIncident.trainarrRouting ? (
                      <div className="sm:col-span-2">
                        <dt className="uppercase tracking-wide">TrainArr routing</dt>
                        <dd className="text-violet-200">
                          {selectedIncident.trainarrRouting.routingStatus} · remediation{' '}
                          {selectedIncident.trainarrRouting.trainarrRemediationId.slice(0, 8)}… ·{' '}
                          {new Date(selectedIncident.trainarrRouting.routedAt).toLocaleString()}
                        </dd>
                      </div>
                    ) : null}
                      </dl>
                  {canManage && onUpdateIncidentStatus ? (
                    <div className="mt-4 flex flex-wrap items-center gap-2">
                      <button
                        type="button"
                        disabled={isUpdatingStatus}
                        onClick={() =>
                          void onUpdateIncidentStatus(
                            selectedIncident.incidentId,
                            selectedIncident.status === 'closed' ? 'open' : 'closed',
                          )
                        }
                        className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
                      >
                        {isUpdatingStatus
                          ? 'Updating status…'
                          : selectedIncident.status === 'closed'
                            ? 'Reopen incident'
                            : 'Close incident'}
                      </button>
                      <span className="text-xs text-slate-400">
                        {selectedIncident.status === 'closed'
                          ? 'Closed incidents can be reopened when follow-up is required.'
                          : 'Close the incident when follow-up is complete.'}
                      </span>
                    </div>
                  ) : null}
                  <div className="mt-5 grid gap-4 lg:grid-cols-2">
                    <section className="rounded-md border border-slate-700 bg-slate-900/50 p-3">
                      <div className="flex items-center justify-between gap-2">
                        <h4 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                          Notes and corrective actions
                        </h4>
                        <span className="text-xs text-slate-400">
                          {selectedIncident.notes?.length ?? 0} records
                        </span>
                      </div>
                      {selectedIncident.notes && selectedIncident.notes.length > 0 ? (
                        <ul className="mt-3 space-y-2">
                          {selectedIncident.notes.map((note) => (
                            <li key={note.noteId} className="rounded-md border border-slate-700 bg-slate-950/50 p-3">
                              <div className="flex flex-wrap items-start justify-between gap-2">
                                <div>
                                  <p className="text-sm font-medium text-slate-100">
                                    {noteTypeLabel(note.noteTypeKey)} · {note.subject}
                                  </p>
                                  <p className="mt-1 text-xs text-slate-400">
                                    {note.status}
                                    {note.dueAt ? ` · due ${new Date(note.dueAt).toLocaleString()}` : ''}
                                    {note.completedAt ? ` · completed ${new Date(note.completedAt).toLocaleString()}` : ''}
                                  </p>
                                </div>
                                {note.noteTypeKey === 'corrective_action' && onUpdateIncidentNoteStatus ? (
                                  <button
                                    type="button"
                                    disabled={isUpdatingIncidentNoteStatus}
                                    onClick={() =>
                                      void onUpdateIncidentNoteStatus(selectedIncident.incidentId, note.noteId, {
                                        status: note.status === 'completed' ? 'open' : 'completed',
                                      })
                                    }
                                    className="rounded-md bg-violet-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-violet-500 disabled:opacity-50"
                                  >
                                    {isUpdatingIncidentNoteStatus
                                      ? 'Updating…'
                                      : note.status === 'completed'
                                        ? 'Reopen corrective action'
                                        : 'Mark complete'}
                                  </button>
                                ) : null}
                              </div>
                              <p className="mt-2 whitespace-pre-wrap text-sm text-slate-300">{note.body}</p>
                            </li>
                          ))}
                        </ul>
                      ) : (
                        <p className="mt-3 text-sm text-slate-400">No notes or corrective actions yet.</p>
                      )}

                      {canManage && onCreateIncidentNote ? (
                        <form className="mt-4 space-y-3" onSubmit={handleNoteSubmit}>
                          <div className="grid gap-3 sm:grid-cols-2">
                            <label className="block text-sm text-slate-300">
                              Note type
                              <select
                                value={noteTypeKey}
                                onChange={(event) => setNoteTypeKey(event.target.value as 'note' | 'corrective_action')}
                                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-slate-100"
                              >
                                <option value="note">Note</option>
                                <option value="corrective_action">Corrective action</option>
                              </select>
                            </label>
                            <label className="block text-sm text-slate-300">
                              Due date
                              <input
                                type="datetime-local"
                                value={noteDueAt}
                                onChange={(event) => setNoteDueAt(event.target.value)}
                                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-slate-100"
                              />
                            </label>
                          </div>
                          <label className="block text-sm text-slate-300">
                            Subject
                            <input
                              value={noteSubject}
                              onChange={(event) => setNoteSubject(event.target.value)}
                              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-slate-100"
                            />
                          </label>
                          <label className="block text-sm text-slate-300">
                            Body
                            <textarea
                              rows={4}
                              value={noteBody}
                              onChange={(event) => setNoteBody(event.target.value)}
                              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-slate-100"
                            />
                          </label>
                          <button
                            type="submit"
                            disabled={isCreatingIncidentNote}
                            className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
                          >
                            {isCreatingIncidentNote ? 'Saving note…' : 'Add note'}
                          </button>
                        </form>
                      ) : null}
                    </section>

                    <section className="rounded-md border border-slate-700 bg-slate-900/50 p-3">
                      <div className="flex items-center justify-between gap-2">
                        <h4 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                          Attachments
                        </h4>
                        <span className="text-xs text-slate-400">
                          {selectedIncident.attachments?.length ?? 0} files
                        </span>
                      </div>
                      {selectedIncident.attachments && selectedIncident.attachments.length > 0 ? (
                        <ul className="mt-3 space-y-2">
                          {selectedIncident.attachments.map((attachment) => (
                            <li
                              key={attachment.attachmentId}
                              className="rounded-md border border-slate-700 bg-slate-950/50 p-3"
                            >
                              <div className="flex flex-wrap items-start justify-between gap-2">
                                <div>
                                  <p className="text-sm font-medium text-slate-100">{attachment.title}</p>
                                  <p className="mt-1 text-xs text-slate-400">
                                    {attachment.fileName} · {attachment.contentType} · {Math.round(attachment.sizeBytes / 1024)} KB
                                  </p>
                                </div>
                                {onDownloadIncidentAttachment ? (
                                  <button
                                    type="button"
                                    onClick={() =>
                                      void onDownloadIncidentAttachment(
                                        selectedIncident.incidentId,
                                        attachment.attachmentId,
                                      )
                                    }
                                    className="rounded-md bg-slate-700 px-3 py-1.5 text-xs font-medium text-white hover:bg-slate-600"
                                  >
                                    Download
                                  </button>
                                ) : null}
                              </div>
                              {attachment.description ? (
                                <p className="mt-2 text-sm text-slate-300">{attachment.description}</p>
                              ) : null}
                            </li>
                          ))}
                        </ul>
                      ) : (
                        <p className="mt-3 text-sm text-slate-400">No attachments uploaded yet.</p>
                      )}

                      {canManage && onCreateIncidentAttachment ? (
                        <form className="mt-4 space-y-3" onSubmit={handleAttachmentSubmit}>
                          <label className="block text-sm text-slate-300">
                            Title
                            <input
                              value={attachmentTitle}
                              onChange={(event) => setAttachmentTitle(event.target.value)}
                              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-slate-100"
                            />
                          </label>
                          <label className="block text-sm text-slate-300">
                            Description
                            <textarea
                              rows={3}
                              value={attachmentDescription}
                              onChange={(event) => setAttachmentDescription(event.target.value)}
                              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950/70 px-3 py-2 text-slate-100"
                            />
                          </label>
                          <label className="block text-sm text-slate-300">
                            File
                            <input
                              type="file"
                              onChange={(event) => void handleAttachmentFileChange(event.target.files?.[0] ?? null)}
                              className="mt-1 block w-full text-sm text-slate-300 file:mr-3 file:rounded-lg file:border-0 file:bg-slate-700 file:px-3 file:py-2 file:text-white hover:file:bg-slate-600"
                            />
                          </label>
                          {attachmentFileName ? (
                            <p className="text-xs text-slate-400">
                              Selected {attachmentFileName} · {attachmentContentType}
                            </p>
                          ) : null}
                          <button
                            type="submit"
                            disabled={isCreatingIncidentAttachment}
                            className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
                          >
                            {isCreatingIncidentAttachment ? 'Uploading…' : 'Upload attachment'}
                          </button>
                        </form>
                      ) : null}
                    </section>
                  </div>

                  {selectedIncident.sourceProduct ||
                  selectedIncident.sourceIncidentId ||
                  selectedIncident.sourceEventKind ||
                  selectedIncident.sourceReferenceKey ||
                  selectedIncident.sourceSnapshot ||
                  selectedIncident.relatedAssetReference ||
                  selectedIncident.relatedWorkOrderReference ||
                  selectedIncident.relatedRouteReference ||
                  selectedIncident.relatedSupplierReference ||
                  selectedIncident.relatedDocumentReference ||
                  selectedIncident.relatedPolicyReference ? (
                    <div className="mt-4 rounded-md border border-slate-700 bg-slate-900/50 p-3">
                      <h4 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                        Source references
                      </h4>
                      <dl className="mt-3 grid gap-2 text-xs text-slate-400 sm:grid-cols-2">
                        {selectedIncident.sourceProduct ? (
                          <div>
                            <dt className="uppercase tracking-wide">Source product</dt>
                            <dd className="text-slate-200">{selectedIncident.sourceProduct}</dd>
                          </div>
                        ) : null}
                        {selectedIncident.sourceIncidentId ? (
                          <div>
                            <dt className="uppercase tracking-wide">Source incident</dt>
                            <dd className="text-slate-200">{selectedIncident.sourceIncidentId}</dd>
                          </div>
                        ) : null}
                        {selectedIncident.sourceEventKind ? (
                          <div>
                            <dt className="uppercase tracking-wide">Source event</dt>
                            <dd className="text-slate-200">{selectedIncident.sourceEventKind}</dd>
                          </div>
                        ) : null}
                        {selectedIncident.sourceReferenceKey ? (
                          <div>
                            <dt className="uppercase tracking-wide">Source reference</dt>
                            <dd className="text-slate-200">{selectedIncident.sourceReferenceKey}</dd>
                          </div>
                        ) : null}
                        {selectedIncident.sourceSnapshot ? (
                          <div className="sm:col-span-2">
                            <dt className="uppercase tracking-wide">Source snapshot</dt>
                            <dd className="text-slate-200">
                              {selectedIncident.sourceSnapshot.sourceEntity} · {selectedIncident.sourceSnapshot.sourceId}
                              {' · '}
                              {selectedIncident.sourceSnapshot.labelSnapshot} · {selectedIncident.sourceSnapshot.statusSnapshot}
                            </dd>
                          </div>
                        ) : null}
                        {selectedIncident.relatedAssetReference ? (
                          <div>
                            <dt className="uppercase tracking-wide">Asset reference</dt>
                            <dd className="text-slate-200">{selectedIncident.relatedAssetReference}</dd>
                          </div>
                        ) : null}
                        {selectedIncident.relatedWorkOrderReference ? (
                          <div>
                            <dt className="uppercase tracking-wide">Work order reference</dt>
                            <dd className="text-slate-200">{selectedIncident.relatedWorkOrderReference}</dd>
                          </div>
                        ) : null}
                        {selectedIncident.relatedRouteReference ? (
                          <div>
                            <dt className="uppercase tracking-wide">Route reference</dt>
                            <dd className="text-slate-200">{selectedIncident.relatedRouteReference}</dd>
                          </div>
                        ) : null}
                        {selectedIncident.relatedSupplierReference ? (
                          <div>
                            <dt className="uppercase tracking-wide">Supplier reference</dt>
                            <dd className="text-slate-200">{selectedIncident.relatedSupplierReference}</dd>
                          </div>
                        ) : null}
                        {selectedIncident.relatedDocumentReference ? (
                          <div>
                            <dt className="uppercase tracking-wide">Document reference</dt>
                            <dd className="text-slate-200">{selectedIncident.relatedDocumentReference}</dd>
                          </div>
                        ) : null}
                        {selectedIncident.relatedPolicyReference ? (
                          <div>
                            <dt className="uppercase tracking-wide">Policy reference</dt>
                            <dd className="text-slate-200">{selectedIncident.relatedPolicyReference}</dd>
                          </div>
                        ) : null}
                      </dl>
                    </div>
                  ) : null}
                  {canManage &&
                  onRouteToTrainarr &&
                  isIncidentRoutableToTrainarr(selectedIncident.reasonCategoryKey) &&
                  selectedIncident.status !== 'closed' &&
                  !selectedIncident.trainarrRouting ? (
                    <button
                      type="button"
                      disabled={isRouting}
                      onClick={() => onRouteToTrainarr(selectedIncident.incidentId)}
                      className="mt-4 rounded-lg bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
                    >
                      {isRouting ? 'Routing to TrainArr…' : 'Route to TrainArr for remediation'}
                    </button>
                  ) : null}
                </>
              ) : (
                <p className="mt-2 text-sm text-slate-400">Incident detail is unavailable.</p>
              )}
            </>
          )}
        </div>
      ) : null}

      {canManage && isCreateDrawerOpen ? (
        <div className="fixed inset-0 z-50 bg-slate-950/60 backdrop-blur-sm">
          <button
            type="button"
            aria-label="Close incident drawer"
            className="absolute inset-0 h-full w-full cursor-default"
            onClick={closeCreateDrawer}
          />
          <aside
            role="dialog"
            aria-modal="true"
            aria-labelledby="incident-create-drawer-title"
            className="absolute right-4 top-4 flex h-[calc(100dvh-2rem)] w-full max-w-[44rem] flex-col overflow-hidden rounded-[28px] border border-slate-800 bg-slate-950 shadow-2xl shadow-slate-950/60"
          >
            <div className="flex items-start justify-between gap-4 border-b border-slate-800/80 p-5">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.28em] text-cyan-300">Incidents</p>
                <h2 id="incident-create-drawer-title" className="mt-2 text-2xl font-medium tracking-tight text-white">
                  Record incident intake
                </h2>
                <p className="mt-1 text-sm text-slate-400">
                  Tenant-scoped drawer for incident creation and TrainArr routing.
                </p>
              </div>
              <button
                type="button"
                onClick={closeCreateDrawer}
                className="grid h-9 w-9 place-items-center rounded-full border border-slate-700 text-slate-100 transition hover:border-cyan-400 hover:text-cyan-100"
                aria-label="Close incident drawer"
                title="Close incident drawer"
              >
                ×
              </button>
            </div>

            <form onSubmit={handleSubmit} className="flex-1 space-y-4 overflow-y-auto p-5">
              <div className="grid gap-3 sm:grid-cols-2">
                <label htmlFor="incident-intake-reason-category" className="block text-xs text-slate-400">
                  Reason category
                  <select
                    id="incident-intake-reason-category"
                    value={reasonCategoryKey}
                    onChange={(event) =>
                      setReasonCategoryKey(event.target.value as PersonnelIncidentReasonCategory | '')
                    }
                    className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  >
                    <option value="">Select reason category</option>
                    {reasonCategoryOptions.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </label>
                <label htmlFor="incident-intake-severity" className="block text-xs text-slate-400">
                  Severity
                  <select
                    id="incident-intake-severity"
                    value={severity}
                    onChange={(event) => setSeverity(event.target.value as PersonnelIncidentSeverity | '')}
                    className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  >
                    <option value="">Select severity</option>
                    {severityOptions.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </label>
              </div>
              <label htmlFor="incident-intake-title" className="block text-xs text-slate-400">
                Title
                <input
                  id="incident-intake-title"
                  value={title}
                  onChange={(event) => setTitle(event.target.value)}
                  required
                  minLength={4}
                  className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                />
              </label>
              <label htmlFor="incident-intake-description" className="block text-xs text-slate-400">
                Description
                <textarea
                  id="incident-intake-description"
                  value={description}
                  onChange={(event) => setDescription(event.target.value)}
                  required
                  minLength={16}
                  rows={4}
                  className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                />
              </label>
              <label htmlFor="incident-intake-occurred-at" className="block text-xs text-slate-400">
                Occurred at
                <input
                  id="incident-intake-occurred-at"
                  type="datetime-local"
                  value={occurredAt}
                  onChange={(event) => setOccurredAt(event.target.value)}
                  required
                  className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                />
              </label>
              <div className="flex items-center justify-end gap-2 border-t border-slate-800 pt-4">
                <Link
                  to="/incidents"
                  className="rounded-lg border border-slate-700 px-4 py-2 text-sm font-medium text-slate-100 hover:border-slate-500"
                >
                  Cancel
                </Link>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
                >
                  {isSubmitting ? 'Recording…' : 'Record incident'}
                </button>
              </div>
            </form>
          </aside>
        </div>
      ) : null}
    </section>
  )
}
