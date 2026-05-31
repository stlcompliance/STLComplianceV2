import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { HomePage } from './HomePage'

const mocked = vi.hoisted(() => ({
  loadSession: vi.fn(),
  getMe: vi.fn(),
  getPeople: vi.fn(),
  getOrgUnits: vi.fn(),
  getPerson: vi.fn(),
  getPersonLookup: vi.fn(),
  getPersonHistorySummary: vi.fn(),
  getWorkforceOnboardingJourney: vi.fn(),
  getPersonOffboarding: vi.fn(),
  updatePerson: vi.fn(),
  updatePersonEmploymentStatus: vi.fn(),
  startPersonOffboarding: vi.fn(),
  executePersonOffboarding: vi.fn(),
}))

vi.mock('../auth/sessionStorage', () => ({
  loadSession: mocked.loadSession,
  clearSession: vi.fn(),
  canExportAuditPackage: vi.fn(() => false),
  canReadReports: vi.fn(() => false),
}))

vi.mock('@stl/shared-ui', () => ({
  ApiErrorCallout: ({ title, message }: { title: string; message: string }) => (
    <div role="alert">
      <p>{title}</p>
      <p>{message}</p>
    </div>
  ),
  PageHeader: ({ title }: { title: string }) => <h1>{title}</h1>,
  getErrorMessage: (error: unknown, fallback = 'Something went wrong.') =>
    error instanceof Error ? error.message : fallback,
}))

vi.mock('../components/PersonProfileEditorPanel', () => ({
  canManagePeople: vi.fn(() => true),
  PersonProfileEditorPanel: ({
    onUpdate,
    onEmploymentStatusChange,
  }: {
    onUpdate: (request: any) => Promise<void>
    onEmploymentStatusChange: (request: any) => Promise<void>
  }) => (
    <>
      <button
        type="button"
        onClick={() =>
          onUpdate({
            givenName: 'Alex',
            familyName: 'Rivera',
            primaryEmail: 'alex.rivera@example.com',
            primaryOrgUnitId: null,
            managerPersonId: null,
            jobTitle: 'Operator',
          })
        }
      >
        Trigger profile update
      </button>
      <button
        type="button"
        onClick={() =>
          onEmploymentStatusChange({
            employmentStatus: 'inactive',
            reason: 'Test change',
          })
        }
      >
        Trigger employment update
      </button>
    </>
  ),
}))

vi.mock('../components/CreatePersonPanel', () => ({ CreatePersonPanel: () => null }))
vi.mock('../components/CertificationPanel', () => ({ CertificationPanel: () => null }))
vi.mock('../components/AuditPackageExportPanel', () => ({ AuditPackageExportPanel: () => null }))
vi.mock('../components/PersonBulkImportPanel', () => ({ PersonBulkImportPanel: () => null }))
vi.mock('../components/PersonExportPanel', () => ({ PersonExportPanel: () => null }))
vi.mock('../components/IncidentsPanel', () => ({
  canManageIncidents: vi.fn(() => false),
  IncidentsPanel: () => null,
}))
vi.mock('../components/ReadinessPanel', () => ({
  canOverrideReadiness: vi.fn(() => false),
  ReadinessPanel: () => null,
}))
vi.mock('../components/ReadinessRollupSupervisorPanel', () => ({
  canViewReadinessRollups: vi.fn(() => false),
  ReadinessRollupSupervisorPanel: () => null,
}))
vi.mock('../components/ManagerHierarchyPanel', () => ({ ManagerHierarchyPanel: () => null }))
vi.mock('../components/OrgHierarchyManager', () => ({
  canManageOrgHierarchy: vi.fn(() => false),
  OrgHierarchyManager: () => null,
}))
vi.mock('../components/PersonOrgAssignmentsManager', () => ({ PersonOrgAssignmentsManager: () => null }))
vi.mock('../components/PermissionProjectionTimelinePanel', () => ({ PermissionProjectionTimelinePanel: () => null }))
vi.mock('../components/PersonHistorySummaryPanel', () => ({ PersonHistorySummaryPanel: () => null }))
vi.mock('../components/PersonLookupPanel', () => ({ PersonLookupPanel: () => null }))
vi.mock('../components/PersonOffboardingPanel', () => ({
  PersonOffboardingPanel: ({
    onStart,
    onExecute,
  }: {
    onStart: (request: any) => Promise<void>
    onExecute: (request: any) => Promise<void>
  }) => (
    <>
      <button
        type="button"
        onClick={() =>
          onStart({
            separationDate: '2026-06-01',
            separationReason: 'role change',
            targetEmploymentStatus: 'inactive',
            disableLoginRequested: true,
            newManagerPersonIdForReports: null,
          })
        }
      >
        Trigger offboarding start
      </button>
      <button
        type="button"
        onClick={() =>
          onExecute({
            newManagerPersonIdForReports: null,
          })
        }
      >
        Trigger offboarding execute
      </button>
    </>
  ),
}))
vi.mock('../components/PersonnelDocumentsPanel', () => ({
  canManagePersonnelDocuments: vi.fn(() => false),
  PersonnelDocumentsPanel: () => null,
}))
vi.mock('../components/PersonnelNotesPanel', () => ({
  canManagePersonnelNotes: vi.fn(() => false),
  PersonnelNotesPanel: () => null,
}))
vi.mock('../components/PersonTrainarrTrainingHistoryPanel', () => ({ PersonTrainarrTrainingHistoryPanel: () => null }))
vi.mock('../components/PersonTimelinePanel', () => ({ PersonTimelinePanel: () => null }))
vi.mock('../components/RoleTemplateAssignmentPanel', () => ({ RoleTemplateAssignmentPanel: () => null }))
vi.mock('../components/WorkforceOnboardingJourneyPanel', () => ({ WorkforceOnboardingJourneyPanel: () => null }))

vi.mock('../api/client', () => ({
  StaffArrApiError: class StaffArrApiError extends Error {
    status = 500
    body: string | null = null
  },
  getMe: mocked.getMe,
  getPeople: mocked.getPeople,
  getOrgUnits: mocked.getOrgUnits,
  getPerson: mocked.getPerson,
  getPersonLookup: mocked.getPersonLookup,
  getPersonHistorySummary: mocked.getPersonHistorySummary,
  getWorkforceOnboardingJourney: mocked.getWorkforceOnboardingJourney,
  getPersonOffboarding: mocked.getPersonOffboarding,
  updatePerson: mocked.updatePerson,
  updatePersonEmploymentStatus: mocked.updatePersonEmploymentStatus,
  startPersonOffboarding: mocked.startPersonOffboarding,
  executePersonOffboarding: mocked.executePersonOffboarding,
  getPersonOrgAssignments: vi.fn(async () => []),
  getManagerChain: vi.fn(async () => []),
  getSubordinates: vi.fn(async () => []),
  getSubordinateDetail: vi.fn(async () => null),
  getPermissionTemplates: vi.fn(async () => []),
  getRoleTemplates: vi.fn(async () => []),
  getPersonRoleAssignments: vi.fn(async () => []),
  getEffectivePermissions: vi.fn(async () => null),
  getPermissionHistoryTimeline: vi.fn(async () => []),
  getPersonTimeline: vi.fn(async () => ({ items: [], totalCount: 0, hasNextPage: false })),
  getPersonTrainarrTrainingHistory: vi.fn(async () => null),
  getCertificationDefinitions: vi.fn(async () => []),
  getPersonCertifications: vi.fn(async () => []),
  getPersonReadiness: vi.fn(async () => null),
  listPersonnelIncidents: vi.fn(async () => []),
  getPersonnelIncident: vi.fn(async () => null),
  listPersonnelNotes: vi.fn(async () => []),
  getPersonnelNote: vi.fn(async () => null),
  listPersonnelDocuments: vi.fn(async () => []),
  getPersonnelDocument: vi.fn(async () => null),
  getSiteReadinessRollups: vi.fn(async () => []),
  getTeamReadinessRollups: vi.fn(async () => []),
  getReadinessRollupMembers: vi.fn(async () => null),
  createOrgUnit: vi.fn(async () => ({})),
  updateOrgUnit: vi.fn(async () => ({})),
  updateOrgUnitStatus: vi.fn(async () => ({})),
  createPersonOrgAssignment: vi.fn(async () => ({})),
  updatePersonOrgAssignment: vi.fn(async () => ({})),
  updatePersonOrgAssignmentStatus: vi.fn(async () => ({})),
  updatePersonManager: vi.fn(async () => ({})),
  upsertPermissionTemplate: vi.fn(async () => ({})),
  createRoleTemplate: vi.fn(async () => ({})),
  updateRoleTemplate: vi.fn(async () => ({})),
  createPersonRoleAssignment: vi.fn(async () => ({})),
  updatePersonRoleAssignmentStatus: vi.fn(async () => ({})),
  grantPersonReadinessOverride: vi.fn(async () => ({})),
  clearPersonReadinessOverride: vi.fn(async () => ({})),
  createPersonnelIncident: vi.fn(async () => ({})),
  routePersonnelIncidentToTrainarr: vi.fn(async () => ({})),
  createPersonnelNote: vi.fn(async () => ({})),
  createPersonnelDocument: vi.fn(async () => ({})),
  grantPersonCertification: vi.fn(async () => ({})),
  updatePersonCertification: vi.fn(async () => ({})),
  createPerson: vi.fn(async () => ({ personId: 'person-1' })),
  personnelDocumentContentUrl: vi.fn(() => '/doc'),
}))

describe('HomePage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mocked.loadSession.mockReturnValue({
      accessToken: 'token',
      accessTokenExpiresAt: '2099-01-01T00:00:00.000Z',
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      tenantSlug: 'tenant',
      tenantDisplayName: 'Tenant',
      displayName: 'Test User',
      email: 'test@example.com',
    })
    mocked.getMe.mockResolvedValue({
      personId: 'person-1',
      displayName: 'Test User',
      email: 'test@example.com',
      tenantRoleKey: 'tenant_admin',
      isPlatformAdmin: false,
      primaryOrgUnitName: 'Ops',
      jobTitle: 'Admin',
    })
    mocked.getPeople.mockResolvedValue([
      {
        personId: 'person-1',
        externalUserId: null,
        displayName: 'Alex Rivera',
        primaryEmail: 'alex.rivera@example.com',
        employmentStatus: 'active',
        primaryOrgUnitId: null,
        primaryOrgUnitName: 'Ops',
        managerPersonId: null,
        jobTitle: 'Operator',
      },
      {
        personId: 'person-2',
        externalUserId: null,
        displayName: 'Sam Patel',
        primaryEmail: 'sam.patel@example.com',
        employmentStatus: 'inactive',
        primaryOrgUnitId: null,
        primaryOrgUnitName: 'Quality',
        managerPersonId: null,
        jobTitle: 'Auditor',
      },
    ])
    mocked.getOrgUnits.mockResolvedValue([])
    mocked.getPerson.mockImplementation(async (_token: string, personId: string) => {
      if (personId === 'person-2') {
        return {
          personId: 'person-2',
          externalUserId: null,
          givenName: 'Sam',
          familyName: 'Patel',
          displayName: 'Sam Patel',
          primaryEmail: 'sam.patel@example.com',
          employmentStatus: 'inactive',
          primaryOrgUnitId: null,
          primaryOrgUnitName: 'Quality',
          managerPersonId: null,
          jobTitle: 'Auditor',
          createdAt: '2026-01-01T00:00:00.000Z',
          updatedAt: '2026-01-01T00:00:00.000Z',
        }
      }
      return {
        personId: 'person-1',
        externalUserId: null,
        givenName: 'Alex',
        familyName: 'Rivera',
        displayName: 'Alex Rivera',
        primaryEmail: 'alex.rivera@example.com',
        employmentStatus: 'active',
        primaryOrgUnitId: null,
        primaryOrgUnitName: 'Ops',
        managerPersonId: null,
        jobTitle: 'Operator',
        createdAt: '2026-01-01T00:00:00.000Z',
        updatedAt: '2026-01-01T00:00:00.000Z',
      }
    })
    mocked.getPersonLookup.mockResolvedValue(null)
    mocked.getPersonHistorySummary.mockResolvedValue(null)
    mocked.getWorkforceOnboardingJourney.mockResolvedValue(null)
    mocked.getPersonOffboarding.mockResolvedValue({
      offboardingId: 'off-1',
      personId: 'person-1',
      status: 'in_progress',
      separationDate: '2026-06-01',
      targetEmploymentStatus: 'inactive',
      activeDirectReportCount: 0,
      steps: [],
      newManagerPersonIdForReports: null,
    })
    mocked.updatePerson.mockResolvedValue({})
    mocked.updatePersonEmploymentStatus.mockResolvedValue({})
    mocked.startPersonOffboarding.mockResolvedValue({})
    mocked.executePersonOffboarding.mockResolvedValue({})
  })

  afterEach(() => {
    cleanup()
  })

  it('refetches lookup, history summary, and onboarding journey after profile update', async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    await waitFor(() => {
      expect(screen.getAllByRole('button', { name: 'Trigger profile update' }).length).toBeGreaterThan(0)
    })
    await waitFor(() => {
      expect(mocked.getPersonLookup).toHaveBeenCalledWith('token', 'person-1')
      expect(mocked.getPersonHistorySummary).toHaveBeenCalledWith('token', 'person-1')
      expect(mocked.getWorkforceOnboardingJourney).toHaveBeenCalledWith('token', 'person-1')
    })

    const initialLookupCalls = mocked.getPersonLookup.mock.calls.length
    const initialHistorySummaryCalls = mocked.getPersonHistorySummary.mock.calls.length
    const initialOnboardingCalls = mocked.getWorkforceOnboardingJourney.mock.calls.length

    fireEvent.click(screen.getAllByRole('button', { name: 'Trigger profile update' })[0]!)

    await waitFor(() => {
      expect(mocked.updatePerson).toHaveBeenCalled()
    })
    await waitFor(() => {
      expect(mocked.getPersonLookup.mock.calls.length).toBeGreaterThan(initialLookupCalls)
      expect(mocked.getPersonHistorySummary.mock.calls.length).toBeGreaterThan(initialHistorySummaryCalls)
      expect(mocked.getWorkforceOnboardingJourney.mock.calls.length).toBeGreaterThan(initialOnboardingCalls)
    })
  })

  it('refetches lookup, history summary, and onboarding journey after employment status update', async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    await waitFor(() => {
      expect(screen.getAllByRole('button', { name: 'Trigger employment update' }).length).toBeGreaterThan(0)
    })
    await waitFor(() => {
      expect(mocked.getPersonLookup).toHaveBeenCalledWith('token', 'person-1')
      expect(mocked.getPersonHistorySummary).toHaveBeenCalledWith('token', 'person-1')
      expect(mocked.getWorkforceOnboardingJourney).toHaveBeenCalledWith('token', 'person-1')
    })

    const initialLookupCalls = mocked.getPersonLookup.mock.calls.length
    const initialHistorySummaryCalls = mocked.getPersonHistorySummary.mock.calls.length
    const initialOnboardingCalls = mocked.getWorkforceOnboardingJourney.mock.calls.length

    fireEvent.click(screen.getAllByRole('button', { name: 'Trigger employment update' })[0]!)

    await waitFor(() => {
      expect(mocked.updatePersonEmploymentStatus).toHaveBeenCalledWith('token', 'person-1', {
        employmentStatus: 'inactive',
        reason: 'Test change',
      })
    })
    await waitFor(() => {
      expect(mocked.getPersonLookup.mock.calls.length).toBeGreaterThan(initialLookupCalls)
      expect(mocked.getPersonHistorySummary.mock.calls.length).toBeGreaterThan(initialHistorySummaryCalls)
      expect(mocked.getWorkforceOnboardingJourney.mock.calls.length).toBeGreaterThan(initialOnboardingCalls)
    })
  })

  it('clears people quick filter when Escape is pressed', async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    const filter = (await screen.findAllByTestId('people-directory-filter'))[0] as HTMLInputElement
    fireEvent.change(filter, { target: { value: 'alex' } })
    expect(filter.value).toBe('alex')
    fireEvent.keyDown(filter, { key: 'Escape' })
    await waitFor(() => {
      expect(filter.value).toBe('')
    })
  })

  it('shows hidden-selection warning and clears filter from warning action', async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    const filter = (await screen.findAllByTestId('people-directory-filter'))[0] as HTMLInputElement
    fireEvent.change(filter, { target: { value: 'zzz' } })

    await waitFor(() => {
      expect(screen.getByText('The selected person is hidden by the current filter.')).toBeTruthy()
      expect(screen.getByText('No people match the current filter. Try a different name, email, or status.')).toBeTruthy()
    })

    fireEvent.click(screen.getByRole('button', { name: 'Clear filter to show selection' }))

    await waitFor(() => {
      expect(filter.value).toBe('')
    })
  })

  it('selects first filtered person when Enter is pressed in quick filter', async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    const filter = (await screen.findAllByTestId('people-directory-filter'))[0] as HTMLInputElement
    fireEvent.change(filter, { target: { value: 'sam' } })
    fireEvent.keyDown(filter, { key: 'Enter' })

    await waitFor(() => {
      expect(screen.getByText('Sam Patel')).toBeTruthy()
    })
  })

  it('shows keyboard guidance only when query has matching results', async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    const filter = (await screen.findAllByTestId('people-directory-filter'))[0] as HTMLInputElement
    expect(screen.queryByText('Use ↑/↓ to move through results, then press Enter to select.')).toBeNull()

    fireEvent.change(filter, { target: { value: 'sam' } })
    expect(screen.getByText('Use ↑/↓ to move through results, then press Enter to select.')).toBeTruthy()

    fireEvent.change(filter, { target: { value: 'zzz' } })
    expect(screen.queryByText('Use ↑/↓ to move through results, then press Enter to select.')).toBeNull()
  })

  it('selects active filtered person when ArrowDown then Enter is pressed in quick filter', async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    const filter = (await screen.findAllByTestId('people-directory-filter'))[0] as HTMLInputElement
    fireEvent.change(filter, { target: { value: 'a' } })
    fireEvent.keyDown(filter, { key: 'ArrowDown' })
    fireEvent.keyDown(filter, { key: 'Enter' })

    await waitFor(() => {
      const person2Calls = mocked.getPerson.mock.calls.filter(([, personId]) => personId === 'person-2')
      expect(person2Calls.length).toBeGreaterThan(0)
    })
  })

  it('does not auto-select on Enter when quick filter is empty', async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    const filter = (await screen.findAllByTestId('people-directory-filter'))[0] as HTMLInputElement
    fireEvent.keyDown(filter, { key: 'Enter' })

    await waitFor(() => {
      const person2Calls = mocked.getPerson.mock.calls.filter(([, personId]) => personId === 'person-2')
      expect(person2Calls).toHaveLength(0)
    })
  })

  it('refetches lookup, history summary, and onboarding journey after offboarding start', async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Trigger offboarding start' })).toBeTruthy()
    })
    const initialLookupCalls = mocked.getPersonLookup.mock.calls.length
    const initialHistorySummaryCalls = mocked.getPersonHistorySummary.mock.calls.length
    const initialOnboardingCalls = mocked.getWorkforceOnboardingJourney.mock.calls.length

    fireEvent.click(screen.getByRole('button', { name: 'Trigger offboarding start' }))

    await waitFor(() => {
      expect(mocked.startPersonOffboarding).toHaveBeenCalled()
    })
    await waitFor(() => {
      expect(mocked.getPersonLookup.mock.calls.length).toBeGreaterThan(initialLookupCalls)
      expect(mocked.getPersonHistorySummary.mock.calls.length).toBeGreaterThan(initialHistorySummaryCalls)
      expect(mocked.getWorkforceOnboardingJourney.mock.calls.length).toBeGreaterThan(initialOnboardingCalls)
    })
  })

  it('refetches lookup, history summary, and onboarding journey after offboarding execute', async () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Trigger offboarding execute' })).toBeTruthy()
    })
    const initialLookupCalls = mocked.getPersonLookup.mock.calls.length
    const initialHistorySummaryCalls = mocked.getPersonHistorySummary.mock.calls.length
    const initialOnboardingCalls = mocked.getWorkforceOnboardingJourney.mock.calls.length

    fireEvent.click(screen.getByRole('button', { name: 'Trigger offboarding execute' }))

    await waitFor(() => {
      expect(mocked.executePersonOffboarding).toHaveBeenCalledWith('token', 'off-1', {
        newManagerPersonIdForReports: null,
      })
    })
    await waitFor(() => {
      expect(mocked.getPersonLookup.mock.calls.length).toBeGreaterThan(initialLookupCalls)
      expect(mocked.getPersonHistorySummary.mock.calls.length).toBeGreaterThan(initialHistorySummaryCalls)
      expect(mocked.getWorkforceOnboardingJourney.mock.calls.length).toBeGreaterThan(initialOnboardingCalls)
    })
  })
})
