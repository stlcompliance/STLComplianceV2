import { test, expect } from '@playwright/test'

import { ensureRoutArrSignatureAttachmentUploadFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr signature attachment journey @requires-live', () => {
  let hasJourneyFixture = false
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
      const fixture = await ensureRoutArrSignatureAttachmentUploadFixture()
      hasJourneyFixture = Boolean(fixture.tripId && fixture.pickupProofId)
      fixtureTripId = fixture.tripId
      fixturePickupProofId = fixture.pickupProofId
      fixtureFileName = fixture.expectedFileName
    } catch {
      hasJourneyFixture = false
      fixtureTripId = null
      fixturePickupProofId = null
      fixtureFileName = null
    }
  })

  test('handoff saves delivery signature on driver portal then downloads it on dispatch read panel', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/driver-portal', page.url()).toString())

    const driverPanel = page.getByTestId('driver-portal-panel')
    await expect(driverPanel).toBeVisible({ timeout: 15_000 })
    await expect(driverPanel.getByRole('heading', { name: 'Driver portal' })).toBeVisible()

    if (!hasJourneyFixture || !fixtureTripId || !fixturePickupProofId || !fixtureFileName) {
      const hasAnyTrip =
        (await driverPanel.locator('[data-testid^="driver-portal-trip-"]').count()) > 0
      if (hasAnyTrip) {
        await expect(
          driverPanel.locator('[data-testid^="driver-portal-trip-"]').first(),
        ).toBeVisible()
      }
      return
    }

    const tripCard = driverPanel.getByTestId(`driver-portal-trip-${fixtureTripId}`)
    await expect(tripCard).toBeVisible({ timeout: 15_000 })

    const captureSection = tripCard.getByTestId(`driver-portal-proof-dvir-${fixtureTripId}`)
    await expect(captureSection).toBeVisible()

    const pickupAttachmentPanel = captureSection.getByTestId(
      `capture-attachments-proof-${fixturePickupProofId}`,
    )
    await expect(pickupAttachmentPanel).toBeVisible()
    await expect(pickupAttachmentPanel).toContainText('pickup proof attachments')

    const startButton = tripCard.getByRole('button', { name: 'Start trip' })
    await expect(startButton).toBeEnabled({ timeout: 20_000 })
    await startButton.click()
    await expect(tripCard).toContainText(/in progress/i, { timeout: 20_000 })

    await tripCard.getByRole('button', { name: 'Quick delivery proof' }).click()
    await expect(captureSection.getByText('delivery proof attachments')).toBeVisible({
      timeout: 20_000,
    })

    const deliveryAttachmentPanel = captureSection
      .locator('[data-testid^="capture-attachments-proof-"]')
      .filter({ hasText: 'delivery proof attachments' })
    await expect(deliveryAttachmentPanel).toBeVisible()

    const signaturePad = deliveryAttachmentPanel.getByTestId('signature-pad')
    await expect(signaturePad).toBeVisible()

    const canvas = signaturePad.locator('canvas')
    const box = await canvas.boundingBox()
    if (box) {
      await page.mouse.move(box.x + 12, box.y + 20)
      await page.mouse.down()
      await page.mouse.move(box.x + 140, box.y + 60)
      await page.mouse.up()
    }

    await signaturePad.getByRole('button', { name: 'Save signature' }).click()
    await expect(deliveryAttachmentPanel.getByText(/signature:/i)).toBeVisible({
      timeout: 20_000,
    })
    await expect(deliveryAttachmentPanel).toContainText(fixtureFileName)

    await page.goto(new URL('/dispatch', page.url()).toString())

    const readPanel = page.getByTestId('trip-proof-dvir-read-panel')
    await readPanel.scrollIntoViewIfNeeded()
    await expect(readPanel).toBeVisible({ timeout: 15_000 })
    await expect(readPanel.getByRole('heading', { name: 'Trip proof & DVIR' })).toBeVisible()

    const tripInput = readPanel.getByPlaceholder('Paste trip GUID')
    const loadButton = readPanel.getByRole('button', { name: 'Load execution' })
    await tripInput.fill(fixtureTripId)
    await expect(loadButton).toBeEnabled()
    await loadButton.click()

    await expect(readPanel.getByText(/Proof records \(2\)/)).toBeVisible({ timeout: 15_000 })

    const attachmentButton = readPanel
      .locator('[data-testid^="proof-attachment-"]')
      .filter({
        hasText: new RegExp(
          `signature:\\s*${fixtureFileName.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}`,
          'i',
        ),
      })
    await expect(attachmentButton).toBeVisible({ timeout: 15_000 })

    const downloadPromise = page.waitForEvent('download', { timeout: 30_000 })
    await attachmentButton.click()
    const download = await downloadPromise
    expect(download.suggestedFilename()).toBe(fixtureFileName)
  })
})
