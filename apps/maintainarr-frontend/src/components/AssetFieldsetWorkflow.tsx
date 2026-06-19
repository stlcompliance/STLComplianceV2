import type {
  AssetFieldContextResponse,
  AssetResponse,
  AssetUpsertV1Request,
  CatalogOptionResponse,
  FieldMetadataResponse,
  FieldsetResponse,
} from '../api/types'

export type AssetFieldValues = Record<string, unknown>
export type AssetFieldErrors = Record<string, string>
export type AssetFieldMode = 'create' | 'edit' | 'read'

export interface AssetWorkflowStep {
  key: string
  label: string
  description: string
  fields: FieldMetadataResponse[]
  isReview?: boolean
}

const baselineFieldKeys = new Set([
  'unitNumber',
  'assetNumber',
  'displayName',
  'assetClass',
  'assetType',
  'assetStatus',
  'status',
  'lifecycleStatus',
  'primaryMeterType',
  'meterType',
  'siteId',
  'homeLocationId',
  'criticality',
])

const sectionOrder = [
  'identity',
  'classification',
  'assignment',
  'configuration',
  'usage',
  'maintenance',
  'inspection',
  'compliance',
  'documents',
  'components',
  'telematics',
  'free_text',
]

const sectionLabels: Record<string, { label: string; description: string; empty: string; action: string }> = {
  identity: {
    label: 'Overview',
    description: 'Core identity, classification, and operating state.',
    empty: 'No identity details recorded yet.',
    action: 'Complete overview',
  },
  classification: {
    label: 'Classification and Identity',
    description: 'Make, model, year, VIN, serial, and identifiers that describe the asset.',
    empty: 'No classification details recorded yet.',
    action: 'Add identity details',
  },
  assignment: {
    label: 'Assignment and Location',
    description: 'Site, team, responsible people, vendor, and customer context.',
    empty: 'No assignment or location data yet.',
    action: 'Assign asset',
  },
  configuration: {
    label: 'Physical Configuration',
    description: 'Fuel, brake, axle, tire, cab, trailer, and equipment configuration.',
    empty: 'No physical configuration recorded yet.',
    action: 'Add configuration',
  },
  usage: {
    label: 'Meters and Usage',
    description: 'Primary meter, usage profile, and reading behavior.',
    empty: 'No meter or usage data recorded yet.',
    action: 'Add meter reading',
  },
  maintenance: {
    label: 'Maintenance Applicability',
    description: 'PM program, PM type, and service applicability.',
    empty: 'No PM program assigned.',
    action: 'Assign PM program',
  },
  inspection: {
    label: 'Inspection Applicability',
    description: 'Inspection templates, inspection types, and requirement cadence.',
    empty: 'No inspection templates assigned.',
    action: 'Assign inspection template',
  },
  compliance: {
    label: 'Regulatory / Compliance Context',
    description: 'Compliance Core references, rulepacks, categories, and evidence requirements.',
    empty: 'No regulatory context selected.',
    action: 'Add regulatory context',
  },
  documents: {
    label: 'Documents and Evidence',
    description: 'Document types, certificates, registrations, and evidence expectations.',
    empty: 'No documents uploaded yet.',
    action: 'Upload document',
  },
  components: {
    label: 'Components / Fitment',
    description: 'Major components, installed equipment, fitment, and preferred parts.',
    empty: 'No component or fitment data recorded yet.',
    action: 'Add component',
  },
  telematics: {
    label: 'Telematics / Diagnostics',
    description: 'Telematics provider, device mapping, diagnostics, and sync state.',
    empty: 'No telematics or diagnostic data connected.',
    action: 'Connect provider',
  },
  free_text: {
    label: 'Notes',
    description: 'Description, internal notes, operator notes, and maintenance notes.',
    empty: 'No notes recorded yet.',
    action: 'Add note',
  },
}

const missingFieldLabels: Record<string, string> = {
  VIN: 'VIN not recorded',
  serialNumber: 'Serial number not recorded',
  licensePlate: 'License plate not recorded',
  unitNumber: 'Unit number not recorded',
  assetNumber: 'Asset number not recorded',
  fleetNumber: 'Fleet number not recorded',
  governingBodyKey: 'No governing body selected',
  rulepackApplicabilityKeys: 'No rulepack applicability selected',
  siteId: 'No site assigned',
  homeLocationId: 'No home location assigned',
  assignedPersonId: 'No assigned person',
  defaultTechnicianPersonId: 'No default technician',
}

function toStringValue(value: unknown): string {
  if (value == null) return ''
  if (Array.isArray(value)) return value.join(', ')
  return String(value)
}

function toValueList(value: unknown): string[] {
  if (Array.isArray(value)) {
    return value.map((item) => String(item)).filter(Boolean)
  }
  const text = toStringValue(value).trim()
  return text ? [text] : []
}

function hasMeaningfulValue(value: unknown): boolean {
  if (Array.isArray(value)) return value.length > 0
  if (value == null) return false
  return String(value).trim().length > 0
}

function normalizeValidationNumber(value: unknown): number | null {
  if (typeof value === 'number') return value
  if (typeof value === 'string' && value.trim()) {
    const parsed = Number(value)
    return Number.isFinite(parsed) ? parsed : null
  }
  return null
}

function humanizeKey(key: string): string {
  return key
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[_-]+/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()
    .replace(/\b\w/g, (char) => char.toUpperCase())
}

function sectionMeta(sectionKey: string) {
  return sectionLabels[sectionKey] ?? {
    label: humanizeKey(sectionKey),
    description: 'Asset metadata from the configured fieldset.',
    empty: `No ${humanizeKey(sectionKey).toLowerCase()} data recorded yet.`,
    action: `Add ${humanizeKey(sectionKey).toLowerCase()}`,
  }
}

function fieldValueByCatalogKey(fieldset: FieldsetResponse, values: AssetFieldValues, catalogKey: string): unknown {
  const match = fieldset.fields.find((field) => field.catalogKey === catalogKey || field.key === catalogKey)
  return match ? values[match.key] : values[catalogKey]
}

function visibilityMatches(expected: unknown, actual: unknown): boolean {
  const actualValues = toValueList(actual).map((value) => value.toLowerCase())
  if (actualValues.length === 0) return false

  if (Array.isArray(expected)) {
    return expected.map(String).some((item) => actualValues.includes(item.toLowerCase()))
  }

  if (typeof expected === 'object' && expected !== null) {
    const rule = expected as Record<string, unknown>
    if (Array.isArray(rule.anyOf)) return visibilityMatches(rule.anyOf, actual)
    if (rule.equals !== undefined) return visibilityMatches(rule.equals, actual)
    if (rule.notEquals !== undefined) return !visibilityMatches(rule.notEquals, actual)
  }

  return actualValues.includes(String(expected).toLowerCase())
}

export function fieldIsVisible(
  fieldset: FieldsetResponse,
  field: FieldMetadataResponse,
  values: AssetFieldValues,
): boolean {
  const visibility = field.visibility
  if (!visibility || Object.keys(visibility).length === 0) return true

  return Object.entries(visibility).every(([driverKey, expected]) => {
    const actual = fieldValueByCatalogKey(fieldset, values, driverKey)
    return visibilityMatches(expected, actual)
  })
}

export function getFilteredOptions(
  fieldset: FieldsetResponse,
  field: FieldMetadataResponse,
  values: AssetFieldValues,
): CatalogOptionResponse[] {
  const options = field.options ?? []
  if (options.length === 0) return []

  return options.filter((option) => {
    const dependencies = option.dependency ?? {}
    return Object.entries(dependencies).every(([catalogKey, optionKey]) => {
      const selected = toValueList(fieldValueByCatalogKey(fieldset, values, catalogKey))
      return selected.some((value) => value.toLowerCase() === optionKey.toLowerCase())
    })
  })
}

export function initializeAssetFieldValues(fieldset: FieldsetResponse): AssetFieldValues {
  return Object.fromEntries(
    fieldset.fields.map((field) => {
      if (field.defaultValue !== null && field.defaultValue !== undefined) {
        return [field.key, field.defaultValue]
      }
      return [field.key, field.control === 'multiSelect' ? [] : '']
    }),
  )
}

export function valuesFromFieldContext(
  asset: AssetResponse,
  fieldContext: AssetFieldContextResponse | null,
): { values: AssetFieldValues; displayValues: Record<string, string> } {
  const values: AssetFieldValues = {
    unitNumber: asset.assetTag,
    assetNumber: asset.assetTag,
    displayName: asset.name,
    description: asset.description,
    assetClass: asset.classKey,
    assetType: asset.typeKey,
    lifecycleStatus: asset.lifecycleStatus,
    siteId: asset.siteRef ?? '',
  }
  const displayValues: Record<string, string> = {
    unitNumber: asset.assetTag,
    assetNumber: asset.assetTag,
    displayName: asset.name,
    description: asset.description,
    assetClass: asset.className,
    assetType: asset.typeName,
    lifecycleStatus: humanizeKey(asset.lifecycleStatus),
    siteId: asset.siteRef ?? '',
  }

  for (const field of fieldContext?.fields ?? []) {
    values[field.key] = field.storedValue ?? ''
    displayValues[field.key] = field.displayValue ?? ''
  }

  return { values, displayValues }
}

export function validateAssetValues(
  fieldset: FieldsetResponse,
  values: AssetFieldValues,
  fields: FieldMetadataResponse[] = fieldset.fields,
): AssetFieldErrors {
  const errors: AssetFieldErrors = {}
  for (const field of fields) {
    if (!fieldIsVisible(fieldset, field, values)) continue
    const value = values[field.key]
    const valueList = toValueList(value)

    if (field.required && valueList.length === 0) {
      errors[field.key] = `${field.label} is required.`
      continue
    }

    if (valueList.length === 0) continue

    const validation = field.validation ?? {}
    const maxLength = normalizeValidationNumber(validation.maxLength)
    const minLength = normalizeValidationNumber(validation.minLength)
    const min = normalizeValidationNumber(validation.min)
    const max = normalizeValidationNumber(validation.max)

    for (const item of valueList) {
      if (maxLength !== null && item.length > maxLength) {
        errors[field.key] = `${field.label} must be ${maxLength} characters or fewer.`
        break
      }
      if (minLength !== null && item.length < minLength) {
        errors[field.key] = `${field.label} must be at least ${minLength} characters.`
        break
      }
      if (typeof validation.pattern === 'string' && validation.pattern) {
        const pattern = new RegExp(validation.pattern)
        if (!pattern.test(item)) {
          errors[field.key] = `${field.label} format is invalid.`
          break
        }
      }
      if (field.type === 'number' || field.type === 'integer' || field.control === 'number') {
        const parsed = Number(item)
        if (!Number.isFinite(parsed)) {
          errors[field.key] = `${field.label} must be a number.`
          break
        }
        if (field.type === 'integer' && !Number.isInteger(parsed)) {
          errors[field.key] = `${field.label} must be a whole number.`
          break
        }
        if (min !== null && parsed < min) {
          errors[field.key] = `${field.label} must be at least ${min}.`
          break
        }
        if (max !== null && parsed > max) {
          errors[field.key] = `${field.label} must be ${max} or less.`
          break
        }
      }
    }
  }
  return errors
}

export function buildAssetUpsertPayload(values: AssetFieldValues): AssetUpsertV1Request {
  const assetTag = toStringValue(values.unitNumber || values.assetNumber).trim()
  const name = toStringValue(values.displayName).trim() || assetTag
  const description = toStringValue(values.description).trim() || null

  return {
    assetTag,
    name,
    description,
    values,
  }
}

export function getCreateWorkflowSteps(fieldset: FieldsetResponse, values: AssetFieldValues): AssetWorkflowStep[] {
  const visibleFields = fieldset.fields.filter((field) => fieldIsVisible(fieldset, field, values))
  const basics = visibleFields.filter((field) => baselineFieldKeys.has(field.key) || field.required)
  const basicKeys = new Set(basics.map((field) => field.key))
  const steps: AssetWorkflowStep[] = [
    {
      key: 'basics',
      label: 'Required Basics',
      description: 'Create the useful asset record first. Deeper operational sections unlock after this is valid.',
      fields: basics,
    },
  ]

  const sections = new Map<string, FieldMetadataResponse[]>()
  for (const field of visibleFields) {
    if (basicKeys.has(field.key)) continue
    const current = sections.get(field.sectionKey) ?? []
    current.push(field)
    sections.set(field.sectionKey, current)
  }

  for (const sectionKey of sectionOrder) {
    const fields = sections.get(sectionKey)
    if (!fields || fields.length === 0) continue
    const meta = sectionMeta(sectionKey)
    steps.push({
      key: sectionKey,
      label: sectionKey === 'classification' ? 'Classification and Identity' : meta.label,
      description: meta.description,
      fields,
    })
  }

  for (const [sectionKey, fields] of sections.entries()) {
    if (sectionOrder.includes(sectionKey) || fields.length === 0) continue
    const meta = sectionMeta(sectionKey)
    steps.push({ key: sectionKey, label: meta.label, description: meta.description, fields })
  }

  steps.push({
    key: 'review',
    label: 'Review and Create',
    description: 'Review the baseline and any optional enrichment you added.',
    fields: [],
    isReview: true,
  })

  return steps
}

export function isCreateBaselineComplete(fieldset: FieldsetResponse, values: AssetFieldValues): boolean {
  const basics = getCreateWorkflowSteps(fieldset, values)[0]?.fields ?? []
  const errors = validateAssetValues(fieldset, values, basics)
  return basics
    .filter((field) => field.required || baselineFieldKeys.has(field.key))
    .filter((field) => field.required || ['unitNumber', 'assetNumber', 'assetClass', 'assetType', 'assetStatus', 'lifecycleStatus'].includes(field.key))
    .every((field) => hasMeaningfulValue(values[field.key]) && !errors[field.key])
}

function controlTypeForField(field: FieldMetadataResponse): string {
  if (field.control === 'searchableSelect' || field.control === 'asyncCombobox') return 'select'
  if (field.control === 'number' || field.type === 'number' || field.type === 'integer') return 'number'
  if (field.type === 'date') return 'date'
  return field.control
}

export function formatAssetFieldValue(
  fieldset: FieldsetResponse,
  field: FieldMetadataResponse,
  value: unknown,
  values: AssetFieldValues,
  displayValues?: Record<string, string>,
): string {
  const displayValue = displayValues?.[field.key]
  if (displayValue && displayValue.trim()) return displayValue

  const valueList = toValueList(value)
  if (valueList.length === 0) return missingFieldLabels[field.key] ?? 'No data yet'

  const options = getFilteredOptions(fieldset, field, values)
  if (options.length === 0) return valueList.join(', ')

  const optionLabels = new Map<string, string>()
  for (const option of options) {
    optionLabels.set(option.key.toLowerCase(), option.label)
    const externalId = option.metadata?.externalId
    if (externalId) optionLabels.set(String(externalId).toLowerCase(), option.label)
  }

  return valueList
    .map((item) => optionLabels.get(item.toLowerCase()) ?? item)
    .join(', ')
}

function AssetFieldControl({
  fieldset,
  field,
  value,
  values,
  error,
  mode,
  onChange,
  displayValues,
}: {
  fieldset: FieldsetResponse
  field: FieldMetadataResponse
  value: unknown
  values: AssetFieldValues
  error?: string
  mode: AssetFieldMode
  onChange?: (fieldKey: string, value: unknown) => void
  displayValues?: Record<string, string>
}) {
  const id = `asset-field-${field.key}`

  if (mode === 'read') {
    return (
      <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
        <dt className="text-xs font-medium text-slate-400">{field.label}</dt>
        <dd className="mt-1 text-sm font-medium text-slate-100">
          {formatAssetFieldValue(fieldset, field, value, values, displayValues)}
        </dd>
      </div>
    )
  }

  const options = getFilteredOptions(fieldset, field, values)
  const controlType = controlTypeForField(field)
  const commonClass =
    'w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 outline-none focus:border-sky-400'
  const disabled = !onChange

  let control
  if (controlType === 'multiSelect') {
    const selected = toValueList(value)
    control = (
      <select
        id={id}
        multiple
        value={selected}
        disabled={disabled}
        onChange={(event) => {
          const next = Array.from(event.target.selectedOptions).map((option) => option.value)
          onChange?.(field.key, next)
        }}
        className={`${commonClass} min-h-28`}
      >
        {options.map((option) => (
          <option key={option.key} value={option.key}>
            {option.label}
          </option>
        ))}
      </select>
    )
  } else if (controlType === 'select') {
    control = (
      <select
        id={id}
        value={toStringValue(value)}
        disabled={disabled}
        onChange={(event) => onChange?.(field.key, event.target.value)}
        className={commonClass}
      >
        <option value="">{field.required ? 'Select value' : 'Not selected'}</option>
        {options.map((option) => (
          <option key={option.key} value={option.key}>
            {option.label}
          </option>
        ))}
      </select>
    )
  } else if (controlType === 'textArea') {
    control = (
      <textarea
        id={id}
        value={toStringValue(value)}
        disabled={disabled}
        onChange={(event) => onChange?.(field.key, event.target.value)}
        rows={4}
        className={commonClass}
      />
    )
  } else {
    control = (
      <input
        id={id}
        type={controlType === 'number' || controlType === 'date' ? controlType : 'text'}
        value={toStringValue(value)}
        disabled={disabled}
        onChange={(event) => onChange?.(field.key, event.target.value)}
        className={commonClass}
      />
    )
  }

  return (
    <div className={controlType === 'multiSelect' || controlType === 'textArea' ? 'md:col-span-2' : ''}>
      <label htmlFor={id} className="text-sm font-medium text-slate-200">
        {field.label}
        {field.required ? <span className="text-amber-300"> *</span> : null}
      </label>
      {field.description ? <p className="mt-1 text-xs text-[var(--color-text-muted)]">{field.description}</p> : null}
      <div className="mt-2">{control}</div>
      {options.length === 0 && (field.catalogKey || field.referenceKey) ? (
        <p className="mt-1 text-xs text-amber-200">No selectable values are available for the current context.</p>
      ) : null}
      {error ? <p className="mt-1 text-xs text-red-300">{error}</p> : null}
    </div>
  )
}

export function AssetFieldsetFields({
  fieldset,
  fields,
  values,
  errors = {},
  mode,
  onChange,
  displayValues,
}: {
  fieldset: FieldsetResponse
  fields: FieldMetadataResponse[]
  values: AssetFieldValues
  errors?: AssetFieldErrors
  mode: AssetFieldMode
  onChange?: (fieldKey: string, value: unknown) => void
  displayValues?: Record<string, string>
}) {
  const visibleFields = fields.filter((field) => fieldIsVisible(fieldset, field, values))
  if (visibleFields.length === 0) {
    return null
  }

  if (mode === 'read') {
    return (
      <dl className="grid gap-3 md:grid-cols-2">
        {visibleFields.map((field) => (
          <AssetFieldControl
            key={field.key}
            fieldset={fieldset}
            field={field}
            value={values[field.key]}
            values={values}
            mode={mode}
            displayValues={displayValues}
          />
        ))}
      </dl>
    )
  }

  return (
    <div className="grid gap-4 md:grid-cols-2">
      {visibleFields.map((field) => (
        <AssetFieldControl
          key={field.key}
          fieldset={fieldset}
          field={field}
          value={values[field.key]}
          values={values}
          error={errors[field.key]}
          mode={mode}
          onChange={onChange}
          displayValues={displayValues}
        />
      ))}
    </div>
  )
}

export function AssetSectionList({
  fieldset,
  values,
  mode,
  onChange,
  errors = {},
  displayValues,
  sectionKeys,
}: {
  fieldset: FieldsetResponse
  values: AssetFieldValues
  mode: AssetFieldMode
  onChange?: (fieldKey: string, value: unknown) => void
  errors?: AssetFieldErrors
  displayValues?: Record<string, string>
  sectionKeys?: string[]
}) {
  const orderedKeys = sectionKeys ?? [
    ...sectionOrder,
    ...fieldset.fields
      .map((field) => field.sectionKey)
      .filter((key) => !sectionOrder.includes(key)),
  ]
  const uniqueKeys = Array.from(new Set(orderedKeys))

  return (
    <div className="space-y-4">
      {uniqueKeys.map((sectionKey) => {
        const meta = sectionMeta(sectionKey)
        const fields = fieldset.fields.filter(
          (field) => field.sectionKey === sectionKey && fieldIsVisible(fieldset, field, values),
        )
        const hasData = fields.some((field) => hasMeaningfulValue(values[field.key]))
        const hasImportantMissingFields = fields.some((field) => missingFieldLabels[field.key])

        if (fields.length === 0 && mode !== 'read') return null
        if (fields.length === 0 && mode === 'read') return null

        return (
          <section key={sectionKey} className="rounded-xl border border-slate-800 bg-slate-900/60 p-5">
            <div className="mb-4">
              <h2 className="text-lg font-semibold text-white">{meta.label}</h2>
              <p className="mt-1 text-sm text-slate-400">{meta.description}</p>
            </div>
            {mode === 'read' && !hasData && !hasImportantMissingFields ? (
              <div className="rounded-lg border border-slate-800 bg-slate-950/70 p-4">
                <p className="text-sm font-medium text-slate-200">{meta.empty}</p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">{meta.action}</p>
              </div>
            ) : (
              <AssetFieldsetFields
                fieldset={fieldset}
                fields={fields}
                values={values}
                errors={errors}
                mode={mode}
                onChange={onChange}
                displayValues={displayValues}
              />
            )}
          </section>
        )
      })}
    </div>
  )
}

export function AssetReviewPanel({
  fieldset,
  values,
  displayValues,
}: {
  fieldset: FieldsetResponse
  values: AssetFieldValues
  displayValues?: Record<string, string>
}) {
  const sectionsWithValues = sectionOrder
    .map((sectionKey) => ({
      sectionKey,
      fields: fieldset.fields.filter(
        (field) =>
          field.sectionKey === sectionKey
          && fieldIsVisible(fieldset, field, values)
          && hasMeaningfulValue(values[field.key]),
      ),
    }))
    .filter((section) => section.fields.length > 0)

  return (
    <div className="space-y-4">
      {sectionsWithValues.length === 0 ? (
        <p className="rounded-lg border border-slate-800 bg-slate-950/70 p-4 text-sm text-slate-400">
          Complete required basics to review this asset.
        </p>
      ) : null}
      {sectionsWithValues.map(({ sectionKey, fields }) => {
        const meta = sectionMeta(sectionKey)
        return (
          <section key={sectionKey} className="rounded-xl border border-slate-800 bg-slate-900/60 p-5">
            <h2 className="text-lg font-semibold text-white">{meta.label}</h2>
            <div className="mt-4">
              <AssetFieldsetFields
                fieldset={fieldset}
                fields={fields}
                values={values}
                mode="read"
                displayValues={displayValues}
              />
            </div>
          </section>
        )
      })}
    </div>
  )
}
