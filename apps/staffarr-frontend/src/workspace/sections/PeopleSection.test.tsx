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
vi.mock('@stl/shared-ui', () => ({
  getErrorMessage: (error: unknown, fallback = 'Something went wrong.') =>
    error instanceof Error ? error.message : fallback,
}))

function buildState(overrides: Partial<StaffArrWorkspaceState> = {}): StaffArrWorkspaceState {
  const setPeopleDirectoryQuery = vi.fn()
  const setSelectedPersonId = vi.fn()
  const base = {
    me: {
      displayName: 'Test User',
      tenantRoleKey: 'tenant_admin',
      personId: 'person-1',
      primaryOrgUnitName: 'Ops',
      jobTitle: 'Admin',
    },
    peopleQuery: { isLoading: false },
    people: [
      {
        personId: 'person-1',
        displayName: 'Alex Rivera',
        primaryEmail: 'alex.rivera@example.com',
        jobTitle: 'Operator',
        employmentStatus: 'active',
      },
      {
        personId: 'person-2',
        displayName: 'Sam Patel',
        primaryEmail: 'sam.patel@example.com',
        jobTitle: 'Auditor',
        employmentStatus: 'inactive',
      },
    ],
    filteredPeople: [
      {
        personId: 'person-1',
        displayName: 'Alex Rivera',
        primaryEmail: 'alex.rivera@example.com',
        jobTitle: 'Operator',
        employmentStatus: 'active',
      },
      {
        personId: 'person-2',
        displayName: 'Sam Patel',
        primaryEmail: 'sam.patel@example.com',
        jobTitle: 'Auditor',
        employmentStatus: 'inactive',
      },
    ],
    peopleDirectoryQuery: '',
    setPeopleDirectoryQuery,
    selectedPersonHiddenByFilter: false,
    selectedPersonId: 'person-1',
    setSelectedPersonId,
    activeDirectoryPersonId: null,
    setActiveDirectoryPersonId: vi.fn(),
    effectivePersonId: 'person-1',
    orgUnits: [],
    canManagePeopleProfiles: true,
    createPersonMutation: { isPending: false, error: null, mutateAsync: vi.fn() },
    personProfileQuery: { isLoading: false, data: null },
    profile: null,
    updatePersonMutation: { isPending: false, mutateAsync: vi.fn() },
    updateEmploymentStatusMutation: { isPending: false, mutateAsync: vi.fn() },
    personProfileMutationError: null,
    selectedPerson: null,
    personNotes: [],
    selectedNoteId: null,
    noteDetailQuery: { data: null, isLoading: false, isError: false, refetch: vi.fn() },
    personNotesQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    canManagePersonNotes: false,
    createNoteMutation: { isPending: false, mutateAsync: vi.fn() },
    noteMutationError: null,
    setSelectedNoteId: vi.fn(),
    accessToken: 'token',
    personDocuments: [],
    selectedDocumentId: null,
    documentDetailQuery: { data: null, isLoading: false, isError: false, refetch: vi.fn() },
    personDocumentsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
    canManagePersonDocuments: false,
    uploadDocumentMutation: { isPending: false, mutateAsync: vi.fn() },
    documentMutationError: null,
    setSelectedDocumentId: vi.fn(),
    personnelDocumentContentUrl: vi.fn(() => ''),
    effectivePersonIdForPeople: 'person-1',
    workforceOnboardingJourneyQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    personOffboardingQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
    canManagePeopleProfilesForOffboarding: true,
    startOffboardingMutation: { isPending: false, mutateAsync: vi.fn() },
    executeOffboardingMutation: { isPending: false, mutateAsync: vi.fn() },
    offboardingMutationError: null,
    personLookupQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
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
    trainarrTrainingHistoryQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
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

describe('PeopleSection quick filter', () => {
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
        selectedPerson: {
          personId: 'person-1',
          displayName: 'Alex Rivera',
        } as any,
        selectedPersonHiddenByFilter: true,
      }),
    )
    expect(screen.getByText('No people match the current filter. Try a different name, email, or status.')).toBeTruthy()
    expect(screen.getByText('The selected person is hidden by the current filter.')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Clear filter to show selection' }))
    expect(setPeopleDirectoryQuery).toHaveBeenCalledWith('')
  })

  it('shows keyboard guidance when query has matching results', () => {
    renderPeopleSection(
      buildState({
        peopleDirectoryQuery: 'sam',
        filteredPeople: [
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
        ],
      }),
    )

    expect(screen.getByText('Use ↑/↓ to move through results, then press Enter to select.')).toBeTruthy()
  })

  it('updates filter input and clears filter', () => {
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
  })

  it('clears filter when Escape is pressed', () => {
    const setPeopleDirectoryQuery = vi.fn()
    renderPeopleSection(
      buildState({
        peopleDirectoryQuery: 'alex',
        setPeopleDirectoryQuery,
      }),
    )
    fireEvent.keyDown(screen.getByTestId('workspace-people-directory-filter'), { key: 'Escape' })
    expect(setPeopleDirectoryQuery).toHaveBeenCalledWith('')
  })

  it('selects first filtered person when Enter is pressed in filter', () => {
    const setSelectedPersonId = vi.fn()
    renderPeopleSection(
      buildState({
        peopleDirectoryQuery: 'sam',
        filteredPeople: [
          {
            personId: 'person-2',
            externalUserId: null,
            displayName: 'Sam Patel',
            primaryEmail: 'sam.patel@example.com',
            primaryOrgUnitId: null,
            primaryOrgUnitName: 'Quality',
            managerPersonId: null,
            jobTitle: 'Auditor',
            employmentStatus: 'inactive',
          },
        ],
        setSelectedPersonId,
      }),
    )
    fireEvent.keyDown(screen.getByTestId('workspace-people-directory-filter'), { key: 'Enter' })
    expect(setSelectedPersonId).toHaveBeenCalledWith('person-2')
  })

  it('selects active filtered person when ArrowDown then Enter is pressed in filter', () => {
    const setSelectedPersonId = vi.fn()
    const setActiveDirectoryPersonId = vi.fn()
    renderPeopleSection(
      buildState({
          peopleDirectoryQuery: 'a',
          filteredPeople: [
            {
              personId: 'person-1',
              externalUserId: null,
              displayName: 'Alex Rivera',
              primaryEmail: 'alex.rivera@example.com',
              primaryOrgUnitId: null,
              primaryOrgUnitName: 'Ops',
              managerPersonId: null,
              jobTitle: 'Operator',
              employmentStatus: 'active',
            },
            {
              personId: 'person-2',
              externalUserId: null,
              displayName: 'Sam Patel',
              primaryEmail: 'sam.patel@example.com',
              primaryOrgUnitId: null,
              primaryOrgUnitName: 'Quality',
              managerPersonId: null,
              jobTitle: 'Auditor',
              employmentStatus: 'inactive',
            },
          ],
          activeDirectoryPersonId: 'person-1',
          setActiveDirectoryPersonId,
          setSelectedPersonId,
      }),
    )

    const filter = screen.getByTestId('workspace-people-directory-filter')
    fireEvent.keyDown(filter, { key: 'ArrowDown' })
    expect(setActiveDirectoryPersonId).toHaveBeenCalledWith('person-2')

    renderPeopleSection(
      buildState({
          peopleDirectoryQuery: 'a',
          filteredPeople: [
            {
              personId: 'person-1',
              externalUserId: null,
              displayName: 'Alex Rivera',
              primaryEmail: 'alex.rivera@example.com',
              primaryOrgUnitId: null,
              primaryOrgUnitName: 'Ops',
              managerPersonId: null,
              jobTitle: 'Operator',
              employmentStatus: 'active',
            },
            {
              personId: 'person-2',
              externalUserId: null,
              displayName: 'Sam Patel',
              primaryEmail: 'sam.patel@example.com',
              primaryOrgUnitId: null,
              primaryOrgUnitName: 'Quality',
              managerPersonId: null,
              jobTitle: 'Auditor',
              employmentStatus: 'inactive',
            },
          ],
          activeDirectoryPersonId: 'person-2',
          setSelectedPersonId,
      }),
    )

    fireEvent.keyDown(screen.getAllByTestId('workspace-people-directory-filter')[1]!, { key: 'Enter' })
    expect(setSelectedPersonId).toHaveBeenCalledWith('person-2')
  })

  it('does not auto-select when Enter is pressed with empty query', () => {
    const setSelectedPersonId = vi.fn()
    renderPeopleSection(
      buildState({
        peopleDirectoryQuery: '',
        setSelectedPersonId,
      }),
    )
    fireEvent.keyDown(screen.getByTestId('workspace-people-directory-filter'), { key: 'Enter' })
    expect(setSelectedPersonId).not.toHaveBeenCalled()
  })

  it('falls back to visible filtered person when active selection is stale', () => {
    const setSelectedPersonId = vi.fn()
    renderPeopleSection(
      buildState({
        peopleDirectoryQuery: 'sam',
        activeDirectoryPersonId: 'person-999',
        filteredPeople: [
          {
            personId: 'person-2',
            externalUserId: null,
            displayName: 'Sam Patel',
            primaryEmail: 'sam.patel@example.com',
            primaryOrgUnitId: null,
            primaryOrgUnitName: 'Quality',
            managerPersonId: null,
            jobTitle: 'Auditor',
            employmentStatus: 'inactive',
          },
        ],
        setSelectedPersonId,
      }),
    )

    fireEvent.keyDown(screen.getByTestId('workspace-people-directory-filter'), { key: 'Enter' })
    expect(setSelectedPersonId).toHaveBeenCalledWith('person-2')
  })

  it('renders the replacement person detail overview', () => {
    const profile = {
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
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    }

    renderPeopleSection(
      buildState({
        profile,
        personProfileQuery: { isLoading: false, data: profile } as any,
        selectedPerson: {
          personId: 'person-1',
          displayName: 'Alex Rivera',
          primaryEmail: 'alex.rivera@example.com',
          employmentStatus: 'active',
          primaryOrgUnitName: 'Ops',
          jobTitle: 'Operator',
        } as any,
      }),
      '/people/details',
    )

    expect(screen.getByRole('heading', { name: 'Alex Rivera' })).toBeTruthy()
    expect(screen.getByText('Person snapshot')).toBeTruthy()
    expect(screen.getByText('Authorization decision')).toBeTruthy()
    expect(screen.getByRole('tab', { name: 'Overview' }).getAttribute('aria-selected')).toBe('true')
    expect(screen.getByRole('tab', { name: 'Documents' })).toBeTruthy()
  })
})
