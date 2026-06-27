import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { ApiErrorCallout } from '@stl/shared-ui'
import type {
  MePortalSummaryResponse,
  PersonnelIncidentSummaryResponse,
  PersonnelUpdateRequestResponse,
  SubmitPersonnelUpdateRequest,
  SubmitSelfReportedPersonnelIncidentRequest,
} from '../api/types'

interface MeSelfServicePortalPanelProps {
  summary: MePortalSummaryResponse | null
  updateRequests: PersonnelUpdateRequestResponse[]
  incidentReports: PersonnelIncidentSummaryResponse[]
  isLoading: boolean
  isSubmittingUpdate: boolean
  isSubmittingIncident: boolean
  errorMessage: string | null
  onSubmitUpdateRequest: (request: SubmitPersonnelUpdateRequest) => Promise<void>
  onSubmitIncidentReport: (request: SubmitSelfReportedPersonnelIncidentRequest) => Promise<void>
}

function readinessLabel(status: string): string {
  return status === 'ready' ? 'Ready' : 'Not ready'
}

function readinessClass(status: string): string {
  return status === 'ready'
    ? 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    : 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
}

function requestTypeLabel(requestType: string): string {
  switch (requestType) {
    case 'phone_update':
      return 'Phone update'
    case 'contact_info_update':
      return 'Contact info update'
    case 'profile_correction':
      return 'Profile correction'
    default:
      return 'Other'
  }
}

function incidentCategoryLabel(category: string): string {
  switch (category) {
    case 'safety':
      return 'Safety'
    case 'conduct':
      return 'Conduct'
    case 'injury':
      return 'Injury'
    case 'equipment':
      return 'Equipment'
    case 'training_compliance':
      return 'Training / compliance'
    case 'policy':
      return 'Policy'
    default:
      return 'Other'
  }
}

type UpdateFieldPolicy = 'direct' | 'review' | 'restricted'

type UpdateFieldOption = {
  value: string
  label: string
  policy: UpdateFieldPolicy
  guidance: string
}

function updateFieldPolicyLabel(policy: UpdateFieldPolicy): string {
  switch (policy) {
    case 'direct':
      return 'Directly editable after approval'
    case 'review':
      return 'Review required'
    case 'restricted':
    default:
      return 'Restricted / HR-only'
  }
}

function updateFieldPolicyClass(policy: UpdateFieldPolicy): string {
  switch (policy) {
    case 'direct':
      return 'border-emerald-800 bg-emerald-950/20 text-emerald-200'
    case 'review':
      return 'border-amber-800 bg-amber-950/20 text-amber-100'
    case 'restricted':
    default:
      return 'border-rose-800 bg-rose-950/20 text-rose-100'
  }
}

function defaultOccurredAtLocalValue(): string {
  const now = new Date()
  now.setMinutes(now.getMinutes() - now.getTimezoneOffset())
  return now.toISOString().slice(0, 16)
}

const UPDATE_FIELD_OPTIONS_BY_REQUEST_TYPE: Record<string, UpdateFieldOption[]> = {
  phone_update: [
    {
      value: 'work_phone',
      label: 'Work phone',
      policy: 'direct',
      guidance: 'Can be applied to the profile after approval and is visible in the HR audit trail.',
    },
    {
      value: 'mobile_phone',
      label: 'Mobile phone',
      policy: 'review',
      guidance: 'Needs HR review before the profile is updated.',
    },
    {
      value: 'emergency_contact_phone',
      label: 'Emergency contact phone',
      policy: 'review',
      guidance: 'Handled as a review-required contact update.',
    },
  ],
  contact_info_update: [
    {
      value: 'primary_email',
      label: 'Primary email',
      policy: 'direct',
      guidance: 'Can also sync to NexArr login/contact records when approved.',
    },
    {
      value: 'work_phone',
      label: 'Work phone',
      policy: 'direct',
      guidance: 'Can be applied to the workforce profile after approval.',
    },
    {
      value: 'mailing_address',
      label: 'Mailing address',
      policy: 'review',
      guidance: 'Address changes can affect downstream systems and require review.',
    },
    {
      value: 'emergency_contact_name',
      label: 'Emergency contact name',
      policy: 'review',
      guidance: 'Requires HR review because it affects emergency contacts.',
    },
    {
      value: 'emergency_contact_phone',
      label: 'Emergency contact phone',
      policy: 'review',
      guidance: 'Requires HR review because it affects emergency contacts.',
    },
  ],
  profile_correction: [
    {
      value: 'given_name',
      label: 'Given name',
      policy: 'direct',
      guidance: 'Can be applied to the workforce profile after approval.',
    },
    {
      value: 'family_name',
      label: 'Family name',
      policy: 'direct',
      guidance: 'Can be applied to the workforce profile after approval.',
    },
    {
      value: 'job_title',
      label: 'Job title',
      policy: 'direct',
      guidance: 'Can be applied to the profile after review and approval.',
    },
    {
      value: 'manager_person',
      label: 'Manager assignment',
      policy: 'review',
      guidance: 'Manager changes are review-required because they affect reporting.',
    },
    {
      value: 'primary_org_unit',
      label: 'Primary org assignment',
      policy: 'review',
      guidance: 'Org assignment changes are review-required because they affect access and routing.',
    },
  ],
  other: [
    {
      value: 'other',
      label: 'Other profile field',
      policy: 'restricted',
      guidance: 'Use HR review for fields not offered directly in self-service.',
    },
  ],
}

export function MeSelfServicePortalPanel({
  summary,
  updateRequests,
  incidentReports,
  isLoading,
  isSubmittingUpdate,
  isSubmittingIncident,
  errorMessage,
  onSubmitUpdateRequest,
  onSubmitIncidentReport,
}: MeSelfServicePortalPanelProps) {
  const [requestType, setRequestType] = useState('phone_update')
  const [fieldKey, setFieldKey] = useState('work_phone')
  const [currentValue, setCurrentValue] = useState('')
  const [requestedValue, setRequestedValue] = useState('')
  const [details, setDetails] = useState('')
  const [incidentCategory, setIncidentCategory] =
    useState<SubmitSelfReportedPersonnelIncidentRequest['reasonCategoryKey']>('safety')
  const [incidentSeverity, setIncidentSeverity] =
    useState<SubmitSelfReportedPersonnelIncidentRequest['severity']>('medium')
  const [incidentTitle, setIncidentTitle] = useState('')
  const [incidentDescription, setIncidentDescription] = useState('')
  const [incidentOccurredAt, setIncidentOccurredAt] = useState(defaultOccurredAtLocalValue())
  const updateFieldOptions = useMemo(
    () => UPDATE_FIELD_OPTIONS_BY_REQUEST_TYPE[requestType] ?? UPDATE_FIELD_OPTIONS_BY_REQUEST_TYPE.other,
    [requestType],
  )
  const selectedFieldOption =
    updateFieldOptions.find((option) => option.value === fieldKey) ?? updateFieldOptions[0]

  useEffect(() => {
    if (updateFieldOptions.some((option) => option.value === fieldKey)) {
      return
    }

    setFieldKey(updateFieldOptions[0]?.value ?? 'other')
  }, [fieldKey, updateFieldOptions])

  if (isLoading) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <p className="text-sm text-slate-400">Loading your workforce profile…</p>
      </section>
    )
  }

  if (!summary) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <ApiErrorCallout
          title="Self-service profile unavailable"
          message="Could not load your workforce profile details."
        />
      </section>
    )
  }

  const { profile, readiness, certifications, permissions, onboarding } = summary
  const primaryAssignment = profile.placement.activeAssignments[0]
  const directFields = updateFieldOptions.filter((option) => option.policy === 'direct')
  const reviewFields = updateFieldOptions.filter((option) => option.policy === 'review')
  const restrictedFields = [
    'Payroll, benefits, legal, medical, and other highly sensitive HR records',
  ]

  const handleSubmitUpdate = async (event: FormEvent) => {
    event.preventDefault()
    await onSubmitUpdateRequest({
      requestType,
      fieldKey,
      currentValue: currentValue.trim() || null,
      requestedValue: requestedValue.trim(),
      details: details.trim() || null,
    })
    setRequestedValue('')
    setDetails('')
  }

  const handleSubmitIncident = async (event: FormEvent) => {
    event.preventDefault()
    await onSubmitIncidentReport({
      reasonCategoryKey: incidentCategory,
      severity: incidentSeverity,
      title: incidentTitle.trim(),
      description: incidentDescription.trim(),
      occurredAt: new Date(incidentOccurredAt).toISOString(),
    })
    setIncidentTitle('')
    setIncidentDescription('')
    setIncidentOccurredAt(defaultOccurredAtLocalValue())
  }

  return (
    <div className="space-y-6" data-testid="me-self-service-portal">
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <header className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-lg font-semibold text-slate-100">{profile.displayName}</h1>
            <p className="mt-1 text-sm text-slate-400">{profile.primaryEmail}</p>
            <p className="mt-1 text-xs text-[var(--color-text-muted)]">
              Person ID {profile.personId} · {profile.employmentStatus}
              {profile.jobTitle ? ` · ${profile.jobTitle}` : ''}
            </p>
          </div>
          <span
            className={`inline-flex rounded-full px-3 py-1 text-xs font-medium ring-1 ${readinessClass(readiness.readinessStatus)}`}
          >
            {readinessLabel(readiness.readinessStatus)}
          </span>
        </header>

        <dl className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <div>
            <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Site / department / team</dt>
            <dd className="mt-1 text-sm text-slate-200">
              {primaryAssignment?.assignmentPath ??
                profile.placement.primaryOrgUnitName ??
                'No active assignment'}
            </dd>
          </div>
          <div>
            <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Position</dt>
            <dd className="mt-1 text-sm text-slate-200">
              {primaryAssignment
                ? profile.placement.activeAssignments[0].positionName
                : (profile.jobTitle ?? '—')}
            </dd>
          </div>
          <div>
            <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Manager</dt>
            <dd className="mt-1 text-sm text-slate-200">
              {profile.placement.managerDisplayName ?? '—'}
            </dd>
          </div>
          {profile.workPhone ? (
            <div>
              <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Work phone</dt>
              <dd className="mt-1 text-sm text-slate-200">{profile.workPhone}</dd>
            </div>
          ) : null}
          <div>
            <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Product access</dt>
            <dd className="mt-1 text-sm text-slate-200">
              {summary.productAccess.length > 0
                ? summary.productAccess.join(', ')
                : summary.session.productKey}
            </dd>
          </div>
          <div>
            <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Certifications</dt>
            <dd className="mt-1 text-sm text-slate-200">
              {certifications.activeCount} active
              {certifications.expiringSoonCount > 0
                ? ` · ${certifications.expiringSoonCount} expiring soon`
                : ''}
              {certifications.missingRequirementCount > 0
                ? ` · ${certifications.missingRequirementCount} gaps`
                : ''}
            </dd>
          </div>
          <div>
            <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Permissions</dt>
            <dd className="mt-1 text-sm text-slate-200">{permissions.permissionCount} assigned</dd>
          </div>
        </dl>
      </section>

      {readiness.blockerMessages.length > 0 ? (
        <section className="rounded-xl border border-amber-700/50 bg-amber-950/30 p-6">
          <h2 className="text-sm font-medium text-amber-200">Readiness notes</h2>
          <p className="mt-1 text-xs text-amber-100/80">{readiness.readinessBasis}</p>
          <ul className="mt-3 list-disc space-y-1 pl-5 text-sm text-amber-100">
            {readiness.blockerMessages.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        </section>
      ) : null}

      {onboarding ? (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
          <h2 className="text-sm font-medium text-slate-300">Onboarding checklist</h2>
          <p className="mt-1 text-sm text-slate-400">
            {onboarding.completedSteps} of {onboarding.totalSteps} steps complete · status{' '}
            {onboarding.overallStatus}
            {onboarding.blockedSteps > 0 ? ` · ${onboarding.blockedSteps} blocked` : ''}
          </p>
        </section>
      ) : null}

      {permissions.permissionSummaries.length > 0 ? (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
          <h2 className="text-sm font-medium text-slate-300">Permissions summary</h2>
          <ul className="mt-3 space-y-2 text-sm text-slate-300">
            {permissions.permissionSummaries.map((line) => (
              <li key={line} className="rounded-lg bg-slate-950/50 px-3 py-2">
                {line}
              </li>
            ))}
          </ul>
          {permissions.permissionCount > permissions.permissionSummaries.length ? (
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">
              Showing {permissions.permissionSummaries.length} of {permissions.permissionCount}{' '}
              permissions.
            </p>
          ) : null}
        </section>
      ) : null}

      {certifications.highlights.length > 0 ? (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
          <h2 className="text-sm font-medium text-slate-300">Certifications</h2>
          <ul className="mt-3 divide-y divide-slate-800 text-sm">
            {certifications.highlights.map((cert) => (
              <li key={cert.personCertificationId} className="flex justify-between gap-4 py-2">
                <span className="text-slate-200">{cert.certificationName}</span>
                <span className="text-slate-400">
                  {cert.effectiveStatus}
                  {cert.expiresAt ? ` · expires ${new Date(cert.expiresAt).toLocaleDateString()}` : ''}
                </span>
              </li>
            ))}
          </ul>
        </section>
      ) : null}

      {summary.directReportCount > 0 ? (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
          <h2 className="text-sm font-medium text-slate-300">Direct reports</h2>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">{summary.directReportCount} people report to you.</p>
          <ul className="mt-3 space-y-2 text-sm text-slate-300">
            {summary.directReportsPreview.map((report) => (
              <li key={report.personId} className="rounded-lg bg-slate-950/50 px-3 py-2">
                {report.displayName} · {report.employmentStatus}
              </li>
            ))}
          </ul>
        </section>
      ) : null}

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <header>
          <h2 className="text-sm font-medium text-slate-300">Request a profile update</h2>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Submit changes for HR review. Updates are not applied until approved.
          </p>
        </header>

        <div
          className="mt-4 rounded-xl border border-slate-800 bg-slate-950/40 p-4"
          data-testid="field-review-guidance"
        >
          <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-300">
            Field review guidance
          </h3>
          <p className="mt-2 text-xs leading-5 text-slate-400">
            Directly editable fields can be applied to your profile after approval. Review-required fields stay in the HR queue until a reviewer applies them. Restricted fields are not exposed here and must be handled by HR.
          </p>
          <div className="mt-4 grid gap-3 md:grid-cols-3">
            <div className="rounded-lg border border-emerald-800 bg-emerald-950/20 p-3">
              <p className="text-xs font-semibold uppercase tracking-wide text-emerald-200">
                Directly editable after approval
              </p>
              <ul className="mt-2 space-y-1 text-sm text-emerald-100">
                {directFields.length > 0 ? directFields.map((option) => (
                  <li key={option.value}>{option.label}</li>
                )) : (
                  <li>None for the selected request type</li>
                )}
              </ul>
            </div>
            <div className="rounded-lg border border-amber-800 bg-amber-950/20 p-3">
              <p className="text-xs font-semibold uppercase tracking-wide text-amber-200">
                Review required
              </p>
              <ul className="mt-2 space-y-1 text-sm text-amber-100">
                {reviewFields.length > 0 ? reviewFields.map((option) => (
                  <li key={option.value}>{option.label}</li>
                )) : (
                  <li>None for the selected request type</li>
                )}
              </ul>
            </div>
            <div className="rounded-lg border border-rose-800 bg-rose-950/20 p-3">
              <p className="text-xs font-semibold uppercase tracking-wide text-rose-200">
                Restricted
              </p>
              <ul className="mt-2 space-y-1 text-sm text-rose-100">
                {restrictedFields.map((field) => (
                  <li key={field}>{field}</li>
                ))}
              </ul>
            </div>
          </div>
          {selectedFieldOption ? (
            <>
              <p className="mt-3 text-xs text-slate-400">
                Selected field:{' '}
                <span
                  className={`inline-flex rounded-full border px-2 py-0.5 font-medium ${updateFieldPolicyClass(selectedFieldOption.policy)}`}
                >
                  {selectedFieldOption.label}
                </span>{' '}
                <span className="ml-2">{updateFieldPolicyLabel(selectedFieldOption.policy)}</span>
              </p>
              <p className="mt-2 text-xs text-slate-400">{selectedFieldOption.guidance}</p>
            </>
          ) : null}
        </div>

        <form className="mt-4 grid gap-4 sm:grid-cols-2" onSubmit={handleSubmitUpdate}>
          <label className="block text-sm">
            <span className="text-slate-400">Request type</span>
            <select
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
              value={requestType}
              onChange={(event) => setRequestType(event.target.value)}
              data-testid="me-update-request-type"
            >
              <option value="phone_update">Phone update</option>
              <option value="contact_info_update">Contact info update</option>
              <option value="profile_correction">Profile correction</option>
              <option value="other">Other</option>
            </select>
          </label>
          <label className="block text-sm">
            <span className="text-slate-400">Field</span>
            <select
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
              value={fieldKey}
              onChange={(event) => setFieldKey(event.target.value)}
              data-testid="me-update-field-key"
            >
              {updateFieldOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm sm:col-span-2">
            <span className="text-slate-400">Current value (optional)</span>
            <input
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
              value={currentValue}
              onChange={(event) => setCurrentValue(event.target.value)}
              data-testid="me-update-current-value"
            />
          </label>
          <label className="block text-sm sm:col-span-2">
            <span className="text-slate-400">Requested value</span>
            <input
              required
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
              value={requestedValue}
              onChange={(event) => setRequestedValue(event.target.value)}
              data-testid="me-update-requested-value"
            />
          </label>
          <label className="block text-sm sm:col-span-2">
            <span className="text-slate-400">Details (optional)</span>
            <textarea
              rows={3}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
              value={details}
              onChange={(event) => setDetails(event.target.value)}
              data-testid="me-update-details"
            />
          </label>
          <p className="sm:col-span-2 text-xs leading-5 text-slate-400">
            If the selected field is eligible for automatic profile apply, a reviewer can apply the change directly to your profile after approval. Otherwise, the request stays review-only and may need an HR correction workflow.
          </p>
          <div className="sm:col-span-2">
            <button
              type="submit"
              disabled={isSubmittingUpdate}
              className="rounded-lg bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
              data-testid="me-update-submit"
            >
              {isSubmittingUpdate ? 'Submitting…' : 'Submit request'}
            </button>
          </div>
        </form>

        {errorMessage ? (
          <div className="mt-3">
            <ApiErrorCallout title="Request submission failed" message={errorMessage} />
          </div>
        ) : null}

        {updateRequests.length > 0 ? (
          <div className="mt-6">
            <h3 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
              Your recent requests
            </h3>
            <ul className="mt-2 divide-y divide-slate-800 text-sm">
              {updateRequests.map((request) => (
                <li key={request.requestId} className="py-2">
                  <span className="text-slate-200">{requestTypeLabel(request.requestType)}</span>
                  <span className="text-[var(--color-text-muted)]">
                    {' '}
                    · {request.fieldKey} → {request.requestedValue} · {request.status} ·{' '}
                    {new Date(request.submittedAt).toLocaleString()}
                  </span>
                </li>
              ))}
            </ul>
          </div>
        ) : null}
      </section>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <header>
          <h2 className="text-sm font-medium text-slate-300">Report an incident or concern</h2>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Submit a workforce incident for HR review. Reports about yourself are routed into the
            personnel incident queue.
          </p>
        </header>

        <form className="mt-4 grid gap-4 sm:grid-cols-2" onSubmit={handleSubmitIncident}>
          <label className="block text-sm">
            <span className="text-slate-400">Category</span>
            <select
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
              value={incidentCategory}
              onChange={(event) =>
                setIncidentCategory(
                  event.target.value as SubmitSelfReportedPersonnelIncidentRequest['reasonCategoryKey'],
                )
              }
              data-testid="me-incident-category"
            >
              <option value="safety">Safety</option>
              <option value="conduct">Conduct</option>
              <option value="injury">Injury</option>
              <option value="equipment">Equipment</option>
              <option value="training_compliance">Training / compliance</option>
              <option value="policy">Policy</option>
              <option value="other">Other</option>
            </select>
          </label>
          <label className="block text-sm">
            <span className="text-slate-400">Severity</span>
            <select
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
              value={incidentSeverity}
              onChange={(event) =>
                setIncidentSeverity(
                  event.target.value as SubmitSelfReportedPersonnelIncidentRequest['severity'],
                )
              }
              data-testid="me-incident-severity"
            >
              <option value="low">Low</option>
              <option value="medium">Medium</option>
              <option value="high">High</option>
              <option value="critical">Critical</option>
            </select>
          </label>
          <label className="block text-sm sm:col-span-2">
            <span className="text-slate-400">When did it occur?</span>
            <input
              required
              type="datetime-local"
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
              value={incidentOccurredAt}
              onChange={(event) => setIncidentOccurredAt(event.target.value)}
              data-testid="me-incident-occurred-at"
            />
          </label>
          <label className="block text-sm sm:col-span-2">
            <span className="text-slate-400">Title</span>
            <input
              required
              minLength={4}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
              value={incidentTitle}
              onChange={(event) => setIncidentTitle(event.target.value)}
              data-testid="me-incident-title"
            />
          </label>
          <label className="block text-sm sm:col-span-2">
            <span className="text-slate-400">Description</span>
            <textarea
              required
              minLength={16}
              rows={4}
              className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
              value={incidentDescription}
              onChange={(event) => setIncidentDescription(event.target.value)}
              data-testid="me-incident-description"
            />
          </label>
          <div className="sm:col-span-2">
            <button
              type="submit"
              disabled={isSubmittingIncident}
              className="rounded-lg bg-rose-700 px-4 py-2 text-sm font-medium text-white hover:bg-rose-600 disabled:opacity-50"
              data-testid="me-incident-submit"
            >
              {isSubmittingIncident ? 'Submitting…' : 'Submit report'}
            </button>
          </div>
        </form>

        {incidentReports.length > 0 ? (
          <div className="mt-6">
            <h3 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
              Your incident reports
            </h3>
            <ul className="mt-2 divide-y divide-slate-800 text-sm">
              {incidentReports.map((incident) => (
                <li key={incident.incidentId} className="py-2">
                  <span className="text-slate-200">{incident.title}</span>
                  <span className="text-[var(--color-text-muted)]">
                    {' '}
                    · {incidentCategoryLabel(incident.reasonCategoryKey)} · {incident.severity} ·{' '}
                    {incident.status} · {new Date(incident.reportedAt).toLocaleString()}
                  </span>
                </li>
              ))}
            </ul>
          </div>
        ) : null}
      </section>
    </div>
  )
}
