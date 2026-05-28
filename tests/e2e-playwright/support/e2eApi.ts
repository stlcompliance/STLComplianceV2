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

function supplyarrApiUrl(): string {
  return process.env.E2E_SUPPLYARR_API_URL ?? 'http://localhost:5106'
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

export type MaintainArrFieldInboxFixture = {
  workOrderId: string
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
  const response = await fetch(`${nexarrApiUrl()}/api/launch/handoff`, {
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
  supplyarr: supplyarrApiUrl,
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

/** Ensures an active TrainArr assignment exists for companion field-inbox deep-link smokes. */

export type CompanionOfflineActionSyncedItem = {
  idempotencyKey: string
  actionKind: string
  taskKey: string
  productKey: string
  syncedAt: string
}

export async function listCompanionOfflineActions(
  companionAccessToken: string,
  limit = 10,
): Promise<{ items: CompanionOfflineActionSyncedItem[] }> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${nexarrApiUrl()}/api/companion/offline-actions?${search}`, {
    headers: {
      Authorization: `Bearer ${companionAccessToken}`,
    },
  })
  return readJson<{ items: CompanionOfflineActionSyncedItem[] }>(response)
}

const platformAuditGenerateScope = 'nexarr.platform_audit_packages.generate'

export async function issueSharedWorkerNexArrServiceToken(
  adminAccessToken: string,
  actionScope: string = platformAuditGenerateScope,
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
      allowedProductKeys: ['nexarr'],
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
      allowedProductKeys: ['nexarr'],
      actionScope,
      lifetimeMinutes: 30,
    }),
  })
  const issued = await readJson<{ accessToken: string }>(issueResponse)
  return issued.accessToken
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
        description: 'Companion deep-link smoke',
        priority: 'high',
        assignedTechnicianPersonId: journeySubjectPersonId,
        pmScheduleId: null,
      }),
    }),
  )

  return { workOrderId: workOrder.workOrderId }
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
