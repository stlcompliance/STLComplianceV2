import { afterEach, describe, expect, it, vi } from 'vitest'

import {
  configureNexarrClient,
  getMe,
  getPlatformAdminDashboard,
  getPlatformAdminLaunchDiagnostics,
  getPlatformAdminProductOverview,
  getPlatformAdminTenantOverview,
  getPlatformAuditPackageExportSummary,
  getPlatformLifecycleOverview,
  getPlatformWorkerHealthOrchestration,
} from './nexarrClient'

describe('nexarrClient worker normalization', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    sessionStorage.clear()
    configureNexarrClient({
      getAccessToken: () => null,
    })
  })

  it('returns canonical lifecycle worker copy on lifecycle overview responses', async () => {
    configureNexarrClient({
      getAccessToken: () => 'test-token',
    })

    const fetchMock = vi.fn().mockResolvedValue(
      new Response(
        JSON.stringify({
          generatedAt: '2026-06-25T12:00:00Z',
          workers: [
            {
              workerKey: 'launch_destination_reconciliation',
              label: 'Launch destination reconciliation',
              description:
                'Maintains compatibility-era launch-destination snapshots for audit and support workflows.',
              isEnabled: false,
              pendingCount: 1,
              latestRun: null,
              serviceTokenScope: 'nexarr.launch_destination.reconcile',
              platformSettingsPath: '/api/platform-admin/launch-destination-reconciliation/settings',
              suiteAdminPath: '/app/platform-admin',
            },
          ],
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )
    vi.stubGlobal('fetch', fetchMock)

    const response = await getPlatformLifecycleOverview()

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/platform-admin/platform-lifecycle/overview',
      expect.objectContaining({
        headers: expect.any(Headers),
      }),
    )
    expect(response.workers[0]).toMatchObject({
      workerKey: 'launch_destination_reconciliation',
      label: 'Launch destination reconciliation',
      description:
        'Maintains compatibility-era launch-destination snapshots for audit and support workflows.',
      serviceTokenScope: 'nexarr.launch_destination.reconcile',
      platformSettingsPath: '/api/platform-admin/launch-destination-reconciliation/settings',
    })
  })

  it('returns canonical worker health orchestration copy when the canonical worker key is present', async () => {
    configureNexarrClient({
      getAccessToken: () => 'test-token',
    })

    const fetchMock = vi.fn().mockResolvedValue(
      new Response(
        JSON.stringify({
          generatedAt: '2026-06-25T12:00:00Z',
          platformHealthStatus: 'Healthy',
          productHealth: [],
          serviceTokens: {
            activeCount: 1,
            expiringWithin24HoursCount: 0,
            expiredRetainedCount: 0,
            revokedRetainedCount: 0,
            pendingCleanupCount: 0,
          },
          activeServiceClientCount: 1,
          workers: [
            {
              workerKey: 'launch_destination_reconciliation',
              label: 'Launch destination reconciliation',
              description:
                'Maintains compatibility-era launch-destination snapshots for audit and support workflows.',
              isEnabled: false,
              pendingCount: 2,
              latestRun: null,
              serviceTokenScope: 'nexarr.launch_destination.reconcile',
              suiteAdminPath: '/app/platform-admin',
            },
          ],
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )
    vi.stubGlobal('fetch', fetchMock)

    const response = await getPlatformWorkerHealthOrchestration()

    expect(response.workers[0]).toMatchObject({
      workerKey: 'launch_destination_reconciliation',
      label: 'Launch destination reconciliation',
      description:
        'Maintains compatibility-era launch-destination snapshots for audit and support workflows.',
      serviceTokenScope: 'nexarr.launch_destination.reconcile',
    })
  })
})

describe('nexarrClient profile normalization', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    sessionStorage.clear()
    configureNexarrClient({
      getAccessToken: () => null,
    })
  })

  it('reads launchable product keys from current profile responses', async () => {
    configureNexarrClient({
      getAccessToken: () => 'test-token',
    })

    const fetchMock = vi.fn().mockResolvedValue(
      new Response(
        JSON.stringify({
          userId: 'user-1',
          email: 'user@example.com',
          displayName: 'Test User',
          activeTenantId: 'tenant-1',
          isPlatformAdmin: false,
          launchableProductKeys: ['suite', 'trainarr'],
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )
    vi.stubGlobal('fetch', fetchMock)

    const response = await getMe()

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/me',
      expect.objectContaining({
        headers: expect.any(Headers),
      }),
    )
    expect(response.launchableProductKeys).toEqual(['suite', 'trainarr'])
  })
})

describe('nexarrClient legacy alias normalization', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    sessionStorage.clear()
    configureNexarrClient({
      getAccessToken: () => null,
    })
  })

  it('normalizes legacy availability-shaped dashboard counts into launch context counts', async () => {
    configureNexarrClient({
      getAccessToken: () => 'test-token',
    })

    const fetchMock = vi.fn().mockResolvedValue(
      new Response(
        JSON.stringify({
          tenantCount: 2,
          activeTenantCount: 1,
          productCount: 4,
          activeProductCount: 3,
          activeAvailabilityCount: 6,
          totalAvailabilityCount: 8,
          serviceClientCount: 1,
          activeServiceTokenCount: 1,
          launchProfileCount: 2,
          pendingHandoffCount: 0,
          expiredUnredeemedHandoffCount: 0,
          auditEventsLast24Hours: 5,
          generatedAt: '2026-06-25T12:00:00Z',
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )
    vi.stubGlobal('fetch', fetchMock)

    const response = await getPlatformAdminDashboard()

    expect(response.activeLaunchableDestinationCount).toBe(6)
    expect(response.totalLaunchableDestinationCount).toBe(8)
    expect('activeAvailabilityCount' in response).toBe(false)
    expect('totalAvailabilityCount' in response).toBe(false)
  })

  it('normalizes legacy tenant, product, and launch-diagnostic rows', async () => {
    configureNexarrClient({
      getAccessToken: () => 'test-token',
    })

    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            items: [
              {
                tenantId: 'tenant-1',
                slug: 'alpha',
                displayName: 'Alpha',
                status: 'active',
                activeAvailabilityCount: 3,
                membershipCount: 2,
                createdAt: '2026-06-25T12:00:00Z',
              },
            ],
            page: 1,
            pageSize: 50,
            totalCount: 1,
            hasNextPage: false,
          }),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      )
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify([
            {
              productKey: 'staffarr',
              displayName: 'StaffArr',
              isActive: true,
              activeAvailabilityCount: 3,
              hasLaunchProfile: true,
              launchProfileActive: true,
              baseUrl: 'https://staffarr.example.test',
            },
          ]),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      )
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            rows: [
              {
                tenantId: 'tenant-1',
                tenantSlug: 'alpha',
                tenantDisplayName: 'Alpha',
                tenantStatus: 'active',
                productKey: 'staffarr',
                productDisplayName: 'StaffArr',
                hasActiveAvailability: true,
                hasLaunchProfile: true,
                launchProfileActive: true,
                callbackAllowlistEntryCount: 1,
                pendingHandoffCount: 0,
                expiredHandoffCount: 0,
                launchReadiness: 'ready',
              },
            ],
            issues: [],
            generatedAt: '2026-06-25T12:00:00Z',
          }),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      )
    vi.stubGlobal('fetch', fetchMock)

    const [tenantResponse, productResponse, diagnosticsResponse] = await Promise.all([
      getPlatformAdminTenantOverview(),
      getPlatformAdminProductOverview(),
      getPlatformAdminLaunchDiagnostics(),
    ])

    expect(tenantResponse.items[0]?.launchableDestinationCount).toBe(3)
    expect(productResponse[0]?.activeTenantDestinationCount).toBe(3)
    expect(diagnosticsResponse.rows[0]?.isLaunchableDestination).toBe(true)
    expect('activeAvailabilityCount' in (tenantResponse.items[0] ?? {})).toBe(false)
    expect('activeAvailabilityCount' in (productResponse[0] ?? {})).toBe(false)
    expect('hasActiveAvailability' in (diagnosticsResponse.rows[0] ?? {})).toBe(false)
  })

  it('normalizes legacy audit export count aliases behind the client boundary', async () => {
    configureNexarrClient({
      getAccessToken: () => 'test-token',
    })

    const fetchMock = vi.fn().mockResolvedValue(
      new Response(
        JSON.stringify({
          filters: {
            tenantId: null,
            from: null,
            to: null,
            action: null,
            result: null,
            targetType: null,
            actorUserId: null,
            productKey: null,
          },
          counts: {
            auditEvents: 3,
            tenants: 1,
            tenantAvailabilityRecords: 7,
            productCatalog: 2,
            platformUsers: 1,
            serviceClients: 1,
            serviceTokens: 0,
            launchProfiles: 2,
            callbackAllowlist: 1,
          },
          byResult: [],
          byAction: [],
          generatedAt: '2026-06-25T12:00:00Z',
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )
    vi.stubGlobal('fetch', fetchMock)

    const response = await getPlatformAuditPackageExportSummary()

    expect(response.counts.tenantLaunchDestinations).toBe(7)
    expect('tenantAvailabilityRecords' in response.counts).toBe(false)
  })
})
