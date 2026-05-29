import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('TrainArr reports workspace @requires-live', () => {
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

  test('reports workspace loads assignment, qualification, compliance, and data export panels', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'trainarr')

    await page.goto(new URL('/reports', page.url()).toString())

    const workspace = page.getByTestId('trainarr-reports-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const assignmentPanel = workspace.getByTestId('assignment-reports-panel')
    await expect(assignmentPanel).toBeVisible()
    await expect(
      assignmentPanel.getByRole('heading', { name: 'Training assignment reports' }),
    ).toBeVisible()

    const assignmentExport = assignmentPanel.getByRole('button', { name: 'Export CSV' })
    await expect(assignmentExport).toBeVisible()
    await expect(assignmentExport).toBeEnabled()

    await assignmentPanel.locator('select').selectOption('assigned')
    await assignmentPanel.getByRole('checkbox').check()

    await expect(assignmentPanel.getByText('Loading assignment report summary…')).not.toBeVisible({
      timeout: 15_000,
    })

    const assignmentHasData =
      (await assignmentPanel.getByText('Assignments in scope').count()) > 0 ||
      (await assignmentPanel.getByText(/No assignments match/i).count()) > 0
    expect(assignmentHasData).toBeTruthy()

    const qualificationPanel = workspace.getByTestId('qualification-reports-panel')
    await qualificationPanel.scrollIntoViewIfNeeded()
    await expect(qualificationPanel).toBeVisible()
    await expect(
      qualificationPanel.getByRole('heading', { name: 'Qualification reports' }),
    ).toBeVisible()

    const qualificationExport = qualificationPanel.getByRole('button', { name: 'Export CSV' })
    await expect(qualificationExport).toBeVisible()
    await expect(qualificationExport).toBeEnabled()

    await qualificationPanel.locator('select').selectOption('issued')

    await expect(qualificationPanel.getByText('Loading qualification report summary…')).not.toBeVisible({
      timeout: 15_000,
    })

    const qualificationHasData =
      (await qualificationPanel.getByText('Qualifications in scope').count()) > 0 ||
      (await qualificationPanel.getByText(/No qualifications match/i).count()) > 0
    expect(qualificationHasData).toBeTruthy()

    const compliancePanel = workspace.getByTestId('compliance-reports-panel')
    await compliancePanel.scrollIntoViewIfNeeded()
    await expect(compliancePanel).toBeVisible()
    await expect(compliancePanel.getByRole('heading', { name: 'Compliance reports' })).toBeVisible()

    const complianceExport = compliancePanel.getByRole('button', { name: 'Export CSV' })
    await expect(complianceExport).toBeVisible()
    await expect(complianceExport).toBeEnabled()

    await compliancePanel.getByRole('checkbox').check()

    await expect(compliancePanel.getByText('Loading compliance report summary…')).not.toBeVisible({
      timeout: 15_000,
    })

    const complianceHasData =
      (await compliancePanel.getByText('Citation attachments').count()) > 0 ||
      (await compliancePanel.getByText(/No remediations match/i).count()) > 0
    expect(complianceHasData).toBeTruthy()

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
