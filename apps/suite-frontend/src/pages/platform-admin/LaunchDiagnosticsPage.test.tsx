import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { LaunchDiagnosticsPage } from './LaunchDiagnosticsPage'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformAdminLaunchDiagnostics: vi.fn(),
  getPlatformAdminLaunchAttempts: vi.fn(),
  validatePlatformLaunch: vi.fn(),
}))

import * as nexarr from '../../api/nexarrClient'

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <LaunchDiagnosticsPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('LaunchDiagnosticsPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('validates launch eligibility and renders response details', async () => {
    vi.mocked(nexarr.getPlatformAdminLaunchDiagnostics).mockResolvedValue({
      rows: [
        {
          tenantId: 'tenant-1',
          tenantSlug: 'demo-stl',
          tenantDisplayName: 'STL Demo Tenant',
          tenantStatus: 'active',
          productKey: 'staffarr',
          productDisplayName: 'StaffArr',
          hasActiveEntitlement: true,
          hasLaunchProfile: true,
          launchProfileActive: true,
          callbackAllowlistEntryCount: 3,
          pendingHandoffCount: 0,
          expiredHandoffCount: 0,
          launchReadiness: 'ready',
        },
      ],
      issues: [],
      generatedAt: new Date().toISOString(),
    })
    vi.mocked(nexarr.getPlatformAdminLaunchAttempts).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 25,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.validatePlatformLaunch).mockResolvedValue({
      tenantId: 'tenant-1',
      productKey: 'staffarr',
      canLaunch: false,
      reasonCode: 'entitlement_inactive',
      launchUrl: 'http://localhost:5175/launch',
    })

    renderPage()

    expect(await screen.findByText('Validate launch eligibility')).toBeTruthy()
    fireEvent.change(screen.getByLabelText('Tenant'), { target: { value: 'tenant-1' } })
    fireEvent.change(screen.getByLabelText('Product'), { target: { value: 'staffarr' } })
    fireEvent.click(screen.getByRole('button', { name: 'Validate launch' }))

    await waitFor(() => {
      expect(nexarr.validatePlatformLaunch).toHaveBeenCalled()
      expect(vi.mocked(nexarr.validatePlatformLaunch).mock.calls[0]?.[0]).toEqual({
        tenantId: 'tenant-1',
        productKey: 'staffarr',
      })
    })
    expect(await screen.findByText('No')).toBeTruthy()
    expect(screen.getByText('entitlement_inactive')).toBeTruthy()
    expect(screen.getByText('http://localhost:5175/launch')).toBeTruthy()
  })

  it('shows error message when launch validation fails', async () => {
    vi.mocked(nexarr.getPlatformAdminLaunchDiagnostics).mockResolvedValue({
      rows: [
        {
          tenantId: 'tenant-1',
          tenantSlug: 'demo-stl',
          tenantDisplayName: 'STL Demo Tenant',
          tenantStatus: 'active',
          productKey: 'staffarr',
          productDisplayName: 'StaffArr',
          hasActiveEntitlement: true,
          hasLaunchProfile: true,
          launchProfileActive: true,
          callbackAllowlistEntryCount: 3,
          pendingHandoffCount: 0,
          expiredHandoffCount: 0,
          launchReadiness: 'ready',
        },
      ],
      issues: [],
      generatedAt: new Date().toISOString(),
    })
    vi.mocked(nexarr.getPlatformAdminLaunchAttempts).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 25,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.validatePlatformLaunch).mockRejectedValue(new Error('validation failed'))

    renderPage()

    expect(await screen.findByText('Validate launch eligibility')).toBeTruthy()
    fireEvent.change(screen.getByLabelText('Tenant'), { target: { value: 'tenant-1' } })
    fireEvent.change(screen.getByLabelText('Product'), { target: { value: 'staffarr' } })
    fireEvent.click(screen.getByRole('button', { name: 'Validate launch' }))

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Failed to validate launch: validation failed',
    )
  })
})
