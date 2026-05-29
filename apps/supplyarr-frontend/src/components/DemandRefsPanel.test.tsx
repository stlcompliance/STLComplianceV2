import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

import { DemandRefsPanel } from './DemandRefsPanel'

describe('DemandRefsPanel', () => {
  it('renders demand reference list and procurement journey', () => {
    render(
      <MemoryRouter>
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
              procurementStatus: 'pr_drafted',
              purchaseRequestId: 'pr-1',
              purchaseOrderId: null,
              lastStatusCallbackAt: '2026-05-27T13:00:00Z',
              receivedAt: '2026-05-27T12:00:00Z',
              updatedAt: '2026-05-27T13:00:00Z',
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
        />
      </MemoryRouter>,
    )

    expect(screen.getByText('WO-1001')).toBeInTheDocument()
    expect(screen.getByText(/Brake service/i)).toBeInTheDocument()
    expect(screen.getByTestId('demand-ref-procurement-journey')).toBeInTheDocument()
    expect(screen.getByTestId('demand-ref-open-pr')).toBeInTheDocument()
  })
})
