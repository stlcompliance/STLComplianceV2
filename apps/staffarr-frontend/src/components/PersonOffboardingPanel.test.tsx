import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PersonOffboardingPanel } from './PersonOffboardingPanel'
import type { PersonOffboardingResponse } from '../api/types'

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

const peopleOptions = [
  { personId: 'person-1', displayName: 'Alex Worker' },
  { personId: 'person-2', displayName: 'Taylor Manager' },
]

const inProgressOffboarding: PersonOffboardingResponse = {
  offboardingId: 'off-1',
  personId: 'person-1',
  status: 'in_progress',
  separationDate: '2026-06-10T00:00:00.000Z',
  separationReason: 'Voluntary',
  targetEmploymentStatus: 'inactive',
  disableLoginRequested: true,
  newManagerPersonIdForReports: null,
  startedAt: '2026-06-01T10:00:00.000Z',
  startedByUserId: 'user-1',
  completedAt: null,
  completedByUserId: null,
  steps: [
    {
      stepKey: 'disable_access',
      title: 'Disable system access',
      detail: 'Revoke active role assignments and disable login.',
      status: 'pending',
      blockerDetail: null,
      sortOrder: 1,
      completedAt: null,
    },
  ],
  activeDirectReportCount: 2,
  openIncidentCount: 0,
  activeRoleAssignmentCount: 1,
  activeOrgAssignmentCount: 1,
}

describe('PersonOffboardingPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('submits start request for a new offboarding workflow', async () => {
    const onStart = vi.fn().mockResolvedValue(undefined)

    render(
      <PersonOffboardingPanel
        personId="person-1"
        personDisplayName="Alex Worker"
        peopleOptions={peopleOptions}
        offboarding={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onStart={onStart}
        onExecute={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    fireEvent.change(screen.getByLabelText(/Separation date/i), {
      target: { value: '2026-06-10' },
    })
    fireEvent.change(screen.getByLabelText(/Separation reason/i), {
      target: { value: 'Voluntary separation' },
    })
    expect(screen.getAllByText('Taylor Manager').length).toBeGreaterThan(0)
    fireEvent.click(screen.getByRole('button', { name: /Start offboarding/i }))

    await waitFor(() => {
      expect(onStart).toHaveBeenCalledWith(
        expect.objectContaining({
          separationReason: 'Voluntary separation',
          targetEmploymentStatus: 'inactive',
          disableLoginRequested: true,
        }),
      )
    })
  })

  it('submits execute request when workflow is in progress', async () => {
    const onExecute = vi.fn().mockResolvedValue(undefined)

    render(
      <PersonOffboardingPanel
        personId="person-1"
        personDisplayName="Alex Worker"
        peopleOptions={peopleOptions}
        offboarding={inProgressOffboarding}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onStart={vi.fn().mockResolvedValue(undefined)}
        onExecute={onExecute}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: /Execute offboarding/i }))

    await waitFor(() => {
      expect(onExecute).toHaveBeenCalledWith({ newManagerPersonIdForReports: null })
    })
  })

  it('uses searchable pickers for replacement manager selection', () => {
    render(
      <PersonOffboardingPanel
        personId="person-1"
        personDisplayName="Alex Worker"
        peopleOptions={peopleOptions}
        offboarding={inProgressOffboarding}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onStart={vi.fn().mockResolvedValue(undefined)}
        onExecute={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getAllByText('Taylor Manager').length).toBeGreaterThan(0)
    expect(screen.getByTestId('execute-offboarding-manager-picker')).toBeTruthy()
    fireEvent.change(screen.getByTestId('execute-offboarding-manager-picker'), {
      target: { value: 'person-2' },
    })
    expect((screen.getByTestId('execute-offboarding-manager-picker') as HTMLInputElement).value).toBe('person-2')
  })

  it('renders offboarding action errors in shared callout', () => {
    render(
      <PersonOffboardingPanel
        personId="person-1"
        personDisplayName="Alex Worker"
        peopleOptions={peopleOptions}
        offboarding={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage="Offboarding service unavailable"
        onStart={vi.fn().mockResolvedValue(undefined)}
        onExecute={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Offboarding action failed')).toBeTruthy()
    expect(screen.getByText('Offboarding service unavailable')).toBeTruthy()
  })

  it('renders retryable query error and suppresses start form', () => {
    const onRetryRead = vi.fn()
    render(
      <PersonOffboardingPanel
        personId="person-1"
        personDisplayName="Alex Worker"
        peopleOptions={peopleOptions}
        offboarding={null}
        isLoading={false}
        isError
        readErrorMessage="offboarding read failed"
        onRetryRead={onRetryRead}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onStart={vi.fn().mockResolvedValue(undefined)}
        onExecute={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Offboarding workflow unavailable')).toBeTruthy()
    expect(screen.getByText('offboarding read failed')).toBeTruthy()
    expect(screen.queryByRole('button', { name: /Start offboarding/i })).toBeNull()
    fireEvent.click(screen.getByRole('button', { name: 'Retry offboarding' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })
})
