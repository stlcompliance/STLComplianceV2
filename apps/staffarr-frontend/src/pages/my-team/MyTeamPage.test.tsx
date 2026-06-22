import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { MyTeamPage } from './MyTeamPage'
import type {
  CertificationDefinitionResponse,
  MyTeamDashboardResponse,
  PersonCertificationResponse,
  PersonReadinessResponse,
} from '../../api/types'

vi.mock('../../auth/sessionStorage', () => ({
  loadSession: () => ({
    accessToken: 'token-1',
  }),
}))

vi.mock('../../api/client', async () => {
  const actual = await vi.importActual<typeof import('../../api/client')>('../../api/client')
  return {
    ...actual,
    getMyTeamDashboard: vi.fn(),
    getPersonReadiness: vi.fn(),
    getCertificationDefinitions: vi.fn(),
    getPersonCertifications: vi.fn(),
    reviewMyTeamPersonnelUpdateRequest: vi.fn().mockResolvedValue(undefined),
  }
})

import {
  getMyTeamDashboard,
  getPersonReadiness,
  getCertificationDefinitions,
  getPersonCertifications,
} from '../../api/client'

const dashboard: MyTeamDashboardResponse = {
  directReportCount: 1,
  notReadyCount: 1,
  missingCertificationCount: 1,
  expiringCertificationCount: 1,
  openIncidentCount: 0,
  pendingUpdateRequestCount: 0,
  onboardingInProgressCount: 0,
  pendingTrainingBlockerCount: 0,
  members: [
    {
      summary: {
        personId: 'person-2',
        displayName: 'Direct Report',
        primaryEmail: 'report@example.com',
        employmentStatus: 'active',
        primaryOrgUnitName: 'Field Team',
        managerPersonId: 'person-1',
        managerDisplayName: 'Manager Example',
        jobTitle: 'Technician',
        depth: 1,
        directReportCount: 0,
        activeAssignmentPath: 'Field Team / Crew A / Technician',
      },
      readinessStatus: 'not_ready',
      blockerCount: 1,
      missingCertificationCount: 1,
      expiringCertificationCount: 1,
      openIncidentCount: 0,
      pendingUpdateRequestCount: 0,
      pendingTrainingBlockerCount: 0,
    },
  ],
  pendingUpdateRequests: [],
}

const readiness: PersonReadinessResponse = {
  personId: 'person-2',
  readinessStatus: 'not_ready',
  readinessBasis: 'certifications',
  calculatedAt: '2026-06-01T12:00:00.000Z',
  sourceTimestamp: '2026-06-01T12:00:00.000Z',
  snapshotAgeMinutes: 5,
  snapshotFreshnessStatus: 'fresh',
  confidenceLevel: 'high',
  reasonCodes: ['missing_certification'],
  requirements: [
    {
      certificationDefinitionId: 'def-1',
      certificationKey: 'readiness.forklift',
      certificationName: 'Forklift Safety',
      requirementStatus: 'missing',
      recordEffectiveStatus: null,
      expiresAt: null,
    },
  ],
  blockers: [],
  activeOverride: null,
}

const definitions: CertificationDefinitionResponse[] = [
  {
    certificationDefinitionId: 'def-1',
    certificationKey: 'readiness.forklift',
    name: 'Forklift Safety',
    description: 'Forklift operation and safety training.',
    category: 'readiness',
    defaultValidityDays: 365,
    status: 'active',
    createdAt: '2026-01-01T00:00:00.000Z',
    updatedAt: '2026-01-01T00:00:00.000Z',
  },
]

const certifications: PersonCertificationResponse[] = [
  {
    personCertificationId: 'cert-1',
    personId: 'person-2',
    certificationDefinitionId: 'def-1',
    certificationKey: 'readiness.forklift',
    certificationName: 'Forklift Safety',
    category: 'readiness',
    sourceType: 'manual',
    status: 'active',
    effectiveStatus: 'active',
    grantedAt: '2026-01-01T00:00:00.000Z',
    expiresAt: new Date(Date.now() + 30 * 86400000).toISOString(),
    notes: 'Expires soon for renewal review.',
    grantedByUserId: 'user-1',
    externalPublicationId: null,
    createdAt: '2026-01-01T00:00:00.000Z',
    updatedAt: '2026-01-01T00:00:00.000Z',
  },
]

describe('MyTeamPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders selected direct report certifications alongside readiness', async () => {
    vi.mocked(getMyTeamDashboard).mockResolvedValue(dashboard)
    vi.mocked(getPersonReadiness).mockResolvedValue(readiness)
    vi.mocked(getCertificationDefinitions).mockResolvedValue(definitions)
    vi.mocked(getPersonCertifications).mockResolvedValue(certifications)

    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
      },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MyTeamPage />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Workforce readiness')).toBeTruthy()
    expect(await screen.findByText('Certifications for Direct Report')).toBeTruthy()
    expect(await screen.findByText('Expiring soon')).toBeTruthy()
    expect(screen.getAllByText('Forklift Safety').length).toBeGreaterThan(0)
  })

  it('shows a safe fallback when the team dashboard fails to load', async () => {
    vi.mocked(getMyTeamDashboard).mockRejectedValueOnce(new Error('socket closed'))

    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
      },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MyTeamPage />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Team dashboard failed to load')).toBeTruthy()
    expect(await screen.findByText('Failed to load direct reports.')).toBeTruthy()
    expect(screen.queryByText('socket closed')).toBeNull()
  })
})
