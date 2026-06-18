import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { UserPreferencesPage } from './UserPreferencesPage'

const updateMyPreferencesMock = vi.fn().mockResolvedValue({ themePreference: 'light' })

vi.mock('../auth/AuthProvider', () => ({
  useAuth: () => ({
    me: {
      userId: 'person-1',
      tenantId: 'tenant-1',
      displayName: 'Demo Admin',
      tenantDisplayName: 'STL Demo Tenant',
      themePreference: 'system',
    },
  }),
}))

vi.mock('../api/nexarrClient', () => ({
  updateMyPreferences: (...args: unknown[]) => updateMyPreferencesMock(...args),
}))

describe('UserPreferencesPage', () => {
  afterEach(() => {
    cleanup()
    localStorage.clear()
    updateMyPreferencesMock.mockClear()
  })

  it('renders suite and current product preference sections', async () => {
    render(
      <MemoryRouter>
        <UserPreferencesPage />
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Preferences' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Suite Preferences' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'NexArr Preferences' })).toBeInTheDocument()
    expect(screen.getByText(/personal preferences for STL Compliance and the current product/i)).toBeInTheDocument()
    expect(screen.queryByRole('tab')).not.toBeInTheDocument()
  })

  it('saves suite theme changes through the platform preference API', async () => {
    render(
      <MemoryRouter>
        <UserPreferencesPage />
      </MemoryRouter>,
    )

    await screen.findByRole('heading', { name: 'Preferences' })

    fireEvent.change(screen.getByLabelText('Theme'), { target: { value: 'light' } })
    fireEvent.click(screen.getByRole('button', { name: 'Save suite preferences' }))

    await waitFor(() => {
      expect(updateMyPreferencesMock).toHaveBeenCalledWith({ themePreference: 'light' })
    })

    expect(await screen.findByText('Suite preferences saved.')).toBeInTheDocument()
  })
})
