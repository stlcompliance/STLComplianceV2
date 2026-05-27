import type { AggregatedFieldInboxResponse, FieldInboxTaskItem } from '../api/types'
import { formatWhen, productLabel, taskTypeLabel } from '../lib/fieldInbox'
import { productLaunchUrl } from '../api/client'

interface FieldInboxPanelProps {
  inbox: AggregatedFieldInboxResponse
  productFilter: string
  onProductFilterChange: (productKey: string) => void
}

export function FieldInboxPanel({
  inbox,
  productFilter,
  onProductFilterChange,
}: FieldInboxPanelProps) {
  const filteredItems = productFilter
    ? inbox.items.filter((item) => item.productKey === productFilter)
    : inbox.items

  const productOptions = Object.keys(inbox.summary.countByProduct).sort()

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

      {inbox.sources.some((source) => source.entitled && !source.fetched) && (
        <div className="rounded-lg border border-amber-500/40 bg-amber-950/30 px-4 py-3 text-sm text-amber-100">
          Some product inboxes could not be loaded. Tasks from entitled products still appear below
          when available.
        </div>
      )}

      {filteredItems.length === 0 ? (
        <div className="rounded-xl border border-slate-700 bg-slate-900/70 px-4 py-10 text-center text-sm text-slate-300">
          No assigned field tasks match this filter.
        </div>
      ) : (
        <ul className="space-y-3">
          {filteredItems.map((task) => (
            <TaskCard key={task.taskKey} task={task} />
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

function TaskCard({ task }: { task: FieldInboxTaskItem }) {
  const launchUrl = productLaunchUrl(task.productKey, task.deepLinkPath)

  return (
    <li className="rounded-xl border border-slate-700 bg-slate-900/80 p-4 shadow-sm">
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
          <span className="rounded bg-amber-950/60 px-2 py-0.5 text-amber-200">
            {task.blockedReason}
          </span>
        )}
      </div>

      {launchUrl ? (
        <a
          href={launchUrl}
          className="mt-4 inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500"
        >
          Open in {productLabel(task.productKey)}
        </a>
      ) : (
        <p className="mt-4 text-xs text-slate-500">Deep link unavailable for this product.</p>
      )}
    </li>
  )
}
