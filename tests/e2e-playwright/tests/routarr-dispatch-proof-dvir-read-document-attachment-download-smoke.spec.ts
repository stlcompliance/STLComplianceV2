import { test, expect } from '@playwright/test'

import { ensureRoutArrDocumentAttachmentDownloadFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr dispatch proof/DVIR read document attachment download @requires-live', () => {
  let hasDownloadFixture = false
  let fixtureTripId: string | null = null
  let fixtureAttachmentId: string | null = null
  let fixtureFileName: string | null = null

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
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    try {
      const fixture = await ensureRoutArrDocumentAttachmentDownloadFixture()
      hasDownloadFixture = Boolean(
        fixture.tripId && fixture.attachmentId && fixture.fileName,
      )
      fixtureTripId = fixture.tripId
      fixtureAttachmentId = fixture.attachmentId
      fixtureFileName = fixture.fileName
    } catch {
      hasDownloadFixture = false
      fixtureTripId = null
      fixtureAttachmentId = null
      fixtureFileName = null
    }
  })

  test('handoff loads dispatch proof/DVIR read panel and downloads proof document attachment', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('trip-proof-dvir-read-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Trip proof & DVIR' })).toBeVisible()

    const tripInput = panel.getByPlaceholder('Paste trip GUID')
    const loadButton = panel.getByRole('button', { name: 'Load execution' })
    await expect(tripInput).toBeVisible()
    await expect(loadButton).toBeVisible()
    await expect(loadButton).toBeDisabled()

    if (!hasDownloadFixture || !fixtureTripId || !fixtureAttachmentId || !fixtureFileName) {
      await tripInput.fill('00000000-0000-0000-0000-000000000001')
      await expect(loadButton).toBeEnabled()
      return
    }

    await tripInput.fill(fixtureTripId)
    await expect(loadButton).toBeEnabled()
    await loadButton.click()

    await expect(panel.getByText(/Proof records \(1\)/)).toBeVisible({ timeout: 15_000 })

    const attachmentButton = panel.getByTestId(`proof-attachment-${fixtureAttachmentId}`)
    await expect(attachmentButton).toBeVisible({ timeout: 15_000 })
    await expect(attachmentButton).toContainText(/document:/i)

    const downloadPromise = page.waitForEvent('download', { timeout: 30_000 })
    await attachmentButton.click()
    const download = await downloadPromise
    expect(download.suggestedFilename()).toBe(fixtureFileName)
  })
})
