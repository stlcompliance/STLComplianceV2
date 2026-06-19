import { useEffect, useMemo, useState, type FormEvent } from 'react'
import {
  ControlledSelect,
  SUITE_SOURCE_PRODUCT_OPTIONS,
  listSourceReferenceOptions,
} from '@stl/shared-ui'
import type {
  CreateFactSourceRequest,
  FactDefinitionResponse,
  FactSourceResponse,
  UpdateFactSourceRequest,
} from '../api/types'

type SourceType = 'static_config' | 'product_api' | 'product_mirror' | 'report_generated'
type FormMode = 'create' | 'edit'

const ACCIDENT_REGISTER_FACT_KEY = 't49_accident_register_current'
const ACCIDENT_REGISTER_REPORT_REFERENCE = 'reportarr:report:accident_register'

const REPORT_GENERATED_EVENT_CLASS_OPTIONS = [
  { value: 'accident', label: 'Accident' },
  { value: 'injury', label: 'Injury' },
  { value: 'near_miss', label: 'Near miss' },
  { value: 'equipment_damage', label: 'Equipment damage' },
  { value: 'safety', label: 'Safety' },
] as const

interface FactSourcesPanelProps {
  factDefinitions: FactDefinitionResponse[]
  factSources: FactSourceResponse[]
  canManage: boolean
  onCreateFactSource: (payload: CreateFactSourceRequest) => Promise<unknown> | unknown
  onUpdateFactSource: (
    factSourceId: string,
    payload: UpdateFactSourceRequest,
  ) => Promise<unknown> | unknown
  isSavingFactSource: boolean
}

interface FactSourceFormState {
  mode: FormMode
  factDefinitionId: string
  factSourceId: string
  sourceKey: string
  sourceType: SourceType
  label: string
  description: string
  productKey: string
  productReference: string
  configJson: string
  priority: string
  isActive: boolean
}

const sourceTypes: Array<{ value: SourceType; label: string; hint: string }> = [
  {
    value: 'static_config',
    label: 'Static config',
    hint: 'Manual typed value held by Compliance Core.',
  },
  {
    value: 'product_api',
    label: 'Product API',
    hint: 'Reference to a product API path or synced snapshot.',
  },
  {
    value: 'product_mirror',
    label: 'Product mirror',
    hint: 'Reference to a mirrored fact provided by another product.',
  },
  {
    value: 'report_generated',
    label: 'Report generated',
    hint: 'Generated report built from selected incident event classes.',
  },
]

const fieldClass =
  'mt-1 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100 disabled:cursor-not-allowed disabled:opacity-60'

function truncate(value: string, maxLength: number): string {
  return value.length > maxLength ? value.slice(0, maxLength) : value
}

function manualSourceKey(fact?: FactDefinitionResponse): string {
  if (!fact) return ''
  return truncate(`manual_${fact.factKey}`, 64)
}

function reportSourceKey(fact?: FactDefinitionResponse): string {
  if (!fact) return ''
  return truncate(`report_${fact.factKey}`, 64)
}

function isAccidentRegisterFact(fact?: FactDefinitionResponse): boolean {
  return fact?.factKey === ACCIDENT_REGISTER_FACT_KEY
}

function defaultReportGeneratedConfigJson(): string {
  return '{\n  "includedEventClasses": [\n    "accident"\n  ]\n}'
}

function parseIncludedEventClasses(configJson: string): string[] {
  try {
    const parsed = JSON.parse(configJson) as { includedEventClasses?: unknown }
    if (!Array.isArray(parsed.includedEventClasses)) {
      return []
    }

    return parsed.includedEventClasses
      .filter((value): value is string => typeof value === 'string')
      .map((value) => value.trim().toLowerCase())
      .filter(Boolean)
      .filter((value, index, values) => values.indexOf(value) === index)
  } catch {
    return []
  }
}

function buildReportGeneratedConfigJson(eventClasses: string[]): string {
  return JSON.stringify(
    {
      includedEventClasses: eventClasses,
    },
    null,
    2,
  )
}

function defaultConfigJson(fact?: FactDefinitionResponse, sourceType: SourceType = 'static_config'): string {
  if (sourceType === 'product_mirror') {
    return '{}'
  }

  if (sourceType === 'report_generated') {
    return defaultReportGeneratedConfigJson()
  }

  switch (fact?.valueType.toLowerCase()) {
    case 'boolean':
      return '{\n  "booleanValue": true\n}'
    case 'number':
      return '{\n  "numberValue": 0\n}'
    case 'date':
      return '{\n  "dateValue": "2026-01-01"\n}'
    default:
      return '{\n  "stringValue": "manual value"\n}'
  }
}

function buildCreateForm(fact?: FactDefinitionResponse): FactSourceFormState {
  const sourceType: SourceType = isAccidentRegisterFact(fact) ? 'report_generated' : 'static_config'

  return {
    mode: 'create',
    factDefinitionId: fact?.factDefinitionId ?? '',
    factSourceId: '',
    sourceKey: sourceType === 'report_generated' ? reportSourceKey(fact) : manualSourceKey(fact),
    sourceType,
    label: truncate(fact ? `${sourceType === 'report_generated' ? 'Generated' : 'Manual'} ${fact.label}` : '', 128),
    description: fact
      ? `${sourceType === 'report_generated' ? 'Generated report mapping' : 'Manual source mapping'} for ${fact.factKey}.`
      : '',
    productKey: sourceType === 'report_generated' ? 'reportarr' : '',
    productReference: sourceType === 'report_generated' ? ACCIDENT_REGISTER_REPORT_REFERENCE : '',
    configJson: defaultConfigJson(fact, sourceType),
    priority: '0',
    isActive: true,
  }
}

function buildEditForm(source: FactSourceResponse): FactSourceFormState {
  const sourceType = sourceTypes.some((option) => option.value === source.sourceType)
    ? (source.sourceType as SourceType)
    : 'static_config'

  return {
    mode: 'edit',
    factDefinitionId: source.factDefinitionId,
    factSourceId: source.factSourceId,
    sourceKey: source.sourceKey,
    sourceType,
    label: source.label,
    description: source.description,
    productKey: source.productKey ?? '',
    productReference: source.productReference ?? '',
    configJson: source.configJson || '{}',
    priority: String(source.priority),
    isActive: source.isActive,
  }
}

function sourceTypeLabel(value: string): string {
  return sourceTypes.find((sourceType) => sourceType.value === value)?.label ?? value
}

function sourceProductLabel(value: string | null | undefined): string {
  if (!value) return 'No source product'

  return (
    SUITE_SOURCE_PRODUCT_OPTIONS.find((option) => option.value === value)?.label ??
    value
      .replace(/[_-]+/g, ' ')
      .replace(/\b\w/g, (char) => char.toUpperCase())
  )
}

export function FactSourcesPanel({
  factDefinitions,
  factSources,
  canManage,
  onCreateFactSource,
  onUpdateFactSource,
  isSavingFactSource,
}: FactSourcesPanelProps) {
  const [form, setForm] = useState<FactSourceFormState>(() => buildCreateForm(factDefinitions[0]))
  const [formError, setFormError] = useState<string | null>(null)

  const factsById = useMemo(
    () => new Map(factDefinitions.map((fact) => [fact.factDefinitionId, fact])),
    [factDefinitions],
  )
  const selectedFact = factsById.get(form.factDefinitionId)
  const sourceReferenceOptions = useMemo(
    () => listSourceReferenceOptions(form.productKey || undefined),
    [form.productKey],
  )
  const includedEventClasses = useMemo(
    () => (form.sourceType === 'report_generated' ? parseIncludedEventClasses(form.configJson) : []),
    [form.configJson, form.sourceType],
  )
  const selectedReferenceOption = useMemo(
    () => sourceReferenceOptions.find((option) => option.value === form.productReference),
    [form.productReference, sourceReferenceOptions],
  )

  useEffect(() => {
    if (!factDefinitions.length) return

    setForm((current) => {
      if (current.factDefinitionId) return current
      return buildCreateForm(factDefinitions[0])
    })
  }, [factDefinitions])

  function resetToCreate(fact = selectedFact ?? factDefinitions[0]) {
    setForm(buildCreateForm(fact))
    setFormError(null)
  }

  function setFactDefinition(factDefinitionId: string) {
    const fact = factsById.get(factDefinitionId)
    setForm(buildCreateForm(fact))
    setFormError(null)
  }

  function setSourceType(sourceType: SourceType) {
    setForm((current) => ({
      ...current,
      sourceType,
      configJson: defaultConfigJson(selectedFact, sourceType),
      productKey:
        sourceType === 'report_generated'
          ? isAccidentRegisterFact(selectedFact) && !current.productKey
            ? 'reportarr'
            : current.productKey
          : current.sourceType === 'report_generated'
            ? ''
            : current.productKey,
      productReference:
        sourceType === 'report_generated'
          ? isAccidentRegisterFact(selectedFact) && !current.productReference
            ? ACCIDENT_REGISTER_REPORT_REFERENCE
            : current.productReference
          : current.sourceType === 'report_generated'
            ? ''
            : current.productReference,
      sourceKey:
        sourceType === 'report_generated' && isAccidentRegisterFact(selectedFact)
          ? reportSourceKey(selectedFact)
          : current.sourceType === 'report_generated'
            ? manualSourceKey(selectedFact) || current.sourceKey
            : current.sourceKey,
      label:
        sourceType === 'report_generated' && isAccidentRegisterFact(selectedFact)
          ? truncate(`Generated ${selectedFact?.label ?? 'report'}`, 128)
          : current.sourceType === 'report_generated'
            ? truncate(selectedFact ? `Manual ${selectedFact.label}` : current.label, 128)
            : current.label,
      description:
        sourceType === 'report_generated' && isAccidentRegisterFact(selectedFact)
          ? `Generated report mapping for ${selectedFact?.factKey ?? 'report'}.`
          : current.sourceType === 'report_generated'
            ? selectedFact
              ? `Manual source mapping for ${selectedFact.factKey}.`
              : current.description
            : current.description,
    }))
    setFormError(null)
  }

  function setProductKey(productKey: string) {
    setForm((current) => {
      const nextReferenceOptions = listSourceReferenceOptions(productKey || undefined)
      const nextProductReference = nextReferenceOptions.some((option) => option.value === current.productReference)
        ? current.productReference
        : ''

      return {
        ...current,
        productKey,
        productReference: nextProductReference,
      }
    })
    setFormError(null)
  }

  function setProductReference(productReference: string) {
    setForm((current) => ({
      ...current,
      productReference,
    }))
    setFormError(null)
  }

  function toggleIncludedEventClass(eventClass: string) {
    setForm((current) => {
      const selectedEventClasses = parseIncludedEventClasses(current.configJson)
      const nextSelected = selectedEventClasses.includes(eventClass)
        ? selectedEventClasses.filter((value) => value !== eventClass)
        : [...selectedEventClasses, eventClass]

      return {
        ...current,
        configJson: buildReportGeneratedConfigJson(nextSelected),
      }
    })
    setFormError(null)
  }

  async function submitFactSource(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormError(null)

    if (!canManage) {
      setFormError('You do not have permission to manage fact mappings.')
      return
    }

    const priority = Number.parseInt(form.priority, 10)
    if (!Number.isFinite(priority)) {
      setFormError('Priority must be a whole number.')
      return
    }

    const configJson = form.configJson.trim() || '{}'
    try {
      JSON.parse(configJson)
    } catch {
      setFormError('Config JSON must be valid JSON.')
      return
    }

    if (form.sourceType === 'report_generated' && parseIncludedEventClasses(configJson).length === 0) {
      setFormError('Generated report sources require at least one included event class.')
      return
    }

    try {
      if (form.mode === 'edit') {
        await onUpdateFactSource(form.factSourceId, {
          label: form.label.trim(),
          description: form.description.trim(),
          productKey: form.productKey.trim() || null,
          productReference: form.productReference.trim() || null,
          configJson,
          priority,
          isActive: form.isActive,
        })
        resetToCreate(selectedFact)
        return
      }

      if (!selectedFact) {
        setFormError('Select a fact before creating a mapping.')
        return
      }

      await onCreateFactSource({
        factDefinitionId: selectedFact.factDefinitionId,
        sourceKey: form.sourceKey.trim(),
        sourceType: form.sourceType,
        label: form.label.trim(),
        description: form.description.trim(),
        productKey: form.productKey.trim() || null,
        productReference: form.productReference.trim() || null,
        configJson,
        priority,
      })
      resetToCreate(selectedFact)
    } catch (error) {
      setFormError(error instanceof Error ? error.message : 'Fact mapping could not be saved.')
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <p className="max-w-4xl text-sm text-slate-400">
          Fact sources bind catalog facts to static configuration, product APIs, product mirrors, or generated
          reports. Compliance Core owns the mapping and rule meaning; source products still own the underlying
          operational record.
        </p>
        {form.mode === 'edit' ? (
          <button
            type="button"
            className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:border-slate-500"
            onClick={() => resetToCreate()}
          >
            New mapping
          </button>
        ) : null}
      </div>

      {canManage ? (
        <form
          className="grid gap-4 rounded-lg border border-slate-800 bg-slate-950/60 p-4 md:grid-cols-2"
          onSubmit={submitFactSource}
        >
          <div className="md:col-span-2">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
              {form.mode === 'edit'
                ? 'Edit fact mapping'
                : form.sourceType === 'report_generated'
                  ? 'Create generated report fact mapping'
                  : 'Create manual fact mapping'}
            </h2>
            <p className="mt-1 text-sm text-slate-400">
              Use this form when a normalized fact needs a manual source, product API reference, mirrored source
              reference, or generated report before evaluations can explain their result chain.
            </p>
          </div>

          <label className="block text-sm text-slate-300">
            Normalized fact
            <select
              value={form.factDefinitionId}
              onChange={(event) => setFactDefinition(event.target.value)}
              className={fieldClass}
              disabled={form.mode === 'edit' || isSavingFactSource}
              required
            >
              <option value="">Select fact...</option>
              {factDefinitions.map((fact) => (
                <option key={fact.factDefinitionId} value={fact.factDefinitionId}>
                  {fact.label} ({fact.factKey})
                </option>
              ))}
            </select>
          </label>

          <label className="block text-sm text-slate-300">
            Source type
            <select
              value={form.sourceType}
              onChange={(event) => setSourceType(event.target.value as SourceType)}
              className={fieldClass}
              disabled={form.mode === 'edit' || isSavingFactSource}
            >
              {sourceTypes.map((sourceType) => (
                <option key={sourceType.value} value={sourceType.value}>
                  {sourceType.label}
                </option>
              ))}
            </select>
            <span className="mt-1 block text-xs text-[var(--color-text-muted)]">
              {sourceTypes.find((sourceType) => sourceType.value === form.sourceType)?.hint}
            </span>
          </label>

          <label className="block text-sm text-slate-300">
            Source key
            <input
              value={form.sourceKey}
              onChange={(event) => setForm((current) => ({ ...current, sourceKey: event.target.value }))}
              className={fieldClass}
              disabled={form.mode === 'edit' || isSavingFactSource}
              maxLength={64}
              required
            />
            {form.mode === 'edit' ? (
              <span className="mt-1 block text-xs text-[var(--color-text-muted)]">Source key is immutable after creation.</span>
            ) : null}
          </label>

          <label className="block text-sm text-slate-300">
            Mapping label
            <input
              value={form.label}
              onChange={(event) => setForm((current) => ({ ...current, label: event.target.value }))}
              className={fieldClass}
              maxLength={128}
              disabled={isSavingFactSource}
              required
            />
          </label>

          <label className="md:col-span-2 block text-sm text-slate-300">
            Description
            <textarea
              value={form.description}
              onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
              className={fieldClass}
              rows={2}
              disabled={isSavingFactSource}
            />
          </label>

          <div className="space-y-1">
            <ControlledSelect
              label="Source product"
              value={form.productKey}
              onChange={setProductKey}
              options={SUITE_SOURCE_PRODUCT_OPTIONS}
              emptyLabel="No source product"
              disabled={isSavingFactSource}
            />
            <p className="text-xs text-[var(--color-text-muted)]">
              Select the owning source product first. Product references below are scoped to that product.
            </p>
          </div>

          <div className="space-y-1">
            <ControlledSelect
              label="Product reference"
              value={form.productReference}
              onChange={setProductReference}
              options={sourceReferenceOptions}
              selectedOption={selectedReferenceOption}
              emptyLabel={form.productKey ? 'Select a product reference' : 'Select a source product first'}
              disabled={isSavingFactSource || !form.productKey}
            />
            <p className="text-xs text-[var(--color-text-muted)]">
              {form.sourceType === 'report_generated'
                ? 'Choose the report reference that will be used as the generated source.'
                : form.productKey
                  ? `Scoped to ${sourceProductLabel(form.productKey)}.`
                  : 'Choose a source product to unlock its scoped reference list.'}
            </p>
          </div>

          {form.sourceType === 'report_generated' ? (
            <fieldset className="md:col-span-2 rounded-lg border border-slate-800 bg-slate-950/40 p-4">
              <legend className="px-1 text-sm font-medium text-slate-300">Included event classes</legend>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                Accident register sources should be generated from the selected event classes. Accident is selected by
                default.
              </p>
              <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                {REPORT_GENERATED_EVENT_CLASS_OPTIONS.map((option) => (
                  <label key={option.value} className="flex items-center gap-2 rounded-md border border-slate-800 bg-slate-900/60 px-3 py-2 text-sm text-slate-200">
                    <input
                      type="checkbox"
                      checked={includedEventClasses.includes(option.value)}
                      onChange={() => toggleIncludedEventClass(option.value)}
                      className="h-4 w-4 rounded border-slate-600 bg-slate-900"
                    />
                    {option.label}
                  </label>
                ))}
              </div>
            </fieldset>
          ) : null}

          <label className="block text-sm text-slate-300">
            Priority
            <input
              type="number"
              value={form.priority}
              onChange={(event) => setForm((current) => ({ ...current, priority: event.target.value }))}
              className={fieldClass}
              disabled={isSavingFactSource}
            />
          </label>

          {form.mode === 'edit' ? (
            <label className="flex items-center gap-2 self-end text-sm text-slate-300">
              <input
                type="checkbox"
                checked={form.isActive}
                onChange={(event) => setForm((current) => ({ ...current, isActive: event.target.checked }))}
                className="h-4 w-4 rounded border-slate-600 bg-slate-900"
                disabled={isSavingFactSource}
              />
              Active mapping
            </label>
          ) : null}

          <label className="md:col-span-2 block text-sm text-slate-300">
            Advanced config JSON
            <textarea
              value={form.configJson}
              onChange={(event) => setForm((current) => ({ ...current, configJson: event.target.value }))}
              className={`${fieldClass} font-mono`}
              rows={6}
              disabled={isSavingFactSource}
              spellCheck={false}
            />
          </label>

          {formError ? (
            <p className="md:col-span-2 rounded-md border border-red-900/70 bg-red-950/40 px-3 py-2 text-sm text-red-100">
              {formError}
            </p>
          ) : null}

          <div className="flex flex-wrap gap-2 md:col-span-2">
            <button
              type="submit"
              disabled={
                isSavingFactSource ||
                !form.factDefinitionId ||
                !form.label.trim() ||
                (form.mode === 'create' && !form.sourceKey.trim())
              }
              className="rounded-md bg-sky-600 px-3 py-2 text-sm font-semibold text-white hover:bg-sky-500 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isSavingFactSource
                ? 'Saving...'
                : form.mode === 'edit'
                  ? 'Save fact mapping'
                  : 'Create fact mapping'}
            </button>
            {form.mode === 'edit' ? (
              <button
                type="button"
                className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:border-slate-500"
                onClick={() => resetToCreate()}
                disabled={isSavingFactSource}
              >
                Cancel edit
              </button>
            ) : null}
          </div>
        </form>
      ) : (
        <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-4 text-sm text-slate-400">
          Fact mappings are read-only for this role.
        </div>
      )}

      <div className="grid gap-6 lg:grid-cols-2">
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Catalog facts</h2>
          {factDefinitions.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No fact definitions yet. Seed the citations & facts catalog first.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {factDefinitions.map((fact) => (
                <li key={fact.factDefinitionId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="font-medium text-slate-100">{fact.label}</p>
                  <p className="font-mono text-xs text-sky-300">{fact.factKey}</p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">{fact.valueType}</p>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Registered sources</h2>
          {factSources.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No fact sources registered yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {factSources.map((source) => (
                <li key={source.factSourceId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <p className="font-medium text-slate-100">{source.label}</p>
                      <p className="font-mono text-xs text-violet-300">{source.sourceKey}</p>
                    </div>
                    <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-400">
                      {sourceTypeLabel(source.sourceType)}
                    </span>
                  </div>
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                    {source.factKey}
                    {source.productKey ? ` · ${source.productKey}` : ''}
                    {` · priority ${source.priority}`}
                  </p>
                  {source.description ? <p className="mt-2 text-sm text-slate-300">{source.description}</p> : null}
                  {source.productReference ? (
                    <p className="mt-1 font-mono text-xs text-[var(--color-text-muted)]">{source.productReference}</p>
                  ) : null}
                  {canManage ? (
                    <button
                      type="button"
                      className="mt-3 rounded-md border border-slate-700 px-3 py-1.5 text-xs font-semibold text-slate-200 hover:border-sky-600 hover:text-sky-100"
                      onClick={() => {
                        setForm(buildEditForm(source))
                        setFormError(null)
                      }}
                      aria-label={`Edit fact mapping ${source.label}`}
                    >
                      Edit mapping
                    </button>
                  ) : null}
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </div>
  )
}
