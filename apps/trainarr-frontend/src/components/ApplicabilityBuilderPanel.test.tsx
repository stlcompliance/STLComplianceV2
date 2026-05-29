import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ApplicabilityBuilderPanel } from './ApplicabilityBuilderPanel'

describe('ApplicabilityBuilderPanel', () => {
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
  })
})
