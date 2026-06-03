import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PermissionCheckPanel } from './PermissionCheckPanel'

describe('PermissionCheckPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders check results and submits the current key list', async () => {
    const onCheckPermissions = vi.fn().mockResolvedValue(undefined)
    const onPermissionCheckInputChange = vi.fn()

    render(
      <PermissionCheckPanel
        personId="person-1"
        personDisplayName="Alex"
        permissionCheckInput="staffarr.people.read, maintainarr.work_orders.close"
        checkResult={{
          personId: 'person-1',
          externalUserId: null,
          isPersonActive: true,
          computedAt: new Date().toISOString(),
          isAuthorizedAll: false,
          isAuthorizedAny: true,
          checks: [
            {
              permissionKey: 'staffarr.people.read',
              granted: true,
              grants: [
                {
                  permissionKey: 'staffarr.people.read',
                  permissionName: 'People read',
                  scopeType: 'tenant',
                  scopeValue: null,
                  roleKey: 'staffarr.viewer',
                  roleName: 'Viewer',
                },
              ],
            },
            {
              permissionKey: 'maintainarr.work_orders.close',
              granted: false,
              grants: [],
            },
          ],
        }}
        isChecking={false}
        errorMessage={null}
        onPermissionCheckInputChange={onPermissionCheckInputChange}
        onCheckPermissions={onCheckPermissions}
      />,
    )

    expect(screen.getByText('Permission check')).toBeTruthy()
    expect(screen.getByText('Active person')).toBeTruthy()
    expect(screen.getByText('staffarr.people.read')).toBeTruthy()
    expect(screen.getByText('No matching grants were found.')).toBeTruthy()

    fireEvent.change(screen.getByLabelText('Permission keys'), {
      target: { value: 'staffarr.people.read' },
    })
    expect(onPermissionCheckInputChange).toHaveBeenCalledWith('staffarr.people.read')

    fireEvent.submit(screen.getByLabelText('Permission keys').closest('form')!)
    expect(onCheckPermissions).toHaveBeenCalledTimes(1)
  })

  it('renders a failure callout', () => {
    render(
      <PermissionCheckPanel
        personId="person-1"
        personDisplayName="Alex"
        permissionCheckInput=""
        checkResult={null}
        isChecking={false}
        errorMessage="permission check failed"
        onPermissionCheckInputChange={vi.fn()}
        onCheckPermissions={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Permission check failed')).toBeTruthy()
    expect(screen.getByText('permission check failed')).toBeTruthy()
  })
})
