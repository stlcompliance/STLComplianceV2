import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { fireEvent } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { ReactNode } from 'react'
import * as client from '../api/client'
import { ProgramBuilderPanel } from './ProgramBuilderPanel'
import type { TrainingDefinitionResponse, TrainingProgramSummaryResponse } from '../api/types'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    generateTrainingProgramDraft: vi.fn(),
  }
})

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
  accessToken: 'token',
  mode: 'drawer' as const,
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

function renderPanel(element: ReactNode) {
  return render(
    <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
      {element}
    </QueryClientProvider>,
  )
}

describe('ProgramBuilderPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders manage gate for non-admin users', () => {
    renderPanel(<ProgramBuilderPanel {...baseProps} />)
    expect(screen.getByText(/program builder requires trainarr admin/i)).toBeInTheDocument()
    expect(screen.getByText('Onboarding bundle')).toBeInTheDocument()
  })

  it('renders create program form for admins', () => {
    renderPanel(
      <ProgramBuilderPanel
        {...baseProps}
        mode="create"
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

  it('generates and applies a catalog-assisted draft', async () => {
    vi.mocked(client.generateTrainingProgramDraft).mockResolvedValue({
      generatedAt: '2026-05-29T12:00:00Z',
      prompt: 'hazmat onboarding for new drivers',
      name: 'Hazmat Onboarding Training Program',
      description: 'Catalog-assisted draft for "hazmat onboarding for new drivers". Recommended definitions: Annual compliance refresher.',
      trainingDefinitionIds: ['d1'],
      matchedDefinitions: [
        {
          trainingDefinitionId: 'd1',
          definitionKey: 'annual_compliance',
          name: 'Annual compliance refresher',
          qualificationKey: 'annual_compliance',
          qualificationName: 'Annual Compliance',
          score: 12,
          matchReason: "name matches 'hazmat'; qualification key matches 'onboarding'",
        },
      ],
      summary: 'Suggested 1 definition(s) from 1 active definitions, led by Annual compliance refresher.',
    })

    renderPanel(
      <ProgramBuilderPanel
        {...baseProps}
        mode="create"
        programs={[]}
        selectedDefinitionIds={[]}
        programKey=""
        programName=""
        programDescription=""
        canManage
      />,
    )

    fireEvent.change(screen.getByRole('textbox', { name: /draft prompt/i }), {
      target: { value: 'hazmat onboarding for new drivers' },
    })
    fireEvent.click(screen.getByRole('button', { name: /generate draft/i }))

    expect(await screen.findByText('Hazmat Onboarding Training Program')).toBeInTheDocument()
    expect(screen.getByText(/Recommended definitions: Annual compliance refresher\./i)).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /apply draft/i }))

    expect(baseProps.onProgramNameChange).toHaveBeenCalledWith('Hazmat Onboarding Training Program')
    expect(baseProps.onProgramDescriptionChange).toHaveBeenCalledWith(
      'Catalog-assisted draft for "hazmat onboarding for new drivers". Recommended definitions: Annual compliance refresher.',
    )
    expect(baseProps.onToggleDefinition).toHaveBeenCalledWith('d1')
  })

  it('renders edit controls when a program is selected', () => {
    renderPanel(
      <ProgramBuilderPanel
        {...baseProps}
        mode="details"
        selectedProgramId="p1"
        selectedProgramDetail={{
          programId: 'p1',
          programKey: 'onboarding',
          name: 'Onboarding bundle',
          description: 'Bundle description for operational staff.',
          status: 'draft',
          definitions: [],
          contentReferences: [],
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
