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
import { Link } from 'react-router-dom'
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

function actionLink(to: string, label: string, icon: ReactNode, primary = false) {
  return (
    <Link
      to={to}
      className={`inline-flex items-center gap-2 rounded-xl px-4 py-3 text-sm font-semibold ${
        primary
          ? 'bg-sky-500 text-slate-950 hover:bg-sky-400'
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
    <div className="rounded-3xl border border-slate-800 bg-slate-950/70 p-8 text-center">
      <Scale className="mx-auto h-10 w-10 text-sky-300" />
      <h1 className="mt-4 text-2xl font-bold text-white">No rule pack selected</h1>
      <p className="mt-2 text-sm text-slate-400">Seed or select a rule pack to view registry profile details.</p>
      <Link
        to="/registry/drawer"
        className="mt-5 inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-sky-400"
      >
        Open registry
      </Link>
    </div>
  )
}

function listPanel<T>(items: T[], emptyText: string, render: (item: T) => ReactNode) {
  if (items.length === 0) return <DetailEmptyState text={emptyText} />
  return <div className="space-y-3">{items.map(render)}</div>
}

export function RegistryDetailProfile({ state: s }: { state: ComplianceCoreWorkspaceState }) {
  const rulePacks = s.rulePacksQuery?.data ?? []
  const rulePack = rulePacks.find((pack) => pack.rulePackId === s.selectedRulePackId) ?? rulePacks[0] ?? null
  if (!rulePack) return noSelection()

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
  const requiredFactSourceCount = factRequirements.filter((requirement) =>
    factSources.some((source) => source.factKey === requirement.factKey && source.isActive),
  ).length
  const blocked = !rulePack.isActive || openFindings.length > 0 || factRequirements.length > requiredFactSourceCount
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
      backLabel="Registry"
      backTo="/registry/drawer"
      breadcrumbs={[rulePack.regulatoryProgramLabel, rulePack.label]}
      icon={<Scale className="h-9 w-9" />}
      title={rulePack.label}
      subtitle={<span>{rulePack.regulatoryProgramLabel} - Version {rulePack.versionNumber}</span>}
      badges={[
        { label: rulePack.packKey, tone: 'info' },
        { label: humanize(rulePack.status), tone: statusTone(rulePack.status) },
        { label: rulePack.isActive ? 'Active' : 'Inactive', tone: rulePack.isActive ? 'good' : 'bad' },
      ]}
      actions={<>{actionLink('/registry/drawer', 'Edit registry', <Pencil className="h-4 w-4" />)}</>}
      metrics={[
        { label: 'Rule-pack state', value: humanize(rulePack.status), hint: `Version ${rulePack.versionNumber}`, icon: <ShieldCheck className="h-5 w-5" />, tone: statusTone(rulePack.status) },
        { label: 'Rules', value: rules.length, hint: content?.hasContent ? 'Content loaded' : 'No content body', icon: <BookOpen className="h-5 w-5" />, tone: rules.length > 0 ? 'good' : 'warn' },
        { label: 'Required facts', value: factRequirements.length, hint: `${requiredFactSourceCount} sourced`, icon: <FileCheck2 className="h-5 w-5" />, tone: factRequirements.length === requiredFactSourceCount ? 'good' : 'warn' },
        { label: 'Open findings', value: openFindings.length, hint: 'Unresolved findings', icon: <AlertTriangle className="h-5 w-5" />, tone: openFindings.length > 0 ? 'bad' : 'good' },
      ]}
      tabs={['Overview', 'Rules', 'Citations', 'Facts', 'Mappings', 'Evaluations', 'History']}
      snapshotTitle="Registry snapshot"
      snapshotSubtitle="Rule-pack identity, regulatory lineage, content version, fact requirements, mappings, and evaluation posture."
      snapshotFields={[
        { label: 'Rule pack ID', value: rulePack.rulePackId, source: 'ComplianceCore source of truth' },
        { label: 'Pack key', value: rulePack.packKey, source: 'Registry key' },
        { label: 'Regulatory program', value: rulePack.regulatoryProgramLabel, source: 'Program registry' },
        { label: 'Jurisdiction', value: program?.jurisdictionLabel ?? 'Not loaded', source: 'Jurisdiction registry' },
        { label: 'Description', value: rulePack.description, source: 'Rule-pack profile' },
        { label: 'Version', value: rulePack.versionNumber, source: 'Rule versioning' },
        { label: 'Status', value: humanize(rulePack.status), source: 'Lifecycle state' },
        { label: 'Created', value: formatDate(rulePack.createdAt), source: 'Audit trail' },
        { label: 'Updated', value: formatDate(rulePack.updatedAt), source: 'Audit trail' },
      ]}
      mainContent={(
        <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
          <h3 className="text-lg font-bold text-white">Rules and citations</h3>
          <div className="mt-4 grid gap-4 lg:grid-cols-2">
            <div>
              <h4 className="mb-3 text-sm font-semibold text-sky-200">Rules</h4>
              {listPanel(rules.slice(0, 5), 'No rule content loaded yet.', (rule) => (
                <div key={rule.ruleKey} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                  <h5 className="font-semibold text-white">{rule.label}</h5>
                  <p className="mt-1 text-sm text-sky-100/75">{rule.factKey} equals {String(rule.expectedValue)}</p>
                </div>
              ))}
            </div>
            <div>
              <h4 className="mb-3 text-sm font-semibold text-sky-200">Citations</h4>
              {listPanel(citations.slice(0, 5), 'No citations linked yet.', (citation) => (
                <div key={citation.citationId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                  <h5 className="font-semibold text-white">{citation.label}</h5>
                  <p className="mt-1 text-sm text-sky-100/75">{citation.sourceReference}</p>
                </div>
              ))}
            </div>
          </div>
        </section>
      )}
      decisionTitle="Registry decision"
      decisionBadge={{ label: blocked ? 'Review' : 'Ready', tone: blocked ? 'warn' : 'good' }}
      decisionIcon={blocked ? <XCircle className="h-5 w-5 text-amber-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={blocked ? 'Rule pack needs registry attention' : 'Rule pack ready for evaluation'}
      decisionDetail={blocked ? 'Inactive state, unresolved findings, or unsourced facts should be reviewed before relying on this pack.' : 'Active rule-pack state, sourced facts, and clear findings support normal workflow-gate evaluation.'}
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
