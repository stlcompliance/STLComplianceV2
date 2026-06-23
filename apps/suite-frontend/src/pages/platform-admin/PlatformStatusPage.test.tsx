import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { PlatformStatusPage } from './PlatformStatusPage'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformHealth: vi.fn(),
  listProducts: vi.fn(),
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <PlatformStatusPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('PlatformStatusPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders product health probes from NexArr health endpoint', async () => {
    vi.mocked(nexarr.getPlatformHealth).mockResolvedValue({
      status: 'Degraded',
      timestampUtc: '2026-06-03T08:00:00Z',
      products: [
        {
          productKey: 'staffarr',
          status: 'Healthy',
          readyUrl: 'http://localhost:5102/health/ready',
          latencyMs: 42,
          errorCode: null,
          errorMessage: null,
          detail: {
            status: 'Healthy',
            product: 'StaffArr',
            version: '2026.06.03+sha-abc123',
            timestampUtc: '2026-06-03T07:59:45Z',
            checks: {
              database: 'Healthy',
              queue: 'Healthy',
            },
          },
        },
        {
          productKey: 'routarr',
          status: 'Unhealthy',
          readyUrl: 'http://localhost:5105/health/ready',
          latencyMs: 100,
          errorCode: 'upstream_503',
          errorMessage: 'Ready probe returned a non-success status.',
          detail: {
            status: 'Degraded',
            product: 'RoutArr',
            version: '2026.06.03+sha-xyz789',
            timestampUtc: '2026-06-03T07:58:10Z',
            checks: {
              api: 'Degraded',
            },
          },
        },
      ],
    })
    vi.mocked(nexarr.listProducts).mockResolvedValue({
      items: [
        {
          productKey: 'staffarr',
          displayName: 'StaffArr',
          sortOrder: 1,
          isActive: true,
          productCategory: 'operations',
          productOwner: 'Product',
          productStatus: 'available',
          canonicalCallbackPath: '/launch/staffarr/callback',
          apiBaseUrl: 'http://localhost:5102',
          healthUrl: 'http://localhost:5102/health',
          serviceAudience: 'staffarr-api',
          marketingUrl: 'https://example.test/staffarr',
          documentationUrl: 'https://example.test/staffarr/docs',
          supportUrl: 'https://example.test/staffarr/support',
          environmentKey: 'development',
          entitlementDependencyRules: 'staffarr.requires.platform.launch',
        },
        {
          productKey: 'routarr',
          displayName: 'RoutArr',
          sortOrder: 2,
          isActive: false,
          productCategory: 'operations',
          productOwner: 'Product',
          productStatus: 'maintenance',
          canonicalCallbackPath: '/launch/routarr/callback',
          apiBaseUrl: '',
          healthUrl: '',
          serviceAudience: 'routarr-api',
          marketingUrl: '',
          documentationUrl: '',
          supportUrl: '',
          environmentKey: 'development',
          entitlementDependencyRules: 'routarr.requires.platform.launch',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 2,
      hasNextPage: false,
    })

    renderPage()

    expect(await screen.findByTestId('platform-status-overall')).toHaveTextContent('Degraded')
    expect(screen.getByTestId('platform-status-product-staffarr')).toHaveTextContent('Healthy')
    expect(screen.getByTestId('platform-status-product-routarr')).toHaveTextContent('Unhealthy')
    expect(screen.getByTestId('platform-status-registry-count')).toHaveTextContent('2 products')
    expect(screen.getByText('Registry configuration gaps')).toBeInTheDocument()
    expect(screen.getByText('RoutArr is missing API URL, health URL.')).toBeInTheDocument()
    expect(screen.getByTestId('platform-status-product-evidence-staffarr')).toHaveTextContent(
      'Deployment evidence',
    )
    expect(screen.getByTestId('platform-status-product-evidence-staffarr')).toHaveTextContent(
      'v2026.06.03+sha-abc123',
    )
    expect(screen.getByTestId('platform-status-product-evidence-staffarr')).toHaveTextContent('database')
    expect(screen.getByTestId('platform-status-product-evidence-routarr')).toHaveTextContent(
      'v2026.06.03+sha-xyz789',
    )
    expect(screen.getByTestId('platform-status-drift-state')).toHaveTextContent('Drift detected')
    expect(screen.getByText('Potential deployment skew')).toBeInTheDocument()
    expect(
      screen.getByText((_, element) => element?.textContent === '2026.06.03+sha-abc123: StaffArr'),
    ).toBeInTheDocument()
    expect(
      screen.getByText((_, element) => element?.textContent === '2026.06.03+sha-xyz789: RoutArr'),
    ).toBeInTheDocument()
    expect(screen.getAllByRole('link', { name: 'Launch diagnostics' })[0]).toHaveAttribute(
      'href',
      '/app/platform-admin/launch',
    )
    expect(screen.getAllByRole('link', { name: 'Worker health' })[0]).toHaveAttribute(
      'href',
      '/app/platform-admin/orchestration',
    )
  })

  it('shows a retryable callout when system health fails to load', async () => {
    vi.mocked(nexarr.getPlatformHealth).mockRejectedValueOnce(new Error('health offline'))

    renderPage()

    expect(await screen.findByText('health offline')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry system status' })).toBeInTheDocument()
  })
})
