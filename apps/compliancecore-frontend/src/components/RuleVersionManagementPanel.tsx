import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { listRuleVersions, publishRuleVersion, rollbackRuleVersion } from '../api/client'
import type { RuleVersionResponse } from '../api/types'

interface RuleVersionManagementPanelProps {
  accessToken: string
  canRead: boolean
  canManage: boolean
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'published':
      return 'bg-emerald-900/60 text-emerald-200'
    case 'review':
      return 'bg-amber-900/60 text-amber-200'
    case 'archived':
      return 'bg-slate-800 text-slate-400'
    default:
      return 'bg-slate-800 text-slate-300'
  }
}

function groupByPackKey(items: RuleVersionResponse[]): Map<string, RuleVersionResponse[]> {
  const groups = new Map<string, RuleVersionResponse[]>()
  for (const item of items) {
    const existing = groups.get(item.packKey) ?? []
    existing.push(item)
    groups.set(item.packKey, existing)
  }
  return groups
}

export function RuleVersionManagementPanel({
  accessToken,
  canRead,
  canManage,
}: RuleVersionManagementPanelProps) {
  const queryClient = useQueryClient()

  const versionsQuery = useQuery({
    queryKey: ['compliancecore-rule-versions', accessToken],
    queryFn: () => listRuleVersions(accessToken),
    enabled: canRead,
  })

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-versions', accessToken] })
    queryClient.invalidateQueries({ queryKey: ['compliancecore-rule-packs'] })
  }

  const publishMutation = useMutation({
    mutationFn: (rulePackId: string) => publishRuleVersion(accessToken, rulePackId),
    onSuccess: invalidate,
  })

  const rollbackMutation = useMutation({
    mutationFn: (rulePackId: string) => rollbackRuleVersion(accessToken, rulePackId),
    onSuccess: invalidate,
  })

  if (!canRead) {
    return null
  }

  const grouped = groupByPackKey(versionsQuery.data?.items ?? [])

  return (
    <section
      className="mt-8 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="rule-version-management-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Rule version publication</h2>
      <p className="mt-1 text-sm text-slate-400">
        Dedicated /api/rule-versions operator flow — publish in-review versions and roll back to prior
        published releases per docs/18.
      </p>

      {versionsQuery.isLoading ? (
        <p className="mt-4 text-sm text-slate-500">Loading rule versions…</p>
      ) : grouped.size === 0 ? (
        <p className="mt-4 text-sm text-slate-500">No rule pack versions registered yet.</p>
      ) : (
        <ul className="mt-4 space-y-4" data-testid="rule-version-list">
          {[...grouped.entries()].map(([packKey, versions]) => (
            <li key={packKey} className="rounded-lg border border-slate-700 bg-slate-950/50 p-4">
              <p className="font-mono text-sm text-amber-300">{packKey}</p>
              <ul className="mt-2 space-y-2">
                {versions.map((version) => (
                  <li
                    key={version.rulePackId}
                    className="flex flex-wrap items-center justify-between gap-2 rounded border border-slate-800 bg-slate-900/60 px-3 py-2"
                  >
                    <div>
                      <p className="text-sm text-slate-100">
                        v{version.versionNumber} · {version.programLabel}
                      </p>
                      <p className="text-xs text-slate-500">{version.programKey}</p>
                    </div>
                    <div className="flex flex-wrap items-center gap-2">
                      <span
                        className={`rounded px-2 py-0.5 text-xs uppercase ${statusBadgeClass(version.status)}`}
                      >
                        {version.status}
                      </span>
                      {canManage && version.status === 'review' ? (
                        <button
                          type="button"
                          className="rounded bg-emerald-800 px-2 py-1 text-xs text-emerald-100 hover:bg-emerald-700 disabled:opacity-50"
                          disabled={publishMutation.isPending}
                          onClick={() => publishMutation.mutate(version.rulePackId)}
                        >
                          Publish
                        </button>
                      ) : null}
                      {canManage && version.status === 'published' && version.versionNumber > 1 ? (
                        <button
                          type="button"
                          className="rounded bg-amber-900 px-2 py-1 text-xs text-amber-100 hover:bg-amber-800 disabled:opacity-50"
                          disabled={rollbackMutation.isPending}
                          onClick={() => rollbackMutation.mutate(version.rulePackId)}
                        >
                          Roll back
                        </button>
                      ) : null}
                    </div>
                  </li>
                ))}
              </ul>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
