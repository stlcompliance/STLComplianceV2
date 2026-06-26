import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ServiceTokenAdminPanel } from './ServiceTokenAdminPanel'

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    listServiceClients: vi.fn(),
    listServiceTokens: vi.fn(),
    getServiceTokenAuditHistory: vi.fn(),
    getServiceTokenDiscovery: vi.fn(),
    getPlatformAdminTenantOverview: vi.fn(),
    getPlatformAdminProductOverview: vi.fn(),
    rotateServiceClient: vi.fn(),
    revokeServiceClient: vi.fn(),
    updateServiceClientAudience: vi.fn(),
    updateServiceClientTenantScope: vi.fn(),
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
          allowedTenantIds: [],
          isActive: true,
          createdAt: '2026-05-01T00:00:00Z',
          lastUsedAt: '2026-05-01T12:00:00Z',
          failedAuthenticationAttempts: 2,
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
    vi.mocked(nexarr.rotateServiceClient).mockResolvedValue(undefined)
    vi.mocked(nexarr.revokeServiceClient).mockResolvedValue(undefined)
    vi.mocked(nexarr.getServiceTokenAuditHistory).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
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
    vi.mocked(nexarr.getServiceTokenDiscovery).mockResolvedValue({
      issuer: 'https://api.example.test',
      audience: 'nexarr-service',
      jwksUri: 'https://api.example.test/api/v1/.well-known/jwks.json',
      supportedAlgorithms: ['RS256'],
      publicKeyAvailable: true,
    })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('service-token-clients-list')).toBeInTheDocument()
    })

    expect(screen.getByTestId('service-token-discovery-key-status')).toHaveTextContent('JWKS available')
    expect(screen.getByText('Issuer')).toBeInTheDocument()
    expect(screen.getByText('https://api.example.test')).toBeInTheDocument()
    expect(screen.getByText('StaffArr Worker')).toBeInTheDocument()
    expect(screen.getByText(/failed auth 2/)).toBeInTheDocument()
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
    vi.mocked(nexarr.getServiceTokenDiscovery).mockResolvedValue({
      issuer: 'https://api.example.test',
      audience: 'nexarr-service',
      jwksUri: 'https://api.example.test/api/v1/.well-known/jwks.json',
      supportedAlgorithms: ['RS256'],
      publicKeyAvailable: true,
    })
    vi.mocked(nexarr.getServiceTokenAuditHistory).mockResolvedValue({
      items: [
        {
          auditEventId: 'audit-1',
          tenantId: null,
          actorUserId: null,
          action: 'service_token.revoke',
          targetType: 'service_token',
          targetId: 'token-1',
          result: 'Success',
          reasonCode: null,
          correlationId: 'corr-1',
          occurredAt: '2026-05-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.rotateServiceClient).mockResolvedValue(undefined)
    vi.mocked(nexarr.revokeServiceClient).mockResolvedValue(undefined)

    renderPanel()

    await waitFor(() => {
      expect(nexarr.listServiceClients).toHaveBeenCalled()
      expect(nexarr.listServiceTokens).toHaveBeenCalled()
      expect(nexarr.getServiceTokenAuditHistory).toHaveBeenCalled()
    })
    expect(vi.mocked(nexarr.listServiceClients).mock.calls[0]).toEqual([1, 25])
    expect(vi.mocked(nexarr.listServiceTokens).mock.calls[0]).toEqual([undefined, 1, 25])
    expect(vi.mocked(nexarr.getServiceTokenAuditHistory).mock.calls[0]).toEqual([
      { page: 1, pageSize: 10, serviceClientId: undefined, tenantId: undefined },
    ])
    expect(screen.getByText('Service token audit history')).toBeInTheDocument()
    expect(await screen.findByText('service_token.revoke')).toBeInTheDocument()

    const nextButtons = await screen.findAllByRole('button', { name: 'Next' })
    fireEvent.click(nextButtons[0]!)
    fireEvent.click(nextButtons[1]!)

    await waitFor(() => {
      expect(vi.mocked(nexarr.listServiceClients).mock.calls[1]).toEqual([2, 25])
      expect(vi.mocked(nexarr.listServiceTokens).mock.calls[1]).toEqual([undefined, 2, 25])
    })
  })

  it('can rotate and revoke service clients', async () => {
    vi.mocked(nexarr.listServiceClients).mockResolvedValue({
      items: [
        {
          serviceClientId: 'client-1',
          clientKey: 'staffarr-worker',
          displayName: 'StaffArr Worker',
          sourceProductKey: 'staffarr',
          allowedProductKeys: ['staffarr'],
          allowedTenantIds: ['tenant-1'],
          isActive: true,
          createdAt: '2026-05-01T00:00:00Z',
          lastUsedAt: '2026-05-01T12:00:00Z',
          failedAuthenticationAttempts: 3,
        },
      ],
      page: 1,
      pageSize: 25,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.listServiceTokens).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 25,
      totalCount: 0,
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
    vi.mocked(nexarr.getServiceTokenDiscovery).mockResolvedValue({
      issuer: 'https://api.example.test',
      audience: 'nexarr-service',
      jwksUri: 'https://api.example.test/api/v1/.well-known/jwks.json',
      supportedAlgorithms: ['RS256'],
      publicKeyAvailable: true,
    })
    vi.mocked(nexarr.getServiceTokenAuditHistory).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.rotateServiceClient).mockResolvedValue(undefined)
    vi.mocked(nexarr.revokeServiceClient).mockResolvedValue(undefined)

    renderPanel()

    await screen.findByText('StaffArr Worker')

    fireEvent.click(screen.getByRole('button', { name: 'Rotate' }))
    fireEvent.click(screen.getByRole('button', { name: 'Rotate client' }))

    await waitFor(() => {
      expect(nexarr.rotateServiceClient).toHaveBeenCalledWith('client-1')
    })

    fireEvent.click(screen.getByRole('button', { name: 'Revoke' }))
    fireEvent.click(screen.getByRole('button', { name: 'Revoke client' }))

    await waitFor(() => {
      expect(nexarr.revokeServiceClient).toHaveBeenCalledWith('client-1')
    })
  })

  it('can update the selected service client audience and tenant scope', async () => {
    vi.mocked(nexarr.listServiceClients).mockResolvedValue({
      items: [
        {
          serviceClientId: 'client-1',
          clientKey: 'staffarr-worker',
          displayName: 'StaffArr Worker',
          sourceProductKey: 'staffarr',
          allowedProductKeys: ['staffarr'],
          allowedTenantIds: ['tenant-1'],
          isActive: true,
          createdAt: '2026-05-01T00:00:00Z',
          lastUsedAt: '2026-05-01T12:00:00Z',
          failedAuthenticationAttempts: 0,
        },
      ],
      page: 1,
      pageSize: 25,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.listServiceTokens).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 25,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [
        {
          tenantId: 'tenant-1',
          slug: 'alpha',
          displayName: 'Alpha',
          status: 'active',
          launchableDestinationCount: 0,
          membershipCount: 0,
          createdAt: '2026-05-01T00:00:00Z',
        },
        {
          tenantId: 'tenant-2',
          slug: 'beta',
          displayName: 'Beta',
          status: 'active',
          launchableDestinationCount: 0,
          membershipCount: 0,
          createdAt: '2026-05-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 2,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([
      {
        productKey: 'staffarr',
        displayName: 'StaffArr',
        isActive: true,
        activeTenantDestinationCount: 0,
        hasLaunchProfile: true,
        launchProfileActive: true,
        baseUrl: 'https://app.stlcompliance.com/staffarr',
      },
      {
        productKey: 'loadarr',
        displayName: 'LoadArr',
        isActive: true,
        activeTenantDestinationCount: 0,
        hasLaunchProfile: true,
        launchProfileActive: true,
        baseUrl: 'https://app.stlcompliance.com/loadarr',
      },
    ])
    vi.mocked(nexarr.getServiceTokenAuditHistory).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.updateServiceClientAudience).mockResolvedValue({
      serviceClientId: 'client-1',
      clientKey: 'staffarr-worker',
      displayName: 'StaffArr Worker',
      sourceProductKey: 'staffarr',
      allowedProductKeys: ['staffarr', 'loadarr'],
      allowedTenantIds: ['tenant-1', 'tenant-2'],
      isActive: true,
      createdAt: '2026-05-01T00:00:00Z',
      lastUsedAt: '2026-05-01T12:00:00Z',
      failedAuthenticationAttempts: 0,
    })
    vi.mocked(nexarr.updateServiceClientTenantScope).mockResolvedValue(undefined)

    renderPanel()

    await screen.findByText('StaffArr Worker')

    fireEvent.click(screen.getByRole('button', { name: 'Manage' }))

    const audiencePanel = screen.getByTestId('service-token-edit-allowed-products')
    const tenantPanel = screen.getByTestId('service-token-edit-allowed-tenants')

    fireEvent.click(within(audiencePanel).getByLabelText('LoadArr'))
    fireEvent.click(within(tenantPanel).getByLabelText('Beta (beta)'))

    fireEvent.click(screen.getByRole('button', { name: 'Save audience' }))
    fireEvent.click(screen.getByRole('button', { name: 'Save tenant scope' }))

    await waitFor(() => {
      expect(nexarr.updateServiceClientAudience).toHaveBeenCalledWith('client-1', ['staffarr', 'loadarr'])
    })
    await waitFor(() => {
      expect(nexarr.updateServiceClientTenantScope).toHaveBeenCalledWith('client-1', ['tenant-1', 'tenant-2'])
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
    vi.mocked(nexarr.getServiceTokenDiscovery).mockResolvedValue({
      issuer: 'https://api.example.test',
      audience: 'nexarr-service',
      jwksUri: 'https://api.example.test/api/v1/.well-known/jwks.json',
      supportedAlgorithms: ['RS256'],
      publicKeyAvailable: true,
    })
    vi.mocked(nexarr.getServiceTokenAuditHistory).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      hasNextPage: false,
    })

    renderPanel()

    expect(await screen.findByText('clients unavailable')).toBeInTheDocument()
    expect(await screen.findByText('tokens unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry clients' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry tokens' })).toBeInTheDocument()
  })
})
