import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { PersonTrainingHistoryPanel } from './PersonTrainingHistoryPanel'

vi.mock('@stl/shared-ui', () => ({
  StaticSearchPicker: ({
    label,
    value,
    onChange,
  }: {
    label: string
    value: string
    onChange: (value: string) => void
  }) => (
    <label>
      {label}
      <input value={value} onChange={(event) => onChange(event.target.value)} />
    </label>
  ),
  ApiErrorCallout: ({
    title,
    message,
    retryLabel,
  }: {
    title: string
    message: string
    retryLabel?: string
  }) => (
    <div>
      <p>{title}</p>
      <p>{message}</p>
      {retryLabel ? <button type="button">{retryLabel}</button> : null}
    </div>
  ),
  getErrorMessage: (error: unknown, fallback: string) =>
    error instanceof Error ? error.message : fallback,
}))

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getPersonTrainingHistory: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <PersonTrainingHistoryPanel
        accessToken="token"
        defaultStaffarrPersonId="person-1"
        personOptions={[{ value: 'person-1', label: 'Person 1' }]}
      />
    </QueryClientProvider>,
  )
}

describe('PersonTrainingHistoryPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when history load fails', async () => {
    vi.mocked(client.getPersonTrainingHistory).mockRejectedValue(new Error('history down'))
    renderPanel()

    expect(await screen.findByText('Training history unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry history' })).toBeInTheDocument()
  })
})
