import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DispatchExceptionQueuePanel } from './DispatchExceptionQueuePanel'

vi.mock('../api/client', () => ({
  listDispatchExceptions: vi.fn(),
  createDispatchException: vi.fn(),
  assignDispatchException: vi.fn(),
  resolveDispatchException: vi.fn(),
  linkDispatchExceptionTrip: vi.fn(),
}))

import * as client from '../api/client'

function renderPanel(canTriage: boolean) {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <DispatchExceptionQueuePanel
        accessToken="token"
        userId="11111111-1111-1111-1111-111111111111"
        canTriage={canTriage}
      />
    </QueryClientProvider>,
  )
}

describe('DispatchExceptionQueuePanel', () => {
  afterEach(() => cleanup())

  it('renders open exceptions from queue API', async () => {
    vi.mocked(client.listDispatchExceptions).mockResolvedValue({
      totalCount: 1,
      openCount: 2,
      items: [
        {
          exceptionId: 'ex-1',
          exceptionKey: 'DEX-20260528-ABC',
          title: 'Late departure',
          description: 'Driver delayed',
          category: 'delay',
          status: 'open',
          tripId: null,
          tripNumber: null,
          tripTitle: null,
          assignedToUserId: null,
          resolutionNotes: '',
          createdByUserId: 'u1',
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          assignedAt: null,
          resolvedAt: null,
        },
      ],
    })

    renderPanel(true)
    expect(await screen.findByText('Exception queue')).toBeTruthy()
    expect(screen.getByText('Late departure')).toBeTruthy()
    expect(screen.getByTestId('exception-row-ex-1')).toBeTruthy()
  })
})
