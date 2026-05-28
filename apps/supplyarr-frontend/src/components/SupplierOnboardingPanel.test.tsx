import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { SupplierOnboardingPanel } from './SupplierOnboardingPanel'

vi.mock('../api/client', () => ({
  getSupplierOnboardingDocumentRequirements: vi.fn().mockResolvedValue({
    requirements: [
      { documentTypeKey: 'w9', label: 'W-9 tax form', isRequired: true },
    ],
  }),
  listPendingSupplierOnboarding: vi.fn().mockResolvedValue([]),
  getSupplierOnboardingByParty: vi.fn(),
  startSupplierOnboarding: vi.fn(),
  submitSupplierOnboarding: vi.fn(),
  approveSupplierOnboarding: vi.fn(),
  rejectSupplierOnboarding: vi.fn(),
  registerPartyComplianceDocument: vi.fn(),
  approvePartyComplianceDocument: vi.fn(),
}))

describe('SupplierOnboardingPanel', () => {
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
})
