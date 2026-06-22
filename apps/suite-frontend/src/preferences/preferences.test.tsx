import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { updateMyPreferences } from '../api/nexarrClient'
import { useSuitePreferences } from './preferences'

vi.mock('../api/nexarrClient', () => ({
  updateMyPreferences: vi.fn().mockResolvedValue(undefined),
}))

function PreferenceProbe() {
  const suitePreferences = useSuitePreferences({
    tenantId: 'tenant-1',
    personId: 'person-1',
    initialTheme: 'dark',
  })

  if (suitePreferences.isLoading) {
    return <p>Loading preferences...</p>
  }

  return (
    <div>
      <p data-testid="hints-value">{String(suitePreferences.preferences.assistantShowAssumptions)}</p>
      <button type="button" onClick={() => suitePreferences.setPreference('assistantShowAssumptions', false)}>
        Hide hints
      </button>
      <button type="button" onClick={() => void suitePreferences.save()}>
        Save
      </button>
    </div>
  )
}

describe('useSuitePreferences', () => {
  afterEach(() => {
    cleanup()
    localStorage.clear()
    vi.clearAllMocks()
  })

  it('persists suite preference state for hints and theme', async () => {
    render(<PreferenceProbe />)

    expect(await screen.findByTestId('hints-value')).toHaveTextContent('true')

    fireEvent.click(screen.getByRole('button', { name: 'Hide hints' }))
    fireEvent.click(screen.getByRole('button', { name: 'Save' }))

    await waitFor(() => {
      expect(updateMyPreferences).toHaveBeenCalledWith({ themePreference: 'dark' })
    })

    const stored = JSON.parse(
      localStorage.getItem('stl.preferences.suite.v1:tenant-1:person-1') ?? '{}',
    ) as Record<string, unknown>
    expect(stored.assistantShowAssumptions).toBe(false)
    expect(stored.theme).toBe('dark')
  })
})
