import { test, expect, type Page } from '@playwright/test'

import {
  ensureTrainArrRoutarrQualificationGateAllowFixture,
  ensureTrainArrRoutarrQualificationGateBlockFixture,
  type TrainArrRoutarrQualificationGateJourneyFixture,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite, returnToSuiteApp } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

const journeyDriverPersonId = '22222222-2222-2222-2222-222222222201'

async function runTrainArrQualificationCheckInUi(
  page: Page,
  fixture: TrainArrRoutarrQualificationGateJourneyFixture,
  expectedOutcome: 'allow' | 'block',
): Promise<void> {
  await launchProductHandoffFromSuite(page, 'trainarr')

  await page.goto(new URL('/qualifications', page.url()).toString())

  const panel = page.getByTestId('authorization-check-operations-panel')
  await expect(panel).toBeVisible({ timeout: 15_000 })

  await panel.getByTestId('authorization-check-person-advanced').fill(fixture.driverPersonId)

  await panel.locator('#authorization-check-definition').selectOption(fixture.trainingDefinitionId)

  await panel.getByTestId('qualification-check-rule-pack').selectOption('driver_qualification')

  await panel.getByRole('button', { name: /run qualification check/i }).click()

  const result = panel.getByTestId('qualification-check-latest-result')
  await expect(result).toBeVisible({ timeout: 15_000 })
  await expect(result).toHaveAttribute('data-outcome', expectedOutcome)
}

async function assignDriverOnUnassignedTrip(
  page: Page,
  fixture: TrainArrRoutarrQualificationGateJourneyFixture,
  confirmDialog: boolean,
): Promise<void> {
  await launchProductHandoffFromSuite(page, 'routarr')

  await page.goto(new URL('/dispatch', page.url()).toString())

  const panel = page.getByTestId('unassigned-work-queue-panel')
  await panel.scrollIntoViewIfNeeded()
  await expect(panel).toBeVisible({ timeout: 15_000 })

  const tripRow = panel.getByTestId(`unassigned-trip-${fixture.tripId}`)
  await expect(tripRow).toBeVisible({ timeout: 15_000 })

  const driverSelect = tripRow.getByRole('combobox')
  await driverSelect.selectOption({ value: fixture.driverPersonId })

  page.once('dialog', (dialog) => {
    expect(dialog.message()).toMatch(/driver eligibility|assignment blocked/i)
    if (confirmDialog) {
      void dialog.accept()
    } else {
      void dialog.dismiss()
    }
  })

  await tripRow.getByRole('button', { name: 'Assign' }).click()
}

test.describe('Cross-product TrainArr qualification gate to RoutArr assign @requires-live', () => {
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
  })

  test('operator confirms TrainArr block then RoutArr assign is blocked', async ({ page }, testInfo) => {
    if (!(await isHandoffFrontendReachable('trainarr'))) {
      testInfo.skip(true, 'TrainArr frontend (5176) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    let fixture: TrainArrRoutarrQualificationGateJourneyFixture
    try {
      fixture = await ensureTrainArrRoutarrQualificationGateBlockFixture()
    } catch {
      testInfo.skip(true, 'TrainArr→RoutArr qualification gate block fixture could not be created.')
    }

    expect(fixture!.driverPersonId).toBe(journeyDriverPersonId)

    await signInFromSuite(page)
    await runTrainArrQualificationCheckInUi(page, fixture!, 'block')
    await returnToSuiteApp(page)
    await assignDriverOnUnassignedTrip(page, fixture!, false)

    const panel = page.getByTestId('unassigned-work-queue-panel')
    await expect(panel.getByTestId('unassigned-queue-status')).toContainText(
      'Assignment cancelled.',
      { timeout: 15_000 },
    )
    await expect(panel.getByTestId(`unassigned-trip-${fixture!.tripId}`)).toBeVisible()
  })

  test('operator overrides TrainArr eligibility block and assigns driver', async ({ page }, testInfo) => {
    if (!(await isHandoffFrontendReachable('trainarr'))) {
      testInfo.skip(true, 'TrainArr frontend (5176) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    let fixture: TrainArrRoutarrQualificationGateJourneyFixture
    try {
      fixture = await ensureTrainArrRoutarrQualificationGateBlockFixture()
    } catch {
      testInfo.skip(true, 'TrainArr→RoutArr qualification gate block fixture could not be created.')
    }

    await signInFromSuite(page)
    await runTrainArrQualificationCheckInUi(page, fixture!, 'block')
    await returnToSuiteApp(page)
    await assignDriverOnUnassignedTrip(page, fixture!, true)

    const panel = page.getByTestId('unassigned-work-queue-panel')
    await expect(panel.getByTestId('unassigned-queue-status')).toContainText('Driver assigned.', {
      timeout: 15_000,
    })
    await expect(panel.getByTestId(`unassigned-trip-${fixture!.tripId}`)).not.toBeVisible({
      timeout: 15_000,
    })
  })

  test('operator confirms TrainArr allow then RoutArr assigns without eligibility block', async ({
    page,
  }, testInfo) => {
    if (!(await isHandoffFrontendReachable('trainarr'))) {
      testInfo.skip(true, 'TrainArr frontend (5176) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    let fixture: TrainArrRoutarrQualificationGateJourneyFixture
    try {
      fixture = await ensureTrainArrRoutarrQualificationGateAllowFixture()
    } catch {
      testInfo.skip(true, 'TrainArr→RoutArr qualification gate allow fixture could not be created.')
    }

    await signInFromSuite(page)
    await runTrainArrQualificationCheckInUi(page, fixture!, 'allow')
    await returnToSuiteApp(page)
    await launchProductHandoffFromSuite(page, 'routarr')

    await page.goto(new URL('/dispatch', page.url()).toString())

    const panel = page.getByTestId('unassigned-work-queue-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })

    const tripRow = panel.getByTestId(`unassigned-trip-${fixture!.tripId}`)
    await expect(tripRow).toBeVisible({ timeout: 15_000 })

    const driverSelect = tripRow.getByRole('combobox')
    await driverSelect.selectOption({ value: fixture!.driverPersonId })

    let dialogShown = false
    page.once('dialog', () => {
      dialogShown = true
    })

    await tripRow.getByRole('button', { name: 'Assign' }).click()

    await expect(panel.getByTestId('unassigned-queue-status')).toContainText('Driver assigned.', {
      timeout: 15_000,
    })
    expect(dialogShown).toBe(false)
    await expect(panel.getByTestId(`unassigned-trip-${fixture!.tripId}`)).not.toBeVisible({
      timeout: 15_000,
    })
  })
})
