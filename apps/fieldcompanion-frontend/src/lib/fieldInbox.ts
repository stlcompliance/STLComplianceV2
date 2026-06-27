import type { FieldInboxProductSlice, FieldInboxTaskItem } from '../api/types'

import { formatInboxSourceError } from './FieldCompanionDeniedReasonCatalog'

const PRODUCT_LABELS: Record<string, string> = {
  fieldcompanion: 'Field Companion',
  maintainarr: 'MaintainArr',
  routarr: 'RoutArr',
  trainarr: 'TrainArr',
  staffarr: 'StaffArr',
  supplyarr: 'SupplyArr',
  loadarr: 'LoadArr',
  recordarr: 'RecordArr',
  assurarr: 'AssurArr',
  ordarr: 'OrdArr',
  customarr: 'CustomArr',
  compliancecore: 'Compliance Core',
}

const TASK_TYPE_LABELS: Record<string, string> = {
  work_order: 'Work order',
  inspection: 'Inspection',
  trip: 'Trip',
  training_assignment: 'Training',
  incident_acknowledgement: 'Incident',
  receiving: 'Receiving',
}

export const FIELD_INBOX_DUE_SOON_WINDOW_MS = 24 * 60 * 60_000
export const FIELD_INBOX_STALE_AFTER_MS = 24 * 60 * 60_000

export type FieldInboxTaskUrgency = 'blocked' | 'overdue' | 'due_soon' | 'stale' | 'current'

export interface FieldInboxTaskInsight {
  bucket: FieldInboxTaskUrgency
  dueLabel: string
  freshnessLabel: string
  isDueSoon: boolean
  isOverdue: boolean
  isStale: boolean
}

export interface FieldInboxTaskGroup {
  bucket: FieldInboxTaskUrgency
  label: string
  description: string
  items: Array<{ task: FieldInboxTaskItem; insight: FieldInboxTaskInsight }>
}

export interface FieldInboxUrgencySummary {
  dueSoonCount: number
  overdueCount: number
  staleCount: number
  urgentCount: number
}

export function productLabel(productKey: string): string {
  return PRODUCT_LABELS[productKey.toLowerCase()] ?? productKey
}

export function productTitle(productKey: string): string {
  const label = productLabel(productKey)
  return label.includes(' ') ? label : `${label}`
}

export function taskTypeLabel(taskType: string): string {
  return TASK_TYPE_LABELS[taskType] ?? taskType.replaceAll('_', ' ')
}

export function formatWhen(value: string | null | undefined): string {
  if (!value) {
    return 'No due date'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  })
}

function formatDuration(ms: number): string {
  if (ms <= 0) {
    return 'just now'
  }

  const totalMinutes = Math.floor(ms / 60_000)
  const days = Math.floor(totalMinutes / (60 * 24))
  const hours = Math.floor((totalMinutes % (60 * 24)) / 60)
  const minutes = totalMinutes % 60

  if (days > 0) {
    return `${days}d ${hours}h`
  }

  if (hours > 0) {
    return `${hours}h ${minutes}m`
  }

  if (minutes > 0) {
    return `${minutes}m`
  }

  return 'just now'
}

function parseDate(value: string | null | undefined): number | null {
  if (!value) {
    return null
  }

  const parsed = Date.parse(value)
  return Number.isNaN(parsed) ? null : parsed
}

export function formatFieldInboxRelativeTime(value: string | null | undefined, now = new Date()): string {
  const parsed = parseDate(value)
  if (parsed === null) {
    return value ?? 'No timestamp'
  }

  const diff = parsed - now.getTime()
  const label = formatDuration(Math.abs(diff))

  if (diff > 0) {
    return `in ${label}`
  }

  if (diff < 0) {
    return `${label} ago`
  }

  return 'just now'
}

export function summarizeFieldInboxTaskInsight(
  task: FieldInboxTaskItem,
  now = new Date(),
): FieldInboxTaskInsight {
  const dueAtMs = parseDate(task.dueAt)
  const sortAtMs = parseDate(task.sortAt)
  const isBlocked = Boolean(task.blockedReason?.trim())
  const isOverdue = dueAtMs !== null && dueAtMs < now.getTime()
  const isDueSoon =
    dueAtMs !== null &&
    dueAtMs >= now.getTime() &&
    dueAtMs - now.getTime() <= FIELD_INBOX_DUE_SOON_WINDOW_MS
  const isStale =
    sortAtMs !== null && now.getTime() - sortAtMs >= FIELD_INBOX_STALE_AFTER_MS

  let bucket: FieldInboxTaskUrgency = 'current'
  if (isBlocked) {
    bucket = 'blocked'
  } else if (isOverdue) {
    bucket = 'overdue'
  } else if (isDueSoon) {
    bucket = 'due_soon'
  } else if (isStale) {
    bucket = 'stale'
  }

  const dueLabel =
    dueAtMs === null
      ? 'No due date'
      : isOverdue
        ? `Overdue by ${formatDuration(now.getTime() - dueAtMs)}`
        : isDueSoon
          ? `Due in ${formatDuration(dueAtMs - now.getTime())}`
          : `Due ${formatWhen(task.dueAt)}`

  const freshnessLabel =
    sortAtMs === null
      ? 'Freshness unavailable'
      : `Updated ${formatFieldInboxRelativeTime(task.sortAt, now)}`

  return {
    bucket,
    dueLabel,
    freshnessLabel,
    isDueSoon,
    isOverdue,
    isStale,
  }
}

export function groupFieldInboxTasks(
  items: FieldInboxTaskItem[],
  now = new Date(),
): FieldInboxTaskGroup[] {
  const groups: Record<FieldInboxTaskUrgency, FieldInboxTaskGroup> = {
    blocked: {
      bucket: 'blocked',
      label: 'Blocked work',
      description: 'Needs a product change, permission update, or validation fix before it can move.',
      items: [],
    },
    overdue: {
      bucket: 'overdue',
      label: 'Overdue work',
      description: 'Past due and should be handled before anything newer.',
      items: [],
    },
    due_soon: {
      bucket: 'due_soon',
      label: 'Due soon',
      description: 'Due within the next 24 hours.',
      items: [],
    },
    stale: {
      bucket: 'stale',
      label: 'Stale work',
      description: 'Has not been refreshed recently and may need a recheck.',
      items: [],
    },
    current: {
      bucket: 'current',
      label: 'Other work',
      description: 'Ready to open now.',
      items: [],
    },
  }

  for (const task of sortTasksForInbox(items, now)) {
    const insight = summarizeFieldInboxTaskInsight(task, now)
    groups[insight.bucket].items.push({ task, insight })
  }

  return [
    groups.blocked,
    groups.overdue,
    groups.due_soon,
    groups.stale,
    groups.current,
  ].filter((group) => group.items.length > 0)
}

export function summarizeFieldInboxUrgency(
  items: FieldInboxTaskItem[],
  now = new Date(),
): FieldInboxUrgencySummary {
  const insights = items.map((task) => summarizeFieldInboxTaskInsight(task, now))

  return {
    dueSoonCount: insights.filter((insight) => insight.bucket === 'due_soon').length,
    overdueCount: insights.filter((insight) => insight.bucket === 'overdue').length,
    staleCount: insights.filter((insight) => insight.bucket === 'stale').length,
    urgentCount: insights.filter((insight) =>
      insight.bucket === 'blocked' || insight.bucket === 'overdue' || insight.bucket === 'due_soon',
    ).length,
  }
}

function sortTasksForInbox(items: FieldInboxTaskItem[], now = new Date()): FieldInboxTaskItem[] {
  const urgencyRank: Record<FieldInboxTaskUrgency, number> = {
    blocked: 0,
    overdue: 1,
    due_soon: 2,
    stale: 3,
    current: 4,
  }

  return [...items].sort((left, right) => {
    const leftInsight = summarizeFieldInboxTaskInsight(left, now)
    const rightInsight = summarizeFieldInboxTaskInsight(right, now)

    const urgencyDelta = urgencyRank[leftInsight.bucket] - urgencyRank[rightInsight.bucket]
    if (urgencyDelta !== 0) {
      return urgencyDelta
    }

    const leftDueAt = parseDate(left.dueAt) ?? Number.POSITIVE_INFINITY
    const rightDueAt = parseDate(right.dueAt) ?? Number.POSITIVE_INFINITY
    if (leftDueAt !== rightDueAt) {
      return leftDueAt - rightDueAt
    }

    const leftSortAt = parseDate(left.sortAt) ?? Number.NEGATIVE_INFINITY
    const rightSortAt = parseDate(right.sortAt) ?? Number.NEGATIVE_INFINITY
    if (leftSortAt !== rightSortAt) {
      return rightSortAt - leftSortAt
    }

    return left.title.localeCompare(right.title)
  })
}

export function filterTasks(
  items: FieldInboxTaskItem[],
  productFilter: string,
): FieldInboxTaskItem[] {
  if (!productFilter) {
    return items
  }

  return items.filter((item) => item.productKey === productFilter)
}

export function availableProductKeys(sources: FieldInboxProductSlice[]): string[] {
  return sources.filter((source) => source.available).map((source) => source.productKey)
}

export function inboxSourceLoadFailures(sources: FieldInboxProductSlice[]): Array<{
  productKey: string
  message: string
}> {
  return sources
    .filter((source) => source.available && !source.fetched)
    .map((source) => ({
      productKey: source.productKey,
      message: formatInboxSourceError(source.productKey, source.errorCode, source.errorMessage),
    }))
}
