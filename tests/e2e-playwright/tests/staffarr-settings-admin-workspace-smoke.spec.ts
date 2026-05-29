import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('StaffArr settings admin workspace @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('staffarr'))) {
      testInfo.skip(true, 'StaffArr frontend (5175) is unreachable.')
    }
  })

  test('settings admin workspace loads all six product-admin panels with save controls', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'staffarr')

    await page.goto(new URL('/admin', page.url()).toString())

    const workspace = page.getByTestId('staffarr-settings-admin-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const exportDeliveryPanel = workspace.getByTestId('person-export-delivery-settings-panel')
    await expect(exportDeliveryPanel).toBeVisible()
    await expect(
      exportDeliveryPanel.getByRole('heading', { name: 'Person export scheduled delivery' }),
    ).toBeVisible()
    await expect(exportDeliveryPanel.getByTestId('person-export-delivery-save')).toBeVisible()
    await expect(exportDeliveryPanel.getByText('Loading pending preview…')).not.toBeVisible({
      timeout: 15_000,
    })
    const exportDeliveryPendingReady =
      (await exportDeliveryPanel.getByTestId('person-export-delivery-pending-empty').count()) > 0 ||
      (await exportDeliveryPanel.getByTestId('person-export-delivery-pending-list').count()) > 0
    expect(exportDeliveryPendingReady).toBeTruthy()
    await expect(exportDeliveryPanel.getByText('Loading delivery runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const exportDeliveryRunsReady =
      (await exportDeliveryPanel.getByTestId('person-export-delivery-runs-empty').count()) > 0 ||
      (await exportDeliveryPanel.getByTestId('person-export-delivery-runs-list').count()) > 0
    expect(exportDeliveryRunsReady).toBeTruthy()
    await expect(exportDeliveryPanel.getByText('Loading delivery notifications…')).not.toBeVisible({
      timeout: 15_000,
    })
    const exportDeliveryNotificationsReady =
      (await exportDeliveryPanel.getByTestId('person-export-delivery-notifications-empty').count()) > 0 ||
      (await exportDeliveryPanel.getByTestId('person-export-delivery-notifications-list').count()) > 0
    expect(exportDeliveryNotificationsReady).toBeTruthy()

    const certificationPanel = workspace.getByTestId('certification-expiration-settings-panel')
    await certificationPanel.scrollIntoViewIfNeeded()
    await expect(certificationPanel).toBeVisible()
    await expect(
      certificationPanel.getByRole('heading', { name: 'Certification expiration worker' }),
    ).toBeVisible()
    await expect(certificationPanel.getByTestId('certification-expiration-save')).toBeVisible()
    await expect(certificationPanel.getByText('Loading pending preview…')).not.toBeVisible({
      timeout: 15_000,
    })
    const certificationPendingReady =
      (await certificationPanel.getByTestId('certification-expiration-pending-empty').count()) > 0 ||
      (await certificationPanel.getByTestId('certification-expiration-pending-list').count()) > 0
    expect(certificationPendingReady).toBeTruthy()
    await expect(certificationPanel.getByText('Loading worker runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const certificationRunsReady =
      (await certificationPanel.getByTestId('certification-expiration-runs-empty').count()) > 0 ||
      (await certificationPanel.getByTestId('certification-expiration-runs-list').count()) > 0
    expect(certificationRunsReady).toBeTruthy()

    const readinessPanel = workspace.getByTestId('readiness-rollup-settings-panel')
    await readinessPanel.scrollIntoViewIfNeeded()
    await expect(readinessPanel).toBeVisible()
    await expect(readinessPanel.getByRole('heading', { name: 'Readiness rollup worker' })).toBeVisible()
    await expect(readinessPanel.getByTestId('readiness-rollup-save')).toBeVisible()
    await expect(readinessPanel.getByText('Loading pending preview…')).not.toBeVisible({ timeout: 15_000 })
    await expect(readinessPanel.getByText('Loading worker runs…')).not.toBeVisible({ timeout: 15_000 })

    const permissionPanel = workspace.getByTestId('permission-projection-settings-panel')
    await permissionPanel.scrollIntoViewIfNeeded()
    await expect(permissionPanel).toBeVisible()
    await expect(
      permissionPanel.getByRole('heading', { name: 'Permission projection worker' }),
    ).toBeVisible()
    await expect(permissionPanel.getByTestId('permission-projection-save')).toBeVisible()
    await expect(permissionPanel.getByText('Loading pending preview…')).not.toBeVisible({ timeout: 15_000 })
    await expect(permissionPanel.getByText('Loading worker runs…')).not.toBeVisible({ timeout: 15_000 })

    const historyPanel = workspace.getByTestId('personnel-history-rollup-settings-panel')
    await historyPanel.scrollIntoViewIfNeeded()
    await expect(historyPanel).toBeVisible()
    await expect(
      historyPanel.getByRole('heading', { name: 'Personnel history rollup worker' }),
    ).toBeVisible()
    await expect(historyPanel.getByTestId('personnel-history-rollup-save')).toBeVisible()
    await expect(historyPanel.getByText('Loading pending preview…')).not.toBeVisible({ timeout: 15_000 })
    await expect(historyPanel.getByText('Loading worker runs…')).not.toBeVisible({ timeout: 15_000 })

    const auditGenerationPanel = workspace.getByTestId('audit-package-generation-settings-panel')
    await auditGenerationPanel.scrollIntoViewIfNeeded()
    await expect(auditGenerationPanel).toBeVisible()
    await expect(
      auditGenerationPanel.getByRole('heading', { name: 'Audit package generation worker' }),
    ).toBeVisible()
    await expect(auditGenerationPanel.getByTestId('audit-package-generation-save')).toBeVisible()
    await expect(auditGenerationPanel.getByText('Loading pending preview…')).not.toBeVisible({
      timeout: 15_000,
    })
    await expect(auditGenerationPanel.getByText('Loading worker runs…')).not.toBeVisible({
      timeout: 15_000,
    })

    await expect(page.getByTestId('staffarr-audit-export-panel')).toBeVisible()
  })
})
