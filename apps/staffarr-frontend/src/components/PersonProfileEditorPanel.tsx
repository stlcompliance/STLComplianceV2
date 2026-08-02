import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { ApiErrorCallout, ConfirmDialog, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { getErrorMessage } from '@stl/shared-ui'
import { getStaffArrFieldset, getStaffPersonRoles, listLocations, listStaffRoles, setStaffPersonRoles } from '../api/client'
import { PersonAccountAccessPanel } from './PersonAccountAccessPanel'
import type {
  OrgUnitResponse,
  SetStaffPersonRoleItemRequest,
  StaffArrFieldOptionResponse,
  StaffArrFieldsetResponse,
  StaffPersonRoleAssignmentResponse,
  StaffPersonDetailResponse,
} from '../api/types'
const WRITER_ROLES = new Set(['tenant_admin', 'staffarr_admin', 'hr_admin'])

interface PersonProfileEditorPanelProps {
  accessToken: string
  profile: StaffPersonDetailResponse
  orgUnits: OrgUnitResponse[]
  peopleOptions: Array<{ personId: string; displayName: string }>
  siteContextOrgUnitId?: string | null
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
  onOpenPermissionsReview?: () => void
  onUpdate: (request: {
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
  }) => Promise<void>
  onEmploymentStatusChange: (request: { employmentStatus: string; reason: string | null }) => Promise<void>
}

export function canManagePeople(roleKey: string, isPlatformAdmin: boolean): boolean {
  return isPlatformAdmin || WRITER_ROLES.has(roleKey)
}

function toOrgUnitOptions(orgUnits: OrgUnitResponse[]): PickerOption[] {
  return orgUnits.map((unit) => ({
    value: unit.orgUnitId,
    label: `${unit.unitType} · ${unit.name}`,
  }))
}

function fieldOptions(
  fieldset: StaffArrFieldsetResponse | undefined,
  fieldKey: string,
): StaffArrFieldOptionResponse[] {
  return fieldset?.fields.find((field) => field.key === fieldKey)?.options ?? []
}

export function PersonProfileEditorPanel({
  accessToken,
  profile,
  orgUnits,
  peopleOptions,
  siteContextOrgUnitId = null,
  canManage,
  isSubmitting,
  errorMessage,
  onOpenPermissionsReview,
  onUpdate,
  onEmploymentStatusChange,
}: PersonProfileEditorPanelProps) {
  const queryClient = useQueryClient()
  const [legalFirstName, setLegalFirstName] = useState(profile.legalFirstName)
  const [legalMiddleName, setLegalMiddleName] = useState(profile.legalMiddleName ?? '')
  const [legalLastName, setLegalLastName] = useState(profile.legalLastName)
  const [preferredName, setPreferredName] = useState(profile.preferredName ?? '')
  const [pronouns, setPronouns] = useState(profile.pronouns ?? '')
  const [primaryEmail, setPrimaryEmail] = useState(profile.primaryEmail)
  const [alternateEmail, setAlternateEmail] = useState(profile.alternateEmail ?? '')
  const [primaryPhone, setPrimaryPhone] = useState(profile.primaryPhone ?? '')
  const [alternatePhone, setAlternatePhone] = useState(profile.alternatePhone ?? '')
  const [workPhone, setWorkPhone] = useState(profile.workPhone ?? '')
  const [primaryOrgUnitId, setPrimaryOrgUnitId] = useState(profile.primaryOrgUnitId ?? '')
  const [managerPersonId, setManagerPersonId] = useState(profile.managerPersonId ?? '')
  const [jobTitle, setJobTitle] = useState(profile.jobTitle ?? '')
  const [workRelationshipType, setWorkRelationshipType] = useState(profile.workRelationshipType ?? 'employee')
  const [employmentType, setEmploymentType] = useState(profile.employmentType ?? 'full_time')
  const [workerCategory, setWorkerCategory] = useState(profile.workerCategory ?? 'employee')
  const [flsaStatus, setFlsaStatus] = useState(profile.flsaStatus ?? 'unknown')
  const [positionNumber, setPositionNumber] = useState(profile.positionNumber ?? '')
  const [currentEmploymentAction, setCurrentEmploymentAction] = useState(profile.currentEmploymentAction ?? '')
  const [currentEmploymentActionAt, setCurrentEmploymentActionAt] = useState(
    profile.currentEmploymentActionAt ? profile.currentEmploymentActionAt.slice(0, 16) : '',
  )
  const [leaveStatus, setLeaveStatus] = useState(profile.leaveStatus ?? 'active')
  const [eligibleForRehire, setEligibleForRehire] = useState(profile.eligibleForRehire ?? true)
  const [startDate, setStartDate] = useState(profile.startDate ? profile.startDate.slice(0, 10) : '')
  const [expectedStartDate, setExpectedStartDate] = useState(
    profile.expectedStartDate ? profile.expectedStartDate.slice(0, 10) : '',
  )
  const [homeBaseLocationId, setHomeBaseLocationId] = useState(profile.homeBaseLocationId ?? '')
  const [statusReason, setStatusReason] = useState('')
  const [statusDraft, setStatusDraft] = useState(profile.employmentStatus)
  const [pendingStatusConfirmation, setPendingStatusConfirmation] = useState(false)
  const [roleIdDraft, setRoleIdDraft] = useState('')
  const [roleScopeTypeDraft, setRoleScopeTypeDraft] = useState<SetStaffPersonRoleItemRequest['assignmentScopeType']>('tenant')
  const [roleScopeRefIdDraft, setRoleScopeRefIdDraft] = useState('')
  const [roleStartsAtDraft, setRoleStartsAtDraft] = useState('')
  const [roleEndsAtDraft, setRoleEndsAtDraft] = useState('')

  const profileFieldsetQuery = useQuery({
    queryKey: ['staffarr-fieldset', accessToken, 'people.profile'],
    queryFn: () => getStaffArrFieldset(accessToken, 'people/profile'),
    enabled: Boolean(accessToken),
  })

  useEffect(() => {
    setLegalFirstName(profile.legalFirstName)
    setLegalMiddleName(profile.legalMiddleName ?? '')
    setLegalLastName(profile.legalLastName)
    setPreferredName(profile.preferredName ?? '')
    setPronouns(profile.pronouns ?? '')
    setPrimaryEmail(profile.primaryEmail)
    setAlternateEmail(profile.alternateEmail ?? '')
    setPrimaryPhone(profile.primaryPhone ?? '')
    setAlternatePhone(profile.alternatePhone ?? '')
    setWorkPhone(profile.workPhone ?? '')
    setPrimaryOrgUnitId(profile.primaryOrgUnitId ?? '')
    setManagerPersonId(profile.managerPersonId ?? '')
    setJobTitle(profile.jobTitle ?? '')
    setWorkRelationshipType(profile.workRelationshipType ?? 'employee')
    setEmploymentType(profile.employmentType ?? 'full_time')
    setWorkerCategory(profile.workerCategory ?? 'employee')
    setFlsaStatus(profile.flsaStatus ?? 'unknown')
    setPositionNumber(profile.positionNumber ?? '')
    setCurrentEmploymentAction(profile.currentEmploymentAction ?? '')
    setCurrentEmploymentActionAt(profile.currentEmploymentActionAt ? profile.currentEmploymentActionAt.slice(0, 16) : '')
    setLeaveStatus(profile.leaveStatus ?? 'active')
    setEligibleForRehire(profile.eligibleForRehire ?? true)
    setStartDate(profile.startDate ? profile.startDate.slice(0, 10) : '')
    setExpectedStartDate(profile.expectedStartDate ? profile.expectedStartDate.slice(0, 10) : '')
    setHomeBaseLocationId(profile.homeBaseLocationId ?? '')
    setStatusReason('')
    setStatusDraft(profile.employmentStatus)
  }, [profile])

  const managerChoices = peopleOptions.filter((person) => person.personId !== profile.personId)
  const orgUnitOptions = toOrgUnitOptions(orgUnits)
  const selectedOrgUnitOption = orgUnitOptions.find((option) => option.value === primaryOrgUnitId)
  const managerOptions: PickerOption[] = managerChoices.map((person) => ({
    value: person.personId,
    label: person.displayName,
  }))
  const selectedManagerOption = managerOptions.find((option) => option.value === managerPersonId)
  const employmentStatusOptions = fieldOptions(profileFieldsetQuery.data, 'employmentStatus')
  const workRelationshipOptions = fieldOptions(profileFieldsetQuery.data, 'workRelationshipType')
  const employmentTypeOptions = fieldOptions(profileFieldsetQuery.data, 'employmentType')
  const workerCategoryOptions = fieldOptions(profileFieldsetQuery.data, 'workerCategory')
  const flsaStatusOptions = fieldOptions(profileFieldsetQuery.data, 'flsaStatus')

  const locationQuery = useQuery({
    queryKey: ['staffarr-site-locations', accessToken, siteContextOrgUnitId],
    queryFn: () => listLocations(accessToken, { siteOrgUnitId: siteContextOrgUnitId! }),
    enabled: Boolean(accessToken && siteContextOrgUnitId),
  })

  const locationOptions = useMemo<PickerOption[]>(
    () =>
      (locationQuery.data ?? []).map((location) => ({
        value: location.locationId,
        label: location.parentPathSnapshot,
      })),
    [locationQuery.data],
  )
  const selectedLocationOption = locationOptions.find((option) => option.value === homeBaseLocationId)
  const personRolesQuery = useQuery({
    queryKey: ['staffarr-person-roles-inline-editor', accessToken, profile.personId],
    queryFn: () => getStaffPersonRoles(accessToken, profile.personId),
    enabled: Boolean(accessToken && profile.personId),
  })
  const rolesCatalogQuery = useQuery({
    queryKey: ['staffarr-role-catalog-inline-editor', accessToken],
    queryFn: () => listStaffRoles(accessToken),
    enabled: Boolean(accessToken),
  })

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onUpdate({
      legalFirstName: legalFirstName.trim(),
      legalMiddleName: legalMiddleName.trim() || null,
      legalLastName: legalLastName.trim(),
      preferredName: preferredName.trim() || null,
      pronouns: pronouns.trim() || null,
      givenName: legalFirstName.trim(),
      familyName: legalLastName.trim(),
      primaryEmail: primaryEmail.trim(),
      alternateEmail: alternateEmail.trim() || null,
      primaryPhone: primaryPhone.trim() || null,
      alternatePhone: alternatePhone.trim() || null,
      workPhone: workPhone.trim() || null,
      workRelationshipType: workRelationshipType || null,
      employmentType: employmentType || null,
      workerCategory: workerCategory || null,
      flsaStatus: flsaStatus || null,
      positionNumber: positionNumber.trim() || null,
      currentEmploymentAction: currentEmploymentAction.trim() || null,
      currentEmploymentActionAt: currentEmploymentActionAt ? new Date(currentEmploymentActionAt).toISOString() : null,
      leaveStatus: leaveStatus || null,
      eligibleForRehire,
      startDate: startDate || null,
      expectedStartDate: expectedStartDate || null,
      primaryOrgUnitId: primaryOrgUnitId || null,
      siteOrgUnitId: siteContextOrgUnitId || null,
      managerPersonId: managerPersonId || null,
      jobTitle: jobTitle.trim() || null,
      homeBaseLocationId: homeBaseLocationId || null,
    })
  }

  const handleApplyStatus = async () => {
    const highImpactStatuses = new Set(['leave', 'suspended', 'terminated', 'inactive'])
    if (highImpactStatuses.has(statusDraft)) {
      setPendingStatusConfirmation(true)
      return
    }

    await onEmploymentStatusChange({ employmentStatus: statusDraft, reason: statusReason || null })
  }

  const confirmApplyStatus = async () => {
    setPendingStatusConfirmation(false)
    await onEmploymentStatusChange({ employmentStatus: statusDraft, reason: statusReason || null })
  }

  const selectedAssignableRole = useMemo(
    () => (rolesCatalogQuery.data ?? []).find((role) => role.roleId === roleIdDraft) ?? null,
    [roleIdDraft, rolesCatalogQuery.data],
  )

  const availableRoleOptions = useMemo<PickerOption[]>(
    () =>
      (rolesCatalogQuery.data ?? [])
        .filter((role) => !role.isArchived)
        .map((role) => ({
          value: role.roleId,
          label: `${role.name} · ${role.permissionCount} permissions · ${role.scopeCount} scopes`,
        })),
    [rolesCatalogQuery.data],
  )
  const selectedAssignableRoleOption = availableRoleOptions.find((option) => option.value === roleIdDraft)

  const roleScopeOptions: Array<{
    value: SetStaffPersonRoleItemRequest['assignmentScopeType']
    label: string
    help: string
  }> = [
    { value: 'tenant', label: 'Entire tenant', help: 'Grants the role anywhere in this tenant.' },
    { value: 'site', label: 'Site only', help: 'Keeps the role limited to one site.' },
    { value: 'department', label: 'Department only', help: 'Applies only to one department.' },
    { value: 'team', label: 'Team only', help: 'Applies only to one team.' },
    { value: 'position', label: 'Position only', help: 'Applies only to one position group.' },
    { value: 'location', label: 'Location only', help: 'Applies only to one physical location.' },
    { value: 'direct_reports', label: 'Direct reports only', help: 'Limits access to the person’s direct reports.' },
    { value: 'own_records', label: 'Own records only', help: 'Limits access to the person’s own records.' },
    { value: 'assigned_assets', label: 'Assigned assets only', help: 'Limits access to assigned assets.' },
  ]

  const selectedScopeNeedsReference = ['site', 'department', 'team', 'position', 'location'].includes(roleScopeTypeDraft)
  const roleScopeReferenceOptions = useMemo<PickerOption[]>(() => {
    if (roleScopeTypeDraft === 'location') {
      return locationOptions
    }

    if (roleScopeTypeDraft === 'site') {
      return orgUnits
        .filter((unit) => unit.unitType.toLowerCase().includes('site'))
        .map((unit) => ({ value: unit.orgUnitId, label: unit.name }))
    }

    if (roleScopeTypeDraft === 'department') {
      return orgUnits
        .filter((unit) => unit.unitType.toLowerCase().includes('department'))
        .map((unit) => ({ value: unit.orgUnitId, label: unit.name }))
    }

    if (roleScopeTypeDraft === 'team') {
      return orgUnits
        .filter((unit) => unit.unitType.toLowerCase().includes('team'))
        .map((unit) => ({ value: unit.orgUnitId, label: unit.name }))
    }

    if (roleScopeTypeDraft === 'position') {
      return orgUnits
        .filter((unit) => unit.unitType.toLowerCase().includes('position'))
        .map((unit) => ({ value: unit.orgUnitId, label: unit.name }))
    }

    return []
  }, [locationOptions, orgUnits, roleScopeTypeDraft])

  const selectedRoleScopeRefOption = roleScopeReferenceOptions.find((option) => option.value === roleScopeRefIdDraft)

  const roleAssignmentsById = useMemo(
    () => new Map((rolesCatalogQuery.data ?? []).map((role) => [role.roleId, role])),
    [rolesCatalogQuery.data],
  )

  const resetRoleDraft = () => {
    setRoleIdDraft('')
    setRoleScopeTypeDraft('tenant')
    setRoleScopeRefIdDraft('')
    setRoleStartsAtDraft('')
    setRoleEndsAtDraft('')
  }

  const mapAssignmentsForSave = (assignments: StaffPersonRoleAssignmentResponse[]): SetStaffPersonRoleItemRequest[] =>
    assignments.map((assignment) => ({
      roleId: assignment.roleId,
      assignmentScopeType: assignment.assignmentScopeType,
      assignmentScopeRefId: assignment.assignmentScopeRefId,
      startsAt: assignment.startsAt,
      endsAt: assignment.endsAt,
    }))

  const roleAssignmentsMutation = useMutation({
    mutationFn: async (nextRoles: SetStaffPersonRoleItemRequest[]) =>
      setStaffPersonRoles(accessToken, profile.personId, { roles: nextRoles }),
    onSuccess: async () => {
      resetRoleDraft()
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-roles-inline-editor', accessToken, profile.personId] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-role-assignments', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person-history-summary', accessToken, profile.personId] }),
      ])
    },
  })

  const handleAssignRole = async () => {
    if (!roleIdDraft) {
      return
    }

    const currentAssignments = personRolesQuery.data ?? []
    const nextRoles = mapAssignmentsForSave(currentAssignments)
    nextRoles.push({
      roleId: roleIdDraft,
      assignmentScopeType: roleScopeTypeDraft,
      assignmentScopeRefId: selectedScopeNeedsReference ? roleScopeRefIdDraft || null : null,
      startsAt: roleStartsAtDraft ? new Date(roleStartsAtDraft).toISOString() : null,
      endsAt: roleEndsAtDraft ? new Date(roleEndsAtDraft).toISOString() : null,
    })
    await roleAssignmentsMutation.mutateAsync(nextRoles)
  }

  const handleRemoveRole = async (personRoleId: string) => {
    const currentAssignments = personRolesQuery.data ?? []
    const remaining = currentAssignments.filter((assignment) => assignment.personRoleId !== personRoleId)
    await roleAssignmentsMutation.mutateAsync(mapAssignmentsForSave(remaining))
  }

  const formatAssignmentScope = (assignment: StaffPersonRoleAssignmentResponse): string => {
    if (!assignment.assignmentScopeRefId) {
      return assignment.assignmentScopeType.replace(/_/g, ' ')
    }

    const orgUnit = orgUnits.find((unit) => unit.orgUnitId === assignment.assignmentScopeRefId)
    const location = locationQuery.data?.find((item) => item.locationId === assignment.assignmentScopeRefId)
    return orgUnit?.name ?? location?.name ?? assignment.assignmentScopeRefId
  }

  const formatDateTime = (value: string | null) => {
    if (!value) {
      return 'Not scheduled'
    }
    const date = new Date(value)
    if (Number.isNaN(date.getTime())) {
      return 'Not scheduled'
    }
    return date.toLocaleString()
  }

  const SectionHeading = ({
    title,
    description,
  }: {
    title: string
    description: string
  }) => (
    <div className="md:col-span-2">
      <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">{title}</h3>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">{description}</p>
    </div>
  )

  return (
    <section className="mt-6 space-y-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 shadow-[var(--shadow-surface)]">
      <ConfirmDialog
        open={pendingStatusConfirmation}
        title={`Apply the ${statusDraft.replace(/_/g, ' ')} status?`}
        description={`Use StaffArr offboarding when you also need to disable login or end active assignments for ${profile.displayName}.`}
        confirmLabel="Apply status"
        cancelLabel="Keep editing"
        danger
        onConfirm={() => void confirmApplyStatus()}
        onCancel={() => setPendingStatusConfirmation(false)}
      />
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-[var(--color-text-secondary)]">Profile management</h2>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Manage the StaffArr-owned person record with separate delegated account controls below.
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          {onOpenPermissionsReview ? (
            <button
              type="button"
              onClick={onOpenPermissionsReview}
              className="rounded-md border border-[var(--color-border-subtle)] px-3 py-2 text-xs font-medium text-[var(--color-text-primary)] hover:border-[var(--color-border-strong)] hover:bg-[var(--color-bg-control-hover)]"
            >
              Review permissions
            </button>
          ) : null}
          {canManage ? (
            <button
              type="submit"
              form="staffarr-person-profile-form"
              disabled={isSubmitting}
              className="rounded-md bg-[var(--color-accent)] px-3 py-2 text-xs font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
            >
              {isSubmitting ? 'Saving...' : 'Save profile changes'}
            </button>
          ) : null}
          <span className={`text-xs ${canManage ? 'text-[var(--color-success-text)]' : 'text-[var(--color-text-muted)]'}`}>
            {canManage ? 'Write enabled' : 'Read only'}
          </span>
        </div>
      </header>

      <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Role and permission access</h3>
            <p className="mt-1 text-xs text-[var(--color-text-muted)]">
              Review who this person can act as, where the role applies, and what changes will be granted before saving.
            </p>
          </div>
          <p className="max-w-sm text-right text-xs text-[var(--color-text-muted)]">
            StaffArr owns person authority. Effective permission detail stays in the permissions review tab.
          </p>
        </div>

        {personRolesQuery.isLoading || rolesCatalogQuery.isLoading ? (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">Loading role assignments...</p>
        ) : null}

        {personRolesQuery.data && personRolesQuery.data.length > 0 ? (
          <div className="mt-4 space-y-3">
            {personRolesQuery.data.map((assignment) => {
              const roleSummary = roleAssignmentsById.get(assignment.roleId)
              return (
                <div key={assignment.personRoleId} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <div className="flex flex-wrap items-center gap-2">
                        <p className="font-semibold text-[var(--color-text-primary)]">{assignment.roleName}</p>
                        <span className="rounded-full bg-[var(--color-bg-control)] px-2 py-0.5 text-[11px] text-[var(--color-text-secondary)]">
                          {assignment.roleType.replace(/_/g, ' ')}
                        </span>
                      </div>
                      <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                        Applies to {formatAssignmentScope(assignment)}. Starts {formatDateTime(assignment.startsAt)}.
                        {assignment.endsAt ? ` Ends ${formatDateTime(assignment.endsAt)}.` : ' No end date set.'}
                      </p>
                      {roleSummary ? (
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {roleSummary.permissionCount} permissions across {roleSummary.scopeCount} saved role scopes.
                        </p>
                      ) : null}
                    </div>
                    {canManage ? (
                      <button
                        type="button"
                        disabled={roleAssignmentsMutation.isPending}
                        onClick={() => void handleRemoveRole(assignment.personRoleId)}
                        className="rounded-md border border-[var(--tone-danger-border)] bg-[var(--tone-danger-bg)] px-3 py-2 text-xs font-medium text-[var(--tone-danger-text)] disabled:opacity-50"
                      >
                        Remove
                      </button>
                    ) : null}
                  </div>
                </div>
              )
            })}
          </div>
        ) : !personRolesQuery.isLoading ? (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">No StaffArr role assignments are currently recorded for this person.</p>
        ) : null}

        {canManage ? (
          <div className="mt-4 rounded-lg border border-dashed border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
            <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
              <label className="block text-sm text-[var(--color-text-secondary)]">
                Role
                <StaticSearchPicker
                  id="edit-person-role-assignment"
                  label="Role"
                  value={roleIdDraft}
                  onChange={setRoleIdDraft}
                  options={availableRoleOptions}
                  placeholder="Search roles..."
                  testId="edit-person-role-assignment-picker"
                  selectedOption={selectedAssignableRoleOption}
                />
              </label>
              <label htmlFor="edit-person-role-scope-type" className="block text-sm text-[var(--color-text-secondary)]">
                Scope
                <select
                  id="edit-person-role-scope-type"
                  value={roleScopeTypeDraft}
                  onChange={(event) => {
                    setRoleScopeTypeDraft(event.target.value as SetStaffPersonRoleItemRequest['assignmentScopeType'])
                    setRoleScopeRefIdDraft('')
                  }}
                  className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                >
                  {roleScopeOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </label>
              <label className="block text-sm text-[var(--color-text-secondary)]">
                Scope reference
                <StaticSearchPicker
                  id="edit-person-role-scope-reference"
                  label="Scope reference"
                  value={roleScopeRefIdDraft}
                  onChange={setRoleScopeRefIdDraft}
                  options={roleScopeReferenceOptions}
                  placeholder={selectedScopeNeedsReference ? 'Search scope...' : 'No reference needed'}
                  testId="edit-person-role-scope-reference-picker"
                  selectedOption={selectedRoleScopeRefOption}
                  disabled={!selectedScopeNeedsReference}
                />
              </label>
              <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-1">
                <label htmlFor="edit-person-role-starts-at" className="block text-sm text-[var(--color-text-secondary)]">
                  Starts at
                  <input
                    id="edit-person-role-starts-at"
                    type="datetime-local"
                    value={roleStartsAtDraft}
                    onChange={(event) => setRoleStartsAtDraft(event.target.value)}
                    className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                  />
                </label>
                <label htmlFor="edit-person-role-ends-at" className="block text-sm text-[var(--color-text-secondary)]">
                  Ends at
                  <input
                    id="edit-person-role-ends-at"
                    type="datetime-local"
                    value={roleEndsAtDraft}
                    onChange={(event) => setRoleEndsAtDraft(event.target.value)}
                    className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                  />
                </label>
              </div>
            </div>

            {selectedAssignableRole ? (
              <div className="mt-4 rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3 text-sm">
                <p className="font-medium text-[var(--color-text-primary)]">Assignment preview</p>
                <p className="mt-1 text-[var(--color-text-muted)]">
                  {selectedAssignableRole.name} grants {selectedAssignableRole.permissionCount} permissions and currently defines {selectedAssignableRole.scopeCount} saved role scopes.
                  This person assignment will apply at the {roleScopeOptions.find((option) => option.value === roleScopeTypeDraft)?.label.toLowerCase()} level.
                </p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  {selectedAssignableRole.description ?? 'No role description has been provided yet.'}
                </p>
              </div>
            ) : null}

            <div className="mt-4 flex flex-wrap items-center justify-between gap-3">
              <p className="text-xs text-[var(--color-text-muted)]">
                {roleScopeOptions.find((option) => option.value === roleScopeTypeDraft)?.help}
              </p>
              <button
                type="button"
                disabled={
                  roleAssignmentsMutation.isPending
                  || !roleIdDraft
                  || (selectedScopeNeedsReference && !roleScopeRefIdDraft)
                }
                onClick={() => void handleAssignRole()}
                className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
              >
                {roleAssignmentsMutation.isPending ? 'Saving assignment...' : 'Assign role'}
              </button>
            </div>
          </div>
        ) : null}

        {roleAssignmentsMutation.error ? (
          <ApiErrorCallout
            title="Role assignment update failed"
            message={getErrorMessage(roleAssignmentsMutation.error, 'Failed to update role assignments.')}
          />
        ) : null}
      </div>

      {canManage ? (
        <form id="staffarr-person-profile-form" className="grid gap-4 md:grid-cols-2" onSubmit={handleSubmit}>
          <SectionHeading
            title="Profile"
            description="Legal and display identity fields that StaffArr owns directly."
          />
          <label htmlFor="edit-person-legal-first-name" className="block text-sm text-[var(--color-text-secondary)]">
            Legal first name
            <input
              id="edit-person-legal-first-name"
              value={legalFirstName}
              onChange={(event) => setLegalFirstName(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
              required
            />
          </label>
          <label htmlFor="edit-person-legal-middle-name" className="block text-sm text-[var(--color-text-secondary)]">
            Legal middle name
            <input
              id="edit-person-legal-middle-name"
              value={legalMiddleName}
              onChange={(event) => setLegalMiddleName(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="edit-person-legal-last-name" className="block text-sm text-[var(--color-text-secondary)]">
            Legal last name
            <input
              id="edit-person-legal-last-name"
              value={legalLastName}
              onChange={(event) => setLegalLastName(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
              required
            />
          </label>
          <label htmlFor="edit-person-preferred-name" className="block text-sm text-[var(--color-text-secondary)]">
            Preferred name
            <input
              id="edit-person-preferred-name"
              value={preferredName}
              onChange={(event) => setPreferredName(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="edit-person-pronouns" className="block text-sm text-[var(--color-text-secondary)] md:col-span-2">
            Pronouns
            <input
              id="edit-person-pronouns"
              value={pronouns}
              onChange={(event) => setPronouns(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <SectionHeading
            title="Contact"
            description="StaffArr-owned work and alternate contact details."
          />
          <label htmlFor="edit-person-primary-email" className="block text-sm text-[var(--color-text-secondary)]">
            Work email
            <input
              id="edit-person-primary-email"
              type="email"
              value={primaryEmail}
              onChange={(event) => setPrimaryEmail(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
              required
            />
          </label>
          <label htmlFor="edit-person-alternate-email" className="block text-sm text-[var(--color-text-secondary)]">
            Alternate email
            <input
              id="edit-person-alternate-email"
              type="email"
              value={alternateEmail}
              onChange={(event) => setAlternateEmail(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="edit-person-primary-phone" className="block text-sm text-[var(--color-text-secondary)]">
            Primary phone
            <input
              id="edit-person-primary-phone"
              value={primaryPhone}
              onChange={(event) => setPrimaryPhone(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="edit-person-alternate-phone" className="block text-sm text-[var(--color-text-secondary)]">
            Alternate phone
            <input
              id="edit-person-alternate-phone"
              value={alternatePhone}
              onChange={(event) => setAlternatePhone(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="edit-person-work-phone" className="block text-sm text-[var(--color-text-secondary)]">
            Work phone
            <input
              id="edit-person-work-phone"
              value={workPhone}
              onChange={(event) => setWorkPhone(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <SectionHeading
            title="Employment"
            description="Worker classification and employment lifecycle details."
          />
          <label htmlFor="edit-person-job-title" className="block text-sm text-[var(--color-text-secondary)]">
            Job title
            <input
              id="edit-person-job-title"
              value={jobTitle}
              onChange={(event) => setJobTitle(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="edit-person-work-relationship" className="block text-sm text-[var(--color-text-secondary)]">
            Work relationship
            <select
              id="edit-person-work-relationship"
              value={workRelationshipType}
              onChange={(event) => setWorkRelationshipType(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            >
              {workRelationshipOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="edit-person-employment-type" className="block text-sm text-[var(--color-text-secondary)]">
            Employment type
            <select
              id="edit-person-employment-type"
              value={employmentType}
              onChange={(event) => setEmploymentType(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            >
              {employmentTypeOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="edit-person-worker-category" className="block text-sm text-[var(--color-text-secondary)]">
            Worker category
            <select
              id="edit-person-worker-category"
              value={workerCategory}
              onChange={(event) => setWorkerCategory(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            >
              {(workerCategoryOptions.length > 0
                ? workerCategoryOptions
                : [
                    { value: 'employee', label: 'Employee', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                    { value: 'contractor', label: 'Contractor', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                    { value: 'intern', label: 'Intern', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                    { value: 'temporary', label: 'Temporary', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                    { value: 'seasonal', label: 'Seasonal', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                    { value: 'volunteer', label: 'Volunteer', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                    { value: 'other', label: 'Other', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                  ]).map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
            </select>
          </label>
          <label htmlFor="edit-person-flsa-status" className="block text-sm text-[var(--color-text-secondary)]">
            FLSA status
            <select
              id="edit-person-flsa-status"
              value={flsaStatus}
              onChange={(event) => setFlsaStatus(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            >
              {(flsaStatusOptions.length > 0
                ? flsaStatusOptions
                : [
                    { value: 'exempt', label: 'Exempt', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                    { value: 'non_exempt', label: 'Non-exempt', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                    { value: 'outside_scope', label: 'Outside scope', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                    { value: 'unknown', label: 'Unknown', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
                  ]).map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
            </select>
          </label>
          <label htmlFor="edit-person-position-number" className="block text-sm text-[var(--color-text-secondary)]">
            Position number
            <input
              id="edit-person-position-number"
              value={positionNumber}
              onChange={(event) => setPositionNumber(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="edit-person-current-employment-action" className="block text-sm text-[var(--color-text-secondary)]">
            Current employment action
            <input
              id="edit-person-current-employment-action"
              value={currentEmploymentAction}
              onChange={(event) => setCurrentEmploymentAction(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="edit-person-current-employment-action-at" className="block text-sm text-[var(--color-text-secondary)]">
            Current employment action at
            <input
              id="edit-person-current-employment-action-at"
              type="datetime-local"
              value={currentEmploymentActionAt}
              onChange={(event) => setCurrentEmploymentActionAt(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="edit-person-leave-status" className="block text-sm text-[var(--color-text-secondary)]">
            Leave status
            <select
              id="edit-person-leave-status"
              value={leaveStatus}
              onChange={(event) => setLeaveStatus(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            >
              {[
                { value: 'active', label: 'Active' },
                { value: 'leave', label: 'Leave' },
                { value: 'suspended', label: 'Suspended' },
                { value: 'terminated', label: 'Terminated' },
                { value: 'inactive', label: 'Inactive' },
              ].map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="edit-person-eligible-for-rehire" className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
            <input
              id="edit-person-eligible-for-rehire"
              type="checkbox"
              checked={eligibleForRehire}
              onChange={(event) => setEligibleForRehire(event.target.checked)}
              className="h-4 w-4 rounded border-[var(--color-border-strong)] bg-[var(--color-bg-control)] text-[var(--color-accent)]"
            />
            Eligible for rehire
          </label>
          <label htmlFor="edit-person-start-date" className="block text-sm text-[var(--color-text-secondary)]">
            Start date
            <input
              id="edit-person-start-date"
              type="date"
              value={startDate}
              onChange={(event) => setStartDate(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="edit-person-expected-start-date" className="block text-sm text-[var(--color-text-secondary)]">
            Expected start date
            <input
              id="edit-person-expected-start-date"
              type="date"
              value={expectedStartDate}
              onChange={(event) => setExpectedStartDate(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <SectionHeading
            title="Organization placement"
            description="Controlled references for org placement, manager, and home location."
          />
          <label htmlFor="edit-person-primary-org-unit" className="block text-sm text-[var(--color-text-secondary)]">
            Primary org unit
            <StaticSearchPicker
              id="edit-person-primary-org-unit"
              label="Primary org unit"
              value={primaryOrgUnitId}
              onChange={setPrimaryOrgUnitId}
              options={orgUnitOptions}
              placeholder="Search org units..."
              testId="edit-person-primary-org-unit-picker"
              selectedOption={selectedOrgUnitOption}
            />
          </label>
          <label htmlFor="edit-person-manager" className="block text-sm text-[var(--color-text-secondary)]">
            Manager
            <StaticSearchPicker
              id="edit-person-manager"
              label="Manager"
              value={managerPersonId}
              onChange={setManagerPersonId}
              options={managerOptions}
              placeholder="Search managers..."
              testId="edit-person-manager-picker"
              selectedOption={selectedManagerOption}
            />
          </label>
          <label htmlFor="edit-person-home-base-location" className="block text-sm text-[var(--color-text-secondary)]">
            Home base location
            <StaticSearchPicker
              id="edit-person-home-base-location"
              label="Home base location"
              value={homeBaseLocationId}
              onChange={setHomeBaseLocationId}
              options={locationOptions}
              placeholder={siteContextOrgUnitId ? 'Search site locations...' : 'Select a site assignment first'}
              testId="edit-person-home-base-location-picker"
              selectedOption={selectedLocationOption}
              disabled={!siteContextOrgUnitId || locationQuery.isLoading}
            />
            {locationQuery.isLoading ? (
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">Loading locations...</p>
            ) : null}
          </label>
          <div className="md:col-span-2 flex flex-wrap items-center justify-between gap-3">
            <p className="text-xs text-[var(--color-text-muted)]">
              Save profile changes separately from role assignment updates so each audit action stays readable.
            </p>
            <button
              type="submit"
              disabled={isSubmitting}
              className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
            >
              {isSubmitting ? 'Saving...' : 'Save profile changes'}
            </button>
          </div>
        </form>
      ) : (
        <p className="text-sm text-[var(--color-text-muted)]">
          Profile edits require tenant admin, StaffArr admin, or HR admin role.
        </p>
      )}

      {canManage ? (
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
          <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Employment status</h3>
          <div className="mt-3 grid gap-3 md:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_auto]">
            <label htmlFor="edit-person-status" className="block text-sm text-[var(--color-text-secondary)]">
              Status
              <select
                id="edit-person-status"
                value={statusDraft}
                onChange={(event) => setStatusDraft(event.target.value)}
                className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
              >
                {employmentStatusOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="edit-person-status-reason" className="block text-sm text-[var(--color-text-secondary)]">
              Reason (optional)
              <input
                id="edit-person-status-reason"
                value={statusReason}
                onChange={(event) => setStatusReason(event.target.value)}
                className="mt-1 w-full rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
              />
            </label>
            <div className="self-end">
              <button
                type="button"
                disabled={isSubmitting || statusDraft === profile.employmentStatus}
                className="rounded-md border border-[var(--color-warning-border)] bg-[var(--color-warning-bg)] px-3 py-2 text-xs text-[var(--color-warning-text)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
                onClick={() => void handleApplyStatus()}
              >
                Apply status
              </button>
            </div>
          </div>
        </div>
      ) : null}

      <PersonAccountAccessPanel
        accessToken={accessToken}
        personId={profile.personId}
        displayName={profile.displayName}
        workEmail={profile.primaryEmail}
        canManage={canManage}
      />

      {errorMessage ? <ApiErrorCallout title="Profile update failed" message={errorMessage} /> : null}
    </section>
  )
}
