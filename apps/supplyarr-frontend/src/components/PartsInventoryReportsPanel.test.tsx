import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')
  return {
    ...actual,
    StaticSearchPicker: ({
      placeholder,
      value,
      options,
      onChange,
      testId,
    }: {
      placeholder?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
      testId?: string
    }) => (
      <select
        aria-label={placeholder ?? 'Static search picker'}
        data-testid={testId}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      >
        <option value="">{placeholder ?? 'Select…'}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    ),
  }
})

import { PartsInventoryReportsPanel } from './PartsInventoryReportsPanel'

vi.mock('../api/client', () => ({
  getPartsInventoryReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    totals: {
      totalPartCount: 2,
      activePartCount: 2,
      locationCount: 1,
      binCount: 2,
      belowReorderPointCount: 1,
      zeroStockPartCount: 0,
      totalQuantityOnHand: 15,
      totalQuantityReserved: 2,
      totalQuantityAvailable: 13,
    },
    locations: [
      {
        inventoryLocationId: 'loc-1',
        locationKey: 'WH-01',
        name: 'Main warehouse',
        status: 'active',
        binCount: 2,
        partCountWithStock: 1,
        quantityOnHand: 15,
        quantityReserved: 2,
        quantityAvailable: 13,
      },
    ],
    parts: [
      {
        partId: 'part-1',
        partKey: 'PART-001',
        displayName: 'Hydraulic filter',
        status: 'active',
        categoryKey: 'filters',
        reorderPoint: 10,
        reorderQuantity: 5,
        quantityOnHand: 8,
        quantityReserved: 2,
        quantityAvailable: 6,
        belowReorderPoint: true,
        supplierLinkCount: 1,
      },
    ],
  }),
  getPartsInventoryPartDetail: vi.fn(),
  getPartsInventoryLocationDetail: vi.fn(),
  exportPartsInventoryReportSummaryCsv: vi.fn(),
}))

describe('PartsInventoryReportsPanel', () => {
  it('renders parts and inventory summary', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <PartsInventoryReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('parts-inventory-reports-panel')).toBeInTheDocument()
    expect(await screen.findByText(/PART-001 · Hydraulic filter/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Export parts CSV/i })).toBeInTheDocument()
    expect(screen.getByTestId('parts-inventory-location-filter')).toBeInTheDocument()
  })

  it('returns null when user cannot read reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <PartsInventoryReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
