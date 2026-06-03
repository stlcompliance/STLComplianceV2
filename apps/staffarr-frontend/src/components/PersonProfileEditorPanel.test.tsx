import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { canManagePeople, PersonProfileEditorPanel } from './PersonProfileEditorPanel'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const mod = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...mod,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      testId,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      options: { value: string; label: string }[]
      testId?: string
    }) => (
      <label htmlFor={testId ?? 'mock-static-search-picker'}>
        {label}
        <input
          id={testId ?? 'mock-static-search-picker'}
          aria-label={label}
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

const profile = {
  personId: '11111111-1111-1111-1111-111111111101',
  externalUserId: null,
  givenName: 'Demo',
  familyName: 'Worker',
  displayName: 'Demo Worker',
  primaryEmail: 'worker@demo.stl',
  employmentStatus: 'active',
  primaryOrgUnitId: null,
  primaryOrgUnitName: null,
  managerPersonId: null,
  jobTitle: 'Technician',
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
}

describe('PersonProfileEditorPanel', () => {
  it('exposes people write role helper', () => {
    expect(canManagePeople('tenant_admin', false)).toBe(true)
    expect(canManagePeople('supervisor', false)).toBe(false)
    expect(canManagePeople('tenant_member', true)).toBe(true)
  })

  it('shows read-only notice for non-writers', () => {
    render(
      <PersonProfileEditorPanel
        profile={profile}
        orgUnits={[]}
        peopleOptions={[]}
        canManage={false}
        isSubmitting={false}
        errorMessage={null}
        onUpdate={async () => {}}
        onEmploymentStatusChange={async () => {}}
      />,
    )

    expect(screen.getByText(/Profile edits require tenant admin/i)).toBeTruthy()
    expect(screen.queryByRole('button', { name: /Save profile changes/i })).toBeNull()
  })

  it('renders edit controls for writers', () => {
    render(
      <PersonProfileEditorPanel
        profile={profile}
        orgUnits={[]}
        peopleOptions={[]}
        canManage={true}
        isSubmitting={false}
        errorMessage={null}
        onUpdate={async () => {}}
        onEmploymentStatusChange={async () => {}}
      />,
    )

    expect(screen.getByRole('button', { name: /Save profile changes/i })).toBeTruthy()
    expect(screen.getByRole('button', { name: /Mark inactive/i })).toBeTruthy()
    expect(screen.getByTestId('edit-person-primary-org-unit-picker')).toBeTruthy()
    expect(screen.getByTestId('edit-person-manager-picker')).toBeTruthy()
  })

  it('renders profile error in callout', () => {
    render(
      <PersonProfileEditorPanel
        profile={profile}
        orgUnits={[]}
        peopleOptions={[]}
        canManage={true}
        isSubmitting={false}
        errorMessage="profile save failed"
        onUpdate={async () => {}}
        onEmploymentStatusChange={async () => {}}
      />,
    )

    expect(screen.getByText('profile save failed')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })
})
