import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ProgramBuilderPanel } from './ProgramBuilderPanel'
import type { TrainingDefinitionResponse, TrainingProgramSummaryResponse } from '../api/types'

const definition: TrainingDefinitionResponse = {
  trainingDefinitionId: 'd1',
  definitionKey: 'annual_compliance',
  name: 'Annual compliance refresher',
  description: 'Required annual training.',
  qualificationKey: 'annual_compliance',
  qualificationName: 'Annual Compliance',
  status: 'active',
  createdAt: '2026-05-27T12:00:00Z',
}

const program: TrainingProgramSummaryResponse = {
  programId: 'p1',
  programKey: 'onboarding',
  name: 'Onboarding bundle',
  status: 'draft',
  definitionCount: 1,
  createdAt: '2026-05-27T12:00:00Z',
  updatedAt: '2026-05-27T12:00:00Z',
}

describe('ProgramBuilderPanel', () => {
  it('renders manage gate for non-admin users', () => {
    render(
      <ProgramBuilderPanel
        programs={[program]}
        definitions={[definition]}
        selectedDefinitionIds={[]}
        selectedProgramId={null}
        selectedDefinitionIdForCitations={null}
        onSelectProgram={vi.fn()}
        onSelectDefinitionForCitations={vi.fn()}
        programKey=""
        programName=""
        programDescription=""
        onProgramKeyChange={vi.fn()}
        onProgramNameChange={vi.fn()}
        onProgramDescriptionChange={vi.fn()}
        onToggleDefinition={vi.fn()}
        onCreateProgram={vi.fn()}
        isCreating={false}
        canManage={false}
      />,
    )
    expect(screen.getByText(/program builder requires trainarr admin/i)).toBeInTheDocument()
    expect(screen.getByText('Onboarding bundle')).toBeInTheDocument()
  })

  it('renders create program form for admins', () => {
    render(
      <ProgramBuilderPanel
        programs={[]}
        definitions={[definition]}
        selectedDefinitionIds={['d1']}
        selectedProgramId={null}
        selectedDefinitionIdForCitations={null}
        onSelectProgram={vi.fn()}
        onSelectDefinitionForCitations={vi.fn()}
        programKey="onboarding"
        programName="Onboarding"
        programDescription="New hire training bundle for operational staff."
        onProgramKeyChange={vi.fn()}
        onProgramNameChange={vi.fn()}
        onProgramDescriptionChange={vi.fn()}
        onToggleDefinition={vi.fn()}
        onCreateProgram={vi.fn()}
        isCreating={false}
        canManage
      />,
    )
    expect(screen.getByRole('button', { name: /create program/i })).toBeInTheDocument()
    expect(screen.getByText('Annual compliance refresher')).toBeInTheDocument()
  })
})
