import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PersonLookupPanel } from './PersonLookupPanel'
import type { PersonLookupResponse } from '../api/types'

const sampleLookup: PersonLookupResponse = {
  personId: '11111111-1111-1111-1111-111111111111',
  externalUserId: null,
  givenName: 'Alex',
  familyName: 'Rivera',
  displayName: 'Alex Rivera',
  primaryEmail: 'alex.rivera@example.com',
  employmentStatus: 'active',
  jobTitle: 'Operator',
  workPhone: null,
  placement: {
    primaryOrgUnitId: '22222222-2222-2222-2222-222222222222',
    primaryOrgUnitName: 'Day Shift',
    primaryOrgUnitType: 'team',
    managerPersonId: '33333333-3333-3333-3333-333333333333',
    managerDisplayName: 'Jordan Lee',
    activeAssignments: [
      {
        assignmentId: '44444444-4444-4444-4444-444444444444',
        siteOrgUnitId: '55555555-5555-5555-5555-555555555555',
        siteName: 'North Plant',
        departmentOrgUnitId: '66666666-6666-6666-6666-666666666666',
        departmentName: 'Operations',
        teamOrgUnitId: '22222222-2222-2222-2222-222222222222',
        teamName: 'Day Shift',
        positionOrgUnitId: '77777777-7777-7777-7777-777777777777',
        positionName: 'Operator',
        assignmentPath: 'North Plant / Operations / Day Shift / Operator',
      },
    ],
  },
  lookedUpAt: '2026-05-28T12:00:00.000Z',
}

describe('PersonLookupPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders identity, placement, and active assignment path', () => {
    render(
      <PersonLookupPanel
        personId={sampleLookup.personId}
        personDisplayName={sampleLookup.displayName}
        lookup={sampleLookup}
        isLoading={false}
      />,
    )

    expect(screen.getByTestId('person-lookup-panel')).toBeTruthy()
    expect(screen.getByText('Alex Rivera')).toBeTruthy()
    expect(screen.getByText('Jordan Lee')).toBeTruthy()
    expect(screen.getByText('North Plant / Operations / Day Shift / Operator')).toBeTruthy()
  })

  it('renders empty assignment state', () => {
    render(
      <PersonLookupPanel
        personId={sampleLookup.personId}
        personDisplayName={sampleLookup.displayName}
        lookup={{
          ...sampleLookup,
          placement: { ...sampleLookup.placement, activeAssignments: [] },
        }}
        isLoading={false}
      />,
    )

    expect(screen.getByText('No active site/department/team assignments.')).toBeTruthy()
  })

  it('renders retryable error callout when lookup query fails', () => {
    const onRetryRead = vi.fn()
    render(
      <PersonLookupPanel
        personId={sampleLookup.personId}
        personDisplayName={sampleLookup.displayName}
        lookup={null}
        isLoading={false}
        isError
        readErrorMessage="lookup service unavailable"
        onRetryRead={onRetryRead}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Person lookup unavailable')).toBeTruthy()
    expect(screen.getByText('lookup service unavailable')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry person lookup' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })

  it('renders unavailable lookup placeholder when payload is missing', () => {
    render(
      <PersonLookupPanel
        personId={sampleLookup.personId}
        personDisplayName={sampleLookup.displayName}
        lookup={null}
        isLoading={false}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Person lookup unavailable')).toBeTruthy()
    expect(screen.getByText('Person lookup is not available for this profile yet.')).toBeTruthy()
  })
})
