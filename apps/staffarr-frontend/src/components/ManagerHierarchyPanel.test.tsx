import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { formatManagerMutationError, ManagerHierarchyPanel } from './ManagerHierarchyPanel'

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

const people = [
  {
    personId: 'person-1',
    externalUserId: null,
    displayName: 'Alice Admin',
    primaryEmail: 'alice@example.com',
    employmentStatus: 'active',
    primaryOrgUnitId: null,
    primaryOrgUnitName: null,
    managerPersonId: null,
    jobTitle: 'Director',
  },
  {
    personId: 'person-2',
    externalUserId: null,
    displayName: 'Bob Lead',
    primaryEmail: 'bob@example.com',
    employmentStatus: 'active',
    primaryOrgUnitId: null,
    primaryOrgUnitName: null,
    managerPersonId: 'person-1',
    jobTitle: 'Lead',
  },
]

describe('ManagerHierarchyPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders read-only fallback for non-writers', () => {
    render(
      <ManagerHierarchyPanel
        selectedPersonId="person-2"
        selectedPersonDisplayName="Bob Lead"
        people={people}
        managerChain={[]}
        subordinates={[]}
        selectedSubordinateId={null}
        selectedSubordinate={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingSubordinateDetail={false}
        isSubordinateDetailError={false}
        subordinateDetailErrorMessage={null}
        onRetrySubordinateDetail={vi.fn()}
        canManage={false}
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectSubordinate={vi.fn()}
        onUpdateManager={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('Read only')).toBeTruthy()
    expect(screen.getByText('Your role does not include manager hierarchy write permission.')).toBeTruthy()
    expect(screen.queryByRole('button', { name: 'Update manager' })).toBeNull()
  })

  it('submits manager update with selected manager', async () => {
    const onUpdateManager = vi.fn(async () => {})
    render(
      <ManagerHierarchyPanel
        selectedPersonId="person-2"
        selectedPersonDisplayName="Bob Lead"
        people={people}
        managerChain={[]}
        subordinates={[]}
        selectedSubordinateId={null}
        selectedSubordinate={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingSubordinateDetail={false}
        isSubordinateDetailError={false}
        subordinateDetailErrorMessage={null}
        onRetrySubordinateDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectSubordinate={vi.fn()}
        onUpdateManager={onUpdateManager}
      />,
    )

    fireEvent.change(screen.getByTestId('manager-hierarchy-manager'), {
      target: { value: 'person-1' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Update manager' }))

    expect(onUpdateManager).toHaveBeenCalledWith('person-1')
  })

  it('classifies mutation errors for UI copy', () => {
    expect(formatManagerMutationError('{"status":403}')).toContain('Forbidden:')
    expect(formatManagerMutationError('{"status":409,"code":"people.manager_cycle"}')).toContain('Conflict:')
    expect(formatManagerMutationError('{"status":400}')).toContain('Validation:')
  })

  it('renders manager error in callout', () => {
    render(
      <ManagerHierarchyPanel
        selectedPersonId="person-2"
        selectedPersonDisplayName="Bob Lead"
        people={people}
        managerChain={[]}
        subordinates={[]}
        selectedSubordinateId={null}
        selectedSubordinate={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingSubordinateDetail={false}
        isSubordinateDetailError={false}
        subordinateDetailErrorMessage={null}
        onRetrySubordinateDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage="update failed"
        onSelectSubordinate={vi.fn()}
        onUpdateManager={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('update failed')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })

  it('renders retryable hierarchy read error callout', () => {
    const onRetryRead = vi.fn()
    render(
      <ManagerHierarchyPanel
        selectedPersonId="person-2"
        selectedPersonDisplayName="Bob Lead"
        people={people}
        managerChain={[]}
        subordinates={[]}
        selectedSubordinateId={null}
        selectedSubordinate={null}
        isLoading={false}
        isError
        readErrorMessage="hierarchy read failed"
        onRetryRead={onRetryRead}
        isLoadingSubordinateDetail={false}
        isSubordinateDetailError={false}
        subordinateDetailErrorMessage={null}
        onRetrySubordinateDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectSubordinate={vi.fn()}
        onUpdateManager={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('Manager hierarchy unavailable')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry hierarchy' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })

  it('renders retryable subordinate detail error callout', () => {
    const onRetrySubordinateDetail = vi.fn()
    render(
      <ManagerHierarchyPanel
        selectedPersonId="person-2"
        selectedPersonDisplayName="Bob Lead"
        people={people}
        managerChain={[]}
        subordinates={[]}
        selectedSubordinateId="person-3"
        selectedSubordinate={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingSubordinateDetail={false}
        isSubordinateDetailError
        subordinateDetailErrorMessage="subordinate detail read failed"
        onRetrySubordinateDetail={onRetrySubordinateDetail}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectSubordinate={vi.fn()}
        onUpdateManager={vi.fn(async () => {})}
      />,
    )

    expect(screen.getByText('Subordinate detail unavailable')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry subordinate detail' }))
    expect(onRetrySubordinateDetail).toHaveBeenCalledTimes(1)
  })
})
