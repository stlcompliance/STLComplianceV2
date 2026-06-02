import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { Navigate, Link, useNavigate } from 'react-router-dom'
import { ArrowLeft, ArrowRight, CheckCircle2, Loader2 } from 'lucide-react'
import { PageHeader } from '@stl/shared-ui'
import {
  createAssetControlledV1,
  getAssetCreateFieldset,
  getMe,
} from '../../api/client'
import type { FieldMetadataResponse, FieldsetResponse } from '../../api/types'
import { canManageAssets, loadSession } from '../../auth/sessionStorage'
import {
  AssetFieldsetFields,
  AssetReviewPanel,
  buildAssetUpsertPayload,
  getCreateWorkflowSteps,
  getFilteredOptions,
  initializeAssetFieldValues,
  isCreateBaselineComplete,
  validateAssetValues,
  type AssetFieldValues,
} from '../../components/AssetFieldsetWorkflow'

function clearInvalidDependentValues(
  fieldset: FieldsetResponse,
  values: AssetFieldValues,
  changedFieldKey: string,
): AssetFieldValues {
  let next = { ...values }
  for (const field of fieldset.fields) {
    if (field.key === changedFieldKey) continue
    if (!field.catalogKey && !field.referenceKey) continue

    const filtered = getFilteredOptions(fieldset, field, next)
    if (filtered.length === 0) continue

    const allowed = new Set(filtered.map((option) => option.key))
    const current = next[field.key]
    if (Array.isArray(current)) {
      const retained = current.filter((item) => allowed.has(String(item)))
      if (retained.length !== current.length) {
        next = { ...next, [field.key]: retained }
      }
      continue
    }

    if (current != null && String(current).trim() && !allowed.has(String(current))) {
      next = { ...next, [field.key]: '' }
    }
  }
  return next
}

function countCompletedFields(fields: FieldMetadataResponse[], values: AssetFieldValues): number {
  return fields.filter((field) => {
    const value = values[field.key]
    return Array.isArray(value) ? value.length > 0 : value != null && String(value).trim().length > 0
  }).length
}

export function AssetCreatePage() {
  const session = loadSession()
  const navigate = useNavigate()
  const [values, setValues] = useState<AssetFieldValues>({})
  const [currentStepIndex, setCurrentStepIndex] = useState(0)
  const [serverError, setServerError] = useState<string | null>(null)

  const meQuery = useQuery({
    queryKey: ['maintainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const fieldsetQuery = useQuery({
    queryKey: ['maintainarr-fieldset-assets-create'],
    queryFn: () => getAssetCreateFieldset(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  useEffect(() => {
    if (!fieldsetQuery.data) return
    setValues((current) => ({ ...initializeAssetFieldValues(fieldsetQuery.data), ...current }))
  }, [fieldsetQuery.data])

  const fieldset = fieldsetQuery.data
  const baselineComplete = fieldset ? isCreateBaselineComplete(fieldset, values) : false
  const allSteps = useMemo(
    () => (fieldset ? getCreateWorkflowSteps(fieldset, values) : []),
    [fieldset, values],
  )
  const steps = baselineComplete ? allSteps : allSteps.slice(0, 1)
  const currentStep = steps[Math.min(currentStepIndex, Math.max(steps.length - 1, 0))]
  const visibleErrors = fieldset && currentStep && !currentStep.isReview
    ? validateAssetValues(fieldset, values, currentStep.fields)
    : {}
  const allErrors = fieldset ? validateAssetValues(fieldset, values) : {}
  const completedInCurrentStep = currentStep ? countCompletedFields(currentStep.fields, values) : 0

  useEffect(() => {
    if (!baselineComplete && currentStepIndex > 0) {
      setCurrentStepIndex(0)
    }
  }, [baselineComplete, currentStepIndex])

  const createMutation = useMutation({
    mutationFn: () => createAssetControlledV1(session!.accessToken, buildAssetUpsertPayload(values)),
    onSuccess: (created) => {
      setServerError(null)
      navigate(`/assets/${created.assetId}?created=1`)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to create asset')
    },
  })

  if (!session) {
    return <Navigate to="/launch" replace />
  }

  const canCreate = meQuery.data
    ? canManageAssets(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  const handleFieldChange = (fieldKey: string, value: unknown) => {
    if (!fieldset) return
    setValues((current) => clearInvalidDependentValues(fieldset, { ...current, [fieldKey]: value }, fieldKey))
    setServerError(null)
  }

  const canGoNext = currentStep && Object.keys(visibleErrors).length === 0
  const canSubmit =
    Boolean(fieldset)
    && baselineComplete
    && Object.keys(allErrors).length === 0
    && !createMutation.isPending

  return (
    <div className="mx-auto max-w-6xl space-y-6" data-testid="asset-create-page">
      <PageHeader
        title="Create Asset"
        subtitle="Guided MaintainArr asset profile setup"
      />

      <div className="flex flex-wrap items-center justify-between gap-3">
        <Link to="/assets/drawer" className="inline-flex items-center gap-2 text-sm text-slate-300 hover:text-white">
          <ArrowLeft className="h-4 w-4" />
          Back to assets
        </Link>
        <div className="rounded-full border border-slate-800 bg-slate-950 px-3 py-1 text-xs text-slate-400">
          {baselineComplete ? 'Optional sections available' : 'Complete required basics to continue'}
        </div>
      </div>

      {meQuery.isLoading || fieldsetQuery.isLoading ? (
        <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-6">
          <div className="flex items-center gap-3 text-sm text-slate-300">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading asset fieldset...
          </div>
        </section>
      ) : null}

      {fieldsetQuery.isError ? (
        <p className="rounded-lg border border-red-800 bg-red-950/40 p-3 text-sm text-red-200">
          Failed to load asset create fieldset.
        </p>
      ) : null}

      {!canCreate && meQuery.isSuccess ? (
        <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-6">
          <h2 className="text-lg font-semibold text-white">Asset creation unavailable</h2>
          <p className="mt-2 text-sm text-slate-400">Your role can view assets but cannot create or update them.</p>
        </section>
      ) : null}

      {serverError ? (
        <p className="rounded-lg border border-red-800 bg-red-950/40 p-3 text-sm text-red-200">{serverError}</p>
      ) : null}

      {fieldset && canCreate ? (
        <>
          <nav className="grid gap-2 md:grid-cols-4 xl:grid-cols-6" aria-label="Asset create steps">
            {allSteps.map((step, index) => {
              const isRevealed = baselineComplete || index === 0
              const isActive = currentStep?.key === step.key
              const completedCount = countCompletedFields(step.fields, values)
              return (
                <button
                  key={step.key}
                  type="button"
                  disabled={!isRevealed}
                  onClick={() => setCurrentStepIndex(index)}
                  className={`rounded-lg border px-3 py-3 text-left text-sm ${
                    isActive
                      ? 'border-sky-400 bg-sky-500/10 text-white'
                      : 'border-slate-800 bg-slate-950/70 text-slate-300'
                  } disabled:cursor-not-allowed disabled:opacity-40`}
                >
                  <span className="block font-medium">{step.label}</span>
                  <span className="mt-1 block text-xs text-slate-500">
                    {step.isReview ? 'Final check' : `${completedCount}/${step.fields.length} fields`}
                  </span>
                </button>
              )
            })}
          </nav>

          <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5">
            <div className="mb-5 flex flex-wrap items-start justify-between gap-3">
              <div>
                <h2 className="text-xl font-semibold text-white">{currentStep?.label}</h2>
                <p className="mt-1 text-sm text-slate-400">{currentStep?.description}</p>
              </div>
              {!currentStep?.isReview ? (
                <span className="rounded-full border border-slate-700 px-3 py-1 text-xs text-slate-400">
                  {completedInCurrentStep}/{currentStep?.fields.length ?? 0} complete
                </span>
              ) : null}
            </div>

            {currentStep?.isReview ? (
              <AssetReviewPanel fieldset={fieldset} values={values} />
            ) : (
              <AssetFieldsetFields
                fieldset={fieldset}
                fields={currentStep?.fields ?? []}
                values={values}
                errors={visibleErrors}
                mode="create"
                onChange={handleFieldChange}
              />
            )}
          </section>

          <div className="sticky bottom-0 z-10 rounded-xl border border-slate-800 bg-slate-950/95 p-4 shadow-2xl">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div className="text-sm text-slate-400">
                {baselineComplete ? (
                  <span className="inline-flex items-center gap-2 text-emerald-300">
                    <CheckCircle2 className="h-4 w-4" />
                    Required asset record is valid
                  </span>
                ) : (
                  'Asset class, type, number, status, and lifecycle are required first.'
                )}
              </div>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  className="rounded-lg border border-slate-700 px-4 py-2 text-sm text-slate-200 disabled:opacity-50"
                  disabled={currentStepIndex === 0}
                  onClick={() => setCurrentStepIndex((current) => Math.max(current - 1, 0))}
                >
                  Previous
                </button>
                {currentStep?.isReview ? (
                  <button
                    type="button"
                    className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                    disabled={!canSubmit}
                    onClick={() => createMutation.mutate()}
                  >
                    {createMutation.isPending ? 'Creating...' : 'Create asset'}
                  </button>
                ) : (
                  <button
                    type="button"
                    className="inline-flex items-center gap-2 rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                    disabled={!canGoNext || (!baselineComplete && currentStepIndex === 0)}
                    onClick={() => setCurrentStepIndex((current) => Math.min(current + 1, steps.length - 1))}
                  >
                    Next
                    <ArrowRight className="h-4 w-4" />
                  </button>
                )}
              </div>
            </div>
          </div>
        </>
      ) : null}
    </div>
  )
}
