import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('Compliance Core audit delivery orchestration @requires-live', () => {
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

  test('admin orchestration panel shows status sections and trigger controls', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await page.goto(new URL('/admin', page.url()).toString())

    const panel = page.getByTestId('compliancecore-audit-delivery-orchestration-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Audit delivery orchestration' })).toBeVisible()

    await expect(panel.getByText('Loading orchestration status')).not.toBeVisible({
      timeout: 15_000,
    })

    const scheduled = panel.getByTestId('compliancecore-orchestration-scheduled-eval')
    await expect(scheduled).toBeVisible()
    await expect(scheduled.getByText(/Pending packs:/i)).toBeVisible()
    const scheduledHasHistory =
      (await scheduled.getByTestId('compliancecore-orchestration-scheduled-last-run').count()) > 0 ||
      (await scheduled.getByText(/No scheduled evaluation runs yet/i).count()) > 0
    expect(scheduledHasHistory).toBeTruthy()

    const m12 = panel.getByTestId('compliancecore-orchestration-m12-batch')
    await expect(m12).toBeVisible()
    await expect(m12.getByText(/Worker:/i)).toBeVisible()
    await expect(m12.getByText(/Last audit delivery hook:/i)).toBeVisible()

    const auditJobs = panel.getByTestId('compliancecore-orchestration-audit-jobs')
    await expect(auditJobs).toBeVisible()
    await expect(auditJobs.getByText(/Pending\/processing:/i)).toBeVisible()

    const scheduledTrigger = panel.getByTestId('compliancecore-orchestration-trigger-scheduled-eval')
    await expect(scheduledTrigger).toBeVisible()
    await expect(scheduledTrigger).toBeEnabled()

    const m12Trigger = panel.getByTestId('compliancecore-orchestration-trigger-m12-batch')
    await expect(m12Trigger).toBeVisible()
  })
})
