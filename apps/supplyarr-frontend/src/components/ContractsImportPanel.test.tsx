import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ContractsImportPanel } from './ContractsImportPanel'
import * as api from '../api/client'

vi.mock('../api/client', async () => {
  const actual = await vi.importActual<typeof import('../api/client')>('../api/client')
  return {
    ...actual,
    importContractsCsv: vi.fn(),
  }
})

afterEach(() => {
  cleanup()
})

function renderPanel(canManage = true) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <ContractsImportPanel accessToken="token" canManage={canManage} />
    </QueryClientProvider>,
  )
}

describe('ContractsImportPanel', () => {
  it('runs a dry-run import and renders validation issues', async () => {
    vi.mocked(api.importContractsCsv).mockResolvedValueOnce({
      importType: 'contracts_csv',
      dryRun: true,
      succeeded: false,
      rowsRead: 1,
      contractsAccepted: 1,
      contractsCreated: 0,
      issues: [
        {
          lineNumber: 2,
          code: 'contract.invalid_status',
          message: 'status is not supported.',
        },
      ],
    })

    renderPanel()

    fireEvent.change(screen.getByLabelText('CSV data'), {
      target: {
        value:
          'vendor_party_key,contract_key,contract_type,title,effective_at,expires_at,renewal_at,payment_terms,freight_terms,warranty_terms,minimum_spend,service_level_agreement,approval_status,status,notes\nSUP-2048,SC-2048,master_supply_agreement,Supply Agreement 2026,2026-01-15T00:00:00Z,2026-12-31T00:00:00Z,2026-11-01T00:00:00Z,Net 30,FOB destination,12 months from receipt,25000,95% on-time shipment rate,approved,active,Priority partner contract',
      },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Run validation' }))

    await waitFor(() => expect(api.importContractsCsv).toHaveBeenCalledTimes(1))
    expect(api.importContractsCsv).toHaveBeenCalledWith(
      'token',
      expect.objectContaining({
        dryRun: true,
        csv: expect.stringContaining('SC-2048'),
      }),
    )

    expect(await screen.findByText('Rows read: 1')).toBeInTheDocument()
    expect(screen.getByText('Import issues found')).toBeInTheDocument()
    expect(screen.getByText('Line 2')).toBeInTheDocument()
    expect(screen.getByText('status is not supported.')).toBeInTheDocument()
  })

  it('hides when the user cannot manage contracts imports', () => {
    renderPanel(false)
    expect(screen.queryByTestId('supplyarr-contract-import-panel')).not.toBeInTheDocument()
  })
})
