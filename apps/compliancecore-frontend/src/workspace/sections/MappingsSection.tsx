import { Link, useLocation } from 'react-router-dom'
import type { ReactNode } from 'react'
import { CitationFactCatalogPanel } from '../../components/CitationFactCatalogPanel'
import { RegulatoryMappingsPanel } from '../../components/RegulatoryMappingsPanel'
import type { FactDefinitionResponse, FactRequirementResponse, FactSourceResponse } from '../../api/types'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

type MappingView = 'overview' | 'coverage' | 'facts' | 'evidence' | 'vocabulary' | 'subjects' | 'outputs'

const mappingViews: Array<{ key: MappingView; label: string; to: string }> = [
  { key: 'overview', label: 'Overview', to: '/mappings' },
  { key: 'coverage', label: 'Coverage Matrix', to: '/mappings/coverage' },
  { key: 'facts', label: 'Fact Mappings', to: '/mappings/facts' },
  { key: 'evidence', label: 'Evidence Mappings', to: '/mappings/evidence' },
  { key: 'vocabulary', label: 'Vocabulary Mappings', to: '/mappings/vocabulary' },
  { key: 'subjects', label: 'Subject Mappings', to: '/mappings/subjects' },
  { key: 'outputs', label: 'Output Signals', to: '/mappings/outputs' },
]

function mappingViewFromPath(pathname: string): MappingView {
  const segment = pathname.split('/').filter(Boolean)[1]
  if (segment && mappingViews.some((view) => view.key === segment)) return segment as MappingView
  return 'overview'
}

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not mapped'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function productLabel(value: string | null | undefined): string {
  if (!value) return 'No source product'
  const labels: Record<string, string> = {
    staffarr: 'StaffArr',
    trainarr: 'TrainArr',
    maintainarr: 'MaintainArr',
    routarr: 'RoutArr',
    loadarr: 'LoadArr',
    supplyarr: 'SupplyArr',
    recordarr: 'RecordArr',
    reportarr: 'ReportArr',
    compliancecore: 'Compliance Core',
  }
  return labels[value.toLowerCase()] ?? humanize(value)
}

function Panel({ title, description, children }: { title: string; description?: string; children: ReactNode }) {
  return (
    <section className="rounded-lg border border-slate-800 bg-slate-950/70 p-5">
      <h2 className="text-lg font-semibold text-white">{title}</h2>
      {description ? <p className="mt-2 max-w-3xl text-sm text-slate-300">{description}</p> : null}
      <div className="mt-4">{children}</div>
    </section>
  )
}

function Metric({ label, value, hint }: { label: string; value: string | number; hint: string }) {
  return (
    <div className="rounded-lg border border-slate-800 bg-slate-900/70 p-4">
      <p className="text-xs font-semibold uppercase text-slate-400">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-white">{value}</p>
      <p className="mt-1 text-xs text-slate-400">{hint}</p>
    </div>
  )
}

function sourcesForFact(factKey: string, sources: FactSourceResponse[]) {
  return sources.filter((source) => source.factKey === factKey && source.isActive)
}

function MappingTabs({ activeView }: { activeView: MappingView }) {
  return (
    <nav className="flex flex-wrap gap-2" aria-label="Mapping Center views">
      {mappingViews.map((view) => (
        <Link
          key={view.key}
          to={view.to}
          className={`rounded-md border px-3 py-2 text-sm ${
            activeView === view.key
              ? 'border-sky-500 bg-sky-950/50 text-sky-100'
              : 'border-slate-800 bg-slate-950 text-slate-300 hover:border-slate-600'
          }`}
        >
          {view.label}
        </Link>
      ))}
    </nav>
  )
}

function CoverageMatrix({
  requirements,
  sources,
}: {
  requirements: FactRequirementResponse[]
  sources: FactSourceResponse[]
}) {
  if (requirements.length === 0) {
    return <p className="text-sm text-slate-400">No active fact requirements are linked to rulepacks or citations yet.</p>
  }

  return (
    <>
    <div className="space-y-3 md:hidden">
      {requirements.map((requirement) => {
        const factSources = sourcesForFact(requirement.factKey, sources)
        const products = [...new Set(factSources.map((source) => productLabel(source.productKey)))]
        return (
          <article key={requirement.factRequirementId} className="rounded-lg border border-slate-800 bg-slate-950/70 p-4">
            <h3 className="font-semibold text-white">{requirement.label || requirement.factLabel}</h3>
            <p className="mt-1 font-mono text-xs text-sky-300">{requirement.factKey}</p>
            <dl className="mt-3 space-y-2 text-sm">
              <div className="flex justify-between gap-3">
                <dt className="text-slate-400">Fact status</dt>
                <dd className="text-slate-100">{factSources.length > 0 ? `${factSources.length} active source${factSources.length === 1 ? '' : 's'}` : 'Unmapped'}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="text-slate-400">Source products</dt>
                <dd className="text-right text-slate-100">{products.length > 0 ? products.join(', ') : 'None'}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="text-slate-400">Rulepack / citation</dt>
                <dd className="text-right text-slate-100">{requirement.rulePackKey ? humanize(requirement.rulePackKey) : requirement.citationKey ?? 'Unlinked'}</dd>
              </div>
            </dl>
            <Link className="mt-3 inline-flex text-sm text-sky-300 hover:text-sky-200" to="/fact-sources">
              {factSources.length > 0 ? 'View sources' : 'Map source'}
            </Link>
          </article>
        )
      })}
    </div>
    <div className="hidden overflow-x-auto rounded-lg border border-slate-800 md:block">
      <table className="min-w-full divide-y divide-slate-800 text-sm">
        <thead className="bg-slate-900/80 text-left text-xs uppercase text-slate-400">
          <tr>
            <th className="px-4 py-3 font-semibold">Requirement</th>
            <th className="px-4 py-3 font-semibold">Fact Status</th>
            <th className="px-4 py-3 font-semibold">Source Products</th>
            <th className="px-4 py-3 font-semibold">Rulepack / Citation</th>
            <th className="px-4 py-3 font-semibold">Action</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-800 bg-slate-950/60">
          {requirements.map((requirement) => {
            const factSources = sourcesForFact(requirement.factKey, sources)
            const products = [...new Set(factSources.map((source) => productLabel(source.productKey)))]
            return (
              <tr key={requirement.factRequirementId}>
                <td className="px-4 py-3">
                  <p className="font-medium text-slate-100">{requirement.label || requirement.factLabel}</p>
                  <p className="mt-1 font-mono text-xs text-sky-300">{requirement.factKey}</p>
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`rounded-md px-2 py-1 text-xs font-semibold ${
                      factSources.length > 0
                        ? 'bg-emerald-950 text-emerald-200'
                        : 'bg-amber-950 text-amber-200'
                    }`}
                  >
                    {factSources.length > 0 ? `${factSources.length} source${factSources.length === 1 ? '' : 's'}` : 'Unmapped'}
                  </span>
                </td>
                <td className="px-4 py-3 text-slate-300">{products.length > 0 ? products.join(', ') : 'None'}</td>
                <td className="px-4 py-3 text-slate-400">
                  {requirement.rulePackKey ? humanize(requirement.rulePackKey) : requirement.citationKey ?? 'Unlinked'}
                </td>
                <td className="px-4 py-3">
                  <Link className="text-sky-300 hover:text-sky-200" to="/fact-sources">
                    {factSources.length > 0 ? 'View sources' : 'Map source'}
                  </Link>
                </td>
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
    </>
  )
}

function FactMappings({
  facts,
  sources,
}: {
  facts: FactDefinitionResponse[]
  sources: FactSourceResponse[]
}) {
  if (facts.length === 0) return <p className="text-sm text-slate-400">No compliance facts are defined yet.</p>

  return (
    <div className="grid gap-3 lg:grid-cols-2">
      {facts.map((fact) => {
        const factSources = sourcesForFact(fact.factKey, sources)
        return (
          <article key={fact.factDefinitionId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-4">
            <div className="flex items-start justify-between gap-3">
              <div>
                <h3 className="font-semibold text-white">{fact.label}</h3>
                <p className="mt-1 font-mono text-xs text-sky-300">{fact.factKey}</p>
              </div>
              <span className="rounded-md bg-slate-800 px-2 py-1 text-xs text-slate-300">{fact.valueType}</span>
            </div>
            <p className="mt-3 text-sm text-slate-300">{fact.description || 'No plain-language meaning recorded.'}</p>
            <div className="mt-4 space-y-2">
              {factSources.length === 0 ? (
                <p className="rounded-md border border-amber-900/70 bg-amber-950/30 px-3 py-2 text-xs text-amber-100">
                  No active product source mapped.
                </p>
              ) : (
                factSources.map((source) => (
                  <div key={source.factSourceId} className="rounded-md border border-slate-800 bg-slate-950 px-3 py-2">
                    <p className="text-sm font-medium text-slate-100">{source.label}</p>
                    <p className="mt-1 text-xs text-slate-400">
                      {productLabel(source.productKey)} · {humanize(source.sourceType)} · priority {source.priority}
                    </p>
                  </div>
                ))
              )}
            </div>
          </article>
        )
      })}
    </div>
  )
}

export function MappingsSection({ state }: Props) {
  const s = state
  const location = useLocation()
  const activeView = mappingViewFromPath(location.pathname)
  const facts = s.factDefinitionsQuery.data ?? []
  const requirements = (s.factRequirementsQuery.data ?? []).filter((requirement) => requirement.isActive)
  const sources = s.factSourcesQuery.data ?? []
  const activeMappings = (s.regulatoryMappingsQuery.data ?? []).filter((mapping) => mapping.isActive)
  const sourcedRequirements = requirements.filter(
    (requirement) => sourcesForFact(requirement.factKey, sources).length > 0,
  )
  const mappedFactCount = facts.filter((fact) => sourcesForFact(fact.factKey, sources).length > 0).length
  const sourceProducts = [...new Set(sources.map((source) => productLabel(source.productKey)))]
  const activeVocabularyTerms = (s.termsQuery.data ?? []).filter((term) => term.isActive)

  const viewContent = (() => {
    switch (activeView) {
      case 'coverage':
        return (
          <Panel
            title="Coverage Matrix"
            description="Shows which rulepack requirements have normalized facts and active product sources. Evidence validity remains separate from fact mapping."
          >
            <CoverageMatrix requirements={requirements} sources={sources} />
          </Panel>
        )
      case 'facts':
        return (
          <Panel
            title="Fact Mappings"
            description="Product fields, events, and records map into normalized Compliance Core facts before rules consume them."
          >
            <FactMappings facts={facts} sources={sources} />
          </Panel>
        )
      case 'evidence':
        return (
          <Panel
            title="Evidence Mappings"
            description="Evidence mapping keeps the proof record separate from the fact it supports. RecordArr owns stored files; Compliance Core owns evidence meaning."
          >
            <div className="grid gap-3 lg:grid-cols-3">
              <Link className="rounded-lg border border-slate-800 bg-slate-900/70 p-4 hover:border-sky-700" to="/evidence-requirements">
                <h3 className="font-semibold text-white">Evidence requirements</h3>
                <p className="mt-2 text-sm text-slate-300">Review what evidence a rulepack or citation requires.</p>
              </Link>
              <Link className="rounded-lg border border-slate-800 bg-slate-900/70 p-4 hover:border-sky-700" to="/evidence-mapping">
                <h3 className="font-semibold text-white">Evidence mapping wizard</h3>
                <p className="mt-2 text-sm text-slate-300">Map imported evidence options to controlled requirements.</p>
              </Link>
              <Link className="rounded-lg border border-slate-800 bg-slate-900/70 p-4 hover:border-sky-700" to="/registry/drawer">
                <h3 className="font-semibold text-white">Registry workbench</h3>
                <p className="mt-2 text-sm text-slate-300">Maintain governing bodies, citations, and controlled evidence vocabulary.</p>
              </Link>
            </div>
          </Panel>
        )
      case 'vocabulary':
        return (
          <Panel
            title="Vocabulary Mappings"
            description="Tenant and product terms resolve to controlled Compliance Core vocabulary so aliases do not become hidden rule logic."
          >
            {activeVocabularyTerms.length === 0 ? (
              <p className="text-sm text-slate-400">No active terms are loaded for the selected vocabulary type.</p>
            ) : (
              <div className="grid gap-3 lg:grid-cols-2">
                {activeVocabularyTerms.map((term) => (
                  <article key={term.termId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-4">
                    <h3 className="font-semibold text-white">{term.label}</h3>
                    <p className="mt-1 font-mono text-xs text-sky-300">{term.termKey}</p>
                    <p className="mt-2 text-sm text-slate-300">{term.description}</p>
                    <p className="mt-2 text-xs text-slate-400">
                      Aliases: {term.aliases.length > 0 ? term.aliases.join(', ') : 'none recorded'}
                    </p>
                  </article>
                ))}
              </div>
            )}
          </Panel>
        )
      case 'subjects':
        return (
          <Panel
            title="Subject Mappings"
            description="Subject mappings explain what Compliance Core is evaluating, while source products retain ownership of the underlying records."
          >
            {sourceProducts.length === 0 ? (
              <p className="text-sm text-slate-400">No active fact sources are mapped to subject-owning products yet.</p>
            ) : (
              <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                {sourceProducts.map((product) => (
                  <article key={product} className="rounded-lg border border-slate-800 bg-slate-900/70 p-4">
                    <h3 className="font-semibold text-white">{product}</h3>
                    <p className="mt-2 text-sm text-slate-300">
                      Provides source facts or evidence context; Compliance Core references it without owning the source record.
                    </p>
                  </article>
                ))}
              </div>
            )}
          </Panel>
        )
      case 'outputs':
        return (
          <Panel
            title="Output Signals"
            description="Output signals separate Compliance Core calculations from product workflow behavior such as warnings, blockers, recommendations, and report gaps."
          >
            {(s.workflowGatesQuery.data ?? []).length === 0 ? (
              <p className="text-sm text-slate-400">No workflow gates are registered as output-producing signals yet.</p>
            ) : (
              <div className="space-y-3">
                {(s.workflowGatesQuery.data ?? []).map((gate) => (
                  <article key={gate.workflowGateId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-4">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                      <div>
                        <h3 className="font-semibold text-white">{gate.label}</h3>
                        <p className="mt-1 text-sm text-slate-300">{gate.description}</p>
                      </div>
                      <span className="rounded-md bg-slate-800 px-2 py-1 text-xs text-slate-300">
                        {gate.isActive ? 'Active signal' : 'Inactive signal'}
                      </span>
                    </div>
                    <p className="mt-2 text-xs text-slate-400">Rulepack: {humanize(gate.packKey)}</p>
                  </article>
                ))}
              </div>
            )}
          </Panel>
        )
      case 'overview':
      default:
        return (
          <div className="space-y-6">
            <Panel
              title="Mapping health"
              description="Mapping Center answers where Compliance Core facts, evidence, vocabulary, subjects, and output signals come from."
            >
              <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
                <Metric label="Facts mapped" value={`${mappedFactCount} / ${facts.length}`} hint="Normalized facts with active sources" />
                <Metric label="Requirements sourced" value={`${sourcedRequirements.length} / ${requirements.length}`} hint="Active requirements with at least one source" />
                <Metric label="Regulatory mappings" value={activeMappings.length} hint="Active rulepack, citation, fact, or key mappings" />
                <Metric label="Source products" value={sourceProducts.length} hint="Products currently feeding mapped facts" />
              </div>
            </Panel>
            <RegulatoryMappingsPanel mappings={s.regulatoryMappingsQuery.data ?? []} />
            <CitationFactCatalogPanel
              citations={s.citationsQuery.data ?? []}
              factDefinitions={facts}
              factRequirements={s.factRequirementsQuery.data ?? []}
            />
          </div>
        )
    }
  })()

  return (
    <div className="space-y-6">
      <Panel title="Mapping Center" description="Product field, event, document, and vocabulary inputs become normalized Compliance Core facts before rulepack calculations consume them. Missing mappings stay visible and reviewable.">
        <div className="grid gap-3 lg:grid-cols-7">
          {['Product source', 'Normalized fact', 'Rulepack requirement', 'Evidence', 'Calculation', 'Result quality', 'Product signal'].map((step, index) => (
            <div key={step} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3">
              <span className="text-xs font-semibold text-sky-300">{index + 1}</span>
              <p className="mt-1 text-sm font-medium text-slate-100">{step}</p>
            </div>
          ))}
        </div>
      </Panel>
      <MappingTabs activeView={activeView} />
      {viewContent}
    </div>
  )
}
