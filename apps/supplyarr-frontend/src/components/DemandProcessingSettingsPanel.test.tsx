import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { DemandProcessingSettingsPanel } from './DemandProcessingSettingsPanel'

vi.mock('../api/client', () => ({
  getDemandProcessingSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    autoCreatePrDraftWhenShort: false,
    minHoursBeforeProcessing: 0,
    stalenessHours: 4,
    notifyOnPrDraftCreated: true,
    updatedAt: null,
  }),
  getPendingDemandProcessing: vi.fn().mockResolvedValue({ asOfUtc: '', stalenessHours: 4, batchSize: 25, items: [] }),
  getDemandProcessingRuns: vi.fn().mockResolvedValue({ items: [] }),
  upsertDemandProcessingSettings: vi.fn(),
}))

describe('DemandProcessingSettingsPanel', () => {
  it('renders settings panel when user can manage', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <DemandProcessingSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('demand-processing-settings-panel')).toBeInTheDocument()
    expect(screen.getByText('Demand processing worker')).toBeInTheDocument()
  })

  it('returns null when user cannot manage', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <DemandProcessingSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
