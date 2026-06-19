import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PersonnelDocumentsPanel } from './PersonnelDocumentsPanel'
import type { PersonnelDocumentSummaryResponse } from '../api/types'

const sampleDocuments: PersonnelDocumentSummaryResponse[] = [
  {
    documentId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    personId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    documentTypeKey: 'employment_contract',
    title: 'Signed offer letter',
    fileName: 'offer-letter.pdf',
    contentType: 'application/pdf',
    sizeBytes: 2048,
    accessLevel: 'manager',
    description: 'Initial employment contract',
    retentionCategory: 'personnel_file',
    expiresAt: null,
    status: 'active',
    restrictedData: false,
    uploadedByUserId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    createdAt: '2026-05-26T15:00:00.000Z',
    updatedAt: '2026-05-26T15:00:00.000Z',
  },
]

describe('PersonnelDocumentsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders document list and upload form for authorized users', () => {
    render(
      <PersonnelDocumentsPanel
        personId={sampleDocuments[0].personId}
        personDisplayName="Alex Worker"
        accessToken="token"
        documents={sampleDocuments}
        selectedDocument={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectDocument={vi.fn()}
        onUploadDocument={vi.fn().mockResolvedValue(undefined)}
        contentUrlFor={() => '/content'}
      />,
    )

    expect(screen.getByText(/Personnel documents/i)).toBeTruthy()
    expect(screen.getByText(/Signed offer letter/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Upload document/i })).toBeTruthy()
  })

  it('renders document action errors in shared callout', () => {
    render(
      <PersonnelDocumentsPanel
        personId={sampleDocuments[0].personId}
        personDisplayName="Alex Worker"
        accessToken="token"
        documents={sampleDocuments}
        selectedDocument={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage="Document upload failed"
        onSelectDocument={vi.fn()}
        onUploadDocument={vi.fn().mockResolvedValue(undefined)}
        contentUrlFor={() => '/content'}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Personnel document action failed')).toBeTruthy()
    expect(screen.getByText('Document upload failed')).toBeTruthy()
  })

  it('renders retryable read error callout when documents query fails', () => {
    const onRetry = vi.fn()
    render(
      <PersonnelDocumentsPanel
        personId={sampleDocuments[0].personId}
        personDisplayName="Alex Worker"
        accessToken="token"
        documents={[]}
        selectedDocument={null}
        isLoading={false}
        isError
        readErrorMessage="document list read failed"
        onRetryRead={onRetry}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectDocument={vi.fn()}
        onUploadDocument={vi.fn().mockResolvedValue(undefined)}
        contentUrlFor={() => '/content'}
      />,
    )

    expect(screen.getByText('Personnel documents unavailable')).toBeTruthy()
    expect(screen.getByText('document list read failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry documents' }))
    expect(onRetry).toHaveBeenCalledTimes(1)
  })

  it('renders retryable detail error callout when document detail query fails', () => {
    const onRetryDetail = vi.fn()
    render(
      <PersonnelDocumentsPanel
        personId={sampleDocuments[0].personId}
        personDisplayName="Alex Worker"
        accessToken="token"
        documents={sampleDocuments}
        selectedDocumentId={sampleDocuments[0].documentId}
        selectedDocument={{
          ...sampleDocuments[0],
        } as any}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError
        detailErrorMessage="document detail read failed"
        onRetryDetail={onRetryDetail}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectDocument={vi.fn()}
        onUploadDocument={vi.fn().mockResolvedValue(undefined)}
        contentUrlFor={() => '/content'}
      />,
    )

    expect(screen.getByText('Document detail unavailable')).toBeTruthy()
    expect(screen.getByText('document detail read failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry document detail' }))
    expect(onRetryDetail).toHaveBeenCalledTimes(1)
  })

  it('shows detail error callout when a document is selected but detail payload is null', () => {
    render(
      <PersonnelDocumentsPanel
        personId={sampleDocuments[0].personId}
        personDisplayName="Alex Worker"
        accessToken="token"
        documents={sampleDocuments}
        selectedDocumentId={sampleDocuments[0].documentId}
        selectedDocument={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError
        detailErrorMessage="document detail missing after read failure"
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectDocument={vi.fn()}
        onUploadDocument={vi.fn().mockResolvedValue(undefined)}
        contentUrlFor={() => '/content'}
      />,
    )

    expect(screen.getByText('Document detail unavailable')).toBeTruthy()
    expect(screen.getByText('document detail missing after read failure')).toBeTruthy()
  })
})
