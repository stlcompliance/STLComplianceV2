import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { History, RefreshCcw, Save, Settings2 } from 'lucide-react'
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { ApiErrorCallout, ConfirmDialog, getErrorMessage } from '@stl/shared-ui'

import {
  getMaintainArrTenantSettings,
  getMaintainArrTenantSettingsAudit,
  resetMaintainArrTenantSettings,
  updateMaintainArrTenantSettings,
} from '../api/client'
import type { MaintainArrTenantSettings, MaintainArrTenantSettingsAuditChange } from '../api/types'

type SectionKey = Exclude<keyof MaintainArrTenantSettings, 'schemaVersion'>

interface MaintainArrTenantSettingsPanelProps {
  accessToken: string
  canManage: boolean
  canAudit?: boolean
}

interface Option {
  value: string
  label: string
}

const operatingModeOptions: Option[] = [
  { value: 'mixed', label: 'Mixed maintenance' },
  { value: 'fleet', label: 'Fleet maintenance' },
  { value: 'facility', label: 'Facility maintenance' },
]

const strictnessOptions: Option[] = [
  { value: 'advisory', label: 'Advisory' },
  { value: 'controlled', label: 'Controlled' },
  { value: 'strict', label: 'Strict' },
]

const numberingOptions: Option[] = [
  { value: 'auto', label: 'Automatic' },
  { value: 'manual', label: 'Manual' },
]

const assetStatusOptions: Option[] = [
  { value: 'active', label: 'Active' },
  { value: 'pending_inspection', label: 'Pending inspection' },
  { value: 'out_of_service', label: 'Out of service' },
]

const priorityOptions: Option[] = [
  { value: 'low', label: 'Low' },
  { value: 'normal', label: 'Normal' },
  { value: 'high', label: 'High' },
  { value: 'urgent', label: 'Urgent' },
]

const laborModeOptions: Option[] = [
  { value: 'both', label: 'Manual or timer' },
  { value: 'manual', label: 'Manual only' },
  { value: 'timer', label: 'Timer only' },
]

const laborRoundingOptions: Option[] = [
  { value: '1', label: '1 minute' },
  { value: '5', label: '5 minutes' },
  { value: '6', label: '6 minutes' },
  { value: '10', label: '10 minutes' },
  { value: '15', label: '15 minutes' },
]

const partsReservationOptions: Option[] = [
  { value: 'none', label: 'No reservation' },
  { value: 'request_only', label: 'Request only' },
  { value: 'reserve_on_assignment', label: 'Reserve on assignment' },
]

const complianceModeOptions: Option[] = [
  { value: 'advisory', label: 'Advisory' },
  { value: 'warn', label: 'Warn users' },
  { value: 'block', label: 'Block action' },
]

const landingPageOptions: Option[] = [
  { value: 'dashboard', label: 'Dashboard' },
  { value: 'work_orders', label: 'Work orders' },
  { value: 'assets', label: 'Assets' },
  { value: 'inspections', label: 'Inspections' },
]

const fieldLabels: Record<string, string> = {
  schemaVersion: 'Schema version',
  operating: 'Operating profile',
  maintenanceOperatingMode: 'Maintenance operating mode',
  maintenanceStrictness: 'Maintenance strictness',
  assets: 'Assets',
  assetNumberingMode: 'Asset numbering',
  assetNumberPrefix: 'Asset prefix',
  requireAssetClassOnCreate: 'Require asset class',
  requireSiteOnAssetCreate: 'Require site',
  requireVinOrSerial: 'Require VIN or serial',
  defaultAssetStatus: 'Default asset status',
  workOrders: 'Work orders',
  workOrderNumberingMode: 'Work order numbering',
  workOrderNumberPrefix: 'Work order prefix',
  defaultPriority: 'Default priority',
  allowUnassignedWorkOrders: 'Allow unassigned work orders',
  requireAssetOnWorkOrder: 'Require asset on work order',
  requireLaborBeforeClose: 'Require labor before close',
  requirePartsBeforeClose: 'Require parts before close',
  requireResolutionNotesBeforeClose: 'Require resolution notes',
  allowReopenClosedWorkOrders: 'Allow reopening closed work orders',
  defects: 'Defects',
  allowOperatorDefectReports: 'Allow operator defect reports',
  allowDefectSubmissionWithoutAsset: 'Allow defect submission without asset',
  requireSeverityOnDefect: 'Require severity',
  requirePhotoForSafetyDefects: 'Require photo for safety defects',
  autoCreateWorkOrderFromDefect: 'Auto-create work order from defect',
  autoMarkAssetOOSForCriticalDefect: 'Auto-mark critical defect OOS',
  enableAIIntakeQuestions: 'Enable AI intake questions',
  aiQuestionsRequiredByDefault: 'Require AI questions by default',
  allowSubmitNowForSafetyIssue: 'Allow submit now for safety issue',
  outOfService: 'Out of service',
  enableOutOfServiceStatus: 'Enable OOS status',
  requireOOSReason: 'Require OOS reason',
  requireSupervisorApprovalForRTS: 'Require supervisor approval for RTS',
  requireInspectionBeforeRTS: 'Require inspection before RTS',
  requireAllCriticalDefectsClosedBeforeRTS: 'Require critical defects closed before RTS',
  allowRTSWithOpenMinorDefects: 'Allow RTS with open minor defects',
  preventiveMaintenance: 'Preventive maintenance',
  pmAutoGenerateWorkOrders: 'Auto-generate PM work orders',
  pmGenerateDaysAhead: 'PM generation window',
  pmGracePeriodDays: 'PM grace period',
  allowPMDeferral: 'Allow PM deferral',
  requireDeferralReason: 'Require deferral reason',
  requireApprovalForPMDeferral: 'Require approval for PM deferral',
  inspections: 'Inspections',
  inspectionAutoCreateDefects: 'Auto-create defects',
  inspectionFailureCreatesWorkOrder: 'Create work order on failed inspection',
  inspectionFailureMarksAssetOOS: 'Mark asset OOS on failed inspection',
  requireSignatureOnInspection: 'Require inspection signature',
  requirePhotoForFailedInspectionItem: 'Require photo for failed item',
  labor: 'Labor',
  enableLaborTracking: 'Enable labor tracking',
  requireLaborOnWorkOrderClose: 'Require labor on close',
  allowMultipleTechniciansPerWO: 'Allow multiple technicians',
  laborTimeEntryMode: 'Labor time entry mode',
  roundLaborMinutesTo: 'Labor rounding',
  parts: 'Parts',
  allowPartsRequestsFromWorkOrders: 'Allow parts requests',
  allowNonCatalogParts: 'Allow non-catalog parts',
  requireReasonForNonCatalogPart: 'Require non-catalog reason',
  partsReservationMode: 'Parts reservation mode',
  scheduling: 'Scheduling',
  enableMaintenanceScheduling: 'Enable scheduling',
  defaultScheduleDurationMinutes: 'Default duration',
  allowDragDropScheduling: 'Allow drag/drop scheduling',
  allowSchedulingWithoutTechnician: 'Allow scheduling without technician',
  allowSchedulingWithoutBay: 'Allow scheduling without bay',
  respectStaffArrAvailability: 'Respect StaffArr availability',
  respectTrainArrQualifications: 'Respect TrainArr qualifications',
  evidence: 'Evidence',
  enablePhotoAttachments: 'Enable photo attachments',
  requirePhotoForCriticalDefect: 'Require photo for critical defect',
  requireTechnicianSignatureOnWO: 'Require technician signature on WO',
  requireSupervisorSignatureOnRTS: 'Require supervisor signature on RTS',
  sendCompletedPacketsToRecordArr: 'Send completed packets to RecordArr',
  notifications: 'Notifications',
  notifyOnCriticalDefect: 'Critical defect',
  notifyOnAssetMarkedOOS: 'Asset marked OOS',
  notifyOnAssetReturnedToService: 'Asset returned to service',
  notifyOnPMComingDue: 'PM coming due',
  notifyOnPMOverdue: 'PM overdue',
  notifyOnWOAssigned: 'Work order assigned',
  notifyOnWOCompleted: 'Work order completed',
  pmDueNotificationDaysAhead: 'PM due notification window',
  mobile: 'Mobile and offline',
  enableMobileMode: 'Enable mobile mode',
  allowOfflineWorkOrders: 'Allow offline work orders',
  allowOfflineInspections: 'Allow offline inspections',
  allowCameraUpload: 'Allow camera upload',
  allowVoiceNotes: 'Allow voice notes',
  requireSyncBeforeClose: 'Require sync before close',
  compliance: 'Compliance',
  enableComplianceCoreChecks: 'Enable ComplianceCore checks',
  complianceCheckMode: 'Compliance check mode',
  checkComplianceOnInspectionComplete: 'Check on inspection complete',
  checkComplianceOnReturnToService: 'Check on return to service',
  showComplianceReasoningToUsers: 'Show compliance reasoning',
  integrations: 'Integrations',
  enableStaffArrPeopleLookup: 'StaffArr people lookup',
  enableStaffArrLocationLookup: 'StaffArr location lookup',
  enableTrainArrQualificationChecks: 'TrainArr qualification checks',
  enableSupplyArrPartsLookup: 'SupplyArr parts lookup',
  enableLoadArrInventoryRequests: 'LoadArr inventory requests',
  enableRoutArrReadinessEvents: 'RoutArr readiness events',
  enableRecordArrDocumentPackets: 'RecordArr document packets',
  ui: 'Interface',
  defaultLandingPage: 'Default landing page',
  showAssetHealthScore: 'Show asset health score',
  showComplianceBadges: 'Show compliance badges',
  showDowntimeMetrics: 'Show downtime metrics',
  showInternalIds: 'Show internal IDs',
}

export function MaintainArrTenantSettingsPanel({
  accessToken,
  canManage,
  canAudit = canManage,
}: MaintainArrTenantSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [draft, setDraft] = useState<MaintainArrTenantSettings | null>(null)
  const [isDirty, setIsDirty] = useState(false)
  const [changeReason, setChangeReason] = useState('')
  const [showHistory, setShowHistory] = useState(false)
  const [pendingReset, setPendingReset] = useState(false)

  const settingsQuery = useQuery({
    queryKey: ['maintainarr-tenant-settings', accessToken],
    queryFn: () => getMaintainArrTenantSettings(accessToken),
    enabled: canManage,
  })

  const auditQuery = useQuery({
    queryKey: ['maintainarr-tenant-settings-audit', accessToken],
    queryFn: () => getMaintainArrTenantSettingsAudit(accessToken, 25),
    enabled: canManage && canAudit && showHistory,
  })

  useEffect(() => {
    if (!settingsQuery.data || isDirty) {
      return
    }

    setDraft(cloneSettings(settingsQuery.data.settings))
  }, [isDirty, settingsQuery.data])

  const saveMutation = useMutation({
    mutationFn: () => {
      if (!draft) {
        throw new Error('Settings have not loaded yet.')
      }

      return updateMaintainArrTenantSettings(accessToken, {
        settings: draft,
        changeReason: changeReason.trim() || null,
      })
    },
    onSuccess: (response) => {
      setDraft(cloneSettings(response.settings))
      setChangeReason('')
      setIsDirty(false)
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-tenant-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-tenant-settings-audit', accessToken] })
    },
  })

  const resetMutation = useMutation({
    mutationFn: () =>
      resetMaintainArrTenantSettings(accessToken, {
        changeReason: changeReason.trim() || 'Reset from MaintainArr tenant settings.',
      }),
    onSuccess: (response) => {
      setDraft(cloneSettings(response.settings))
      setChangeReason('')
      setIsDirty(false)
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-tenant-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-tenant-settings-audit', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  const updateSetting = <
    S extends SectionKey,
    K extends keyof MaintainArrTenantSettings[S],
  >(
    section: S,
    key: K,
    value: MaintainArrTenantSettings[S][K],
  ) => {
    setDraft((current) => {
      if (!current) {
        return current
      }

      return {
        ...current,
        [section]: {
          ...current[section],
          [key]: value,
        },
      } as MaintainArrTenantSettings
    })
    setIsDirty(true)
  }

  const handleReset = () => {
    setPendingReset(true)
  }

  const isLoading = settingsQuery.isLoading && !draft
  const isSaving = saveMutation.isPending || resetMutation.isPending
  const lastUpdated = settingsQuery.data?.updatedAtUtc
    ? formatDate(settingsQuery.data.updatedAtUtc)
    : 'Not saved yet'

  return (
    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid="maintainarr-tenant-settings-panel"
    >
      <ConfirmDialog
        open={pendingReset}
        title="Confirm reset"
        description="Reset all MaintainArr tenant settings to the canonical defaults? This records an audit entry."
        confirmLabel="Reset"
        cancelLabel="Cancel"
        danger
        onConfirm={() => {
          setPendingReset(false)
          resetMutation.mutate()
        }}
        onCancel={() => setPendingReset(false)}
      />
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <div className="flex items-center gap-2">
            <Settings2 className="h-5 w-5 text-amber-300" aria-hidden="true" />
            <h2 className="text-lg font-semibold text-foreground">MaintainArr tenant settings</h2>
          </div>
          <p className="mt-1 max-w-3xl text-sm text-muted-foreground">
            Configure tenant-level defaults, workflow gates, evidence requirements, mobile behavior,
            and product integrations used by MaintainArr.
          </p>
          <p className="mt-1 text-xs text-muted-foreground">Last updated: {lastUpdated}</p>
        </div>

        <div className="flex flex-wrap gap-2">
          {canAudit ? (
            <button
              type="button"
              className="inline-flex items-center gap-2 rounded-md border border-input bg-background px-3 py-2 text-sm font-medium disabled:opacity-50"
              onClick={() => setShowHistory((current) => !current)}
              disabled={isSaving}
              data-testid="maintainarr-tenant-settings-history-toggle"
            >
              <History className="h-4 w-4" aria-hidden="true" />
              {showHistory ? 'Hide history' : 'Show history'}
            </button>
          ) : null}
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-md border border-input bg-background px-3 py-2 text-sm font-medium disabled:opacity-50"
            onClick={handleReset}
            disabled={!draft || isSaving}
            data-testid="maintainarr-tenant-settings-reset"
          >
            <RefreshCcw className="h-4 w-4" aria-hidden="true" />
            Reset
          </button>
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-md bg-primary px-3 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
            onClick={() => saveMutation.mutate()}
            disabled={!draft || !isDirty || isSaving}
            data-testid="maintainarr-tenant-settings-save"
          >
            <Save className="h-4 w-4" aria-hidden="true" />
            {saveMutation.isPending ? 'Saving...' : 'Save'}
          </button>
        </div>
      </div>

      {settingsQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Tenant settings unavailable"
            message={getErrorMessage(settingsQuery.error, 'Failed to load MaintainArr tenant settings.')}
            retryLabel="Retry settings"
            onRetry={() => {
              void settingsQuery.refetch()
            }}
          />
        </div>
      ) : null}

      {saveMutation.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Save failed"
            message={getErrorMessage(saveMutation.error, 'Failed to save MaintainArr tenant settings.')}
          />
        </div>
      ) : null}

      {resetMutation.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Reset failed"
            message={getErrorMessage(resetMutation.error, 'Failed to reset MaintainArr tenant settings.')}
          />
        </div>
      ) : null}

      <label className="mt-4 block text-sm" htmlFor="maintainarr-settings-change-reason">
        <span className="font-medium text-foreground">Change reason</span>
        <input
          id="maintainarr-settings-change-reason"
          className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
          maxLength={512}
          placeholder="Optional audit note"
          value={changeReason}
          onChange={(event) => setChangeReason(event.target.value)}
          data-testid="maintainarr-tenant-settings-change-reason"
        />
      </label>

      {isLoading ? <p className="mt-4 text-sm text-muted-foreground">Loading tenant settings...</p> : null}

      {draft ? (
        <div className="mt-5 space-y-6">
          <SettingsGroup
            title="Operating profile"
            description="Set the default maintenance posture for this tenant."
          >
            <SelectField
              id="maintainarr-operating-mode"
              label="Operating mode"
              value={draft.operating.maintenanceOperatingMode}
              options={operatingModeOptions}
              onChange={(value) => updateSetting('operating', 'maintenanceOperatingMode', value)}
            />
            <SelectField
              id="maintainarr-strictness-mode"
              label="Strictness"
              value={draft.operating.maintenanceStrictness}
              options={strictnessOptions}
              onChange={(value) => updateSetting('operating', 'maintenanceStrictness', value)}
              help="Strict mode turns configured workflow gates into hard requirements."
            />
          </SettingsGroup>

          <SettingsGroup title="Assets" description="Control asset creation defaults and required identifiers.">
            <SelectField
              id="maintainarr-asset-numbering"
              label="Asset numbering"
              value={draft.assets.assetNumberingMode}
              options={numberingOptions}
              onChange={(value) => updateSetting('assets', 'assetNumberingMode', value)}
            />
            <TextField
              id="maintainarr-asset-prefix"
              label="Asset prefix"
              value={draft.assets.assetNumberPrefix ?? ''}
              onChange={(value) => updateSetting('assets', 'assetNumberPrefix', value || null)}
              placeholder="AST"
              help="Letters, numbers, hyphen, or underscore; up to 16 characters."
            />
            <SelectField
              id="maintainarr-default-asset-status"
              label="Default asset status"
              value={draft.assets.defaultAssetStatus}
              options={assetStatusOptions}
              onChange={(value) => updateSetting('assets', 'defaultAssetStatus', value)}
            />
            <ToggleField
              id="maintainarr-require-asset-class"
              label="Require asset class on create"
              checked={draft.assets.requireAssetClassOnCreate}
              onChange={(value) => updateSetting('assets', 'requireAssetClassOnCreate', value)}
            />
            <ToggleField
              id="maintainarr-require-asset-site"
              label="Require site on asset create"
              checked={draft.assets.requireSiteOnAssetCreate}
              onChange={(value) => updateSetting('assets', 'requireSiteOnAssetCreate', value)}
            />
            <ToggleField
              id="maintainarr-require-vin-serial"
              label="Require VIN or serial"
              checked={draft.assets.requireVinOrSerial}
              onChange={(value) => updateSetting('assets', 'requireVinOrSerial', value)}
            />
          </SettingsGroup>

          <SettingsGroup title="Work orders" description="Set numbering, default priority, and close gates.">
            <SelectField
              id="maintainarr-work-order-numbering"
              label="Work order numbering"
              value={draft.workOrders.workOrderNumberingMode}
              options={numberingOptions}
              onChange={(value) => updateSetting('workOrders', 'workOrderNumberingMode', value)}
            />
            <TextField
              id="maintainarr-work-order-prefix"
              label="Work order prefix"
              value={draft.workOrders.workOrderNumberPrefix ?? ''}
              onChange={(value) => updateSetting('workOrders', 'workOrderNumberPrefix', value || null)}
              placeholder="WO"
              help="Used for automatic work order numbers."
            />
            <SelectField
              id="maintainarr-default-priority"
              label="Default priority"
              value={draft.workOrders.defaultPriority}
              options={priorityOptions}
              onChange={(value) => updateSetting('workOrders', 'defaultPriority', value)}
            />
            <ToggleField
              id="maintainarr-allow-unassigned-wo"
              label="Allow unassigned work orders"
              checked={draft.workOrders.allowUnassignedWorkOrders}
              onChange={(value) => updateSetting('workOrders', 'allowUnassignedWorkOrders', value)}
            />
            <ToggleField
              id="maintainarr-require-asset-wo"
              label="Require asset on work order"
              checked={draft.workOrders.requireAssetOnWorkOrder}
              onChange={(value) => updateSetting('workOrders', 'requireAssetOnWorkOrder', value)}
            />
            <ToggleField
              id="maintainarr-require-labor-close"
              label="Require labor before close"
              checked={draft.workOrders.requireLaborBeforeClose}
              onChange={(value) => updateSetting('workOrders', 'requireLaborBeforeClose', value)}
            />
            <ToggleField
              id="maintainarr-require-parts-close"
              label="Require parts before close"
              checked={draft.workOrders.requirePartsBeforeClose}
              onChange={(value) => updateSetting('workOrders', 'requirePartsBeforeClose', value)}
            />
            <ToggleField
              id="maintainarr-require-resolution-close"
              label="Require resolution notes before close"
              checked={draft.workOrders.requireResolutionNotesBeforeClose}
              onChange={(value) => updateSetting('workOrders', 'requireResolutionNotesBeforeClose', value)}
            />
            <ToggleField
              id="maintainarr-allow-reopen-wo"
              label="Allow reopening closed work orders"
              checked={draft.workOrders.allowReopenClosedWorkOrders}
              onChange={(value) => updateSetting('workOrders', 'allowReopenClosedWorkOrders', value)}
            />
          </SettingsGroup>

          <SettingsGroup title="Defects" description="Configure operator defect intake and critical defect behavior.">
            <ToggleField
              id="maintainarr-allow-operator-defects"
              label="Allow operator defect reports"
              checked={draft.defects.allowOperatorDefectReports}
              onChange={(value) => updateSetting('defects', 'allowOperatorDefectReports', value)}
            />
            <ToggleField
              id="maintainarr-allow-defect-without-asset"
              label="Allow defect submission without asset"
              checked={draft.defects.allowDefectSubmissionWithoutAsset}
              onChange={(value) => updateSetting('defects', 'allowDefectSubmissionWithoutAsset', value)}
              help="Current defect records still keep any supplied asset reference as a stable MaintainArr reference."
            />
            <ToggleField
              id="maintainarr-require-defect-severity"
              label="Require severity on defect"
              checked={draft.defects.requireSeverityOnDefect}
              onChange={(value) => updateSetting('defects', 'requireSeverityOnDefect', value)}
            />
            <ToggleField
              id="maintainarr-photo-safety-defects"
              label="Require photo for safety defects"
              checked={draft.defects.requirePhotoForSafetyDefects}
              onChange={(value) => updateSetting('defects', 'requirePhotoForSafetyDefects', value)}
              disabled={!draft.evidence.enablePhotoAttachments}
              help="Depends on photo attachments being enabled."
            />
            <ToggleField
              id="maintainarr-defect-auto-wo"
              label="Auto-create work order from defect"
              checked={draft.defects.autoCreateWorkOrderFromDefect}
              onChange={(value) => updateSetting('defects', 'autoCreateWorkOrderFromDefect', value)}
            />
            <ToggleField
              id="maintainarr-defect-critical-oos"
              label="Auto-mark asset OOS for critical defect"
              checked={draft.defects.autoMarkAssetOOSForCriticalDefect}
              onChange={(value) => updateSetting('defects', 'autoMarkAssetOOSForCriticalDefect', value)}
              disabled={!draft.outOfService.enableOutOfServiceStatus}
              help="Depends on out-of-service status being enabled."
            />
            <ToggleField
              id="maintainarr-ai-intake"
              label="Enable AI intake questions"
              checked={draft.defects.enableAIIntakeQuestions}
              onChange={(value) => updateSetting('defects', 'enableAIIntakeQuestions', value)}
            />
            <ToggleField
              id="maintainarr-ai-required-default"
              label="Require AI questions by default"
              checked={draft.defects.aiQuestionsRequiredByDefault}
              onChange={(value) => updateSetting('defects', 'aiQuestionsRequiredByDefault', value)}
              disabled={!draft.defects.enableAIIntakeQuestions}
            />
            <ToggleField
              id="maintainarr-submit-now-safety"
              label="Allow submit now for safety issue"
              checked={draft.defects.allowSubmitNowForSafetyIssue}
              onChange={(value) => updateSetting('defects', 'allowSubmitNowForSafetyIssue', value)}
            />
          </SettingsGroup>

          <SettingsGroup title="Out of service and return to service" description="Set RTS review gates.">
            <ToggleField
              id="maintainarr-enable-oos"
              label="Enable out-of-service status"
              checked={draft.outOfService.enableOutOfServiceStatus}
              onChange={(value) => updateSetting('outOfService', 'enableOutOfServiceStatus', value)}
            />
            <ToggleField
              id="maintainarr-oos-reason"
              label="Require OOS reason"
              checked={draft.outOfService.requireOOSReason}
              onChange={(value) => updateSetting('outOfService', 'requireOOSReason', value)}
              disabled={!draft.outOfService.enableOutOfServiceStatus}
            />
            <ToggleField
              id="maintainarr-rts-supervisor"
              label="Require supervisor approval for RTS"
              checked={draft.outOfService.requireSupervisorApprovalForRTS}
              onChange={(value) => updateSetting('outOfService', 'requireSupervisorApprovalForRTS', value)}
              disabled={!draft.outOfService.enableOutOfServiceStatus}
            />
            <ToggleField
              id="maintainarr-rts-inspection"
              label="Require inspection before RTS"
              checked={draft.outOfService.requireInspectionBeforeRTS}
              onChange={(value) => updateSetting('outOfService', 'requireInspectionBeforeRTS', value)}
              disabled={!draft.outOfService.enableOutOfServiceStatus}
            />
            <ToggleField
              id="maintainarr-rts-critical-closed"
              label="Require all critical defects closed before RTS"
              checked={draft.outOfService.requireAllCriticalDefectsClosedBeforeRTS}
              onChange={(value) => updateSetting('outOfService', 'requireAllCriticalDefectsClosedBeforeRTS', value)}
              disabled={!draft.outOfService.enableOutOfServiceStatus}
            />
            <ToggleField
              id="maintainarr-rts-open-minor"
              label="Allow RTS with open minor defects"
              checked={draft.outOfService.allowRTSWithOpenMinorDefects}
              onChange={(value) => updateSetting('outOfService', 'allowRTSWithOpenMinorDefects', value)}
              disabled={!draft.outOfService.enableOutOfServiceStatus}
            />
          </SettingsGroup>

          <SettingsGroup title="Preventive maintenance" description="Control PM generation and deferral rules.">
            <ToggleField
              id="maintainarr-pm-auto-wo"
              label="Auto-generate work orders"
              checked={draft.preventiveMaintenance.pmAutoGenerateWorkOrders}
              onChange={(value) => updateSetting('preventiveMaintenance', 'pmAutoGenerateWorkOrders', value)}
            />
            <NumberField
              id="maintainarr-pm-days-ahead"
              label="Generate days ahead"
              value={draft.preventiveMaintenance.pmGenerateDaysAhead}
              min={0}
              max={365}
              onChange={(value) => updateSetting('preventiveMaintenance', 'pmGenerateDaysAhead', value)}
            />
            <NumberField
              id="maintainarr-pm-grace-days"
              label="Grace period days"
              value={draft.preventiveMaintenance.pmGracePeriodDays}
              min={0}
              max={365}
              onChange={(value) => updateSetting('preventiveMaintenance', 'pmGracePeriodDays', value)}
            />
            <ToggleField
              id="maintainarr-pm-deferral"
              label="Allow PM deferral"
              checked={draft.preventiveMaintenance.allowPMDeferral}
              onChange={(value) => updateSetting('preventiveMaintenance', 'allowPMDeferral', value)}
            />
            <ToggleField
              id="maintainarr-pm-deferral-reason"
              label="Require deferral reason"
              checked={draft.preventiveMaintenance.requireDeferralReason}
              onChange={(value) => updateSetting('preventiveMaintenance', 'requireDeferralReason', value)}
              disabled={!draft.preventiveMaintenance.allowPMDeferral}
            />
            <ToggleField
              id="maintainarr-pm-deferral-approval"
              label="Require approval for PM deferral"
              checked={draft.preventiveMaintenance.requireApprovalForPMDeferral}
              onChange={(value) => updateSetting('preventiveMaintenance', 'requireApprovalForPMDeferral', value)}
              disabled={!draft.preventiveMaintenance.allowPMDeferral}
            />
          </SettingsGroup>

          <SettingsGroup title="Inspections" description="Set behavior for failed inspection findings.">
            <ToggleField
              id="maintainarr-inspection-auto-defects"
              label="Auto-create defects"
              checked={draft.inspections.inspectionAutoCreateDefects}
              onChange={(value) => updateSetting('inspections', 'inspectionAutoCreateDefects', value)}
            />
            <ToggleField
              id="maintainarr-inspection-fail-wo"
              label="Create work order on failed inspection"
              checked={draft.inspections.inspectionFailureCreatesWorkOrder}
              onChange={(value) => updateSetting('inspections', 'inspectionFailureCreatesWorkOrder', value)}
            />
            <ToggleField
              id="maintainarr-inspection-fail-oos"
              label="Mark asset OOS on failed inspection"
              checked={draft.inspections.inspectionFailureMarksAssetOOS}
              onChange={(value) => updateSetting('inspections', 'inspectionFailureMarksAssetOOS', value)}
              disabled={!draft.outOfService.enableOutOfServiceStatus}
            />
            <ToggleField
              id="maintainarr-inspection-signature"
              label="Require signature on inspection"
              checked={draft.inspections.requireSignatureOnInspection}
              onChange={(value) => updateSetting('inspections', 'requireSignatureOnInspection', value)}
            />
            <ToggleField
              id="maintainarr-inspection-photo-failed"
              label="Require photo for failed item"
              checked={draft.inspections.requirePhotoForFailedInspectionItem}
              onChange={(value) => updateSetting('inspections', 'requirePhotoForFailedInspectionItem', value)}
              disabled={!draft.evidence.enablePhotoAttachments}
            />
          </SettingsGroup>

          <SettingsGroup title="Labor" description="Configure labor capture for work orders.">
            <ToggleField
              id="maintainarr-enable-labor"
              label="Enable labor tracking"
              checked={draft.labor.enableLaborTracking}
              onChange={(value) => updateSetting('labor', 'enableLaborTracking', value)}
            />
            <ToggleField
              id="maintainarr-labor-close"
              label="Require labor on work order close"
              checked={draft.labor.requireLaborOnWorkOrderClose}
              onChange={(value) => updateSetting('labor', 'requireLaborOnWorkOrderClose', value)}
              disabled={!draft.labor.enableLaborTracking}
            />
            <ToggleField
              id="maintainarr-multiple-techs"
              label="Allow multiple technicians per work order"
              checked={draft.labor.allowMultipleTechniciansPerWO}
              onChange={(value) => updateSetting('labor', 'allowMultipleTechniciansPerWO', value)}
              disabled={!draft.labor.enableLaborTracking}
            />
            <SelectField
              id="maintainarr-labor-entry-mode"
              label="Labor time entry mode"
              value={draft.labor.laborTimeEntryMode}
              options={laborModeOptions}
              onChange={(value) => updateSetting('labor', 'laborTimeEntryMode', value)}
              disabled={!draft.labor.enableLaborTracking}
            />
            <SelectField
              id="maintainarr-labor-rounding"
              label="Round labor minutes to"
              value={String(draft.labor.roundLaborMinutesTo)}
              options={laborRoundingOptions}
              onChange={(value) => updateSetting('labor', 'roundLaborMinutesTo', Number(value))}
              disabled={!draft.labor.enableLaborTracking}
            />
          </SettingsGroup>

          <SettingsGroup title="Parts" description="Control parts requests and reservation behavior.">
            <ToggleField
              id="maintainarr-parts-requests"
              label="Allow parts requests from work orders"
              checked={draft.parts.allowPartsRequestsFromWorkOrders}
              onChange={(value) => updateSetting('parts', 'allowPartsRequestsFromWorkOrders', value)}
            />
            <ToggleField
              id="maintainarr-non-catalog-parts"
              label="Allow non-catalog parts"
              checked={draft.parts.allowNonCatalogParts}
              onChange={(value) => updateSetting('parts', 'allowNonCatalogParts', value)}
              disabled={!draft.parts.allowPartsRequestsFromWorkOrders}
            />
            <ToggleField
              id="maintainarr-non-catalog-reason"
              label="Require reason for non-catalog part"
              checked={draft.parts.requireReasonForNonCatalogPart}
              onChange={(value) => updateSetting('parts', 'requireReasonForNonCatalogPart', value)}
              disabled={!draft.parts.allowPartsRequestsFromWorkOrders || !draft.parts.allowNonCatalogParts}
            />
            <SelectField
              id="maintainarr-parts-reservation"
              label="Parts reservation mode"
              value={draft.parts.partsReservationMode}
              options={partsReservationOptions}
              onChange={(value) => updateSetting('parts', 'partsReservationMode', value)}
              disabled={!draft.parts.allowPartsRequestsFromWorkOrders}
            />
          </SettingsGroup>

          <SettingsGroup title="Scheduling" description="Configure maintenance calendar defaults and guards.">
            <ToggleField
              id="maintainarr-enable-scheduling"
              label="Enable maintenance scheduling"
              checked={draft.scheduling.enableMaintenanceScheduling}
              onChange={(value) => updateSetting('scheduling', 'enableMaintenanceScheduling', value)}
            />
            <NumberField
              id="maintainarr-default-duration"
              label="Default duration minutes"
              value={draft.scheduling.defaultScheduleDurationMinutes}
              min={5}
              max={1440}
              onChange={(value) => updateSetting('scheduling', 'defaultScheduleDurationMinutes', value)}
              disabled={!draft.scheduling.enableMaintenanceScheduling}
            />
            <ToggleField
              id="maintainarr-drag-drop-scheduling"
              label="Allow drag/drop scheduling"
              checked={draft.scheduling.allowDragDropScheduling}
              onChange={(value) => updateSetting('scheduling', 'allowDragDropScheduling', value)}
              disabled={!draft.scheduling.enableMaintenanceScheduling}
            />
            <ToggleField
              id="maintainarr-schedule-without-tech"
              label="Allow scheduling without technician"
              checked={draft.scheduling.allowSchedulingWithoutTechnician}
              onChange={(value) => updateSetting('scheduling', 'allowSchedulingWithoutTechnician', value)}
              disabled={!draft.scheduling.enableMaintenanceScheduling}
            />
            <ToggleField
              id="maintainarr-schedule-without-bay"
              label="Allow scheduling without bay"
              checked={draft.scheduling.allowSchedulingWithoutBay}
              onChange={(value) => updateSetting('scheduling', 'allowSchedulingWithoutBay', value)}
              disabled={!draft.scheduling.enableMaintenanceScheduling}
            />
            <ToggleField
              id="maintainarr-respect-staffarr"
              label="Respect StaffArr availability"
              checked={draft.scheduling.respectStaffArrAvailability}
              onChange={(value) => updateSetting('scheduling', 'respectStaffArrAvailability', value)}
              disabled={!draft.scheduling.enableMaintenanceScheduling || !draft.integrations.enableStaffArrPeopleLookup}
            />
            <ToggleField
              id="maintainarr-respect-trainarr"
              label="Respect TrainArr qualifications"
              checked={draft.scheduling.respectTrainArrQualifications}
              onChange={(value) => updateSetting('scheduling', 'respectTrainArrQualifications', value)}
              disabled={!draft.scheduling.enableMaintenanceScheduling || !draft.integrations.enableTrainArrQualificationChecks}
              help="Requires TrainArr entitlement and qualification checks."
            />
          </SettingsGroup>

          <SettingsGroup title="Evidence" description="Set photo and signature requirements.">
            <ToggleField
              id="maintainarr-enable-photos"
              label="Enable photo attachments"
              checked={draft.evidence.enablePhotoAttachments}
              onChange={(value) => updateSetting('evidence', 'enablePhotoAttachments', value)}
            />
            <ToggleField
              id="maintainarr-photo-critical"
              label="Require photo for critical defect"
              checked={draft.evidence.requirePhotoForCriticalDefect}
              onChange={(value) => updateSetting('evidence', 'requirePhotoForCriticalDefect', value)}
              disabled={!draft.evidence.enablePhotoAttachments}
            />
            <ToggleField
              id="maintainarr-technician-signature"
              label="Require technician signature on work order"
              checked={draft.evidence.requireTechnicianSignatureOnWO}
              onChange={(value) => updateSetting('evidence', 'requireTechnicianSignatureOnWO', value)}
            />
            <ToggleField
              id="maintainarr-rts-signature"
              label="Require supervisor signature on RTS"
              checked={draft.evidence.requireSupervisorSignatureOnRTS}
              onChange={(value) => updateSetting('evidence', 'requireSupervisorSignatureOnRTS', value)}
              disabled={!draft.outOfService.enableOutOfServiceStatus}
            />
            <ToggleField
              id="maintainarr-recordarr-packets"
              label="Send completed packets to RecordArr"
              checked={draft.evidence.sendCompletedPacketsToRecordArr}
              onChange={(value) => updateSetting('evidence', 'sendCompletedPacketsToRecordArr', value)}
              disabled={!draft.integrations.enableRecordArrDocumentPackets}
            />
          </SettingsGroup>

          <SettingsGroup title="Notifications" description="Choose which maintenance events raise notifications.">
            <ToggleField
              id="maintainarr-notify-critical"
              label="Notify on critical defect"
              checked={draft.notifications.notifyOnCriticalDefect}
              onChange={(value) => updateSetting('notifications', 'notifyOnCriticalDefect', value)}
            />
            <ToggleField
              id="maintainarr-notify-oos"
              label="Notify on asset marked OOS"
              checked={draft.notifications.notifyOnAssetMarkedOOS}
              onChange={(value) => updateSetting('notifications', 'notifyOnAssetMarkedOOS', value)}
              disabled={!draft.outOfService.enableOutOfServiceStatus}
            />
            <ToggleField
              id="maintainarr-notify-rts"
              label="Notify on asset returned to service"
              checked={draft.notifications.notifyOnAssetReturnedToService}
              onChange={(value) => updateSetting('notifications', 'notifyOnAssetReturnedToService', value)}
              disabled={!draft.outOfService.enableOutOfServiceStatus}
            />
            <ToggleField
              id="maintainarr-notify-pm-due"
              label="Notify on PM coming due"
              checked={draft.notifications.notifyOnPMComingDue}
              onChange={(value) => updateSetting('notifications', 'notifyOnPMComingDue', value)}
            />
            <ToggleField
              id="maintainarr-notify-pm-overdue"
              label="Notify on PM overdue"
              checked={draft.notifications.notifyOnPMOverdue}
              onChange={(value) => updateSetting('notifications', 'notifyOnPMOverdue', value)}
            />
            <ToggleField
              id="maintainarr-notify-wo-assigned"
              label="Notify on work order assigned"
              checked={draft.notifications.notifyOnWOAssigned}
              onChange={(value) => updateSetting('notifications', 'notifyOnWOAssigned', value)}
            />
            <ToggleField
              id="maintainarr-notify-wo-completed"
              label="Notify on work order completed"
              checked={draft.notifications.notifyOnWOCompleted}
              onChange={(value) => updateSetting('notifications', 'notifyOnWOCompleted', value)}
            />
            <NumberField
              id="maintainarr-pm-notification-days"
              label="PM due notification days ahead"
              value={draft.notifications.pmDueNotificationDaysAhead}
              min={0}
              max={365}
              onChange={(value) => updateSetting('notifications', 'pmDueNotificationDaysAhead', value)}
            />
          </SettingsGroup>

          <SettingsGroup title="Mobile and offline" description="Set mobile capture and sync expectations.">
            <ToggleField
              id="maintainarr-enable-mobile"
              label="Enable mobile mode"
              checked={draft.mobile.enableMobileMode}
              onChange={(value) => updateSetting('mobile', 'enableMobileMode', value)}
            />
            <ToggleField
              id="maintainarr-offline-work-orders"
              label="Allow offline work orders"
              checked={draft.mobile.allowOfflineWorkOrders}
              onChange={(value) => updateSetting('mobile', 'allowOfflineWorkOrders', value)}
              disabled={!draft.mobile.enableMobileMode}
            />
            <ToggleField
              id="maintainarr-offline-inspections"
              label="Allow offline inspections"
              checked={draft.mobile.allowOfflineInspections}
              onChange={(value) => updateSetting('mobile', 'allowOfflineInspections', value)}
              disabled={!draft.mobile.enableMobileMode}
            />
            <ToggleField
              id="maintainarr-camera-upload"
              label="Allow camera upload"
              checked={draft.mobile.allowCameraUpload}
              onChange={(value) => updateSetting('mobile', 'allowCameraUpload', value)}
              disabled={!draft.mobile.enableMobileMode || !draft.evidence.enablePhotoAttachments}
            />
            <ToggleField
              id="maintainarr-voice-notes"
              label="Allow voice notes"
              checked={draft.mobile.allowVoiceNotes}
              onChange={(value) => updateSetting('mobile', 'allowVoiceNotes', value)}
              disabled={!draft.mobile.enableMobileMode}
            />
            <ToggleField
              id="maintainarr-sync-before-close"
              label="Require sync before close"
              checked={draft.mobile.requireSyncBeforeClose}
              onChange={(value) => updateSetting('mobile', 'requireSyncBeforeClose', value)}
              disabled={!draft.mobile.enableMobileMode}
            />
          </SettingsGroup>

          <SettingsGroup title="Compliance" description="Control ComplianceCore checks from MaintainArr workflows.">
            <ToggleField
              id="maintainarr-enable-compliance"
              label="Enable ComplianceCore checks"
              checked={draft.compliance.enableComplianceCoreChecks}
              onChange={(value) => updateSetting('compliance', 'enableComplianceCoreChecks', value)}
            />
            <SelectField
              id="maintainarr-compliance-mode"
              label="Compliance check mode"
              value={draft.compliance.complianceCheckMode}
              options={complianceModeOptions}
              onChange={(value) => updateSetting('compliance', 'complianceCheckMode', value)}
              disabled={!draft.compliance.enableComplianceCoreChecks}
            />
            <ToggleField
              id="maintainarr-compliance-inspection"
              label="Check on inspection complete"
              checked={draft.compliance.checkComplianceOnInspectionComplete}
              onChange={(value) => updateSetting('compliance', 'checkComplianceOnInspectionComplete', value)}
              disabled={!draft.compliance.enableComplianceCoreChecks}
            />
            <ToggleField
              id="maintainarr-compliance-rts"
              label="Check on return to service"
              checked={draft.compliance.checkComplianceOnReturnToService}
              onChange={(value) => updateSetting('compliance', 'checkComplianceOnReturnToService', value)}
              disabled={!draft.compliance.enableComplianceCoreChecks}
            />
            <ToggleField
              id="maintainarr-compliance-reasoning"
              label="Show compliance reasoning to users"
              checked={draft.compliance.showComplianceReasoningToUsers}
              onChange={(value) => updateSetting('compliance', 'showComplianceReasoningToUsers', value)}
              disabled={!draft.compliance.enableComplianceCoreChecks}
            />
          </SettingsGroup>

          <SettingsGroup
            title="Integrations"
            description="Use stable lookups when linking related records."
          >
            <ToggleField
              id="maintainarr-staffarr-people"
              label="Enable StaffArr people lookup"
              checked={draft.integrations.enableStaffArrPeopleLookup}
              onChange={(value) => updateSetting('integrations', 'enableStaffArrPeopleLookup', value)}
            />
            <ToggleField
              id="maintainarr-staffarr-location"
              label="Enable StaffArr location lookup"
              checked={draft.integrations.enableStaffArrLocationLookup}
              onChange={(value) => updateSetting('integrations', 'enableStaffArrLocationLookup', value)}
            />
            <ToggleField
              id="maintainarr-trainarr-qualifications"
              label="Enable TrainArr qualification checks"
              checked={draft.integrations.enableTrainArrQualificationChecks}
              onChange={(value) => updateSetting('integrations', 'enableTrainArrQualificationChecks', value)}
              help="Requires TrainArr entitlement."
            />
            <ToggleField
              id="maintainarr-supplyarr-parts"
              label="Enable SupplyArr parts lookup"
              checked={draft.integrations.enableSupplyArrPartsLookup}
              onChange={(value) => updateSetting('integrations', 'enableSupplyArrPartsLookup', value)}
            />
            <ToggleField
              id="maintainarr-loadarr-inventory"
              label="Enable LoadArr inventory requests"
              checked={draft.integrations.enableLoadArrInventoryRequests}
              onChange={(value) => updateSetting('integrations', 'enableLoadArrInventoryRequests', value)}
            />
            <ToggleField
              id="maintainarr-routarr-readiness"
              label="Enable RoutArr readiness events"
              checked={draft.integrations.enableRoutArrReadinessEvents}
              onChange={(value) => updateSetting('integrations', 'enableRoutArrReadinessEvents', value)}
            />
            <ToggleField
              id="maintainarr-recordarr-packets-integration"
              label="Enable RecordArr document packets"
              checked={draft.integrations.enableRecordArrDocumentPackets}
              onChange={(value) => updateSetting('integrations', 'enableRecordArrDocumentPackets', value)}
            />
          </SettingsGroup>

          <SettingsGroup title="Interface" description="Choose MaintainArr visibility defaults for this tenant.">
            <SelectField
              id="maintainarr-default-landing-page"
              label="Default landing page"
              value={draft.ui.defaultLandingPage}
              options={landingPageOptions}
              onChange={(value) => updateSetting('ui', 'defaultLandingPage', value)}
            />
            <ToggleField
              id="maintainarr-show-health-score"
              label="Show asset health score"
              checked={draft.ui.showAssetHealthScore}
              onChange={(value) => updateSetting('ui', 'showAssetHealthScore', value)}
            />
            <ToggleField
              id="maintainarr-show-compliance-badges"
              label="Show compliance badges"
              checked={draft.ui.showComplianceBadges}
              onChange={(value) => updateSetting('ui', 'showComplianceBadges', value)}
            />
            <ToggleField
              id="maintainarr-show-downtime"
              label="Show downtime metrics"
              checked={draft.ui.showDowntimeMetrics}
              onChange={(value) => updateSetting('ui', 'showDowntimeMetrics', value)}
            />
            <ToggleField
              id="maintainarr-show-internal-ids"
              label="Show internal IDs"
              checked={draft.ui.showInternalIds}
              onChange={(value) => updateSetting('ui', 'showInternalIds', value)}
              help="Keep disabled for normal operations."
            />
          </SettingsGroup>
        </div>
      ) : null}

      {showHistory && canAudit ? (
        <div className="mt-6 border-t border-border pt-4" data-testid="maintainarr-tenant-settings-history">
          <h3 className="text-sm font-semibold text-foreground">Settings history</h3>
          {auditQuery.isLoading ? (
            <p className="mt-2 text-sm text-muted-foreground">Loading settings history...</p>
          ) : null}
          {auditQuery.isError ? (
            <div className="mt-3">
              <ApiErrorCallout
                title="History unavailable"
                message={getErrorMessage(auditQuery.error, 'Failed to load MaintainArr settings history.')}
                retryLabel="Retry history"
                onRetry={() => {
                  void auditQuery.refetch()
                }}
              />
            </div>
          ) : null}
          {auditQuery.data && auditQuery.data.items.length === 0 ? (
            <p className="mt-2 text-sm text-muted-foreground">No settings changes have been recorded yet.</p>
          ) : null}
          {auditQuery.data && auditQuery.data.items.length > 0 ? (
            <ul className="mt-3 divide-y divide-border rounded-md border border-border text-sm">
              {auditQuery.data.items.map((item, index) => (
                <li key={`${item.changedAtUtc}-${index}`} className="px-3 py-3">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <span className="font-medium text-foreground">{formatDate(item.changedAtUtc)}</span>
                    <span className="text-xs text-muted-foreground">
                      {item.changes.length} {item.changes.length === 1 ? 'change' : 'changes'}
                    </span>
                  </div>
                  {item.changeReason ? (
                    <p className="mt-1 text-xs text-muted-foreground">{item.changeReason}</p>
                  ) : null}
                  {item.changes.length > 0 ? (
                    <ul className="mt-2 space-y-1 text-xs text-muted-foreground">
                      {item.changes.slice(0, 8).map((change) => (
                        <li key={change.path}>
                          <span className="font-medium text-foreground">{formatPath(change.path)}:</span>{' '}
                          {formatChange(change)}
                        </li>
                      ))}
                      {item.changes.length > 8 ? <li>{item.changes.length - 8} more changes</li> : null}
                    </ul>
                  ) : (
                    <p className="mt-2 text-xs text-muted-foreground">No behavior fields changed.</p>
                  )}
                </li>
              ))}
            </ul>
          ) : null}
        </div>
      ) : null}
    </section>
  )
}

function SettingsGroup({
  title,
  description,
  children,
}: {
  title: string
  description: string
  children: ReactNode
}) {
  return (
    <fieldset className="border-t border-border pt-4">
      <legend className="pr-3 text-sm font-semibold text-foreground">{title}</legend>
      <p className="mb-3 text-sm text-muted-foreground">{description}</p>
      <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">{children}</div>
    </fieldset>
  )
}

function ToggleField({
  id,
  label,
  checked,
  onChange,
  disabled = false,
  help,
}: {
  id: string
  label: string
  checked: boolean
  onChange: (value: boolean) => void
  disabled?: boolean
  help?: string
}) {
  const helpId = help ? `${id}-help` : undefined

  return (
    <label
      className={`flex min-h-16 items-start gap-3 rounded-md border border-border bg-background/40 p-3 text-sm ${
        disabled ? 'opacity-60' : ''
      }`}
      htmlFor={id}
    >
      <input
        id={id}
        type="checkbox"
        className="mt-1"
        checked={checked}
        onChange={(event) => onChange(event.target.checked)}
        disabled={disabled}
        aria-describedby={helpId}
      />
      <span>
        <span className="font-medium text-foreground">{label}</span>
        {help ? (
          <span id={helpId} className="mt-1 block text-xs text-muted-foreground">
            {help}
          </span>
        ) : null}
      </span>
    </label>
  )
}

function SelectField({
  id,
  label,
  value,
  options,
  onChange,
  disabled = false,
  help,
}: {
  id: string
  label: string
  value: string
  options: Option[]
  onChange: (value: string) => void
  disabled?: boolean
  help?: string
}) {
  const helpId = help ? `${id}-help` : undefined

  return (
    <label className="block text-sm" htmlFor={id}>
      <span className="font-medium text-foreground">{label}</span>
      <select
        id={id}
        className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        disabled={disabled}
        aria-describedby={helpId}
      >
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      {help ? (
        <span id={helpId} className="mt-1 block text-xs text-muted-foreground">
          {help}
        </span>
      ) : null}
    </label>
  )
}

function NumberField({
  id,
  label,
  value,
  min,
  max,
  onChange,
  disabled = false,
}: {
  id: string
  label: string
  value: number
  min: number
  max: number
  onChange: (value: number) => void
  disabled?: boolean
}) {
  return (
    <label className="block text-sm" htmlFor={id}>
      <span className="font-medium text-foreground">{label}</span>
      <input
        id={id}
        className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2"
        type="number"
        min={min}
        max={max}
        value={value}
        onChange={(event) => onChange(Number(event.target.value))}
        disabled={disabled}
      />
      <span className="mt-1 block text-xs text-muted-foreground">
        Allowed range: {min} to {max}.
      </span>
    </label>
  )
}

function TextField({
  id,
  label,
  value,
  onChange,
  placeholder,
  help,
}: {
  id: string
  label: string
  value: string
  onChange: (value: string) => void
  placeholder?: string
  help?: string
}) {
  const helpId = help ? `${id}-help` : undefined

  return (
    <label className="block text-sm" htmlFor={id}>
      <span className="font-medium text-foreground">{label}</span>
      <input
        id={id}
        className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2"
        type="text"
        value={value}
        placeholder={placeholder}
        maxLength={16}
        onChange={(event) => onChange(event.target.value)}
        aria-describedby={helpId}
      />
      {help ? (
        <span id={helpId} className="mt-1 block text-xs text-muted-foreground">
          {help}
        </span>
      ) : null}
    </label>
  )
}

function cloneSettings(settings: MaintainArrTenantSettings): MaintainArrTenantSettings {
  return JSON.parse(JSON.stringify(settings)) as MaintainArrTenantSettings
}

function formatDate(value: string): string {
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? value : date.toLocaleString()
}

function formatPath(path: string): string {
  return path
    .split('.')
    .map((part) => fieldLabels[part] ?? toTitleCase(part))
    .join(' / ')
}

function formatChange(change: MaintainArrTenantSettingsAuditChange): string {
  return `${formatValue(change.before)} to ${formatValue(change.after)}`
}

function formatValue(value: string | null): string {
  if (value == null || value === '') {
    return 'not set'
  }

  if (value === 'true') {
    return 'enabled'
  }

  if (value === 'false') {
    return 'disabled'
  }

  return fieldLabels[value] ?? value.replaceAll('_', ' ')
}

function toTitleCase(value: string): string {
  return value
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replaceAll('_', ' ')
    .replace(/\b\w/g, (letter) => letter.toUpperCase())
}
