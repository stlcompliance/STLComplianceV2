import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('StaffArr reports workspace @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('staffarr'))) {
      testInfo.skip(true, 'StaffArr frontend (5175) is unreachable.')
    }
  })

  test('reports workspace loads personnel, readiness, incident, and data export panels', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'staffarr')

    await page.goto(new URL('/reports', page.url()).toString())

    const workspace = page.getByTestId('staffarr-reports-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const personnelPanel = workspace.getByTestId('personnel-reports-panel')
    await expect(personnelPanel).toBeVisible()
    await expect(personnelPanel.getByRole('heading', { name: 'Personnel reports' })).toBeVisible()

    const personnelExport = personnelPanel.getByRole('button', { name: 'Export CSV' })
    await expect(personnelExport).toBeVisible()
    await expect(personnelExport).toBeEnabled()

    await personnelPanel.locator('select').selectOption('active')

    await expect(personnelPanel.getByText('Loading personnel report summary…')).not.toBeVisible({
      timeout: 15_000,
    })

    const personnelHasData =
      (await personnelPanel.getByText('People in scope').count()) > 0 ||
      (await personnelPanel.getByText(/No people match/i).count()) > 0
    expect(personnelHasData).toBeTruthy()

    const readinessPanel = workspace.getByTestId('readiness-reports-panel')
    await readinessPanel.scrollIntoViewIfNeeded()
    await expect(readinessPanel).toBeVisible()
    await expect(readinessPanel.getByRole('heading', { name: 'Readiness reports' })).toBeVisible()

    const readinessExport = readinessPanel.getByRole('button', { name: 'Export CSV' })
    await expect(readinessExport).toBeVisible()
    await expect(readinessExport).toBeEnabled()

    await readinessPanel.getByRole('checkbox').check()

    await expect(readinessPanel.getByText('Loading readiness report summary…')).not.toBeVisible({
      timeout: 15_000,
    })

    const readinessHasData =
      (await readinessPanel.getByText('Rollups in scope').count()) > 0 ||
      (await readinessPanel.getByText(/No readiness rollups match/i).count()) > 0
    expect(readinessHasData).toBeTruthy()

    const incidentPanel = workspace.getByTestId('incident-reports-panel')
    await incidentPanel.scrollIntoViewIfNeeded()
    await expect(incidentPanel).toBeVisible()
    await expect(incidentPanel.getByRole('heading', { name: 'Incident reports' })).toBeVisible()

    const incidentExport = incidentPanel.getByRole('button', { name: 'Export CSV' })
    await expect(incidentExport).toBeVisible()
    await expect(incidentExport).toBeEnabled()

    await incidentPanel.getByRole('checkbox').check()

    await expect(incidentPanel.getByText('Loading incident report summary…')).not.toBeVisible({
      timeout: 15_000,
    })

    const incidentHasData =
      (await incidentPanel.getByText('Incidents in scope').count()) > 0 ||
      (await incidentPanel.getByText(/No incidents match/i).count()) > 0
    expect(incidentHasData).toBeTruthy()

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
