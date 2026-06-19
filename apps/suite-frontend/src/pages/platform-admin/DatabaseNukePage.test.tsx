import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ToastProvider } from '../../feedback'
import { DatabaseNukePage } from './DatabaseNukePage'

vi.mock('../../api/nexarrClient', () => ({
  executeDatabaseNuke: vi.fn(),
  getDatabaseNukePreview: vi.fn(),
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <ToastProvider>
        <MemoryRouter>
          <DatabaseNukePage />
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>,
  )
}

describe('DatabaseNukePage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('surfaces preview blockers before execution is allowed', async () => {
    vi.mocked(nexarr.getDatabaseNukePreview).mockResolvedValue({
      isEnabled: true,
      confirmationPhrase: 'NUKE DEMO DATA',
      generatedAt: new Date('2026-06-19T12:00:00Z').toISOString(),
      targets: [
        {
          productDatabase: 'staffarr',
          status: 'ready',
          connectionConfigured: true,
          tableCount: 2,
          truncateTableCount: 1,
          preserveTableCount: 1,
          estimatedRowsToDelete: 120,
          estimatedRowsPreserved: 30,
          tablesToTruncate: [],
          preservedTables: [],
          errorCode: null,
          errorMessage: null,
        },
        {
          productDatabase: 'routarr',
          status: 'error',
          connectionConfigured: true,
          tableCount: 1,
          truncateTableCount: 1,
          preserveTableCount: 0,
          estimatedRowsToDelete: 42,
          estimatedRowsPreserved: 0,
          tablesToTruncate: [],
          preservedTables: [],
          errorCode: 'fk_blocker',
          errorMessage: 'Foreign key blockers prevent truncation.',
        },
        {
          productDatabase: 'loadarr',
          status: 'missing_connection',
          connectionConfigured: false,
          tableCount: 0,
          truncateTableCount: 0,
          preserveTableCount: 0,
          estimatedRowsToDelete: 0,
          estimatedRowsPreserved: 0,
          tablesToTruncate: [],
          preservedTables: [],
          errorCode: null,
          errorMessage: null,
        },
      ],
    })

    renderPage()

    expect(await screen.findByText('Execution blockers')).toBeTruthy()
    expect(screen.getAllByText('routarr').length).toBeGreaterThan(1)
    expect(screen.getAllByText('Foreign key blockers prevent truncation.').length).toBeGreaterThan(1)
    expect(screen.getAllByText('loadarr').length).toBeGreaterThan(1)
    expect(
      screen.getAllByText('No connection string is configured for this product database.').length,
    ).toBeGreaterThan(1)
    expect(screen.getByRole('button', { name: 'Run database nuke' })).toBeDisabled()
  })
})
