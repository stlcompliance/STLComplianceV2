import { test, expect } from '@playwright/test'

import {
  assertRoutArrDispatchNotificationSettingsMatch,
  routArrDispatchNotificationReEnableAfterExplicitClearNewWebhookUrl,
  routArrDispatchNotificationReEnableAfterExplicitClearOriginalWebhookUrl,
  seedRoutArrDispatchNotificationSettingsDisabledAfterExplicitClear,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe(
  'RoutArr settings notification re-enable after explicit clear empty webhook @requires-live',
  () => {
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

    test('re-enable after explicit clear shows empty webhook and requires new URL before save', async ({
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
      const save = panel.getByTestId('notification-settings-save')

      const initialEnabled = await enabled.isChecked()
      const initialWebhook = await webhook.inputValue()

      await seedRoutArrDispatchNotificationSettingsDisabledAfterExplicitClear()

      await page.reload()
      await expect(panel).toBeVisible({ timeout: 15_000 })
      await expect(enabled).not.toBeChecked()
      await expect(webhook).toHaveValue('')
      await expect(webhookError).toHaveCount(0)

      await assertRoutArrDispatchNotificationSettingsMatch({
        isEnabled: false,
        notificationWebhookUrl: null,
      })

      await enabled.check()
      await expect(enabled).toBeChecked()
      await expect(webhook).toHaveValue('')
      await expect(webhook).not.toHaveValue(
        routArrDispatchNotificationReEnableAfterExplicitClearOriginalWebhookUrl,
      )
      await expect(webhookError).toHaveCount(0)

      await save.click()
      await expect(webhookError).toBeVisible()
      await expect(webhookError).toContainText(/required when dispatch notifications are enabled/i)

      await assertRoutArrDispatchNotificationSettingsMatch({
        isEnabled: false,
        notificationWebhookUrl: null,
      })

      await webhook.fill(routArrDispatchNotificationReEnableAfterExplicitClearNewWebhookUrl)
      await expect(webhookError).toHaveCount(0)
      await save.click()
      await expect(enabled).toBeChecked()
      await expect(webhook).toHaveValue(
        routArrDispatchNotificationReEnableAfterExplicitClearNewWebhookUrl,
      )
      await expect(webhookError).toHaveCount(0)

      await assertRoutArrDispatchNotificationSettingsMatch({
        isEnabled: true,
        notificationWebhookUrl: routArrDispatchNotificationReEnableAfterExplicitClearNewWebhookUrl,
      })

      await page.reload()
      await expect(panel).toBeVisible({ timeout: 15_000 })
      await expect(enabled).toBeChecked()
      await expect(webhook).toHaveValue(
        routArrDispatchNotificationReEnableAfterExplicitClearNewWebhookUrl,
      )

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
  },
)
