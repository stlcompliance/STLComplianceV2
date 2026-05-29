import { ProgramBuilderPanel } from '../../components/ProgramBuilderPanel'
import { ApplicabilityBuilderPanel } from '../../components/ApplicabilityBuilderPanel'
import { StepBuilderPanel } from '../../components/StepBuilderPanel'
import { TrainingMatrixPanel } from '../../components/TrainingMatrixPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function ProgramsSection({ state }: Props) {
  const s = state
  return (
    <div className="space-y-6">
      <ProgramBuilderPanel
        programs={s.programsQuery.data ?? []}
        definitions={s.definitionsQuery.data ?? []}
        selectedDefinitionIds={s.selectedProgramDefinitionIds}
        selectedProgramId={s.selectedProgramId}
        selectedProgramDetail={s.programDetailQuery.data}
        programVersions={s.programVersionsQuery.data ?? []}
        selectedDefinitionIdForCitations={s.selectedDefinitionIdForCitations}
        onSelectProgram={s.setSelectedProgramId}
        onSelectDefinitionForCitations={s.setSelectedDefinitionIdForCitations}
        programKey={s.programKey}
        programName={s.programName}
        programDescription={s.programDescription}
        onProgramKeyChange={s.setProgramKey}
        onProgramNameChange={s.setProgramName}
        onProgramDescriptionChange={s.setProgramDescription}
        onToggleDefinition={s.toggleProgramDefinition}
        onCreateProgram={() => s.createProgramMutation.mutate()}
        onSaveProgram={() => s.saveProgramMutation.mutate()}
        onPublishProgram={() => s.publishProgramMutation.mutate()}
        onStartRevision={() => s.startRevisionMutation.mutate()}
        isCreating={s.createProgramMutation.isPending}
        isSaving={s.saveProgramMutation.isPending}
        isPublishing={s.publishProgramMutation.isPending}
        isStartingRevision={s.startRevisionMutation.isPending}
        canManage={s.canPrograms}
      />
      <StepBuilderPanel
        definitions={s.definitionsQuery.data ?? []}
        selectedDefinitionId={s.selectedDefinitionIdForCitations}
        steps={s.definitionStepsQuery.data ?? []}
        isLoading={s.definitionStepsQuery.isLoading}
        canManage={s.canPrograms}
        isSubmitting={s.createDefinitionStepMutation.isPending}
        onSelectDefinition={s.setSelectedDefinitionIdForCitations}
        onCreateStep={async (request) => {
          await s.createDefinitionStepMutation.mutateAsync(request)
        }}
        onDeleteStep={async (stepId) => {
          await s.deleteDefinitionStepMutation.mutateAsync(stepId)
        }}
      />
      <TrainingMatrixPanel
        entries={s.trainingMatrixQuery.data?.entries ?? []}
        programs={s.programsQuery.data ?? []}
        definitions={s.definitionsQuery.data ?? []}
        applicabilityKey={s.matrixApplicabilityKey}
        applicabilityLabel={s.matrixApplicabilityLabel}
        targetType={s.matrixTargetType}
        targetId={s.matrixTargetId}
        requirementLevel={s.matrixRequirementLevel}
        sortOrder={s.matrixSortOrder}
        onApplicabilityKeyChange={s.setMatrixApplicabilityKey}
        onApplicabilityLabelChange={s.setMatrixApplicabilityLabel}
        onTargetTypeChange={s.setMatrixTargetType}
        onTargetIdChange={s.setMatrixTargetId}
        onRequirementLevelChange={s.setMatrixRequirementLevel}
        onSortOrderChange={s.setMatrixSortOrder}
        onCreateEntry={() => s.createMatrixEntryMutation.mutate()}
        onDeleteEntry={(id) => s.deleteMatrixEntryMutation.mutate(id)}
        isCreating={s.createMatrixEntryMutation.isPending}
        deletingEntryId={s.deletingMatrixEntryId}
        canManage={s.canPrograms}
      />
      <ApplicabilityBuilderPanel
        profiles={s.requirementBuilderQuery.data?.profiles ?? []}
        requirements={s.requirementBuilderQuery.data?.requirements ?? []}
        programs={s.programsQuery.data ?? []}
        definitions={s.definitionsQuery.data ?? []}
        profileLabel={s.profileLabel}
        profileScopeType={s.profileScopeType}
        profileScopeKey={s.profileScopeKey}
        profileDescription={s.profileDescription}
        requirementKey={s.requirementKey}
        requirementLabel={s.requirementLabel}
        requirementSource={s.requirementSource}
        requirementSourceKey={s.requirementSourceKey}
        requirementTargetType={s.requirementTargetType}
        requirementTargetId={s.requirementTargetId}
        requirementProfileId={s.requirementProfileId}
        requirementLevel={s.requirementLevel}
        onProfileLabelChange={s.setProfileLabel}
        onProfileScopeTypeChange={s.setProfileScopeType}
        onProfileScopeKeyChange={s.setProfileScopeKey}
        onProfileDescriptionChange={s.setProfileDescription}
        onRequirementKeyChange={s.setRequirementKey}
        onRequirementLabelChange={s.setRequirementLabel}
        onRequirementSourceChange={s.setRequirementSource}
        onRequirementSourceKeyChange={s.setRequirementSourceKey}
        onRequirementTargetTypeChange={s.setRequirementTargetType}
        onRequirementTargetIdChange={s.setRequirementTargetId}
        onRequirementProfileIdChange={s.setRequirementProfileId}
        onRequirementLevelChange={s.setRequirementLevel}
        onCreateProfile={() => s.createApplicabilityProfileMutation.mutate()}
        onCreateRequirement={() => s.createRequirementMutation.mutate()}
        onDeleteProfile={(id) => s.deleteApplicabilityProfileMutation.mutate(id)}
        onDeleteRequirement={(id) => s.deleteRequirementMutation.mutate(id)}
        onSyncRequirement={(id) => s.syncRequirementToMatrixMutation.mutate(id)}
        isCreatingProfile={s.createApplicabilityProfileMutation.isPending}
        isCreatingRequirement={s.createRequirementMutation.isPending}
        deletingProfileId={s.deletingApplicabilityProfileId}
        deletingRequirementId={s.deletingRequirementId}
        syncingRequirementId={s.syncingRequirementId}
        canManage={s.canPrograms}
      />
    </div>
  )
}
