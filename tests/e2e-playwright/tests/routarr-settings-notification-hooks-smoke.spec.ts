import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr settings dispatch notification hooks @requires-live', () => {
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

  test('settings notification panel loads, saves toggles and webhook URL, and shows recent dispatches', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('notification-settings-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Dispatch notifications' })).toBeVisible()

    const enabled = panel.getByTestId('notification-settings-enabled')
    const webhook = panel.getByTestId('notification-settings-webhook')
    const tripAssigned = panel.getByTestId('notification-trip-assigned')
    await expect(enabled).toBeVisible()
    await expect(webhook).toBeVisible()
    await expect(tripAssigned).toBeVisible()

    const initialEnabled = await enabled.isChecked()
    const initialWebhook = await webhook.inputValue()
    const initialTripAssigned = await tripAssigned.isChecked()
    const alternateWebhook =
      initialWebhook === 'https://hooks.example.com/routarr-e2e-w279'
        ? 'https://hooks.example.com/routarr-e2e-w279-alt'
        : 'https://hooks.example.com/routarr-e2e-w279'

    if (initialEnabled) {
      await enabled.uncheck()
    } else {
      await enabled.check()
    }
    await webhook.fill(alternateWebhook)
    if (initialTripAssigned) {
      await tripAssigned.uncheck()
    } else {
      await tripAssigned.check()
    }

    await panel.getByTestId('notification-settings-save').click()
    await expect(enabled).toHaveJSProperty('checked', !initialEnabled)
    await expect(webhook).toHaveValue(alternateWebhook)
    await expect(tripAssigned).toHaveJSProperty('checked', !initialTripAssigned)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(enabled).toHaveJSProperty('checked', !initialEnabled)
    await expect(webhook).toHaveValue(alternateWebhook)
    await expect(tripAssigned).toHaveJSProperty('checked', !initialTripAssigned)

    await expect(panel.getByRole('heading', { name: 'Recent dispatches' })).toBeVisible()
    const dispatchesEmpty = panel.getByTestId('notification-dispatches-empty')
    const dispatchesList = panel.getByTestId('notification-dispatches-list')
    await expect(dispatchesEmpty.or(dispatchesList)).toBeVisible({ timeout: 15_000 })

    if (initialEnabled) {
      await enabled.check()
    } else {
      await enabled.uncheck()
    }
    await webhook.fill(initialWebhook)
    if (initialTripAssigned) {
      await tripAssigned.check()
    } else {
      await tripAssigned.uncheck()
    }
    await panel.getByTestId('notification-settings-save').click()
    await expect(enabled).toHaveJSProperty('checked', initialEnabled)
    await expect(webhook).toHaveValue(initialWebhook)
    await expect(tripAssigned).toHaveJSProperty('checked', initialTripAssigned)
  })
})
