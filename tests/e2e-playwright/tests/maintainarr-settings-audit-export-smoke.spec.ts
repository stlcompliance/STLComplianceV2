import { test, expect } from '@playwright/test'

import {
  issueSharedWorkerServiceToken,
  loginNexArr,
  processMaintainArrAuditPackageGenerationBatch,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('MaintainArr settings audit export @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('maintainarr'))) {
      testInfo.skip(true, 'MaintainArr frontend (5178) is unreachable.')
    }
  })

  test('settings audit export manifest, summary, filters, and background ZIP job', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'maintainarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('maintainarr-audit-export-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(page.getByRole('heading', { name: 'Audit package export' })).toBeVisible()

    const manifest = page.getByTestId('maintainarr-audit-manifest-section')
    await expect(manifest.getByRole('listitem').first()).toBeVisible({ timeout: 15_000 })

    const summary = page.getByTestId('maintainarr-audit-summary-section')
    await expect(summary).toBeVisible()
    await expect(summary.getByTestId('maintainarr-audit-summary-counts')).toBeVisible({
      timeout: 15_000,
    })

    const timeline = page.getByTestId('maintainarr-audit-timeline-section')
    await expect(timeline).toBeVisible()
    await expect(timeline).not.toContainText('Loading audit timeline', { timeout: 15_000 })

    await expect(page.getByTestId('maintainarr-audit-filter-action')).toBeVisible()
    await expect(page.getByTestId('maintainarr-audit-download-csv')).toBeVisible()

    await page.getByRole('button', { name: 'Preview JSON export' }).click()
    const jsonPreview = page.getByTestId('maintainarr-audit-json-preview')
    await expect(jsonPreview).toBeVisible({ timeout: 15_000 })
    await expect(jsonPreview).toContainText(/Package/)

    const downloadPromise = page.waitForEvent('download', { timeout: 30_000 })
    await page.getByRole('button', { name: 'Download ZIP package' }).click()
    const download = await downloadPromise
    expect(download.suggestedFilename()).toMatch(/maintainarr-audit-package.*\.zip/i)

    await page.getByRole('button', { name: 'Background ZIP export' }).click()
    const jobStatus = page.getByTestId('maintainarr-audit-job-status')
    await expect(jobStatus).toBeVisible({ timeout: 15_000 })
    await expect(jobStatus).toContainText(/pending|processing/i)

    const adminToken = await loginNexArr()
    const workerToken = await issueSharedWorkerServiceToken(
      adminToken,
      ['maintainarr'],
      'maintainarr.audit_packages.generate',
    )
    await processMaintainArrAuditPackageGenerationBatch(workerToken)

    await expect(jobStatus).toHaveAttribute('data-job-status', 'completed', { timeout: 30_000 })
  })
})
