import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('MaintainArr reports workspace @requires-live', () => {
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

  test('reports workspace loads compliance, executive, maintenance, and data export panels', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'maintainarr')

    await page.goto(new URL('/reports', page.url()).toString())

    const workspace = page.getByTestId('maintainarr-reports-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const compliancePanel = workspace.getByTestId('compliance-reports-panel')
    await expect(compliancePanel).toBeVisible()
    await expect(compliancePanel.getByRole('heading', { name: 'Compliance reports' })).toBeVisible()

    const complianceExport = compliancePanel.getByRole('button', { name: 'Export CSV' })
    await expect(complianceExport).toBeVisible()
    await expect(complianceExport).toBeEnabled()

    await compliancePanel.getByRole('checkbox').check()

    await expect(compliancePanel.getByText('Loading compliance report…')).not.toBeVisible({
      timeout: 15_000,
    })

    const complianceHasData =
      (await compliancePanel.getByText('Inspection pass %').count()) > 0 ||
      (await compliancePanel.getByText('Attention items').count()) > 0
    expect(complianceHasData).toBeTruthy()

    const executivePanel = workspace.getByTestId('executive-reports-panel')
    await executivePanel.scrollIntoViewIfNeeded()
    await expect(executivePanel).toBeVisible()
    await expect(executivePanel.getByRole('heading', { name: 'Executive summary' })).toBeVisible()

    const executiveExport = executivePanel.getByRole('button', { name: 'Export CSV' })
    await expect(executiveExport).toBeVisible()
    await expect(executiveExport).toBeEnabled()

    await expect(executivePanel.getByText('Loading executive summary…')).not.toBeVisible({
      timeout: 15_000,
    })

    const executiveHasData =
      (await executivePanel.getByText('Fleet ready %').count()) > 0 ||
      (await executivePanel.getByText('Open work orders').count()) > 0
    expect(executiveHasData).toBeTruthy()

    const maintenancePanel = workspace.getByTestId('maintenance-reports-panel')
    await maintenancePanel.scrollIntoViewIfNeeded()
    await expect(maintenancePanel).toBeVisible()
    await expect(maintenancePanel.getByRole('heading', { name: 'Maintenance reports' })).toBeVisible()

    const maintenanceExport = maintenancePanel.getByRole('button', { name: 'Export CSV' })
    await expect(maintenanceExport).toBeVisible()
    await expect(maintenanceExport).toBeEnabled()

    await maintenancePanel.locator('select').selectOption('active')

    await expect(maintenancePanel.getByText('Loading maintenance report summary…')).not.toBeVisible({
      timeout: 15_000,
    })

    const maintenanceHasData =
      (await maintenancePanel.getByText('Assets').count()) > 0 ||
      (await maintenancePanel.locator('table tbody tr').count()) > 0
    expect(maintenanceHasData).toBeTruthy()

    const dataExportsPanel = workspace.getByTestId('data-exports-panel')
    await dataExportsPanel.scrollIntoViewIfNeeded()
    await expect(dataExportsPanel).toBeVisible()
    await expect(dataExportsPanel.getByRole('heading', { name: 'Data exports' })).toBeVisible()

    const downloadButtons = dataExportsPanel.getByRole('button', { name: 'Download CSV' })
    await expect(downloadButtons.first()).toBeVisible({ timeout: 15_000 })
    expect(await downloadButtons.count()).toBeGreaterThanOrEqual(3)
    await expect(downloadButtons.first()).toBeEnabled()
  })
})
