import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

import { SuppliersSection } from './SuppliersSection'

vi.mock('../../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/client')>()

  return {
    ...actual,
    getSupplierOnboarding: vi.fn().mockResolvedValue({
      onboardingId: 'onb-1',
      supplierId: 'supplier-1',
      supplierKey: 'sup-2048',
      supplierUnitKind: 'identity',
      parentSupplierId: null,
      parentSupplierDisplayName: null,
      displayName: 'Midwest Fleet Parts & Service',
      onboardingStatus: 'draft',
      notes: 'Awaiting insurance certificate',
      submittedAt: null,
      reviewedAt: null,
      rejectionReason: '',
      documentRequirements: [
        { documentTypeKey: 'w9', label: 'W-9 tax form', isRequired: true, isSatisfied: true, satisfyingDocumentId: 'doc-1', satisfyingReviewStatus: 'approved' },
        { documentTypeKey: 'insurance_certificate', label: 'Certificate of insurance', isRequired: true, isSatisfied: false, satisfyingDocumentId: null, satisfyingReviewStatus: null },
      ],
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    }),
    getSupplierDirectoryMetadata: vi.fn().mockResolvedValue({
      unitKindOptions: [],
      serviceTypeOptions: [],
    }),
  }
})

const supplier = {
  supplierId: 'supplier-1',
  supplierKey: 'sup-2048',
  parentSupplierId: null,
  parentSupplierDisplayName: null,
  unitKind: 'identity',
  displayName: 'Midwest Fleet Parts & Service',
  legalName: 'Midwest Fleet Parts & Service LLC',
  taxIdentifier: '12-3456789',
  approvalStatus: 'approved',
  status: 'active',
  notes: '',
  serviceTypes: ['products', 'parts'],
  addressLine1: '1200 Westport Rd',
  addressLine2: '',
  locality: 'Kansas City',
  regionCode: 'MO',
  postalCode: '64111',
  countryCode: 'US',
  childUnitCount: 1,
  contacts: [
    {
      contactId: 'contact-1',
      contactName: 'Sarah Jenkins',
      email: 'sarah@midwestfleet.example',
      phone: '(555) 774-2190',
      roleLabel: 'Account Manager',
      isPrimary: true,
      createdAt: '2026-01-03T00:00:00Z',
    },
  ],
  createdAt: '2026-01-03T00:00:00Z',
  updatedAt: '2026-06-01T00:00:00Z',
}

const subUnit = {
  ...supplier,
  supplierId: 'supplier-2',
  supplierKey: 'sup-2048-kc',
  parentSupplierId: 'supplier-1',
  parentSupplierDisplayName: 'Midwest Fleet Parts & Service',
  unitKind: 'sub_unit',
  displayName: 'Midwest Fleet Parts & Service - Kansas City',
  childUnitCount: 0,
  serviceTypes: ['parts', 'maintenance'],
}

const contract = {
  contractId: 'contract-1',
  contractKey: 'SC-2048',
  contractType: 'master_supply_agreement',
  title: 'Supply Agreement 2026',
  supplierId: 'supplier-1',
  supplierKey: 'sup-2048',
  supplierDisplayName: 'Midwest Fleet Parts & Service',
  parentSupplierId: null,
  parentSupplierDisplayName: null,
  supplierUnitKind: 'identity',
  supplierServiceTypes: ['products', 'parts'],
  effectiveAt: '2026-01-15T00:00:00Z',
  expiresAt: '2026-12-31T00:00:00Z',
  renewalAt: '2026-11-01T00:00:00Z',
  paymentTerms: 'Net 30',
  freightTerms: 'FOB destination',
  warrantyTerms: '12 months from receipt',
  minimumSpend: 25000,
  serviceLevelAgreement: '95% on-time shipment rate',
  approvalStatus: 'approved',
  status: 'active',
  notes: 'Priority partner contract',
  createdByUserId: 'user-1',
  createdAt: '2026-01-10T00:00:00Z',
  updatedAt: '2026-06-02T00:00:00Z',
}

const baseState = {
  accessToken: '',
  canManage: true,
  canApprovePr: true,
  canReadSuppliers: false,
  canReadAuditHistory: false,
  canReadSupplyReadiness: false,
  suppliersQuery: { data: [], isLoading: false },
  supplierDirectory: [],
  contractsQuery: { data: [], isLoading: false },
  partsQuery: { data: [], isLoading: false },
  purchaseOrdersQuery: { data: [], isLoading: false },
  purchaseRequestsQuery: { data: [], isLoading: false },
  supplierKey: '',
  supplierName: '',
  supplierLegalName: '',
  supplierTaxId: '',
  supplierNotes: '',
  supplierParentUnitId: '',
  supplierUnitKind: 'identity',
  supplierServiceTypes: 'products,parts',
  supplierAddressLine1: '',
  supplierLocality: '',
  supplierRegionCode: '',
  supplierPostalCode: '',
  supplierCountryCode: 'US',
  setSupplierKey: () => {},
  setSupplierName: () => {},
  setSupplierLegalName: () => {},
  setSupplierTaxId: () => {},
  setSupplierNotes: () => {},
  setSupplierParentUnitId: () => {},
  setSupplierUnitKind: () => {},
  setSupplierServiceTypes: () => {},
  setSupplierAddressLine1: () => {},
  setSupplierLocality: () => {},
  setSupplierRegionCode: () => {},
  setSupplierPostalCode: () => {},
  setSupplierCountryCode: () => {},
  createSupplierMutation: { mutate: () => {}, isPending: false },
  updateSupplierMutation: { mutate: () => {}, isPending: false },
  updateSupplierApprovalMutation: { mutate: () => {}, isPending: false },
  updateSupplierStatusMutation: { mutate: () => {}, isPending: false },
  addSupplierContactMutation: { mutate: () => {}, isPending: false },
}

function renderSuppliersSection(path = '/suppliers/drawer', state: unknown = baseState) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[path]}>
        <SuppliersSection state={state as never} />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('SuppliersSection', () => {
  it('renders the unified supplier directory workspace', () => {
    renderSuppliersSection()
    expect(screen.getByTestId('supplyarr-supplier-directory')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Supplier directory' })).toBeInTheDocument()
    expect(screen.getByText('0 supplier identities · 0 supplier sub-units')).toBeInTheDocument()
  })

  it('renders the replacement supplier profile detail view', async () => {
    const view = renderSuppliersSection('/suppliers/details', {
      ...baseState,
      accessToken: 'token',
      supplierDirectory: [supplier, subUnit],
      suppliersQuery: { data: [supplier, subUnit], isLoading: false },
      contractsQuery: { data: [contract], isLoading: false },
    } as never)

    const page = within(view.container)
    expect(page.getAllByTestId('supplyarr-supplier-profile').at(-1)).toBeInTheDocument()
    expect(page.getByRole('heading', { name: 'Midwest Fleet Parts & Service' })).toBeInTheDocument()
    expect(page.getAllByText('Supplier snapshot').at(-1)).toBeInTheDocument()
    expect(page.getByText('Contracts & terms')).toBeInTheDocument()
    expect(page.getByText('SC-2048')).toBeInTheDocument()
    expect(page.getAllByText('Supplier sub-units').at(-1)).toBeInTheDocument()
    expect(page.getByText('Sourcing readiness')).toBeInTheDocument()
    expect(page.getByText('Stock and parts sourcing')).toBeInTheDocument()
    expect(page.getByText('Use this identity when sourcing can route across multiple supplier sub-units.')).toBeInTheDocument()
    expect(page.getByText('Midwest Fleet Parts & Service - Kansas City')).toBeInTheDocument()
    expect(page.getAllByText(/Products, Parts/i).length).toBeGreaterThan(0)
    expect(page.getByText('Onboarding posture')).toBeInTheDocument()
    expect(await page.findByText('Draft')).toBeInTheDocument()
    expect(await page.findByText('1/2 approved')).toBeInTheDocument()
  })
})
