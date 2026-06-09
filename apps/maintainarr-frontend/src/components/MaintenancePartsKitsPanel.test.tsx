import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

vi.mock('../api/client', () => ({
  getMaintenancePartsKits: vi.fn(),
  getMaintenancePartsKit: vi.fn(),
  createMaintenancePartsKit: vi.fn(),
  updateMaintenancePartsKit: vi.fn(),
  updateMaintenancePartsKitStatus: vi.fn(),
  createMaintenancePartsKitLine: vi.fn(),
  updateMaintenancePartsKitLine: vi.fn(),
  deleteMaintenancePartsKitLine: vi.fn(),
}))

const client = await import('../api/client')
const { MaintenancePartsKitsPanel } = await import('./MaintenancePartsKitsPanel')

function renderPanel() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <MemoryRouter>
      <QueryClientProvider client={queryClient}>
        <MaintenancePartsKitsPanel accessToken="token" canManage />
      </QueryClientProvider>
    </MemoryRouter>,
  )
}

describe('MaintenancePartsKitsPanel', () => {
  it('renders the kit registry and selected kit details', async () => {
    vi.mocked(client.getMaintenancePartsKits).mockResolvedValue({
      items: [
        {
          partsKitId: '11111111-1111-1111-1111-111111111111',
          kitNumber: 'KIT-001',
          title: 'Brake service kit',
          description: 'Standard brake service materials',
          assetTypeApplicability: ['truck'],
          workOrderTypeApplicability: ['corrective'],
          pmPlanRef: 'PM-1',
          status: 'active',
          lineRefs: ['22222222-2222-2222-2222-222222222222'],
          lines: [],
          createdAt: '2026-06-06T00:00:00Z',
          updatedAt: '2026-06-06T00:00:00Z',
        },
      ],
    })
    vi.mocked(client.getMaintenancePartsKit).mockResolvedValue({
      partsKitId: '11111111-1111-1111-1111-111111111111',
      kitNumber: 'KIT-001',
      title: 'Brake service kit',
      description: 'Standard brake service materials',
      assetTypeApplicability: ['truck'],
      workOrderTypeApplicability: ['corrective'],
      pmPlanRef: 'PM-1',
      status: 'active',
      lineRefs: ['22222222-2222-2222-2222-222222222222'],
      lines: [
        {
          partsKitLineId: '22222222-2222-2222-2222-222222222222',
          partsKitId: '11111111-1111-1111-1111-111111111111',
          itemRef: 'pad-set',
          itemDescriptionSnapshot: 'Brake pad set',
          quantity: 4,
          unitOfMeasure: 'each',
          required: true,
          substituteAllowed: false,
          createdAt: '2026-06-06T00:00:00Z',
          updatedAt: '2026-06-06T00:00:00Z',
        },
      ],
      createdAt: '2026-06-06T00:00:00Z',
      updatedAt: '2026-06-06T00:00:00Z',
    })

    renderPanel()

    expect(await screen.findByText('Maintenance parts kits')).toBeInTheDocument()
    expect(await screen.findByText('KIT-001')).toBeInTheDocument()
    expect(screen.getByText('Brake service kit')).toBeInTheDocument()
    expect(await screen.findByText('Brake pad set')).toBeInTheDocument()
  })
})
