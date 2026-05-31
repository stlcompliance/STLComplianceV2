import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { formatAssignmentMutationError, PersonOrgAssignmentsManager } from './PersonOrgAssignmentsManager'

const orgUnits = [
  { orgUnitId: 's1', unitType: 'site', name: 'North Site', parentOrgUnitId: null, status: 'active' as const },
  { orgUnitId: 'd1', unitType: 'department', name: 'Ops', parentOrgUnitId: 's1', status: 'active' as const },
  { orgUnitId: 't1', unitType: 'team', name: 'Field Team', parentOrgUnitId: 'd1', status: 'active' as const },
  { orgUnitId: 'p1', unitType: 'position', name: 'Operator', parentOrgUnitId: 't1', status: 'active' as const },
]

const assignments = [
  {
    assignmentId: 'a1',
    personId: 'person-1',
    siteOrgUnitId: 's1',
    departmentOrgUnitId: 'd1',
    teamOrgUnitId: 't1',
    positionOrgUnitId: 'p1',
    status: 'active' as const,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
]

describe('PersonOrgAssignmentsManager', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders read-only state for non-writers', () => {
    render(
      <PersonOrgAssignmentsManager
        personId="person-1"
        personDisplayName="Jane Doe"
        orgUnits={orgUnits}
        assignments={assignments}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage={false}
        isSubmitting={false}
        actionErrorMessage={null}
        onCreate={vi.fn(async () => {})}
        onUpdate={vi.fn(async () => {})}
        onStatusChange={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('Read only')).toBeTruthy()
    expect(screen.getByText('Your role does not include org assignment write permission.')).toBeTruthy()
    expect(screen.queryByText('Create assignment')).toBeNull()
  })

  it('dispatches status toggle for selected assignment', async () => {
    const onStatusChange = vi.fn(async () => {})
    render(
      <PersonOrgAssignmentsManager
        personId="person-1"
        personDisplayName="Jane Doe"
        orgUnits={orgUnits}
        assignments={assignments}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onCreate={vi.fn(async () => {})}
        onUpdate={vi.fn(async () => {})}
        onStatusChange={onStatusChange}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'North Site / Ops / Field Team / Operator' }))
    fireEvent.click(screen.getByRole('button', { name: 'Deactivate' }))

    expect(onStatusChange).toHaveBeenCalledWith('a1', 'inactive')
  })

  it('classifies conflict errors for UI copy', () => {
    expect(formatAssignmentMutationError('{"status":409,"code":"org_assignment.link_invalid"}')).toContain('Conflict:')
    expect(formatAssignmentMutationError('{"status":403}')).toContain('Forbidden:')
    expect(formatAssignmentMutationError('{"status":400}')).toContain('Validation:')
  })

  it('renders assignment error in callout', () => {
    render(
      <PersonOrgAssignmentsManager
        personId="person-1"
        personDisplayName="Jane Doe"
        orgUnits={orgUnits}
        assignments={assignments}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage="assignment failed"
        onCreate={vi.fn(async () => {})}
        onUpdate={vi.fn(async () => {})}
        onStatusChange={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('assignment failed')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })

  it('renders retryable read error callout', () => {
    const onRetryRead = vi.fn()
    render(
      <PersonOrgAssignmentsManager
        personId="person-1"
        personDisplayName="Jane Doe"
        orgUnits={orgUnits}
        assignments={[]}
        isLoading={false}
        isError
        readErrorMessage="assignment read failed"
        onRetryRead={onRetryRead}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onCreate={vi.fn(async () => {})}
        onUpdate={vi.fn(async () => {})}
        onStatusChange={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('Org assignments unavailable')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry assignments' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })
})
