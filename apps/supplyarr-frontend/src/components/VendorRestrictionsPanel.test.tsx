import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SupplierRestrictionsPanel } from './VendorRestrictionsPanel'

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

vi.mock('../api/client', () => ({
  listSupplierRestrictions: vi.fn().mockResolvedValue([]),
  listRestrictionsForSupplier: vi.fn().mockResolvedValue([]),
  getSupplierRestrictionEnforcement: vi.fn().mockResolvedValue({
    supplierId: 'party-1',
    supplierKey: 'acme-hq',
    supplierDisplayName: 'HQ Counter',
    parentSupplierId: 'parent-1',
    parentSupplierDisplayName: 'Acme Supply',
    supplierUnitKind: 'sub_unit',
    supplierServiceTypes: ['parts'],
    isBlocked: false,
    blockReason: null,
    activeScopes: [],
  }),
  createSupplierRestriction: vi.fn(),
  liftSupplierRestriction: vi.fn(),
}))

describe('SupplierRestrictionsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders when user can manage', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SupplierRestrictionsPanel
          accessToken="token"
          canManage={true}
          restrictableSuppliers={[
            {
              supplierId: 'party-1',
              supplierKey: 'acme-hq',
              parentSupplierId: 'parent-1',
              parentSupplierDisplayName: 'Acme Supply',
              unitKind: 'sub_unit',
              displayName: 'HQ Counter',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'approved',
              status: 'active',
              notes: '',
              serviceTypes: ['parts'],
              addressLine1: '100 Main St',
              locality: 'Tulsa',
              regionCode: 'OK',
              postalCode: '74101',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('supplier-restrictions-panel')).toBeInTheDocument()
    expect(screen.getByText('Supplier restrictions')).toBeInTheDocument()
  })

  it('uses a searchable picker for supplier hierarchy selection', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SupplierRestrictionsPanel
          accessToken="token"
          canManage={true}
          restrictableSuppliers={[
            {
              supplierId: 'party-1',
              supplierKey: 'acme-hq',
              parentSupplierId: 'parent-1',
              parentSupplierDisplayName: 'Acme Supply',
              unitKind: 'sub_unit',
              displayName: 'HQ Counter',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'approved',
              status: 'active',
              notes: '',
              serviceTypes: ['parts'],
              addressLine1: '100 Main St',
              locality: 'Tulsa',
              regionCode: 'OK',
              postalCode: '74101',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
            {
              supplierId: 'party-2',
              supplierKey: 'bravo-west',
              parentSupplierId: 'parent-2',
              parentSupplierDisplayName: 'Bravo Supply',
              unitKind: 'sub_unit',
              displayName: 'West Service Desk',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'approved',
              status: 'active',
              notes: '',
              serviceTypes: ['maintenance'],
              addressLine1: '44 Service Ave',
              locality: 'Oklahoma City',
              regionCode: 'OK',
              postalCode: '73102',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('supplier-restrictions-panel')).toBeInTheDocument()
    expect(screen.getByTestId('supplier-restriction-supplier-picker-options')).toHaveTextContent(
      'Acme Supply · HQ Counter (acme-hq) · Sub-unit',
    )
    expect(screen.getByTestId('supplier-restriction-supplier-picker-options')).toHaveTextContent(
      'Bravo Supply · West Service Desk (bravo-west) · Sub-unit',
    )

    fireEvent.change(screen.getByTestId('supplier-restriction-supplier-picker'), {
      target: { value: 'party-2' },
    })

    expect(screen.getByLabelText(/Restriction reason/i)).toBeInTheDocument()
    expect(screen.getByText(/Bravo Supply · West Service Desk \(bravo-west\) · Sub-unit · 44 Service Ave, Oklahoma City, OK, 73102 · Maintenance/i)).toBeInTheDocument()
  })

  it('returns null when user cannot manage', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <SupplierRestrictionsPanel accessToken="token" canManage={false} restrictableSuppliers={[]} />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
