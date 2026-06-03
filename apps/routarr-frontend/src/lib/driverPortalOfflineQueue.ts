import {
  closeDriverPortalTrip,
  completeDriverPortalTrip,
  createDriverPortalTripProof,
  dispatchDriverPortalTrip,
  reportDriverPortalTripException,
  startDriverPortalTrip,
  submitDriverPortalTripDvir,
} from '../api/client'
import type {
  CreateTripProofRequest,
  DriverPortalReportExceptionRequest,
  SubmitTripDvirRequest,
} from '../api/types'

const STORAGE_KEY = 'routarr.driver-portal.offline-actions.v1'

type BaseOfflineAction = {
  id: string
  createdAt: string
  tripId: string
  tripNumber: string
  tripTitle: string
}

export type DriverPortalOfflineAction =
  | (BaseOfflineAction & {
      kind: 'trip'
      action: 'dispatch' | 'start' | 'complete' | 'close'
    })
  | (BaseOfflineAction & {
      kind: 'proof'
      payload: CreateTripProofRequest
    })
  | (BaseOfflineAction & {
      kind: 'dvir'
      payload: SubmitTripDvirRequest
    })
  | (BaseOfflineAction & {
      kind: 'exception'
      payload: DriverPortalReportExceptionRequest
    })

type BaseOfflineActionInput = Omit<BaseOfflineAction, 'id' | 'createdAt'>

export type DriverPortalOfflineActionInput =
  | (BaseOfflineActionInput & {
      kind: 'trip'
      action: 'dispatch' | 'start' | 'complete' | 'close'
    })
  | (BaseOfflineActionInput & {
      kind: 'proof'
      payload: CreateTripProofRequest
    })
  | (BaseOfflineActionInput & {
      kind: 'dvir'
      payload: SubmitTripDvirRequest
    })
  | (BaseOfflineActionInput & {
      kind: 'exception'
      payload: DriverPortalReportExceptionRequest
    })

function hasWindowStorage() {
  return typeof window !== 'undefined' && typeof window.localStorage !== 'undefined'
}

function readStoredActions(): DriverPortalOfflineAction[] {
  if (!hasWindowStorage()) {
    return []
  }

  const raw = window.localStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return []
  }

  try {
    const parsed = JSON.parse(raw) as DriverPortalOfflineAction[]
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

function writeStoredActions(actions: DriverPortalOfflineAction[]) {
  if (!hasWindowStorage()) {
    return
  }

  window.localStorage.setItem(STORAGE_KEY, JSON.stringify(actions))
}

export function loadDriverPortalOfflineActions(): DriverPortalOfflineAction[] {
  return readStoredActions()
}

export function saveDriverPortalOfflineActions(actions: DriverPortalOfflineAction[]) {
  writeStoredActions(actions)
}

export function enqueueDriverPortalOfflineAction(
  action: DriverPortalOfflineActionInput,
): DriverPortalOfflineAction {
  const id = typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function'
    ? crypto.randomUUID()
    : `${Date.now()}-${Math.random().toString(16).slice(2)}`
  const createdAt = new Date().toISOString()
  const entry =
    action.kind === 'trip'
      ? { ...action, id, createdAt }
      : action.kind === 'proof'
        ? { ...action, id, createdAt }
        : action.kind === 'dvir'
          ? { ...action, id, createdAt }
          : { ...action, id, createdAt }

  const next = [...readStoredActions(), entry]
  writeStoredActions(next)
  return entry
}

export function removeDriverPortalOfflineAction(actionId: string) {
  const next = readStoredActions().filter((action) => action.id !== actionId)
  writeStoredActions(next)
  return next
}

export function clearDriverPortalOfflineActions() {
  writeStoredActions([])
}

export function formatDriverPortalOfflineAction(action: DriverPortalOfflineAction) {
  switch (action.kind) {
    case 'trip':
      return `${action.tripNumber} · ${action.tripTitle} · ${action.action}`
    case 'proof':
      return `${action.tripNumber} · ${action.tripTitle} · proof (${action.payload.proofType})`
    case 'dvir':
      return `${action.tripNumber} · ${action.tripTitle} · ${action.payload.phase.replace('_', ' ')} DVIR`
    case 'exception':
      return `${action.tripNumber} · ${action.tripTitle} · exception (${action.payload.exceptionType ?? 'other'})`
  }
}

export function isDriverPortalOfflineError(error: unknown) {
  if (typeof window !== 'undefined' && window.navigator.onLine === false) {
    return true
  }

  if (error instanceof TypeError) {
    return true
  }

  const message = error instanceof Error ? error.message : String(error)
  return /failed to fetch|network error|fetch failed/i.test(message)
}

export async function replayDriverPortalOfflineAction(accessToken: string, action: DriverPortalOfflineAction) {
  switch (action.kind) {
    case 'trip':
      if (action.action === 'dispatch') return dispatchDriverPortalTrip(accessToken, action.tripId)
      if (action.action === 'start') return startDriverPortalTrip(accessToken, action.tripId)
      if (action.action === 'complete') return completeDriverPortalTrip(accessToken, action.tripId)
      return closeDriverPortalTrip(accessToken, action.tripId)
    case 'proof':
      return createDriverPortalTripProof(accessToken, action.tripId, action.payload)
    case 'dvir':
      return submitDriverPortalTripDvir(accessToken, action.tripId, action.payload)
    case 'exception':
      return reportDriverPortalTripException(accessToken, action.tripId, action.payload)
  }
}
