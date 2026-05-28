import { test, expect } from '@playwright/test'

import {
  issueSharedWorkerServiceToken,
  loginNexArr,
  processRoutArrAuditPackageGenerationBatch,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr reports audit export @requires-live', () => {
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

  test('reports audit export manifest, summary, filters, and background ZIP job', async ({ page }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/reports', page.url()).toString())

    const panel = page.getByTestId('routarr-audit-export-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Audit package export' })).toBeVisible()

    const manifest = page.getByTestId('routarr-audit-manifest-section')
    await expect(manifest.getByRole('listitem').first()).toBeVisible({ timeout: 15_000 })

    const summary = page.getByTestId('routarr-audit-summary-section')
    await expect(summary).toBeVisible()
    await expect(summary.getByTestId('routarr-audit-summary-counts')).toBeVisible({
      timeout: 15_000,
    })

    const timeline = page.getByTestId('routarr-audit-timeline-section')
    await expect(timeline).toBeVisible()
    await expect(timeline).not.toContainText('Loading audit timeline', { timeout: 15_000 })

    await expect(page.getByTestId('routarr-audit-filter-action')).toBeVisible()
    await expect(page.getByTestId('routarr-audit-download-csv')).toBeVisible()
    await expect(page.getByTestId('routarr-audit-download-json')).toBeVisible()

    await page.getByRole('button', { name: 'Preview JSON export' }).click()
    const jsonPreview = page.getByTestId('routarr-audit-json-preview')
    await expect(jsonPreview).toBeVisible({ timeout: 15_000 })
    await expect(jsonPreview).toContainText(/Package/)

    const downloadPromise = page.waitForEvent('download', { timeout: 30_000 })
    await page.getByRole('button', { name: 'Download ZIP package' }).click()
    const download = await downloadPromise
    expect(download.suggestedFilename()).toMatch(/routarr-audit-package.*\.zip/i)

    await page.getByRole('button', { name: 'Background ZIP export' }).click()
    const jobStatus = page.getByTestId('routarr-audit-job-status')
    await expect(jobStatus).toBeVisible({ timeout: 15_000 })
    await expect(jobStatus).toContainText(/pending|processing/i)

    const adminToken = await loginNexArr()
    const workerToken = await issueSharedWorkerServiceToken(
      adminToken,
      ['routarr'],
      'routarr.audit_packages.generate',
    )
    await processRoutArrAuditPackageGenerationBatch(workerToken)

    await expect(jobStatus).toHaveAttribute('data-job-status', 'completed', { timeout: 30_000 })
  })
})
