import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
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
          { value: 'pending_start', label: 'Pending start', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
          { value: 'active', label: 'Active', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
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
    QuestionnaireFlow: () => null,
    StaticSearchPicker: ({
      value,
      onChange,
      options,
      testId,
      placeholder,
      disabled,
    }: {
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      testId?: string
      placeholder?: string
      disabled?: boolean
    }) => (
      <label>
        {placeholder}
        <input
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

import { CreatePersonPanel } from './CreatePersonPanel'

function renderPanel(node: ReactNode) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })

  return render(<QueryClientProvider client={queryClient}>{node}</QueryClientProvider>)
}

describe('CreatePersonPanel', () => {
  it('renders nothing when user cannot manage people', () => {
    const { container } = renderPanel(
      <CreatePersonPanel
        accessToken="token"
        tenantId="tenant-1"
        complianceCoreApiBase="http://compliancecore.test"
        orgUnits={[]}
        peopleOptions={[]}
        canManage={false}
        isSubmitting={false}
        errorMessage={null}
        onCreate={vi.fn()}
      />,
    )

    expect(container.innerHTML).toBe('')
  })

  it('submits the guided create person request', async () => {
    const onCreate = vi.fn().mockResolvedValue(undefined)

    renderPanel(
      <CreatePersonPanel
        accessToken="token"
        tenantId="tenant-1"
        complianceCoreApiBase="http://compliancecore.test"
        orgUnits={[
          { orgUnitId: 'site-1', unitType: 'site', name: 'North Site', parentOrgUnitId: null, status: 'active' },
          { orgUnitId: 'dept-1', unitType: 'department', name: 'Operations', parentOrgUnitId: 'site-1', status: 'active' },
          { orgUnitId: 'team-1', unitType: 'team', name: 'Day Shift', parentOrgUnitId: 'dept-1', status: 'active' },
          { orgUnitId: 'position-1', unitType: 'position', name: 'Operator I', parentOrgUnitId: 'team-1', status: 'active' },
        ]}
        peopleOptions={[
          { personId: 'person-1', displayName: 'Alex Worker' },
          { personId: 'person-2', displayName: 'Taylor Manager' },
        ]}
        canManage
        isSubmitting={false}
        errorMessage={null}
        onCreate={onCreate}
      />,
    )

    fireEvent.change(screen.getByLabelText(/Legal first name/i), { target: { value: 'Ada' } })
    fireEvent.change(screen.getByLabelText(/Legal last name/i), { target: { value: 'Lovelace' } })
    fireEvent.change(screen.getByLabelText(/Preferred name/i), { target: { value: 'Addie' } })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))

    fireEvent.change(screen.getByLabelText(/Primary email/i), { target: { value: 'ada@example.com' } })
    fireEvent.change(screen.getByLabelText(/^Job title$/i), { target: { value: 'Operator' } })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))

    fireEvent.change(screen.getByTestId('create-person-site-org-unit'), {
      target: { value: 'site-1' },
    })
    fireEvent.change(screen.getByTestId('create-person-department-org-unit'), {
      target: { value: 'dept-1' },
    })
    fireEvent.change(screen.getByTestId('create-person-team-org-unit'), {
      target: { value: 'team-1' },
    })
    fireEvent.change(screen.getByTestId('create-person-position-org-unit'), {
      target: { value: 'position-1' },
    })
    fireEvent.change(screen.getByTestId('create-person-manager'), {
      target: { value: 'person-2' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))

    fireEvent.click(screen.getByLabelText(/Person can log in through NexArr/i))
    fireEvent.change(screen.getByLabelText(/Temporary password/i), {
      target: { value: 'TempPass!123' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
    fireEvent.click(screen.getByRole('button', { name: /Create person/i }))

    await waitFor(() => {
      expect(onCreate).toHaveBeenCalledWith({
        legalFirstName: 'Ada',
        legalMiddleName: null,
        legalLastName: 'Lovelace',
        preferredName: 'Addie',
        pronouns: null,
        givenName: 'Ada',
        familyName: 'Lovelace',
        primaryEmail: 'ada@example.com',
        alternateEmail: null,
        primaryPhone: null,
        alternatePhone: null,
        workPhone: null,
        employmentStatus: 'pending_start',
        workRelationshipType: 'employee',
        employmentType: 'full_time',
        startDate: null,
        expectedStartDate: null,
        primaryOrgUnitId: 'dept-1',
        siteOrgUnitId: 'site-1',
        departmentOrgUnitId: 'dept-1',
        teamOrgUnitId: 'team-1',
        positionOrgUnitId: 'position-1',
        managerPersonId: 'person-2',
        jobTitle: 'Operator',
        homeBaseLocationId: null,
        canLogin: true,
        temporaryPassword: 'TempPass!123',
      })
    })
  })

  it('moves to review before creating when the form is submitted early', async () => {
    const onCreate = vi.fn().mockResolvedValue(undefined)

    const { container, getByLabelText, getByRole, getByText } = renderPanel(
      <CreatePersonPanel
        accessToken="token"
        tenantId="tenant-1"
        complianceCoreApiBase="http://compliancecore.test"
        orgUnits={[
          { orgUnitId: 'site-1', unitType: 'site', name: 'North Site', parentOrgUnitId: null, status: 'active' },
          { orgUnitId: 'dept-1', unitType: 'department', name: 'Operations', parentOrgUnitId: 'site-1', status: 'active' },
          { orgUnitId: 'team-1', unitType: 'team', name: 'Day Shift', parentOrgUnitId: 'dept-1', status: 'active' },
          { orgUnitId: 'position-1', unitType: 'position', name: 'Operator I', parentOrgUnitId: 'team-1', status: 'active' },
        ]}
        peopleOptions={[
          { personId: 'person-1', displayName: 'Alex Worker' },
          { personId: 'person-2', displayName: 'Taylor Manager' },
        ]}
        canManage
        isSubmitting={false}
        errorMessage={null}
        onCreate={onCreate}
      />,
    )

    fireEvent.change(getByLabelText(/Legal first name/i), { target: { value: 'Ada' } })
    fireEvent.change(getByLabelText(/Legal last name/i), { target: { value: 'Lovelace' } })
    fireEvent.click(getByRole('button', { name: 'Next' }))

    fireEvent.change(getByLabelText(/Primary email/i), { target: { value: 'ada@example.com' } })
    fireEvent.click(getByRole('button', { name: 'Next' }))

    fireEvent.change(container.querySelector('[data-testid="create-person-site-org-unit"]')!, {
      target: { value: 'site-1' },
    })
    fireEvent.change(container.querySelector('[data-testid="create-person-department-org-unit"]')!, {
      target: { value: 'dept-1' },
    })
    fireEvent.change(container.querySelector('[data-testid="create-person-team-org-unit"]')!, {
      target: { value: 'team-1' },
    })
    fireEvent.change(container.querySelector('[data-testid="create-person-position-org-unit"]')!, {
      target: { value: 'position-1' },
    })

    fireEvent.submit(container.querySelector('form[data-testid="create-person-form"]')!)

    await waitFor(() => {
      expect(onCreate).not.toHaveBeenCalled()
      expect(getByText('Login intent')).toBeTruthy()
      expect(getByRole('button', { name: /Create person/i })).toBeTruthy()
    })
  })

  it('renders create error in callout', () => {
    renderPanel(
      <CreatePersonPanel
        accessToken="token"
        tenantId="tenant-1"
        complianceCoreApiBase="http://compliancecore.test"
        orgUnits={[]}
        peopleOptions={[]}
        canManage
        isSubmitting={false}
        errorMessage="create failed"
        onCreate={vi.fn()}
      />,
    )

    expect(screen.getByText('create failed')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })
})
