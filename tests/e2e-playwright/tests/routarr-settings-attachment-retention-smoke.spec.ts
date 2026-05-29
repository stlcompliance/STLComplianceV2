import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr settings attachment retention @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }
  })

  test('settings attachment retention panel loads, saves retention days, and shows recent runs', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('attachment-retention-settings-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(
      panel.getByRole('heading', { name: 'Trip capture attachment retention' }),
    ).toBeVisible()

    const enabled = panel.getByTestId('attachment-retention-enabled')
    const daysInput = panel.getByTestId('attachment-retention-days')
    await expect(enabled).toBeVisible()
    await expect(daysInput).toBeVisible()

    const initialEnabled = await enabled.isChecked()
    const initialDays = await daysInput.inputValue()
    const alternateDays = initialDays === '366' ? '367' : '366'

    if (initialEnabled) {
      await enabled.uncheck()
    } else {
      await enabled.check()
    }
    await daysInput.fill(alternateDays)

    await panel.getByTestId('attachment-retention-save').click()
    await expect(enabled).toHaveJSProperty('checked', !initialEnabled)
    await expect(daysInput).toHaveValue(alternateDays)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(enabled).toHaveJSProperty('checked', !initialEnabled)
    await expect(daysInput).toHaveValue(alternateDays)

    await expect(panel.getByRole('heading', { name: 'Recent retention runs' })).toBeVisible()
    const runsEmpty = panel.getByTestId('attachment-retention-runs-empty')
    const runsList = panel.getByTestId('attachment-retention-runs-list')
    await expect(runsEmpty.or(runsList)).toBeVisible({ timeout: 15_000 })

    if (initialEnabled) {
      await enabled.check()
    } else {
      await enabled.uncheck()
    }
    await daysInput.fill(initialDays)
    await panel.getByTestId('attachment-retention-save').click()
    await expect(enabled).toHaveJSProperty('checked', initialEnabled)
    await expect(daysInput).toHaveValue(initialDays)
  })
})
