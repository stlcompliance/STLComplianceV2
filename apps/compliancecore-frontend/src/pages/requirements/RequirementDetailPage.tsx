import { useSearchParams, Link } from 'react-router-dom'
import {
  AlertTriangle,
  BookOpen,
  CheckCircle2,
  FileCheck2,
  GitBranch,
  History,
  Scale,
  ShieldCheck,
} from 'lucide-react'
import {
  DetailEmptyState,
  ProfileDetailsLayout,
  type DetailRailSectionConfig,
} from '@stl/shared-ui'
import { SituationEvaluatorPanel } from '../../components/SituationEvaluatorPanel'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

const REQUIREMENT_TABS = ['overview', 'requirements', 'citations', 'logic', 'history'] as const

type RequirementTab = (typeof REQUIREMENT_TABS)[number]

function normalizeRequirementTab(value: string | null): RequirementTab {
  return value && REQUIREMENT_TABS.includes(value as RequirementTab) ? (value as RequirementTab) : 'overview'
}

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
  const activeTab = normalizeRequirementTab(searchParams.get('tab'))

  if (!requirement) {
    return (
      <div className="space-y-6">
        <DetailEmptyState text="Seed the registry catalog first to populate requirement detail views." />
        <Link
          to="/mappings"
          className="inline-flex rounded-md bg-sky-500 px-4 py-2 text-sm font-semibold text-[var(--color-text-primary)]"
        >
          Open mappings
        </Link>
      </div>
    )
  }

  const factDefinition = state.factDefinitionsQuery.data?.find(
    (fact) => fact.factDefinitionId === requirement.factDefinitionId,
  )
  const rulePack = state.rulePacksQuery.data?.find((pack) => pack.rulePackId === requirement.rulePackId) ?? null
  const citation = state.citationsQuery.data?.find((item) => item.citationId === requirement.citationId) ?? null
  const relatedRequirements = requirements.filter(
    (item) =>
      item.factRequirementId !== requirement.factRequirementId &&
      (item.factKey === requirement.factKey || item.rulePackId === requirement.rulePackId),
  )

  const blocked = !requirement.isActive || !rulePack || !citation
  const setActiveTab = (tab: RequirementTab) => {
    const next = new URLSearchParams(searchParams)
    next.set('tab', tab)
    setSearchParams(next, { replace: true })
  }

  const mainContent = (() => {
    switch (activeTab) {
      case 'requirements':
        return (
          <div className="space-y-4 rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
            <h2 className="text-lg font-bold text-white">Requirement details</h2>
            <p className="text-sm text-slate-400">Identity, scope, and active state for this requirement.</p>
            <dl className="grid gap-4 md:grid-cols-2">
              <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                <dt className="text-xs font-semibold uppercase tracking-wide text-slate-400">Requirement key</dt>
                <dd className="mt-2 text-sm font-semibold text-white">{requirement.requirementKey}</dd>
              </div>
              <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                <dt className="text-xs font-semibold uppercase tracking-wide text-slate-400">Fact key</dt>
                <dd className="mt-2 text-sm font-semibold text-white">{requirement.factKey}</dd>
              </div>
              <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                <dt className="text-xs font-semibold uppercase tracking-wide text-slate-400">Required</dt>
                <dd className="mt-2 text-sm font-semibold text-white">{requirement.isRequired ? 'Yes' : 'No'}</dd>
              </div>
              <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                <dt className="text-xs font-semibold uppercase tracking-wide text-slate-400">Status</dt>
                <dd className="mt-2 text-sm font-semibold text-white">{humanize(requirement.isActive ? 'active' : 'inactive')}</dd>
              </div>
            </dl>
          </div>
        )
      case 'citations':
        return (
          <div className="space-y-4 rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
            <h2 className="text-lg font-bold text-white">Citation details</h2>
            <p className="text-sm text-slate-400">Linked source references for this requirement.</p>
            <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
              <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Citation</h3>
              <p className="mt-2 text-sm text-slate-200">{citation?.sourceReference ?? requirement.citationKey ?? 'No citation linked.'}</p>
            </div>
            <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
              <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Related citation key</h3>
              <p className="mt-2 text-sm text-slate-200">{citation?.citationKey ?? requirement.citationKey ?? 'Not recorded'}</p>
            </div>
          </div>
        )
      case 'logic':
        return (
          <div className="space-y-4 rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
            <h2 className="text-lg font-bold text-white">Applicability and logic</h2>
            <p className="text-sm text-slate-400">
              The selected rule pack determines how this requirement is evaluated.
            </p>
            <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
              <p className="text-sm text-slate-200">
                {rulePack
                  ? `This requirement is evaluated through ${rulePack.packKey} within ${rulePack.regulatoryProgramLabel}.`
                  : 'No rule pack is linked yet, so applicability and compliance logic are not yet resolved.'}
              </p>
            </div>
          </div>
        )
      case 'history':
        return (
          <div className="space-y-4 rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
            <h2 className="text-lg font-bold text-white">History</h2>
            <p className="text-sm text-slate-400">Lifecycle timestamps and recent status for this requirement.</p>
            <div className="grid gap-4 md:grid-cols-2">
              <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Created</h3>
                <p className="mt-2 text-sm text-slate-200">{new Date(requirement.createdAt).toLocaleString()}</p>
              </div>
              <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Updated</h3>
                <p className="mt-2 text-sm text-slate-200">{new Date(requirement.updatedAt).toLocaleString()}</p>
              </div>
            </div>
          </div>
        )
      case 'overview':
      default:
        return (
          <div className="space-y-4 rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
            <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
              <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Plain-language summary</h2>
              <p className="mt-2 text-sm text-slate-200">
                {requirement.description || 'No plain-language summary has been captured yet.'}
              </p>
            </div>

            <div className="grid gap-4 md:grid-cols-2">
              <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Citations</h3>
                <p className="mt-2 text-sm text-slate-200">
                  {citation?.sourceReference ?? requirement.citationKey ?? 'No citation linked.'}
                </p>
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
          </div>
        )
    }
  })()

  const railSections: DetailRailSectionConfig[] = [
    {
      title: 'Related requirements',
      icon: <GitBranch className="h-5 w-5" />,
      content:
        relatedRequirements.length === 0 ? (
          <DetailEmptyState text="No related requirements were found for this fact or rule pack." />
        ) : (
          <ul className="space-y-2">
            {relatedRequirements.slice(0, 6).map((item) => (
              <li
                key={item.factRequirementId}
                className="rounded-lg border border-slate-800 bg-slate-950/50 p-3 text-sm text-slate-200"
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <div className="font-medium">{item.label}</div>
                    <div className="mt-1 text-xs text-[var(--color-text-muted)]">{item.requirementKey}</div>
                  </div>
                  <span className="rounded-full border border-slate-700 px-2 py-0.5 text-xs text-slate-400">
                    {item.isRequired ? 'required' : 'optional'}
                  </span>
                </div>
              </li>
            ))}
          </ul>
        ),
    },
    {
      title: 'Evaluation tools',
      icon: <ShieldCheck className="h-5 w-5" />,
      content: (
        <div className="space-y-3">
          <p className="text-sm text-slate-400">
            Use the situation evaluator to test whether the active fact requirements and exception logic behave as
            expected.
          </p>
          <SituationEvaluatorPanel
            accessToken={state.accessToken}
            canEvaluate={state.canManage || state.canEvaluateRisk}
            factRequirements={requirements}
          />
        </div>
      ),
    },
  ]

  return (
    <ProfileDetailsLayout
      testId="compliancecore-requirement-detail"
      backLabel="Registry"
      backTo="/registry/drawer"
      breadcrumbs={[rulePack?.label ?? 'Requirements', requirement.requirementKey]}
      icon={<Scale className="h-9 w-9" />}
      title={requirement.label}
      subtitle={`${requirement.requirementKey} · ${requirement.factLabel}`}
      badges={[
        { label: requirement.isRequired ? 'Required' : 'Optional', tone: requirement.isRequired ? 'warn' : 'neutral' },
        { label: requirement.isActive ? 'Active' : 'Inactive', tone: requirement.isActive ? 'good' : 'bad' },
        { label: rulePack ? rulePack.label : 'Unlinked', tone: rulePack ? 'info' : 'warn' },
      ]}
      actions={
        <Link
          to="/mappings"
          className="inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-3 text-sm font-bold text-[var(--color-text-primary)] hover:bg-sky-400"
        >
          Open mappings
        </Link>
      }
      metrics={[
        {
          label: 'Fact definition',
          value: factDefinition?.label ?? 'Not linked',
          hint: factDefinition?.valueType ?? 'Not linked',
          icon: <BookOpen className="h-5 w-5" />,
          tone: factDefinition ? 'good' : 'warn',
        },
        {
          label: 'Citation',
          value: citation?.citationKey ?? 'Not linked',
          hint: citation?.sourceReference ?? requirement.citationKey ?? 'No citation linked',
          icon: <FileCheck2 className="h-5 w-5" />,
          tone: citation ? 'good' : 'warn',
        },
        {
          label: 'Related',
          value: relatedRequirements.length,
          hint: 'Linked requirements',
          icon: <GitBranch className="h-5 w-5" />,
          tone: relatedRequirements.length > 0 ? 'info' : 'neutral',
        },
        {
          label: 'Status',
          value: humanize(requirement.isActive ? 'active' : 'inactive'),
          hint: `Updated ${new Date(requirement.updatedAt).toLocaleString()}`,
          icon: <History className="h-5 w-5" />,
          tone: requirement.isActive ? 'good' : 'bad',
        },
      ]}
      tabs={REQUIREMENT_TABS.map((tab) => ({ key: tab, label: tab === 'overview' ? 'Overview' : humanize(tab) }))}
      activeTab={activeTab}
      onTabChange={(tabKey) => setActiveTab(normalizeRequirementTab(tabKey))}
      snapshotTitle="Requirement snapshot"
      snapshotSubtitle="Fact requirement identity, linked citation and rule pack, fact definition, and lifecycle state."
      snapshotFields={[
        { label: 'Requirement ID', value: requirement.factRequirementId, source: 'ComplianceCore source of truth' },
        { label: 'Requirement key', value: requirement.requirementKey, source: 'Registry key' },
        { label: 'Fact key', value: requirement.factKey, source: 'Fact requirement' },
        { label: 'Fact label', value: requirement.factLabel, source: 'Fact definition' },
        { label: 'Rule pack', value: rulePack?.label ?? requirement.rulePackKey ?? 'Unlinked', source: 'Rule pack registry' },
        { label: 'Citation', value: citation?.label ?? requirement.citationKey ?? 'Unlinked', source: 'Citation registry' },
        { label: 'Required', value: requirement.isRequired ? 'Yes' : 'No', source: 'Requirement rule' },
        { label: 'Created', value: new Date(requirement.createdAt).toLocaleString(), source: 'Audit trail' },
        { label: 'Updated', value: new Date(requirement.updatedAt).toLocaleString(), source: 'Audit trail' },
      ]}
      mainContent={
        <div className="space-y-4">
          <label className="block text-sm text-slate-300">
            Choose requirement
            <select
              value={requirement.factRequirementId}
              onChange={(event) => {
                const next = new URLSearchParams(searchParams)
                next.set('requirementId', event.target.value)
                next.set('tab', activeTab)
                setSearchParams(next, { replace: true })
              }}
              className="mt-2 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
            >
              {requirements.map((item) => (
                <option key={item.factRequirementId} value={item.factRequirementId}>
                  {item.requirementKey} - {item.label}
                </option>
              ))}
            </select>
          </label>

          {mainContent}
        </div>
      }
      decisionTitle="Requirement decision"
      decisionBadge={{ label: blocked ? 'Review' : 'Ready', tone: blocked ? 'warn' : 'good' }}
      decisionIcon={
        blocked ? (
          <AlertTriangle className="h-5 w-5 text-amber-300" />
        ) : (
          <CheckCircle2 className="h-5 w-5 text-emerald-300" />
        )
      }
      decisionSummary={blocked ? 'Requirement needs linkage or activation review' : 'Requirement ready for registry evaluation'}
      decisionDetail={
        blocked
          ? 'Inactive state, missing citation, or missing rule pack should be reviewed before relying on this requirement.'
          : 'Active requirement, linked citation, and rule-pack context support normal registry evaluation.'
      }
      allowedChecks={[
        requirement.isActive,
        Boolean(rulePack),
        Boolean(citation),
        Boolean(factDefinition),
      ].filter(Boolean).length}
      blockedChecks={[!requirement.isActive, !rulePack, !citation].filter(Boolean).length}
      railSections={railSections}
    />
  )
}
