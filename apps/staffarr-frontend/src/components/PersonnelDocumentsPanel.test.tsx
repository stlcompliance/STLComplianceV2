import { cleanup, render, screen } from '@testing-library/react'
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
    description: 'Initial employment contract',
    expiresAt: null,
    status: 'active',
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
        isLoadingDetail={false}
        canManage
        isSubmitting={false}
        errorMessage={null}
        onSelectDocument={vi.fn()}
        onUploadDocument={vi.fn().mockResolvedValue(undefined)}
        contentUrlFor={() => '/content'}
      />,
    )

    expect(screen.getByText(/Personnel documents/i)).toBeTruthy()
    expect(screen.getByText(/Signed offer letter/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Upload document/i })).toBeTruthy()
  })
})
