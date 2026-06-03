import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { formatRoleTemplateMutationError, RoleTemplateAssignmentPanel } from './RoleTemplateAssignmentPanel'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      id,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      id?: string
    }) => (
      <label htmlFor={id ?? label}>
        {label}
        <input
          id={id ?? label}
          aria-label={label}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
      </label>
    ),
  }
})

const orgUnits = [
  {
    orgUnitId: 'site-1',
    unitType: 'site',
    name: 'HQ',
    parentOrgUnitId: null,
    status: 'active' as const,
  },
]

describe('RoleTemplateAssignmentPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows read-only fallback for non-writers', () => {
    render(
      <RoleTemplateAssignmentPanel
        personId="person-1"
        personDisplayName="Alex"
        orgUnits={orgUnits}
        permissionTemplates={[]}
        roleTemplates={[]}
        roleAssignments={[]}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage={false}
        isSubmitting={false}
        actionErrorMessage={null}
        onUpsertPermissionTemplate={vi.fn(async () => {})}
        onCreateRoleTemplate={vi.fn(async () => {})}
        onUpdateRoleTemplateStatus={vi.fn(async () => {})}
        onCreateRoleAssignment={vi.fn(async () => {})}
        onUpdateRoleAssignmentStatus={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('Read only')).toBeTruthy()
    expect(screen.getByText('Your role does not include role template or permission assignment write permission.')).toBeTruthy()
    expect(screen.queryByText('Upsert permission template')).toBeNull()
  })

  it('submits permission template upsert', async () => {
    const onUpsert = vi.fn(async () => {})
    render(
      <RoleTemplateAssignmentPanel
        personId="person-1"
        personDisplayName="Alex"
        orgUnits={orgUnits}
        permissionTemplates={[]}
        roleTemplates={[]}
        roleAssignments={[]}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onUpsertPermissionTemplate={onUpsert}
        onCreateRoleTemplate={vi.fn(async () => {})}
        onUpdateRoleTemplateStatus={vi.fn(async () => {})}
        onCreateRoleAssignment={vi.fn(async () => {})}
        onUpdateRoleAssignmentStatus={vi.fn(async () => {})}
      />,
    )

    fireEvent.change(screen.getByLabelText('Permission name'), {
      target: { value: 'Assign permissions' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Save permission template' }))

    expect(onUpsert).toHaveBeenCalledWith({
      permissionKey: 'perm.staffarr.assignpermissions',
      name: 'Assign permissions',
      description: null,
    })
  })

  it('submits expiring role assignment payload and renders expiry metadata', async () => {
    const onCreateRoleAssignment = vi.fn(async () => {})
    render(
      <RoleTemplateAssignmentPanel
        personId="person-1"
        personDisplayName="Alex"
        orgUnits={orgUnits}
        permissionTemplates={[
          {
            permissionTemplateId: 'perm-1',
            permissionKey: 'staffarr.people.read',
            name: 'People read',
            description: null,
            status: 'active',
          },
        ]}
        roleTemplates={[
          {
            roleTemplateId: 'role-1',
            roleKey: 'staffarr.supervisor',
            name: 'StaffArr Supervisor',
            description: null,
            status: 'active',
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
            permissions: [
              {
                mappingId: 'mapping-1',
                permissionTemplateId: 'perm-1',
                permissionKey: 'staffarr.people.read',
                permissionName: 'People read',
                scopeType: 'tenant',
                scopeValue: null,
              },
            ],
          },
        ]}
        roleAssignments={[
          {
            assignmentId: 'assign-1',
            personId: 'person-1',
            roleTemplateId: 'role-1',
            roleKey: 'staffarr.supervisor',
            roleName: 'StaffArr Supervisor',
            scopeType: 'tenant',
            scopeValue: null,
            status: 'active',
            effectiveStatus: 'active',
            expiresAt: new Date('2026-06-03T12:30:00Z').toISOString(),
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
          },
        ]}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onUpsertPermissionTemplate={vi.fn(async () => {})}
        onCreateRoleTemplate={vi.fn(async () => {})}
        onUpdateRoleTemplateStatus={vi.fn(async () => {})}
        onCreateRoleAssignment={onCreateRoleAssignment}
        onUpdateRoleAssignmentStatus={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText(/expires/i)).toBeTruthy()

    fireEvent.change(screen.getByLabelText('Role template'), {
      target: { value: 'role-1' },
    })
    fireEvent.change(screen.getByLabelText('Expiration (optional)'), {
      target: { value: '2026-06-03T12:30' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Create role assignment' }))

    expect(onCreateRoleAssignment).toHaveBeenCalledWith({
      roleTemplateId: 'role-1',
      scopeType: 'tenant',
      scopeValue: null,
      expiresAt: new Date('2026-06-03T12:30').toISOString(),
    })
  })

  it('shows approval actions for pending review assignments', () => {
    const onUpdateRoleAssignmentStatus = vi.fn(async () => {})
    render(
      <RoleTemplateAssignmentPanel
        personId="person-1"
        personDisplayName="Alex"
        orgUnits={orgUnits}
        permissionTemplates={[]}
        roleTemplates={[]}
        roleAssignments={[
          {
            assignmentId: 'assign-review',
            personId: 'person-1',
            roleTemplateId: 'role-review',
            roleKey: 'staffarr.high.risk',
            roleName: 'High Risk Role',
            scopeType: 'tenant',
            scopeValue: null,
            status: 'pending_review',
            effectiveStatus: 'pending_review',
            expiresAt: null,
            createdAt: '2026-06-03T00:00:00Z',
            updatedAt: '2026-06-03T00:00:00Z',
          },
        ]}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onUpsertPermissionTemplate={vi.fn(async () => {})}
        onCreateRoleTemplate={vi.fn(async () => {})}
        onUpdateRoleTemplateStatus={vi.fn(async () => {})}
        onCreateRoleAssignment={vi.fn(async () => {})}
        onUpdateRoleAssignmentStatus={onUpdateRoleAssignmentStatus}
      />,
    )

    expect(screen.getByText(/pending review/)).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Approve' }))
    expect(onUpdateRoleAssignmentStatus).toHaveBeenCalledWith('assign-review', 'active')
    fireEvent.click(screen.getByRole('button', { name: 'Reject' }))
    expect(onUpdateRoleAssignmentStatus).toHaveBeenCalledWith('assign-review', 'inactive')
  })

  it('formats mutation error classification', () => {
    expect(formatRoleTemplateMutationError('{"status":403}')).toContain('Forbidden:')
    expect(formatRoleTemplateMutationError('{"status":409}')).toContain('Conflict:')
    expect(formatRoleTemplateMutationError('{"status":400}')).toContain('Validation:')
  })

  it('renders role template error in callout', () => {
    render(
      <RoleTemplateAssignmentPanel
        personId="person-1"
        personDisplayName="Alex"
        orgUnits={orgUnits}
        permissionTemplates={[]}
        roleTemplates={[]}
        roleAssignments={[]}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage="template failed"
        onUpsertPermissionTemplate={vi.fn(async () => {})}
        onCreateRoleTemplate={vi.fn(async () => {})}
        onUpdateRoleTemplateStatus={vi.fn(async () => {})}
        onCreateRoleAssignment={vi.fn(async () => {})}
        onUpdateRoleAssignmentStatus={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('template failed')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })

  it('renders retryable read error callout', () => {
    const onRetryRead = vi.fn()
    render(
      <RoleTemplateAssignmentPanel
        personId="person-1"
        personDisplayName="Alex"
        orgUnits={orgUnits}
        permissionTemplates={[]}
        roleTemplates={[]}
        roleAssignments={[]}
        isLoading={false}
        isError
        readErrorMessage="permissions data read failed"
        onRetryRead={onRetryRead}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onUpsertPermissionTemplate={vi.fn(async () => {})}
        onCreateRoleTemplate={vi.fn(async () => {})}
        onUpdateRoleTemplateStatus={vi.fn(async () => {})}
        onCreateRoleAssignment={vi.fn(async () => {})}
        onUpdateRoleAssignmentStatus={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('Role and permission data unavailable')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry permissions data' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })
})
