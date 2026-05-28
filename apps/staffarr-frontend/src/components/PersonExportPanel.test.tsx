import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PersonExportPanel } from './PersonExportPanel'
import { exportPeopleJson, getOrgUnits, getPersonExportPreset, upsertPersonExportPreset, upsertPersonExportSchedule } from '../api/client'

vi.mock('../api/client', () => ({
  getPeopleExportManifest: vi.fn().mockResolvedValue({
    packageVersion: '1',
    csvHeader: 'givenName,familyName,primaryEmail',
    formats: [{ key: 'csv', contentType: 'text/csv', fileName: 'people.csv', description: 'CSV' }],
  }),
  getOrgUnits: vi.fn().mockResolvedValue([
    {
      orgUnitId: '11111111-1111-1111-1111-111111111111',
      unitType: 'site',
      name: 'North Site',
      parentOrgUnitId: null,
      status: 'active',
    },
    {
      orgUnitId: '22222222-2222-2222-2222-222222222222',
      unitType: 'site',
      name: 'South Site',
      parentOrgUnitId: null,
      status: 'inactive',
    },
  ]),
  getPersonExportPreset: vi.fn().mockResolvedValue(null),
  getPersonExportSchedule: vi.fn().mockResolvedValue({
    isEnabled: false,
    intervalHours: 24,
    lastDeliveredAt: null,
    updatedAt: null,
  }),
  upsertPersonExportPreset: vi.fn().mockResolvedValue({
    employmentStatus: 'active',
    orgUnitId: '11111111-1111-1111-1111-111111111111',
    presetKey: 'active-at-org-unit',
    updatedAt: '2026-05-27T12:00:00Z',
  }),
  upsertPersonExportSchedule: vi.fn().mockResolvedValue({
    isEnabled: true,
    intervalHours: 12,
    lastDeliveredAt: null,
    updatedAt: '2026-05-27T12:00:00Z',
  }),
  exportPeopleCsv: vi.fn(),
  exportPeopleJson: vi.fn().mockResolvedValue({
    packageVersion: '1',
    generatedAt: '2026-05-27T12:00:00Z',
    personCount: 1,
    people: [],
  }),
  exportPeopleZip: vi.fn(),
}))

function renderPanel(canExport: boolean) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <PersonExportPanel accessToken="token" canExport={canExport} />
    </QueryClientProvider>,
  )
}

describe('PersonExportPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows read-only notice for non-writers', () => {
    renderPanel(false)
    expect(screen.getByText(/Person export requires tenant admin/i)).toBeTruthy()
    expect(screen.queryByRole('button', { name: /Download CSV/i })).toBeNull()
  })

  it('renders export controls for writers', async () => {
    renderPanel(true)
    expect(await screen.findByText(/Person export bundle/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Download CSV/i })).toBeTruthy()
    expect(screen.getByRole('button', { name: /Download ZIP bundle/i })).toBeTruthy()
    expect(screen.getByLabelText(/Primary org unit filter/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Active workforce/i })).toBeTruthy()
    expect((screen.getByRole('button', { name: /Active at org unit/i }) as HTMLButtonElement).disabled).toBe(true)
    expect(screen.getByRole('button', { name: /Save tenant default/i })).toBeTruthy()
  })

  it('loads tenant export preset on mount', async () => {
    vi.mocked(getPersonExportPreset).mockResolvedValueOnce({
      employmentStatus: 'inactive',
      orgUnitId: null,
      presetKey: 'inactive-records',
      updatedAt: '2026-05-27T12:00:00Z',
    })

    renderPanel(true)
    await waitFor(() => {
      expect(getPersonExportPreset).toHaveBeenCalledWith('token')
    })
    await waitFor(() => {
      expect(screen.getByText(/Filtering by status inactive/i)).toBeTruthy()
    })
  })

  it('saves tenant export preset from current filters', async () => {
    renderPanel(true)
    await screen.findByRole('option', { name: /North Site/i })

    fireEvent.click(screen.getByRole('button', { name: /Active workforce/i }))
    fireEvent.click(screen.getByRole('button', { name: /Save tenant default/i }))

    await waitFor(() => {
      expect(upsertPersonExportPreset).toHaveBeenCalledWith('token', {
        employmentStatus: 'active',
        orgUnitId: null,
        presetKey: 'active-workforce',
      })
    })
  })

  it('saves tenant export schedule', async () => {
    renderPanel(true)
    await screen.findByRole('button', { name: /Save schedule/i })

    const checkbox = screen.getByRole('checkbox', { name: /Enable scheduled delivery/i })
    fireEvent.click(checkbox)
    fireEvent.change(screen.getByLabelText(/Delivery interval \(hours\)/i), { target: { value: '12' } })
    fireEvent.click(screen.getByRole('button', { name: /Save schedule/i }))

    await waitFor(() => {
      expect(upsertPersonExportSchedule).toHaveBeenCalledWith('token', {
        isEnabled: true,
        intervalHours: 12,
      })
    })
  })

  it('applies active-at-org-unit preset and exports combined filters', async () => {
    renderPanel(true)
    await screen.findByRole('option', { name: /North Site/i })

    const orgUnitSelect = screen.getAllByRole('combobox')[1]
    fireEvent.change(orgUnitSelect, {
      target: { value: '11111111-1111-1111-1111-111111111111' },
    })
    await waitFor(() => {
      expect((orgUnitSelect as HTMLSelectElement).value).toBe('11111111-1111-1111-1111-111111111111')
    })

    fireEvent.click(screen.getByRole('button', { name: /Active at org unit/i }))
    await waitFor(() => {
      expect(screen.getByText(/Filtering by status active and org unit selected/i)).toBeTruthy()
    })

    fireEvent.click(screen.getByRole('button', { name: /Preview JSON export/i }))

    await waitFor(() => {
      expect(exportPeopleJson).toHaveBeenCalledWith('token', {
        employmentStatus: 'active',
        orgUnitId: '11111111-1111-1111-1111-111111111111',
      })
    })
  })

  it('passes org unit filter to JSON export', async () => {
    renderPanel(true)
    await screen.findByRole('option', { name: /North Site/i })

    const orgUnitSelect = screen.getAllByRole('combobox')[1]
    fireEvent.change(orgUnitSelect, {
      target: { value: '11111111-1111-1111-1111-111111111111' },
    })
    await waitFor(() => {
      expect((orgUnitSelect as HTMLSelectElement).value).toBe('11111111-1111-1111-1111-111111111111')
    })
    fireEvent.click(screen.getByRole('button', { name: /Preview JSON export/i }))

    await waitFor(() => {
      expect(exportPeopleJson).toHaveBeenCalledWith('token', {
        employmentStatus: undefined,
        orgUnitId: '11111111-1111-1111-1111-111111111111',
      })
    })
    expect(getOrgUnits).toHaveBeenCalledWith('token')
  })
})
