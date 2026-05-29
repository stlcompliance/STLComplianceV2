export const WORK_ORDER_LABOR_TYPES = ['regular', 'overtime', 'travel'] as const
export type WorkOrderLaborType = (typeof WORK_ORDER_LABOR_TYPES)[number]

export function workOrderEditable(status: string): boolean {
  return status === 'open' || status === 'in_progress'
}

export function nextWorkOrderStatusAction(status: string): 'in_progress' | 'completed' | null {
  if (status === 'open') {
    return 'in_progress'
  }

  if (status === 'in_progress') {
    return 'completed'
  }

  return null
}

export function workOrderStatusActionLabel(status: string): string | null {
  const action = nextWorkOrderStatusAction(status)
  if (action === 'in_progress') {
    return 'Start work'
  }

  if (action === 'completed') {
    return 'Complete work order'
  }

  return null
}

export function parseLaborHoursInput(value: string): number | null {
  const trimmed = value.trim()
  if (!trimmed) {
    return null
  }

  const parsed = Number(trimmed)
  if (!Number.isFinite(parsed) || parsed <= 0) {
    return null
  }

  return parsed
}
