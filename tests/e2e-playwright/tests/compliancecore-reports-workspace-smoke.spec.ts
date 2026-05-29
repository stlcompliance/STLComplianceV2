import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('Compliance Core reports workspace @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('compliancecore'))) {
      testInfo.skip(true, 'Compliance Core frontend (5177) is unreachable.')
    }
  })

  test('reports workspace loads compliance, operator, and data export panels', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await page.goto(new URL('/reports', page.url()).toString())

    const workspace = page.getByTestId('compliancecore-reports-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const compliancePanel = workspace.getByTestId('compliance-reports-panel')
    await expect(compliancePanel).toBeVisible()
    await expect(compliancePanel.getByRole('heading', { name: 'Compliance reports' })).toBeVisible()

    const complianceExport = compliancePanel.getByRole('button', { name: 'Export CSV' })
    await expect(complianceExport).toBeVisible()
    await expect(complianceExport).toBeEnabled()

    await compliancePanel.locator('select').selectOption('block')

    await expect(compliancePanel.getByText('Loading compliance report summary…')).not.toBeVisible({
      timeout: 15_000,
    })

    const complianceHasData =
      (await compliancePanel.getByText('Findings in scope').count()) > 0 ||
      (await compliancePanel.getByText(/No findings match/i).count()) > 0
    expect(complianceHasData).toBeTruthy()

    const operatorPanel = workspace.getByTestId('operator-reports-panel')
    await operatorPanel.scrollIntoViewIfNeeded()
    await expect(operatorPanel).toBeVisible()
    await expect(operatorPanel.getByRole('heading', { name: 'Operator reports' })).toBeVisible()

    const operatorExport = operatorPanel.getByRole('button', { name: 'Export CSV' })
    await expect(operatorExport).toBeVisible()
    await expect(operatorExport).toBeEnabled()

    await operatorPanel.getByRole('checkbox').check()

    await expect(operatorPanel.getByText('Loading operator report summary…')).not.toBeVisible({
      timeout: 15_000,
    })

    const operatorHasData =
      (await operatorPanel.getByText('Evaluations').count()) > 0 ||
      (await operatorPanel.getByText(/No evaluations match/i).count()) > 0
    expect(operatorHasData).toBeTruthy()

    const dataExportsPanel = workspace.getByTestId('data-exports-panel')
    await dataExportsPanel.scrollIntoViewIfNeeded()
    await expect(dataExportsPanel).toBeVisible()
    await expect(dataExportsPanel.getByRole('heading', { name: 'Data exports' })).toBeVisible()

    await expect(dataExportsPanel.getByText('Loading export manifest…')).not.toBeVisible({
      timeout: 15_000,
    })

    const downloadButtons = dataExportsPanel.getByRole('button', { name: 'Download CSV' })
    await expect(downloadButtons.first()).toBeVisible()
    expect(await downloadButtons.count()).toBeGreaterThanOrEqual(3)
    await expect(downloadButtons.first()).toBeEnabled()
  })
})
