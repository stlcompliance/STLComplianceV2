import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { CertificationsSection } from './CertificationsSection'
import { IncidentsSection } from './IncidentsSection'
import { ReadinessSection } from './ReadinessSection'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

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

vi.mock('../../components/CertificationPanel', () => ({
  CertificationPanel: ({ actionErrorMessage }: { actionErrorMessage: string | null }) => (
    <div data-testid="certifications-action-error">{actionErrorMessage ?? ''}</div>
  ),
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
      incidentMutationError: new Error('Incident service unreachable'),
      setSelectedIncidentId: vi.fn(),
    } as unknown as StaffArrWorkspaceState

    render(<IncidentsSection state={state} />)
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

  it('passes generic certification mutation errors to CertificationPanel', () => {
    const state = {
      selectedPerson: { personId: 'person-1', displayName: 'Alex Rivera' },
      certificationDefinitions: [],
      personCertifications: [],
      certificationDefinitionsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
      personCertificationsQuery: { isLoading: false, isError: false, error: null, refetch: vi.fn() },
      canManagePeopleProfiles: true,
      grantCertificationMutation: { isPending: false, mutateAsync: vi.fn() },
      updateCertificationMutation: { isPending: false, mutateAsync: vi.fn() },
      certificationMutationError: new Error('Certification write timeout'),
    } as unknown as StaffArrWorkspaceState

    render(<CertificationsSection state={state} />)
    expect(screen.getByTestId('certifications-action-error').textContent).toContain('Certification write timeout')
  })
})
