import type { AggregatedFieldInboxResponse, FieldInboxTaskItem } from '../api/types'
import { ApiErrorCallout } from '@stl/shared-ui'
import { formatBlockedTaskReason } from '../lib/companionDeniedReasonCatalog'
import { formatWhen, inboxSourceLoadFailures, productLabel, taskTypeLabel } from '../lib/fieldInbox'
import { isMaintainarrInspectionTask, isMaintainarrWorkOrderTask, isRoutarrTripTask, isSupplyarrReceivingTask, isTrainarrFieldTask } from '../lib/evidenceCapture'
import { FieldTaskReceivingPanel } from './FieldTaskReceivingPanel'
import { productLaunchUrl } from '../api/client'
import type { MergedSubmissionChip } from '../lib/submissionState'
import { FieldTaskEvidencePanel } from './FieldTaskEvidencePanel'
import { FieldTaskDvirPanel } from './FieldTaskDvirPanel'
import { FieldTaskInspectionPanel } from './FieldTaskInspectionPanel'
import { FieldTaskWorkOrderPanel } from './FieldTaskWorkOrderPanel'
import { TaskSubmissionStatusBadge } from './TaskSubmissionStatusBadge'

interface FieldInboxPanelProps {
  inbox: AggregatedFieldInboxResponse
  productFilter: string
  onProductFilterChange: (productKey: string) => void
  accessToken: string
  getSubmissionChips?: (taskKey: string) => MergedSubmissionChip[]
  acknowledgedTaskKeys?: ReadonlySet<string>
  onAcknowledgeTask?: (task: FieldInboxTaskItem) => void
  onEvidenceUploadComplete?: () => void
  highlightedTaskKey?: string | null
}

export function FieldInboxPanel({
  inbox,
  productFilter,
  onProductFilterChange,
  accessToken,
  getSubmissionChips,
  acknowledgedTaskKeys,
  onAcknowledgeTask,
  onEvidenceUploadComplete,
  highlightedTaskKey,
}: FieldInboxPanelProps) {
  const filteredItems = productFilter
    ? inbox.items.filter((item) => item.productKey === productFilter)
    : inbox.items

  const productOptions = Object.keys(inbox.summary.countByProduct).sort()
  const sourceFailures = inboxSourceLoadFailures(inbox.sources)

  return (
    <section className="space-y-4">
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        <SummaryCard label="Assigned" value={inbox.summary.totalCount} />
        <SummaryCard label="Blocked" value={inbox.summary.blockedCount} accent="amber" />
        <SummaryCard label="Products" value={productOptions.length} />
        <SummaryCard
          label="Synced"
          value={inbox.sources.filter((source) => source.fetched).length}
        />
      </div>

      <div className="flex flex-wrap gap-2">
        <FilterChip active={!productFilter} label="All" onClick={() => onProductFilterChange('')} />
        {productOptions.map((productKey) => (
          <FilterChip
            key={productKey}
            active={productFilter === productKey}
            label={productLabel(productKey)}
            count={inbox.summary.countByProduct[productKey] ?? 0}
            onClick={() => onProductFilterChange(productKey)}
          />
        ))}
      </div>

      {sourceFailures.length > 0 && (
        <ApiErrorCallout
          testId="companion-inbox-source-errors"
          tone="warning"
          title="Some product inboxes could not be loaded"
          message={sourceFailures
            .map((failure) => `${productLabel(failure.productKey)}: ${failure.message}`)
            .join(' | ')}
        />
      )}

      {filteredItems.length === 0 ? (
        <div className="rounded-xl border border-slate-700 bg-slate-900/70 px-4 py-10 text-center text-sm text-slate-300">
          No assigned field tasks match this filter.
        </div>
      ) : (
        <ul className="space-y-3">
          {filteredItems.map((task) => (
            <TaskCard
              key={task.taskKey}
              task={task}
              accessToken={accessToken}
              submissionChips={getSubmissionChips?.(task.taskKey) ?? []}
              acknowledged={acknowledgedTaskKeys?.has(task.taskKey) ?? false}
              onAcknowledge={onAcknowledgeTask}
              onEvidenceUploadComplete={onEvidenceUploadComplete}
              highlighted={highlightedTaskKey === task.taskKey}
            />
          ))}
        </ul>
      )}
    </section>
  )
}

function SummaryCard({
  label,
  value,
  accent,
}: {
  label: string
  value: number
  accent?: 'amber'
}) {
  return (
    <div className="rounded-xl border border-slate-700 bg-slate-900/70 px-4 py-3">
      <p className="text-xs uppercase tracking-wide text-slate-400">{label}</p>
      <p
        className={`mt-1 text-2xl font-semibold ${accent === 'amber' ? 'text-amber-300' : 'text-teal-300'}`}
      >
        {value}
      </p>
    </div>
  )
}

function FilterChip({
  label,
  count,
  active,
  onClick,
}: {
  label: string
  count?: number
  active: boolean
  onClick: () => void
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`rounded-full px-3 py-1.5 text-sm font-medium transition ${
        active
          ? 'bg-teal-600 text-white'
          : 'border border-slate-600 bg-slate-900 text-slate-200 hover:border-teal-500'
      }`}
    >
      {label}
      {typeof count === 'number' ? ` (${count})` : ''}
    </button>
  )
}

function TaskCard({
  task,
  accessToken,
  submissionChips,
  acknowledged,
  onAcknowledge,
  onEvidenceUploadComplete,
  highlighted,
}: {
  task: FieldInboxTaskItem
  accessToken: string
  submissionChips: MergedSubmissionChip[]
  acknowledged: boolean
  onAcknowledge?: (task: FieldInboxTaskItem) => void
  onEvidenceUploadComplete?: () => void
  highlighted?: boolean
}) {
  const launchUrl = task.deepLinkUrl ?? productLaunchUrl(task.productKey, task.deepLinkPath)

  return (
    <li
      className={`rounded-xl border bg-slate-900/80 p-4 shadow-sm ${
        highlighted ? 'border-teal-400 ring-2 ring-teal-500/40' : 'border-slate-700'
      }`}
      data-testid="companion-field-inbox-task"
      data-task-key={task.taskKey}
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-teal-300">
            {productLabel(task.productKey)} · {taskTypeLabel(task.taskType)}
          </p>
          <h3 className="mt-1 text-base font-semibold text-white">{task.title}</h3>
          {task.subtitle && <p className="mt-1 text-sm text-slate-300">{task.subtitle}</p>}
        </div>
        <span className="rounded-full bg-slate-800 px-2.5 py-1 text-xs font-medium uppercase text-slate-200">
          {task.status.replaceAll('_', ' ')}
        </span>
      </div>

      <div className="mt-3 flex flex-wrap items-center gap-3 text-xs text-slate-400">
        <span>{formatWhen(task.dueAt ?? task.sortAt)}</span>
        {task.priority && <span className="uppercase">Priority {task.priority}</span>}
        {task.blockedReason && (
          <span
            className="rounded bg-amber-950/60 px-2 py-0.5 text-amber-200"
            data-testid="companion-task-blocked-reason"
          >
            {formatBlockedTaskReason(task.blockedReason)}
          </span>
        )}
      </div>

      <TaskSubmissionStatusBadge chips={submissionChips} />

      <div className="mt-4 flex flex-wrap gap-2">
        {onAcknowledge && (
          <button
            type="button"
            className="inline-flex min-h-11 items-center rounded-lg border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-500 disabled:opacity-50"
            disabled={acknowledged}
            data-testid="companion-acknowledge-task"
            onClick={() => onAcknowledge(task)}
          >
            {acknowledged ? 'Acknowledged' : 'Acknowledge'}
          </button>
        )}
        {launchUrl ? (
          <a
            href={launchUrl}
            className="inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500"
          >
            Open in {productLabel(task.productKey)}
          </a>
        ) : (
          <p className="inline-flex min-h-11 items-center text-xs text-slate-500">
            Deep link unavailable for this product.
          </p>
        )}
      </div>

      {isTrainarrFieldTask(task.taskKey) && (
        <FieldTaskEvidencePanel
          accessToken={accessToken}
          task={task}
          onUploadComplete={onEvidenceUploadComplete}
        />
      )}

      {isRoutarrTripTask(task.taskKey) && (
        <FieldTaskDvirPanel
          accessToken={accessToken}
          task={task}
          onSubmitComplete={onEvidenceUploadComplete}
        />
      )}

      {isMaintainarrInspectionTask(task.taskKey) && (
        <FieldTaskInspectionPanel
          accessToken={accessToken}
          task={task}
          onSubmitComplete={onEvidenceUploadComplete}
        />
      )}

      {isMaintainarrWorkOrderTask(task.taskKey) && (
        <FieldTaskWorkOrderPanel
          accessToken={accessToken}
          task={task}
          onSubmitComplete={onEvidenceUploadComplete}
        />
      )}

      {isSupplyarrReceivingTask(task.taskKey) && (
        <FieldTaskReceivingPanel
          accessToken={accessToken}
          task={task}
          onSubmitComplete={onEvidenceUploadComplete}
        />
      )}
    </li>
  )
}
