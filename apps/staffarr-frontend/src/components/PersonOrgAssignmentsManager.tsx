import { type FormEvent, useMemo, useState } from 'react'
import { ApiErrorCallout } from '@stl/shared-ui'
import type { OrgUnitAssignmentResponse, OrgUnitResponse } from '../api/types'

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
  onCreate: (request: {
    siteOrgUnitId: string
    departmentOrgUnitId: string
    teamOrgUnitId: string
    positionOrgUnitId: string
  }) => Promise<void>
  onUpdate: (
    assignmentId: string,
    request: {
      siteOrgUnitId: string
      departmentOrgUnitId: string
      teamOrgUnitId: string
      positionOrgUnitId: string
    },
  ) => Promise<void>
  onStatusChange: (assignmentId: string, status: 'active' | 'inactive') => Promise<void>
}

function byType(orgUnits: OrgUnitResponse[], unitType: string): OrgUnitResponse[] {
  return orgUnits
    .filter((x) => x.status === 'active' && x.unitType.toLowerCase() === unitType)
    .sort((a, b) => a.name.localeCompare(b.name))
}

function displayUnitName(orgUnits: OrgUnitResponse[], orgUnitId: string): string {
  return orgUnits.find((x) => x.orgUnitId === orgUnitId)?.name ?? orgUnitId
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
  const siteUnits = useMemo(() => byType(orgUnits, 'site'), [orgUnits])
  const departmentUnits = useMemo(() => byType(orgUnits, 'department'), [orgUnits])
  const teamUnits = useMemo(() => byType(orgUnits, 'team'), [orgUnits])
  const positionUnits = useMemo(() => byType(orgUnits, 'position'), [orgUnits])
  const normalizedError = formatAssignmentMutationError(actionErrorMessage)
  const [selectedAssignmentId, setSelectedAssignmentId] = useState<string | null>(null)

  const [createSiteId, setCreateSiteId] = useState('')
  const [createDepartmentId, setCreateDepartmentId] = useState('')
  const [createTeamId, setCreateTeamId] = useState('')
  const [createPositionId, setCreatePositionId] = useState('')

  const [editSiteId, setEditSiteId] = useState('')
  const [editDepartmentId, setEditDepartmentId] = useState('')
  const [editTeamId, setEditTeamId] = useState('')
  const [editPositionId, setEditPositionId] = useState('')

  const selected = selectedAssignmentId
    ? assignments.find((x) => x.assignmentId === selectedAssignmentId) ?? null
    : null

  const hasAllUnitTypes =
    siteUnits.length > 0 && departmentUnits.length > 0 && teamUnits.length > 0 && positionUnits.length > 0

  const pickForEdit = (assignment: OrgUnitAssignmentResponse) => {
    setSelectedAssignmentId(assignment.assignmentId)
    setEditSiteId(assignment.siteOrgUnitId)
    setEditDepartmentId(assignment.departmentOrgUnitId)
    setEditTeamId(assignment.teamOrgUnitId)
    setEditPositionId(assignment.positionOrgUnitId)
  }

  const handleCreate = async (event: FormEvent) => {
    event.preventDefault()
    await onCreate({
      siteOrgUnitId: createSiteId,
      departmentOrgUnitId: createDepartmentId,
      teamOrgUnitId: createTeamId,
      positionOrgUnitId: createPositionId,
    })
  }

  const handleUpdate = async (event: FormEvent) => {
    event.preventDefault()
    if (!selected) {
      return
    }

    await onUpdate(selected.assignmentId, {
      siteOrgUnitId: editSiteId,
      departmentOrgUnitId: editDepartmentId,
      teamOrgUnitId: editTeamId,
      positionOrgUnitId: editPositionId,
    })
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-medium text-slate-300">Org-unit assignments</h2>
        <span className={`text-xs ${canManage ? 'text-emerald-300' : 'text-slate-500'}`}>
          {canManage ? 'Write enabled' : 'Read only'}
        </span>
      </div>
      <p className="mt-2 text-xs text-slate-500">
        Managing assignments for {personDisplayName} ({personId})
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
            <li key={assignment.assignmentId} className="flex items-center justify-between py-3 text-sm">
              <button
                type="button"
                disabled={!canManage}
                onClick={() => pickForEdit(assignment)}
                className="text-left text-white hover:text-sky-300 disabled:text-slate-200"
              >
                {displayUnitName(orgUnits, assignment.siteOrgUnitId)} /{' '}
                {displayUnitName(orgUnits, assignment.departmentOrgUnitId)} /{' '}
                {displayUnitName(orgUnits, assignment.teamOrgUnitId)} /{' '}
                {displayUnitName(orgUnits, assignment.positionOrgUnitId)}
              </button>
              <span className="text-xs uppercase tracking-wide text-slate-500">{assignment.status}</span>
            </li>
          ))}
        </ul>
      ) : null}

      {canManage && !isLoading && !isError ? (
        <div className="mt-6 grid gap-6 lg:grid-cols-2">
          <form className="space-y-3" onSubmit={handleCreate}>
            <h3 className="text-sm font-medium text-slate-300">Create assignment</h3>
            {!hasAllUnitTypes ? (
              <p className="text-xs text-amber-300">Create active site/department/team/position org units first.</p>
            ) : null}
            <label htmlFor="create-assignment-site" className="block text-sm text-slate-300">
              Site
              <select
                id="create-assignment-site"
                value={createSiteId}
                onChange={(event) => setCreateSiteId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
                disabled={!hasAllUnitTypes}
              >
                <option value="">Select site</option>
                {siteUnits.map((unit) => (
                  <option key={unit.orgUnitId} value={unit.orgUnitId}>
                    {unit.name}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="create-assignment-department" className="block text-sm text-slate-300">
              Department
              <select
                id="create-assignment-department"
                value={createDepartmentId}
                onChange={(event) => setCreateDepartmentId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
                disabled={!hasAllUnitTypes}
              >
                <option value="">Select department</option>
                {departmentUnits.map((unit) => (
                  <option key={unit.orgUnitId} value={unit.orgUnitId}>
                    {unit.name}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="create-assignment-team" className="block text-sm text-slate-300">
              Team
              <select
                id="create-assignment-team"
                value={createTeamId}
                onChange={(event) => setCreateTeamId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
                disabled={!hasAllUnitTypes}
              >
                <option value="">Select team</option>
                {teamUnits.map((unit) => (
                  <option key={unit.orgUnitId} value={unit.orgUnitId}>
                    {unit.name}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="create-assignment-position" className="block text-sm text-slate-300">
              Position
              <select
                id="create-assignment-position"
                value={createPositionId}
                onChange={(event) => setCreatePositionId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
                disabled={!hasAllUnitTypes}
              >
                <option value="">Select position</option>
                {positionUnits.map((unit) => (
                  <option key={unit.orgUnitId} value={unit.orgUnitId}>
                    {unit.name}
                  </option>
                ))}
              </select>
            </label>
            <button
              type="submit"
              className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
              disabled={isSubmitting || !hasAllUnitTypes}
            >
              {isSubmitting ? 'Saving…' : 'Create assignment'}
            </button>
          </form>

          <form className="space-y-3" onSubmit={handleUpdate}>
            <h3 className="text-sm font-medium text-slate-300">Edit selected assignment</h3>
            {!selected ? <p className="text-sm text-slate-500">Select an assignment from the list to edit.</p> : null}
            <label htmlFor="edit-assignment-site" className="block text-sm text-slate-300">
              Site
              <select
                id="edit-assignment-site"
                value={editSiteId}
                onChange={(event) => setEditSiteId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
                disabled={!selected}
              >
                <option value="">Select site</option>
                {siteUnits.map((unit) => (
                  <option key={unit.orgUnitId} value={unit.orgUnitId}>
                    {unit.name}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="edit-assignment-department" className="block text-sm text-slate-300">
              Department
              <select
                id="edit-assignment-department"
                value={editDepartmentId}
                onChange={(event) => setEditDepartmentId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
                disabled={!selected}
              >
                <option value="">Select department</option>
                {departmentUnits.map((unit) => (
                  <option key={unit.orgUnitId} value={unit.orgUnitId}>
                    {unit.name}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="edit-assignment-team" className="block text-sm text-slate-300">
              Team
              <select
                id="edit-assignment-team"
                value={editTeamId}
                onChange={(event) => setEditTeamId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
                disabled={!selected}
              >
                <option value="">Select team</option>
                {teamUnits.map((unit) => (
                  <option key={unit.orgUnitId} value={unit.orgUnitId}>
                    {unit.name}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="edit-assignment-position" className="block text-sm text-slate-300">
              Position
              <select
                id="edit-assignment-position"
                value={editPositionId}
                onChange={(event) => setEditPositionId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
                disabled={!selected}
              >
                <option value="">Select position</option>
                {positionUnits.map((unit) => (
                  <option key={unit.orgUnitId} value={unit.orgUnitId}>
                    {unit.name}
                  </option>
                ))}
              </select>
            </label>
            <div className="flex gap-3">
              <button
                type="submit"
                className="rounded bg-slate-700 px-3 py-2 text-sm text-white disabled:opacity-50"
                disabled={!selected || isSubmitting}
              >
                Save changes
              </button>
              <button
                type="button"
                className="rounded bg-amber-700 px-3 py-2 text-sm text-white disabled:opacity-50"
                onClick={() =>
                  selected
                    ? onStatusChange(selected.assignmentId, selected.status === 'active' ? 'inactive' : 'active')
                    : null
                }
                disabled={!selected || isSubmitting}
              >
                {selected?.status === 'active' ? 'Deactivate' : 'Activate'}
              </button>
            </div>
          </form>
        </div>
      ) : !isLoading && !isError ? (
        <p className="mt-4 text-xs text-slate-500">Your role does not include org assignment write permission.</p>
      ) : null}
    </section>
  )
}
