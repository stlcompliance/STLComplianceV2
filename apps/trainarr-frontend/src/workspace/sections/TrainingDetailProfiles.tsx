import {
  AlertTriangle,
  BookOpen,
  CheckCircle2,
  ClipboardCheck,
  FileText,
  GraduationCap,
  History,
  Layers,
  Pencil,
  RefreshCw,
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
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

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
  if (['active', 'published', 'current', 'complete', 'completed'].includes(normalized)) return 'good'
  if (['draft', 'pending', 'review', 'in_progress'].includes(normalized)) return 'warn'
  if (['inactive', 'revoked', 'expired', 'blocked', 'failed'].includes(normalized)) return 'bad'
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

function noSelection(title: string, text: string, to: string) {
  return (
    <div className="rounded-3xl border border-slate-800 bg-slate-950/70 p-8 text-center">
      <GraduationCap className="mx-auto h-10 w-10 text-sky-300" />
      <h1 className="mt-4 text-2xl font-bold text-white">{title}</h1>
      <p className="mt-2 text-sm text-slate-400">{text}</p>
      <Link
        to={to}
        className="mt-5 inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-sky-400"
      >
        Open drawer
      </Link>
    </div>
  )
}

function listPanel<T>(items: T[], emptyText: string, render: (item: T) => ReactNode) {
  if (items.length === 0) return <DetailEmptyState text={emptyText} />
  return <div className="space-y-3">{items.map(render)}</div>
}

export function TrainingProgramProfile({ state: s }: { state: TrainArrWorkspaceState }) {
  const detail = s.programDetailQuery?.data ?? null
  const summary = (s.programsQuery?.data ?? []).find((program) => program.programId === s.selectedProgramId)
    ?? (s.programsQuery?.data ?? [])[0]
    ?? null
  const program = detail ?? summary
  if (!program) {
    return noSelection('No training program selected', 'Select or create a training program to view its detail profile.', '/programs/drawer')
  }

  const definitions = detail?.definitions ?? []
  const versions = s.programVersionsQuery?.data ?? []
  const matrixEntries = (s.trainingMatrixQuery?.data?.entries ?? []).filter(
    (entry) => entry.trainingProgramId === program.programId,
  )
  const requirements = (s.requirementBuilderQuery?.data?.requirements ?? []).filter(
    (requirement) => requirement.trainingProgramId === program.programId,
  )
  const rulePackRequirements = s.programRulePackRequirementsQuery?.data ?? []
  const definitionCount = detail?.definitions.length ?? summary?.definitionCount ?? 0
  const publishedVersionCount = summary?.publishedVersionCount ?? versions.filter((version) => version.status === 'published').length
  const blocked = program.status !== 'active' || definitionCount === 0
  const rails: DetailRailSectionConfig[] = [
    {
      title: 'Definitions',
      icon: <BookOpen className="h-5 w-5" />,
      content: listPanel(definitions.slice(0, 4), 'No training definitions linked yet.', (definition) => (
        <div key={definition.trainingDefinitionId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
          <h3 className="font-semibold text-white">{definition.name}</h3>
          <p className="mt-1 text-xs text-slate-400">{definition.definitionKey}</p>
        </div>
      )),
    },
    {
      title: 'Versions',
      icon: <History className="h-5 w-5" />,
      content: listPanel(versions.slice(0, 4), 'No program versions loaded.', (version) => (
        <div key={version.programVersionId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <h3 className="font-semibold text-white">Version {version.versionNumber}</h3>
              <p className="mt-1 text-xs text-slate-400">{formatDate(version.publishedAt ?? version.createdAt)}</p>
            </div>
            <DetailBadge label={humanize(version.status)} tone={statusTone(version.status)} />
          </div>
        </div>
      )),
    },
  ]

  return (
    <ProfileDetailsLayout
      testId="trainarr-program-profile"
      backLabel="Programs"
      backTo="/programs/drawer"
      breadcrumbs={[program.programKey, program.name]}
      icon={<GraduationCap className="h-9 w-9" />}
      title={program.name}
      subtitle={<span>{'description' in program ? program.description : 'Training program'} - {definitionCount} definitions</span>}
      badges={[
        { label: program.programKey.toUpperCase(), tone: 'info' },
        { label: humanize(program.status), tone: statusTone(program.status) },
        { label: `${publishedVersionCount} published versions`, tone: 'neutral' },
      ]}
      actions={<>{actionLink('/programs/drawer', 'Edit program', <Pencil className="h-4 w-4" />)}</>}
      metrics={[
        { label: 'Program state', value: humanize(program.status), hint: `${publishedVersionCount} published versions`, icon: <ShieldCheck className="h-5 w-5" />, tone: statusTone(program.status) },
        { label: 'Definitions', value: definitionCount, hint: 'Linked training definitions', icon: <BookOpen className="h-5 w-5" />, tone: definitionCount > 0 ? 'good' : 'warn' },
        { label: 'Matrix coverage', value: matrixEntries.length, hint: 'Applicability entries', icon: <Layers className="h-5 w-5" />, tone: 'info' },
        { label: 'Rule packs', value: rulePackRequirements.length, hint: 'Compliance Core links', icon: <FileText className="h-5 w-5" />, tone: rulePackRequirements.length > 0 ? 'good' : 'neutral' },
      ]}
      tabs={['Overview', 'Definitions', 'Versions', 'Matrix', 'Rule Packs', 'Assignments', 'History']}
      snapshotTitle="Training program snapshot"
      snapshotSubtitle="Program identity, lifecycle state, definition links, published versions, applicability, and compliance references."
      snapshotFields={[
        { label: 'Program ID', value: program.programId, source: 'TrainArr source of truth' },
        { label: 'Program key', value: program.programKey, source: 'Program registry' },
        { label: 'Description', value: 'description' in program ? program.description : 'Not loaded', source: 'Program profile' },
        { label: 'Status', value: humanize(program.status), source: 'Lifecycle state' },
        { label: 'Definitions', value: definitionCount, source: 'Program builder' },
        { label: 'Published versions', value: publishedVersionCount, source: 'Version history' },
        { label: 'Matrix entries', value: matrixEntries.length, source: 'Training matrix' },
        { label: 'Requirements', value: requirements.length, source: 'Applicability builder' },
        { label: 'Updated', value: formatDate(program.updatedAt), source: 'Audit trail' },
      ]}
      mainContent={(
        <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
          <h3 className="text-lg font-bold text-white">Applicability and requirements</h3>
          <div className="mt-4">
            {listPanel(requirements.slice(0, 5), 'No applicability requirements mapped yet.', (requirement) => (
              <div key={requirement.requirementId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <h4 className="font-semibold text-white">{requirement.label}</h4>
                    <p className="mt-1 text-sm text-sky-100/75">{humanize(requirement.requirementSource)}</p>
                  </div>
                  <DetailBadge label={humanize(requirement.status)} tone={statusTone(requirement.status)} />
                </div>
              </div>
            ))}
          </div>
        </section>
      )}
      decisionTitle="Program decision"
      decisionBadge={{ label: blocked ? 'Setup needed' : 'Ready', tone: blocked ? 'warn' : 'good' }}
      decisionIcon={blocked ? <AlertTriangle className="h-5 w-5 text-amber-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={blocked ? 'Program needs definition or lifecycle work' : 'Program ready for assignment'}
      decisionDetail={blocked ? 'Inactive programs or programs without linked definitions should be completed before assignment.' : 'Program state, definitions, and compliance links support normal training assignments.'}
      allowedChecks={[program.status === 'active', definitionCount > 0, publishedVersionCount > 0].filter(Boolean).length}
      blockedChecks={[program.status !== 'active', definitionCount === 0].filter(Boolean).length}
      railSections={rails}
    />
  )
}

export function RulePackProfile({ state: s }: { state: TrainArrWorkspaceState }) {
  const definitionRequirements = s.definitionRulePackRequirementsQuery?.data ?? []
  const programRequirements = s.programRulePackRequirementsQuery?.data ?? []
  const allRequirements = [...definitionRequirements, ...programRequirements]
  const rulePackKey =
    s.impactRulePackKeyInput?.trim() ||
    s.rulePackKeyInput?.trim() ||
    allRequirements[0]?.rulePackKey ||
    'driver_qualification'
  const matchingRequirements = allRequirements.filter((requirement) => requirement.rulePackKey === rulePackKey)
  const metadata = matchingRequirements.find((requirement) => requirement.metadata)?.metadata ?? null
  const impact = s.rulePackImpactAssessment
  const hasDrift = Boolean(impact?.summary.hasDrift || impact?.summary.requiresAttention)
  const blocked = hasDrift || (metadata ? !metadata.isActive : false)

  return (
    <ProfileDetailsLayout
      testId="trainarr-rule-pack-profile"
      backLabel="Rule packs"
      backTo="/rule-packs/drawer"
      breadcrumbs={[rulePackKey, metadata?.label ?? 'Rule pack']}
      icon={<FileText className="h-9 w-9" />}
      title={metadata?.label ?? rulePackKey}
      subtitle={<span>{metadata?.regulatoryProgramLabel ?? 'Compliance Core rule pack'} - {matchingRequirements.length} requirements</span>}
      badges={[
        { label: rulePackKey, tone: 'info' },
        { label: metadata ? humanize(metadata.status) : 'Unvalidated', tone: statusTone(metadata?.status) },
        { label: impact ? `Assessed ${formatDate(impact.assessedAt)}` : 'No impact run', tone: impact ? 'neutral' : 'warn' },
      ]}
      actions={<>{actionLink('/rule-packs/drawer', 'Edit requirements', <Pencil className="h-4 w-4" />)}</>}
      metrics={[
        { label: 'Requirement links', value: matchingRequirements.length, hint: 'Definition and program links', icon: <FileText className="h-5 w-5" />, tone: matchingRequirements.length > 0 ? 'good' : 'warn' },
        { label: 'Definitions', value: impact?.summary.definitionCount ?? definitionRequirements.length, hint: 'Affected definitions', icon: <BookOpen className="h-5 w-5" />, tone: 'info' },
        { label: 'Programs', value: impact?.summary.programCount ?? programRequirements.length, hint: 'Affected programs', icon: <GraduationCap className="h-5 w-5" />, tone: 'info' },
        { label: 'Drift', value: hasDrift ? 'Yes' : 'No', hint: 'Version or status drift', icon: <RefreshCw className="h-5 w-5" />, tone: hasDrift ? 'warn' : 'good' },
      ]}
      tabs={['Overview', 'Requirements', 'Impact', 'Definitions', 'Programs', 'Assignments', 'History']}
      snapshotTitle="Rule pack snapshot"
      snapshotSubtitle="Compliance Core rule-pack metadata, TrainArr requirement links, impact drift, and affected training records."
      snapshotFields={[
        { label: 'Rule pack key', value: rulePackKey, source: 'Compliance Core reference' },
        { label: 'Label', value: metadata?.label ?? impact?.currentState?.label ?? 'Not validated', source: 'Compliance Core metadata' },
        { label: 'Regulatory program', value: metadata?.regulatoryProgramLabel ?? impact?.currentState?.regulatoryProgramLabel ?? 'Not recorded', source: 'Compliance Core metadata' },
        { label: 'Version', value: metadata?.versionNumber ?? impact?.currentState?.versionNumber ?? 'Not recorded', source: 'Rule-pack version' },
        { label: 'Status', value: humanize(metadata?.status ?? impact?.currentState?.status), source: 'Rule-pack status' },
        { label: 'Requirement links', value: matchingRequirements.length, source: 'TrainArr requirements' },
        { label: 'Affected assignments', value: impact?.summary.activeAssignmentCount ?? 0, source: 'Impact assessment' },
        { label: 'Affected qualifications', value: impact?.summary.activeQualificationCount ?? 0, source: 'Impact assessment' },
        { label: 'Assessed', value: formatDate(impact?.assessedAt), source: 'Impact run' },
      ]}
      mainContent={(
        <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
          <h3 className="text-lg font-bold text-white">Recommended actions</h3>
          <div className="mt-4">
            {listPanel(impact?.recommendedActions.slice(0, 5) ?? [], 'No impact recommendations loaded.', (action) => (
              <div key={`${action.actionType}-${action.entityId ?? action.message}`} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <h4 className="font-semibold text-white">{humanize(action.actionType)}</h4>
                    <p className="mt-1 text-sm text-sky-100/75">{action.message}</p>
                  </div>
                  <DetailBadge label={humanize(action.priority)} tone={statusTone(action.priority)} />
                </div>
              </div>
            ))}
          </div>
        </section>
      )}
      decisionTitle="Rule-pack decision"
      decisionBadge={{ label: blocked ? 'Review' : 'Current', tone: blocked ? 'warn' : 'good' }}
      decisionIcon={blocked ? <XCircle className="h-5 w-5 text-amber-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={blocked ? 'Rule-pack links need review' : 'Rule-pack links are current'}
      decisionDetail={blocked ? 'Inactive metadata or impact drift should be reviewed before relying on downstream qualification checks.' : 'Rule-pack metadata and requirement links support normal qualification enforcement.'}
      allowedChecks={[matchingRequirements.length > 0, !hasDrift, metadata?.isActive !== false].filter(Boolean).length}
      blockedChecks={[matchingRequirements.length === 0, hasDrift, metadata?.isActive === false].filter(Boolean).length}
      railSections={[
        {
          title: 'Requirement links',
          icon: <ClipboardCheck className="h-5 w-5" />,
          content: listPanel(matchingRequirements.slice(0, 5), 'No TrainArr requirements currently reference this rule pack.', (requirement) => (
            <div key={requirement.requirementId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="font-semibold text-white">{humanize(requirement.entityType)}</h3>
                  <p className="mt-1 text-xs text-slate-400">{requirement.entityId}</p>
                </div>
                <DetailBadge label={requirement.metadata ? 'Validated' : 'Pending'} tone={requirement.metadata ? 'good' : 'warn'} />
              </div>
            </div>
          )),
        },
      ]}
    />
  )
}
