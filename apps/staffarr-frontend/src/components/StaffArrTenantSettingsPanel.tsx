import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { RefreshCcw, RotateCcw, Save } from 'lucide-react'
import {
  getStaffArrTenantSettings,
  getStaffArrTenantSettingsDefaults,
  updateStaffArrTenantSettings,
} from '../api/client'
import type {
  StaffArrTenantSettingsResponse,
  UpsertStaffArrTenantSettingsRequest,
} from '../api/types'

type StaffArrTenantSettingsPanelProps = {
  accessToken: string
  canManage: boolean
}

type SettingsDraft = UpsertStaffArrTenantSettingsRequest
type SettingsTab = 'people' | 'org' | 'roles' | 'incidents' | 'governance' | 'integrations'

const tabs: Array<{ key: SettingsTab; label: string }> = [
  { key: 'people', label: 'People' },
  { key: 'org', label: 'Org & Locations' },
  { key: 'roles', label: 'Roles & Permissions' },
  { key: 'incidents', label: 'Incidents' },
  { key: 'governance', label: 'Governance' },
  { key: 'integrations', label: 'Integrations' },
]

const displayNameOptions = [
  ['preferred_first_last', 'Preferred first + last'],
  ['legal_first_last', 'Legal first + last'],
  ['last_first', 'Last, first'],
  ['first_last_employee_number', 'First + last'],
]

const employmentStatusOptions = [
  ['applicant', 'Applicant'],
  ['pending_start', 'Pending start'],
  ['onboarding', 'Onboarding'],
  ['active', 'Active'],
  ['leave', 'Leave'],
  ['suspended', 'Suspended'],
  ['terminated', 'Terminated'],
  ['inactive', 'Inactive'],
  ['archived', 'Archived'],
]

const profileSectionOptions = [
  ['identity', 'Identity'],
  ['work', 'Work'],
  ['contact', 'Contact'],
  ['emergency', 'Emergency'],
  ['address', 'Address'],
  ['photo', 'Photo'],
]

function toDraft(settings: StaffArrTenantSettingsResponse): SettingsDraft {
  return {
    personDirectory: { ...settings.personDirectory },
    personLifecycle: { ...settings.personLifecycle },
    orgStructure: { ...settings.orgStructure },
    locationHierarchy: { ...settings.locationHierarchy },
    rolePermissions: { ...settings.rolePermissions },
    teamsAssignments: { ...settings.teamsAssignments },
    incidents: { ...settings.incidents },
    profileFieldGovernance: {
      ...settings.profileFieldGovernance,
      requiredProfileSections: [...settings.profileFieldGovernance.requiredProfileSections],
      optionalProfileSections: [...settings.profileFieldGovernance.optionalProfileSections],
    },
    notificationsReviews: { ...settings.notificationsReviews },
    dataGovernanceAudit: { ...settings.dataGovernanceAudit },
    crossProductReferences: { ...settings.crossProductReferences },
  }
}

function cloneDraft(draft: SettingsDraft): SettingsDraft {
  return structuredClone(draft)
}

function sameDraft(left: SettingsDraft | null, right: SettingsDraft | null): boolean {
  return JSON.stringify(left) === JSON.stringify(right)
}

function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'Something went wrong.'
}

function validateDraft(draft: SettingsDraft): string | null {
  if (!draft.personDirectory.employeeNumberLabel.trim()) {
    return 'Employee number label is required.'
  }

  if (
    draft.rolePermissions.roleExpirationEnabled &&
    (!draft.rolePermissions.defaultRoleGrantDurationDays ||
      draft.rolePermissions.defaultRoleGrantDurationDays <= 0)
  ) {
    return 'Default role grant duration must be positive when role expiration is enabled.'
  }

  if (
    draft.teamsAssignments.allowTemporaryAssignments &&
    (!draft.teamsAssignments.temporaryAssignmentMaxDurationDays ||
      draft.teamsAssignments.temporaryAssignmentMaxDurationDays <= 0)
  ) {
    return 'Temporary assignment maximum must be positive when temporary assignments are enabled.'
  }

  if (
    draft.incidents.trainArrRoutingEnabled &&
    (!draft.incidents.retrainingRecommendationThreshold ||
      draft.incidents.retrainingRecommendationThreshold <= 0)
  ) {
    return 'Retraining recommendation threshold must be positive when TrainArr routing is enabled.'
  }

  if (
    draft.dataGovernanceAudit.recordRetentionHintDays != null &&
    draft.dataGovernanceAudit.recordRetentionHintDays <= 0
  ) {
    return 'Record retention hint must be positive when supplied.'
  }

  return null
}

export function StaffArrTenantSettingsPanel({
  accessToken,
  canManage,
}: StaffArrTenantSettingsPanelProps) {
  const [activeTab, setActiveTab] = useState<SettingsTab>('people')
  const [draft, setDraft] = useState<SettingsDraft | null>(null)
  const [savedDraft, setSavedDraft] = useState<SettingsDraft | null>(null)
  const [localError, setLocalError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const queryClient = useQueryClient()

  const settingsQuery = useQuery({
    queryKey: ['staffarr-tenant-settings', accessToken],
    queryFn: () => getStaffArrTenantSettings(accessToken),
    enabled: Boolean(accessToken && canManage),
  })

  const defaultsQuery = useQuery({
    queryKey: ['staffarr-tenant-settings-defaults', accessToken],
    queryFn: () => getStaffArrTenantSettingsDefaults(accessToken),
    enabled: false,
  })

  useEffect(() => {
    if (!settingsQuery.data) {
      return
    }

    const nextDraft = toDraft(settingsQuery.data)
    setDraft(nextDraft)
    setSavedDraft(cloneDraft(nextDraft))
  }, [settingsQuery.data])

  const updateMutation = useMutation({
    mutationFn: (request: SettingsDraft) => updateStaffArrTenantSettings(accessToken, request),
    onSuccess: async (settings) => {
      const nextDraft = toDraft(settings)
      setDraft(nextDraft)
      setSavedDraft(cloneDraft(nextDraft))
      setLocalError(null)
      setSuccessMessage('Settings saved.')
      queryClient.setQueryData(['staffarr-tenant-settings', accessToken], settings)
    },
  })

  const dirty = useMemo(() => !sameDraft(draft, savedDraft), [draft, savedDraft])

  const updateGroup = <K extends keyof SettingsDraft>(
    group: K,
    patch: Partial<SettingsDraft[K]>,
  ) => {
    setDraft((current) =>
      current
        ? {
            ...current,
            [group]: {
              ...current[group],
              ...patch,
            },
          }
        : current,
    )
    setLocalError(null)
    setSuccessMessage(null)
  }

  const toggleProfileSection = (
    field: 'requiredProfileSections' | 'optionalProfileSections',
    value: string,
    checked: boolean,
  ) => {
    if (!draft) return

    const values = new Set(draft.profileFieldGovernance[field])
    if (checked) {
      values.add(value)
    } else {
      values.delete(value)
    }

    updateGroup('profileFieldGovernance', { [field]: Array.from(values) })
  }

  const handleSave = () => {
    if (!draft) return

    const validation = validateDraft(draft)
    if (validation) {
      setLocalError(validation)
      setSuccessMessage(null)
      return
    }

    updateMutation.mutate(draft)
  }

  const handleResetChanges = () => {
    if (!savedDraft) return

    setDraft(cloneDraft(savedDraft))
    setLocalError(null)
    setSuccessMessage(null)
  }

  const handleLoadDefaults = async () => {
    const result = await defaultsQuery.refetch()
    if (result.data) {
      setDraft(toDraft(result.data))
      setLocalError(null)
      setSuccessMessage('Defaults loaded. Save to apply them.')
    }
  }

  if (!canManage) {
    return null
  }

  if (settingsQuery.isLoading || !draft) {
    return (
      <section
        className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm"
        data-testid="staffarr-tenant-settings-panel"
      >
        <p className="text-sm text-slate-500">Loading tenant settings...</p>
      </section>
    )
  }

  const readOnly = updateMutation.isPending || defaultsQuery.isFetching
  const lastSaved = settingsQuery.data?.updatedAt
    ? new Date(settingsQuery.data.updatedAt).toLocaleString()
    : null

  return (
    <section
      className="rounded-lg border border-slate-200 bg-white shadow-sm"
      data-testid="staffarr-tenant-settings-panel"
    >
      <div className="border-b border-slate-200 px-5 py-4">
        <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <h2 className="text-base font-semibold text-slate-950">Tenant behavior settings</h2>
            <p className="mt-1 text-sm text-slate-500">
              Defaults, validation, visibility, review rules, and cross-product behavior.
            </p>
            <p className="mt-1 text-xs text-slate-400">
              {dirty ? 'Unsaved changes' : lastSaved ? `Current saved state: ${lastSaved}` : 'Current saved state'}
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              onClick={handleLoadDefaults}
              disabled={readOnly}
              className="inline-flex items-center gap-2 rounded-md border border-slate-300 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <RefreshCcw aria-hidden="true" size={16} />
              Load defaults
            </button>
            <button
              type="button"
              onClick={handleResetChanges}
              disabled={readOnly || !dirty}
              className="inline-flex items-center gap-2 rounded-md border border-slate-300 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <RotateCcw aria-hidden="true" size={16} />
              Reset changes
            </button>
            <button
              type="button"
              onClick={handleSave}
              disabled={readOnly || !dirty}
              className="inline-flex items-center gap-2 rounded-md bg-slate-900 px-3 py-2 text-sm font-medium text-white hover:bg-slate-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <Save aria-hidden="true" size={16} />
              Save settings
            </button>
          </div>
        </div>
        {(localError || updateMutation.error || settingsQuery.error || successMessage) && (
          <div className="mt-3">
            {successMessage ? (
              <p className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-800">
                {successMessage}
              </p>
            ) : (
              <p role="alert" className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
                {localError ?? errorMessage(updateMutation.error ?? settingsQuery.error)}
              </p>
            )}
          </div>
        )}
      </div>

      <div className="border-b border-slate-200 px-5 pt-4">
        <div className="flex gap-2 overflow-x-auto pb-3" role="tablist" aria-label="StaffArr setting groups">
          {tabs.map((tab) => (
            <button
              key={tab.key}
              type="button"
              role="tab"
              aria-selected={activeTab === tab.key}
              onClick={() => setActiveTab(tab.key)}
              className={`whitespace-nowrap rounded-md px-3 py-2 text-sm font-medium ${
                activeTab === tab.key
                  ? 'bg-slate-900 text-white'
                  : 'border border-slate-200 text-slate-600 hover:bg-slate-50'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      <div className="space-y-6 px-5 py-5">
        {activeTab === 'people' && (
          <>
            <SettingsSection title="Directory">
              <SelectField
                label="Display name format"
                value={draft.personDirectory.displayNameFormat}
                options={displayNameOptions}
                disabled={readOnly}
                onChange={(displayNameFormat) => updateGroup('personDirectory', { displayNameFormat })}
              />
              <TextField
                label="Employee number label"
                value={draft.personDirectory.employeeNumberLabel}
                disabled={readOnly}
                onChange={(employeeNumberLabel) => updateGroup('personDirectory', { employeeNumberLabel })}
              />
              <SelectField
                label="Employee number uniqueness"
                value={draft.personDirectory.employeeNumberUniquenessScope}
                options={[
                  ['tenant', 'Across tenant'],
                  ['site', 'Within site'],
                  ['none', 'Not enforced'],
                ]}
                disabled={readOnly}
                onChange={(employeeNumberUniquenessScope) =>
                  updateGroup('personDirectory', { employeeNumberUniquenessScope })
                }
              />
              <SelectField
                label="Contact visibility"
                value={draft.personDirectory.contactVisibilityMode}
                options={[
                  ['admin_only', 'Admins only'],
                  ['manager_admin', 'Managers and admins'],
                  ['directory', 'Directory visible'],
                ]}
                disabled={readOnly}
                onChange={(contactVisibilityMode) => updateGroup('personDirectory', { contactVisibilityMode })}
              />
              <ToggleField
                label="Preferred name"
                checked={draft.personDirectory.preferredNameEnabled}
                disabled={readOnly}
                onChange={(preferredNameEnabled) => updateGroup('personDirectory', { preferredNameEnabled })}
              />
              <ToggleField
                label="Employee number required"
                checked={draft.personDirectory.employeeNumberRequired}
                disabled={readOnly}
                onChange={(employeeNumberRequired) => updateGroup('personDirectory', { employeeNumberRequired })}
              />
              <ToggleField
                label="Profile photo"
                checked={draft.personDirectory.profilePhotoEnabled}
                disabled={readOnly}
                onChange={(profilePhotoEnabled) => updateGroup('personDirectory', { profilePhotoEnabled })}
              />
              <ToggleField
                label="Emergency contact section"
                checked={draft.personDirectory.emergencyContactEnabled}
                disabled={readOnly}
                onChange={(emergencyContactEnabled) => updateGroup('personDirectory', { emergencyContactEnabled })}
              />
              <ToggleField
                label="Personal address section"
                checked={draft.personDirectory.personalAddressEnabled}
                disabled={readOnly}
                onChange={(personalAddressEnabled) => updateGroup('personDirectory', { personalAddressEnabled })}
              />
            </SettingsSection>

            <SettingsSection title="Lifecycle">
              <SelectField
                label="Default status on create"
                value={draft.personLifecycle.defaultPersonStatusOnCreate}
                options={employmentStatusOptions}
                disabled={readOnly}
                onChange={(defaultPersonStatusOnCreate) =>
                  updateGroup('personLifecycle', { defaultPersonStatusOnCreate })
                }
              />
              <SelectField
                label="Rehire match behavior"
                value={draft.personLifecycle.rehireMatchBehavior}
                options={[
                  ['none', 'No matching'],
                  ['flag_possible_match', 'Flag possible match'],
                  ['block_until_review', 'Block until reviewed'],
                ]}
                disabled={readOnly}
                onChange={(rehireMatchBehavior) => updateGroup('personLifecycle', { rehireMatchBehavior })}
              />
              <ToggleField
                label="Manager required before activation"
                checked={draft.personLifecycle.requireManagerBeforeActivation}
                disabled={readOnly}
                onChange={(requireManagerBeforeActivation) =>
                  updateGroup('personLifecycle', { requireManagerBeforeActivation })
                }
              />
              <ToggleField
                label="Position required before activation"
                checked={draft.personLifecycle.requirePositionBeforeActivation}
                disabled={readOnly}
                onChange={(requirePositionBeforeActivation) =>
                  updateGroup('personLifecycle', { requirePositionBeforeActivation })
                }
              />
              <ToggleField
                label="Home location required before activation"
                checked={draft.personLifecycle.requireHomeLocationBeforeActivation}
                disabled={readOnly}
                onChange={(requireHomeLocationBeforeActivation) =>
                  updateGroup('personLifecycle', { requireHomeLocationBeforeActivation })
                }
              />
              <ToggleField
                label="Inactive people can be assigned work"
                checked={draft.personLifecycle.allowInactivePeopleToBeAssignedWork}
                disabled={readOnly}
                onChange={(allowInactivePeopleToBeAssignedWork) =>
                  updateGroup('personLifecycle', { allowInactivePeopleToBeAssignedWork })
                }
              />
              <ToggleField
                label="Deactivation reason required"
                checked={draft.personLifecycle.deactivationReasonRequired}
                disabled={readOnly}
                onChange={(deactivationReasonRequired) =>
                  updateGroup('personLifecycle', { deactivationReasonRequired })
                }
              />
              <ToggleField
                label="Remove roles on deactivation"
                checked={draft.personLifecycle.autoRemoveRolesOnDeactivation}
                disabled={readOnly}
                onChange={(autoRemoveRolesOnDeactivation) =>
                  updateGroup('personLifecycle', { autoRemoveRolesOnDeactivation })
                }
              />
              <ToggleField
                label="End team assignments on deactivation"
                checked={draft.personLifecycle.autoEndTeamAssignmentsOnDeactivation}
                disabled={readOnly}
                onChange={(autoEndTeamAssignmentsOnDeactivation) =>
                  updateGroup('personLifecycle', { autoEndTeamAssignmentsOnDeactivation })
                }
              />
            </SettingsSection>
          </>
        )}

        {activeTab === 'org' && (
          <>
            <SettingsSection title="Organization">
              <SelectField
                label="Org hierarchy mode"
                value={draft.orgStructure.orgHierarchyMode}
                options={[
                  ['standard', 'Standard'],
                  ['flat', 'Flat'],
                  ['strict_site_department_team_position', 'Strict site, department, team, position'],
                ]}
                disabled={readOnly}
                onChange={(orgHierarchyMode) => updateGroup('orgStructure', { orgHierarchyMode })}
              />
              <SelectField
                label="Team membership mode"
                value={draft.teamsAssignments.teamMembershipMode}
                options={[
                  ['flexible', 'Flexible'],
                  ['single_team', 'Single team'],
                  ['matrix', 'Matrix'],
                ]}
                disabled={readOnly}
                onChange={(teamMembershipMode) => updateGroup('teamsAssignments', { teamMembershipMode })}
              />
              <ToggleField
                label="Every person must be in an org unit"
                checked={draft.orgStructure.requireEveryPersonInOrgUnit}
                disabled={readOnly}
                onChange={(requireEveryPersonInOrgUnit) =>
                  updateGroup('orgStructure', { requireEveryPersonInOrgUnit })
                }
              />
              <ToggleField
                label="Department must sit under site"
                checked={draft.orgStructure.requireDepartmentUnderSite}
                disabled={readOnly}
                onChange={(requireDepartmentUnderSite) =>
                  updateGroup('orgStructure', { requireDepartmentUnderSite })
                }
              />
              <ToggleField
                label="Matrix membership"
                checked={draft.orgStructure.allowMatrixMembership}
                disabled={readOnly}
                onChange={(allowMatrixMembership) => updateGroup('orgStructure', { allowMatrixMembership })}
              />
              <ToggleField
                label="Primary assignment required"
                checked={draft.orgStructure.primaryAssignmentRequired}
                disabled={readOnly}
                onChange={(primaryAssignmentRequired) =>
                  updateGroup('orgStructure', { primaryAssignmentRequired })
                }
              />
              <ToggleField
                label="Manager hierarchy required"
                checked={draft.orgStructure.managerHierarchyRequired}
                disabled={readOnly}
                onChange={(managerHierarchyRequired) =>
                  updateGroup('orgStructure', { managerHierarchyRequired })
                }
              />
              <ToggleField
                label="Skip-level managers"
                checked={draft.orgStructure.allowSkipLevelManagers}
                disabled={readOnly}
                onChange={(allowSkipLevelManagers) =>
                  updateGroup('orgStructure', { allowSkipLevelManagers })
                }
              />
              <ToggleField
                label="Prevent circular reporting"
                checked={draft.orgStructure.preventCircularReporting}
                disabled
                onChange={() => undefined}
              />
              <ToggleField
                label="Temporary assignments"
                checked={draft.teamsAssignments.allowTemporaryAssignments}
                disabled={readOnly}
                onChange={(allowTemporaryAssignments) =>
                  updateGroup('teamsAssignments', {
                    allowTemporaryAssignments,
                    temporaryAssignmentMaxDurationDays: allowTemporaryAssignments
                      ? draft.teamsAssignments.temporaryAssignmentMaxDurationDays ?? 90
                      : null,
                  })
                }
              />
              <NumberField
                label="Temporary assignment max days"
                value={draft.teamsAssignments.temporaryAssignmentMaxDurationDays}
                disabled={readOnly || !draft.teamsAssignments.allowTemporaryAssignments}
                onChange={(temporaryAssignmentMaxDurationDays) =>
                  updateGroup('teamsAssignments', { temporaryAssignmentMaxDurationDays })
                }
              />
              <ToggleField
                label="Assignment effective dating"
                checked={draft.teamsAssignments.assignmentEffectiveDatingEnabled}
                disabled={readOnly}
                onChange={(assignmentEffectiveDatingEnabled) =>
                  updateGroup('teamsAssignments', { assignmentEffectiveDatingEnabled })
                }
              />
              <SelectField
                label="Historical assignment visibility"
                value={draft.teamsAssignments.historicalAssignmentVisibilityMode}
                options={[
                  ['admin_all', 'Admins see all history'],
                  ['manager_limited', 'Managers limited'],
                  ['person_self', 'Person self only'],
                ]}
                disabled={readOnly}
                onChange={(historicalAssignmentVisibilityMode) =>
                  updateGroup('teamsAssignments', { historicalAssignmentVisibilityMode })
                }
              />
              <ToggleField
                label="Open positions"
                checked={draft.teamsAssignments.allowOpenPositions}
                disabled={readOnly}
                onChange={(allowOpenPositions) => updateGroup('teamsAssignments', { allowOpenPositions })}
              />
              <ToggleField
                label="Team lead required"
                checked={draft.teamsAssignments.requireTeamLead}
                disabled={readOnly}
                onChange={(requireTeamLead) => updateGroup('teamsAssignments', { requireTeamLead })}
              />
            </SettingsSection>

            <SettingsSection title="Locations">
              <SelectField
                label="Location hierarchy mode"
                value={draft.locationHierarchy.locationHierarchyMode}
                options={[
                  ['site_required', 'Site required'],
                  ['flat_site', 'Flat by site'],
                  ['strict_tree', 'Strict tree'],
                ]}
                disabled={readOnly}
                onChange={(locationHierarchyMode) =>
                  updateGroup('locationHierarchy', { locationHierarchyMode })
                }
              />
              <SelectField
                label="Location code uniqueness"
                value={draft.locationHierarchy.locationCodeUniquenessScope}
                options={[
                  ['tenant', 'Across tenant'],
                  ['site', 'Within site'],
                  ['parent', 'Under same parent'],
                  ['none', 'Not enforced'],
                ]}
                disabled={readOnly}
                onChange={(locationCodeUniquenessScope) =>
                  updateGroup('locationHierarchy', { locationCodeUniquenessScope })
                }
              />
              <SelectField
                label="Archived location behavior"
                value={draft.locationHierarchy.archivedLocationAssignmentBehavior}
                options={[
                  ['block_new_assignments', 'Block new assignments'],
                  ['warn', 'Warn'],
                  ['allow', 'Allow'],
                ]}
                disabled={readOnly}
                onChange={(archivedLocationAssignmentBehavior) =>
                  updateGroup('locationHierarchy', { archivedLocationAssignmentBehavior })
                }
              />
              <ToggleField
                label="Location code required"
                checked={draft.locationHierarchy.requireLocationCode}
                disabled={readOnly}
                onChange={(requireLocationCode) => updateGroup('locationHierarchy', { requireLocationCode })}
              />
              <ToggleField
                label="Operational locations"
                checked={draft.locationHierarchy.allowOperationalLocations}
                disabled={readOnly}
                onChange={(allowOperationalLocations) =>
                  updateGroup('locationHierarchy', { allowOperationalLocations })
                }
              />
              <ToggleField
                label="Bins and shelves"
                checked={draft.locationHierarchy.allowAddressableBinsShelves}
                disabled={readOnly}
                onChange={(allowAddressableBinsShelves) =>
                  updateGroup('locationHierarchy', { allowAddressableBinsShelves })
                }
              />
              <ToggleField
                label="Mobile locations"
                checked={draft.locationHierarchy.allowMobileLocations}
                disabled={readOnly}
                onChange={(allowMobileLocations) =>
                  updateGroup('locationHierarchy', { allowMobileLocations })
                }
              />
              <ToggleField
                label="Parent required except root"
                checked={draft.locationHierarchy.requireParentLocationExceptRoot}
                disabled={readOnly}
                onChange={(requireParentLocationExceptRoot) =>
                  updateGroup('locationHierarchy', { requireParentLocationExceptRoot })
                }
              />
            </SettingsSection>
          </>
        )}

        {activeTab === 'roles' && (
          <SettingsSection title="Roles and permissions">
            <ToggleField
              label="Role assignment approval"
              checked={draft.rolePermissions.roleAssignmentApprovalRequired}
              disabled={readOnly}
              onChange={(roleAssignmentApprovalRequired) =>
                updateGroup('rolePermissions', { roleAssignmentApprovalRequired })
              }
            />
            <ToggleField
              label="Self-service role requests"
              checked={draft.rolePermissions.allowSelfServiceRoleRequests}
              disabled={readOnly}
              onChange={(allowSelfServiceRoleRequests) =>
                updateGroup('rolePermissions', { allowSelfServiceRoleRequests })
              }
            />
            <ToggleField
              label="Role expiration"
              checked={draft.rolePermissions.roleExpirationEnabled}
              disabled={readOnly}
              onChange={(roleExpirationEnabled) => {
                updateGroup('rolePermissions', {
                  roleExpirationEnabled,
                  defaultRoleGrantDurationDays: roleExpirationEnabled
                    ? draft.rolePermissions.defaultRoleGrantDurationDays ?? 365
                    : null,
                })
                if (!roleExpirationEnabled) {
                  updateGroup('notificationsReviews', { notifyBeforeRoleExpiration: false })
                }
              }}
            />
            <NumberField
              label="Default role grant duration"
              value={draft.rolePermissions.defaultRoleGrantDurationDays}
              disabled={readOnly || !draft.rolePermissions.roleExpirationEnabled}
              onChange={(defaultRoleGrantDurationDays) =>
                updateGroup('rolePermissions', { defaultRoleGrantDurationDays })
              }
            />
            <ToggleField
              label="Assignment reason required"
              checked={draft.rolePermissions.requireAssignmentReason}
              disabled={readOnly}
              onChange={(requireAssignmentReason) =>
                updateGroup('rolePermissions', { requireAssignmentReason })
              }
            />
            <SelectField
              label="Permission review cadence"
              value={draft.rolePermissions.permissionReviewCadence}
              options={[
                ['none', 'None'],
                ['monthly', 'Monthly'],
                ['quarterly', 'Quarterly'],
                ['semiannual', 'Semiannual'],
                ['annual', 'Annual'],
              ]}
              disabled={readOnly}
              onChange={(permissionReviewCadence) =>
                updateGroup('rolePermissions', { permissionReviewCadence })
              }
            />
            <ToggleField
              label="Remove roles on inactive person"
              checked={draft.rolePermissions.autoRemoveRolesOnInactivePerson}
              disabled={readOnly}
              onChange={(autoRemoveRolesOnInactivePerson) =>
                updateGroup('rolePermissions', { autoRemoveRolesOnInactivePerson })
              }
            />
            <ToggleField
              label="Direct permissions"
              checked={draft.rolePermissions.allowDirectPermissions}
              disabled={readOnly}
              onChange={(allowDirectPermissions) => updateGroup('rolePermissions', { allowDirectPermissions })}
            />
            <ToggleField
              label="Prefer roles over direct permissions"
              checked={draft.rolePermissions.preferRolesOverDirectPermissions}
              disabled={readOnly}
              onChange={(preferRolesOverDirectPermissions) =>
                updateGroup('rolePermissions', { preferRolesOverDirectPermissions })
              }
            />
            <ToggleField
              label="Site-scoped role assignments"
              checked={draft.rolePermissions.siteScopedRoleAssignmentsEnabled}
              disabled={readOnly}
              onChange={(siteScopedRoleAssignmentsEnabled) =>
                updateGroup('rolePermissions', { siteScopedRoleAssignmentsEnabled })
              }
            />
          </SettingsSection>
        )}

        {activeTab === 'incidents' && (
          <SettingsSection title="Incident intake and routing">
            <ToggleField
              label="Incident intake"
              checked={draft.incidents.incidentIntakeEnabled}
              disabled={readOnly}
              onChange={(incidentIntakeEnabled) => updateGroup('incidents', { incidentIntakeEnabled })}
            />
            <ToggleField
              label="Incident category required"
              checked={draft.incidents.requireIncidentCategory}
              disabled={readOnly || !draft.incidents.incidentIntakeEnabled}
              onChange={(requireIncidentCategory) => updateGroup('incidents', { requireIncidentCategory })}
            />
            <ToggleField
              label="Involved person required"
              checked={draft.incidents.requireInvolvedPerson}
              disabled={readOnly || !draft.incidents.incidentIntakeEnabled}
              onChange={(requireInvolvedPerson) => updateGroup('incidents', { requireInvolvedPerson })}
            />
            <SelectField
              label="Manager notification mode"
              value={draft.incidents.managerNotificationMode}
              options={[
                ['none', 'Do not notify'],
                ['optional', 'Optional'],
                ['always', 'Always'],
              ]}
              disabled={readOnly || !draft.incidents.incidentIntakeEnabled}
              onChange={(managerNotificationMode) => updateGroup('incidents', { managerNotificationMode })}
            />
            <ToggleField
              label="TrainArr routing"
              checked={draft.incidents.trainArrRoutingEnabled}
              disabled={readOnly || !draft.incidents.incidentIntakeEnabled}
              onChange={(trainArrRoutingEnabled) =>
                updateGroup('incidents', {
                  trainArrRoutingEnabled,
                  retrainingRecommendationThreshold: trainArrRoutingEnabled
                    ? draft.incidents.retrainingRecommendationThreshold ?? 3
                    : null,
                })
              }
            />
            <NumberField
              label="Retraining recommendation threshold"
              value={draft.incidents.retrainingRecommendationThreshold}
              disabled={
                readOnly || !draft.incidents.incidentIntakeEnabled || !draft.incidents.trainArrRoutingEnabled
              }
              onChange={(retrainingRecommendationThreshold) =>
                updateGroup('incidents', { retrainingRecommendationThreshold })
              }
            />
            <SelectField
              label="Incident visibility"
              value={draft.incidents.incidentVisibilityMode}
              options={[
                ['hr_only', 'HR only'],
                ['management', 'Management'],
                ['site_management', 'Site management'],
              ]}
              disabled={readOnly || !draft.incidents.incidentIntakeEnabled}
              onChange={(incidentVisibilityMode) => updateGroup('incidents', { incidentVisibilityMode })}
            />
            <ToggleField
              label="Closure approval"
              checked={draft.incidents.closureApprovalRequired}
              disabled={readOnly || !draft.incidents.incidentIntakeEnabled}
              onChange={(closureApprovalRequired) =>
                updateGroup('incidents', { closureApprovalRequired })
              }
            />
          </SettingsSection>
        )}

        {activeTab === 'governance' && (
          <>
            <SettingsSection title="Profile fields">
              <CheckboxGroup
                label="Required profile sections"
                values={draft.profileFieldGovernance.requiredProfileSections}
                options={profileSectionOptions}
                disabled={readOnly}
                onChange={(value, checked) => toggleProfileSection('requiredProfileSections', value, checked)}
              />
              <CheckboxGroup
                label="Optional profile sections"
                values={draft.profileFieldGovernance.optionalProfileSections}
                options={profileSectionOptions}
                disabled={readOnly}
                onChange={(value, checked) => toggleProfileSection('optionalProfileSections', value, checked)}
              />
              <ToggleField
                label="Custom profile fields"
                checked={draft.profileFieldGovernance.customProfileFieldsEnabled}
                disabled={readOnly}
                onChange={(customProfileFieldsEnabled) =>
                  updateGroup('profileFieldGovernance', {
                    customProfileFieldsEnabled,
                    fieldVisibilityByRoleEnabled: customProfileFieldsEnabled
                      ? draft.profileFieldGovernance.fieldVisibilityByRoleEnabled
                      : false,
                    fieldEditabilityByRoleEnabled: customProfileFieldsEnabled
                      ? draft.profileFieldGovernance.fieldEditabilityByRoleEnabled
                      : false,
                  })
                }
              />
              <ToggleField
                label="Field visibility by role"
                checked={draft.profileFieldGovernance.fieldVisibilityByRoleEnabled}
                disabled={readOnly || !draft.profileFieldGovernance.customProfileFieldsEnabled}
                onChange={(fieldVisibilityByRoleEnabled) =>
                  updateGroup('profileFieldGovernance', { fieldVisibilityByRoleEnabled })
                }
              />
              <ToggleField
                label="Field editability by role"
                checked={draft.profileFieldGovernance.fieldEditabilityByRoleEnabled}
                disabled={readOnly || !draft.profileFieldGovernance.customProfileFieldsEnabled}
                onChange={(fieldEditabilityByRoleEnabled) =>
                  updateGroup('profileFieldGovernance', { fieldEditabilityByRoleEnabled })
                }
              />
              <ToggleField
                label="Field review required"
                checked={draft.profileFieldGovernance.fieldReviewRequired}
                disabled={readOnly}
                onChange={(fieldReviewRequired) =>
                  updateGroup('profileFieldGovernance', { fieldReviewRequired })
                }
              />
              <ToggleField
                label="Field history"
                checked={draft.profileFieldGovernance.fieldHistoryEnabled}
                disabled={readOnly}
                onChange={(fieldHistoryEnabled) =>
                  updateGroup('profileFieldGovernance', { fieldHistoryEnabled })
                }
              />
            </SettingsSection>

            <SettingsSection title="Audit, import, and review">
              <ToggleField
                label="Audit profile changes"
                checked={draft.dataGovernanceAudit.auditProfileChanges}
                disabled={readOnly}
                onChange={(auditProfileChanges) => updateGroup('dataGovernanceAudit', { auditProfileChanges })}
              />
              <ToggleField
                label="Audit role changes"
                checked={draft.dataGovernanceAudit.auditRoleChanges}
                disabled={readOnly}
                onChange={(auditRoleChanges) => updateGroup('dataGovernanceAudit', { auditRoleChanges })}
              />
              <ToggleField
                label="Audit org and location changes"
                checked={draft.dataGovernanceAudit.auditOrgLocationChanges}
                disabled={readOnly}
                onChange={(auditOrgLocationChanges) =>
                  updateGroup('dataGovernanceAudit', { auditOrgLocationChanges })
                }
              />
              <ToggleField
                label="Sensitive edit reason required"
                checked={draft.dataGovernanceAudit.requireChangeReasonForSensitiveEdits}
                disabled={readOnly}
                onChange={(requireChangeReasonForSensitiveEdits) =>
                  updateGroup('dataGovernanceAudit', { requireChangeReasonForSensitiveEdits })
                }
              />
              <ToggleField
                label="Soft archive only"
                checked={draft.dataGovernanceAudit.softArchiveOnly}
                disabled={readOnly}
                onChange={(softArchiveOnly) => updateGroup('dataGovernanceAudit', { softArchiveOnly })}
              />
              <NumberField
                label="Record retention hint days"
                value={draft.dataGovernanceAudit.recordRetentionHintDays}
                disabled={readOnly}
                onChange={(recordRetentionHintDays) =>
                  updateGroup('dataGovernanceAudit', { recordRetentionHintDays })
                }
              />
              <ToggleField
                label="Exports enabled"
                checked={draft.dataGovernanceAudit.exportEnabled}
                disabled={readOnly}
                onChange={(exportEnabled) => updateGroup('dataGovernanceAudit', { exportEnabled })}
              />
              <ToggleField
                label="Bulk import enabled"
                checked={draft.dataGovernanceAudit.bulkImportEnabled}
                disabled={readOnly}
                onChange={(bulkImportEnabled) => updateGroup('dataGovernanceAudit', { bulkImportEnabled })}
              />
              <ToggleField
                label="Bulk import review required"
                checked={draft.dataGovernanceAudit.bulkImportReviewRequired}
                disabled={readOnly || !draft.dataGovernanceAudit.bulkImportEnabled}
                onChange={(bulkImportReviewRequired) =>
                  updateGroup('dataGovernanceAudit', { bulkImportReviewRequired })
                }
              />
              <ToggleField
                label="Notify manager on new person"
                checked={draft.notificationsReviews.notifyManagerOnNewPerson}
                disabled={readOnly}
                onChange={(notifyManagerOnNewPerson) =>
                  updateGroup('notificationsReviews', { notifyManagerOnNewPerson })
                }
              />
              <ToggleField
                label="Notify on manager change"
                checked={draft.notificationsReviews.notifyOnManagerChange}
                disabled={readOnly}
                onChange={(notifyOnManagerChange) =>
                  updateGroup('notificationsReviews', { notifyOnManagerChange })
                }
              />
              <ToggleField
                label="Notify on role grant or removal"
                checked={draft.notificationsReviews.notifyOnRoleGrantRemoval}
                disabled={readOnly}
                onChange={(notifyOnRoleGrantRemoval) =>
                  updateGroup('notificationsReviews', { notifyOnRoleGrantRemoval })
                }
              />
              <ToggleField
                label="Notify before role expiration"
                checked={draft.notificationsReviews.notifyBeforeRoleExpiration}
                disabled={readOnly || !draft.rolePermissions.roleExpirationEnabled}
                onChange={(notifyBeforeRoleExpiration) =>
                  updateGroup('notificationsReviews', { notifyBeforeRoleExpiration })
                }
              />
              <ToggleField
                label="Notify on inactive assignment conflict"
                checked={draft.notificationsReviews.notifyOnInactiveAssignmentConflict}
                disabled={readOnly}
                onChange={(notifyOnInactiveAssignmentConflict) =>
                  updateGroup('notificationsReviews', { notifyOnInactiveAssignmentConflict })
                }
              />
              <ToggleField
                label="Review reminders"
                checked={draft.notificationsReviews.reviewRemindersEnabled}
                disabled={readOnly}
                onChange={(reviewRemindersEnabled) =>
                  updateGroup('notificationsReviews', { reviewRemindersEnabled })
                }
              />
              <SelectField
                label="Digest frequency"
                value={draft.notificationsReviews.digestFrequency}
                options={[
                  ['none', 'None'],
                  ['daily', 'Daily'],
                  ['weekly', 'Weekly'],
                ]}
                disabled={readOnly}
                onChange={(digestFrequency) => updateGroup('notificationsReviews', { digestFrequency })}
              />
            </SettingsSection>
          </>
        )}

        {activeTab === 'integrations' && (
          <SettingsSection title="Cross-product references">
            <ToggleField
              label="People reference API"
              checked={draft.crossProductReferences.exposePeopleReferenceApi}
              disabled={readOnly}
              onChange={(exposePeopleReferenceApi) =>
                updateGroup('crossProductReferences', { exposePeopleReferenceApi })
              }
            />
            <ToggleField
              label="Location reference API"
              checked={draft.crossProductReferences.exposeLocationReferenceApi}
              disabled={readOnly}
              onChange={(exposeLocationReferenceApi) =>
                updateGroup('crossProductReferences', { exposeLocationReferenceApi })
              }
            />
            <ToggleField
              label="Org unit reference API"
              checked={draft.crossProductReferences.exposeOrgUnitReferenceApi}
              disabled={readOnly}
              onChange={(exposeOrgUnitReferenceApi) =>
                updateGroup('crossProductReferences', { exposeOrgUnitReferenceApi })
              }
            />
            <ToggleField
              label="Publish person lifecycle events"
              checked={draft.crossProductReferences.publishPersonLifecycleEvents}
              disabled={readOnly}
              onChange={(publishPersonLifecycleEvents) =>
                updateGroup('crossProductReferences', { publishPersonLifecycleEvents })
              }
            />
            <ToggleField
              label="Publish org and location events"
              checked={draft.crossProductReferences.publishOrgLocationEvents}
              disabled={readOnly}
              onChange={(publishOrgLocationEvents) =>
                updateGroup('crossProductReferences', { publishOrgLocationEvents })
              }
            />
            <ToggleField
              label="Product-originated person proposals"
              checked={draft.crossProductReferences.allowProductOriginatedPersonProposals}
              disabled={readOnly}
              onChange={(allowProductOriginatedPersonProposals) =>
                updateGroup('crossProductReferences', { allowProductOriginatedPersonProposals })
              }
            />
            <ToggleField
              label="Review product-originated proposals"
              checked={draft.crossProductReferences.requireReviewForProductOriginatedProposals}
              disabled={readOnly || !draft.crossProductReferences.allowProductOriginatedPersonProposals}
              onChange={(requireReviewForProductOriginatedProposals) =>
                updateGroup('crossProductReferences', { requireReviewForProductOriginatedProposals })
              }
            />
            <SelectField
              label="Snapshot label policy"
              value={draft.crossProductReferences.snapshotLabelPolicy}
              options={[
                ['display_label_only', 'Display label only'],
                ['display_label_with_status', 'Display label with status'],
                ['display_label_with_source', 'Display label with source'],
              ]}
              disabled={readOnly}
              onChange={(snapshotLabelPolicy) =>
                updateGroup('crossProductReferences', { snapshotLabelPolicy })
              }
            />
          </SettingsSection>
        )}
      </div>
    </section>
  )
}

function SettingsSection({
  title,
  children,
}: {
  title: string
  children: React.ReactNode
}) {
  return (
    <section>
      <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-500">{title}</h3>
      <div className="mt-3 grid gap-4 md:grid-cols-2 xl:grid-cols-3">{children}</div>
    </section>
  )
}

function TextField({
  label,
  value,
  disabled,
  onChange,
}: {
  label: string
  value: string
  disabled?: boolean
  onChange: (value: string) => void
}) {
  return (
    <label className="grid gap-1 text-sm">
      <span className="font-medium text-slate-700">{label}</span>
      <input
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value)}
        className="rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-900 shadow-sm disabled:bg-slate-100 disabled:text-slate-500"
      />
    </label>
  )
}

function NumberField({
  label,
  value,
  disabled,
  onChange,
}: {
  label: string
  value: number | null
  disabled?: boolean
  onChange: (value: number | null) => void
}) {
  return (
    <label className="grid gap-1 text-sm">
      <span className="font-medium text-slate-700">{label}</span>
      <input
        type="number"
        min={1}
        value={value ?? ''}
        disabled={disabled}
        onChange={(event) => {
          const next = event.target.value
          onChange(next === '' ? null : Number(next))
        }}
        className="rounded-md border border-slate-300 px-3 py-2 text-sm text-slate-900 shadow-sm disabled:bg-slate-100 disabled:text-slate-500"
      />
    </label>
  )
}

function SelectField({
  label,
  value,
  options,
  disabled,
  onChange,
}: {
  label: string
  value: string
  options: string[][]
  disabled?: boolean
  onChange: (value: string) => void
}) {
  return (
    <label className="grid gap-1 text-sm">
      <span className="font-medium text-slate-700">{label}</span>
      <select
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value)}
        className="rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm disabled:bg-slate-100 disabled:text-slate-500"
      >
        {options.map(([optionValue, labelText]) => (
          <option key={optionValue} value={optionValue}>
            {labelText}
          </option>
        ))}
      </select>
    </label>
  )
}

function ToggleField({
  label,
  checked,
  disabled,
  onChange,
}: {
  label: string
  checked: boolean
  disabled?: boolean
  onChange: (checked: boolean) => void
}) {
  return (
    <label className="flex min-h-12 items-center justify-between gap-3 rounded-md border border-slate-200 px-3 py-2 text-sm">
      <span className="font-medium text-slate-700">{label}</span>
      <input
        type="checkbox"
        checked={checked}
        disabled={disabled}
        onChange={(event) => onChange(event.target.checked)}
        className="h-4 w-4 rounded border-slate-300"
      />
    </label>
  )
}

function CheckboxGroup({
  label,
  values,
  options,
  disabled,
  onChange,
}: {
  label: string
  values: string[]
  options: string[][]
  disabled?: boolean
  onChange: (value: string, checked: boolean) => void
}) {
  return (
    <fieldset className="rounded-md border border-slate-200 px-3 py-2">
      <legend className="px-1 text-sm font-medium text-slate-700">{label}</legend>
      <div className="mt-2 grid gap-2">
        {options.map(([value, labelText]) => (
          <label key={value} className="flex items-center gap-2 text-sm text-slate-700">
            <input
              type="checkbox"
              checked={values.includes(value)}
              disabled={disabled}
              onChange={(event) => onChange(value, event.target.checked)}
              className="h-4 w-4 rounded border-slate-300"
            />
            {labelText}
          </label>
        ))}
      </div>
    </fieldset>
  )
}
