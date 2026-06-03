import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...actual,
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
        <input
          id={id}
          data-testid={testId}
          value={value}
          placeholder={placeholder}
          onChange={(event) => onChange(event.target.value)}
        />
        <ul>
          {options.map((option) => (
            <li key={option.value}>{option.label}</li>
          ))}
        </ul>
      </label>
    ),
  }
})
import { ApplicabilityBuilderPanel } from './ApplicabilityBuilderPanel'

describe('ApplicabilityBuilderPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders guided builder and requirement rows', () => {
    render(
      <ApplicabilityBuilderPanel
        profiles={[
          {
            applicabilityProfileId: 'p1',
            profileKey: 'role_template:driver',
            label: 'Commercial driver',
            description: null,
            scopeType: 'role_template',
            scopeKey: 'driver',
            sourceProduct: null,
            sourceRecordId: null,
            sourceUpdatedAt: null,
            createdAt: '2026-05-28T12:00:00Z',
            updatedAt: '2026-05-28T12:00:00Z',
          },
        ]}
        requirements={[
          {
            requirementId: 'r1',
            requirementKey: 'cdl_renewal',
            label: 'CDL renewal',
            description: null,
            requirementSource: 'internal',
            sourceKey: null,
            trainingProgramId: null,
            trainingProgramName: null,
            trainingDefinitionId: 'd1',
            trainingDefinitionName: 'CDL renewal definition',
            applicabilityProfileId: 'p1',
            applicabilityProfileKey: 'role_template:driver',
            applicabilityProfileLabel: 'Commercial driver',
            requirementLevel: 'required',
            sortOrder: 0,
            status: 'active',
            createdAt: '2026-05-28T12:00:00Z',
            updatedAt: '2026-05-28T12:00:00Z',
          },
        ]}
        programs={[]}
        definitions={[]}
        profileLabel=""
        profileScopeType="role_template"
        profileScopeKey=""
        profileDescription=""
        requirementKey=""
        requirementLabel=""
        requirementSource="internal"
        requirementSourceKey=""
        requirementTargetType="definition"
        requirementTargetId=""
        requirementProfileId=""
        requirementLevel="required"
        onProfileLabelChange={vi.fn()}
        onProfileScopeTypeChange={vi.fn()}
        onProfileScopeKeyChange={vi.fn()}
        onProfileDescriptionChange={vi.fn()}
        onRequirementKeyChange={vi.fn()}
        onRequirementLabelChange={vi.fn()}
        onRequirementSourceChange={vi.fn()}
        onRequirementSourceKeyChange={vi.fn()}
        onRequirementTargetTypeChange={vi.fn()}
        onRequirementTargetIdChange={vi.fn()}
        onRequirementProfileIdChange={vi.fn()}
        onRequirementLevelChange={vi.fn()}
        onCreateProfile={vi.fn()}
        onCreateRequirement={vi.fn()}
        onDeleteProfile={vi.fn()}
        onDeleteRequirement={vi.fn()}
        onSyncRequirement={vi.fn()}
        isCreatingProfile={false}
        isCreatingRequirement={false}
        deletingProfileId={null}
        deletingRequirementId={null}
        syncingRequirementId={null}
        canManage
      />,
    )

    expect(screen.getByTestId('applicability-builder-panel')).toBeInTheDocument()
    expect(screen.getByText(/CDL renewal definition/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /create applicability profile/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /save requirement mapping/i })).toBeInTheDocument()
    expect(screen.getByTestId('applicability-profile-scope-key-picker')).toBeInTheDocument()
    expect(screen.queryByTestId('requirement-mapping-source-key')).not.toBeInTheDocument()
    expect(screen.getByTestId('requirement-mapping-target')).toBeInTheDocument()
  })

  it('wires searchable picker-backed scope, source, and target selection', () => {
    const onProfileScopeKeyChange = vi.fn()
    const onRequirementSourceKeyChange = vi.fn()
    const onRequirementTargetIdChange = vi.fn()

    render(
      <ApplicabilityBuilderPanel
        profiles={[]}
        requirements={[]}
        programs={[
          {
            programId: 'prog-1',
            programKey: 'program-1',
            name: 'Program One',
            status: 'published',
            definitionCount: 1,
            publishedVersionCount: 1,
            createdAt: '2026-05-28T12:00:00Z',
            updatedAt: '2026-05-28T12:00:00Z',
          },
        ]}
        definitions={[
          {
            trainingDefinitionId: 'def-1',
            definitionKey: 'def-1',
            name: 'Definition One',
            description: '',
            qualificationKey: 'qual-1',
            qualificationName: 'Qualification One',
            status: 'active',
            createdAt: '2026-05-28T12:00:00Z',
          },
        ]}
        profileLabel="Driver"
        profileScopeType="role_template"
        profileScopeKey=""
        profileDescription=""
        requirementKey="req-1"
        requirementLabel="Requirement"
        requirementSource="citation"
        requirementSourceKey=""
        requirementTargetType="program"
        requirementTargetId=""
        requirementProfileId=""
        requirementLevel="required"
        onProfileLabelChange={vi.fn()}
        onProfileScopeTypeChange={vi.fn()}
        onProfileScopeKeyChange={onProfileScopeKeyChange}
        onProfileDescriptionChange={vi.fn()}
        onRequirementKeyChange={vi.fn()}
        onRequirementLabelChange={vi.fn()}
        onRequirementSourceChange={vi.fn()}
        onRequirementSourceKeyChange={onRequirementSourceKeyChange}
        onRequirementTargetTypeChange={vi.fn()}
        onRequirementTargetIdChange={onRequirementTargetIdChange}
        onRequirementProfileIdChange={vi.fn()}
        onRequirementLevelChange={vi.fn()}
        onCreateProfile={vi.fn()}
        onCreateRequirement={vi.fn()}
        onDeleteProfile={vi.fn()}
        onDeleteRequirement={vi.fn()}
        onSyncRequirement={vi.fn()}
        isCreatingProfile={false}
        isCreatingRequirement={false}
        deletingProfileId={null}
        deletingRequirementId={null}
        syncingRequirementId={null}
        canManage
      />,
    )

    fireEvent.change(screen.getByTestId('applicability-profile-scope-key-picker'), {
      target: { value: 'scope-1' },
    })
    fireEvent.change(screen.getByTestId('requirement-mapping-source-key'), {
      target: { value: 'source-1' },
    })
    fireEvent.change(screen.getByTestId('requirement-mapping-target'), {
      target: { value: 'prog-1' },
    })

    expect(onProfileScopeKeyChange).toHaveBeenCalledWith('scope-1')
    expect(onRequirementSourceKeyChange).toHaveBeenCalledWith('source-1')
    expect(onRequirementTargetIdChange).toHaveBeenCalledWith('prog-1')
  })
})
