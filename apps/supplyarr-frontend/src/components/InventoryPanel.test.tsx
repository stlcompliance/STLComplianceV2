import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { InventoryPanel } from './InventoryPanel'

const baseProps = {
  locations: [
    {
      locationId: 'loc-1',
      locationKey: 'main-wh',
      name: 'Main Warehouse',
      locationType: 'warehouse',
      addressLine: '100 Dock St',
      status: 'active',
      binCount: 1,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
  ],
  bins: [
    {
      binId: 'bin-1',
      locationId: 'loc-1',
      locationKey: 'main-wh',
      binKey: 'a-01',
      name: 'Aisle 01',
      status: 'active',
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
  ],
  stockLevels: [
    {
      stockLevelId: 'stock-1',
      partId: 'part-1',
      partKey: 'filter-001',
      partDisplayName: 'Oil Filter',
      binId: 'bin-1',
      binKey: 'a-01',
      binName: 'Aisle 01',
      locationId: 'loc-1',
      locationKey: 'main-wh',
      locationName: 'Main Warehouse',
      quantityOnHand: 12,
      quantityReserved: 0,
      quantityAvailable: 12,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
  ],
  parts: [
    {
      partId: 'part-1',
      partKey: 'filter-001',
      catalogId: null,
      catalogKey: null,
      displayName: 'Oil Filter',
      description: '',
      categoryKey: 'filters',
      unitOfMeasure: 'each',
      manufacturerName: '',
      manufacturerPartNumber: '',
      status: 'active',
      reorderPoint: null,
      reorderQuantity: null,
      manufacturerAliases: [],
      vendorLinks: [],
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
  ],
  canManage: false,
  isLoading: false,
  locationKey: '',
  locationName: '',
  locationType: 'warehouse',
  addressLine: '',
  binKey: '',
  binName: '',
  selectedLocationId: 'loc-1',
  selectedPartId: '',
  selectedBinId: '',
  stockQuantity: '',
  onLocationKeyChange: () => {},
  onLocationNameChange: () => {},
  onLocationTypeChange: () => {},
  onAddressLineChange: () => {},
  onBinKeyChange: () => {},
  onBinNameChange: () => {},
  onSelectedLocationIdChange: () => {},
  onSelectedPartIdChange: () => {},
  onSelectedBinIdChange: () => {},
  onStockQuantityChange: () => {},
  onCreateLocation: () => {},
  onCreateBin: () => {},
  onUpsertStock: () => {},
  isCreatingLocation: false,
  isCreatingBin: false,
  isUpsertingStock: false,
}

describe('InventoryPanel', () => {
  it('renders locations, bins, and stock levels', () => {
    render(<InventoryPanel {...baseProps} />)

    expect(screen.getAllByText(/Main Warehouse/i).length).toBeGreaterThan(0)
    expect(screen.getByText(/Aisle 01/i)).toBeTruthy()
    expect(screen.getByText(/Oil Filter/i)).toBeTruthy()
    expect(screen.getByText(/on hand 12/i)).toBeTruthy()
  })
})
