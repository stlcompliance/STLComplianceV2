import { MAX_OFFLINE_QUEUE_SIZE } from './offlineSyncOutcome'

export const OFFLINE_QUEUE_STORAGE_KEY = 'stl-fieldcompanion-offline-queue-v1'

export const OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE = 'field_inbox.acknowledge'
export const OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH = 'staffarr.clock.punch'
export const CLOCK_QUEUE_TASK_KEY = 'clock:self'
export const CLOCK_QUEUE_PRODUCT_KEY = 'staffarr'

export class OfflineQueueCapacityError extends Error {
  constructor() {
    super(`Offline queue is full (max ${MAX_OFFLINE_QUEUE_SIZE} pending actions).`)
    this.name = 'OfflineQueueCapacityError'
  }
}

export interface QueuedClockPunchPayload {
  eventType: 'clock_in' | 'clock_out'
  eventTimestamp: string
  capturedAt: string | null
  timezone: string
  idempotencyKey: string
  sourceDeviceId?: string | null
  geoPoint?: string | null
  siteRef?: string | null
  locationRef?: string | null
  notes?: string | null
}

export interface QueuedOfflineActionBase {
  idempotencyKey: string
  actionKind: typeof OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE | typeof OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH
  taskKey: string
  productKey: string
  clientCreatedAt: string
  title: string
}

export interface QueuedFieldInboxAcknowledgeAction extends QueuedOfflineActionBase {
  actionKind: typeof OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE
}

export interface QueuedClockPunchAction extends QueuedOfflineActionBase {
  actionKind: typeof OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH
  payload: QueuedClockPunchPayload
}

export type QueuedOfflineAction = QueuedFieldInboxAcknowledgeAction | QueuedClockPunchAction

export interface OfflineQueueSnapshot {
  pending: QueuedOfflineAction[]
  lastSyncedAt: string | null
  lastSyncError: string | null
}

function readRaw(): OfflineQueueSnapshot {
  if (typeof window === 'undefined') {
    return { pending: [], lastSyncedAt: null, lastSyncError: null }
  }

  try {
    const raw = window.localStorage.getItem(OFFLINE_QUEUE_STORAGE_KEY)
    if (!raw) {
      return { pending: [], lastSyncedAt: null, lastSyncError: null }
    }
    const parsed = JSON.parse(raw) as OfflineQueueSnapshot
    return {
      pending: Array.isArray(parsed.pending) ? parsed.pending : [],
      lastSyncedAt: parsed.lastSyncedAt ?? null,
      lastSyncError: parsed.lastSyncError ?? null,
    }
  } catch {
    return { pending: [], lastSyncedAt: null, lastSyncError: null }
  }
}

function writeRaw(snapshot: OfflineQueueSnapshot): void {
  window.localStorage.setItem(OFFLINE_QUEUE_STORAGE_KEY, JSON.stringify(snapshot))
}

export function getOfflineQueueSnapshot(): OfflineQueueSnapshot {
  return readRaw()
}

export function enqueueFieldInboxAcknowledge(input: {
  taskKey: string
  productKey: string
  title: string
}): QueuedOfflineAction {
  const snapshot = readRaw()
  const existing = snapshot.pending.find(
    (item) => item.taskKey === input.taskKey && item.productKey === input.productKey,
  )
  if (existing) {
    return existing
  }

  if (snapshot.pending.length >= MAX_OFFLINE_QUEUE_SIZE) {
    throw new OfflineQueueCapacityError()
  }

  const action: QueuedOfflineAction = {
    idempotencyKey: crypto.randomUUID(),
    actionKind: OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE,
    taskKey: input.taskKey,
    productKey: input.productKey,
    clientCreatedAt: new Date().toISOString(),
    title: input.title,
  }

  snapshot.pending = [...snapshot.pending, action]
  writeRaw(snapshot)
  return action
}

export function enqueueClockPunch(input: {
  eventType: 'clock_in' | 'clock_out'
  eventTimestamp: string
  capturedAt: string | null
  timezone: string
  sourceDeviceId?: string | null
  geoPoint?: string | null
  siteRef?: string | null
  locationRef?: string | null
  notes?: string | null
}): QueuedClockPunchAction {
  const snapshot = readRaw()
  if (snapshot.pending.length >= MAX_OFFLINE_QUEUE_SIZE) {
    throw new OfflineQueueCapacityError()
  }

  const idempotencyKey = crypto.randomUUID()
  const action: QueuedClockPunchAction = {
    idempotencyKey,
    actionKind: OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH,
    taskKey: CLOCK_QUEUE_TASK_KEY,
    productKey: CLOCK_QUEUE_PRODUCT_KEY,
    clientCreatedAt: new Date().toISOString(),
    title: input.eventType === 'clock_in' ? 'Clock in' : 'Clock out',
    payload: {
      eventType: input.eventType,
      eventTimestamp: input.eventTimestamp,
      capturedAt: input.capturedAt,
      timezone: input.timezone,
      idempotencyKey,
      sourceDeviceId: input.sourceDeviceId ?? null,
      geoPoint: input.geoPoint ?? null,
      siteRef: input.siteRef ?? null,
      locationRef: input.locationRef ?? null,
      notes: input.notes ?? null,
    },
  }

  snapshot.pending = [...snapshot.pending, action]
  writeRaw(snapshot)
  return action
}

export function removePendingByIdempotencyKeys(keys: ReadonlySet<string>): void {
  const snapshot = readRaw()
  snapshot.pending = snapshot.pending.filter((item) => !keys.has(item.idempotencyKey))
  writeRaw(snapshot)
}

export function markSyncSuccess(syncedKeys: ReadonlySet<string>): void {
  const snapshot = readRaw()
  snapshot.pending = snapshot.pending.filter((item) => !syncedKeys.has(item.idempotencyKey))
  snapshot.lastSyncedAt = new Date().toISOString()
  snapshot.lastSyncError = null
  writeRaw(snapshot)
}

export function markSyncPartial(input: {
  syncedKeys: ReadonlySet<string>
  permanentRejectedKeys: ReadonlySet<string>
  lastSyncError: string | null
}): void {
  const snapshot = readRaw()
  snapshot.pending = snapshot.pending.filter(
    (item) =>
      !input.syncedKeys.has(item.idempotencyKey)
      && !input.permanentRejectedKeys.has(item.idempotencyKey),
  )
  if (input.syncedKeys.size > 0) {
    snapshot.lastSyncedAt = new Date().toISOString()
  }

  snapshot.lastSyncError = input.lastSyncError
  writeRaw(snapshot)
}

export function markSyncFailure(message: string): void {
  const snapshot = readRaw()
  snapshot.lastSyncError = message
  writeRaw(snapshot)
}

export function clearOfflineQueueForTests(): void {
  if (typeof window !== 'undefined') {
    window.localStorage.removeItem(OFFLINE_QUEUE_STORAGE_KEY)
  }
}
