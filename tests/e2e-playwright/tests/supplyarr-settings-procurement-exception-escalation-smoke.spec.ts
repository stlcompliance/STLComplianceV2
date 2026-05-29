import { test, expect } from '@playwright/test'

import {
  assertSupplyArrProcurementExceptionEscalationPendingContainsFromHandoff,
  ensureSupplyArrProcurementExceptionEscalationFixture,
  getSupplyArrProcurementExceptionEscalationSettingsFromHandoff,
} from '../support/e2eApi.js'
import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('SupplyArr settings procurement exception escalation @requires-live', () => {
  let overdueExceptionKey = ''
  let hasEscalationFixture = false

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
    if (!(await isHandoffFrontendReachable('supplyarr'))) {
      testInfo.skip(true, 'SupplyArr frontend (5179) is unreachable.')
    }

    try {
      const fixture = await ensureSupplyArrProcurementExceptionEscalationFixture()
      overdueExceptionKey = fixture.overdueExceptionKey
      hasEscalationFixture = true
    } catch {
      hasEscalationFixture = false
      overdueExceptionKey = ''
    }
  })

  test('settings escalation panel saves toggles and shows pending preview when enabled', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'supplyarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const panel = page.getByTestId('procurement-exception-escalation-settings-panel')
    await panel.scrollIntoViewIfNeeded()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(
      panel.getByRole('heading', { name: 'Procurement exception SLA escalation' }),
    ).toBeVisible()

    const enabled = panel.getByTestId('procurement-exception-escalation-enabled')
    const cooldownInput = panel.getByTestId('procurement-exception-escalation-cooldown-hours')
    const maxEscalationsInput = panel.getByTestId('procurement-exception-escalation-max-escalations')
    const notify = panel.getByTestId('procurement-exception-escalation-notify')
    await expect(enabled).toBeVisible()
    await expect(cooldownInput).toBeVisible()
    await expect(maxEscalationsInput).toBeVisible()
    await expect(notify).toBeVisible()

    const initialEnabled = await enabled.isChecked()
    const initialCooldown = await cooldownInput.inputValue()
    const initialMaxEscalations = await maxEscalationsInput.inputValue()
    const initialNotify = await notify.isChecked()
    const alternateCooldown = initialCooldown === '25' ? '26' : '25'
    const alternateMaxEscalations = initialMaxEscalations === '6' ? '7' : '6'

    await enabled.check()
    await cooldownInput.fill(alternateCooldown)
    await maxEscalationsInput.fill(alternateMaxEscalations)
    if (initialNotify) {
      await notify.uncheck()
    } else {
      await notify.check()
    }

    await panel.getByTestId('procurement-exception-escalation-save').click()
    await expect(enabled).toHaveJSProperty('checked', true)
    await expect(cooldownInput).toHaveValue(alternateCooldown)
    await expect(maxEscalationsInput).toHaveValue(alternateMaxEscalations)
    await expect(notify).toHaveJSProperty('checked', !initialNotify)

    const savedSettings = await getSupplyArrProcurementExceptionEscalationSettingsFromHandoff()
    expect(savedSettings.isEnabled).toBe(true)
    expect(savedSettings.escalationCooldownHours).toBe(Number(alternateCooldown))
    expect(savedSettings.maxEscalationsPerException).toBe(Number(alternateMaxEscalations))
    expect(savedSettings.notifyOnProcurementExceptionSlaEscalation).toBe(!initialNotify)

    await page.reload()
    await expect(panel).toBeVisible({ timeout: 15_000 })
    await expect(enabled).toHaveJSProperty('checked', true)
    await expect(cooldownInput).toHaveValue(alternateCooldown)
    await expect(maxEscalationsInput).toHaveValue(alternateMaxEscalations)
    await expect(notify).toHaveJSProperty('checked', !initialNotify)

    await expect(panel.getByRole('heading', { name: 'Due for escalation' })).toBeVisible()
    const pendingEmpty = panel.getByTestId('procurement-exception-escalation-pending-empty')
    const pendingList = panel.getByTestId('procurement-exception-escalation-pending-list')
    await expect(pendingEmpty.or(pendingList)).toBeVisible({ timeout: 15_000 })

    if (hasEscalationFixture && overdueExceptionKey) {
      await assertSupplyArrProcurementExceptionEscalationPendingContainsFromHandoff(
        overdueExceptionKey,
      )
      await expect(
        panel.getByTestId(`procurement-exception-escalation-pending-${overdueExceptionKey}`),
      ).toBeVisible({ timeout: 15_000 })
      await expect(
        panel.getByTestId(`procurement-exception-escalation-pending-${overdueExceptionKey}`),
      ).toContainText(overdueExceptionKey)
    }

    await expect(panel.getByRole('heading', { name: 'Recent runs' })).toBeVisible()
    const runsEmpty = panel.getByTestId('procurement-exception-escalation-runs-empty')
    const runsList = panel.getByTestId('procurement-exception-escalation-runs-list')
    await expect(runsEmpty.or(runsList)).toBeVisible({ timeout: 15_000 })

    await expect(panel.getByRole('heading', { name: 'Recent escalation events' })).toBeVisible()
    const eventsEmpty = panel.getByTestId('procurement-exception-escalation-events-empty')
    const eventsList = panel.getByTestId('procurement-exception-escalation-events-list')
    await expect(eventsEmpty.or(eventsList)).toBeVisible({ timeout: 15_000 })

    if (initialEnabled) {
      await enabled.check()
    } else {
      await enabled.uncheck()
    }
    await cooldownInput.fill(initialCooldown)
    await maxEscalationsInput.fill(initialMaxEscalations)
    if (initialNotify) {
      await notify.check()
    } else {
      await notify.uncheck()
    }
    await panel.getByTestId('procurement-exception-escalation-save').click()
    await expect(enabled).toHaveJSProperty('checked', initialEnabled)
    await expect(cooldownInput).toHaveValue(initialCooldown)
    await expect(maxEscalationsInput).toHaveValue(initialMaxEscalations)
    await expect(notify).toHaveJSProperty('checked', initialNotify)
  })
})
