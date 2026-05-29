import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr settings trip completion rollup @requires-live', () => {
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

  test('settings trip completion rollup panel loads, saves staleness hours, and shows recent runs', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('trip-completion-rollup-settings-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(
      panel.getByRole('heading', { name: 'Trip completion rollup worker' }),
    ).toBeVisible()

    const enabled = panel.getByTestId('trip-completion-rollup-enabled')
    const stalenessInput = panel.getByTestId('trip-completion-rollup-staleness')
    await expect(enabled).toBeVisible()
    await expect(stalenessInput).toBeVisible()

    const initialEnabled = await enabled.isChecked()
    const initialStaleness = await stalenessInput.inputValue()
    const alternateStaleness = initialStaleness === '2' ? '3' : '2'

    if (initialEnabled) {
      await enabled.uncheck()
    } else {
      await enabled.check()
    }
    await stalenessInput.fill(alternateStaleness)

    await panel.getByTestId('trip-completion-rollup-save').click()
    await expect(enabled).toHaveJSProperty('checked', !initialEnabled)
    await expect(stalenessInput).toHaveValue(alternateStaleness)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(enabled).toHaveJSProperty('checked', !initialEnabled)
    await expect(stalenessInput).toHaveValue(alternateStaleness)

    await expect(panel.getByRole('heading', { name: 'Recent worker runs' })).toBeVisible()
    const runsEmpty = panel.getByTestId('trip-completion-rollup-runs-empty')
    const runsList = panel.getByTestId('trip-completion-rollup-runs-list')
    await expect(runsEmpty.or(runsList)).toBeVisible({ timeout: 15_000 })

    if (initialEnabled) {
      await enabled.check()
    } else {
      await enabled.uncheck()
    }
    await stalenessInput.fill(initialStaleness)
    await panel.getByTestId('trip-completion-rollup-save').click()
    await expect(enabled).toHaveJSProperty('checked', initialEnabled)
    await expect(stalenessInput).toHaveValue(initialStaleness)
  })
})
