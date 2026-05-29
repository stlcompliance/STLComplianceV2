export interface DowntimeDeepLinkContext {
  assetId: string | null
  workOrderId: string | null
  defectId: string | null
  eventId: string | null
}

export interface DowntimeFollowUpResponse {
  eventId: string
  assetId: string
  deepLinkPath: string
  reason: string
  trigger: string
}

export function parseDowntimeDeepLink(search: string): DowntimeDeepLinkContext {
  const params = new URLSearchParams(search)
  return {
    assetId: params.get('assetId'),
    workOrderId: params.get('workOrderId'),
    defectId: params.get('defectId'),
    eventId: params.get('eventId'),
  }
}

export function buildDowntimeDeepLinkPath(context: DowntimeDeepLinkContext): string {
  const params = new URLSearchParams()
  if (context.assetId) {
    params.set('assetId', context.assetId)
  }
  if (context.workOrderId) {
    params.set('workOrderId', context.workOrderId)
  }
  if (context.defectId) {
    params.set('defectId', context.defectId)
  }
  if (context.eventId) {
    params.set('eventId', context.eventId)
  }
  const query = params.toString()
  return query ? `/downtime?${query}` : '/downtime'
}
