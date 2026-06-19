import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { PeopleSection } from './PeopleSection'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

vi.mock('../../components/CreatePersonPanel', () => ({ CreatePersonPanel: () => null }))
vi.mock('../../components/PersonProfileEditorPanel', () => ({ PersonProfileEditorPanel: () => null }))
vi.mock('../../components/PersonLookupPanel', () => ({ PersonLookupPanel: () => null }))
vi.mock('../../components/PersonTimelinePanel', () => ({ PersonTimelinePanel: () => null }))
vi.mock('../../components/PersonTrainarrTrainingHistoryPanel', () => ({ PersonTrainarrTrainingHistoryPanel: () => null }))
vi.mock('../../components/WorkforceOnboardingJourneyPanel', () => ({ WorkforceOnboardingJourneyPanel: () => null }))
vi.mock('../../components/PersonOffboardingPanel', () => ({ PersonOffboardingPanel: () => null }))
vi.mock('../../components/PersonHistorySummaryPanel', () => ({ PersonHistorySummaryPanel: () => null }))
vi.mock('../../components/PersonnelNotesPanel', () => ({ PersonnelNotesPanel: () => null }))
vi.mock('../../components/PersonnelDocumentsPanel', () => ({ PersonnelDocumentsPanel: () => null }))
vi.mock('../../components/TrainingAcknowledgementsPanel', () => ({ TrainingAcknowledgementsPanel: () => null }))
vi.mock('../../components/PersonOrgAssignmentsManager', () => ({ PersonOrgAssignmentsManager: () => null }))
vi.mock('../../components/ManagerHierarchyPanel', () => ({ ManagerHierarchyPanel: () => null }))
vi.mock('./CertificationsSection', () => ({ CertificationsSection: () => null }))
vi.mock('./IncidentsSection', () => ({ IncidentsSection: () => null }))
vi.mock('./PermissionsSection', () => ({ PermissionsSection: () => null }))
vi.mock('@stl/shared-ui', () => ({
  DetailBadge: ({ label }: { label: string }) => <span>{label}</span>,
  getErrorMessage: (error: unknown, fallback = 'Something went wrong.') =>
    error instanceof Error ? error.message : fallback,
  ProfileDetailsLayout: ({
    testId,
    title,
    snapshotTitle,
    decisionTitle,
    tabs,
    activeTab,
  }: {
    testId?: string
    title: string
    snapshotTitle: string
    decisionTitle: string
    activeTab?: string
    tabs: Array<string | { key: string; label: string }>
  }) => (
    <div data-testid={testId}>
      <h1>{title}</h1>
      <h2>{snapshotTitle}</h2>
      <h2>{decisionTitle}</h2>
      <div role="tablist">
        {tabs.map((tab, index) => {
          const key = typeof tab === 'string' ? tab : tab.key
          const label = typeof tab === 'string' ? tab : tab.label
          const selected = activeTab ? activeTab === key : index === 0
          return (
            <button key={key} type="button" role="tab" aria-selected={selected}>
              {label}
            </button>
          )
        })}
      </div>
    </div>
  ),
}))

function buildPerson(personId: string, displayName: string, email: string, employmentStatus: string) {
  const [givenName, familyName = 'Worker'] = displayName.split(' ')
  return {
    personId,
    externalUserId: null,
    givenName,
    familyName,
    displayName,
    primaryEmail: email,
    employmentStatus,
    primaryOrgUnitId: null,
    primaryOrgUnitName: 'Ops',
    managerPersonId: null,
    jobTitle: 'Operator',
    preferredName: null,
    workRelationshipType: 'employee',
    employmentType: 'full_time',
    workerCategory: 'employee',
    flsaStatus: 'unknown',
    positionNumber: 'POS-1001',
    currentEmploymentAction: 'hire',
    currentEmploymentActionAt: null,
    leaveStatus: 'active',
    eligibleForRehire: true,
    canLoginSnapshot: false,
    hasUserAccountSnapshot: false,
  }
}

function buildProfile(personId = 'person-1') {
  return {
    personId,
    externalUserId: null,
    givenName: 'Alex',
    familyName: 'Rivera',
    legalFirstName: 'Alex',
    legalMiddleName: null,
    legalLastName: 'Rivera',
    preferredName: null,
    pronouns: null,
    displayName: 'Alex Rivera',
    primaryEmail: 'alex.rivera@example.com',
    alternateEmail: null,
    primaryPhone: null,
    alternatePhone: null,
    workPhone: null,
    employmentStatus: 'active',
    workRelationshipType: 'employee',
    employmentType: 'full_time',
    primaryOrgUnitId: null,
    primaryOrgUnitName: 'Ops',
    managerPersonId: null,
    jobTitle: 'Operator',
    startDate: null,
    expectedStartDate: null,
    workerCategory: 'employee',
    flsaStatus: 'unknown',
    positionNumber: 'POS-1001',
    currentEmploymentAction: 'hire',
    currentEmploymentActionAt: null,
    leaveStatus: 'active',
    eligibleForRehire: true,
    homeBaseLocationId: null,
    homeBaseLocationName: null,
    canLoginSnapshot: false,
    hasUserAccountSnapshot: false,
    createdAt: '2026-06-01T00:00:00Z',
    updatedAt: '2026-06-01T00:00:00Z',
  }
}

function buildState(overrides: Partial<StaffArrWorkspaceState> = {}): StaffArrWorkspaceState {
  const people = [
    buildPerson('person-1', 'Alex Rivera', 'alex.rivera@example.com', 'active'),
    buildPerson('person-2', 'Sam Patel', 'sam.patel@example.com', 'inactive'),
  ]

  const base = {
    peopleQuery: { isLoading: false },
    people,
    filteredPeople: people,
    peopleDirectoryQuery: '',
    setPeopleDirectoryQuery: vi.fn(),
    selectedPersonHiddenByFilter: false,
    selectedPersonId: 'person-1',
    setSelectedPersonId: vi.fn(),
    activeDirectoryPersonId: null,
    setActiveDirectoryPersonId: vi.fn(),
    effectivePersonId: 'person-1',
    peopleDetailTab: 'overview',
    setPeopleDetailTab: vi.fn(),
    orgUnits: [],
    accessToken: 'token',
    canManagePeopleProfiles: true,
    canManagePersonNotes: false,
    canManagePersonDocuments: false,
    canManageHierarchy: false,
    peopleDetailQuery: null,
    createPersonMutation: { isPending: false, error: null, mutateAsync: vi.fn() },
    updatePersonMutation: { isPending: false, mutateAsync: vi.fn() },
    updateEmploymentStatusMutation: { isPending: false, mutateAsync: vi.fn() },
    personProfileMutationError: null,
    roleTemplates: [],
    roleAssignments: [],
    effectivePermissions: null,
    permissionHistory: [],
    permissionTemplates: [],
    productPermissionCatalogProductKey: '',
    setProductPermissionCatalogProductKey: vi.fn(),
    productPermissionCatalogQuery: { data: [], isLoading: false, isError: false, error: null, refetch: vi.fn() },
    permissionTemplatesQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    roleTemplatesQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    roleAssignmentsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    effectivePermissionsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    permissionHistoryQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    permissionCheckInput: 'staffarr.people.read',
    setPermissionCheckInput: vi.fn(),
    permissionCheckMutation: { data: null, isPending: false, error: null, mutateAsync: vi.fn(), reset: vi.fn() },
    permissionCheckMutationError: null,
    profile: null,
    personProfileQuery: { isLoading: false, data: null },
    personSummaryQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn(), data: null },
    selectedPerson: null,
    assignments: [],
    assignmentQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    managerChain: [],
    subordinates: [],
    selectedSubordinateId: null,
    setSelectedSubordinateId: vi.fn(),
    selectedSubordinateDetail: null,
    managerChainQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    subordinatesQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    subordinateDetailQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    updateManagerMutation: { isPending: false, mutateAsync: vi.fn() },
    managerMutationError: null,
    createAssignmentMutation: { isPending: false, mutateAsync: vi.fn() },
    updateAssignmentMutation: { isPending: false, mutateAsync: vi.fn() },
    updateAssignmentStatusMutation: { isPending: false, mutateAsync: vi.fn() },
    assignmentMutationError: null,
    personLookupQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    personReadinessQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    certificationDefinitions: [],
    personCertifications: [],
    certificationDefinitionsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    personCertificationsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    trainarrTrainingHistoryQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    workforceOnboardingJourneyQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    personOffboardingQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    startOffboardingMutation: { isPending: false, mutateAsync: vi.fn() },
    executeOffboardingMutation: { isPending: false, mutateAsync: vi.fn() },
    offboardingMutationError: null,
    personIncidents: [],
    selectedIncidentId: null,
    setSelectedIncidentId: vi.fn(),
    incidentDetailQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    personIncidentsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    createIncidentMutation: { isPending: false, mutateAsync: vi.fn() },
    routeIncidentToTrainarrMutation: { isPending: false, mutateAsync: vi.fn() },
    updateIncidentStatusMutation: { isPending: false, mutateAsync: vi.fn() },
    createIncidentNoteMutation: { isPending: false, mutateAsync: vi.fn() },
    updateIncidentNoteStatusMutation: { isPending: false, mutateAsync: vi.fn() },
    createIncidentAttachmentMutation: { isPending: false, mutateAsync: vi.fn() },
    incidentMutationError: null,
    incidentNoteMutationError: null,
    incidentAttachmentMutationError: null,
    downloadIncidentAttachment: vi.fn(),
    personNotes: [],
    selectedNoteId: null,
    setSelectedNoteId: vi.fn(),
    noteDetailQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    personNotesQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    createNoteMutation: { isPending: false, mutateAsync: vi.fn() },
    noteMutationError: null,
    personDocuments: [],
    selectedDocumentId: null,
    setSelectedDocumentId: vi.fn(),
    documentDetailQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    personDocumentsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    uploadDocumentMutation: { isPending: false, mutateAsync: vi.fn() },
    documentMutationError: null,
    personnelDocumentContentUrl: vi.fn(() => ''),
    personHistorySummaryQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    personTimelineEntries: [],
    personTimelineTotalCount: 0,
    personTimelinePage: 1,
    personTimelinePageSize: 25,
    personTimelineHasNextPage: false,
    personTimelineCategoryFilter: '',
    personTimelineQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    setPersonTimelineCategoryFilter: vi.fn(),
    setPersonTimelinePage: vi.fn(),
    setPersonTimelinePageSize: vi.fn(),
  } as unknown as StaffArrWorkspaceState

  return { ...base, ...overrides }
}

function renderPeopleSection(state: StaffArrWorkspaceState, initialPath = '/people') {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <PeopleSection state={state} />
    </MemoryRouter>,
  )
}

describe('PeopleSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows count and filtered list', () => {
    renderPeopleSection(buildState())

    expect(screen.getByText('Showing 2 of 2 people')).toBeTruthy()
    expect(screen.getByText('Alex Rivera')).toBeTruthy()
    expect(screen.getByText('Sam Patel')).toBeTruthy()
  })

  it('shows no-match state and hidden-selection warning', () => {
    const setPeopleDirectoryQuery = vi.fn()

    renderPeopleSection(
      buildState({
        peopleDirectoryQuery: 'zzz',
        setPeopleDirectoryQuery,
        filteredPeople: [],
        selectedPerson: buildPerson('person-1', 'Alex Rivera', 'alex.rivera@example.com', 'active') as any,
        selectedPersonHiddenByFilter: true,
      }),
    )

    expect(screen.getByText('No people match the current filter. Try a different name, email, or status.')).toBeTruthy()
    expect(screen.getByText('The selected person is hidden by the current filter.')).toBeTruthy()

    fireEvent.click(screen.getByRole('button', { name: 'Clear filter to show selection' }))
    expect(setPeopleDirectoryQuery).toHaveBeenCalledWith('')
  })

  it('shows keyboard guidance only when query has matching results', () => {
    renderPeopleSection(
      buildState({
        peopleDirectoryQuery: 'sam',
        filteredPeople: [buildPerson('person-2', 'Sam Patel', 'sam.patel@example.com', 'inactive')] as any,
      }),
    )

    expect(screen.getByText('Use ↑/↓ to move through results, then press Enter to select.')).toBeTruthy()
  })

  it('updates and clears the filter input', () => {
    const setPeopleDirectoryQuery = vi.fn()

    renderPeopleSection(
      buildState({
        peopleDirectoryQuery: 'alex',
        setPeopleDirectoryQuery,
      }),
    )

    fireEvent.change(screen.getByTestId('workspace-people-directory-filter'), {
      target: { value: 'sam' },
    })
    expect(setPeopleDirectoryQuery).toHaveBeenCalledWith('sam')

    fireEvent.click(screen.getByRole('button', { name: 'Clear' }))
    expect(setPeopleDirectoryQuery).toHaveBeenCalledWith('')

    fireEvent.keyDown(screen.getByTestId('workspace-people-directory-filter'), { key: 'Escape' })
    expect(setPeopleDirectoryQuery).toHaveBeenCalledWith('')
  })

  it('selects the first filtered person when Enter is pressed', () => {
    const setSelectedPersonId = vi.fn()

    renderPeopleSection(
      buildState({
        peopleDirectoryQuery: 'sam',
        filteredPeople: [buildPerson('person-2', 'Sam Patel', 'sam.patel@example.com', 'inactive')] as any,
        setSelectedPersonId,
      }),
    )

    fireEvent.keyDown(screen.getByTestId('workspace-people-directory-filter'), { key: 'Enter' })
    expect(setSelectedPersonId).toHaveBeenCalledWith('person-2')
  })

  it('moves the active quick-filter result with arrow keys', () => {
    const setActiveDirectoryPersonId = vi.fn()

    renderPeopleSection(
      buildState({
        peopleDirectoryQuery: 'a',
        activeDirectoryPersonId: 'person-1',
        setActiveDirectoryPersonId,
      }),
    )

    fireEvent.keyDown(screen.getByTestId('workspace-people-directory-filter'), { key: 'ArrowDown' })
    expect(setActiveDirectoryPersonId).toHaveBeenCalledWith('person-2')
  })

  it('renders the detail overview with the new tabbed shell', () => {
    const profile = buildProfile()

    renderPeopleSection(
      buildState({
        profile,
        personProfileQuery: { isLoading: false, data: profile } as any,
        selectedPerson: buildPerson('person-1', 'Alex Rivera', 'alex.rivera@example.com', 'active') as any,
      }),
      '/people/details?person=person-1&tab=overview',
    )

    expect(screen.getByRole('heading', { name: 'Alex Rivera' })).toBeTruthy()
    expect(screen.getByText('Person snapshot')).toBeTruthy()
    expect(screen.getByText('Authorization decision')).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Overview' }).getAttribute('aria-selected')).toBe('true')
    expect(screen.getByRole('tab', { name: 'Documents' })).toBeTruthy()
  })
})
