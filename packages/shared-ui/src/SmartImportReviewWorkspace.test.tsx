import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SmartImportReviewWorkspace } from './SmartImportReviewWorkspace'

const baseBatch = {
  batchId: 'batch-1',
  status: 'review_required',
  destinationProductHint: 'staffarr',
  sourceLabel: 'people.csv',
  fileCount: 1,
  proposedRecordCount: 1,
  createdAt: '2026-06-11T00:00:00Z',
  updatedAt: '2026-06-11T00:00:00Z',
}

describe('SmartImportReviewWorkspace', () => {
  afterEach(() => {
    cleanup()
  })

  it('uploads a source file with the selected destination product', async () => {
    const onUpload = vi.fn()
    const file = new File(['person,training'], 'training.csv', { type: 'text/csv' })
    const { container } = render(
      <SmartImportReviewWorkspace
        batches={[]}
        onRefresh={vi.fn()}
        onSelectBatch={vi.fn()}
        onUpload={onUpload}
        onReview={vi.fn()}
        onCreateCommitPlan={vi.fn()}
        initialDestinationProduct="trainarr"
      />,
    )

    expect(screen.getByLabelText('Destination')).toHaveValue('trainarr')
    fireEvent.change(screen.getByLabelText('Destination'), { target: { value: 'recordarr' } })
    fireEvent.change(container.querySelector('input[type="file"]')!, { target: { files: [file] } })
    fireEvent.click(screen.getByRole('button', { name: /Upload/i }))

    await waitFor(() => expect(onUpload).toHaveBeenCalledWith(file, 'recordarr'))
  })

  it('accepts OrdArr as an explicit review destination', async () => {
    const onUpload = vi.fn()
    const file = new File(['order,request'], 'orders.csv', { type: 'text/csv' })
    const { container } = render(
      <SmartImportReviewWorkspace
        batches={[]}
        onRefresh={vi.fn()}
        onSelectBatch={vi.fn()}
        onUpload={onUpload}
        onReview={vi.fn()}
        onCreateCommitPlan={vi.fn()}
        initialDestinationProduct="ordarr"
      />,
    )

    expect(screen.getByLabelText('Destination')).toHaveValue('ordarr')
    fireEvent.change(container.querySelector('input[type="file"]')!, { target: { files: [file] } })
    fireEvent.click(screen.getByRole('button', { name: /Upload/i }))

    await waitFor(() => expect(onUpload).toHaveBeenCalledWith(file, 'ordarr'))
  })

  it('supports review decisions and commit plan creation for proposed records', async () => {
    const onReview = vi.fn()
    const onCreateCommitPlan = vi.fn()

    render(
      <SmartImportReviewWorkspace
        batches={[
          {
            ...baseBatch,
          },
        ]}
        selectedBatch={{
          batch: baseBatch,
          files: [],
          classifications: [],
          proposedRecords: [
            {
              proposedRecordId: 'proposed-1',
              destinationProduct: 'staffarr',
              entityType: 'person',
              operation: 'create',
              confidence: 91,
              reviewStatus: 'pending',
              requiresReview: true,
              reviewReasons: ['person_create_or_link'],
              proposedPayload: {
                proposedFields: {
                  displayName: 'Avery Tech',
                },
              },
            },
          ],
          commitPlans: [],
        }}
        onRefresh={vi.fn()}
        onSelectBatch={vi.fn()}
        onUpload={vi.fn()}
        onReview={onReview}
        onCreateCommitPlan={onCreateCommitPlan}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Approve' }))
    fireEvent.click(screen.getByRole('button', { name: /Plan commit/i }))

    await waitFor(() => expect(onReview).toHaveBeenCalledWith('proposed-1', 'approved'))
    expect(onCreateCommitPlan).toHaveBeenCalledWith('batch-1')
  })

  it('renders retained files, classifications, review reasons, and proposed fields', () => {
    render(
      <SmartImportReviewWorkspace
        batches={[baseBatch]}
        selectedBatch={{
          batch: baseBatch,
          files: [
            {
              fileId: 'file-1',
              fileName: 'people.csv',
              contentType: 'text/csv',
              sizeBytes: 1536,
              sha256: '0123456789abcdef0123456789abcdef',
              recordArrRecordId: 'recordarr-record-123456789',
              recordArrFileId: 'recordarr-file-123456789',
              status: 'retained',
            },
          ],
          classifications: [
            {
              classificationId: 'classification-1',
              destinationProduct: 'staffarr',
              entityType: 'person',
              confidence: 91,
              requiresReview: true,
              reviewReasons: ['person_create_or_link', 'human_confirmation_required'],
              notes: 'Looks like a staff import.',
            },
          ],
          proposedRecords: [
            {
              proposedRecordId: 'proposed-1',
              destinationProduct: 'staffarr',
              entityType: 'person',
              operation: 'create',
              confidence: 91,
              reviewStatus: 'review_required',
              requiresReview: true,
              reviewReasons: ['person_create_or_link'],
              proposedPayload: {
                proposedFields: {
                  displayName: 'Avery Tech',
                  roleKey: 'maintenance_manager',
                },
              },
            },
          ],
          commitPlans: [
            {
              commitPlanId: 'plan-1',
              status: 'draft',
              stepCount: 1,
              completedStepCount: 0,
              failedStepCount: 0,
              createdAt: '2026-06-11T00:00:00Z',
            },
          ],
        }}
        onRefresh={vi.fn()}
        onSelectBatch={vi.fn()}
        onUpload={vi.fn()}
        onReview={vi.fn()}
        onCreateCommitPlan={vi.fn()}
      />,
    )

    expect(screen.getByText('Retained source files')).toBeInTheDocument()
    expect(screen.getAllByText('people.csv').length).toBeGreaterThan(0)
    expect(screen.getByText('1.5 KB')).toBeInTheDocument()
    expect(screen.getByText('Classification evidence')).toBeInTheDocument()
    expect(screen.getByText('Looks like a staff import.')).toBeInTheDocument()
    expect(screen.getAllByText('Person create/link decision').length).toBeGreaterThan(0)
    expect(screen.getByText('Human confirmation required')).toBeInTheDocument()
    expect(screen.getByText('Avery Tech')).toBeInTheDocument()
    expect(screen.getByText('maintenance_manager')).toBeInTheDocument()
    expect(screen.getByText('Commit plans')).toBeInTheDocument()
  })

  it('hides rejected batches from the upload-side queue', () => {
    render(
      <SmartImportReviewWorkspace
        batches={[
          {
            ...baseBatch,
            batchId: 'batch-active',
            sourceLabel: 'active-assets.csv',
          },
          {
            ...baseBatch,
            batchId: 'batch-rejected',
            status: 'rejected',
            sourceLabel: 'rejected-assets.csv',
          },
        ]}
        onRefresh={vi.fn()}
        onSelectBatch={vi.fn()}
        onUpload={vi.fn()}
        onReview={vi.fn()}
        onCreateCommitPlan={vi.fn()}
      />,
    )

    expect(screen.getByText('active-assets.csv')).toBeInTheDocument()
    expect(screen.queryByText('rejected-assets.csv')).not.toBeInTheDocument()
  })
})
