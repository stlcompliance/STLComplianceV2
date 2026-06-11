export type ReportArrHandoffSessionResponse = {
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
  callbackUrl: string | null
}

export type ReportArrSessionBootstrapResponse = {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasReportArrEntitlement: boolean
  entitlements: string[]
}

export type ReportArrMeResponse = {
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasReportArrEntitlement: boolean
  entitlements: string[]
}

export type ReportArrSummaryResponse = {
  generatedAt: string
  freshnessStatus: string
  datasetCount: number
  dashboardCount: number
  reportDefinitionCount: number
  reportRunCount: number
  kpiCount: number
  alertCount: number
  auditPackageCount: number
  recentDatasets: ReportArrDatasetResponse[]
  recentDashboards: ReportArrDashboardResponse[]
  recentReports: ReportArrReportDefinitionResponse[]
  recentAlerts: ReportArrAlertResponse[]
  recentAuditPackages: ReportArrAuditPackageResponse[]
}

export type ReportArrDatasetResponse = {
  datasetId: string
  datasetNumber: string
  datasetKey: string
  title: string
  description: string
  datasetType: string
  status: string
  refreshMode: string
  refreshFrequency: string
  freshnessStatus: string
  sourceProducts: string[]
  sourceConnectors: string[]
  lastRefreshedAt: string | null
  lastSuccessfulRefreshAt: string | null
  lastFailedRefreshAt: string | null
  schemaVersion: string
  fieldDefinitions: string[]
  sourceTraceabilityRules: string
  retentionPolicy: string
  ownerPersonId: string
  createdAt: string
  updatedAt: string
  tenantId: string
}

export type ReportArrDatasetFieldResponse = {
  fieldId: string
  datasetId: string
  fieldKey: string
  displayName: string
  description: string
  dataType: string
  sourceProduct: string
  sourceFieldPath: string
  aggregationAllowed: boolean
  filterAllowed: boolean
  groupAllowed: boolean
  sortAllowed: boolean
  piiSensitive: boolean
  restricted: boolean
  complianceSensitive: boolean
  tenantId: string
}

export type ReportArrSourceConnectorResponse = {
  sourceConnectorId: string
  sourceProduct: string
  connectorType: string
  status: string
  serviceClientRef: string
  lastConnectedAt: string | null
  lastErrorAt: string | null
  lastErrorMessage: string | null
  supportedEventTypes: string[]
  supportedDatasets: string[]
  tenantId: string
}

export type ReportArrIngestionCursorResponse = {
  ingestionCursorId: string
  sourceConnectorId: string
  sourceProduct: string
  cursorType: string
  cursorValue: string
  lastEventId: string | null
  lastEventAt: string | null
  lastIngestedAt: string | null
  status: string
  tenantId: string
}

export type ReportArrSourceEventReceiptResponse = {
  sourceEventReceiptId: string
  sourceProduct: string
  sourceEventId: string
  eventType: string
  sourceObjectRef: string | null
  receivedAt: string
  processedAt: string | null
  status: string
  failureReason: string | null
  correlationId: string | null
  tenantId: string
}

export type ReportArrReadModelResponse = {
  readModelId: string
  readModelNumber: string
  readModelKey: string
  title: string
  description: string
  readModelType: string
  status: string
  primaryEntityType: string
  primarySourceProduct: string
  datasetRefs: string[]
  fieldDefinitions: string[]
  refreshJobRefs: string[]
  lastRebuiltAt: string | null
  lastUpdatedAt: string | null
  updatedAt: string
  tenantId: string
}

export type ReportArrReadModelRecordResponse = {
  readModelRecordId: string
  readModelId: string
  primaryEntityRef: string
  data: string
  sourceTraces: string[]
  statusSnapshot: string
  effectiveAt: string
  lastSourceUpdatedAt: string | null
  ingestedAt: string
  updatedAt: string
  tenantId: string
}

export type ReportArrDatasetLineageResponse = {
  lineageId: string
  datasetId: string
  sourceProduct: string
  sourceObjectType: string
  sourceFieldPath: string
  datasetFieldKey: string
  transformationDescription: string
  confidence: string
  tenantId: string
}

export type ReportArrDashboardResponse = {
  dashboardId: string
  dashboardNumber: string
  dashboardKey: string
  title: string
  description: string
  dashboardType: string
  status: string
  ownerPersonId: string
  defaultDateRange: string
  freshnessStatus: string
  widgetRefs: string[]
  filterRefs: string[]
  drilldownRefs: string[]
  accessPolicyRef: string
  lastViewedAt: string | null
  createdAt: string
  createdByPersonId: string
  updatedAt: string
  updatedByPersonId: string
  tenantId: string
}

export type ReportArrDashboardAccessPolicyResponse = {
  accessPolicyId: string
  dashboardId: string
  visibility: string
  allowedPersonRefs: string[]
  allowedRoleRefs: string[]
  allowedPermissionRefs: string[]
  sourceProductRestrictions: string[]
  exportAllowed: boolean
  createdAt: string
  updatedAt: string
  tenantId: string
}

export type ReportArrDashboardFilterResponse = {
  filterId: string
  dashboardId: string
  filterKey: string
  label: string
  filterType: string
  datasetFieldKey: string
  defaultValue: string
  allowedValuesSource: string
  required: boolean
  visible: boolean
  tenantId: string
}

export type ReportArrDrilldownDefinitionResponse = {
  drilldownId: string
  dashboardId: string
  title: string
  description: string
  sourceWidgetRef: string
  targetType: string
  targetRef: string
  parameterMappings: string[]
  requiredPermissionRefs: string[]
  status: string
  tenantId: string
}

export type ReportArrDashboardWidgetResponse = {
  widgetId: string
  dashboardId: string
  widgetKey: string
  title: string
  description: string
  widgetType: string
  status: string
  datasetRef: string
  readModelRef: string
  queryDefinition: string
  filterBindings: string[]
  drilldownTargetRef: string
  sortOrder: number
  layout: string
  visualizationSettings: string
  freshnessStatus: string
  lastRenderedAt: string | null
  tenantId: string
}

export type ReportArrWidgetVisualizationSettingsResponse = {
  widgetId: string
  chartType: string
  xField: string | null
  yField: string | null
  seriesField: string | null
  groupField: string | null
  valueField: string | null
  labelField: string | null
  dateField: string | null
  colorRuleRefs: string[]
  thresholdRefs: string[]
  displayFormat: string
  showLegend: boolean
  showDataLabels: boolean
  maxRows: number
  tenantId: string
}

export type ReportArrReportDefinitionResponse = {
  reportDefinitionId: string
  reportNumber: string
  reportKey: string
  title: string
  description: string
  reportType: string
  status: string
  datasetRefs: string[]
  readModelRefs: string[]
  parameterRefs: string[]
  defaultFilters: string[]
  layoutDefinition: string
  sectionRefs: string[]
  exportFormats: string[]
  accessPolicyRef: string
  ownerPersonId: string
  createdAt: string
  createdByPersonId: string
  updatedAt: string
  updatedByPersonId: string
  tenantId: string
}

export type ReportArrReportAccessPolicyResponse = {
  accessPolicyId: string
  reportDefinitionId: string
  visibility: string
  allowedPersonRefs: string[]
  allowedRoleRefs: string[]
  allowedPermissionRefs: string[]
  sourceProductRestrictions: string[]
  exportAllowed: boolean
  scheduleAllowed: boolean
  externalDeliveryAllowed: boolean
  createdAt: string
  updatedAt: string
  tenantId: string
}

export type ReportArrReportParameterResponse = {
  parameterId: string
  reportDefinitionId: string
  parameterKey: string
  label: string
  parameterType: string
  required: boolean
  defaultValue: string
  allowedValuesSource: string
  validationRules: string
  tenantId: string
}

export type ReportArrReportSectionResponse = {
  sectionId: string
  reportDefinitionId: string
  sequence: number
  title: string
  description: string
  sectionType: string
  datasetRef: string
  queryDefinition: string
  layoutSettings: string
  tenantId: string
}

export type ReportArrReportRunResponse = {
  reportRunId: string
  reportRunNumber: string
  reportDefinitionId: string
  title: string
  status: string
  requestedByPersonId: string
  requestedAt: string
  startedAt: string | null
  completedAt: string | null
  parametersUsed: string[]
  filtersUsed: string[]
  outputFormat: string
  outputRecordRef: string | null
  outputPackageRef: string | null
  rowCount: number
  warningCount: number
  exportJobId: string | null
  errorCount: number
  errorMessage: string | null
  freshnessStatus: string
  sourceTraceSummary: string
  freshnessSummary: string
  tenantId: string
}

export type ReportArrReportScheduleResponse = {
  scheduleId: string
  scheduleNumber: string
  reportDefinitionId: string
  title: string
  status: string
  cadence: string
  timezone: string
  cronExpression: string | null
  nextRunAt: string | null
  lastRunAt: string | null
  startsAt: string | null
  endsAt: string | null
  parameters: string[]
  recipients: string[]
  deliveryMethod: string
  createdByPersonId: string
  createdAt: string
  updatedAt: string
  tenantId: string
}

export type ReportArrReportRecipientResponse = {
  recipientId: string
  scheduleId: string
  recipientType: string
  recipientRef: string
  email: string | null
  deliveryFormat: string
  status: string
  tenantId: string
}

export type ReportArrExportJobResponse = {
  exportJobId: string
  exportNumber: string
  reportRunId: string
  title: string
  status: string
  exportType: string
  exportFormat: string
  requestedByPersonId: string
  requestedAt: string
  startedAt: string | null
  completedAt: string | null
  sourceRef: string | null
  outputRecordRef: string | null
  rowCount: number
  fileSizeBytesSnapshot: number
  expiresAt: string | null
  errorMessage: string | null
  generatedAt: string
  deliveredAt: string | null
  recordArrPackageRef: string | null
  tenantId: string
}

export type ReportArrMetricDefinitionResponse = {
  metricId: string
  metricKey: string
  title: string
  description: string
  metricType: string
  sourceDatasetRef: string
  fieldRefs: string[]
  formula: string
  filterDefinition: string
  groupingOptions: string
  dateField: string
  status: string
  tenantId: string
}

export type ReportArrMetricValueResponse = {
  metricValueId: string
  metricId: string
  periodStart: string
  periodEnd: string
  value: number
  groupKey: string | null
  groupLabel: string | null
  sourceTraceSummary: string
  calculatedAt: string
  tenantId: string
}

export type ReportArrAnalyticsSnapshotResponse = {
  analyticsSnapshotId: string
  snapshotNumber: string
  snapshotType: string
  status: string
  periodStart: string
  periodEnd: string
  datasetRefs: string[]
  kpiValueRefs: string[]
  metricValueRefs: string[]
  generatedAt: string
  generatedBy: string
  tenantId: string
}

export type ReportArrTrendAnalysisResponse = {
  trendAnalysisId: string
  metricRef: string
  kpiRef: string | null
  periodStart: string
  periodEnd: string
  trend: string
  changeValue: number
  changePercent: number
  confidence: string
  explanation: string
  generatedAt: string
  tenantId: string
}

export type ReportArrExceptionQueryResponse = {
  exceptionQueryId: string
  queryKey: string
  title: string
  description: string
  sourceDatasetRef: string
  condition: string
  severity: string
  status: string
  ownerPersonId: string
  tenantId: string
}

export type ReportArrExceptionResultResponse = {
  exceptionResultId: string
  exceptionQueryId: string
  sourceObjectRef: string
  title: string
  summary: string
  severity: string
  status: string
  detectedAt: string
  acknowledgedByPersonId: string | null
  acknowledgedAt: string | null
  resolvedAt: string | null
  sourceTrace: string
  tenantId: string
}

export type ReportArrKpiDefinitionResponse = {
  kpiId: string
  kpiNumber: string
  kpiKey: string
  title: string
  description: string
  category: string
  status: string
  formula: string
  sourceDatasetRefs: string[]
  sourceMetricRefs: string[]
  targetValue: number | null
  warningThreshold: number | null
  criticalThreshold: number | null
  higherIsBetter: boolean
  displayFormat: string
  ownerPersonId: string
  createdAt: string
  updatedAt: string
  tenantId: string
}

export type ReportArrKpiValueResponse = {
  kpiValueId: string
  kpiId: string
  periodStart: string
  periodEnd: string
  value: number
  targetValueSnapshot: number | null
  warningThresholdSnapshot: number | null
  criticalThresholdSnapshot: number | null
  status: string
  trend: string
  sourceTraceSummary: string
  calculatedAt: string
  tenantId: string
}

export type ReportArrAlertResponse = {
  alertId: string
  alertNumber: string
  title: string
  description: string
  alertType: string
  status: string
  datasetRef: string
  metricRef: string
  condition: string
  severity: string
  triggeredAt: string | null
  acknowledgedByPersonId: string | null
  acknowledgedAt: string | null
  resolvedAt: string | null
  notificationRefs: string[]
  tenantId: string
}

export type ReportArrAuditPackageResponse = {
  auditReportPackageId: string
  packageNumber: string
  title: string
  description: string
  status: string
  requestedByPersonId: string
  auditScope: ReportArrAuditScopeResponse
  complianceEvaluationRefs: string[]
  sourceProductRefs: string[]
  sourceObjectRefs: string[]
  recordArrPackageRef: string | null
  reportRunRefs: string[]
  missingEvidenceSummary: string
  invalidEvidenceSummary: string
  readinessScore: number
  generatedAt: string
  lockedAt: string | null
  tenantId: string
}

export type ReportArrAuditScopeResponse = {
  auditScopeId: string
  scopeType: string
  dateRangeStart: string | null
  dateRangeEnd: string | null
  productFilters: string[]
  objectRefs: string[]
  rulepackRefs: string[]
  siteRefs: string[]
  departmentRefs: string[]
  includeEvidence: boolean
  includeSourceTrace: boolean
  tenantId: string
}

export type ReportArrRefreshJobResponse = {
  refreshJobId: string
  datasetId: string
  readModelId: string | null
  refreshType: string
  status: string
  requestedByPersonId: string
  queuedAt: string
  startedAt: string | null
  completedAt: string | null
  recordsProcessed: number
  recordsCreated: number
  recordsUpdated: number
  recordsSkipped: number
  errorCount: number
  errorMessage: string | null
  tenantId: string
}

export type ReportArrIntegrationEventRequest = {
  sourceProduct: string
  sourceEventId: string
  eventType: string
  sourceObjectRef: string | null
  correlationId: string | null
}

export type ReportArrCreateDatasetRequest = {
  datasetKey: string
  title: string
  description: string
  datasetType: string
  sourceProducts: string[]
  ownerPersonId: string
}

export type ReportArrRefreshDatasetRequest = {
  requestedByPersonId: string
}

export type ReportArrCreateDashboardRequest = {
  dashboardKey: string
  title: string
  description: string
  dashboardType: string
  defaultDateRange: string
  ownerPersonId: string
}

export type ReportArrUpdateDashboardRequest = {
  title: string
  description: string
  status: string
  defaultDateRange: string
}

export type ReportArrCreateReportDefinitionRequest = {
  reportKey: string
  title: string
  description: string
  reportType: string
  layoutDefinition: string
  exportFormats: string[]
  ownerPersonId: string
  datasetRefs?: string[]
  readModelRefs?: string[]
  parameterRefs?: string[]
  defaultFilters?: string[]
  sectionRefs?: string[]
  accessPolicyRef?: string
}

export type ReportArrUpdateReportDefinitionRequest = {
  status: string
  requestedByPersonId: string
}

export type ReportArrCreateReportRunRequest = {
  reportDefinitionId: string
  requestedByPersonId: string
  exportFormat: string | null
  parametersUsed: string[]
  filtersUsed: string[]
}

export type ReportArrCancelReportRunRequest = {
  requestedByPersonId: string
  reason: string | null
}

export type ReportArrCreateReportScheduleRequest = {
  reportDefinitionId: string
  title: string
  cadence: string
  timezone: string
  cronExpression: string | null
  deliveryMethod: string
  recipients: string[]
  parameters: string[]
  requestedByPersonId: string
}

export type ReportArrUpdateReportScheduleRequest = {
  status: string
  cadence: string
  nextRunAt: string | null
  requestedByPersonId: string
}

export type ReportArrCreateExportRequest = {
  reportRunId: string | null
  exportType: string | null
  sourceRef: string | null
  exportFormat: string
  requestedByPersonId: string
}

export type ReportArrCalculateKpiRequest = {
  periodStart: string
  periodEnd: string
  requestedByPersonId: string
}

export type ReportArrAcknowledgeAlertRequest = {
  requestedByPersonId: string
}

export type ReportArrResolveAlertRequest = {
  requestedByPersonId: string
}

export type ReportArrCreateAuditPackageRequest = {
  auditScopeId: string
  title: string
  description: string
  requestedByPersonId: string
}

export type ReportArrLockAuditPackageRequest = {
  requestedByPersonId: string
}
