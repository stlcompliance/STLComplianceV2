import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr settings integration events @requires-live', () => {
  test.beforeEach(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(
        true,
        'Suite frontend (5174) and NexArr API (5101) must be running. Use scripts/ops/e2e-stack-up.ps1 and e2e-frontends-preview.ps1.',
      )
    }
    if (!(await isHandoffFrontendReachable('supplyarr'))) {
      testInfo.skip(true, 'SupplyArr frontend (5179) is unreachable.')
    }
  })

  test('settings integration event panel saves; readiness dashboard loads metrics', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const integrationPanel = page.getByTestId('integration-event-settings-panel')
    await integrationPanel.scrollIntoViewIfNeeded()
    await expect(integrationPanel).toBeVisible({ timeout: 15_000 })
    await expect(
      integrationPanel.getByRole('heading', { name: 'Integration event outbox / inbox' }),
    ).toBeVisible()

    const enabled = integrationPanel.getByRole('checkbox', { name: 'Enabled' })
    await expect(enabled).toBeVisible()
    await enabled.check()

    const retryInterval = integrationPanel.getByLabel('Retry interval (minutes)')
    await retryInterval.fill('20')

    await integrationPanel.getByRole('button', { name: 'Save settings' }).click()
    await expect(integrationPanel.getByText('Recent outbox')).toBeVisible({ timeout: 15_000 })

    await page.goto(new URL('/readiness', page.url()).toString())

    const readinessPanel = page.getByTestId('supply-readiness-dashboard-panel')
    await expect(readinessPanel).toBeVisible({ timeout: 15_000 })
    await expect(readinessPanel.getByRole('heading', { name: 'Supply readiness' })).toBeVisible()
    await expect(readinessPanel.getByText('Available qty')).toBeVisible({ timeout: 15_000 })
  })
})
