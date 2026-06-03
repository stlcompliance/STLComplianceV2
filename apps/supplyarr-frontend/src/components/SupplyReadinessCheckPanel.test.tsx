import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SupplyReadinessCheckPanel } from './SupplyReadinessCheckPanel'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()

  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      placeholder,
      testId,
    }: {
      label: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      placeholder?: string
      testId?: string
    }) => (
      <label>
        <span>{label}</span>
        <input
          aria-label={label}
          data-testid={testId}
          placeholder={placeholder}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <div data-testid={`${testId ?? 'picker'}-options`}>
          {options.map((option) => (
            <span key={option.value}>{option.label}</span>
          ))}
        </div>
      </label>
    ),
  }
})

const { partId } = vi.hoisted(() => ({
  partId: '11111111-1111-1111-1111-111111111111',
}))

vi.mock('../api/client', () => ({
  getPartSupplyReadiness: vi.fn().mockResolvedValue({
    partId,
    partKey: 'PART-1',
    displayName: 'Test part',
    status: 'active',
    readinessStatus: 'not_ready',
    readinessBasis: 'supply_blockers',
    calculatedAt: '2026-05-28T12:00:00Z',
    blockers: [
      {
        reasonCode: 'part_stockout',
        message: 'No available stock for this part.',
        sourceEntityType: 'part',
        sourceEntityId: partId,
        relatedEntityId: null,
      },
    ],
    availability: {
      quantityOnHand: 0,
      quantityReserved: 0,
      quantityAvailable: 0,
      reorderPoint: 5,
      activeReservationCount: 0,
      openBackorderCount: 0,
    },
  }),
  getVendorSupplyReadiness: vi.fn(),
  getProcurementPathReadiness: vi.fn(),
}))

describe('SupplyReadinessCheckPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders part readiness blockers when a part is selected', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <SupplyReadinessCheckPanel
          accessToken="token"
          canRead
          parts={[
            {
              partId,
              partKey: 'PART-1',
              displayName: 'Test part',
              status: 'active',
            } as never,
          ]}
          vendors={[]}
        />
      </QueryClientProvider>,
    )

    fireEvent.change(screen.getByTestId('readiness-check-part-picker'), { target: { value: partId } })

    expect(await screen.findByText('part_stockout')).toBeInTheDocument()
    expect(screen.getByText('not_ready')).toBeInTheDocument()
  })

  it('renders searchable vendor picker in vendor mode', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <SupplyReadinessCheckPanel
          accessToken="token"
          canRead
          parts={[]}
          vendors={[
            {
              partyId: 'vendor-1',
              partyKey: 'ACME',
              displayName: 'Acme Supply',
              partyType: 'vendor',
              status: 'active',
              approvalStatus: 'approved',
              legalName: '',
              taxIdentifier: null,
              notes: '',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Vendor' }))

    expect(await screen.findByTestId('readiness-check-vendor-picker')).toBeInTheDocument()
    expect(screen.getByTestId('readiness-check-vendor-picker-options')).toHaveTextContent(
      'ACME · Acme Supply',
    )
  })

  it('returns null when user cannot read', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <SupplyReadinessCheckPanel accessToken="token" canRead={false} parts={[]} vendors={[]} />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
