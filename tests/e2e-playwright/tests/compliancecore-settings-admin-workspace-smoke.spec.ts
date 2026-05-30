import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('Compliance Core settings admin workspace @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('compliancecore'))) {
      testInfo.skip(true, 'Compliance Core frontend (5177) is unreachable.')
    }
  })

  test('admin workspace loads all nine product-admin panels with save/evaluate controls', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'compliancecore')

    await page.goto(new URL('/admin', page.url()).toString())

    const workspace = page.getByTestId('compliancecore-settings-admin-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const orchestrationPanel = workspace.getByTestId(
      'compliancecore-audit-delivery-orchestration-panel',
    )
    await expect(orchestrationPanel).toBeVisible()
    await expect(
      orchestrationPanel.getByRole('heading', { name: 'Audit delivery orchestration' }),
    ).toBeVisible()
    await expect(orchestrationPanel.getByText('Loading orchestration status')).not.toBeVisible({
      timeout: 15_000,
    })
    await expect(orchestrationPanel.getByTestId('compliancecore-orchestration-scheduled-eval')).toBeVisible()
    await expect(orchestrationPanel.getByTestId('compliancecore-orchestration-m12-batch')).toBeVisible()
    await expect(orchestrationPanel.getByTestId('compliancecore-orchestration-audit-jobs')).toBeVisible()

    const m12Panel = workspace.getByTestId('compliancecore-m12-analytics-worker-settings-panel')
    await m12Panel.scrollIntoViewIfNeeded()
    await expect(m12Panel).toBeVisible()
    await expect(m12Panel.getByRole('heading', { name: 'M12 analytics worker' })).toBeVisible()
    await expect(m12Panel.getByTestId('compliancecore-m12-worker-save')).toBeVisible()

    const readinessPanel = workspace.getByTestId('readiness-forecast-panel')
    await readinessPanel.scrollIntoViewIfNeeded()
    await expect(readinessPanel).toBeVisible()
    await expect(readinessPanel.getByRole('heading', { name: 'Readiness forecasting' })).toBeVisible()
    await expect(readinessPanel.getByTestId('readiness-forecast-evaluate')).toBeVisible()
    const readinessListReady =
      (await readinessPanel.getByTestId('readiness-forecast-list-empty').count()) > 0 ||
      (await readinessPanel.getByTestId('readiness-forecast-list').count()) > 0
    expect(readinessListReady).toBeTruthy()

    const controlPanel = workspace.getByTestId('control-effectiveness-panel')
    await controlPanel.scrollIntoViewIfNeeded()
    await expect(controlPanel).toBeVisible()
    await expect(controlPanel.getByRole('heading', { name: 'Control effectiveness' })).toBeVisible()
    await expect(controlPanel.getByTestId('control-effectiveness-evaluate')).toBeVisible()
    const controlListReady =
      (await controlPanel.getByTestId('control-effectiveness-list-empty').count()) > 0 ||
      (await controlPanel.getByTestId('control-effectiveness-list').count()) > 0
    expect(controlListReady).toBeTruthy()

    const missingEvidencePanel = workspace.getByTestId('missing-evidence-warnings-panel')
    await missingEvidencePanel.scrollIntoViewIfNeeded()
    await expect(missingEvidencePanel).toBeVisible()
    await expect(
      missingEvidencePanel.getByRole('heading', { name: 'Missing evidence warnings' }),
    ).toBeVisible()
    await expect(missingEvidencePanel.getByTestId('missing-evidence-evaluate')).toBeVisible()
    const missingEvidenceListReady =
      (await missingEvidencePanel.getByTestId('missing-evidence-list-empty').count()) > 0 ||
      (await missingEvidencePanel.getByTestId('missing-evidence-list').count()) > 0
    expect(missingEvidenceListReady).toBeTruthy()

    const riskPanel = workspace.getByTestId('risk-scoring-panel')
    await riskPanel.scrollIntoViewIfNeeded()
    await expect(riskPanel).toBeVisible()
    await expect(riskPanel.getByRole('heading', { name: 'Risk scoring' })).toBeVisible()
    await expect(riskPanel.getByTestId('risk-scoring-evaluate')).toBeVisible()
    const riskListReady =
      (await riskPanel.getByTestId('risk-scoring-list-empty').count()) > 0 ||
      (await riskPanel.getByTestId('risk-scoring-list').count()) > 0
    expect(riskListReady).toBeTruthy()

    const ruleChangePanel = workspace.getByTestId('rule-change-monitoring-panel')
    await ruleChangePanel.scrollIntoViewIfNeeded()
    await expect(ruleChangePanel).toBeVisible()
    await expect(ruleChangePanel.getByRole('heading', { name: 'Rule change monitoring' })).toBeVisible()
    const ruleChangeEventsReady =
      (await ruleChangePanel.getByTestId('rule-change-events-empty').count()) > 0 ||
      (await ruleChangePanel.getByTestId('rule-change-events-list').count()) > 0
    expect(ruleChangeEventsReady).toBeTruthy()

    const sourceIngestionPanel = workspace.getByTestId('source-ingestion-panel')
    await sourceIngestionPanel.scrollIntoViewIfNeeded()
    await expect(sourceIngestionPanel).toBeVisible()
    await expect(sourceIngestionPanel.getByRole('heading', { name: 'Source ingestion' })).toBeVisible()
    await expect(sourceIngestionPanel.getByTestId('source-ingestion-validate')).toBeVisible()
    await expect(sourceIngestionPanel.getByTestId('source-ingestion-commit')).toBeVisible()
    const sourceBatchesReady =
      (await sourceIngestionPanel.getByTestId('source-ingestion-batches-empty').count()) > 0 ||
      (await sourceIngestionPanel.getByTestId('source-ingestion-batches-list').count()) > 0
    expect(sourceBatchesReady).toBeTruthy()

    const csvPanel = workspace.getByTestId('csv-import-export-panel')
    await csvPanel.scrollIntoViewIfNeeded()
    await expect(csvPanel).toBeVisible()
    await expect(csvPanel.getByRole('heading', { name: '10-CSV import / export' })).toBeVisible()
    await expect(csvPanel.getByTestId('csv-import-export-manifest')).toBeVisible()
    await expect(csvPanel.getByTestId('csv-import-export-download')).toBeVisible()
  })
})
