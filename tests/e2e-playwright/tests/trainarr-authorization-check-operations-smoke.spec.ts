import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('TrainArr authorization check operations @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('trainarr'))) {
      testInfo.skip(true, 'TrainArr frontend (5176) is unreachable.')
    }
  })

  test('qualifications workspace shows authorization check operations panel with history', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'trainarr')

    await page.goto(new URL('/qualifications', page.url()).toString())

    const panel = page.getByTestId('authorization-check-operations-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: /authorization check operations/i })).toBeVisible()

    const history = panel.getByTestId('authorization-check-history')
    await expect(history).toBeVisible()

    const historyReady =
      (await panel.getByTestId('authorization-check-history-empty').count()) > 0 ||
      (await panel.getByTestId('authorization-check-history-list').count()) > 0 ||
      (await panel.getByTestId('authorization-check-history-loading').count()) > 0
    expect(historyReady).toBeTruthy()

    await expect(panel.getByRole('button', { name: /run qualification check/i })).toBeVisible()
  })
})
