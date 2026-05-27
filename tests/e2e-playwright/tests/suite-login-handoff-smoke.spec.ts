import { test, expect } from '@playwright/test'
import {
  demoCredentials,
  isLiveModeEnabled,
  isLiveStackReachable,
} from '../support/liveProbe.js'

test.describe('Suite frontend NexArr journey @requires-live', () => {
  test.beforeEach(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(
        true,
        'Suite frontend (5174) and NexArr API (5101) must be running. Start docker-compose and `npm run dev` in apps/suite-frontend.',
      )
    }
  })

  test('login → dashboard → StaffArr launch surface', async ({ page }) => {
    await page.goto('/login')
    await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible()

    await page.getByLabel('Email').fill(demoCredentials.email)
    await page.getByLabel('Password').fill(demoCredentials.password)
    await page.getByRole('button', { name: 'Sign in' }).click()

    await expect(page.getByRole('heading', { name: /Welcome,/ })).toBeVisible({
      timeout: 15_000,
    })

    await page.goto('/app/staffarr/launch')
    await expect(
      page.getByRole('button', { name: 'Launch product (handoff)' }),
    ).toBeVisible({ timeout: 15_000 })
  })

  test('handoff issues redirect to product app', async ({ page }) => {
    await page.goto('/login')
    await page.getByLabel('Email').fill(demoCredentials.email)
    await page.getByLabel('Password').fill(demoCredentials.password)
    await page.getByRole('button', { name: 'Sign in' }).click()
    await expect(page.getByRole('heading', { name: /Welcome,/ })).toBeVisible({
      timeout: 15_000,
    })

    await page.goto('/app/staffarr/launch')
    const launchButton = page.getByRole('button', {
      name: 'Launch product (handoff)',
    })
    await expect(launchButton).toBeVisible({ timeout: 15_000 })

    await Promise.all([
      page.waitForURL(/localhost:5175|staffarr/i, { timeout: 20_000 }),
      launchButton.click(),
    ])
  })
})
