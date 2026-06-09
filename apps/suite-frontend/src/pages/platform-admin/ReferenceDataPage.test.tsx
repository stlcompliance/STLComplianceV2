import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ToastProvider } from '../../feedback'
import * as nexarr from '../../api/nexarrClient'
import { ReferenceDataPage } from './ReferenceDataPage'

vi.mock('../../api/nexarrClient', () => ({
  getReferenceDataDashboard: vi.fn(),
  listReferenceDatasets: vi.fn(),
  listReferenceSources: vi.fn(),
  listReferenceImports: vi.fn(),
  listReferenceStagingRecords: vi.fn(),
  listReferenceCrosswalks: vi.fn(),
  listReferencePublishHistory: vi.fn(),
  createReferenceDataset: vi.fn(),
  createReferenceSource: vi.fn(),
  createReferenceImport: vi.fn(),
  createReferenceMasterCsvImport: vi.fn(),
  publishReferenceDataset: vi.fn(),
  approveReferenceStagingRecord: vi.fn(),
  rejectReferenceStagingRecord: vi.fn(),
  mergeReferenceStagingRecord: vi.fn(),
  escalateReferenceStagingRecord: vi.fn(),
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <ToastProvider>
        <ReferenceDataPage />
      </ToastProvider>
    </QueryClientProvider>,
  )
}

function mockReferenceDataQueries() {
  vi.mocked(nexarr.getReferenceDataDashboard).mockResolvedValue({
    datasetCount: 1,
    sourceCount: 1,
    jobCount: 1,
    pendingReviewCount: 2,
    failedImportCount: 0,
    publishedEntityCount: 2,
    crosswalkCount: 2,
    publishEventCount: 2,
    generatedAt: new Date().toISOString(),
  })

  vi.mocked(nexarr.listReferenceDatasets).mockResolvedValue([
    {
      id: 'dataset-1',
      key: 'vehicle-taxonomy',
      name: 'Vehicle Taxonomy',
      category: 'vehicle',
      ownerService: 'MaintainArr',
      status: 'published',
      currentPublishedVersion: 'v1',
      sourceCount: 1,
      entityCount: 1,
      pendingReviewCount: 2,
      failedImportCount: 0,
      lastPublishedAt: new Date().toISOString(),
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    },
  ])

  vi.mocked(nexarr.listReferenceSources).mockResolvedValue([
    {
      id: 'source-1',
      key: 'nhtsa-vpic',
      name: 'NHTSA vPIC',
      sourceType: 'connector',
      connectorType: 'nhtsa',
      authorityRank: 100,
      refreshCadence: 'daily',
      termsNotes: null,
      enabled: true,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    },
  ])

  vi.mocked(nexarr.listReferenceImports).mockResolvedValue([
    {
      id: 'import-1',
      datasetId: 'dataset-1',
      datasetKey: 'vehicle-taxonomy',
      datasetName: 'Vehicle Taxonomy',
      sourceId: 'source-1',
      sourceKey: 'nhtsa-vpic',
      sourceName: 'NHTSA vPIC',
      tenantId: null,
      requestedByPersonId: null,
      status: 'in_progress',
      rawObjectKey: 'seed/reference/vehicle-taxonomy.csv',
      fileName: 'vehicle-taxonomy.csv',
      startedAt: new Date().toISOString(),
      completedAt: null,
      errorSummary: null,
      stagingRecordCount: 2,
      pendingReviewCount: 2,
      approvedCount: 0,
      rejectedCount: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    },
  ])

  vi.mocked(nexarr.listReferenceStagingRecords).mockResolvedValue([
    {
      id: 'staging-1',
      jobId: 'import-1',
      datasetId: 'dataset-1',
      datasetKey: 'vehicle-taxonomy',
      sourceId: 'source-1',
      sourceKey: 'nhtsa-vpic',
      targetDatasetId: 'dataset-1',
      targetDatasetKey: 'vehicle-taxonomy',
      targetDatasetName: 'Vehicle Taxonomy',
      targetOwnerService: 'MaintainArr',
      rowNumber: 1,
      rawPayloadJson: '{}',
      normalizedPayloadJson: '{}',
      proposedEntityType: 'vehicle',
      proposedCanonicalKey: '1fdxf46u1ec1gk5',
      confidence: 0.97,
      status: 'needs_review',
      reviewReason: null,
      reviewerPersonId: null,
      reviewedAt: null,
      referenceEntityId: null,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    },
  ])

  vi.mocked(nexarr.listReferenceCrosswalks).mockResolvedValue([])
  vi.mocked(nexarr.listReferencePublishHistory).mockResolvedValue([])
}

describe('ReferenceDataPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders seeded reference data and review queue', async () => {
    mockReferenceDataQueries()

    renderPage()

    expect(await screen.findByText('Reference data')).toBeTruthy()
    expect(screen.getAllByText('Vehicle Taxonomy').length).toBeGreaterThan(0)
    expect(screen.getAllByText('NHTSA vPIC').length).toBeGreaterThan(0)
    expect(screen.getByRole('button', { name: 'Download template CSV' })).toBeTruthy()
    expect(await screen.findByText('Upload master CSV')).toBeTruthy()
    expect(await screen.findByRole('button', { name: 'Approve' })).toBeTruthy()
  })

  it('downloads a master CSV template', async () => {
    mockReferenceDataQueries()
    const createObjectUrl = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:reference-template')
    const revokeObjectUrl = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
    const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {})

    renderPage()
    fireEvent.click(await screen.findByRole('button', { name: 'Download template CSV' }))

    expect(createObjectUrl).toHaveBeenCalledTimes(1)
    expect(click).toHaveBeenCalledTimes(1)
    expect(revokeObjectUrl).toHaveBeenCalledWith('blob:reference-template')

    const blob = createObjectUrl.mock.calls[0]?.[0] as Blob
    expect(await blob.text()).toContain('dataset_key,entity_type,canonical_key,display_name')
    expect(await blob.text()).toContain('Replace or remove this sample row before upload.')

    createObjectUrl.mockRestore()
    revokeObjectUrl.mockRestore()
    click.mockRestore()
  })
})
