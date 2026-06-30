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

import { EmergencyPurchasePanel } from './EmergencyPurchasePanel'

vi.mock('../api/client', () => ({
  getEmergencyPurchases: vi.fn().mockResolvedValue([]),
  listPendingEmergencyPurchases: vi.fn().mockResolvedValue([]),
  createEmergencyPurchase: vi.fn(),
  expeditedSubmitEmergencyPurchase: vi.fn(),
  managerOverrideApproveEmergencyPurchase: vi.fn(),
  issueEmergencyPurchaseOrder: vi.fn(),
}))

describe('EmergencyPurchasePanel', () => {
  it('renders when user can create emergency purchases', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <EmergencyPurchasePanel
          accessToken="token"
          canCreate={true}
          canOverrideApprove={true}
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
              createdAt: '',
              updatedAt: '',
            },
          ]}
          suppliers={[
            {
              supplierId: 'vendor-1',
              partyId: 'vendor-1',
              displayName: 'North Yard Counter',
              supplierKey: 'acme-north-yard',
              parentSupplierDisplayName: 'Acme Supply',
              unitKind: 'sub_unit',
            },
          ]}
        />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('emergency-purchase-panel')).toBeInTheDocument()
    expect(screen.getByTestId('emergency-purchase-supplier-unit-picker')).toHaveTextContent('Acme Supply')
    expect(screen.getByTestId('emergency-purchase-part-picker')).toHaveTextContent('Oil Filter')
    expect(screen.getByLabelText('Supplier identity or sub-unit')).toBeInTheDocument()
  })

  it('returns null when user has no emergency permissions', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <EmergencyPurchasePanel
          accessToken="token"
          canCreate={false}
          canOverrideApprove={false}
          parts={[]}
          suppliers={[]}
        />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
