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
    vi.restoreAllMocks()
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

  it('bulk approves only proposed records that still need review decisions', async () => {
    const onApproveAll = vi.fn().mockResolvedValue(undefined)
    const onReview = vi.fn()
    const confirm = vi.spyOn(window, 'confirm').mockReturnValue(true)

    render(
      <SmartImportReviewWorkspace
        batches={[
          {
            ...baseBatch,
            proposedRecordCount: 4,
          },
        ]}
        selectedBatch={{
          batch: {
            ...baseBatch,
            proposedRecordCount: 4,
          },
          files: [],
          classifications: [],
          proposedRecords: [
            {
              proposedRecordId: 'proposed-review',
              destinationProduct: 'maintainarr',
              entityType: 'asset',
              operation: 'create',
              confidence: 82,
              reviewStatus: 'review_required',
              requiresReview: true,
              reviewReasons: ['asset_create_or_link'],
              proposedPayload: {
                proposedFields: {
                  assetTag: 'ASET-001',
                },
              },
            },
            {
              proposedRecordId: 'proposed-needs',
              destinationProduct: 'maintainarr',
              entityType: 'asset',
              operation: 'create',
              confidence: 77,
              reviewStatus: 'needs_changes',
              requiresReview: true,
              reviewReasons: ['human_confirmation_required'],
              proposedPayload: {
                proposedFields: {
                  assetTag: 'ASET-002',
                },
              },
            },
            {
              proposedRecordId: 'proposed-approved',
              destinationProduct: 'maintainarr',
              entityType: 'asset',
              operation: 'create',
              confidence: 94,
              reviewStatus: 'approved',
              requiresReview: false,
              reviewReasons: [],
              proposedPayload: {
                proposedFields: {
                  assetTag: 'ASET-003',
                },
              },
            },
            {
              proposedRecordId: 'proposed-rejected',
              destinationProduct: 'maintainarr',
              entityType: 'asset',
              operation: 'create',
              confidence: 68,
              reviewStatus: 'rejected',
              requiresReview: true,
              reviewReasons: [],
              proposedPayload: {
                proposedFields: {
                  assetTag: 'ASET-004',
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
        onApproveAll={onApproveAll}
        onCreateCommitPlan={vi.fn()}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Approve all (2)' }))

    await waitFor(() => {
      expect(onApproveAll).toHaveBeenCalledWith(['proposed-review', 'proposed-needs'])
    })
    expect(confirm).toHaveBeenCalledWith(
      'Approve 2 proposed records? 2 already approved or rejected records will be skipped.',
    )
    expect(onReview).not.toHaveBeenCalled()
  })

  it('applies manual source-column mapping overrides for delimited imports', async () => {
    const onApplyMappingOverride = vi.fn().mockResolvedValue(undefined)
    const confirm = vi.spyOn(window, 'confirm').mockReturnValue(true)

    render(
      <SmartImportReviewWorkspace
        batches={[
          {
            ...baseBatch,
            destinationProductHint: 'maintainarr',
            proposedRecordCount: 2,
          },
        ]}
        selectedBatch={{
          batch: {
            ...baseBatch,
            destinationProductHint: 'maintainarr',
            proposedRecordCount: 2,
          },
          files: [
            {
              fileId: 'file-1',
              fileName: 'assets.tsv',
              contentType: 'text/tab-separated-values',
              sizeBytes: 120,
              sha256: '0123456789abcdef',
              status: 'retained',
            },
          ],
          classifications: [
            {
              classificationId: 'classification-1',
              destinationProduct: 'maintainarr',
              entityType: 'asset',
              confidence: 90,
              requiresReview: true,
              reviewReasons: ['human_confirmation_required'],
            },
          ],
          proposedRecords: [
            {
              proposedRecordId: 'proposed-1',
              destinationProduct: 'maintainarr',
              entityType: 'asset',
              operation: 'create',
              confidence: 90,
              reviewStatus: 'review_required',
              requiresReview: true,
              reviewReasons: ['human_confirmation_required'],
              proposedPayload: {
                sourceFields: {
                  'Fleet Asset': '16458',
                  Description: 'Portable generator',
                },
                proposedFields: {
                  assetTag: 'wrong',
                },
              },
            },
            {
              proposedRecordId: 'proposed-2',
              destinationProduct: 'maintainarr',
              entityType: 'asset',
              operation: 'create',
              confidence: 90,
              reviewStatus: 'review_required',
              requiresReview: true,
              reviewReasons: ['human_confirmation_required'],
              proposedPayload: {
                sourceFields: {
                  'Fleet Asset': '16459',
                  Description: 'Refrigerated trailer',
                },
                proposedFields: {
                  assetTag: 'wrong',
                },
              },
            },
          ],
          commitPlans: [],
        }}
        onRefresh={vi.fn()}
        onSelectBatch={vi.fn()}
        onUpload={vi.fn()}
        onReview={vi.fn()}
        onApproveAll={vi.fn()}
        onApplyMappingOverride={onApplyMappingOverride}
        onCreateCommitPlan={vi.fn()}
      />,
    )

    expect(screen.getByText('Manual mapping override')).toBeInTheDocument()
    const targetInputs = screen.getAllByPlaceholderText('Ignore')
    fireEvent.change(targetInputs[0], { target: { value: 'displayName' } })
    fireEvent.change(targetInputs[1], { target: { value: 'assetTag' } })
    fireEvent.click(screen.getByRole('button', { name: 'Apply mapping (2)' }))

    await waitFor(() => {
      expect(onApplyMappingOverride).toHaveBeenCalledWith([
        { sourceField: 'Description', targetField: 'displayName' },
        { sourceField: 'Fleet Asset', targetField: 'assetTag' },
      ])
    })
    expect(confirm).toHaveBeenCalledWith(
      'Apply 2 manual mappings to 2 proposed records? Approved records will return to review and rejected records will be skipped.',
    )
  })

  it('offers SupplyArr part-specific mapping targets for part imports', () => {
    const { container } = render(
      <SmartImportReviewWorkspace
        batches={[
          {
            ...baseBatch,
            destinationProductHint: 'supplyarr',
            proposedRecordCount: 1,
          },
        ]}
        selectedBatch={{
          batch: {
            ...baseBatch,
            destinationProductHint: 'supplyarr',
            proposedRecordCount: 1,
          },
          files: [],
          classifications: [
            {
              classificationId: 'classification-part-1',
              destinationProduct: 'supplyarr',
              entityType: 'part',
              confidence: 94,
              requiresReview: true,
              reviewReasons: ['human_confirmation_required'],
            },
          ],
          proposedRecords: [
            {
              proposedRecordId: 'proposed-part-1',
              destinationProduct: 'supplyarr',
              entityType: 'part',
              operation: 'create',
              confidence: 94,
              reviewStatus: 'review_required',
              requiresReview: true,
              reviewReasons: ['human_confirmation_required'],
              proposedPayload: {
                sourceFields: {
                  'Part Number': 'FILT-2048',
                  Manufacturer: 'Acme',
                },
                proposedFields: {
                  displayName: 'Oil Filter',
                },
              },
            },
          ],
          commitPlans: [],
        }}
        onRefresh={vi.fn()}
        onSelectBatch={vi.fn()}
        onUpload={vi.fn()}
        onReview={vi.fn()}
        onApplyMappingOverride={vi.fn()}
        onCreateCommitPlan={vi.fn()}
      />,
    )

    const optionValues = Array.from(
      container.querySelectorAll('#smart-import-target-field-options option'),
    ).map((option) => (option as HTMLOptionElement).value)

    expect(optionValues).toEqual(
      expect.arrayContaining([
        'categoryKey',
        'description',
        'displayName',
        'manufacturerName',
        'manufacturerPartNumber',
        'notes',
        'partKey',
        'status',
        'unitOfMeasure',
      ]),
    )
  })

  it('offers StaffArr person-specific mapping targets for person imports', () => {
    const { container } = render(
      <SmartImportReviewWorkspace
        batches={[
          {
            ...baseBatch,
            destinationProductHint: 'staffarr',
            proposedRecordCount: 1,
          },
        ]}
        selectedBatch={{
          batch: {
            ...baseBatch,
            destinationProductHint: 'staffarr',
            proposedRecordCount: 1,
          },
          files: [],
          classifications: [
            {
              classificationId: 'classification-person-1',
              destinationProduct: 'staffarr',
              entityType: 'person',
              confidence: 96,
              requiresReview: true,
              reviewReasons: ['human_confirmation_required'],
            },
          ],
          proposedRecords: [
            {
              proposedRecordId: 'proposed-person-1',
              destinationProduct: 'staffarr',
              entityType: 'person',
              operation: 'create',
              confidence: 96,
              reviewStatus: 'review_required',
              requiresReview: true,
              reviewReasons: ['human_confirmation_required'],
              proposedPayload: {
                sourceFields: {
                  Employee: 'Avery Tech',
                  Email: 'avery@example.com',
                },
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
        onReview={vi.fn()}
        onApplyMappingOverride={vi.fn()}
        onCreateCommitPlan={vi.fn()}
      />,
    )

    const optionValues = Array.from(
      container.querySelectorAll('#smart-import-target-field-options option'),
    ).map((option) => (option as HTMLOptionElement).value)

    expect(optionValues).toEqual(
      expect.arrayContaining([
        'displayName',
        'email',
        'firstName',
        'lastName',
        'legalName',
        'personId',
        'personNumber',
        'preferredName',
        'status',
      ]),
    )
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
