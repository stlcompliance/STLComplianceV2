import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr reports workspace @requires-live', () => {
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

  test('reports workspace loads dispatch, route, proof-DVIR, and data export panels', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/reports', page.url()).toString())

    const workspace = page.getByTestId('routarr-reports-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const dispatchPanel = workspace.getByTestId('dispatch-reports-panel')
    await expect(dispatchPanel).toBeVisible()
    await expect(
      dispatchPanel.getByRole('heading', { name: 'Dispatch & transportation reports' }),
    ).toBeVisible()

    const dispatchExport = dispatchPanel.getByRole('button', { name: 'Export CSV' })
    await expect(dispatchExport).toBeVisible()
    await expect(dispatchExport).toBeEnabled()

    await dispatchPanel.locator('select').selectOption('weekly')

    await expect(dispatchPanel.getByText('Loading dispatch report summary')).not.toBeVisible({
      timeout: 15_000,
    })

    const dispatchHasData =
      (await dispatchPanel.getByText('Trips in scope').count()) > 0 ||
      (await dispatchPanel.getByText(/No trips in this reporting window/i).count()) > 0
    expect(dispatchHasData).toBeTruthy()

    const routePanel = workspace.getByTestId('route-reports-panel')
    await routePanel.scrollIntoViewIfNeeded()
    await expect(routePanel).toBeVisible()
    await expect(
      routePanel.getByRole('heading', { name: 'Route & stop execution reports' }),
    ).toBeVisible()

    const routeExport = routePanel.getByRole('button', { name: 'Export CSV' })
    await expect(routeExport).toBeVisible()
    await expect(routeExport).toBeEnabled()

    await routePanel.locator('select').selectOption('weekly')

    await expect(routePanel.getByText('Loading route report summary')).not.toBeVisible({
      timeout: 15_000,
    })

    const routeHasData =
      (await routePanel.getByText('Routes in scope').count()) > 0 ||
      (await routePanel.getByText(/No routes in this reporting window/i).count()) > 0
    expect(routeHasData).toBeTruthy()

    const proofDvirPanel = workspace.getByTestId('proof-dvir-reports-panel')
    await proofDvirPanel.scrollIntoViewIfNeeded()
    await expect(proofDvirPanel).toBeVisible()
    await expect(proofDvirPanel.getByRole('heading', { name: 'Proof & DVIR reports' })).toBeVisible()

    const proofDvirExport = proofDvirPanel.getByRole('button', { name: 'Export CSV' })
    await expect(proofDvirExport).toBeVisible()
    await expect(proofDvirExport).toBeEnabled()

    await proofDvirPanel.locator('select').selectOption('weekly')

    await expect(proofDvirPanel.getByText('Loading proof/DVIR report summary')).not.toBeVisible({
      timeout: 15_000,
    })

    const proofDvirHasData =
      (await proofDvirPanel.getByText('Proof records').count()) > 0 ||
      (await proofDvirPanel.getByText(/No proof or DVIR activity in this window/i).count()) > 0
    expect(proofDvirHasData).toBeTruthy()

    const dataExportsPanel = workspace.getByTestId('data-exports-panel')
    await dataExportsPanel.scrollIntoViewIfNeeded()
    await expect(dataExportsPanel).toBeVisible()
    await expect(dataExportsPanel.getByRole('heading', { name: 'Data exports' })).toBeVisible()

    await expect(dataExportsPanel.getByText('Loading export manifest')).not.toBeVisible({
      timeout: 15_000,
    })

    const downloadButtons = dataExportsPanel.getByRole('button', { name: 'Download CSV' })
    await expect(downloadButtons.first()).toBeVisible()
    expect(await downloadButtons.count()).toBeGreaterThanOrEqual(3)
    await expect(downloadButtons.first()).toBeEnabled()
  })
})
