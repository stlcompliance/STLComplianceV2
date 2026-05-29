import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import { getM12AnalyticsWorkerSettings, upsertM12AnalyticsWorkerSettings } from '../api/client'

interface M12AnalyticsWorkerSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function M12AnalyticsWorkerSettingsPanel({
  accessToken,
  canManage,
}: M12AnalyticsWorkerSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [defaultScopeKey, setDefaultScopeKey] = useState('tenant')
  const [intervalHours, setIntervalHours] = useState('24')
  const [riskScoringEnabled, setRiskScoringEnabled] = useState(true)
  const [missingEvidenceEnabled, setMissingEvidenceEnabled] = useState(true)
  const [controlEffectivenessEnabled, setControlEffectivenessEnabled] = useState(true)
  const [readinessForecastEnabled, setReadinessForecastEnabled] = useState(true)
  const [auditDeliveryEnabled, setAuditDeliveryEnabled] = useState(false)

  const settingsQuery = useQuery({
    queryKey: ['compliancecore-m12-analytics-worker-settings', accessToken],
    queryFn: () => getM12AnalyticsWorkerSettings(accessToken),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setDefaultScopeKey(data.defaultScopeKey)
    setIntervalHours(String(data.intervalHours))
    setRiskScoringEnabled(data.riskScoringEnabled)
    setMissingEvidenceEnabled(data.missingEvidenceEnabled)
    setControlEffectivenessEnabled(data.controlEffectivenessEnabled)
    setReadinessForecastEnabled(data.readinessForecastEnabled)
    setAuditDeliveryEnabled(data.auditDeliveryEnabled)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertM12AnalyticsWorkerSettings(accessToken, {
        isEnabled,
        defaultScopeKey: defaultScopeKey.trim() || 'tenant',
        intervalHours: Number.parseInt(intervalHours, 10) || 24,
        riskScoringEnabled,
        missingEvidenceEnabled,
        controlEffectivenessEnabled,
        readinessForecastEnabled,
        auditDeliveryEnabled,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['compliancecore-m12-analytics-worker-settings', accessToken],
      })
    },
  })

  if (!canManage) {
    return null
  }

  const settings = settingsQuery.data

  return (
    <section
      data-testid="compliancecore-m12-analytics-worker-settings-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">M12 analytics worker</h2>
        <p className="mt-1 text-sm text-slate-400">
          Configure scheduled shared-worker batches for risk scoring, missing evidence warnings,
          control effectiveness, readiness forecasting, and optional audit package delivery.
        </p>
      </header>

      <label htmlFor="compliancecore-m12-worker-enabled" className="flex items-center gap-2 text-sm text-slate-200">
        <input
          id="compliancecore-m12-worker-enabled"
          type="checkbox"
          checked={isEnabled}
          onChange={(e) => setIsEnabled(e.target.checked)}
          data-testid="compliancecore-m12-worker-enabled"
        />
        Enable scheduled M12 analytics batches
      </label>

      <div className="grid gap-3 sm:grid-cols-2">
        <label htmlFor="compliancecore-m12-worker-scope" className="block text-sm text-slate-300">
          Default analytics scope key
          <input
            id="compliancecore-m12-worker-scope"
            type="text"
            value={defaultScopeKey}
            onChange={(e) => setDefaultScopeKey(e.target.value)}
            data-testid="compliancecore-m12-worker-scope"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
        <label htmlFor="compliancecore-m12-worker-interval" className="block text-sm text-slate-300">
          Batch interval (hours)
          <input
            id="compliancecore-m12-worker-interval"
            type="number"
            min={1}
            max={168}
            value={intervalHours}
            onChange={(e) => setIntervalHours(e.target.value)}
            data-testid="compliancecore-m12-worker-interval"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
      </div>

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Batch steps</h3>
        <div className="mt-3 grid gap-2 sm:grid-cols-2">
          <label htmlFor="compliancecore-m12-worker-risk" className="flex items-center gap-2 text-sm text-slate-300">
            <input
              id="compliancecore-m12-worker-risk"
              type="checkbox"
              checked={riskScoringEnabled}
              onChange={(e) => setRiskScoringEnabled(e.target.checked)}
              data-testid="compliancecore-m12-worker-risk"
            />
            Risk scoring batch step
          </label>
          <label htmlFor="compliancecore-m12-worker-missing-evidence" className="flex items-center gap-2 text-sm text-slate-300">
            <input
              id="compliancecore-m12-worker-missing-evidence"
              type="checkbox"
              checked={missingEvidenceEnabled}
              onChange={(e) => setMissingEvidenceEnabled(e.target.checked)}
              data-testid="compliancecore-m12-worker-missing-evidence"
            />
            Missing evidence warnings batch step
          </label>
          <label htmlFor="compliancecore-m12-worker-control" className="flex items-center gap-2 text-sm text-slate-300">
            <input
              id="compliancecore-m12-worker-control"
              type="checkbox"
              checked={controlEffectivenessEnabled}
              onChange={(e) => setControlEffectivenessEnabled(e.target.checked)}
              data-testid="compliancecore-m12-worker-control"
            />
            Control effectiveness batch step
          </label>
          <label htmlFor="compliancecore-m12-worker-forecast" className="flex items-center gap-2 text-sm text-slate-300">
            <input
              id="compliancecore-m12-worker-forecast"
              type="checkbox"
              checked={readinessForecastEnabled}
              onChange={(e) => setReadinessForecastEnabled(e.target.checked)}
              data-testid="compliancecore-m12-worker-forecast"
            />
            Readiness forecast batch step
          </label>
          <label htmlFor="compliancecore-m12-worker-audit-delivery" className="flex items-center gap-2 text-sm text-slate-300 sm:col-span-2">
            <input
              id="compliancecore-m12-worker-audit-delivery"
              type="checkbox"
              checked={auditDeliveryEnabled}
              onChange={(e) => setAuditDeliveryEnabled(e.target.checked)}
              data-testid="compliancecore-m12-worker-audit-delivery"
            />
            Audit package delivery hook (enqueue ZIP generation job)
          </label>
        </div>
      </div>

      {settings && (
        <div
          data-testid="compliancecore-m12-worker-last-runs"
          className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-400"
        >
          <p>
            Last batch:{' '}
            {settings.lastBatchRunAt
              ? new Date(settings.lastBatchRunAt).toLocaleString()
              : 'Never'}
          </p>
          <p className="mt-1">
            Last readiness forecast:{' '}
            {settings.lastReadinessForecastRunAt
              ? new Date(settings.lastReadinessForecastRunAt).toLocaleString()
              : 'Never'}
          </p>
          <p className="mt-1">
            Last audit delivery:{' '}
            {settings.lastAuditDeliveryRunAt
              ? new Date(settings.lastAuditDeliveryRunAt).toLocaleString()
              : 'Never'}
          </p>
        </div>
      )}

      <button
        type="button"
        onClick={() => saveMutation.mutate()}
        disabled={saveMutation.isPending}
        data-testid="compliancecore-m12-worker-save"
        className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
      >
        {saveMutation.isPending ? 'Saving…' : 'Save worker settings'}
      </button>
    </section>
  )
}
