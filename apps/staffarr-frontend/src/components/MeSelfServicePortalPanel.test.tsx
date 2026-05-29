import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MeSelfServicePortalPanel } from './MeSelfServicePortalPanel'
import type { MePortalSummaryResponse } from '../api/types'

const summary: MePortalSummaryResponse = {
  session: {
    userId: 'user-1',
    personId: 'person-1',
    email: 'worker@example.com',
    displayName: 'Worker Example',
    tenantId: 'tenant-1',
    tenantRoleKey: 'tenant_member',
    isPlatformAdmin: false,
    productKey: 'staffarr',
    hasStaffArrEntitlement: true,
    primaryOrgUnitName: 'Main Site',
    jobTitle: 'Technician',
    entitlements: ['staffarr', 'trainarr'],
  },
  profile: {
    personId: 'person-1',
    externalUserId: null,
    givenName: 'Worker',
    familyName: 'Example',
    displayName: 'Worker Example',
    primaryEmail: 'worker@example.com',
    employmentStatus: 'active',
    jobTitle: 'Technician',
    workPhone: '+1 555 0100',
    placement: {
      primaryOrgUnitId: 'org-1',
      primaryOrgUnitName: 'Main Site',
      primaryOrgUnitType: 'site',
      managerPersonId: 'manager-1',
      managerDisplayName: 'Manager Example',
      activeAssignments: [
        {
          assignmentId: 'assign-1',
          siteOrgUnitId: 'site-1',
          siteName: 'Main Site',
          departmentOrgUnitId: 'dept-1',
          departmentName: 'Maintenance',
          teamOrgUnitId: 'team-1',
          teamName: 'Crew A',
          positionOrgUnitId: 'pos-1',
          positionName: 'Technician',
          assignmentPath: 'Main Site / Maintenance / Crew A / Technician',
        },
      ],
    },
    lookedUpAt: new Date().toISOString(),
  },
  readiness: {
    readinessStatus: 'not_ready',
    readinessBasis: 'Missing required certification',
    blockerMessages: ['Forklift certification is missing'],
  },
  certifications: {
    activeCount: 1,
    expiringSoonCount: 0,
    missingRequirementCount: 1,
    highlights: [],
  },
  permissions: {
    permissionCount: 2,
    permissionSummaries: ['View people (staffarr.people.read, tenant)'],
  },
  onboarding: {
    overallStatus: 'in_progress',
    completedSteps: 2,
    totalSteps: 5,
    blockedSteps: 1,
  },
  directReportCount: 0,
  directReportsPreview: [],
  productAccess: ['staffarr', 'trainarr'],
}

describe('MeSelfServicePortalPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders profile summary and submits update request', async () => {
    const onSubmitUpdate = vi.fn().mockResolvedValue(undefined)
    const onSubmitIncident = vi.fn().mockResolvedValue(undefined)

    render(
      <MeSelfServicePortalPanel
        summary={summary}
        updateRequests={[]}
        incidentReports={[]}
        isLoading={false}
        isSubmittingUpdate={false}
        isSubmittingIncident={false}
        errorMessage={null}
        onSubmitUpdateRequest={onSubmitUpdate}
        onSubmitIncidentReport={onSubmitIncident}
      />,
    )

    expect(screen.getByTestId('me-self-service-portal')).toBeTruthy()
    expect(screen.getByText('Forklift certification is missing')).toBeTruthy()
    expect(screen.getByText(/steps complete/)).toBeTruthy()

    fireEvent.change(screen.getByTestId('me-update-requested-value'), {
      target: { value: '+1 555 0100' },
    })
    fireEvent.submit(screen.getByTestId('me-update-submit').closest('form')!)

    expect(onSubmitUpdate).toHaveBeenCalledWith(
      expect.objectContaining({
        requestType: 'phone_update',
        fieldKey: 'work_phone',
        requestedValue: '+1 555 0100',
      }),
    )
  })

  it('submits incident self-report', async () => {
    const onSubmitUpdate = vi.fn().mockResolvedValue(undefined)
    const onSubmitIncident = vi.fn().mockResolvedValue(undefined)

    render(
      <MeSelfServicePortalPanel
        summary={summary}
        updateRequests={[]}
        incidentReports={[]}
        isLoading={false}
        isSubmittingUpdate={false}
        isSubmittingIncident={false}
        errorMessage={null}
        onSubmitUpdateRequest={onSubmitUpdate}
        onSubmitIncidentReport={onSubmitIncident}
      />,
    )

    const portal = screen.getByTestId('me-self-service-portal')
    fireEvent.change(within(portal).getByTestId('me-incident-title'), {
      target: { value: 'Slip on loading dock' },
    })
    fireEvent.change(within(portal).getByTestId('me-incident-description'), {
      target: { value: 'I slipped on a wet loading dock while moving pallets.' },
    })
    fireEvent.submit(within(portal).getByTestId('me-incident-submit').closest('form')!)

    expect(onSubmitIncident).toHaveBeenCalledWith(
      expect.objectContaining({
        reasonCategoryKey: 'safety',
        severity: 'medium',
        title: 'Slip on loading dock',
        description: 'I slipped on a wet loading dock while moving pallets.',
      }),
    )
  })

  it('lists prior incident reports', () => {
    render(
      <MeSelfServicePortalPanel
        summary={summary}
        updateRequests={[]}
        incidentReports={[
          {
            incidentId: 'inc-1',
            personId: 'person-1',
            reasonCategoryKey: 'safety',
            severity: 'medium',
            status: 'submitted',
            title: 'Near miss in warehouse',
            occurredAt: new Date().toISOString(),
            reportedAt: new Date().toISOString(),
            reportedByUserId: 'user-1',
            trainarrRouting: null,
          },
        ]}
        isLoading={false}
        isSubmittingUpdate={false}
        isSubmittingIncident={false}
        errorMessage={null}
        onSubmitUpdateRequest={vi.fn()}
        onSubmitIncidentReport={vi.fn()}
      />,
    )

    expect(screen.getByText('Near miss in warehouse')).toBeTruthy()
    expect(screen.getByText(/submitted/)).toBeTruthy()
  })
})
