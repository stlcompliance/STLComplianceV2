import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('MaintainArr settings admin workspace @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('maintainarr'))) {
      testInfo.skip(true, 'MaintainArr frontend (5178) is unreachable.')
    }
  })

  test('settings admin workspace loads all five product-admin panels with save controls', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'maintainarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const workspace = page.getByTestId('maintainarr-settings-admin-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const pmDueScanPanel = workspace.getByTestId('pm-due-scan-settings-panel')
    await expect(pmDueScanPanel).toBeVisible()
    await expect(pmDueScanPanel.getByRole('heading', { name: 'PM due scan worker' })).toBeVisible()
    await expect(pmDueScanPanel.getByTestId('pm-due-scan-save')).toBeVisible()
    await expect(pmDueScanPanel.getByText('Loading pending preview…')).not.toBeVisible({
      timeout: 15_000,
    })
    const pmDueScanPendingReady =
      (await pmDueScanPanel.getByTestId('pm-due-scan-pending-empty').count()) > 0 ||
      (await pmDueScanPanel.getByTestId('pm-due-scan-pending-list').count()) > 0
    expect(pmDueScanPendingReady).toBeTruthy()
    await expect(pmDueScanPanel.getByText('Loading worker runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const pmDueScanRunsReady =
      (await pmDueScanPanel.getByTestId('pm-due-scan-runs-empty').count()) > 0 ||
      (await pmDueScanPanel.getByTestId('pm-due-scan-runs-list').count()) > 0
    expect(pmDueScanRunsReady).toBeTruthy()

    const historyRollupPanel = workspace.getByTestId('maintenance-history-rollup-settings-panel')
    await historyRollupPanel.scrollIntoViewIfNeeded()
    await expect(historyRollupPanel).toBeVisible()
    await expect(
      historyRollupPanel.getByRole('heading', { name: 'Maintenance history rollup worker' }),
    ).toBeVisible()
    await expect(historyRollupPanel.getByTestId('maintenance-history-rollup-save')).toBeVisible()
    await expect(historyRollupPanel.getByText('Loading pending preview…')).not.toBeVisible({
      timeout: 15_000,
    })
    await expect(historyRollupPanel.getByText('Loading worker runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const historyRollupRunsReady =
      (await historyRollupPanel.getByTestId('maintenance-history-rollup-runs-empty').count()) > 0 ||
      (await historyRollupPanel.getByTestId('maintenance-history-rollup-runs-list').count()) > 0
    expect(historyRollupRunsReady).toBeTruthy()

    const assetStatusRollupPanel = workspace.getByTestId('asset-status-rollup-settings-panel')
    await assetStatusRollupPanel.scrollIntoViewIfNeeded()
    await expect(assetStatusRollupPanel).toBeVisible()
    await expect(
      assetStatusRollupPanel.getByRole('heading', { name: 'Asset status rollup worker' }),
    ).toBeVisible()
    await expect(assetStatusRollupPanel.getByTestId('asset-status-rollup-save')).toBeVisible()
    await expect(assetStatusRollupPanel.getByText('Loading pending preview…')).not.toBeVisible({
      timeout: 15_000,
    })
    await expect(assetStatusRollupPanel.getByText('Loading worker runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const assetStatusRollupRunsReady =
      (await assetStatusRollupPanel.getByTestId('asset-status-rollup-runs-empty').count()) > 0 ||
      (await assetStatusRollupPanel.getByTestId('asset-status-rollup-runs-list').count()) > 0
    expect(assetStatusRollupRunsReady).toBeTruthy()

    const defectEscalationPanel = workspace.getByTestId('defect-escalation-settings-panel')
    await defectEscalationPanel.scrollIntoViewIfNeeded()
    await expect(defectEscalationPanel).toBeVisible()
    await expect(
      defectEscalationPanel.getByRole('heading', { name: 'Defect escalation worker' }),
    ).toBeVisible()
    await expect(defectEscalationPanel.getByTestId('defect-escalation-save')).toBeVisible()
    await expect(defectEscalationPanel.getByText('Loading pending preview…')).not.toBeVisible({
      timeout: 15_000,
    })
    const defectEscalationPendingReady =
      (await defectEscalationPanel.getByTestId('defect-escalation-pending-empty').count()) > 0 ||
      (await defectEscalationPanel.getByTestId('defect-escalation-pending-list').count()) > 0
    expect(defectEscalationPendingReady).toBeTruthy()
    await expect(defectEscalationPanel.getByText('Loading worker runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const defectEscalationRunsReady =
      (await defectEscalationPanel.getByTestId('defect-escalation-runs-empty').count()) > 0 ||
      (await defectEscalationPanel.getByTestId('defect-escalation-runs-list').count()) > 0
    expect(defectEscalationRunsReady).toBeTruthy()
    await expect(defectEscalationPanel.getByText('Loading escalation events…')).not.toBeVisible({
      timeout: 15_000,
    })
    const defectEscalationEventsReady =
      (await defectEscalationPanel.getByTestId('defect-escalation-events-empty').count()) > 0 ||
      (await defectEscalationPanel.getByTestId('defect-escalation-events-list').count()) > 0
    expect(defectEscalationEventsReady).toBeTruthy()

    const notificationPanel = workspace.getByTestId('notification-settings-panel')
    await notificationPanel.scrollIntoViewIfNeeded()
    await expect(notificationPanel).toBeVisible()
    await expect(
      notificationPanel.getByRole('heading', { name: 'Maintenance notifications' }),
    ).toBeVisible()
    await expect(notificationPanel.getByTestId('notification-settings-save')).toBeVisible()
    await expect(notificationPanel.getByText('Loading dispatch history…')).not.toBeVisible({
      timeout: 15_000,
    })
    const notificationDispatchesReady =
      (await notificationPanel.getByTestId('notification-dispatches-empty').count()) > 0 ||
      (await notificationPanel.getByTestId('notification-dispatches-list').count()) > 0
    expect(notificationDispatchesReady).toBeTruthy()
  })
})
