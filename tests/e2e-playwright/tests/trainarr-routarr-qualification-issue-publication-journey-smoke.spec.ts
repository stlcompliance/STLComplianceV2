import { test, expect, type Page } from '@playwright/test'

import {
  ensureTrainArrDriverQualificationCompletionFixture,
  ensureTrainArrRoutarrQualificationIssuePublicationTripFixture,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite, returnToSuiteApp } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

const journeyDriverPersonId = '22222222-2222-2222-2222-222222222201'

async function completeTrainArrAssignmentInUi(page: Page, assignmentId: string): Promise<void> {
  await launchProductHandoffFromSuite(page, 'trainarr')
  await page.goto(new URL(`/assignments/${assignmentId}`, page.url()).toString())

  await expect(page.getByTestId('assignment-workspace')).toBeVisible({ timeout: 15_000 })

  const completionPanel = page
    .locator('section')
    .filter({ has: page.getByRole('heading', { name: /evaluation & signoffs/i }) })
    .first()

  await expect(completionPanel).toBeVisible({ timeout: 15_000 })
  await completionPanel.getByLabel('Result').selectOption('pass')
  await completionPanel.getByLabel('Score (optional)').fill('100')
  await completionPanel.getByLabel('Notes').fill('Browser completion proving qualification publication.')
  await completionPanel.getByRole('button', { name: /submit evaluation/i }).click()

  await completionPanel.getByLabel('Signoff notes (optional)').fill('Browser signoff path.')
  await completionPanel.getByRole('button', { name: /trainee signoff/i }).click()
  await completionPanel.getByRole('button', { name: /trainer signoff/i }).click()

  await expect(page.getByRole('button', { name: /mark assignment complete/i })).toBeEnabled({
    timeout: 15_000,
  })
  await page.getByRole('button', { name: /mark assignment complete/i }).click()

  await expect(page.getByText(/staffarr grant publication/i)).toBeVisible({ timeout: 20_000 })
  await expect(page.getByText(/qualification issued/i)).toBeVisible({ timeout: 20_000 })
}

async function assignDriverOnUnassignedTrip(
  page: Page,
  tripId: string,
): Promise<void> {
  await launchProductHandoffFromSuite(page, 'routarr')
  await page.goto(new URL('/dispatch', page.url()).toString())

  const panel = page.getByTestId('unassigned-work-queue-panel')
  await panel.scrollIntoViewIfNeeded()
  await expect(panel).toBeVisible({ timeout: 15_000 })

  const tripRow = panel.getByTestId(`unassigned-trip-${tripId}`)
  await expect(tripRow).toBeVisible({ timeout: 15_000 })

  const driverSelect = tripRow.getByRole('combobox')
  await driverSelect.selectOption({ value: journeyDriverPersonId })

  page.once('dialog', (dialog) => {
    throw new Error(`Unexpected driver eligibility dialog: ${dialog.message()}`)
  })

  await tripRow.getByRole('button', { name: 'Assign' }).click()
}

test.describe('Cross-product TrainArr qualification issue publication to RoutArr assign @requires-live', () => {
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

  test('operator completes TrainArr assignment in the browser, sees the grant publication, then RoutArr assigns the driver without a block dialog', async ({
    page,
  }, testInfo) => {
    if (!(await isHandoffFrontendReachable('trainarr'))) {
      testInfo.skip(true, 'TrainArr frontend (5176) is unreachable.')
    }
    if (!(await isHandoffFrontendReachable('routarr'))) {
      testInfo.skip(true, 'RoutArr frontend (5180) is unreachable.')
    }

    let fixture: Awaited<ReturnType<typeof ensureTrainArrDriverQualificationCompletionFixture>>
    let tripId: string
    try {
      fixture = await ensureTrainArrDriverQualificationCompletionFixture()
      tripId = (await ensureTrainArrRoutarrQualificationIssuePublicationTripFixture()).tripId
    } catch {
      testInfo.skip(true, 'TrainArr qualification completion fixture could not be created.')
      return
    }

    await signInFromSuite(page)
    await completeTrainArrAssignmentInUi(page, fixture.assignmentId)
    await returnToSuiteApp(page)
    await assignDriverOnUnassignedTrip(page, tripId)

    const panel = page.getByTestId('unassigned-work-queue-panel')
    await expect(panel.getByTestId('unassigned-queue-status')).toContainText('Driver assigned.', {
      timeout: 15_000,
    })
    await expect(panel.getByTestId(`unassigned-trip-${tripId}`)).not.toBeVisible({
      timeout: 15_000,
    })
  })
})
