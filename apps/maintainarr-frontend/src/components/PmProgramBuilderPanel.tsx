import { buildSemanticKey, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

import type {
  AssetResponse,
  AssetTypeResponse,
  PmProgramDetailResponse,
  PmProgramSummaryResponse,
  PmScheduleResponse,
} from '../api/types'

interface PmProgramBuilderPanelProps {
  mode: 'drawer' | 'details' | 'create'
  canManage: boolean
  programs: PmProgramSummaryResponse[]
  selectedProgram: PmProgramDetailResponse | null
  assetTypes: AssetTypeResponse[]
  assets: AssetResponse[]
  availableSchedules: PmScheduleResponse[]
  isLoading: boolean
  isDetailLoading: boolean
  isSchedulesLoading: boolean
  programKey: string
  programName: string
  programDescription: string
  scopeType: string
  selectedAssetTypeId: string
  selectedAssetId: string
  selectedProgramId: string
  selectedScheduleIds: string[]
  onProgramKeyChange: (value: string) => void
  onProgramNameChange: (value: string) => void
  onProgramDescriptionChange: (value: string) => void
  onScopeTypeChange: (value: string) => void
  onSelectedAssetTypeIdChange: (value: string) => void
  onSelectedAssetIdChange: (value: string) => void
  onSelectedProgramIdChange: (value: string) => void
  onSelectedScheduleIdsChange: (value: string[]) => void
  onCreateProgram: () => void
  onSaveSchedules: () => void
  onActivateProgram: () => void
  isCreatingProgram: boolean
  isSavingSchedules: boolean
}

export function PmProgramBuilderPanel({
  mode,
  canManage,
  programs,
  selectedProgram,
  assetTypes,
  assets,
  availableSchedules,
  isLoading,
  isDetailLoading,
  isSchedulesLoading,
  programKey,
  programName,
  programDescription,
  scopeType,
  selectedAssetTypeId,
  selectedAssetId,
  selectedProgramId,
  selectedScheduleIds,
  onProgramKeyChange,
  onProgramNameChange,
  onProgramDescriptionChange,
  onScopeTypeChange,
  onSelectedAssetTypeIdChange,
  onSelectedAssetIdChange,
  onSelectedProgramIdChange,
  onSelectedScheduleIdsChange,
  onCreateProgram,
  onSaveSchedules,
  onActivateProgram,
  isCreatingProgram,
  isSavingSchedules,
}: PmProgramBuilderPanelProps) {
  const [showProgramKeyPolicy, setShowProgramKeyPolicy] = useState(false)
  void programKey
  const existingProgramKeys = programs.map((program) => program.programKey)
  const assetTypeOptions = useMemo<PickerOption[]>(
    () => assetTypes.map((type) => ({ value: type.assetTypeId, label: type.name })),
    [assetTypes],
  )
  const selectedAssetTypeOption = useMemo<PickerOption | undefined>(
    () => assetTypeOptions.find((option) => option.value === selectedAssetTypeId),
    [assetTypeOptions, selectedAssetTypeId],
  )
  const scopedAssets =
    scopeType === 'asset_type' && selectedAssetTypeId
      ? assets.filter((asset) => asset.assetTypeId === selectedAssetTypeId)
      : assets
  const assetOptions = useMemo<PickerOption[]>(
    () => scopedAssets.map((asset) => ({ value: asset.assetId, label: `${asset.assetTag} — ${asset.name}` })),
    [scopedAssets],
  )
  const selectedAssetOption = useMemo<PickerOption | undefined>(
    () => assetOptions.find((option) => option.value === selectedAssetId),
    [assetOptions, selectedAssetId],
  )
  const generatedProgramKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'inspection',
        kind: 'program',
        title: programName.trim(),
        existingKeys: existingProgramKeys,
        maxLength: 128,
      }),
    [existingProgramKeys, programName],
  )

  useEffect(() => {
    onProgramKeyChange(generatedProgramKey)
  }, [generatedProgramKey, onProgramKeyChange])

  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading PM programs…</p>
  }

  const toggleSchedule = (scheduleId: string) => {
    if (selectedScheduleIds.includes(scheduleId)) {
      onSelectedScheduleIdsChange(selectedScheduleIds.filter((id) => id !== scheduleId))
      return
    }
    onSelectedScheduleIdsChange([...selectedScheduleIds, scheduleId])
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <h2 className="text-lg font-semibold text-white">PM program builder</h2>
      <p className="mt-1 text-sm text-slate-400">
        Group PM schedules into programs scoped by asset type or individual asset.
      </p>

      {mode === 'create' && canManage ? (
        <div className="mt-6 grid gap-4 rounded-lg border border-slate-700 bg-slate-950/40 p-4 md:grid-cols-3">
          <div className="space-y-1 text-sm">
            <div className="text-xs text-slate-400">Reference is auto-generated from program name.</div>
            {!showProgramKeyPolicy ? (
              <button
                type="button"
                className="text-xs text-[var(--color-text-muted)] underline-offset-2 hover:text-slate-300 hover:underline"
                onClick={() => setShowProgramKeyPolicy(true)}
                disabled={isCreatingProgram}
              >
                Key policy
              </button>
            ) : null}
          </div>
          <label className="block text-sm" htmlFor="pmprogrambuilder-name">
          <span className="text-slate-300">Name</span>
          <input id="pmprogrambuilder-name"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
              value={programName}
              onChange={(e) => onProgramNameChange(e.target.value)}
            />
          </label>
          <label className="block text-sm" htmlFor="pmprogrambuilder-description">
          <span className="text-slate-300">Description</span>
          <input id="pmprogrambuilder-description"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
              value={programDescription}
              onChange={(e) => onProgramDescriptionChange(e.target.value)}
            />
          </label>
          <label className="block text-sm" htmlFor="pmprogrambuilder-scope-type">
          <span className="text-slate-300">Scope type</span>
          <select id="pmprogrambuilder-scope-type"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-3 py-2"
              value={scopeType}
              onChange={(e) => onScopeTypeChange(e.target.value)}
            >
              <option value="asset_type">Asset type</option>
              <option value="asset">Single asset</option>
            </select>
          </label>
          {scopeType === 'asset_type' ? (
            <StaticSearchPicker
              id="pmprogrambuilder-asset-type"
              label="Asset type"
              value={selectedAssetTypeId}
              onChange={onSelectedAssetTypeIdChange}
              options={assetTypeOptions}
              placeholder="Search asset types…"
              testId="pmprogrambuilder-asset-type-picker"
              selectedOption={selectedAssetTypeOption}
            />
          ) : (
            <StaticSearchPicker
              id="pmprogrambuilder-asset"
              label="Asset"
              value={selectedAssetId}
              onChange={onSelectedAssetIdChange}
              options={assetOptions}
              placeholder="Search assets…"
              testId="pmprogrambuilder-asset-picker"
              selectedOption={selectedAssetOption}
            />
          )}
          <div className="md:col-span-3">
            <button
              type="button"
              className="rounded bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
              disabled={isCreatingProgram || !programName.trim()}
              onClick={onCreateProgram}
            >
              {isCreatingProgram ? 'Creating…' : 'Create PM program'}
            </button>
          </div>
        </div>
      ) : null}

      <div className="mt-6">
        <h3 className="text-sm font-medium text-slate-300">Programs</h3>
        {programs.length === 0 ? (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">No PM programs yet.</p>
        ) : (
          <ul className="mt-2 divide-y divide-slate-800 rounded-lg border border-slate-700">
            {programs.map((program) => (
              <li key={program.pmProgramId}>
                <button
                  type="button"
                  className={`flex w-full items-center justify-between px-4 py-3 text-left text-sm hover:bg-slate-800/60 ${
                    selectedProgramId === program.pmProgramId ? 'bg-slate-800/80' : ''
                  }`}
                  onClick={() => onSelectedProgramIdChange(program.pmProgramId)}
                >
                  <span>
                    <span className="font-medium text-white">{program.name}</span>
                  </span>
                  <span className="text-slate-400">
                    {program.scopeType === 'asset_type'
                      ? program.assetTypeName ?? 'Asset type'
                      : program.assetTag ?? 'Asset'}{' '}
                    · {program.scheduleCount} schedules · {program.status}
                  </span>
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>

      {mode !== 'drawer' && selectedProgramId ? (
        <div className="mt-6 rounded-lg border border-slate-700 bg-slate-950/40 p-4">
          {isDetailLoading ? (
            <p className="text-sm text-slate-400">Loading program detail…</p>
          ) : selectedProgram ? (
            <>
              <div className="flex flex-wrap items-center justify-between gap-2">
                <h3 className="text-base font-medium text-white">{selectedProgram.name}</h3>
                <span className="rounded bg-slate-800 px-2 py-1 text-xs text-slate-300">{selectedProgram.status}</span>
              </div>
              <p className="mt-2 text-sm text-slate-400">{selectedProgram.description || 'No description.'}</p>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                Scope:{' '}
                {selectedProgram.scopeType === 'asset_type'
                  ? `Asset type ${selectedProgram.assetTypeName ?? ''}`
                  : `Asset ${selectedProgram.assetTag ?? ''} — ${selectedProgram.assetName ?? ''}`}
              </p>

              <div className="mt-4">
                <h4 className="text-sm font-medium text-slate-300">Assigned schedules</h4>
                {selectedProgram.schedules.length === 0 ? (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">No schedules assigned yet.</p>
                ) : (
                  <ul className="mt-2 space-y-1 text-sm text-slate-300">
                    {selectedProgram.schedules.map((schedule) => (
                        <li key={schedule.pmScheduleId}>
                        {schedule.name} ({schedule.assetTag}) · {schedule.dueStatus}
                      </li>
                    ))}
                  </ul>
                )}
              </div>

              {canManage ? (
                <div className="mt-6">
                  <h4 className="text-sm font-medium text-slate-300">Assign schedules</h4>
                  {isSchedulesLoading ? (
                    <p className="mt-2 text-sm text-slate-400">Loading available schedules…</p>
                  ) : availableSchedules.length === 0 ? (
                    <p className="mt-2 text-sm text-[var(--color-text-muted)]">No PM schedules match this program scope.</p>
                  ) : (
                    <ul className="mt-2 max-h-48 space-y-2 overflow-y-auto rounded border border-slate-700 p-3">
                      {availableSchedules.map((schedule) => (
                        <li key={schedule.pmScheduleId}>
                          <label className="flex items-center gap-2 text-sm text-slate-300">
                            <input id="pmprogrambuilder"
                              type="checkbox"
                              checked={selectedScheduleIds.includes(schedule.pmScheduleId)}
                              onChange={() => toggleSchedule(schedule.pmScheduleId)}
                            />
                            {schedule.name} ({schedule.assetTag})
                          </label>
                        </li>
                      ))}
                    </ul>
                  )}
                  <div className="mt-4 flex flex-wrap gap-3">
                    <button
                      type="button"
                      className="rounded bg-sky-700 px-4 py-2 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
                      disabled={isSavingSchedules}
                      onClick={onSaveSchedules}
                    >
                      {isSavingSchedules ? 'Saving…' : 'Save schedule assignments'}
                    </button>
                    {selectedProgram.status === 'draft' ? (
                      <button
                        type="button"
                        className="rounded bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
                        disabled={isSavingSchedules || selectedProgram.schedules.length === 0}
                        onClick={onActivateProgram}
                      >
                        Activate program
                      </button>
                    ) : null}
                  </div>
                </div>
              ) : null}
            </>
          ) : null}
        </div>
      ) : null}
    </section>
  )
}
