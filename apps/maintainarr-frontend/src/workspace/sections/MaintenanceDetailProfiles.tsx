import {
  Activity,
  AlertTriangle,
  BadgeCheck,
  CalendarClock,
  CheckCircle2,
  ClipboardCheck,
  FileText,
  Gauge,
  History,
  ListChecks,
  Pencil,
  Play,
  Route,
  ShieldCheck,
  Wrench,
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
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

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
  if (['active', 'completed', 'closed', 'resolved', 'pass', 'passed', 'current'].includes(normalized)) return 'good'
  if (['due', 'open', 'in_progress', 'pending', 'draft', 'medium'].includes(normalized)) return 'warn'
  if (['overdue', 'failed', 'critical', 'high', 'cancelled', 'blocked'].includes(normalized)) return 'bad'
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

function listPanel<T>({
  items,
  emptyText,
  render,
}: {
  items: T[]
  emptyText: string
  render: (item: T, index: number) => ReactNode
}) {
  if (items.length === 0) return <DetailEmptyState text={emptyText} />
  return <div className="space-y-3">{items.map(render)}</div>
}

function noSelection(title: string, text: string, to: string) {
  return (
    <div className="rounded-3xl border border-slate-800 bg-slate-950/70 p-8 text-center">
      <Wrench className="mx-auto h-10 w-10 text-sky-300" />
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

export function PmProgramProfile({ state: s }: { state: MaintainArrWorkspaceState }) {
  const programs = s.pmProgramsQuery?.data ?? []
  const detail = s.pmProgramDetailQuery?.data ?? null
  const summary = programs.find((program) => program.pmProgramId === s.selectedProgramId) ?? programs[0] ?? null
  const program = detail ?? summary
  if (!program) {
    return noSelection('No PM program selected', 'Create or select a PM program to view its detail profile.', '/pm-programs/drawer')
  }

  const schedules = detail?.schedules ?? []
  const scheduleCount = detail?.schedules.length ?? summary?.scheduleCount ?? 0
  const dueSchedules = schedules.filter((schedule) => ['due', 'overdue'].includes(schedule.dueStatus))
  const activeSchedules = schedules.filter((schedule) => schedule.status === 'active')
  const scopeLabel = detail?.assetName ?? detail?.assetTypeName ?? summary?.assetTag ?? summary?.assetTypeName ?? 'Not scoped'
  const blocked = program.status !== 'active' || dueSchedules.some((schedule) => schedule.dueStatus === 'overdue')
  const rails: DetailRailSectionConfig[] = [
    {
      title: 'Linked schedules',
      icon: <ListChecks className="h-5 w-5" />,
      content: listPanel({
        items: schedules.slice(0, 4),
        emptyText: 'No schedules linked yet.',
        render: (schedule) => (
          <div key={schedule.pmScheduleId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
            <div className="flex items-start justify-between gap-3">
              <div>
                <h3 className="font-semibold text-white">{schedule.name}</h3>
                <p className="mt-1 text-xs text-slate-400">{schedule.assetTag} - {humanize(schedule.status)}</p>
              </div>
              <DetailBadge label={humanize(schedule.dueStatus)} tone={statusTone(schedule.dueStatus)} />
            </div>
          </div>
        ),
      }),
    },
    {
      title: 'Recent activity',
      icon: <History className="h-5 w-5" />,
      content: (
        <div className="space-y-3 text-sm text-slate-300">
          <p>Created {formatDate(program.createdAt)}</p>
          <p>Updated {formatDate(program.updatedAt)}</p>
        </div>
      ),
    },
  ]

  return (
    <ProfileDetailsLayout
      testId="maintainarr-pm-program-profile"
      backLabel="PM programs"
      backTo="/pm-programs/drawer"
      breadcrumbs={[humanize(program.scopeType), program.name]}
      icon={<Route className="h-9 w-9" />}
      title={program.name}
      subtitle={<span>{scopeLabel} - Preventive maintenance program</span>}
      badges={[
        { label: program.programKey.toUpperCase(), tone: 'info' },
        { label: humanize(program.status), tone: statusTone(program.status) },
        { label: `${scheduleCount} schedules`, tone: 'neutral' },
      ]}
      actions={<>{actionLink('/pm-programs/drawer', 'Edit program', <Pencil className="h-4 w-4" />)}</>}
      metrics={[
        { label: 'Program state', value: humanize(program.status), hint: humanize(program.scopeType), icon: <ShieldCheck className="h-5 w-5" />, tone: statusTone(program.status) },
        { label: 'Schedules', value: scheduleCount, hint: `${activeSchedules.length} active`, icon: <ListChecks className="h-5 w-5" />, tone: 'info' },
        { label: 'Due items', value: dueSchedules.length, hint: 'Due or overdue schedules', icon: <CalendarClock className="h-5 w-5" />, tone: dueSchedules.length > 0 ? 'warn' : 'good' },
        { label: 'Scope', value: humanize(program.scopeType), hint: scopeLabel, icon: <Route className="h-5 w-5" />, tone: 'neutral' },
      ]}
      tabs={['Overview', 'Schedules', 'Assets', 'Work Orders', 'Compliance', 'History']}
      snapshotTitle="PM program snapshot"
      snapshotSubtitle="Program identity, schedule coverage, scope, lifecycle state, and operating references."
      snapshotFields={[
        { label: 'Program ID', value: program.pmProgramId, source: 'MaintainArr source of truth' },
        { label: 'Program key', value: program.programKey, source: 'Program registry' },
        { label: 'Description', value: detail?.description ?? 'Not recorded', source: 'Program profile' },
        { label: 'Scope type', value: humanize(program.scopeType), source: 'Program scope' },
        { label: 'Asset type', value: detail?.assetTypeName ?? summary?.assetTypeName ?? 'Not recorded', source: 'Asset catalog' },
        { label: 'Asset', value: detail?.assetName ?? summary?.assetTag ?? 'Not recorded', source: 'Asset registry' },
        { label: 'Status', value: humanize(program.status), source: 'Lifecycle state' },
        { label: 'Created', value: formatDate(program.createdAt), source: 'Audit trail' },
        { label: 'Updated', value: formatDate(program.updatedAt), source: 'Audit trail' },
      ]}
      mainContent={(
        <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
          <h3 className="text-lg font-bold text-white">Schedule coverage</h3>
          <div className="mt-4">
            {listPanel({
              items: schedules.slice(0, 5),
              emptyText: 'Select a program with linked schedules to review coverage.',
              render: (schedule) => (
                <div key={schedule.pmScheduleId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <h4 className="font-semibold text-white">{schedule.name}</h4>
                      <p className="mt-1 text-sm text-sky-100/75">{schedule.assetName}</p>
                    </div>
                    <DetailBadge label={humanize(schedule.dueStatus)} tone={statusTone(schedule.dueStatus)} />
                  </div>
                </div>
              ),
            })}
          </div>
        </section>
      )}
      decisionTitle="Program decision"
      decisionBadge={{ label: blocked ? 'Attention' : 'Ready', tone: blocked ? 'warn' : 'good' }}
      decisionIcon={blocked ? <AlertTriangle className="h-5 w-5 text-amber-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={blocked ? 'Program needs schedule attention' : 'Program ready for PM compliance'}
      decisionDetail={blocked ? 'Inactive status or due schedules require maintenance planning before this program is fully compliant.' : 'Active program state and linked schedules support normal PM execution.'}
      allowedChecks={[program.status === 'active', schedules.length > 0, dueSchedules.length === 0].filter(Boolean).length}
      blockedChecks={[program.status !== 'active', dueSchedules.length > 0].filter(Boolean).length}
      railSections={rails}
    />
  )
}

export function MeterProfile({ state: s }: { state: MaintainArrWorkspaceState }) {
  const meters = s.assetMetersQuery?.data ?? []
  const meter = meters.find((item) => item.assetMeterId === s.selectedMeterId) ?? meters[0] ?? null
  if (!meter) {
    return noSelection('No meter selected', 'Select an asset meter to view readings, forecasts, and PM linkage.', '/meters/drawer')
  }

  const readings = s.meterReadingsQuery?.data ?? []
  const forecast = s.meterForecastQuery?.data ?? null
  const dueForecasts = forecast?.linkedSchedules.filter((item) => item.dueStatus === 'due' || item.isDueFromUsage) ?? []

  return (
    <ProfileDetailsLayout
      testId="maintainarr-meter-profile"
      backLabel="Meters"
      backTo="/meters/drawer"
      breadcrumbs={[meter.assetTag, meter.name]}
      icon={<Gauge className="h-9 w-9" />}
      title={meter.name}
      subtitle={<span>{meter.assetName} - {meter.unit}</span>}
      badges={[
        { label: meter.meterKey.toUpperCase(), tone: 'info' },
        { label: humanize(meter.status), tone: statusTone(meter.status) },
      ]}
      actions={<>{actionLink('/meters/drawer', 'Record reading', <Pencil className="h-4 w-4" />, true)}</>}
      metrics={[
        { label: 'Current reading', value: `${meter.currentReading} ${meter.unit}`, hint: `Baseline ${meter.baselineReading}`, icon: <Gauge className="h-5 w-5" />, tone: 'info' },
        { label: 'Readings', value: readings.length, hint: `Last ${formatDate(meter.lastReadingAt)}`, icon: <Activity className="h-5 w-5" />, tone: 'neutral' },
        { label: 'PM links', value: forecast?.linkedSchedules.length ?? 0, hint: 'Usage-based schedules', icon: <ListChecks className="h-5 w-5" />, tone: 'neutral' },
        { label: 'Due from usage', value: dueForecasts.length, hint: 'Forecasted due items', icon: <CalendarClock className="h-5 w-5" />, tone: dueForecasts.length > 0 ? 'warn' : 'good' },
      ]}
      tabs={['Overview', 'Readings', 'Forecast', 'PM Links', 'History']}
      snapshotTitle="Meter snapshot"
      snapshotSubtitle="Meter identity, current operating value, reading cadence, and PM forecast context."
      snapshotFields={[
        { label: 'Meter ID', value: meter.assetMeterId, source: 'MaintainArr source of truth' },
        { label: 'Asset', value: `${meter.assetTag} - ${meter.assetName}`, source: 'Asset registry' },
        { label: 'Meter key', value: meter.meterKey, source: 'Meter registry' },
        { label: 'Unit', value: meter.unit, source: 'Meter definition' },
        { label: 'Baseline', value: meter.baselineReading, source: 'Meter definition' },
        { label: 'Current reading', value: meter.currentReading, source: 'Latest reading' },
        { label: 'Last reading', value: formatDate(meter.lastReadingAt), source: 'Reading history' },
        { label: 'Status', value: humanize(meter.status), source: 'Lifecycle state' },
        { label: 'Updated', value: formatDate(meter.updatedAt), source: 'Audit trail' },
      ]}
      mainContent={(
        <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
          <h3 className="text-lg font-bold text-white">Recent readings</h3>
          <div className="mt-4">
            {listPanel({
              items: readings.slice(0, 5),
              emptyText: 'No readings recorded for this meter yet.',
              render: (reading) => (
                <div key={reading.meterReadingId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <h4 className="font-semibold text-white">{reading.readingValue} {meter.unit}</h4>
                      <p className="mt-1 text-sm text-sky-100/75">{reading.notes || 'No notes'}</p>
                    </div>
                    <span className="text-xs text-slate-400">{formatDate(reading.readAt)}</span>
                  </div>
                </div>
              ),
            })}
          </div>
        </section>
      )}
      decisionTitle="Meter decision"
      decisionBadge={{ label: dueForecasts.length > 0 ? 'Due watch' : 'Current', tone: dueForecasts.length > 0 ? 'warn' : 'good' }}
      decisionIcon={dueForecasts.length > 0 ? <AlertTriangle className="h-5 w-5 text-amber-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={dueForecasts.length > 0 ? 'Usage threshold approaching' : 'Meter readings support PM forecast'}
      decisionDetail={dueForecasts.length > 0 ? 'One or more linked schedules are due from usage and should be reviewed.' : 'Latest reading and forecast data support normal PM planning.'}
      allowedChecks={[meter.status === 'active', readings.length > 0, dueForecasts.length === 0].filter(Boolean).length}
      blockedChecks={[meter.status !== 'active', dueForecasts.length > 0].filter(Boolean).length}
      railSections={[
        {
          title: 'Linked PM forecast',
          icon: <CalendarClock className="h-5 w-5" />,
          content: listPanel({
            items: forecast?.linkedSchedules.slice(0, 4) ?? [],
            emptyText: 'No PM schedules linked to this meter.',
            render: (item) => (
              <div key={item.pmScheduleId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <h3 className="font-semibold text-white">{item.name}</h3>
                    <p className="mt-1 text-xs text-slate-400">{item.usageUntilDue ?? 0} {meter.unit} until due</p>
                  </div>
                  <DetailBadge label={humanize(item.dueStatus)} tone={statusTone(item.dueStatus)} />
                </div>
              </div>
            ),
          }),
        },
      ]}
    />
  )
}

export function WorkOrderProfile({ state: s }: { state: MaintainArrWorkspaceState }) {
  const detail = s.workOrderDetailQuery?.data ?? null
  const summary = (s.workOrdersQuery?.data ?? []).find((order) => order.workOrderId === s.selectedWorkOrderId)
    ?? (s.workOrdersQuery?.data ?? [])[0]
    ?? null
  const order = detail ?? summary
  if (!order) {
    return noSelection('No work order selected', 'Select a work order to view execution, labor, evidence, and supply readiness.', '/work-orders/drawer')
  }

  const tasks = s.workOrderTasksQuery?.data ?? []
  const labor = s.workOrderLaborQuery?.data ?? []
  const evidence = s.workOrderEvidenceQuery?.data ?? []
  const partsDemand = s.workOrderPartsDemandQuery?.data ?? []
  const blockers = s.workOrderSupplyReadinessQuery?.data?.lines.flatMap((line) => line.blockers) ?? []
  const laborHours = labor.reduce((total, entry) => total + entry.hoursWorked, 0)
  const blocked = blockers.length > 0 || ['cancelled'].includes(order.status)

  return (
    <ProfileDetailsLayout
      testId="maintainarr-work-order-profile"
      backLabel="Work orders"
      backTo="/work-orders/drawer"
      breadcrumbs={[order.assetTag, order.workOrderNumber]}
      icon={<Wrench className="h-9 w-9" />}
      title={order.title}
      subtitle={<span>{order.assetName} - {humanize(order.priority)} priority</span>}
      badges={[
        { label: order.workOrderNumber, tone: 'info' },
        { label: humanize(order.status), tone: statusTone(order.status) },
        { label: humanize(order.source), tone: 'neutral' },
      ]}
      actions={<>{actionLink(`/work-orders/${order.workOrderId}`, 'Open workspace', <Play className="h-4 w-4" />, true)}</>}
      metrics={[
        { label: 'Status', value: humanize(order.status), hint: `Source ${humanize(order.source)}`, icon: <ShieldCheck className="h-5 w-5" />, tone: statusTone(order.status) },
        { label: 'Tasks', value: tasks.length, hint: 'Execution checklist', icon: <ListChecks className="h-5 w-5" />, tone: 'neutral' },
        { label: 'Labor', value: `${laborHours.toFixed(1)}h`, hint: 'Logged technician time', icon: <Activity className="h-5 w-5" />, tone: 'info' },
        { label: 'Parts demand', value: partsDemand.length, hint: `${blockers.length} supply blockers`, icon: <AlertTriangle className="h-5 w-5" />, tone: blockers.length > 0 ? 'warn' : 'good' },
      ]}
      tabs={['Overview', 'Tasks', 'Labor', 'Parts', 'Evidence', 'History']}
      snapshotTitle="Work order snapshot"
      snapshotSubtitle="Work order identity, asset context, assignment, source, and execution lifecycle."
      snapshotFields={[
        { label: 'Work order ID', value: order.workOrderId, source: 'MaintainArr source of truth' },
        { label: 'Number', value: order.workOrderNumber, source: 'Work order registry' },
        { label: 'Asset', value: `${order.assetTag} - ${order.assetName}`, source: 'Asset registry' },
        { label: 'Priority', value: humanize(order.priority), source: 'Planner input' },
        { label: 'Status', value: humanize(order.status), source: 'Lifecycle state' },
        { label: 'Assigned technician', value: order.assignedTechnicianPersonId ?? 'Unassigned', source: 'StaffArr personId' },
        { label: 'Defect', value: detail?.defectTitle ?? order.defectId ?? 'Not linked', source: 'Defect linkage' },
        { label: 'PM schedule', value: detail?.pmScheduleName ?? order.pmScheduleId ?? 'Not linked', source: 'PM linkage' },
        { label: 'Created', value: formatDate(order.createdAt), source: 'Audit trail' },
      ]}
      mainContent={(
        <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
          <h3 className="text-lg font-bold text-white">Execution tasks</h3>
          <div className="mt-4">
            {listPanel({
              items: tasks.slice(0, 5),
              emptyText: 'No task lines logged yet.',
              render: (task) => (
                <div key={task.taskLineId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <h4 className="font-semibold text-white">{task.title}</h4>
                    <DetailBadge label={humanize(task.status)} tone={statusTone(task.status)} />
                  </div>
                </div>
              ),
            })}
          </div>
        </section>
      )}
      decisionTitle="Work order decision"
      decisionBadge={{ label: blocked ? 'Attention' : 'Executable', tone: blocked ? 'warn' : 'good' }}
      decisionIcon={blocked ? <AlertTriangle className="h-5 w-5 text-amber-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={blocked ? 'Execution needs attention' : 'Work order can proceed'}
      decisionDetail={blocked ? 'Supply blockers or lifecycle state require planner review before normal execution.' : 'Assignment, task, and supply checks support normal work order execution.'}
      allowedChecks={[order.status !== 'cancelled', blockers.length === 0, tasks.length > 0].filter(Boolean).length}
      blockedChecks={[order.status === 'cancelled', blockers.length > 0].filter(Boolean).length}
      railSections={[
        {
          title: 'Evidence',
          icon: <FileText className="h-5 w-5" />,
          content: listPanel({
            items: evidence.slice(0, 4),
            emptyText: 'No evidence uploaded yet.',
            render: (item) => (
              <div key={item.evidenceId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <h3 className="font-semibold text-white">{item.fileName}</h3>
                <p className="mt-1 text-xs text-slate-400">{humanize(item.evidenceTypeKey)} - {formatDate(item.createdAt)}</p>
              </div>
            ),
          }),
        },
      ]}
    />
  )
}

export function DefectProfile({ state: s }: { state: MaintainArrWorkspaceState }) {
  const defect = (s.defectsQuery?.data ?? []).find((item) => item.defectId === s.selectedDefectId)
    ?? (s.defectsQuery?.data ?? [])[0]
    ?? null
  if (!defect) {
    return noSelection('No defect selected', 'Select a defect to review severity, evidence, work order linkage, and readiness impact.', '/defects/drawer')
  }

  const evidence = s.defectEvidenceQuery?.data ?? []
  const blocked = ['critical', 'high'].includes(defect.severity) && !['resolved', 'closed'].includes(defect.status)

  return (
    <ProfileDetailsLayout
      testId="maintainarr-defect-profile"
      backLabel="Defects"
      backTo="/defects/drawer"
      breadcrumbs={[defect.assetTag, defect.title]}
      icon={<AlertTriangle className="h-9 w-9" />}
      title={defect.title}
      subtitle={<span>{defect.assetName} - {humanize(defect.source)}</span>}
      badges={[
        { label: humanize(defect.severity), tone: statusTone(defect.severity) },
        { label: humanize(defect.status), tone: statusTone(defect.status) },
      ]}
      actions={<>{actionLink('/defects/drawer', 'Create WO', <Wrench className="h-4 w-4" />, true)}</>}
      metrics={[
        { label: 'Severity', value: humanize(defect.severity), hint: 'Defect classification', icon: <AlertTriangle className="h-5 w-5" />, tone: statusTone(defect.severity) },
        { label: 'Status', value: humanize(defect.status), hint: `Source ${humanize(defect.source)}`, icon: <ShieldCheck className="h-5 w-5" />, tone: statusTone(defect.status) },
        { label: 'Evidence', value: defect.evidenceCount, hint: `${evidence.length} loaded`, icon: <FileText className="h-5 w-5" />, tone: 'neutral' },
        { label: 'Age', value: formatDate(defect.createdAt), hint: `Updated ${formatDate(defect.updatedAt)}`, icon: <CalendarClock className="h-5 w-5" />, tone: 'neutral' },
      ]}
      tabs={['Overview', 'Evidence', 'Work Orders', 'Inspections', 'History']}
      snapshotTitle="Defect snapshot"
      snapshotSubtitle="Defect identity, asset context, severity, source, evidence, and resolution posture."
      snapshotFields={[
        { label: 'Defect ID', value: defect.defectId, source: 'MaintainArr source of truth' },
        { label: 'Asset', value: `${defect.assetTag} - ${defect.assetName}`, source: 'Asset registry' },
        { label: 'Checklist item', value: defect.checklistItemKey ?? 'Not linked', source: 'Inspection source' },
        { label: 'Severity', value: humanize(defect.severity), source: 'Reporter input' },
        { label: 'Status', value: humanize(defect.status), source: 'Lifecycle state' },
        { label: 'Source', value: humanize(defect.source), source: 'Defect origin' },
        { label: 'Reported by', value: defect.reportedByUserId, source: 'User audit' },
        { label: 'Created', value: formatDate(defect.createdAt), source: 'Audit trail' },
        { label: 'Resolved', value: formatDate(defect.resolvedAt), source: 'Resolution record' },
      ]}
      mainContent={(
        <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
          <h3 className="text-lg font-bold text-white">Evidence package</h3>
          <div className="mt-4">
            {listPanel({
              items: evidence.slice(0, 5),
              emptyText: 'No evidence uploaded for this defect.',
              render: (item) => (
                <div key={item.evidenceId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                  <h4 className="font-semibold text-white">{item.fileName}</h4>
                  <p className="mt-1 text-sm text-sky-100/75">{item.notes ?? humanize(item.evidenceTypeKey)}</p>
                </div>
              ),
            })}
          </div>
        </section>
      )}
      decisionTitle="Defect decision"
      decisionBadge={{ label: blocked ? 'Repair required' : 'Monitor', tone: blocked ? 'bad' : 'warn' }}
      decisionIcon={blocked ? <XCircle className="h-5 w-5 text-red-300" /> : <AlertTriangle className="h-5 w-5 text-amber-300" />}
      decisionSummary={blocked ? 'Defect may block dispatch' : 'Defect can be monitored'}
      decisionDetail={blocked ? 'High or critical open defect should be repaired or converted to a work order before normal use.' : 'Current defect status supports monitoring and normal maintenance triage.'}
      allowedChecks={[!blocked, evidence.length > 0, defect.status !== 'open'].filter(Boolean).length}
      blockedChecks={[blocked].filter(Boolean).length}
      railSections={[
        {
          title: 'Linked records',
          icon: <History className="h-5 w-5" />,
          content: (
            <div className="space-y-3 text-sm text-slate-300">
              <p>Inspection run: {defect.inspectionRunId ?? 'Not linked'}</p>
              <p>Checklist: {defect.checklistItemKey ?? 'Not linked'}</p>
            </div>
          ),
        },
      ]}
    />
  )
}

export function InspectionRunProfile({ state: s }: { state: MaintainArrWorkspaceState }) {
  const detail = s.inspectionRunQuery?.data ?? null
  const summary = (s.inspectionRunsQuery?.data ?? []).find((run) => run.inspectionRunId === s.selectedRunId)
    ?? (s.inspectionRunsQuery?.data ?? [])[0]
    ?? null
  const run = detail ?? summary
  if (!run) {
    return noSelection('No inspection selected', 'Select an inspection run to review answers, evidence, defects, and completion state.', '/inspections/drawer')
  }

  const answers = detail?.answers ?? []
  const items = detail?.checklistItems ?? []
  const answerCount = detail?.answers.length ?? summary?.answerCount ?? 0
  const requiredItemCount = detail?.checklistItems.filter((item) => item.isRequired).length ?? summary?.requiredItemCount ?? 0
  const evidence = s.inspectionRunEvidenceQuery?.data ?? []
  const failedAnswers = answers.filter((answer) => answer.passFailValue === 'fail')
  const blocked = run.result === 'fail' || failedAnswers.length > 0

  return (
    <ProfileDetailsLayout
      testId="maintainarr-inspection-profile"
      backLabel="Inspections"
      backTo="/inspections/drawer"
      breadcrumbs={[run.assetTag, run.templateName]}
      icon={<ClipboardCheck className="h-9 w-9" />}
      title={run.templateName}
      subtitle={<span>{run.assetName} - Version {run.templateVersion}</span>}
      badges={[
        { label: humanize(run.status), tone: statusTone(run.status) },
        { label: humanize(run.result), tone: statusTone(run.result) },
      ]}
      actions={<>{actionLink('/inspections/drawer', 'Continue run', <Play className="h-4 w-4" />, true)}</>}
      metrics={[
        { label: 'Inspection state', value: humanize(run.status), hint: `Result ${humanize(run.result)}`, icon: <ShieldCheck className="h-5 w-5" />, tone: statusTone(run.result ?? run.status) },
        { label: 'Answers', value: answerCount, hint: `${requiredItemCount} required`, icon: <ListChecks className="h-5 w-5" />, tone: 'info' },
        { label: 'Failures', value: failedAnswers.length, hint: 'Failed answers', icon: <AlertTriangle className="h-5 w-5" />, tone: failedAnswers.length > 0 ? 'bad' : 'good' },
        { label: 'Evidence', value: evidence.length, hint: 'Uploaded files', icon: <FileText className="h-5 w-5" />, tone: 'neutral' },
      ]}
      tabs={['Overview', 'Checklist', 'Answers', 'Defects', 'Evidence', 'History']}
      snapshotTitle="Inspection snapshot"
      snapshotSubtitle="Run identity, asset, template version, checklist completion, result, and evidence context."
      snapshotFields={[
        { label: 'Run ID', value: run.inspectionRunId, source: 'MaintainArr source of truth' },
        { label: 'Asset', value: `${run.assetTag} - ${run.assetName}`, source: 'Asset registry' },
        { label: 'Template', value: run.templateName, source: 'Inspection template' },
        { label: 'Version', value: run.templateVersion, source: 'Template snapshot' },
        { label: 'Status', value: humanize(run.status), source: 'Lifecycle state' },
        { label: 'Result', value: humanize(run.result), source: 'Inspection outcome' },
        { label: 'Started by', value: run.startedByUserId, source: 'User audit' },
        { label: 'Started', value: formatDate(run.startedAt), source: 'Audit trail' },
        { label: 'Completed', value: formatDate(run.completedAt), source: 'Audit trail' },
      ]}
      mainContent={(
        <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
          <h3 className="text-lg font-bold text-white">Checklist answers</h3>
          <div className="mt-4">
            {listPanel({
              items: answers.slice(0, 5),
              emptyText: 'No answers loaded for this run.',
              render: (answer) => (
                <div key={answer.answerId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <h4 className="font-semibold text-white">{answer.itemKey}</h4>
                      <p className="mt-1 text-sm text-sky-100/75">{answer.textValue ?? answer.numericValue ?? 'Recorded answer'}</p>
                    </div>
                    <DetailBadge label={humanize(answer.passFailValue)} tone={statusTone(answer.passFailValue)} />
                  </div>
                </div>
              ),
            })}
          </div>
        </section>
      )}
      decisionTitle="Inspection decision"
      decisionBadge={{ label: blocked ? 'Failed' : 'Pass', tone: blocked ? 'bad' : 'good' }}
      decisionIcon={blocked ? <XCircle className="h-5 w-5 text-red-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={blocked ? 'Inspection requires defect follow-up' : 'Inspection supports normal operation'}
      decisionDetail={blocked ? 'Failed answers should be reviewed and converted to defects when appropriate.' : 'Completed or passing inspection data supports readiness records.'}
      allowedChecks={[run.status === 'completed', !blocked, answerCount >= requiredItemCount].filter(Boolean).length}
      blockedChecks={[blocked].filter(Boolean).length}
      railSections={[
        {
          title: 'Checklist items',
          icon: <ListChecks className="h-5 w-5" />,
          content: listPanel({
            items: items.slice(0, 4),
            emptyText: 'Checklist snapshot not loaded.',
            render: (item) => (
              <div key={item.checklistItemId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <h3 className="font-semibold text-white">{item.prompt}</h3>
                <p className="mt-1 text-xs text-slate-400">{humanize(item.itemType)} - {item.isRequired ? 'Required' : 'Optional'}</p>
              </div>
            ),
          }),
        },
      ]}
    />
  )
}

export function InspectionTemplateProfile({ state: s }: { state: MaintainArrWorkspaceState }) {
  const detail = s.templateDetailQuery?.data ?? null
  const summary = (s.templatesQuery?.data ?? []).find((template) => template.inspectionTemplateId === s.selectedTemplateId)
    ?? (s.templatesQuery?.data ?? [])[0]
    ?? null
  const template = detail ?? summary
  if (!template) {
    return noSelection('No template selected', 'Select an inspection template to view categories, checklist items, asset type coverage, and version state.', '/inspection-templates/drawer')
  }

  const categories = detail?.categories ?? []
  const checklistItems = detail?.checklistItems ?? []
  const linkedAssetTypes = detail?.linkedAssetTypes ?? []
  const requiredItems = checklistItems.filter((item) => item.isRequired)
  const blocked = template.status !== 'active' || checklistItems.length === 0

  return (
    <ProfileDetailsLayout
      testId="maintainarr-template-profile"
      backLabel="Templates"
      backTo="/inspection-templates/drawer"
      breadcrumbs={[template.templateKey, template.name]}
      icon={<BadgeCheck className="h-9 w-9" />}
      title={template.name}
      subtitle={<span>Inspection template - Version {template.version}</span>}
      badges={[
        { label: template.templateKey.toUpperCase(), tone: 'info' },
        { label: humanize(template.status), tone: statusTone(template.status) },
      ]}
      actions={<>{actionLink('/inspection-templates/drawer', 'Edit template', <Pencil className="h-4 w-4" />)}</>}
      metrics={[
        { label: 'Template state', value: humanize(template.status), hint: `Version ${template.version}`, icon: <ShieldCheck className="h-5 w-5" />, tone: statusTone(template.status) },
        { label: 'Categories', value: 'categoryCount' in template ? template.categoryCount : categories.length, hint: 'Checklist grouping', icon: <ListChecks className="h-5 w-5" />, tone: 'neutral' },
        { label: 'Checklist items', value: 'checklistItemCount' in template ? template.checklistItemCount : checklistItems.length, hint: `${requiredItems.length} required`, icon: <ClipboardCheck className="h-5 w-5" />, tone: 'info' },
        { label: 'Asset types', value: 'linkedAssetTypeCount' in template ? template.linkedAssetTypeCount : linkedAssetTypes.length, hint: 'Coverage links', icon: <Route className="h-5 w-5" />, tone: 'neutral' },
      ]}
      tabs={['Overview', 'Categories', 'Checklist', 'Asset Types', 'History']}
      snapshotTitle="Template snapshot"
      snapshotSubtitle="Template identity, version, lifecycle state, checklist composition, and coverage references."
      snapshotFields={[
        { label: 'Template ID', value: template.inspectionTemplateId, source: 'MaintainArr source of truth' },
        { label: 'Template key', value: template.templateKey, source: 'Template registry' },
        { label: 'Description', value: template.description, source: 'Template profile' },
        { label: 'Version', value: template.version, source: 'Template versioning' },
        { label: 'Status', value: humanize(template.status), source: 'Lifecycle state' },
        { label: 'Categories', value: categories.length, source: 'Checklist builder' },
        { label: 'Required items', value: requiredItems.length, source: 'Checklist builder' },
        { label: 'Created', value: formatDate(template.createdAt), source: 'Audit trail' },
        { label: 'Updated', value: formatDate(template.updatedAt), source: 'Audit trail' },
      ]}
      mainContent={(
        <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
          <h3 className="text-lg font-bold text-white">Checklist items</h3>
          <div className="mt-4">
            {listPanel({
              items: checklistItems.slice(0, 5),
              emptyText: 'No checklist items have been added yet.',
              render: (item) => (
                <div key={item.checklistItemId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <h4 className="font-semibold text-white">{item.prompt}</h4>
                      <p className="mt-1 text-sm text-sky-100/75">{item.itemKey} - {humanize(item.itemType)}</p>
                    </div>
                    <DetailBadge label={item.isRequired ? 'Required' : 'Optional'} tone={item.isRequired ? 'warn' : 'neutral'} />
                  </div>
                </div>
              ),
            })}
          </div>
        </section>
      )}
      decisionTitle="Template decision"
      decisionBadge={{ label: blocked ? 'Setup needed' : 'Ready', tone: blocked ? 'warn' : 'good' }}
      decisionIcon={blocked ? <AlertTriangle className="h-5 w-5 text-amber-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={blocked ? 'Template needs activation or checklist work' : 'Template ready for inspections'}
      decisionDetail={blocked ? 'Inactive templates or templates without checklist items should be completed before field use.' : 'Active template state and checklist coverage support normal inspection execution.'}
      allowedChecks={[template.status === 'active', checklistItems.length > 0, linkedAssetTypes.length > 0].filter(Boolean).length}
      blockedChecks={[template.status !== 'active', checklistItems.length === 0].filter(Boolean).length}
      railSections={[
        {
          title: 'Asset type coverage',
          icon: <Route className="h-5 w-5" />,
          content: listPanel({
            items: linkedAssetTypes.slice(0, 4),
            emptyText: 'No asset types linked yet.',
            render: (item) => (
              <div key={item.assetTypeId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <h3 className="font-semibold text-white">{item.typeName}</h3>
                <p className="mt-1 text-xs text-slate-400">{item.className}</p>
              </div>
            ),
          }),
        },
      ]}
    />
  )
}
