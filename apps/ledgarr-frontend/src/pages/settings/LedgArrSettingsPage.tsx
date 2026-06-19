import { type ReactNode, useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { AlertTriangle, History, RotateCcw, Save, Settings2 } from 'lucide-react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  getLedgArrTenantSettings,
  getLedgArrTenantSettingsAudit,
  getLedgArrTenantSettingsOptions,
  resetLedgArrTenantSettingsSection,
  updateLedgArrTenantSettingsSection,
  validateLedgArrTenantSettingsSection,
  type LedgArrSettingsAuditItem,
  type LedgArrSettingsOptionsResponse,
} from '../../api/client'

const SECTION_HELPERS: Record<string, string> = {
  generalLedger: 'Configure tenant-level accounting, posting, close, and core control behavior.',
  legalEntities: 'LedgArr legal entities are accounting and business entities. Compliance governing bodies such as FMCSA, OSHA, EPA, and MSHA are owned by Compliance Core.',
  chartOfAccounts: 'Default account mappings must reference LedgArr-owned GL accounts by account code.',
  dimensions: 'Dimensions can reference records from other STL products for financial reporting, but LedgArr does not own those records.',
  postingSources: 'Source products submit finance packets to LedgArr. LedgArr validates and posts accounting entries while preserving each product’s source-of-truth ownership.',
  tax: 'Tax accounting is configured in LedgArr while RecordArr owns evidence storage and Compliance Core owns rule meaning.',
  evidence: 'RecordArr stores financial documents and evidence. LedgArr settings here govern when attachments are required.',
}

const ENUM_OPTIONS: Record<string, string[]> = {
  accountingBasis: ['accrual', 'cash', 'hybrid'],
  exchangeRateProvider: ['manual', 'ecb', 'xe', 'none'],
  exchangeRateLockPolicy: ['allowChanges', 'lockOnPreview', 'lockOnPost'],
  fiscalCalendarType: ['calendarMonth', 'fourFourFive', 'custom'],
  periodCloseMode: ['softCloseThenHardClose', 'hardCloseOnly', 'manual'],
  intercompanyBalancingMode: ['dueToDueFrom', 'clearing', 'manual'],
  chartOfAccountsTemplate: ['stlStandard', 'manufacturing', 'logistics', 'service'],
  accountNumberingScheme: ['numeric', 'segmented'],
  inventoryValuationMethod: ['fifo', 'weightedAverage', 'standardCost', 'specificIdentification'],
  revenueRecognitionMode: ['pointInTime', 'overTime', 'manual'],
  creditLimitEnforcement: ['none', 'warn', 'block'],
  creditHoldEnforcement: ['none', 'warn', 'block'],
  taxCalculationMode: ['manual', 'internalTable', 'externalProvider'],
  taxRoundingMethod: ['nearest', 'up', 'down'],
  taxPostingDatePolicy: ['transactionDate', 'invoiceDate', 'periodEnd'],
  intercompanySettlementFrequency: ['daily', 'weekly', 'monthly', 'manual'],
  retentionPolicy: ['standard', 'extended', 'permanent'],
  ledgarrOperatingMode: ['systemOfRecord', 'subledgerOnly', 'externalErpMirror', 'disabled'],
  externalErpProvider: ['quickbooks', 'netsuite', 'sage', 'xero', 'microsoftDynamics', 'sap', 'custom', 'none'],
  syncDirection: ['outboundOnly', 'inboundOnly', 'bidirectional', 'none'],
  postingBatchMode: ['realTime', 'scheduled', 'manualReview'],
  defaultFinancialStatementBasis: ['accrual', 'cash', 'hybrid'],
  defaultComparisonPeriod: ['priorMonth', 'priorQuarter', 'priorYear'],
}

const MULTI_ENUM_OPTIONS: Record<string, string[]> = {
  enabledDimensions: ['legalEntity', 'costCenter', 'department', 'site', 'location', 'customer', 'vendor', 'order', 'purchaseOrder', 'workOrder', 'asset', 'route', 'trip', 'load', 'warehouse', 'project', 'productLine', 'serviceLine', 'person', 'equipmentClass', 'revenueCategory', 'expenseCategory'],
  supportedLegalEntityTypes: ['company', 'division', 'branch'],
  enabledDepreciationMethods: ['straightLine', 'doubleDecliningBalance', 'unitsOfProduction'],
  requiredReasonCodes: ['void', 'reversal', 'writeOff', 'closeOverride', 'paymentHoldRelease', 'creditHoldOverride', 'manualJournal'],
  closeTasksByModule: ['gl', 'ap', 'ar', 'inventory', 'fixedAssets', 'banking', 'tax', 'payrollExport', 'intercompany'],
  enabledPaymentMethods: ['ach', 'check', 'wire', 'card'],
}

type SaveState = {
  kind: 'idle' | 'saving' | 'success' | 'error'
  message?: string
}

type SettingsPageProps = {
  accessToken: string
  canManage: boolean
}

type ReferenceValue = {
  productKey: string
  objectType: string
  publicKey: string
  displayName: string
}

export function LedgArrSettingsPage({ accessToken, canManage }: SettingsPageProps) {
  const settingsQuery = useQuery({
    queryKey: ['ledgarr', 'settings', accessToken],
    queryFn: () => getLedgArrTenantSettings(accessToken),
    enabled: Boolean(accessToken),
  })
  const optionsQuery = useQuery({
    queryKey: ['ledgarr', 'settings-options', accessToken],
    queryFn: () => getLedgArrTenantSettingsOptions(accessToken),
    enabled: Boolean(accessToken),
  })

  const sections = settingsQuery.data?.sections ?? []
  const [selectedSectionKey, setSelectedSectionKey] = useState<string>('generalLedger')
  const [drafts, setDrafts] = useState<Record<string, Record<string, unknown>>>({})
  const [errors, setErrors] = useState<Record<string, Record<string, string[]>>>({})
  const [saveState, setSaveState] = useState<Record<string, SaveState>>({})
  const [reasonBySection, setReasonBySection] = useState<Record<string, string>>({})
  const [showAudit, setShowAudit] = useState<Record<string, boolean>>({})

  const currentSection = sections.find((section) => section.sectionKey === selectedSectionKey) ?? sections[0] ?? null
  const currentDraft = currentSection ? drafts[currentSection.sectionKey] ?? structuredClone(currentSection.value) : null

  const auditQuery = useQuery({
    queryKey: ['ledgarr', 'settings-audit', accessToken, currentSection?.sectionKey],
    queryFn: () => getLedgArrTenantSettingsAudit(accessToken, currentSection!.sectionKey),
    enabled: Boolean(accessToken && currentSection && showAudit[currentSection.sectionKey]),
  })

  const dirtySections = useMemo(() => {
    const result = new Set<string>()
    for (const section of sections) {
      const draft = drafts[section.sectionKey]
      if (draft && JSON.stringify(draft) !== JSON.stringify(section.value)) {
        result.add(section.sectionKey)
      }
    }
    return result
  }, [drafts, sections])

  async function saveCurrentSection() {
    if (!currentSection || !currentDraft) {
      return
    }

    setSaveState((state) => ({ ...state, [currentSection.sectionKey]: { kind: 'saving' } }))
    const reason = reasonBySection[currentSection.sectionKey] ?? ''
    try {
      const validation = await validateLedgArrTenantSettingsSection(accessToken, currentSection.sectionKey, {
        value: currentDraft,
        expectedRowVersion: currentSection.rowVersion,
        reason,
      })
      if (!validation.isValid) {
        setErrors((state) => ({ ...state, [currentSection.sectionKey]: validation.errors }))
        setSaveState((state) => ({
          ...state,
          [currentSection.sectionKey]: { kind: 'error', message: 'Validation failed. Review the highlighted fields.' },
        }))
        return
      }

      await updateLedgArrTenantSettingsSection(accessToken, currentSection.sectionKey, {
        value: currentDraft,
        expectedRowVersion: currentSection.rowVersion,
        reason,
      })
      setErrors((state) => ({ ...state, [currentSection.sectionKey]: {} }))
      setSaveState((state) => ({ ...state, [currentSection.sectionKey]: { kind: 'success', message: 'Section saved.' } }))
      await settingsQuery.refetch()
    } catch (error) {
      setSaveState((state) => ({
        ...state,
        [currentSection.sectionKey]: { kind: 'error', message: getErrorMessage(error, 'Failed to save section.') },
      }))
    }
  }

  async function resetCurrentSection() {
    if (!currentSection) {
      return
    }
    if (!window.confirm(`Reset ${currentSection.displayName} to LedgArr defaults?`)) {
      return
    }

    try {
      await resetLedgArrTenantSettingsSection(accessToken, currentSection.sectionKey, reasonBySection[currentSection.sectionKey] ?? '')
      setSaveState((state) => ({ ...state, [currentSection.sectionKey]: { kind: 'success', message: 'Section reset to defaults.' } }))
      setDrafts((state) => {
        const next = { ...state }
        delete next[currentSection.sectionKey]
        return next
      })
      await settingsQuery.refetch()
    } catch (error) {
      setSaveState((state) => ({
        ...state,
        [currentSection.sectionKey]: { kind: 'error', message: getErrorMessage(error, 'Failed to reset section.') },
      }))
    }
  }

  function updateDraft(sectionKey: string, nextValue: Record<string, unknown>) {
    setDrafts((state) => ({ ...state, [sectionKey]: nextValue }))
  }

  if (settingsQuery.isLoading || optionsQuery.isLoading) {
    return (
      <div className="ledgarr-page">
        <div className="space-y-4">
          <div className="h-8 w-64 animate-pulse rounded bg-slate-800" />
          <div className="grid gap-4 lg:grid-cols-[280px_minmax(0,1fr)]">
            <div className="space-y-3 rounded-xl border border-slate-800 bg-slate-900/70 p-4">
              <div className="h-5 w-32 animate-pulse rounded bg-slate-800" />
              {Array.from({ length: 8 }).map((_, index) => (
                <div key={index} className="h-10 animate-pulse rounded bg-slate-800/80" />
              ))}
            </div>
            <div className="space-y-3 rounded-xl border border-slate-800 bg-slate-900/70 p-4">
              <div className="h-6 w-48 animate-pulse rounded bg-slate-800" />
              {Array.from({ length: 10 }).map((_, index) => (
                <div key={index} className="h-12 animate-pulse rounded bg-slate-800/80" />
              ))}
            </div>
          </div>
        </div>
      </div>
    )
  }

  if (settingsQuery.isError || optionsQuery.isError) {
    return (
      <div className="ledgarr-page">
        <ApiErrorCallout
          title="Unable to load LedgArr settings"
          message={getErrorMessage(settingsQuery.error ?? optionsQuery.error, 'Failed to load LedgArr settings.')}
        />
      </div>
    )
  }

  if (!currentSection || !currentDraft) {
    return (
      <div className="ledgarr-page">
        <div className="rounded-xl border border-dashed border-slate-700 p-6 text-sm text-slate-400">
          No LedgArr settings sections are available for this tenant.
        </div>
      </div>
    )
  }

  const sectionErrors = errors[currentSection.sectionKey] ?? {}
  const state = saveState[currentSection.sectionKey] ?? { kind: 'idle' as const }
  const requiresReason = currentSection.highImpactFields.some((field) => dirtyFieldSet(currentSection.value, currentDraft).has(field))

  return (
    <div className="ledgarr-page">
      <div className="flex flex-col gap-3 border-b border-slate-700/70 pb-4 lg:flex-row lg:items-end lg:justify-between">
        <div className="space-y-2">
          <p className="ledgarr-label">LedgArr Settings</p>
          <h1 className="text-2xl font-semibold text-slate-50">Tenant accounting configuration</h1>
          <p className="max-w-4xl text-sm text-slate-300">
            Configure tenant-level accounting, posting, close, evidence, and ERP behavior inside LedgArr without hardcoded accounting assumptions.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <span className="ledgarr-pill">{canManage ? 'Manage enabled' : 'Read only'}</span>
          {dirtySections.size > 0 ? <span className="ledgarr-pill warning">{dirtySections.size} unsaved section{dirtySections.size === 1 ? '' : 's'}</span> : null}
        </div>
      </div>

      {!canManage ? (
        <div className="mt-4 rounded-xl border border-slate-700 bg-slate-900/80 p-4 text-sm text-slate-300">
          You can view LedgArr tenant settings, but editing is disabled because this session does not have `ledgarr.settings.manage`.
        </div>
      ) : null}

      <div className="mt-6 grid gap-4 lg:grid-cols-[280px_minmax(0,1fr)]">
        <aside className="rounded-xl border border-slate-800 bg-slate-900/75 p-3">
          <div className="mb-2 flex items-center gap-2 px-2 text-sm font-semibold text-slate-200">
            <Settings2 className="h-4 w-4 text-cyan-300" />
            Sections
          </div>
          <nav className="space-y-1">
            {sections.map((section) => (
              <button
                key={section.sectionKey}
                type="button"
                onClick={() => setSelectedSectionKey(section.sectionKey)}
                className={`w-full rounded-lg px-3 py-2 text-left text-sm transition ${
                  section.sectionKey === currentSection.sectionKey
                    ? 'bg-cyan-500/12 text-cyan-100 ring-1 ring-cyan-500/40'
                    : 'text-slate-300 hover:bg-slate-800 hover:text-slate-100'
                }`}
              >
                <div className="flex items-center justify-between gap-2">
                  <span>{section.displayName}</span>
                  {dirtySections.has(section.sectionKey) ? <span className="h-2 w-2 rounded-full bg-amber-400" /> : null}
                </div>
              </button>
            ))}
          </nav>
        </aside>

        <section className="rounded-xl border border-slate-800 bg-slate-900/75 p-4">
          <div className="flex flex-col gap-3 border-b border-slate-800 pb-4 lg:flex-row lg:items-start lg:justify-between">
            <div className="space-y-2">
              <h2 className="text-xl font-semibold text-slate-50">{currentSection.displayName}</h2>
              <p className="text-sm text-slate-300">{SECTION_HELPERS[currentSection.sectionKey] ?? currentSection.description}</p>
            </div>
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => setShowAudit((state) => ({ ...state, [currentSection.sectionKey]: !state[currentSection.sectionKey] }))}
                className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-100 hover:bg-slate-800"
              >
                <span className="inline-flex items-center gap-2">
                  <History className="h-4 w-4" />
                  {showAudit[currentSection.sectionKey] ? 'Hide audit' : 'Show audit'}
                </span>
              </button>
              <button
                type="button"
                onClick={resetCurrentSection}
                disabled={!canManage}
                className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-100 hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <span className="inline-flex items-center gap-2">
                  <RotateCcw className="h-4 w-4" />
                  Reset to default
                </span>
              </button>
              <button
                type="button"
                onClick={saveCurrentSection}
                disabled={!canManage || state.kind === 'saving'}
                className="rounded-md bg-cyan-500 px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-cyan-400 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <span className="inline-flex items-center gap-2">
                  <Save className="h-4 w-4" />
                  {state.kind === 'saving' ? 'Saving…' : 'Save section'}
                </span>
              </button>
            </div>
          </div>

          {requiresReason ? (
            <div className="mt-4 rounded-xl border border-amber-500/40 bg-amber-500/10 p-4 text-sm text-amber-100">
              <div className="flex items-start gap-3">
                <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
                <div className="space-y-2">
                  <p className="font-medium">High-impact LedgArr settings changed</p>
                  <p>These changes affect financial controls or operating mode. A change reason is required before save.</p>
                  <label className="block text-xs uppercase tracking-wide text-amber-50/90">
                    Change reason
                    <input
                      value={reasonBySection[currentSection.sectionKey] ?? ''}
                      onChange={(event) => setReasonBySection((state) => ({ ...state, [currentSection.sectionKey]: event.target.value }))}
                      className="mt-2 w-full rounded-md border border-amber-500/30 bg-slate-950/70 px-3 py-2 text-sm text-slate-100 outline-none ring-0"
                      placeholder="Explain the accounting or control impact."
                    />
                  </label>
                </div>
              </div>
            </div>
          ) : null}

          {state.kind !== 'idle' && state.message ? (
            <div className={`mt-4 rounded-lg px-3 py-2 text-sm ${
              state.kind === 'error' ? 'border border-rose-500/40 bg-rose-500/10 text-rose-100' : 'border border-emerald-500/40 bg-emerald-500/10 text-emerald-100'
            }`}>
              {state.message}
            </div>
          ) : null}

          <div className="mt-5 space-y-5">
            {renderValueEditor({
              value: currentDraft,
              onChange: (nextValue) => updateDraft(currentSection.sectionKey, nextValue as Record<string, unknown>),
              path: '',
              options: optionsQuery.data!,
              errors: sectionErrors,
              readOnly: !canManage,
            })}
          </div>

          {showAudit[currentSection.sectionKey] ? (
            <div className="mt-8 border-t border-slate-800 pt-5">
              <h3 className="text-sm font-semibold text-slate-100">Audit history</h3>
              <p className="mt-1 text-sm text-slate-400">Every successful update and reset is written to the LedgArr settings audit trail.</p>
              <div className="mt-4 space-y-3">
                {auditQuery.isLoading ? <div className="text-sm text-slate-400">Loading audit history…</div> : null}
                {auditQuery.isError ? (
                  <ApiErrorCallout title="Unable to load audit history" message={getErrorMessage(auditQuery.error, 'Failed to load settings audit.')} />
                ) : null}
                {auditQuery.data?.items.length ? auditQuery.data.items.map((item) => <AuditCard key={item.auditId} item={item} />) : null}
                {auditQuery.data && auditQuery.data.items.length === 0 ? (
                  <div className="rounded-lg border border-dashed border-slate-700 p-4 text-sm text-slate-400">No audit events have been recorded for this section yet.</div>
                ) : null}
              </div>
            </div>
          ) : null}
        </section>
      </div>
    </div>
  )
}

function AuditCard({ item }: { item: LedgArrSettingsAuditItem }) {
  return (
    <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <span className="ledgarr-pill">{new Date(item.changedAtUtc).toLocaleString()}</span>
        <span className="text-xs text-slate-400">Actor {item.changedByPersonId}</span>
      </div>
      <p className="mt-2 text-sm text-slate-200">{item.changeReason || 'No change reason recorded.'}</p>
      {item.diffJson ? <pre className="mt-3 overflow-x-auto rounded bg-slate-900 p-3 text-xs text-slate-300">{item.diffJson}</pre> : null}
    </div>
  )
}

type RenderEditorArgs = {
  value: unknown
  onChange: (next: unknown) => void
  path: string
  options: LedgArrSettingsOptionsResponse
  errors: Record<string, string[]>
  readOnly: boolean
}

function renderValueEditor({ value, onChange, path, options, errors, readOnly }: RenderEditorArgs) {
  if (typeof value === 'boolean') {
    return (
      <label className="flex items-center gap-3 rounded-lg border border-slate-800 bg-slate-950/40 px-3 py-3 text-sm text-slate-200">
        <input type="checkbox" checked={value} disabled={readOnly} onChange={(event) => onChange(event.target.checked)} />
        <span>{humanizePathLeaf(path)}</span>
      </label>
    )
  }

  if (typeof value === 'number') {
    return (
      <FieldShell label={humanizePathLeaf(path)} path={path} errors={errors}>
        <input
          type="number"
          value={value}
          disabled={readOnly}
          onChange={(event) => onChange(Number(event.target.value))}
          className="w-full rounded-md border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100"
        />
      </FieldShell>
    )
  }

  if (typeof value === 'string') {
    const selectOptions = optionsForStringField(path, options)
    if (selectOptions.length > 0) {
      return (
        <FieldShell label={humanizePathLeaf(path)} path={path} errors={errors}>
          <select
            value={value}
            disabled={readOnly}
            onChange={(event) => onChange(event.target.value)}
            className="w-full rounded-md border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100"
          >
            <option value="">Select…</option>
            {selectOptions.map((option) => (
              <option key={option.value} value={option.value}>{option.label}</option>
            ))}
          </select>
        </FieldShell>
      )
    }

    return (
      <FieldShell label={humanizePathLeaf(path)} path={path} errors={errors}>
        <input
          value={value}
          disabled={readOnly}
          onChange={(event) => onChange(event.target.value)}
          className="w-full rounded-md border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100"
        />
      </FieldShell>
    )
  }

  if (Array.isArray(value)) {
    if (value.every((item) => typeof item === 'string')) {
      const currentValues = value as string[]
      const choices = MULTI_ENUM_OPTIONS[path.split('.').at(-1) ?? ''] ?? []
      if (choices.length > 0) {
        return (
          <FieldShell label={humanizePathLeaf(path)} path={path} errors={errors}>
            <div className="grid gap-2 md:grid-cols-2">
              {choices.map((choice) => (
                <label key={choice} className="flex items-center gap-2 rounded-md border border-slate-800 bg-slate-950/40 px-3 py-2 text-sm text-slate-200">
                  <input
                    type="checkbox"
                    checked={currentValues.includes(choice)}
                    disabled={readOnly}
                    onChange={(event) => {
                      const next = event.target.checked
                        ? [...currentValues, choice]
                        : currentValues.filter((item) => item !== choice)
                      onChange(next)
                    }}
                  />
                  {humanizeValue(choice)}
                </label>
              ))}
            </div>
          </FieldShell>
        )
      }

      return (
        <FieldShell label={humanizePathLeaf(path)} path={path} errors={errors}>
          <textarea
            value={currentValues.join(', ')}
            disabled={readOnly}
            onChange={(event) => onChange(event.target.value.split(',').map((item) => item.trim()).filter(Boolean))}
            className="min-h-24 w-full rounded-md border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100"
          />
        </FieldShell>
      )
    }

    if (value.every(isReferenceValue)) {
      const selected = value as ReferenceValue[]
      return (
        <FieldShell label={humanizePathLeaf(path)} path={path} errors={errors}>
          <div className="space-y-2 rounded-lg border border-slate-800 bg-slate-950/40 p-3">
            {options.crossProductReferences.map((option) => {
              const checked = selected.some((item) => item.productKey === option.productKey && item.objectType === option.objectType && item.publicKey === option.publicKey)
              return (
                <label key={option.value} className="flex items-center gap-2 text-sm text-slate-200">
                  <input
                    type="checkbox"
                    checked={checked}
                    disabled={readOnly}
                    onChange={(event) => {
                      const next = event.target.checked
                        ? [...selected, referenceFromOption(option)]
                        : selected.filter((item) => !(item.productKey === option.productKey && item.objectType === option.objectType && item.publicKey === option.publicKey))
                      onChange(next)
                    }}
                  />
                  {option.label}
                </label>
              )
            })}
            {!options.crossProductReferences.length ? <p className="text-sm text-slate-400">No cross-product references are available yet for this tenant.</p> : null}
          </div>
        </FieldShell>
      )
    }

    return (
      <FieldShell label={humanizePathLeaf(path)} path={path} errors={errors}>
        <div className="rounded-lg border border-slate-800 bg-slate-950/40 p-3 text-sm text-slate-400">
          Complex list editing is read through the current typed settings model. This list currently contains {value.length} item{value.length === 1 ? '' : 's'}.
        </div>
      </FieldShell>
    )
  }

  if (isReferenceValue(value)) {
    const selectOptions = options.crossProductReferences
    const selectedValue = value.productKey ? `${value.productKey}|${value.objectType}|${value.publicKey}` : ''
    return (
      <FieldShell label={humanizePathLeaf(path)} path={path} errors={errors}>
        <select
          value={selectedValue}
          disabled={readOnly}
          onChange={(event) => {
            const option = selectOptions.find((item) => item.value === event.target.value)
            onChange(option ? referenceFromOption(option) : { productKey: '', objectType: '', publicKey: '', displayName: '' })
          }}
          className="w-full rounded-md border border-slate-700 bg-slate-950/70 px-3 py-2 text-sm text-slate-100"
        >
          <option value="">Select owning-product reference…</option>
          {selectOptions.map((option) => (
            <option key={option.value} value={option.value}>{option.label}</option>
          ))}
        </select>
      </FieldShell>
    )
  }

  if (value && typeof value === 'object') {
    const objectValue = value as Record<string, unknown>
    return (
      <div className={path ? 'rounded-xl border border-slate-800 bg-slate-950/35 p-4' : ''}>
        {path ? <h3 className="mb-4 text-sm font-semibold text-slate-100">{humanizePathLeaf(path)}</h3> : null}
        <div className="space-y-4">
          {Object.entries(objectValue).map(([key, nestedValue]) => (
            <div key={key}>
              {renderValueEditor({
                value: nestedValue,
                onChange: (nextNestedValue) => onChange({ ...objectValue, [key]: nextNestedValue }),
                path: path ? `${path}.${key}` : key,
                options,
                errors,
                readOnly,
              })}
            </div>
          ))}
        </div>
      </div>
    )
  }

  return <div className="text-sm text-slate-400">Unsupported field type.</div>
}

function FieldShell({
  label,
  path,
  errors,
  children,
}: {
  label: string
  path: string
  errors: Record<string, string[]>
  children: ReactNode
}) {
  const fieldKey = path.split('.').at(-1) ?? path
  return (
    <label className="block space-y-2">
      <span className="text-sm font-medium text-slate-100">{label}</span>
      {children}
      {errors[fieldKey]?.length ? <span className="text-xs text-rose-300">{errors[fieldKey].join(' ')}</span> : null}
    </label>
  )
}

function optionsForStringField(path: string, options: LedgArrSettingsOptionsResponse) {
  const key = path.split('.').at(-1) ?? ''
  if (key.endsWith('AccountRef')) {
    return options.accounts
  }
  if (key.endsWith('LegalEntityRef') || key === 'defaultLegalEntityRef' || key === 'consolidationEliminationEntityRef' || key === 'eliminationEntityRef') {
    return options.legalEntities
  }
  if (key.toLowerCase().includes('currency')) {
    return options.currencies
  }
  return (ENUM_OPTIONS[key] ?? []).map((value) => ({ value, label: humanizeValue(value) }))
}

function dirtyFieldSet(source: Record<string, unknown>, draft: Record<string, unknown>, prefix = '', result = new Set<string>()) {
  for (const key of new Set([...Object.keys(source), ...Object.keys(draft)])) {
    const nextPrefix = prefix ? `${prefix}.${key}` : key
    const sourceValue = source[key]
    const draftValue = draft[key]
    if (typeof sourceValue === 'object' && sourceValue && typeof draftValue === 'object' && draftValue && !Array.isArray(sourceValue) && !Array.isArray(draftValue)) {
      dirtyFieldSet(sourceValue as Record<string, unknown>, draftValue as Record<string, unknown>, nextPrefix, result)
      continue
    }
    if (JSON.stringify(sourceValue) !== JSON.stringify(draftValue)) {
      result.add(key)
      result.add(nextPrefix)
    }
  }
  return result
}

function isReferenceValue(value: unknown): value is ReferenceValue {
  return Boolean(
    value
    && typeof value === 'object'
    && 'productKey' in value
    && 'objectType' in value
    && 'publicKey' in value
    && 'displayName' in value,
  )
}

function referenceFromOption(option: LedgArrSettingsOptionsResponse['crossProductReferences'][number]): ReferenceValue {
  return {
    productKey: option.productKey,
    objectType: option.objectType,
    publicKey: option.publicKey,
    displayName: option.label,
  }
}

function humanizePathLeaf(path: string) {
  const key = path.split('.').at(-1) ?? path
  return humanizeValue(key)
}

function humanizeValue(value: string) {
  return value
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[_-]+/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()
    .replace(/^./, (character) => character.toUpperCase())
}
