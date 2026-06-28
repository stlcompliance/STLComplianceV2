import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  AlertTriangle,
  CheckCircle2,
  FileCheck2,
  RotateCcw,
  Save,
  Settings,
  ShieldCheck,
} from 'lucide-react'
import {
  getLoadArrTenantSettings,
  getLoadArrTenantSettingsAudit,
  getLoadArrTenantSettingsOptions,
  replaceLoadArrTenantSettings,
  resetLoadArrTenantSettingsSection,
  type LoadArrTenantSettingsFieldOption,
  type LoadArrTenantSettingsOptionsResponse,
  type LoadArrTenantSettingsSections,
  type LoadArrTenantSettingsValidationMessage,
} from '../api/client'

interface TenantSettingsPanelProps {
  accessToken: string | undefined
}

type FlatValue = string | number | boolean | null

type DraftValidation = {
  errors: LoadArrTenantSettingsValidationMessage[]
  warnings: LoadArrTenantSettingsValidationMessage[]
}

export function TenantSettingsPanel({ accessToken }: TenantSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [activeSection, setActiveSection] = useState('overview')
  const [baseline, setBaseline] = useState<LoadArrTenantSettingsSections | null>(null)
  const [draft, setDraft] = useState<LoadArrTenantSettingsSections | null>(null)
  const [reason, setReason] = useState('')
  const [warningsAcknowledged, setWarningsAcknowledged] = useState(false)
  const [resetSectionKey, setResetSectionKey] = useState<string | null>(null)

  const settingsQuery = useQuery({
    queryKey: ['loadarr-tenant-settings', accessToken],
    queryFn: () => getLoadArrTenantSettings(accessToken!),
    enabled: Boolean(accessToken),
    retry: false,
  })

  const optionsQuery = useQuery({
    queryKey: ['loadarr-tenant-settings-options', accessToken],
    queryFn: () => getLoadArrTenantSettingsOptions(accessToken!),
    enabled: Boolean(accessToken),
    retry: false,
  })

  const auditQuery = useQuery({
    queryKey: ['loadarr-tenant-settings-audit', accessToken],
    queryFn: () => getLoadArrTenantSettingsAudit(accessToken!, 50),
    enabled: Boolean(accessToken),
    retry: false,
  })

  useEffect(() => {
    if (!settingsQuery.data) {
      return
    }

    setBaseline(settingsQuery.data.settings)
    setDraft(settingsQuery.data.settings)
    setWarningsAcknowledged(false)
  }, [settingsQuery.data?.rowVersion])

  const validation = useMemo(() => validateTenantSettingsDraft(draft), [draft])
  const options = optionsQuery.data
  const diffs = useMemo(() => buildTenantSettingsDiff(baseline, draft, options), [baseline, draft, options])
  const isDirty = diffs.length > 0
  const warningCodes = validation.warnings.map((warning) => warning.code)
  const canSave =
    Boolean(accessToken) &&
    Boolean(settingsQuery.data) &&
    Boolean(draft) &&
    isDirty &&
    validation.errors.length === 0 &&
    (warningCodes.length === 0 || warningsAcknowledged)

  const saveMutation = useMutation({
    mutationFn: () =>
      replaceLoadArrTenantSettings(
        accessToken!,
        settingsQuery.data!.rowVersion,
        draft!,
        reason || null,
        warningsAcknowledged ? warningCodes : [],
      ),
    onSuccess: (response) => {
      setBaseline(response.settings)
      setDraft(response.settings)
      setReason('')
      setWarningsAcknowledged(false)
      void queryClient.invalidateQueries({ queryKey: ['loadarr-tenant-settings-audit', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['loadarr-tenant-settings', accessToken] })
    },
  })

  const resetMutation = useMutation({
    mutationFn: (sectionKey: string) =>
      resetLoadArrTenantSettingsSection(
        accessToken!,
        sectionKey,
        settingsQuery.data!.rowVersion,
        reason || null,
      ),
    onSuccess: (response) => {
      setBaseline(response.settings)
      setDraft(response.settings)
      setResetSectionKey(null)
      setReason('')
      setWarningsAcknowledged(false)
      void queryClient.invalidateQueries({ queryKey: ['loadarr-tenant-settings-audit', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['loadarr-tenant-settings', accessToken] })
    },
  })

  if (!accessToken) {
    return (
      <section className="receiving-layout" aria-label="LoadArr tenant settings">
        <article className="workflow-panel">
          <div className="empty-state">
            <strong>Settings unavailable</strong>
            <span>LoadArr session context is required.</span>
          </div>
        </article>
      </section>
    )
  }

  if (settingsQuery.isLoading || optionsQuery.isLoading) {
    return (
      <section className="receiving-layout" aria-label="LoadArr tenant settings">
        <article className="workflow-panel">
          <div className="empty-state">
            <strong>Loading tenant settings</strong>
            <span>Fetching LoadArr execution policy.</span>
          </div>
        </article>
      </section>
    )
  }

  if (settingsQuery.isError || optionsQuery.isError || !draft || !options || !settingsQuery.data) {
    const error = settingsQuery.error ?? optionsQuery.error
    return (
      <section className="receiving-layout" aria-label="LoadArr tenant settings">
        <article className="workflow-panel">
          <div className="empty-state">
            <strong>Tenant settings unavailable</strong>
            <span>{error instanceof Error ? error.message : 'Unable to load tenant settings.'}</span>
          </div>
        </article>
      </section>
    )
  }

  const activeSectionOption = options.sections.find((section) => section.key === activeSection)
  const combinedWarnings = mergeMessages(settingsQuery.data.validation.warnings, validation.warnings)

  return (
    <section className="settings-layout" aria-label="LoadArr tenant settings">
      <article className="workflow-panel settings-main">
        <div className="section-heading">
          <Settings aria-hidden="true" />
          <h2>Tenant settings</h2>
        </div>

        <div className="settings-tabs" role="tablist" aria-label="Settings sections">
          <button
            type="button"
            className={activeSection === 'overview' ? 'active' : ''}
            onClick={() => setActiveSection('overview')}
          >
            Overview
          </button>
          {options.sections.map((section) => (
            <button
              type="button"
              key={section.key}
              className={activeSection === section.key ? 'active' : ''}
              onClick={() => setActiveSection(section.key)}
            >
              {section.label}
            </button>
          ))}
          <button
            type="button"
            className={activeSection === 'audit' ? 'active' : ''}
            onClick={() => setActiveSection('audit')}
          >
            Audit history
          </button>
        </div>

        {activeSection === 'overview' && (
          <div className="settings-overview">
            <div className="data-grid handoff-grid">
              <SettingsFact label="Version" value={String(settingsQuery.data.version)} />
              <SettingsFact
                label="Updated"
                value={`${formatDate(settingsQuery.data.updatedAt)} by ${settingsQuery.data.updatedByDisplayNameSnapshot ?? 'system'}`}
              />
              <SettingsFact label="Sections" value={String(options.sections.length)} />
              <SettingsFact label="Events" value={String(options.eventNames.length)} />
            </div>

            <div className="settings-callout">
              <ShieldCheck aria-hidden="true" />
              <div>
                <strong>Ownership guardrails</strong>
                <span>
                  StaffArr owns people and internal locations; SupplyArr owns item and vendor context; Compliance Core
                  owns rulings; RecordArr owns retained document packets.
                </span>
              </div>
            </div>

            {settingsQuery.data.validation.dependencyHints.length > 0 && (
              <div className="settings-message-list">
                {settingsQuery.data.validation.dependencyHints.slice(0, 8).map((hint) => (
                  <div className="settings-hint" key={hint.code}>
                    <FileCheck2 aria-hidden="true" />
                    <span>{hint.message}</span>
                  </div>
                ))}
              </div>
            )}

            <div className="settings-event-grid" aria-label="LoadArr event names">
              {options.eventNames.map((eventName) => (
                <span key={eventName}>{eventName}</span>
              ))}
            </div>
          </div>
        )}

        {activeSectionOption && (
          <div className="settings-section-editor">
            <div className="settings-section-heading">
              <div>
                <span className="kicker">{activeSectionOption.key}</span>
                <h2>{activeSectionOption.label}</h2>
              </div>
              <button
                type="button"
                className="secondary-action settings-inline-action"
                onClick={() => setResetSectionKey(activeSectionOption.key)}
              >
                <RotateCcw aria-hidden="true" />
                <span>Reset section</span>
              </button>
            </div>

            <p className="notes">{activeSectionOption.description}</p>

            <div className="form-grid settings-field-grid">
              {activeSectionOption.fields.map((field) => (
                <TenantSettingsField
                  key={field.key}
                  field={field}
                  options={options}
                  value={draft[activeSectionOption.key]?.[field.key]}
                  onChange={(value) => {
                    setDraft((current) => updateDraftField(current, activeSectionOption.key, field.key, value))
                    setWarningsAcknowledged(false)
                  }}
                />
              ))}
            </div>
          </div>
        )}

        {activeSection === 'audit' && (
          <div className="settings-audit-table" aria-label="Settings audit history">
            {auditQuery.isLoading ? (
              <div className="empty-state">
                <strong>Loading audit history</strong>
                <span>Fetching settings changes.</span>
              </div>
            ) : auditQuery.isError ? (
              <div className="empty-state">
                <strong>Audit history unavailable</strong>
                <span>{auditQuery.error instanceof Error ? auditQuery.error.message : 'Unable to load audit history.'}</span>
              </div>
            ) : (
              <table>
                <thead>
                  <tr>
                    <th>Changed</th>
                    <th>Section</th>
                    <th>Actor</th>
                    <th>Fields</th>
                    <th>Reason</th>
                  </tr>
                </thead>
                <tbody>
                  {(auditQuery.data?.items ?? []).map((entry) => (
                    <tr key={`${entry.changedAt}-${entry.settingsVersionAfter}-${entry.sectionKey}`}>
                      <td>{formatDate(entry.changedAt)}</td>
                      <td>{labelForSection(options, entry.sectionKey)}</td>
                      <td>{entry.changedByDisplayNameSnapshot ?? 'system'}</td>
                      <td>{entry.changedFields.slice(0, 4).join(', ') || 'Defaults initialized'}</td>
                      <td>{entry.reason ?? entry.changeSource}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        )}
      </article>

      <aside className="side-panel settings-save-panel" aria-label="Settings save review">
        <div className="section-heading">
          <FileCheck2 aria-hidden="true" />
          <h2>Review</h2>
        </div>

        {validation.errors.length > 0 && (
          <SettingsMessages title="Blocking errors" messages={validation.errors} tone="error" />
        )}

        {combinedWarnings.length > 0 && (
          <SettingsMessages title="Warnings" messages={combinedWarnings} tone="warning" />
        )}

        <div className="settings-diff-preview" aria-label="Settings diff preview">
          <strong>Diff preview</strong>
          {diffs.length > 0 ? (
            <ul>
              {diffs.slice(0, 12).map((diff) => (
                <li key={diff.path}>
                  <span>{diff.label}</span>
                  <small>
                    {formatFieldValue(diff.before)} to {formatFieldValue(diff.after)}
                  </small>
                </li>
              ))}
            </ul>
          ) : (
            <span>No unsaved changes</span>
          )}
        </div>

        {combinedWarnings.length > 0 && (
          <label className="settings-checkbox">
            <input
              type="checkbox"
              checked={warningsAcknowledged}
              onChange={(event) => setWarningsAcknowledged(event.target.checked)}
            />
            <span>Acknowledge risk warnings</span>
          </label>
        )}

        <label className="field-block">
          <span className="field-label">Reason</span>
          <textarea
            className="field-control"
            value={reason}
            onChange={(event) => setReason(event.target.value)}
          />
        </label>

        <button
          type="button"
          className="primary-action"
          disabled={!canSave || saveMutation.isPending}
          onClick={() => void saveMutation.mutate()}
        >
          <Save aria-hidden="true" />
          <span>{saveMutation.isPending ? 'Saving' : 'Save settings'}</span>
        </button>

        {saveMutation.isSuccess && (
          <div className="settings-inline-success">
            <CheckCircle2 aria-hidden="true" />
            <span>Settings saved</span>
          </div>
        )}

        {saveMutation.isError && (
          <div className="settings-inline-error">
            <AlertTriangle aria-hidden="true" />
            <span>{saveMutation.error instanceof Error ? saveMutation.error.message : 'Save failed.'}</span>
          </div>
        )}
      </aside>

      {resetSectionKey && (
        <div className="settings-modal-backdrop" role="presentation">
          <div className="settings-modal" role="dialog" aria-modal="true" aria-labelledby="reset-section-title">
            <div className="section-heading">
              <RotateCcw aria-hidden="true" />
              <h2 id="reset-section-title">Reset {labelForSection(options, resetSectionKey)}</h2>
            </div>
            <p className="notes">
              Current values in this section will return to LoadArr defaults and an audit entry will be recorded.
            </p>
            <div className="action-row">
              <button
                type="button"
                className="secondary-action"
                onClick={() => setResetSectionKey(null)}
              >
                Cancel
              </button>
              <button
                type="button"
                className="primary-action"
                disabled={resetMutation.isPending}
                onClick={() => void resetMutation.mutate(resetSectionKey)}
              >
                <RotateCcw aria-hidden="true" />
                <span>{resetMutation.isPending ? 'Resetting' : 'Reset section'}</span>
              </button>
            </div>
            {resetMutation.isError && (
              <div className="settings-inline-error">
                <AlertTriangle aria-hidden="true" />
                <span>{resetMutation.error instanceof Error ? resetMutation.error.message : 'Reset failed.'}</span>
              </div>
            )}
          </div>
        </div>
      )}
    </section>
  )
}

function TenantSettingsField({
  field,
  options,
  value,
  onChange,
}: {
  field: LoadArrTenantSettingsFieldOption
  options: LoadArrTenantSettingsOptionsResponse
  value: unknown
  onChange: (value: unknown) => void
}) {
  if (field.inputType === 'boolean') {
    return (
      <label className={`settings-toggle ${field.risky ? 'risky' : ''}`}>
        <input
          type="checkbox"
          checked={Boolean(value)}
          onChange={(event) => onChange(event.target.checked)}
        />
        <span>{field.label}</span>
      </label>
    )
  }

  if (field.inputType === 'enum') {
    return (
      <label className="field-block">
        <span className="field-label">{field.label}</span>
        <select
          className="field-control"
          value={typeof value === 'string' ? value : ''}
          onChange={(event) => onChange(event.target.value)}
        >
          {(field.enumKey ? options.enumOptions[field.enumKey] : []).map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    )
  }

  if (field.inputType === 'number') {
    return (
      <label className="field-block">
        <span className="field-label">{field.label}</span>
        <input
          className="field-control"
          type="number"
          min={field.min ?? undefined}
          max={field.max ?? undefined}
          value={value === null || value === undefined ? '' : String(value)}
          onChange={(event) => onChange(event.target.value === '' ? null : Number(event.target.value))}
        />
      </label>
    )
  }

  return (
    <label className="field-block">
      <span className="field-label">{field.label}</span>
      <input
        className="field-control"
        value={value === null || value === undefined ? '' : String(value)}
        onChange={(event) => onChange(event.target.value || null)}
      />
    </label>
  )
}

function SettingsFact({ label, value }: { label: string; value: string }) {
  return (
    <div className="audit-fact">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  )
}

function SettingsMessages({
  title,
  messages,
  tone,
}: {
  title: string
  messages: LoadArrTenantSettingsValidationMessage[]
  tone: 'error' | 'warning'
}) {
  return (
    <div className={`settings-validation ${tone}`}>
      <strong>{title}</strong>
      <ul>
        {messages.map((message) => (
          <li key={message.code}>{message.message}</li>
        ))}
      </ul>
    </div>
  )
}

function updateDraftField(
  current: LoadArrTenantSettingsSections | null,
  sectionKey: string,
  fieldKey: string,
  value: unknown,
): LoadArrTenantSettingsSections | null {
  if (!current) {
    return current
  }

  return {
    ...current,
    [sectionKey]: {
      ...(current[sectionKey] ?? {}),
      [fieldKey]: value,
    },
  }
}

export function buildTenantSettingsDiff(
  baseline: LoadArrTenantSettingsSections | null,
  draft: LoadArrTenantSettingsSections | null,
  options: LoadArrTenantSettingsOptionsResponse | undefined,
) {
  if (!baseline || !draft || !options) {
    return []
  }

  const baselineFlat = flattenSettings(baseline)
  const draftFlat = flattenSettings(draft)
  return Array.from(new Set([...Object.keys(baselineFlat), ...Object.keys(draftFlat)]))
    .filter((key) => baselineFlat[key] !== draftFlat[key])
    .sort()
    .map((path) => ({
      path,
      label: labelForPath(options, path),
      before: baselineFlat[path] ?? null,
      after: draftFlat[path] ?? null,
    }))
}

export function validateTenantSettingsDraft(
  draft: LoadArrTenantSettingsSections | null,
): DraftValidation {
  if (!draft) {
    return { errors: [], warnings: [] }
  }

  const errors: LoadArrTenantSettingsValidationMessage[] = []
  const warnings: LoadArrTenantSettingsValidationMessage[] = []
  const receiving = draft.receiving ?? {}
  const dock = draft.dockAppointments ?? {}
  const traceability = draft.traceability ?? {}
  const documents = draft.labelingAndDocuments ?? {}
  const inventory = draft.inventoryControl ?? {}
  const compliance = draft.compliance ?? {}
  const movement = draft.movement ?? {}
  const exceptions = draft.exceptions ?? {}
  const mobile = draft.mobileScanner ?? {}

  const overReceiptTolerance = Number(receiving.overReceiptTolerancePercent)
  if (Number.isFinite(overReceiptTolerance) && (overReceiptTolerance < 0 || overReceiptTolerance > 100)) {
    message(errors, 'loadarr.ui.receiving.over_receipt_tolerance_range', 'receiving', 'overReceiptTolerancePercent', 'Over-receipt tolerance must be between 0 and 100.', 'error')
  }

  if (receiving.allowOverReceipt === false && overReceiptTolerance !== 0) {
    message(errors, 'loadarr.ui.receiving.over_receipt_disabled_tolerance', 'receiving', 'overReceiptTolerancePercent', 'Over-receipt tolerance must be zero when over-receipt is disabled.', 'error')
  }

  const appointmentMinutes = Number(dock.defaultAppointmentMinutes)
  if (Number.isFinite(appointmentMinutes) && (appointmentMinutes < 5 || appointmentMinutes > 1440)) {
    message(errors, 'loadarr.ui.dock.default_appointment_range', 'dockAppointments', 'defaultAppointmentMinutes', 'Default appointment minutes must be between 5 and 1440.', 'error')
  }

  if (traceability.enableLpn === false && traceability.requireLpnScan === true) {
    message(errors, 'loadarr.ui.traceability.lpn_scan_requires_lpn', 'traceability', 'requireLpnScan', 'LPN scan cannot be required when LPN is disabled.', 'error')
  }

  if (traceability.enableLpn === false && documents.generateLpnLabels === true) {
    message(errors, 'loadarr.ui.documents.lpn_labels_require_lpn', 'labelingAndDocuments', 'generateLpnLabels', 'LPN labels require LPN traceability.', 'error')
  }

  if (inventory.allowNegativeInventory === true) {
    message(warnings, 'loadarr.ui.inventory.negative_inventory', 'inventoryControl', 'allowNegativeInventory', 'Negative inventory requires product admin acknowledgement.', 'warning')
  }

  if (compliance.enableComplianceCoreChecks === false) {
    message(warnings, 'loadarr.ui.compliance.disabled', 'compliance', 'enableComplianceCoreChecks', 'Disabling Compliance Core checks is a high-risk warehouse execution change.', 'warning')
  }

  if (
    movement.allowMovementOutOfQuarantine === true &&
    movement.requireAssurArrDispositionBeforeReleaseFromQuarantine === false
  ) {
    message(warnings, 'loadarr.ui.movement.quarantine_release_without_assurarr', 'movement', 'allowMovementOutOfQuarantine', 'Movement out of quarantine without AssurArr disposition requires acknowledgement.', 'warning')
  }

  if (exceptions.allowLocalWarehouseDisposition === true) {
    message(warnings, 'loadarr.ui.exceptions.local_disposition', 'exceptions', 'allowLocalWarehouseDisposition', 'Local warehouse disposition changes quality handoff behavior.', 'warning')
  }

  if (mobile.allowOfflineTaskExecution === true) {
    message(warnings, 'loadarr.ui.mobile.offline_execution', 'mobileScanner', 'allowOfflineTaskExecution', 'Offline execution remains a readiness policy until authoritative sync can preserve inventory conflicts.', 'warning')
  }

  return { errors, warnings }
}

function message(
  target: LoadArrTenantSettingsValidationMessage[],
  code: string,
  sectionKey: string,
  fieldPath: string,
  text: string,
  severity: string,
) {
  target.push({ code, sectionKey, fieldPath, message: text, severity })
}

function flattenSettings(settings: LoadArrTenantSettingsSections): Record<string, FlatValue> {
  const flat: Record<string, FlatValue> = {}

  for (const [sectionKey, section] of Object.entries(settings)) {
    for (const [fieldKey, value] of Object.entries(section ?? {})) {
      if (
        typeof value === 'string' ||
        typeof value === 'number' ||
        typeof value === 'boolean' ||
        value === null
      ) {
        flat[`${sectionKey}.${fieldKey}`] = value
      }
    }
  }

  return flat
}

function mergeMessages(
  first: LoadArrTenantSettingsValidationMessage[],
  second: LoadArrTenantSettingsValidationMessage[],
) {
  const seen = new Set<string>()
  return [...first, ...second].filter((message) => {
    if (seen.has(message.code)) {
      return false
    }
    seen.add(message.code)
    return true
  })
}

function labelForPath(options: LoadArrTenantSettingsOptionsResponse, path: string) {
  const [sectionKey, fieldKey] = path.split('.')
  const section = options.sections.find((candidate) => candidate.key === sectionKey)
  const field = section?.fields.find((candidate) => candidate.key === fieldKey)
  return field ? `${section?.label ?? sectionKey}: ${field.label}` : path
}

function labelForSection(options: LoadArrTenantSettingsOptionsResponse, sectionKey: string) {
  if (sectionKey === 'all') {
    return 'All sections'
  }
  return options.sections.find((section) => section.key === sectionKey)?.label ?? sectionKey
}

function formatFieldValue(value: FlatValue) {
  if (typeof value === 'boolean') {
    return value ? 'on' : 'off'
  }
  if (value === null || value === undefined) {
    return 'blank'
  }
  return String(value)
}

function formatDate(value: string) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  })
}
