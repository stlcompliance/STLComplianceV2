export interface HandoffSessionResponse {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  entitlements: string[]
  themePreference?: string | null
  callbackUrl: string | null
}

export interface StaffArrMeResponse {
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasStaffArrEntitlement: boolean
  primaryOrgUnitName: string | null
  jobTitle: string | null
  entitlements: string[]
}

export interface StaffArrFieldOptionResponse {
  value: string
  label: string
  hint: string | null
  owner: string
  sourceOfTruth: string
}

export interface StaffArrFieldDefinitionResponse {
  key: string
  label: string
  control: string
  required: boolean
  owner: string
  sourceOfTruth: string
  options: StaffArrFieldOptionResponse[]
}

export interface StaffArrFieldsetResponse {
  key: string
  label: string
  entityType: string
  purpose: string
  fields: StaffArrFieldDefinitionResponse[]
}

export interface EmploymentApplicationControlOptionResponse {
  value: string
  label: string
  hint: string | null
}

export interface EmploymentApplicationTargetFieldResponse {
  value: string
  label: string
  stage: string
  hint: string | null
  owner: string
  sourceOfTruth: string
}

export interface EmploymentApplicationTargetFieldGroupResponse {
  key: string
  label: string
  fields: EmploymentApplicationTargetFieldResponse[]
}

export interface EmploymentApplicationBuilderCatalogResponse {
  controlOptions: EmploymentApplicationControlOptionResponse[]
  targetFieldGroups: EmploymentApplicationTargetFieldGroupResponse[]
}

export interface RecruitingRequisitionResponse {
  id: string
  requisitionNumber: string
  title: string
  jobCode: string
  jobFamily: string
  departmentRef: string | null
  siteRef: string | null
  locationRef: string | null
  hiringManagerPersonId: string | null
  recruiterPersonId: string | null
  status: string
  headcountRequested: number
  filledCount: number
  openDate: string | null
  targetStartDate: string | null
  sourceProductKey: string | null
  sourceRef: string | null
  createdAt: string
  updatedAt: string
}

export interface UpsertRecruitingRequisitionRequest {
  requisitionNumber: string
  title: string
  jobCode: string
  jobFamily: string
  departmentRef?: string | null
  siteRef?: string | null
  locationRef?: string | null
  hiringManagerPersonId?: string | null
  recruiterPersonId?: string | null
  status: string
  headcountRequested: number
  filledCount: number
  openDate?: string | null
  targetStartDate?: string | null
  sourceProductKey?: string | null
  sourceRef?: string | null
}

export interface RecruitingCandidateResponse {
  id: string
  recruitingRequisitionId: string | null
  employmentApplicationSubmissionId: string | null
  personId: string | null
  candidateName: string
  candidateEmail: string
  candidatePhone: string | null
  sourceType: string
  stage: string
  status: string
  backgroundCheckStatus: string | null
  drugScreenStatus: string | null
  physicalStatus: string | null
  offerStatus: string | null
  score: number | null
  notes: string | null
  sourceProductKey: string | null
  sourceRef: string | null
  createdAt: string
  updatedAt: string
}

export interface UpsertRecruitingCandidateRequest {
  recruitingRequisitionId?: string | null
  employmentApplicationSubmissionId?: string | null
  personId?: string | null
  candidateName: string
  candidateEmail: string
  candidatePhone?: string | null
  sourceType: string
  stage: string
  status: string
  backgroundCheckStatus?: string | null
  drugScreenStatus?: string | null
  physicalStatus?: string | null
  offerStatus?: string | null
  score?: number | null
  notes?: string | null
  sourceProductKey?: string | null
  sourceRef?: string | null
}

export interface RecruitingInterviewStageResponse {
  id: string
  recruitingCandidateId: string
  stageName: string
  status: string
  scheduledAt: string | null
  completedAt: string | null
  interviewerPersonId: string | null
  score: number | null
  recommendation: string | null
  notes: string | null
  createdAt: string
  updatedAt: string
}

export interface UpsertRecruitingInterviewStageRequest {
  recruitingCandidateId: string
  stageName: string
  status: string
  scheduledAt?: string | null
  completedAt?: string | null
  interviewerPersonId?: string | null
  score?: number | null
  recommendation?: string | null
  notes?: string | null
}

export interface RecruitingOfferResponse {
  id: string
  recruitingCandidateId: string
  status: string
  title: string
  payBasis: string
  annualSalary: number | null
  hourlyRate: number | null
  startDate: string | null
  approvedAt: string | null
  approvedByPersonId: string | null
  acceptedAt: string | null
  declinedAt: string | null
  notes: string | null
  sourceProductKey: string | null
  sourceRef: string | null
  createdAt: string
  updatedAt: string
}

export interface UpsertRecruitingOfferRequest {
  recruitingCandidateId: string
  status: string
  title: string
  payBasis: string
  annualSalary?: number | null
  hourlyRate?: number | null
  startDate?: string | null
  approvedAt?: string | null
  approvedByPersonId?: string | null
  acceptedAt?: string | null
  declinedAt?: string | null
  notes?: string | null
  sourceProductKey?: string | null
  sourceRef?: string | null
}

export interface LaunchHandoffResponse {
  handoffCode: string
  handoffId: string
  expiresAt: string
  launchUrl: string
}

export interface StaffArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasStaffArrEntitlement: boolean
  entitlements: string[]
}

export interface MePortalPermissionSummaryResponse {
  permissionCount: number
  permissionSummaries: string[]
}

export interface MePortalCertificationSummaryResponse {
  activeCount: number
  expiringSoonCount: number
  missingRequirementCount: number
  highlights: PersonCertificationResponse[]
}

export interface MePortalReadinessSummaryResponse {
  readinessStatus: string
  readinessBasis: string
  blockerMessages: string[]
}

export interface MePortalOnboardingSummaryResponse {
  overallStatus: string
  completedSteps: number
  totalSteps: number
  blockedSteps: number
}

export interface MePortalSummaryResponse {
  session: StaffArrMeResponse
  profile: PersonLookupResponse
  readiness: MePortalReadinessSummaryResponse
  certifications: MePortalCertificationSummaryResponse
  permissions: MePortalPermissionSummaryResponse
  onboarding: MePortalOnboardingSummaryResponse | null
  directReportCount: number
  directReportsPreview: SubordinateSummaryResponse[]
  productAccess: string[]
}

export interface StaffArrPersonIntegrationSummaryResponse {
  person: StaffPersonDetailResponse
  readiness: PersonReadinessResponse
  permissionProjection: EffectivePermissionProjectionResponse
  qualificationsSnapshot: TrainarrPersonTrainingHistoryResponse
  historySummary: PersonnelHistorySummaryResponse
  activeRestrictions: ReadinessOverrideResponse[]
}

export interface PersonnelUpdateRequestResponse {
  requestId: string
  personId: string
  requestType: string
  status: string
  fieldKey: string
  currentValue: string | null
  requestedValue: string
  details: string | null
  submittedByUserId: string
  submittedAt: string
  reviewedByUserId: string | null
  reviewedAt: string | null
  reviewNotes: string | null
  createdAt: string
  updatedAt: string
}

export interface SubmitPersonnelUpdateRequest {
  requestType: string
  fieldKey: string
  currentValue: string | null
  requestedValue: string
  details: string | null
}

export interface ReviewPersonnelUpdateRequest {
  decision: 'approve' | 'deny'
  reviewNotes: string | null
  applyToProfile?: boolean
}

export interface PersonnelUpdateRequestReviewResponse {
  request: PersonnelUpdateRequestResponse
  appliedToProfile: boolean
}

export interface MyTeamMemberResponse {
  summary: SubordinateSummaryResponse
  readinessStatus: string
  blockerCount: number
  missingCertificationCount: number
  expiringCertificationCount: number
  openIncidentCount: number
  pendingUpdateRequestCount: number
  pendingTrainingBlockerCount: number
}

export interface MyTeamDashboardResponse {
  directReportCount: number
  notReadyCount: number
  missingCertificationCount: number
  expiringCertificationCount: number
  openIncidentCount: number
  pendingUpdateRequestCount: number
  onboardingInProgressCount: number
  pendingTrainingBlockerCount: number
  members: MyTeamMemberResponse[]
  pendingUpdateRequests: PersonnelUpdateRequestResponse[]
}

export interface StaffPersonSummaryResponse {
  personId: string
  externalUserId: string | null
  givenName: string
  familyName: string
  displayName: string
  primaryEmail: string
  employmentStatus: string
  primaryOrgUnitId: string | null
  primaryOrgUnitName: string | null
  managerPersonId: string | null
  jobTitle: string | null
  preferredName?: string | null
  workRelationshipType?: string | null
  employmentType?: string | null
  workerCategory?: string | null
  flsaStatus?: string | null
  positionNumber?: string | null
  currentEmploymentAction?: string | null
  currentEmploymentActionAt?: string | null
  leaveStatus?: string | null
  eligibleForRehire?: boolean
  canLoginSnapshot?: boolean
  hasUserAccountSnapshot?: boolean
}

export interface StaffPersonDetailResponse {
  personId: string
  externalUserId: string | null
  givenName: string
  familyName: string
  legalFirstName: string
  legalMiddleName: string | null
  legalLastName: string
  preferredName: string | null
  pronouns: string | null
  displayName: string
  primaryEmail: string
  alternateEmail: string | null
  primaryPhone: string | null
  alternatePhone: string | null
  workPhone: string | null
  employmentStatus: string
  workRelationshipType: string | null
  employmentType: string | null
  workerCategory: string | null
  flsaStatus: string | null
  positionNumber: string | null
  currentEmploymentAction: string | null
  currentEmploymentActionAt: string | null
  leaveStatus: string | null
  eligibleForRehire: boolean
  primaryOrgUnitId: string | null
  primaryOrgUnitName: string | null
  managerPersonId: string | null
  jobTitle: string | null
  startDate: string | null
  expectedStartDate: string | null
  homeBaseLocationId: string | null
  homeBaseLocationName: string | null
  canLoginSnapshot: boolean
  hasUserAccountSnapshot: boolean
  createdAt: string
  updatedAt: string
}

export interface PersonLookupOrgAssignmentResponse {
  assignmentId: string
  siteOrgUnitId: string
  siteName: string
  departmentOrgUnitId: string
  departmentName: string
  teamOrgUnitId: string
  teamName: string
  positionOrgUnitId: string
  positionName: string
  assignmentPath: string
  status?: OrgUnitAssignmentStatus
  isPrimary?: boolean
  effectiveAt?: string | null
  endsAt?: string | null
  reason?: string | null
}

export interface PersonLookupPlacementResponse {
  primaryOrgUnitId: string | null
  primaryOrgUnitName: string | null
  primaryOrgUnitType: string | null
  managerPersonId: string | null
  managerDisplayName: string | null
  activeAssignments: PersonLookupOrgAssignmentResponse[]
}

export interface PersonLookupResponse {
  personId: string
  externalUserId: string | null
  givenName: string
  familyName: string
  displayName: string
  primaryEmail: string
  employmentStatus: string
  jobTitle: string | null
  workPhone: string | null
  placement: PersonLookupPlacementResponse
  lookedUpAt: string
}

export type OrgUnitType =
  | 'company'
  | 'division'
  | 'region'
  | 'business_unit'
  | 'cost_center'
  | 'site'
  | 'department'
  | 'team'
  | 'position'
  | 'other'

export type OrgUnitStatus = 'planned' | 'active' | 'inactive' | 'archived'

export type LocationStatus = 'planned' | 'active' | 'inactive' | 'restricted' | 'archived'

export type LocationAllowedProductUsage =
  | 'maintainarr'
  | 'loadarr'
  | 'routarr'
  | 'trainarr'
  | 'staffarr'
  | 'compliancecore'
  | 'all'

export type LocationType =
  | 'site'
  | 'building'
  | 'warehouse'
  | 'dock'
  | 'room'
  | 'yard'
  | 'parts_room'
  | 'staging_area'
  | 'quarantine_area'
  | 'inspection_hold'
  | 'receiving_staging'
  | 'putaway_queue'
  | 'maintenance_handoff'
  | 'service_counter'
  | 'technician_pickup'
  | 'service_truck'
  | 'shelf'
  | 'bin'
  | 'parking_area'
  | 'work_cell'
  | 'production_line'
  | 'office'
  | 'training_room'
  | 'break_room'
  | 'restricted_area'
  | 'company'
  | 'division'
  | 'region'
  | 'business_unit'
  | 'cost_center'
  | 'department'
  | 'team'
  | 'position'
  | 'other'

export type OrgUnitSiteType =
  | 'office'
  | 'warehouse'
  | 'plant'
  | 'shop'
  | 'yard'
  | 'terminal'
  | 'customer_embedded'
  | 'mixed'
  | 'other'

export type OrgUnitTeamType =
  | 'operational'
  | 'maintenance'
  | 'warehouse'
  | 'dispatch'
  | 'safety'
  | 'quality'
  | 'training'
  | 'admin'
  | 'project'
  | 'emergency_response'

export type OrgUnitAssignmentStatus = 'planned' | 'active' | 'ended' | 'canceled'

export interface CreateStaffPersonRequest {
  legalFirstName?: string | null
  legalMiddleName?: string | null
  legalLastName?: string | null
  preferredName?: string | null
  pronouns?: string | null
  givenName?: string | null
  familyName?: string | null
  primaryEmail: string
  employmentStatus: string
  workRelationshipType?: string | null
  employmentType?: string | null
  workerCategory?: string | null
  flsaStatus?: string | null
  positionNumber?: string | null
  currentEmploymentAction?: string | null
  currentEmploymentActionAt?: string | null
  leaveStatus?: string | null
  eligibleForRehire?: boolean
  alternateEmail?: string | null
  primaryPhone?: string | null
  alternatePhone?: string | null
  workPhone?: string | null
  startDate?: string | null
  expectedStartDate?: string | null
  primaryOrgUnitId?: string | null
  siteOrgUnitId?: string | null
  departmentOrgUnitId?: string | null
  teamOrgUnitId?: string | null
  positionOrgUnitId?: string | null
  managerPersonId?: string | null
  jobTitle?: string | null
  homeBaseLocationId?: string | null
  canLogin?: boolean
  temporaryPassword?: string | null
}

export interface UpdateStaffPersonRequest {
  legalFirstName?: string | null
  legalMiddleName?: string | null
  legalLastName?: string | null
  preferredName?: string | null
  pronouns?: string | null
  givenName?: string | null
  familyName?: string | null
  primaryEmail: string
  alternateEmail?: string | null
  primaryPhone?: string | null
  alternatePhone?: string | null
  workPhone?: string | null
  workRelationshipType?: string | null
  employmentType?: string | null
  workerCategory?: string | null
  flsaStatus?: string | null
  positionNumber?: string | null
  currentEmploymentAction?: string | null
  currentEmploymentActionAt?: string | null
  leaveStatus?: string | null
  eligibleForRehire?: boolean
  startDate?: string | null
  expectedStartDate?: string | null
  primaryOrgUnitId?: string | null
  siteOrgUnitId?: string | null
  managerPersonId?: string | null
  jobTitle?: string | null
  homeBaseLocationId?: string | null
  canLoginSnapshot?: boolean | null
}

export interface UpdatePersonEmploymentStatusRequest {
  employmentStatus: string
  reason: string | null
}

export interface PersonAccountAccessSummaryResponse {
  personId: string
  workEmail: string
  hasPlatformIdentity: boolean
  hasPlatformLogin: boolean
  accountState:
    | 'no_platform_login'
    | 'login_enabled'
    | 'login_disabled'
    | 'login_locked'
    | 'password_change_required'
    | 'pending_verification'
    | 'invite_pending'
    | 'account_unavailable'
  loginEmail: string | null
  loginEmailMatchesWorkEmail: boolean
  isEnabled: boolean
  isMfaEnabled: boolean
  requiresPasswordChange: boolean
  launchEligible: boolean
  tenantRoleSummary: string | null
  lastLoginAt: string | null
  lastProductLaunchAt: string | null
  integrationAvailable: boolean
  notice: string | null
}

export interface PersonAccountAccessActionResponse {
  summary: PersonAccountAccessSummaryResponse
  message: string
}

export interface ProvisionPersonAccountRequest {
  loginEmail: string
  temporaryPassword: string
  syncWorkEmail?: boolean
}

export interface UpdatePersonLoginEmailRequest {
  loginEmail: string
  syncWorkEmail?: boolean
}

export interface PersonAccountActionRequest {
  reason?: string | null
}

export interface BulkPersonImportRowRequest {
  givenName: string
  familyName: string
  primaryEmail: string
  employmentStatus?: string
  primaryOrgUnitId?: string | null
  managerPersonId?: string | null
  managerEmail?: string | null
  jobTitle?: string | null
}

export interface BulkPersonImportRequest {
  people: BulkPersonImportRowRequest[]
  dryRun?: boolean
}

export interface BulkPersonImportRowResult {
  rowIndex: number
  primaryEmail: string
  status: string
  personId: string | null
  errorCode: string | null
  message: string | null
}

export interface BulkPersonImportResponse {
  importId: string
  dryRun: boolean
  totalRows: number
  createdCount: number
  validatedCount: number
  errorCount: number
  results: BulkPersonImportRowResult[]
}

export interface PersonExportFormatDescriptor {
  key: string
  contentType: string
  fileName: string
  description: string
}

export interface PersonExportManifestResponse {
  packageVersion: string
  csvHeader: string
  formats: PersonExportFormatDescriptor[]
}

export interface PersonExportRowItem {
  personId: string
  givenName: string
  familyName: string
  primaryEmail: string
  employmentStatus: string
  jobTitle: string | null
  managerEmail: string | null
  primaryOrgUnitId: string | null
  primaryOrgUnitName: string | null
  createdAt: string
  updatedAt: string
}

export interface PersonExportResponse {
  exportId: string
  tenantId: string
  generatedAt: string
  personCount: number
  people: PersonExportRowItem[]
}

export interface PersonExportFilters {
  employmentStatus?: string
  orgUnitId?: string
}

export interface PersonExportPresetResponse {
  employmentStatus: string | null
  orgUnitId: string | null
  presetKey: string | null
  updatedAt: string
}

export interface UpsertPersonExportPresetRequest {
  employmentStatus?: string
  orgUnitId?: string | null
  presetKey?: string | null
}

export interface PersonExportScheduleResponse {
  isEnabled: boolean
  intervalHours: number
  lastDeliveredAt: string | null
  updatedAt: string | null
  notificationWebhookUrl: string | null
  notifyOnSuccess: boolean
  notifyOnFailure: boolean
}

export interface UpsertPersonExportScheduleRequest {
  isEnabled: boolean
  intervalHours: number
  notificationWebhookUrl?: string | null
  notifyOnSuccess: boolean
  notifyOnFailure: boolean
}

export interface PersonExportDeliveryNotificationItem {
  notificationId: string
  deliveryRunId: string | null
  eventKind: string
  deliveryStatus: string
  webhookHost: string | null
  httpStatusCode: number | null
  errorMessage: string | null
  exportId: string | null
  personCount: number | null
  attemptedAt: string
}

export interface PersonExportDeliveryNotificationsResponse {
  items: PersonExportDeliveryNotificationItem[]
}

export interface PendingPersonExportDeliveriesResponse {
  asOfUtc: string
  batchSize: number
  items: Array<{
    tenantId: string
    intervalHours: number
    lastDeliveredAt: string | null
  }>
}

export interface PersonExportDeliveryRunItem {
  runId: string
  status: string
  exportId: string
  personCount: number
  intervalHours: number
  skipReason: string | null
  startedAt: string
  completedAt: string
}

export interface PersonExportDeliveryRunsResponse {
  items: PersonExportDeliveryRunItem[]
}

export interface StaffArrWorkerSettingsResponse {
  workerKey: string
  isEnabled: boolean
  scanIntervalMinutes: number
  batchSize: number
  stalenessHours: number | null
  lastRunAt: string | null
  pendingCount: number
}

export interface UpsertStaffArrWorkerSettingsRequest {
  isEnabled: boolean
  scanIntervalMinutes: number
  batchSize: number
  stalenessHours?: number | null
}

export interface PersonDirectorySettingsDto {
  displayNameFormat: string
  preferredNameEnabled: boolean
  employeeNumberLabel: string
  employeeNumberRequired: boolean
  employeeNumberUniquenessScope: string
  profilePhotoEnabled: boolean
  contactVisibilityMode: string
  emergencyContactEnabled: boolean
  personalAddressEnabled: boolean
}

export interface PersonLifecycleSettingsDto {
  defaultPersonStatusOnCreate: string
  requireManagerBeforeActivation: boolean
  requirePositionBeforeActivation: boolean
  requireHomeLocationBeforeActivation: boolean
  allowInactivePeopleToBeAssignedWork: boolean
  rehireMatchBehavior: string
  deactivationReasonRequired: boolean
  autoRemoveRolesOnDeactivation: boolean
  autoEndTeamAssignmentsOnDeactivation: boolean
}

export interface OrgStructureSettingsDto {
  orgHierarchyMode: string
  requireEveryPersonInOrgUnit: boolean
  requireDepartmentUnderSite: boolean
  allowMatrixMembership: boolean
  primaryAssignmentRequired: boolean
  managerHierarchyRequired: boolean
  allowSkipLevelManagers: boolean
  preventCircularReporting: boolean
}

export interface LocationHierarchySettingsDto {
  locationHierarchyMode: string
  requireLocationCode: boolean
  locationCodeUniquenessScope: string
  allowOperationalLocations: boolean
  allowAddressableBinsShelves: boolean
  allowMobileLocations: boolean
  requireParentLocationExceptRoot: boolean
  archivedLocationAssignmentBehavior: string
}

export interface RolePermissionSettingsDto {
  roleAssignmentApprovalRequired: boolean
  allowSelfServiceRoleRequests: boolean
  roleExpirationEnabled: boolean
  defaultRoleGrantDurationDays: number | null
  requireAssignmentReason: boolean
  permissionReviewCadence: string
  autoRemoveRolesOnInactivePerson: boolean
  allowDirectPermissions: boolean
  preferRolesOverDirectPermissions: boolean
  siteScopedRoleAssignmentsEnabled: boolean
}

export interface TeamAssignmentSettingsDto {
  teamMembershipMode: string
  requireTeamLead: boolean
  allowTemporaryAssignments: boolean
  temporaryAssignmentMaxDurationDays: number | null
  assignmentEffectiveDatingEnabled: boolean
  historicalAssignmentVisibilityMode: string
  allowOpenPositions: boolean
}

export interface IncidentRoutingSettingsDto {
  incidentIntakeEnabled: boolean
  requireIncidentCategory: boolean
  requireInvolvedPerson: boolean
  managerNotificationMode: string
  trainArrRoutingEnabled: boolean
  retrainingRecommendationThreshold: number | null
  incidentVisibilityMode: string
  closureApprovalRequired: boolean
}

export interface ProfileFieldGovernanceSettingsDto {
  requiredProfileSections: string[]
  optionalProfileSections: string[]
  customProfileFieldsEnabled: boolean
  fieldVisibilityByRoleEnabled: boolean
  fieldEditabilityByRoleEnabled: boolean
  fieldReviewRequired: boolean
  fieldHistoryEnabled: boolean
}

export interface NotificationReviewSettingsDto {
  notifyManagerOnNewPerson: boolean
  notifyOnManagerChange: boolean
  notifyOnRoleGrantRemoval: boolean
  notifyBeforeRoleExpiration: boolean
  notifyOnInactiveAssignmentConflict: boolean
  reviewRemindersEnabled: boolean
  digestFrequency: string
}

export interface DataGovernanceAuditSettingsDto {
  auditProfileChanges: boolean
  auditRoleChanges: boolean
  auditOrgLocationChanges: boolean
  requireChangeReasonForSensitiveEdits: boolean
  softArchiveOnly: boolean
  recordRetentionHintDays: number | null
  exportEnabled: boolean
  bulkImportEnabled: boolean
  bulkImportReviewRequired: boolean
}

export interface CrossProductReferenceSettingsDto {
  exposePeopleReferenceApi: boolean
  exposeLocationReferenceApi: boolean
  exposeOrgUnitReferenceApi: boolean
  publishPersonLifecycleEvents: boolean
  publishOrgLocationEvents: boolean
  allowProductOriginatedPersonProposals: boolean
  requireReviewForProductOriginatedProposals: boolean
  snapshotLabelPolicy: string
}

export interface UpsertStaffArrTenantSettingsRequest {
  personDirectory: PersonDirectorySettingsDto
  personLifecycle: PersonLifecycleSettingsDto
  orgStructure: OrgStructureSettingsDto
  locationHierarchy: LocationHierarchySettingsDto
  rolePermissions: RolePermissionSettingsDto
  teamsAssignments: TeamAssignmentSettingsDto
  incidents: IncidentRoutingSettingsDto
  profileFieldGovernance: ProfileFieldGovernanceSettingsDto
  notificationsReviews: NotificationReviewSettingsDto
  dataGovernanceAudit: DataGovernanceAuditSettingsDto
  crossProductReferences: CrossProductReferenceSettingsDto
}

export interface StaffArrTenantSettingsResponse extends UpsertStaffArrTenantSettingsRequest {
  tenantId: string
  createdAt: string
  updatedAt: string
}

export interface EmploymentApplicationFieldOptionRequest {
  value: string
  label: string
}

export interface EmploymentApplicationFieldRequest {
  fieldKey: string
  label: string
  control: 'text' | 'email' | 'phone' | 'textarea' | 'date' | 'select' | 'multi_select' | 'number' | 'yes_no'
  required: boolean
  mappingMode: 'create' | 'eventual' | 'unmapped'
  targetFieldKey: string | null
  helpText: string | null
  placeholder: string | null
  options: EmploymentApplicationFieldOptionRequest[]
}

export interface EmploymentApplicationTemplateCreateRequest {
  templateKey: string
  templateName: string
  title: string
  subtitle: string
  submitLabel: string
  publicLinkExpiresAt: string | null
  fields: EmploymentApplicationFieldRequest[]
}

export interface EmploymentApplicationTemplateUpsertRequest {
  templateName: string
  title: string
  subtitle: string
  submitLabel: string
  publicLinkExpiresAt: string | null
  fields: EmploymentApplicationFieldRequest[]
}

export interface EmploymentApplicationTemplateResponse extends EmploymentApplicationTemplateUpsertRequest {
  employmentApplicationTemplateId: string
  templateKey: string
  version: number
  status: string
  publicToken: string
  createdAt: string
  updatedAt: string
  publishedAt: string | null
  retiredAt: string | null
}

export interface PublicEmploymentApplicationResponse {
  employmentApplicationTemplateId: string
  templateKey: string
  templateName: string
  title: string
  subtitle: string
  submitLabel: string
  version: number
  fields: EmploymentApplicationFieldRequest[]
  publicLinkExpiresAt: string
  createdAt: string
  updatedAt: string
}

export interface SubmitEmploymentApplicationRequest {
  answers: Record<string, string | null>
}

export interface EmploymentApplicationSubmissionResponse {
  employmentApplicationSubmissionId: string
  createdPersonId: string | null
  createdCandidateId: string | null
  recruitingRequisitionId: string | null
  status: string
  applicantDisplayName: string
  applicantEmail: string
  templateKey: string
  templateVersion: number
  submittedAt: string
  createRequestValues: Record<string, string | null>
  eventualProfileValues: Record<string, string | null>
}

export interface EmploymentApplicationSubmissionListItemResponse {
  employmentApplicationSubmissionId: string
  createdPersonId: string | null
  createdCandidateId: string | null
  recruitingRequisitionId: string | null
  status: string
  applicantDisplayName: string
  applicantEmail: string
  templateKey: string
  templateVersion: number
  submittedAt: string
}

export interface StaffArrWorkerPendingPreviewResponse {
  workerKey: string
  asOfUtc: string
  batchSize: number
  itemCount: number
  previewLines: string[]
}

export interface StaffArrWorkerRunItem {
  runId: string
  status: string
  candidatesFound: number
  processedCount: number
  skippedCount: number
  summary: string | null
  startedAt: string
  completedAt: string
}

export interface StaffArrWorkerRunsResponse {
  items: StaffArrWorkerRunItem[]
}

export interface OrgUnitResponse {
  orgUnitId: string
  unitType: OrgUnitType
  name: string
  parentOrgUnitId: string | null
  status: OrgUnitStatus
  code?: string | null
  description?: string | null
  managerPersonId?: string | null
  effectiveStartDate?: string | null
  effectiveEndDate?: string | null
  siteType?: OrgUnitSiteType | null
  timezone?: string | null
  phone?: string | null
  emergencyContact?: string | null
  teamType?: OrgUnitTeamType | null
  positionCode?: string | null
  defaultSiteOrgUnitId?: string | null
  complianceSensitive?: boolean
  safetySensitive?: boolean
  canSupervise?: boolean
  canApprove?: boolean
  archivedAt?: string | null
  archivedByUserId?: string | null
  archiveReason?: string | null
  descendantCount?: number
  assignmentCount?: number
}

export interface StaffArrIntegrationLocationResponse {
  locationId: string
  tenantId: string
  locationNumber: string
  name: string
  locationType: string
  parentLocationId: string | null
  siteOrgUnitId: string | null
  siteNameSnapshot: string
  parentPathSnapshot: string
  status: string
  allowedProductUsage: string
  description?: string | null
  archivedAt?: string | null
  archivedByUserId?: string | null
  archiveReason?: string | null
}

export interface InternalLocationResponse {
  locationId: string
  tenantId: string
  locationNumber: string
  name: string
  locationType: LocationType
  parentLocationId: string | null
  siteOrgUnitId: string | null
  siteNameSnapshot: string
  parentPathSnapshot: string
  status: LocationStatus
  allowedProductUsage: LocationAllowedProductUsage
  code?: string | null
  description?: string | null
  archivedAt?: string | null
  archivedByUserId?: string | null
  archiveReason?: string | null
  descendantCount?: number
  assignmentCount?: number
}

export interface ReadinessOverrideResponse {
  overrideId: string
  personId: string
  status: string
  reason: string
  grantedAt: string
  expiresAt: string | null
  grantedByUserId: string
  clearedAt: string | null
  clearedByUserId: string | null
}

export interface StaffArrRestrictionSnapshotResponse {
  personId: string
  activeRestrictions: ReadinessOverrideResponse[]
  readinessBlockers: ReadinessBlockerResponse[]
}

export interface CreateOrgUnitRequest {
  unitType: OrgUnitType
  name: string
  parentOrgUnitId: string | null
  code?: string | null
  description?: string | null
  managerPersonId?: string | null
  effectiveStartDate?: string | null
  effectiveEndDate?: string | null
  siteType?: OrgUnitSiteType | null
  timezone?: string | null
  phone?: string | null
  emergencyContact?: string | null
  teamType?: OrgUnitTeamType | null
  positionCode?: string | null
  defaultSiteOrgUnitId?: string | null
  complianceSensitive?: boolean
  safetySensitive?: boolean
  canSupervise?: boolean
  canApprove?: boolean
  status?: OrgUnitStatus | null
}

export interface UpdateOrgUnitRequest {
  unitType: OrgUnitType
  name: string
  parentOrgUnitId: string | null
  code?: string | null
  description?: string | null
  managerPersonId?: string | null
  effectiveStartDate?: string | null
  effectiveEndDate?: string | null
  siteType?: OrgUnitSiteType | null
  timezone?: string | null
  phone?: string | null
  emergencyContact?: string | null
  teamType?: OrgUnitTeamType | null
  positionCode?: string | null
  defaultSiteOrgUnitId?: string | null
  complianceSensitive?: boolean
  safetySensitive?: boolean
  canSupervise?: boolean
  canApprove?: boolean
  status?: OrgUnitStatus | null
}

export interface UpdateOrgUnitStatusRequest {
  status: OrgUnitStatus
  reason?: string | null
}

export interface RestoreOrgUnitRequest {
  status?: Exclude<OrgUnitStatus, 'archived'> | null
}

export interface CreateInternalLocationRequest {
  name: string
  locationType: LocationType
  parentLocationId: string | null
  siteOrgUnitId: string | null
  code?: string | null
  description?: string | null
  status?: LocationStatus
  allowedProductUsage?: LocationAllowedProductUsage
}

export interface UpdateInternalLocationRequest {
  name: string
  locationType: LocationType
  parentLocationId: string | null
  siteOrgUnitId: string | null
  code?: string | null
  description?: string | null
  status?: LocationStatus
  allowedProductUsage?: LocationAllowedProductUsage
}

export interface ArchiveInternalLocationRequest {
  reason: string
}

export interface OrgUnitAssignmentResponse {
  assignmentId: string
  personId: string
  siteOrgUnitId: string
  departmentOrgUnitId: string
  teamOrgUnitId: string
  positionOrgUnitId: string
  status: OrgUnitAssignmentStatus
  createdAt: string
  updatedAt: string
  isPrimary?: boolean
  effectiveAt?: string | null
  endsAt?: string | null
  reason?: string | null
}

export interface CreateOrgUnitAssignmentRequest {
  siteOrgUnitId: string
  departmentOrgUnitId: string
  teamOrgUnitId: string
  positionOrgUnitId: string
  status?: OrgUnitAssignmentStatus
  isPrimary?: boolean | null
  effectiveAt?: string | null
  endsAt?: string | null
  reason?: string | null
}

export interface UpdateOrgUnitAssignmentRequest {
  siteOrgUnitId: string
  departmentOrgUnitId: string
  teamOrgUnitId: string
  positionOrgUnitId: string
  status?: OrgUnitAssignmentStatus
  isPrimary?: boolean | null
  effectiveAt?: string | null
  endsAt?: string | null
  reason?: string | null
}

export interface UpdateOrgUnitAssignmentStatusRequest {
  status: OrgUnitAssignmentStatus
  endsAt?: string | null
  reason?: string | null
}

export interface UpdatePersonManagerRequest {
  managerPersonId: string | null
}

export interface PersonManagerResponse {
  personId: string
  managerPersonId: string | null
  managerDisplayName: string | null
  updatedAt: string
}

export interface ManagerChainEntryResponse {
  personId: string
  displayName: string
  primaryEmail: string
  jobTitle: string | null
  primaryOrgUnitName: string | null
  managerPersonId: string | null
  level: number
}

export interface SubordinateSummaryResponse {
  personId: string
  displayName: string
  primaryEmail: string
  employmentStatus: string
  jobTitle: string | null
  primaryOrgUnitName: string | null
  managerPersonId: string | null
  managerDisplayName: string | null
  depth: number
  directReportCount: number
  activeAssignmentPath: string | null
}

export interface ProductPermissionCatalogItemResponse {
  permissionTemplateId: string
  productKey: string
  permissionKey: string
  label: string
  description: string | null
  scope: string
  sensitivity: string
  status: string
  lastSyncedAt: string
}

export interface EffectivePermissionSourceResponse {
  assignmentId: string
  roleId: string
  roleKey: string
  roleName: string
  assignmentStatus: 'active' | 'inactive'
  assignmentScopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
  assignmentScopeValue: string | null
  assignedAt: string
}

export interface EffectivePermissionResponse {
  permissionKey: string
  permissionName: string
  scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
  scopeValue: string | null
  sources: EffectivePermissionSourceResponse[]
}

export interface EffectivePermissionProjectionResponse {
  personId: string
  computedAt: string
  permissions: EffectivePermissionResponse[]
}

export interface PermissionCheckGrantResponse {
  permissionKey: string
  permissionName: string
  scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
  scopeValue: string | null
  roleKey: string
  roleName: string
}

export interface PermissionCheckItemResponse {
  permissionKey: string
  granted: boolean
  grants: PermissionCheckGrantResponse[]
}

export interface PermissionCheckResponse {
  personId: string
  externalUserId: string | null
  isPersonActive: boolean
  computedAt: string
  isAuthorizedAll: boolean
  isAuthorizedAny: boolean
  checks: PermissionCheckItemResponse[]
}

export interface StaffRoleSummaryResponse {
  roleId: string
  tenantId: string
  name: string
  description: string | null
  roleType: 'system_template' | 'tenant_role' | 'product_template'
  isSystem: boolean
  isArchived: boolean
  permissionCount: number
  scopeCount: number
  assignedPersonCount: number
  createdAt: string
  updatedAt: string
}

export interface StaffRolePermissionResponse {
  id: string
  productKey: string
  permissionKey: string
  effect: 'allow' | 'deny'
  label: string
  description: string | null
  riskLevel: 'low' | 'medium' | 'high' | 'critical'
  requiresScope: boolean
  supportedScopeTypes: string[]
  dependsOn: string[]
  conflictsWith: string[]
  createdAt: string
}

export interface StaffRoleScopeResponse {
  id: string
  scopeType:
    | 'tenant'
    | 'site'
    | 'department'
    | 'location'
    | 'team'
    | 'position'
    | 'record_set'
    | 'assigned_assets'
    | 'own_records'
    | 'direct_reports'
  scopeRefId: string | null
  scopeRefSnapshot: string | null
  createdAt: string
}

export interface StaffRoleAssignedPersonResponse {
  personRoleId: string
  personId: string
  displayName: string
  assignmentScopeType:
    | 'tenant'
    | 'site'
    | 'department'
    | 'location'
    | 'team'
    | 'position'
    | 'record_set'
    | 'assigned_assets'
    | 'own_records'
    | 'direct_reports'
  assignmentScopeRefId: string | null
  startsAt: string | null
  endsAt: string | null
  createdAt: string
}

export interface PermissionAuditLogEntryResponse {
  id: string
  tenantId: string
  actorPersonId: string | null
  action: string
  roleId: string | null
  beforeJson: string | null
  afterJson: string | null
  reason: string | null
  createdAt: string
}

export interface StaffRoleDetailResponse {
  roleId: string
  tenantId: string
  name: string
  description: string | null
  roleType: 'system_template' | 'tenant_role' | 'product_template'
  isSystem: boolean
  isArchived: boolean
  createdAt: string
  updatedAt: string
  permissions: StaffRolePermissionResponse[]
  scopes: StaffRoleScopeResponse[]
  assignedPeople: StaffRoleAssignedPersonResponse[]
  auditHistory: PermissionAuditLogEntryResponse[]
}

export interface CreateStaffRoleRequest {
  name: string
  description: string | null
  roleType?: 'tenant_role' | 'product_template'
}

export interface UpdateStaffRoleRequest {
  name: string
  description: string | null
}

export interface ArchiveStaffRoleRequest {
  reason: string | null
}

export interface CloneStaffRoleRequest {
  name: string
  description: string | null
  roleType?: 'tenant_role' | 'product_template'
}

export interface SetStaffRolePermissionItemRequest {
  productKey: string
  permissionKey: string
  effect: 'allow' | 'deny'
}

export interface SetStaffRolePermissionsRequest {
  permissions: SetStaffRolePermissionItemRequest[]
}

export interface SetStaffRoleScopeItemRequest {
  scopeType:
    | 'tenant'
    | 'site'
    | 'department'
    | 'location'
    | 'team'
    | 'position'
    | 'record_set'
    | 'assigned_assets'
    | 'own_records'
    | 'direct_reports'
  scopeRefId: string | null
  scopeRefSnapshot: string | null
}

export interface SetStaffRoleScopesRequest {
  scopes: SetStaffRoleScopeItemRequest[]
}

export interface StaffPersonRoleAssignmentResponse {
  personRoleId: string
  tenantId: string
  personId: string
  roleId: string
  roleName: string
  roleType: 'system_template' | 'tenant_role' | 'product_template'
  roleIsSystem: boolean
  roleIsArchived: boolean
  assignmentScopeType:
    | 'tenant'
    | 'site'
    | 'department'
    | 'location'
    | 'team'
    | 'position'
    | 'record_set'
    | 'assigned_assets'
    | 'own_records'
    | 'direct_reports'
  assignmentScopeRefId: string | null
  startsAt: string | null
  endsAt: string | null
  assignedByPersonId: string | null
  createdAt: string
}

export interface SetStaffPersonRoleItemRequest {
  roleId: string
  assignmentScopeType:
    | 'tenant'
    | 'site'
    | 'department'
    | 'location'
    | 'team'
    | 'position'
    | 'record_set'
    | 'assigned_assets'
    | 'own_records'
    | 'direct_reports'
  assignmentScopeRefId: string | null
  startsAt: string | null
  endsAt: string | null
}

export interface SetStaffPersonRolesRequest {
  roles: SetStaffPersonRoleItemRequest[]
}

export interface PermissionCatalogPermissionResponse {
  key: string
  label: string
  description: string | null
  riskLevel: 'low' | 'medium' | 'high' | 'critical'
  requiresScope: boolean
  supportedScopeTypes: string[]
  dependsOn: string[]
  conflictsWith: string[]
}

export interface PermissionCatalogPermissionGroupResponse {
  key: string
  label: string
  permissions: PermissionCatalogPermissionResponse[]
}

export interface PermissionCatalogModuleResponse {
  key: string
  label: string
  description: string | null
  permissionGroups: PermissionCatalogPermissionGroupResponse[]
}

export interface PermissionCatalogResponse {
  productKey: string
  productName: string
  version: string
  modules: PermissionCatalogModuleResponse[]
}

export interface RefreshPermissionCatalogRequest {
  productKey?: string | null
}

export interface RefreshPermissionCatalogResponse {
  refreshedAt: string
  catalogs: PermissionCatalogResponse[]
}

export interface PermissionEvaluationResourceRequest {
  type: string
  id: string | null
  siteId: string | null
  locationId: string | null
  departmentId: string | null
  teamId: string | null
  positionId: string | null
  recordSetId: string | null
  assignedPersonId: string | null
  ownerPersonId: string | null
  personId: string | null
  managerPersonId: string | null
}

export interface PermissionEvaluateRequest {
  tenantId: string
  personId: string
  productKey: string
  permissionKey: string
  resource: PermissionEvaluationResourceRequest | null
}

export interface PermissionEvaluateResponse {
  allowed: boolean
  reason: string
  roleIds: string[]
  scopeMatched: boolean
}

export interface CertificationDefinitionResponse {
  certificationDefinitionId: string
  certificationKey: string
  name: string
  description: string | null
  category: string
  defaultValidityDays: number | null
  status: 'active' | 'inactive'
  createdAt: string
  updatedAt: string
}

export interface UpsertCertificationDefinitionRequest {
  certificationKey: string
  name: string
  description: string | null
  category: string
  defaultValidityDays: number | null
}

export interface PersonCertificationResponse {
  personCertificationId: string
  personId: string
  certificationDefinitionId: string
  certificationKey: string
  certificationName: string
  category: string
  sourceType: string
  status: 'active' | 'expired' | 'revoked'
  effectiveStatus: 'active' | 'expired' | 'revoked'
  grantedAt: string
  expiresAt: string | null
  notes: string | null
  grantedByUserId: string | null
  externalPublicationId: string | null
  createdAt: string
  updatedAt: string
}

export interface GrantPersonCertificationRequest {
  certificationDefinitionId: string
  grantedAt: string | null
  expiresAt: string | null
  notes: string | null
}

export interface UpdatePersonCertificationRequest {
  status: 'active' | 'expired' | 'revoked'
  expiresAt: string | null
  notes: string | null
}

export interface ReadinessRequirementStatusResponse {
  certificationDefinitionId: string
  certificationKey: string
  certificationName: string
  requirementStatus: 'satisfied' | 'missing' | 'expired' | 'revoked'
  recordEffectiveStatus: string | null
  expiresAt: string | null
}

export interface ReadinessBlockerResponse {
  blockerSource: 'certification' | 'training'
  blockerType:
    | 'missing'
    | 'expired'
    | 'revoked'
    | 'missing_assignment'
    | 'overdue'
    | 'failed'
    | 'suspended'
  message: string
  certificationKey: string | null
  certificationName: string | null
  qualificationKey: string | null
  qualificationName: string | null
}

export interface ReadinessOverrideSummaryResponse {
  overrideId: string
  reason: string
  grantedAt: string
  expiresAt: string | null
  grantedByUserId: string
}

export interface PersonReadinessResponse {
  personId: string
  readinessStatus: 'ready' | 'not_ready'
  readinessBasis: 'certifications' | 'manual_override' | 'training_blockers'
  calculatedAt: string
  sourceTimestamp: string
  snapshotAgeMinutes: number
  snapshotFreshnessStatus: 'fresh' | 'aging' | 'stale'
  confidenceLevel: 'high' | 'medium' | 'low'
  reasonCodes: string[]
  requirements: ReadinessRequirementStatusResponse[]
  blockers: ReadinessBlockerResponse[]
  activeOverride: ReadinessOverrideSummaryResponse | null
}

export interface GrantReadinessOverrideRequest {
  reason: string
  expiresAt: string | null
}

export interface ReadinessRollupSummaryResponse {
  orgUnitId: string
  scopeType: 'team' | 'site'
  orgUnitName: string
  totalMembers: number
  readyCount: number
  notReadyCount: number
  overrideCount: number
  readyPercent: number
  confidenceLevel: 'high' | 'medium' | 'low'
  confidenceScore: number
  computedAt: string
}

export interface ReadinessRollupMemberResponse {
  personId: string
  displayName: string
  readinessStatus: 'ready' | 'not_ready'
  readinessBasis: 'certifications' | 'manual_override' | 'training_blockers'
  hasActiveOverride: boolean
  blockerCount: number
  primaryBlockerMessage: string | null
}

export interface ReadinessRollupMembersResponse {
  rollup: ReadinessRollupSummaryResponse
  members: ReadinessRollupMemberResponse[]
}

export type ReadinessRollupSelection = {
  scopeType: 'team' | 'site'
  orgUnitId: string
  orgUnitName: string
}

export type PersonnelIncidentReasonCategory =
  | 'safety'
  | 'conduct'
  | 'behavior'
  | 'injury'
  | 'equipment'
  | 'equipment_damage'
  | 'training_compliance'
  | 'training_issue'
  | 'policy'
  | 'policy_violation'
  | 'attendance'
  | 'near_miss'
  | 'other'

export type PersonnelIncidentStatus = 'draft' | 'submitted' | 'open' | 'in_review' | 'closed'

export type PersonnelIncidentSeverity = 'low' | 'medium' | 'high' | 'critical'

export type PersonnelIncidentSource =
  | 'staffarr'
  | 'self_report'
  | 'manager_report'
  | 'safety_observation'
  | 'compliancecore'
  | 'maintainarr'
  | 'routarr'
  | 'supplyarr'
  | 'trainarr'
  | 'other'

export type PersonnelIncidentType =
  | 'injury'
  | 'safety'
  | 'behavior'
  | 'equipment_damage'
  | 'policy_violation'
  | 'training_issue'
  | 'attendance'
  | 'near_miss'
  | 'other'

export type PersonnelIncidentReadinessDecision = 'allowed' | 'watched' | 'restricted'

export interface IncidentTrainarrRoutingResponse {
  routingStatus: string
  trainarrRemediationId: string
  routedAt: string
  routedByUserId: string
}

export interface TrainingAcknowledgementResponse {
  acknowledgementId: string
  personId: string
  trainarrAcknowledgementRequestId: string
  trainarrAssignmentId: string
  trainingTitle: string
  assignmentReason: string
  summary: string
  status: string
  dueAt: string | null
  requestedAt: string
  acknowledgedAt: string | null
  acknowledgedByUserId: string | null
  createdAt: string
  updatedAt: string
}

export interface PersonnelIncidentSummaryResponse {
  incidentId: string
  personId: string
  reasonCategoryKey: PersonnelIncidentReasonCategory
  severity: PersonnelIncidentSeverity
  status: string
  title: string
  occurredAt: string
  reportedAt: string
  reportedByUserId: string
  trainarrRouting: IncidentTrainarrRoutingResponse | null
  incidentSource?: PersonnelIncidentSource | null
  incidentType?: PersonnelIncidentType | null
  discoveredAt?: string | null
  reporterPersonId?: string | null
  managerPersonId?: string | null
  categoryKeys?: PersonnelIncidentType[] | null
  readinessDecision?: PersonnelIncidentReadinessDecision | null
  trainingReviewRequired?: boolean
  sourceProduct?: string | null
  sourceIncidentId?: string | null
  sourceEventKind?: string | null
  sourceReferenceKey?: string | null
  sourceSnapshot?: ProductSourceReferenceSnapshotResponse | null
}

export interface ProductSourceReferenceSnapshotResponse {
  sourceProduct: string
  sourceEntity: string
  sourceId: string
  labelSnapshot: string
  statusSnapshot: string
  selectedAt: string
  lastVerifiedAt: string
  lastSyncedAt: string | null
  isAuthoritative: boolean
}

export interface PersonnelIncidentDetailResponse extends PersonnelIncidentSummaryResponse {
  description: string
  createdAt: string
  updatedAt: string
  siteOrgUnitId?: string | null
  departmentOrgUnitId?: string | null
  locationDetail?: string | null
  witnessPersonIds?: string[] | null
  additionalInvolvedPersonIds?: string[] | null
  employeeSelfReport?: boolean
  immediateActionsTaken?: string | null
  rootCause?: string | null
  workRestriction?: string | null
  returnToWorkNeeded?: string | null
  ppeConcern?: string | null
  medicalAttention?: string | null
  outOfServiceRemoveFromDuty?: string | null
  followUpRequired?: string | null
  trainingReviewRequired?: boolean
  trainingReviewReason?: string | null
  relatedAssetReference?: string | null
  relatedWorkOrderReference?: string | null
  relatedRouteReference?: string | null
  relatedSupplierReference?: string | null
  relatedDocumentReference?: string | null
  relatedPolicyReference?: string | null
  evidencePackageRequested?: boolean
  notifyManager?: boolean
  notifySafetyCompliance?: boolean
  notifyHr?: boolean
  createFollowUpTask?: boolean
  followUpDueAt?: string | null
  notes?: IncidentNoteSummaryResponse[] | null
  attachments?: IncidentAttachmentSummaryResponse[] | null
}

export interface RouteIncidentToTrainarrResponse {
  incidentId: string
  personId: string
  reasonCategoryKey: PersonnelIncidentReasonCategory
  status: string
  trainarrRouting: IncidentTrainarrRoutingResponse
}

export interface CreatePersonnelIncidentRequest {
  personId: string
  reasonCategoryKey: PersonnelIncidentReasonCategory
  severity: PersonnelIncidentSeverity
  title: string
  description: string
  occurredAt: string
  status?: PersonnelIncidentStatus | null
  incidentSource?: PersonnelIncidentSource | null
  incidentType?: PersonnelIncidentType | null
  discoveredAt?: string | null
  siteOrgUnitId?: string | null
  departmentOrgUnitId?: string | null
  locationDetail?: string | null
  reporterPersonId?: string | null
  managerPersonId?: string | null
  witnessPersonIds?: string[] | null
  additionalInvolvedPersonIds?: string[] | null
  employeeSelfReport?: boolean
  immediateActionsTaken?: string | null
  rootCause?: string | null
  categoryKeys?: PersonnelIncidentType[] | null
  readinessDecision?: PersonnelIncidentReadinessDecision | null
  workRestriction?: string | null
  returnToWorkNeeded?: string | null
  ppeConcern?: string | null
  medicalAttention?: string | null
  outOfServiceRemoveFromDuty?: string | null
  followUpRequired?: string | null
  trainingReviewRequired?: boolean
  trainingReviewReason?: string | null
  relatedAssetReference?: string | null
  relatedWorkOrderReference?: string | null
  relatedRouteReference?: string | null
  relatedSupplierReference?: string | null
  relatedDocumentReference?: string | null
  relatedPolicyReference?: string | null
  evidencePackageRequested?: boolean
  notifyManager?: boolean
  notifySafetyCompliance?: boolean
  notifyHr?: boolean
  createFollowUpTask?: boolean
  followUpDueAt?: string | null
}

export interface UpdatePersonnelIncidentStatusRequest {
  status: PersonnelIncidentStatus
}

export type IncidentNoteTypeKey = 'note' | 'corrective_action'

export type IncidentNoteStatus = 'open' | 'completed'

export interface CreateIncidentNoteRequest {
  noteTypeKey: IncidentNoteTypeKey
  subject: string
  body: string
  dueAt?: string | null
}

export interface UpdateIncidentNoteStatusRequest {
  status: IncidentNoteStatus
}

export interface IncidentNoteSummaryResponse {
  noteId: string
  incidentId: string
  noteTypeKey: IncidentNoteTypeKey
  subject: string
  body: string
  status: IncidentNoteStatus
  dueAt: string | null
  completedAt: string | null
  createdByUserId: string
  createdAt: string
  updatedAt: string
}

export interface CreateIncidentAttachmentRequest {
  title: string
  fileName: string
  contentType: string
  contentBase64: string
  description?: string | null
}

export interface IncidentAttachmentSummaryResponse {
  attachmentId: string
  incidentId: string
  title: string
  fileName: string
  contentType: string
  sizeBytes: number
  description?: string | null
  uploadedByUserId: string
  createdAt: string
  updatedAt: string
}

export interface SubmitSelfReportedPersonnelIncidentRequest {
  reasonCategoryKey: PersonnelIncidentReasonCategory
  severity: PersonnelIncidentSeverity
  title: string
  description: string
  occurredAt: string
}

export type PersonnelNoteCategoryKey =
  | 'general'
  | 'performance'
  | 'coaching'
  | 'disciplinary'
  | 'medical'
  | 'other'

export type PersonnelNoteVisibilityKey = 'hr_only' | 'management' | 'personnel_visible'

export interface PersonnelNoteSummaryResponse {
  noteId: string
  personId: string
  categoryKey: PersonnelNoteCategoryKey
  visibilityKey: PersonnelNoteVisibilityKey
  subject: string
  status: string
  createdByUserId: string
  createdAt: string
  updatedAt: string
}

export interface PersonnelNoteDetailResponse extends PersonnelNoteSummaryResponse {
  body: string
}

export interface CreatePersonnelNoteRequest {
  categoryKey: PersonnelNoteCategoryKey
  visibilityKey: PersonnelNoteVisibilityKey
  subject: string
  body: string
}

export type PersonnelDocumentTypeKey =
  | 'id_verification'
  | 'employment_contract'
  | 'certification_copy'
  | 'medical_form'
  | 'policy_acknowledgment'
  | 'offer_letter'
  | 'employment_agreement'
  | 'handbook_acknowledgment'
  | 'emergency_contact'
  | 'job_description_acknowledgment'
  | 'corrective_action'
  | 'performance_review'
  | 'leave_paperwork'
  | 'termination_paperwork'
  | 'work_authorization'
  | 'medical_accommodation'
  | 'eeo_self_id'
  | 'other'

export interface PersonnelDocumentSummaryResponse {
  documentId: string
  personId: string
  documentTypeKey: PersonnelDocumentTypeKey
  accessLevel: 'employee' | 'manager' | 'hr' | 'restricted'
  retentionCategory:
    | 'personnel_file'
    | 'employment_eligibility'
    | 'discipline'
    | 'performance'
    | 'leave'
    | 'termination'
    | 'medical'
    | 'eeo'
    | 'other'
  restrictedData: boolean
  title: string
  fileName: string
  contentType: string
  sizeBytes: number
  description: string | null
  expiresAt: string | null
  status: string
  uploadedByUserId: string
  createdAt: string
  updatedAt: string
}

export interface PersonnelDocumentDetailResponse extends PersonnelDocumentSummaryResponse {}

export interface CreatePersonnelDocumentRequest {
  documentTypeKey: PersonnelDocumentTypeKey
  accessLevel: 'employee' | 'manager' | 'hr' | 'restricted'
  retentionCategory:
    | 'personnel_file'
    | 'employment_eligibility'
    | 'discipline'
    | 'performance'
    | 'leave'
    | 'termination'
    | 'medical'
    | 'eeo'
    | 'other'
  restrictedData: boolean
  title: string
  fileName: string
  contentType: string
  contentBase64: string
  description: string | null
  expiresAt: string | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  hasNextPage: boolean
}

export interface PersonTimelineEntryResponse {
  entryId: string
  personId: string
  category:
    | 'incident'
    | 'incident_routing'
    | 'readiness'
    | 'certification'
    | 'permission'
    | 'training_blocker'
    | 'personnel_note'
    | 'personnel_document'
    | 'recruiting'
  eventType: string
  title: string
  detail: string | null
  occurredAt: string
  actorUserId: string | null
  sourceEntityType: string
  sourceEntityId: string
  externalReferenceId: string | null
}

export interface TrainarrPersonTrainingHistoryEntryItem {
  entryId: string
  eventKind: string
  summary: string
  relatedEntityType: string | null
  relatedEntityId: string | null
  occurredAt: string
}

export interface TrainarrPersonTrainingHistoryResponse {
  personId: string
  sourceProduct: string
  sourceNote: string
  totalCount: number
  items: TrainarrPersonTrainingHistoryEntryItem[]
}

export interface WorkforceOnboardingJourneyStepResponse {
  stepKey: string
  title: string
  detail: string
  status: string
  statusReason: string | null
}

export interface WorkforceOnboardingJourneyResponse {
  personId: string
  journeyKey: string
  overallStatus: string
  overallSummary: string
  steps: WorkforceOnboardingJourneyStepResponse[]
  trainarrIntegrationNote: string | null
}

export interface PersonOffboardingStepResponse {
  stepKey: string
  title: string
  detail: string
  status: string
  blockerDetail: string | null
  sortOrder: number
  completedAt: string | null
}

export interface PersonOffboardingResponse {
  offboardingId: string
  personId: string
  status: string
  separationDate: string
  separationReason: string | null
  targetEmploymentStatus: string
  disableLoginRequested: boolean
  newManagerPersonIdForReports: string | null
  startedAt: string
  startedByUserId: string
  completedAt: string | null
  completedByUserId: string | null
  steps: PersonOffboardingStepResponse[]
  activeDirectReportCount: number
  openIncidentCount: number
  activeRoleAssignmentCount: number
  activeOrgAssignmentCount: number
}

export interface StartPersonOffboardingRequest {
  personId: string
  separationDate: string
  separationReason: string | null
  targetEmploymentStatus: string
  disableLoginRequested: boolean
  newManagerPersonIdForReports: string | null
}

export interface ExecutePersonOffboardingRequest {
  newManagerPersonIdForReports: string | null
}

export interface PersonnelHistorySummaryResponse {
  personId: string
  eventCount: number
  incidentCount: number
  certificationCount: number
  permissionCount: number
  readinessCount: number
  trainingBlockerCount: number
  personnelNoteCount: number
  personnelDocumentCount: number
  lastEventAt: string | null
  computedAt: string
  isMaterialized: boolean
}

export interface EntityExportFormatDescriptor {
  formatKey: string
  contentType: string
  fileNameTemplate: string
  description: string
}

export interface EntityExportManifestEntity {
  entityKey: string
  exportPath: string
  displayName: string
  csvHeader: string
  description: string
  formats: EntityExportFormatDescriptor[]
}

export interface EntityExportManifestResponse {
  packageVersion: string
  entities: EntityExportManifestEntity[]
}

export interface AuditPackageSectionDescriptor {
  key: string
  fileName: string
  label: string
  description: string
}

export interface AuditPackageManifestResponse {
  packageVersion: string
  sections: AuditPackageSectionDescriptor[]
}

export interface AuditPackageAppliedFilters {
  from: string | null
  to: string | null
  action: string | null
  result: string | null
  targetType: string | null
  actorUserId: string | null
  personId: string | null
}

export interface AuditPackageFilterOptions {
  actions: string[]
  results: string[]
  targetTypes: string[]
  actorUserIds: string[]
}

export interface AuditPackageBreakdownItem {
  key: string
  count: number
}

export interface AuditPackageCountsResponse {
  auditEvents: number
  people: number
  permissionHistory: number
  personCertifications: number
  personnelIncidents: number
  readinessOverrides: number
  trainingBlockers: number
}

export interface AuditPackageExportSummary {
  filters: AuditPackageAppliedFilters
  counts: AuditPackageCountsResponse
  byResult: AuditPackageBreakdownItem[]
  byAction: AuditPackageBreakdownItem[]
  generatedAt: string
}

export interface AuditPackageScope {
  from?: string
  to?: string
  action?: string
  result?: string
  targetType?: string
  actorUserId?: string
  personId?: string
}

export interface StaffArrAuditEventExportItem {
  auditEventId: string
  actorUserId: string | null
  action: string
  targetType: string
  targetId: string | null
  result: string
  reasonCode: string | null
  correlationId: string
  occurredAt: string
}

export interface AuditPackageGenerationJobResponse {
  jobId: string
  status: string
  format: string
  from: string | null
  to: string | null
  packageId: string | null
  errorMessage: string | null
  createdAt: string
  startedAt: string | null
  completedAt: string | null
  downloadReady: boolean
}

export interface AuditPackageExportResponse {
  packageId: string
  tenantId: string
  generatedAt: string
  appliedFilters?: AuditPackageAppliedFilters | null
  counts: AuditPackageCountsResponse
  auditEvents?: StaffArrAuditEventExportItem[]
  people?: unknown[]
  permissionHistory?: unknown[]
  personCertifications?: unknown[]
  personnelIncidents?: unknown[]
  readinessOverrides?: unknown[]
  trainingBlockers?: unknown[]
}

export interface PersonnelReportSummaryItem {
  personId: string
  displayName: string
  employmentStatus: string
  primaryOrgUnitName: string | null
}

export interface PersonnelReportSummaryResponse {
  totalPeople: number
  activeCount: number
  inactiveCount: number
  onLeaveCount: number
  activePercent: number
  recentPeople: PersonnelReportSummaryItem[]
}

export interface ReadinessReportSummaryItem {
  rollupId: string
  orgUnitName: string
  scopeType: string
  notReadyCount: number
}

export interface ReadinessReportSummaryResponse {
  totalRollups: number
  totalMembers: number
  readyCount: number
  notReadyCount: number
  overrideCount: number
  readyPercent: number
  recentRollups: ReadinessReportSummaryItem[]
}

export interface IncidentReportSummaryItem {
  incidentId: string
  title: string
  severity: string
  status: string
}

export interface IncidentReportSummaryResponse {
  totalIncidents: number
  openCount: number
  closedCount: number
  highSeverityOpenCount: number
  recentIncidents: IncidentReportSummaryItem[]
}

export interface CertificationReportSummaryItem {
  personCertificationId: string
  personDisplayName: string
  certificationName: string
  certificationKey: string
  status: string
}

export interface CertificationReportSummaryResponse {
  totalPeople: number
  activeCertificationCount: number
  expiringSoonCount: number
  expiredCertificationCount: number
  missingCertificationCount: number
  recentCertifications: CertificationReportSummaryItem[]
}
