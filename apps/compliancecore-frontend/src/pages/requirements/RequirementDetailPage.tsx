import { useSearchParams, Link } from 'react-router-dom'
import { PageHeader, DetailEmptyState } from '@stl/shared-ui'
import { SituationEvaluatorPanel } from '../../components/SituationEvaluatorPanel'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function RequirementDetailPage() {
  const state = useComplianceCoreWorkspaceState()
  const [searchParams, setSearchParams] = useSearchParams()

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  const requirements = state.factRequirementsQuery.data ?? []
  const selectedRequirementKey = searchParams.get('requirementKey') ?? searchParams.get('requirementId') ?? ''
  const requirement =
    requirements.find(
      (item) => item.factRequirementId === selectedRequirementKey || item.requirementKey === selectedRequirementKey,
    ) ?? requirements[0] ?? null

  if (!requirement) {
    return (
      <div className="space-y-6">
        <PageHeader title="Requirement detail" subtitle="No requirement is available yet." />
        <DetailEmptyState text="Seed the registry catalog first to populate requirement detail views." />
        <Link to="/mappings" className="inline-flex rounded-md bg-sky-500 px-4 py-2 text-sm font-semibold text-slate-950">
          Open mappings
        </Link>
      </div>
    )
  }

  const factDefinition = state.factDefinitionsQuery.data?.find((fact) => fact.factDefinitionId === requirement.factDefinitionId)
  const rulePack = state.rulePacksQuery.data?.find((pack) => pack.rulePackId === requirement.rulePackId) ?? null
  const citation = state.citationsQuery.data?.find((item) => item.citationId === requirement.citationId) ?? null
  const relatedRequirements = requirements.filter(
    (item) =>
      item.factRequirementId !== requirement.factRequirementId &&
      (item.factKey === requirement.factKey || item.rulePackId === requirement.rulePackId),
  )

  return (
    <div className="space-y-8">
      <PageHeader
        title="Requirement detail"
        subtitle={`${requirement.requirementKey} · ${requirement.label}`}
      />

      <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
        <label className="block text-sm text-slate-300">
          Choose requirement
          <select
            value={requirement.factRequirementId}
            onChange={(event) =>
              setSearchParams({ requirementId: event.target.value })
            }
            className="mt-2 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
          >
            {requirements.map((item) => (
              <option key={item.factRequirementId} value={item.factRequirementId}>
                {item.requirementKey} - {item.label}
              </option>
            ))}
          </select>
        </label>
      </div>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1.6fr)_minmax(320px,0.9fr)]">
        <section className="space-y-4 rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
          <div className="grid gap-3 md:grid-cols-2">
            <DetailField label="Requirement key" value={requirement.requirementKey} />
            <DetailField label="Fact key" value={requirement.factKey} />
            <DetailField label="Fact label" value={requirement.factLabel} />
            <DetailField label="Required" value={requirement.isRequired ? 'Yes' : 'No'} />
            <DetailField label="Rule pack" value={rulePack?.label ?? requirement.rulePackKey ?? 'Unlinked'} />
            <DetailField label="Citation" value={citation?.label ?? requirement.citationKey ?? 'Unlinked'} />
          </div>

          <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Plain-language summary</h2>
            <p className="mt-2 text-sm text-slate-200">
              {requirement.description || 'No plain-language summary has been captured yet.'}
            </p>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
              <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Citations</h3>
              <p className="mt-2 text-sm text-slate-200">{citation?.sourceReference ?? requirement.citationKey ?? 'No citation linked.'}</p>
            </div>
            <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
              <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Applicability / logic</h3>
              <p className="mt-2 text-sm text-slate-200">
                {rulePack
                  ? `This requirement is evaluated through ${rulePack.packKey} within ${rulePack.regulatoryProgramLabel}.`
                  : 'No rule pack is linked yet, so applicability and compliance logic are not yet resolved.'}
              </p>
            </div>
          </div>

          <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
            <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Related requirements</h3>
            {relatedRequirements.length === 0 ? (
              <p className="mt-2 text-sm text-slate-400">No related requirements were found for this fact or rule pack.</p>
            ) : (
              <ul className="mt-3 space-y-2">
                {relatedRequirements.slice(0, 6).map((item) => (
                  <li key={item.factRequirementId} className="rounded-lg border border-slate-800 bg-slate-950/50 p-3 text-sm text-slate-200">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <div className="font-medium">{item.label}</div>
                        <div className="mt-1 text-xs text-slate-500">{item.requirementKey}</div>
                      </div>
                      <span className="rounded-full border border-slate-700 px-2 py-0.5 text-xs text-slate-400">
                        {item.isRequired ? 'required' : 'optional'}
                      </span>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </section>

        <aside className="space-y-4 rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Requirement snapshot</h2>
          <DetailField label="Fact definition" value={factDefinition?.label ?? 'Not linked'} />
          <DetailField label="Value type" value={factDefinition?.valueType ?? 'Not linked'} />
          <DetailField label="Status" value={requirement.isActive ? 'Active' : 'Inactive'} />
          <DetailField label="Created" value={new Date(requirement.createdAt).toLocaleString()} />
          <DetailField label="Updated" value={new Date(requirement.updatedAt).toLocaleString()} />
          <DetailField label="Linked citation" value={citation?.citationKey ?? requirement.citationKey ?? 'Not linked'} />
          <DetailField label="Linked rule pack" value={rulePack?.packKey ?? requirement.rulePackKey ?? 'Not linked'} />
        </aside>
      </div>

      <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Test evaluation panel</h2>
        <p className="mt-2 text-sm text-slate-400">
          Use the situation evaluator to test whether the active fact requirements and exception logic behave as expected.
        </p>
        <div className="mt-4">
          <SituationEvaluatorPanel
            accessToken={state.accessToken}
            canEvaluate={state.canManage || state.canEvaluateRisk}
            factRequirements={requirements}
          />
        </div>
      </section>
    </div>
  )
}

function DetailField({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
      <div className="text-xs uppercase tracking-wide text-slate-500">{label}</div>
      <div className="mt-2 text-sm text-slate-100">{value}</div>
    </div>
  )
}
