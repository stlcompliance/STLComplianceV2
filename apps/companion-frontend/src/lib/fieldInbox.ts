import type { FieldInboxProductSlice, FieldInboxTaskItem } from '../api/types'

const PRODUCT_LABELS: Record<string, string> = {
  maintainarr: 'MaintainArr',
  routarr: 'RoutArr',
  trainarr: 'TrainArr',
  staffarr: 'StaffArr',
  supplyarr: 'SupplyArr',
}

const TASK_TYPE_LABELS: Record<string, string> = {
  work_order: 'Work order',
  inspection: 'Inspection',
  trip: 'Trip',
  training_assignment: 'Training',
  incident_acknowledgement: 'Incident',
  receiving: 'Receiving',
}

export function productLabel(productKey: string): string {
  return PRODUCT_LABELS[productKey.toLowerCase()] ?? productKey
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

export function filterTasks(
  items: FieldInboxTaskItem[],
  productFilter: string,
): FieldInboxTaskItem[] {
  if (!productFilter) {
    return items
  }

  return items.filter((item) => item.productKey === productFilter)
}

export function entitledProductKeys(sources: FieldInboxProductSlice[]): string[] {
  return sources.filter((source) => source.entitled).map((source) => source.productKey)
}
