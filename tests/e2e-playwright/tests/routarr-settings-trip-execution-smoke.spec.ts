import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr settings trip execution capture policy @requires-live', () => {
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

  test('settings trip execution panel loads policy toggles, saves, and persists', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('trip-execution-settings-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(
      panel.getByRole('heading', { name: 'Trip proof & DVIR capture policy' }),
    ).toBeVisible()

    await expect(
      panel.getByRole('checkbox', { name: 'Require pre-trip DVIR before start' }),
    ).toBeVisible()
    await expect(
      panel.getByRole('checkbox', { name: 'Require pickup proof before start' }),
    ).toBeVisible()
    await expect(
      panel.getByRole('checkbox', { name: 'Block start when pre-trip DVIR is fail' }),
    ).toBeVisible()
    await expect(
      panel.getByRole('checkbox', { name: 'Require post-trip DVIR before complete' }),
    ).toBeVisible()
    await expect(
      panel.getByRole('checkbox', { name: 'Require delivery proof before complete' }),
    ).toBeVisible()
    await expect(
      panel.getByRole('checkbox', { name: 'Block complete when post-trip DVIR is fail' }),
    ).toBeVisible()

    await expect(panel.getByText('Attachment requirements')).toBeVisible()
    await expect(
      panel.getByRole('checkbox', { name: 'Require pickup proof photo before start' }),
    ).toBeVisible()
    await expect(
      panel.getByRole('checkbox', { name: 'Require pre-trip DVIR photo before start' }),
    ).toBeVisible()
    await expect(
      panel.getByRole('checkbox', { name: 'Require delivery proof photo before complete' }),
    ).toBeVisible()
    await expect(
      panel.getByRole('checkbox', { name: 'Require delivery signature before complete' }),
    ).toBeVisible()
    await expect(
      panel.getByRole('checkbox', { name: 'Require post-trip DVIR photo before complete' }),
    ).toBeVisible()

    const postTripDvir = panel.getByRole('checkbox', {
      name: 'Require post-trip DVIR before complete',
    })
    const initialChecked = await postTripDvir.isChecked()

    if (initialChecked) {
      await postTripDvir.uncheck()
    } else {
      await postTripDvir.check()
    }

    await panel.getByRole('button', { name: 'Save capture policy' }).click()
    await expect(postTripDvir).toHaveJSProperty('checked', !initialChecked)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(postTripDvir).toHaveJSProperty('checked', !initialChecked)

    if (initialChecked) {
      await postTripDvir.check()
    } else {
      await postTripDvir.uncheck()
    }
    await panel.getByRole('button', { name: 'Save capture policy' }).click()
    await expect(postTripDvir).toHaveJSProperty('checked', initialChecked)
  })
})
