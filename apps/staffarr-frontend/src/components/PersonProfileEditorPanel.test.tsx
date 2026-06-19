import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'

const mocked = vi.hoisted(() => ({
  getStaffArrFieldset: vi.fn(async () => ({
    key: 'people.profile',
    label: 'People profile',
    entityType: 'staff_person',
    purpose: 'profile',
    fields: [
      {
        key: 'employmentStatus',
        label: 'Employment status',
        control: 'select',
        required: true,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'active', label: 'Active', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
          { value: 'inactive', label: 'Inactive', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
        ],
      },
      {
        key: 'workRelationshipType',
        label: 'Work relationship',
        control: 'select',
        required: true,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'employee', label: 'Employee', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
        ],
      },
      {
        key: 'employmentType',
        label: 'Employment type',
        control: 'select',
        required: false,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'full_time', label: 'Full time', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
        ],
      },
    ],
  })),
  listLocations: vi.fn(async () => []),
}))

vi.mock('../api/client', () => ({
  getStaffArrFieldset: mocked.getStaffArrFieldset,
  listLocations: mocked.listLocations,
  listSiteLocations: mocked.listLocations,
}))

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
      disabled,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      options: { value: string; label: string }[]
      testId?: string
      disabled?: boolean
    }) => (
      <label htmlFor={testId ?? 'mock-static-search-picker'}>
        {label}
        <input
          id={testId ?? 'mock-static-search-picker'}
          aria-label={label}
          data-testid={testId}
          disabled={disabled}
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

import { canManagePeople, PersonProfileEditorPanel } from './PersonProfileEditorPanel'

const profile = {
  personId: '11111111-1111-1111-1111-111111111101',
  externalUserId: null,
  givenName: 'Demo',
  familyName: 'Worker',
  legalFirstName: 'Demo',
  legalMiddleName: null,
  legalLastName: 'Worker',
  preferredName: 'Demo',
  pronouns: null,
  displayName: 'Demo Worker',
  primaryEmail: 'worker@demo.stl',
  alternateEmail: null,
  primaryPhone: null,
  alternatePhone: null,
  workPhone: null,
  employmentStatus: 'active',
  workRelationshipType: 'employee',
  employmentType: 'full_time',
  workerCategory: 'employee',
  flsaStatus: 'unknown',
  positionNumber: 'POS-1001',
  currentEmploymentAction: 'hire',
  currentEmploymentActionAt: null,
  leaveStatus: 'active',
  eligibleForRehire: true,
  primaryOrgUnitId: null,
  primaryOrgUnitName: null,
  managerPersonId: null,
  jobTitle: 'Technician',
  startDate: null,
  expectedStartDate: null,
  homeBaseLocationId: null,
  homeBaseLocationName: null,
  canLoginSnapshot: false,
  hasUserAccountSnapshot: false,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
}

function renderPanel(node: ReactNode) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })

  return render(<QueryClientProvider client={queryClient}>{node}</QueryClientProvider>)
}

describe('PersonProfileEditorPanel', () => {
  it('exposes people write role helper', () => {
    expect(canManagePeople('tenant_admin', false)).toBe(true)
    expect(canManagePeople('supervisor', false)).toBe(false)
    expect(canManagePeople('tenant_member', true)).toBe(true)
  })

  it('shows read-only notice for non-writers', () => {
    renderPanel(
      <PersonProfileEditorPanel
        accessToken="token"
        profile={profile}
        orgUnits={[]}
        peopleOptions={[]}
        siteContextOrgUnitId={null}
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
    renderPanel(
      <PersonProfileEditorPanel
        accessToken="token"
        profile={profile}
        orgUnits={[]}
        peopleOptions={[]}
        siteContextOrgUnitId={null}
        canManage
        isSubmitting={false}
        errorMessage={null}
        onUpdate={async () => {}}
        onEmploymentStatusChange={async () => {}}
      />,
    )

    expect(screen.getByRole('button', { name: /Save profile changes/i })).toBeTruthy()
    expect(screen.getByRole('button', { name: /Apply status/i })).toBeTruthy()
    expect(screen.getByTestId('edit-person-primary-org-unit-picker')).toBeTruthy()
    expect(screen.getByTestId('edit-person-manager-picker')).toBeTruthy()
  })

  it('renders profile error in callout', () => {
    renderPanel(
      <PersonProfileEditorPanel
        accessToken="token"
        profile={profile}
        orgUnits={[]}
        peopleOptions={[]}
        siteContextOrgUnitId={null}
        canManage
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
