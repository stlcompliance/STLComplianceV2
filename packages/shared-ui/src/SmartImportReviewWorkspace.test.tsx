import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { SmartImportReviewWorkspace } from './SmartImportReviewWorkspace'

describe('SmartImportReviewWorkspace', () => {
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

  it('supports review decisions and commit plan creation for proposed records', async () => {
    const onReview = vi.fn()
    const onCreateCommitPlan = vi.fn()

    render(
      <SmartImportReviewWorkspace
        batches={[
          {
            batchId: 'batch-1',
            status: 'review_required',
            destinationProductHint: 'staffarr',
            sourceLabel: 'people.csv',
            proposedRecordCount: 1,
            updatedAt: '2026-06-11T00:00:00Z',
          },
        ]}
        selectedBatch={{
          batch: {
            batchId: 'batch-1',
            status: 'review_required',
            destinationProductHint: 'staffarr',
            sourceLabel: 'people.csv',
            proposedRecordCount: 1,
            updatedAt: '2026-06-11T00:00:00Z',
          },
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
              proposedPayload: {},
            },
          ],
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
})
