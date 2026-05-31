import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PermissionProjectionTimelinePanel } from './PermissionProjectionTimelinePanel'

const orgUnits = [
  {
    orgUnitId: 'site-1',
    unitType: 'site',
    name: 'HQ',
    parentOrgUnitId: null,
    status: 'active' as const,
  },
]

describe('PermissionProjectionTimelinePanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders effective permissions and timeline entries', () => {
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
                  roleTemplateId: 'role-1',
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
        timeline={[
          {
            eventId: 'event-1',
            personId: 'person-1',
            assignmentId: 'assignment-1',
            roleTemplateId: 'role-1',
            permissionTemplateId: 'permission-1',
            actorUserId: 'actor-1',
            eventType: 'assignment_created',
            assignmentStatus: 'active',
            roleKey: 'staffarr.viewer',
            roleName: 'Viewer',
            permissionKey: 'staffarr.people.read',
            permissionName: 'People read',
            scopeType: 'site',
            scopeValue: 'site-1',
            occurredAt: new Date().toISOString(),
          },
        ]}
      />,
    )

    expect(screen.getByText('Scoped effective permissions and history')).toBeTruthy()
    expect(screen.getByText('People read')).toBeTruthy()
    expect(screen.getByText('Assignment created')).toBeTruthy()
    expect(screen.getByText(/staffarr.people.read via staffarr.viewer/i)).toBeTruthy()
  })

  it('renders retryable read error callout', () => {
    const onRetryRead = vi.fn()
    render(
      <PermissionProjectionTimelinePanel
        personDisplayName="Alex"
        orgUnits={orgUnits}
        projection={null}
        timeline={[]}
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
