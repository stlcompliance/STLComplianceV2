import { type Dispatch, type FormEvent, type SetStateAction, useEffect, useMemo, useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import type {
  CreateOrgUnitAssignmentRequest,
  OrgUnitAssignmentResponse,
  OrgUnitResponse,
  UpdateOrgUnitAssignmentRequest,
  UpdateOrgUnitAssignmentStatusRequest,
} from '../api/types'

interface PersonOrgAssignmentsManagerProps {
  personId: string
  personDisplayName: string
  orgUnits: OrgUnitResponse[]
  assignments: OrgUnitAssignmentResponse[]
  isLoading?: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
  canManage: boolean
  isSubmitting: boolean
  actionErrorMessage: string | null
  onCreate: (request: CreateOrgUnitAssignmentRequest) => Promise<void>
  onUpdate: (assignmentId: string, request: UpdateOrgUnitAssignmentRequest) => Promise<void>
  onStatusChange: (
    assignmentId: string,
    request: UpdateOrgUnitAssignmentStatusRequest,
  ) => Promise<void>
}

type EditableAssignmentStatus = 'planned' | 'active'

type AssignmentDraft = {
  siteOrgUnitId: string
  departmentOrgUnitId: string
  teamOrgUnitId: string
  positionOrgUnitId: string
  status: EditableAssignmentStatus
  isPrimary: boolean
  effectiveAt: string
  endsAt: string
  reason: string
}

function humanize(value: string): string {
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (character) => character.toUpperCase())
}

function matchesAllowedStatuses(status: OrgUnitResponse['status'], allowedStatuses: OrgUnitResponse['status'][]): boolean {
  return allowedStatuses.includes(status)
}

function isDescendantOrSelf(
  nodeId: string,
  ancestorId: string,
  byId: Map<string, OrgUnitResponse>,
): boolean {
  if (nodeId === ancestorId) {
    return true
  }

  let cursor = byId.get(nodeId)?.parentOrgUnitId ?? null
  while (cursor) {
    if (cursor === ancestorId) {
      return true
    }

    cursor = byId.get(cursor)?.parentOrgUnitId ?? null
  }

  return false
}

function toIsoDateTime(value: string): string | null {
  if (!value) {
    return null
  }

  return new Date(value).toISOString()
}

function fromIsoDateTime(value: string | null): string {
  if (!value) {
    return ''
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }

  const offsetMs = date.getTimezoneOffset() * 60_000
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16)
}

function emptyDraft(): AssignmentDraft {
  return {
    siteOrgUnitId: '',
    departmentOrgUnitId: '',
    teamOrgUnitId: '',
    positionOrgUnitId: '',
    status: 'active',
    isPrimary: true,
    effectiveAt: '',
    endsAt: '',
    reason: '',
  }
}

function hydrateDraft(assignment: OrgUnitAssignmentResponse): AssignmentDraft {
  return {
    siteOrgUnitId: assignment.siteOrgUnitId,
    departmentOrgUnitId: assignment.departmentOrgUnitId,
    teamOrgUnitId: assignment.teamOrgUnitId,
    positionOrgUnitId: assignment.positionOrgUnitId,
    status: assignment.status === 'planned' ? 'planned' : 'active',
    isPrimary: assignment.isPrimary ?? false,
    effectiveAt: fromIsoDateTime(assignment.effectiveAt ?? null),
    endsAt: fromIsoDateTime(assignment.endsAt ?? null),
    reason: assignment.reason ?? '',
  }
}

function buildPlacementOptions(
  orgUnits: OrgUnitResponse[],
  byId: Map<string, OrgUnitResponse>,
  draft: AssignmentDraft,
) {
  const allowedStatuses: OrgUnitResponse['status'][] =
    draft.status === 'active' ? ['active'] : ['planned', 'active']
  const sites = orgUnits
    .filter((unit) => unit.unitType === 'site' && matchesAllowedStatuses(unit.status, allowedStatuses))
    .sort((left, right) => left.name.localeCompare(right.name))
  const departments = orgUnits
    .filter((unit) =>
      unit.unitType === 'department'
      && matchesAllowedStatuses(unit.status, allowedStatuses)
      && (!draft.siteOrgUnitId || isDescendantOrSelf(unit.orgUnitId, draft.siteOrgUnitId, byId)),
    )
    .sort((left, right) => left.name.localeCompare(right.name))
  const teams = orgUnits
    .filter((unit) =>
      unit.unitType === 'team'
      && matchesAllowedStatuses(unit.status, allowedStatuses)
      && (!draft.departmentOrgUnitId || isDescendantOrSelf(unit.orgUnitId, draft.departmentOrgUnitId, byId)),
    )
    .sort((left, right) => left.name.localeCompare(right.name))
  const positions = orgUnits
    .filter((unit) =>
      unit.unitType === 'position'
      && matchesAllowedStatuses(unit.status, allowedStatuses)
      && (!draft.teamOrgUnitId || isDescendantOrSelf(unit.orgUnitId, draft.teamOrgUnitId, byId)),
    )
    .sort((left, right) => left.name.localeCompare(right.name))

  return { sites, departments, teams, positions }
}

function toPickerOptions(orgUnits: OrgUnitResponse[]): PickerOption[] {
  return orgUnits.map((unit) => ({
    value: unit.orgUnitId,
    label: unit.name,
  }))
}

function displayUnitName(orgUnits: OrgUnitResponse[], orgUnitId: string): string {
  return orgUnits.find((unit) => unit.orgUnitId === orgUnitId)?.name ?? orgUnitId
}

function formatWhen(value: string | null): string | null {
  if (!value) {
    return null
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString()
}

function serializeDraft(draft: AssignmentDraft): CreateOrgUnitAssignmentRequest {
  return {
    siteOrgUnitId: draft.siteOrgUnitId,
    departmentOrgUnitId: draft.departmentOrgUnitId,
    teamOrgUnitId: draft.teamOrgUnitId,
    positionOrgUnitId: draft.positionOrgUnitId,
    status: draft.status,
    isPrimary: draft.isPrimary,
    effectiveAt: toIsoDateTime(draft.effectiveAt),
    endsAt: toIsoDateTime(draft.endsAt),
    reason: draft.reason.trim() || null,
  }
}

export function formatAssignmentMutationError(errorMessage: string | null): string | null {
  if (!errorMessage) {
    return null
  }

  const normalized = errorMessage.toLowerCase()
  if (normalized.includes('"status":403') || normalized.includes('forbidden')) {
    return `Forbidden: ${errorMessage}`
  }

  if (normalized.includes('"status":409') || normalized.includes('conflict')) {
    return `Conflict: ${errorMessage}`
  }

  if (normalized.includes('"status":400') || normalized.includes('validation')) {
    return `Validation: ${errorMessage}`
  }

  return errorMessage
}

export function PersonOrgAssignmentsManager({
  personId,
  personDisplayName,
  orgUnits,
  assignments,
  isLoading = false,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
  canManage,
  isSubmitting,
  actionErrorMessage,
  onCreate,
  onUpdate,
  onStatusChange,
}: PersonOrgAssignmentsManagerProps) {
  const [selectedAssignmentId, setSelectedAssignmentId] = useState<string | null>(null)
  const [createDraft, setCreateDraft] = useState<AssignmentDraft>(() => emptyDraft())
  const [editDraft, setEditDraft] = useState<AssignmentDraft>(() => emptyDraft())
  const normalizedError = formatAssignmentMutationError(actionErrorMessage)
  const byId = useMemo(() => new Map(orgUnits.map((unit) => [unit.orgUnitId, unit])), [orgUnits])
  const selected = selectedAssignmentId
    ? assignments.find((assignment) => assignment.assignmentId === selectedAssignmentId) ?? null
    : null
  const createOptions = useMemo(
    () => buildPlacementOptions(orgUnits, byId, createDraft),
    [byId, createDraft, orgUnits],
  )
  const editOptions = useMemo(
    () => buildPlacementOptions(orgUnits, byId, editDraft),
    [byId, editDraft, orgUnits],
  )

  useEffect(() => {
    if (selected) {
      setEditDraft(hydrateDraft(selected))
    }
  }, [selected])

  const selectedCreateSiteOption = useMemo(
    () => toPickerOptions(createOptions.sites).find((option) => option.value === createDraft.siteOrgUnitId),
    [createDraft.siteOrgUnitId, createOptions.sites],
  )
  const selectedCreateDepartmentOption = useMemo(
    () =>
      toPickerOptions(createOptions.departments).find(
        (option) => option.value === createDraft.departmentOrgUnitId,
      ),
    [createDraft.departmentOrgUnitId, createOptions.departments],
  )
  const selectedCreateTeamOption = useMemo(
    () => toPickerOptions(createOptions.teams).find((option) => option.value === createDraft.teamOrgUnitId),
    [createDraft.teamOrgUnitId, createOptions.teams],
  )
  const selectedCreatePositionOption = useMemo(
    () =>
      toPickerOptions(createOptions.positions).find(
        (option) => option.value === createDraft.positionOrgUnitId,
      ),
    [createDraft.positionOrgUnitId, createOptions.positions],
  )
  const selectedEditSiteOption = useMemo(
    () => toPickerOptions(editOptions.sites).find((option) => option.value === editDraft.siteOrgUnitId),
    [editDraft.siteOrgUnitId, editOptions.sites],
  )
  const selectedEditDepartmentOption = useMemo(
    () =>
      toPickerOptions(editOptions.departments).find(
        (option) => option.value === editDraft.departmentOrgUnitId,
      ),
    [editDraft.departmentOrgUnitId, editOptions.departments],
  )
  const selectedEditTeamOption = useMemo(
    () => toPickerOptions(editOptions.teams).find((option) => option.value === editDraft.teamOrgUnitId),
    [editDraft.teamOrgUnitId, editOptions.teams],
  )
  const selectedEditPositionOption = useMemo(
    () =>
      toPickerOptions(editOptions.positions).find(
        (option) => option.value === editDraft.positionOrgUnitId,
      ),
    [editDraft.positionOrgUnitId, editOptions.positions],
  )

  const hasAllUnitTypes =
    createOptions.sites.length > 0
    && createOptions.departments.length > 0
    && createOptions.teams.length > 0
    && createOptions.positions.length > 0

  const editChainChanged = Boolean(
    selected
    && (
      selected.siteOrgUnitId !== editDraft.siteOrgUnitId
      || selected.departmentOrgUnitId !== editDraft.departmentOrgUnitId
      || selected.teamOrgUnitId !== editDraft.teamOrgUnitId
      || selected.positionOrgUnitId !== editDraft.positionOrgUnitId
    ),
  )
  const editWillTransfer = Boolean(selected && selected.status === 'active' && editChainChanged)

  const handleCreate = async (event: FormEvent) => {
    event.preventDefault()
    await onCreate(serializeDraft(createDraft))
    setCreateDraft((current) => ({ ...emptyDraft(), status: current.status }))
  }

  const handleUpdate = async (event: FormEvent) => {
    event.preventDefault()
    if (!selected) {
      return
    }

    await onUpdate(selected.assignmentId, serializeDraft(editDraft) as UpdateOrgUnitAssignmentRequest)
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-medium text-slate-300">Org-unit assignments</h2>
        <span className={`text-xs ${canManage ? 'text-emerald-300' : 'text-[var(--color-text-muted)]'}`}>
          {canManage ? 'Write enabled' : 'Read only'}
        </span>
      </div>
      <p className="mt-2 text-xs text-[var(--color-text-muted)]">
        Managing placements for {personDisplayName} ({personId}). Active placement edits create transfer history instead
        of overwriting the current chain.
      </p>

      {normalizedError ? (
        <div className="mt-3">
          <ApiErrorCallout title="Org assignment update failed" message={normalizedError} />
        </div>
      ) : null}

      {isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Org assignments unavailable"
            message={readErrorMessage ?? 'Failed to load org assignments and org unit options.'}
            onRetry={onRetryRead}
            retryLabel="Retry assignments"
          />
        </div>
      ) : null}

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading linked org assignments…</p>
      ) : !isError && assignments.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">No linked site/department/team/position assignments.</p>
      ) : !isError ? (
        <ul className="mt-4 divide-y divide-slate-700">
          {assignments.map((assignment) => (
            <li key={assignment.assignmentId} className="flex items-start justify-between gap-4 py-3 text-sm">
              <button
                type="button"
                onClick={() => {
                  setSelectedAssignmentId(assignment.assignmentId)
                  setEditDraft(hydrateDraft(assignment))
                }}
                className="text-left text-white hover:text-sky-300"
              >
                <div className="font-medium">
                  {assignment.assignmentId === selectedAssignmentId ? 'Selected: ' : ''}
                  {displayUnitName(orgUnits, assignment.siteOrgUnitId)} /{' '}
                  {displayUnitName(orgUnits, assignment.departmentOrgUnitId)} /{' '}
                  {displayUnitName(orgUnits, assignment.teamOrgUnitId)} /{' '}
                  {displayUnitName(orgUnits, assignment.positionOrgUnitId)}
                </div>
                <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                  {assignment.isPrimary ? 'Primary placement · ' : ''}
                  {humanize(assignment.status)}
                  {assignment.effectiveAt ? ` · effective ${formatWhen(assignment.effectiveAt)}` : ''}
                  {assignment.endsAt ? ` · ends ${formatWhen(assignment.endsAt)}` : ''}
                </div>
                {assignment.reason ? (
                  <div className="mt-1 text-xs text-[var(--color-text-muted)]">{assignment.reason}</div>
                ) : null}
              </button>
              <span className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                {assignment.status}
              </span>
            </li>
          ))}
        </ul>
      ) : null}

      {canManage && !isLoading && !isError ? (
        <div className="mt-6 grid gap-6 xl:grid-cols-2">
          <form className="space-y-4" onSubmit={handleCreate}>
            <h3 className="text-sm font-medium text-slate-300">Create placement</h3>
            {!hasAllUnitTypes ? (
              <p className="text-xs text-amber-300">
                Create active/planned site, department, team, and position org units first.
              </p>
            ) : null}

            <PlacementFields
              draft={createDraft}
              setDraft={setCreateDraft}
              options={createOptions}
              selectedOptions={{
                site: selectedCreateSiteOption,
                department: selectedCreateDepartmentOption,
                team: selectedCreateTeamOption,
                position: selectedCreatePositionOption,
              }}
              prefix="create-assignment"
              disabled={isSubmitting || !hasAllUnitTypes}
            />

            <button
              type="submit"
              className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
              disabled={isSubmitting || !hasAllUnitTypes}
            >
              {isSubmitting ? 'Saving…' : 'Create placement'}
            </button>
          </form>

          <form className="space-y-4" onSubmit={handleUpdate}>
            <h3 className="text-sm font-medium text-slate-300">Edit selected placement</h3>
            {!selected ? (
              <p className="text-sm text-[var(--color-text-muted)]">Select a placement from the list to edit.</p>
            ) : null}

            {editWillTransfer ? (
              <div className="rounded-lg border border-amber-700/60 bg-amber-950/30 p-3 text-xs text-amber-100">
                Saving these chain changes will end the current active placement and create a successor row with the
                new site/department/team/position path.
              </div>
            ) : null}

            <PlacementFields
              draft={editDraft}
              setDraft={setEditDraft}
              options={editOptions}
              selectedOptions={{
                site: selectedEditSiteOption,
                department: selectedEditDepartmentOption,
                team: selectedEditTeamOption,
                position: selectedEditPositionOption,
              }}
              prefix="edit-assignment"
              disabled={!selected || isSubmitting || selected?.status === 'ended' || selected?.status === 'canceled'}
            />

            <div className="flex flex-wrap gap-3">
              <button
                type="submit"
                className="rounded bg-slate-700 px-3 py-2 text-sm text-white disabled:opacity-50"
                disabled={!selected || isSubmitting || selected?.status === 'ended' || selected?.status === 'canceled'}
              >
                Save changes
              </button>
              {selected?.status === 'planned' ? (
                <>
                  <button
                    type="button"
                    className="rounded border border-emerald-700 px-3 py-2 text-sm text-emerald-200 disabled:opacity-50"
                    onClick={() =>
                      onStatusChange(selected.assignmentId, {
                        status: 'active',
                        reason: editDraft.reason.trim() || null,
                      })
                    }
                    disabled={isSubmitting}
                  >
                    Activate now
                  </button>
                  <button
                    type="button"
                    className="rounded border border-amber-700 px-3 py-2 text-sm text-amber-200 disabled:opacity-50"
                    onClick={() =>
                      onStatusChange(selected.assignmentId, {
                        status: 'canceled',
                        endsAt: toIsoDateTime(editDraft.endsAt) ?? toIsoDateTime(editDraft.effectiveAt),
                        reason: editDraft.reason.trim() || null,
                      })
                    }
                    disabled={isSubmitting}
                  >
                    Cancel placement
                  </button>
                </>
              ) : null}
              {selected?.status === 'active' ? (
                <button
                  type="button"
                  className="rounded border border-amber-700 px-3 py-2 text-sm text-amber-200 disabled:opacity-50"
                  onClick={() =>
                    onStatusChange(selected.assignmentId, {
                      status: 'ended',
                      endsAt: toIsoDateTime(editDraft.endsAt),
                      reason: editDraft.reason.trim() || null,
                    })
                  }
                  disabled={isSubmitting}
                >
                  End placement
                </button>
              ) : null}
            </div>
          </form>
        </div>
      ) : !isLoading && !isError ? (
        <p className="mt-4 text-xs text-[var(--color-text-muted)]">Your role does not include org assignment write permission.</p>
      ) : null}
    </section>
  )
}

function PlacementFields({
  draft,
  setDraft,
  options,
  selectedOptions,
  prefix,
  disabled,
}: {
  draft: AssignmentDraft
  setDraft: Dispatch<SetStateAction<AssignmentDraft>>
  options: ReturnType<typeof buildPlacementOptions>
  selectedOptions: {
    site: PickerOption | undefined
    department: PickerOption | undefined
    team: PickerOption | undefined
    position: PickerOption | undefined
  }
  prefix: string
  disabled: boolean
}) {
  const siteOptions = useMemo(() => toPickerOptions(options.sites), [options.sites])
  const departmentOptions = useMemo(() => toPickerOptions(options.departments), [options.departments])
  const teamOptions = useMemo(() => toPickerOptions(options.teams), [options.teams])
  const positionOptions = useMemo(() => toPickerOptions(options.positions), [options.positions])

  return (
    <div className="space-y-4">
      <div className="grid gap-4 md:grid-cols-2">
        <label className="block text-sm text-slate-300">
          Placement status
          <select
            value={draft.status}
            onChange={(event) =>
              setDraft((current) => ({
                ...current,
                status: event.target.value as EditableAssignmentStatus,
              }))
            }
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
            disabled={disabled}
          >
            <option value="active">Active</option>
            <option value="planned">Planned</option>
          </select>
        </label>

        <label className="flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-950/50 px-3 py-2 text-sm text-slate-300">
          <input
            type="checkbox"
            checked={draft.isPrimary}
            onChange={(event) =>
              setDraft((current) => ({ ...current, isPrimary: event.target.checked }))
            }
            disabled={disabled}
          />
          Primary placement
        </label>

        <label className="block text-sm text-slate-300">
          Effective at
          <input
            type="datetime-local"
            value={draft.effectiveAt}
            onChange={(event) =>
              setDraft((current) => ({ ...current, effectiveAt: event.target.value }))
            }
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
            disabled={disabled}
          />
        </label>

        <label className="block text-sm text-slate-300">
          Ends at
          <input
            type="datetime-local"
            value={draft.endsAt}
            onChange={(event) =>
              setDraft((current) => ({ ...current, endsAt: event.target.value }))
            }
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
            disabled={disabled}
          />
        </label>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <label className="block text-sm text-slate-300">
          Site
          <div className="mt-1">
            <StaticSearchPicker
              id={`${prefix}-site`}
              label="Site"
              value={draft.siteOrgUnitId}
              onChange={(value) =>
                setDraft((current) => ({
                  ...current,
                  siteOrgUnitId: value,
                  departmentOrgUnitId: '',
                  teamOrgUnitId: '',
                  positionOrgUnitId: '',
                }))
              }
              options={siteOptions}
              placeholder="Search sites…"
              testId={`${prefix}-site-picker`}
              selectedOption={selectedOptions.site}
              disabled={disabled}
            />
          </div>
        </label>

        <label className="block text-sm text-slate-300">
          Department
          <div className="mt-1">
            <StaticSearchPicker
              id={`${prefix}-department`}
              label="Department"
              value={draft.departmentOrgUnitId}
              onChange={(value) =>
                setDraft((current) => ({
                  ...current,
                  departmentOrgUnitId: value,
                  teamOrgUnitId: '',
                  positionOrgUnitId: '',
                }))
              }
              options={departmentOptions}
              placeholder="Search departments…"
              testId={`${prefix}-department-picker`}
              selectedOption={selectedOptions.department}
              disabled={disabled}
            />
          </div>
        </label>

        <label className="block text-sm text-slate-300">
          Team
          <div className="mt-1">
            <StaticSearchPicker
              id={`${prefix}-team`}
              label="Team"
              value={draft.teamOrgUnitId}
              onChange={(value) =>
                setDraft((current) => ({
                  ...current,
                  teamOrgUnitId: value,
                  positionOrgUnitId: '',
                }))
              }
              options={teamOptions}
              placeholder="Search teams…"
              testId={`${prefix}-team-picker`}
              selectedOption={selectedOptions.team}
              disabled={disabled}
            />
          </div>
        </label>

        <label className="block text-sm text-slate-300">
          Position
          <div className="mt-1">
            <StaticSearchPicker
              id={`${prefix}-position`}
              label="Position"
              value={draft.positionOrgUnitId}
              onChange={(value) =>
                setDraft((current) => ({ ...current, positionOrgUnitId: value }))
              }
              options={positionOptions}
              placeholder="Search positions…"
              testId={`${prefix}-position-picker`}
              selectedOption={selectedOptions.position}
              disabled={disabled}
            />
          </div>
        </label>
      </div>

      <label className="block text-sm text-slate-300">
        Reason
        <textarea
          value={draft.reason}
          onChange={(event) => setDraft((current) => ({ ...current, reason: event.target.value }))}
          className="mt-1 min-h-24 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
          disabled={disabled}
        />
      </label>
    </div>
  )
}
