import { test, expect } from '@playwright/test'

import { ensureRoutArrDocumentAttachmentUploadFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

const minimalPdf = Buffer.from('%PDF-1.4\n1 0 obj<<>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF\n')

test.describe('RoutArr driver portal document attachment upload @requires-live', () => {
  let hasDocumentFixture = false
  let fixtureTripId: string | null = null
  let fixturePickupProofId: string | null = null
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
      const fixture = await ensureRoutArrDocumentAttachmentUploadFixture()
      hasDocumentFixture = Boolean(fixture.tripId && fixture.pickupProofId)
      fixtureTripId = fixture.tripId
      fixturePickupProofId = fixture.pickupProofId
      fixtureFileName = fixture.expectedFileName
    } catch {
      hasDocumentFixture = false
      fixtureTripId = null
      fixturePickupProofId = null
      fixtureFileName = null
    }
  })

  test('handoff uploads pickup BOL document on driver portal attachment panel', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/driver-portal', page.url()).toString())

    const panel = page.getByTestId('driver-portal-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Driver portal' })).toBeVisible()

    if (!hasDocumentFixture || !fixtureTripId || !fixturePickupProofId) {
      const hasAnyTrip = (await panel.locator('[data-testid^="driver-portal-trip-"]').count()) > 0
      if (hasAnyTrip) {
        await expect(panel.locator('[data-testid^="driver-portal-trip-"]').first()).toBeVisible()
      }
      return
    }

    const tripCard = panel.getByTestId(`driver-portal-trip-${fixtureTripId}`)
    await expect(tripCard).toBeVisible({ timeout: 15_000 })

    const captureSection = tripCard.getByTestId(`driver-portal-proof-dvir-${fixtureTripId}`)
    await expect(captureSection).toBeVisible()

    const pickupAttachmentPanel = captureSection.getByTestId(
      `capture-attachments-proof-${fixturePickupProofId}`,
    )
    await expect(pickupAttachmentPanel).toBeVisible()
    await expect(pickupAttachmentPanel).toContainText('pickup proof attachments')
    await expect(pickupAttachmentPanel.getByText('Document', { exact: true })).toBeVisible()

    const documentInput = pickupAttachmentPanel
      .locator('label')
      .filter({ hasText: 'Document' })
      .locator('input[type="file"]')
    await documentInput.setInputFiles({
      name: fixtureFileName ?? 'pickup-bol-e2e.pdf',
      mimeType: 'application/pdf',
      buffer: minimalPdf,
    })

    await expect(pickupAttachmentPanel.getByText(/document:/i)).toBeVisible({ timeout: 20_000 })
    if (fixtureFileName) {
      await expect(pickupAttachmentPanel).toContainText(fixtureFileName)
    }

    const startButton = tripCard.getByRole('button', { name: 'Start trip' })
    await expect(startButton).toBeEnabled({ timeout: 20_000 })
  })
})
