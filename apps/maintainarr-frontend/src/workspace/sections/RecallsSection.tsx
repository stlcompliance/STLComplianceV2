import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Loader2, Radar, RefreshCw, Search } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useState } from 'react'

import {
  getRecallDashboard,
  getRecallProviderHealth,
  getRecallProviders,
  listRecallCampaigns,
  searchRecallCampaignByNumber,
  searchRecallCampaignsByVehicle,
} from '../../api/client'
import type { RecallCampaignResponse } from '../../api/types'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

function formatDateTime(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleString()
}

function statusTone(status: string): string {
  const normalized = status.toLowerCase()
  if (['healthy', 'active', 'ready', 'open', 'verified_open'].includes(normalized)) {
    return 'border-emerald-500/30 bg-emerald-500/10 text-emerald-100'
  }
  if (['warn', 'warning', 'planned', 'pending', 'potential_match', 'needs_vin_check', 'needs_manual_review'].includes(normalized)) {
    return 'border-amber-500/30 bg-amber-500/10 text-amber-100'
  }
  if (['disabled', 'error', 'resolved', 'dismissed', 'closed'].includes(normalized)) {
    return 'border-rose-500/30 bg-rose-500/10 text-rose-100'
  }
  return 'border-slate-700 bg-slate-900 text-slate-200'
}

function summaryTone(count: number): string {
  return count > 0 ? 'text-white' : 'text-slate-300'
}

function campaignDisplayLabel(campaign: RecallCampaignResponse): string {
  return campaign.nhtsaCampaignNumber
    ?? campaign.manufacturerCampaignNumber
    ?? campaign.sourceProviderRecordId
    ?? campaign.campaignId
}

function compareCampaignRisk(a: RecallCampaignResponse, b: RecallCampaignResponse): number {
  const verifiedOpenDiff = b.verifiedOpenCaseCount - a.verifiedOpenCaseCount
  if (verifiedOpenDiff !== 0) return verifiedOpenDiff

  const openDiff = b.openCaseCount - a.openCaseCount
  if (openDiff !== 0) return openDiff

  const assetCaseDiff = b.assetCaseCount - a.assetCaseCount
  if (assetCaseDiff !== 0) return assetCaseDiff

  return Date.parse(b.updatedAt) - Date.parse(a.updatedAt)
}

function coverageTone(coveragePercent: number, openCaseCount: number): string {
  if (openCaseCount > 0) {
    return 'border-amber-500/30 bg-amber-500/10 text-amber-100'
  }
  if (coveragePercent === 100) {
    return 'border-emerald-500/30 bg-emerald-500/10 text-emerald-100'
  }
  if (coveragePercent > 0) {
    return 'border-sky-500/30 bg-sky-500/10 text-sky-100'
  }
  return 'border-slate-700 bg-slate-900 text-slate-200'
}

export function RecallsSection({ state }: Props) {
  const s = state
  const queryClient = useQueryClient()
  const [vehicleYear, setVehicleYear] = useState('')
  const [vehicleMake, setVehicleMake] = useState('')
  const [vehicleModel, setVehicleModel] = useState('')
  const [campaignNumber, setCampaignNumber] = useState('')
  const [vehicleResults, setVehicleResults] = useState<RecallCampaignResponse[]>([])
  const [campaignResults, setCampaignResults] = useState<RecallCampaignResponse[]>([])
  const [serverError, setServerError] = useState<string | null>(null)

  const dashboardQuery = useQuery({
    queryKey: ['maintainarr-recall-dashboard'],
    queryFn: () => getRecallDashboard(s.accessToken),
    enabled: Boolean(s.accessToken),
  })

  const providersQuery = useQuery({
    queryKey: ['maintainarr-recall-providers'],
    queryFn: () => getRecallProviders(s.accessToken),
    enabled: Boolean(s.accessToken),
  })

  const providerHealthQuery = useQuery({
    queryKey: ['maintainarr-recall-provider-health'],
    queryFn: () => getRecallProviderHealth(s.accessToken),
    enabled: Boolean(s.accessToken),
  })

  const campaignsQuery = useQuery({
    queryKey: ['maintainarr-recall-campaigns'],
    queryFn: () => listRecallCampaigns(s.accessToken, { limit: 50 }),
    enabled: Boolean(s.accessToken),
  })

  const refreshMutation = useMutation({
    mutationFn: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-recall-dashboard'] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-recall-providers'] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-recall-provider-health'] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-recall-campaigns'] }),
      ])
    },
  })

  const vehicleSearchMutation = useMutation({
    mutationFn: async () => {
      const year = Number(vehicleYear)
      if (!Number.isFinite(year) || year <= 0) {
        throw new Error('Enter a valid model year.')
      }
      if (!vehicleMake.trim() || !vehicleModel.trim()) {
        throw new Error('Enter make and model.')
      }
      return searchRecallCampaignsByVehicle(s.accessToken, {
        year,
        make: vehicleMake.trim(),
        model: vehicleModel.trim(),
      })
    },
    onSuccess: (results) => {
      setServerError(null)
      setVehicleResults(results)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to search recall campaigns.')
    },
  })

  const campaignSearchMutation = useMutation({
    mutationFn: async () => {
      if (!campaignNumber.trim()) {
        throw new Error('Enter a campaign number.')
      }
      return searchRecallCampaignByNumber(s.accessToken, {
        campaignNumber: campaignNumber.trim(),
      })
    },
    onSuccess: (results) => {
      setServerError(null)
      setCampaignResults(results)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to search campaign number.')
    },
  })

  const dashboard = dashboardQuery.data ?? null
  const providers = providersQuery.data ?? []
  const providerHealth = providerHealthQuery.data ?? []
  const campaigns = campaignsQuery.data ?? []

  const searchResults = vehicleResults.length > 0 ? vehicleResults : campaignResults
  const trackedCampaignCount = campaigns.length
  const campaignsWithCases = campaigns.filter((campaign) => campaign.assetCaseCount > 0)
  const campaignsWithOpenCases = campaigns.filter((campaign) => campaign.openCaseCount > 0)
  const totalAssetCases = campaigns.reduce((count, campaign) => count + campaign.assetCaseCount, 0)
  const totalOpenCases = campaigns.reduce((count, campaign) => count + campaign.openCaseCount, 0)
  const totalVerifiedOpenCases = campaigns.reduce((count, campaign) => count + campaign.verifiedOpenCaseCount, 0)
  const coveragePercent = trackedCampaignCount > 0
    ? Math.round((campaignsWithCases.length / trackedCampaignCount) * 100)
    : 0
  const topRiskCampaigns = [...campaigns].sort(compareCampaignRisk).slice(0, 3)
  const latestCampaignUpdate = campaigns.reduce<RecallCampaignResponse | null>((latest, campaign) => {
    if (!latest) return campaign
    return Date.parse(campaign.updatedAt) > Date.parse(latest.updatedAt) ? campaign : latest
  }, null)
  const coverageSummary = trackedCampaignCount === 0
    ? 'No recall campaigns are stored yet.'
    : [
        `${campaignsWithCases.length} of ${trackedCampaignCount} tracked campaign${trackedCampaignCount === 1 ? '' : 's'} have asset cases.`,
        totalOpenCases > 0
          ? `${totalOpenCases} open case${totalOpenCases === 1 ? '' : 's'} remain across ${campaignsWithOpenCases.length} campaign${campaignsWithOpenCases.length === 1 ? '' : 's'}.`
          : 'No open cases remain.',
        totalVerifiedOpenCases > 0
          ? `${totalVerifiedOpenCases} verified-open case${totalVerifiedOpenCases === 1 ? '' : 's'} still need closeout.`
          : null,
      ].filter(Boolean).join(' ')

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className="text-sm font-medium text-white">Recall dashboard</p>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Campaign discovery, provider health, and fleet-level recall attention items.
          </p>
        </div>
        <button
          type="button"
          className="inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-xs font-medium text-slate-100 hover:bg-slate-800 disabled:opacity-50"
          disabled={refreshMutation.isPending}
          onClick={() => refreshMutation.mutate()}
        >
          <RefreshCw className={`h-4 w-4 ${refreshMutation.isPending ? 'animate-spin' : ''}`} />
          Refresh dashboard
        </button>
      </div>

      {serverError ? (
        <p className="rounded-lg border border-rose-500/30 bg-rose-500/10 p-3 text-sm text-rose-100">{serverError}</p>
      ) : null}

      <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-4">
        {[
          ['Verified open', dashboard?.verifiedOpenRecallCount ?? 0],
          ['Potential matches', dashboard?.potentialMatchCount ?? 0],
          ['Park it', dashboard?.parkItWarningCount ?? 0],
          ['Park outside', dashboard?.parkOutsideWarningCount ?? 0],
        ].map(([label, count]) => (
          <div key={label} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</div>
            <div className={`mt-1 text-lg font-semibold ${summaryTone(Number(count))}`}>{count}</div>
          </div>
        ))}
      </div>

      <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-4">
        {[
          ['Work orders', dashboard?.workOrdersCreatedCount ?? 0],
          ['Completed verified this month', dashboard?.completedVerifiedThisMonthCount ?? 0],
          ['Overdue review', dashboard?.overdueReviewCount ?? 0],
          ['Never checked', dashboard?.assetsNeverCheckedCount ?? 0],
        ].map(([label, count]) => (
          <div key={label} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</div>
            <div className={`mt-1 text-lg font-semibold ${summaryTone(Number(count))}`}>{count}</div>
          </div>
        ))}
      </div>

      <section className="space-y-4 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <p className="text-sm font-medium text-white">Campaign coverage</p>
            <p className="mt-1 text-xs text-[var(--color-text-muted)]">
              Tracked campaigns, open-case concentration, and residual risk hotspots.
            </p>
          </div>
          <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${coverageTone(coveragePercent, totalOpenCases)}`}>
            {trackedCampaignCount === 0 ? 'No campaigns' : `${coveragePercent}% covered`}
          </span>
        </div>

        <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-4">
          {[
            ['Tracked campaigns', trackedCampaignCount],
            ['Campaigns with cases', campaignsWithCases.length],
            ['Open cases', totalOpenCases],
            ['Verified open', totalVerifiedOpenCases],
          ].map(([label, count]) => (
            <div key={label} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
              <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</div>
              <div className={`mt-1 text-lg font-semibold ${summaryTone(Number(count))}`}>{count}</div>
            </div>
          ))}
        </div>

        <div className="grid gap-3 xl:grid-cols-[1.3fr_0.7fr]">
          <div className="rounded-lg border border-slate-800 bg-slate-950/70 p-4">
            <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Coverage summary</div>
            <p className="mt-2 text-sm leading-6 text-slate-300">{coverageSummary}</p>
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">
              Total asset cases: {totalAssetCases}
              {latestCampaignUpdate ? ` · Latest update ${campaignDisplayLabel(latestCampaignUpdate)} on ${formatDateTime(latestCampaignUpdate.updatedAt)}` : ''}
            </p>
          </div>

          <div className="rounded-lg border border-slate-800 bg-slate-950/70 p-4">
            <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Coverage watchlist</div>
            {topRiskCampaigns.length === 0 ? (
              <p className="mt-2 text-sm text-slate-400">No recall campaigns are stored yet.</p>
            ) : (
              <ul className="mt-2 space-y-2">
                {topRiskCampaigns.map((campaign) => (
                  <li key={campaign.campaignId} className="rounded-lg border border-slate-800 bg-slate-950/80 p-3">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <div className="text-sm font-medium text-white">{campaignDisplayLabel(campaign)}</div>
                        <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {campaign.component} · {campaign.manufacturer}
                        </div>
                      </div>
                      <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${statusTone(campaign.campaignStatus)}`}>
                        {campaign.campaignStatus}
                      </span>
                    </div>
                    <div className="mt-2 text-xs text-slate-400">
                      {campaign.assetCaseCount} asset case{campaign.assetCaseCount === 1 ? '' : 's'} · {campaign.openCaseCount} open · {campaign.verifiedOpenCaseCount} verified open
                    </div>
                    <div className="mt-1 text-[11px] text-[var(--color-text-muted)]">
                      Updated {formatDateTime(campaign.updatedAt)}
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      </section>

      <div className="grid gap-4 xl:grid-cols-2">
        <section className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
            <Search className="h-4 w-4" />
            Search by vehicle
          </div>
          <div className="grid gap-3 md:grid-cols-3">
            <label className="space-y-1 text-xs text-slate-300">
              <span className="text-[var(--color-text-muted)]">Model year</span>
              <input
                className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-white outline-none ring-0 placeholder:text-[var(--color-text-muted)]"
                value={vehicleYear}
                onChange={(event) => setVehicleYear(event.target.value)}
                placeholder="2024"
              />
            </label>
            <label className="space-y-1 text-xs text-slate-300">
              <span className="text-[var(--color-text-muted)]">Make</span>
              <input
                className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-white outline-none ring-0 placeholder:text-[var(--color-text-muted)]"
                value={vehicleMake}
                onChange={(event) => setVehicleMake(event.target.value)}
                placeholder="Freightliner"
              />
            </label>
            <label className="space-y-1 text-xs text-slate-300">
              <span className="text-[var(--color-text-muted)]">Model</span>
              <input
                className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-white outline-none ring-0 placeholder:text-[var(--color-text-muted)]"
                value={vehicleModel}
                onChange={(event) => setVehicleModel(event.target.value)}
                placeholder="Cascadia"
              />
            </label>
          </div>
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg bg-sky-600 px-3 py-2 text-xs font-semibold text-white hover:bg-sky-500 disabled:opacity-50"
            disabled={vehicleSearchMutation.isPending}
            onClick={() => vehicleSearchMutation.mutate()}
          >
            {vehicleSearchMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Radar className="h-4 w-4" />}
            Search recalls
          </button>
        </section>

        <section className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
            <Search className="h-4 w-4" />
            Search by campaign number
          </div>
          <label className="space-y-1 text-xs text-slate-300">
            <span className="text-[var(--color-text-muted)]">Campaign number</span>
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-white outline-none ring-0 placeholder:text-[var(--color-text-muted)]"
              value={campaignNumber}
              onChange={(event) => setCampaignNumber(event.target.value)}
              placeholder="23V123000"
            />
          </label>
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg bg-sky-600 px-3 py-2 text-xs font-semibold text-white hover:bg-sky-500 disabled:opacity-50"
            disabled={campaignSearchMutation.isPending}
            onClick={() => campaignSearchMutation.mutate()}
          >
            {campaignSearchMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Radar className="h-4 w-4" />}
            Search campaigns
          </button>
        </section>
      </div>

      {searchResults.length > 0 ? (
        <section className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
            <Radar className="h-4 w-4" />
            Search results
          </div>
          <div className="space-y-2">
            {searchResults.slice(0, 5).map((campaign) => (
              <article key={campaign.campaignId} className="rounded-lg border border-slate-800 bg-slate-950/70 p-3">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <div className="text-sm font-medium text-white">
                      {campaignDisplayLabel(campaign)}
                    </div>
                    <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                      {campaign.component} · {campaign.manufacturer}
                    </div>
                  </div>
                  <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${statusTone(campaign.campaignStatus)}`}>
                    {campaign.campaignStatus}
                  </span>
                </div>
                <p className="mt-2 text-xs leading-5 text-slate-400">{campaign.summary}</p>
                <div className="mt-2 flex flex-wrap gap-2 text-[11px] text-[var(--color-text-muted)]">
                  <span>{campaign.assetCaseCount} asset case(s)</span>
                  <span>{campaign.openCaseCount} open</span>
                  <span>{campaign.verifiedOpenCaseCount} verified open</span>
                </div>
              </article>
            ))}
          </div>
        </section>
      ) : null}

      <section className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
        <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
          <Radar className="h-4 w-4" />
          Active campaigns
        </div>
        {campaignsQuery.isLoading ? (
          <p className="text-sm text-slate-400">Loading recall campaigns…</p>
        ) : campaigns.length === 0 ? (
          <p className="text-sm text-slate-400">No recall campaigns are stored yet.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full border-separate border-spacing-y-2 text-left text-sm">
              <thead className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-3 py-2">Campaign</th>
                  <th className="px-3 py-2">Component</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2">Cases</th>
                  <th className="px-3 py-2">Updated</th>
                </tr>
              </thead>
              <tbody>
                {campaigns.slice(0, 12).map((campaign) => (
                  <tr key={campaign.campaignId} className="rounded-lg border border-slate-800 bg-slate-950/70 text-slate-300">
                    <td className="px-3 py-3">
                      <div className="font-medium text-white">
                        {campaignDisplayLabel(campaign)}
                      </div>
                      <div className="mt-1 text-xs text-[var(--color-text-muted)]">{campaign.sourceProvider} · {campaign.sourceType}</div>
                    </td>
                    <td className="px-3 py-3">
                      <div className="text-white">{campaign.component}</div>
                      <div className="mt-1 text-xs text-[var(--color-text-muted)]">{campaign.manufacturer}</div>
                    </td>
                    <td className="px-3 py-3">
                      <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${statusTone(campaign.campaignStatus)}`}>
                        {campaign.campaignStatus}
                      </span>
                    </td>
                    <td className="px-3 py-3 text-xs text-slate-400">
                      {campaign.assetCaseCount} total, {campaign.openCaseCount} open
                    </td>
                    <td className="px-3 py-3 text-xs text-slate-400">{formatDateTime(campaign.updatedAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <div className="grid gap-4 xl:grid-cols-2">
        <section className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
            <Radar className="h-4 w-4" />
            Attention items
          </div>
          {dashboard?.attentionItems.length ? (
            <div className="space-y-2">
              {dashboard.attentionItems.slice(0, 5).map((item) => (
                <article key={item.caseId} className="rounded-lg border border-slate-800 bg-slate-950/70 p-3">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <div className="text-sm font-medium text-white">
                        <Link to={`/assets/${item.assetId}`} className="hover:text-sky-200">
                          {item.assetTag}
                        </Link>
                      </div>
                      <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {item.assetName} · {item.campaignNumber} · {item.component}
                      </div>
                    </div>
                    <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${statusTone(item.status)}`}>
                      {item.status}
                    </span>
                  </div>
                  <div className="mt-2 text-xs text-slate-400">
                    {item.readinessImpact} · review {formatDateTime(item.nextReviewAt)}
                  </div>
                </article>
              ))}
            </div>
          ) : (
            <p className="text-sm text-slate-400">No attention items are queued right now.</p>
          )}
        </section>

        <section className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
            <Radar className="h-4 w-4" />
            Provider health
          </div>
          <div className="space-y-3">
            {providers.map((provider) => {
              const health = providerHealth.find((item) => item.providerKey === provider.providerKey)
              return (
                <article key={provider.providerKey} className="rounded-lg border border-slate-800 bg-slate-950/70 p-3">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <div className="text-sm font-medium text-white">{provider.displayName}</div>
                      <div className="mt-1 text-xs text-[var(--color-text-muted)]">{provider.description}</div>
                    </div>
                    <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${statusTone(provider.status)}`}>
                      {provider.status}
                    </span>
                  </div>
                  <div className="mt-2 text-xs text-slate-400">
                    {provider.sourceOfTruth}
                    {health ? ` · ${health.message}` : ''}
                  </div>
                  {provider.lastError ? (
                    <p className="mt-2 rounded-md border border-rose-500/20 bg-rose-500/10 p-2 text-xs text-rose-100">
                      {provider.lastError}
                    </p>
                  ) : null}
                </article>
              )
            })}
          </div>
        </section>
      </div>
    </div>
  )
}
