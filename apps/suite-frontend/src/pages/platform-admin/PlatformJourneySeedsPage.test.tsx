import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ToastProvider } from '../../feedback'
import { PlatformJourneySeedsPage } from './PlatformJourneySeedsPage'

vi.mock('../../api/nexarrClient', () => ({
  listReferenceDatasets: vi.fn(),
  createReferenceDatasetInput: vi.fn(),
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={client}>
      <ToastProvider>
        <PlatformJourneySeedsPage />
      </ToastProvider>
    </QueryClientProvider>,
  )
}

describe('PlatformJourneySeedsPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('lets the user add a single value or bulk import values for a selected dataset', async () => {
    vi.mocked(nexarr.listReferenceDatasets).mockResolvedValue([
      {
        id: 'dataset-1',
        key: 'maintainarr-asset-class',
        name: 'Asset Class',
        category: 'asset_class',
        ownerService: 'MaintainArr',
        status: 'ready',
        currentPublishedVersion: null,
        sourceCount: 1,
        entityCount: 0,
        pendingReviewCount: 0,
        failedImportCount: 0,
        lastPublishedAt: null,
        createdAt: '2026-06-08T15:00:00Z',
        updatedAt: '2026-06-08T15:00:00Z',
      },
    ])
    vi.mocked(nexarr.createReferenceDatasetInput).mockResolvedValue({
      id: 'job-1',
      datasetId: 'dataset-1',
      datasetKey: 'maintainarr-asset-class',
      datasetName: 'Asset Class',
      sourceId: 'source-1',
      sourceKey: 'platform-admin-input',
      sourceName: 'Platform admin input',
      tenantId: null,
      requestedByPersonId: null,
      status: 'in_progress',
      rawObjectKey: null,
      fileName: null,
      startedAt: '2026-06-08T15:01:00Z',
      completedAt: null,
      errorSummary: null,
      stagingRecordCount: 1,
      pendingReviewCount: 1,
      approvedCount: 0,
      rejectedCount: 0,
      createdAt: '2026-06-08T15:01:00Z',
      updatedAt: '2026-06-08T15:01:00Z',
    })

    renderPage()

    expect(await screen.findByText('MaintainArr - Asset Class')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Value'), { target: { value: 'Asset Class A' } })
    fireEvent.click(screen.getByRole('button', { name: 'Add value' }))

    await waitFor(() => {
      expect(nexarr.createReferenceDatasetInput).toHaveBeenCalledWith('dataset-1', {
        value: 'Asset Class A',
      })
    })

    expect(await screen.findByText('Latest dataset input')).toBeInTheDocument()
    expect(screen.getByText('Platform admin input')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Values'), {
      target: { value: 'Asset Class B\nAsset Class C' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Import values' }))

    await waitFor(() => {
      expect(nexarr.createReferenceDatasetInput).toHaveBeenLastCalledWith('dataset-1', {
        valuesText: 'Asset Class B\nAsset Class C',
      })
    })
  })
})
