import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { CertificationsSection } from './CertificationsSection'
import { IncidentsSection } from './IncidentsSection'
import { ReadinessSection } from './ReadinessSection'
import { createLaunchHandoff } from '../../api/client'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

vi.mock('../../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/client')>()
  return {
    ...actual,
    getStaffArrFieldset: vi.fn().mockResolvedValue({
      fields: [],
    }),
    createLaunchHandoff: vi.fn(),
  }
})

vi.mock('../../components/IncidentsPanel', () => ({
  IncidentsPanel: ({ actionErrorMessage }: { actionErrorMessage: string | null }) => (
    <div data-testid="incidents-action-error">{actionErrorMessage ?? ''}</div>
  ),
}))

vi.mock('../../components/ReadinessPanel', () => ({
  ReadinessPanel: ({ overrideErrorMessage }: { overrideErrorMessage: string | null }) => (
    <div data-testid="readiness-action-error">{overrideErrorMessage ?? ''}</div>
  ),
}))

vi.mock('../../components/ReadinessRollupSupervisorPanel', () => ({
  ReadinessRollupSupervisorPanel: () => null,
}))

describe('Workspace section action error normalization', () => {
  afterEach(() => {
    cleanup()
  })

  it('passes generic incident mutation errors to IncidentsPanel', () => {
    const state = {
      selectedPerson: { personId: 'person-1', displayName: 'Alex Rivera' },
      personIncidents: [],
      selectedIncidentId: null,
      incidentDetailQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
      personIncidentsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
      canManagePersonIncidents: true,
      createIncidentMutation: { isPending: false, mutateAsync: vi.fn() },
      routeIncidentToTrainarrMutation: { isPending: false, mutateAsync: vi.fn() },
      updateIncidentStatusMutation: { isPending: false, mutateAsync: vi.fn() },
      createIncidentNoteMutation: { isPending: false, mutateAsync: vi.fn() },
      updateIncidentNoteStatusMutation: { isPending: false, mutateAsync: vi.fn() },
      createIncidentAttachmentMutation: { isPending: false, mutateAsync: vi.fn() },
      incidentMutationError: new Error('Incident service unreachable'),
      setSelectedIncidentId: vi.fn(),
    } as unknown as StaffArrWorkspaceState

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <IncidentsSection state={state} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('incidents-action-error').textContent).toContain('Incident service unreachable')
  })

  it('passes generic readiness override errors to ReadinessPanel', () => {
    const state = {
      canViewReadinessRollupSummaries: false,
      selectedPerson: { personId: 'person-1', displayName: 'Alex Rivera' },
      personReadinessQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
      canOverridePersonReadiness: true,
      grantReadinessOverrideMutation: { isPending: false, mutateAsync: vi.fn() },
      clearReadinessOverrideMutation: { isPending: false, mutateAsync: vi.fn() },
      readinessOverrideMutationError: new Error('Override policy check failed'),
    } as unknown as StaffArrWorkspaceState

    render(<ReadinessSection state={state} />)
    expect(screen.getByTestId('readiness-action-error').textContent).toContain('Override policy check failed')
  })

  it('renders the TrainArr-owned certification mirror without surfacing StaffArr write errors', () => {
    const state = {
      selectedPerson: { personId: 'person-1', displayName: 'Alex Rivera' },
      accessToken: 'token',
      certificationDefinitions: [],
      personCertifications: [],
      personReadinessQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
      certificationDefinitionsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
      personCertificationsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
      canManagePeopleProfiles: true,
      grantCertificationMutation: { isPending: false, mutateAsync: vi.fn() },
      updateCertificationMutation: { isPending: false, mutateAsync: vi.fn() },
      certificationMutationError: new Error('Certification write timeout'),
    } as unknown as StaffArrWorkspaceState

    render(<CertificationsSection state={state} />)
    expect(screen.getByText('Certification actions moved to TrainArr')).toBeTruthy()
    expect(screen.queryByText('Certification write timeout')).toBeNull()
  })

  it('shows a safe fallback when TrainArr handoff launch fails', async () => {
    vi.mocked(createLaunchHandoff).mockRejectedValueOnce(new Error('handoff down'))

    const state = {
      selectedPerson: { personId: 'person-1', displayName: 'Alex Rivera' },
      accessToken: 'token',
      certificationDefinitions: [],
      personCertifications: [],
      personReadinessQuery: { data: null, isLoading: false, isError: false, error: null, refetch: vi.fn() },
      certificationDefinitionsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
      personCertificationsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
      canManagePeopleProfiles: true,
      grantCertificationMutation: { isPending: false, mutateAsync: vi.fn() },
      updateCertificationMutation: { isPending: false, mutateAsync: vi.fn() },
      certificationMutationError: null,
    } as unknown as StaffArrWorkspaceState

    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    render(<CertificationsSection state={state} />)
    fireEvent.click(screen.getByRole('button', { name: 'Open in TrainArr' }))

    expect(await screen.findByText('TrainArr is temporarily unavailable. Please try again.')).toBeTruthy()
    expect(consoleError).toHaveBeenCalled()
    consoleError.mockRestore()
  })
})
