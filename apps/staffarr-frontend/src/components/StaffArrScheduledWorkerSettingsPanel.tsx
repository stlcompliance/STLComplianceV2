import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getStaffArrWorkerPendingPreview,
  getStaffArrWorkerRuns,
  getStaffArrWorkerSettings,
  upsertStaffArrWorkerSettings,
} from '../api/client'
import type { StaffArrWorkerPanelConfig } from '../lib/staffarrWorkerPanels'

interface StaffArrScheduledWorkerSettingsPanelProps {
  accessToken: string
  canManage: boolean
  config: StaffArrWorkerPanelConfig
}

export function StaffArrScheduledWorkerSettingsPanel({
  accessToken,
  canManage,
  config,
}: StaffArrScheduledWorkerSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [scanIntervalMinutes, setScanIntervalMinutes] = useState(30)
  const [batchSize, setBatchSize] = useState(50)
  const [stalenessHours, setStalenessHours] = useState(1)

  const settingsQuery = useQuery({
    queryKey: ['staffarr-worker-settings', config.workerKey, accessToken],
    queryFn: () => getStaffArrWorkerSettings(accessToken, config.workerKey),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['staffarr-worker-pending', config.workerKey, accessToken],
    queryFn: () => getStaffArrWorkerPendingPreview(accessToken, config.workerKey),
    enabled: canManage,
  })

  const runsQuery = useQuery({
    queryKey: ['staffarr-worker-runs', config.workerKey, accessToken],
    queryFn: () => getStaffArrWorkerRuns(accessToken, config.workerKey, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setScanIntervalMinutes(data.scanIntervalMinutes)
    setBatchSize(data.batchSize)
    if (config.supportsStaleness && data.stalenessHours != null) {
      setStalenessHours(data.stalenessHours)
    }
    setInitialized(true)
  }, [config.supportsStaleness, initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertStaffArrWorkerSettings(accessToken, config.workerKey, {
        isEnabled,
        scanIntervalMinutes,
        batchSize,
        stalenessHours: config.supportsStaleness ? stalenessHours : null,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['staffarr-worker-settings', config.workerKey, accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['staffarr-worker-pending', config.workerKey, accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['staffarr-worker-runs', config.workerKey, accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  const lastRunLabel = settingsQuery.data?.lastRunAt
    ? new Date(settingsQuery.data.lastRunAt).toLocaleString()
    : 'Never'

  return (
    <section
      className="rounded-lg border border-slate-700 bg-slate-900/80 p-4 shadow-sm"
      data-testid={config.panelTestId}
    >
      <h2 className="text-lg font-semibold text-slate-50">{config.heading}</h2>
      <p className="mt-1 text-sm text-slate-400">{config.description}</p>

      <dl className="mt-4 grid gap-2 text-sm sm:grid-cols-2">
        <div>
          <dt className="text-[var(--color-text-muted)]">Last run</dt>
          <dd className="font-medium text-slate-100">{lastRunLabel}</dd>
        </div>
        <div>
          <dt className="text-[var(--color-text-muted)]">Pending candidates</dt>
          <dd className="font-medium text-slate-100">{settingsQuery.data?.pendingCount ?? 0}</dd>
        </div>
      </dl>

      <div className="mt-4 space-y-3">
        <label htmlFor={`${config.workerKey}-enabled`} className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id={`${config.workerKey}-enabled`}
            type="checkbox"
            data-testid={`${config.workerKey}-enabled`}
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable worker for this tenant
        </label>

        <label htmlFor={`${config.workerKey}-scan-interval`} className="block text-sm text-slate-200">
          Scan interval (minutes)
          <input
            id={`${config.workerKey}-scan-interval`}
            className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2"
            type="number"
            min={1}
            max={1440}
            value={scanIntervalMinutes}
            onChange={(event) => setScanIntervalMinutes(Number(event.target.value))}
          />
        </label>

        <label htmlFor={`${config.workerKey}-batch-size`} className="block text-sm text-slate-200">
          Batch size
          <input
            id={`${config.workerKey}-batch-size`}
            className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2"
            type="number"
            min={1}
            max={500}
            value={batchSize}
            onChange={(event) => setBatchSize(Number(event.target.value))}
          />
        </label>

        {config.supportsStaleness ? (
          <label htmlFor={`${config.workerKey}-staleness-hours`} className="block text-sm text-slate-200">
            Staleness window (hours)
            <input
              id={`${config.workerKey}-staleness-hours`}
              className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2"
              type="number"
              min={1}
              max={168}
              value={stalenessHours}
              onChange={(event) => setStalenessHours(Number(event.target.value))}
            />
          </label>
        ) : null}

        <button
          type="button"
          className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid={config.saveTestId}
        >
          {saveMutation.isPending ? 'Saving…' : `Save ${config.heading.toLowerCase()} settings`}
        </button>
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Pending preview</h3>
        {pendingQuery.isLoading ? (
          <p className="mt-2 text-sm text-slate-400">Loading pending preview…</p>
        ) : null}
        {pendingQuery.data && pendingQuery.data.previewLines.length === 0 ? (
          <p className="mt-2 text-sm text-slate-400" data-testid={`${config.workerKey}-pending-empty`}>
            No pending candidates for this worker.
          </p>
        ) : null}
        {pendingQuery.data && pendingQuery.data.previewLines.length > 0 ? (
          <ul
            className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm"
            data-testid={`${config.workerKey}-pending-list`}
          >
            {pendingQuery.data.previewLines.map((line) => (
              <li key={line} className="px-3 py-2 text-slate-200">
                {line}
              </li>
            ))}
          </ul>
        ) : null}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent runs</h3>
        {runsQuery.isLoading ? (
          <p className="mt-2 text-sm text-slate-400">Loading worker runs…</p>
        ) : null}
        {runsQuery.data && runsQuery.data.items.length === 0 ? (
          <p className="mt-2 text-sm text-slate-400" data-testid={`${config.workerKey}-runs-empty`}>
            No worker runs yet.
          </p>
        ) : null}
        {runsQuery.data && runsQuery.data.items.length > 0 ? (
          <ul
            className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm"
            data-testid={`${config.workerKey}-runs-list`}
          >
            {runsQuery.data.items.map((run) => (
              <li key={run.runId} className="px-3 py-2 text-slate-200">
                {run.processedCount} processed / {run.skippedCount} skipped from {run.candidatesFound} candidates
              </li>
            ))}
          </ul>
        ) : null}
      </div>
    </section>
  )
}
