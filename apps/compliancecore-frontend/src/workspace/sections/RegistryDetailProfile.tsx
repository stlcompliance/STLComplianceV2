import {
  AlertTriangle,
  BookOpen,
  CheckCircle2,
  FileCheck2,
  GitBranch,
  History,
  Pencil,
  Scale,
  ShieldCheck,
  XCircle,
} from 'lucide-react'
import type { ReactNode } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import {
  DetailBadge,
  DetailEmptyState,
  ProfileDetailsLayout,
  type DetailRailSectionConfig,
  type DetailTone,
} from '@stl/shared-ui'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatDate(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleDateString(undefined, { month: 'short', day: '2-digit', year: 'numeric' })
}

function statusTone(value: string | null | undefined): DetailTone {
  const normalized = value?.toLowerCase() ?? ''
  if (['active', 'published', 'pass', 'passed', 'resolved'].includes(normalized)) return 'good'
  if (['draft', 'review', 'pending', 'warning'].includes(normalized)) return 'warn'
  if (['inactive', 'failed', 'fail', 'open', 'critical', 'high'].includes(normalized)) return 'bad'
  return 'neutral'
}

function productLabel(value: string | null | undefined): string {
  if (!value) return 'Unassigned product'
  const labels: Record<string, string> = {
    compliancecore: 'Compliance Core',
    routarr: 'RoutArr',
    staffarr: 'StaffArr',
    trainarr: 'TrainArr',
    maintainarr: 'MaintainArr',
    recordarr: 'RecordArr',
    reportarr: 'ReportArr',
    supplyarr: 'SupplyArr',
    loadarr: 'LoadArr',
  }
  return labels[value.toLowerCase()] ?? humanize(value)
}

function actionLink(to: string, label: string, icon: ReactNode, primary = false) {
  return (
    <Link
      to={to}
      className={`inline-flex items-center gap-2 rounded-md px-4 py-3 text-sm font-semibold ${
        primary
          ? 'bg-sky-500 text-[var(--color-text-primary)] hover:bg-sky-400'
          : 'border border-slate-800 bg-slate-900 text-white hover:border-sky-700'
      }`}
    >
      {icon}
      {label}
    </Link>
  )
}

function noSelection() {
  return (
    <div className="rounded-lg border border-slate-800 bg-slate-950/70 p-8 text-center">
      <Scale className="mx-auto h-10 w-10 text-sky-300" />
      <h1 className="mt-4 text-2xl font-bold text-white">No rulepack selected</h1>
      <p className="mt-2 text-sm text-slate-400">Seed or select a rulepack to view calculation, mapping, and evaluation details.</p>
      <Link
        to="/rulepacks"
        className="mt-5 inline-flex items-center gap-2 rounded-md bg-sky-500 px-4 py-2 text-sm font-semibold text-[var(--color-text-primary)] hover:bg-sky-400"
      >
        Open rulepacks
      </Link>
    </div>
  )
}

function listPanel<T>(items: T[], emptyText: string, render: (item: T) => ReactNode) {
  if (items.length === 0) return <DetailEmptyState text={emptyText} />
  return <div className="space-y-3">{items.map(render)}</div>
}

const REGISTRY_TABS = [
  'overview',
  'applicability',
  'requirements',
  'facts',
  'calculations',
  'evidence',
  'mappings',
  'trace',
  'versions',
] as const

type RegistryTab = (typeof REGISTRY_TABS)[number]

function normalizeRegistryTab(value: string | null): RegistryTab {
  return value && REGISTRY_TABS.includes(value as RegistryTab) ? (value as RegistryTab) : 'overview'
}

function tabLabel(tab: RegistryTab): string {
  if (tab === 'facts') return 'Facts Needed'
  if (tab === 'trace') return 'Evaluation Trace'
  return tab === 'overview' ? 'Overview' : humanize(tab)
}

export function RegistryDetailProfile({ state: s }: { state: ComplianceCoreWorkspaceState }) {
  const [searchParams, setSearchParams] = useSearchParams()
  const rulePacks = s.rulePacksQuery?.data ?? []
  const rulePack = rulePacks.find((pack) => pack.rulePackId === s.selectedRulePackId) ?? rulePacks[0] ?? null
  if (!rulePack) return noSelection()
  const activeTab = normalizeRegistryTab(searchParams.get('tab'))

  const program = (s.programsQuery?.data ?? []).find(
    (item) => item.regulatoryProgramId === rulePack.regulatoryProgramId,
  )
  const citations = (s.citationsQuery?.data ?? []).filter(
    (citation) => citation.rulePackId === rulePack.rulePackId || citation.regulatoryProgramId === rulePack.regulatoryProgramId,
  )
  const factRequirements = (s.factRequirementsQuery?.data ?? []).filter(
    (requirement) => requirement.rulePackId === rulePack.rulePackId,
  )
  const mappings = (s.regulatoryMappingsQuery?.data ?? []).filter(
    (mapping) => mapping.rulePackId === rulePack.rulePackId || mapping.regulatoryProgramId === rulePack.regulatoryProgramId,
  )
  const content = s.rulePackContentQuery?.data
  const rules = content?.content?.rules ?? []
  const evaluations = s.ruleEvaluationsQuery?.data ?? []
  const findings = (s.findingsQuery?.data ?? []).filter((finding) => finding.rulePackId === rulePack.rulePackId)
  const openFindings = findings.filter((finding) => !['resolved', 'closed'].includes(finding.status))
  const factSources = s.factSourcesQuery?.data ?? []
  const sourceProducts = [
    ...new Set(
      factSources
        .filter((source) => factRequirements.some((requirement) => requirement.factKey === source.factKey))
        .map((source) => productLabel(source.productKey)),
    ),
  ]
  const workflowGates = (s.workflowGatesQuery?.data ?? []).filter(
    (gate) => gate.rulePackId === rulePack.rulePackId,
  )
  const requiredFactSourceCount = factRequirements.filter((requirement) =>
    factSources.some((source) => source.factKey === requirement.factKey && source.isActive),
  ).length
  const blocked = !rulePack.isActive || openFindings.length > 0 || factRequirements.length > requiredFactSourceCount
  const setActiveTab = (tab: RegistryTab) => {
    const next = new URLSearchParams(searchParams)
    next.set('tab', tab)
    setSearchParams(next, { replace: true })
  }

  const mainContent = (() => {
    switch (activeTab) {
      case 'applicability':
        return (
          <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Why this rulepack applies</h3>
            <p className="mt-1 text-sm text-slate-400">
              Applicability should be readable before users inspect rule logic. Compliance Core
              decides whether this pack is likely relevant; source products still own the
              operational records being evaluated.
            </p>
            <div className="mt-4 grid gap-4 lg:grid-cols-2">
              <div className="rounded-lg border border-slate-800 bg-slate-950/80 p-4">
                <h4 className="font-semibold text-white">Plain-language applicability</h4>
                <ul className="mt-3 space-y-2 text-sm text-slate-300">
                  <li>This pack belongs to {rulePack.regulatoryProgramLabel}.</li>
                  <li>{citations.length} linked citation{citations.length === 1 ? '' : 's'} provide source authority.</li>
                  <li>{factRequirements.length} fact requirement{factRequirements.length === 1 ? '' : 's'} help decide applicability and outcomes.</li>
                  <li>{sourceProducts.length > 0 ? `${sourceProducts.join(', ')} provide mapped source context.` : 'No mapped source products are linked yet.'}</li>
                </ul>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-950/80 p-4">
                <h4 className="font-semibold text-white">Applicability logic pattern</h4>
                <p className="mt-3 text-sm text-slate-300">
                  IF required facts and mapped evidence indicate this rulepack is relevant, THEN
                  Compliance Core can run the pack and return an explainable result. Unknown or
                  conflicting facts should create review work rather than silently degrading output.
                </p>
                <Link className="mt-4 inline-flex text-sm font-semibold text-sky-300 hover:text-sky-200" to="/evaluation/tester">
                  Test a situation
                </Link>
              </div>
            </div>
          </section>
        )
      case 'requirements':
        return (
          <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Requirements</h3>
            <p className="mt-1 text-sm text-slate-400">
              Requirements are the obligations this rulepack evaluates. Each requirement should
              connect to a citation, normalized facts, evidence, calculations, and product impact.
            </p>
            <div className="mt-4 space-y-3">
              {listPanel(factRequirements, 'No requirements are linked to this rulepack yet.', (requirement) => {
                const sourceCount = factSources.filter(
                  (source) => source.factKey === requirement.factKey && source.isActive,
                ).length
                return (
                  <div key={requirement.factRequirementId} className="rounded-lg border border-slate-800 bg-slate-950/80 p-4">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                      <div>
                        <h4 className="font-semibold text-white">{requirement.label}</h4>
                        <p className="mt-1 text-sm text-slate-300">{requirement.description}</p>
                      </div>
                      <DetailBadge
                        label={requirement.isRequired ? 'Required' : 'Optional'}
                        tone={requirement.isRequired ? 'warn' : 'neutral'}
                      />
                    </div>
                    <div className="mt-3 grid gap-3 text-xs text-slate-400 sm:grid-cols-3">
                      <span>Citation: {requirement.citationKey ?? 'not linked'}</span>
                      <span>Fact: {requirement.factKey}</span>
                      <span>Sources: {sourceCount}</span>
                    </div>
                  </div>
                )
              })}
            </div>
          </section>
        )
      case 'facts':
        return (
          <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Facts Needed</h3>
            <p className="mt-1 text-sm text-slate-400">
              Facts are the bridge between legal logic and product-owned data. Product fields,
              events, or documents map into normalized Compliance Core facts before calculations
              use them.
            </p>
            <div className="mt-4 space-y-3">
              {listPanel(factRequirements, 'No facts required by this rulepack yet.', (requirement) => {
                const linkedSources = factSources.filter(
                  (source) => source.factKey === requirement.factKey && source.isActive,
                )
                return (
                  <div key={requirement.factRequirementId} className="rounded-lg border border-slate-800 bg-slate-950/80 p-4">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <h4 className="font-semibold text-white">{requirement.factLabel || requirement.label}</h4>
                        <p className="mt-1 font-mono text-xs text-sky-300">{requirement.factKey}</p>
                      </div>
                      <DetailBadge
                        label={linkedSources.length > 0 ? 'Mapped' : 'Unmapped'}
                        tone={linkedSources.length > 0 ? 'good' : 'warn'}
                      />
                    </div>
                    <p className="mt-3 text-sm text-slate-300">{requirement.description}</p>
                    <div className="mt-3 space-y-2">
                      {linkedSources.length === 0 ? (
                        <p className="rounded-md border border-amber-900/70 bg-amber-950/30 px-3 py-2 text-xs text-amber-100">
                          No active source product, questionnaire fallback, or evidence source is mapped.
                        </p>
                      ) : (
                        linkedSources.map((source) => (
                          <div key={source.factSourceId} className="rounded-md border border-slate-800 bg-slate-900 px-3 py-2">
                            <p className="text-sm font-medium text-slate-100">{source.label}</p>
                            <p className="mt-1 text-xs text-slate-400">
                              {productLabel(source.productKey)} - {humanize(source.sourceType)}
                            </p>
                          </div>
                        ))
                      )}
                    </div>
                  </div>
                )
              })}
            </div>
          </section>
        )
      case 'calculations':
        return (
          <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Calculations</h3>
            <p className="mt-1 text-sm text-slate-400">
              Calculation recipes transform facts into results. The rule content below is shown as
              plain-language evaluation intent, not raw rule JSON.
            </p>
            <div className="mt-4 space-y-3">
              {listPanel(rules, 'No calculation content is loaded yet.', (rule) => (
                <div key={rule.ruleKey} className="rounded-lg border border-slate-800 bg-slate-950/80 p-4">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <h4 className="font-semibold text-white">{rule.label}</h4>
                      <p className="mt-1 text-sm text-slate-300">
                        Checks whether {rule.factKey} equals {String(rule.expectedValue)}.
                      </p>
                    </div>
                    <DetailBadge label={rule.nonWaivable ? 'Non-waivable' : humanize(rule.type)} tone="info" />
                  </div>
                </div>
              ))}
            </div>
          </section>
        )
      case 'evidence':
        return (
          <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Evidence</h3>
            <p className="mt-1 text-sm text-slate-400">
              Evidence is separate from facts. A document can exist without satisfying a requirement
              unless it matches the required type, subject, date range, acceptance state, and review state.
            </p>
            <div className="mt-4 grid gap-4 lg:grid-cols-3">
              <Link className="rounded-lg border border-slate-800 bg-slate-950/80 p-4 hover:border-sky-700" to="/evidence-requirements">
                <h4 className="font-semibold text-white">Evidence requirements</h4>
                <p className="mt-2 text-sm text-slate-300">Review what proof this pack or its citations require.</p>
              </Link>
              <Link className="rounded-lg border border-slate-800 bg-slate-950/80 p-4 hover:border-sky-700" to="/evidence-mapping">
                <h4 className="font-semibold text-white">Evidence mappings</h4>
                <p className="mt-2 text-sm text-slate-300">Map imported evidence options to controlled requirements.</p>
              </Link>
              <div className="rounded-lg border border-slate-800 bg-slate-950/80 p-4">
                <h4 className="font-semibold text-white">RecordArr boundary</h4>
                <p className="mt-2 text-sm text-slate-300">
                  RecordArr owns stored files and retention. Compliance Core owns evidence meaning.
                </p>
              </div>
            </div>
          </section>
        )
      case 'mappings':
        return (
          <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Mappings for this rulepack</h3>
            <p className="mt-1 text-sm text-slate-400">
              These mappings are scoped to this rulepack or its regulatory program. Use Mapping
              Center for cross-rulepack coverage, vocabulary, subjects, and outputs.
            </p>
            <div className="mt-4 space-y-3">
              {listPanel(mappings, 'No regulatory mappings linked yet.', (mapping) => (
                <div key={mapping.regulatoryMappingId} className="rounded-lg border border-slate-800 bg-slate-900 p-4">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <h4 className="font-semibold text-white">{mapping.label}</h4>
                      <p className="mt-1 text-xs text-slate-400">{mapping.description}</p>
                    </div>
                    <DetailBadge label={humanize(mapping.targetKind)} tone={mapping.isActive ? 'good' : 'neutral'} />
                  </div>
                  <p className="mt-3 font-mono text-xs text-sky-300">{mapping.mappingKey}</p>
                </div>
              ))}
            </div>
          </section>
        )
      case 'trace':
        return (
          <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Evaluation Trace</h3>
            <p className="mt-1 text-sm text-slate-400">
              A trace explains why the rulepack applied, which facts were used or missing, which
              calculations ran, and what result or product signal was produced.
            </p>
            <div className="mt-4 space-y-3">
              {listPanel(evaluations, 'No evaluations run for this rulepack yet.', (evaluation) => (
                <div key={evaluation.evaluationRunId} className="rounded-lg border border-slate-800 bg-slate-900 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <h4 className="font-semibold text-white">{formatDate(evaluation.createdAt)}</h4>
                      <p className="mt-1 text-xs text-slate-400">{evaluation.ruleResults.length} calculation result{evaluation.ruleResults.length === 1 ? '' : 's'}</p>
                    </div>
                    <DetailBadge label={humanize(evaluation.overallResult)} tone={statusTone(evaluation.overallResult)} />
                  </div>
                  <div className="mt-3 grid gap-3 text-xs text-slate-400 sm:grid-cols-3">
                    <span>Facts used: {Object.keys(evaluation.factInputs).length}</span>
                    <span>Findings: {evaluation.findingsEmitted?.length ?? 0}</span>
                    <Link className="text-sky-300 hover:text-sky-200" to="/evaluation/traces">Explain result</Link>
                  </div>
                </div>
              ))}
            </div>
          </section>
        )
      case 'versions':
        return (
          <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Versions</h3>
            <p className="mt-1 text-sm text-slate-400">
              Rulepack versions preserve what was active when prior decisions were made. A new
              version should not overwrite historical interpretation.
            </p>
            <div className="mt-4 grid gap-4 lg:grid-cols-2">
              <div className="rounded-lg border border-slate-800 bg-slate-950/80 p-4">
                <h4 className="font-semibold text-white">Lifecycle</h4>
                <dl className="mt-3 space-y-2 text-sm text-slate-300">
                  <div className="flex items-center justify-between gap-3">
                    <dt className="text-slate-400">Created</dt>
                    <dd>{formatDate(rulePack.createdAt)}</dd>
                  </div>
                  <div className="flex items-center justify-between gap-3">
                    <dt className="text-slate-400">Updated</dt>
                    <dd>{formatDate(rulePack.updatedAt)}</dd>
                  </div>
                  <div className="flex items-center justify-between gap-3">
                    <dt className="text-slate-400">Version</dt>
                    <dd>{rulePack.versionNumber}</dd>
                  </div>
                </dl>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-950/80 p-4">
                <h4 className="font-semibold text-white">Recent findings</h4>
                <div className="mt-3 space-y-3">
                  {listPanel(findings, 'No findings emitted for this rule pack.', (finding) => (
                    <div key={finding.findingId} className="rounded-md border border-slate-800 bg-slate-900 p-3">
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <p className="font-medium text-white">{finding.title}</p>
                          <p className="mt-1 text-xs text-slate-400">{finding.reasonCode}</p>
                        </div>
                        <DetailBadge label={humanize(finding.severity)} tone={statusTone(finding.severity)} />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </section>
        )
      case 'overview':
      default:
        return (
          <div className="space-y-4">
            <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
              <h3 className="text-lg font-bold text-white">What this rulepack calculates</h3>
              <p className="mt-2 text-sm text-slate-300">{rulePack.description}</p>
              <div className="mt-4 grid gap-3 lg:grid-cols-7">
                {['Source law', 'Requirement', 'Required facts', 'Mapped sources', 'Calculation', 'Result', 'Product impact'].map((step, index) => (
                  <div key={step} className="rounded-lg border border-slate-800 bg-slate-900 p-3">
                    <span className="text-xs font-semibold text-sky-300">{index + 1}</span>
                    <p className="mt-1 text-sm font-medium text-slate-100">{step}</p>
                  </div>
                ))}
              </div>
            </section>
            <section className="grid gap-4 lg:grid-cols-3">
              <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
                <h4 className="font-semibold text-white">Data readiness</h4>
                <dl className="mt-3 space-y-2 text-sm text-slate-300">
                  <div className="flex justify-between gap-3"><dt className="text-slate-400">Required facts</dt><dd>{factRequirements.length}</dd></div>
                  <div className="flex justify-between gap-3"><dt className="text-slate-400">Mapped facts</dt><dd>{requiredFactSourceCount}</dd></div>
                  <div className="flex justify-between gap-3"><dt className="text-slate-400">Unmapped facts</dt><dd>{Math.max(factRequirements.length - requiredFactSourceCount, 0)}</dd></div>
                  <div className="flex justify-between gap-3"><dt className="text-slate-400">Open findings</dt><dd>{openFindings.length}</dd></div>
                </dl>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
                <h4 className="font-semibold text-white">Product inputs</h4>
                <div className="mt-3 space-y-2">
                  {sourceProducts.length === 0 ? (
                    <p className="text-sm text-slate-400">No product inputs mapped yet.</p>
                  ) : (
                    sourceProducts.map((product) => (
                      <p key={product} className="rounded-md border border-slate-800 bg-slate-900 px-3 py-2 text-sm text-slate-200">{product}</p>
                    ))
                  )}
                </div>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
                <h4 className="font-semibold text-white">Product outputs</h4>
                <div className="mt-3 space-y-2">
                  {workflowGates.length === 0 ? (
                    <p className="text-sm text-slate-400">No output-producing workflow gates registered yet.</p>
                  ) : (
                    workflowGates.map((gate) => (
                      <div key={gate.workflowGateId} className="rounded-md border border-slate-800 bg-slate-900 px-3 py-2">
                        <p className="text-sm text-slate-100">{gate.label}</p>
                        <p className="mt-1 text-xs text-slate-400">{gate.isActive ? 'Active' : 'Inactive'}</p>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </section>
            <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
              <h3 className="text-lg font-bold text-white">Rules and citations</h3>
              <div className="mt-4 grid gap-4 lg:grid-cols-2">
                <div>
                  <h4 className="mb-3 text-sm font-semibold text-sky-200">Calculations</h4>
                  {listPanel(rules.slice(0, 5), 'No calculation content loaded yet.', (rule) => (
                    <div key={rule.ruleKey} className="rounded-lg border border-slate-800 bg-slate-950/80 p-4">
                      <h5 className="font-semibold text-white">{rule.label}</h5>
                      <p className="mt-1 text-sm text-sky-100/75">
                        {rule.factKey} equals {String(rule.expectedValue)}
                      </p>
                    </div>
                  ))}
                </div>
                <div>
                  <h4 className="mb-3 text-sm font-semibold text-sky-200">Citations</h4>
                  {listPanel(citations.slice(0, 5), 'No citations linked yet.', (citation) => (
                    <div key={citation.citationId} className="rounded-lg border border-slate-800 bg-slate-950/80 p-4">
                      <h5 className="font-semibold text-white">{citation.label}</h5>
                      <p className="mt-1 text-sm text-sky-100/75">{citation.sourceReference}</p>
                    </div>
                  ))}
                </div>
              </div>
            </section>
          </div>
        )
    }
  })()

  const rails: DetailRailSectionConfig[] = [
    {
      title: 'Required facts',
      icon: <FileCheck2 className="h-5 w-5" />,
      content: listPanel(factRequirements.slice(0, 5), 'No facts required by this rule pack yet.', (requirement) => (
        <div key={requirement.factRequirementId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <h3 className="font-semibold text-white">{requirement.label}</h3>
              <p className="mt-1 text-xs text-slate-400">{requirement.factKey}</p>
            </div>
            <DetailBadge label={requirement.isRequired ? 'Required' : 'Optional'} tone={requirement.isRequired ? 'warn' : 'neutral'} />
          </div>
        </div>
      )),
    },
    {
      title: 'Recent findings',
      icon: <AlertTriangle className="h-5 w-5" />,
      content: listPanel(findings.slice(0, 4), 'No findings emitted for this rule pack.', (finding) => (
        <div key={finding.findingId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <h3 className="font-semibold text-white">{finding.title}</h3>
              <p className="mt-1 text-xs text-slate-400">{finding.reasonCode}</p>
            </div>
            <DetailBadge label={humanize(finding.severity)} tone={statusTone(finding.severity)} />
          </div>
        </div>
      )),
    },
  ]

  return (
    <ProfileDetailsLayout
      testId="compliancecore-registry-profile"
      backLabel="Rulepacks"
      backTo="/rulepacks"
      breadcrumbs={[rulePack.regulatoryProgramLabel, rulePack.label]}
      icon={<Scale className="h-9 w-9" />}
      title={rulePack.label}
      subtitle={<span>{rulePack.regulatoryProgramLabel} - Version {rulePack.versionNumber}</span>}
      badges={[
        { label: rulePack.packKey, tone: 'info' },
        { label: humanize(rulePack.status), tone: statusTone(rulePack.status) },
        { label: rulePack.isActive ? 'Active' : 'Inactive', tone: rulePack.isActive ? 'good' : 'bad' },
      ]}
      actions={<>{actionLink('/registry/drawer', 'Open registry workbench', <Pencil className="h-4 w-4" />)}</>}
      metrics={[
        { label: 'Rulepack state', value: humanize(rulePack.status), hint: `Version ${rulePack.versionNumber}`, icon: <ShieldCheck className="h-5 w-5" />, tone: statusTone(rulePack.status) },
        { label: 'Rules', value: rules.length, hint: content?.hasContent ? 'Content loaded' : 'No content body', icon: <BookOpen className="h-5 w-5" />, tone: rules.length > 0 ? 'good' : 'warn' },
        { label: 'Required facts', value: factRequirements.length, hint: `${requiredFactSourceCount} sourced`, icon: <FileCheck2 className="h-5 w-5" />, tone: factRequirements.length === requiredFactSourceCount ? 'good' : 'warn' },
        { label: 'Open findings', value: openFindings.length, hint: 'Unresolved findings', icon: <AlertTriangle className="h-5 w-5" />, tone: openFindings.length > 0 ? 'bad' : 'good' },
      ]}
      tabs={REGISTRY_TABS.map((tab) => ({ key: tab, label: tabLabel(tab) }))}
      activeTab={activeTab}
      onTabChange={(tabKey) => setActiveTab(normalizeRegistryTab(tabKey))}
      snapshotTitle="Rulepack snapshot"
      snapshotSubtitle="Rulepack identity, regulatory lineage, content version, fact requirements, mappings, and evaluation posture."
      snapshotFields={[
        { label: 'Rulepack ID', value: rulePack.rulePackId, source: 'Compliance Core source of truth' },
        { label: 'Pack key', value: rulePack.packKey, source: 'Registry key' },
        { label: 'Regulatory program', value: rulePack.regulatoryProgramLabel, source: 'Program registry' },
        { label: 'Jurisdiction', value: program?.jurisdictionLabel ?? 'Not loaded', source: 'Jurisdiction registry' },
        { label: 'Description', value: rulePack.description, source: 'Rulepack profile' },
        { label: 'Version', value: rulePack.versionNumber, source: 'Rule versioning' },
        { label: 'Status', value: humanize(rulePack.status), source: 'Lifecycle state' },
        { label: 'Created', value: formatDate(rulePack.createdAt), source: 'Audit trail' },
        { label: 'Updated', value: formatDate(rulePack.updatedAt), source: 'Audit trail' },
      ]}
      mainContent={mainContent}
      decisionTitle="Rulepack decision"
      decisionBadge={{ label: blocked ? 'Review' : 'Ready', tone: blocked ? 'warn' : 'good' }}
      decisionIcon={blocked ? <XCircle className="h-5 w-5 text-amber-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={blocked ? 'Rulepack needs review' : 'Rulepack ready for evaluation'}
      decisionDetail={blocked ? 'Inactive state, unresolved findings, or unsourced facts should be reviewed before relying on this pack.' : 'Active rulepack state, sourced facts, and clear findings support normal workflow-gate evaluation.'}
      allowedChecks={[rulePack.isActive, rules.length > 0, factRequirements.length === requiredFactSourceCount, openFindings.length === 0].filter(Boolean).length}
      blockedChecks={[!rulePack.isActive, factRequirements.length > requiredFactSourceCount, openFindings.length > 0].filter(Boolean).length}
      railSections={[
        ...rails,
        {
          title: 'Mappings',
          icon: <GitBranch className="h-5 w-5" />,
          content: listPanel(mappings.slice(0, 4), 'No regulatory mappings linked yet.', (mapping) => (
            <div key={mapping.regulatoryMappingId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
              <h3 className="font-semibold text-white">{mapping.label}</h3>
              <p className="mt-1 text-xs text-slate-400">{humanize(mapping.targetKind)}</p>
            </div>
          )),
        },
        {
          title: 'Evaluations',
          icon: <History className="h-5 w-5" />,
          content: listPanel(evaluations.slice(0, 4), 'No evaluations run for this pack yet.', (evaluation) => (
            <div key={evaluation.evaluationRunId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="font-semibold text-white">{formatDate(evaluation.createdAt)}</h3>
                  <p className="mt-1 text-xs text-slate-400">{evaluation.ruleResults.length} rule results</p>
                </div>
                <DetailBadge label={humanize(evaluation.overallResult)} tone={statusTone(evaluation.overallResult)} />
              </div>
            </div>
          )),
        },
      ]}
    />
  )
}
