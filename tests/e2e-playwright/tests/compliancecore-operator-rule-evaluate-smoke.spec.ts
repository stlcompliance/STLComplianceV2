import { test, expect } from '@playwright/test'

import { seedComplianceCoreJourney } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('Compliance Core operator rule evaluation @requires-live', () => {
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

  test('operator seeds rule content and evaluates driver qualification pack', async ({ page }) => {
    await seedComplianceCoreJourney()
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await expect(
      page.getByRole('heading', { name: 'Compliance registry' }),
    ).toBeVisible({ timeout: 15_000 })

    await page.goto(new URL('/evaluation', page.url()).toString())
    await expect(page.getByRole('heading', { name: 'Rule evaluation' })).toBeVisible({
      timeout: 15_000,
    })
    const panel = page.getByTestId('rule-evaluation-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })

    await page.getByTestId('rule-evaluation-seed-content').click()
    await expect(page.getByTestId('rule-evaluation-run')).toBeEnabled({ timeout: 15_000 })

    const licenseFact = panel.locator('label').filter({ hasText: 'driver_license_valid' })
    const medicalFact = panel.locator('label').filter({ hasText: 'medical_cert_on_file' })
    if (await licenseFact.isVisible()) {
      await licenseFact.getByRole('checkbox').check()
    }
    if (await medicalFact.isVisible()) {
      await medicalFact.getByRole('checkbox').check()
    }

    await page.getByTestId('rule-evaluation-run').click()
    const result = page.getByTestId('rule-evaluation-latest-result')
    await expect(result).toBeVisible({ timeout: 15_000 })
    await expect(result).toHaveAttribute('data-overall-result', 'pass')
    await expect(result).toContainText(/pass/i)
  })
})
