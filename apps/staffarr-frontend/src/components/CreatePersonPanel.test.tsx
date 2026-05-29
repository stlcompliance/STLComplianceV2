import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
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

  it('submits create person request', () => {
    const onCreate = vi.fn().mockResolvedValue(undefined)

    render(
      <CreatePersonPanel
        orgUnits={[]}
        peopleOptions={[]}
        canManage
        isSubmitting={false}
        errorMessage={null}
        onCreate={onCreate}
      />,
    )

    fireEvent.change(screen.getByLabelText(/Given name/i), { target: { value: 'Ada' } })
    fireEvent.change(screen.getByLabelText(/Family name/i), { target: { value: 'Lovelace' } })
    fireEvent.change(screen.getByLabelText(/Primary email/i), { target: { value: 'ada@example.com' } })
    fireEvent.click(screen.getByRole('button', { name: /Create person/i }))

    expect(onCreate).toHaveBeenCalledWith({
      givenName: 'Ada',
      familyName: 'Lovelace',
      primaryEmail: 'ada@example.com',
      employmentStatus: 'active',
      primaryOrgUnitId: null,
      managerPersonId: null,
      jobTitle: null,
    })
  })
})
