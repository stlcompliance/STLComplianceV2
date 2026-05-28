import { test, expect } from '@playwright/test'

import {
  issueSharedWorkerServiceToken,
  loginNexArr,
  processTrainArrAuditPackageGenerationBatch,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('TrainArr settings audit export @requires-live', () => {
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

  test('settings audit export manifest, date filters, summary, and background ZIP job', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'trainarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('trainarr-audit-package-export-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Training audit package export' })).toBeVisible()

    await expect(panel.getByRole('listitem').first()).toBeVisible({ timeout: 15_000 })

    await expect(panel.getByText('From date', { exact: true })).toBeVisible()
    await expect(panel.getByText('To date', { exact: true })).toBeVisible()

    await expect(panel.getByRole('button', { name: 'Download ZIP package' })).toBeVisible()
    await expect(panel.getByRole('button', { name: 'Background ZIP export' })).toBeVisible()

    await panel.getByRole('button', { name: 'Preview JSON export' }).click()
    await expect(panel.getByText(/Audit events:/)).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByText(/Assignments:/)).toBeVisible()

    const downloadPromise = page.waitForEvent('download', { timeout: 30_000 })
    await panel.getByRole('button', { name: 'Download ZIP package' }).click()
    const download = await downloadPromise
    expect(download.suggestedFilename()).toMatch(/trainarr-audit-package.*\.zip/i)

    await panel.getByRole('button', { name: 'Background ZIP export' }).click()
    const jobStatus = panel.getByTestId('audit-package-job-status')
    await expect(jobStatus).toBeVisible({ timeout: 15_000 })
    await expect(jobStatus).toContainText(/pending|processing/i)

    const adminToken = await loginNexArr()
    const workerToken = await issueSharedWorkerServiceToken(
      adminToken,
      ['trainarr'],
      'trainarr.audit_packages.generate',
    )
    await processTrainArrAuditPackageGenerationBatch(workerToken)

    await expect(jobStatus).toHaveAttribute('data-job-status', 'completed', { timeout: 30_000 })
  })
})
