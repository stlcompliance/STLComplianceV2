import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr reports workspace @requires-live', () => {
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

  test('reports workspace loads all five M12 report panels', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    await page.goto(new URL('/reports', page.url()).toString())

    const workspace = page.getByTestId('supplyarr-reports-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const vendorPanel = workspace.getByTestId('vendor-reports-panel')
    await expect(vendorPanel).toBeVisible()
    await expect(vendorPanel.getByRole('heading', { name: 'Vendor reports' })).toBeVisible()

    const vendorExport = vendorPanel.getByRole('button', { name: 'Export CSV' })
    await expect(vendorExport).toBeVisible()
    await expect(vendorExport).toBeEnabled()

    await vendorPanel.locator('select').selectOption('approved')
    await vendorPanel.getByRole('checkbox').check()

    await expect(vendorPanel.getByText('Loading vendor report summary')).not.toBeVisible({
      timeout: 15_000,
    })

    const vendorHasData =
      (await vendorPanel.locator('span.rounded-md').count()) > 0 ||
      (await vendorPanel.getByText(/No vendors match/i).count()) > 0
    expect(vendorHasData).toBeTruthy()

    const partsPanel = workspace.getByTestId('parts-inventory-reports-panel')
    await partsPanel.scrollIntoViewIfNeeded()
    await expect(partsPanel).toBeVisible()
    await expect(
      partsPanel.getByRole('heading', { name: 'Parts and inventory reports' }),
    ).toBeVisible()

    const partsExport = partsPanel.getByRole('button', { name: 'Export parts CSV' })
    await expect(partsExport).toBeVisible()
    await expect(partsExport).toBeEnabled()

    await partsPanel.getByRole('checkbox', { name: 'Below reorder point only' }).check()

    await expect(partsPanel.getByText('Loading parts and inventory summary')).not.toBeVisible({
      timeout: 15_000,
    })

    const partsHasData =
      (await partsPanel.getByText(/^Parts:/i).count()) > 0 ||
      (await partsPanel.getByText(/No parts match/i).count()) > 0
    expect(partsHasData).toBeTruthy()

    const purchasingPanel = workspace.getByTestId('purchasing-reports-panel')
    await purchasingPanel.scrollIntoViewIfNeeded()
    await expect(purchasingPanel).toBeVisible()
    await expect(purchasingPanel.getByRole('heading', { name: 'Purchasing reports' })).toBeVisible()

    const purchasingExport = purchasingPanel.getByRole('button', { name: 'Export CSV' })
    await expect(purchasingExport).toBeVisible()
    await expect(purchasingExport).toBeEnabled()

    await purchasingPanel.getByRole('checkbox').check()

    await expect(purchasingPanel.getByText('Loading purchasing summary')).not.toBeVisible({
      timeout: 15_000,
    })

    const purchasingHasData =
      (await purchasingPanel.getByText(/^PRs:/i).count()) > 0 ||
      (await purchasingPanel.getByText(/No documents match/i).count()) > 0
    expect(purchasingHasData).toBeTruthy()

    const compliancePanel = workspace.getByTestId('compliance-reports-panel')
    await compliancePanel.scrollIntoViewIfNeeded()
    await expect(compliancePanel).toBeVisible()
    await expect(compliancePanel.getByRole('heading', { name: 'Compliance reports' })).toBeVisible()

    const complianceExport = compliancePanel.getByRole('button', { name: 'Export CSV' })
    await expect(complianceExport).toBeVisible()
    await expect(complianceExport).toBeEnabled()

    await compliancePanel.getByRole('checkbox').check()

    await expect(compliancePanel.getByText('Loading compliance summary')).not.toBeVisible({
      timeout: 15_000,
    })

    const complianceHasData =
      (await compliancePanel.getByText(/^Parties:/i).count()) > 0 ||
      (await compliancePanel.getByText(/^Documents:/i).count()) > 0
    expect(complianceHasData).toBeTruthy()

    const auditPanel = workspace.getByTestId('audit-history-panel')
    await auditPanel.scrollIntoViewIfNeeded()
    await expect(auditPanel).toBeVisible()
    await expect(auditPanel.getByRole('heading', { name: 'Audit history' })).toBeVisible()

    await expect(auditPanel.getByText('Loading audit history')).not.toBeVisible({
      timeout: 15_000,
    })

    const auditHasData =
      (await auditPanel.locator('table tbody tr').count()) > 0 ||
      (await auditPanel.getByText(/No audit events match/i).count()) > 0
    expect(auditHasData).toBeTruthy()
  })
})
