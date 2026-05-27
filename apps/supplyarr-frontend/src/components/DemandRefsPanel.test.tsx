import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { DemandRefsPanel } from './DemandRefsPanel'

describe('DemandRefsPanel', () => {
  it('renders demand reference list', () => {
    render(
      <DemandRefsPanel
        demandRefs={[
          {
            demandRefId: 'ref-1',
            maintainarrPublicationId: 'pub-1',
            maintainarrWorkOrderId: 'wo-1',
            maintainarrWorkOrderNumber: 'WO-1001',
            maintainarrAssetId: 'asset-1',
            title: 'Brake service',
            notes: '',
            status: 'received',
            procurementStatus: 'received',
            purchaseRequestId: null,
            purchaseOrderId: null,
            lastStatusCallbackAt: null,
            receivedAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
            lines: [
              {
                lineId: 'line-1',
                lineNumber: 1,
                maintainarrDemandLineId: 'demand-1',
                partId: null,
                partNumber: 'BRK-001',
                description: '',
                quantityRequested: 2,
                unitOfMeasure: 'each',
                notes: '',
              },
            ],
          },
        ]}
        parts={[]}
        canCreatePurchaseRequest
        isLoading={false}
        selectedDemandRefId="ref-1"
        prRequestKey=""
        prTitle=""
        prNotes=""
        onSelectedDemandRefIdChange={vi.fn()}
        onPrRequestKeyChange={vi.fn()}
        onPrTitleChange={vi.fn()}
        onPrNotesChange={vi.fn()}
        onCreatePurchaseRequest={vi.fn()}
        isCreatingPurchaseRequest={false}
      />,
    )

    expect(screen.getByText('WO-1001')).toBeInTheDocument()
    expect(screen.getByText(/Brake service/i)).toBeInTheDocument()
  })
})
