import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { formatRoleTemplateMutationError, RoleTemplateAssignmentPanel } from './RoleTemplateAssignmentPanel'

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
        canManage={false}
        isSubmitting={false}
        errorMessage={null}
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
        canManage
        isSubmitting={false}
        errorMessage={null}
        onUpsertPermissionTemplate={onUpsert}
        onCreateRoleTemplate={vi.fn(async () => {})}
        onUpdateRoleTemplateStatus={vi.fn(async () => {})}
        onCreateRoleAssignment={vi.fn(async () => {})}
        onUpdateRoleAssignmentStatus={vi.fn(async () => {})}
      />,
    )

    fireEvent.change(screen.getByPlaceholderText('staffarr.permissions.assign'), {
      target: { value: 'staffarr.permissions.assign' },
    })
    fireEvent.change(screen.getByPlaceholderText('Permission name'), {
      target: { value: 'Assign permissions' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Save permission template' }))

    expect(onUpsert).toHaveBeenCalledWith({
      permissionKey: 'staffarr.permissions.assign',
      name: 'Assign permissions',
      description: null,
    })
  })

  it('formats mutation error classification', () => {
    expect(formatRoleTemplateMutationError('{"status":403}')).toContain('Forbidden:')
    expect(formatRoleTemplateMutationError('{"status":409}')).toContain('Conflict:')
    expect(formatRoleTemplateMutationError('{"status":400}')).toContain('Validation:')
  })
})
