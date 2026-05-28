import { test, expect } from '@playwright/test'
import { ensureTrainArrFieldInboxFixture, listCompanionOfflineActions } from '../support/e2eApi.js'
import {
  isCompanionFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'
import { companionFrontend, handoffUrlPattern } from '../support/productFrontends.js'

const companionSessionStorageKey = 'stl.companion.session'

test.describe('Companion offline queue and notification surfaces @requires-live', () => {
  test.beforeAll(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(true, 'Suite and NexArr API must be reachable for live E2E.')
    }
    if (!(await isCompanionFrontendReachable())) {
      testInfo.skip(true, 'Companion preview (5181) must be running.')
    }

    await ensureTrainArrFieldInboxFixture()
  })

  test('queues field acknowledge offline then syncs to NexArr', async ({ page, context }, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }

    await signInFromSuite(page)
    await page.goto('/app/companion/launch')

    const launchButton = page.getByRole('button', { name: 'Launch product (handoff)' })
    await expect(launchButton).toBeVisible({ timeout: 15_000 })
    await Promise.all([
      page.waitForURL(handoffUrlPattern(companionFrontend), { timeout: 30_000 }),
      launchButton.click(),
    ])

    await expect(page.getByTestId('companion-offline-queue-panel')).toBeVisible({ timeout: 20_000 })
    await expect(page.getByTestId('companion-connection-status')).toHaveText('Online')

    const notificationPanel = page.getByTestId('companion-notification-settings-panel')
    await expect(notificationPanel).toBeVisible()
    await notificationPanel.getByLabel(/Enable operational notifications/i).check()
    await notificationPanel
      .getByPlaceholder('https://hooks.example.com/companion')
      .fill('https://hooks.example.test/companion-e2e')
    await page.getByTestId('companion-save-notification-settings').click()
    await expect(page.getByTestId('companion-push-readiness-label')).toBeVisible()

    const acknowledgeButton = page.getByTestId('companion-acknowledge-task').first()
    await expect(acknowledgeButton).toBeVisible({ timeout: 15_000 })

    await context.setOffline(true)
    await expect(page.getByTestId('companion-connection-status')).toHaveText('Offline', {
      timeout: 10_000,
    })

    await acknowledgeButton.click()
    await expect(page.getByTestId('companion-offline-pending-count')).toHaveText('1 pending')

    await context.setOffline(false)
    await expect(page.getByTestId('companion-connection-status')).toHaveText('Online', {
      timeout: 10_000,
    })

    await page.getByTestId('companion-offline-sync-now').click()
    await expect(page.getByTestId('companion-offline-pending-count')).toHaveText('0 pending', {
      timeout: 20_000,
    })

    const accessToken = await page.evaluate((storageKey) => {
      const raw = window.sessionStorage.getItem(storageKey)
      if (!raw) {
        return null
      }
      const session = JSON.parse(raw) as { accessToken?: string }
      return session.accessToken ?? null
    }, companionSessionStorageKey)

    expect(accessToken).toBeTruthy()
    const history = await listCompanionOfflineActions(accessToken!, 5)
    expect(history.items.length).toBeGreaterThan(0)
    expect(history.items.some((item) => item.actionKind === 'field_inbox.acknowledge')).toBe(true)
  })
})
