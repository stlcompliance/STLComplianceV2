import {
  CompanionAuthReasonCodes,
  CompanionFieldValidationReasonCodes,
  CompanionLaunchDenialCodes,
  CompanionOfflineActionReasonCodes,
  CompanionScanReasonCodes,
} from './companionValidationReasonCodes'

const PLAIN_MESSAGES: Record<string, string> = {
  [CompanionFieldValidationReasonCodes.InvalidTaskKey]:
    'This task reference is not recognized. Scan or select a task from your field inbox.',
  [CompanionFieldValidationReasonCodes.ProductMismatch]:
    'The product selected does not match this task.',
  [CompanionFieldValidationReasonCodes.UnsupportedSubmissionKind]:
    'That type of field submission is not supported.',
  [CompanionFieldValidationReasonCodes.NotEntitled]:
    'Your account is not entitled to work on tasks for this product.',
  [CompanionFieldValidationReasonCodes.NotInInbox]:
    'This task is not in your field inbox. Refresh your inbox or ask a supervisor to reassign the work.',
  [CompanionFieldValidationReasonCodes.EvidenceUnsupported]:
    'Evidence capture is not available for this task yet. Open the task in the owning product app.',
  [CompanionFieldValidationReasonCodes.DvirUnsupported]:
    'DVIR submission is not available for this task. Open the trip in RoutArr to complete inspection paperwork.',
  [CompanionFieldValidationReasonCodes.InspectionUnsupported]:
    'Inspection capture is not available for this task. Open the inspection in MaintainArr to continue.',
  [CompanionFieldValidationReasonCodes.WorkOrderUnsupported]:
    'Work order updates are not available for this task. Open the work order in MaintainArr to continue.',
  [CompanionFieldValidationReasonCodes.ReceivingUnsupported]:
    'Receiving updates are not available for this task. Open the receipt in SupplyArr to continue.',
  [CompanionFieldValidationReasonCodes.InboxUnavailable]:
    'We could not load the product inbox to verify this task. Try again when connectivity improves.',
  [CompanionScanReasonCodes.InvalidPayload]:
    'The scan did not contain a recognizable field task.',
  [CompanionScanReasonCodes.NotEntitled]:
    'You are not entitled to open tasks for this product.',
  [CompanionScanReasonCodes.NotInInbox]: 'This task is not in your field inbox.',
  [CompanionAuthReasonCodes.NotEntitled]:
    'Companion access requires companion or field-product entitlement.',
  [CompanionAuthReasonCodes.Unauthorized]: 'Sign in again to continue field work.',
  [CompanionOfflineActionReasonCodes.IdempotencyRequired]:
    'Each offline action needs a unique idempotency key before sync.',
  [CompanionOfflineActionReasonCodes.TaskRequired]:
    'Each offline action must include a task and product reference.',
  [CompanionOfflineActionReasonCodes.UnsupportedKind]:
    'Only field inbox acknowledgments can be queued offline right now.',
  [CompanionLaunchDenialCodes.Denied]: 'Product launch is not permitted.',
  [CompanionLaunchDenialCodes.TenantSuspended]:
    'This tenant is suspended. Contact your administrator.',
  [CompanionLaunchDenialCodes.NotEntitled]:
    'Your account is not entitled to open this product.',
  [CompanionLaunchDenialCodes.EntitlementInactive]:
    'This product entitlement is inactive for your tenant.',
  [CompanionLaunchDenialCodes.ProfileMissing]:
    'Launch is not configured for this product yet.',
  product_url_missing: 'This product API is not configured for field inbox aggregation.',
  upstream_unreachable:
    'Could not reach the product inbox. Try again when connectivity improves.',
}

const BLOCKED_TASK_NEXT_STEPS: Record<string, string> = {
  'pre-trip dvir required':
    'Complete the pre-trip DVIR below or open the trip in RoutArr.',
  'post-trip dvir required':
    'Complete the post-trip DVIR below or open the trip in RoutArr.',
  'pre-trip and post-trip dvir required':
    'Complete both DVIR phases below or open the trip in RoutArr.',
  'evidence required': 'Upload required evidence below or open the assignment in TrainArr.',
}

export interface DeniedReasonInput {
  reasonCode?: string | null
  reasonMessage?: string | null
}

export function reasonCodeToPlainMessage(code: string, fallback?: string): string {
  const normalized = code.trim()
  if (!normalized) {
    return fallback ?? 'This field action is not allowed.'
  }

  return PLAIN_MESSAGES[normalized] ?? fallback ?? 'This field action is not allowed.'
}

export function resolveDeniedReason(
  input: DeniedReasonInput,
  fallback: string,
): string {
  if (input.reasonMessage?.trim()) {
    return input.reasonMessage.trim()
  }

  if (input.reasonCode?.trim()) {
    return reasonCodeToPlainMessage(input.reasonCode, fallback)
  }

  return fallback
}

export function formatBlockedTaskReason(blockedReason: string): string {
  const trimmed = blockedReason.trim()
  if (!trimmed) {
    return 'This task is blocked until required work is completed.'
  }

  const hint = BLOCKED_TASK_NEXT_STEPS[trimmed.toLowerCase()]
  if (hint) {
    return `${trimmed} ${hint}`
  }

  return trimmed
}

export function formatInboxSourceError(
  productKey: string,
  errorCode: string | null | undefined,
  errorMessage: string | null | undefined,
): string {
  if (errorMessage?.trim()) {
    return errorMessage.trim()
  }

  if (errorCode?.trim()) {
    return reasonCodeToPlainMessage(
      errorCode,
      `${productKey} field inbox could not be loaded.`,
    )
  }

  return `${productKey} field inbox could not be loaded.`
}
