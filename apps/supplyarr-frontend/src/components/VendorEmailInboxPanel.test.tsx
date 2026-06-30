import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { VendorEmailInboxPanel } from './VendorEmailInboxPanel'

const mocks = vi.hoisted(() => ({
  getVendorEmailInbox: vi.fn().mockResolvedValue({
    items: [
      {
        messageId: 'message-1',
        messageKey: 'mail-001',
        messageKind: 'quote_received',
        senderEmail: 'vendor@example.com',
        senderName: 'Vendor Supply',
      subject: 'RFQ-001 quote attached',
      bodyPreview: 'Please see our quote attached.',
      matchStatus: 'matched',
      matchReason: 'matched explicit reference key',
      supplierId: 'vendor-1',
      supplierKey: 'ACME',
      supplierDisplayName: 'Acme Supply',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'ACME',
      vendorDisplayName: 'Acme Supply',
        linkedReferenceType: 'rfq',
        linkedReferenceId: 'rfq-1',
        linkedReferenceKey: 'RFQ-001',
        receivedAt: '2026-06-01T00:00:00Z',
        createdAt: '2026-06-01T00:00:00Z',
        updatedAt: '2026-06-01T00:00:00Z',
        processedAt: '2026-06-01T00:00:00Z',
      },
    ],
  }),
  ingestVendorEmailInbox: vi.fn().mockResolvedValue({
    wasDuplicate: false,
    message: {
      messageId: 'message-2',
      messageKey: 'mail-002',
      messageKind: 'order_confirmation_received',
      senderEmail: 'vendor@example.com',
      senderName: 'Vendor Supply',
      subject: 'PO-001 order confirmation',
      bodyPreview: 'Confirmed.',
      matchStatus: 'matched',
      matchReason: 'matched explicit reference key',
      supplierId: 'vendor-1',
      supplierKey: 'ACME',
      supplierDisplayName: 'Acme Supply',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'ACME',
      vendorDisplayName: 'Acme Supply',
      linkedReferenceType: 'purchase_order',
      linkedReferenceId: 'po-1',
      linkedReferenceKey: 'PO-001',
      receivedAt: '2026-06-01T00:00:00Z',
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
      processedAt: '2026-06-01T00:00:00Z',
    },
  }),
}))

vi.mock('../api/client', () => ({
  getVendorEmailInbox: mocks.getVendorEmailInbox,
  ingestVendorEmailInbox: mocks.ingestVendorEmailInbox,
}))

function renderPanel(canManage = true) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={client}>
      <VendorEmailInboxPanel accessToken="token" canManage={canManage} />
    </QueryClientProvider>,
  )
}

describe('VendorEmailInboxPanel', () => {
  it('renders and ingests supplier emails', async () => {
    renderPanel()

    expect(await screen.findByTestId('supplier-email-inbox-panel')).toBeInTheDocument()
    expect(await screen.findByText('RFQ-001 quote attached')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Message key'), { target: { value: 'mail-002' } })
    fireEvent.change(screen.getByLabelText('Message kind'), {
      target: { value: 'order_confirmation_received' },
    })
    fireEvent.change(screen.getByLabelText('Sender email'), { target: { value: 'vendor@example.com' } })
    fireEvent.change(screen.getByLabelText('Sender name'), { target: { value: 'Vendor Supply' } })
    fireEvent.change(screen.getByLabelText('Subject'), { target: { value: 'PO-001 order confirmation' } })
    fireEvent.change(screen.getByLabelText('Reference key'), { target: { value: 'PO-001' } })
    fireEvent.change(screen.getByLabelText('Email body'), { target: { value: 'Confirmed.' } })
    fireEvent.click(screen.getByRole('button', { name: 'Ingest email' }))

    await waitFor(() => expect(mocks.ingestVendorEmailInbox).toHaveBeenCalledTimes(1))
    expect(mocks.ingestVendorEmailInbox).toHaveBeenCalledWith('token', {
      messageKey: 'mail-002',
      messageKind: 'order_confirmation_received',
      senderEmail: 'vendor@example.com',
      senderName: 'Vendor Supply',
      subject: 'PO-001 order confirmation',
      body: 'Confirmed.',
      referenceKey: 'PO-001',
    })
    expect(await screen.findByText(/Last ingest created a new inbox record/i)).toBeInTheDocument()
  })

  it('returns null when user cannot manage', () => {
    const { container } = renderPanel(false)
    expect(container).toBeEmptyDOMElement()
  })
})
