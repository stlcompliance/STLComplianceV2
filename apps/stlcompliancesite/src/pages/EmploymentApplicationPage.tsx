import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { CheckCircle2, Loader2 } from 'lucide-react'
import { useParams } from 'react-router-dom'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import { siteConfig } from '../lib/siteConfig'

type EmploymentApplicationField = {
  fieldKey: string
  label: string
  control: 'text' | 'email' | 'phone' | 'textarea' | 'date' | 'select' | 'multi_select' | 'number' | 'yes_no'
  required: boolean
  mappingMode: 'create' | 'eventual' | 'unmapped'
  targetFieldKey: string | null
  helpText: string | null
  placeholder: string | null
  options: Array<{ value: string; label: string }>
}

type PublicEmploymentApplicationResponse = {
  employmentApplicationTemplateId: string
  templateKey: string
  templateName: string
  title: string
  subtitle: string
  submitLabel: string
  version: number
  fields: EmploymentApplicationField[]
  publicLinkExpiresAt: string
  createdAt: string
  updatedAt: string
}

type SubmissionResponse = {
  employmentApplicationSubmissionId: string
  createdPersonId: string | null
  status: string
  applicantDisplayName: string
  applicantEmail: string
  submittedAt: string
  createRequestValues: Record<string, string | null>
  eventualProfileValues: Record<string, string | null>
}

const apiBase = import.meta.env.VITE_STAFFARR_API_BASE ?? 'http://localhost:5102'

async function parseJson<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || `${fallbackMessage} (${response.status})`)
  }
  return (await response.json()) as T
}

export function EmploymentApplicationPage() {
  const { token = '' } = useParams()
  const [application, setApplication] = useState<PublicEmploymentApplicationResponse | null>(null)
  const [answers, setAnswers] = useState<Record<string, string>>({})
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submission, setSubmission] = useState<SubmissionResponse | null>(null)

  useEffect(() => {
    let cancelled = false

    async function load() {
      try {
        setLoading(true)
        setError(null)
        const response = await fetch(`${apiBase}/api/public/employment-applications/${encodeURIComponent(token)}`)
        const data = await parseJson<PublicEmploymentApplicationResponse>(response, 'Failed to load application')
        if (!cancelled) {
          setApplication(data)
          setAnswers(
            data.fields.reduce<Record<string, string>>((accumulator, field) => {
              accumulator[field.fieldKey] = ''
              return accumulator
            }, {}),
          )
        }
      } catch (loadError) {
        if (!cancelled) {
          setError(loadError instanceof Error ? loadError.message : 'Unable to load application.')
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void load()

    return () => {
      cancelled = true
    }
  }, [token])

  const fieldCount = useMemo(() => application?.fields.length ?? 0, [application?.fields.length])

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    if (!application) return

    try {
      setSubmitting(true)
      setError(null)
      const response = await fetch(`${apiBase}/api/public/employment-applications/${encodeURIComponent(token)}/submissions`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ answers }),
      })
      const data = await parseJson<SubmissionResponse>(response, 'Failed to submit application')
      setSubmission(data)
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Unable to submit application.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <>
      <SiteSeo
        title={`Apply — ${siteConfig.siteName}`}
        description="Public employment application for STL Compliance StaffArr."
        path={`/apply/${token}`}
      />
      <PageHero
        eyebrow="Employment application"
        title={application?.title ?? 'Apply to join the team'}
        subtitle={application ? `${application.subtitle} Version ${application.version}` : 'Loading application...'}
      />

      <section className="mx-auto max-w-4xl px-4 pb-16 sm:px-6">
        {loading ? (
          <div className="flex items-center gap-3 rounded-2xl border border-slate-700 bg-slate-900/70 p-6 text-slate-300">
            <Loader2 className="h-5 w-5 animate-spin" />
            Loading application...
          </div>
        ) : error ? (
          <div className="rounded-2xl border border-rose-900/50 bg-rose-950/30 p-6 text-rose-100">
            <h2 className="text-lg font-semibold">We could not load this application</h2>
            <p className="mt-2 text-sm text-rose-100/80">{error}</p>
          </div>
        ) : submission ? (
          <div className="rounded-2xl border border-emerald-500/40 bg-emerald-950/25 p-6 text-emerald-50">
            <div className="flex items-center gap-2 text-lg font-semibold">
              <CheckCircle2 className="h-5 w-5" />
              Application received
            </div>
            <p className="mt-3 text-sm text-emerald-50/80">
              We created the applicant profile in StaffArr and queued any eventual profile values for review.
            </p>
            <dl className="mt-5 grid gap-4 text-sm sm:grid-cols-2">
              <div>
                <dt className="text-emerald-100/60">Applicant</dt>
                <dd className="text-emerald-50">{submission.applicantDisplayName || submission.applicantEmail}</dd>
              </div>
              <div>
                <dt className="text-emerald-100/60">Status</dt>
                <dd className="text-emerald-50">{submission.status.replaceAll('_', ' ')}</dd>
              </div>
              <div>
                <dt className="text-emerald-100/60">Template</dt>
                <dd className="text-emerald-50">
                  {application?.templateName ?? 'Employment application'} v{application?.version ?? ''}
                </dd>
              </div>
            </dl>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-5 rounded-2xl border border-slate-700 bg-slate-900/70 p-6">
            <div className="grid gap-4 md:grid-cols-2">
              {application?.fields.map((field) => (
                <FieldInput
                  key={field.fieldKey}
                  field={field}
                  value={answers[field.fieldKey] ?? ''}
                  onChange={(value) => setAnswers((current) => ({ ...current, [field.fieldKey]: value }))}
                />
              ))}
            </div>
            <button
              type="submit"
              disabled={submitting}
              className="w-full rounded-xl bg-teal-600 px-5 py-3 text-sm font-semibold text-white hover:bg-teal-500 disabled:opacity-50"
            >
              {submitting ? 'Submitting...' : application?.submitLabel ?? 'Submit application'}
            </button>
            <p className="text-xs text-[var(--color-text-muted)]">
              This application currently includes {fieldCount} field{fieldCount === 1 ? '' : 's'} and expires{' '}
              {application ? new Date(application.publicLinkExpiresAt).toLocaleString() : 'when the link expires'}.
            </p>
          </form>
        )}
      </section>
    </>
  )
}

function FieldInput({
  field,
  value,
  onChange,
}: {
  field: EmploymentApplicationField
  value: string
  onChange: (value: string) => void
}) {
  const baseClassName = 'mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white'
  const selectedValues = field.control === 'multi_select' ? value.split(',').map((entry) => entry.trim()).filter(Boolean) : []

  return (
    <label className="block text-sm font-medium text-slate-200 md:col-span-1">
      {field.label}
      {field.control === 'textarea' ? (
        <textarea
          required={field.required}
          rows={4}
          value={value}
          placeholder={field.placeholder ?? undefined}
          onChange={(event) => onChange(event.target.value)}
          className={baseClassName}
        />
      ) : field.control === 'select' ? (
        <select
          required={field.required}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          className={baseClassName}
        >
          <option value="">Select one</option>
          {field.options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      ) : field.control === 'multi_select' ? (
        <select
          required={field.required}
          multiple
          value={selectedValues}
          onChange={(event) => {
            const chosen = Array.from(event.target.selectedOptions).map((option) => option.value)
            onChange(chosen.join(','))
          }}
          className={`${baseClassName} min-h-32`}
        >
          {field.options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      ) : field.control === 'yes_no' ? (
        <input
          type="checkbox"
          checked={value === 'true'}
          onChange={(event) => onChange(event.target.checked ? 'true' : '')}
          className="mt-3 h-4 w-4 rounded border-slate-500 bg-slate-950 text-teal-500"
        />
      ) : (
        <input
          required={field.required}
          type={field.control}
          value={value}
          placeholder={field.placeholder ?? undefined}
          onChange={(event) => onChange(event.target.value)}
          className={baseClassName}
        />
      )}
      {field.helpText ? <span className="mt-1 block text-xs font-normal text-[var(--color-text-muted)]">{field.helpText}</span> : null}
    </label>
  )
}
