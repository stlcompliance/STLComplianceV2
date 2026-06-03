export type LoadArrReportInventoryBalance = {
  id: string
  supplyarrItemId: string
  itemNameSnapshot: string
  unitOfMeasureSnapshot: string
  state: string
  locationId: string
  locationNameSnapshot: string
  staffarrSiteNameSnapshot?: string
  quantityOnHand: number
  quantityReserved: number
  quantityAllocated: number
  quantityBlocked: number
  originEventType: string
  originReference: string
  traceTags: string[]
  notes: string
}

export type LoadArrReportHold = {
  id: string
  holdType: string
  supplyarrItemId: string
  locationNameSnapshot: string
  status: string
  reason: string
  sourceReference: string
  openedAtUtc: string
}

export type LoadArrReportUnexplainedInventoryRecord = {
  id: string
  recordNumber: string
  status: string
  discoverySource: string
  staffarrSiteOrgUnitId: string
  staffarrSiteNameSnapshot: string
  warehouseLocationId: string
  locationNameSnapshot: string
  supplyarrItemId: string
  itemNameSnapshot: string
  expectedQuantity: number
  quantity: number
  varianceQuantity: number
  unitOfMeasure: string
  reasonCode: string
  evidenceSummary: string
  resolutionState: string
  discoveredAtUtc: string
  resolvedAtUtc: string | null
}

export type LoadArrReportCount = {
  id: string
  countNumber: string
  status: string
  countType: string
  staffarrSiteNameSnapshot: string
  warehouseLocationId: string
  locationNameSnapshot: string
  supplyarrItemId: string
  itemNameSnapshot: string
  expectedQuantity: number
  countedQuantity: number
  varianceQuantity: number
  unitOfMeasure: string
  reasonCode: string
  createdAtUtc: string
  completedAtUtc: string | null
  approvedAtUtc: string | null
  updatedAtUtc: string
  inventoryAdjustmentId?: string | null
}

export type LoadArrReportAdjustment = {
  id: string
  adjustmentNumber: string
  status: string
  adjustmentType: string
  staffarrSiteNameSnapshot: string
  warehouseLocationId: string
  locationNameSnapshot: string
  supplyarrItemId: string
  itemNameSnapshot: string
  quantityDelta: number
  unitOfMeasure: string
  reasonCode: string
  createdAtUtc: string
  approvedAtUtc: string | null
  updatedAtUtc: string
}

export type LoadArrReportTimelineRow = {
  id: string
  category: 'movement' | 'origin' | 'hold' | 'unexplained' | 'count' | 'adjustment'
  title: string
  subtitle: string
  itemNameSnapshot: string
  locationNameSnapshot: string
  status: string
  quantity: number
  occurredAtUtc: string
  reason: string
}

export type LoadArrInventoryByLocationRow = {
  locationId: string
  locationNameSnapshot: string
  staffarrSiteNameSnapshot: string
  itemCount: number
  quantityOnHand: number
  quantityReserved: number
  quantityAllocated: number
  quantityBlocked: number
  activeStates: string[]
}

export type LoadArrInventoryByItemRow = {
  supplyarrItemId: string
  itemNameSnapshot: string
  unitOfMeasureSnapshot: string
  locationCount: number
  quantityOnHand: number
  quantityReserved: number
  quantityAllocated: number
  quantityBlocked: number
  stateCount: number
}

export type LoadArrInventoryByStatusRow = {
  state: string
  itemCount: number
  locationCount: number
  quantityOnHand: number
  quantityBlocked: number
}

function withinRange(isoDate: string, since?: string, until?: string) {
  if (!since && !until) {
    return true
  }

  const timestamp = Date.parse(isoDate)
  if (Number.isNaN(timestamp)) {
    return false
  }

  if (since) {
    const sinceTimestamp = Date.parse(since)
    if (!Number.isNaN(sinceTimestamp) && timestamp < sinceTimestamp) {
      return false
    }
  }

  if (until) {
    const untilTimestamp = Date.parse(until)
    if (!Number.isNaN(untilTimestamp) && timestamp > untilTimestamp) {
      return false
    }
  }

  return true
}

function normalizeNeedle(value: string) {
  return value.trim().toLowerCase()
}

function matchesNeedle(values: Array<string | number | null | undefined>, needle: string) {
  if (!needle) {
    return true
  }

  const normalizedNeedle = normalizeNeedle(needle)
  return values.some((value) => String(value ?? '').toLowerCase().includes(normalizedNeedle))
}

export function buildInventoryByLocationRows(
  inventory: LoadArrReportInventoryBalance[],
  filters?: {
    locationId?: string
    itemId?: string
    state?: string
    query?: string
  },
) {
  const filtered = inventory.filter((item) => {
    if (filters?.locationId && item.locationId !== filters.locationId) {
      return false
    }

    if (filters?.itemId && item.supplyarrItemId !== filters.itemId) {
      return false
    }

    if (filters?.state && item.state !== filters.state) {
      return false
    }

    if (filters?.query) {
      return matchesNeedle(
        [
          item.staffarrSiteNameSnapshot,
          item.locationNameSnapshot,
          item.supplyarrItemId,
          item.itemNameSnapshot,
          item.state,
          item.originEventType,
          item.originReference,
          item.notes,
          ...item.traceTags,
        ],
        filters.query,
      )
    }

    return true
  })

  const grouped = new Map<string, LoadArrInventoryByLocationRow>()
  for (const item of filtered) {
    const existing = grouped.get(item.locationId)
    if (!existing) {
      grouped.set(item.locationId, {
        locationId: item.locationId,
        locationNameSnapshot: item.locationNameSnapshot,
        staffarrSiteNameSnapshot: item.staffarrSiteNameSnapshot ?? 'Unknown site',
        itemCount: 1,
        quantityOnHand: item.quantityOnHand,
        quantityReserved: item.quantityReserved,
        quantityAllocated: item.quantityAllocated,
        quantityBlocked: item.quantityBlocked,
        activeStates: [item.state],
      })
      continue
    }

    existing.itemCount += 1
    existing.quantityOnHand += item.quantityOnHand
    existing.quantityReserved += item.quantityReserved
    existing.quantityAllocated += item.quantityAllocated
    existing.quantityBlocked += item.quantityBlocked
    if (!existing.activeStates.includes(item.state)) {
      existing.activeStates.push(item.state)
    }
  }

  return Array.from(grouped.values()).sort((left, right) =>
    `${left.staffarrSiteNameSnapshot} ${left.locationNameSnapshot}`.localeCompare(
      `${right.staffarrSiteNameSnapshot} ${right.locationNameSnapshot}`,
    ),
  )
}

export function buildInventoryByItemRows(
  inventory: LoadArrReportInventoryBalance[],
  filters?: {
    locationId?: string
    itemId?: string
    state?: string
    query?: string
  },
) {
  const filtered = inventory.filter((item) => {
    if (filters?.locationId && item.locationId !== filters.locationId) {
      return false
    }

    if (filters?.itemId && item.supplyarrItemId !== filters.itemId) {
      return false
    }

    if (filters?.state && item.state !== filters.state) {
      return false
    }

    if (filters?.query) {
      return matchesNeedle(
        [
          item.staffarrSiteNameSnapshot,
          item.locationNameSnapshot,
          item.supplyarrItemId,
          item.itemNameSnapshot,
          item.state,
          item.originEventType,
          item.originReference,
          item.notes,
          ...item.traceTags,
        ],
        filters.query,
      )
    }

    return true
  })

  const grouped = new Map<string, LoadArrInventoryByItemRow>()
  for (const item of filtered) {
    const existing = grouped.get(item.supplyarrItemId)
    if (!existing) {
      grouped.set(item.supplyarrItemId, {
        supplyarrItemId: item.supplyarrItemId,
        itemNameSnapshot: item.itemNameSnapshot,
        unitOfMeasureSnapshot: item.unitOfMeasureSnapshot,
        locationCount: 1,
        quantityOnHand: item.quantityOnHand,
        quantityReserved: item.quantityReserved,
        quantityAllocated: item.quantityAllocated,
        quantityBlocked: item.quantityBlocked,
        stateCount: 1,
      })
      continue
    }

    existing.locationCount += 1
    existing.quantityOnHand += item.quantityOnHand
    existing.quantityReserved += item.quantityReserved
    existing.quantityAllocated += item.quantityAllocated
    existing.quantityBlocked += item.quantityBlocked
    existing.stateCount += 1
  }

  return Array.from(grouped.values()).sort((left, right) =>
    left.itemNameSnapshot.localeCompare(right.itemNameSnapshot),
  )
}

export function buildInventoryByStatusRows(
  inventory: LoadArrReportInventoryBalance[],
  filters?: {
    locationId?: string
    itemId?: string
    state?: string
    query?: string
  },
) {
  const filtered = inventory.filter((item) => {
    if (filters?.locationId && item.locationId !== filters.locationId) {
      return false
    }

    if (filters?.itemId && item.supplyarrItemId !== filters.itemId) {
      return false
    }

    if (filters?.state && item.state !== filters.state) {
      return false
    }

    if (filters?.query) {
      return matchesNeedle(
        [
          item.staffarrSiteNameSnapshot,
          item.locationNameSnapshot,
          item.supplyarrItemId,
          item.itemNameSnapshot,
          item.state,
          item.originEventType,
          item.originReference,
          item.notes,
          ...item.traceTags,
        ],
        filters.query,
      )
    }

    return true
  })

  const grouped = new Map<string, LoadArrInventoryByStatusRow>()
  for (const item of filtered) {
    const existing = grouped.get(item.state)
    if (!existing) {
      grouped.set(item.state, {
        state: item.state,
        itemCount: 1,
        locationCount: 1,
        quantityOnHand: item.quantityOnHand,
        quantityBlocked: item.quantityBlocked,
      })
      continue
    }

    existing.itemCount += 1
    existing.locationCount += 1
    existing.quantityOnHand += item.quantityOnHand
    existing.quantityBlocked += item.quantityBlocked
  }

  return Array.from(grouped.values()).sort((left, right) => left.state.localeCompare(right.state))
}

export function buildOriginHistoryRows(
  inventory: LoadArrReportInventoryBalance[],
  filters?: {
    locationId?: string
    itemId?: string
    state?: string
    query?: string
  },
) {
  return inventory
    .filter((item) => {
      if (filters?.locationId && item.locationId !== filters.locationId) {
        return false
      }

      if (filters?.itemId && item.supplyarrItemId !== filters.itemId) {
        return false
      }

      if (filters?.state && item.state !== filters.state) {
        return false
      }

      if (filters?.query) {
        return matchesNeedle(
          [
            item.staffarrSiteNameSnapshot,
            item.locationNameSnapshot,
            item.supplyarrItemId,
            item.itemNameSnapshot,
            item.originEventType,
            item.originReference,
            item.notes,
            ...item.traceTags,
          ],
          filters.query,
        )
      }

      return true
    })
    .map((item) => ({
      id: item.id,
      category: 'origin' as const,
      title: `${item.originEventType.replaceAll('_', ' ')}`,
      subtitle: item.originReference,
      itemNameSnapshot: item.itemNameSnapshot,
      locationNameSnapshot: item.locationNameSnapshot,
      status: item.state,
      quantity: item.quantityOnHand,
      occurredAtUtc: '',
      reason: item.notes,
    }))
    .sort((left, right) => left.title.localeCompare(right.title))
}

export function buildMovementHistoryRows(
  counts: LoadArrReportCount[],
  adjustments: LoadArrReportAdjustment[],
  holds: LoadArrReportHold[],
  unexplainedInventory: LoadArrReportUnexplainedInventoryRecord[],
  filters?: {
    locationId?: string
    locationNameSnapshot?: string
    itemId?: string
    state?: string
    sinceUtc?: string
    untilUtc?: string
    query?: string
  },
) {
  const countRows = counts
    .filter((count) => {
      if (filters?.locationId && count.warehouseLocationId !== filters.locationId) {
        return false
      }

      if (filters?.itemId && count.supplyarrItemId !== filters.itemId) {
        return false
      }

      if (filters?.state && count.status !== filters.state) {
        return false
      }

      if (!withinRange(count.updatedAtUtc, filters?.sinceUtc, filters?.untilUtc)) {
        return false
      }

      if (filters?.query) {
        return matchesNeedle(
          [
            count.countNumber,
            count.status,
            count.countType,
            count.staffarrSiteNameSnapshot,
            count.locationNameSnapshot,
            count.itemNameSnapshot,
            count.reasonCode,
          ],
          filters.query,
        )
      }

      return true
    })
    .map((count) => ({
      id: count.id,
      category: 'count' as const,
      title: count.countNumber,
      subtitle: count.countType.replaceAll('_', ' '),
      itemNameSnapshot: count.itemNameSnapshot,
      locationNameSnapshot: count.locationNameSnapshot,
      status: count.status,
      quantity: count.varianceQuantity,
      occurredAtUtc: count.updatedAtUtc,
      reason: count.reasonCode,
    }))

  const adjustmentRows = adjustments
    .filter((adjustment) => {
      if (filters?.locationId && adjustment.warehouseLocationId !== filters.locationId) {
        return false
      }

      if (filters?.itemId && adjustment.supplyarrItemId !== filters.itemId) {
        return false
      }

      if (filters?.state && adjustment.status !== filters.state) {
        return false
      }

      if (!withinRange(adjustment.updatedAtUtc, filters?.sinceUtc, filters?.untilUtc)) {
        return false
      }

      if (filters?.query) {
        return matchesNeedle(
          [
            adjustment.adjustmentNumber,
            adjustment.status,
            adjustment.adjustmentType,
            adjustment.staffarrSiteNameSnapshot,
            adjustment.locationNameSnapshot,
            adjustment.itemNameSnapshot,
            adjustment.reasonCode,
          ],
          filters.query,
        )
      }

      return true
    })
    .map((adjustment) => ({
      id: adjustment.id,
      category: 'adjustment' as const,
      title: adjustment.adjustmentNumber,
      subtitle: adjustment.adjustmentType.replaceAll('_', ' '),
      itemNameSnapshot: adjustment.itemNameSnapshot,
      locationNameSnapshot: adjustment.locationNameSnapshot,
      status: adjustment.status,
      quantity: adjustment.quantityDelta,
      occurredAtUtc: adjustment.updatedAtUtc,
      reason: adjustment.reasonCode,
    }))

  const holdRows = holds
    .filter((hold) => {
      if (filters?.locationNameSnapshot && hold.locationNameSnapshot !== filters.locationNameSnapshot) {
        return false
      }

      if (filters?.itemId && hold.supplyarrItemId !== filters.itemId) {
        return false
      }

      if (filters?.state && hold.status !== filters.state) {
        return false
      }

      if (!withinRange(hold.openedAtUtc, filters?.sinceUtc, filters?.untilUtc)) {
        return false
      }

      if (filters?.query) {
        return matchesNeedle(
          [hold.holdType, hold.locationNameSnapshot, hold.supplyarrItemId, hold.reason, hold.sourceReference],
          filters.query,
        )
      }

      return true
    })
    .map((hold) => ({
      id: hold.id,
      category: 'hold' as const,
      title: hold.holdType.replaceAll('_', ' '),
      subtitle: hold.sourceReference,
      itemNameSnapshot: hold.supplyarrItemId,
      locationNameSnapshot: hold.locationNameSnapshot,
      status: hold.status,
      quantity: 0,
      occurredAtUtc: hold.openedAtUtc,
      reason: hold.reason,
    }))

  const unexplainedRows = unexplainedInventory
    .filter((record) => {
      if (filters?.locationId && record.warehouseLocationId !== filters.locationId) {
        return false
      }

      if (filters?.itemId && record.supplyarrItemId !== filters.itemId) {
        return false
      }

      if (filters?.state && record.status !== filters.state) {
        return false
      }

      if (!withinRange(record.discoveredAtUtc, filters?.sinceUtc, filters?.untilUtc)) {
        return false
      }

      if (filters?.query) {
        return matchesNeedle(
          [
            record.recordNumber,
            record.status,
            record.discoverySource,
            record.staffarrSiteNameSnapshot,
            record.locationNameSnapshot,
            record.itemNameSnapshot,
            record.reasonCode,
            record.resolutionState,
            record.evidenceSummary,
          ],
          filters.query,
        )
      }

      return true
    })
    .map((record) => ({
      id: record.id,
      category: 'unexplained' as const,
      title: record.recordNumber,
      subtitle: record.discoverySource.replaceAll('_', ' '),
      itemNameSnapshot: record.itemNameSnapshot,
      locationNameSnapshot: record.locationNameSnapshot,
      status: record.status,
      quantity: record.varianceQuantity,
      occurredAtUtc: record.discoveredAtUtc,
      reason: record.reasonCode,
    }))

  return [...countRows, ...adjustmentRows, ...holdRows, ...unexplainedRows].sort((left, right) =>
    right.occurredAtUtc.localeCompare(left.occurredAtUtc),
  )
}

export function buildCountVarianceRows(
  counts: LoadArrReportCount[],
  adjustments: LoadArrReportAdjustment[],
  filters?: {
    locationId?: string
    itemId?: string
    state?: string
    sinceUtc?: string
    untilUtc?: string
    query?: string
  },
) {
  const adjustmentIndex = new Map(adjustments.map((adjustment) => [adjustment.adjustmentNumber, adjustment]))

  return counts
    .filter((count) => count.varianceQuantity !== 0)
    .filter((count) => {
      if (filters?.locationId && count.warehouseLocationId !== filters.locationId) {
        return false
      }

      if (filters?.itemId && count.supplyarrItemId !== filters.itemId) {
        return false
      }

      if (filters?.state && count.status !== filters.state) {
        return false
      }

      if (!withinRange(count.updatedAtUtc, filters?.sinceUtc, filters?.untilUtc)) {
        return false
      }

      if (filters?.query) {
        return matchesNeedle(
          [
            count.countNumber,
            count.status,
            count.countType,
            count.staffarrSiteNameSnapshot,
            count.locationNameSnapshot,
            count.itemNameSnapshot,
            count.reasonCode,
            adjustmentIndex.get(count.inventoryAdjustmentId ?? '')?.reasonCode,
          ],
          filters.query,
        )
      }

      return true
    })
    .map((count) => ({
      id: count.id,
      countNumber: count.countNumber,
      countType: count.countType,
      status: count.status,
      locationNameSnapshot: count.locationNameSnapshot,
      itemNameSnapshot: count.itemNameSnapshot,
      expectedQuantity: count.expectedQuantity,
      countedQuantity: count.countedQuantity,
      varianceQuantity: count.varianceQuantity,
      unitOfMeasure: count.unitOfMeasure,
      reasonCode: count.reasonCode,
      inventoryAdjustmentId: count.inventoryAdjustmentId,
      approvedAtUtc: count.approvedAtUtc,
      updatedAtUtc: count.updatedAtUtc,
      adjustmentReasonCode: adjustmentIndex.get(count.inventoryAdjustmentId ?? '')?.reasonCode ?? '',
    }))
}
