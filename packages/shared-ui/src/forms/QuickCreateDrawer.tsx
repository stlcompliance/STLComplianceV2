import { AlertTriangle, Loader2, Plus, X } from 'lucide-react'
import { type FormEvent, useEffect, useMemo, useState } from 'react'

import type {
  DuplicateCandidateResponse,
  QuickCreateFieldDescriptor,
  QuickCreateResponse,
  QuickCreateSchemaResponse,
} from './referenceTypes'

export type QuickCreateDrawerProps = {
  open: boolean
  schema?: QuickCreateSchemaResponse | null
  initialValues?: Record<string, string>
  onClose: () => void
  onCreate: (values: Record<string, string>) => Promise<QuickCreateResponse>
  onCreated?: (response: QuickCreateResponse) => void
  testId?: string
}

export function QuickCreateDrawer({
  open,
  schema,
  initialValues,
  onClose,
  onCreate,
  onCreated,
  testId,
}: QuickCreateDrawerProps) {
  const fields = useMemo(() => schema?.fields ?? [], [schema?.fields])
  const [values, setValues] = useState<Record<string, string>>({})
  const [duplicates, setDuplicates] = useState<DuplicateCandidateResponse[]>([])
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    if (!open) {
      return
    }

    setValues(buildInitialValues(fields, initialValues))
    setDuplicates([])
    setError(null)
  }, [fields, initialValues, open])

  if (!open) {
    return null
  }

  const allowed = schema?.allowed === true
  const title = schema ? `Create ${schema.referenceType}` : 'Quick create'

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!allowed) {
      return
    }

    setIsSubmitting(true)
    setError(null)
    setDuplicates([])

    try {
      const response = await onCreate(values)
      if (response.duplicateCandidates.length > 0 && !response.created) {
        setDuplicates(response.duplicateCandidates)
        setError(response.message ?? 'Possible duplicates need review before creating this record.')
        return
      }

      onCreated?.(response)
      onClose()
    } catch (err) {
      console.error('Quick create failed', err)
      setError('Quick create is temporarily unavailable. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="fixed inset-0 z-[70] flex justify-end bg-slate-950/70" data-testid={testId}>
      <button
        type="button"
        aria-label="Close quick create"
        className="absolute inset-0 cursor-default"
        onClick={onClose}
      />
      <aside className="relative flex h-full w-full max-w-md flex-col border-l border-slate-700 bg-slate-950 shadow-2xl shadow-slate-950">
        <header className="flex items-start justify-between gap-3 border-b border-slate-800 px-5 py-4">
          <div>
            <p className="text-sm font-semibold text-slate-100">{title}</p>
            <p className="mt-1 text-xs text-slate-400">
              {schema ? `Managed by ${schema.managedByLabel}` : 'Loading schema'}
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md p-1.5 text-slate-400 hover:bg-slate-900 hover:text-slate-100"
            aria-label="Close"
          >
            <X className="h-4 w-4" aria-hidden />
          </button>
        </header>

        <form className="flex min-h-0 flex-1 flex-col" onSubmit={submit}>
          <div className="min-h-0 flex-1 space-y-4 overflow-y-auto px-5 py-4">
            {!schema ? (
              <p className="text-sm text-slate-400">Loading quick-create settings...</p>
            ) : null}

            {schema && !allowed ? (
              <div className="rounded-lg border border-amber-500/40 bg-amber-500/10 px-3 py-3 text-sm text-amber-100">
                <div className="flex gap-2">
                  <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" aria-hidden />
                  <p>{schema.disabledReason ?? 'Quick create is not available for this reference type.'}</p>
                </div>
              </div>
            ) : null}

            {fields.map((field) => (
              <QuickCreateField
                key={field.key}
                field={field}
                value={values[field.key] ?? ''}
                disabled={!allowed || isSubmitting}
                onChange={(value) => setValues((current) => ({ ...current, [field.key]: value }))}
              />
            ))}

            {duplicates.length > 0 ? (
              <div className="rounded-lg border border-amber-500/40 bg-amber-500/10 px-3 py-3">
                <p className="text-sm font-medium text-amber-100">Possible duplicates</p>
                <ul className="mt-2 space-y-2">
                  {duplicates.map((candidate) => (
                    <li key={candidate.referenceId} className="text-sm text-amber-50">
                      <span className="font-medium">{candidate.displayLabel}</span>
                      {candidate.secondaryLabel ? (
                        <span className="text-amber-100/80"> - {candidate.secondaryLabel}</span>
                      ) : null}
                      {candidate.matchReason ? (
                        <span className="block text-xs text-amber-100/70">{candidate.matchReason}</span>
                      ) : null}
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}

            {error ? (
              <p className="rounded-lg border border-rose-500/40 bg-rose-500/10 px-3 py-2 text-sm text-rose-100">
                {error}
              </p>
            ) : null}
          </div>

          <footer className="flex items-center justify-end gap-2 border-t border-slate-800 px-5 py-4">
            <button
              type="button"
              onClick={onClose}
              className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:bg-slate-900"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={!allowed || isSubmitting}
              className="inline-flex items-center gap-2 rounded-md bg-sky-500 px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-sky-400 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting ? (
                <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
              ) : (
                <Plus className="h-4 w-4" aria-hidden />
              )}
              Create
            </button>
          </footer>
        </form>
      </aside>
    </div>
  )
}

function QuickCreateField({
  field,
  value,
  disabled,
  onChange,
}: {
  field: QuickCreateFieldDescriptor
  value: string
  disabled: boolean
  onChange: (value: string) => void
}) {
  const inputClass =
    'mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 placeholder:text-[var(--color-text-muted)] focus:border-sky-400 focus:outline-none focus:ring-2 focus:ring-sky-400/30 disabled:cursor-not-allowed disabled:opacity-60'

  return (
    <label className="block text-sm text-slate-300">
      <span>
        {field.label}
        {field.required ? <span className="text-rose-300"> *</span> : null}
      </span>
      {field.fieldType === 'textarea' ? (
        <textarea
          value={value}
          required={field.required}
          disabled={disabled}
          placeholder={field.placeholder ?? undefined}
          onChange={(event) => onChange(event.target.value)}
          className={`${inputClass} min-h-24 resize-y`}
        />
      ) : field.fieldType === 'select' ? (
        <select
          value={value}
          required={field.required}
          disabled={disabled}
          onChange={(event) => onChange(event.target.value)}
          className={inputClass}
        >
          <option value="">Select...</option>
          {(field.options ?? []).map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      ) : (
        <input
          type={field.fieldType === 'email' || field.fieldType === 'tel' ? field.fieldType : 'text'}
          value={value}
          required={field.required}
          disabled={disabled}
          placeholder={field.placeholder ?? undefined}
          onChange={(event) => onChange(event.target.value)}
          className={inputClass}
        />
      )}
    </label>
  )
}

function buildInitialValues(
  fields: QuickCreateFieldDescriptor[],
  initialValues?: Record<string, string>,
) {
  return fields.reduce<Record<string, string>>((acc, field) => {
    acc[field.key] = initialValues?.[field.key] ?? field.defaultValue ?? ''
    return acc
  }, {})
}
