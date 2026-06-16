import {
  AlertTriangle,
  CalendarClock,
  CheckCircle2,
  CircleSlash,
  Clock,
  ExternalLink,
  Filter,
  ListTodo,
  RefreshCw,
  Search,
  UserRound,
  XCircle,
} from 'lucide-react'
import type { ReactNode } from 'react'
import { useMemo, useState } from 'react'

export type SchedulingAction =
  | 'schedule'
  | 'reschedule'
  | 'unschedule'
  | 'cancel'
  | 'complete'
  | 'override'

export type SchedulingWindow = {
  startAt?: string | null
  endAt?: string | null
  timeZone?: string | null
}

export type SchedulingSourceReference = {
  productKey: string
  resourceType: string
  resourceId: string
  label?: string | null
  href?: string | null
}

export type SchedulingResourceAssignment = {
  resourceId: string
  label: string
  productKey?: string | null
  role?: string | null
  status?: string | null
}

export type SchedulingLocationAssignment = {
  locationId: string
  label: string
  productKey?: string | null
  status?: string | null
}

export type SchedulingConflict = {
  conflictType: string
  severity: 'info' | 'warning' | 'blocking' | string
  message: string
  sourceReference?: SchedulingSourceReference | null
}

export type SchedulingDisplayItem = {
  itemId: string
  productKey: string
  itemType: string
  title: string
  status: string
  priority?: string | null
  requestedWindow?: SchedulingWindow | null
  promisedWindow?: SchedulingWindow | null
  scheduledWindow?: SchedulingWindow | null
  resourceAssignments?: SchedulingResourceAssignment[]
  resourceNeeds?: SchedulingResourceAssignment[]
  locationAssignments?: SchedulingLocationAssignment[]
  sourceReferences?: SchedulingSourceReference[]
  blockers?: SchedulingConflict[]
  allowedActions?: SchedulingAction[]
  permissions?: Partial<Record<SchedulingAction, boolean>>
}

export type SchedulingResourceLane = {
  resourceId: string
  label: string
  productKey?: string | null
  status?: string | null
}

export type SchedulingBoardProps = {
  title?: string
  unscheduledItems: SchedulingDisplayItem[]
  scheduledItems: SchedulingDisplayItem[]
  resources: SchedulingResourceLane[]
  isLoading?: boolean
  conflicts?: SchedulingConflict[]
  onRefresh?: () => Promise<void> | void
  onSchedule?: (item: SchedulingDisplayItem) => Promise<void> | void
  onReschedule?: (item: SchedulingDisplayItem) => Promise<void> | void
  onUnschedule?: (item: SchedulingDisplayItem) => Promise<void> | void
  onCancel?: (item: SchedulingDisplayItem) => Promise<void> | void
  onComplete?: (item: SchedulingDisplayItem) => Promise<void> | void
  onOpenProductRecord?: (reference: SchedulingSourceReference, item: SchedulingDisplayItem) => void
}

function cx(...classes: Array<string | false | null | undefined>): string {
  return classes.filter(Boolean).join(' ')
}

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not set'
  const spaced = value.replace(/([a-z0-9])([A-Z])/g, '$1 $2').replace(/[._-]+/g, ' ').trim()
  return spaced ? spaced.charAt(0).toUpperCase() + spaced.slice(1) : 'Not set'
}

function formatWindow(window: SchedulingWindow | null | undefined): string {
  if (!window?.startAt && !window?.endAt) {
    return 'Not set'
  }

  const start = formatDateTime(window.startAt)
  const end = formatDateTime(window.endAt)
  if (start === 'Not set') return end
  if (end === 'Not set') return start
  return `${start} to ${end}`
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) return 'Not set'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}

function productLabel(productKey: string): string {
  const normalized = productKey.trim().toLowerCase()
  if (normalized.endsWith('arr')) {
    return `${normalized.slice(0, -3).replace(/^\w/, (value) => value.toUpperCase())}Arr`
  }

  if (normalized === 'compliancecore') return 'Compliance Core'
  if (normalized === 'referencedatacore') return 'Reference Data Core'
  return humanize(productKey)
}

function canRun(item: SchedulingDisplayItem, action: SchedulingAction): boolean {
  const listed = item.allowedActions?.includes(action) ?? true
  const permitted = item.permissions?.[action] ?? true
  return listed && permitted
}

function assignedToResource(item: SchedulingDisplayItem, resourceId: string): boolean {
  return item.resourceAssignments?.some((assignment) => assignment.resourceId === resourceId) ?? false
}

function itemTone(item: SchedulingDisplayItem): string {
  if (item.blockers?.some((conflict) => conflict.severity === 'blocking')) {
    return 'border-red-500/40 bg-red-950/20 text-red-50'
  }
  if (item.priority === 'high' || item.priority === 'urgent') {
    return 'border-amber-500/40 bg-amber-950/20 text-amber-50'
  }
  return 'border-sky-500/30 bg-slate-950/70 text-slate-100'
}

function ActionButton({
  action,
  item,
  onClick,
  icon,
}: {
  action: string
  item: SchedulingDisplayItem
  onClick?: (item: SchedulingDisplayItem) => Promise<void> | void
  icon: ReactNode
}) {
  if (!onClick) return null

  return (
    <button
      type="button"
      onClick={() => onClick(item)}
      title={action}
      className="inline-flex min-h-10 items-center gap-2 rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm font-semibold text-slate-100 hover:border-teal-500/60 hover:bg-slate-900 disabled:cursor-not-allowed disabled:opacity-50"
    >
      {icon}
      <span>{action}</span>
    </button>
  )
}

function WorkItemButton({
  item,
  selected,
  onSelect,
}: {
  item: SchedulingDisplayItem
  selected: boolean
  onSelect: (item: SchedulingDisplayItem) => void
}) {
  const blockers = item.blockers?.length ?? 0
  return (
    <button
      type="button"
      onClick={() => onSelect(item)}
      className={cx(
        'w-full rounded-md border p-3 text-left transition-colors',
        itemTone(item),
        selected && 'ring-2 ring-teal-400/70',
      )}
    >
      <span className="flex items-start justify-between gap-3">
        <span className="min-w-0">
          <span className="block truncate text-sm font-semibold">{item.title}</span>
          <span className="mt-1 block text-xs text-slate-400">
            {productLabel(item.productKey)} / {humanize(item.itemType)}
          </span>
        </span>
        {blockers > 0 ? (
          <span className="inline-flex shrink-0 items-center gap-1 rounded-md border border-red-500/30 px-2 py-1 text-xs text-red-200">
            <AlertTriangle className="h-3.5 w-3.5" aria-hidden />
            {blockers}
          </span>
        ) : null}
      </span>
      <span className="mt-3 grid gap-1 text-xs text-slate-300">
        <span>Requested: {formatWindow(item.requestedWindow)}</span>
        <span>Promised: {formatWindow(item.promisedWindow)}</span>
        <span>Scheduled: {formatWindow(item.scheduledWindow)}</span>
      </span>
    </button>
  )
}

export function SchedulingBoard({
  title = 'Planning board',
  unscheduledItems,
  scheduledItems,
  resources,
  isLoading = false,
  conflicts = [],
  onRefresh,
  onSchedule,
  onReschedule,
  onUnschedule,
  onCancel,
  onComplete,
  onOpenProductRecord,
}: SchedulingBoardProps) {
  const [selectedItemId, setSelectedItemId] = useState<string | null>(
    unscheduledItems[0]?.itemId ?? scheduledItems[0]?.itemId ?? null,
  )
  const [query, setQuery] = useState('')
  const [statusFilter, setStatusFilter] = useState<'all' | 'blocked' | 'unscheduled'>('all')

  const allItems = useMemo(() => [...unscheduledItems, ...scheduledItems], [scheduledItems, unscheduledItems])
  const selectedItem = allItems.find((item) => item.itemId === selectedItemId) ?? allItems[0] ?? null
  const normalizedQuery = query.trim().toLowerCase()
  const visibleUnscheduled = unscheduledItems.filter((item) => {
    const matchesQuery =
      normalizedQuery.length === 0 ||
      item.title.toLowerCase().includes(normalizedQuery) ||
      item.productKey.toLowerCase().includes(normalizedQuery)
    const matchesStatus =
      statusFilter === 'all' ||
      statusFilter === 'unscheduled' ||
      (statusFilter === 'blocked' && (item.blockers?.length ?? 0) > 0)
    return matchesQuery && matchesStatus
  })
  const selectedConflicts = [
    ...(selectedItem?.blockers ?? []),
    ...conflicts,
  ]
  const unassignedScheduled = scheduledItems.filter((item) => (item.resourceAssignments?.length ?? 0) === 0)

  const selectItem = (item: SchedulingDisplayItem) => setSelectedItemId(item.itemId)

  return (
    <div className="space-y-5 text-slate-100" data-testid="scheduling-board">
      <section className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold tracking-normal text-white">{title}</h1>
          <p className="mt-1 text-sm text-slate-400">{scheduledItems.length} scheduled / {unscheduledItems.length} unscheduled</p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <label className="relative block">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-500" aria-hidden />
            <input
              aria-label="Search work"
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              className="min-h-10 rounded-md border border-slate-700 bg-slate-950 py-2 pl-9 pr-3 text-sm text-slate-100 outline-none focus:border-teal-500"
            />
          </label>
          <label className="inline-flex min-h-10 items-center gap-2 rounded-md border border-slate-700 bg-slate-950 px-3 text-sm text-slate-200">
            <Filter className="h-4 w-4 text-slate-400" aria-hidden />
            <select
              aria-label="Filter work"
              value={statusFilter}
              onChange={(event) => setStatusFilter(event.target.value as typeof statusFilter)}
              className="bg-transparent text-sm outline-none"
            >
              <option value="all">All</option>
              <option value="unscheduled">Unscheduled</option>
              <option value="blocked">Blocked</option>
            </select>
          </label>
          {onRefresh ? (
            <button
              type="button"
              onClick={() => onRefresh()}
              title="Refresh"
              aria-label="Refresh"
              className="inline-flex h-10 w-10 items-center justify-center rounded-md border border-slate-700 bg-slate-950 text-slate-100 hover:border-teal-500/60"
            >
              <RefreshCw className={cx('h-4 w-4', isLoading && 'animate-spin')} aria-hidden />
            </button>
          ) : null}
        </div>
      </section>

      <div className="grid gap-5 xl:grid-cols-[20rem_minmax(0,1fr)_24rem]">
        <aside className="min-h-[32rem] rounded-md border border-slate-800 bg-slate-950/70">
          <div className="flex items-center gap-2 border-b border-slate-800 px-4 py-3">
            <ListTodo className="h-4 w-4 text-teal-300" aria-hidden />
            <h2 className="text-sm font-semibold text-white">Unscheduled work</h2>
          </div>
          <div className="max-h-[44rem] space-y-3 overflow-y-auto p-3">
            {visibleUnscheduled.length === 0 ? (
              <p className="rounded-md border border-slate-800 bg-slate-900/70 p-3 text-sm text-slate-400">No matching work.</p>
            ) : (
              visibleUnscheduled.map((item) => (
                <WorkItemButton
                  key={item.itemId}
                  item={item}
                  selected={item.itemId === selectedItem?.itemId}
                  onSelect={selectItem}
                />
              ))
            )}
          </div>
        </aside>

        <section className="min-h-[32rem] overflow-hidden rounded-md border border-slate-800 bg-slate-950/60">
          <div className="flex items-center gap-2 border-b border-slate-800 px-4 py-3">
            <CalendarClock className="h-4 w-4 text-teal-300" aria-hidden />
            <h2 className="text-sm font-semibold text-white">Resource plan</h2>
          </div>
          <div className="divide-y divide-slate-800">
            {[...resources, ...(unassignedScheduled.length ? [{ resourceId: 'unassigned', label: 'Unassigned' }] : [])].map((resource) => {
              const laneItems = resource.resourceId === 'unassigned'
                ? unassignedScheduled
                : scheduledItems.filter((item) => assignedToResource(item, resource.resourceId))
              return (
                <div key={resource.resourceId} className="grid min-h-28 gap-3 p-4 lg:grid-cols-[12rem_minmax(0,1fr)]">
                  <div className="flex items-start gap-2 text-sm">
                    <UserRound className="mt-0.5 h-4 w-4 shrink-0 text-slate-400" aria-hidden />
                    <div className="min-w-0">
                      <p className="truncate font-semibold text-white">{resource.label}</p>
                      <p className="text-xs text-slate-500">{humanize(resource.status)}</p>
                    </div>
                  </div>
                  <div className="grid gap-3 md:grid-cols-2 2xl:grid-cols-3">
                    {laneItems.length === 0 ? (
                      <div className="flex min-h-20 items-center rounded-md border border-dashed border-slate-800 px-3 text-sm text-slate-500">
                        No scheduled work
                      </div>
                    ) : (
                      laneItems.map((item) => (
                        <WorkItemButton
                          key={`${resource.resourceId}-${item.itemId}`}
                          item={item}
                          selected={item.itemId === selectedItem?.itemId}
                          onSelect={selectItem}
                        />
                      ))
                    )}
                  </div>
                </div>
              )
            })}
          </div>
        </section>

        <aside className="min-h-[32rem] rounded-md border border-slate-800 bg-slate-950/70">
          <div className="flex items-center gap-2 border-b border-slate-800 px-4 py-3">
            <Clock className="h-4 w-4 text-teal-300" aria-hidden />
            <h2 className="text-sm font-semibold text-white">Details</h2>
          </div>
          {selectedItem ? (
            <div className="space-y-5 p-4">
              <div>
                <p className="text-xs font-semibold uppercase text-teal-300">{productLabel(selectedItem.productKey)}</p>
                <h3 className="mt-1 text-lg font-bold text-white">{selectedItem.title}</h3>
                <p className="mt-1 text-sm text-slate-400">{humanize(selectedItem.status)} / {humanize(selectedItem.itemType)}</p>
              </div>

              <div className="grid gap-3 text-sm">
                <div className="rounded-md border border-slate-800 bg-slate-900/50 p-3">
                  <p className="text-xs font-semibold uppercase text-slate-500">Requested window</p>
                  <p className="mt-1 text-slate-100">{formatWindow(selectedItem.requestedWindow)}</p>
                </div>
                <div className="rounded-md border border-slate-800 bg-slate-900/50 p-3">
                  <p className="text-xs font-semibold uppercase text-slate-500">Promised window</p>
                  <p className="mt-1 text-slate-100">{formatWindow(selectedItem.promisedWindow)}</p>
                </div>
                <div className="rounded-md border border-slate-800 bg-slate-900/50 p-3">
                  <p className="text-xs font-semibold uppercase text-slate-500">Scheduled window</p>
                  <p className="mt-1 text-slate-100">{formatWindow(selectedItem.scheduledWindow)}</p>
                </div>
              </div>

              <div className="flex flex-wrap gap-2">
                {canRun(selectedItem, 'schedule') ? (
                  <ActionButton action="Schedule" item={selectedItem} onClick={onSchedule} icon={<CalendarClock className="h-4 w-4" aria-hidden />} />
                ) : null}
                {canRun(selectedItem, 'reschedule') ? (
                  <ActionButton action="Reschedule" item={selectedItem} onClick={onReschedule} icon={<Clock className="h-4 w-4" aria-hidden />} />
                ) : null}
                {canRun(selectedItem, 'unschedule') ? (
                  <ActionButton action="Unschedule" item={selectedItem} onClick={onUnschedule} icon={<CircleSlash className="h-4 w-4" aria-hidden />} />
                ) : null}
                {canRun(selectedItem, 'complete') ? (
                  <ActionButton action="Complete" item={selectedItem} onClick={onComplete} icon={<CheckCircle2 className="h-4 w-4" aria-hidden />} />
                ) : null}
                {canRun(selectedItem, 'cancel') ? (
                  <ActionButton action="Cancel" item={selectedItem} onClick={onCancel} icon={<XCircle className="h-4 w-4" aria-hidden />} />
                ) : null}
              </div>

              <div>
                <h4 className="text-sm font-semibold text-white">Source records</h4>
                <div className="mt-2 space-y-2">
                  {(selectedItem.sourceReferences ?? []).map((reference) => (
                    <button
                      key={`${reference.productKey}-${reference.resourceType}-${reference.resourceId}`}
                      type="button"
                      onClick={() => onOpenProductRecord?.(reference, selectedItem)}
                      className="flex w-full items-center justify-between gap-3 rounded-md border border-slate-800 bg-slate-900/50 px-3 py-2 text-left text-sm text-slate-200 hover:border-teal-500/60"
                    >
                      <span className="min-w-0 truncate">
                        {reference.label ?? `${humanize(reference.resourceType)} ${reference.resourceId}`}
                      </span>
                      <ExternalLink className="h-4 w-4 shrink-0 text-slate-400" aria-hidden />
                    </button>
                  ))}
                </div>
              </div>

              {selectedConflicts.length > 0 ? (
                <div className="rounded-md border border-red-500/30 bg-red-950/20 p-3" role="status">
                  <div className="flex items-center gap-2 text-sm font-semibold text-red-100">
                    <AlertTriangle className="h-4 w-4" aria-hidden />
                    Conflict review
                  </div>
                  <div className="mt-3 space-y-2">
                    {selectedConflicts.map((conflict, index) => (
                      <p key={`${conflict.conflictType}-${index}`} className="text-sm text-red-100">
                        {humanize(conflict.conflictType)}: {conflict.message}
                      </p>
                    ))}
                  </div>
                </div>
              ) : null}
            </div>
          ) : (
            <p className="p-4 text-sm text-slate-400">No work selected.</p>
          )}
        </aside>
      </div>
    </div>
  )
}
