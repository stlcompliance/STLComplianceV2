import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr settings admin workspace @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('supplyarr'))) {
      testInfo.skip(true, 'SupplyArr frontend (5179) is unreachable.')
    }
  })

  test('settings admin workspace loads all nine product-admin panels with save controls', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const workspace = page.getByTestId('supplyarr-settings-admin-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const notificationPanel = workspace.getByTestId('notification-settings-panel')
    await expect(notificationPanel).toBeVisible()
    await expect(
      notificationPanel.getByRole('heading', { name: 'Procurement notifications' }),
    ).toBeVisible()
    await expect(notificationPanel.getByTestId('notification-settings-save')).toBeVisible()
    await expect(notificationPanel.getByText('Loading dispatch history…')).not.toBeVisible({
      timeout: 15_000,
    })
    const notificationDispatchesReady =
      (await notificationPanel.getByTestId('notification-dispatches-empty').count()) > 0 ||
      (await notificationPanel.getByTestId('notification-dispatches-list').count()) > 0
    expect(notificationDispatchesReady).toBeTruthy()

    const pricePanel = workspace.getByTestId('price-snapshot-settings-panel')
    await pricePanel.scrollIntoViewIfNeeded()
    await expect(pricePanel).toBeVisible()
    await expect(pricePanel.getByRole('heading', { name: 'Price snapshot worker' })).toBeVisible()
    await expect(
      pricePanel.getByRole('button', { name: 'Save price snapshot settings' }),
    ).toBeVisible()

    const leadTimePanel = workspace.getByTestId('lead-time-snapshot-settings-panel')
    await leadTimePanel.scrollIntoViewIfNeeded()
    await expect(leadTimePanel).toBeVisible()
    await expect(
      leadTimePanel.getByRole('heading', { name: 'Lead-time snapshot worker' }),
    ).toBeVisible()
    await expect(
      leadTimePanel.getByRole('button', { name: 'Save lead-time snapshot settings' }),
    ).toBeVisible()

    const availabilityPanel = workspace.getByTestId('availability-snapshot-settings-panel')
    await availabilityPanel.scrollIntoViewIfNeeded()
    await expect(availabilityPanel).toBeVisible()
    await expect(
      availabilityPanel.getByRole('heading', { name: 'Availability snapshot worker' }),
    ).toBeVisible()
    await expect(
      availabilityPanel.getByRole('button', { name: 'Save availability snapshot settings' }),
    ).toBeVisible()

    const coordinationPanel = workspace.getByTestId('procurement-coordination-settings-panel')
    await coordinationPanel.scrollIntoViewIfNeeded()
    await expect(coordinationPanel).toBeVisible()
    await expect(
      coordinationPanel.getByRole('heading', { name: 'Procurement coordination worker' }),
    ).toBeVisible()
    await expect(
      coordinationPanel.getByRole('button', { name: 'Save coordination settings' }),
    ).toBeVisible()

    const approvalPanel = workspace.getByTestId('approval-reminder-settings-panel')
    await approvalPanel.scrollIntoViewIfNeeded()
    await expect(approvalPanel).toBeVisible()
    await expect(
      approvalPanel.getByRole('heading', { name: 'Approval reminder worker' }),
    ).toBeVisible()
    await expect(
      approvalPanel.getByRole('button', { name: 'Save reminder settings' }),
    ).toBeVisible()

    const escalationPanel = workspace.getByTestId('procurement-exception-escalation-settings-panel')
    await escalationPanel.scrollIntoViewIfNeeded()
    await expect(escalationPanel).toBeVisible()
    await expect(
      escalationPanel.getByRole('heading', { name: 'Procurement exception SLA escalation' }),
    ).toBeVisible()
    await expect(escalationPanel.getByTestId('procurement-exception-escalation-save')).toBeVisible()
    await expect(escalationPanel.getByText('Loading pending preview…')).not.toBeVisible({
      timeout: 15_000,
    })
    const escalationRunsReady =
      (await escalationPanel.getByTestId('procurement-exception-escalation-runs-empty').count()) >
        0 ||
      (await escalationPanel.getByTestId('procurement-exception-escalation-runs-list').count()) > 0
    expect(escalationRunsReady).toBeTruthy()

    const demandPanel = workspace.getByTestId('demand-processing-settings-panel')
    await demandPanel.scrollIntoViewIfNeeded()
    await expect(demandPanel).toBeVisible()
    await expect(
      demandPanel.getByRole('heading', { name: 'Demand processing worker' }),
    ).toBeVisible()
    await expect(demandPanel.getByRole('button', { name: 'Save settings' })).toBeVisible()
    await expect(demandPanel.getByText('Loading settings…')).not.toBeVisible({ timeout: 15_000 })

    const integrationPanel = workspace.getByTestId('integration-event-settings-panel')
    await integrationPanel.scrollIntoViewIfNeeded()
    await expect(integrationPanel).toBeVisible()
    await expect(
      integrationPanel.getByRole('heading', { name: 'Integration event outbox / inbox' }),
    ).toBeVisible()
    await expect(integrationPanel.getByRole('button', { name: 'Save settings' })).toBeVisible()
    await expect(integrationPanel.getByText('Recent outbox')).toBeVisible({ timeout: 15_000 })
    await expect(integrationPanel.getByText('Recent inbox')).toBeVisible()
  })
})
