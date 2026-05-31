import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MyTeamPanel } from './MyTeamPanel'
import type { MyTeamDashboardResponse } from '../api/types'

const dashboard: MyTeamDashboardResponse = {
  directReportCount: 2,
  notReadyCount: 1,
  expiringCertificationCount: 1,
  openIncidentCount: 1,
  pendingUpdateRequestCount: 1,
  onboardingInProgressCount: 1,
  pendingTrainingBlockerCount: 1,
  members: [
    {
      summary: {
        personId: 'report-1',
        displayName: 'Alex Report',
        primaryEmail: 'alex.report@example.com',
        employmentStatus: 'active',
        jobTitle: 'Technician',
        primaryOrgUnitName: 'Main Site',
        managerPersonId: 'manager-1',
        managerDisplayName: 'Team Manager',
        depth: 1,
        directReportCount: 0,
        activeAssignmentPath: 'Main Site / Maintenance / Crew A / Technician',
      },
      readinessStatus: 'not_ready',
      blockerCount: 2,
      expiringCertificationCount: 1,
      openIncidentCount: 1,
      pendingUpdateRequestCount: 1,
      pendingTrainingBlockerCount: 1,
    },
    {
      summary: {
        personId: 'report-2',
        displayName: 'Blake Report',
        primaryEmail: 'blake.report@example.com',
        employmentStatus: 'active',
        jobTitle: 'Operator',
        primaryOrgUnitName: 'Main Site',
        managerPersonId: 'manager-1',
        managerDisplayName: 'Team Manager',
        depth: 1,
        directReportCount: 0,
        activeAssignmentPath: null,
      },
      readinessStatus: 'ready',
      blockerCount: 0,
      expiringCertificationCount: 0,
      openIncidentCount: 0,
      pendingUpdateRequestCount: 0,
      pendingTrainingBlockerCount: 0,
    },
  ],
  pendingUpdateRequests: [
    {
      requestId: 'req-1',
      personId: 'report-1',
      requestType: 'phone_update',
      status: 'submitted',
      fieldKey: 'work_phone',
      currentValue: '+1 555 0000',
      requestedValue: '+1 555 0100',
      details: 'Updated mobile',
      submittedByUserId: 'user-1',
      submittedAt: new Date().toISOString(),
      reviewedByUserId: null,
      reviewedAt: null,
      reviewNotes: null,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    },
  ],
}

describe('MyTeamPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders team metrics and member readiness', () => {
    render(<MyTeamPanel dashboard={dashboard} isLoading={false} errorMessage={null} />)

    expect(screen.getByTestId('my-team-panel')).toBeTruthy()
    expect(screen.getByTestId('my-team-metric-headcount').textContent).toContain('2')
    expect(screen.getByTestId('my-team-metric-not-ready').textContent).toContain('1')
    expect(screen.getByTestId('my-team-member-report-1')).toBeTruthy()
    expect(screen.getByTestId('my-team-members-table').textContent).toContain('Not ready')
    expect(screen.getByTestId('my-team-pending-request-req-1')).toBeTruthy()
  })

  it('shows empty state when manager has no direct reports', () => {
    render(
      <MyTeamPanel
        dashboard={{
          directReportCount: 0,
          notReadyCount: 0,
          expiringCertificationCount: 0,
          openIncidentCount: 0,
          pendingUpdateRequestCount: 0,
          onboardingInProgressCount: 0,
          pendingTrainingBlockerCount: 0,
          members: [],
          pendingUpdateRequests: [],
        }}
        isLoading={false}
        errorMessage={null}
      />,
    )

    expect(screen.getByText(/do not have any direct reports/i)).toBeTruthy()
  })

  it('calls review handler when manager approves a pending request', async () => {
    const onReviewRequest = vi.fn().mockResolvedValue(undefined)

    render(
      <MyTeamPanel
        dashboard={dashboard}
        isLoading={false}
        errorMessage={null}
        onReviewRequest={onReviewRequest}
      />,
    )

    fireEvent.click(screen.getByTestId('my-team-approve-req-1'))

    await waitFor(() => {
      expect(onReviewRequest).toHaveBeenCalledWith('req-1', {
        decision: 'approve',
        reviewNotes: null,
        applyToProfile: true,
      })
    })
  })

  it('renders dashboard fetch errors in a shared alert callout', () => {
    render(
      <MyTeamPanel
        dashboard={null}
        isLoading={false}
        errorMessage="Dashboard request failed"
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Team dashboard failed to load')).toBeTruthy()
    expect(screen.getByText('Dashboard request failed')).toBeTruthy()
  })

  it('renders review errors in a shared alert callout', () => {
    render(
      <MyTeamPanel
        dashboard={dashboard}
        isLoading={false}
        errorMessage={null}
        onReviewRequest={vi.fn()}
        reviewingRequestId="req-1"
        reviewErrorMessage="Could not submit decision"
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Request review failed')).toBeTruthy()
    expect(screen.getByText('Could not submit decision')).toBeTruthy()
  })

  it('renders unavailable dashboard state in shared callout', () => {
    render(
      <MyTeamPanel
        dashboard={null}
        isLoading={false}
        errorMessage={null}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Team dashboard unavailable')).toBeTruthy()
    expect(screen.getByText('Could not load team dashboard data.')).toBeTruthy()
  })
})
