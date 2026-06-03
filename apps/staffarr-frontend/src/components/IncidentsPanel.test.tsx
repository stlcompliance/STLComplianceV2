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
        selectedIncidentId={null}
        selectedIncident={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
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
        selectedIncidentId={null}
        selectedIncident={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
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
        selectedIncidentId={trainingIncident.incidentId}
        selectedIncident={trainingIncident}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
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

  it('renders close and reopen incident actions from the selected detail', () => {
    const onUpdateIncidentStatus = vi.fn().mockResolvedValue(undefined)
    const openIncident: PersonnelIncidentDetailResponse = {
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
        selectedIncidentId={openIncident.incidentId}
        selectedIncident={openIncident}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectIncident={vi.fn()}
        onCreateIncident={vi.fn()}
        onRouteToTrainarr={vi.fn()}
        onUpdateIncidentStatus={onUpdateIncidentStatus}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Close incident' }))
    expect(onUpdateIncidentStatus).toHaveBeenCalledWith(openIncident.incidentId, 'closed')

    cleanup()

    render(
      <IncidentsPanel
        personId={sampleIncidents[0].personId}
        personDisplayName="Alex Worker"
        incidents={[]}
        selectedIncidentId={openIncident.incidentId}
        selectedIncident={{ ...openIncident, status: 'closed' }}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectIncident={vi.fn()}
        onCreateIncident={vi.fn()}
        onRouteToTrainarr={vi.fn()}
        onUpdateIncidentStatus={onUpdateIncidentStatus}
      />,
    )

    expect(screen.getByRole('button', { name: 'Reopen incident' })).toBeTruthy()
  })

  it('renders cross-product source references when present', () => {
    const routedIncident: PersonnelIncidentDetailResponse = {
      incidentId: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
      personId: sampleIncidents[0].personId,
      reasonCategoryKey: 'training_compliance',
      severity: 'medium',
      status: 'open',
      title: 'External product incident',
      description: 'Imported from RoutArr with a source snapshot.',
      occurredAt: '2026-05-26T14:30:00.000Z',
      reportedAt: '2026-05-26T15:00:00.000Z',
      reportedByUserId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
      createdAt: '2026-05-26T15:00:00.000Z',
      updatedAt: '2026-05-26T15:00:00.000Z',
      sourceProduct: 'routarr',
      sourceIncidentId: 'route-inc-123',
      sourceEventKind: 'driver_incident',
      sourceReferenceKey: 'route-incident-ref-1',
      sourceSnapshot: {
        sourceProduct: 'routarr',
        sourceEntity: 'incident',
        sourceId: 'route-inc-123',
        labelSnapshot: 'Driver near miss',
        statusSnapshot: 'open',
        selectedAt: '2026-05-26T14:30:00.000Z',
        lastVerifiedAt: '2026-05-26T15:00:00.000Z',
        lastSyncedAt: '2026-05-26T15:00:00.000Z',
        isAuthoritative: true,
      },
      relatedRouteReference: 'Route 47',
      relatedDocumentReference: 'doc-1',
      trainarrRouting: null,
    }

    render(
      <IncidentsPanel
        personId={sampleIncidents[0].personId}
        personDisplayName="Alex Worker"
        incidents={[]}
        selectedIncidentId={routedIncident.incidentId}
        selectedIncident={routedIncident}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage={false}
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectIncident={vi.fn()}
        onCreateIncident={vi.fn()}
      />,
    )

    expect(screen.getByText('Source references')).toBeTruthy()
    expect(screen.getByText('routarr')).toBeTruthy()
    expect(screen.getByText('route-inc-123')).toBeTruthy()
    expect(screen.getByText(/Driver near miss/i)).toBeTruthy()
    expect(screen.getByText('Route 47')).toBeTruthy()
  })

  it('renders incident notes and attachments and drives the new actions', () => {
    const onCreateIncidentNote = vi.fn().mockResolvedValue(undefined)
    const onUpdateIncidentNoteStatus = vi.fn().mockResolvedValue(undefined)
    const onDownloadIncidentAttachment = vi.fn().mockResolvedValue(undefined)

    const incident: PersonnelIncidentDetailResponse = {
      incidentId: 'ffffeeee-dddd-cccc-bbbb-aaaaaaaaaaaa',
      personId: sampleIncidents[0].personId,
      reasonCategoryKey: 'safety',
      severity: 'medium',
      status: 'open',
      title: 'Broken dock light',
      description: 'A dock light was reported broken and requires follow-up maintenance.',
      occurredAt: '2026-05-26T14:30:00.000Z',
      reportedAt: '2026-05-26T15:00:00.000Z',
      reportedByUserId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
      createdAt: '2026-05-26T15:00:00.000Z',
      updatedAt: '2026-05-26T15:00:00.000Z',
      trainarrRouting: null,
      notes: [
        {
          noteId: '11111111-2222-3333-4444-555555555555',
          incidentId: 'ffffeeee-dddd-cccc-bbbb-aaaaaaaaaaaa',
          noteTypeKey: 'corrective_action',
          subject: 'Replace dock light',
          body: 'Maintenance should replace the dock light and confirm illumination.',
          status: 'open',
          dueAt: '2026-05-27T12:00:00.000Z',
          completedAt: null,
          createdByUserId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
          createdAt: '2026-05-26T16:00:00.000Z',
          updatedAt: '2026-05-26T16:00:00.000Z',
        },
      ],
      attachments: [
        {
          attachmentId: '66666666-7777-8888-9999-000000000000',
          incidentId: 'ffffeeee-dddd-cccc-bbbb-aaaaaaaaaaaa',
          title: 'Broken dock light photo',
          fileName: 'dock-light.jpg',
          contentType: 'image/jpeg',
          sizeBytes: 2048,
          description: 'Photo from the shift lead.',
          uploadedByUserId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
          createdAt: '2026-05-26T16:30:00.000Z',
          updatedAt: '2026-05-26T16:30:00.000Z',
        },
      ],
    }

    render(
      <IncidentsPanel
        personId={sampleIncidents[0].personId}
        personDisplayName="Alex Worker"
        incidents={[]}
        selectedIncidentId={incident.incidentId}
        selectedIncident={incident}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectIncident={vi.fn()}
        onCreateIncident={vi.fn()}
        onCreateIncidentNote={onCreateIncidentNote}
        onUpdateIncidentNoteStatus={onUpdateIncidentNoteStatus}
        onCreateIncidentAttachment={vi.fn()}
        onDownloadIncidentAttachment={onDownloadIncidentAttachment}
      />,
    )

    expect(screen.getByText(/Notes and corrective actions/i)).toBeTruthy()
    expect(screen.getByText(/Attachments/i)).toBeTruthy()

    fireEvent.change(screen.getByLabelText(/^Subject$/i), {
      target: { value: 'Check lighting replacement' },
    })
    fireEvent.change(screen.getByLabelText(/^Body$/i), {
      target: { value: 'Document that maintenance will replace the dock light before next shift.' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Add note/i }))
    expect(onCreateIncidentNote).toHaveBeenCalled()

    fireEvent.click(screen.getByRole('button', { name: /Mark complete/i }))
    expect(onUpdateIncidentNoteStatus).toHaveBeenCalledWith(
      incident.incidentId,
      incident.notes![0].noteId,
      { status: 'completed' },
    )

    fireEvent.click(screen.getByRole('button', { name: /Download/i }))
    expect(onDownloadIncidentAttachment).toHaveBeenCalledWith(
      incident.incidentId,
      incident.attachments![0].attachmentId,
    )
  })

  it('renders incident action errors in shared callout', () => {
    render(
      <IncidentsPanel
        personId={sampleIncidents[0].personId}
        personDisplayName="Alex Worker"
        incidents={sampleIncidents}
        selectedIncidentId={null}
        selectedIncident={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage="Incident intake failed"
        onSelectIncident={vi.fn()}
        onCreateIncident={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Personnel incident action failed')).toBeTruthy()
    expect(screen.getByText('Incident intake failed')).toBeTruthy()
  })

  it('renders retryable read error callout when incidents query fails', () => {
    const onRetryRead = vi.fn()
    render(
      <IncidentsPanel
        personId={sampleIncidents[0].personId}
        personDisplayName="Alex Worker"
        incidents={[]}
        selectedIncidentId={null}
        selectedIncident={null}
        isLoading={false}
        isError
        readErrorMessage="incidents read failed"
        onRetryRead={onRetryRead}
        isLoadingDetail={false}
        isDetailError={false}
        detailErrorMessage={null}
        onRetryDetail={vi.fn()}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectIncident={vi.fn()}
        onCreateIncident={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Personnel incidents unavailable')).toBeTruthy()
    expect(screen.getByText('incidents read failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry incidents' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })

  it('renders detail error when selected incident detail query fails with null payload', () => {
    const onRetryDetail = vi.fn()
    render(
      <IncidentsPanel
        personId={sampleIncidents[0].personId}
        personDisplayName="Alex Worker"
        incidents={sampleIncidents}
        selectedIncidentId={sampleIncidents[0].incidentId}
        selectedIncident={null}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isLoadingDetail={false}
        isDetailError
        detailErrorMessage="incident detail read failed"
        onRetryDetail={onRetryDetail}
        canManage
        isSubmitting={false}
        actionErrorMessage={null}
        onSelectIncident={vi.fn()}
        onCreateIncident={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Incident detail unavailable')).toBeTruthy()
    expect(screen.getByText('incident detail read failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry incident detail' }))
    expect(onRetryDetail).toHaveBeenCalledTimes(1)
  })
})
