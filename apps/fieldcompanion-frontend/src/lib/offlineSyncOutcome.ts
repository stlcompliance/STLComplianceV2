import type { FieldCompanionOfflineActionRejectedItem } from '../api/types'

import { FieldCompanionFieldValidationReasonCodes } from './FieldCompanionValidationReasonCodes'

export const MAX_OFFLINE_QUEUE_SIZE = 50

export function isRetryableOfflineRejection(reasonCode: string): boolean {
  return reasonCode === FieldCompanionFieldValidationReasonCodes.InboxUnavailable
}

export function summarizeOfflineSyncOutcome(input: {
  accepted: number
  duplicates: number
  rejected: number
}): string | null {
  const syncedCount = input.accepted + input.duplicates
  if (syncedCount === 0 && input.rejected === 0) {
    return null
  }

  const parts: string[] = []
  if (syncedCount > 0) {
    parts.push(
      syncedCount === 1
        ? '1 offline action synced'
        : `${syncedCount} offline actions synced`,
    )
  }

  if (input.rejected > 0) {
    parts.push(
      input.rejected === 1 ? '1 could not sync' : `${input.rejected} could not sync`,
    )
  }

  return parts.join('; ') + '.'
}

export function partitionRejectedItems(items: FieldCompanionOfflineActionRejectedItem[]): {
  retryableKeys: Set<string>
  permanentKeys: Set<string>
} {
  const retryableKeys = new Set<string>()
  const permanentKeys = new Set<string>()

  for (const item of items) {
    if (isRetryableOfflineRejection(item.reasonCode)) {
      retryableKeys.add(item.idempotencyKey)
    } else {
      permanentKeys.add(item.idempotencyKey)
    }
  }

  return { retryableKeys, permanentKeys }
}
