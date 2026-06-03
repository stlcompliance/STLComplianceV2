import { describe, expect, it } from 'vitest'
import {
  buildCountVarianceRows,
  buildInventoryByItemRows,
  buildInventoryByLocationRows,
  buildInventoryByStatusRows,
  buildMovementHistoryRows,
  buildOriginHistoryRows,
} from './reports'

const inventory = [
  {
    id: 'inv-1',
    supplyarrItemId: 'SUP-001',
    itemNameSnapshot: 'Valve Kit',
    unitOfMeasureSnapshot: 'each',
    state: 'available',
    locationId: 'loc-1',
    locationNameSnapshot: 'Receiving Dock',
    staffarrSiteNameSnapshot: 'North Yard',
    quantityOnHand: 10,
    quantityReserved: 2,
    quantityAllocated: 1,
    quantityBlocked: 0,
    originEventType: 'purchase_receipt',
    originReference: 'PO-1',
    traceTags: ['lot:A'],
    notes: 'Ready',
  },
  {
    id: 'inv-2',
    supplyarrItemId: 'SUP-002',
    itemNameSnapshot: 'Rotor Assembly',
    unitOfMeasureSnapshot: 'each',
    state: 'blocked',
    locationId: 'loc-1',
    locationNameSnapshot: 'Receiving Dock',
    staffarrSiteNameSnapshot: 'North Yard',
    quantityOnHand: 4,
    quantityReserved: 0,
    quantityAllocated: 0,
    quantityBlocked: 4,
    originEventType: 'route_return',
    originReference: 'RT-9',
    traceTags: ['truck_stock'],
    notes: 'Hold',
  },
]

const counts = [
  {
    id: 'count-1',
    countNumber: 'CNT-1',
    status: 'approved',
    countType: 'cycle_count',
    staffarrSiteNameSnapshot: 'North Yard',
    warehouseLocationId: 'loc-1',
    locationNameSnapshot: 'Receiving Dock',
    supplyarrItemId: 'SUP-001',
    itemNameSnapshot: 'Valve Kit',
    expectedQuantity: 8,
    countedQuantity: 10,
    varianceQuantity: 2,
    unitOfMeasure: 'each',
    reasonCode: 'cycle_count_variance',
    createdAtUtc: '2026-06-03T12:00:00Z',
    completedAtUtc: '2026-06-03T12:05:00Z',
    approvedAtUtc: '2026-06-03T12:10:00Z',
    updatedAtUtc: '2026-06-03T12:10:00Z',
    inventoryAdjustmentId: 'ADJ-1',
  },
]

const adjustments = [
  {
    id: 'adj-1',
    adjustmentNumber: 'ADJ-1',
    status: 'approved',
    adjustmentType: 'gain',
    staffarrSiteNameSnapshot: 'North Yard',
    warehouseLocationId: 'loc-1',
    locationNameSnapshot: 'Receiving Dock',
    supplyarrItemId: 'SUP-001',
    itemNameSnapshot: 'Valve Kit',
    quantityDelta: 2,
    unitOfMeasure: 'each',
    reasonCode: 'cycle_count_variance',
    createdAtUtc: '2026-06-03T12:00:00Z',
    approvedAtUtc: '2026-06-03T12:10:00Z',
    updatedAtUtc: '2026-06-03T12:10:00Z',
  },
]

describe('loadarr reports helpers', () => {
  it('aggregates inventory by location, item, and status', () => {
    expect(buildInventoryByLocationRows(inventory)).toEqual([
      {
        locationId: 'loc-1',
        locationNameSnapshot: 'Receiving Dock',
        staffarrSiteNameSnapshot: 'North Yard',
        itemCount: 2,
        quantityOnHand: 14,
        quantityReserved: 2,
        quantityAllocated: 1,
        quantityBlocked: 4,
        activeStates: ['available', 'blocked'],
      },
    ])

    expect(buildInventoryByItemRows(inventory)).toEqual([
      {
        supplyarrItemId: 'SUP-002',
        itemNameSnapshot: 'Rotor Assembly',
        unitOfMeasureSnapshot: 'each',
        locationCount: 1,
        quantityOnHand: 4,
        quantityReserved: 0,
        quantityAllocated: 0,
        quantityBlocked: 4,
        stateCount: 1,
      },
      {
        supplyarrItemId: 'SUP-001',
        itemNameSnapshot: 'Valve Kit',
        unitOfMeasureSnapshot: 'each',
        locationCount: 1,
        quantityOnHand: 10,
        quantityReserved: 2,
        quantityAllocated: 1,
        quantityBlocked: 0,
        stateCount: 1,
      },
    ])

    expect(buildInventoryByStatusRows(inventory)).toEqual([
      {
        state: 'available',
        itemCount: 1,
        locationCount: 1,
        quantityOnHand: 10,
        quantityBlocked: 0,
      },
      {
        state: 'blocked',
        itemCount: 1,
        locationCount: 1,
        quantityOnHand: 4,
        quantityBlocked: 4,
      },
    ])
  })

  it('builds origin, movement, and variance report rows', () => {
    expect(buildOriginHistoryRows(inventory)).toHaveLength(2)
    expect(
      buildMovementHistoryRows(
        counts,
        adjustments,
        [
          {
            id: 'hold-1',
            holdType: 'quality',
            supplyarrItemId: 'SUP-001',
            locationNameSnapshot: 'Receiving Dock',
            status: 'open',
            reason: 'SDS mismatch',
            sourceReference: 'CC-1',
            openedAtUtc: '2026-06-03T12:15:00Z',
          },
        ],
        [
          {
            id: 'ui-1',
            recordNumber: 'UI-1',
            status: 'needs_review',
            discoverySource: 'cycle_count_variance',
            staffarrSiteOrgUnitId: 'site-1',
            staffarrSiteNameSnapshot: 'North Yard',
            warehouseLocationId: 'loc-1',
            locationNameSnapshot: 'Receiving Dock',
            supplyarrItemId: 'SUP-001',
            itemNameSnapshot: 'Valve Kit',
            expectedQuantity: 8,
            quantity: 10,
            varianceQuantity: 2,
            unitOfMeasure: 'each',
            reasonCode: 'cycle_count_variance',
            evidenceSummary: 'Variance located during count',
            resolutionState: 'needs_investigation',
            discoveredAtUtc: '2026-06-03T12:20:00Z',
            resolvedAtUtc: null,
          },
        ],
      ),
    ).toHaveLength(4)

    expect(buildCountVarianceRows(counts, adjustments)).toEqual([
      {
        id: 'count-1',
        countNumber: 'CNT-1',
        countType: 'cycle_count',
        status: 'approved',
        locationNameSnapshot: 'Receiving Dock',
        itemNameSnapshot: 'Valve Kit',
        expectedQuantity: 8,
        countedQuantity: 10,
        varianceQuantity: 2,
        unitOfMeasure: 'each',
        reasonCode: 'cycle_count_variance',
        inventoryAdjustmentId: 'ADJ-1',
        approvedAtUtc: '2026-06-03T12:10:00Z',
        updatedAtUtc: '2026-06-03T12:10:00Z',
        adjustmentReasonCode: 'cycle_count_variance',
      },
    ])
  })
})
