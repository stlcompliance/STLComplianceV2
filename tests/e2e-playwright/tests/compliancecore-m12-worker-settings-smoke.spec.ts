import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('Compliance Core M12 worker settings @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('compliancecore'))) {
      testInfo.skip(true, 'Compliance Core frontend (5177) is unreachable.')
    }
  })

  test('admin M12 analytics worker settings load, save, and show last-run placeholders', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await page.goto(new URL('/admin', page.url()).toString())

    const panel = page.getByTestId('compliancecore-m12-analytics-worker-settings-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(page.getByRole('heading', { name: 'M12 analytics worker' })).toBeVisible()

    const enabled = page.getByTestId('compliancecore-m12-worker-enabled')
    await expect(enabled).toBeVisible()
    await enabled.check()

    await page.getByTestId('compliancecore-m12-worker-scope').fill('tenant')
    await page.getByTestId('compliancecore-m12-worker-interval').fill('24')
    await page.getByTestId('compliancecore-m12-worker-forecast').check()
    await page.getByTestId('compliancecore-m12-worker-audit-delivery').check()

    await page.getByTestId('compliancecore-m12-worker-save').click()

    const lastRuns = page.getByTestId('compliancecore-m12-worker-last-runs')
    await expect(lastRuns).toBeVisible({ timeout: 15_000 })
    await expect(lastRuns).toContainText(/Last batch/i)
    await expect(lastRuns).toContainText(/Last readiness forecast/i)
    await expect(lastRuns).toContainText(/Last audit delivery/i)
  })
})
