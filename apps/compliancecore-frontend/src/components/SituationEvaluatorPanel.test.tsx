import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SituationEvaluatorPanel } from './SituationEvaluatorPanel'

vi.mock('../api/client', () => ({
  createTheoreticalSituation: vi.fn(),
  duplicateTheoreticalSituationFromTemplate: vi.fn(),
  evaluateTheoreticalSituation: vi.fn(),
  getTheoreticalContextFields: vi.fn().mockResolvedValue([
    {
      contextKey: 'commercial_motor_vehicle_operation',
      label: 'Commercial motor vehicle operation',
      controlType: 'yes_no_unknown',
      controlledVocabularyType: 'boolean_unknown',
      required: true,
      situationKinds: [],
      values: [
        { key: 'yes', label: 'Yes', description: '', category: 'context', edgeCase: false },
        { key: 'no', label: 'No', description: '', category: 'context', edgeCase: false },
        { key: 'unknown', label: 'Unknown', description: '', category: 'context', edgeCase: false },
      ],
    },
  ]),
  getTheoreticalEvidenceStates: vi.fn().mockResolvedValue([
    { key: 'valid', label: 'Exists and valid', description: '', category: 'evidence_state', edgeCase: false },
    { key: 'missing', label: 'Does not exist', description: '', category: 'evidence_state', edgeCase: false },
    { key: 'unknown', label: 'Unknown', description: '', category: 'evidence_state', edgeCase: false },
  ]),
  getTheoreticalIncidentOptions: vi.fn().mockResolvedValue([
    { key: 'accident', label: 'Accident', description: '', category: 'incident', edgeCase: false },
  ]),
  getTheoreticalMaterialClasses: vi.fn().mockResolvedValue([
    { key: 'unknown', label: 'Unknown', description: '', category: 'hazmat_class', edgeCase: false },
  ]),
  getTheoreticalNextContext: vi.fn(),
  getTheoreticalSituationKinds: vi.fn().mockResolvedValue([
    {
      key: 'driver_dispatch_readiness',
      label: 'Driver dispatch readiness',
      description: 'Evaluate whether a driver can be dispatched.',
      category: 'driver',
      edgeCase: false,
    },
  ]),
  resolveTheoreticalApplicability: vi.fn(),
  saveTheoreticalSituationTemplate: vi.fn(),
  setTheoreticalSituationContext: vi.fn(),
  setTheoreticalSituationFacts: vi.fn(),
  setTheoreticalSituationIncidents: vi.fn(),
}))

describe('SituationEvaluatorPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders a structured situation wizard without rule-pack selection', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SituationEvaluatorPanel accessToken="token" canEvaluate={true} factRequirements={[]} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Situation Evaluator')).toBeInTheDocument()
    expect(await screen.findByText('Driver dispatch readiness')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Start Situation/i })).toBeEnabled()
    expect(screen.queryByText(/rule pack to evaluate/i)).not.toBeInTheDocument()
  })

  it('disables simulation actions for users without evaluation permission', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SituationEvaluatorPanel accessToken="token" canEvaluate={false} factRequirements={[]} />
      </QueryClientProvider>,
    )

    expect(await screen.findByRole('button', { name: /Start Situation/i })).toBeDisabled()
  })
})
