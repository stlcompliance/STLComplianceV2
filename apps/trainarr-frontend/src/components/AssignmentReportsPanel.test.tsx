import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { AssignmentReportsPanel } from './AssignmentReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getAssignmentReportSummary: vi.fn(),
    exportAssignmentReportSummaryCsv: vi.fn(),
  }
})

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...actual,
    ApiErrorCallout: ({
      title,
      retryLabel,
      onRetry,
    }: {
      title: string
      retryLabel?: string
      onRetry?: () => void
    }) => (
      <div>
        <div>{title}</div>
        {retryLabel && onRetry ? (
          <button type="button" onClick={onRetry}>
            {retryLabel}
          </button>
        ) : null}
      </div>
    ),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <AssignmentReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('AssignmentReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders summary metrics and effectiveness analytics', async () => {
    vi.mocked(client.getAssignmentReportSummary).mockResolvedValueOnce({
      totalAssignments: 4,
      openAssignments: 2,
      completedAssignments: 2,
      overdueAssignments: 1,
      completionRatePercent: 50,
      analytics: {
        averageCompletionDays: 3.2,
        evaluationPassRatePercent: 75,
        averageEvaluationScore: 88.5,
        evidenceCoveragePercent: 100,
        signoffCoveragePercent: 50,
        totalLaborHours: 12.5,
        totalLaborCost: 625,
        averageLaborHoursPerCompletedAssignment: 6.25,
        averageLaborCostPerCompletedAssignment: 312.5,
        localizedContentReferenceCount: 2,
        distinctContentLocaleCount: 2,
      },
      recentAssignments: [
        {
          assignmentId: 'assignment-1',
          staffarrPersonId: 'person-1',
          definitionKey: 'def-1',
          definitionName: 'Definition 1',
          status: 'completed',
          dueAt: null,
          isOverdue: false,
          createdAt: new Date().toISOString(),
          completedAt: new Date().toISOString(),
        },
      ],
    })
    vi.mocked(client.exportAssignmentReportSummaryCsv).mockResolvedValue(new Blob(['x']))

    renderPanel()

    expect(await screen.findByText('Training effectiveness')).toBeInTheDocument()
    expect(screen.getByText('3.2 days')).toBeInTheDocument()
    expect(screen.getByText('75.0%')).toBeInTheDocument()
    expect(screen.getByText('88.5')).toBeInTheDocument()
    expect(screen.getByText('100.0%')).toBeInTheDocument()
    expect(screen.getByText('12.50')).toBeInTheDocument()
    expect(screen.getByText('$625.00')).toBeInTheDocument()
    expect(screen.getByText('6.25')).toBeInTheDocument()
    expect(screen.getByText('$312.50')).toBeInTheDocument()
    expect(screen.getByText('Locale-tagged refs')).toBeInTheDocument()
    expect(screen.getByText('Locales represented')).toBeInTheDocument()
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getAssignmentReportSummary).mockRejectedValue(new Error('summary down'))
    vi.mocked(client.exportAssignmentReportSummaryCsv).mockResolvedValue(new Blob(['x']))

    renderPanel()

    expect(await screen.findByText('Assignment report unavailable')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Retry summary' }))

    await waitFor(() => {
      expect(client.getAssignmentReportSummary).toHaveBeenCalledTimes(2)
    })
  })

  it('renders effectiveness analytics', async () => {
    vi.mocked(client.getAssignmentReportSummary).mockResolvedValueOnce({
      totalAssignments: 4,
      openAssignments: 2,
      completedAssignments: 2,
      overdueAssignments: 1,
      completionRatePercent: 50,
      analytics: {
        averageCompletionDays: 3.2,
        evaluationPassRatePercent: 75,
        averageEvaluationScore: 88.5,
        evidenceCoveragePercent: 100,
        signoffCoveragePercent: 50,
        totalLaborHours: 12.5,
        totalLaborCost: 625,
        averageLaborHoursPerCompletedAssignment: 6.25,
        averageLaborCostPerCompletedAssignment: 312.5,
        localizedContentReferenceCount: 2,
        distinctContentLocaleCount: 2,
      },
      recentAssignments: [],
    })
    vi.mocked(client.exportAssignmentReportSummaryCsv).mockResolvedValue(new Blob(['x']))

    renderPanel()

    expect(await screen.findByText('Training effectiveness')).toBeInTheDocument()
    expect(screen.getByText('3.2 days')).toBeInTheDocument()
    expect(screen.getByText('75.0%')).toBeInTheDocument()
    expect(screen.getByText('88.5')).toBeInTheDocument()
    expect(screen.getByText('$625.00')).toBeInTheDocument()
  })
})
