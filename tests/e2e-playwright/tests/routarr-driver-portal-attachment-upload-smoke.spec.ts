import { test, expect } from '@playwright/test'

import { ensureRoutArrAttachmentUploadFixture } from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('RoutArr driver portal attachment upload @requires-live', () => {
  let hasAttachmentFixture = false
  let fixtureTripId: string | null = null
  let fixturePickupProofId: string | null = null

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
      const fixture = await ensureRoutArrAttachmentUploadFixture()
      hasAttachmentFixture = Boolean(fixture.tripId && fixture.pickupProofId)
      fixtureTripId = fixture.tripId
      fixturePickupProofId = fixture.pickupProofId
    } catch {
      hasAttachmentFixture = false
      fixtureTripId = null
      fixturePickupProofId = null
    }
  })

  test('handoff uploads pickup photo and delivery signature on driver portal attachment panels', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/driver-portal', page.url()).toString())

    const panel = page.getByTestId('driver-portal-panel')
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(panel.getByRole('heading', { name: 'Driver portal' })).toBeVisible()

    if (!hasAttachmentFixture || !fixtureTripId || !fixturePickupProofId) {
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

    const photoInput = pickupAttachmentPanel.locator('input[type="file"][accept="image/*"]')
    await photoInput.setInputFiles({
      name: 'pickup-e2e.jpg',
      mimeType: 'image/jpeg',
      buffer: Buffer.from([0xff, 0xd8, 0xff, 0xd9]),
    })

    await expect(pickupAttachmentPanel.getByText(/photo:/i)).toBeVisible({ timeout: 20_000 })

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
  })
})
