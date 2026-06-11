import type { DetailTone } from '@stl/shared-ui'

export function humanizeVendorOrderValue(value: string | null | undefined): string {
  if (!value) {
    return 'Not recorded'
  }

  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (character) => character.toUpperCase())
}

export function formatVendorOrderDateTime(value: string | null | undefined): string {
  if (!value) {
    return 'Not recorded'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString()
}

export function formatVendorOrderDate(value: string | null | undefined): string {
  if (!value) {
    return 'Not recorded'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleDateString()
}

export function vendorOrderStatusTone(status: string | null | undefined): DetailTone {
  const normalized = status?.toLowerCase() ?? ''
  if (['completed_ready_for_dispatch', 'closed'].includes(normalized)) return 'good'
  if (['partially_ready', 'pending_vendor_acknowledgment', 'acknowledged', 'in_progress', 'sent_to_vendor'].includes(normalized)) return 'warn'
  if (['unable_to_fulfill', 'cancelled', 'split'].includes(normalized)) return 'bad'
  if (['draft'].includes(normalized)) return 'neutral'
  return 'info'
}

export function quantitySummary(ordered: number, ready: number, remaining: number, uom: string): string {
  return `${ready} ready / ${ordered} ordered / ${remaining} remaining ${uom}`
}
