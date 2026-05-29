import { test, expect } from '@playwright/test'

import { isLiveModeEnabled, isLiveStackReachable, signInFromSuite } from '../support/liveProbe.js'

test.describe('Suite platform-admin worker health orchestration @requires-live', () => {
  test.beforeEach(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(
        true,
        'Suite frontend (5174) and NexArr API (5101) must be running. Start scripts/ops/e2e-stack-up.ps1 and e2e-frontends-preview.ps1.',
      )
    }
  })

  test('orchestration panel shows product health, token inventory, workers, and trigger controls', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await page.goto('/app/platform-admin/orchestration')

    const panel = page.getByTestId('platform-worker-health-orchestration-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(
      page.getByRole('heading', { name: 'Service token & worker health' }),
    ).toBeVisible()

    await expect(panel.getByText('Loading orchestration status')).not.toBeVisible({
      timeout: 15_000,
    })

    const productHealth = panel.getByTestId('platform-orchestration-product-health')
    await expect(productHealth).toBeVisible()
    await expect(panel.getByTestId('platform-orchestration-health-status')).toBeVisible()
    await expect(productHealth.getByTestId('platform-orchestration-health-staffarr')).toBeVisible()

    const tokens = panel.getByTestId('platform-orchestration-service-tokens')
    await expect(tokens).toBeVisible()
    await expect(tokens.getByText('Active')).toBeVisible()
    await expect(tokens.getByText('Pending cleanup')).toBeVisible()

    const cleanupWorker = panel.getByTestId('platform-orchestration-worker-service_token_cleanup')
    await expect(cleanupWorker).toBeVisible()
    await expect(cleanupWorker.getByText(/Pending \(sample\):/i)).toBeVisible()
    const cleanupHasHistory =
      (await cleanupWorker.getByText(/Last run/i).count()) > 0 ||
      (await cleanupWorker.getByText(/No batch runs recorded yet/i).count()) > 0
    expect(cleanupHasHistory).toBeTruthy()

    const entitlementWorker = panel.getByTestId(
      'platform-orchestration-worker-entitlement_reconciliation',
    )
    await expect(entitlementWorker).toBeVisible()
    await expect(entitlementWorker.getByText(/Enabled|Disabled/)).toBeVisible()

    const lifecycleWorker = panel.getByTestId('platform-orchestration-worker-tenant_lifecycle')
    await expect(lifecycleWorker).toBeVisible()
    await expect(lifecycleWorker.getByRole('link', { name: 'Open settings →' })).toBeVisible()

    const cleanupTrigger = panel.getByTestId('platform-orchestration-trigger-service-token-cleanup')
    await expect(cleanupTrigger).toBeVisible()

    const entitlementTrigger = panel.getByTestId(
      'platform-orchestration-trigger-entitlement-reconciliation',
    )
    await expect(entitlementTrigger).toBeVisible()

    const lifecycleTrigger = panel.getByTestId('platform-orchestration-trigger-tenant-lifecycle')
    await expect(lifecycleTrigger).toBeVisible()
  })
})
