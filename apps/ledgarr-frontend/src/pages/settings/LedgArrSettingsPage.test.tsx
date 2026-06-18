import type { ReactNode } from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { LedgArrSettingsPage } from './LedgArrSettingsPage'

vi.mock('../../api/client', () => ({
  getLedgArrTenantSettings: vi.fn(async () => ({
    settingsVersion: 1,
    sections: [
      {
        sectionKey: 'generalLedger',
        displayName: 'General Ledger',
        description: 'General Ledger settings',
        rowVersion: 'rv-1',
        highImpactFields: ['accountingBasis'],
        value: {
          accountingBasis: 'accrual',
          baseCurrency: 'USD',
          requireJournalApproval: true,
        },
      },
    ],
  })),
  getLedgArrTenantSettingsOptions: vi.fn(async () => ({
    accounts: [{ value: '2000', label: '2000 - Accounts Payable', productKey: 'ledgarr', objectType: 'glAccount', publicKey: '2000', status: 'active' }],
    legalEntities: [],
    currencies: [{ value: 'USD', label: 'USD - US Dollar' }],
    sourceProducts: [{ value: 'maintainarr', label: 'MaintainArr' }],
    sections: [{ value: 'generalLedger', label: 'General Ledger' }],
    crossProductReferences: [],
  })),
  getLedgArrTenantSettingsAudit: vi.fn(async () => ({ items: [] })),
  validateLedgArrTenantSettingsSection: vi.fn(async () => ({ isValid: true, errors: {} })),
  updateLedgArrTenantSettingsSection: vi.fn(async () => ({
    sectionKey: 'generalLedger',
    displayName: 'General Ledger',
    description: 'General Ledger settings',
    rowVersion: 'rv-2',
    highImpactFields: ['accountingBasis'],
    value: {
      accountingBasis: 'cash',
      baseCurrency: 'USD',
      requireJournalApproval: true,
    },
  })),
  resetLedgArrTenantSettingsSection: vi.fn(async () => ({
    sectionKey: 'generalLedger',
    displayName: 'General Ledger',
    description: 'General Ledger settings',
    rowVersion: 'rv-3',
    highImpactFields: ['accountingBasis'],
    value: {
      accountingBasis: 'accrual',
      baseCurrency: 'USD',
      requireJournalApproval: true,
    },
  })),
}))

describe('LedgArrSettingsPage', () => {
  it('renders the settings workspace', async () => {
    renderWithProviders(<LedgArrSettingsPage accessToken="token" canManage />)

    await waitFor(() => expect(screen.getByText('Tenant accounting configuration')).toBeInTheDocument())
    expect(screen.getByRole('button', { name: 'Save section' })).toBeInTheDocument()
    expect(screen.getAllByText('General Ledger').length).toBeGreaterThan(0)
  })

  it('shows read-only state when manage permission is missing', async () => {
    renderWithProviders(<LedgArrSettingsPage accessToken="token" canManage={false} />)

    await waitFor(() => expect(screen.getByText(/editing is disabled/i)).toBeInTheDocument())
  })
})

function renderWithProviders(element: ReactNode) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      {element}
    </QueryClientProvider>,
  )
}
