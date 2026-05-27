import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { IncidentsPanel, isIncidentRoutableToTrainarr } from './IncidentsPanel'
import type { PersonnelIncidentDetailResponse } from '../api/types'
import type { PersonnelIncidentSummaryResponse } from '../api/types'

const sampleIncidents: PersonnelIncidentSummaryResponse[] = [
  {
    incidentId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    personId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    reasonCategoryKey: 'safety',
    severity: 'high',
    status: 'open',
    title: 'Forklift near-miss in warehouse aisle',
    occurredAt: '2026-05-26T14:30:00.000Z',
    reportedAt: '2026-05-26T15:00:00.000Z',
    reportedByUserId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    trainarrRouting: null,
  },
]

describe('IncidentsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders incident list and intake form for authorized users', () => {
    render(
      <IncidentsPanel
        personId={sampleIncidents[0].personId}
        personDisplayName="Alex Worker"
        incidents={sampleIncidents}
        selectedIncident={null}
        isLoading={false}
        isLoadingDetail={false}
        canManage
        isSubmitting={false}
        errorMessage={null}
        onSelectIncident={vi.fn()}
        onCreateIncident={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText(/Personnel incidents/i)).toBeTruthy()
    expect(screen.getByText(/Forklift near-miss in warehouse aisle/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Record incident/i })).toBeTruthy()
  })

  it('submits incident intake with person context', async () => {
    const onCreateIncident = vi.fn().mockResolvedValue(undefined)

    render(
      <IncidentsPanel
        personId={sampleIncidents[0].personId}
        personDisplayName="Alex Worker"
        incidents={[]}
        selectedIncident={null}
        isLoading={false}
        isLoadingDetail={false}
        canManage
        isSubmitting={false}
        errorMessage={null}
        onSelectIncident={vi.fn()}
        onCreateIncident={onCreateIncident}
      />,
    )

    fireEvent.change(screen.getByLabelText(/Title/i), {
      target: { value: 'Slip on loading dock' },
    })
    fireEvent.change(screen.getByLabelText(/Description/i), {
      target: {
        value: 'Employee slipped on wet surface during inbound shift; no injury reported but documented for safety review.',
      },
    })
    fireEvent.click(screen.getByRole('button', { name: /Record incident/i }))

    expect(onCreateIncident).toHaveBeenCalled()
    const payload = onCreateIncident.mock.calls[0][0]
    expect(payload.personId).toBe(sampleIncidents[0].personId)
    expect(payload.title).toBe('Slip on loading dock')
  })

  it('shows route button for training compliance incidents without routing', () => {
    const trainingIncident: PersonnelIncidentDetailResponse = {
      incidentId: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
      personId: sampleIncidents[0].personId,
      reasonCategoryKey: 'training_compliance',
      severity: 'high',
      status: 'open',
      title: 'Missed annual compliance training deadline',
      description:
        'Employee missed required annual compliance training deadline and cannot be assigned until remediated.',
      occurredAt: '2026-05-26T14:30:00.000Z',
      reportedAt: '2026-05-26T15:00:00.000Z',
      reportedByUserId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
      createdAt: '2026-05-26T15:00:00.000Z',
      updatedAt: '2026-05-26T15:00:00.000Z',
      trainarrRouting: null,
    }

    render(
      <IncidentsPanel
        personId={sampleIncidents[0].personId}
        personDisplayName="Alex Worker"
        incidents={[]}
        selectedIncident={trainingIncident}
        isLoading={false}
        isLoadingDetail={false}
        canManage
        isSubmitting={false}
        errorMessage={null}
        onSelectIncident={vi.fn()}
        onCreateIncident={vi.fn()}
        onRouteToTrainarr={vi.fn()}
      />,
    )

    expect(
      screen.getByRole('button', { name: /Route to TrainArr for remediation/i }),
    ).toBeTruthy()
    expect(isIncidentRoutableToTrainarr('training_compliance')).toBe(true)
    expect(isIncidentRoutableToTrainarr('safety')).toBe(false)
  })
})
