import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getDemandProcessingRuns,
  getDemandProcessingSettings,
  getPendingDemandProcessing,
  upsertDemandProcessingSettings,
} from '../api/client'

interface DemandProcessingSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

const SOURCE_LABELS: {
  key: keyof SourceFlags
  label: string
  help: string
}[] = [
  {
    key: 'processMaintainarrDemandRefs',
    label: 'MaintainArr work-order demand',
    help: 'Parts demand published from MaintainArr work orders (enabled by default).',
  },
  {
    key: 'processRoutarrDemandRefs',
    label: 'RoutArr trip parts demand',
    help: 'Trip material demand from RoutArr; requires RoutArr to publish demand refs to SupplyArr.',
  },
  {
    key: 'processTrainarrDemandRefs',
    label: 'TrainArr assignment materials demand',
    help: 'Assignment material lines from TrainArr; status callbacks update procurement badges.',
  },
  {
    key: 'processStaffarrDemandRefs',
    label: 'StaffArr incident supply demand',
    help: 'Incident-driven supply demand from StaffArr personnel workflows.',
  },
]

type SourceFlags = {
  processMaintainarrDemandRefs: boolean
  processRoutarrDemandRefs: boolean
  processTrainarrDemandRefs: boolean
  processStaffarrDemandRefs: boolean
}

export function DemandProcessingSettingsPanel({
  accessToken,
  canManage,
}: DemandProcessingSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [autoCreatePrDraftWhenShort, setAutoCreatePrDraftWhenShort] = useState(false)
  const [minHoursBeforeProcessing, setMinHoursBeforeProcessing] = useState(0)
  const [stalenessHours, setStalenessHours] = useState(4)
  const [notifyOnPrDraftCreated, setNotifyOnPrDraftCreated] = useState(true)
  const [sourceFlags, setSourceFlags] = useState<SourceFlags>({
    processMaintainarrDemandRefs: true,
    processRoutarrDemandRefs: false,
    processTrainarrDemandRefs: false,
    processStaffarrDemandRefs: false,
  })

  const settingsQuery = useQuery({
    queryKey: ['supplyarr-demand-processing-settings', accessToken],
    queryFn: () => getDemandProcessingSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['supplyarr-demand-processing-pending', accessToken],
    queryFn: () => getPendingDemandProcessing(accessToken),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['supplyarr-demand-processing-runs', accessToken],
    queryFn: () => getDemandProcessingRuns(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setAutoCreatePrDraftWhenShort(data.autoCreatePrDraftWhenShort)
    setMinHoursBeforeProcessing(data.minHoursBeforeProcessing)
    setStalenessHours(data.stalenessHours)
    setNotifyOnPrDraftCreated(data.notifyOnPrDraftCreated)
    setSourceFlags({
      processMaintainarrDemandRefs: data.processMaintainarrDemandRefs,
      processRoutarrDemandRefs: data.processRoutarrDemandRefs,
      processTrainarrDemandRefs: data.processTrainarrDemandRefs,
      processStaffarrDemandRefs: data.processStaffarrDemandRefs,
    })
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const anySourceEnabled = Object.values(sourceFlags).some(Boolean)
  const settingsValidationError =
    isEnabled && !anySourceEnabled
      ? 'Enable at least one demand source when the worker is on.'
      : autoCreatePrDraftWhenShort && !anySourceEnabled
        ? 'Auto PR draft requires at least one enabled demand source.'
        : null

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertDemandProcessingSettings(accessToken, {
        isEnabled,
        autoCreatePrDraftWhenShort,
        minHoursBeforeProcessing,
        stalenessHours,
        notifyOnPrDraftCreated,
        ...sourceFlags,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-demand-processing-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-demand-processing-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-demand-processing-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-demand-processing', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="demand-processing-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Demand processing worker</h2>
      <p className="mt-1 text-sm text-slate-400">
        Scheduled stock checks on received demand references from enabled sources, with optional auto PR draft
        creation when stock is short.
      </p>

      {settingsQuery.isLoading && <p className="mt-3 text-sm text-slate-500">Loading settings…</p>}

      {settingsQuery.data && (
        <div className="mt-4 grid gap-4 md:grid-cols-2">
          <label className="flex items-center gap-2 text-sm text-slate-300 md:col-span-2">
            <input
              type="checkbox"
              checked={isEnabled}
              onChange={(event) => setIsEnabled(event.target.checked)}
            />
            Enable demand processing worker
          </label>
          <fieldset className="md:col-span-2">
            <legend className="text-sm font-medium text-slate-300">Demand sources</legend>
            <p className="mt-1 text-xs text-slate-500">
              Select which cross-product demand publications the worker may evaluate. At least one
              source is required when the worker or auto PR draft is enabled.
            </p>
            <div className="mt-2 grid gap-3 sm:grid-cols-2">
              {SOURCE_LABELS.map(({ key, label, help }) => (
                <label
                  key={key}
                  className="flex gap-2 rounded-md border border-slate-800 p-2 text-sm text-slate-400"
                >
                  <input
                    type="checkbox"
                    className="mt-1"
                    checked={sourceFlags[key]}
                    onChange={(event) =>
                      setSourceFlags((prev) => ({ ...prev, [key]: event.target.checked }))
                    }
                  />
                  <span>
                    <span className="text-slate-300">{label}</span>
                    <span className="mt-1 block text-xs text-slate-500">{help}</span>
                  </span>
                </label>
              ))}
            </div>
          </fieldset>
          {settingsValidationError ? (
            <p className="text-sm text-amber-300 md:col-span-2">{settingsValidationError}</p>
          ) : null}
          <label className="flex items-center gap-2 text-sm text-slate-300 md:col-span-2">
            <input
              type="checkbox"
              checked={autoCreatePrDraftWhenShort}
              onChange={(event) => setAutoCreatePrDraftWhenShort(event.target.checked)}
            />
            Auto-create PR draft when stock is short
          </label>
          <label className="block text-sm text-slate-400">
            Min hours before first processing
            <input
              type="number"
              min={0}
              max={168}
              className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
              value={minHoursBeforeProcessing}
              onChange={(event) => setMinHoursBeforeProcessing(Number(event.target.value))}
            />
          </label>
          <label className="block text-sm text-slate-400">
            Reprocess staleness (hours)
            <input
              type="number"
              min={1}
              max={168}
              className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
              value={stalenessHours}
              onChange={(event) => setStalenessHours(Number(event.target.value))}
            />
          </label>
          <label className="flex items-center gap-2 text-sm text-slate-300 md:col-span-2">
            <input
              type="checkbox"
              checked={notifyOnPrDraftCreated}
              onChange={(event) => setNotifyOnPrDraftCreated(event.target.checked)}
            />
            Notify when auto PR draft is created (MaintainArr webhook only)
          </label>
          <button
            type="button"
            className="rounded bg-emerald-600 px-3 py-1.5 text-sm text-white disabled:opacity-50 md:col-span-2 md:w-fit"
            disabled={saveMutation.isPending || Boolean(settingsValidationError)}
            onClick={() => saveMutation.mutate()}
          >
            Save settings
          </button>
        </div>
      )}

      {isEnabled && pendingQuery.data && (
        <div className="mt-4 rounded-md border border-slate-800 p-3 text-sm">
          <h3 className="font-medium text-slate-200">Pending processing</h3>
          <p className="text-slate-500">{pendingQuery.data.items.length} demand references due</p>
          {pendingQuery.data.items.length > 0 && (
            <ul className="mt-2 space-y-1 text-xs text-slate-500">
              {pendingQuery.data.items.slice(0, 5).map((item) => (
                <li key={item.demandRefId}>
                  {item.demandRefSource}: {item.sourceRefKey} · {item.title}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {runsQuery.data && runsQuery.data.items.length > 0 && (
        <div className="mt-4 rounded-md border border-slate-800 p-3 text-sm">
          <h3 className="font-medium text-slate-200">Recent runs</h3>
          <ul className="mt-2 space-y-1 text-slate-400">
            {runsQuery.data.items.map((run) => (
              <li key={run.runId}>
                {run.candidatesFound} candidates · {run.processedCount} processed · {run.prDraftsCreatedCount} PR
                drafts
              </li>
            ))}
          </ul>
        </div>
      )}
    </section>
  )
}
