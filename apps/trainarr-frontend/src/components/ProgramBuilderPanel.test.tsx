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
  publishedVersionCount: 0,
  createdAt: '2026-05-27T12:00:00Z',
  updatedAt: '2026-05-27T12:00:00Z',
}

const baseProps = {
  programs: [program],
  definitions: [definition],
  selectedDefinitionIds: [] as string[],
  selectedProgramId: null as string | null,
  selectedProgramDetail: undefined,
  programVersions: [],
  selectedDefinitionIdForCitations: null,
  onSelectProgram: vi.fn(),
  onSelectDefinitionForCitations: vi.fn(),
  programKey: '',
  programName: '',
  programDescription: '',
  onProgramKeyChange: vi.fn(),
  onProgramNameChange: vi.fn(),
  onProgramDescriptionChange: vi.fn(),
  onToggleDefinition: vi.fn(),
  onCreateProgram: vi.fn(),
  onSaveProgram: vi.fn(),
  onPublishProgram: vi.fn(),
  onStartRevision: vi.fn(),
  isCreating: false,
  isSaving: false,
  isPublishing: false,
  isStartingRevision: false,
  canManage: false,
}

describe('ProgramBuilderPanel', () => {
  it('renders manage gate for non-admin users', () => {
    render(<ProgramBuilderPanel {...baseProps} />)
    expect(screen.getByText(/program builder requires trainarr admin/i)).toBeInTheDocument()
    expect(screen.getByText('Onboarding bundle')).toBeInTheDocument()
  })

  it('renders create program form for admins', () => {
    render(
      <ProgramBuilderPanel
        {...baseProps}
        programs={[]}
        selectedDefinitionIds={['d1']}
        programKey="onboarding"
        programName="Onboarding"
        programDescription="New hire training bundle for operational staff."
        canManage
      />,
    )
    expect(screen.getByRole('button', { name: /create program/i })).toBeInTheDocument()
    expect(screen.getByText('Annual compliance refresher')).toBeInTheDocument()
  })

  it('renders edit controls when a program is selected', () => {
    render(
      <ProgramBuilderPanel
        {...baseProps}
        selectedProgramId="p1"
        selectedProgramDetail={{
          programId: 'p1',
          programKey: 'onboarding',
          name: 'Onboarding bundle',
          description: 'Bundle description for operational staff.',
          status: 'draft',
          definitions: [],
          createdAt: '2026-05-27T12:00:00Z',
          updatedAt: '2026-05-27T12:00:00Z',
        }}
        programName="Onboarding bundle"
        programDescription="Bundle description for operational staff."
        canManage
      />,
    )
    expect(screen.getByRole('button', { name: /save draft/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /publish version/i })).toBeInTheDocument()
  })
})
