import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { clearSubmissionStateForTests, pushSubmissionToast, setLocalSubmission } from '../lib/submissionState'
import type { FieldTaskSubmissionStatusResponse } from '../api/types'
import { useFieldTaskSubmissionState } from './useFieldTaskSubmissionState'

vi.mock('../api/client', () => ({
  getFieldTaskSubmissionStatus: vi.fn(),
}))

const api = await import('../api/client')

function Harness({ taskKeys }: { taskKeys: string[] }) {
  const { getChips, toasts, dismissToast, refreshServerStatus, isLoadingServer } =
    useFieldTaskSubmissionState('access-token', taskKeys)

  return (
    <div>
      <div data-testid="loading-state">{isLoadingServer ? 'loading' : 'ready'}</div>
      <pre data-testid="chips-task-a">{JSON.stringify(getChips('task-a'))}</pre>
      <pre data-testid="chips-task-b">{JSON.stringify(getChips('task-b'))}</pre>
      <div data-testid="toast-count">{toasts.length}</div>
      <button type="button" onClick={() => dismissToast(toasts[0]?.id ?? '')}>
        Dismiss toast
      </button>
      <button type="button" onClick={refreshServerStatus}>
        Refresh
      </button>
    </div>
  )
}

describe('useFieldTaskSubmissionState', () => {
  afterEach(() => {
    cleanup()
    clearSubmissionStateForTests()
    vi.clearAllMocks()
  })

  function renderHarness(taskKeys: string[]) {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <Harness taskKeys={taskKeys} />
      </QueryClientProvider>,
    )

    return queryClient
  }

  it('merges local in-flight submissions over synced server status', async () => {
    const response: FieldTaskSubmissionStatusResponse = {
      items: [
        {
          taskKey: 'task-a',
          submissionKind: 'acknowledge',
          status: 'synced',
          detailMessage: 'Server sync',
          recordedAt: '2026-06-26T12:00:00.000Z',
        },
        {
          taskKey: 'task-b',
          submissionKind: 'evidence',
          status: 'synced',
          detailMessage: 'Uploaded evidence',
          recordedAt: '2026-06-26T12:00:00.000Z',
        },
      ],
    }

    vi.mocked(api.getFieldTaskSubmissionStatus).mockResolvedValue(response)
    setLocalSubmission({
      taskKey: 'task-a',
      kind: 'acknowledge',
      phase: 'syncing',
      message: 'Syncing now',
    })

    renderHarness(['task-a', 'task-b'])

    await waitFor(() => {
      expect(screen.getByTestId('loading-state')).toHaveTextContent('ready')
    })

    expect(api.getFieldTaskSubmissionStatus).toHaveBeenCalledWith('access-token', ['task-a', 'task-b'])
    expect(screen.getByTestId('chips-task-a')).toHaveTextContent('"label":"Acknowledgment syncing"')
    expect(screen.getByTestId('chips-task-a')).toHaveTextContent('"tone":"progress"')
    expect(screen.getByTestId('chips-task-a')).toHaveTextContent('Syncing now')
    expect(screen.getByTestId('chips-task-b')).toHaveTextContent('"label":"Evidence submitted"')
    expect(screen.getByTestId('chips-task-b')).toHaveTextContent('"tone":"success"')
    expect(screen.getByTestId('chips-task-b')).toHaveTextContent('Uploaded evidence')
  })

  it('dismisses submission toasts and refreshes server status', async () => {
    vi.mocked(api.getFieldTaskSubmissionStatus).mockResolvedValue({ items: [] })
    pushSubmissionToast({ tone: 'info', message: 'Queued acknowledgment.' })

    const queryClient = renderHarness(['task-a'])
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries')

    await waitFor(() => {
      expect(screen.getByTestId('toast-count')).toHaveTextContent('1')
    })

    fireEvent.click(screen.getByRole('button', { name: 'Dismiss toast' }))
    expect(screen.getByTestId('toast-count')).toHaveTextContent('0')

    fireEvent.click(screen.getByRole('button', { name: 'Refresh' }))
    expect(invalidateSpy).toHaveBeenCalledWith({
      queryKey: ['fieldcompanion-submission-status', 'access-token'],
    })
  })
})
