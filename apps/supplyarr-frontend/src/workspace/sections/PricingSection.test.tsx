import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { PricingSection } from './PricingSection'

vi.mock('../../components/PricingLeadTimePanel', () => ({
  PricingLeadTimePanel: () => <div data-testid="pricing-lead-time-panel" />,
}))

vi.mock('../../components/AvailabilitySnapshotsPanel', () => ({
  AvailabilitySnapshotsPanel: () => <div data-testid="availability-snapshots-panel" />,
}))

const baseState = {
  partsQuery: { data: [], isLoading: false },
  supplierDirectory: [],
  vendorsQuery: { data: [], isLoading: false },
  pricingSnapshotsQuery: { data: [], isLoading: false },
  leadTimeSnapshotsQuery: { data: [], isLoading: false },
  availabilitySnapshotsQuery: { data: [], isLoading: false },
  canManageCatalog: true,
  pricingSnapshotKey: '',
  leadTimeSnapshotKey: '',
  selectedSnapshotVendorLinkId: '',
  snapshotUnitPrice: '',
  snapshotCurrencyCode: 'USD',
  snapshotMinimumOrderQty: '',
  snapshotLeadTimeDays: '',
  snapshotNotes: '',
  snapshotCurrentOnly: true,
  availabilitySnapshotKey: '',
  selectedAvailabilityVendorLinkId: '',
  availabilityQuantity: '',
  availabilityStatus: 'in_stock',
  availabilityNotes: '',
  availabilityCurrentOnly: true,
  setPricingSnapshotKey: () => {},
  setLeadTimeSnapshotKey: () => {},
  setSelectedSnapshotVendorLinkId: () => {},
  setSnapshotUnitPrice: () => {},
  setSnapshotCurrencyCode: () => {},
  setSnapshotMinimumOrderQty: () => {},
  setSnapshotLeadTimeDays: () => {},
  setSnapshotNotes: () => {},
  setSnapshotCurrentOnly: () => {},
  setAvailabilitySnapshotKey: () => {},
  setSelectedAvailabilityVendorLinkId: () => {},
  setAvailabilityQuantity: () => {},
  setAvailabilityStatus: () => {},
  setAvailabilityNotes: () => {},
  setAvailabilityCurrentOnly: () => {},
  createPricingSnapshotMutation: { mutate: () => {}, isPending: false },
  createLeadTimeSnapshotMutation: { mutate: () => {}, isPending: false },
  createAvailabilitySnapshotMutation: { mutate: () => {}, isPending: false },
} as never

describe('PricingSection', () => {
  it('renders pricing snapshots workspace with both panels', () => {
    render(<PricingSection state={baseState} />)
    expect(screen.getByTestId('supplyarr-pricing-snapshots-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('pricing-lead-time-panel')).toBeInTheDocument()
    expect(screen.getByTestId('availability-snapshots-panel')).toBeInTheDocument()
  })
})
