import { ApiErrorCallout, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useMemo } from 'react'
import type {
  ReadinessRollupMemberResponse,
  ReadinessRollupMembersResponse,
  ReadinessRollupSelection,
  ReadinessRollupSummaryResponse,
} from '../api/types'

interface ReadinessRollupSupervisorPanelProps {
  teamRollups: ReadinessRollupSummaryResponse[]
  siteRollups: ReadinessRollupSummaryResponse[]
  siteFilterOrgUnitId: string | null
  onSiteFilterChange: (siteOrgUnitId: string | null) => void
  memberReadinessFilter: 'all' | 'not_ready' | 'missing_certifications'
  onMemberReadinessFilterChange: (filter: 'all' | 'not_ready' | 'missing_certifications') => void
  selectedRollup: ReadinessRollupSelection | null
  onSelectRollup: (rollup: ReadinessRollupSelection | null) => void
  rollupMembers: ReadinessRollupMembersResponse | null
  rollupMembersLoading: boolean
  rollupMembersReadErrorMessage: string | null
  onRetryRollupMembersRead?: () => void
  onSelectPerson?: (personId: string) => void
  isLoading: boolean
  readErrorMessage: string | null
  onRetryRead?: () => void
}

function readinessBarClass(readyPercent: number): string {
  if (readyPercent >= 90) {
    return 'bg-emerald-500'
  }

  if (readyPercent >= 70) {
    return 'bg-amber-400'
  }

  return 'bg-rose-500'
}

function readinessStatusLabel(status: ReadinessRollupMemberResponse['readinessStatus']): string {
  return status === 'ready' ? 'Ready' : 'Not ready'
}

function readinessStatusClass(status: ReadinessRollupMemberResponse['readinessStatus']): string {
  return status === 'ready' ? 'text-emerald-300' : 'text-rose-300'
}

function confidenceLabel(level: ReadinessRollupSummaryResponse['confidenceLevel']): string {
  switch (level) {
    case 'high':
      return 'High'
    case 'medium':
      return 'Medium'
    case 'low':
      return 'Low'
    default:
      return level
  }
}

function confidenceClass(level: ReadinessRollupSummaryResponse['confidenceLevel']): string {
  switch (level) {
    case 'high':
      return 'text-emerald-300'
    case 'medium':
      return 'text-amber-200'
    case 'low':
      return 'text-rose-300'
    default:
      return 'text-slate-300'
  }
}

function RollupTable({
  title,
  rollups,
  selectedRollup,
  onSelectRollup,
}: {
  title: string
  rollups: ReadinessRollupSummaryResponse[]
  selectedRollup: ReadinessRollupSelection | null
  onSelectRollup: (rollup: ReadinessRollupSelection | null) => void
}) {
  if (rollups.length === 0) {
    return (
      <div>
        <h3 className="text-sm font-medium text-slate-300">{title}</h3>
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">
          No rollups computed yet. The scheduled worker refreshes summaries periodically.
        </p>
      </div>
    )
  }

  return (
    <div>
      <h3 className="text-sm font-medium text-slate-300">{title}</h3>
      <div className="mt-3 overflow-x-auto">
        <table className="min-w-full text-left text-sm" data-testid={`readiness-rollup-${title.toLowerCase()}-table`}>
          <thead className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
            <tr>
              <th className="pb-2 pr-4 font-medium">Unit</th>
              <th className="pb-2 pr-4 font-medium">Members</th>
              <th className="pb-2 pr-4 font-medium">Ready</th>
              <th className="pb-2 pr-4 font-medium">Not ready</th>
              <th className="pb-2 pr-4 font-medium">Overrides</th>
              <th className="pb-2 pr-4 font-medium">Confidence</th>
              <th className="pb-2 font-medium">Ready %</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800">
            {rollups.map((rollup) => {
              const isSelected =
                selectedRollup?.scopeType === rollup.scopeType
                && selectedRollup.orgUnitId === rollup.orgUnitId

              return (
                <tr
                  key={`${rollup.scopeType}-${rollup.orgUnitId}`}
                  className={isSelected ? 'bg-slate-800/60' : undefined}
                >
                  <td className="py-3 pr-4">
                    <button
                      type="button"
                      data-testid={`readiness-rollup-select-${rollup.scopeType}-${rollup.orgUnitId}`}
                      onClick={() =>
                        onSelectRollup(
                          isSelected
                            ? null
                            : {
                                scopeType: rollup.scopeType,
                                orgUnitId: rollup.orgUnitId,
                                orgUnitName: rollup.orgUnitName,
                              },
                        )
                      }
                      className="text-left text-white underline-offset-2 hover:underline"
                    >
                      {rollup.orgUnitName}
                    </button>
                  </td>
                  <td className="py-3 pr-4 text-slate-300">{rollup.totalMembers}</td>
                  <td className="py-3 pr-4 text-emerald-300">{rollup.readyCount}</td>
                  <td className="py-3 pr-4 text-rose-300">{rollup.notReadyCount}</td>
                  <td className="py-3 pr-4 text-amber-200">{rollup.overrideCount}</td>
                  <td className={`py-3 pr-4 ${confidenceClass(rollup.confidenceLevel)}`}>
                    <div className="flex flex-col">
                      <span>{confidenceLabel(rollup.confidenceLevel)} confidence</span>
                      <span className="text-xs text-[var(--color-text-muted)]">Score {rollup.confidenceScore}</span>
                    </div>
                  </td>
                  <td className="py-3">
                    <div className="flex min-w-[8rem] items-center gap-2">
                      <div className="h-2 flex-1 rounded-full bg-slate-800">
                        <div
                          className={`h-2 rounded-full ${readinessBarClass(rollup.readyPercent)}`}
                          style={{ width: `${Math.min(rollup.readyPercent, 100)}%` }}
                        />
                      </div>
                      <span className="w-12 text-right text-slate-300">{rollup.readyPercent.toFixed(1)}%</span>
                    </div>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </div>
  )
}

function RollupMembersDrillDown({
  selectedRollup,
  rollupMembers,
  isLoading,
  readErrorMessage,
  onRetryRead,
  memberReadinessFilter,
  onMemberReadinessFilterChange,
  onSelectPerson,
}: {
  selectedRollup: ReadinessRollupSelection
  rollupMembers: ReadinessRollupMembersResponse | null
  isLoading: boolean
  readErrorMessage: string | null
  memberReadinessFilter: 'all' | 'not_ready' | 'missing_certifications'
  onMemberReadinessFilterChange: (filter: 'all' | 'not_ready' | 'missing_certifications') => void
  onSelectPerson?: (personId: string) => void
  onRetryRead?: () => void
}) {
  const members = rollupMembers?.members ?? []

  return (
    <section
      className="mt-8 rounded-lg border border-slate-700 bg-slate-950/40 p-5"
      data-testid="readiness-rollup-drilldown"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="text-sm font-medium text-slate-200">
            {selectedRollup.orgUnitName} — member readiness
          </h3>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            Drill-down for {selectedRollup.scopeType} scope. Select a person to load readiness detail below.
          </p>
        </div>
        <label htmlFor="readiness-rollup-member-filter" className="flex items-center gap-2 text-sm text-slate-400">
          <span>Show</span>
          <select
            id="readiness-rollup-member-filter"
            data-testid="readiness-rollup-member-filter"
            value={memberReadinessFilter}
            onChange={(event) =>
              onMemberReadinessFilterChange(
                event.target.value === 'not_ready'
                  ? 'not_ready'
                  : event.target.value === 'missing_certifications'
                    ? 'missing_certifications'
                    : 'all',
              )
            }
            className="rounded-md border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
          >
            <option value="all">All members</option>
            <option value="not_ready">Not ready only</option>
            <option value="missing_certifications">Missing certifications only</option>
          </select>
        </label>
      </div>

      {readErrorMessage ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Member drill-down failed"
            message={readErrorMessage}
            onRetry={onRetryRead}
            retryLabel={onRetryRead ? 'Retry members' : undefined}
          />
        </div>
      ) : null}
      {isLoading ? <p className="mt-4 text-sm text-slate-400">Loading members…</p> : null}

      {!isLoading && !readErrorMessage && members.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">No members match this filter.</p>
      ) : null}

      {!isLoading && !readErrorMessage && members.length > 0 ? (
        <div className="mt-4 overflow-x-auto">
          {rollupMembers ? (
            <div className="mb-4 grid gap-3 sm:grid-cols-4">
              <div className="rounded-lg border border-slate-600 bg-slate-950/40 px-4 py-3">
                <p className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">Members</p>
                <p className="mt-1 text-sm text-slate-100">{rollupMembers.rollup.totalMembers}</p>
              </div>
              <div className="rounded-lg border border-slate-600 bg-slate-950/40 px-4 py-3">
                <p className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">Ready %</p>
                <p className="mt-1 text-sm text-slate-100">{rollupMembers.rollup.readyPercent.toFixed(1)}%</p>
              </div>
              <div className="rounded-lg border border-slate-600 bg-slate-950/40 px-4 py-3">
                <p className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">Confidence</p>
                <p className={`mt-1 text-sm ${confidenceClass(rollupMembers.rollup.confidenceLevel)}`}>
                  {confidenceLabel(rollupMembers.rollup.confidenceLevel)} confidence
                </p>
              </div>
              <div className="rounded-lg border border-slate-600 bg-slate-950/40 px-4 py-3">
                <p className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">Confidence score</p>
                <p className="mt-1 text-sm text-slate-100">{rollupMembers.rollup.confidenceScore}</p>
              </div>
            </div>
          ) : null}
          <table className="min-w-full text-left text-sm" data-testid="readiness-rollup-members-table">
            <thead className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
              <tr>
                <th className="pb-2 pr-4 font-medium">Person</th>
                <th className="pb-2 pr-4 font-medium">Status</th>
                <th className="pb-2 pr-4 font-medium">Basis</th>
                <th className="pb-2 pr-4 font-medium">Blockers</th>
                <th className="pb-2 font-medium">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800">
              {members.map((member) => (
                <tr key={member.personId}>
                  <td className="py-3 pr-4 text-white">
                    {member.displayName}
                    {member.hasActiveOverride ? (
                      <span className="ml-2 text-xs text-amber-300">Override</span>
                    ) : null}
                  </td>
                  <td className={`py-3 pr-4 ${readinessStatusClass(member.readinessStatus)}`}>
                    {readinessStatusLabel(member.readinessStatus)}
                  </td>
                  <td className="py-3 pr-4 text-slate-400">{member.readinessBasis}</td>
                  <td className="py-3 pr-4 text-slate-300">
                    {member.blockerCount > 0
                      ? member.primaryBlockerMessage ?? `${member.blockerCount} blocker(s)`
                      : '—'}
                  </td>
                  <td className="py-3">
                    {onSelectPerson ? (
                      <button
                        type="button"
                        data-testid={`readiness-rollup-member-select-${member.personId}`}
                        onClick={() => onSelectPerson(member.personId)}
                        className="text-sm text-sky-300 hover:text-sky-200"
                      >
                        View readiness
                      </button>
                    ) : null}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}
    </section>
  )
}

export function canViewReadinessRollups(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) {
    return true
  }

  return ['tenant_admin', 'staffarr_admin', 'hr_admin', 'supervisor'].includes(tenantRoleKey)
}

export function ReadinessRollupSupervisorPanel({
  teamRollups,
  siteRollups,
  siteFilterOrgUnitId,
  onSiteFilterChange,
  memberReadinessFilter,
  onMemberReadinessFilterChange,
  selectedRollup,
  onSelectRollup,
  rollupMembers,
  rollupMembersLoading,
  rollupMembersReadErrorMessage,
  onRetryRollupMembersRead,
  onSelectPerson,
  isLoading,
  readErrorMessage,
  onRetryRead,
}: ReadinessRollupSupervisorPanelProps) {
  const siteFilterOptions = useMemo<PickerOption[]>(
    () => siteRollups.map((site) => ({ value: site.orgUnitId, label: site.orgUnitName })),
    [siteRollups],
  )
  const selectedSiteOption = useMemo(
    () => siteFilterOptions.find((option) => option.value === siteFilterOrgUnitId),
    [siteFilterOptions, siteFilterOrgUnitId],
  )

  return (
    <section className="mt-8 rounded-xl border border-slate-700 bg-slate-900/60 p-6" data-testid="readiness-rollup-supervisor-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Team and site readiness rollups</h2>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            Materialized summaries refreshed by the readiness rollup worker. Select a unit to drill into members.
          </p>
        </div>
        <label className="flex items-center gap-2 text-sm text-slate-400">
          <span>Filter teams by site</span>
          <StaticSearchPicker
            value={siteFilterOrgUnitId ?? ''}
            onChange={(value) => onSiteFilterChange(value || null)}
            options={siteFilterOptions}
            selectedOption={selectedSiteOption}
            placeholder="All sites"
            testId="readiness-rollup-site-filter"
          />
        </label>
      </div>

      {readErrorMessage ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Readiness rollup load failed"
            message={readErrorMessage}
            onRetry={onRetryRead}
            retryLabel={onRetryRead ? 'Retry rollups' : undefined}
          />
        </div>
      ) : null}
      {isLoading ? <p className="mt-4 text-sm text-slate-400">Loading readiness rollups…</p> : null}

      {!isLoading && !readErrorMessage ? (
        <div className="mt-6 grid gap-8 lg:grid-cols-2">
          <RollupTable
            title="Teams"
            rollups={teamRollups}
            selectedRollup={selectedRollup}
            onSelectRollup={onSelectRollup}
          />
          <RollupTable
            title="Sites"
            rollups={siteRollups}
            selectedRollup={selectedRollup}
            onSelectRollup={onSelectRollup}
          />
        </div>
      ) : null}

      {selectedRollup ? (
        <RollupMembersDrillDown
          selectedRollup={selectedRollup}
          rollupMembers={rollupMembers}
          isLoading={rollupMembersLoading}
          readErrorMessage={rollupMembersReadErrorMessage}
          memberReadinessFilter={memberReadinessFilter}
          onMemberReadinessFilterChange={onMemberReadinessFilterChange}
          onSelectPerson={onSelectPerson}
          onRetryRead={onRetryRollupMembersRead}
        />
      ) : null}
    </section>
  )
}
