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
  const trainarr = handoffProductFrontends.find((p) => p.productKey === 'trainarr')
  return trainarr?.baseUrl ?? 'http://localhost:5176'
}

export type TrainArrJourneyFixture = {
  trainingAssignmentId: string
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
  const response = await fetch(`${trainarrApiUrl()}/api/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  const payload = await readJson<{ accessToken: string }>(response)
  return payload.accessToken
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
export async function ensureTrainArrFieldInboxFixture(): Promise<TrainArrJourneyFixture> {
  const nexarrToken = await loginNexArr()
  const handoffCode = await createHandoff(
    nexarrToken,
    'trainarr',
    `${trainarrFrontendUrl()}/launch`,
  )
  const trainarrToken = await redeemTrainArrHandoff(handoffCode)
  const journey = await seedTrainArrJourney(trainarrToken)
  const trainingAssignmentId = await createActiveTrainingAssignment(
    trainarrToken,
    journey.trainingDefinitionId,
  )
  return { trainingAssignmentId }
}
