import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  getStaffArrTenantSettings,
  getStaffArrTenantSettingsDefaults,
  updateStaffArrTenantSettings,
} from '../api/client'
import type { StaffArrTenantSettingsResponse } from '../api/types'
import { StaffArrTenantSettingsPanel } from './StaffArrTenantSettingsPanel'

vi.mock('../api/client', () => ({
  getStaffArrTenantSettings: vi.fn(),
  getStaffArrTenantSettingsDefaults: vi.fn(),
  updateStaffArrTenantSettings: vi.fn(),
}))

function makeSettings(
  overrides: Partial<StaffArrTenantSettingsResponse> = {},
): StaffArrTenantSettingsResponse {
  return {
    tenantId: 'tenant-1',
    personDirectory: {
      displayNameFormat: 'preferred_first_last',
      preferredNameEnabled: true,
      employeeNumberLabel: 'Employee number',
      employeeNumberRequired: false,
      employeeNumberUniquenessScope: 'tenant',
      profilePhotoEnabled: false,
      contactVisibilityMode: 'manager_admin',
      emergencyContactEnabled: true,
      personalAddressEnabled: false,
    },
    personLifecycle: {
      defaultPersonStatusOnCreate: 'pending_start',
      requireManagerBeforeActivation: false,
      requirePositionBeforeActivation: false,
      requireHomeLocationBeforeActivation: false,
      allowInactivePeopleToBeAssignedWork: false,
      rehireMatchBehavior: 'flag_possible_match',
      deactivationReasonRequired: true,
      autoRemoveRolesOnDeactivation: false,
      autoEndTeamAssignmentsOnDeactivation: false,
    },
    orgStructure: {
      orgHierarchyMode: 'standard',
      requireEveryPersonInOrgUnit: false,
      requireDepartmentUnderSite: true,
      allowMatrixMembership: true,
      primaryAssignmentRequired: false,
      managerHierarchyRequired: false,
      allowSkipLevelManagers: true,
      preventCircularReporting: true,
    },
    locationHierarchy: {
      locationHierarchyMode: 'site_required',
      requireLocationCode: false,
      locationCodeUniquenessScope: 'parent',
      allowOperationalLocations: true,
      allowAddressableBinsShelves: true,
      allowMobileLocations: true,
      requireParentLocationExceptRoot: false,
      archivedLocationAssignmentBehavior: 'block_new_assignments',
    },
    rolePermissions: {
      roleAssignmentApprovalRequired: false,
      allowSelfServiceRoleRequests: false,
      roleExpirationEnabled: false,
      defaultRoleGrantDurationDays: null,
      requireAssignmentReason: false,
      permissionReviewCadence: 'quarterly',
      autoRemoveRolesOnInactivePerson: false,
      allowDirectPermissions: false,
      preferRolesOverDirectPermissions: true,
      siteScopedRoleAssignmentsEnabled: true,
    },
    teamsAssignments: {
      teamMembershipMode: 'flexible',
      requireTeamLead: false,
      allowTemporaryAssignments: true,
      temporaryAssignmentMaxDurationDays: 90,
      assignmentEffectiveDatingEnabled: true,
      historicalAssignmentVisibilityMode: 'admin_all',
      allowOpenPositions: true,
    },
    incidents: {
      incidentIntakeEnabled: true,
      requireIncidentCategory: true,
      requireInvolvedPerson: true,
      managerNotificationMode: 'optional',
      trainArrRoutingEnabled: true,
      retrainingRecommendationThreshold: 3,
      incidentVisibilityMode: 'management',
      closureApprovalRequired: false,
    },
    profileFieldGovernance: {
      requiredProfileSections: ['identity', 'work'],
      optionalProfileSections: ['contact', 'emergency', 'address', 'photo'],
      customProfileFieldsEnabled: false,
      fieldVisibilityByRoleEnabled: false,
      fieldEditabilityByRoleEnabled: false,
      fieldReviewRequired: false,
      fieldHistoryEnabled: true,
    },
    notificationsReviews: {
      notifyManagerOnNewPerson: true,
      notifyOnManagerChange: true,
      notifyOnRoleGrantRemoval: true,
      notifyBeforeRoleExpiration: false,
      notifyOnInactiveAssignmentConflict: true,
      reviewRemindersEnabled: true,
      digestFrequency: 'daily',
    },
    dataGovernanceAudit: {
      auditProfileChanges: true,
      auditRoleChanges: true,
      auditOrgLocationChanges: true,
      requireChangeReasonForSensitiveEdits: false,
      softArchiveOnly: true,
      recordRetentionHintDays: null,
      exportEnabled: true,
      bulkImportEnabled: true,
      bulkImportReviewRequired: false,
    },
    crossProductReferences: {
      exposePeopleReferenceApi: true,
      exposeLocationReferenceApi: true,
      exposeOrgUnitReferenceApi: true,
      publishPersonLifecycleEvents: true,
      publishOrgLocationEvents: true,
      allowProductOriginatedPersonProposals: false,
      requireReviewForProductOriginatedProposals: true,
      snapshotLabelPolicy: 'display_label_with_status',
    },
    createdAt: '2026-06-18T00:00:00Z',
    updatedAt: '2026-06-18T00:00:00Z',
    ...overrides,
  }
}

function renderPanel(settings = makeSettings()) {
  vi.mocked(getStaffArrTenantSettings).mockResolvedValue(settings)
  vi.mocked(getStaffArrTenantSettingsDefaults).mockResolvedValue(makeSettings())
  vi.mocked(updateStaffArrTenantSettings).mockImplementation(async (_token, request) => ({
    ...settings,
    ...request,
    updatedAt: '2026-06-18T00:05:00Z',
  }))

  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <StaffArrTenantSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('StaffArrTenantSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders grouped settings tabs and sections', async () => {
    renderPanel()

    expect(await screen.findByRole('tab', { name: 'People' })).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Org & Locations' })).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Roles & Permissions' })).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Incidents' })).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Governance' })).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Integrations' })).toBeTruthy()
    expect(screen.getByText('Directory')).toBeTruthy()
    expect(screen.getByText('Lifecycle')).toBeTruthy()

    fireEvent.click(screen.getByRole('tab', { name: 'Org & Locations' }))
    expect(screen.getByText('Organization')).toBeTruthy()
    expect(screen.getByText('Locations')).toBeTruthy()
  })

  it('disables role duration when role expiration is disabled', async () => {
    renderPanel()

    fireEvent.click(await screen.findByRole('tab', { name: 'Roles & Permissions' }))

    const durationInput = screen.getByLabelText('Default role grant duration') as HTMLInputElement
    expect(durationInput.disabled).toBe(true)
  })

  it('saves changed settings successfully', async () => {
    renderPanel()

    await screen.findByRole('tab', { name: 'People' })
    fireEvent.click(screen.getByLabelText('Preferred name'))
    fireEvent.click(screen.getByRole('button', { name: /Save settings/i }))

    await screen.findByText('Settings saved.')
    expect(updateStaffArrTenantSettings).toHaveBeenCalledTimes(1)
  })

  it('shows validation failures before submitting invalid settings', async () => {
    renderPanel()

    fireEvent.click(await screen.findByRole('tab', { name: 'Roles & Permissions' }))
    fireEvent.click(screen.getByLabelText('Role expiration'))
    fireEvent.change(screen.getByLabelText('Default role grant duration'), { target: { value: '0' } })
    fireEvent.click(screen.getByRole('button', { name: /Save settings/i }))

    expect((await screen.findByRole('alert')).textContent).toMatch(
      /Default role grant duration must be positive/i,
    )
    await waitFor(() => expect(updateStaffArrTenantSettings).not.toHaveBeenCalled())
  })
})
