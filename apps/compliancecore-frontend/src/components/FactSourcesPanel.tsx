import { useEffect, useMemo, useState, type FormEvent } from 'react'
import type {
  CreateFactSourceRequest,
  FactDefinitionResponse,
  FactSourceResponse,
  UpdateFactSourceRequest,
} from '../api/types'

type SourceType = 'static_config' | 'product_api' | 'product_mirror'
type FormMode = 'create' | 'edit'

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

function defaultConfigJson(fact?: FactDefinitionResponse, sourceType: SourceType = 'static_config'): string {
  if (sourceType === 'product_mirror') {
    return '{}'
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
  return {
    mode: 'create',
    factDefinitionId: fact?.factDefinitionId ?? '',
    factSourceId: '',
    sourceKey: manualSourceKey(fact),
    sourceType: 'static_config',
    label: truncate(fact ? `Manual ${fact.label}` : '', 128),
    description: fact ? `Manual source mapping for ${fact.factKey}.` : '',
    productKey: '',
    productReference: '',
    configJson: defaultConfigJson(fact),
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
    }))
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
          Fact sources bind catalog facts to static configuration, product APIs, or product mirrors. Compliance Core
          owns the mapping and rule meaning; source products still own the underlying operational record.
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
              {form.mode === 'edit' ? 'Edit fact mapping' : 'Create manual fact mapping'}
            </h2>
            <p className="mt-1 text-sm text-slate-400">
              Use this form when a normalized fact needs a manual source, product API reference, or mirrored source
              reference before evaluations can explain their result chain.
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
            <span className="mt-1 block text-xs text-slate-500">
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
              <span className="mt-1 block text-xs text-slate-500">Source key is immutable after creation.</span>
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

          <label className="block text-sm text-slate-300">
            Source product
            <input
              value={form.productKey}
              onChange={(event) => setForm((current) => ({ ...current, productKey: event.target.value }))}
              className={fieldClass}
              placeholder="trainarr, recordarr, loadarr"
              disabled={isSavingFactSource}
            />
          </label>

          <label className="block text-sm text-slate-300">
            Product reference
            <input
              value={form.productReference}
              onChange={(event) => setForm((current) => ({ ...current, productReference: event.target.value }))}
              className={fieldClass}
              placeholder="API path, field, event, mirror key, or document class"
              disabled={isSavingFactSource}
              maxLength={256}
            />
          </label>

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
                  <p className="mt-1 text-xs text-slate-500">{fact.valueType}</p>
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
                  <p className="mt-2 text-xs text-slate-500">
                    {source.factKey}
                    {source.productKey ? ` · ${source.productKey}` : ''}
                    {` · priority ${source.priority}`}
                  </p>
                  {source.description ? <p className="mt-2 text-sm text-slate-300">{source.description}</p> : null}
                  {source.productReference && (
                    <p className="mt-1 font-mono text-xs text-slate-500">{source.productReference}</p>
                  )}
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
