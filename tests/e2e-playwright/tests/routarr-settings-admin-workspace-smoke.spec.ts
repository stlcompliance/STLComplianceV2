import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr settings admin workspace @requires-live', () => {
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

  test('settings admin workspace loads all four product-admin panels with save controls', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const workspace = page.getByTestId('routarr-settings-admin-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const notificationPanel = workspace.getByTestId('notification-settings-panel')
    await expect(notificationPanel).toBeVisible()
    await expect(notificationPanel.getByRole('heading', { name: 'Dispatch notifications' })).toBeVisible()
    await expect(notificationPanel.getByTestId('notification-settings-save')).toBeVisible()
    await expect(notificationPanel.getByText('Loading dispatch history…')).not.toBeVisible({
      timeout: 15_000,
    })
    const notificationDispatchesReady =
      (await notificationPanel.getByTestId('notification-dispatches-empty').count()) > 0 ||
      (await notificationPanel.getByTestId('notification-dispatches-list').count()) > 0
    expect(notificationDispatchesReady).toBeTruthy()

    const tripExecutionPanel = workspace.getByTestId('trip-execution-settings-panel')
    await tripExecutionPanel.scrollIntoViewIfNeeded()
    await expect(tripExecutionPanel).toBeVisible()
    await expect(
      tripExecutionPanel.getByRole('heading', { name: 'Trip proof & DVIR capture policy' }),
    ).toBeVisible()
    await expect(tripExecutionPanel.getByRole('button', { name: 'Save capture policy' })).toBeVisible()
    await expect(
      tripExecutionPanel.getByRole('checkbox', { name: 'Require pre-trip DVIR before start' }),
    ).toBeVisible()

    const rollupPanel = workspace.getByTestId('trip-completion-rollup-settings-panel')
    await rollupPanel.scrollIntoViewIfNeeded()
    await expect(rollupPanel).toBeVisible()
    await expect(
      rollupPanel.getByRole('heading', { name: 'Trip completion rollup worker' }),
    ).toBeVisible()
    await expect(rollupPanel.getByTestId('trip-completion-rollup-save')).toBeVisible()
    await expect(rollupPanel.getByText('Loading worker runs…')).not.toBeVisible({ timeout: 15_000 })
    const rollupRunsReady =
      (await rollupPanel.getByTestId('trip-completion-rollup-runs-empty').count()) > 0 ||
      (await rollupPanel.getByTestId('trip-completion-rollup-runs-list').count()) > 0
    expect(rollupRunsReady).toBeTruthy()

    const retentionPanel = workspace.getByTestId('attachment-retention-settings-panel')
    await retentionPanel.scrollIntoViewIfNeeded()
    await expect(retentionPanel).toBeVisible()
    await expect(
      retentionPanel.getByRole('heading', { name: 'Trip capture attachment retention' }),
    ).toBeVisible()
    await expect(retentionPanel.getByTestId('attachment-retention-save')).toBeVisible()
    await expect(retentionPanel.getByText('Loading retention runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const retentionRunsReady =
      (await retentionPanel.getByTestId('attachment-retention-runs-empty').count()) > 0 ||
      (await retentionPanel.getByTestId('attachment-retention-runs-list').count()) > 0
    expect(retentionRunsReady).toBeTruthy()
  })
})
