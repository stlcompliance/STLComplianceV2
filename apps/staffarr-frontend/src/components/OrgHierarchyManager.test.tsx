import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { canManageOrgHierarchy, OrgHierarchyManager } from './OrgHierarchyManager'

const sampleOrgUnits = [
  {
    orgUnitId: '11111111-1111-1111-1111-111111111111',
    unitType: 'department',
    name: 'Operations',
    parentOrgUnitId: null,
    status: 'active' as const,
  },
  {
    orgUnitId: '22222222-2222-2222-2222-222222222222',
    unitType: 'team',
    name: 'Field Team',
    parentOrgUnitId: '11111111-1111-1111-1111-111111111111',
    status: 'active' as const,
  },
]

describe('OrgHierarchyManager', () => {
  afterEach(() => {
    cleanup()
  })

  it('grants write access to expected roles', () => {
    expect(canManageOrgHierarchy('tenant_admin', false)).toBe(true)
    expect(canManageOrgHierarchy('staffarr_admin', false)).toBe(true)
    expect(canManageOrgHierarchy('hr_admin', false)).toBe(true)
    expect(canManageOrgHierarchy('supervisor', false)).toBe(false)
    expect(canManageOrgHierarchy('tenant_member', true)).toBe(true)
  })

  it('renders read-only notice for non-writers', () => {
    render(
      <OrgHierarchyManager
        orgUnits={sampleOrgUnits}
        canManage={false}
        isSubmitting={false}
        errorMessage={null}
        onCreate={vi.fn(async () => {})}
        onUpdate={vi.fn(async () => {})}
        onStatusChange={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('Read only')).toBeTruthy()
    expect(screen.getByText('Your role does not include org hierarchy write permission.')).toBeTruthy()
    expect(screen.queryByText('Create org unit')).toBeNull()
  })

  it('calls status update when toggling selected unit', async () => {
    const onStatusChange = vi.fn(async () => {})
    render(
      <OrgHierarchyManager
        orgUnits={sampleOrgUnits}
        canManage
        isSubmitting={false}
        errorMessage={null}
        onCreate={vi.fn(async () => {})}
        onUpdate={vi.fn(async () => {})}
        onStatusChange={onStatusChange}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Operations' }))
    fireEvent.click(screen.getByRole('button', { name: 'Deactivate' }))

    expect(onStatusChange).toHaveBeenCalledWith('11111111-1111-1111-1111-111111111111', 'inactive')
  })
})
