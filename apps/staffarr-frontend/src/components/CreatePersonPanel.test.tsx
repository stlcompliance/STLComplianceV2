import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

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

import { CreatePersonPanel } from './CreatePersonPanel'

describe('CreatePersonPanel', () => {
  it('renders nothing when user cannot manage people', () => {
    const { container } = render(
      <CreatePersonPanel
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

  it('submits create person request using searchable selectors', async () => {
    const onCreate = vi.fn().mockResolvedValue(undefined)

    render(
      <CreatePersonPanel
        orgUnits={[
          { orgUnitId: 'site-1', unitType: 'site', name: 'North Site', parentOrgUnitId: null, status: 'active' },
          { orgUnitId: 'dept-1', unitType: 'department', name: 'Operations', parentOrgUnitId: 'site-1', status: 'active' },
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

    fireEvent.change(screen.getByLabelText(/Given name/i), { target: { value: 'Ada' } })
    fireEvent.change(screen.getByLabelText(/Family name/i), { target: { value: 'Lovelace' } })
    fireEvent.change(screen.getByLabelText(/Primary email/i), { target: { value: 'ada@example.com' } })
    fireEvent.change(screen.getByTestId('create-person-primary-org-unit'), {
      target: { value: 'site-1' },
    })
    fireEvent.change(screen.getByTestId('create-person-manager'), {
      target: { value: 'person-2' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Create person/i }))

    await waitFor(() => {
      expect(onCreate).toHaveBeenCalledWith({
        givenName: 'Ada',
        familyName: 'Lovelace',
        primaryEmail: 'ada@example.com',
        employmentStatus: 'active',
        primaryOrgUnitId: 'site-1',
        managerPersonId: 'person-2',
        jobTitle: null,
      })
    })
  })

  it('renders create error in callout', () => {
    render(
      <CreatePersonPanel
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
