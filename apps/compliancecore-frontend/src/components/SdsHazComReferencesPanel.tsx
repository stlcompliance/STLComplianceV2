import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import {
  ControlledSelect,
  GeneratedKeyField,
  slugifyKey,
  StaticSearchPicker,
  type PickerOption,
} from '@stl/shared-ui'
import {
  createHazComReference,
  createSdsReference,
  listHazComReferences,
  listSdsReferences,
} from '../api/client'

interface SdsHazComReferencesPanelProps {
  accessToken: string
  canRead: boolean
  canManage: boolean
}

export function SdsHazComReferencesPanel({
  accessToken,
  canRead,
  canManage,
}: SdsHazComReferencesPanelProps) {
  const queryClient = useQueryClient()
  const [productName, setProductName] = useState('')
  const [sdsKeyManual, setSdsKeyManual] = useState('')
  const [confirmedSdsKey, setConfirmedSdsKey] = useState<string | null>(null)
  const [hazComTitle, setHazComTitle] = useState('')
  const [hazComKeyManual, setHazComKeyManual] = useState('')
  const [confirmedHazComKey, setConfirmedHazComKey] = useState<string | null>(null)
  const [linkedSdsKey, setLinkedSdsKey] = useState('')

  const generatedSdsKey = useMemo(
    () => slugifyKey(sdsKeyManual || productName),
    [productName, sdsKeyManual],
  )
  const generatedHazComKey = useMemo(
    () => slugifyKey(hazComKeyManual || hazComTitle),
    [hazComKeyManual, hazComTitle],
  )

  const sdsQuery = useQuery({
    queryKey: ['compliancecore-sds-references', accessToken],
    queryFn: () => listSdsReferences(accessToken),
    enabled: canRead,
  })

  const hazComQuery = useQuery({
    queryKey: ['compliancecore-hazcom-references', accessToken],
    queryFn: () => listHazComReferences(accessToken),
    enabled: canRead,
  })

  const sdsPickerOptions: PickerOption[] = useMemo(
    () =>
      (sdsQuery.data ?? []).map((item) => ({
        value: item.sdsKey,
        label: `${item.productName || item.sdsKey} (${item.sdsKey})`,
        inactive: !item.isActive,
      })),
    [sdsQuery.data],
  )

  const createSdsMutation = useMutation({
    mutationFn: () =>
      createSdsReference(accessToken, {
        sdsKey: (sdsKeyManual.trim() || generatedSdsKey).trim(),
        materialKeyId: null,
        productName,
        manufacturer: '',
        documentUrl: '',
        revisionDate: null,
      }),
    onSuccess: (created) => {
      setConfirmedSdsKey(created.sdsKey)
      setProductName('')
      setSdsKeyManual('')
      queryClient.invalidateQueries({ queryKey: ['compliancecore-sds-references', accessToken] })
    },
  })

  const createHazComMutation = useMutation({
    mutationFn: () =>
      createHazComReference(accessToken, {
        hazComKey: (hazComKeyManual.trim() || generatedHazComKey).trim(),
        title: hazComTitle,
        description: '',
        linkedSdsKey: linkedSdsKey || null,
        locationRef: '',
        documentUrl: '',
      }),
    onSuccess: (created) => {
      setConfirmedHazComKey(created.hazComKey)
      setHazComTitle('')
      setHazComKeyManual('')
      setLinkedSdsKey('')
      queryClient.invalidateQueries({ queryKey: ['compliancecore-hazcom-references', accessToken] })
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="mt-8 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="sds-hazcom-references-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">SDS / HazCom references</h2>
      <p className="mt-1 text-sm text-slate-400">
        First-class /api/sds and /api/hazcom surfaces per docs/18 — complements 9-CSV import.
      </p>

      <div className="mt-4 grid gap-6 lg:grid-cols-2">
        <div>
          <h3 className="text-sm font-medium text-slate-200">SDS references</h3>
          <ul className="mt-2 space-y-1 text-sm text-slate-300" data-testid="sds-reference-list">
            {(sdsQuery.data ?? []).map((item) => (
              <li key={item.sdsReferenceId}>
                {item.sdsKey} — {item.productName || 'No product name'}
              </li>
            ))}
          </ul>
          {canManage ? (
            <form
              className="mt-3 space-y-2"
              onSubmit={(event) => {
                event.preventDefault()
                createSdsMutation.mutate()
              }}
            >
              <label className="block text-sm text-slate-300">
                Product name
                <input
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm"
                  placeholder="product name"
                  value={productName}
                  onChange={(event) => {
                    setProductName(event.target.value)
                    setConfirmedSdsKey(null)
                  }}
                />
              </label>
              <GeneratedKeyField
                sourceLabel={productName}
                generatedKey={generatedSdsKey}
                confirmedKey={confirmedSdsKey}
                manualOverride={sdsKeyManual}
                onManualOverrideChange={setSdsKeyManual}
                showAdvancedKey
              />
              <button
                type="submit"
                className="rounded bg-sky-700 px-3 py-1 text-sm text-white disabled:opacity-50"
                disabled={!generatedSdsKey || createSdsMutation.isPending}
              >
                Add SDS
              </button>
            </form>
          ) : null}
        </div>

        <div>
          <h3 className="text-sm font-medium text-slate-200">HazCom references</h3>
          <ul className="mt-2 space-y-1 text-sm text-slate-300" data-testid="hazcom-reference-list">
            {(hazComQuery.data ?? []).map((item) => (
              <li key={item.hazComReferenceId}>
                {item.hazComKey} — {item.title}
              </li>
            ))}
          </ul>
          {canManage ? (
            <form
              className="mt-3 space-y-2"
              onSubmit={(event) => {
                event.preventDefault()
                createHazComMutation.mutate()
              }}
            >
              <label className="block text-sm text-slate-300">
                Title
                <input
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm"
                  placeholder="title"
                  value={hazComTitle}
                  onChange={(event) => {
                    setHazComTitle(event.target.value)
                    setConfirmedHazComKey(null)
                  }}
                />
              </label>
              <GeneratedKeyField
                sourceLabel={hazComTitle}
                generatedKey={generatedHazComKey}
                confirmedKey={confirmedHazComKey}
                manualOverride={hazComKeyManual}
                onManualOverrideChange={setHazComKeyManual}
                showAdvancedKey
              />
              <StaticSearchPicker
                label="Linked SDS (optional)"
                value={linkedSdsKey}
                onChange={setLinkedSdsKey}
                options={sdsPickerOptions}
                placeholder="Search SDS…"
                testId="hazcom-linked-sds-picker"
              />
              <button
                type="submit"
                className="rounded bg-sky-700 px-3 py-1 text-sm text-white disabled:opacity-50"
                disabled={!generatedHazComKey || !hazComTitle.trim() || createHazComMutation.isPending}
              >
                Add HazCom
              </button>
            </form>
          ) : null}
        </div>
      </div>
    </section>
  )
}
