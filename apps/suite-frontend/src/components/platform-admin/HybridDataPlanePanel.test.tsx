import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { HybridDataPlanePanel } from './HybridDataPlanePanel'

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
      <label htmlFor={id} className="block text-sm text-slate-300">
        {label}
        <select
          id={id}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          data-testid={testId}
          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
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

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    getPlatformAdminTenantOverview: vi.fn(),
    getPlatformAdminProductOverview: vi.fn(),
    listDataPlaneProfiles: vi.fn(),
    listEffectiveDataPlaneProfiles: vi.fn(),
    validateDataPlaneProfile: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <HybridDataPlanePanel />
    </QueryClientProvider>,
  )
}

const tenantId = '11111111-1111-1111-1111-111111111101'

describe('HybridDataPlanePanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    vi.useRealTimers()
  })

  it('shows effective deployment map for selected tenant', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [
        {
          tenantId,
          slug: 'demo',
          displayName: 'Demo Tenant',
          status: 'Active',
          activeEntitlementCount: 1,
          membershipCount: 2,
          createdAt: '2026-01-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([
      {
        productKey: 'staffarr',
        displayName: 'StaffArr',
        isActive: true,
        activeEntitlementCount: 1,
        hasLaunchProfile: true,
        launchProfileActive: true,
        baseUrl: 'http://localhost:5175',
      },
    ])
    vi.mocked(nexarr.listDataPlaneProfiles).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 100,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.listEffectiveDataPlaneProfiles).mockResolvedValue([
      {
        tenantId,
        productKey: 'staffarr',
        productDisplayName: 'StaffArr',
        deploymentMode: 'hosted',
        trustStatus: 'trusted',
      },
    ])

    renderPanel()

    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Demo Tenant/ })).toBeInTheDocument()
    })

    await user.selectOptions(screen.getByTestId('data-plane-tenant'), tenantId)

    await waitFor(() => {
      expect(nexarr.listEffectiveDataPlaneProfiles).toHaveBeenCalledWith(tenantId)
    })

    expect(screen.getByTestId('data-plane-effective-section')).toBeInTheDocument()
    expect(screen.getByText('hosted')).toBeInTheDocument()
  })

  it('uses backend pagination for stored overrides', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [
        {
          tenantId,
          slug: 'demo',
          displayName: 'Demo Tenant',
          status: 'Active',
          activeEntitlementCount: 1,
          membershipCount: 2,
          createdAt: '2026-01-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([])
    vi.mocked(nexarr.listEffectiveDataPlaneProfiles).mockResolvedValue([])
    vi.mocked(nexarr.listDataPlaneProfiles)
      .mockResolvedValueOnce({
        items: [],
        page: 1,
        pageSize: 25,
        totalCount: 30,
        hasNextPage: true,
      })
      .mockResolvedValueOnce({
        items: [],
        page: 2,
        pageSize: 25,
        totalCount: 30,
        hasNextPage: false,
      })

    renderPanel()
    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Demo Tenant/ })).toBeInTheDocument()
    })
    await user.selectOptions(await screen.findByTestId('data-plane-tenant'), tenantId)

    await waitFor(() => {
      expect(vi.mocked(nexarr.listDataPlaneProfiles).mock.calls[0]?.[0]).toMatchObject({
        tenantId,
        page: 1,
        pageSize: 25,
      })
    })

    await user.click(screen.getByRole('button', { name: 'Next' }))

    await waitFor(() => {
      expect(vi.mocked(nexarr.listDataPlaneProfiles).mock.calls[1]?.[0]).toMatchObject({
        tenantId,
        page: 2,
        pageSize: 25,
      })
    })
  })

  it('shows retryable error callouts for effective map and overrides', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [
        {
          tenantId,
          slug: 'demo',
          displayName: 'Demo Tenant',
          status: 'Active',
          activeEntitlementCount: 1,
          membershipCount: 2,
          createdAt: '2026-01-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([])
    vi.mocked(nexarr.listDataPlaneProfiles).mockRejectedValue(new Error('overrides unavailable'))
    vi.mocked(nexarr.listEffectiveDataPlaneProfiles).mockRejectedValue(
      new Error('effective unavailable'),
    )

    renderPanel()
    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Demo Tenant/ })).toBeInTheDocument()
    })
    await user.selectOptions(await screen.findByTestId('data-plane-tenant'), tenantId)

    expect(await screen.findByText('effective unavailable')).toBeInTheDocument()
    expect(await screen.findByText('overrides unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry effective map' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry overrides' })).toBeInTheDocument()
  })

  it('validates and saves the selected data plane profile', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [
        {
          tenantId,
          slug: 'demo',
          displayName: 'Demo Tenant',
          status: 'Active',
          activeEntitlementCount: 1,
          membershipCount: 2,
          createdAt: '2026-01-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([
      {
        productKey: 'staffarr',
        displayName: 'StaffArr',
        isActive: true,
        activeEntitlementCount: 1,
        hasLaunchProfile: true,
        launchProfileActive: true,
        baseUrl: 'http://localhost:5175',
      },
    ])
    vi.mocked(nexarr.listDataPlaneProfiles).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 100,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.listEffectiveDataPlaneProfiles).mockResolvedValue([])
    vi.mocked(nexarr.validateDataPlaneProfile).mockResolvedValue({
      profile: {
        profileId: '22222222-2222-2222-2222-222222222222',
        tenantId,
        tenantSlug: 'demo',
        tenantDisplayName: 'Demo Tenant',
        productKey: 'staffarr',
        productDisplayName: 'StaffArr',
        deploymentMode: 'customer_hosted',
        dataEndpointUrl: 'https://customer.example/staffarr',
        trustStatus: 'trusted',
        notes: 'Validated customer deployment',
        modifiedAt: '2026-01-02T12:00:00Z',
      },
      validationStatus: 'Trusted',
      readyUrl: 'https://customer.example/staffarr/health/ready',
      latencyMs: 48,
      errorCode: null,
      errorMessage: null,
      validatedAt: '2026-01-02T12:00:00Z',
    })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Demo Tenant/ })).toBeInTheDocument()
    })

    await user.selectOptions(screen.getByTestId('data-plane-tenant'), tenantId)
    await user.selectOptions(screen.getByTestId('data-plane-product'), 'staffarr')
    await user.selectOptions(screen.getByTestId('data-plane-deployment-mode'), 'customer_hosted')
    await user.clear(screen.getByTestId('data-plane-endpoint'))
    await user.type(screen.getByTestId('data-plane-endpoint'), 'https://customer.example/staffarr')
    await user.clear(screen.getByTestId('data-plane-notes'))
    await user.type(screen.getByTestId('data-plane-notes'), 'Validated customer deployment')
    await user.click(screen.getByTestId('data-plane-validate'))

    await waitFor(() => {
      expect(nexarr.validateDataPlaneProfile).toHaveBeenCalledWith({
        tenantId,
        productKey: 'staffarr',
        deploymentMode: 'customer_hosted',
        dataEndpointUrl: 'https://customer.example/staffarr',
        notes: 'Validated customer deployment',
      })
    })

    expect(screen.getByTestId('data-plane-validation-result')).toHaveTextContent('Validation trusted')
    expect(screen.getByText(/Ready URL: https:\/\/customer\.example\/staffarr\/health\/ready/)).toBeInTheDocument()
    expect(screen.getByText(/Latency: 48 ms/)).toBeInTheDocument()
    expect(screen.getByTestId('data-plane-trust-status')).toHaveValue('trusted')
  }, 10000)
})
