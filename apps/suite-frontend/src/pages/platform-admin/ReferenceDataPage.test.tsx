import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ToastProvider } from '../../feedback'
import { ReferenceDataPage } from './ReferenceDataPage'

vi.mock('../../api/nexarrClient', () => ({
  getReferenceDataDashboard: vi.fn(),
  listReferenceDatasets: vi.fn(),
  listReferenceSources: vi.fn(),
  listReferenceImports: vi.fn(),
  listReferenceDatasetEntities: vi.fn(),
  listReferenceStagingRecords: vi.fn(),
  listReferenceCrosswalks: vi.fn(),
  listReferencePublishHistory: vi.fn(),
  createReferenceDataset: vi.fn(),
  updateReferenceDataset: vi.fn(),
  deleteReferenceDataset: vi.fn(),
  createReferenceDatasetInput: vi.fn(),
  createReferenceSource: vi.fn(),
  createReferenceImport: vi.fn(),
  createReferenceMasterCsvImport: vi.fn(),
  publishReferenceDataset: vi.fn(),
  publishReferenceDatasets: vi.fn(),
  publishAllReferenceDatasets: vi.fn(),
  updateReferenceEntity: vi.fn(),
  deleteReferenceEntity: vi.fn(),
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
    datasetCount: 2,
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
    {
      id: 'dataset-2',
      key: 'governing-bodies',
      name: 'Governing Bodies',
      category: 'compliance',
      ownerService: 'Compliance Core',
      status: 'ready',
      currentPublishedVersion: null,
      sourceCount: 1,
      entityCount: 1,
      pendingReviewCount: 0,
      failedImportCount: 0,
      lastPublishedAt: null,
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
      sourceKey: 'platform-admin-input',
      sourceName: 'Platform admin input',
      tenantId: null,
      requestedByPersonId: null,
      status: 'completed',
      rawObjectKey: 'seed/reference/vehicle-taxonomy.csv',
      fileName: 'vehicle-taxonomy.csv',
      startedAt: new Date().toISOString(),
      completedAt: new Date().toISOString(),
      errorSummary: null,
      stagingRecordCount: 2,
      pendingReviewCount: 2,
      approvedCount: 1,
      rejectedCount: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    },
  ])

  vi.mocked(nexarr.listReferenceDatasetEntities).mockResolvedValue([
    {
      id: 'entity-1',
      datasetId: 'dataset-1',
      datasetKey: 'vehicle-taxonomy',
      datasetName: 'Vehicle Taxonomy',
      entityType: 'vehicle',
      canonicalKey: 'fleet-truck-001',
      displayName: 'Fleet Truck 001',
      status: 'active',
      normalizedFieldsJson: '{"displayName":"Fleet Truck 001"}',
      firstSeenSourceId: 'source-1',
      firstSeenSourceKey: 'platform-admin-input',
      currentVersionId: 'version-1',
      currentVersion: 1,
      publishedAt: new Date().toISOString(),
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      versions: [
        {
          id: 'version-1',
          referenceEntityId: 'entity-1',
          version: 1,
          fieldsJson: '{"displayName":"Fleet Truck 001"}',
          sourceEvidenceJson: '{"value":"Fleet Truck 001"}',
          effectiveDate: '2026-06-09',
          publishedAt: new Date().toISOString(),
          supersededByVersionId: null,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        },
      ],
      crosswalks: [],
      tenantOverlays: [],
      productMappings: [],
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

  it('renders the consolidated reference data and dataset inputs surface', async () => {
    mockReferenceDataQueries()

    renderPage()

    expect(await screen.findByText('Reference data')).toBeInTheDocument()
    expect(screen.getByText('Dataset Control Plane')).toBeInTheDocument()
    expect(screen.getByText('Dataset Inputs And Current Entities')).toBeInTheDocument()
    expect(await screen.findByText('Fleet Truck 001')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Download template CSV' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Upload master CSV' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Approve' })).toBeInTheDocument()
  })

  it('supports batch publish from the combined dataset table', async () => {
    mockReferenceDataQueries()
    vi.mocked(nexarr.publishReferenceDatasets).mockResolvedValue({
      requestedCount: 1,
      publishedCount: 1,
      items: [],
      processedAt: new Date().toISOString(),
    })

    renderPage()

    fireEvent.click(await screen.findByLabelText('Select Vehicle Taxonomy'))
    fireEvent.click(screen.getByRole('button', { name: 'Publish selected' }))

    await waitFor(() => {
      expect(nexarr.publishReferenceDatasets).toHaveBeenCalledWith({
        datasetIds: ['dataset-1'],
        summary: 'Published from platform admin batch',
      })
    })
  })

  it('lets the user add dataset inputs from the same page', async () => {
    mockReferenceDataQueries()
    vi.mocked(nexarr.createReferenceDatasetInput).mockResolvedValue({
      id: 'job-2',
      datasetId: 'dataset-1',
      datasetKey: 'vehicle-taxonomy',
      datasetName: 'Vehicle Taxonomy',
      sourceId: 'source-1',
      sourceKey: 'platform-admin-input',
      sourceName: 'Platform admin input',
      tenantId: null,
      requestedByPersonId: null,
      status: 'completed',
      rawObjectKey: null,
      fileName: null,
      startedAt: new Date().toISOString(),
      completedAt: new Date().toISOString(),
      errorSummary: null,
      stagingRecordCount: 1,
      pendingReviewCount: 0,
      approvedCount: 1,
      rejectedCount: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    })

    renderPage()

    fireEvent.change(await screen.findByLabelText('Value'), {
      target: { value: 'Fleet Truck 002' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Add value' }))

    await waitFor(() => {
      expect(nexarr.createReferenceDatasetInput).toHaveBeenCalledWith('dataset-1', {
        value: 'Fleet Truck 002',
      })
    })
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
