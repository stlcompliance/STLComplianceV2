import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

const session = {
  accessToken: 'token',
  personId: 'person-2',
}

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const mod = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...mod,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      testId,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      options: { value: string; label: string }[]
      testId?: string
    }) => (
      <label htmlFor={testId ?? 'mock-static-search-picker'}>
        {label}
        <input
          id={testId ?? 'mock-static-search-picker'}
          aria-label={label}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <ul>
          {options.map((option) => (
            <li key={option.value}>{option.label}</li>
          ))}
        </ul>
      </label>
    ),
  }
})

vi.mock('../../auth/sessionStorage', () => ({
  loadSession: vi.fn(() => session),
}))

vi.mock('../../api/client', () => ({
  createPersonnelIncident: vi.fn(),
  getOrgUnits: vi.fn(),
  getPeople: vi.fn(),
}))

import * as client from '../../api/client'
import { IncidentCreatePage } from './IncidentCreatePage'

describe('IncidentCreatePage', () => {
  afterEach(() => {
    cleanup()
  })

  it('submits a payload using searchable people and org-unit pickers', async () => {
    vi.mocked(client.getPeople).mockResolvedValue([
      {
        personId: 'person-1',
        externalUserId: null,
        displayName: 'Alex Worker',
        primaryEmail: 'alex@example.com',
        employmentStatus: 'active',
        primaryOrgUnitId: 'site-1',
        primaryOrgUnitName: 'North Site',
        managerPersonId: 'person-3',
        jobTitle: 'Technician',
      },
      {
        personId: 'person-2',
        externalUserId: null,
        displayName: 'Taylor Reporter',
        primaryEmail: 'taylor@example.com',
        employmentStatus: 'active',
        primaryOrgUnitId: 'site-2',
        primaryOrgUnitName: 'South Site',
        managerPersonId: 'person-3',
        jobTitle: 'Supervisor',
      },
      {
        personId: 'person-3',
        externalUserId: null,
        displayName: 'Morgan Manager',
        primaryEmail: 'morgan@example.com',
        employmentStatus: 'active',
        primaryOrgUnitId: 'site-1',
        primaryOrgUnitName: 'North Site',
        managerPersonId: null,
        jobTitle: 'Manager',
      },
    ])
    vi.mocked(client.getOrgUnits).mockResolvedValue([
      {
        orgUnitId: 'site-1',
        unitType: 'site',
        name: 'North Site',
        parentOrgUnitId: null,
        status: 'active',
      },
      {
        orgUnitId: 'dept-1',
        unitType: 'department',
        name: 'Operations',
        parentOrgUnitId: 'site-1',
        status: 'active',
      },
    ])
    vi.mocked(client.createPersonnelIncident).mockResolvedValue({
      incidentId: 'incident-1',
      personId: 'person-1',
      reasonCategoryKey: 'safety',
      severity: 'medium',
      status: 'submitted',
      title: 'Forklift near miss',
      occurredAt: '2026-06-01T00:00:00Z',
      reportedAt: '2026-06-01T00:00:00Z',
      reportedByUserId: 'person-2',
      trainarrRouting: null,
      incidentSource: 'staffarr',
      incidentType: 'safety',
      readinessDecision: 'allowed',
      trainingReviewRequired: false,
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
      description: 'A forklift came close to a pedestrian in the loading area.',
      siteOrgUnitId: 'site-1',
      departmentOrgUnitId: 'dept-1',
      locationDetail: 'Dock 4',
      witnessPersonIds: [],
      additionalInvolvedPersonIds: [],
      employeeSelfReport: false,
      immediateActionsTaken: 'Area secured and supervisor notified.',
      rootCause: null,
      workRestriction: null,
      returnToWorkNeeded: null,
      ppeConcern: null,
      medicalAttention: null,
      outOfServiceRemoveFromDuty: null,
      followUpRequired: null,
      trainingReviewReason: null,
      relatedAssetReference: null,
      relatedWorkOrderReference: null,
      relatedRouteReference: null,
      relatedSupplierReference: null,
      relatedDocumentReference: null,
      relatedPolicyReference: null,
      evidencePackageRequested: false,
      notifyManager: true,
      notifySafetyCompliance: true,
      notifyHr: false,
      createFollowUpTask: false,
    })

    const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <MemoryRouter>
        <QueryClientProvider client={qc}>
          <IncidentCreatePage />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Create Incident' })).toBeTruthy()

    fireEvent.change(screen.getByLabelText(/Title/i), {
      target: { value: 'Forklift near miss' },
    })
    fireEvent.change(screen.getByTestId('incident-affected-person-picker'), {
      target: { value: 'person-1' },
    })
    fireEvent.change(screen.getByTestId('incident-reporter-picker'), {
      target: { value: 'person-2' },
    })
    fireEvent.change(screen.getByTestId('incident-manager-picker'), {
      target: { value: 'person-3' },
    })
    fireEvent.change(screen.getByTestId('incident-site-picker'), {
      target: { value: 'site-1' },
    })
    fireEvent.change(screen.getByTestId('incident-department-picker'), {
      target: { value: 'dept-1' },
    })
    fireEvent.change(screen.getByLabelText(/Location detail/i), {
      target: { value: 'Dock 4' },
    })
    fireEvent.change(screen.getByLabelText(/What happened\?/i), {
      target: { value: 'A forklift came close to a pedestrian in the loading area.' },
    })
    fireEvent.change(screen.getByLabelText(/Immediate actions taken/i), {
      target: { value: 'Area secured and supervisor notified.' },
    })
    fireEvent.click(screen.getAllByRole('button', { name: 'Submit Incident' })[0])

    await waitFor(() => {
      expect(client.createPersonnelIncident).toHaveBeenCalledWith(
        'token',
        expect.objectContaining({
          personId: 'person-1',
          reporterPersonId: 'person-2',
          managerPersonId: 'person-3',
          siteOrgUnitId: 'site-1',
          departmentOrgUnitId: 'dept-1',
          title: 'Forklift near miss',
        }),
      )
    })
  })
})
