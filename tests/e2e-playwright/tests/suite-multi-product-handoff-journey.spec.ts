import { test, expect } from '@playwright/test'

import {
  launchProductHandoffFromSuite,
  returnToSuiteApp,
} from '../support/handoffJourney.js'
import {
  demoTenant,
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

const journeyProducts = [
  {
    productKey: 'staffarr',
    surfaceHeading: 'People directory',
  },
  {
    productKey: 'trainarr',
    surfaceHeading: 'Training qualification workspace',
  },
  {
    productKey: 'compliancecore',
    surfaceHeading: 'Compliance authority registry',
  },
] as const

test.describe('Suite multi-product handoff journey @requires-live', () => {
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

  test('chains StaffArr, TrainArr, and Compliance Core handoffs in one session', async ({
    page,
  }, testInfo) => {
    for (const step of journeyProducts) {
      if (!(await isHandoffFrontendReachable(step.productKey))) {
        testInfo.skip(true, `${step.productKey} frontend is unreachable for multi-product journey.`)
      }
    }

    await signInFromSuite(page)

    for (const step of journeyProducts) {
      await launchProductHandoffFromSuite(page, step.productKey)

      await expect(page.getByTestId('workspace-tenant-display-name')).toBeVisible({
        timeout: 15_000,
      })
      await expect(page.getByTestId('workspace-tenant-display-name')).toHaveText(
        demoTenant.displayName,
      )
      await expect(page.getByRole('heading', { name: step.surfaceHeading, exact: true })).toBeVisible({
        timeout: 15_000,
      })

      await returnToSuiteApp(page)
    }
  })
})
