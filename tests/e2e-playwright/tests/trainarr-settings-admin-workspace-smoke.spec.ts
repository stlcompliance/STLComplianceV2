import { test, expect } from '@playwright/test'

import { launchProductHandoffFromSuite } from '../support/handoffJourney.js'
import {
  isHandoffFrontendReachable,
  isLiveModeEnabled,
  isLiveStackReachable,
  signInFromSuite,
} from '../support/liveProbe.js'

test.describe('TrainArr settings admin workspace @requires-live', () => {
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
    if (!(await isHandoffFrontendReachable('trainarr'))) {
      testInfo.skip(true, 'TrainArr frontend (5176) is unreachable.')
    }
  })

  test('settings admin workspace loads all ten product-admin panels with save controls', async ({
    page,
  }) => {
    await signInFromSuite(page)
    await launchProductHandoffFromSuite(page, 'trainarr')

    await page.goto(new URL('/settings', page.url()).toString())

    const workspace = page.getByTestId('trainarr-settings-admin-workspace')
    await expect(workspace).toBeVisible({ timeout: 15_000 })

    const integrationPanel = workspace.getByTestId('integration-settings-panel')
    await expect(integrationPanel).toBeVisible()
    await expect(
      integrationPanel.getByRole('heading', { name: 'Cross-product integrations' }),
    ).toBeVisible()
    await expect(integrationPanel.getByTestId('integration-settings-save')).toBeVisible()
    await expect(integrationPanel.getByText('Probing integrations…')).not.toBeVisible({
      timeout: 15_000,
    })
    await expect(integrationPanel.getByTestId('integration-probes-list')).toBeVisible()

    const notificationPanel = workspace.getByTestId('notification-settings-panel')
    await notificationPanel.scrollIntoViewIfNeeded()
    await expect(notificationPanel).toBeVisible()
    await expect(
      notificationPanel.getByRole('heading', { name: 'Training notifications' }),
    ).toBeVisible()
    await expect(notificationPanel.getByTestId('notification-settings-save')).toBeVisible()
    await expect(notificationPanel.getByText('Loading dispatch history…')).not.toBeVisible({
      timeout: 15_000,
    })
    const notificationDispatchesReady =
      (await notificationPanel.getByTestId('notification-dispatches-empty').count()) > 0 ||
      (await notificationPanel.getByTestId('notification-dispatches-list').count()) > 0
    expect(notificationDispatchesReady).toBeTruthy()

    const reminderEscalationPanel = workspace.getByTestId('assignment-reminder-escalation-settings-panel')
    await reminderEscalationPanel.scrollIntoViewIfNeeded()
    await expect(reminderEscalationPanel).toBeVisible()
    await expect(
      reminderEscalationPanel.getByRole('heading', {
        name: 'Assignment due reminders & escalations',
      }),
    ).toBeVisible()
    await expect(reminderEscalationPanel.getByTestId('due-reminder-save')).toBeVisible()
    await expect(reminderEscalationPanel.getByTestId('assignment-escalation-save')).toBeVisible()

    const recertificationPanel = workspace.getByTestId('recertification-settings-panel')
    await recertificationPanel.scrollIntoViewIfNeeded()
    await expect(recertificationPanel).toBeVisible()
    await expect(
      recertificationPanel.getByRole('heading', { name: 'Recertification assignments' }),
    ).toBeVisible()
    await expect(recertificationPanel.getByTestId('recertification-save')).toBeVisible()
    await expect(recertificationPanel.getByText('Loading assignment runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const recertificationRunsReady =
      (await recertificationPanel.getByTestId('recertification-runs-empty').count()) > 0 ||
      (await recertificationPanel.getByTestId('recertification-runs-list').count()) > 0
    expect(recertificationRunsReady).toBeTruthy()

    const recalculationPanel = workspace.getByTestId('qualification-recalculation-settings-panel')
    await recalculationPanel.scrollIntoViewIfNeeded()
    await expect(recalculationPanel).toBeVisible()
    await expect(
      recalculationPanel.getByRole('heading', { name: 'Qualification recalculation' }),
    ).toBeVisible()
    await expect(recalculationPanel.getByTestId('qualification-recalculation-save')).toBeVisible()
    await expect(recalculationPanel.getByText('Loading recalculation states…')).not.toBeVisible({
      timeout: 15_000,
    })
    await expect(recalculationPanel.getByText('Loading recalculation runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const recalculationStatesReady =
      (await recalculationPanel.getByTestId('qualification-recalculation-states-empty').count()) >
        0 ||
      (await recalculationPanel.getByTestId('qualification-recalculation-states-list').count()) > 0
    const recalculationRunsReady =
      (await recalculationPanel.getByTestId('qualification-recalculation-runs-empty').count()) > 0 ||
      (await recalculationPanel.getByTestId('qualification-recalculation-runs-list').count()) > 0
    expect(recalculationStatesReady).toBeTruthy()
    expect(recalculationRunsReady).toBeTruthy()

    const rulePackImpactPanel = workspace.getByTestId('rule-pack-impact-settings-panel')
    await rulePackImpactPanel.scrollIntoViewIfNeeded()
    await expect(rulePackImpactPanel).toBeVisible()
    await expect(
      rulePackImpactPanel.getByRole('heading', { name: 'Rule pack impact scanning' }),
    ).toBeVisible()
    await expect(rulePackImpactPanel.getByTestId('rule-pack-impact-save')).toBeVisible()
    await expect(rulePackImpactPanel.getByText('Loading impact states…')).not.toBeVisible({
      timeout: 15_000,
    })
    await expect(rulePackImpactPanel.getByText('Loading impact runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const impactStatesReady =
      (await rulePackImpactPanel.getByTestId('rule-pack-impact-states-empty').count()) > 0 ||
      (await rulePackImpactPanel.getByTestId('rule-pack-impact-states-list').count()) > 0
    const impactRunsReady =
      (await rulePackImpactPanel.getByTestId('rule-pack-impact-runs-empty').count()) > 0 ||
      (await rulePackImpactPanel.getByTestId('rule-pack-impact-runs-list').count()) > 0
    expect(impactStatesReady).toBeTruthy()
    expect(impactRunsReady).toBeTruthy()

    const evidenceRetentionPanel = workspace.getByTestId('evidence-retention-settings-panel')
    await evidenceRetentionPanel.scrollIntoViewIfNeeded()
    await expect(evidenceRetentionPanel).toBeVisible()
    await expect(
      evidenceRetentionPanel.getByRole('heading', { name: 'Training evidence retention' }),
    ).toBeVisible()
    await expect(evidenceRetentionPanel.getByTestId('evidence-retention-save')).toBeVisible()
    await expect(evidenceRetentionPanel.getByText('Loading retention runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const retentionRunsReady =
      (await evidenceRetentionPanel.getByTestId('evidence-retention-runs-empty').count()) > 0 ||
      (await evidenceRetentionPanel.getByTestId('evidence-retention-runs-list').count()) > 0
    expect(retentionRunsReady).toBeTruthy()

    const orphanReferencePanel = workspace.getByTestId('orphan-reference-settings-panel')
    await orphanReferencePanel.scrollIntoViewIfNeeded()
    await expect(orphanReferencePanel).toBeVisible()
    await expect(
      orphanReferencePanel.getByRole('heading', { name: 'Cross-product orphan references' }),
    ).toBeVisible()
    await expect(orphanReferencePanel.getByTestId('orphan-reference-save')).toBeVisible()
    await expect(orphanReferencePanel.getByText('Loading orphan findings…')).not.toBeVisible({
      timeout: 15_000,
    })
    await expect(orphanReferencePanel.getByText('Loading scan runs…')).not.toBeVisible({
      timeout: 15_000,
    })
    const orphanFindingsReady =
      (await orphanReferencePanel.getByTestId('orphan-reference-findings-empty').count()) > 0 ||
      (await orphanReferencePanel.getByTestId('orphan-reference-findings-list').count()) > 0
    const orphanRunsReady =
      (await orphanReferencePanel.getByTestId('orphan-reference-runs-empty').count()) > 0 ||
      (await orphanReferencePanel.getByTestId('orphan-reference-runs-list').count()) > 0
    expect(orphanFindingsReady).toBeTruthy()
    expect(orphanRunsReady).toBeTruthy()

    const staffarrPublicationPanel = workspace.getByTestId('staffarr-publication-settings-panel')
    await staffarrPublicationPanel.scrollIntoViewIfNeeded()
    await expect(staffarrPublicationPanel).toBeVisible()
    await expect(
      staffarrPublicationPanel.getByRole('heading', { name: 'StaffArr publication retry' }),
    ).toBeVisible()
    await expect(staffarrPublicationPanel.getByTestId('staffarr-publication-save')).toBeVisible()
    await expect(staffarrPublicationPanel.getByText('Loading deliveries…')).not.toBeVisible({
      timeout: 15_000,
    })
    const publicationDeliveriesReady =
      (await staffarrPublicationPanel.getByTestId('staffarr-publication-deliveries-empty').count()) >
        0 ||
      (await staffarrPublicationPanel.getByTestId('staffarr-publication-deliveries-list').count()) > 0
    expect(publicationDeliveriesReady).toBeTruthy()

    const eventProcessingPanel = workspace.getByTestId('event-processing-settings-panel')
    await eventProcessingPanel.scrollIntoViewIfNeeded()
    await expect(eventProcessingPanel).toBeVisible()
    await expect(
      eventProcessingPanel.getByRole('heading', { name: 'Training event processing' }),
    ).toBeVisible()
    await expect(eventProcessingPanel.getByTestId('event-processing-save')).toBeVisible()
    await expect(eventProcessingPanel.getByText('Loading events…')).not.toBeVisible({
      timeout: 15_000,
    })
    const eventProcessingEventsReady =
      (await eventProcessingPanel.getByTestId('event-processing-events-empty').count()) > 0 ||
      (await eventProcessingPanel.getByTestId('event-processing-events-list').count()) > 0
    expect(eventProcessingEventsReady).toBeTruthy()
  })
})
