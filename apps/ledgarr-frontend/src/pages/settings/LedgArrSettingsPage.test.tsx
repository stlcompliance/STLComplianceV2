import type { ReactNode } from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
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
  getLedgArrTenantSettingsAudit: vi.fn(async () => ({
    items: [
      {
        auditId: 'audit-1',
        sectionKey: 'generalLedger',
        changedAtUtc: '2026-06-23T12:00:00Z',
        changedByPersonId: 'person-123',
        changeReason: 'Updated audit payload summary',
        diffJson: JSON.stringify(
          {
            customNote: {
              before: 'alpha-preview',
              after: 'beta-preview',
            },
          },
          null,
          2,
        ),
      },
    ],
  })),
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

  it('requires typed confirmation before resetting a section', async () => {
    renderWithProviders(<LedgArrSettingsPage accessToken="token" canManage />)

    await waitFor(() => expect(screen.getAllByRole('button', { name: 'Reset to default' }).length).toBeGreaterThan(0))
    const resetButton = screen.getAllByRole('button', { name: 'Reset to default' })[0]
    expect(resetButton).toBeDisabled()

    fireEvent.change(screen.getAllByLabelText('Reset confirmation')[0], {
      target: { value: 'General Ledger' },
    })

    expect(resetButton).toBeEnabled()

    fireEvent.click(resetButton)

    await waitFor(() => expect(screen.getByText('Section reset to defaults.')).toBeInTheDocument())
  })

  it('summarizes audit diffs and keeps raw payload behind advanced details', async () => {
    renderWithProviders(<LedgArrSettingsPage accessToken="token" canManage />)

    const showAuditButton = (await screen.findAllByRole('button', { name: 'Show audit' }))[0]
    fireEvent.click(showAuditButton)

    await screen.findByText('Audit history')
    await waitFor(() => expect(screen.getByText('Custom Note')).toBeInTheDocument())
    expect(screen.getByText('alpha-preview')).toBeInTheDocument()
    expect(screen.getByText('beta-preview')).toBeInTheDocument()

    const rawPayload = screen.getByText(/"customNote"/)
    expect(rawPayload).not.toBeVisible()

    fireEvent.click(screen.getByText('Advanced technical details'))

    expect(rawPayload).toBeVisible()
    expect(screen.getByText('Changed by person ID')).toBeInTheDocument()
    expect(screen.getByText('person-123')).toBeInTheDocument()
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
