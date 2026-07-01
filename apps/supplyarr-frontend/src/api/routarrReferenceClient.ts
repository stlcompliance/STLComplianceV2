const apiBase = import.meta.env.VITE_ROUTARR_API_BASE ?? ''

export interface RoutArrTripReference {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  vehicleRefKey: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  supplierOrderId?: string | null
  brokerOrderId?: string | null
  dispatchBlockReason?: string | null
  supplierReadinessStatusSnapshot?: string | null
  supplierQuantityReadySnapshot?: number | null
  supplierOrderedQuantitySnapshot?: number | null
  supplierExpectedReadyAtSnapshot?: string | null
  supplierConfirmedReadyAtSnapshot?: string | null
  dispatchOverrideAt?: string | null
  dispatchOverrideReason?: string | null
  dispatchBlocks?: Array<{
    dispatchBlockId: string
    blockType: string
    blockReason: string
    status: string
  }>
}

class RoutArrReferenceApiError extends Error {
  constructor(message: string) {
    super(message)
    this.name = 'RoutArrReferenceApiError'
  }
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

export async function listRoutArrTripsBySupplierOrder(
  accessToken: string,
  supplierOrderId: string,
): Promise<RoutArrTripReference[]> {
  if (!apiBase) {
    return []
  }

  const query = new URLSearchParams({ supplierOrderId })
  const response = await fetch(`${apiBase}/api/v1/trips?${query.toString()}`, {
    headers: authHeaders(accessToken),
  })

  if (!response.ok) {
    throw new RoutArrReferenceApiError('Failed to load related RoutArr trips')
  }

  return (await response.json()) as RoutArrTripReference[]
}
