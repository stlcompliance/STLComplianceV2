import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { OrgUnitResponse } from '../api/types'
import { PermissionProjectionTimelinePanel } from './PermissionProjectionTimelinePanel'

const orgUnits: OrgUnitResponse[] = [
  {
    orgUnitId: 'site-1',
    unitType: 'site',
    name: 'HQ',
    parentOrgUnitId: null,
    status: 'active',
  },
]

describe('PermissionProjectionTimelinePanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders effective permissions', () => {
    render(
      <PermissionProjectionTimelinePanel
        personDisplayName="Alex"
        orgUnits={orgUnits}
        projection={{
          personId: 'person-1',
          computedAt: new Date().toISOString(),
          permissions: [
            {
              permissionKey: 'staffarr.people.read',
              permissionName: 'People read',
              scopeType: 'site',
              scopeValue: 'site-1',
              sources: [
                {
                  assignmentId: 'assignment-1',
                  roleId: 'role-1',
                  roleKey: 'staffarr.viewer',
                  roleName: 'Viewer',
                  assignmentStatus: 'active',
                  assignmentScopeType: 'site',
                  assignmentScopeValue: 'site-1',
                  assignedAt: new Date().toISOString(),
                },
              ],
            },
          ],
        }}
      />,
    )

    expect(screen.getByText('Scoped effective permissions')).toBeTruthy()
    expect(screen.getByText('People read')).toBeTruthy()
  })

  it('renders retryable read error callout', () => {
    const onRetryRead = vi.fn()
    render(
      <PermissionProjectionTimelinePanel
        personDisplayName="Alex"
        orgUnits={orgUnits}
        projection={null}
        isLoading={false}
        isError
        readErrorMessage="permission read failed"
        onRetryRead={onRetryRead}
      />,
    )

    expect(screen.getByText('Permission projection unavailable')).toBeTruthy()
    expect(screen.getByText('permission read failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry permissions' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })
})
