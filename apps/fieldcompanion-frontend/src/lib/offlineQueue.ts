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
}

interface QueuedClockPunchSensitivePayload {
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

const queuedClockPunchSensitivePayloads = new Map<string, QueuedClockPunchSensitivePayload>()

function cacheClockPunchSensitivePayload(
  idempotencyKey: string,
  payload: QueuedClockPunchSensitivePayload,
): void {
  const normalizedPayload: QueuedClockPunchSensitivePayload = {}

  if (payload.sourceDeviceId != null) {
    normalizedPayload.sourceDeviceId = payload.sourceDeviceId
  }
  if (payload.geoPoint != null) {
    normalizedPayload.geoPoint = payload.geoPoint
  }
  if (payload.siteRef != null) {
    normalizedPayload.siteRef = payload.siteRef
  }
  if (payload.locationRef != null) {
    normalizedPayload.locationRef = payload.locationRef
  }
  if (payload.notes != null) {
    normalizedPayload.notes = payload.notes
  }

  if (Object.keys(normalizedPayload).length === 0) {
    queuedClockPunchSensitivePayloads.delete(idempotencyKey)
    return
  }

  queuedClockPunchSensitivePayloads.set(idempotencyKey, normalizedPayload)
}

function clearClockPunchSensitivePayloads(keys: ReadonlySet<string>): void {
  for (const key of keys) {
    queuedClockPunchSensitivePayloads.delete(key)
  }
}

function getClockPunchSensitivePayload(
  idempotencyKey: string,
): QueuedClockPunchSensitivePayload | undefined {
  return queuedClockPunchSensitivePayloads.get(idempotencyKey)
}

function isClockPunchPayload(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function sanitizePendingAction(
  item: unknown,
): { action: QueuedOfflineAction | null; wasSanitized: boolean } {
  if (typeof item !== 'object' || item === null) {
    return { action: null, wasSanitized: true }
  }

  const raw = item as Partial<QueuedOfflineAction> & {
    payload?: unknown
  }

  if (raw.actionKind !== OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH) {
    if (
      raw.actionKind !== OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE
      || typeof raw.idempotencyKey !== 'string'
      || typeof raw.taskKey !== 'string'
      || typeof raw.productKey !== 'string'
      || typeof raw.clientCreatedAt !== 'string'
      || typeof raw.title !== 'string'
    ) {
      return { action: null, wasSanitized: true }
    }

    return {
      action: {
        idempotencyKey: raw.idempotencyKey,
        actionKind: OFFLINE_ACTION_FIELD_INBOX_ACKNOWLEDGE,
        taskKey: raw.taskKey,
        productKey: raw.productKey,
        clientCreatedAt: raw.clientCreatedAt,
        title: raw.title,
      },
      wasSanitized: false,
    }
  }

  if (!isClockPunchPayload(raw.payload)) {
    return { action: null, wasSanitized: true }
  }

  const payload = raw.payload
  const idempotencyKey = typeof raw.idempotencyKey === 'string' ? raw.idempotencyKey : ''
  const sanitizedPayload: QueuedClockPunchPayload = {
    eventType: payload.eventType === 'clock_out' ? 'clock_out' : 'clock_in',
    eventTimestamp: typeof payload.eventTimestamp === 'string' ? payload.eventTimestamp : new Date().toISOString(),
    capturedAt: typeof payload.capturedAt === 'string' || payload.capturedAt === null ? payload.capturedAt : null,
    timezone: typeof payload.timezone === 'string' ? payload.timezone : 'UTC',
    idempotencyKey: typeof payload.idempotencyKey === 'string' && payload.idempotencyKey.length > 0
      ? payload.idempotencyKey
      : idempotencyKey,
  }

  const sensitivePayload: QueuedClockPunchSensitivePayload = {}
  if (typeof payload.sourceDeviceId === 'string' || payload.sourceDeviceId === null) {
    sensitivePayload.sourceDeviceId = payload.sourceDeviceId
  }
  if (typeof payload.geoPoint === 'string' || payload.geoPoint === null) {
    sensitivePayload.geoPoint = payload.geoPoint
  }
  if (typeof payload.siteRef === 'string' || payload.siteRef === null) {
    sensitivePayload.siteRef = payload.siteRef
  }
  if (typeof payload.locationRef === 'string' || payload.locationRef === null) {
    sensitivePayload.locationRef = payload.locationRef
  }
  if (typeof payload.notes === 'string' || payload.notes === null) {
    sensitivePayload.notes = payload.notes
  }
  if (sanitizedPayload.idempotencyKey) {
    cacheClockPunchSensitivePayload(sanitizedPayload.idempotencyKey, sensitivePayload)
  }

  const wasSanitized =
    payload.eventType !== sanitizedPayload.eventType
    || payload.eventTimestamp !== sanitizedPayload.eventTimestamp
    || payload.capturedAt !== sanitizedPayload.capturedAt
    || payload.timezone !== sanitizedPayload.timezone
    || payload.idempotencyKey !== sanitizedPayload.idempotencyKey
    || typeof payload.sourceDeviceId === 'string'
    || payload.sourceDeviceId === null
    || typeof payload.geoPoint === 'string'
    || payload.geoPoint === null
    || typeof payload.siteRef === 'string'
    || payload.siteRef === null
    || typeof payload.locationRef === 'string'
    || payload.locationRef === null
    || typeof payload.notes === 'string'
    || payload.notes === null

  if (
    typeof raw.idempotencyKey !== 'string'
    || typeof raw.taskKey !== 'string'
    || typeof raw.productKey !== 'string'
    || typeof raw.clientCreatedAt !== 'string'
    || typeof raw.title !== 'string'
  ) {
    return { action: null, wasSanitized: true }
  }

  return {
    action: {
      idempotencyKey: raw.idempotencyKey,
      actionKind: OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH,
      taskKey: raw.taskKey,
      productKey: raw.productKey,
      clientCreatedAt: raw.clientCreatedAt,
      title: raw.title,
      payload: sanitizedPayload,
    },
    wasSanitized,
  }
}

function sanitizeSnapshot(snapshot: OfflineQueueSnapshot): {
  snapshot: OfflineQueueSnapshot
  wasSanitized: boolean
} {
  let wasSanitized = false
  const pending: QueuedOfflineAction[] = []

  for (const item of snapshot.pending) {
    const normalized = sanitizePendingAction(item)
    if (normalized.action) {
      pending.push(normalized.action)
    }
    wasSanitized ||= normalized.wasSanitized
  }

  return {
    snapshot: {
      pending,
      lastSyncedAt: snapshot.lastSyncedAt,
      lastSyncError: snapshot.lastSyncError,
    },
    wasSanitized,
  }
}

function readRaw(): OfflineQueueSnapshot {
  if (typeof window === 'undefined') {
    return { pending: [], lastSyncedAt: null, lastSyncError: null }
  }

  try {
    const raw = window.sessionStorage.getItem(OFFLINE_QUEUE_STORAGE_KEY)
    if (!raw) {
      return { pending: [], lastSyncedAt: null, lastSyncError: null }
    }
    const parsed = JSON.parse(raw) as OfflineQueueSnapshot
    const snapshot = {
      pending: Array.isArray(parsed.pending) ? parsed.pending : [],
      lastSyncedAt: parsed.lastSyncedAt ?? null,
      lastSyncError: parsed.lastSyncError ?? null,
    }
    const sanitized = sanitizeSnapshot(snapshot)
    if (sanitized.wasSanitized) {
      writeRaw(sanitized.snapshot)
    }
    return sanitized.snapshot
  } catch {
    return { pending: [], lastSyncedAt: null, lastSyncError: null }
  }
}

function writeRaw(snapshot: OfflineQueueSnapshot): void {
  window.sessionStorage.setItem(OFFLINE_QUEUE_STORAGE_KEY, JSON.stringify(snapshot))
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
    },
  }

  snapshot.pending = [...snapshot.pending, action]
  writeRaw(snapshot)
  return action
}

export function removePendingByIdempotencyKeys(keys: ReadonlySet<string>): void {
  const snapshot = readRaw()
  snapshot.pending = snapshot.pending.filter((item) => !keys.has(item.idempotencyKey))
  clearClockPunchSensitivePayloads(keys)
  writeRaw(snapshot)
}

export function markSyncSuccess(syncedKeys: ReadonlySet<string>): void {
  const snapshot = readRaw()
  snapshot.pending = snapshot.pending.filter((item) => !syncedKeys.has(item.idempotencyKey))
  clearClockPunchSensitivePayloads(syncedKeys)
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
  clearClockPunchSensitivePayloads(new Set([...input.syncedKeys, ...input.permanentRejectedKeys]))
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

export function getQueuedOfflineActionPayload(
  action: QueuedOfflineAction,
): unknown {
  if (action.actionKind !== OFFLINE_ACTION_STAFFARR_CLOCK_PUNCH) {
    return undefined
  }

  return {
    ...action.payload,
    ...getClockPunchSensitivePayload(action.idempotencyKey),
  }
}

export function clearOfflineQueueState(): void {
  if (typeof window !== 'undefined') {
    window.sessionStorage.removeItem(OFFLINE_QUEUE_STORAGE_KEY)
  }
  queuedClockPunchSensitivePayloads.clear()
}

export function clearOfflineQueueForTests(): void {
  clearOfflineQueueState()
}
