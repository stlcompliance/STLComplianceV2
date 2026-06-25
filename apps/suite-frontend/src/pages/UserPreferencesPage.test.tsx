import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { UserPreferencesPage } from './UserPreferencesPage'

const useAuthMock = {
  me: {
    userId: 'person-1',
    tenantId: 'tenant-1',
    displayName: 'Demo Admin',
    tenantDisplayName: 'STL Demo Tenant',
    themePreference: 'system',
  },
}

vi.mock('../auth/AuthProvider', () => ({
  useAuth: () => useAuthMock,
}))

vi.mock('../api/nexarrClient', () => ({
  updateMyPreferences: vi.fn().mockResolvedValue({ themePreference: 'dark' }),
  updateMyPassword: vi.fn(),
}))

describe('UserPreferencesPage', () => {
  afterEach(() => {
    cleanup()
    localStorage.clear()
  })

  it('renders suite and current product preference sections', async () => {
    render(
      <MemoryRouter initialEntries={['/app/staffarr/preferences']}>
        <Routes>
          <Route path="/app/:productKey/preferences" element={<UserPreferencesPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Preferences' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Suite Preferences' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'StaffArr Preferences' })).toBeInTheDocument()
    expect(screen.getByText(/personal preferences for this app/i)).toBeInTheDocument()
    expect(screen.getByText('Product launch alerts')).toBeInTheDocument()
    expect(screen.getByText('Enable launch availability alerts')).toBeInTheDocument()
    expect(screen.queryByRole('tab')).not.toBeInTheDocument()
  })

  it('saves suite theme changes locally within the app', async () => {
    render(
      <MemoryRouter initialEntries={['/app/trainarr/preferences']}>
        <Routes>
          <Route path="/app/:productKey/preferences" element={<UserPreferencesPage />} />
        </Routes>
      </MemoryRouter>,
    )

    await screen.findByRole('heading', { name: 'Preferences' })

    fireEvent.change(screen.getByLabelText('Theme'), { target: { value: 'light' } })
    fireEvent.click(screen.getByRole('button', { name: 'Save suite preferences' }))

    await waitFor(() => {
      expect(
        localStorage.getItem('stl.theme.preference.v1:app:suite:tenant:tenant-1:user:person-1'),
      ).toBe('light')
    })

    expect(await screen.findByText('Suite preferences saved.')).toBeInTheDocument()
  })
})
