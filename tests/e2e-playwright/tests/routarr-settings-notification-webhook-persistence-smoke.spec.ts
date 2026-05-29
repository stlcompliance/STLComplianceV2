import { test, expect } from '@playwright/test'

import {
  assertRoutArrDispatchNotificationWebhookUrlPersisted,
  routArrDispatchNotificationWebhookPersistenceAlternateUrl,
  routArrDispatchNotificationWebhookPersistencePrimaryUrl,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr settings notification webhook persistence @requires-live', () => {
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

  test('notification settings panel persists webhook URL changes across save and reload', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('notification-settings-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Dispatch notifications' })).toBeVisible()

    const webhook = panel.getByTestId('notification-settings-webhook')
    await expect(webhook).toBeVisible()

    const initialWebhook = await webhook.inputValue()

    await webhook.fill(routArrDispatchNotificationWebhookPersistencePrimaryUrl)
    await panel.getByTestId('notification-settings-save').click()
    await expect(webhook).toHaveValue(routArrDispatchNotificationWebhookPersistencePrimaryUrl)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(webhook).toHaveValue(routArrDispatchNotificationWebhookPersistencePrimaryUrl)
    await assertRoutArrDispatchNotificationWebhookUrlPersisted(
      routArrDispatchNotificationWebhookPersistencePrimaryUrl,
    )

    await webhook.fill(routArrDispatchNotificationWebhookPersistenceAlternateUrl)
    await panel.getByTestId('notification-settings-save').click()
    await expect(webhook).toHaveValue(routArrDispatchNotificationWebhookPersistenceAlternateUrl)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(webhook).toHaveValue(routArrDispatchNotificationWebhookPersistenceAlternateUrl)
    await assertRoutArrDispatchNotificationWebhookUrlPersisted(
      routArrDispatchNotificationWebhookPersistenceAlternateUrl,
    )

    await webhook.fill(initialWebhook)
    await panel.getByTestId('notification-settings-save').click()
    await expect(webhook).toHaveValue(initialWebhook)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(webhook).toHaveValue(initialWebhook)
    await assertRoutArrDispatchNotificationWebhookUrlPersisted(initialWebhook)
  })
})
