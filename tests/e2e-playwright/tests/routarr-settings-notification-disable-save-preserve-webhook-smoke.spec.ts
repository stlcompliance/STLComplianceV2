import { test, expect } from '@playwright/test'

import {
  assertRoutArrDispatchNotificationSettingsMatch,
  routArrDispatchNotificationDisableSavePreserveWebhookUrl,
  seedRoutArrDispatchNotificationSettingsForDisableSavePreserve,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr settings notification disable-save preserves webhook @requires-live', () => {
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

  test('disable and save keeps last webhook URL in API while disabled', async ({ page }) => {
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

    await seedRoutArrDispatchNotificationSettingsForDisableSavePreserve()

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(enabled).toBeChecked()
    await expect(webhook).toHaveValue(routArrDispatchNotificationDisableSavePreserveWebhookUrl)

    await assertRoutArrDispatchNotificationSettingsMatch({
      isEnabled: true,
      notificationWebhookUrl: routArrDispatchNotificationDisableSavePreserveWebhookUrl,
    })

    await enabled.uncheck()
    await expect(webhookError).toHaveCount(0)
    await panel.getByTestId('notification-settings-save').click()
    await expect(enabled).not.toBeChecked()

    await assertRoutArrDispatchNotificationSettingsMatch({
      isEnabled: false,
      notificationWebhookUrl: routArrDispatchNotificationDisableSavePreserveWebhookUrl,
    })

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(enabled).not.toBeChecked()
    await expect(webhook).toHaveValue(routArrDispatchNotificationDisableSavePreserveWebhookUrl)
    await expect(webhookError).toHaveCount(0)

    await assertRoutArrDispatchNotificationSettingsMatch({
      isEnabled: false,
      notificationWebhookUrl: routArrDispatchNotificationDisableSavePreserveWebhookUrl,
    })

    await enabled.check()
    await expect(webhook).toHaveValue(routArrDispatchNotificationDisableSavePreserveWebhookUrl)
    await expect(webhookError).toHaveCount(0)

    await assertRoutArrDispatchNotificationSettingsMatch({
      isEnabled: false,
      notificationWebhookUrl: routArrDispatchNotificationDisableSavePreserveWebhookUrl,
    })

    if (initialEnabled) {
      await enabled.check()
      if (initialWebhook) {
        await webhook.fill(initialWebhook)
      }
    } else {
      await enabled.uncheck()
      if (initialWebhook) {
        await webhook.fill(initialWebhook)
      }
    }
    await panel.getByTestId('notification-settings-save').click()
    await expect(enabled).toHaveJSProperty('checked', initialEnabled)
    await expect(webhook).toHaveValue(initialWebhook)
    await expect(webhookError).toHaveCount(0)

    await assertRoutArrDispatchNotificationSettingsMatch({
      isEnabled: initialEnabled,
      notificationWebhookUrl: initialWebhook.trim() || null,
    })
  })
})
