export const OFFLINE_QUEUE_STALE_AFTER_MS = 24 * 60 * 60_000

export interface OfflineQueueFreshnessInput {
  pending: ReadonlyArray<{ clientCreatedAt: string }>
  lastSyncedAt: string | null
  now?: Date
}

export interface OfflineQueueFreshnessSnapshot {
  oldestPendingAt: string | null
  oldestPendingAgeLabel: string | null
  lastSyncedAgeLabel: string | null
  isStale: boolean
  staleThresholdLabel: string
}

function formatDuration(ms: number): string {
  if (ms <= 0) {
    return 'just now'
  }

  const totalMinutes = Math.floor(ms / 60_000)
  const days = Math.floor(totalMinutes / (60 * 24))
  const hours = Math.floor((totalMinutes % (60 * 24)) / 60)
  const minutes = totalMinutes % 60

  if (days > 0) {
    return `${days}d ${hours}h`
  }

  if (hours > 0) {
    return `${hours}h ${minutes}m`
  }

  if (minutes > 0) {
    return `${minutes}m`
  }

  return 'just now'
}

export function formatOfflineQueueAge(value: string, now = new Date()): string {
  return formatDuration(now.getTime() - new Date(value).getTime())
}

export function summarizeOfflineQueueFreshness(
  input: OfflineQueueFreshnessInput,
): OfflineQueueFreshnessSnapshot {
  const now = input.now ?? new Date()
  const oldestPending = input.pending
    .map((item) => item.clientCreatedAt)
    .filter((value) => typeof value === 'string' && value.length > 0)
    .sort((left, right) => new Date(left).getTime() - new Date(right).getTime())[0] ?? null

  const oldestPendingAgeLabel = oldestPending ? formatOfflineQueueAge(oldestPending, now) : null
  const lastSyncedAgeLabel = input.lastSyncedAt ? formatOfflineQueueAge(input.lastSyncedAt, now) : null
  const oldestPendingAgeMs = oldestPending ? now.getTime() - new Date(oldestPending).getTime() : 0

  return {
    oldestPendingAt: oldestPending,
    oldestPendingAgeLabel,
    lastSyncedAgeLabel,
    isStale: oldestPendingAgeMs >= OFFLINE_QUEUE_STALE_AFTER_MS,
    staleThresholdLabel: '24h',
  }
}
