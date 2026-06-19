import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Check,
  History,
  RefreshCw,
  RotateCcw,
  Save,
  SlidersHorizontal,
  X,
} from 'lucide-react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getEditableRoutArrTenantSettings,
  getRoutArrTenantSettingAuditHistory,
  getRoutArrTenantSettingsOptions,
  previewRoutArrTenantSettings,
  resetRoutArrTenantSettingGroup,
  updateRoutArrTenantSettingGroup,
  validateRoutArrTenantSettingGroup,
} from '../api/client'
import type {
  RoutArrSettingFieldResponse,
  RoutArrSettingGroupResponse,
  RoutArrSettingValue,
  RoutArrSettingsValidationResponse,
  RoutArrTenantSettingOverrideResponse,
  RoutArrTenantSettingsResponse,
} from '../api/types'

type Props = {
  accessToken: string
  canManage: boolean
}

type Drafts = Record<string, Record<string, RoutArrSettingValue>>

function groupDraft(group: RoutArrSettingGroupResponse): Record<string, RoutArrSettingValue> {
  return Object.fromEntries(group.fields.map((field) => [field.settingKey, cloneValue(field.value)]))
}

function cloneValue(value: RoutArrSettingValue): RoutArrSettingValue {
  return Array.isArray(value) ? [...value] : value
}

function valuesEqual(left: RoutArrSettingValue, right: RoutArrSettingValue): boolean {
  if (Array.isArray(left) || Array.isArray(right)) {
    return JSON.stringify(left ?? []) === JSON.stringify(right ?? [])
  }
  return String(left ?? '') === String(right ?? '')
}

function formatTimestamp(value: string | null | undefined): string {
  if (!value) return 'Never'
  try {
    return new Date(value).toLocaleString()
  } catch {
    return value
  }
}

function formatValue(value: RoutArrSettingValue): string {
  if (Array.isArray(value)) return value.join(', ') || 'None'
  if (typeof value === 'boolean') return value ? 'Enabled' : 'Disabled'
  if (value === null || value === '') return 'None'
  return String(value).replaceAll('_', ' ')
}

function sourceLabel(source: string): string {
  if (source === 'platformDefault') return 'Platform default'
  if (source === 'tenantDefault') return 'Tenant default'
  if (source === 'emergencyOverride') return 'Emergency override'
  return source.replace('Override', ' override').replace(/([a-z])([A-Z])/g, '$1 $2')
}

function validationForGroup(
  response: RoutArrSettingsValidationResponse | null,
  groupKey: string,
): string[] {
  if (!response || response.isValid) return []
  return response.issues
    .filter((issue) => issue.fieldPath.startsWith(`${groupKey}.`))
    .map((issue) => issue.message)
}

export function RoutArrTenantSettingsPanel({ accessToken, canManage }: Props) {
  const queryClient = useQueryClient()
  const [activeGroupKey, setActiveGroupKey] = useState('')
  const [drafts, setDrafts] = useState<Drafts>({})
  const [dirtyGroups, setDirtyGroups] = useState<Set<string>>(new Set())
  const [validation, setValidation] = useState<RoutArrSettingsValidationResponse | null>(null)
  const [previewOverrideKey, setPreviewOverrideKey] = useState('')

  const settingsQuery = useQuery({
    queryKey: ['routarr-tenant-settings', accessToken],
    queryFn: () => getEditableRoutArrTenantSettings(accessToken),
    enabled: canManage,
  })

  const optionsQuery = useQuery({
    queryKey: ['routarr-tenant-settings-options', accessToken],
    queryFn: () => getRoutArrTenantSettingsOptions(accessToken),
    enabled: canManage,
  })

  const activeGroup = useMemo(() => {
    const groups = settingsQuery.data?.groups ?? []
    return groups.find((group) => group.groupKey === activeGroupKey) ?? groups[0] ?? null
  }, [activeGroupKey, settingsQuery.data?.groups])

  const auditQuery = useQuery({
    queryKey: ['routarr-tenant-settings-audit', accessToken, activeGroup?.groupKey],
    queryFn: () => getRoutArrTenantSettingAuditHistory(accessToken, activeGroup?.groupKey, 8),
    enabled: canManage && Boolean(activeGroup?.groupKey),
  })

  const previewOverride = useMemo(() => {
    return settingsQuery.data?.overrides.find((item) => item.overrideKey === previewOverrideKey) ?? null
  }, [previewOverrideKey, settingsQuery.data?.overrides])

  const previewQuery = useQuery({
    queryKey: ['routarr-tenant-settings-preview', accessToken, previewOverrideKey],
    queryFn: () =>
      previewRoutArrTenantSettings(accessToken, {
        scopes: previewOverride ? [previewOverride.scope] : [],
      }),
    enabled: canManage && Boolean(previewOverride),
  })

  useEffect(() => {
    const groups = settingsQuery.data?.groups ?? []
    if (groups.length === 0) return
    setDrafts(Object.fromEntries(groups.map((group) => [group.groupKey, groupDraft(group)])))
    setDirtyGroups(new Set())
    setValidation(null)
    setActiveGroupKey((current) => current || groups[0].groupKey)
  }, [settingsQuery.data])

  const saveMutation = useMutation({
    mutationFn: async (group: RoutArrSettingGroupResponse) => {
      const values = drafts[group.groupKey] ?? groupDraft(group)
      const validationResult = await validateRoutArrTenantSettingGroup(accessToken, {
        settingGroup: group.groupKey,
        values,
      })
      if (!validationResult.isValid) {
        setValidation(validationResult)
        return null
      }
      setValidation(null)
      return updateRoutArrTenantSettingGroup(accessToken, group.groupKey, {
        expectedVersion: settingsQuery.data?.version ?? null,
        values,
      })
    },
    onSuccess: async (response) => {
      if (!response) return
      await queryClient.invalidateQueries({ queryKey: ['routarr-tenant-settings', accessToken] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-tenant-settings-audit', accessToken] })
    },
  })

  const resetMutation = useMutation({
    mutationFn: (group: RoutArrSettingGroupResponse) =>
      resetRoutArrTenantSettingGroup(accessToken, group.groupKey, {
        expectedVersion: settingsQuery.data?.version ?? null,
      }),
    onSuccess: async () => {
      setValidation(null)
      await queryClient.invalidateQueries({ queryKey: ['routarr-tenant-settings', accessToken] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-tenant-settings-audit', accessToken] })
    },
  })

  if (!canManage) return null

  if (settingsQuery.isLoading || optionsQuery.isLoading) {
    return <p className="mt-8 text-sm text-slate-400">Loading RoutArr settings...</p>
  }

  if (settingsQuery.isError) {
    return (
      <ApiErrorCallout
        className="mt-8"
        title="RoutArr settings unavailable"
        message={getErrorMessage(settingsQuery.error, 'Failed to load RoutArr settings.')}
        retryLabel="Retry"
        onRetry={() => void settingsQuery.refetch()}
      />
    )
  }

  const settings = settingsQuery.data
  const groups = settings?.groups ?? []
  const draft = activeGroup ? drafts[activeGroup.groupKey] ?? groupDraft(activeGroup) : {}
  const hasUnsaved = activeGroup
    ? activeGroup.fields.some((field) => !valuesEqual(draft[field.settingKey], field.value))
    : false
  const groupValidationMessages = activeGroup
    ? validationForGroup(validation, activeGroup.groupKey)
    : []

  return (
    <section className="mt-8" data-testid="routarr-tenant-settings-panel">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-slate-50">RoutArr settings</h1>
          <p className="mt-1 max-w-3xl text-sm text-slate-400">
            Tenant-scoped transportation behavior with typed defaults, scoped overrides, and audit.
          </p>
        </div>
        <div className="rounded border border-slate-700 bg-slate-900 px-3 py-2 text-xs text-slate-400">
          Version <span className="text-slate-100">{settings?.version ?? '...'}</span>
        </div>
      </div>

      <div className="mt-5 grid gap-5 lg:grid-cols-[260px_minmax(0,1fr)]">
        <nav className="rounded border border-slate-700 bg-slate-900 p-2" aria-label="RoutArr settings sections">
          <div className="flex items-center gap-2 px-2 py-2 text-xs font-semibold uppercase text-[var(--color-text-muted)]">
            <SlidersHorizontal className="h-4 w-4" aria-hidden="true" />
            Sections
          </div>
          <div className="mt-1 grid gap-1">
            {groups.map((group) => {
              const dirty = dirtyGroups.has(group.groupKey)
              const active = activeGroup?.groupKey === group.groupKey
              return (
                <button
                  key={group.groupKey}
                  type="button"
                  className={[
                    'flex items-center justify-between gap-2 rounded px-3 py-2 text-left text-sm',
                    active
                      ? 'bg-sky-900/50 text-sky-100 ring-1 ring-sky-700'
                      : 'text-slate-300 hover:bg-slate-800',
                  ].join(' ')}
                  onClick={() => setActiveGroupKey(group.groupKey)}
                >
                  <span>{group.label}</span>
                  {dirty ? <span className="h-2 w-2 rounded-full bg-amber-400" aria-label="Unsaved" /> : null}
                </button>
              )
            })}
          </div>
        </nav>

        {activeGroup ? (
          <div className="space-y-5">
            <section className="rounded border border-slate-700 bg-slate-900 p-4">
              <div className="flex flex-wrap items-start justify-between gap-4">
                <div>
                  <h2 className="text-lg font-semibold text-slate-50">{activeGroup.label}</h2>
                  <p className="mt-1 text-sm text-slate-400">{activeGroup.description}</p>
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                    Last updated {formatTimestamp(activeGroup.lastUpdatedAt)} by{' '}
                    {activeGroup.lastUpdatedByPersonId ?? 'platform default'}
                  </p>
                </div>
                {hasUnsaved ? (
                  <span className="rounded bg-amber-500/15 px-2 py-1 text-xs font-medium text-amber-200">
                    Unsaved changes
                  </span>
                ) : (
                  <span className="inline-flex items-center gap-1 rounded bg-emerald-500/15 px-2 py-1 text-xs font-medium text-emerald-200">
                    <Check className="h-3.5 w-3.5" aria-hidden="true" />
                    Saved
                  </span>
                )}
              </div>

              {groupValidationMessages.length > 0 ? (
                <div className="mt-4 rounded border border-rose-800 bg-rose-950/40 p-3 text-sm text-rose-100">
                  {groupValidationMessages.map((message) => (
                    <p key={message}>{message}</p>
                  ))}
                </div>
              ) : null}

              <div className="mt-5 grid gap-4 xl:grid-cols-2">
                {activeGroup.fields.map((field) => (
                  <SettingField
                    key={field.settingKey}
                    field={field}
                    value={draft[field.settingKey] ?? field.value}
                    onChange={(value) => {
                      setDrafts((current) => ({
                        ...current,
                        [activeGroup.groupKey]: {
                          ...(current[activeGroup.groupKey] ?? groupDraft(activeGroup)),
                          [field.settingKey]: value,
                        },
                      }))
                      setDirtyGroups((current) => new Set(current).add(activeGroup.groupKey))
                    }}
                  />
                ))}
              </div>

              <div className="mt-5 flex flex-wrap gap-3">
                <button
                  type="button"
                  className="inline-flex items-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
                  disabled={!hasUnsaved || saveMutation.isPending}
                  onClick={() => saveMutation.mutate(activeGroup)}
                  data-testid="routarr-settings-save-section"
                >
                  <Save className="h-4 w-4" aria-hidden="true" />
                  {saveMutation.isPending ? 'Saving...' : 'Save section'}
                </button>
                <button
                  type="button"
                  className="inline-flex items-center gap-2 rounded border border-slate-700 px-3 py-2 text-sm font-medium text-slate-200 hover:bg-slate-800 disabled:opacity-50"
                  disabled={!hasUnsaved}
                  onClick={() => {
                    setDrafts((current) => ({
                      ...current,
                      [activeGroup.groupKey]: groupDraft(activeGroup),
                    }))
                    setDirtyGroups((current) => {
                      const next = new Set(current)
                      next.delete(activeGroup.groupKey)
                      return next
                    })
                    setValidation(null)
                  }}
                >
                  <X className="h-4 w-4" aria-hidden="true" />
                  Cancel
                </button>
                <button
                  type="button"
                  className="inline-flex items-center gap-2 rounded border border-amber-700 px-3 py-2 text-sm font-medium text-amber-100 hover:bg-amber-950/30 disabled:opacity-50"
                  disabled={resetMutation.isPending}
                  onClick={() => {
                    if (window.confirm(`Reset ${activeGroup.label} to platform defaults?`)) {
                      resetMutation.mutate(activeGroup)
                    }
                  }}
                  data-testid="routarr-settings-reset-section"
                >
                  <RotateCcw className="h-4 w-4" aria-hidden="true" />
                  Reset to default
                </button>
              </div>

              {saveMutation.isError ? (
                <ApiErrorCallout
                  className="mt-4"
                  title="Save failed"
                  message={getErrorMessage(saveMutation.error, 'Failed to save RoutArr settings.')}
                />
              ) : null}
              {resetMutation.isError ? (
                <ApiErrorCallout
                  className="mt-4"
                  title="Reset failed"
                  message={getErrorMessage(resetMutation.error, 'Failed to reset RoutArr settings.')}
                />
              ) : null}
            </section>

            <section className="grid gap-5 xl:grid-cols-2">
              <AuditPanel
                isLoading={auditQuery.isLoading}
                error={auditQuery.error}
                onRetry={() => void auditQuery.refetch()}
                items={auditQuery.data?.items ?? []}
              />
              <PreviewPanel
                activeGroupKey={activeGroup.groupKey}
                overrides={settings?.overrides ?? []}
                selectedOverrideKey={previewOverrideKey}
                onSelect={setPreviewOverrideKey}
                preview={previewQuery.data ?? null}
                isLoading={previewQuery.isLoading}
                error={previewQuery.error}
                onRetry={() => void previewQuery.refetch()}
              />
            </section>
          </div>
        ) : null}
      </div>
    </section>
  )
}

function SettingField({
  field,
  value,
  onChange,
}: {
  field: RoutArrSettingFieldResponse
  value: RoutArrSettingValue
  onChange: (value: RoutArrSettingValue) => void
}) {
  const id = `routarr-setting-${field.settingKey}`
  const commonClass =
    'mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100'

  return (
    <label className="block rounded border border-slate-800 bg-slate-950/50 p-3 text-sm text-slate-200" htmlFor={id}>
      <span className="font-medium">{field.label}</span>
      <span className="ml-2 rounded bg-slate-800 px-1.5 py-0.5 text-[11px] text-slate-400">
        {sourceLabel(field.effectiveSource)}
      </span>
      <span className="mt-1 block text-xs text-[var(--color-text-muted)]">{field.helpText}</span>

      {field.valueKind === 'boolean' ? (
        <span className="mt-3 flex items-center gap-2">
          <input
            id={id}
            type="checkbox"
            checked={Boolean(value)}
            onChange={(event) => onChange(event.target.checked)}
          />
          <span>{Boolean(value) ? 'Enabled' : 'Disabled'}</span>
        </span>
      ) : null}

      {field.valueKind === 'enum' ? (
        <select
          id={id}
          className={commonClass}
          value={String(value ?? '')}
          onChange={(event) => onChange(event.target.value)}
        >
          {field.options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      ) : null}

      {field.valueKind === 'integer' || field.valueKind === 'decimal' || field.valueKind === 'durationMinutes' ? (
        <input
          id={id}
          className={commonClass}
          type="number"
          value={Number(value ?? 0)}
          onChange={(event) => onChange(field.valueKind === 'decimal' ? Number(event.target.value) : Math.trunc(Number(event.target.value)))}
        />
      ) : null}

      {field.valueKind === 'time' ? (
        <input
          id={id}
          className={commonClass}
          type="time"
          value={String(value ?? '')}
          onChange={(event) => onChange(event.target.value)}
        />
      ) : null}

      {field.valueKind === 'text' ? (
        <input
          id={id}
          className={commonClass}
          type="text"
          value={String(value ?? '')}
          onChange={(event) => onChange(event.target.value)}
        />
      ) : null}

      {field.valueKind === 'multiSelect' ? (
        <div className="mt-3 grid gap-2 sm:grid-cols-2">
          {field.options.map((option) => {
            const values = Array.isArray(value) ? value : []
            const checked = values.includes(option.value)
            return (
              <label key={option.value} className="flex items-center gap-2 text-xs text-slate-300" htmlFor={`${id}-${option.value}`}>
                <input
                  id={`${id}-${option.value}`}
                  type="checkbox"
                  checked={checked}
                  onChange={(event) => {
                    const next = event.target.checked
                      ? [...values, option.value]
                      : values.filter((item) => item !== option.value)
                    onChange(next)
                  }}
                />
                {option.label}
              </label>
            )
          })}
        </div>
      ) : null}

      <span className="mt-2 block text-xs text-[var(--color-text-muted)]">
        Platform default: {formatValue(field.platformDefaultValue)}
      </span>
    </label>
  )
}

function AuditPanel({
  items,
  isLoading,
  error,
  onRetry,
}: {
  items: Array<{
    auditKey: string
    action: string
    changedKeys: string[]
    changedByPersonId: string
    changedAt: string
    summary: string
  }>
  isLoading: boolean
  error: unknown
  onRetry: () => void
}) {
  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="flex items-center justify-between gap-3">
        <h3 className="inline-flex items-center gap-2 text-sm font-semibold text-slate-100">
          <History className="h-4 w-4" aria-hidden="true" />
          Audit history
        </h3>
        <button type="button" className="text-slate-400 hover:text-slate-100" onClick={onRetry} aria-label="Refresh audit">
          <RefreshCw className="h-4 w-4" aria-hidden="true" />
        </button>
      </div>
      {error ? (
        <ApiErrorCallout className="mt-3" message={getErrorMessage(error, 'Failed to load audit history.')} />
      ) : null}
      {isLoading ? <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading audit...</p> : null}
      {!isLoading && items.length === 0 ? (
        <p className="mt-3 rounded border border-dashed border-slate-700 px-3 py-4 text-sm text-[var(--color-text-muted)]">
          No audit entries for this section yet.
        </p>
      ) : null}
      <div className="mt-3 space-y-2">
        {items.map((item) => (
          <article key={item.auditKey} className="rounded border border-slate-800 bg-slate-950 p-3">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <p className="text-sm font-medium text-slate-100">{item.action.replaceAll('_', ' ')}</p>
              <p className="text-xs text-[var(--color-text-muted)]">{formatTimestamp(item.changedAt)}</p>
            </div>
            <p className="mt-1 text-xs text-[var(--color-text-muted)]">
              {item.summary} · StaffArr person {item.changedByPersonId}
            </p>
            <p className="mt-1 text-xs text-[var(--color-text-muted)]">{item.changedKeys.join(', ')}</p>
          </article>
        ))}
      </div>
    </section>
  )
}

function PreviewPanel({
  activeGroupKey,
  overrides,
  selectedOverrideKey,
  onSelect,
  preview,
  isLoading,
  error,
  onRetry,
}: {
  activeGroupKey: string
  overrides: RoutArrTenantSettingOverrideResponse[]
  selectedOverrideKey: string
  onSelect: (value: string) => void
  preview: RoutArrTenantSettingsResponse | null
  isLoading: boolean
  error: unknown
  onRetry: () => void
}) {
  const activePreviewGroup = preview?.groups.find((group) => group.groupKey === activeGroupKey)
  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="flex items-center justify-between gap-3">
        <h3 className="text-sm font-semibold text-slate-100">Effective value preview</h3>
        <button type="button" className="text-slate-400 hover:text-slate-100" onClick={onRetry} aria-label="Refresh preview">
          <RefreshCw className="h-4 w-4" aria-hidden="true" />
        </button>
      </div>
      <select
        className="mt-3 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
        value={selectedOverrideKey}
        onChange={(event) => onSelect(event.target.value)}
        aria-label="Saved override scope"
      >
        <option value="">Tenant defaults</option>
        {overrides.map((override) => (
          <option key={override.overrideKey} value={override.overrideKey}>
            {override.scope.scopeType} · {override.scope.displayLabelSnapshot} · {override.settingGroup}.{override.settingKey}
          </option>
        ))}
      </select>
      {error ? (
        <ApiErrorCallout className="mt-3" message={getErrorMessage(error, 'Failed to preview effective settings.')} />
      ) : null}
      {isLoading ? <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading preview...</p> : null}
      {!selectedOverrideKey ? (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Choose a saved scoped override to preview effective values.</p>
      ) : null}
      {activePreviewGroup ? (
        <div className="mt-3 space-y-2">
          {activePreviewGroup.fields.slice(0, 8).map((field) => (
            <div key={field.settingKey} className="flex items-center justify-between gap-3 rounded border border-slate-800 bg-slate-950 px-3 py-2 text-sm">
              <span className="text-slate-300">{field.label}</span>
              <span className="text-right text-slate-100">{formatValue(field.value)}</span>
            </div>
          ))}
        </div>
      ) : null}
    </section>
  )
}
