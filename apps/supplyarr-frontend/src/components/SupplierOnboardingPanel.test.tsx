import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SupplierOnboardingPanel } from './SupplierOnboardingPanel'
import * as clientApi from '../api/client'

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
  getSupplierOnboardingDocumentRequirements: vi.fn().mockResolvedValue({
    requirements: [
      { documentTypeKey: 'w9', label: 'W-9 tax form', isRequired: true },
      { documentTypeKey: 'insurance', label: 'Insurance certificate', isRequired: true },
    ],
  }),
  listPendingSupplierOnboarding: vi.fn().mockResolvedValue([]),
  getSupplierOnboardingByParty: vi.fn().mockResolvedValue(null),
  listPartyComplianceDocuments: vi.fn().mockResolvedValue([
    {
      documentId: 'doc-1',
      externalPartyId: 'p1',
      documentKey: 'w9-001',
      documentTypeKey: 'w9',
      title: 'W-9 tax form',
      version: 1,
      reviewStatus: 'approved',
      expiresAt: null,
      effectiveAt: '2026-01-01T00:00:00Z',
      fileName: 'w9.pdf',
      contentType: 'application/pdf',
      sizeBytes: 1200,
      notes: '',
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
    {
      documentId: 'doc-2',
      externalPartyId: 'p1',
      documentKey: 'coi-001',
      documentTypeKey: 'insurance',
      title: 'Insurance certificate',
      version: 1,
      reviewStatus: 'pending_review',
      expiresAt: '2026-07-01T00:00:00Z',
      effectiveAt: '2026-01-01T00:00:00Z',
      fileName: 'coi.pdf',
      contentType: 'application/pdf',
      sizeBytes: 1500,
      notes: '',
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
  ]),
  startSupplierOnboarding: vi.fn(),
  submitSupplierOnboarding: vi.fn(),
  approveSupplierOnboarding: vi.fn(),
  rejectSupplierOnboarding: vi.fn(),
  registerPartyComplianceDocument: vi.fn(),
  approvePartyComplianceDocument: vi.fn(),
}))

describe('SupplierOnboardingPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders when user can manage onboarding', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SupplierOnboardingPanel
          accessToken="token"
          canManage={true}
          canReview={true}
          onboardableParties={[
            {
              partyId: 'p1',
              partyKey: 'V-1',
              partyType: 'vendor',
              displayName: 'Vendor One',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'pending',
              status: 'active',
              notes: '',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('supplier-onboarding-panel')).toBeInTheDocument()
  })

  it('returns null when user has no onboarding permissions', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <SupplierOnboardingPanel
          accessToken="token"
          canManage={false}
          canReview={false}
          onboardableParties={[]}
        />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })

  it('shows action error callout when start onboarding fails', async () => {
    vi.mocked(clientApi.startSupplierOnboarding).mockRejectedValueOnce(new Error('start failed'))

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SupplierOnboardingPanel
          accessToken="token"
          canManage={true}
          canReview={true}
          onboardableParties={[
            {
              partyId: 'p1',
              partyKey: 'V-1',
              partyType: 'vendor',
              displayName: 'Vendor One',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'pending',
              status: 'active',
              notes: '',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )

    fireEvent.change(await screen.findByLabelText('Onboarding party'), { target: { value: 'p1' } })
    expect(await screen.findByRole('heading', { name: 'Compliance documents' })).toBeInTheDocument()
    expect(await screen.findByText(/2 document\(s\)/i)).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Required documents' })).toBeInTheDocument()
    expect(screen.getAllByText('approved')).toHaveLength(2)
    expect(screen.getAllByText('expiring soon')).toHaveLength(2)
    fireEvent.click(screen.getByRole('button', { name: /Start onboarding|Restart \/ start draft/ }))

    expect(await screen.findByText('start failed')).toBeInTheDocument()
    expect(screen.getByTestId('supplier-onboarding-action-error')).toBeInTheDocument()
  })

  it('uses a searchable party picker for onboarding selection', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SupplierOnboardingPanel
          accessToken="token"
          canManage={true}
          canReview={true}
          onboardableParties={[
            {
              partyId: 'p1',
              partyKey: 'V-1',
              partyType: 'vendor',
              displayName: 'Vendor One',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'pending',
              status: 'active',
              notes: '',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
            {
              partyId: 'p2',
              partyKey: 'S-1',
              partyType: 'supplier',
              displayName: 'Supplier Two',
              legalName: '',
              taxIdentifier: null,
              approvalStatus: 'pending',
              status: 'active',
              notes: '',
              contacts: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('supplier-onboarding-panel')).toBeInTheDocument()
    expect(screen.getByTestId('supplier-onboarding-party-picker-options')).toHaveTextContent(
      'Vendor One (vendor)',
    )
    expect(screen.getByTestId('supplier-onboarding-party-picker-options')).toHaveTextContent(
      'Supplier Two (supplier)',
    )

    fireEvent.change(screen.getByTestId('supplier-onboarding-party-picker'), {
      target: { value: 'p2' },
    })

    expect(await screen.findByLabelText('Onboarding notes')).toBeInTheDocument()
  })
})
