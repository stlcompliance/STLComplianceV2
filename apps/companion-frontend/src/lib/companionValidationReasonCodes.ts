export const CompanionFieldValidationReasonCodes = {
  InvalidTaskKey: 'companion.field_task.invalid_key',
  ProductMismatch: 'companion.field_task.product_mismatch',
  UnsupportedSubmissionKind: 'companion.field_task.unsupported_submission_kind',
  NotEntitled: 'companion.field_task.not_entitled',
  NotInInbox: 'companion.field_task.not_in_inbox',
  EvidenceUnsupported: 'companion.field_evidence.unsupported_task',
  DvirUnsupported: 'companion.field_dvir.unsupported_task',
  InspectionUnsupported: 'companion.field_inspection.unsupported_task',
  WorkOrderUnsupported: 'companion.field_work_order.unsupported_task',
  ReceivingUnsupported: 'companion.field_receiving.unsupported_task',
  InboxUnavailable: 'companion.field_task.inbox_unavailable',
} as const

export const CompanionScanReasonCodes = {
  InvalidPayload: 'scan.invalid_payload',
  NotEntitled: 'scan.not_entitled',
  NotInInbox: 'scan.not_in_inbox',
} as const

export const CompanionLaunchDenialCodes = {
  Denied: 'launch.denied',
  TenantSuspended: 'tenant_suspended',
  NotEntitled: 'not_entitled',
  EntitlementInactive: 'entitlement_inactive',
  ProfileMissing: 'profile_missing',
} as const

export const CompanionOfflineActionReasonCodes = {
  IdempotencyRequired: 'companion.offline_actions.idempotency_required',
  TaskRequired: 'companion.offline_actions.task_required',
  UnsupportedKind: 'companion.offline_actions.unsupported_kind',
} as const

export const CompanionAuthReasonCodes = {
  NotEntitled: 'auth.not_entitled',
  Unauthorized: 'auth.unauthorized',
} as const
