import { test, expect } from '@playwright/test'

import {
  issueSharedWorkerNexArrServiceToken,
  loginNexArr,
  processPlatformAuditPackageGenerationBatch,
} from '../support/e2eApi.js'
import { isLiveModeEnabled, isLiveStackReachable, signInFromSuite } from '../support/liveProbe.js'

test.describe('Suite platform-admin audit export @requires-live', () => {
  test.beforeEach(async ({}, testInfo) => {
    if (!isLiveModeEnabled()) {
      testInfo.skip(true, 'Set E2E_LIVE=1 to run browser E2E against docker-compose.')
    }
    if (!(await isLiveStackReachable())) {
      testInfo.skip(
        true,
        'Suite frontend (5174) and NexArr API (5101) must be running. Start scripts/ops/e2e-stack-up.ps1 and e2e-frontends-preview.ps1.',
      )
    }
  })

  test('platform admin audit export manifest, timeline, sync JSON, and background ZIP job', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await page.goto('/app/platform-admin/audit-export')

    const panel = page.getByTestId('platform-audit-export-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(
      page.getByRole('heading', { name: 'Platform audit package export' }),
    ).toBeVisible()

    const manifest = page.getByTestId('platform-audit-manifest-section')
    await expect(manifest.getByRole('listitem').first()).toBeVisible({ timeout: 15_000 })

    const summary = page.getByTestId('platform-audit-summary-section')
    await expect(summary).toBeVisible()
    await expect(summary.getByTestId('platform-audit-summary-counts')).toBeVisible({
      timeout: 15_000,
    })

    const timeline = page.getByTestId('platform-audit-timeline-section')
    await expect(timeline).toBeVisible()
    await expect(timeline).not.toContainText('Loading audit timeline', { timeout: 15_000 })

    await expect(page.getByTestId('platform-audit-filter-action')).toBeVisible()

    await page.getByRole('button', { name: 'Preview JSON export' }).click()
    const jsonPreview = page.getByTestId('platform-audit-json-preview')
    await expect(jsonPreview).toBeVisible({ timeout: 15_000 })
    await expect(jsonPreview).toContainText(/Package/)

    const downloadPromise = page.waitForEvent('download', { timeout: 30_000 })
    await page.getByRole('button', { name: 'Download ZIP package' }).click()
    const download = await downloadPromise
    expect(download.suggestedFilename()).toMatch(/nexarr-platform-audit-package.*\.zip/i)

    await page.getByRole('button', { name: 'Background ZIP export' }).click()
    const jobStatus = page.getByTestId('platform-audit-job-status')
    await expect(jobStatus).toBeVisible({ timeout: 15_000 })
    await expect(jobStatus).toContainText(/pending|processing/i)

    const adminToken = await loginNexArr()
    const workerToken = await issueSharedWorkerNexArrServiceToken(adminToken)
    await processPlatformAuditPackageGenerationBatch(workerToken)

    await expect(jobStatus).toHaveAttribute('data-job-status', 'completed', { timeout: 30_000 })
  })
})
