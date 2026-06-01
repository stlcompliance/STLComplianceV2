import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ServiceTokenAdminPanel } from './ServiceTokenAdminPanel'

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    listServiceClients: vi.fn(),
    listServiceTokens: vi.fn(),
    getPlatformAdminTenantOverview: vi.fn(),
    getPlatformAdminProductOverview: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <ServiceTokenAdminPanel />
    </QueryClientProvider>,
  )
}

describe('ServiceTokenAdminPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders clients and issued tokens', async () => {
    vi.mocked(nexarr.listServiceClients).mockResolvedValue({
      items: [
        {
          serviceClientId: 'client-1',
          clientKey: 'staffarr-worker',
          displayName: 'StaffArr Worker',
          sourceProductKey: 'staffarr',
          allowedProductKeys: ['staffarr'],
          isActive: true,
          createdAt: '2026-05-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.listServiceTokens).mockResolvedValue({
      items: [
        {
          tokenId: 'token-1',
          serviceClientId: 'client-1',
          clientKey: 'staffarr-worker',
          tenantId: null,
          allowedProductKeys: ['staffarr'],
          actionScope: 'read',
          expiresAt: '2026-05-02T00:00:00Z',
          revokedAt: null,
          createdAt: '2026-05-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 50,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 100,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([])

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('service-token-clients-list')).toBeInTheDocument()
    })

    expect(screen.getByText('StaffArr Worker')).toBeInTheDocument()
    expect(screen.getByTestId('service-token-list')).toBeInTheDocument()
  })

  it('uses backend pagination for clients and tokens', async () => {
    vi.mocked(nexarr.listServiceClients)
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
    vi.mocked(nexarr.listServiceTokens)
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
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 100,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([])

    renderPanel()

    await waitFor(() => {
      expect(nexarr.listServiceClients).toHaveBeenCalled()
      expect(nexarr.listServiceTokens).toHaveBeenCalled()
    })
    expect(vi.mocked(nexarr.listServiceClients).mock.calls[0]).toEqual([1, 25])
    expect(vi.mocked(nexarr.listServiceTokens).mock.calls[0]).toEqual([undefined, 1, 25])

    const nextButtons = await screen.findAllByRole('button', { name: 'Next' })
    fireEvent.click(nextButtons[0]!)
    fireEvent.click(nextButtons[1]!)

    await waitFor(() => {
      expect(vi.mocked(nexarr.listServiceClients).mock.calls[1]).toEqual([2, 25])
      expect(vi.mocked(nexarr.listServiceTokens).mock.calls[1]).toEqual([undefined, 2, 25])
    })
  })

  it('shows retryable query errors for clients and tokens', async () => {
    vi.mocked(nexarr.listServiceClients).mockRejectedValue(new Error('clients unavailable'))
    vi.mocked(nexarr.listServiceTokens).mockRejectedValue(new Error('tokens unavailable'))
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 100,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([])

    renderPanel()

    expect(await screen.findByText('clients unavailable')).toBeInTheDocument()
    expect(await screen.findByText('tokens unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry clients' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry tokens' })).toBeInTheDocument()
  })
})
