import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      options,
      onChange,
      placeholder,
      testId,
    }: {
      label?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
      placeholder?: string
      testId?: string
    }) => (
      <label>
        {label ? <span>{label}</span> : null}
        <select
          aria-label={label ?? placeholder ?? 'Static search picker'}
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
      </label>
    ),
  }
})

import { OutboundShipmentsPanel } from './OutboundShipmentsPanel'

vi.mock('../api/client', () => ({
  createOutboundShipment: vi.fn(),
  getOutboundShipments: vi.fn().mockResolvedValue([
    {
      shipmentId: 'ship-1',
      shipmentKey: 'shipment-1',
      status: 'created',
      shipVia: 'manual',
      destinationName: 'Central Depot',
      destinationAddressSnapshot: '1 Depot Way',
      routarrShipmentIntentId: null,
      routarrRouteId: null,
      routarrStatus: 'pending',
      idempotencyKey: 'shipment-1-idem',
      lines: [
        {
          shipmentLineId: 'line-1',
          partId: 'part-1',
          partKey: 'filter-001',
          partDisplayName: 'Oil Filter',
          fromBinId: 'bin-1',
          fromBinKey: 'a-01',
          quantityRequested: 2,
          quantityReserved: 0,
          quantityPicked: 0,
          quantityShipped: 0,
          status: 'created',
        },
      ],
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
  ]),
}))

describe('OutboundShipmentsPanel', () => {
  it('renders outbound shipments and create workflow', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <OutboundShipmentsPanel
          accessToken="token"
          canManage={true}
          parts={[
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
          ]}
          bins={[
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
          ]}
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('outbound-shipment-list')).toBeInTheDocument()
    expect(await screen.findByText(/shipment-1/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Create shipment/i })).toBeInTheDocument()
    expect(screen.getByTestId('outbound-shipment-detail')).toBeInTheDocument()
    expect(screen.getByLabelText('Part')).toBeInTheDocument()
    expect(screen.getByLabelText('From bin')).toBeInTheDocument()
  })
})
