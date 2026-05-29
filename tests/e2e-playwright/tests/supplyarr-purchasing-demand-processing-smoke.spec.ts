import { test, expect } from '@playwright/test'

import { ensureSupplyArrDemandProcessingFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr purchasing demand processing @requires-live', () => {
  let demandRefId: string
  let sourceRefKey: string
  let title: string

  test.beforeAll(async ({}, testInfo) => {
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

    const fixture = await ensureSupplyArrDemandProcessingFixture()
    demandRefId = fixture.demandRefId
    sourceRefKey = fixture.sourceRefKey
    title = fixture.title
  })

  test('handoff opens purchasing demand processing panel with pending queue and operator controls', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    await page.goto(new URL('/purchasing', page.url()).toString())

    const panel = page.getByTestId('demand-processing-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Demand processing' })).toBeVisible()

    await expect(panel.getByTestId('demand-processing-summary')).toBeVisible()
    await expect(panel.getByTestId('demand-processing-pending-count')).toContainText('Pending:')

    const pendingQueue = panel.getByTestId('demand-processing-pending-queue')
    await expect(pendingQueue).toBeVisible({ timeout: 15_000 })

    const row = panel.getByTestId(`demand-processing-row-${demandRefId}`)
    await expect(row).toBeVisible({ timeout: 15_000 })
    await expect(row).toContainText(sourceRefKey)
    await expect(row).toContainText(title)

    await expect(row.getByTestId(`demand-processing-retry-${demandRefId}`)).toBeVisible()
    await expect(row.getByTestId(`demand-processing-create-pr-${demandRefId}`)).toBeVisible()

    await row.getByTestId(`demand-processing-view-status-${demandRefId}`).click()
    const detail = row.getByTestId('demand-processing-detail')
    await expect(detail).toBeVisible({ timeout: 15_000 })
    await expect(detail).toContainText('Line availability')
    await expect(detail).toContainText('(short)')
  })
})
