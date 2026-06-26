import {
  FieldCompanionAuthReasonCodes,
  FieldCompanionFieldValidationReasonCodes,
  FieldCompanionLaunchDenialCodes,
  FieldCompanionOfflineActionReasonCodes,
  FieldCompanionScanReasonCodes,
} from './FieldCompanionValidationReasonCodes'

const PLAIN_MESSAGES: Record<string, string> = {
  [FieldCompanionFieldValidationReasonCodes.InvalidTaskKey]:
    'This task reference is not recognized. Scan or select a task from your field inbox.',
  [FieldCompanionFieldValidationReasonCodes.ProductMismatch]:
    'The product selected does not match this task.',
  [FieldCompanionFieldValidationReasonCodes.UnsupportedSubmissionKind]:
    'That type of field submission is not supported.',
  [FieldCompanionFieldValidationReasonCodes.AccessUnavailable]:
    'Your account does not have permission to work on tasks for this product.',
  [FieldCompanionFieldValidationReasonCodes.NotInInbox]:
    'This task is not in your field inbox. Refresh your inbox or ask a supervisor to reassign the work.',
  [FieldCompanionFieldValidationReasonCodes.EvidenceUnsupported]:
    'Evidence capture is not available for this task yet. Open the task in the web app to continue.',
  [FieldCompanionFieldValidationReasonCodes.DvirUnsupported]:
    'DVIR submission is not available for this task. Open the trip in RoutArr to complete inspection paperwork.',
  [FieldCompanionFieldValidationReasonCodes.InspectionUnsupported]:
    'Inspection capture is not available for this task. Open the inspection in MaintainArr to continue.',
  [FieldCompanionFieldValidationReasonCodes.WorkOrderUnsupported]:
    'Work order updates are not available for this task. Open the work order in MaintainArr to continue.',
  [FieldCompanionFieldValidationReasonCodes.ReceivingUnsupported]:
    'Receiving updates are not available for this task. Open the receipt in the web app to continue.',
  [FieldCompanionFieldValidationReasonCodes.InboxUnavailable]:
    'We could not load the product inbox to verify this task. Try again when connectivity improves.',
  [FieldCompanionScanReasonCodes.InvalidPayload]:
    'The scan did not contain a recognizable field task.',
  [FieldCompanionScanReasonCodes.AccessUnavailable]:
    'You do not have permission to open tasks for this product.',
  [FieldCompanionScanReasonCodes.NotInInbox]: 'This task is not in your field inbox.',
  [FieldCompanionAuthReasonCodes.AccessUnavailable]:
    'Field Companion is not available for your current account or tenant context.',
  [FieldCompanionAuthReasonCodes.Unauthorized]: 'Sign in again to continue field work.',
  'fieldcompanion.not_available': 'Field Companion is not available for this tenant right now.',
  [FieldCompanionOfflineActionReasonCodes.IdempotencyRequired]:
    'Each offline action needs a unique idempotency key before sync.',
  [FieldCompanionOfflineActionReasonCodes.TaskRequired]:
    'Each offline action must include a task and product reference.',
  [FieldCompanionOfflineActionReasonCodes.UnsupportedKind]:
    'Only field inbox acknowledgments can be queued offline right now.',
  [FieldCompanionLaunchDenialCodes.Denied]: 'Product launch is not permitted.',
  [FieldCompanionLaunchDenialCodes.TenantSuspended]:
    'This tenant is suspended. Contact your administrator.',
  [FieldCompanionLaunchDenialCodes.AccessUnavailable]:
    'This product is unavailable for your tenant right now.',
  [FieldCompanionLaunchDenialCodes.AvailabilityInactive]:
    'This product is unavailable for your tenant right now.',
  [FieldCompanionLaunchDenialCodes.ProfileMissing]:
    'Launch is not configured for this product yet.',
  product_url_missing: 'This product API is not configured for field inbox aggregation.',
  upstream_unreachable:
    'Could not reach the product inbox. Try again when connectivity improves.',
}

function normalizeReasonCode(code: string): string {
  const normalized = code.trim()
  if (
    normalized === 'product_not_available' ||
    normalized === 'product_unavailable' ||
    normalized === 'launch.product_unavailable' ||
    normalized === 'not_available' ||
    normalized === 'handoff.not_available'
  ) {
    return FieldCompanionLaunchDenialCodes.AccessUnavailable
  }

  if (
    normalized === 'launch.product_unavailable' ||
    normalized === 'availability_revoked' ||
    normalized === 'launch.availability_revoked'
  ) {
    return FieldCompanionLaunchDenialCodes.AvailabilityInactive
  }

  return normalized
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
  const normalized = normalizeReasonCode(code)
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
