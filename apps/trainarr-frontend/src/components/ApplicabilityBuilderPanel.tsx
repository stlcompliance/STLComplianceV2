import { buildSemanticKey, ControlledSelect, type PickerOption } from '@stl/shared-ui'
import { useEffect, useMemo } from 'react'

import type {
  CreateTrainingRequirementRequest,
  TrainingApplicabilityProfileResponse,
  TrainingDefinitionResponse,
  TrainingProgramSummaryResponse,
  TrainingRequirementResponse,
} from '../api/types'

export const APPLICABILITY_SCOPE_OPTIONS = [
  { value: 'role_template', label: 'Role template' },
  { value: 'org_unit', label: 'Org unit' },
  { value: 'job_code', label: 'Job code' },
  { value: 'site', label: 'Site' },
  { value: 'custom', label: 'Custom scope' },
] as const

export const REQUIREMENT_SOURCE_OPTIONS = [
  { value: 'internal', label: 'Internal requirement' },
  { value: 'rule_pack', label: 'Compliance Core rule pack' },
  { value: 'citation', label: 'Citation reference' },
] as const

interface ApplicabilityBuilderPanelProps {
  profiles: TrainingApplicabilityProfileResponse[]
  requirements: TrainingRequirementResponse[]
  programs: TrainingProgramSummaryResponse[]
  definitions: TrainingDefinitionResponse[]
  profileLabel: string
  profileScopeType: string
  profileScopeKey: string
  profileDescription: string
  requirementKey: string
  requirementLabel: string
  requirementSource: string
  requirementSourceKey: string
  requirementTargetType: 'program' | 'definition'
  requirementTargetId: string
  requirementProfileId: string
  requirementLevel: string
  onProfileLabelChange: (value: string) => void
  onProfileScopeTypeChange: (value: string) => void
  onProfileScopeKeyChange: (value: string) => void
  onProfileDescriptionChange: (value: string) => void
  onRequirementKeyChange: (value: string) => void
  onRequirementLabelChange: (value: string) => void
  onRequirementSourceChange: (value: string) => void
  onRequirementSourceKeyChange: (value: string) => void
  onRequirementTargetTypeChange: (value: 'program' | 'definition') => void
  onRequirementTargetIdChange: (value: string) => void
  onRequirementProfileIdChange: (value: string) => void
  onRequirementLevelChange: (value: string) => void
  onCreateProfile: () => void
  onCreateRequirement: () => void
  onDeleteProfile: (profileId: string) => void
  onDeleteRequirement: (requirementId: string) => void
  onSyncRequirement: (requirementId: string) => void
  isCreatingProfile: boolean
  isCreatingRequirement: boolean
  deletingProfileId: string | null
  deletingRequirementId: string | null
  syncingRequirementId: string | null
  canManage: boolean
}

export function ApplicabilityBuilderPanel({
  profiles,
  requirements,
  programs,
  definitions,
  profileLabel,
  profileScopeType,
  profileScopeKey,
  profileDescription,
  requirementKey,
  requirementLabel,
  requirementSource,
  requirementSourceKey,
  requirementTargetType,
  requirementTargetId,
  requirementProfileId,
  requirementLevel,
  onProfileLabelChange,
  onProfileScopeTypeChange,
  onProfileScopeKeyChange,
  onProfileDescriptionChange,
  onRequirementKeyChange,
  onRequirementLabelChange,
  onRequirementSourceChange,
  onRequirementSourceKeyChange,
  onRequirementTargetTypeChange,
  onRequirementTargetIdChange,
  onRequirementProfileIdChange,
  onRequirementLevelChange,
  onCreateProfile,
  onCreateRequirement,
  onDeleteProfile,
  onDeleteRequirement,
  onSyncRequirement,
  isCreatingProfile,
  isCreatingRequirement,
  deletingProfileId,
  deletingRequirementId,
  syncingRequirementId,
  canManage,
}: ApplicabilityBuilderPanelProps) {
  const generatedProfileScopeKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'profile',
        kind: profileScopeType.replace(/[^a-z0-9]/gi, '').toLowerCase() || 'scope',
        title: profileLabel.trim(),
        existingKeys: profiles
          .filter((profile) => profile.scopeType === profileScopeType)
          .map((profile) => profile.scopeKey),
        maxLength: 128,
      }),
    [profileLabel, profileScopeType, profiles],
  )
  const generatedRequirementKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'train',
        kind: 'req',
        title: requirementLabel.trim(),
        existingKeys: requirements.map((row) => row.requirementKey),
        maxLength: 128,
      }),
    [requirementLabel, requirements],
  )
  const scopeKeyOptions = useMemo<PickerOption[]>(
    () =>
      profiles
        .filter((profile) => profile.scopeType === profileScopeType)
        .map((profile) => ({ value: profile.scopeKey, label: `${profile.scopeKey} (${profile.label})` })),
    [profileScopeType, profiles],
  )
  const sourceKeyOptions = useMemo<PickerOption[]>(
    () =>
      requirements
        .filter(
          (row) =>
            row.requirementSource === requirementSource &&
            typeof row.sourceKey === 'string' &&
            row.sourceKey.trim().length > 0,
        )
        .map((row) => ({ value: row.sourceKey!, label: row.sourceKey! })),
    [requirementSource, requirements],
  )

  useEffect(() => {
    onProfileScopeKeyChange(generatedProfileScopeKey)
  }, [generatedProfileScopeKey, onProfileScopeKeyChange])

  useEffect(() => {
    onRequirementKeyChange(generatedRequirementKey)
  }, [generatedRequirementKey, onRequirementKeyChange])

  useEffect(() => {
    if (requirementSource === 'internal') {
      onRequirementSourceKeyChange('')
    }
  }, [onRequirementSourceKeyChange, requirementSource])

  const targets =
    requirementTargetType === 'program'
      ? programs.map((program) => ({ id: program.programId, label: program.name }))
      : definitions.map((definition) => ({ id: definition.trainingDefinitionId, label: definition.name }))

  const groupedRequirements = requirements.reduce<Record<string, TrainingRequirementResponse[]>>((acc, row) => {
    const key = row.applicabilityProfileLabel ?? 'Unscoped'
    acc[key] = acc[key] ?? []
    acc[key].push(row)
    return acc
  }, {})

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
      data-testid="applicability-builder-panel"
    >
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
        Requirement mapping / applicability builder
      </h2>
      <p className="mt-1 text-xs text-slate-500">
        Guided authoring for structured applicability scopes and requirement-to-program mappings. StaffArr owns org
        truth; scope keys are local references TrainArr uses for matrix sync.
      </p>

      {!canManage ? (
        <p className="mt-3 text-sm text-slate-400">Applicability builder requires trainarr admin access.</p>
      ) : (
        <div className="mt-4 space-y-6">
          <div>
            <h3 className="text-xs font-semibold uppercase tracking-wide text-violet-300">Step 1 — Applicability profile</h3>
            <div className="mt-3 grid gap-3 sm:grid-cols-2">
              <label htmlFor="applicability-profile-label" className="block text-xs text-slate-400">
                Label
                <input
                  id="applicability-profile-label"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                  value={profileLabel}
                  onChange={(event) => onProfileLabelChange(event.target.value)}
                />
              </label>
              <label htmlFor="applicability-profile-scope-type" className="block text-xs text-slate-400">
                Scope type
                <select
                  id="applicability-profile-scope-type"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                  value={profileScopeType}
                  onChange={(event) => onProfileScopeTypeChange(event.target.value)}
                >
                  {APPLICABILITY_SCOPE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </label>
              <div className="space-y-1 text-xs text-slate-400">
                <p>Scope reference is generated automatically from the profile label.</p>
                <ControlledSelect
                  label="Known scope references"
                  value={profileScopeKey}
                  onChange={onProfileScopeKeyChange}
                  options={scopeKeyOptions}
                  emptyLabel="Use generated key"
                  testId="applicability-profile-scope-key-picker"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                />
              </div>
              <label htmlFor="applicability-profile-description" className="block text-xs text-slate-400 sm:col-span-2">
                Description
                <input
                  id="applicability-profile-description"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                  value={profileDescription}
                  onChange={(event) => onProfileDescriptionChange(event.target.value)}
                />
              </label>
              <div className="sm:col-span-2">
                <button
                  type="button"
                  className="rounded bg-violet-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-600 disabled:opacity-50"
                  disabled={isCreatingProfile || !profileLabel.trim() || !profileScopeKey.trim()}
                  onClick={onCreateProfile}
                >
                  {isCreatingProfile ? 'Creating profile…' : 'Create applicability profile'}
                </button>
              </div>
            </div>
          </div>

          <div className="border-t border-slate-700 pt-4">
            <h3 className="text-xs font-semibold uppercase tracking-wide text-violet-300">Step 2 — Requirement mapping</h3>
            <div className="mt-3 grid gap-3 sm:grid-cols-2">
              <div className="space-y-1 text-xs text-slate-400">Requirement reference is generated automatically from the requirement label.</div>
              <label htmlFor="requirement-mapping-label" className="block text-xs text-slate-400">
                Label
                <input
                  id="requirement-mapping-label"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                  value={requirementLabel}
                  onChange={(event) => onRequirementLabelChange(event.target.value)}
                />
              </label>
              <label htmlFor="requirement-mapping-source" className="block text-xs text-slate-400">
                Source
                <select
                  id="requirement-mapping-source"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                  value={requirementSource}
                  onChange={(event) => onRequirementSourceChange(event.target.value)}
                >
                  {REQUIREMENT_SOURCE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </label>
              {requirementSource === 'internal' ? null : (
                <ControlledSelect
                  label="Source reference"
                  value={requirementSourceKey}
                  onChange={onRequirementSourceKeyChange}
                  options={sourceKeyOptions}
                  emptyLabel="Select source reference…"
                  testId="requirement-mapping-source-key"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                />
              )}
              <label htmlFor="requirement-mapping-profile" className="block text-xs text-slate-400">
                Applicability profile
                <select
                  id="requirement-mapping-profile"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                  value={requirementProfileId}
                  onChange={(event) => onRequirementProfileIdChange(event.target.value)}
                >
                  <option value="">Select profile…</option>
                  {profiles.map((profile) => (
                    <option key={profile.applicabilityProfileId} value={profile.applicabilityProfileId}>
                      {profile.label}
                    </option>
                  ))}
                </select>
              </label>
              <label htmlFor="requirement-mapping-level" className="block text-xs text-slate-400">
                Requirement level
                <select
                  id="requirement-mapping-level"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                  value={requirementLevel}
                  onChange={(event) => onRequirementLevelChange(event.target.value)}
                >
                  <option value="required">Required</option>
                  <option value="recommended">Recommended</option>
                </select>
              </label>
              <label htmlFor="requirement-mapping-target-type" className="block text-xs text-slate-400">
                Target type
                <select
                  id="requirement-mapping-target-type"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                  value={requirementTargetType}
                  onChange={(event) => onRequirementTargetTypeChange(event.target.value as 'program' | 'definition')}
                >
                  <option value="program">Program</option>
                  <option value="definition">Definition</option>
                </select>
              </label>
              <label htmlFor="requirement-mapping-target" className="block text-xs text-slate-400">
                Target
                <select
                  id="requirement-mapping-target"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                  value={requirementTargetId}
                  onChange={(event) => onRequirementTargetIdChange(event.target.value)}
                >
                  <option value="">Select…</option>
                  {targets.map((target) => (
                    <option key={target.id} value={target.id}>
                      {target.label}
                    </option>
                  ))}
                </select>
              </label>
              <div className="sm:col-span-2">
                <button
                  type="button"
                  className="rounded bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
                  disabled={
                    isCreatingRequirement ||
                    !requirementKey.trim() ||
                    !requirementLabel.trim() ||
                    !requirementTargetId ||
                    !requirementProfileId
                  }
                  onClick={onCreateRequirement}
                >
                  {isCreatingRequirement ? 'Saving requirement…' : 'Save requirement mapping'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      <div className="mt-6 grid gap-6 lg:grid-cols-2">
        <div>
          <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Applicability profiles</h3>
          {profiles.length === 0 ? (
            <p className="mt-2 text-sm text-slate-500">No profiles yet.</p>
          ) : (
            <ul className="mt-2 space-y-2">
              {profiles.map((profile) => (
                <li
                  key={profile.applicabilityProfileId}
                  className="flex flex-wrap items-start justify-between gap-2 rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm"
                >
                  <div>
                    <p className="font-medium text-slate-100">{profile.label}</p>
                    {profile.description ? <p className="mt-1 text-xs text-slate-500">{profile.description}</p> : null}
                  </div>
                  {canManage ? (
                    <button
                      type="button"
                      className="text-xs text-red-300 hover:text-red-200 disabled:opacity-50"
                      disabled={deletingProfileId === profile.applicabilityProfileId}
                      onClick={() => onDeleteProfile(profile.applicabilityProfileId)}
                    >
                      Remove
                    </button>
                  ) : null}
                </li>
              ))}
            </ul>
          )}
        </div>

        <div>
          <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Requirement mappings</h3>
          {requirements.length === 0 ? (
            <p className="mt-2 text-sm text-slate-500">No requirement mappings yet.</p>
          ) : (
            <ul className="mt-2 space-y-4">
              {Object.entries(groupedRequirements).map(([group, rows]) => (
                <li key={group}>
                  <p className="text-xs font-medium uppercase tracking-wide text-violet-300">{group}</p>
                  <ul className="mt-1 space-y-1">
                    {rows.map((row) => (
                      <li
                        key={row.requirementId}
                        className="flex flex-wrap items-center justify-between gap-2 rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm"
                      >
                        <span className="text-slate-200">
                          {row.label} · {row.trainingProgramName ?? row.trainingDefinitionName ?? '—'} ·{' '}
                          {row.requirementLevel}
                        </span>
                        {canManage ? (
                          <span className="flex flex-wrap gap-2">
                            <button
                              type="button"
                              className="text-xs text-sky-300 hover:text-sky-200 disabled:opacity-50"
                              disabled={syncingRequirementId === row.requirementId}
                              onClick={() => onSyncRequirement(row.requirementId)}
                            >
                              Sync to matrix
                            </button>
                            <button
                              type="button"
                              className="text-xs text-red-300 hover:text-red-200 disabled:opacity-50"
                              disabled={deletingRequirementId === row.requirementId}
                              onClick={() => onDeleteRequirement(row.requirementId)}
                            >
                              Remove
                            </button>
                          </span>
                        ) : null}
                      </li>
                    ))}
                  </ul>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  )
}

export function buildRequirementCreatePayload(
  requirementKey: string,
  requirementLabel: string,
  requirementSource: string,
  requirementSourceKey: string,
  requirementTargetType: 'program' | 'definition',
  requirementTargetId: string,
  requirementProfileId: string,
  requirementLevel: string,
): CreateTrainingRequirementRequest {
  return {
    requirementKey: requirementKey.trim(),
    label: requirementLabel.trim(),
    requirementSource,
    sourceKey: requirementSourceKey.trim() || null,
    trainingProgramId: requirementTargetType === 'program' ? requirementTargetId : null,
    trainingDefinitionId: requirementTargetType === 'definition' ? requirementTargetId : null,
    applicabilityProfileId: requirementProfileId,
    requirementLevel,
    sortOrder: 0,
  }
}
