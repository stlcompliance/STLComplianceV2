import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi, beforeEach } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { EntitlementReconciliationSettingsPanel } from './EntitlementReconciliationSettingsPanel'

vi.mock('../../api/nexarrClient', () => ({
  getEntitlementReconciliationSettings: vi.fn(),
  upsertEntitlementReconciliationSettings: vi.fn(),
  getEntitlementReconciliationRuns: vi.fn(),
  getEntitlementReconciliationPending: vi.fn(),
}))

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <EntitlementReconciliationSettingsPanel />
    </QueryClientProvider>,
  )
}

describe('EntitlementReconciliationSettingsPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(nexarr.getEntitlementReconciliationSettings).mockResolvedValue({
      isEnabled: true,
      autoGrantFromLicense: true,
      autoRevokeStaleEntitlements: true,
      updatedAt: '2026-05-28T12:00:00Z',
    })
    vi.mocked(nexarr.getEntitlementReconciliationRuns).mockResolvedValue({ items: [] })
    vi.mocked(nexarr.getEntitlementReconciliationPending).mockResolvedValue({
      asOfUtc: '2026-05-28T12:00:00Z',
      batchSize: 10,
      items: [],
    })
  })

  it('renders reconciliation settings from API', async () => {
    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('entitlement-reconciliation-enabled')).toBeChecked()
    })
    expect(screen.getByTestId('entitlement-reconciliation-auto-grant')).toBeChecked()
    expect(screen.getByTestId('entitlement-reconciliation-auto-revoke')).toBeChecked()
    expect(screen.getByTestId('entitlement-reconciliation-runs-empty')).toBeInTheDocument()
    expect(screen.getByTestId('entitlement-reconciliation-pending-empty')).toBeInTheDocument()
  })

  it('renders callout when settings fail to load', async () => {
    vi.mocked(nexarr.getEntitlementReconciliationSettings).mockRejectedValueOnce(
      new Error('reconciliation unavailable'),
    )

    renderPanel()

    expect(await screen.findByText('reconciliation unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
