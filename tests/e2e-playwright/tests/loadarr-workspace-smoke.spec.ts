import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('LoadArr workspace @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('loadarr'))) {
      testInfo.skip(true, 'LoadArr frontend (5182) is unreachable.')
    }
  })

  test('handoff opens the warehouse workspace with core execution routes', async ({ page }) => {
    const expectDetailRoute = async (path: string, detailLabel: string, expectedText: string) => {
      await page.goto(new URL(path, page.url()).toString())
      await expect(page).toHaveURL(new RegExp(`${path.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}$`))
      await expect(page.getByLabel(detailLabel)).toContainText(expectedText)
    }

    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'loadarr')

    await page.goto(new URL('/inventory', page.url()).toString())
    await expect(page).toHaveURL(/\/work\/inventory$/)

    await expect(page.getByRole('heading', { name: 'Warehouse execution' })).toBeVisible({
      timeout: 15_000,
    })
    await expect(page.getByLabel('Warehouse metrics')).toBeVisible()
    await expect(page.getByText('Active locations')).toBeVisible()

    await page.goto(new URL('/work/receiving', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Manual receiving' })).toBeVisible({
      timeout: 15_000,
    })
    await expect(page.getByLabel('Guided receiving workflow')).toBeVisible()
    await expect(page.getByLabel('Receiving type')).toBeVisible()

    await expectDetailRoute('/work/expected-receipts/task-receive-24018', 'Expected receipt detail', 'EXP-4018')
    await expectDetailRoute('/work/receiving/recv-24018', 'Receiving completion audit', 'RCV-24018')
    await expectDetailRoute('/work/transfers/xfer-24018-putaway', 'Transfer completion audit', 'TRF-24018')
    await expectDetailRoute('/work/backorders/truck-stock-17-rotor', 'Backorder detail', 'TRK-17-ROTOR')
    await expectDetailRoute('/supply/vendor-returns/bal-brake-rotor', 'Vendor return detail', 'RT-7781')

    await page.goto(new URL('/work/unexplained', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Unexplained inventory' })).toBeVisible({
      timeout: 15_000,
    })
    await expect(page.getByLabel('Unexplained inventory workflow')).toBeVisible()

    await expectDetailRoute('/work/cycle-counts/count-8021', 'Count approval and adjustment detail', 'CNT-260602-1945')
    await expectDetailRoute('/records/adjustment-history/adj-count-8021', 'Adjustment detail', 'ADJ-260602-1954')
    await expectDetailRoute('/work/holds/hold-adh-49', 'Create inventory hold', 'quality_hold')
    await expectDetailRoute('/work/exceptions/quarantine', 'Exception detail', 'UNX-ADH-49')
    await expectDetailRoute('/work/shipping/handoff-rt-7781', 'Handoff detail', 'RT-7781')

    await page.goto(new URL('/admin/integrations', page.url()).toString())
    await expect(page.getByLabel('Route and product handoffs')).toBeVisible()

    await page.goto(new URL('/work/transfers', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Controlled transfer' })).toBeVisible({
      timeout: 15_000,
    })
    await expect(page.getByLabel('Guided transfer workflow')).toBeVisible()

    await page.goto(new URL('/work/holds', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Create hold' })).toBeVisible({
      timeout: 15_000,
    })
    await expect(page.getByLabel('Holds and quarantine')).toBeVisible()

    await page.goto(new URL('/admin/permissions', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Permission catalog' })).toBeVisible({
      timeout: 15_000,
    })
    await expect(page.getByLabel('Permission mapping summary')).toBeVisible()
  })
})
