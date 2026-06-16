import { demoCredentials } from './liveProbe.js'
import { handoffProductFrontends } from './productFrontends.js'

const journeySubjectPersonId = '22222222-2222-2222-2222-222222222201'

function nexarrApiUrl(): string {
  return process.env.E2E_NEXARR_URL ?? 'http://localhost:5101'
}

function trainarrApiUrl(): string {
  return process.env.E2E_TRAINARR_API_URL ?? 'http://localhost:5103'
}

function trainarrFrontendUrl(): string {
  return productFrontendUrl('trainarr')
}

function maintainarrApiUrl(): string {
  return process.env.E2E_MAINTAINARR_API_URL ?? 'http://localhost:5104'
}

function routarrApiUrl(): string {
  return process.env.E2E_ROUTARR_API_URL ?? 'http://localhost:5105'
}

function loadarrApiUrl(): string {
  return process.env.E2E_LOADARR_API_URL ?? 'http://localhost:5108'
}

function recordarrApiUrl(): string {
  return process.env.E2E_RECORDARR_API_URL ?? 'http://localhost:5110'
}

function supplyarrApiUrl(): string {
  return process.env.E2E_SUPPLYARR_API_URL ?? 'http://localhost:5106'
}

function ordarrApiUrl(): string {
  return process.env.E2E_ORDARR_API_URL ?? 'http://localhost:5112'
}

function staffarrApiUrl(): string {
  return process.env.E2E_STAFFARR_API_URL ?? 'http://localhost:5102'
}

function compliancecoreApiUrl(): string {
  return process.env.E2E_COMPLIANCECORE_API_URL ?? 'http://localhost:5107'
}

function productFrontendUrl(productKey: string): string {
  const frontend = handoffProductFrontends.find((p) => p.productKey === productKey)
  if (!frontend) {
    throw new Error(`Unknown product frontend key: ${productKey}`)
  }
  return frontend.baseUrl
}

export type TrainArrJourneyFixture = {
  trainingAssignmentId: string
}

export type TrainArrQualificationCompletionJourneyFixture = {
  trainingDefinitionId: string
  assignmentId: string
}

export type MaintainArrFieldInboxFixture = {
  workOrderId: string
}

export type MaintainArrPartsDemandJourneyFixture = {
  workOrderId: string
  workOrderNumber: string
  partNumber: string
}

export type RoutArrFieldInboxFixture = {
  tripId: string
}

export type SupplyArrFieldInboxFixture = {
  receivingReceiptId: string
}

async function readJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new Error(`HTTP ${response.status}: ${body}`)
  }
  return (await response.json()) as T
}

export async function loginNexArr(): Promise<string> {
  const response = await fetch(`${nexarrApiUrl()}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email: demoCredentials.email,
      password: demoCredentials.password,
      tenantId: demoCredentials.tenantId,
    }),
  })
  const payload = await readJson<{ accessToken: string }>(response)
  return payload.accessToken
}

export async function createHandoff(
  accessToken: string,
  productKey: string,
  callbackUrl: string,
): Promise<string> {
  const response = await fetch(`${nexarrApiUrl()}/api/v1/launch/handoff`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ productKey, callbackUrl }),
  })
  const payload = await readJson<{ handoffCode: string }>(response)
  return payload.handoffCode
}

export async function redeemTrainArrHandoff(handoffCode: string): Promise<string> {
  return redeemProductHandoff('trainarr', handoffCode)
}

const productApiUrls: Record<string, () => string> = {
  staffarr: staffarrApiUrl,
  trainarr: trainarrApiUrl,
  maintainarr: maintainarrApiUrl,
  routarr: routarrApiUrl,
  loadarr: loadarrApiUrl,
  recordarr: recordarrApiUrl,
  supplyarr: supplyarrApiUrl,
  ordarr: ordarrApiUrl,
  compliancecore: compliancecoreApiUrl,
}

async function redeemProductHandoff(productKey: string, handoffCode: string): Promise<string> {
  const resolveApiUrl = productApiUrls[productKey]
  if (!resolveApiUrl) {
    throw new Error(`No API URL configured for product ${productKey}`)
  }
  const apiUrl = resolveApiUrl()

  const response = await fetch(`${apiUrl}/api/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  const payload = await readJson<{ accessToken: string }>(response)
  return payload.accessToken
}

async function redeemHandoffForProduct(productKey: string): Promise<string> {
  const nexarrToken = await loginNexArr()
  const handoffCode = await createHandoff(
    nexarrToken,
    productKey,
    `${productFrontendUrl(productKey)}/launch`,
  )
  return redeemProductHandoff(productKey, handoffCode)
}

type TrainArrJourneySeedResponse = {
  staffarrPersonId: string
  trainingDefinitionId: string
  trainingAssignmentId: string
}

export async function seedTrainArrJourney(trainarrAccessToken: string): Promise<TrainArrJourneySeedResponse> {
  const response = await fetch(`${trainarrApiUrl()}/api/load-test-journey/seed`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${trainarrAccessToken}` },
  })
  return readJson<TrainArrJourneySeedResponse>(response)
}

async function createActiveTrainingAssignment(
  trainarrAccessToken: string,
  trainingDefinitionId: string,
): Promise<string> {
  const response = await fetch(`${trainarrApiUrl()}/api/training-assignments`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${trainarrAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      staffarrPersonId: journeySubjectPersonId,
      trainingDefinitionId,
      staffarrIncidentRemediationId: null,
      assignmentReason: 'manual',
      dueAt: null,
    }),
  })
  const payload = await readJson<{ assignmentId: string }>(response)
  return payload.assignmentId
}

/** Ensures an active TrainArr assignment exists for fieldcompanion field-inbox deep-link smokes. */

export type FieldCompanionOfflineActionSyncedItem = {
  idempotencyKey: string
  actionKind: string
  taskKey: string
  productKey: string
  syncedAt: string
}

export async function listFieldCompanionOfflineActions(
  FieldCompanionAccessToken: string,
  limit = 10,
): Promise<{ items: FieldCompanionOfflineActionSyncedItem[] }> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${nexarrApiUrl()}/api/fieldcompanion/offline-actions?${search}`, {
    headers: {
      Authorization: `Bearer ${FieldCompanionAccessToken}`,
    },
  })
  return readJson<{ items: FieldCompanionOfflineActionSyncedItem[] }>(response)
}

const platformAuditGenerateScope = 'nexarr.platform_audit_packages.generate'
const routarrAttachmentRetentionPurgeScope = 'routarr.attachments.retention.purge'
const routarrTripCompletionRollupScope = 'routarr.trips.completion.rollup'
const routarrDispatchNotificationScope = 'routarr.notifications.dispatch'

export type RoutArrAttachmentRetentionSettings = {
  isEnabled: boolean
  retentionDaysAfterTripClose: number
}

export async function upsertRoutArrAttachmentRetentionSettings(
  routarrAccessToken: string,
  settings: RoutArrAttachmentRetentionSettings,
): Promise<RoutArrAttachmentRetentionSettings> {
  const response = await fetch(`${routarrApiUrl()}/api/attachment-retention-settings`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${routarrAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(settings),
  })
  return readJson<RoutArrAttachmentRetentionSettings>(response)
}

export async function processRoutArrAttachmentRetentionBatch(
  serviceToken: string,
  batchSize = 5,
): Promise<void> {
  const response = await fetch(
    `${routarrApiUrl()}/api/internal/attachment-retention/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        batchSize,
      }),
    },
  )
  await readJson(response)
}

export async function issueRoutArrAttachmentRetentionWorkerToken(
  adminAccessToken: string,
): Promise<string> {
  return issueSharedWorkerServiceToken(
    adminAccessToken,
    ['routarr'],
    routarrAttachmentRetentionPurgeScope,
  )
}

export type RoutArrTripCompletionRollupSettings = {
  isEnabled: boolean
  stalenessHours: number
}

export async function upsertRoutArrTripCompletionRollupSettings(
  routarrAccessToken: string,
  settings: RoutArrTripCompletionRollupSettings,
): Promise<RoutArrTripCompletionRollupSettings> {
  const response = await fetch(`${routarrApiUrl()}/api/trip-completion-rollup-settings`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${routarrAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(settings),
  })
  return readJson<RoutArrTripCompletionRollupSettings>(response)
}

export async function processRoutArrTripCompletionRollupBatch(
  serviceToken: string,
  batchSize = 5,
): Promise<void> {
  const response = await fetch(
    `${routarrApiUrl()}/api/internal/trip-completion-rollups/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        batchSize,
        stalenessHours: null,
      }),
    },
  )
  await readJson(response)
}

export async function issueRoutArrTripCompletionRollupWorkerToken(
  adminAccessToken: string,
): Promise<string> {
  return issueSharedWorkerServiceToken(
    adminAccessToken,
    ['routarr'],
    routarrTripCompletionRollupScope,
  )
}

export type RoutArrDispatchNotificationSettings = {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnTripAssigned: boolean
  notifyOnTripDispatched: boolean
  notifyOnTripInProgress: boolean
  notifyOnTripCompleted: boolean
  notifyOnTripCancelled: boolean
}

/** W289 settings-only webhook persistence smoke (read-only example.com sink). */
export const routArrDispatchNotificationWebhookPersistencePrimaryUrl =
  'https://hooks.example.com/routarr-e2e-w289'

export const routArrDispatchNotificationWebhookPersistenceAlternateUrl =
  'https://hooks.example.com/routarr-e2e-w289-alt'

/** W290 settings-only webhook validation smoke. */
export const routArrDispatchNotificationWebhookValidationInvalidUrl = 'not-a-valid-url'

export const routArrDispatchNotificationWebhookValidationValidUrl =
  'https://hooks.example.com/routarr-e2e-w290'

export async function getRoutArrDispatchNotificationSettings(
  routarrAccessToken: string,
): Promise<RoutArrDispatchNotificationSettings> {
  const response = await fetch(`${routarrApiUrl()}/api/notification-settings`, {
    headers: { Authorization: `Bearer ${routarrAccessToken}` },
  })
  return readJson<RoutArrDispatchNotificationSettings>(response)
}

/** Confirms tenant notification webhook URL persisted after UI save + reload (W289). */
export async function assertRoutArrDispatchNotificationWebhookUrlPersisted(
  expectedWebhookUrl: string,
): Promise<void> {
  const token = await redeemHandoffForProduct('routarr')
  const settings = await getRoutArrDispatchNotificationSettings(token)
  const actual = settings.notificationWebhookUrl ?? ''
  if (actual !== expectedWebhookUrl) {
    throw new Error(
      `Expected RoutArr notification webhook URL "${expectedWebhookUrl}", got "${actual}".`,
    )
  }
}

/** Confirms RoutArr notification settings PUT is rejected (W290). */
export async function assertRoutArrDispatchNotificationSettingsUpsertRejected(
  settings: RoutArrDispatchNotificationSettings,
  expectedStatus = 400,
): Promise<void> {
  const token = await redeemHandoffForProduct('routarr')
  const response = await fetch(`${routarrApiUrl()}/api/notification-settings`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(settings),
  })

  if (response.status !== expectedStatus) {
    const body = await response.text()
    throw new Error(
      `Expected RoutArr notification settings upsert status ${expectedStatus}, got ${response.status}: ${body}`,
    )
  }
}

/** Confirms persisted RoutArr notification settings match expected fields (W291). */
export async function assertRoutArrDispatchNotificationSettingsMatch(
  expected: Partial<RoutArrDispatchNotificationSettings>,
): Promise<void> {
  const token = await redeemHandoffForProduct('routarr')
  const actual = await getRoutArrDispatchNotificationSettings(token)

  if (expected.isEnabled !== undefined && actual.isEnabled !== expected.isEnabled) {
    throw new Error(
      `Expected RoutArr notification isEnabled ${expected.isEnabled}, got ${actual.isEnabled}.`,
    )
  }

  if (
    expected.notificationWebhookUrl !== undefined &&
    (actual.notificationWebhookUrl ?? null) !== expected.notificationWebhookUrl
  ) {
    throw new Error(
      `Expected RoutArr notification webhook "${expected.notificationWebhookUrl ?? ''}", got "${actual.notificationWebhookUrl ?? ''}".`,
    )
  }
}

/** W292 settings-only re-enable preserves prior webhook smoke. */
export const routArrDispatchNotificationReEnablePreserveWebhookUrl =
  'https://hooks.example.com/routarr-e2e-w292'

/** Seeds enabled notification settings with a known webhook URL for W292. */
export async function seedRoutArrDispatchNotificationSettingsWithSavedWebhook(
  webhookUrl = routArrDispatchNotificationReEnablePreserveWebhookUrl,
): Promise<void> {
  const token = await redeemHandoffForProduct('routarr')
  await upsertRoutArrDispatchNotificationSettings(token, {
    isEnabled: true,
    notificationWebhookUrl: webhookUrl,
    notifyOnTripAssigned: true,
    notifyOnTripDispatched: true,
    notifyOnTripInProgress: true,
    notifyOnTripCompleted: true,
    notifyOnTripCancelled: true,
  })
}

/** W293 settings-only disable-and-save preserves webhook in API smoke. */
export const routArrDispatchNotificationDisableSavePreserveWebhookUrl =
  'https://hooks.example.com/routarr-e2e-w293'

/** Seeds enabled notification settings with a known webhook URL for W293. */
export async function seedRoutArrDispatchNotificationSettingsForDisableSavePreserve(
  webhookUrl = routArrDispatchNotificationDisableSavePreserveWebhookUrl,
): Promise<void> {
  await seedRoutArrDispatchNotificationSettingsWithSavedWebhook(webhookUrl)
}

/** W298 settings-only explicit webhook clear on disable smoke. */
export const routArrDispatchNotificationExplicitClearWebhookUrl =
  'https://hooks.example.com/routarr-e2e-w298'

/** Seeds enabled notification settings with a known webhook URL for W298. */
export async function seedRoutArrDispatchNotificationSettingsForExplicitClear(
  webhookUrl = routArrDispatchNotificationExplicitClearWebhookUrl,
): Promise<void> {
  await seedRoutArrDispatchNotificationSettingsWithSavedWebhook(webhookUrl)
}

/** W299 settings-only preserve vs explicit-clear contrast smoke. */
export const routArrDispatchNotificationDisableContrastWebhookUrl =
  'https://hooks.example.com/routarr-e2e-w299'

/** Seeds enabled notification settings with a known webhook URL for W299. */
export async function seedRoutArrDispatchNotificationSettingsForDisableContrast(
  webhookUrl = routArrDispatchNotificationDisableContrastWebhookUrl,
): Promise<void> {
  await seedRoutArrDispatchNotificationSettingsWithSavedWebhook(webhookUrl)
}

/** W300 settings-only re-enable after explicit clear empty webhook smoke. */
export const routArrDispatchNotificationReEnableAfterExplicitClearOriginalWebhookUrl =
  'https://hooks.example.com/routarr-e2e-w300-original'

/** New webhook URL entered after re-enable when prior URL was explicitly cleared. */
export const routArrDispatchNotificationReEnableAfterExplicitClearNewWebhookUrl =
  'https://hooks.example.com/routarr-e2e-w300-new'

/** W305 settings-only re-enable new webhook reload persistence smoke (W300 follow-up). */
export const routArrDispatchNotificationReEnableNewWebhookReloadPersistenceOriginalWebhookUrl =
  routArrDispatchNotificationReEnableAfterExplicitClearOriginalWebhookUrl

/** New webhook saved after explicit clear; must persist across reloads and toggle off/on. */
export const routArrDispatchNotificationReEnableNewWebhookReloadPersistenceNewWebhookUrl =
  'https://hooks.example.com/routarr-e2e-w305-new'

/** W307 settings-only disable-save then re-enable reload persistence smoke (W293/W305 follow-up). */
export const routArrDispatchNotificationDisableSaveReEnableReloadPersistenceWebhookUrl =
  'https://hooks.example.com/routarr-e2e-w307'

/** Seeds enabled notification settings with a known webhook URL for W307. */
export async function seedRoutArrDispatchNotificationSettingsForDisableSaveReEnableReloadPersistence(
  webhookUrl = routArrDispatchNotificationDisableSaveReEnableReloadPersistenceWebhookUrl,
): Promise<void> {
  await seedRoutArrDispatchNotificationSettingsForDisableSavePreserve(webhookUrl)
}

/**
 * Seeds enabled settings with a known webhook, then explicit-clear disable (W298)
 * so re-enable starts from an empty webhook in API/UI.
 */
export async function seedRoutArrDispatchNotificationSettingsDisabledAfterExplicitClear(
  originalWebhookUrl = routArrDispatchNotificationReEnableAfterExplicitClearOriginalWebhookUrl,
): Promise<void> {
  await seedRoutArrDispatchNotificationSettingsWithSavedWebhook(originalWebhookUrl)
  await assertRoutArrDispatchNotificationSettingsExplicitClearOnDisable()
}

/** Asserts disabled settings with explicit clear intent remove the saved webhook (W298). */
export async function assertRoutArrDispatchNotificationSettingsExplicitClearOnDisable(): Promise<void> {
  const token = await redeemHandoffForProduct('routarr')
  const response = await fetch(`${routarrApiUrl()}/api/notification-settings`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      isEnabled: false,
      notificationWebhookUrl: null,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
      clearNotificationWebhookOnDisable: true,
    }),
  })

  if (!response.ok) {
    const body = await response.text()
    throw new Error(
      `Expected RoutArr explicit clear-on-disable upsert to succeed, got ${response.status}: ${body}`,
    )
  }

  const payload = (await response.json()) as RoutArrDispatchNotificationSettings
  if (payload.isEnabled) {
    throw new Error('Expected RoutArr notification settings to be disabled after explicit clear upsert.')
  }
  if (payload.notificationWebhookUrl !== null) {
    throw new Error(
      `Expected webhook to be cleared after explicit disable intent, got ${payload.notificationWebhookUrl ?? 'undefined'}.`,
    )
  }

  await assertRoutArrDispatchNotificationSettingsMatch({
    isEnabled: false,
    notificationWebhookUrl: null,
  })
}

/** Confirms disabled settings with empty webhook are accepted (W291). */
export async function assertRoutArrDispatchNotificationSettingsUpsertAcceptedWhenDisabled(): Promise<void> {
  const token = await redeemHandoffForProduct('routarr')
  const response = await fetch(`${routarrApiUrl()}/api/notification-settings`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      isEnabled: false,
      notificationWebhookUrl: null,
      notifyOnTripAssigned: true,
      notifyOnTripDispatched: true,
      notifyOnTripInProgress: true,
      notifyOnTripCompleted: true,
      notifyOnTripCancelled: true,
    } satisfies RoutArrDispatchNotificationSettings),
  })

  if (!response.ok) {
    const body = await response.text()
    throw new Error(
      `Expected RoutArr disabled notification settings upsert to succeed, got ${response.status}: ${body}`,
    )
  }
}

export async function upsertRoutArrDispatchNotificationSettings(
  routarrAccessToken: string,
  settings: RoutArrDispatchNotificationSettings,
): Promise<RoutArrDispatchNotificationSettings> {
  const response = await fetch(`${routarrApiUrl()}/api/notification-settings`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${routarrAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(settings),
  })
  return readJson<RoutArrDispatchNotificationSettings>(response)
}

export async function processRoutArrDispatchNotificationBatch(
  serviceToken: string,
  batchSize = 5,
): Promise<void> {
  const response = await fetch(
    `${routarrApiUrl()}/api/internal/dispatch-notifications/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        batchSize,
      }),
    },
  )
  await readJson(response)
}

export async function issueRoutArrDispatchNotificationWorkerToken(
  adminAccessToken: string,
): Promise<string> {
  return issueSharedWorkerServiceToken(
    adminAccessToken,
    ['routarr'],
    routarrDispatchNotificationScope,
  )
}

export type RoutArrDispatchNotificationDispatchItem = {
  notificationId: string
  eventKind: string
  dispatchStatus: string
  tripId: string
  webhookHost: string | null
  httpStatusCode: number | null
  errorMessage: string | null
}

export type RoutArrDispatchNotificationJourneyFixture = {
  tripId: string
  expectedEventKind: string
}

export async function listRoutArrDispatchNotificationDispatches(
  routarrAccessToken: string,
  limit = 10,
): Promise<{ items: RoutArrDispatchNotificationDispatchItem[] }> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(
    `${routarrApiUrl()}/api/notification-settings/dispatches?${search}`,
    {
      headers: { Authorization: `Bearer ${routarrAccessToken}` },
    },
  )
  return readJson<{ items: RoutArrDispatchNotificationDispatchItem[] }>(response)
}

/**
 * Enables dispatch notifications, creates a trip, assigns a driver, and transitions to
 * dispatched so a pending outbox row is enqueued (W127/W280 Playwright journey smoke).
 */
export async function ensureRoutArrDispatchNotificationJourneyFixture(): Promise<RoutArrDispatchNotificationJourneyFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedEventKind = 'trip_dispatched'

  await upsertRoutArrDispatchNotificationSettings(token, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w280',
    notifyOnTripAssigned: false,
    notifyOnTripDispatched: true,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: false,
  })

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E notification dispatch ${suffix}`,
        description: 'W280 Playwright notification dispatch journey smoke',
        vehicleRefKey: 'VEH-E2E-NOTIFY',
        scheduledStartAt: new Date(now + 60 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 5 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  const dispatches = await listRoutArrDispatchNotificationDispatches(token, 20)
  const matching = dispatches.items.find(
    (item) => item.tripId === trip.tripId && item.eventKind === expectedEventKind,
  )
  if (!matching) {
    throw new Error(
      `Expected pending ${expectedEventKind} dispatch for trip ${trip.tripId} after status change.`,
    )
  }

  return { tripId: trip.tripId, expectedEventKind }
}

/**
 * Enables trip-assigned dispatch notifications, creates a trip, assigns a driver only
 * (no status transition) so a pending trip_assigned outbox row is enqueued (W127/W281).
 */
export async function ensureRoutArrDispatchNotificationTripAssignedJourneyFixture(): Promise<RoutArrDispatchNotificationJourneyFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedEventKind = 'trip_assigned'

  await upsertRoutArrDispatchNotificationSettings(token, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w281',
    notifyOnTripAssigned: true,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: false,
  })

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E notification trip assigned ${suffix}`,
        description: 'W281 Playwright trip-assigned notification dispatch journey smoke',
        vehicleRefKey: 'VEH-E2E-NOTIFY-ASSIGNED',
        scheduledStartAt: new Date(now + 60 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 5 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  const dispatches = await listRoutArrDispatchNotificationDispatches(token, 20)
  const matching = dispatches.items.find(
    (item) => item.tripId === trip.tripId && item.eventKind === expectedEventKind,
  )
  if (!matching) {
    throw new Error(
      `Expected pending ${expectedEventKind} dispatch for trip ${trip.tripId} after assign-driver.`,
    )
  }

  return { tripId: trip.tripId, expectedEventKind }
}

/**
 * Enables trip-in-progress dispatch notifications, creates a trip, assigns a driver,
 * transitions through dispatched, then to in_progress so a pending trip_in_progress
 * outbox row is enqueued (W127/W282).
 */
export async function ensureRoutArrDispatchNotificationTripInProgressJourneyFixture(): Promise<RoutArrDispatchNotificationJourneyFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedEventKind = 'trip_in_progress'

  await upsertRoutArrDispatchNotificationSettings(token, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w282',
    notifyOnTripAssigned: false,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: true,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: false,
  })

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E notification trip in progress ${suffix}`,
        description: 'W282 Playwright trip-in-progress notification dispatch journey smoke',
        vehicleRefKey: 'VEH-E2E-NOTIFY-IN-PROGRESS',
        scheduledStartAt: new Date(now + 60 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 5 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'in_progress' }),
    }),
  )

  const dispatches = await listRoutArrDispatchNotificationDispatches(token, 20)
  const matching = dispatches.items.find(
    (item) => item.tripId === trip.tripId && item.eventKind === expectedEventKind,
  )
  if (!matching) {
    throw new Error(
      `Expected pending ${expectedEventKind} dispatch for trip ${trip.tripId} after in_progress status change.`,
    )
  }

  return { tripId: trip.tripId, expectedEventKind }
}

/**
 * Enables trip-completed dispatch notifications, creates a trip, assigns a driver,
 * transitions through dispatched and in_progress, then to completed so a pending
 * trip_completed outbox row is enqueued (W127/W283).
 */
export async function ensureRoutArrDispatchNotificationTripCompletedJourneyFixture(): Promise<RoutArrDispatchNotificationJourneyFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedEventKind = 'trip_completed'

  await upsertRoutArrDispatchNotificationSettings(token, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w283',
    notifyOnTripAssigned: false,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: true,
    notifyOnTripCancelled: false,
  })

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E notification trip completed ${suffix}`,
        description: 'W283 Playwright trip-completed notification dispatch journey smoke',
        vehicleRefKey: 'VEH-E2E-NOTIFY-COMPLETED',
        scheduledStartAt: new Date(now + 60 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 5 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'in_progress' }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'completed' }),
    }),
  )

  const dispatches = await listRoutArrDispatchNotificationDispatches(token, 20)
  const matching = dispatches.items.find(
    (item) => item.tripId === trip.tripId && item.eventKind === expectedEventKind,
  )
  if (!matching) {
    throw new Error(
      `Expected pending ${expectedEventKind} dispatch for trip ${trip.tripId} after completed status change.`,
    )
  }

  return { tripId: trip.tripId, expectedEventKind }
}

/**
 * Enables trip-cancelled dispatch notifications, creates a trip, assigns a driver,
 * then transitions to cancelled so a pending trip_cancelled outbox row is enqueued
 * (W127/W284).
 */
export async function ensureRoutArrDispatchNotificationTripCancelledJourneyFixture(): Promise<RoutArrDispatchNotificationJourneyFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedEventKind = 'trip_cancelled'

  await upsertRoutArrDispatchNotificationSettings(token, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w284',
    notifyOnTripAssigned: false,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: true,
  })

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E notification trip cancelled ${suffix}`,
        description: 'W284 Playwright trip-cancelled notification dispatch journey smoke',
        vehicleRefKey: 'VEH-E2E-NOTIFY-CANCELLED',
        scheduledStartAt: new Date(now + 60 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 5 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'cancelled' }),
    }),
  )

  const dispatches = await listRoutArrDispatchNotificationDispatches(token, 20)
  const matching = dispatches.items.find(
    (item) => item.tripId === trip.tripId && item.eventKind === expectedEventKind,
  )
  if (!matching) {
    throw new Error(
      `Expected pending ${expectedEventKind} dispatch for trip ${trip.tripId} after cancelled status change.`,
    )
  }

  return { tripId: trip.tripId, expectedEventKind }
}

export type RoutArrDispatchNotificationMultiEventJourneyFixture = {
  completedPathTripId: string
  completedPathExpectedEventKinds: string[]
  completedPathAbsentEventKinds: string[]
  cancelledBranchTripId: string
  cancelledBranchExpectedEventKind: string
}

async function createRoutArrNotificationE2eTrip(
  token: string,
  suffix: number,
  titlePrefix: string,
  description: string,
  vehicleRefKey: string,
): Promise<{ tripId: string }> {
  const now = Date.now()
  return readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `${titlePrefix} ${suffix}`,
        description,
        vehicleRefKey,
        scheduledStartAt: new Date(now + 60 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 5 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )
}

async function assignRoutArrNotificationE2eTripDriver(
  token: string,
  tripId: string,
): Promise<void> {
  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )
}

async function setRoutArrNotificationE2eTripStatus(
  token: string,
  tripId: string,
  dispatchStatus: string,
): Promise<void> {
  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus }),
    }),
  )
}

function assertRoutArrDispatchNotificationEventKinds(
  items: RoutArrDispatchNotificationDispatchItem[],
  tripId: string,
  expectedEventKinds: string[],
  absentEventKinds: string[] = [],
): void {
  const tripItems = items.filter((item) => item.tripId === tripId)
  for (const eventKind of expectedEventKinds) {
    const matching = tripItems.find((item) => item.eventKind === eventKind)
    if (!matching) {
      throw new Error(
        `Expected ${eventKind} dispatch for trip ${tripId}; found ${tripItems.map((item) => item.eventKind).join(', ') || 'none'}.`,
      )
    }
  }
  for (const eventKind of absentEventKinds) {
    const matching = tripItems.find((item) => item.eventKind === eventKind)
    if (matching) {
      throw new Error(
        `Expected no ${eventKind} dispatch for trip ${tripId} when toggle disabled.`,
      )
    }
  }
}

/**
 * Multi-event notification journey: completed path (assign → dispatched → in_progress →
 * completed with selective toggles) plus cancelled branch (cancelled-only toggle).
 * Verifies only enabled event kinds enqueue outbox rows (W127/W285).
 */
export async function ensureRoutArrDispatchNotificationMultiEventJourneyFixture(): Promise<RoutArrDispatchNotificationMultiEventJourneyFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const completedPathExpectedEventKinds = [
    'trip_assigned',
    'trip_dispatched',
    'trip_in_progress',
    'trip_completed',
  ]
  const completedPathAbsentEventKinds = ['trip_cancelled']

  await upsertRoutArrDispatchNotificationSettings(token, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w285-completed',
    notifyOnTripAssigned: true,
    notifyOnTripDispatched: true,
    notifyOnTripInProgress: true,
    notifyOnTripCompleted: true,
    notifyOnTripCancelled: false,
  })

  const completedTrip = await createRoutArrNotificationE2eTrip(
    token,
    suffix,
    'E2E notification multi completed',
    'W285 Playwright multi-event notification completed path',
    'VEH-E2E-NOTIFY-MULTI-COMPLETED',
  )

  await assignRoutArrNotificationE2eTripDriver(token, completedTrip.tripId)
  await setRoutArrNotificationE2eTripStatus(token, completedTrip.tripId, 'dispatched')
  await setRoutArrNotificationE2eTripStatus(token, completedTrip.tripId, 'in_progress')
  await setRoutArrNotificationE2eTripStatus(token, completedTrip.tripId, 'completed')

  let dispatches = await listRoutArrDispatchNotificationDispatches(token, 50)
  assertRoutArrDispatchNotificationEventKinds(
    dispatches.items,
    completedTrip.tripId,
    completedPathExpectedEventKinds,
    completedPathAbsentEventKinds,
  )

  await upsertRoutArrDispatchNotificationSettings(token, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w285-cancelled',
    notifyOnTripAssigned: false,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: true,
  })

  const cancelledTrip = await createRoutArrNotificationE2eTrip(
    token,
    suffix + 1,
    'E2E notification multi cancelled',
    'W285 Playwright multi-event notification cancelled branch',
    'VEH-E2E-NOTIFY-MULTI-CANCELLED',
  )

  await assignRoutArrNotificationE2eTripDriver(token, cancelledTrip.tripId)
  await setRoutArrNotificationE2eTripStatus(token, cancelledTrip.tripId, 'cancelled')

  dispatches = await listRoutArrDispatchNotificationDispatches(token, 50)
  assertRoutArrDispatchNotificationEventKinds(
    dispatches.items,
    cancelledTrip.tripId,
    ['trip_cancelled'],
    ['trip_assigned', 'trip_dispatched', 'trip_in_progress', 'trip_completed'],
  )

  return {
    completedPathTripId: completedTrip.tripId,
    completedPathExpectedEventKinds,
    completedPathAbsentEventKinds,
    cancelledBranchTripId: cancelledTrip.tripId,
    cancelledBranchExpectedEventKind: 'trip_cancelled',
  }
}

export const routArrDispatchNotificationUiNegativeSmokeDisabledEventKinds = [
  'trip_assigned',
  'trip_in_progress',
  'trip_completed',
  'trip_cancelled',
] as const

export const routArrDispatchNotificationUiNegativeSmokeEnabledEventKind = 'trip_dispatched'

/** First trip after UI save with all completion-path event toggles enabled (W287). */
export const routArrDispatchNotificationUiSecondLifecycleFirstTripExpectedEventKinds = [
  'trip_assigned',
  'trip_dispatched',
  'trip_in_progress',
  'trip_completed',
] as const

export const routArrDispatchNotificationUiSecondLifecycleSecondTripEnabledEventKind =
  'trip_dispatched'

/** Second trip after post-save selective disable (W287; mirrors W286 negative smoke). */
export const routArrDispatchNotificationUiSecondLifecycleSecondTripDisabledEventKinds =
  routArrDispatchNotificationUiNegativeSmokeDisabledEventKinds

/** Completed-path event kinds after UI save with all toggles enabled (W288). */
export const routArrDispatchNotificationAllEventsProcessBatchCompletedPathExpectedEventKinds = [
  'trip_assigned',
  'trip_dispatched',
  'trip_in_progress',
  'trip_completed',
] as const

export const routArrDispatchNotificationAllEventsProcessBatchCancelledBranchExpectedEventKind =
  'trip_cancelled'

/** All five RoutArr dispatch notification event kinds (W288 process-batch journey). */
export const routArrDispatchNotificationAllFiveEventKinds = [
  ...routArrDispatchNotificationAllEventsProcessBatchCompletedPathExpectedEventKinds,
  routArrDispatchNotificationAllEventsProcessBatchCancelledBranchExpectedEventKind,
] as const

/**
 * Creates a trip and runs assign → dispatched → in_progress → completed without
 * changing notification settings (W286 UI negative smoke — settings saved via live panel first).
 */
export async function createAndRunRoutArrDispatchNotificationFullLifecycle(): Promise<string> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const trip = await createRoutArrNotificationE2eTrip(
    token,
    suffix,
    'E2E notification UI negative',
    'W286 Playwright notification settings UI negative smoke',
    'VEH-E2E-NOTIFY-UI-NEG',
  )
  await assignRoutArrNotificationE2eTripDriver(token, trip.tripId)
  await setRoutArrNotificationE2eTripStatus(token, trip.tripId, 'dispatched')
  await setRoutArrNotificationE2eTripStatus(token, trip.tripId, 'in_progress')
  await setRoutArrNotificationE2eTripStatus(token, trip.tripId, 'completed')
  return trip.tripId
}

/**
 * Creates a trip, assigns a driver, and cancels without changing notification settings
 * (W288 all-events process-batch journey — settings saved via live panel first).
 */
export async function createAndRunRoutArrDispatchNotificationCancelledBranch(): Promise<string> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const trip = await createRoutArrNotificationE2eTrip(
    token,
    suffix,
    'E2E notification UI cancelled branch',
    'W288 Playwright notification all-events process-batch journey smoke',
    'VEH-E2E-NOTIFY-UI-CANCELLED',
  )
  await assignRoutArrNotificationE2eTripDriver(token, trip.tripId)
  await setRoutArrNotificationE2eTripStatus(token, trip.tripId, 'cancelled')
  return trip.tripId
}

/**
 * API assertion that only enabled event kinds enqueued for a trip (W286 negative smoke).
 */
export async function assertRoutArrDispatchNotificationDispatchesForTrip(
  tripId: string,
  expectedEventKinds: string[],
  absentEventKinds: string[] = [],
): Promise<void> {
  const token = await redeemHandoffForProduct('routarr')
  const dispatches = await listRoutArrDispatchNotificationDispatches(token, 50)
  assertRoutArrDispatchNotificationEventKinds(
    dispatches.items,
    tripId,
    expectedEventKinds,
    absentEventKinds,
  )
}

/**
 * API assertion that dispatch rows for a trip are no longer pending (W288 process-batch journey).
 */
export async function assertRoutArrDispatchNotificationDispatchesProcessedForTrip(
  tripId: string,
  expectedEventKinds: string[],
): Promise<void> {
  const token = await redeemHandoffForProduct('routarr')
  const dispatches = await listRoutArrDispatchNotificationDispatches(token, 50)
  const tripItems = dispatches.items.filter((item) => item.tripId === tripId)
  for (const eventKind of expectedEventKinds) {
    const matching = tripItems.find((item) => item.eventKind === eventKind)
    if (!matching) {
      throw new Error(
        `Expected processed ${eventKind} dispatch for trip ${tripId}; found ${tripItems.map((item) => item.eventKind).join(', ') || 'none'}.`,
      )
    }
    if (matching.dispatchStatus.toLowerCase() === 'pending') {
      throw new Error(
        `Expected ${eventKind} dispatch for trip ${tripId} to be processed; still pending.`,
      )
    }
  }
}

export async function issueSharedWorkerServiceToken(
  adminAccessToken: string,
  allowedProductKeys: string[],
  actionScope: string,
): Promise<string> {
  const clientKey = `shared-worker-e2e-${Date.now()}`
  const registerResponse = await fetch(`${nexarrApiUrl()}/api/service-tokens/clients`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${adminAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      clientKey,
      displayName: 'E2E shared worker',
      sourceProductKey: 'shared-worker',
      allowedProductKeys,
    }),
  })
  const client = await readJson<{ serviceClientId: string }>(registerResponse)

  const issueResponse = await fetch(`${nexarrApiUrl()}/api/service-tokens`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${adminAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      serviceClientId: client.serviceClientId,
      tenantId: demoCredentials.tenantId,
      allowedProductKeys,
      actionScope,
      lifetimeMinutes: 30,
    }),
  })
  const issued = await readJson<{ accessToken: string }>(issueResponse)
  return issued.accessToken
}

export async function issueSharedWorkerNexArrServiceToken(
  adminAccessToken: string,
  actionScope: string = platformAuditGenerateScope,
): Promise<string> {
  return issueSharedWorkerServiceToken(adminAccessToken, ['nexarr'], actionScope)
}

export async function processPlatformAuditPackageGenerationBatch(
  serviceToken: string,
  batchSize = 5,
): Promise<void> {
  const response = await fetch(
    `${nexarrApiUrl()}/api/internal/platform-audit-package-jobs/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        batchSize,
      }),
    },
  )
  await readJson(response)
}

export async function processMaintainArrAuditPackageGenerationBatch(
  serviceToken: string,
  batchSize = 5,
): Promise<void> {
  const response = await fetch(
    `${maintainarrApiUrl()}/api/internal/audit-package-jobs/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        batchSize,
      }),
    },
  )
  await readJson(response)
}

export async function processStaffArrAuditPackageGenerationBatch(
  serviceToken: string,
  batchSize = 5,
): Promise<void> {
  const response = await fetch(
    `${staffarrApiUrl()}/api/internal/audit-package-jobs/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        batchSize,
      }),
    },
  )
  await readJson(response)
}

export async function processTrainArrAuditPackageGenerationBatch(
  serviceToken: string,
  batchSize = 5,
): Promise<void> {
  const response = await fetch(
    `${trainarrApiUrl()}/api/internal/audit-package-jobs/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        batchSize,
      }),
    },
  )
  await readJson(response)
}

export async function processRoutArrAuditPackageGenerationBatch(
  serviceToken: string,
  batchSize = 5,
): Promise<void> {
  const response = await fetch(
    `${routarrApiUrl()}/api/internal/audit-package-jobs/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        batchSize,
      }),
    },
  )
  await readJson(response)
}

export async function processComplianceCoreM12AnalyticsBatch(
  serviceToken: string,
  batchSize = 5,
): Promise<void> {
  const response = await fetch(
    `${compliancecoreApiUrl()}/api/internal/m12-analytics-batches/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        intervalHours: 24,
        batchSize,
      }),
    },
  )
  await readJson(response)
}

export type ComplianceCoreJourneyFixture = {
  rulePackId: string
  rulePackKey: string
}

export async function seedComplianceCoreJourney(
  compliancecoreAccessToken?: string,
): Promise<ComplianceCoreJourneyFixture> {
  const token = compliancecoreAccessToken ?? (await redeemHandoffForProduct('compliancecore'))
  const response = await fetch(`${compliancecoreApiUrl()}/api/load-test-journey/seed`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
  })
  const payload = await readJson<{ rulePackId: string; rulePackKey: string }>(response)
  return { rulePackId: payload.rulePackId, rulePackKey: payload.rulePackKey }
}

const dispatchDriverQualificationGateKey = 'dispatch_driver_qualification'
const driverLicenseFactKey = 'driver_license_valid'
const e2eBlockLicenseSourceKey = 'e2e_w331_dispatch_gate_block_license'
const e2eAllowLicenseSourceKey = 'e2e_w331_dispatch_gate_allow_license'
const e2eWarnUnresolvedFactKey = 'e2e_w332_dispatch_unresolved_fact'
const e2eWarnUnresolvedRuleKey = 'e2e_w332_unresolved_warn_rule'
const e2eWarnResolvedSourceKey = 'e2e_w332_dispatch_unresolved_fact_resolved'

export type ComplianceCoreRoutArrDispatchGateJourneyFixture = {
  tripId: string
  driverPersonId: string
  dispatchGateKey: string
}

async function listComplianceCoreFactDefinitions(
  token: string,
): Promise<Array<{ factDefinitionId: string; factKey: string }>> {
  const response = await fetch(`${compliancecoreApiUrl()}/api/fact-definitions`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  return readJson(response)
}

async function listComplianceCoreFactSources(
  token: string,
  factDefinitionId: string,
): Promise<Array<{ sourceKey: string }>> {
  const url = new URL(`${compliancecoreApiUrl()}/api/fact-sources`)
  url.searchParams.set('factDefinitionId', factDefinitionId)
  const response = await fetch(url.toString(), {
    headers: { Authorization: `Bearer ${token}` },
  })
  return readJson(response)
}

async function ensureComplianceCoreDriverLicenseStaticOverride(
  token: string,
  sourceKey: string,
  booleanValue: boolean,
  priority: number,
): Promise<void> {
  const definitions = await listComplianceCoreFactDefinitions(token)
  const definition = definitions.find((entry) => entry.factKey === driverLicenseFactKey)
  if (!definition) {
    throw new Error(`${driverLicenseFactKey} fact definition was not found after journey seed.`)
  }

  const existing = await listComplianceCoreFactSources(token, definition.factDefinitionId)
  if (existing.some((source) => source.sourceKey === sourceKey)) {
    return
  }

  await readJson(
    await fetch(`${compliancecoreApiUrl()}/api/fact-sources`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        factDefinitionId: definition.factDefinitionId,
        sourceKey,
        sourceType: 'static_config',
        label: `E2E W331 license override (${booleanValue})`,
        description:
          'Cross-product Compliance Core workflow gate → RoutArr dispatch assign Playwright fixture.',
        productKey: null,
        productReference: null,
        configJson: JSON.stringify({ booleanValue }),
        priority,
      }),
    }),
  )
}

async function ensureComplianceCoreDispatchGateUnresolvedFactResolvedOverride(
  token: string,
): Promise<void> {
  await ensureComplianceCoreFactDefinition(
    token,
    e2eWarnUnresolvedFactKey,
    'E2E dispatch gate unresolved fact',
  )

  const definitions = await listComplianceCoreFactDefinitions(token)
  const definition = definitions.find((entry) => entry.factKey === e2eWarnUnresolvedFactKey)
  if (!definition) {
    throw new Error(`${e2eWarnUnresolvedFactKey} fact definition was not found.`)
  }

  const existing = await listComplianceCoreFactSources(token, definition.factDefinitionId)
  if (existing.some((source) => source.sourceKey === e2eWarnResolvedSourceKey)) {
    return
  }

  await readJson(
    await fetch(`${compliancecoreApiUrl()}/api/fact-sources`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        factDefinitionId: definition.factDefinitionId,
        sourceKey: e2eWarnResolvedSourceKey,
        sourceType: 'static_config',
        label: 'E2E W332 unresolved fact resolved override',
        description:
          'Resolves W332 warn fact so block/allow dispatch gate fixtures remain deterministic.',
        productKey: null,
        productReference: null,
        configJson: JSON.stringify({ booleanValue: true }),
        priority: -15,
      }),
    }),
  )
}

async function ensureRoutArrJourneyDriverPersonRef(routarrToken: string): Promise<void> {
  await readJson(
    await fetch(`${routarrApiUrl()}/api/dispatch/driver-refs`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${routarrToken}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        personId: journeySubjectPersonId,
        displayName: 'E2E journey driver',
        mirroredAt: null,
      }),
    }),
  )
}

type ComplianceCoreRulePackContentResponse = {
  hasContent: boolean
  content: {
    schemaVersion: number
    logic: string
    rules: Array<{
      ruleKey: string
      label: string
      type: string
      factKey: string
      expectedValue: boolean
    }>
  } | null
}

async function ensureComplianceCoreFactDefinition(
  token: string,
  factKey: string,
  label: string,
): Promise<void> {
  const definitions = await listComplianceCoreFactDefinitions(token)
  if (definitions.some((entry) => entry.factKey === factKey)) {
    return
  }

  await readJson(
    await fetch(`${compliancecoreApiUrl()}/api/fact-definitions`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        factKey,
        label,
        description: 'E2E W332 unresolved workflow gate fact for RoutArr command-center assign.',
        valueType: 'boolean',
      }),
    }),
  )
}

async function ensureComplianceCoreDispatchGateUnresolvedWarnRulePack(
  token: string,
  rulePackId: string,
): Promise<void> {
  await ensureComplianceCoreFactDefinition(
    token,
    e2eWarnUnresolvedFactKey,
    'E2E dispatch gate unresolved fact',
  )

  const contentResponse = await readJson<ComplianceCoreRulePackContentResponse>(
    await fetch(`${compliancecoreApiUrl()}/api/rule-packs/${rulePackId}/content`, {
      headers: { Authorization: `Bearer ${token}` },
    }),
  )

  if (!contentResponse.hasContent || !contentResponse.content) {
    throw new Error(`Rule pack ${rulePackId} has no content for W332 warn fixture.`)
  }

  if (
    contentResponse.content.rules.some(
      (rule) => rule.factKey === e2eWarnUnresolvedFactKey,
    )
  ) {
    return
  }

  await readJson(
    await fetch(`${compliancecoreApiUrl()}/api/rule-packs/${rulePackId}/content`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        content: {
          schemaVersion: contentResponse.content.schemaVersion,
          logic: contentResponse.content.logic,
          rules: [
            ...contentResponse.content.rules,
            {
              ruleKey: e2eWarnUnresolvedRuleKey,
              label: 'E2E unresolved dispatch gate fact',
              type: 'fact_boolean',
              factKey: e2eWarnUnresolvedFactKey,
              expectedValue: true,
            },
          ],
        },
      }),
    }),
  )
}

export async function checkRoutArrDispatchWorkflowGates(
  routarrToken: string,
  tripId: string,
  driverPersonId: string,
): Promise<{ outcome: string; isBlocking: boolean }> {
  const response = await fetch(`${routarrApiUrl()}/api/dispatch-workflow-gates/check`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${routarrToken}`, 'Content-Type': 'application/json' },
    body: JSON.stringify({
      tripId,
      driverPersonId,
      assignmentKind: 'driver',
    }),
  })
  return readJson(response)
}

/**
 * Seeds Compliance Core dispatch gates with a blocking license fact override, creates an
 * unassigned RoutArr trip, and asserts RoutArr dispatch workflow gate check returns block.
 */
export async function ensureComplianceCoreRoutArrDispatchGateBlockFixture(): Promise<ComplianceCoreRoutArrDispatchGateJourneyFixture> {
  const compliancecoreToken = await redeemHandoffForProduct('compliancecore')
  const journey = await seedComplianceCoreJourney(compliancecoreToken)
  await ensureComplianceCoreDispatchGateUnresolvedWarnRulePack(
    compliancecoreToken,
    journey.rulePackId,
  )
  await ensureComplianceCoreDispatchGateUnresolvedFactResolvedOverride(compliancecoreToken)
  await ensureComplianceCoreDriverLicenseStaticOverride(
    compliancecoreToken,
    e2eBlockLicenseSourceKey,
    false,
    -10,
  )

  const routarrToken = await redeemHandoffForProduct('routarr')
  await ensureRoutArrJourneyDriverPersonRef(routarrToken)
  const suffix = Date.now()
  const now = Date.now()
  const trip = await createUnassignedTrip(routarrToken, {
    title: `E2E CC gate block journey ${suffix}`,
    scheduledStartAt: new Date(now + 2 * 60 * 60 * 1000).toISOString(),
    scheduledEndAt: new Date(now + 6 * 60 * 60 * 1000).toISOString(),
  })

  const gateCheck = await checkRoutArrDispatchWorkflowGates(
    routarrToken,
    trip.tripId,
    journeySubjectPersonId,
  )
  if (gateCheck.outcome !== 'block' || !gateCheck.isBlocking) {
    throw new Error(
      `Expected RoutArr dispatch workflow gate block for trip ${trip.tripId}; got ${gateCheck.outcome}.`,
    )
  }

  return {
    tripId: trip.tripId,
    driverPersonId: journeySubjectPersonId,
    dispatchGateKey: dispatchDriverQualificationGateKey,
  }
}

/**
 * Seeds Compliance Core dispatch gates with an allowing license fact override, creates an
 * unassigned RoutArr trip, and asserts RoutArr dispatch workflow gate check returns allow.
 */
export async function ensureComplianceCoreRoutArrDispatchGateAllowFixture(): Promise<ComplianceCoreRoutArrDispatchGateJourneyFixture> {
  const compliancecoreToken = await redeemHandoffForProduct('compliancecore')
  const journey = await seedComplianceCoreJourney(compliancecoreToken)
  await ensureComplianceCoreDispatchGateUnresolvedWarnRulePack(
    compliancecoreToken,
    journey.rulePackId,
  )
  await ensureComplianceCoreDispatchGateUnresolvedFactResolvedOverride(compliancecoreToken)
  await ensureComplianceCoreDriverLicenseStaticOverride(
    compliancecoreToken,
    e2eAllowLicenseSourceKey,
    true,
    -20,
  )

  const routarrToken = await redeemHandoffForProduct('routarr')
  await ensureRoutArrJourneyDriverPersonRef(routarrToken)
  const suffix = Date.now()
  const now = Date.now()
  const trip = await createUnassignedTrip(routarrToken, {
    title: `E2E CC gate allow journey ${suffix}`,
    scheduledStartAt: new Date(now + 2 * 60 * 60 * 1000).toISOString(),
    scheduledEndAt: new Date(now + 6 * 60 * 60 * 1000).toISOString(),
  })

  const gateCheck = await checkRoutArrDispatchWorkflowGates(
    routarrToken,
    trip.tripId,
    journeySubjectPersonId,
  )
  if (gateCheck.outcome !== 'allow') {
    throw new Error(
      `Expected RoutArr dispatch workflow gate allow for trip ${trip.tripId}; got ${gateCheck.outcome}.`,
    )
  }

  return {
    tripId: trip.tripId,
    driverPersonId: journeySubjectPersonId,
    dispatchGateKey: dispatchDriverQualificationGateKey,
  }
}

const e2eDriverQualificationKey = 'driver_qualification'
const e2eDriverQualificationRulePackKey = 'driver_qualification'
const e2eDriverQualificationDefinitionKey = 'e2e_playwright_driver_qualification'
const e2eDriverQualificationAssignmentReason = 'e2e_playwright_driver_qualification_gate'

export type TrainArrRoutarrQualificationGateJourneyFixture = {
  tripId: string
  driverPersonId: string
  qualificationKey: string
  trainingDefinitionId: string
}

type TrainArrQualificationCheckApiResponse = {
  checkId: string
  outcome: string
  reasonCode: string
  message: string
  qualificationKey: string
}

type TrainArrQualificationIssueSummary = {
  qualificationIssueId: string
  staffarrPersonId: string
  qualificationKey: string
  status: string
}

type TrainArrTrainingDefinitionSummary = {
  trainingDefinitionId: string
  definitionKey: string
  qualificationKey: string
}

async function listTrainArrTrainingDefinitions(
  trainarrToken: string,
): Promise<TrainArrTrainingDefinitionSummary[]> {
  const response = await fetch(`${trainarrApiUrl()}/api/training-definitions`, {
    headers: { Authorization: `Bearer ${trainarrToken}` },
  })
  return readJson(response)
}

async function ensureTrainArrE2eDriverQualificationDefinition(
  trainarrToken: string,
): Promise<TrainArrTrainingDefinitionSummary> {
  const definitions = await listTrainArrTrainingDefinitions(trainarrToken)
  const existing = definitions.find(
    (entry) => entry.definitionKey === e2eDriverQualificationDefinitionKey,
  )
  if (existing) {
    return existing
  }

  const created = await readJson<TrainArrTrainingDefinitionSummary>(
    await fetch(`${trainarrApiUrl()}/api/training-definitions`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${trainarrToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        definitionKey: e2eDriverQualificationDefinitionKey,
        name: 'E2E Playwright driver qualification',
        description:
          'Cross-product TrainArr qualification gate → RoutArr driver eligibility Playwright fixture.',
        qualificationKey: e2eDriverQualificationKey,
        qualificationName: 'Driver qualification',
      }),
    }),
  )
  return created
}

async function runTrainArrQualificationCheckApi(
  trainarrToken: string,
  staffarrPersonId: string,
  trainingDefinitionId: string,
): Promise<TrainArrQualificationCheckApiResponse> {
  const response = await fetch(`${trainarrApiUrl()}/api/qualification-checks`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${trainarrToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      staffarrPersonId,
      qualificationKey: e2eDriverQualificationKey,
      rulePackKey: e2eDriverQualificationRulePackKey,
      trainingDefinitionId,
      context: null,
    }),
  })
  return readJson(response)
}

async function listTrainArrQualificationIssues(
  trainarrToken: string,
  status?: string,
): Promise<TrainArrQualificationIssueSummary[]> {
  const url = new URL(`${trainarrApiUrl()}/api/qualification-issues`)
  if (status) {
    url.searchParams.set('status', status)
  }
  const response = await fetch(url.toString(), {
    headers: { Authorization: `Bearer ${trainarrToken}` },
  })
  return readJson(response)
}

async function ensureTrainArrDriverQualificationIssued(
  trainarrToken: string,
): Promise<{ trainingDefinitionId: string; qualificationIssueId: string }> {
  const definition = await ensureTrainArrE2eDriverQualificationDefinition(trainarrToken)

  const issued = await listTrainArrQualificationIssues(trainarrToken, 'issued')
  const existingIssue = issued.find(
    (entry) =>
      entry.staffarrPersonId === journeySubjectPersonId
      && entry.qualificationKey === e2eDriverQualificationKey,
  )
  if (existingIssue) {
    return {
      trainingDefinitionId: definition.trainingDefinitionId,
      qualificationIssueId: existingIssue.qualificationIssueId,
    }
  }

  const check = await runTrainArrQualificationCheckApi(
    trainarrToken,
    journeySubjectPersonId,
    definition.trainingDefinitionId,
  )

  const createdAssignment = await readJson<{ assignmentId: string }>(
    await fetch(`${trainarrApiUrl()}/api/training-assignments`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${trainarrToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        staffarrPersonId: journeySubjectPersonId,
        trainingDefinitionId: definition.trainingDefinitionId,
        staffarrIncidentRemediationId: null,
        assignmentReason: e2eDriverQualificationAssignmentReason,
        dueAt: null,
        authorizationQualificationCheckId: check.checkId,
      }),
    }),
  )

  await readJson(
    await fetch(
      `${trainarrApiUrl()}/api/training-assignments/${createdAssignment.assignmentId}/complete`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${trainarrToken}` },
      },
    ),
  )

  const issuesAfterComplete = await listTrainArrQualificationIssues(trainarrToken, 'issued')
  const issue = issuesAfterComplete.find(
    (entry) =>
      entry.staffarrPersonId === journeySubjectPersonId
      && entry.qualificationKey === e2eDriverQualificationKey,
  )
  if (!issue) {
    throw new Error(
      `Expected issued ${e2eDriverQualificationKey} qualification for ${journeySubjectPersonId}.`,
    )
  }

  return {
    trainingDefinitionId: definition.trainingDefinitionId,
    qualificationIssueId: issue.qualificationIssueId,
  }
}

async function suspendTrainArrDriverQualificationIssue(
  trainarrToken: string,
  qualificationIssueId: string,
): Promise<void> {
  await readJson(
    await fetch(
      `${trainarrApiUrl()}/api/qualification-issues/${qualificationIssueId}/suspend`,
      {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${trainarrToken}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          reason: 'E2E Playwright TrainArr→RoutArr qualification gate block fixture.',
        }),
      },
    ),
  )
}

export async function checkRoutarrDriverEligibility(
  routarrToken: string,
  driverPersonId: string,
): Promise<{
  outcome: string
  isBlocking: boolean
  trainarrOutcome: string | null
  message: string
}> {
  const response = await fetch(`${routarrApiUrl()}/api/driver-eligibility/check`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${routarrToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ personId: driverPersonId }),
  })
  const payload = await readJson<{
    outcome: string
    isBlocking: boolean
    message: string
    trainArr: { outcome: string } | null
  }>(response)
  return {
    outcome: payload.outcome,
    isBlocking: payload.isBlocking,
    trainarrOutcome: payload.trainArr?.outcome ?? null,
    message: payload.message,
  }
}

export async function ensureTrainArrRoutarrQualificationGateTrip(
  routarrToken: string,
): Promise<{ tripId: string }> {
  await ensureRoutArrJourneyDriverPersonRef(routarrToken)
  const suffix = Date.now()
  const now = Date.now()
  return createUnassignedTrip(routarrToken, {
    title: `E2E TrainArr qualification gate ${suffix}`,
    scheduledStartAt: new Date(now + 2 * 60 * 60 * 1000).toISOString(),
    scheduledEndAt: new Date(now + 6 * 60 * 60 * 1000).toISOString(),
  })
}

/**
 * Creates a RoutArr trip fixture for the TrainArr qualification publication browser journey.
 * The trip is independent from TrainArr qualification state; the browser completion step is
 * responsible for issuing the new qualification grant that unlocks dispatch eligibility.
 */
export async function ensureTrainArrRoutarrQualificationIssuePublicationTripFixture(): Promise<{ tripId: string }> {
  const routarrToken = await redeemHandoffForProduct('routarr')
  return ensureTrainArrRoutarrQualificationGateTrip(routarrToken)
}

/**
 * Creates a live TrainArr assignment that can be completed in the browser to issue a fresh
 * qualification grant for the shared journey driver person.
 */
export async function ensureTrainArrDriverQualificationCompletionFixture(): Promise<TrainArrQualificationCompletionJourneyFixture> {
  const trainarrToken = await redeemHandoffForProduct('trainarr')
  const definition = await ensureTrainArrE2eDriverQualificationDefinition(trainarrToken)
  const check = await runTrainArrQualificationCheckApi(
    trainarrToken,
    journeySubjectPersonId,
    definition.trainingDefinitionId,
  )

  const assignment = await readJson<{ assignmentId: string }>(
    await fetch(`${trainarrApiUrl()}/api/training-assignments`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${trainarrToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        staffarrPersonId: journeySubjectPersonId,
        trainingDefinitionId: definition.trainingDefinitionId,
        staffarrIncidentRemediationId: null,
        assignmentReason: e2eDriverQualificationAssignmentReason,
        dueAt: null,
        authorizationQualificationCheckId: check.checkId,
      }),
    }),
  )

  return {
    trainingDefinitionId: definition.trainingDefinitionId,
    assignmentId: assignment.assignmentId,
  }
}

/**
 * Compliance Core workflow gate allow + suspended TrainArr driver qualification + unassigned trip.
 * RoutArr driver eligibility consumes POST /api/integrations/routarr-qualification-check.
 */
export async function ensureTrainArrRoutarrQualificationGateBlockFixture(): Promise<TrainArrRoutarrQualificationGateJourneyFixture> {
  await ensureComplianceCoreRoutArrDispatchGateAllowFixture()

  const trainarrToken = await redeemHandoffForProduct('trainarr')
  const issued = await ensureTrainArrDriverQualificationIssued(trainarrToken)
  await suspendTrainArrDriverQualificationIssue(trainarrToken, issued.qualificationIssueId)

  const trainarrCheck = await runTrainArrQualificationCheckApi(
    trainarrToken,
    journeySubjectPersonId,
    issued.trainingDefinitionId,
  )
  if (trainarrCheck.outcome !== 'block') {
    throw new Error(
      `Expected TrainArr qualification check block after suspend; got ${trainarrCheck.outcome}.`,
    )
  }

  const routarrToken = await redeemHandoffForProduct('routarr')
  const trip = await ensureTrainArrRoutarrQualificationGateTrip(routarrToken)

  const gateCheck = await checkRoutArrDispatchWorkflowGates(
    routarrToken,
    trip.tripId,
    journeySubjectPersonId,
  )
  if (gateCheck.outcome !== 'allow') {
    throw new Error(
      `Expected Compliance Core workflow gate allow for qualification gate block fixture; got ${gateCheck.outcome}.`,
    )
  }

  const eligibility = await checkRoutarrDriverEligibility(routarrToken, journeySubjectPersonId)
  if (eligibility.outcome !== 'block' || !eligibility.isBlocking) {
    throw new Error(
      `Expected RoutArr driver eligibility block from TrainArr; got ${eligibility.outcome}.`,
    )
  }

  return {
    tripId: trip.tripId,
    driverPersonId: journeySubjectPersonId,
    qualificationKey: e2eDriverQualificationKey,
    trainingDefinitionId: issued.trainingDefinitionId,
  }
}

/**
 * Compliance Core workflow gate allow + issued TrainArr driver qualification + unassigned trip.
 */
export async function ensureTrainArrRoutarrQualificationGateAllowFixture(): Promise<TrainArrRoutarrQualificationGateJourneyFixture> {
  await ensureComplianceCoreRoutArrDispatchGateAllowFixture()

  const trainarrToken = await redeemHandoffForProduct('trainarr')
  const issued = await ensureTrainArrDriverQualificationIssued(trainarrToken)

  const trainarrCheck = await runTrainArrQualificationCheckApi(
    trainarrToken,
    journeySubjectPersonId,
    issued.trainingDefinitionId,
  )
  if (trainarrCheck.outcome !== 'allow') {
    throw new Error(
      `Expected TrainArr qualification check allow after issuance; got ${trainarrCheck.outcome}.`,
    )
  }

  const routarrToken = await redeemHandoffForProduct('routarr')
  const trip = await ensureTrainArrRoutarrQualificationGateTrip(routarrToken)

  const gateCheck = await checkRoutArrDispatchWorkflowGates(
    routarrToken,
    trip.tripId,
    journeySubjectPersonId,
  )
  if (gateCheck.outcome !== 'allow') {
    throw new Error(
      `Expected Compliance Core workflow gate allow for qualification gate allow fixture; got ${gateCheck.outcome}.`,
    )
  }

  const eligibility = await checkRoutarrDriverEligibility(routarrToken, journeySubjectPersonId)
  if (eligibility.outcome !== 'allow' || eligibility.isBlocking) {
    throw new Error(
      `Expected RoutArr driver eligibility allow from TrainArr; got ${eligibility.outcome}.`,
    )
  }

  return {
    tripId: trip.tripId,
    driverPersonId: journeySubjectPersonId,
    qualificationKey: e2eDriverQualificationKey,
    trainingDefinitionId: issued.trainingDefinitionId,
  }
}

/**
 * Seeds Compliance Core dispatch gates with a required but unresolved fact (warn), creates an
 * unassigned RoutArr trip, and asserts RoutArr dispatch workflow gate check returns warn.
 */
export async function ensureComplianceCoreRoutArrDispatchGateWarnFixture(): Promise<ComplianceCoreRoutArrDispatchGateJourneyFixture> {
  const compliancecoreToken = await redeemHandoffForProduct('compliancecore')
  const journey = await seedComplianceCoreJourney(compliancecoreToken)
  await ensureComplianceCoreDispatchGateUnresolvedWarnRulePack(
    compliancecoreToken,
    journey.rulePackId,
  )
  await ensureComplianceCoreDriverLicenseStaticOverride(
    compliancecoreToken,
    e2eAllowLicenseSourceKey,
    true,
    -20,
  )

  const routarrToken = await redeemHandoffForProduct('routarr')
  await ensureRoutArrJourneyDriverPersonRef(routarrToken)
  const suffix = Date.now()
  const now = Date.now()
  const trip = await createUnassignedTrip(routarrToken, {
    title: `E2E CC gate warn journey ${suffix}`,
    scheduledStartAt: new Date(now + 2 * 60 * 60 * 1000).toISOString(),
    scheduledEndAt: new Date(now + 6 * 60 * 60 * 1000).toISOString(),
  })

  const gateCheck = await checkRoutArrDispatchWorkflowGates(
    routarrToken,
    trip.tripId,
    journeySubjectPersonId,
  )
  if (gateCheck.outcome !== 'warn' || gateCheck.isBlocking) {
    throw new Error(
      `Expected RoutArr dispatch workflow gate warn for trip ${trip.tripId}; got ${gateCheck.outcome} (blocking=${gateCheck.isBlocking}).`,
    )
  }

  return {
    tripId: trip.tripId,
    driverPersonId: journeySubjectPersonId,
    dispatchGateKey: dispatchDriverQualificationGateKey,
  }
}

/**
 * W334: Same as block fixture — unassigned trip appears in BulkDispatchPanel active trips.
 */
export async function ensureComplianceCoreRoutArrDispatchGateBulkDispatchBlockFixture(): Promise<ComplianceCoreRoutArrDispatchGateJourneyFixture> {
  return ensureComplianceCoreRoutArrDispatchGateBlockFixture()
}

/**
 * W334: Same as warn fixture — unassigned trip appears in BulkDispatchPanel active trips.
 */
export async function ensureComplianceCoreRoutArrDispatchGateBulkDispatchWarnFixture(): Promise<ComplianceCoreRoutArrDispatchGateJourneyFixture> {
  return ensureComplianceCoreRoutArrDispatchGateWarnFixture()
}

/**
 * W335: Same as block fixture — unassigned trip appears in UnassignedWorkQueuePanel for bulk assign.
 */
export async function ensureComplianceCoreRoutArrDispatchGateUnassignedBulkBlockFixture(): Promise<ComplianceCoreRoutArrDispatchGateJourneyFixture> {
  return ensureComplianceCoreRoutArrDispatchGateBlockFixture()
}

/**
 * W335: Same as warn fixture — unassigned trip appears in UnassignedWorkQueuePanel for bulk assign.
 */
export async function ensureComplianceCoreRoutArrDispatchGateUnassignedBulkWarnFixture(): Promise<ComplianceCoreRoutArrDispatchGateJourneyFixture> {
  return ensureComplianceCoreRoutArrDispatchGateWarnFixture()
}

export type ComplianceCoreRoutArrDispatchGateTripAssignedNotificationFixture =
  ComplianceCoreRoutArrDispatchGateJourneyFixture & {
    expectedEventKind: 'trip_assigned'
  }

/**
 * W336: Compliance Core dispatch gate block fixture plus trip-assigned notification
 * settings enabled. Trip remains unassigned so Playwright can override-assign via UI.
 */
export async function ensureComplianceCoreRoutArrDispatchGateTripAssignedNotificationFixture(): Promise<ComplianceCoreRoutArrDispatchGateTripAssignedNotificationFixture> {
  const fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
  const routarrToken = await redeemHandoffForProduct('routarr')

  await upsertRoutArrDispatchNotificationSettings(routarrToken, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w336',
    notifyOnTripAssigned: true,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: false,
  })

  return { ...fixture, expectedEventKind: 'trip_assigned' }
}

export type ComplianceCoreRoutArrDispatchGateCommandCenterTripAssignedNotificationFixture =
  ComplianceCoreRoutArrDispatchGateTripAssignedNotificationFixture

/**
 * W337: Compliance Core dispatch gate block fixture plus trip-assigned notification
 * settings enabled for command-center drag-and-drop assign path. Trip remains unassigned
 * so Playwright can override-assign via command center UI.
 */
export async function ensureComplianceCoreRoutArrDispatchGateCommandCenterTripAssignedNotificationFixture(): Promise<ComplianceCoreRoutArrDispatchGateCommandCenterTripAssignedNotificationFixture> {
  const fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
  const routarrToken = await redeemHandoffForProduct('routarr')

  await upsertRoutArrDispatchNotificationSettings(routarrToken, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w337',
    notifyOnTripAssigned: true,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: false,
  })

  return { ...fixture, expectedEventKind: 'trip_assigned' }
}

export type ComplianceCoreRoutArrDispatchGateBulkDispatchTripAssignedNotificationFixture =
  ComplianceCoreRoutArrDispatchGateTripAssignedNotificationFixture

/**
 * W338: Compliance Core dispatch gate block fixture plus trip-assigned notification
 * settings enabled for bulk dispatch panel override path. Trip remains unassigned
 * so Playwright can override-assign via bulk dispatch UI.
 */
export async function ensureComplianceCoreRoutArrDispatchGateBulkDispatchTripAssignedNotificationFixture(): Promise<ComplianceCoreRoutArrDispatchGateBulkDispatchTripAssignedNotificationFixture> {
  const fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
  const routarrToken = await redeemHandoffForProduct('routarr')

  await upsertRoutArrDispatchNotificationSettings(routarrToken, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w338',
    notifyOnTripAssigned: true,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: false,
  })

  return { ...fixture, expectedEventKind: 'trip_assigned' }
}

export type ComplianceCoreRoutArrDispatchGateUnassignedBulkAssignTripAssignedNotificationFixture =
  ComplianceCoreRoutArrDispatchGateTripAssignedNotificationFixture

/**
 * W339: Compliance Core dispatch gate block fixture plus trip-assigned notification
 * settings enabled for unassigned work queue bulk assign override path. Trip remains
 * unassigned so Playwright can override-assign via unassigned bulk assign UI.
 */
export async function ensureComplianceCoreRoutArrDispatchGateUnassignedBulkAssignTripAssignedNotificationFixture(): Promise<ComplianceCoreRoutArrDispatchGateUnassignedBulkAssignTripAssignedNotificationFixture> {
  const fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
  const routarrToken = await redeemHandoffForProduct('routarr')

  await upsertRoutArrDispatchNotificationSettings(routarrToken, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w339',
    notifyOnTripAssigned: true,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: false,
  })

  return { ...fixture, expectedEventKind: 'trip_assigned' }
}

export type ComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture =
  ComplianceCoreRoutArrDispatchGateJourneyFixture & {
    expectedEventKind: 'trip_dispatched'
  }

/**
 * W341: Compliance Core dispatch gate block fixture plus trip-dispatched notification
 * settings enabled. Trip remains unassigned so Playwright can override-assign via UI,
 * then dispatch to enqueued trip_dispatched outbox row (not trip_assigned).
 */
export async function ensureComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture(): Promise<ComplianceCoreRoutArrDispatchGateTripDispatchedNotificationFixture> {
  const fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
  const routarrToken = await redeemHandoffForProduct('routarr')

  await upsertRoutArrDispatchNotificationSettings(routarrToken, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w341',
    notifyOnTripAssigned: false,
    notifyOnTripDispatched: true,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: false,
  })

  return { ...fixture, expectedEventKind: 'trip_dispatched' }
}

export type ComplianceCoreRoutArrDispatchGateTripInProgressNotificationFixture =
  ComplianceCoreRoutArrDispatchGateJourneyFixture & {
    expectedEventKind: 'trip_in_progress'
  }

/**
 * W342: Compliance Core dispatch gate block fixture plus trip-in-progress notification
 * settings enabled. Trip remains unassigned so Playwright can override-assign via UI,
 * then dispatch and bulk status change to in_progress for trip_in_progress outbox row.
 */
export async function ensureComplianceCoreRoutArrDispatchGateTripInProgressNotificationFixture(): Promise<ComplianceCoreRoutArrDispatchGateTripInProgressNotificationFixture> {
  const fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
  const routarrToken = await redeemHandoffForProduct('routarr')

  await upsertRoutArrDispatchNotificationSettings(routarrToken, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w342',
    notifyOnTripAssigned: false,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: true,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: false,
  })

  return { ...fixture, expectedEventKind: 'trip_in_progress' }
}

export type ComplianceCoreRoutArrDispatchGateTripCompletedNotificationFixture =
  ComplianceCoreRoutArrDispatchGateJourneyFixture & {
    expectedEventKind: 'trip_completed'
  }

/**
 * W343: Compliance Core dispatch gate block fixture plus trip-completed notification
 * settings enabled. Trip remains unassigned so Playwright can override-assign via UI,
 * then dispatch, bulk in_progress, and bulk completed for trip_completed outbox row.
 */
export async function ensureComplianceCoreRoutArrDispatchGateTripCompletedNotificationFixture(): Promise<ComplianceCoreRoutArrDispatchGateTripCompletedNotificationFixture> {
  const fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
  const routarrToken = await redeemHandoffForProduct('routarr')

  await upsertRoutArrDispatchNotificationSettings(routarrToken, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w343',
    notifyOnTripAssigned: false,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: true,
    notifyOnTripCancelled: false,
  })

  return { ...fixture, expectedEventKind: 'trip_completed' }
}

export type ComplianceCoreRoutArrDispatchGateTripCancelledNotificationFixture =
  ComplianceCoreRoutArrDispatchGateJourneyFixture & {
    expectedEventKind: 'trip_cancelled'
  }

/**
 * W344: Compliance Core dispatch gate block fixture plus trip-cancelled notification
 * settings enabled. Trip remains unassigned so Playwright can override-assign via UI,
 * then dispatch and bulk cancelled status change for trip_cancelled outbox row.
 */
export async function ensureComplianceCoreRoutArrDispatchGateTripCancelledNotificationFixture(): Promise<ComplianceCoreRoutArrDispatchGateTripCancelledNotificationFixture> {
  const fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
  const routarrToken = await redeemHandoffForProduct('routarr')

  await upsertRoutArrDispatchNotificationSettings(routarrToken, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w344',
    notifyOnTripAssigned: false,
    notifyOnTripDispatched: false,
    notifyOnTripInProgress: false,
    notifyOnTripCompleted: false,
    notifyOnTripCancelled: true,
  })

  return { ...fixture, expectedEventKind: 'trip_cancelled' }
}

export type ComplianceCoreRoutArrDispatchGateMultiEventNotificationFixture =
  ComplianceCoreRoutArrDispatchGateJourneyFixture & {
    expectedEventKinds: string[]
    absentEventKinds: string[]
  }

/**
 * W351: Compliance Core dispatch gate block fixture plus all completion-path
 * notification toggles enabled (cancelled off). Trip remains unassigned so
 * Playwright can override-assign via UI, then dispatch, bulk in_progress, and
 * bulk completed for four pending outbox rows on one trip.
 */
export async function ensureComplianceCoreRoutArrDispatchGateMultiEventNotificationFixture(): Promise<ComplianceCoreRoutArrDispatchGateMultiEventNotificationFixture> {
  const fixture = await ensureComplianceCoreRoutArrDispatchGateBlockFixture()
  const routarrToken = await redeemHandoffForProduct('routarr')
  const expectedEventKinds = [
    'trip_assigned',
    'trip_dispatched',
    'trip_in_progress',
    'trip_completed',
  ]
  const absentEventKinds = ['trip_cancelled']

  await upsertRoutArrDispatchNotificationSettings(routarrToken, {
    isEnabled: true,
    notificationWebhookUrl: 'https://hooks.example.com/routarr-e2e-w351',
    notifyOnTripAssigned: true,
    notifyOnTripDispatched: true,
    notifyOnTripInProgress: true,
    notifyOnTripCompleted: true,
    notifyOnTripCancelled: false,
  })

  return { ...fixture, expectedEventKinds, absentEventKinds }
}

export async function ensureTrainArrFieldInboxFixture(): Promise<TrainArrJourneyFixture> {
  const trainarrToken = await redeemHandoffForProduct('trainarr')
  const journey = await seedTrainArrJourney(trainarrToken)
  const trainingAssignmentId = await createActiveTrainingAssignment(
    trainarrToken,
    journey.trainingDefinitionId,
  )
  return { trainingAssignmentId }
}

export async function ensureMaintainArrFieldInboxFixture(): Promise<MaintainArrFieldInboxFixture> {
  const token = await redeemHandoffForProduct('maintainarr')
  const suffix = Date.now()

  const assetClass = await readJson<{ assetClassId: string }>(
    await fetch(`${maintainarrApiUrl()}/api/asset-classes`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        classKey: `e2e-class-${suffix}`,
        name: 'E2E production',
        description: 'E2E asset class',
      }),
    }),
  )

  const assetType = await readJson<{ assetTypeId: string }>(
    await fetch(`${maintainarrApiUrl()}/api/asset-types`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        assetClassId: assetClass.assetClassId,
        typeKey: `e2e-type-${suffix}`,
        name: 'E2E conveyor',
        description: 'E2E',
      }),
    }),
  )

  const asset = await readJson<{ assetId: string }>(
    await fetch(`${maintainarrApiUrl()}/api/assets`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        assetTypeId: assetType.assetTypeId,
        assetTag: `E2E-${suffix}`,
        name: 'E2E conveyor',
        locationLabel: 'Line 1',
        notes: null,
      }),
    }),
  )

  const workOrder = await readJson<{ workOrderId: string }>(
    await fetch(`${maintainarrApiUrl()}/api/work-orders`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        assetId: asset.assetId,
        title: `E2E work order ${suffix}`,
        description: 'Field Companion deep-link smoke',
        priority: 'high',
        assignedTechnicianPersonId: journeySubjectPersonId,
        pmScheduleId: null,
      }),
    }),
  )

  return { workOrderId: workOrder.workOrderId }
}

export async function ensureMaintainArrPartsDemandJourneyFixture(): Promise<MaintainArrPartsDemandJourneyFixture> {
  const supplyarrToken = await redeemHandoffForProduct('supplyarr')
  const maintainarrToken = await redeemHandoffForProduct('maintainarr')
  const suffix = Date.now()
  const partNumber = `E2E-BRK-${suffix}`

  const part = await readJson<{ partId: string }>(
    await fetch(`${supplyarrApiUrl()}/api/parts`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${supplyarrToken}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        partNumber,
        manufacturerPartNumber: null,
        name: 'E2E brake pads',
        description: 'Cross-product parts demand journey',
        categoryKey: 'general',
        unitOfMeasure: 'each',
        manufacturerName: 'Acme',
        manufacturerSku: 'AC-100',
      }),
    }),
  )

  const assetClass = await readJson<{ assetClassId: string }>(
    await fetch(`${maintainarrApiUrl()}/api/asset-classes`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${maintainarrToken}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        classKey: `e2e-demand-class-${suffix}`,
        name: 'E2E demand class',
        description: '',
      }),
    }),
  )

  const assetType = await readJson<{ assetTypeId: string }>(
    await fetch(`${maintainarrApiUrl()}/api/asset-types`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${maintainarrToken}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        assetClassId: assetClass.assetClassId,
        typeKey: `e2e-demand-type-${suffix}`,
        name: 'E2E demand type',
        description: '',
      }),
    }),
  )

  const asset = await readJson<{ assetId: string }>(
    await fetch(`${maintainarrApiUrl()}/api/assets`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${maintainarrToken}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        assetTypeId: assetType.assetTypeId,
        assetTag: `E2E-DEMAND-${suffix}`,
        name: 'E2E demand asset',
        locationLabel: 'Shop',
        notes: null,
      }),
    }),
  )

  const workOrder = await readJson<{ workOrderId: string; workOrderNumber: string }>(
    await fetch(`${maintainarrApiUrl()}/api/work-orders`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${maintainarrToken}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        assetId: asset.assetId,
        title: `E2E parts demand ${suffix}`,
        description: 'Cross-product MaintainArr → SupplyArr journey',
        priority: 'medium',
        assignedTechnicianPersonId: null,
        pmScheduleId: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${maintainarrApiUrl()}/api/work-orders/${workOrder.workOrderId}/parts-demand`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${maintainarrToken}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        supplyarrPartId: part.partId,
        partNumber,
        description: 'E2E brake pads',
        quantityRequested: 2,
        unitOfMeasure: 'each',
        notes: 'Journey smoke',
      }),
    }),
  )

  return {
    workOrderId: workOrder.workOrderId,
    workOrderNumber: workOrder.workOrderNumber,
    partNumber,
  }
}

export async function ensureRoutArrFieldInboxFixture(): Promise<RoutArrFieldInboxFixture> {
  const token = await redeemHandoffForProduct('routarr')

  const seed = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/load-test-journey/seed`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${seed.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${seed.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'assigned' }),
    }),
  )

  return { tripId: seed.tripId }
}

export async function ensureSupplyArrFieldInboxFixture(): Promise<SupplyArrFieldInboxFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()

  const vendor = await readJson<{ partyId: string }>(
    await fetch(`${supplyarrApiUrl()}/api/vendors`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        partyKey: `e2e-vendor-${suffix}`,
        displayName: 'E2E Vendor',
        legalName: 'E2E Vendor LLC',
        contactEmail: null,
        notes: '',
      }),
    }),
  )

  const part = await readJson<{ partId: string }>(
    await fetch(`${supplyarrApiUrl()}/api/parts`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        partKey: `e2e-part-${suffix}`,
        catalogId: null,
        displayName: 'E2E Part',
        description: '',
        categoryKey: 'general',
        unitOfMeasure: 'each',
        manufacturerName: '',
        manufacturerPartNumber: '',
      }),
    }),
  )

  const location = await readJson<{ locationId: string }>(
    await fetch(`${supplyarrApiUrl()}/api/inventory/locations`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        locationKey: `e2e-loc-${suffix}`,
        name: 'E2E Warehouse',
        locationType: 'warehouse',
        addressLine: 'Dock',
      }),
    }),
  )

  const bin = await readJson<{ binId: string }>(
    await fetch(`${supplyarrApiUrl()}/api/inventory/locations/${location.locationId}/bins`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ binKey: `e2e-bin-${suffix}`, name: 'E2E Bin' }),
    }),
  )

  const purchaseRequest = await readJson<{ purchaseRequestId: string }>(
    await fetch(`${supplyarrApiUrl()}/api/purchase-requests`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requestKey: `e2e-pr-${suffix}`,
        title: 'E2E receiving PR',
        notes: '',
        vendorPartyId: vendor.partyId,
        lines: [{ partId: part.partId, quantity: 3, notes: '' }],
      }),
    }),
  )

  await readJson(
    await fetch(
      `${supplyarrApiUrl()}/api/purchase-requests/${purchaseRequest.purchaseRequestId}/submit`,
      { method: 'POST', headers: { Authorization: `Bearer ${token}` } },
    ),
  )
  await readJson(
    await fetch(
      `${supplyarrApiUrl()}/api/purchase-requests/${purchaseRequest.purchaseRequestId}/approve`,
      { method: 'POST', headers: { Authorization: `Bearer ${token}` } },
    ),
  )

  const purchaseOrder = await readJson<{ purchaseOrderId: string }>(
    await fetch(
      `${supplyarrApiUrl()}/api/purchase-orders/from-purchase-request/${purchaseRequest.purchaseRequestId}`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderKey: `e2e-po-${suffix}`, notes: null, requestedDeliveryAt: null }),
      },
    ),
  )

  await readJson(
    await fetch(`${supplyarrApiUrl()}/api/purchase-orders/${purchaseOrder.purchaseOrderId}/approve`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
    }),
  )
  await readJson(
    await fetch(`${supplyarrApiUrl()}/api/purchase-orders/${purchaseOrder.purchaseOrderId}/issue`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
    }),
  )

  const receipt = await readJson<{ receivingReceiptId: string }>(
    await fetch(
      `${supplyarrApiUrl()}/api/receiving/from-purchase-order/${purchaseOrder.purchaseOrderId}`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          receiptKey: `e2e-rcpt-${suffix}`,
          inventoryBinId: bin.binId,
          notes: 'E2E deep link',
        }),
      },
    ),
  )

  return { receivingReceiptId: receipt.receivingReceiptId }
}

const trainarrDemandStatusIngestScope = 'trainarr.demand_status.write'

export type TrainArrMaterialDemandFixture = TrainArrJourneyFixture & {
  procurementStatusSeeded: boolean
}

type TrainArrMaterialDemandLineResponse = {
  demandLineId: string
  status: string
  procurementStatus: string
}

type PublishTrainArrMaterialDemandResponse = {
  publicationId: string
  demandRefId: string
  purchaseRequestId: string | null
}

export async function issueSupplyarrToTrainarrDemandStatusToken(
  adminAccessToken: string,
): Promise<string> {
  const clientKey = `supplyarr-trainarr-e2e-${Date.now()}`
  const registerResponse = await fetch(`${nexarrApiUrl()}/api/service-tokens/clients`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${adminAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      clientKey,
      displayName: 'E2E SupplyArr TrainArr demand status',
      sourceProductKey: 'supplyarr',
      allowedProductKeys: ['trainarr'],
    }),
  })
  const client = await readJson<{ serviceClientId: string }>(registerResponse)

  const issueResponse = await fetch(`${nexarrApiUrl()}/api/service-tokens`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${adminAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      serviceClientId: client.serviceClientId,
      tenantId: demoCredentials.tenantId,
      allowedProductKeys: ['trainarr'],
      actionScope: trainarrDemandStatusIngestScope,
      lifetimeMinutes: 30,
    }),
  })
  const issued = await readJson<{ accessToken: string }>(issueResponse)
  return issued.accessToken
}

export async function createTrainArrMaterialDemandLine(
  trainarrAccessToken: string,
  assignmentId: string,
  payload: {
    partNumber: string
    quantityRequested?: number
    supplyarrPartId?: string | null
  },
): Promise<TrainArrMaterialDemandLineResponse> {
  const response = await fetch(
    `${trainarrApiUrl()}/api/training-assignments/${assignmentId}/material-demand`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${trainarrAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        supplyarrPartId: payload.supplyarrPartId ?? null,
        partNumber: payload.partNumber,
        description: 'E2E material demand',
        quantityRequested: payload.quantityRequested ?? 1,
        unitOfMeasure: 'each',
        notes: null,
      }),
    },
  )
  return readJson<TrainArrMaterialDemandLineResponse>(response)
}

export async function publishTrainArrMaterialDemand(
  trainarrAccessToken: string,
  assignmentId: string,
  createPurchaseRequestDraft = false,
): Promise<PublishTrainArrMaterialDemandResponse> {
  const response = await fetch(
    `${trainarrApiUrl()}/api/training-assignments/${assignmentId}/material-demand/publish`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${trainarrAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ createPurchaseRequestDraft }),
    },
  )
  return readJson<PublishTrainArrMaterialDemandResponse>(response)
}

export async function ingestTrainarrSupplyarrDemandStatus(
  serviceToken: string,
  payload: {
    trainarrPublicationId: string
    supplyarrDemandRefId: string
    supplyarrCallbackPublicationId: string
    eventType?: string
    procurementStatus?: string
    message?: string
  },
): Promise<void> {
  const response = await fetch(`${trainarrApiUrl()}/api/integrations/supplyarr-demand-status`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${serviceToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      tenantId: demoCredentials.tenantId,
      trainarrPublicationId: payload.trainarrPublicationId,
      supplyarrDemandRefId: payload.supplyarrDemandRefId,
      supplyarrCallbackPublicationId: payload.supplyarrCallbackPublicationId,
      eventType: payload.eventType ?? 'pr_submitted',
      procurementStatus: payload.procurementStatus ?? 'pr_submitted',
      supplyarrPurchaseRequestId: null,
      supplyarrPurchaseOrderId: null,
      supplyarrReceivingReceiptId: null,
      quantityReceivedDelta: null,
      message: payload.message ?? 'E2E procurement status callback',
      occurredAt: new Date().toISOString(),
    }),
  })
  await readJson(response)
}

/**
 * Active assignment plus optional published demand line and procurement status (W233 / W234).
 * Publish + callback require TrainArr and SupplyArr APIs; failures leave procurementStatusSeeded false.
 */
export async function ensureTrainArrMaterialDemandFixture(): Promise<TrainArrMaterialDemandFixture> {
  const trainarrToken = await redeemHandoffForProduct('trainarr')
  const journey = await seedTrainArrJourney(trainarrToken)
  const trainingAssignmentId = await createActiveTrainingAssignment(
    trainarrToken,
    journey.trainingDefinitionId,
  )

  let procurementStatusSeeded = false

  try {
    await createTrainArrMaterialDemandLine(trainarrToken, trainingAssignmentId, {
      partNumber: `E2E-MD-${Date.now()}`,
    })
    const published = await publishTrainArrMaterialDemand(trainarrToken, trainingAssignmentId, false)
    const adminToken = await loginNexArr()
    const statusToken = await issueSupplyarrToTrainarrDemandStatusToken(adminToken)
    await ingestTrainarrSupplyarrDemandStatus(statusToken, {
      trainarrPublicationId: published.publicationId,
      supplyarrDemandRefId: published.demandRefId,
      supplyarrCallbackPublicationId: crypto.randomUUID(),
      procurementStatus: 'pr_submitted',
      eventType: 'pr_submitted',
      message: 'E2E Playwright procurement status',
    })
    procurementStatusSeeded = true
  } catch {
    procurementStatusSeeded = false
  }

  return { trainingAssignmentId, procurementStatusSeeded }
}

export type SupplyArrDemandProcessingFixture = {
  demandRefId: string
  sourceRefKey: string
  title: string
}

type SupplyArrDemandProcessingJourneySeedResponse = {
  demandRefId: string
  demandRefSource: string
  sourceRefKey: string
  title: string
  demandRefCreated: boolean
  settingsEnsured: boolean
}

export async function seedSupplyArrDemandProcessingJourney(
  supplyarrAccessToken: string,
): Promise<SupplyArrDemandProcessingFixture> {
  const response = await fetch(`${supplyarrApiUrl()}/api/load-test-journey/seed`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${supplyarrAccessToken}` },
  })
  const payload = await readJson<SupplyArrDemandProcessingJourneySeedResponse>(response)
  return {
    demandRefId: payload.demandRefId,
    sourceRefKey: payload.sourceRefKey,
    title: payload.title,
  }
}

/** Idempotent MaintainArr demand ref plus demand-processing settings for W246/W294 smokes. */
export async function ensureSupplyArrDemandProcessingFixture(): Promise<SupplyArrDemandProcessingFixture> {
  const supplyarrToken = await redeemHandoffForProduct('supplyarr')
  return seedSupplyArrDemandProcessingJourney(supplyarrToken)
}

export type SupplyArrProcurementExceptionsFixture = {
  purchaseRequestId: string
  requestKey: string
  exceptionIds: string[]
  overdueExceptionId: string | null
  openExceptionId: string | null
}

type ProcurementExceptionSummary = {
  exceptionId: string
  isSlaBreached: boolean
}

async function createProcurementExceptionForPurchaseRequest(
  supplyarrAccessToken: string,
  purchaseRequestId: string,
  payload: {
    exceptionKey: string
    title: string
    category?: string
    slaDueAt?: string
  },
): Promise<ProcurementExceptionSummary> {
  const response = await fetch(
    `${supplyarrApiUrl()}/api/purchase-requests/${purchaseRequestId}/procurement-exceptions`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${supplyarrAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        exceptionKey: payload.exceptionKey,
        exceptionCategory: payload.category ?? 'policy_violation',
        title: payload.title,
        description: 'E2E Playwright procurement exception smoke',
        assignedToUserId: null,
        slaDueAt: payload.slaDueAt ?? null,
      }),
    },
  )
  return readJson<ProcurementExceptionSummary>(response)
}

async function createMinimalPurchaseRequestForExceptions(
  supplyarrAccessToken: string,
  suffix: number,
): Promise<{ purchaseRequestId: string; requestKey: string }> {
  const vendor = await readJson<{ partyId: string }>(
    await fetch(`${supplyarrApiUrl()}/api/vendors`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${supplyarrAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        partyKey: `e2e-pe-vendor-${suffix}`,
        displayName: 'E2E PE Vendor',
        legalName: 'E2E PE Vendor LLC',
        contactEmail: null,
        notes: '',
      }),
    }),
  )

  const part = await readJson<{ partId: string }>(
    await fetch(`${supplyarrApiUrl()}/api/parts`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${supplyarrAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        partKey: `e2e-pe-part-${suffix}`,
        catalogId: null,
        displayName: 'E2E PE Part',
        description: '',
        categoryKey: 'general',
        unitOfMeasure: 'each',
        manufacturerName: '',
        manufacturerPartNumber: '',
      }),
    }),
  )

  const purchaseRequest = await readJson<{ purchaseRequestId: string; requestKey: string }>(
    await fetch(`${supplyarrApiUrl()}/api/purchase-requests`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${supplyarrAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        requestKey: `e2e-pe-pr-${suffix}`,
        title: 'E2E procurement exception PR',
        notes: '',
        vendorPartyId: vendor.partyId,
        lines: [{ partId: part.partId, quantity: 2, notes: '' }],
      }),
    }),
  )

  return {
    purchaseRequestId: purchaseRequest.purchaseRequestId,
    requestKey: purchaseRequest.requestKey ?? `e2e-pe-pr-${suffix}`,
  }
}

async function createIssuedPurchaseOrderForExceptions(
  supplyarrAccessToken: string,
  suffix: number,
): Promise<{
  purchaseRequestId: string
  requestKey: string
  purchaseOrderId: string
  orderKey: string
}> {
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    supplyarrAccessToken,
    suffix,
  )

  await readJson(
    await fetch(
      `${supplyarrApiUrl()}/api/purchase-requests/${purchaseRequestId}/submit`,
      { method: 'POST', headers: { Authorization: `Bearer ${supplyarrAccessToken}` } },
    ),
  )
  await readJson(
    await fetch(
      `${supplyarrApiUrl()}/api/purchase-requests/${purchaseRequestId}/approve`,
      { method: 'POST', headers: { Authorization: `Bearer ${supplyarrAccessToken}` } },
    ),
  )

  const purchaseOrder = await readJson<{ purchaseOrderId: string; orderKey: string }>(
    await fetch(
      `${supplyarrApiUrl()}/api/purchase-orders/from-purchase-request/${purchaseRequestId}`,
      {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${supplyarrAccessToken}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          orderKey: `e2e-pe-po-${suffix}`,
          notes: null,
          requestedDeliveryAt: null,
        }),
      },
    ),
  )

  await readJson(
    await fetch(
      `${supplyarrApiUrl()}/api/purchase-orders/${purchaseOrder.purchaseOrderId}/approve`,
      { method: 'POST', headers: { Authorization: `Bearer ${supplyarrAccessToken}` } },
    ),
  )
  await readJson(
    await fetch(
      `${supplyarrApiUrl()}/api/purchase-orders/${purchaseOrder.purchaseOrderId}/issue`,
      { method: 'POST', headers: { Authorization: `Bearer ${supplyarrAccessToken}` } },
    ),
  )

  return {
    purchaseRequestId,
    requestKey,
    purchaseOrderId: purchaseOrder.purchaseOrderId,
    orderKey: purchaseOrder.orderKey ?? `e2e-pe-po-${suffix}`,
  }
}

/**
 * Purchase request subject plus open/overdue procurement exceptions for W250/W295 Playwright smokes.
 * Creates one overdue SLA exception and one open exception; no investigate/resolve mutations.
 */
export async function ensureSupplyArrProcurementExceptionsFixture(): Promise<SupplyArrProcurementExceptionsFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )

  const overdue = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: `PEX-OVD-${suffix}`,
    title: `E2E overdue procurement exception ${suffix}`,
    category: 'policy_violation',
    slaDueAt: new Date(Date.now() - 60 * 60 * 1000).toISOString(),
  })

  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: `PEX-OPEN-${suffix}`,
    title: `E2E open procurement exception ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    exceptionIds: [overdue.exceptionId, open.exceptionId],
    overdueExceptionId: overdue.exceptionId,
    openExceptionId: open.exceptionId,
  }
}

export type SupplyArrProcurementExceptionInvestigateResolveJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  openExceptionId: string
  openExceptionKey: string
}

export type SupplyArrProcurementExceptionDetail = {
  exceptionId: string
  exceptionKey: string
  status: string
  assignedToUserId: string | null
  resolutionTemplateKey: string | null
  resolutionNotes: string
  waiveJustification: string
  waiveRejectionReason: string
  linkedPurchaseRequestId: string | null
  linkedPurchaseRequestKey: string | null
  linkedPurchaseOrderId: string | null
  linkedPurchaseOrderKey: string | null
  closedAt: string | null
  cancelledAt: string | null
  cancellationReason: string
  reopenedAt: string | null
  lastReopenReason: string
  reopenCount: number
}

export type SupplyArrMeProfile = {
  userId: string
}

export async function getSupplyArrMeFromHandoff(): Promise<SupplyArrMeProfile> {
  const token = await redeemHandoffForProduct('supplyarr')
  const response = await fetch(`${supplyarrApiUrl()}/api/me`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  const payload = await readJson<{ userId: string }>(response)
  return { userId: payload.userId }
}

/**
 * Single open procurement exception on a PR for W303 investigate→resolve journey.
 */
export async function ensureSupplyArrProcurementExceptionInvestigateResolveJourneyFixture(): Promise<SupplyArrProcurementExceptionInvestigateResolveJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )

  const openExceptionKey = `PEX-JRN-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E investigate resolve journey ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function getSupplyArrProcurementException(
  supplyarrAccessToken: string,
  exceptionId: string,
): Promise<SupplyArrProcurementExceptionDetail> {
  const response = await fetch(`${supplyarrApiUrl()}/api/procurement-exceptions/${exceptionId}`, {
    headers: { Authorization: `Bearer ${supplyarrAccessToken}` },
  })
  return readJson<SupplyArrProcurementExceptionDetail>(response)
}

export async function getSupplyArrProcurementExceptionFromHandoff(
  exceptionId: string,
): Promise<SupplyArrProcurementExceptionDetail> {
  const token = await redeemHandoffForProduct('supplyarr')
  return getSupplyArrProcurementException(token, exceptionId)
}

export async function assertSupplyArrProcurementExceptionStatus(
  supplyarrAccessToken: string,
  exceptionId: string,
  expectedStatus: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementException(supplyarrAccessToken, exceptionId)
  if (detail.status !== expectedStatus) {
    throw new Error(
      `Expected exception ${exceptionId} status ${expectedStatus}, got ${detail.status}`,
    )
  }
}

export async function assertSupplyArrProcurementExceptionStatusFromHandoff(
  exceptionId: string,
  expectedStatus: string,
): Promise<void> {
  const token = await redeemHandoffForProduct('supplyarr')
  await assertSupplyArrProcurementExceptionStatus(token, exceptionId, expectedStatus)
}

export async function assertSupplyArrProcurementExceptionResolvedWithTemplateFromHandoff(
  exceptionId: string,
  expectedTemplateKey: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'resolved') {
    throw new Error(`Expected exception ${exceptionId} status resolved, got ${detail.status}`)
  }
  if (detail.resolutionTemplateKey !== expectedTemplateKey) {
    throw new Error(
      `Expected exception ${exceptionId} resolution template ${expectedTemplateKey}, got ${detail.resolutionTemplateKey ?? '(null)'}`,
    )
  }
}

export type SupplyArrProcurementExceptionCloseAfterResolveJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Single open procurement exception on a PR for W313 investigate→resolve→close journey.
 */
export async function ensureSupplyArrProcurementExceptionCloseAfterResolveJourneyFixture(): Promise<SupplyArrProcurementExceptionCloseAfterResolveJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )

  const openExceptionKey = `PEX-CAR-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E close after resolve journey ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionClosedAfterResolveFromHandoff(
  exceptionId: string,
  expectedTemplateKey: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'closed') {
    throw new Error(`Expected exception ${exceptionId} status closed, got ${detail.status}`)
  }
  if (!detail.closedAt) {
    throw new Error(`Expected exception ${exceptionId} closedAt to be set`)
  }
  if (detail.resolutionTemplateKey !== expectedTemplateKey) {
    throw new Error(
      `Expected exception ${exceptionId} resolution template ${expectedTemplateKey}, got ${detail.resolutionTemplateKey ?? '(null)'}`,
    )
  }
}

export const supplyArrProcurementExceptionWaiveCloseJourneyJustification =
  'E2E Playwright waive/close journey — one-time policy exception approved.'

export type SupplyArrProcurementExceptionWaiveCloseJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Single open procurement exception on a PR for W304 investigate→request waive→approve waive→close journey.
 */
export async function ensureSupplyArrProcurementExceptionWaiveCloseJourneyFixture(): Promise<SupplyArrProcurementExceptionWaiveCloseJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )

  const openExceptionKey = `PEX-WC-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E waive close journey ${suffix}`,
    category: 'policy_violation',
  })

  return {
    purchaseRequestId,
    requestKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionWaivedWithJustificationFromHandoff(
  exceptionId: string,
  expectedJustification: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'waived') {
    throw new Error(`Expected exception ${exceptionId} status waived, got ${detail.status}`)
  }
  if (detail.waiveJustification !== expectedJustification) {
    throw new Error(
      `Expected exception ${exceptionId} waive justification "${expectedJustification}", got "${detail.waiveJustification}"`,
    )
  }
}

export async function assertSupplyArrProcurementExceptionClosedFromHandoff(
  exceptionId: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'closed') {
    throw new Error(`Expected exception ${exceptionId} status closed, got ${detail.status}`)
  }
  if (!detail.closedAt) {
    throw new Error(`Expected exception ${exceptionId} closedAt to be set`)
  }
}

/** Matches ProcurementExceptionsPanel default reject-waive reason. */
export const supplyArrProcurementExceptionRejectWaiveDefaultReason =
  'Waive not justified for this procurement record.'

export const supplyArrProcurementExceptionRejectWaiveJourneyJustification =
  'E2E Playwright reject-waive journey — waiver request for policy review.'

export type SupplyArrProcurementExceptionRejectWaiveJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Single open procurement exception on a PR for W306 investigate→request waive→reject waive journey.
 */
export async function ensureSupplyArrProcurementExceptionRejectWaiveJourneyFixture(): Promise<SupplyArrProcurementExceptionRejectWaiveJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )

  const openExceptionKey = `PEX-RW-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E reject waive journey ${suffix}`,
    category: 'policy_violation',
  })

  return {
    purchaseRequestId,
    requestKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionRejectedWaiveFromHandoff(
  exceptionId: string,
  expectedRejectionReason: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'investigating') {
    throw new Error(
      `Expected exception ${exceptionId} status investigating after reject waive, got ${detail.status}`,
    )
  }
  if (detail.waiveRejectionReason !== expectedRejectionReason) {
    throw new Error(
      `Expected exception ${exceptionId} waive rejection reason "${expectedRejectionReason}", got "${detail.waiveRejectionReason}"`,
    )
  }
}

export const supplyArrProcurementExceptionCancelJourneyReason =
  'E2E Playwright cancel journey — duplicate exception opened in error.'

export type SupplyArrProcurementExceptionCancelJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Single open procurement exception on a PR for W308 investigate→cancel with reason journey.
 */
export async function ensureSupplyArrProcurementExceptionCancelJourneyFixture(): Promise<SupplyArrProcurementExceptionCancelJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )

  const openExceptionKey = `PEX-CAN-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E cancel journey ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionCancelledWithReasonFromHandoff(
  exceptionId: string,
  expectedCancellationReason: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'cancelled') {
    throw new Error(
      `Expected exception ${exceptionId} status cancelled, got ${detail.status}`,
    )
  }
  if (detail.cancellationReason !== expectedCancellationReason) {
    throw new Error(
      `Expected exception ${exceptionId} cancellation reason "${expectedCancellationReason}", got "${detail.cancellationReason}"`,
    )
  }
  if (!detail.cancelledAt) {
    throw new Error(`Expected exception ${exceptionId} cancelledAt to be set`)
  }
}

export const supplyArrProcurementExceptionPostCancelReopenJourneyReason =
  'E2E Playwright post-cancel reopen journey — resume investigation after mistaken cancel.'

export type SupplyArrProcurementExceptionPostCancelReopenJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Single open procurement exception on a PR for W311 investigate→cancel→reopen journey.
 */
export async function ensureSupplyArrProcurementExceptionPostCancelReopenJourneyFixture(): Promise<SupplyArrProcurementExceptionPostCancelReopenJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )

  const openExceptionKey = `PEX-PCR-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E post-cancel reopen journey ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionReopenedFromHandoff(
  exceptionId: string,
  expectedReopenReason: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'investigating') {
    throw new Error(
      `Expected exception ${exceptionId} status investigating after reopen, got ${detail.status}`,
    )
  }
  if (detail.lastReopenReason !== expectedReopenReason) {
    throw new Error(
      `Expected exception ${exceptionId} reopen reason "${expectedReopenReason}", got "${detail.lastReopenReason}"`,
    )
  }
  if (!detail.reopenedAt) {
    throw new Error(`Expected exception ${exceptionId} reopenedAt to be set`)
  }
  if (detail.reopenCount < 1) {
    throw new Error(`Expected exception ${exceptionId} reopenCount >= 1, got ${detail.reopenCount}`)
  }
}

export type SupplyArrProcurementExceptionPostRejectResolveJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Single open procurement exception on a PR for W309 investigate→request waive→reject waive→resolve journey.
 */
export async function ensureSupplyArrProcurementExceptionPostRejectResolveJourneyFixture(): Promise<SupplyArrProcurementExceptionPostRejectResolveJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )

  const openExceptionKey = `PEX-PRR-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E post-reject resolve journey ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionPostRejectResolvedWithTemplateFromHandoff(
  exceptionId: string,
  expectedTemplateKey: string,
  expectedWaiveRejectionReason: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'resolved') {
    throw new Error(
      `Expected exception ${exceptionId} status resolved after post-reject resolve, got ${detail.status}`,
    )
  }
  if (detail.resolutionTemplateKey !== expectedTemplateKey) {
    throw new Error(
      `Expected exception ${exceptionId} resolution template ${expectedTemplateKey}, got ${detail.resolutionTemplateKey ?? '(null)'}`,
    )
  }
  if (detail.waiveRejectionReason !== expectedWaiveRejectionReason) {
    throw new Error(
      `Expected exception ${exceptionId} waive rejection reason "${expectedWaiveRejectionReason}", got "${detail.waiveRejectionReason}"`,
    )
  }
}

export type SupplyArrProcurementExceptionAssignLinkJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  followUpPurchaseRequestId: string
  followUpRequestKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Unassigned open exception on a PR plus a separate follow-up PR for W310 assign→link journey.
 */
export async function ensureSupplyArrProcurementExceptionAssignLinkJourneyFixture(): Promise<SupplyArrProcurementExceptionAssignLinkJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )
  const followUp = await createMinimalPurchaseRequestForExceptions(token, suffix + 1)

  const openExceptionKey = `PEX-AL-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E assign link journey ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    followUpPurchaseRequestId: followUp.purchaseRequestId,
    followUpRequestKey: followUp.requestKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionAssignedAndLinkedFromHandoff(
  exceptionId: string,
  expectedAssignedToUserId: string,
  expectedLinkedPurchaseRequestId: string,
  expectedLinkedPurchaseRequestKey: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'open') {
    throw new Error(`Expected exception ${exceptionId} status open, got ${detail.status}`)
  }
  if (detail.assignedToUserId !== expectedAssignedToUserId) {
    throw new Error(
      `Expected exception ${exceptionId} assignedToUserId ${expectedAssignedToUserId}, got ${detail.assignedToUserId ?? '(null)'}`,
    )
  }
  if (detail.linkedPurchaseRequestId !== expectedLinkedPurchaseRequestId) {
    throw new Error(
      `Expected exception ${exceptionId} linkedPurchaseRequestId ${expectedLinkedPurchaseRequestId}, got ${detail.linkedPurchaseRequestId ?? '(null)'}`,
    )
  }
  if (detail.linkedPurchaseRequestKey !== expectedLinkedPurchaseRequestKey) {
    throw new Error(
      `Expected exception ${exceptionId} linkedPurchaseRequestKey ${expectedLinkedPurchaseRequestKey}, got ${detail.linkedPurchaseRequestKey ?? '(null)'}`,
    )
  }
}

export type SupplyArrProcurementExceptionInvestigateLinkResolveJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  followUpPurchaseRequestId: string
  followUpRequestKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Open exception on a PR plus a separate follow-up PR for W311 investigate→link→resolve journey.
 */
export async function ensureSupplyArrProcurementExceptionInvestigateLinkResolveJourneyFixture(): Promise<SupplyArrProcurementExceptionInvestigateLinkResolveJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )
  const followUp = await createMinimalPurchaseRequestForExceptions(token, suffix + 1)

  const openExceptionKey = `PEX-ILR-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E investigate link resolve journey ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    followUpPurchaseRequestId: followUp.purchaseRequestId,
    followUpRequestKey: followUp.requestKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionInvestigateLinkResolvedFromHandoff(
  exceptionId: string,
  expectedTemplateKey: string,
  expectedLinkedPurchaseRequestId: string,
  expectedLinkedPurchaseRequestKey: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'resolved') {
    throw new Error(`Expected exception ${exceptionId} status resolved, got ${detail.status}`)
  }
  if (detail.resolutionTemplateKey !== expectedTemplateKey) {
    throw new Error(
      `Expected exception ${exceptionId} resolution template ${expectedTemplateKey}, got ${detail.resolutionTemplateKey ?? '(null)'}`,
    )
  }
  if (detail.linkedPurchaseRequestId !== expectedLinkedPurchaseRequestId) {
    throw new Error(
      `Expected exception ${exceptionId} linkedPurchaseRequestId ${expectedLinkedPurchaseRequestId}, got ${detail.linkedPurchaseRequestId ?? '(null)'}`,
    )
  }
  if (detail.linkedPurchaseRequestKey !== expectedLinkedPurchaseRequestKey) {
    throw new Error(
      `Expected exception ${exceptionId} linkedPurchaseRequestKey ${expectedLinkedPurchaseRequestKey}, got ${detail.linkedPurchaseRequestKey ?? '(null)'}`,
    )
  }
}

export type SupplyArrProcurementExceptionCloseAfterLinkPrResolveJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  followUpPurchaseRequestId: string
  followUpRequestKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Open exception on a PR plus a separate follow-up PR for W314 investigate→link PR→resolve→close journey.
 */
export async function ensureSupplyArrProcurementExceptionCloseAfterLinkPrResolveJourneyFixture(): Promise<SupplyArrProcurementExceptionCloseAfterLinkPrResolveJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )
  const followUp = await createMinimalPurchaseRequestForExceptions(token, suffix + 1)

  const openExceptionKey = `PEX-CALPR-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E close after link PR resolve journey ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    followUpPurchaseRequestId: followUp.purchaseRequestId,
    followUpRequestKey: followUp.requestKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionClosedAfterLinkPrResolveFromHandoff(
  exceptionId: string,
  expectedTemplateKey: string,
  expectedLinkedPurchaseRequestId: string,
  expectedLinkedPurchaseRequestKey: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'closed') {
    throw new Error(`Expected exception ${exceptionId} status closed, got ${detail.status}`)
  }
  if (!detail.closedAt) {
    throw new Error(`Expected exception ${exceptionId} closedAt to be set`)
  }
  if (detail.resolutionTemplateKey !== expectedTemplateKey) {
    throw new Error(
      `Expected exception ${exceptionId} resolution template ${expectedTemplateKey}, got ${detail.resolutionTemplateKey ?? '(null)'}`,
    )
  }
  if (detail.linkedPurchaseRequestId !== expectedLinkedPurchaseRequestId) {
    throw new Error(
      `Expected exception ${exceptionId} linkedPurchaseRequestId ${expectedLinkedPurchaseRequestId}, got ${detail.linkedPurchaseRequestId ?? '(null)'}`,
    )
  }
  if (detail.linkedPurchaseRequestKey !== expectedLinkedPurchaseRequestKey) {
    throw new Error(
      `Expected exception ${exceptionId} linkedPurchaseRequestKey ${expectedLinkedPurchaseRequestKey}, got ${detail.linkedPurchaseRequestKey ?? '(null)'}`,
    )
  }
}

export type SupplyArrProcurementExceptionInvestigateLinkPoResolveJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  followUpPurchaseOrderId: string
  followUpOrderKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Open exception on a PR plus a separate issued follow-up PO for W312 investigate→link PO→resolve journey.
 */
export async function ensureSupplyArrProcurementExceptionInvestigateLinkPoResolveJourneyFixture(): Promise<SupplyArrProcurementExceptionInvestigateLinkPoResolveJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )
  const followUp = await createIssuedPurchaseOrderForExceptions(token, suffix + 1)

  const openExceptionKey = `PEX-ILPO-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E investigate link PO resolve journey ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    followUpPurchaseOrderId: followUp.purchaseOrderId,
    followUpOrderKey: followUp.orderKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionInvestigateLinkPoResolvedFromHandoff(
  exceptionId: string,
  expectedTemplateKey: string,
  expectedLinkedPurchaseOrderId: string,
  expectedLinkedPurchaseOrderKey: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'resolved') {
    throw new Error(`Expected exception ${exceptionId} status resolved, got ${detail.status}`)
  }
  if (detail.resolutionTemplateKey !== expectedTemplateKey) {
    throw new Error(
      `Expected exception ${exceptionId} resolution template ${expectedTemplateKey}, got ${detail.resolutionTemplateKey ?? '(null)'}`,
    )
  }
  if (detail.linkedPurchaseOrderId !== expectedLinkedPurchaseOrderId) {
    throw new Error(
      `Expected exception ${exceptionId} linkedPurchaseOrderId ${expectedLinkedPurchaseOrderId}, got ${detail.linkedPurchaseOrderId ?? '(null)'}`,
    )
  }
  if (detail.linkedPurchaseOrderKey !== expectedLinkedPurchaseOrderKey) {
    throw new Error(
      `Expected exception ${exceptionId} linkedPurchaseOrderKey ${expectedLinkedPurchaseOrderKey}, got ${detail.linkedPurchaseOrderKey ?? '(null)'}`,
    )
  }
}

export type SupplyArrProcurementExceptionCloseAfterLinkPoResolveJourneyFixture = {
  purchaseRequestId: string
  requestKey: string
  followUpPurchaseOrderId: string
  followUpOrderKey: string
  openExceptionId: string
  openExceptionKey: string
}

/**
 * Open exception on a PR plus a separate issued follow-up PO for W315 investigate→link PO→resolve→close journey.
 */
export async function ensureSupplyArrProcurementExceptionCloseAfterLinkPoResolveJourneyFixture(): Promise<SupplyArrProcurementExceptionCloseAfterLinkPoResolveJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId, requestKey } = await createMinimalPurchaseRequestForExceptions(
    token,
    suffix,
  )
  const followUp = await createIssuedPurchaseOrderForExceptions(token, suffix + 1)

  const openExceptionKey = `PEX-CALPO-${suffix}`
  const open = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: openExceptionKey,
    title: `E2E close after link PO resolve journey ${suffix}`,
    category: 'approval_delay',
  })

  return {
    purchaseRequestId,
    requestKey,
    followUpPurchaseOrderId: followUp.purchaseOrderId,
    followUpOrderKey: followUp.orderKey,
    openExceptionId: open.exceptionId,
    openExceptionKey,
  }
}

export async function assertSupplyArrProcurementExceptionClosedAfterLinkPoResolveFromHandoff(
  exceptionId: string,
  expectedTemplateKey: string,
  expectedLinkedPurchaseOrderId: string,
  expectedLinkedPurchaseOrderKey: string,
): Promise<void> {
  const detail = await getSupplyArrProcurementExceptionFromHandoff(exceptionId)
  if (detail.status !== 'closed') {
    throw new Error(`Expected exception ${exceptionId} status closed, got ${detail.status}`)
  }
  if (!detail.closedAt) {
    throw new Error(`Expected exception ${exceptionId} closedAt to be set`)
  }
  if (detail.resolutionTemplateKey !== expectedTemplateKey) {
    throw new Error(
      `Expected exception ${exceptionId} resolution template ${expectedTemplateKey}, got ${detail.resolutionTemplateKey ?? '(null)'}`,
    )
  }
  if (detail.linkedPurchaseOrderId !== expectedLinkedPurchaseOrderId) {
    throw new Error(
      `Expected exception ${exceptionId} linkedPurchaseOrderId ${expectedLinkedPurchaseOrderId}, got ${detail.linkedPurchaseOrderId ?? '(null)'}`,
    )
  }
  if (detail.linkedPurchaseOrderKey !== expectedLinkedPurchaseOrderKey) {
    throw new Error(
      `Expected exception ${exceptionId} linkedPurchaseOrderKey ${expectedLinkedPurchaseOrderKey}, got ${detail.linkedPurchaseOrderKey ?? '(null)'}`,
    )
  }
}

export type SupplyArrProcurementExceptionEscalationFixture = {
  overdueExceptionId: string
  overdueExceptionKey: string
}

/**
 * Overdue SLA procurement exception for W296/W297 Settings escalation pending preview smokes.
 * Creates one overdue active exception; no escalation batch mutations.
 */
export async function ensureSupplyArrProcurementExceptionEscalationFixture(): Promise<SupplyArrProcurementExceptionEscalationFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const suffix = Date.now()
  const { purchaseRequestId } = await createMinimalPurchaseRequestForExceptions(token, suffix)

  const overdueExceptionKey = `PEX-ESC-${suffix}`
  const overdue = await createProcurementExceptionForPurchaseRequest(token, purchaseRequestId, {
    exceptionKey: overdueExceptionKey,
    title: `E2E escalation pending exception ${suffix}`,
    category: 'policy_violation',
    slaDueAt: new Date(Date.now() - 60 * 60 * 1000).toISOString(),
  })

  return {
    overdueExceptionId: overdue.exceptionId,
    overdueExceptionKey,
  }
}

export type SupplyArrProcurementExceptionEscalationSettingsSnapshot = {
  isEnabled: boolean
  escalationCooldownHours: number
  maxEscalationsPerException: number
  notifyOnProcurementExceptionSlaEscalation: boolean
}

export async function getSupplyArrProcurementExceptionEscalationSettings(
  supplyarrAccessToken: string,
): Promise<SupplyArrProcurementExceptionEscalationSettingsSnapshot> {
  const response = await fetch(
    `${supplyarrApiUrl()}/api/procurement-exception-escalation-settings`,
    {
      headers: { Authorization: `Bearer ${supplyarrAccessToken}` },
    },
  )
  return readJson<SupplyArrProcurementExceptionEscalationSettingsSnapshot>(response)
}

export async function getSupplyArrProcurementExceptionEscalationSettingsFromHandoff(): Promise<SupplyArrProcurementExceptionEscalationSettingsSnapshot> {
  const token = await redeemHandoffForProduct('supplyarr')
  return getSupplyArrProcurementExceptionEscalationSettings(token)
}

export async function assertSupplyArrProcurementExceptionEscalationPendingContains(
  supplyarrAccessToken: string,
  exceptionKey: string,
): Promise<void> {
  const response = await fetch(
    `${supplyarrApiUrl()}/api/procurement-exception-escalation-settings/pending`,
    {
      headers: { Authorization: `Bearer ${supplyarrAccessToken}` },
    },
  )
  const payload = await readJson<{ items: Array<{ exceptionKey: string }> }>(response)
  const found = payload.items.some((item) => item.exceptionKey === exceptionKey)
  if (!found) {
    throw new Error(
      `Expected pending escalation preview to include ${exceptionKey}, got ${payload.items.map((item) => item.exceptionKey).join(', ') || '(empty)'}`,
    )
  }
}

export async function assertSupplyArrProcurementExceptionEscalationPendingContainsFromHandoff(
  exceptionKey: string,
): Promise<void> {
  const token = await redeemHandoffForProduct('supplyarr')
  await assertSupplyArrProcurementExceptionEscalationPendingContains(token, exceptionKey)
}

export const supplyArrProcurementExceptionEscalationJourneyWebhookUrl =
  'https://hooks.example.com/supplyarr-e2e-w301'

const supplyArrProcurementExceptionEscalationScope = 'supplyarr.procurement_exceptions.escalate'

export type SupplyArrProcurementExceptionEscalationJourneyFixture = {
  overdueExceptionId: string
  overdueExceptionKey: string
}

/**
 * Overdue SLA exception plus enabled escalation + notification settings for W301 process-batch journey.
 */
export async function ensureSupplyArrProcurementExceptionEscalationJourneyFixture(): Promise<SupplyArrProcurementExceptionEscalationJourneyFixture> {
  const token = await redeemHandoffForProduct('supplyarr')
  const fixture = await ensureSupplyArrProcurementExceptionEscalationFixture()

  await upsertSupplyArrProcurementExceptionEscalationSettings(token, {
    isEnabled: true,
    escalationCooldownHours: 24,
    maxEscalationsPerException: 5,
    notifyOnProcurementExceptionSlaEscalation: true,
  })

  await upsertSupplyArrProcurementNotificationSettings(token, {
    isEnabled: true,
    notificationWebhookUrl: supplyArrProcurementExceptionEscalationJourneyWebhookUrl,
    notifyOnPurchaseRequestSubmitted: false,
    notifyOnPurchaseRequestApproved: false,
    notifyOnPurchaseOrderIssued: false,
    notifyOnReceivingReceiptPosted: false,
  })

  return {
    overdueExceptionId: fixture.overdueExceptionId,
    overdueExceptionKey: fixture.overdueExceptionKey,
  }
}

export async function upsertSupplyArrProcurementExceptionEscalationSettings(
  supplyarrAccessToken: string,
  settings: SupplyArrProcurementExceptionEscalationSettingsSnapshot,
): Promise<SupplyArrProcurementExceptionEscalationSettingsSnapshot> {
  const response = await fetch(
    `${supplyarrApiUrl()}/api/procurement-exception-escalation-settings`,
    {
      method: 'PUT',
      headers: {
        Authorization: `Bearer ${supplyarrAccessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(settings),
    },
  )
  return readJson<SupplyArrProcurementExceptionEscalationSettingsSnapshot>(response)
}

export type SupplyArrProcurementNotificationSettingsSnapshot = {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnPurchaseRequestSubmitted: boolean
  notifyOnPurchaseRequestApproved: boolean
  notifyOnPurchaseOrderIssued: boolean
  notifyOnReceivingReceiptPosted: boolean
}

export async function upsertSupplyArrProcurementNotificationSettings(
  supplyarrAccessToken: string,
  settings: SupplyArrProcurementNotificationSettingsSnapshot,
): Promise<SupplyArrProcurementNotificationSettingsSnapshot> {
  const response = await fetch(`${supplyarrApiUrl()}/api/notification-settings`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${supplyarrAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(settings),
  })
  return readJson<SupplyArrProcurementNotificationSettingsSnapshot>(response)
}

export async function issueSupplyArrProcurementExceptionEscalationWorkerToken(
  adminAccessToken: string,
): Promise<string> {
  return issueSharedWorkerServiceToken(
    adminAccessToken,
    ['supplyarr'],
    supplyArrProcurementExceptionEscalationScope,
  )
}

export type ProcessSupplyArrProcurementExceptionEscalationBatchResult = {
  candidatesFound: number
  escalatedCount: number
}

export async function processSupplyArrProcurementExceptionEscalationBatch(
  serviceToken: string,
  batchSize = 25,
): Promise<ProcessSupplyArrProcurementExceptionEscalationBatchResult> {
  const response = await fetch(
    `${supplyarrApiUrl()}/api/internal/procurement-exception-escalations/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        batchSize,
      }),
    },
  )
  return readJson<ProcessSupplyArrProcurementExceptionEscalationBatchResult>(response)
}

export type SupplyArrProcurementExceptionEscalationEventSnapshot = {
  eventId: string
  procurementExceptionId: string
  exceptionKey: string
  escalationLevel: number
  actionKind: string
  notificationDispatchId: string | null
}

export async function assertSupplyArrProcurementExceptionEscalationEventsContain(
  supplyarrAccessToken: string,
  exceptionKey: string,
): Promise<void> {
  const response = await fetch(
    `${supplyarrApiUrl()}/api/procurement-exception-escalation-settings/events?limit=25`,
    {
      headers: { Authorization: `Bearer ${supplyarrAccessToken}` },
    },
  )
  const payload = await readJson<{ items: SupplyArrProcurementExceptionEscalationEventSnapshot[] }>(
    response,
  )
  const found = payload.items.some((item) => item.exceptionKey === exceptionKey)
  if (!found) {
    throw new Error(
      `Expected escalation events to include ${exceptionKey}, got ${payload.items.map((item) => item.exceptionKey).join(', ') || '(empty)'}`,
    )
  }
}

export async function assertSupplyArrProcurementExceptionEscalationEventsContainFromHandoff(
  exceptionKey: string,
): Promise<void> {
  const token = await redeemHandoffForProduct('supplyarr')
  await assertSupplyArrProcurementExceptionEscalationEventsContain(token, exceptionKey)
}

export async function assertSupplyArrProcurementExceptionEscalationRunEscalated(
  supplyarrAccessToken: string,
  minimumEscalatedCount: number,
): Promise<void> {
  const response = await fetch(
    `${supplyarrApiUrl()}/api/procurement-exception-escalation-settings/runs?limit=5`,
    {
      headers: { Authorization: `Bearer ${supplyarrAccessToken}` },
    },
  )
  const payload = await readJson<{
    items: Array<{ escalatedCount: number; candidatesFound: number }>
  }>(response)
  const latest = payload.items[0]
  if (!latest || latest.escalatedCount < minimumEscalatedCount) {
    throw new Error(
      `Expected latest escalation run to escalate at least ${minimumEscalatedCount}, got ${latest?.escalatedCount ?? 0}`,
    )
  }
}

export async function assertSupplyArrProcurementExceptionEscalationRunEscalatedFromHandoff(
  minimumEscalatedCount: number,
): Promise<void> {
  const token = await redeemHandoffForProduct('supplyarr')
  await assertSupplyArrProcurementExceptionEscalationRunEscalated(token, minimumEscalatedCount)
}

export type SupplyArrProcurementNotificationDispatchItem = {
  notificationId: string
  eventKind: string
  dispatchStatus: string
  relatedEntityId: string
}

export async function assertSupplyArrProcurementNotificationDispatchPending(
  supplyarrAccessToken: string,
  eventKind: string,
  relatedEntityId: string,
): Promise<void> {
  const response = await fetch(`${supplyarrApiUrl()}/api/notification-settings/dispatches?limit=25`, {
    headers: { Authorization: `Bearer ${supplyarrAccessToken}` },
  })
  const payload = await readJson<{ items: SupplyArrProcurementNotificationDispatchItem[] }>(response)
  const found = payload.items.some(
    (item) =>
      item.eventKind === eventKind &&
      item.relatedEntityId === relatedEntityId &&
      item.dispatchStatus.toLowerCase() === 'pending',
  )
  if (!found) {
    throw new Error(
      `Expected pending ${eventKind} dispatch for ${relatedEntityId}, got ${payload.items.map((item) => `${item.eventKind}:${item.dispatchStatus}`).join(', ') || '(empty)'}`,
    )
  }
}

export async function assertSupplyArrProcurementNotificationDispatchPendingFromHandoff(
  eventKind: string,
  relatedEntityId: string,
): Promise<void> {
  const token = await redeemHandoffForProduct('supplyarr')
  await assertSupplyArrProcurementNotificationDispatchPending(token, eventKind, relatedEntityId)
}

const supplyArrProcurementNotificationScope = 'supplyarr.notifications.dispatch'

export async function issueSupplyArrProcurementNotificationWorkerToken(
  adminAccessToken: string,
): Promise<string> {
  return issueSharedWorkerServiceToken(
    adminAccessToken,
    ['supplyarr'],
    supplyArrProcurementNotificationScope,
  )
}

export type ProcessSupplyArrProcurementNotificationBatchResult = {
  pendingFound: number
  dispatchedCount: number
  skippedCount: number
}

export async function processSupplyArrProcurementNotificationBatch(
  serviceToken: string,
  batchSize = 25,
): Promise<ProcessSupplyArrProcurementNotificationBatchResult> {
  const response = await fetch(
    `${supplyarrApiUrl()}/api/internal/procurement-notifications/process-batch`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${serviceToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        tenantId: demoCredentials.tenantId,
        asOfUtc: null,
        batchSize,
      }),
    },
  )
  return readJson<ProcessSupplyArrProcurementNotificationBatchResult>(response)
}

export async function listSupplyArrProcurementNotificationDispatches(
  supplyarrAccessToken: string,
  limit = 25,
): Promise<{ items: SupplyArrProcurementNotificationDispatchItem[] }> {
  const response = await fetch(`${supplyarrApiUrl()}/api/notification-settings/dispatches?limit=${limit}`, {
    headers: { Authorization: `Bearer ${supplyarrAccessToken}` },
  })
  return readJson<{ items: SupplyArrProcurementNotificationDispatchItem[] }>(response)
}

export async function assertSupplyArrProcurementNotificationDispatchProcessed(
  supplyarrAccessToken: string,
  eventKind: string,
  relatedEntityId: string,
): Promise<void> {
  const payload = await listSupplyArrProcurementNotificationDispatches(supplyarrAccessToken, 50)
  const matching = payload.items.find(
    (item) => item.eventKind === eventKind && item.relatedEntityId === relatedEntityId,
  )
  if (!matching) {
    throw new Error(
      `Expected processed ${eventKind} dispatch for ${relatedEntityId}; found ${payload.items.map((item) => `${item.eventKind}:${item.dispatchStatus}`).join(', ') || 'none'}.`,
    )
  }
  if (matching.dispatchStatus.toLowerCase() === 'pending') {
    throw new Error(
      `Expected ${eventKind} dispatch for ${relatedEntityId} to be processed; still pending.`,
    )
  }
}

export async function assertSupplyArrProcurementNotificationDispatchProcessedFromHandoff(
  eventKind: string,
  relatedEntityId: string,
): Promise<void> {
  const token = await redeemHandoffForProduct('supplyarr')
  await assertSupplyArrProcurementNotificationDispatchProcessed(token, eventKind, relatedEntityId)
}

export type RoutArrDispatchExceptionTriageFixture = {
  exceptionIds: string[]
  overdueExceptionId: string | null
  openExceptionId: string | null
}

type DispatchExceptionSummary = {
  exceptionId: string
  isSlaBreached: boolean
}

async function createDispatchException(
  routarrAccessToken: string,
  payload: {
    title: string
    description?: string
    category?: string
    slaDueAt?: string
  },
): Promise<DispatchExceptionSummary> {
  const response = await fetch(`${routarrApiUrl()}/api/dispatch/exceptions`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${routarrAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      title: payload.title,
      description: payload.description ?? 'E2E Playwright dispatch exception triage',
      category: payload.category ?? 'delay',
      tripId: null,
      assignedToUserId: null,
      slaDueAt: payload.slaDueAt ?? null,
    }),
  })
  return readJson<DispatchExceptionSummary>(response)
}

/**
 * Open dispatch exceptions for W254/W256 Playwright triage depth smokes.
 * Creates one overdue SLA row and one open row; no bulk assign/resolve mutations.
 */
export async function ensureRoutArrDispatchExceptionTriageFixture(): Promise<RoutArrDispatchExceptionTriageFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()

  const overdue = await createDispatchException(token, {
    title: `E2E overdue exception ${suffix}`,
    category: 'delay',
    slaDueAt: new Date(Date.now() - 60 * 60 * 1000).toISOString(),
  })

  const open = await createDispatchException(token, {
    title: `E2E open exception ${suffix}`,
    category: 'driver',
  })

  return {
    exceptionIds: [overdue.exceptionId, open.exceptionId],
    overdueExceptionId: overdue.exceptionId,
    openExceptionId: open.exceptionId,
  }
}

export type RoutArrUnassignedQueuePreviewFixture = {
  tripIds: string[]
  lateTripId: string | null
  onTrackTripId: string | null
}

async function createUnassignedTrip(
  routarrAccessToken: string,
  payload: {
    title: string
    description?: string
    scheduledStartAt: string
    scheduledEndAt: string
  },
): Promise<{ tripId: string }> {
  const response = await fetch(`${routarrApiUrl()}/api/trips`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${routarrAccessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      title: payload.title,
      description: payload.description ?? 'E2E Playwright unassigned queue preview',
      vehicleRefKey: null,
      scheduledStartAt: payload.scheduledStartAt,
      scheduledEndAt: payload.scheduledEndAt,
      loads: null,
    }),
  })
  const trip = await readJson<{ tripId: string }>(response)
  return { tripId: trip.tripId }
}

/**
 * Unassigned trips for W255/W258 Playwright preview-before-assign depth smokes.
 * Creates one late and one on-track unassigned trip; no assign-driver mutations.
 */
export async function ensureRoutArrUnassignedQueuePreviewFixture(): Promise<RoutArrUnassignedQueuePreviewFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()

  const late = await createUnassignedTrip(token, {
    title: `E2E late unassigned ${suffix}`,
    scheduledStartAt: new Date(now - 2 * 60 * 60 * 1000).toISOString(),
    scheduledEndAt: new Date(now + 2 * 60 * 60 * 1000).toISOString(),
  })

  const onTrack = await createUnassignedTrip(token, {
    title: `E2E on-track unassigned ${suffix}`,
    scheduledStartAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
    scheduledEndAt: new Date(now + 8 * 60 * 60 * 1000).toISOString(),
  })

  return {
    tripIds: [late.tripId, onTrack.tripId],
    lateTripId: late.tripId,
    onTrackTripId: onTrack.tripId,
  }
}

export type RoutArrProofDvirCaptureFixture = {
  tripId: string
}

export type RoutArrAttachmentUploadFixture = {
  tripId: string
  pickupProofId: string
}

export type RoutArrAttachmentDownloadFixture = {
  tripId: string
  pickupProofId: string
  attachmentId: string
  fileName: string
}

export type RoutArrDocumentAttachmentUploadFixture = {
  tripId: string
  pickupProofId: string
  expectedFileName: string
}

export type RoutArrPhotoAttachmentUploadFixture = {
  tripId: string
  pickupProofId: string
  expectedFileName: string
}

export type RoutArrSignatureAttachmentUploadFixture = {
  tripId: string
  pickupProofId: string
  expectedFileName: string
}

export type RoutArrDvirPhotoAttachmentUploadFixture = {
  tripId: string
  expectedFileName: string
}

export type RoutArrPostTripDvirPhotoAttachmentUploadFixture = {
  tripId: string
  expectedFileName: string
}

export type RoutArrTripCompleteAfterCaptureFixture = {
  tripId: string
  expectedFileName: string
}

export type RoutArrTripCloseAfterCompleteFixture = {
  tripId: string
  expectedFileName: string
}

/**
 * Dispatched driver-portal trip for W257/W259 Playwright proof/DVIR capture depth smokes.
 * Assigns demo admin (journey subject), sets dispatched status, ensures pre-trip DVIR policy.
 */
export async function ensureRoutArrProofDvirCaptureFixture(): Promise<RoutArrProofDvirCaptureFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: true,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: false,
        requirePickupProofBeforeStart: false,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E capture depth ${suffix}`,
        description: 'W259 Playwright proof/DVIR capture depth smoke',
        vehicleRefKey: 'VEH-E2E-CAPTURE',
        scheduledStartAt: new Date(now + 60 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 5 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  return { tripId: trip.tripId }
}

/**
 * Dispatched driver-portal trip with pickup proof for W261/W264 Playwright attachment upload smokes.
 * Enables pickup photo + delivery signature policies; pre-creates pickup proof only (uploads happen in UI).
 */
export async function ensureRoutArrAttachmentUploadFixture(): Promise<RoutArrAttachmentUploadFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: false,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: true,
        requirePickupProofBeforeStart: true,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
        requirePickupProofPhotoBeforeStart: true,
        requireDeliveryProofPhotoBeforeComplete: false,
        requireDeliverySignatureBeforeComplete: true,
        requirePreTripDvirPhotoBeforeStart: false,
        requirePostTripDvirPhotoBeforeComplete: false,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E attachment upload ${suffix}`,
        description: 'W264 Playwright driver-portal attachment upload smoke',
        vehicleRefKey: 'VEH-E2E-ATTACH',
        scheduledStartAt: new Date(now - 30 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  const proof = await readJson<{ proofId: string }>(
    await fetch(`${routarrApiUrl()}/api/driver-portal/trips/${trip.tripId}/proofs`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        proofType: 'pickup',
        vehicleRefKey: 'VEH-E2E-ATTACH',
        referenceKey: `BOL-${suffix}`,
        notes: 'W264 attachment upload fixture',
      }),
    }),
  )

  return { tripId: trip.tripId, pickupProofId: proof.proofId }
}

/**
 * Dispatched driver-portal trip with pickup proof for W266 Playwright document attachment upload smoke
 * and W269 end-to-end document journey smoke (browser upload → dispatch read download).
 * No photo/signature gates — pickup proof + document upload happen in the browser.
 */
export async function ensureRoutArrDocumentAttachmentUploadFixture(): Promise<RoutArrDocumentAttachmentUploadFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedFileName = `pickup-bol-e2e-${suffix}.pdf`

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: false,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: false,
        requirePickupProofBeforeStart: true,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
        requirePickupProofPhotoBeforeStart: false,
        requireDeliveryProofPhotoBeforeComplete: false,
        requireDeliverySignatureBeforeComplete: false,
        requirePreTripDvirPhotoBeforeStart: false,
        requirePostTripDvirPhotoBeforeComplete: false,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E document upload ${suffix}`,
        description: 'W266 Playwright driver-portal document attachment upload smoke',
        vehicleRefKey: 'VEH-E2E-DOC',
        scheduledStartAt: new Date(now - 30 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  const proof = await readJson<{ proofId: string }>(
    await fetch(`${routarrApiUrl()}/api/driver-portal/trips/${trip.tripId}/proofs`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        proofType: 'pickup',
        vehicleRefKey: 'VEH-E2E-DOC',
        referenceKey: `BOL-DOC-${suffix}`,
        notes: 'W266 document attachment upload fixture',
      }),
    }),
  )

  return {
    tripId: trip.tripId,
    pickupProofId: proof.proofId,
    expectedFileName,
  }
}

/**
 * Dispatched driver-portal trip with pickup proof for W270 end-to-end photo journey smoke
 * (browser upload → dispatch read download).
 * No document/signature gates — pickup proof + photo upload happen in the browser.
 */
export async function ensureRoutArrPhotoAttachmentUploadFixture(): Promise<RoutArrPhotoAttachmentUploadFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedFileName = `pickup-photo-e2e-${suffix}.jpg`

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: false,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: false,
        requirePickupProofBeforeStart: true,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
        requirePickupProofPhotoBeforeStart: false,
        requireDeliveryProofPhotoBeforeComplete: false,
        requireDeliverySignatureBeforeComplete: false,
        requirePreTripDvirPhotoBeforeStart: false,
        requirePostTripDvirPhotoBeforeComplete: false,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E photo journey ${suffix}`,
        description: 'W270 Playwright photo attachment journey smoke',
        vehicleRefKey: 'VEH-E2E-PHOTO-J',
        scheduledStartAt: new Date(now - 30 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  const proof = await readJson<{ proofId: string }>(
    await fetch(`${routarrApiUrl()}/api/driver-portal/trips/${trip.tripId}/proofs`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        proofType: 'pickup',
        vehicleRefKey: 'VEH-E2E-PHOTO-J',
        referenceKey: `BOL-PHOTO-${suffix}`,
        notes: 'W270 photo attachment journey fixture',
      }),
    }),
  )

  return {
    tripId: trip.tripId,
    pickupProofId: proof.proofId,
    expectedFileName,
  }
}

/**
 * Dispatched driver-portal trip with pickup proof for W271 end-to-end signature journey smoke
 * (browser delivery signature upload → dispatch read download).
 * No pickup photo or delivery signature gates — start trip, delivery proof, and signature happen in the browser.
 */
export async function ensureRoutArrSignatureAttachmentUploadFixture(): Promise<RoutArrSignatureAttachmentUploadFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedFileName = 'signature.png'

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: false,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: true,
        requirePickupProofBeforeStart: true,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
        requirePickupProofPhotoBeforeStart: false,
        requireDeliveryProofPhotoBeforeComplete: false,
        requireDeliverySignatureBeforeComplete: false,
        requirePreTripDvirPhotoBeforeStart: false,
        requirePostTripDvirPhotoBeforeComplete: false,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E signature journey ${suffix}`,
        description: 'W271 Playwright signature attachment journey smoke',
        vehicleRefKey: 'VEH-E2E-SIG-J',
        scheduledStartAt: new Date(now - 30 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  const proof = await readJson<{ proofId: string }>(
    await fetch(`${routarrApiUrl()}/api/driver-portal/trips/${trip.tripId}/proofs`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        proofType: 'pickup',
        vehicleRefKey: 'VEH-E2E-SIG-J',
        referenceKey: `BOL-SIG-${suffix}`,
        notes: 'W271 signature attachment journey fixture',
      }),
    }),
  )

  return {
    tripId: trip.tripId,
    pickupProofId: proof.proofId,
    expectedFileName,
  }
}

/**
 * Dispatched driver-portal trip for W272 end-to-end DVIR photo journey smoke
 * (browser pre-trip DVIR submit + photo upload → dispatch read download).
 * No pre-submitted DVIR or attachments — capture happens in the browser.
 */
export async function ensureRoutArrDvirPhotoAttachmentUploadFixture(): Promise<RoutArrDvirPhotoAttachmentUploadFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedFileName = `pretrip-dvir-photo-e2e-${suffix}.jpg`

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: true,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: false,
        requirePickupProofBeforeStart: false,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
        requirePickupProofPhotoBeforeStart: false,
        requireDeliveryProofPhotoBeforeComplete: false,
        requireDeliverySignatureBeforeComplete: false,
        requirePreTripDvirPhotoBeforeStart: false,
        requirePostTripDvirPhotoBeforeComplete: false,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E DVIR photo journey ${suffix}`,
        description: 'W272 Playwright DVIR photo attachment journey smoke',
        vehicleRefKey: 'VEH-E2E-DVIR-J',
        scheduledStartAt: new Date(now - 30 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  return {
    tripId: trip.tripId,
    expectedFileName,
  }
}

/**
 * Dispatched driver-portal trip for W273 end-to-end post-trip DVIR photo journey smoke
 * (browser start trip → post-trip DVIR submit + photo upload → dispatch read download).
 * No pre-submitted DVIR or attachments — capture happens in the browser.
 */
export async function ensureRoutArrPostTripDvirPhotoAttachmentUploadFixture(): Promise<RoutArrPostTripDvirPhotoAttachmentUploadFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedFileName = `posttrip-dvir-photo-e2e-${suffix}.jpg`

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: false,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: false,
        requirePickupProofBeforeStart: false,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
        requirePickupProofPhotoBeforeStart: false,
        requireDeliveryProofPhotoBeforeComplete: false,
        requireDeliverySignatureBeforeComplete: false,
        requirePreTripDvirPhotoBeforeStart: false,
        requirePostTripDvirPhotoBeforeComplete: false,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E post-trip DVIR photo journey ${suffix}`,
        description: 'W273 Playwright post-trip DVIR photo attachment journey smoke',
        vehicleRefKey: 'VEH-E2E-POST-DVIR-J',
        scheduledStartAt: new Date(now - 30 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  return {
    tripId: trip.tripId,
    expectedFileName,
  }
}

/**
 * Dispatched driver-portal trip for W274 end-to-end trip-complete-after-capture journey smoke
 * (browser start trip → post-trip DVIR submit + photo upload → Complete → dispatch read verification).
 * Post-trip DVIR photo is required before complete; no pre-submitted DVIR or attachments.
 */
export async function ensureRoutArrTripCompleteAfterCaptureFixture(): Promise<RoutArrTripCompleteAfterCaptureFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()
  const expectedFileName = `posttrip-complete-dvir-photo-e2e-${suffix}.jpg`

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: false,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: false,
        requirePickupProofBeforeStart: false,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
        requirePickupProofPhotoBeforeStart: false,
        requireDeliveryProofPhotoBeforeComplete: false,
        requireDeliverySignatureBeforeComplete: false,
        requirePreTripDvirPhotoBeforeStart: false,
        requirePostTripDvirPhotoBeforeComplete: true,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E trip complete after capture ${suffix}`,
        description: 'W274 Playwright trip-complete-after-capture journey smoke',
        vehicleRefKey: 'VEH-E2E-COMPLETE-J',
        scheduledStartAt: new Date(now - 30 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  return {
    tripId: trip.tripId,
    expectedFileName,
  }
}

/**
 * Dispatched driver-portal trip for W275 end-to-end trip-close-after-complete journey smoke
 * (browser start trip → post-trip DVIR submit + photo upload → Complete → Close → dispatch read verification).
 * Reuses the same tenant execution policy and trip seed shape as W274.
 */
export async function ensureRoutArrTripCloseAfterCompleteFixture(): Promise<RoutArrTripCloseAfterCompleteFixture> {
  const fixture = await ensureRoutArrTripCompleteAfterCaptureFixture()
  return {
    tripId: fixture.tripId,
    expectedFileName: fixture.expectedFileName,
  }
}

/**
 * Dispatched trip with pickup proof + photo attachment for W265 Playwright dispatcher download smoke.
 * Uploads attachment via API (read-only UI path on dispatch proof/DVIR read panel).
 */
export async function ensureRoutArrAttachmentDownloadFixture(): Promise<RoutArrAttachmentDownloadFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: false,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: false,
        requirePickupProofBeforeStart: true,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
        requirePickupProofPhotoBeforeStart: false,
        requireDeliveryProofPhotoBeforeComplete: false,
        requireDeliverySignatureBeforeComplete: false,
        requirePreTripDvirPhotoBeforeStart: false,
        requirePostTripDvirPhotoBeforeComplete: false,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E attachment download ${suffix}`,
        description: 'W265 Playwright dispatch proof/DVIR read attachment download smoke',
        vehicleRefKey: 'VEH-E2E-DL',
        scheduledStartAt: new Date(now - 30 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  const proof = await readJson<{ proofId: string }>(
    await fetch(`${routarrApiUrl()}/api/driver-portal/trips/${trip.tripId}/proofs`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        proofType: 'pickup',
        vehicleRefKey: 'VEH-E2E-DL',
        referenceKey: `BOL-DL-${suffix}`,
        notes: 'W265 attachment download fixture',
      }),
    }),
  )

  const fileName = `pickup-dispatch-e2e-${suffix}.jpg`
  const attachment = await readJson<{ attachmentId: string; fileName: string }>(
    await fetch(
      `${routarrApiUrl()}/api/driver-portal/trips/${trip.tripId}/proofs/${proof.proofId}/attachments`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          attachmentKind: 'photo',
          fileName,
          contentType: 'image/jpeg',
          contentBase64: Buffer.from([0xff, 0xd8, 0xff, 0xd9]).toString('base64'),
          notes: 'W265 dispatcher download seed',
        }),
      },
    ),
  )

  return {
    tripId: trip.tripId,
    pickupProofId: proof.proofId,
    attachmentId: attachment.attachmentId,
    fileName: attachment.fileName ?? fileName,
  }
}

export type RoutArrDvirAttachmentDownloadFixture = {
  tripId: string
  dvirId: string
  attachmentId: string
  fileName: string
}

export type RoutArrDocumentAttachmentDownloadFixture = {
  tripId: string
  pickupProofId: string
  attachmentId: string
  fileName: string
}

const minimalPdfBytes = Buffer.from(
  '%PDF-1.4\n1 0 obj<<>>endobj\ntrailer<</Root 1 0 R>>\n%%EOF\n',
)

/**
 * Dispatched trip with pre-trip DVIR + photo attachment for W267 Playwright dispatcher download smoke.
 * Submits DVIR and uploads attachment via API (read-only UI path on dispatch proof/DVIR read panel).
 */
export async function ensureRoutArrDvirAttachmentDownloadFixture(): Promise<RoutArrDvirAttachmentDownloadFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: false,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: false,
        requirePickupProofBeforeStart: false,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
        requirePickupProofPhotoBeforeStart: false,
        requireDeliveryProofPhotoBeforeComplete: false,
        requireDeliverySignatureBeforeComplete: false,
        requirePreTripDvirPhotoBeforeStart: false,
        requirePostTripDvirPhotoBeforeComplete: false,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E DVIR attachment download ${suffix}`,
        description: 'W267 Playwright dispatch proof/DVIR read DVIR attachment download smoke',
        vehicleRefKey: 'VEH-E2E-DVIR-DL',
        scheduledStartAt: new Date(now - 30 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  const dvir = await readJson<{ dvirId: string }>(
    await fetch(`${routarrApiUrl()}/api/driver-portal/trips/${trip.tripId}/dvir`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        phase: 'pre_trip',
        vehicleRefKey: 'VEH-E2E-DVIR-DL',
        result: 'pass',
        odometerReading: 12000,
        defectNotes: null,
      }),
    }),
  )

  const fileName = `pretrip-dvir-dispatch-e2e-${suffix}.jpg`
  const attachment = await readJson<{ attachmentId: string; fileName: string }>(
    await fetch(
      `${routarrApiUrl()}/api/driver-portal/trips/${trip.tripId}/dvir/${dvir.dvirId}/attachments`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          attachmentKind: 'photo',
          fileName,
          contentType: 'image/jpeg',
          contentBase64: Buffer.from([0xff, 0xd8, 0xff, 0xd9]).toString('base64'),
          notes: 'W267 dispatcher DVIR download seed',
        }),
      },
    ),
  )

  return {
    tripId: trip.tripId,
    dvirId: dvir.dvirId,
    attachmentId: attachment.attachmentId,
    fileName: attachment.fileName ?? fileName,
  }
}

/**
 * Dispatched trip with pickup proof + document (PDF) attachment for W268 Playwright dispatcher download smoke.
 * Uploads attachment via API (read-only UI path on dispatch proof/DVIR read panel).
 */
export async function ensureRoutArrDocumentAttachmentDownloadFixture(): Promise<RoutArrDocumentAttachmentDownloadFixture> {
  const token = await redeemHandoffForProduct('routarr')
  const suffix = Date.now()
  const now = Date.now()

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trip-execution-settings`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        requirePreTripDvirBeforeStart: false,
        requirePostTripDvirBeforeComplete: false,
        requireDeliveryProofBeforeComplete: false,
        requirePickupProofBeforeStart: true,
        blockTripStartOnDvirFail: true,
        blockTripCompleteOnDvirFail: true,
        requirePickupProofPhotoBeforeStart: false,
        requireDeliveryProofPhotoBeforeComplete: false,
        requireDeliverySignatureBeforeComplete: false,
        requirePreTripDvirPhotoBeforeStart: false,
        requirePostTripDvirPhotoBeforeComplete: false,
      }),
    }),
  )

  const trip = await readJson<{ tripId: string }>(
    await fetch(`${routarrApiUrl()}/api/trips`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: `E2E document attachment download ${suffix}`,
        description: 'W268 Playwright dispatch proof/DVIR read document attachment download smoke',
        vehicleRefKey: 'VEH-E2E-DOC-DL',
        scheduledStartAt: new Date(now - 30 * 60 * 1000).toISOString(),
        scheduledEndAt: new Date(now + 4 * 60 * 60 * 1000).toISOString(),
        loads: null,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/assign-driver`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        driverPersonId: journeySubjectPersonId,
        ignoreAvailabilityConflicts: true,
        ignoreEligibilityBlocks: true,
        ignoreWorkflowGateBlocks: true,
      }),
    }),
  )

  await readJson(
    await fetch(`${routarrApiUrl()}/api/trips/${trip.tripId}/status`, {
      method: 'PATCH',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ dispatchStatus: 'dispatched' }),
    }),
  )

  const proof = await readJson<{ proofId: string }>(
    await fetch(`${routarrApiUrl()}/api/driver-portal/trips/${trip.tripId}/proofs`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        proofType: 'pickup',
        vehicleRefKey: 'VEH-E2E-DOC-DL',
        referenceKey: `BOL-DOC-DL-${suffix}`,
        notes: 'W268 document attachment download fixture',
      }),
    }),
  )

  const fileName = `pickup-bol-dispatch-e2e-${suffix}.pdf`
  const attachment = await readJson<{ attachmentId: string; fileName: string }>(
    await fetch(
      `${routarrApiUrl()}/api/driver-portal/trips/${trip.tripId}/proofs/${proof.proofId}/attachments`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          attachmentKind: 'document',
          fileName,
          contentType: 'application/pdf',
          contentBase64: minimalPdfBytes.toString('base64'),
          notes: 'W268 dispatcher document download seed',
        }),
      },
    ),
  )

  return {
    tripId: trip.tripId,
    pickupProofId: proof.proofId,
    attachmentId: attachment.attachmentId,
    fileName: attachment.fileName ?? fileName,
  }
}
