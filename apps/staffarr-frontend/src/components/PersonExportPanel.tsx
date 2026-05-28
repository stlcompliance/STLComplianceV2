import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  exportPeopleCsv,
  exportPeopleJson,
  exportPeopleZip,
  getOrgUnits,
  getPeopleExportManifest,
  getPersonExportPreset,
  getPersonExportDeliveryNotifications,
  getPersonExportSchedule,
  upsertPersonExportPreset,
  upsertPersonExportSchedule,
} from '../api/client'
import type { PersonExportFilters, PersonExportResponse } from '../api/types'
import {
  PERSON_EXPORT_FILTER_PRESETS,
  applyPersonExportFilterPreset,
  describeActiveExportFilters,
  describeTenantExportPreset,
  inferPersonExportFilterPresetKey,
  isPersonExportFilterPresetEnabled,
  personExportPresetResponseToState,
  resolvePersonExportFilters,
  type PersonExportFilterPresetKey,
} from '../lib/personExportFilterPresets'

interface PersonExportPanelProps {
  accessToken: string
  canExport: boolean
}

export function PersonExportPanel({ accessToken, canExport }: PersonExportPanelProps) {
  const queryClient = useQueryClient()
  const [employmentStatus, setEmploymentStatus] = useState('')
  const [orgUnitId, setOrgUnitId] = useState('')
  const [filtersInitialized, setFiltersInitialized] = useState(false)
  const [scheduleEnabled, setScheduleEnabled] = useState(false)
  const [scheduleIntervalHours, setScheduleIntervalHours] = useState('24')
  const [notificationWebhookUrl, setNotificationWebhookUrl] = useState('')
  const [notifyOnSuccess, setNotifyOnSuccess] = useState(true)
  const [notifyOnFailure, setNotifyOnFailure] = useState(true)
  const [scheduleInitialized, setScheduleInitialized] = useState(false)
  const [lastJsonExport, setLastJsonExport] = useState<PersonExportResponse | null>(null)

  const manifestQuery = useQuery({
    queryKey: ['staffarr-people-export-manifest', accessToken],
    queryFn: () => getPeopleExportManifest(accessToken),
    enabled: canExport,
  })

  const orgUnitsQuery = useQuery({
    queryKey: ['staffarr-org-units', accessToken],
    queryFn: () => getOrgUnits(accessToken),
    enabled: canExport,
  })

  const tenantPresetQuery = useQuery({
    queryKey: ['staffarr-people-export-preset', accessToken],
    queryFn: () => getPersonExportPreset(accessToken),
    enabled: canExport,
  })

  const exportScheduleQuery = useQuery({
    queryKey: ['staffarr-people-export-schedule', accessToken],
    queryFn: () => getPersonExportSchedule(accessToken),
    enabled: canExport,
  })

  const deliveryNotificationsQuery = useQuery({
    queryKey: ['staffarr-people-export-delivery-notifications', accessToken],
    queryFn: () => getPersonExportDeliveryNotifications(accessToken, 5),
    enabled: canExport,
  })

  useEffect(() => {
    if (scheduleInitialized || exportScheduleQuery.isLoading || !exportScheduleQuery.data) {
      return
    }
    setScheduleEnabled(exportScheduleQuery.data.isEnabled)
    setScheduleIntervalHours(String(exportScheduleQuery.data.intervalHours))
    setNotificationWebhookUrl(exportScheduleQuery.data.notificationWebhookUrl ?? '')
    setNotifyOnSuccess(exportScheduleQuery.data.notifyOnSuccess)
    setNotifyOnFailure(exportScheduleQuery.data.notifyOnFailure)
    setScheduleInitialized(true)
  }, [scheduleInitialized, exportScheduleQuery.data, exportScheduleQuery.isLoading])

  useEffect(() => {
    if (filtersInitialized || tenantPresetQuery.isLoading || tenantPresetQuery.data === undefined) {
      return
    }

    if (tenantPresetQuery.data) {
      const state = personExportPresetResponseToState(tenantPresetQuery.data)
      setEmploymentStatus(state.employmentStatus)
      setOrgUnitId(state.orgUnitId)
    }

    setFiltersInitialized(true)
  }, [filtersInitialized, tenantPresetQuery.data, tenantPresetQuery.isLoading])

  const filters = resolvePersonExportFilters({ employmentStatus, orgUnitId })

  const applyPreset = (presetKey: PersonExportFilterPresetKey) => {
    const next = applyPersonExportFilterPreset(presetKey, { employmentStatus, orgUnitId })
    setEmploymentStatus(next.employmentStatus)
    setOrgUnitId(next.orgUnitId)
  }

  const applyTenantDefault = () => {
    if (!tenantPresetQuery.data) {
      return
    }
    const state = personExportPresetResponseToState(tenantPresetQuery.data)
    setEmploymentStatus(state.employmentStatus)
    setOrgUnitId(state.orgUnitId)
  }

  const saveTenantPresetMutation = useMutation({
    mutationFn: () =>
      upsertPersonExportPreset(accessToken, {
        employmentStatus: filters.employmentStatus,
        orgUnitId: filters.orgUnitId ?? null,
        presetKey: inferPersonExportFilterPresetKey({ employmentStatus, orgUnitId }),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['staffarr-people-export-preset', accessToken] })
    },
  })

  const saveExportScheduleMutation = useMutation({
    mutationFn: () =>
      upsertPersonExportSchedule(accessToken, {
        isEnabled: scheduleEnabled,
        intervalHours: Number.parseInt(scheduleIntervalHours, 10) || 24,
        notificationWebhookUrl: notificationWebhookUrl.trim() || null,
        notifyOnSuccess,
        notifyOnFailure,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['staffarr-people-export-schedule', accessToken] })
      queryClient.invalidateQueries({
        queryKey: ['staffarr-people-export-delivery-notifications', accessToken],
      })
    },
  })

  const csvExportMutation = useMutation({
    mutationFn: (exportFilters: PersonExportFilters) => exportPeopleCsv(accessToken, exportFilters),
    onSuccess: (csv) => {
      const blob = new Blob([csv], { type: 'text/csv' })
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `staffarr-people-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const zipExportMutation = useMutation({
    mutationFn: (exportFilters: PersonExportFilters) => exportPeopleZip(accessToken, exportFilters),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `staffarr-people-export-${new Date().toISOString().slice(0, 10)}.zip`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const jsonExportMutation = useMutation({
    mutationFn: (exportFilters: PersonExportFilters) => exportPeopleJson(accessToken, exportFilters),
    onSuccess: (result) => {
      setLastJsonExport(result)
    },
  })

  return (
    <section className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Person export bundle</h2>
        <p className="mt-1 text-sm text-slate-400">
          Export workforce directory CSV compatible with bulk import, plus JSON preview and ZIP bundle.
        </p>
      </header>

      {canExport ? (
        <>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
            <p className="font-mono text-xs text-slate-400">
              {manifestQuery.data?.csvHeader ?? PeopleExportServiceFallbackHeader}
            </p>
          </div>

          <div className="space-y-2">
            <p className="text-sm text-slate-300">Quick filter presets</p>
            <div className="flex flex-wrap gap-2">
              {PERSON_EXPORT_FILTER_PRESETS.map((preset) => {
                const enabled = isPersonExportFilterPresetEnabled(preset, orgUnitId)
                return (
                  <button
                    key={preset.key}
                    type="button"
                    title={preset.description}
                    disabled={!enabled}
                    onClick={() => applyPreset(preset.key)}
                    className="rounded-md border border-slate-600 px-3 py-1.5 text-xs text-slate-100 hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-40"
                  >
                    {preset.label}
                  </button>
                )
              })}
            </div>
            <p className="text-xs text-slate-500">{describeActiveExportFilters({ employmentStatus, orgUnitId })}</p>
          </div>

          <div className="space-y-2 rounded-lg border border-slate-800 bg-slate-950/40 p-3">
            <p className="text-sm text-slate-300">Tenant export default</p>
            {tenantPresetQuery.data ? (
              <p className="text-xs text-slate-500">{describeTenantExportPreset(tenantPresetQuery.data)}</p>
            ) : (
              <p className="text-xs text-slate-500">No tenant default saved yet.</p>
            )}
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                disabled={saveTenantPresetMutation.isPending}
                onClick={() => saveTenantPresetMutation.mutate()}
                className="rounded-md border border-slate-600 px-3 py-1.5 text-xs text-slate-100 hover:bg-slate-800 disabled:opacity-50"
              >
                Save tenant default
              </button>
              <button
                type="button"
                disabled={!tenantPresetQuery.data}
                onClick={applyTenantDefault}
                className="rounded-md border border-slate-600 px-3 py-1.5 text-xs text-slate-100 hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-40"
              >
                Apply tenant default
              </button>
            </div>
            {saveTenantPresetMutation.isSuccess ? (
              <p className="text-xs text-emerald-400">Tenant default saved.</p>
            ) : null}
          </div>

          <div className="space-y-2 rounded-lg border border-slate-800 bg-slate-950/40 p-3">
            <p className="text-sm text-slate-300">Scheduled export delivery</p>
            <p className="text-xs text-slate-500">
              Runs workforce exports on an interval using the tenant default filters. Delivery is recorded in StaffArr audit history.
            </p>
            <label className="flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={scheduleEnabled}
                onChange={(event) => {
                  setScheduleInitialized(true)
                  setScheduleEnabled(event.target.checked)
                }}
                className="rounded border-slate-600 bg-slate-950"
              />
              Enable scheduled delivery
            </label>
            <label className="block text-sm text-slate-300">
              Delivery interval (hours)
              <input
                type="number"
                min={1}
                max={720}
                value={scheduleIntervalHours}
                onChange={(event) => {
                  setScheduleInitialized(true)
                  setScheduleIntervalHours(event.target.value)
                }}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            {exportScheduleQuery.data?.lastDeliveredAt ? (
              <p className="text-xs text-slate-500">
                Last delivered {new Date(exportScheduleQuery.data.lastDeliveredAt).toLocaleString()}
              </p>
            ) : (
              <p className="text-xs text-slate-500">No scheduled delivery recorded yet.</p>
            )}
            <label className="block text-sm text-slate-300">
              Notification webhook URL (optional)
              <input
                type="url"
                value={notificationWebhookUrl}
                onChange={(event) => {
                  setScheduleInitialized(true)
                  setNotificationWebhookUrl(event.target.value)
                }}
                placeholder="https://hooks.example.com/staffarr-export"
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label className="flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={notifyOnSuccess}
                onChange={(event) => {
                  setScheduleInitialized(true)
                  setNotifyOnSuccess(event.target.checked)
                }}
                className="rounded border-slate-600 bg-slate-950"
              />
              Notify on successful delivery
            </label>
            <label className="flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={notifyOnFailure}
                onChange={(event) => {
                  setScheduleInitialized(true)
                  setNotifyOnFailure(event.target.checked)
                }}
                className="rounded border-slate-600 bg-slate-950"
              />
              Notify on failed delivery
            </label>
            {deliveryNotificationsQuery.data?.items[0] ? (
              <p className="text-xs text-slate-500">
                Last notification: {deliveryNotificationsQuery.data.items[0].eventKind}{' '}
                {deliveryNotificationsQuery.data.items[0].deliveryStatus}
                {deliveryNotificationsQuery.data.items[0].webhookHost
                  ? ` → ${deliveryNotificationsQuery.data.items[0].webhookHost}`
                  : ''}
              </p>
            ) : null}
            <button
              type="button"
              disabled={saveExportScheduleMutation.isPending}
              onClick={() => saveExportScheduleMutation.mutate()}
              className="rounded-md border border-slate-600 px-3 py-1.5 text-xs text-slate-100 hover:bg-slate-800 disabled:opacity-50"
            >
              Save schedule
            </button>
            {saveExportScheduleMutation.isSuccess ? (
              <p className="text-xs text-emerald-400">Export schedule saved.</p>
            ) : null}
          </div>

          <label className="block text-sm text-slate-300">
            Employment status filter (optional)
            <select
              value={employmentStatus}
              onChange={(event) => setEmploymentStatus(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">All statuses</option>
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
              <option value="terminated">Terminated</option>
            </select>
          </label>

          <label className="block text-sm text-slate-300">
            Primary org unit filter (optional)
            <select
              value={orgUnitId}
              onChange={(event) => setOrgUnitId(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">All org units</option>
              {(orgUnitsQuery.data ?? [])
                .filter((unit) => unit.status === 'active')
                .map((unit) => (
                  <option key={unit.orgUnitId} value={unit.orgUnitId}>
                    {unit.unitType} · {unit.name}
                  </option>
                ))}
            </select>
          </label>

          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              disabled={csvExportMutation.isPending}
              onClick={() => csvExportMutation.mutate(filters)}
              className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            >
              Download CSV
            </button>
            <button
              type="button"
              disabled={zipExportMutation.isPending}
              onClick={() => zipExportMutation.mutate(filters)}
              className="rounded-md border border-slate-600 px-4 py-2 text-sm text-slate-100 hover:bg-slate-800 disabled:opacity-50"
            >
              Download ZIP bundle
            </button>
            <button
              type="button"
              disabled={jsonExportMutation.isPending}
              onClick={() => jsonExportMutation.mutate(filters)}
              className="rounded-md border border-slate-600 px-4 py-2 text-sm text-slate-100 hover:bg-slate-800 disabled:opacity-50"
            >
              Preview JSON export
            </button>
          </div>
        </>
      ) : (
        <p className="text-sm text-slate-500">
          Person export requires tenant admin, StaffArr admin, or HR admin role.
        </p>
      )}

      {lastJsonExport ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Exported {lastJsonExport.personCount} people at{' '}
            {new Date(lastJsonExport.generatedAt).toLocaleString()}
          </p>
        </div>
      ) : null}
    </section>
  )
}

const PeopleExportServiceFallbackHeader =
  'givenName,familyName,primaryEmail,employmentStatus,jobTitle,managerEmail,primaryOrgUnitId,personId'
