import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { PlatformWorkerHealthOrchestrationPanel } from './PlatformWorkerHealthOrchestrationPanel'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformWorkerHealthOrchestration: vi.fn(),
  triggerPlatformServiceTokenCleanup: vi.fn(),
  triggerPlatformEntitlementReconciliation: vi.fn(),
  triggerPlatformTenantLifecycle: vi.fn(),
  triggerPlatformOutboxPublisher: vi.fn(),
}))

function renderPanel() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <PlatformWorkerHealthOrchestrationPanel />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('PlatformWorkerHealthOrchestrationPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders health, tokens, and worker trigger controls', async () => {
    vi.mocked(nexarr.getPlatformWorkerHealthOrchestration).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      platformHealthStatus: 'Degraded',
      productHealth: [
        {
          productKey: 'staffarr',
          status: 'NotConfigured',
          readyUrl: null,
          latencyMs: null,
          errorCode: 'not_configured',
          errorMessage: null,
        },
      ],
      serviceTokens: {
        activeCount: 2,
        expiringWithin24HoursCount: 0,
        expiredRetainedCount: 1,
        revokedRetainedCount: 0,
        pendingCleanupCount: 0,
      },
      activeServiceClientCount: 1,
      workers: [
        {
          workerKey: 'service_token_cleanup',
          label: 'Service token cleanup',
          description: 'Purges expired tokens.',
          isEnabled: true,
          pendingCount: 0,
          latestRun: null,
          serviceTokenScope: 'nexarr.service_tokens.cleanup.purge',
          suiteAdminPath: '/app/platform-admin/service-tokens',
        },
        {
          workerKey: 'entitlement_reconciliation',
          label: 'Entitlement reconciliation',
          description: 'Aligns entitlements.',
          isEnabled: false,
          pendingCount: 1,
          latestRun: null,
          serviceTokenScope: 'nexarr.entitlements.reconcile',
          suiteAdminPath: '/app/platform-admin/entitlements',
        },
        {
          workerKey: 'tenant_lifecycle',
          label: 'Tenant lifecycle',
          description: 'Suspends tenants.',
          isEnabled: true,
          pendingCount: 0,
          latestRun: null,
          serviceTokenScope: 'nexarr.tenants.lifecycle.process',
          suiteAdminPath: '/app/platform-admin/tenant-lifecycle',
        },
      ],
    })

    renderPanel()

    expect(
      await screen.findByTestId('platform-worker-health-orchestration-panel'),
    ).toBeTruthy()
    const healthBadge = await screen.findByTestId('platform-orchestration-health-status')
    expect(healthBadge.textContent).toContain('Degraded')
    expect(await screen.findByTestId('platform-orchestration-service-tokens')).toBeTruthy()
    const cleanupTrigger = screen.getByTestId(
      'platform-orchestration-trigger-service-token-cleanup',
    ) as HTMLButtonElement
    expect(cleanupTrigger.disabled).toBe(false)
    const entitlementTrigger = screen.getByTestId(
      'platform-orchestration-trigger-entitlement-reconciliation',
    ) as HTMLButtonElement
    expect(entitlementTrigger.disabled).toBe(true)
    const lifecycleTrigger = screen.getByTestId(
      'platform-orchestration-trigger-tenant-lifecycle',
    ) as HTMLButtonElement
    expect(lifecycleTrigger.disabled).toBe(false)
  })

  it('renders callout when orchestration status fails', async () => {
    vi.mocked(nexarr.getPlatformWorkerHealthOrchestration).mockRejectedValueOnce(
      new Error('orchestration down'),
    )

    renderPanel()

    expect(await screen.findByText('orchestration down')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry orchestration' })).toBeTruthy()
  })
})
