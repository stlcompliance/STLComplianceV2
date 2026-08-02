import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

import {
  getSupplierOnboardingDocumentRequirements,
  upsertSupplierOnboardingDocumentRequirements,
} from '../api/client'

const DEFAULT_REQUIREMENTS = [
  { documentTypeKey: 'w9', label: 'W-9 tax form' },
  { documentTypeKey: 'insurance_certificate', label: 'Certificate of insurance' },
  { documentTypeKey: 'supplier_agreement', label: 'Signed supplier agreement' },
] as const

interface SupplierOnboardingRequirementsPanelProps {
  accessToken: string
  canManage: boolean
}

export function SupplierOnboardingRequirementsPanel({
  accessToken,
  canManage,
}: SupplierOnboardingRequirementsPanelProps) {
  const queryClient = useQueryClient()
  const [selectedRequirementKeys, setSelectedRequirementKeys] = useState<string[]>([])
  const [customRequirementKey, setCustomRequirementKey] = useState('')

  const requirementsQuery = useQuery({
    queryKey: ['supplyarr-onboarding-requirements', accessToken],
    queryFn: () => getSupplierOnboardingDocumentRequirements(accessToken),
    enabled: canManage,
  })

  useEffect(() => {
    if (!requirementsQuery.data) {
      return
    }
    setSelectedRequirementKeys(
      requirementsQuery.data.requirements.map((requirement) => requirement.documentTypeKey),
    )
  }, [requirementsQuery.data])

  const requirementOptions = useMemo(() => {
    const currentLabels = new Map(
      requirementsQuery.data?.requirements.map((requirement) => [
        requirement.documentTypeKey.toLowerCase(),
        requirement.label,
      ]) ?? [],
    )
    return Array.from(
      new Map(
        [...DEFAULT_REQUIREMENTS, ...(requirementsQuery.data?.requirements ?? [])].map((requirement) => {
          const key = requirement.documentTypeKey.toLowerCase()
          return [
            key,
            {
              documentTypeKey: key,
              label: currentLabels.get(key) ?? requirement.label,
            },
          ] as const
        }),
      ).values(),
    )
  }, [requirementsQuery.data])

  const customRequirementOptions = useMemo(
    () =>
      selectedRequirementKeys
        .filter(
          (requirementKey) =>
            !DEFAULT_REQUIREMENTS.some(
              (defaultRequirement) => defaultRequirement.documentTypeKey === requirementKey,
            ),
        )
        .map((requirementKey) => requirementKey.trim())
        .filter(Boolean),
    [selectedRequirementKeys],
  )

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertSupplierOnboardingDocumentRequirements(accessToken, {
        requiredDocumentTypeKeys: selectedRequirementKeys,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-onboarding-requirements', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  if (requirementsQuery.isLoading || !requirementsQuery.data) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
        <h2 className="text-lg font-semibold text-white">Onboarding requirements</h2>
        <p className="mt-3 text-sm text-slate-400">Loading onboarding requirements…</p>
      </section>
    )
  }

  const requiredCount = selectedRequirementKeys.length
  const defaultCount = DEFAULT_REQUIREMENTS.filter((requirement) =>
    selectedRequirementKeys.includes(requirement.documentTypeKey),
  ).length
  const customCount = customRequirementOptions.length

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-5"
      data-testid="supplier-onboarding-requirements-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-white">Onboarding requirements</h2>
          <p className="mt-1 text-sm text-slate-400">
            Control the document gate that blocks supplier onboarding submission until approved records exist.
          </p>
        </div>
        <div className="text-xs text-slate-400">
          {requiredCount} required · {defaultCount} standard · {customCount} custom
        </div>
      </div>

      {requirementsQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Unable to load onboarding requirements"
            message={getErrorMessage(requirementsQuery.error, 'Failed to load onboarding requirements.')}
            onRetry={() => void requirementsQuery.refetch()}
            retryLabel="Retry requirements"
          />
        </div>
      ) : null}

      <div className="mt-4 space-y-3">
        {requirementOptions.map((requirement) => {
          const checked = selectedRequirementKeys.includes(requirement.documentTypeKey)
          return (
            <label
              key={requirement.documentTypeKey}
              className="flex items-start gap-3 rounded-lg border border-slate-700 bg-slate-950/40 p-3 text-sm text-slate-200"
            >
              <input
                type="checkbox"
                checked={checked}
                onChange={(event) => {
                  setSelectedRequirementKeys((current) =>
                    event.target.checked
                      ? Array.from(new Set([...current, requirement.documentTypeKey]))
                      : current.filter((key) => key !== requirement.documentTypeKey),
                  )
                }}
              />
              <span>
                <span className="font-medium text-slate-100">{requirement.label}</span>
                <span className="ml-2 text-xs text-slate-400">{requirement.documentTypeKey}</span>
              </span>
            </label>
          )
        })}

        <label htmlFor="supplier-onboarding-requirement-custom" className="block text-sm text-slate-300">
          Add custom requirement key
          <div className="mt-1 flex gap-2">
            <input
              id="supplier-onboarding-requirement-custom"
              className="w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              placeholder="supplier_certificate"
              value={customRequirementKey}
              onChange={(event) => setCustomRequirementKey(event.target.value)}
            />
            <button
              type="button"
              className="rounded-md border border-slate-600 px-3 py-2 text-sm text-slate-100 disabled:opacity-50"
              disabled={!customRequirementKey.trim()}
              onClick={() => {
                const normalizedKey = customRequirementKey.trim().toLowerCase()
                setSelectedRequirementKeys((current) =>
                  Array.from(new Set([...current, normalizedKey])),
                )
                setCustomRequirementKey('')
              }}
            >
              Add
            </button>
          </div>
        </label>

        {customRequirementOptions.length > 0 ? (
          <div className="rounded-lg border border-slate-800 bg-slate-950/40 p-3 text-sm text-slate-300">
            <p className="font-medium text-slate-100">Custom requirements in scope</p>
            <ul className="mt-2 space-y-1">
              {customRequirementOptions.map((requirementKey) => (
                <li key={requirementKey} className="flex items-center justify-between gap-3">
                  <span>{requirementKey}</span>
                  <button
                    type="button"
                    className="text-xs text-slate-400 underline decoration-dotted hover:text-white"
                    onClick={() =>
                      setSelectedRequirementKeys((current) =>
                        current.filter((key) => key !== requirementKey),
                      )
                    }
                  >
                    Remove
                  </button>
                </li>
              ))}
            </ul>
          </div>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-2">
        <button
          type="button"
          className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="supplier-onboarding-requirements-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save requirements'}
        </button>
        <button
          type="button"
          className="rounded-md border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() =>
            setSelectedRequirementKeys(DEFAULT_REQUIREMENTS.map((requirement) => requirement.documentTypeKey))
          }
        >
          Reset to defaults
        </button>
      </div>

      {saveMutation.isError ? (
        <ApiErrorCallout
          title="Save failed"
          message={getErrorMessage(saveMutation.error, 'Failed to save onboarding requirements.')}
        />
      ) : null}
    </section>
  )
}
