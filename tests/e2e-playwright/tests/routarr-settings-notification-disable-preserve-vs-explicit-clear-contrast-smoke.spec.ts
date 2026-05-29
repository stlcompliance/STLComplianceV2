import { test, expect } from '@playwright/test'

import {
  assertRoutArrDispatchNotificationSettingsMatch,
  routArrDispatchNotificationDisableContrastWebhookUrl,
  seedRoutArrDispatchNotificationSettingsForDisableContrast,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr settings notification disable preserve vs explicit clear contrast @requires-live', () => {
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

  test('one session compares preserve-on-disable then explicit-clear paths', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('notification-settings-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })

    const enabled = panel.getByTestId('notification-settings-enabled')
    const webhook = panel.getByTestId('notification-settings-webhook')
    const webhookError = panel.getByTestId('notification-settings-webhook-error')
    const clearOnDisable = panel.getByTestId('notification-settings-clear-webhook-on-disable')
    const save = panel.getByTestId('notification-settings-save')

    const initialEnabled = await enabled.isChecked()
    const initialWebhook = await webhook.inputValue()

    await seedRoutArrDispatchNotificationSettingsForDisableContrast()

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(enabled).toBeChecked()
    await expect(webhook).toHaveValue(routArrDispatchNotificationDisableContrastWebhookUrl)
    await expect(clearOnDisable).toHaveCount(0)

    await assertRoutArrDispatchNotificationSettingsMatch({
      isEnabled: true,
      notificationWebhookUrl: routArrDispatchNotificationDisableContrastWebhookUrl,
    })

    // Phase 1 (W293): disable without explicit clear preserves webhook in API and UI.
    await enabled.uncheck()
    await expect(webhookError).toHaveCount(0)
    await expect(clearOnDisable).toBeVisible()
    await expect(clearOnDisable).not.toBeChecked()
    await save.click()
    await expect(enabled).not.toBeChecked()
    await expect(clearOnDisable).toHaveCount(0)

    await assertRoutArrDispatchNotificationSettingsMatch({
      isEnabled: false,
      notificationWebhookUrl: routArrDispatchNotificationDisableContrastWebhookUrl,
    })

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(enabled).not.toBeChecked()
    await expect(webhook).toHaveValue(routArrDispatchNotificationDisableContrastWebhookUrl)
    await expect(webhookError).toHaveCount(0)

    // Phase 2 (W298): re-enable then disable with explicit clear removes webhook from API and UI.
    await enabled.check()
    await expect(webhook).toHaveValue(routArrDispatchNotificationDisableContrastWebhookUrl)
    await expect(webhookError).toHaveCount(0)

    await enabled.uncheck()
    await expect(clearOnDisable).toBeVisible()
    await clearOnDisable.check()
    await expect(webhook).toHaveValue('')
    await save.click()
    await expect(enabled).not.toBeChecked()
    await expect(clearOnDisable).toHaveCount(0)

    await assertRoutArrDispatchNotificationSettingsMatch({
      isEnabled: false,
      notificationWebhookUrl: null,
    })

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(enabled).not.toBeChecked()
    await expect(webhook).toHaveValue('')
    await expect(webhookError).toHaveCount(0)

    await assertRoutArrDispatchNotificationSettingsMatch({
      isEnabled: false,
      notificationWebhookUrl: null,
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
    await save.click()
    await expect(enabled).toHaveJSProperty('checked', initialEnabled)
    await expect(webhook).toHaveValue(initialWebhook)
    await expect(webhookError).toHaveCount(0)

    await assertRoutArrDispatchNotificationSettingsMatch({
      isEnabled: initialEnabled,
      notificationWebhookUrl: initialWebhook.trim() || null,
    })
  })
})
