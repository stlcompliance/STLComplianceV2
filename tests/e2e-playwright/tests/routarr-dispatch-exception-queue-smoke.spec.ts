import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr dispatch exception queue @requires-live', () => {
  test.beforeAll(async ({}, testInfo) => {
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

  test('handoff opens dispatch exception queue with create form or empty list', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('dispatch-exception-queue-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Exception queue' })).toBeVisible()

    const titleInput = panel.getByPlaceholder('Exception title')
    const logButton = panel.getByRole('button', { name: 'Log exception' })
    const hasCreateForm =
      (await titleInput.count()) > 0 && (await logButton.count()) > 0

    const exceptionRows = panel.locator('[data-testid^="exception-row-"]')
    const hasRows = (await exceptionRows.count()) > 0
    const hasEmptyState = (await panel.getByText('No open exceptions').count()) > 0

    if (hasCreateForm) {
      await expect(titleInput).toBeVisible()
      await expect(logButton).toBeVisible()
    }

    expect(hasRows || hasEmptyState).toBeTruthy()
  })
})
