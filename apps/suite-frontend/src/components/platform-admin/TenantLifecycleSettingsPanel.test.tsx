import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi, beforeEach } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { TenantLifecycleSettingsPanel } from './TenantLifecycleSettingsPanel'

vi.mock('../../api/nexarrClient', () => ({
  getTenantLifecycleSettings: vi.fn(),
  upsertTenantLifecycleSettings: vi.fn(),
  getTenantLifecycleRuns: vi.fn(),
  getTenantLifecyclePending: vi.fn(),
}))

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <TenantLifecycleSettingsPanel />
    </QueryClientProvider>,
  )
}

describe('TenantLifecycleSettingsPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(nexarr.getTenantLifecycleSettings).mockResolvedValue({
      isEnabled: true,
      autoSuspendWhenNoValidLicense: true,
      suspendGraceDaysAfterLastLicenseExpiry: 7,
      autoReactivateWhenValidLicense: true,
      revokeSessionsOnSuspend: true,
      updatedAt: '2026-05-28T12:00:00Z',
    })
    vi.mocked(nexarr.getTenantLifecycleRuns).mockResolvedValue({ items: [] })
    vi.mocked(nexarr.getTenantLifecyclePending).mockResolvedValue({
      asOfUtc: '2026-05-28T12:00:00Z',
      batchSize: 10,
      items: [],
    })
  })

  it('renders lifecycle settings from API', async () => {
    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('tenant-lifecycle-enabled')).toBeChecked()
    })
    expect(screen.getByText(/License-based tenant suspension is retired under the fixed-suite launch model/i)).toBeInTheDocument()
    expect(screen.getByTestId('tenant-lifecycle-auto-suspend')).toBeChecked()
    expect(screen.getByTestId('tenant-lifecycle-auto-suspend')).toBeDisabled()
    expect(screen.getByTestId('tenant-lifecycle-grace-days')).toHaveValue(7)
    expect(screen.getByTestId('tenant-lifecycle-grace-days')).toBeDisabled()
    expect(screen.getByTestId('tenant-lifecycle-auto-reactivate')).toBeDisabled()
    expect(screen.getByTestId('tenant-lifecycle-runs-empty')).toBeInTheDocument()
    expect(screen.getByTestId('tenant-lifecycle-pending-empty')).toBeInTheDocument()
  })

  it('renders an API error callout when settings fail to load', async () => {
    vi.mocked(nexarr.getTenantLifecycleSettings).mockRejectedValueOnce(new Error('settings unavailable'))

    renderPanel()

    expect(await screen.findByText('settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
