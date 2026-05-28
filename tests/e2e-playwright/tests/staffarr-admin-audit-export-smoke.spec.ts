import { test, expect } from '@playwright/test'

import {
  issueSharedWorkerServiceToken,
  loginNexArr,
  processStaffArrAuditPackageGenerationBatch,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('StaffArr admin audit export @requires-live', () => {
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

  test('admin audit export manifest, summary, filters, and background ZIP job', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'staffarr')

    await page.goto(new URL('/admin', page.url()).toString())

    const panel = page.getByTestId('staffarr-audit-export-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Audit package export' })).toBeVisible()

    const manifest = page.getByTestId('staffarr-audit-manifest-section')
    await expect(manifest.getByRole('listitem').first()).toBeVisible({ timeout: 15_000 })

    const summary = page.getByTestId('staffarr-audit-summary-section')
    await expect(summary).toBeVisible()
    await expect(summary.getByTestId('staffarr-audit-summary-counts')).toBeVisible({
      timeout: 15_000,
    })

    const timeline = page.getByTestId('staffarr-audit-timeline-section')
    await expect(timeline).toBeVisible()
    await expect(timeline).not.toContainText('Loading audit timeline', { timeout: 15_000 })

    await expect(page.getByTestId('staffarr-audit-filter-action')).toBeVisible()
    await expect(page.getByTestId('staffarr-audit-download-csv')).toBeVisible()
    await expect(page.getByTestId('staffarr-audit-download-json')).toBeVisible()

    await page.getByRole('button', { name: 'Preview JSON export' }).click()
    const jsonPreview = page.getByTestId('staffarr-audit-json-preview')
    await expect(jsonPreview).toBeVisible({ timeout: 15_000 })
    await expect(jsonPreview).toContainText(/Package/)

    const downloadPromise = page.waitForEvent('download', { timeout: 30_000 })
    await page.getByRole('button', { name: 'Download ZIP package' }).click()
    const download = await downloadPromise
    expect(download.suggestedFilename()).toMatch(/staffarr-audit-package.*\.zip/i)

    await page.getByRole('button', { name: 'Background ZIP export' }).click()
    const jobStatus = page.getByTestId('audit-package-job-status')
    await expect(jobStatus).toBeVisible({ timeout: 15_000 })
    await expect(jobStatus).toContainText(/pending|processing/i)

    const adminToken = await loginNexArr()
    const workerToken = await issueSharedWorkerServiceToken(
      adminToken,
      ['staffarr'],
      'staffarr.audit_packages.generate',
    )
    await processStaffArrAuditPackageGenerationBatch(workerToken)

    await expect(jobStatus).toHaveAttribute('data-job-status', 'completed', { timeout: 30_000 })
  })
})
