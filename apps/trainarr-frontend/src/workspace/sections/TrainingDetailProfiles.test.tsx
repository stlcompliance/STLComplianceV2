import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import type { ReactNode } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', () => ({
  DetailBadge: ({ label }: { label: string }) => <span>{label}</span>,
  DetailEmptyState: ({ text }: { text: string }) => <p>{text}</p>,
  StaticSearchPicker: ({
    id,
    label,
    value,
    onChange,
    options,
    placeholder,
    testId,
  }: {
    id: string
    label: string
    value: string
    onChange: (value: string) => void
    options: Array<{ value: string; label: string }>
    placeholder?: string
    testId?: string
  }) => (
    <label htmlFor={id}>
      <span>{label}</span>
      <select
        id={id}
        aria-label={label}
        data-testid={testId}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      >
        <option value="">{placeholder ?? 'Select…'}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
  ),
  ProfileDetailsLayout: ({
    title,
    mainContent,
    railSections,
    snapshotFields,
  }: {
    title: string
    mainContent?: ReactNode
    railSections?: Array<{ title: string; content: ReactNode }>
    snapshotFields?: Array<{ label: string; value: unknown }>
  }) => (
    <div>
      <h1>{title}</h1>
      <div>{mainContent}</div>
      <div>
        {(railSections ?? []).map((section) => (
          <section key={section.title}>
            <h2>{section.title}</h2>
            {section.content}
          </section>
        ))}
      </div>
      <dl>
        {(snapshotFields ?? []).map((field) => (
          <div key={field.label}>
            <dt>{field.label}</dt>
            <dd>{String(field.value)}</dd>
          </div>
        ))}
      </dl>
    </div>
  ),
}))

vi.mock('../../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/client')>()
  return {
    ...actual,
    removeTrainingProgramContentReference: vi.fn().mockResolvedValue(undefined),
  }
})

import { TrainingProgramProfile } from './TrainingDetailProfiles'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'
import { removeTrainingProgramContentReference } from '../../api/client'

describe('TrainingProgramProfile', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders content references and wires the remove action', async () => {
    const invalidateQueries = vi.fn().mockResolvedValue(undefined)
    const setRemovingProgramContentReferenceId = vi.fn()
    const state = {
      session: { accessToken: 'trainarr-token' },
      accessToken: 'trainarr-token',
      canPrograms: true,
      selectedProgramId: 'program-1',
      programDetailQuery: {
        data: {
          programId: 'program-1',
          programKey: 'onboarding',
          name: 'Onboarding bundle',
          description: 'Bundle description.',
          status: 'draft',
          definitions: [],
          contentReferences: [
            {
              contentReferenceId: 'ref-1',
              trainingProgramId: 'program-1',
              contentType: 'compliance_core_citation',
              title: 'Citation pack',
              referenceValue: 'cfr-391-11',
              notes: 'Driver qualification citation',
              localeTag: 'en-us',
              createdByUserId: 'user-1',
              createdAt: '2026-05-28T12:00:00Z',
            },
          ],
          createdAt: '2026-05-28T12:00:00Z',
          updatedAt: '2026-05-28T12:00:00Z',
        },
      },
      programsQuery: {
        data: [
          {
            programId: 'program-1',
            programKey: 'onboarding',
            name: 'Onboarding bundle',
            status: 'draft',
            definitionCount: 0,
            publishedVersionCount: 0,
            createdAt: '2026-05-28T12:00:00Z',
            updatedAt: '2026-05-28T12:00:00Z',
          },
        ],
      },
      programVersionsQuery: { data: [] },
      trainingMatrixQuery: { data: { entries: [] } },
      requirementBuilderQuery: { data: { requirements: [] } },
      programRulePackRequirementsQuery: { data: [] },
      attachProgramContentReferenceMutation: { mutate: vi.fn(), isPending: false },
      contentReferenceTypeKey: 'compliance_core_citation',
      contentReferenceTitle: 'Citation pack',
      contentReferenceValue: 'cfr-391-11',
      contentReferenceNotes: 'Driver qualification citation',
      contentReferenceLocaleTag: 'en-us',
      setContentReferenceTypeKey: vi.fn(),
      setContentReferenceTitle: vi.fn(),
      setContentReferenceValue: vi.fn(),
      setContentReferenceNotes: vi.fn(),
      setContentReferenceLocaleTag: vi.fn(),
      removingProgramContentReferenceId: null,
      setRemovingProgramContentReferenceId,
      queryClient: { invalidateQueries },
    } as unknown as TrainArrWorkspaceState

    render(
      <MemoryRouter>
        <TrainingProgramProfile state={state} />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Content references', level: 2 })).toBeInTheDocument()
    expect(screen.getByText('Citation pack')).toBeInTheDocument()
    expect(screen.getByText(/Locale en-us/i)).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /remove/i }))

    expect(setRemovingProgramContentReferenceId).toHaveBeenCalledWith('ref-1')
    expect(removeTrainingProgramContentReference).toHaveBeenCalledWith(
      'trainarr-token',
      'program-1',
      'ref-1',
    )
    await waitFor(() =>
      expect(invalidateQueries).toHaveBeenCalledWith({
        queryKey: ['trainarr-program-detail', 'trainarr-token', 'program-1'],
      }),
    )
  })
})
