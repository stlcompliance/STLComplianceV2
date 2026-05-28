import { test, expect } from '@playwright/test'

import {
  ensureMaintainArrFieldInboxFixture,
  ensureRoutArrFieldInboxFixture,
  ensureSupplyArrFieldInboxFixture,
} from '../support/e2eApi.js'
import {
  isCompanionFrontendReachable,
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'
import { companionFrontend, handoffProductFrontends, handoffUrlPattern } from '../support/productFrontends.js'

async function launchCompanionFieldInbox(page: import('@playwright/test').Page) {
  await signInFromSuite(page)
  await page.goto('/app/companion/launch')

  const launchButton = page.getByRole('button', { name: 'Launch product (handoff)' })
  await expect(launchButton).toBeVisible({ timeout: 15_000 })

  await Promise.all([
    page.waitForURL(handoffUrlPattern(companionFrontend), { timeout: 30_000 }),
    launchButton.click(),
  ])

  await expect(page.getByRole('heading', { name: 'Field inbox' })).toBeVisible({ timeout: 20_000 })
}

test.describe('Companion field inbox → operations product deep links @requires-live', () => {
  test.beforeEach(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(true, 'Suite and NexArr API must be reachable for live E2E.')
    }
    if (!(await isCompanionFrontendReachable())) {
      testInfo.skip(true, 'Companion (5181) preview must be running.')
    }
  })

  test('MaintainArr work order deep link opens workspace', async ({ page }, testInfo) => {
    if (!(await isHandoffFrontendReachable('maintainarr'))) {
      testInfo.skip(true, 'MaintainArr frontend (5178) preview must be running.')
    }

    const fixture = await ensureMaintainArrFieldInboxFixture()
    const maintainarr = handoffProductFrontends.find((p) => p.productKey === 'maintainarr')!

    await launchCompanionFieldInbox(page)

    const filter = page.getByRole('button', { name: /MaintainArr/i })
    if (await filter.isVisible()) {
      await filter.click()
    }

    const openLink = page.getByRole('link', { name: /Open in MaintainArr/i }).first()
    await expect(openLink).toBeVisible({ timeout: 15_000 })
    expect(await openLink.getAttribute('href')).toMatch(
      new RegExp(`/work-orders/${fixture.workOrderId}`, 'i'),
    )

    await Promise.all([
      page.waitForURL(new RegExp(`/work-orders/${fixture.workOrderId}`, 'i'), { timeout: 30_000 }),
      openLink.click(),
    ])

    await expect(page).toHaveURL(handoffUrlPattern(maintainarr))
    await expect(page.getByTestId('work-order-workspace')).toBeVisible({ timeout: 15_000 })
  })

  test('RoutArr trip deep link opens workspace', async ({ page }, testInfo) => {
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) preview must be running.')
    }

    const fixture = await ensureRoutArrFieldInboxFixture()
    const routarr = handoffProductFrontends.find((p) => p.productKey === 'routarr')!

    await launchCompanionFieldInbox(page)

    const filter = page.getByRole('button', { name: /RoutArr/i })
    if (await filter.isVisible()) {
      await filter.click()
    }

    const openLink = page.getByRole('link', { name: /Open in RoutArr/i }).first()
    await expect(openLink).toBeVisible({ timeout: 15_000 })
    expect(await openLink.getAttribute('href')).toMatch(new RegExp(`/trips/${fixture.tripId}`, 'i'))

    await Promise.all([
      page.waitForURL(new RegExp(`/trips/${fixture.tripId}`, 'i'), { timeout: 30_000 }),
      openLink.click(),
    ])

    await expect(page).toHaveURL(handoffUrlPattern(routarr))
    await expect(page.getByTestId('trip-workspace')).toBeVisible({ timeout: 15_000 })
  })

  test('SupplyArr receiving deep link opens workspace', async ({ page }, testInfo) => {
    if (!(await isHandoffFrontendReachable('supplyarr'))) {
      testInfo.skip(true, 'SupplyArr frontend (5179) preview must be running.')
    }

    const fixture = await ensureSupplyArrFieldInboxFixture()
    const supplyarr = handoffProductFrontends.find((p) => p.productKey === 'supplyarr')!

    await launchCompanionFieldInbox(page)

    const filter = page.getByRole('button', { name: /SupplyArr/i })
    if (await filter.isVisible()) {
      await filter.click()
    }

    const openLink = page.getByRole('link', { name: /Open in SupplyArr/i }).first()
    await expect(openLink).toBeVisible({ timeout: 15_000 })
    expect(await openLink.getAttribute('href')).toMatch(
      new RegExp(`/receiving/${fixture.receivingReceiptId}`, 'i'),
    )

    await Promise.all([
      page.waitForURL(new RegExp(`/receiving/${fixture.receivingReceiptId}`, 'i'), {
        timeout: 30_000,
      }),
      openLink.click(),
    ])

    await expect(page).toHaveURL(handoffUrlPattern(supplyarr))
    await expect(page.getByTestId('receiving-workspace')).toBeVisible({ timeout: 15_000 })
  })
})
