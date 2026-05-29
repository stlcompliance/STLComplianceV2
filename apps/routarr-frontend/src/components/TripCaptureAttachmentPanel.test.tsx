import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { TripCaptureAttachmentPanel } from './TripCaptureAttachmentPanel'

vi.mock('../api/client', () => ({
  uploadDriverPortalCaptureAttachment: vi.fn().mockResolvedValue({
    attachmentId: 'att-1',
    tripId: 'trip-1',
    subjectType: 'proof',
    subjectId: 'proof-1',
    attachmentKind: 'signature',
    fileName: 'signature.png',
    contentType: 'image/png',
    sizeBytes: 128,
    notes: null,
    capturedByPersonId: 'person-1',
    createdAt: new Date().toISOString(),
  }),
}))

describe('TripCaptureAttachmentPanel', () => {
  afterEach(() => cleanup())

  it('renders attachment controls for proof subject', () => {
    render(
      <TripCaptureAttachmentPanel
        accessToken="token"
        tripId="trip-1"
        subjectType="proof"
        subjectId="proof-1"
        subjectLabel="pickup proof"
        attachments={[]}
        onUploaded={() => undefined}
      />,
    )

    expect(screen.getByTestId('capture-attachments-proof-proof-1')).toBeTruthy()
    expect(screen.getByText(/pickup proof attachments/)).toBeTruthy()
    expect(screen.getByText('Photo')).toBeTruthy()
    expect(screen.getByText('Document')).toBeTruthy()
    expect(screen.getByTestId('signature-pad')).toBeTruthy()
  })
})
