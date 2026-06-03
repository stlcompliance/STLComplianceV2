import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { LaunchDiagnosticsPage } from './LaunchDiagnosticsPage'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      id,
      value,
      onChange,
      options,
      placeholder,
      testId,
    }: {
      label: string
      id: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      placeholder?: string
      testId?: string
    }) => (
      <label htmlFor={id} className="text-xs font-medium text-slate-600">
        {label}
        <select
          id={id}
          className="mt-1 w-full rounded-md border border-slate-300 px-2 py-1 text-sm"
          value={value}
          onChange={(event) => onChange(event.target.value)}
          data-testid={testId}
        >
          <option value="">{placeholder ?? `Select ${label.toLowerCase()}…`}</option>
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

  it('filters launch attempts by actor, correlation, and time range', async () => {
    const expectedFrom = new Date('2026-06-01T00:00').toISOString()
    const expectedTo = new Date('2026-06-03T23:59').toISOString()

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
      items: [
        {
          auditEventId: 'audit-1',
          tenantId: 'tenant-1',
          tenantSlug: 'demo-stl',
          tenantDisplayName: 'STL Demo Tenant',
          actorUserId: 'user-1',
          actorEmail: 'driver@example.com',
          actorDisplayName: 'Driver One',
          productKey: 'staffarr',
          productDisplayName: 'StaffArr',
          action: 'launch.requested',
          result: 'success',
          reasonCode: null,
          targetType: 'launch',
          targetId: 'launch-1',
          correlationId: 'corr-1',
          occurredAt: new Date().toISOString(),
          remediationHint: null,
        },
      ],
      page: 1,
      pageSize: 25,
      totalCount: 1,
      hasNextPage: false,
    })

    renderPage()

    expect(await screen.findByText('Recent launch attempts')).toBeTruthy()
    fireEvent.change(screen.getByLabelText('Actor user ID'), { target: { value: 'user-1' } })
    fireEvent.change(screen.getByLabelText('Correlation ID'), { target: { value: 'corr-1' } })
    fireEvent.change(screen.getByLabelText('From'), { target: { value: '2026-06-01T00:00' } })
    fireEvent.change(screen.getByLabelText('To'), { target: { value: '2026-06-03T23:59' } })

    await waitFor(() => {
      expect(nexarr.getPlatformAdminLaunchAttempts).toHaveBeenLastCalledWith({
        tenantId: undefined,
        productKey: undefined,
        result: undefined,
        userId: 'user-1',
        correlationId: 'corr-1',
        fromUtc: expectedFrom,
        toUtc: expectedTo,
        page: 1,
        pageSize: 25,
      })
    })
    expect(await screen.findByText('Driver One')).toBeTruthy()
    expect(screen.getByText('corr-1')).toBeTruthy()
  })

  it('filters launch attempts by tenant and product pickers', async () => {
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

    renderPage()

    expect(await screen.findByText('Recent launch attempts')).toBeTruthy()
    fireEvent.change(screen.getByLabelText('Filter tenant'), { target: { value: 'tenant-1' } })
    await waitFor(() => {
      expect(screen.getByLabelText('Filter product')).toBeInTheDocument()
    })
    fireEvent.change(screen.getByLabelText('Filter product'), { target: { value: 'staffarr' } })

    await waitFor(() => {
      expect(nexarr.getPlatformAdminLaunchAttempts).toHaveBeenLastCalledWith({
        tenantId: 'tenant-1',
        productKey: 'staffarr',
        result: undefined,
        userId: undefined,
        correlationId: undefined,
        fromUtc: undefined,
        toUtc: undefined,
        page: 1,
        pageSize: 25,
      })
    })
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

    expect(await screen.findByRole('alert')).toHaveTextContent('validation failed')
  })

  it('shows retryable callout when diagnostics query fails', async () => {
    vi.mocked(nexarr.getPlatformAdminLaunchDiagnostics).mockRejectedValueOnce(
      new Error('diagnostics unavailable'),
    )
    vi.mocked(nexarr.getPlatformAdminLaunchAttempts).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 25,
      totalCount: 0,
      hasNextPage: false,
    })

    renderPage()

    expect(await screen.findByText('diagnostics unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry diagnostics' })).toBeTruthy()
  })
})
