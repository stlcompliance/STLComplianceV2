import { type FormEvent, useMemo, useState } from 'react'
import { ApiErrorCallout } from '@stl/shared-ui'
import type { OrgUnitResponse } from '../api/types'

interface OrgHierarchyManagerProps {
  orgUnits: OrgUnitResponse[]
  isLoading?: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
  canManage: boolean
  isSubmitting: boolean
  actionErrorMessage: string | null
  onCreate: (request: { unitType: string; name: string; parentOrgUnitId: string | null }) => Promise<void>
  onUpdate: (orgUnitId: string, request: { unitType: string; name: string; parentOrgUnitId: string | null }) => Promise<void>
  onStatusChange: (orgUnitId: string, status: 'active' | 'inactive') => Promise<void>
}

interface OrgNode extends OrgUnitResponse {
  children: OrgNode[]
}

const WRITER_ROLES = new Set(['tenant_admin', 'staffarr_admin', 'hr_admin'])

function buildTree(orgUnits: OrgUnitResponse[]): OrgNode[] {
  const byId = new Map<string, OrgNode>()
  const roots: OrgNode[] = []
  for (const unit of orgUnits) {
    byId.set(unit.orgUnitId, { ...unit, children: [] })
  }

  for (const node of byId.values()) {
    if (node.parentOrgUnitId && byId.has(node.parentOrgUnitId)) {
      byId.get(node.parentOrgUnitId)!.children.push(node)
    } else {
      roots.push(node)
    }
  }

  const sortNodes = (nodes: OrgNode[]) => {
    nodes.sort((a, b) => a.name.localeCompare(b.name))
    for (const node of nodes) {
      sortNodes(node.children)
    }
  }
  sortNodes(roots)
  return roots
}

function flattenTree(nodes: OrgNode[], depth = 0): Array<{ node: OrgNode; depth: number }> {
  const rows: Array<{ node: OrgNode; depth: number }> = []
  for (const node of nodes) {
    rows.push({ node, depth })
    rows.push(...flattenTree(node.children, depth + 1))
  }
  return rows
}

export function canManageOrgHierarchy(roleKey: string, isPlatformAdmin: boolean): boolean {
  return isPlatformAdmin || WRITER_ROLES.has(roleKey)
}

export function OrgHierarchyManager({
  orgUnits,
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
}: OrgHierarchyManagerProps) {
  const [createUnitType, setCreateUnitType] = useState('department')
  const [createName, setCreateName] = useState('')
  const [createParentId, setCreateParentId] = useState<string>('')
  const [selectedOrgUnitId, setSelectedOrgUnitId] = useState<string | null>(null)
  const [editUnitType, setEditUnitType] = useState('department')
  const [editName, setEditName] = useState('')
  const [editParentId, setEditParentId] = useState<string>('')

  const orgTree = useMemo(() => buildTree(orgUnits), [orgUnits])
  const rows = useMemo(() => flattenTree(orgTree), [orgTree])

  const selected = selectedOrgUnitId ? orgUnits.find((x) => x.orgUnitId === selectedOrgUnitId) ?? null : null

  const selectableParents = selected
    ? rows.filter((x) => x.node.orgUnitId !== selected.orgUnitId).map((x) => x.node)
    : rows.map((x) => x.node)

  const handlePickForEdit = (orgUnit: OrgUnitResponse) => {
    setSelectedOrgUnitId(orgUnit.orgUnitId)
    setEditUnitType(orgUnit.unitType)
    setEditName(orgUnit.name)
    setEditParentId(orgUnit.parentOrgUnitId ?? '')
  }

  const handleCreate = async (event: FormEvent) => {
    event.preventDefault()
    await onCreate({
      unitType: createUnitType,
      name: createName,
      parentOrgUnitId: createParentId || null,
    })
    setCreateName('')
    setCreateParentId('')
  }

  const handleUpdate = async (event: FormEvent) => {
    event.preventDefault()
    if (!selected) {
      return
    }

    await onUpdate(selected.orgUnitId, {
      unitType: editUnitType,
      name: editName,
      parentOrgUnitId: editParentId || null,
    })
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-medium text-slate-300">Org hierarchy management</h2>
        <span className={`text-xs ${canManage ? 'text-emerald-300' : 'text-slate-500'}`}>
          {canManage ? 'Write enabled' : 'Read only'}
        </span>
      </div>
      {actionErrorMessage ? (
        <div className="mt-3">
          <ApiErrorCallout title="Org hierarchy update failed" message={actionErrorMessage} />
        </div>
      ) : null}
      {isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Org hierarchy unavailable"
            message={readErrorMessage ?? 'Failed to load org hierarchy data.'}
            onRetry={onRetryRead}
            retryLabel="Retry org hierarchy"
          />
        </div>
      ) : null}

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading org hierarchy…</p>
      ) : !isError && rows.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">No org units configured yet.</p>
      ) : !isError ? (
        <ul className="mt-4 divide-y divide-slate-700">
          {rows.map(({ node, depth }) => (
            <li key={node.orgUnitId} className="flex items-center justify-between py-2 text-sm">
              <button
                type="button"
                onClick={() => handlePickForEdit(node)}
                className="text-left text-white hover:text-sky-300"
                style={{ paddingLeft: `${depth * 16}px` }}
                disabled={!canManage}
              >
                {node.name}
              </button>
              <span className="text-xs uppercase tracking-wide text-slate-500">
                {node.unitType} · {node.status}
              </span>
            </li>
          ))}
        </ul>
      ) : null}

      {canManage && !isLoading && !isError ? (
        <div className="mt-6 grid gap-6 lg:grid-cols-2">
          <form className="space-y-3" onSubmit={handleCreate}>
            <h3 className="text-sm font-medium text-slate-300">Create org unit</h3>
            <label htmlFor="create-org-unit-name" className="block text-sm text-slate-300">
              Org unit name
              <input
                id="create-org-unit-name"
                value={createName}
                onChange={(event) => setCreateName(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
              />
            </label>
            <label htmlFor="create-org-unit-type" className="block text-sm text-slate-300">
              Org unit type
              <input
                id="create-org-unit-type"
                value={createUnitType}
                onChange={(event) => setCreateUnitType(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
              />
            </label>
            <label htmlFor="create-org-unit-parent" className="block text-sm text-slate-300">
              Parent org unit
              <select
                id="create-org-unit-parent"
                value={createParentId}
                onChange={(event) => setCreateParentId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
              >
              <option value="">No parent (root)</option>
              {rows.map(({ node }) => (
                <option key={node.orgUnitId} value={node.orgUnitId}>
                  {node.name}
                </option>
              ))}
              </select>
            </label>
            <button
              type="submit"
              className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Saving…' : 'Create'}
            </button>
          </form>

          <form className="space-y-3" onSubmit={handleUpdate}>
            <h3 className="text-sm font-medium text-slate-300">Edit selected org unit</h3>
            {!selected ? <p className="text-sm text-slate-500">Select a unit from the hierarchy to edit.</p> : null}
            <label htmlFor="edit-org-unit-name" className="block text-sm text-slate-300">
              Org unit name
              <input
                id="edit-org-unit-name"
                value={editName}
                onChange={(event) => setEditName(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                disabled={!selected}
                required
              />
            </label>
            <label htmlFor="edit-org-unit-type" className="block text-sm text-slate-300">
              Org unit type
              <input
                id="edit-org-unit-type"
                value={editUnitType}
                onChange={(event) => setEditUnitType(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                disabled={!selected}
                required
              />
            </label>
            <label htmlFor="edit-org-unit-parent" className="block text-sm text-slate-300">
              Parent org unit
              <select
                id="edit-org-unit-parent"
                value={editParentId}
                onChange={(event) => setEditParentId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                disabled={!selected}
              >
              <option value="">No parent (root)</option>
              {selectableParents.map((unit) => (
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
                  selected ? onStatusChange(selected.orgUnitId, selected.status === 'active' ? 'inactive' : 'active') : null
                }
                disabled={!selected || isSubmitting}
              >
                {selected?.status === 'active' ? 'Deactivate' : 'Activate'}
              </button>
            </div>
          </form>
        </div>
      ) : !isLoading && !isError ? (
        <p className="mt-4 text-xs text-slate-500">Your role does not include org hierarchy write permission.</p>
      ) : null}
    </section>
  )
}
