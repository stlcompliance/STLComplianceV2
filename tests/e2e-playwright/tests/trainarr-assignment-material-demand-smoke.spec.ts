import { test, expect } from '@playwright/test'

import { ensureTrainArrMaterialDemandFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('TrainArr assignment material demand @requires-live', () => {
  let assignmentId: string
  let procurementStatusSeeded = false

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
    if (!(await isHandoffFrontendReachable('trainarr'))) {
      testInfo.skip(true, 'TrainArr frontend (5176) is unreachable.')
    }

    const fixture = await ensureTrainArrMaterialDemandFixture()
    assignmentId = fixture.trainingAssignmentId
    procurementStatusSeeded = fixture.procurementStatusSeeded
  })

  test('handoff opens assignment workspace material demand panel with lines and procurement visibility', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'trainarr')

    await page.goto(new URL(`/assignments/${assignmentId}`, page.url()).toString())

    await expect(page.getByTestId('assignment-workspace')).toBeVisible({ timeout: 15_000 })

    const panel = page.getByTestId('assignment-material-demand')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByText(/Material demand \(SupplyArr\)/i)).toBeVisible()

    if (procurementStatusSeeded) {
      await expect(panel.getByText('pr_submitted').first()).toBeVisible({ timeout: 15_000 })
      await expect(page.getByTestId('material-demand-status-timeline')).toBeVisible()
      await expect(page.getByTestId('material-demand-status-timeline')).toContainText(
        /E2E Playwright procurement status/i,
      )
      return
    }

    const partNumber = `E2E-UI-${Date.now()}`
    await panel.getByLabel('Part number').fill(partNumber)
    await panel.getByLabel('Quantity').fill('1')
    await panel.getByRole('button', { name: /Add demand line/i }).click()
    await expect(panel.getByText(partNumber)).toBeVisible({ timeout: 15_000 })

    const publishButton = panel.getByRole('button', { name: /Publish .* to SupplyArr/i })
    if (await publishButton.isVisible()) {
      await publishButton.click()
      await expect(panel.getByText('published').first()).toBeVisible({ timeout: 20_000 })
    }
  })
})
