import { useEffect } from 'react'
import { useSearchParams } from 'react-router-dom'
import { PageHeader } from '@stl/shared-ui'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'
import { RegistryDetailProfile } from '../../workspace/sections/RegistryDetailProfile'

export function RulePackDetailPage() {
  const state = useComplianceCoreWorkspaceState()
  const [searchParams] = useSearchParams()

  useEffect(() => {
    const rulePackId = searchParams.get('rulePackId')
    if (rulePackId && rulePackId !== state.selectedRulePackId) {
      state.setSelectedRulePackId(rulePackId)
    }
  }, [searchParams, state.selectedRulePackId, state.setSelectedRulePackId])

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  const selectedRulePack =
    state.rulePacksQuery.data?.find((pack) => pack.rulePackId === state.selectedRulePackId) ??
    state.rulePacksQuery.data?.[0] ??
    null

  return (
    <div className="space-y-6">
      <PageHeader
        title="Rule pack detail"
        subtitle={selectedRulePack ? `${selectedRulePack.packKey} · version ${selectedRulePack.versionNumber}` : 'Select a rule pack to inspect its content, citations, requirements, and lifecycle.'}
      />
      <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
        <label className="block text-sm text-slate-300">
          Choose rule pack
          <select
            value={state.selectedRulePackId || selectedRulePack?.rulePackId || ''}
            onChange={(event) => state.setSelectedRulePackId(event.target.value)}
            className="mt-2 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
          >
            {state.rulePacksQuery.data?.map((pack) => (
              <option key={pack.rulePackId} value={pack.rulePackId}>
                {pack.packKey} - v{pack.versionNumber} ({pack.status})
              </option>
            ))}
          </select>
        </label>
      </div>
      <RegistryDetailProfile state={state} />
    </div>
  )
}
