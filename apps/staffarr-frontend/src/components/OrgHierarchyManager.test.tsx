import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { OrgUnitResponse } from '../api/types'
import { canManageOrgHierarchy, OrgHierarchyManager } from './OrgHierarchyManager'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const mod = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...mod,
    StaticSearchPicker: ({
      value,
      onChange,
      options,
      testId,
      placeholder,
    }: {
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      testId?: string
      placeholder?: string
    }) => (
      <label>
        {placeholder}
        <input
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <ul>
          {options.map((option) => (
            <li key={option.value}>{option.label}</li>
          ))}
        </ul>
      </label>
    ),
  }
})

const sampleOrgUnits: OrgUnitResponse[] = [
  {
    orgUnitId: '00000000-0000-0000-0000-000000000000',
    unitType: 'site',
    name: 'North Site',
    parentOrgUnitId: null,
    status: 'active',
  },
  {
    orgUnitId: '11111111-1111-1111-1111-111111111111',
    unitType: 'department',
    name: 'Operations',
    parentOrgUnitId: '00000000-0000-0000-0000-000000000000',
    status: 'active',
  },
  {
    orgUnitId: '22222222-2222-2222-2222-222222222222',
    unitType: 'team',
    name: 'Field Team',
    parentOrgUnitId: '11111111-1111-1111-1111-111111111111',
    status: 'active',
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
        actionErrorMessage={null}
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
        actionErrorMessage={null}
        onCreate={vi.fn(async () => {})}
        onUpdate={vi.fn(async () => {})}
        onStatusChange={onStatusChange}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Operations' }))
    fireEvent.click(screen.getByRole('button', { name: 'Deactivate' }))

    expect(onStatusChange).toHaveBeenCalledWith('11111111-1111-1111-1111-111111111111', 'inactive')
  })

  it('uses searchable parent pickers for create and edit flows', async () => {
    const onCreate = vi.fn(async () => {})
    const onUpdate = vi.fn(async () => {})

    render(
      <OrgHierarchyManager
        orgUnits={sampleOrgUnits}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onCreate={onCreate}
        onUpdate={onUpdate}
        onStatusChange={vi.fn(async () => {})}
      />,
    )

    fireEvent.change(screen.getAllByLabelText(/Org unit name/i)[0], {
      target: { value: 'Safety' },
    })
    fireEvent.change(screen.getByTestId('create-org-unit-parent'), {
      target: { value: '00000000-0000-0000-0000-000000000000' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Create' }))

    expect(onCreate).toHaveBeenCalledWith({
      unitType: 'department',
      name: 'Safety',
      code: null,
      parentOrgUnitId: '00000000-0000-0000-0000-000000000000',
      description: null,
      managerPersonId: null,
      effectiveStartDate: null,
      effectiveEndDate: null,
      siteType: null,
      timezone: null,
      phone: null,
      emergencyContact: null,
      teamType: null,
      positionCode: null,
      defaultSiteOrgUnitId: null,
      complianceSensitive: false,
      safetySensitive: false,
      canSupervise: false,
      canApprove: false,
      status: 'planned',
    })

    fireEvent.click(screen.getByRole('button', { name: 'Field Team' }))
    fireEvent.change(screen.getAllByLabelText(/Org unit name/i)[1], {
      target: { value: 'Field Ops' },
    })
    fireEvent.change(screen.getByTestId('edit-org-unit-parent'), {
      target: { value: '11111111-1111-1111-1111-111111111111' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Save changes' }))

    expect(onUpdate).toHaveBeenCalledWith('22222222-2222-2222-2222-222222222222', {
      unitType: 'team',
      name: 'Field Ops',
      code: null,
      parentOrgUnitId: '11111111-1111-1111-1111-111111111111',
      description: null,
      managerPersonId: null,
      effectiveStartDate: null,
      effectiveEndDate: null,
      siteType: null,
      timezone: null,
      phone: null,
      emergencyContact: null,
      teamType: null,
      positionCode: null,
      defaultSiteOrgUnitId: null,
      complianceSensitive: false,
      safetySensitive: false,
      canSupervise: false,
      canApprove: false,
      status: 'active',
    })
  })

  it('renders backend errors in shared callout', () => {
    render(
      <OrgHierarchyManager
        orgUnits={sampleOrgUnits}
        canManage
        isSubmitting={false}
        actionErrorMessage="Could not save org unit"
        onCreate={vi.fn(async () => {})}
        onUpdate={vi.fn(async () => {})}
        onStatusChange={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Org hierarchy update failed')).toBeTruthy()
    expect(screen.getByText('Could not save org unit')).toBeTruthy()
  })

  it('renders read error with retry action', () => {
    const onRetryRead = vi.fn()
    render(
      <OrgHierarchyManager
        orgUnits={sampleOrgUnits}
        isError
        readErrorMessage="Org hierarchy query failed"
        onRetryRead={onRetryRead}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onCreate={vi.fn(async () => {})}
        onUpdate={vi.fn(async () => {})}
        onStatusChange={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('Org hierarchy unavailable')).toBeTruthy()
    expect(screen.getByText('Org hierarchy query failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry org hierarchy' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })
})
