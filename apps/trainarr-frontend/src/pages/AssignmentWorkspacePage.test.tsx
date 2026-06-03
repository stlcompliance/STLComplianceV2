import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AssignmentWorkspacePage } from './AssignmentWorkspacePage'

const { mockAssignment, mockSteps } = vi.hoisted(() => ({
  mockAssignment: {
    assignmentId: '00000000-0000-0000-0000-000000000099',
    staffarrPersonId: '00000000-0000-0000-0000-000000000001',
    trainingDefinitionId: '00000000-0000-0000-0000-000000000010',
    trainingDefinitionName: 'Hazmat annual',
    definitionKey: 'hazmat_annual',
    qualificationKey: 'hazmat',
    qualificationName: 'Hazmat endorsement',
    staffarrIncidentRemediationId: null,
    assignmentReason: 'manual',
    status: 'in_progress',
    dueAt: null,
    assignedByUserId: '00000000-0000-0000-0000-000000000002',
    blockerPublicationId: null,
    completedAt: null,
    completedByUserId: null,
    createdAt: '2026-05-27T10:00:00.000Z',
    updatedAt: '2026-05-27T10:00:00.000Z',
    evidenceCount: 0,
    evaluation: null,
    signoffs: [],
    completionRequirementsMet: false,
    qualificationIssue: null,
  },
  mockSteps: [
    {
      progressId: '00000000-0000-0000-0000-0000000000a1',
      trainingAssignmentId: '00000000-0000-0000-0000-000000000099',
      stepId: '00000000-0000-0000-0000-0000000000b1',
      stepKey: 'practical-check',
      name: 'Practical check',
      description: 'Demonstrate the procedure under observation.',
      stepType: 'practical',
      configJson: JSON.stringify(
        {
          skillTaskName: 'Demonstrate the procedure under observation.',
          passCriteria: 'Perform the procedure safely and in the correct sequence.',
          observationPrompts: ['Setup', 'Execution', 'Shutdown'],
        },
        null,
        2,
      ),
      sortOrder: 0,
      status: 'pending',
      isVisible: true,
      quizScorePercent: null,
      responseJson: null,
      completedAt: null,
    },
  ],
}))

vi.mock('../api/client', () => ({
  getMe: vi.fn().mockResolvedValue({
    userId: '00000000-0000-0000-0000-000000000002',
    personId: '00000000-0000-0000-0000-000000000001',
    displayName: 'Demo Trainee',
    tenantRoleKey: 'tenant_member',
    isPlatformAdmin: false,
  }),
  getTrainingAssignment: vi.fn().mockResolvedValue(mockAssignment),
  getTrainingAssignmentSteps: vi.fn().mockResolvedValue(mockSteps),
  getTrainingAssignmentLaborEntries: vi.fn().mockResolvedValue([]),
  getTrainingEvidence: vi.fn().mockResolvedValue([]),
  createTrainingEvidence: vi.fn(),
  createTrainingAssignmentLaborEntry: vi.fn(),
  removeTrainingAssignmentLaborEntry: vi.fn(),
  submitTrainingEvaluation: vi.fn(),
  submitTrainingSignoff: vi.fn(),
  completeTrainingAssignment: vi.fn(),
}))

vi.mock('../auth/sessionStorage', () => ({
  canCompleteAssignment: () => false,
  canManageAssignments: () => false,
  canSubmitEvaluation: () => true,
  canSubmitTraineeSignoff: () => true,
  canSubmitTrainerSignoff: () => false,
  canUploadEvidence: () => true,
  loadSession: () => ({
    accessToken: 'token',
    accessTokenExpiresAt: '2099-01-01T00:00:00.000Z',
    userId: '00000000-0000-0000-0000-000000000002',
    personId: '00000000-0000-0000-0000-000000000001',
    tenantId: '00000000-0000-0000-0000-0000000000aa',
    tenantSlug: 'demo',
    tenantDisplayName: 'Demo Tenant',
    displayName: 'Demo Trainee',
    email: 'trainee@example.com',
  }),
}))

describe('AssignmentWorkspacePage', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders assignment detail for deep link route', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/assignments/00000000-0000-0000-0000-000000000099']}>
          <Routes>
            <Route path="/assignments/:assignmentId" element={<AssignmentWorkspacePage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('assignment-workspace')).toBeInTheDocument()
    expect(screen.getByText('Hazmat annual')).toBeInTheDocument()
    expect(screen.getByTestId('assignment-evidence-section')).toBeInTheDocument()
    expect(screen.getByTestId('assignment-labor-panel')).toBeInTheDocument()
    expect(await screen.findByLabelText('Practical result')).toBeInTheDocument()
    expect(await screen.findByLabelText('Observation notes')).toBeInTheDocument()
  })

  it('renders evidence section on evidence deep link route', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <MemoryRouter
          initialEntries={['/assignments/00000000-0000-0000-0000-000000000099/evidence']}
        >
          <Routes>
            <Route
              path="/assignments/:assignmentId/evidence"
              element={<AssignmentWorkspacePage focus="evidence" />}
            />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('assignment-evidence-section')).toBeInTheDocument()
    await waitFor(() => expect(screen.getByText('Hazmat annual')).toBeInTheDocument())
  })
})
