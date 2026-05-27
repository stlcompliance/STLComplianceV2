import { type FormEvent, useState } from 'react'
import type {
  CreatePersonnelIncidentRequest,
  PersonnelIncidentDetailResponse,
  PersonnelIncidentReasonCategory,
  PersonnelIncidentSeverity,
  PersonnelIncidentSummaryResponse,
} from '../api/types'

interface IncidentsPanelProps {
  personId: string
  personDisplayName: string
  incidents: PersonnelIncidentSummaryResponse[]
  selectedIncident: PersonnelIncidentDetailResponse | null
  isLoading: boolean
  isLoadingDetail: boolean
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
  onSelectIncident: (incidentId: string) => void
  onCreateIncident: (request: CreatePersonnelIncidentRequest) => Promise<void>
  onRouteToTrainarr?: (incidentId: string) => Promise<void>
  isRouting?: boolean
}

export function isIncidentRoutableToTrainarr(reasonCategoryKey: string): boolean {
  return reasonCategoryKey === 'training_compliance'
}

const reasonCategoryOptions: { value: PersonnelIncidentReasonCategory; label: string }[] = [
  { value: 'safety', label: 'Safety' },
  { value: 'conduct', label: 'Conduct' },
  { value: 'injury', label: 'Injury' },
  { value: 'equipment', label: 'Equipment' },
  { value: 'training_compliance', label: 'Training compliance' },
  { value: 'policy', label: 'Policy' },
  { value: 'other', label: 'Other' },
]

const severityOptions: { value: PersonnelIncidentSeverity; label: string }[] = [
  { value: 'low', label: 'Low' },
  { value: 'medium', label: 'Medium' },
  { value: 'high', label: 'High' },
  { value: 'critical', label: 'Critical' },
]

export function canManageIncidents(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) {
    return true
  }

  return ['tenant_admin', 'staffarr_admin', 'hr_admin'].includes(tenantRoleKey)
}

function formatCategoryLabel(key: string): string {
  const match = reasonCategoryOptions.find((option) => option.value === key)
  return match?.label ?? key
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

export function IncidentsPanel({
  personId,
  personDisplayName,
  incidents,
  selectedIncident,
  isLoading,
  isLoadingDetail,
  canManage,
  isSubmitting,
  errorMessage,
  onSelectIncident,
  onCreateIncident,
  onRouteToTrainarr,
  isRouting = false,
}: IncidentsPanelProps) {
  const [reasonCategoryKey, setReasonCategoryKey] = useState<PersonnelIncidentReasonCategory>('safety')
  const [severity, setSeverity] = useState<PersonnelIncidentSeverity>('medium')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [occurredAt, setOccurredAt] = useState(() => new Date().toISOString().slice(0, 16))

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
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
          <span className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-300 ring-1 ring-slate-600">
            staffarr.incidents.manage
          </span>
        ) : null}
      </div>

      {errorMessage ? (
        <p className="mt-4 rounded-lg border border-rose-500/40 bg-rose-950/40 px-3 py-2 text-sm text-rose-200">
          {errorMessage}
        </p>
      ) : null}

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading incidents…</p>
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
                  {formatCategoryLabel(incident.reasonCategoryKey)} · {incident.status} · reported{' '}
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

      {selectedIncident ? (
        <div className="mt-4 rounded-lg border border-slate-700 bg-slate-950/50 p-4">
          <h3 className="text-sm font-medium text-slate-200">Incident detail</h3>
          {isLoadingDetail ? (
            <p className="mt-2 text-sm text-slate-400">Loading detail…</p>
          ) : (
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
              {canManage &&
              onRouteToTrainarr &&
              isIncidentRoutableToTrainarr(selectedIncident.reasonCategoryKey) &&
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
          )}
        </div>
      ) : null}

      {canManage ? (
        <form onSubmit={handleSubmit} className="mt-6 space-y-3 border-t border-slate-700 pt-4">
          <h3 className="text-sm font-medium text-slate-200">Record incident intake</h3>
          <div className="grid gap-3 sm:grid-cols-2">
            <label className="block text-xs text-slate-400">
              Reason category
              <select
                value={reasonCategoryKey}
                onChange={(event) =>
                  setReasonCategoryKey(event.target.value as PersonnelIncidentReasonCategory)
                }
                className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              >
                {reasonCategoryOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
            <label className="block text-xs text-slate-400">
              Severity
              <select
                value={severity}
                onChange={(event) => setSeverity(event.target.value as PersonnelIncidentSeverity)}
                className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              >
                {severityOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
          </div>
          <label className="block text-xs text-slate-400">
            Title
            <input
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              required
              minLength={4}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-xs text-slate-400">
            Description
            <textarea
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              required
              minLength={16}
              rows={4}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-xs text-slate-400">
            Occurred at
            <input
              type="datetime-local"
              value={occurredAt}
              onChange={(event) => setOccurredAt(event.target.value)}
              required
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            {isSubmitting ? 'Recording…' : 'Record incident'}
          </button>
        </form>
      ) : null}
    </section>
  )
}
