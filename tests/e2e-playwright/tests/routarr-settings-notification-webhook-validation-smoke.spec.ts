import { test, expect } from '@playwright/test'

import {
  assertRoutArrDispatchNotificationSettingsUpsertRejected,
  routArrDispatchNotificationWebhookValidationInvalidUrl,
  routArrDispatchNotificationWebhookValidationValidUrl,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr settings notification webhook validation @requires-live', () => {
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

  test('notification settings panel blocks empty and invalid webhook when enabled', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('notification-settings-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })

    const enabled = panel.getByTestId('notification-settings-enabled')
    const webhook = panel.getByTestId('notification-settings-webhook')
    const webhookError = panel.getByTestId('notification-settings-webhook-error')

    const initialEnabled = await enabled.isChecked()
    const initialWebhook = await webhook.inputValue()

    await enabled.check()
    await webhook.fill('')
    await panel.getByTestId('notification-settings-save').click()

    await expect(webhookError).toBeVisible()
    await expect(webhookError).toContainText(/required when dispatch notifications are enabled/i)
    await expect(panel.getByTestId('notification-settings-save-error')).toHaveCount(0)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(enabled).toHaveJSProperty('checked', initialEnabled)
    await expect(webhook).toHaveValue(initialWebhook)

    await assertRoutArrDispatchNotificationSettingsUpsertRejected({
      isEnabled: true,
      notificationWebhookUrl: null,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
    })

    await enabled.check()
    await webhook.fill(routArrDispatchNotificationWebhookValidationInvalidUrl)
    await panel.getByTestId('notification-settings-save').click()

    await expect(webhookError).toBeVisible()
    await expect(webhookError).toContainText(/absolute URL/i)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(webhook).toHaveValue(initialWebhook)

    await assertRoutArrDispatchNotificationSettingsUpsertRejected({
      isEnabled: true,
      notificationWebhookUrl: routArrDispatchNotificationWebhookValidationInvalidUrl,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
    })

    await enabled.check()
    await webhook.fill(routArrDispatchNotificationWebhookValidationValidUrl)
    await panel.getByTestId('notification-settings-save').click()
    await expect(webhook).toHaveValue(routArrDispatchNotificationWebhookValidationValidUrl)
    await expect(webhookError).toHaveCount(0)

    await webhook.fill(initialWebhook)
    if (initialEnabled) {
      await enabled.check()
    } else {
      await enabled.uncheck()
    }
    await panel.getByTestId('notification-settings-save').click()
    await expect(enabled).toHaveJSProperty('checked', initialEnabled)
    await expect(webhook).toHaveValue(initialWebhook)
  })
})
