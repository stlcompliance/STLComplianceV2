import { useEffect, useMemo, useState } from 'react'
import { ApiErrorCallout, DetailEmptyState, PageHeader, getErrorMessage } from '@stl/shared-ui'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createExceptionExemption,
  deactivateExceptionExemption,
  getExceptionExemptionEffectOptions,
  getExceptionExemptionTypeOptions,
  listExceptionExemptions,
  updateExceptionExemption,
  ComplianceCoreApiError,
} from '../../api/client'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'
import {
  buildExceptionExemptionSummary,
  buildExceptionExemptionTechnicalDetails,
} from '../../lib/exceptionExemptionDisplay'

export function ExceptionExemptionsPage() {
  const state = useComplianceCoreWorkspaceState()
  const queryClient = useQueryClient()
  const [typeFilter, setTypeFilter] = useState('all')
  const [activeOnly, setActiveOnly] = useState(true)
  const [selectedId, setSelectedId] = useState('')
  const [key, setKey] = useState('')
  const [label, setLabel] = useState('')
  const [type, setType] = useState('regulatory_exception')
  const [effectType, setEffectType] = useState('makes_requirement_not_applicable')
  const [packKey, setPackKey] = useState('')
  const [citationKey, setCitationKey] = useState('')
  const [governingBody, setGoverningBody] = useState('')
  const [programKey, setProgramKey] = useState('')
  const [applicabilityKey, setApplicabilityKey] = useState('')
  const [appliesToSubjectKind, setAppliesToSubjectKind] = useState('')
  const [appliesToSourceProduct, setAppliesToSourceProduct] = useState('')
  const [appliesToSourceEntity, setAppliesToSourceEntity] = useState('')
  const [conditionLogicJson, setConditionLogicJson] = useState('{}')
  const [issuingAuthority, setIssuingAuthority] = useState('')
  const [authorizationNumber, setAuthorizationNumber] = useState('')
  const [effectiveAt, setEffectiveAt] = useState('')
  const [expiresAt, setExpiresAt] = useState('')
  const [description, setDescription] = useState('')

  function buildMutationTechnicalDetails(error: unknown): Array<{ label: string; value: string }> {
    if (error instanceof ComplianceCoreApiError) {
      const entries: Array<{ label: string; value: string }> = [{ label: 'HTTP status', value: String(error.status) }]

      if (error.body.trim()) {
        try {
          const parsed = JSON.parse(error.body) as Record<string, unknown>
          const possibleTextFields = [
            ['Correlation ID', parsed.correlationId],
            ['Request ID', parsed.requestId],
            ['Trace ID', parsed.traceId],
            ['Title', parsed.title],
            ['Detail', parsed.detail],
          ] as const

          for (const [label, value] of possibleTextFields) {
            if (typeof value === 'string' && value.trim()) {
              entries.push({ label, value: value.trim() })
            }
          }

          const validationErrors = parsed.errors && typeof parsed.errors === 'object' && !Array.isArray(parsed.errors)
            ? Object.entries(parsed.errors as Record<string, string[] | string>)
                .flatMap(([field, value]) => {
                  const values = Array.isArray(value) ? value : [value]
                  return values
                    .map((item) => String(item).trim())
                    .filter(Boolean)
                    .map((item) => `${field}: ${item}`)
                })
            : []

          if (validationErrors.length > 0) {
            entries.push({ label: 'Validation errors', value: validationErrors.join('; ') })
          } else if (entries.length === 1) {
            entries.push({ label: 'Response body', value: error.body })
          }
        } catch {
          entries.push({ label: 'Response body', value: error.body })
        }
      }

      return entries
    }

    if (error instanceof Error && error.message.trim()) {
      return [{ label: 'Error message', value: error.message.trim() }]
    }

    return [{ label: 'Details', value: 'No technical details available.' }]
  }

  function mutationErrorFooter(message: string, error: unknown) {
    const entries = buildMutationTechnicalDetails(error)

    return (
      <div className="space-y-2 text-xs text-slate-200">
        <p>{message}</p>
        <details className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
          <summary className="cursor-pointer text-[11px] font-semibold uppercase tracking-wide text-slate-100">
            Advanced technical details
          </summary>
          <dl className="mt-3 grid gap-2 md:grid-cols-2">
            {entries.map((entry) => (
              <div key={entry.label} className="rounded-md border border-slate-800 bg-slate-900/70 p-2">
                <dt className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">{entry.label}</dt>
                <dd className="mt-1 break-all text-sm text-slate-100">{entry.value}</dd>
              </div>
            ))}
          </dl>
        </details>
      </div>
    )
  }

  const exemptionsQuery = useQuery({
    queryKey: ['compliancecore-exception-exemptions', state.accessToken, typeFilter, activeOnly],
    queryFn: () =>
      listExceptionExemptions(state.accessToken, {
        type: typeFilter === 'all' ? undefined : typeFilter,
        includeInactive: !activeOnly,
      }),
    enabled: Boolean(state.accessToken),
  })

  const typeOptionsQuery = useQuery({
    queryKey: ['compliancecore-exception-exemption-types', state.accessToken],
    queryFn: () => getExceptionExemptionTypeOptions(state.accessToken),
    enabled: Boolean(state.accessToken),
  })

  const effectOptionsQuery = useQuery({
    queryKey: ['compliancecore-exception-exemption-effects', state.accessToken],
    queryFn: () => getExceptionExemptionEffectOptions(state.accessToken),
    enabled: Boolean(state.accessToken),
  })

  const selected = useMemo(
    () => exemptionsQuery.data?.find((item) => item.exceptionExemptionId === selectedId) ?? null,
    [exemptionsQuery.data, selectedId],
  )

  useEffect(() => {
    if (selected) {
      setKey(selected.key)
      setLabel(selected.label)
      setType(selected.type)
      setEffectType(selected.effectType)
      setPackKey(selected.packKey)
      setCitationKey(selected.citationKey)
      setGoverningBody(selected.governingBody)
      setProgramKey(selected.programKey)
      setApplicabilityKey(selected.applicabilityKey)
      setAppliesToSubjectKind(selected.appliesToSubjectKind)
      setAppliesToSourceProduct(selected.appliesToSourceProduct)
      setAppliesToSourceEntity(selected.appliesToSourceEntity)
      setConditionLogicJson(selected.conditionLogicJson)
      setIssuingAuthority(selected.issuingAuthority)
      setAuthorizationNumber(selected.authorizationNumber)
      setEffectiveAt(selected.effectiveAt ? selected.effectiveAt.slice(0, 16) : '')
      setExpiresAt(selected.expiresAt ? selected.expiresAt.slice(0, 16) : '')
      setDescription(selected.description)
    }
  }, [selected])

  const saveMutation = useMutation({
    mutationFn: async () => {
      const payload = {
        key,
        label,
        type,
        effectType,
        governingBody: governingBody || null,
        programKey: programKey || null,
        packKey: packKey || null,
        citationKey: citationKey || null,
        applicabilityKey: applicabilityKey || null,
        appliesToSubjectKind: appliesToSubjectKind || null,
        appliesToSourceProduct: appliesToSourceProduct || null,
        appliesToSourceEntity: appliesToSourceEntity || null,
        conditionLogicJson: conditionLogicJson || '{}',
        issuingAuthority: issuingAuthority || null,
        authorizationNumber: authorizationNumber || null,
        effectiveAt: effectiveAt ? new Date(`${effectiveAt}:00`).toISOString() : null,
        expiresAt: expiresAt ? new Date(`${expiresAt}:00`).toISOString() : null,
        description: description || null,
      }

      if (selected) {
        return updateExceptionExemption(state.accessToken, selected.exceptionExemptionId, payload)
      }

      return createExceptionExemption(state.accessToken, {
        ...payload,
        active: true,
      })
    },
    onSuccess: async (saved) => {
      setSelectedId(saved.exceptionExemptionId)
      await queryClient.invalidateQueries({ queryKey: ['compliancecore-exception-exemptions'] })
    },
  })

  const deactivateMutation = useMutation({
    mutationFn: () => {
      if (!selected) {
        throw new Error('Select an exception/exemption first.')
      }
      return deactivateExceptionExemption(state.accessToken, selected.exceptionExemptionId)
    },
    onSuccess: async () => {
      setSelectedId('')
      await queryClient.invalidateQueries({ queryKey: ['compliancecore-exception-exemptions'] })
    },
  })

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Exception exemptions"
        subtitle="Create, review, and deactivate legal relief records that alter normal compliance evaluation."
      />

      <div className="flex flex-wrap items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/70 p-4 text-sm text-slate-300">
        <label className="flex items-center gap-2">
          Type
          <select
            value={typeFilter}
            onChange={(event) => setTypeFilter(event.target.value)}
            className="rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
          >
            <option value="all">All</option>
            {(typeOptionsQuery.data ?? []).map((option) => (
              <option key={option.key} value={option.key}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
        <label className="flex items-center gap-2">
          <input type="checkbox" checked={activeOnly} onChange={(event) => setActiveOnly(event.target.checked)} />
          Active only
        </label>
      </div>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_minmax(360px,0.92fr)]">
        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Records</h2>
          {exemptionsQuery.isError ? (
            <div className="mt-3">
              <ApiErrorCallout
                title="Unable to load exception exemptions"
                message={getErrorMessage(exemptionsQuery.error, 'Failed to load exception exemptions.')}
                retryLabel="Retry"
                onRetry={() => void exemptionsQuery.refetch()}
              />
            </div>
          ) : null}
          {exemptionsQuery.data?.length === 0 ? (
            <div className="mt-4">
              <DetailEmptyState text="No exception/exemption records match this filter." />
            </div>
          ) : (
            <div className="mt-4 space-y-2">
              {(exemptionsQuery.data ?? []).map((item) => (
                <button
                  key={item.exceptionExemptionId}
                  type="button"
                  onClick={() => setSelectedId(item.exceptionExemptionId)}
                  className={`w-full rounded-xl border px-4 py-3 text-left transition ${
                    item.exceptionExemptionId === selectedId
                      ? 'border-sky-500 bg-sky-500/10'
                      : 'border-slate-800 bg-slate-900/60 hover:border-slate-700'
                  }`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <div className="font-medium text-slate-100">{item.label}</div>
                      <div className="mt-1 text-xs text-[var(--color-text-muted)]">{item.key}</div>
                    </div>
                    <span className="rounded-full border border-slate-700 px-2 py-0.5 text-xs text-slate-400">
                      {item.active ? 'active' : 'inactive'}
                    </span>
                  </div>
                  <div className="mt-2 flex flex-wrap gap-2 text-xs text-slate-400">
                    <span>{item.type}</span>
                    <span>{item.effectType}</span>
                    <span>{item.packKey || 'tenant-wide'}</span>
                  </div>
                </button>
              ))}
            </div>
          )}
        </section>

        <section className="space-y-4 rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
            {selected ? 'Edit record' : 'Create record'}
          </h2>
          <div className="grid gap-3 md:grid-cols-2">
            <Field label="Key" value={key} onChange={setKey} />
            <Field label="Label" value={label} onChange={setLabel} />
            <SelectField
              label="Type"
              value={type}
              onChange={setType}
              options={typeOptionsQuery.data ?? []}
            />
            <SelectField
              label="Effect type"
              value={effectType}
              onChange={setEffectType}
              options={effectOptionsQuery.data ?? []}
            />
            <Field label="Pack key" value={packKey} onChange={setPackKey} />
            <Field label="Citation key" value={citationKey} onChange={setCitationKey} />
            <Field label="Governing body" value={governingBody} onChange={setGoverningBody} />
            <Field label="Program key" value={programKey} onChange={setProgramKey} />
            <Field label="Applicability key" value={applicabilityKey} onChange={setApplicabilityKey} />
            <Field label="Subject kind" value={appliesToSubjectKind} onChange={setAppliesToSubjectKind} />
            <Field label="Source product" value={appliesToSourceProduct} onChange={setAppliesToSourceProduct} />
            <Field label="Source entity" value={appliesToSourceEntity} onChange={setAppliesToSourceEntity} />
            <Field label="Issuing authority" value={issuingAuthority} onChange={setIssuingAuthority} />
            <Field label="Authorization number" value={authorizationNumber} onChange={setAuthorizationNumber} />
            <Field label="Effective at" value={effectiveAt} onChange={setEffectiveAt} type="datetime-local" />
            <Field label="Expires at" value={expiresAt} onChange={setExpiresAt} type="datetime-local" />
          </div>
          <label className="block text-sm text-slate-300">
            Condition logic JSON
            <textarea
              value={conditionLogicJson}
              onChange={(event) => setConditionLogicJson(event.target.value)}
              rows={5}
              className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 font-mono text-sm text-slate-100"
            />
          </label>
          <label className="block text-sm text-slate-300">
            Description
            <textarea
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              rows={4}
              className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              onClick={() => saveMutation.mutate()}
              disabled={saveMutation.isPending}
              className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            >
              {saveMutation.isPending ? 'Saving…' : selected ? 'Save changes' : 'Create record'}
            </button>
            <button
              type="button"
              onClick={() => deactivateMutation.mutate()}
              disabled={!selected || deactivateMutation.isPending}
              className="rounded-md border border-slate-700 px-4 py-2 text-sm font-medium text-slate-200 hover:bg-slate-800 disabled:opacity-50"
            >
              {deactivateMutation.isPending ? 'Deactivating…' : 'Deactivate'}
            </button>
          </div>
          {saveMutation.isError ? (
            <ApiErrorCallout
              title="Save failed"
              message={getErrorMessage(saveMutation.error, 'Failed to save exception exemption.')}
              footer={mutationErrorFooter('Your edits remain in the form, but nothing was saved.', saveMutation.error)}
            />
          ) : null}
          {deactivateMutation.isError ? (
            <ApiErrorCallout
              title="Deactivate failed"
              message={getErrorMessage(deactivateMutation.error, 'Failed to deactivate exception exemption.')}
              footer={mutationErrorFooter('The selected exemption remains active, and no change was applied.', deactivateMutation.error)}
            />
          ) : null}
        </section>
      </div>

      {selected ? (
        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Selected record</h2>
          <div className="mt-3 space-y-3">
            <dl className="grid gap-3 md:grid-cols-2">
              {buildExceptionExemptionSummary(selected).map((entry) => (
                <div key={entry.label} className="rounded-xl border border-slate-800 bg-slate-900/70 p-3">
                  <dt className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">{entry.label}</dt>
                  <dd className="mt-1 break-words text-sm text-slate-100">{entry.value}</dd>
                </div>
              ))}
            </dl>
            <details className="rounded-xl border border-slate-800 bg-slate-900/70 p-3 text-xs text-slate-300">
              <summary className="cursor-pointer text-[11px] font-semibold uppercase tracking-wide text-slate-100">
                Advanced technical details
              </summary>
              <dl className="mt-3 grid gap-3 md:grid-cols-2">
                {buildExceptionExemptionTechnicalDetails(selected).map((entry) => (
                  <div key={entry.label} className="rounded-lg border border-slate-800 bg-slate-950/70 p-3">
                    <dt className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">{entry.label}</dt>
                    <dd className="mt-1 break-all text-sm text-slate-100">{entry.value}</dd>
                  </div>
                ))}
              </dl>
            </details>
          </div>
        </section>
      ) : null}
    </div>
  )
}

function Field({
  label,
  value,
  onChange,
  type = 'text',
}: {
  label: string
  value: string
  onChange: (value: string) => void
  type?: string
}) {
  return (
    <label className="block text-sm text-slate-300">
      {label}
      <input
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
      />
    </label>
  )
}

function SelectField({
  label,
  value,
  onChange,
  options,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  options: Array<{ key: string; label: string }>
}) {
  return (
    <label className="block text-sm text-slate-300">
      {label}
      <select
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
      >
        {options.map((option) => (
          <option key={option.key} value={option.key}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
  )
}
