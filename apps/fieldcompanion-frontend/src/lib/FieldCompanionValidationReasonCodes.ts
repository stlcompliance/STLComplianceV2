export const FieldCompanionFieldValidationReasonCodes = {
  InvalidTaskKey: 'fieldcompanion.field_task.invalid_key',
  ProductMismatch: 'fieldcompanion.field_task.product_mismatch',
  UnsupportedSubmissionKind: 'fieldcompanion.field_task.unsupported_submission_kind',
  AccessUnavailable: 'fieldcompanion.field_task.not_available',
  NotInInbox: 'fieldcompanion.field_task.not_in_inbox',
  EvidenceUnsupported: 'fieldcompanion.field_evidence.unsupported_task',
  DvirUnsupported: 'fieldcompanion.field_dvir.unsupported_task',
  InspectionUnsupported: 'fieldcompanion.field_inspection.unsupported_task',
  WorkOrderUnsupported: 'fieldcompanion.field_work_order.unsupported_task',
  ReceivingUnsupported: 'fieldcompanion.field_receiving.unsupported_task',
  InboxUnavailable: 'fieldcompanion.field_task.inbox_unavailable',
} as const

export const FieldCompanionScanReasonCodes = {
  InvalidPayload: 'scan.invalid_payload',
  AccessUnavailable: 'scan.not_available',
  NotInInbox: 'scan.not_in_inbox',
} as const

export const FieldCompanionLaunchDenialCodes = {
  Denied: 'launch.denied',
  TenantSuspended: 'tenant_suspended',
  AccessUnavailable: 'not_available',
  AvailabilityInactive: 'availability_inactive',
  ProfileMissing: 'profile_missing',
} as const

export const FieldCompanionOfflineActionReasonCodes = {
  IdempotencyRequired: 'fieldcompanion.offline_actions.idempotency_required',
  TaskRequired: 'fieldcompanion.offline_actions.task_required',
  UnsupportedKind: 'fieldcompanion.offline_actions.unsupported_kind',
} as const

export const FieldCompanionAuthReasonCodes = {
  AccessUnavailable: 'auth.not_available',
  Unauthorized: 'auth.unauthorized',
} as const
