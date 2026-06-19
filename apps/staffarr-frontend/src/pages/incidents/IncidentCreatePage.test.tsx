import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

const session = {
  accessToken: 'token',
  personId: 'person-2',
}

vi.mock('@stl/shared-ui', () => ({
  ApiErrorCallout: ({ message }: { message: string }) => <div>{message}</div>,
  ReferenceProviderClient: class ReferenceProviderClient {
    constructor() {}
  },
  ReferencePicker: ({
    referenceType,
    value,
    onChange,
    testId,
  }: {
    referenceType: string
    value: { referenceId: string } | null
    onChange: (value: unknown | null) => void
    testId?: string
  }) => (
    <input
      data-testid={testId}
      value={value?.referenceId ?? ''}
      onChange={(event) => {
        const referenceId = event.target.value
        if (!referenceId) {
          onChange(null)
          return
        }

        onChange({
          ownerProductKey: referenceType === 'asset' ? 'maintainarr' : 'supplyarr',
          referenceType,
          referenceId,
          displayLabelSnapshot: referenceType === 'asset' ? 'Forklift A' : 'Acme Supply Co',
          secondaryLabelSnapshot: referenceType === 'asset' ? 'ASSET-001' : 'SUP-001',
          statusSnapshot: 'active',
          ownerVersion: '2026-06-17T00:00:00Z',
          createdVia: 'selected',
        })
      }}
    />
  ),
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
  getErrorMessage: (error: unknown, fallback: string) =>
    error instanceof Error && error.message ? error.message : fallback,
}))

vi.mock('../../auth/sessionStorage', () => ({
  loadSession: vi.fn(() => session),
}))

vi.mock('../../api/client', () => ({
  createPersonnelIncident: vi.fn(),
  getStaffArrFieldset: vi.fn().mockResolvedValue({
    key: 'personnel-incidents.create',
    label: 'Personnel incident create',
    entityType: 'personnel_incident',
    purpose: 'create',
    fields: [
      {
        key: 'incidentSource',
        label: 'Incident source',
        control: 'select',
        required: true,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'staffarr', label: 'StaffArr incident intake', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
        ],
      },
      {
        key: 'incidentType',
        label: 'Incident type',
        control: 'select',
        required: true,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'safety', label: 'Safety', hint: null, owner: 'compliancecore', sourceOfTruth: 'compliancecore.mapped_staffarr_fieldset' },
          { value: 'training_issue', label: 'Training issue', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
        ],
      },
      {
        key: 'severity',
        label: 'Severity',
        control: 'select',
        required: true,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'medium', label: 'Medium', hint: null, owner: 'compliancecore', sourceOfTruth: 'compliancecore.mapped_staffarr_fieldset' },
        ],
      },
      {
        key: 'readinessDecision',
        label: 'Readiness decision',
        control: 'select',
        required: true,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'allowed', label: 'Allowed', hint: 'May continue normal work', owner: 'compliancecore', sourceOfTruth: 'compliancecore.mapped_staffarr_fieldset' },
          { value: 'watched', label: 'Watched', hint: 'Needs watch or monitoring', owner: 'compliancecore', sourceOfTruth: 'compliancecore.mapped_staffarr_fieldset' },
          { value: 'restricted', label: 'Restricted', hint: 'Must be restricted or limited', owner: 'compliancecore', sourceOfTruth: 'compliancecore.mapped_staffarr_fieldset' },
        ],
      },
      {
        key: 'workRestriction',
        label: 'Work restriction',
        control: 'select',
        required: true,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'none', label: 'None', hint: null, owner: 'compliancecore', sourceOfTruth: 'compliancecore.mapped_staffarr_fieldset' },
        ],
      },
      {
        key: 'yesNoPending',
        label: 'Yes/no/pending',
        control: 'select',
        required: false,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'no', label: 'No', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
          { value: 'yes', label: 'Yes', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
          { value: 'pending', label: 'Pending', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
        ],
      },
      {
        key: 'ppeConcern',
        label: 'PPE concern',
        control: 'select',
        required: false,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'none', label: 'None', hint: null, owner: 'compliancecore', sourceOfTruth: 'compliancecore.mapped_staffarr_fieldset' },
        ],
      },
      {
        key: 'medicalAttention',
        label: 'Medical attention',
        control: 'select',
        required: false,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'none', label: 'None', hint: null, owner: 'compliancecore', sourceOfTruth: 'compliancecore.mapped_staffarr_fieldset' },
        ],
      },
      {
        key: 'followUpRequired',
        label: 'Follow-up required',
        control: 'select',
        required: true,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'conditional', label: 'Conditional', hint: null, owner: 'staffarr', sourceOfTruth: 'staffarr.fieldset' },
        ],
      },
      {
        key: 'trainingReviewReason',
        label: 'Training review reason',
        control: 'select',
        required: false,
        owner: 'staffarr',
        sourceOfTruth: 'staffarr.fieldset',
        options: [
          { value: 'certification_gap', label: 'Certification gap', hint: null, owner: 'compliancecore', sourceOfTruth: 'compliancecore.mapped_staffarr_fieldset' },
        ],
      },
    ],
  }),
  getMaintainArrAssetReferences: vi.fn(),
  getMaintainArrWorkOrderReferences: vi.fn(),
  getOrgUnits: vi.fn(),
  getPeople: vi.fn(),
  getRecordArrControlledDocumentReferences: vi.fn(),
  getRoutArrRouteReferences: vi.fn(),
  getSupplyArrSupplierReferences: vi.fn(),
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
        givenName: 'Alex',
        familyName: 'Worker',
        displayName: 'Alex Worker',
        primaryEmail: 'alex@example.com',
        employmentStatus: 'active',
        primaryOrgUnitId: 'dept-1',
        primaryOrgUnitName: 'Operations',
        managerPersonId: null,
        jobTitle: 'Technician',
      },
      {
        personId: 'person-2',
        externalUserId: null,
        givenName: 'Taylor',
        familyName: 'Reporter',
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
        givenName: 'Morgan',
        familyName: 'Manager',
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
        managerPersonId: 'person-3',
        status: 'active',
      },
    ])
    vi.mocked(client.getMaintainArrAssetReferences).mockResolvedValue([
      {
        assetId: 'asset-1',
        assetTag: 'ASSET-001',
        name: 'Forklift A',
        lifecycleStatus: 'active',
      },
    ])
    vi.mocked(client.getMaintainArrWorkOrderReferences).mockResolvedValue([
      {
        workOrderId: 'wo-1',
        workOrderNumber: 'WO-1001',
        title: 'Forklift inspection',
        status: 'open',
      },
    ])
    vi.mocked(client.getRoutArrRouteReferences).mockResolvedValue([
      {
        routeId: 'route-1',
        routeNumber: 'R-47',
        title: 'Dock transfer route',
        routeStatus: 'active',
      },
    ])
    vi.mocked(client.getSupplyArrSupplierReferences).mockResolvedValue([
      {
        partyId: 'supplier-1',
        partyKey: 'SUP-001',
        displayName: 'Acme Supply Co',
        legalName: 'Acme Supply Company LLC',
        status: 'active',
      },
    ])
    vi.mocked(client.getRecordArrControlledDocumentReferences).mockResolvedValue([
      {
        controlledDocumentId: 'doc-1',
        documentNumber: 'POL-001',
        title: 'Forklift safety policy',
        controlledDocumentType: 'policy',
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
    await waitFor(() => {
      expect(screen.getByTestId('incident-manager-picker').textContent).toContain('Morgan Manager')
    })
    fireEvent.change(screen.getByTestId('incident-reporter-picker'), {
      target: { value: 'person-2' },
    })
    fireEvent.change(screen.getByTestId('incident-site-picker'), {
      target: { value: 'site-1' },
    })
    fireEvent.change(screen.getByTestId('incident-department-picker'), {
      target: { value: 'dept-1' },
    })
    fireEvent.change(screen.getByTestId('incident-asset-reference-picker'), {
      target: { value: 'asset-1' },
    })
    fireEvent.change(screen.getByTestId('incident-work-order-reference-picker'), {
      target: { value: 'wo-1' },
    })
    fireEvent.change(screen.getByTestId('incident-route-reference-picker'), {
      target: { value: 'route-1' },
    })
    fireEvent.change(screen.getByTestId('incident-supplier-reference-picker'), {
      target: { value: 'supplier-1' },
    })
    fireEvent.change(screen.getByTestId('incident-document-reference-picker'), {
      target: { value: 'doc-1' },
    })
    fireEvent.change(screen.getByTestId('incident-policy-reference-picker'), {
      target: { value: 'doc-1' },
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
          relatedWorkOrderReference: 'wo-1',
          relatedRouteReference: 'route-1',
          relatedDocumentReference: 'doc-1',
          relatedPolicyReference: 'doc-1',
          personId: 'person-1',
          reporterPersonId: 'person-2',
          managerPersonId: 'person-3',
          siteOrgUnitId: 'site-1',
          departmentOrgUnitId: 'dept-1',
          title: 'Forklift near miss',
        }),
      )
    })

    const payload = vi.mocked(client.createPersonnelIncident).mock.calls[0][1]
    expect(JSON.parse(payload.relatedAssetReference!)).toMatchObject({
      ownerProductKey: 'maintainarr',
      referenceType: 'asset',
      referenceId: 'asset-1',
      displayLabelSnapshot: 'Forklift A',
    })
    expect(JSON.parse(payload.relatedSupplierReference!)).toMatchObject({
      ownerProductKey: 'supplyarr',
      referenceType: 'supplier',
      referenceId: 'supplier-1',
      displayLabelSnapshot: 'Acme Supply Co',
    })
  })

  it('prefills the affected person from the person-scoped create link', async () => {
    vi.mocked(client.getPeople).mockResolvedValue([
      {
        personId: 'person-1',
        externalUserId: null,
        givenName: 'Alex',
        familyName: 'Worker',
        displayName: 'Alex Worker',
        primaryEmail: 'alex@example.com',
        employmentStatus: 'active',
        primaryOrgUnitId: 'dept-1',
        primaryOrgUnitName: 'Operations',
        managerPersonId: null,
        jobTitle: 'Technician',
      },
      {
        personId: 'person-3',
        externalUserId: null,
        givenName: 'Morgan',
        familyName: 'Manager',
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
        managerPersonId: 'person-3',
        status: 'active',
      },
    ])

    const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <MemoryRouter initialEntries={['/incidents/create?personId=person-1']}>
        <QueryClientProvider client={qc}>
          <IncidentCreatePage />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    await waitFor(() => {
      expect((screen.getByTestId('incident-affected-person-picker') as HTMLInputElement).value).toBe('person-1')
    })
    expect(screen.getByTestId('incident-manager-picker').textContent).toContain('Morgan Manager')
    expect(screen.getByTestId('incident-manager-picker').textContent).toContain('Operations hierarchy')
  })
})
