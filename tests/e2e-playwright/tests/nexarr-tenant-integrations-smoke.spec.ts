import { test, expect } from '@playwright/test'

import {
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('NexArr tenant integrations @requires-live', () => {
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
  })

  test('integration catalog and manual mapping route load in NexArr', async ({ page }) => {
    await signInFromSuite(page)

    await page.goto('/app/nexarr/integrations')
    await expect(page.getByTestId('tenant-integrations-panel')).toBeVisible({ timeout: 15_000 })
    await expect(page.getByRole('heading', { name: 'NexArr integrations' })).toBeVisible()
    await expect(page.getByText('QuickBooks')).toBeVisible()

    await page.goto('/app/nexarr/integrations/edi-x12/mappings')
    await expect(page.getByTestId('tenant-integration-mappings')).toBeVisible({ timeout: 15_000 })
    await expect(page.getByRole('heading', { name: 'EDI X12 mappings' })).toBeVisible()
    await expect(page.getByLabel('Template name')).toHaveValue('default')
    await expect(page.getByRole('button', { name: 'Save mapping' })).toBeVisible()
  })
})
