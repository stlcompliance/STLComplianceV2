import { type Dispatch, type FormEvent, type SetStateAction, useEffect, useMemo, useState } from 'react'
import {
  ApiErrorCallout,
  StaticSearchPicker,
  type PickerOption,
} from '@stl/shared-ui'
import type {
  CreateOrgUnitRequest,
  OrgUnitResponse,
  OrgUnitSiteType,
  OrgUnitStatus,
  OrgUnitTeamType,
  OrgUnitType,
  UpdateOrgUnitRequest,
} from '../api/types'

interface OrgHierarchyManagerProps {
  orgUnits: OrgUnitResponse[]
  peopleOptions?: Array<{ personId: string; displayName: string }>
  isLoading?: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
  canManage: boolean
  isSubmitting: boolean
  actionErrorMessage: string | null
  onCreate: (request: CreateOrgUnitRequest) => Promise<void>
  onUpdate: (orgUnitId: string, request: UpdateOrgUnitRequest) => Promise<void>
  onStatusChange: (orgUnitId: string, status: OrgUnitStatus) => Promise<void>
}

interface OrgNode extends OrgUnitResponse {
  children: OrgNode[]
}

type OrgUnitDraft = {
  unitType: OrgUnitType
  name: string
  code: string
  parentOrgUnitId: string
  status: OrgUnitStatus
  description: string
  managerPersonId: string
  effectiveStartDate: string
  effectiveEndDate: string
  siteType: OrgUnitSiteType | ''
  timezone: string
  phone: string
  emergencyContact: string
  teamType: OrgUnitTeamType | ''
  positionCode: string
  defaultSiteOrgUnitId: string
  complianceSensitive: boolean
  safetySensitive: boolean
  canSupervise: boolean
  canApprove: boolean
}

const WRITER_ROLES = new Set(['tenant_admin', 'staffarr_admin', 'hr_admin'])

const UNIT_TYPE_OPTIONS: Array<{ value: OrgUnitType; label: string }> = [
  { value: 'company', label: 'Company' },
  { value: 'division', label: 'Division' },
  { value: 'region', label: 'Region' },
  { value: 'business_unit', label: 'Business unit' },
  { value: 'cost_center', label: 'Cost center' },
  { value: 'site', label: 'Site' },
  { value: 'department', label: 'Department' },
  { value: 'team', label: 'Team' },
  { value: 'position', label: 'Position' },
  { value: 'other', label: 'Other' },
] as const

const STATUS_OPTIONS: Array<{ value: OrgUnitStatus; label: string }> = [
  { value: 'planned', label: 'Planned' },
  { value: 'active', label: 'Active' },
  { value: 'inactive', label: 'Inactive' },
  { value: 'archived', label: 'Archived' },
] as const

const SITE_TYPE_OPTIONS: Array<{ value: OrgUnitSiteType; label: string }> = [
  { value: 'office', label: 'Office' },
  { value: 'warehouse', label: 'Warehouse' },
  { value: 'plant', label: 'Plant' },
  { value: 'shop', label: 'Shop' },
  { value: 'yard', label: 'Yard' },
  { value: 'terminal', label: 'Terminal' },
  { value: 'customer_embedded', label: 'Customer embedded' },
  { value: 'mixed', label: 'Mixed use' },
  { value: 'other', label: 'Other' },
] as const

const TEAM_TYPE_OPTIONS: Array<{ value: OrgUnitTeamType; label: string }> = [
  { value: 'operational', label: 'Operational' },
  { value: 'maintenance', label: 'Maintenance' },
  { value: 'warehouse', label: 'Warehouse' },
  { value: 'dispatch', label: 'Dispatch' },
  { value: 'safety', label: 'Safety' },
  { value: 'quality', label: 'Quality' },
  { value: 'training', label: 'Training' },
  { value: 'admin', label: 'Admin' },
  { value: 'project', label: 'Project' },
  { value: 'emergency_response', label: 'Emergency response' },
] as const

const ROOT_ELIGIBLE_TYPES = new Set<OrgUnitType>([
  'company',
  'division',
  'region',
  'business_unit',
  'cost_center',
  'site',
  'other',
])

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
    nodes.sort((left, right) => left.name.localeCompare(right.name))
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

function isAllowedParentType(unitType: OrgUnitType, parentType: OrgUnitType | null): boolean {
  if (parentType == null) {
    return ROOT_ELIGIBLE_TYPES.has(unitType)
  }

  if (unitType === 'site') {
    return parentType !== 'site'
      && parentType !== 'department'
      && parentType !== 'team'
      && parentType !== 'position'
  }

  if (unitType === 'department') {
    return parentType === 'site'
  }

  if (unitType === 'team') {
    return parentType === 'department'
  }

  if (unitType === 'position') {
    return parentType === 'team'
  }

  return parentType !== 'site'
    && parentType !== 'department'
    && parentType !== 'team'
    && parentType !== 'position'
}

function humanize(value: string): string {
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (character) => character.toUpperCase())
}

function emptyDraft(unitType: OrgUnitType = 'site'): OrgUnitDraft {
  return {
    unitType,
    name: '',
    code: '',
    parentOrgUnitId: '',
    status: 'planned',
    description: '',
    managerPersonId: '',
    effectiveStartDate: '',
    effectiveEndDate: '',
    siteType: '',
    timezone: '',
    phone: '',
    emergencyContact: '',
    teamType: '',
    positionCode: '',
    defaultSiteOrgUnitId: '',
    complianceSensitive: false,
    safetySensitive: false,
    canSupervise: false,
    canApprove: false,
  }
}

function hydrateDraft(orgUnit: OrgUnitResponse): OrgUnitDraft {
  return {
    unitType: orgUnit.unitType,
    name: orgUnit.name,
    code: orgUnit.code ?? '',
    parentOrgUnitId: orgUnit.parentOrgUnitId ?? '',
    status: orgUnit.status,
    description: orgUnit.description ?? '',
    managerPersonId: orgUnit.managerPersonId ?? '',
    effectiveStartDate: orgUnit.effectiveStartDate?.slice(0, 10) ?? '',
    effectiveEndDate: orgUnit.effectiveEndDate?.slice(0, 10) ?? '',
    siteType: orgUnit.siteType ?? '',
    timezone: orgUnit.timezone ?? '',
    phone: orgUnit.phone ?? '',
    emergencyContact: orgUnit.emergencyContact ?? '',
    teamType: orgUnit.teamType ?? '',
    positionCode: orgUnit.positionCode ?? '',
    defaultSiteOrgUnitId: orgUnit.defaultSiteOrgUnitId ?? '',
    complianceSensitive: orgUnit.complianceSensitive ?? false,
    safetySensitive: orgUnit.safetySensitive ?? false,
    canSupervise: orgUnit.canSupervise ?? false,
    canApprove: orgUnit.canApprove ?? false,
  }
}

function toIsoDate(value: string): string | null {
  if (!value) {
    return null
  }

  return new Date(`${value}T00:00:00`).toISOString()
}

function serializeDraft(draft: OrgUnitDraft): CreateOrgUnitRequest {
  return {
    unitType: draft.unitType,
    name: draft.name.trim(),
    code: draft.code.trim() || null,
    parentOrgUnitId: draft.parentOrgUnitId || null,
    description: draft.description.trim() || null,
    managerPersonId: draft.managerPersonId || null,
    effectiveStartDate: toIsoDate(draft.effectiveStartDate),
    effectiveEndDate: toIsoDate(draft.effectiveEndDate),
    siteType: draft.siteType || null,
    timezone: draft.timezone.trim() || null,
    phone: draft.phone.trim() || null,
    emergencyContact: draft.emergencyContact.trim() || null,
    teamType: draft.teamType || null,
    positionCode: draft.positionCode.trim() || null,
    defaultSiteOrgUnitId: draft.defaultSiteOrgUnitId || null,
    complianceSensitive: draft.complianceSensitive,
    safetySensitive: draft.safetySensitive,
    canSupervise: draft.canSupervise,
    canApprove: draft.canApprove,
    status: draft.status,
  }
}

function isDescendantOf(
  candidateId: string,
  ancestorId: string,
  byId: Map<string, OrgUnitResponse>,
): boolean {
  let cursor = byId.get(candidateId)?.parentOrgUnitId ?? null
  while (cursor) {
    if (cursor === ancestorId) {
      return true
    }

    cursor = byId.get(cursor)?.parentOrgUnitId ?? null
  }

  return false
}

function toIndentedOptions(rows: Array<{ node: OrgNode; depth: number }>): PickerOption[] {
  return rows.map(({ node, depth }) => ({
    value: node.orgUnitId,
    label: `${'  '.repeat(depth)}${node.name}`,
  }))
}

function statusActionLabel(status: OrgUnitStatus): string {
  switch (status) {
    case 'planned':
      return 'Mark planned'
    case 'active':
      return 'Activate'
    case 'inactive':
      return 'Deactivate'
    case 'archived':
      return 'Archive'
    default:
      return humanize(status)
  }
}

function OrgUnitFormFields({
  prefix,
  draft,
  setDraft,
  parentOptions,
  managerOptions,
  defaultSiteOptions,
  disabled,
}: {
  prefix: string
  draft: OrgUnitDraft
  setDraft: Dispatch<SetStateAction<OrgUnitDraft>>
  parentOptions: PickerOption[]
  managerOptions: PickerOption[]
  defaultSiteOptions: PickerOption[]
  disabled: boolean
}) {
  const selectedParentOption = parentOptions.find((option) => option.value === draft.parentOrgUnitId)
  const selectedManagerOption = managerOptions.find((option) => option.value === draft.managerPersonId)
  const selectedDefaultSiteOption = defaultSiteOptions.find(
    (option) => option.value === draft.defaultSiteOrgUnitId,
  )
  const showsPlacementDefaults = draft.unitType === 'department' || draft.unitType === 'team' || draft.unitType === 'position'
  const showsPositionFlags = showsPlacementDefaults

  return (
    <div className="grid gap-4 md:grid-cols-2">
      <label htmlFor={`${prefix}-name`} className="block text-sm text-slate-300">
        Org unit name
        <input
          id={`${prefix}-name`}
          value={draft.name}
          onChange={(event) => setDraft((current) => ({ ...current, name: event.target.value }))}
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
          disabled={disabled}
          required
        />
      </label>

      <label htmlFor={`${prefix}-code`} className="block text-sm text-slate-300">
        Code
        <input
          id={`${prefix}-code`}
          value={draft.code}
          onChange={(event) => setDraft((current) => ({ ...current, code: event.target.value }))}
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
          disabled={disabled}
          placeholder="Optional. Auto-generated if blank."
        />
      </label>

      <label htmlFor={`${prefix}-unit-type`} className="block text-sm text-slate-300">
        Unit type
        <select
          id={`${prefix}-unit-type`}
          value={draft.unitType}
          onChange={(event) =>
            setDraft((current) => ({
              ...current,
              unitType: event.target.value as OrgUnitType,
              parentOrgUnitId: '',
              siteType: '',
              timezone: '',
              phone: '',
              emergencyContact: '',
              teamType: '',
              positionCode: '',
              defaultSiteOrgUnitId: '',
            }))
          }
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
          disabled={disabled}
        >
          {UNIT_TYPE_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>

      <label htmlFor={`${prefix}-status`} className="block text-sm text-slate-300">
        Lifecycle status
        <select
          id={`${prefix}-status`}
          value={draft.status}
          onChange={(event) =>
            setDraft((current) => ({ ...current, status: event.target.value as OrgUnitStatus }))
          }
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
          disabled={disabled}
        >
          {STATUS_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>

      <label className="block text-sm text-slate-300">
        Parent org unit
        <div className="mt-1">
          <StaticSearchPicker
            value={draft.parentOrgUnitId}
            onChange={(value) => setDraft((current) => ({ ...current, parentOrgUnitId: value }))}
            options={parentOptions}
            selectedOption={selectedParentOption}
            placeholder="No parent (root)"
            testId={`${prefix}-parent`}
            disabled={disabled}
          />
        </div>
      </label>

      <label className="block text-sm text-slate-300 md:col-span-2">
        Description
        <textarea
          value={draft.description}
          onChange={(event) => setDraft((current) => ({ ...current, description: event.target.value }))}
          className="mt-1 min-h-24 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
          disabled={disabled}
        />
      </label>

      <label className="block text-sm text-slate-300">
        Manager person
        <div className="mt-1">
          <StaticSearchPicker
            value={draft.managerPersonId}
            onChange={(value) => setDraft((current) => ({ ...current, managerPersonId: value }))}
            options={managerOptions}
            selectedOption={selectedManagerOption}
            placeholder="No manager"
            testId={`${prefix}-manager`}
            disabled={disabled}
          />
        </div>
      </label>

      <label htmlFor={`${prefix}-effective-start`} className="block text-sm text-slate-300">
        Effective start date
        <input
          id={`${prefix}-effective-start`}
          type="date"
          value={draft.effectiveStartDate}
          onChange={(event) =>
            setDraft((current) => ({ ...current, effectiveStartDate: event.target.value }))
          }
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
          disabled={disabled}
        />
      </label>

      <label htmlFor={`${prefix}-effective-end`} className="block text-sm text-slate-300">
        Effective end date
        <input
          id={`${prefix}-effective-end`}
          type="date"
          value={draft.effectiveEndDate}
          onChange={(event) =>
            setDraft((current) => ({ ...current, effectiveEndDate: event.target.value }))
          }
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
          disabled={disabled}
        />
      </label>

      {draft.unitType === 'site' ? (
        <>
          <label htmlFor={`${prefix}-site-type`} className="block text-sm text-slate-300">
            Site type
            <select
              id={`${prefix}-site-type`}
              value={draft.siteType}
              onChange={(event) =>
                setDraft((current) => ({ ...current, siteType: event.target.value as OrgUnitSiteType | '' }))
              }
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
              disabled={disabled}
            >
              <option value="">Unspecified</option>
              {SITE_TYPE_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>

          <label htmlFor={`${prefix}-timezone`} className="block text-sm text-slate-300">
            Timezone
            <input
              id={`${prefix}-timezone`}
              value={draft.timezone}
              onChange={(event) => setDraft((current) => ({ ...current, timezone: event.target.value }))}
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
              disabled={disabled}
            />
          </label>

          <label htmlFor={`${prefix}-phone`} className="block text-sm text-slate-300">
            Phone
            <input
              id={`${prefix}-phone`}
              value={draft.phone}
              onChange={(event) => setDraft((current) => ({ ...current, phone: event.target.value }))}
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
              disabled={disabled}
            />
          </label>

          <label htmlFor={`${prefix}-emergency-contact`} className="block text-sm text-slate-300">
            Emergency contact
            <input
              id={`${prefix}-emergency-contact`}
              value={draft.emergencyContact}
              onChange={(event) =>
                setDraft((current) => ({ ...current, emergencyContact: event.target.value }))
              }
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
              disabled={disabled}
            />
          </label>
        </>
      ) : null}

      {draft.unitType === 'team' ? (
        <label htmlFor={`${prefix}-team-type`} className="block text-sm text-slate-300">
          Team type
          <select
            id={`${prefix}-team-type`}
            value={draft.teamType}
            onChange={(event) =>
              setDraft((current) => ({ ...current, teamType: event.target.value as OrgUnitTeamType | '' }))
            }
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
            disabled={disabled}
          >
            <option value="">Unspecified</option>
            {TEAM_TYPE_OPTIONS.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
      ) : null}

      {showsPlacementDefaults ? (
        <label className="block text-sm text-slate-300">
          Default site
          <div className="mt-1">
            <StaticSearchPicker
              value={draft.defaultSiteOrgUnitId}
              onChange={(value) => setDraft((current) => ({ ...current, defaultSiteOrgUnitId: value }))}
              options={defaultSiteOptions}
              selectedOption={selectedDefaultSiteOption}
              placeholder="No default site"
              testId={`${prefix}-default-site`}
              disabled={disabled}
            />
          </div>
        </label>
      ) : null}

      {draft.unitType === 'position' ? (
        <label htmlFor={`${prefix}-position-code`} className="block text-sm text-slate-300">
          Position code
          <input
            id={`${prefix}-position-code`}
            value={draft.positionCode}
            onChange={(event) =>
              setDraft((current) => ({ ...current, positionCode: event.target.value }))
            }
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
            disabled={disabled}
          />
        </label>
      ) : null}

      {showsPositionFlags ? (
        <div className="rounded-lg border border-slate-700 bg-slate-950/50 p-4 md:col-span-2">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">Placement flags</p>
          <div className="mt-3 grid gap-3 sm:grid-cols-2">
            <label className="flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={draft.complianceSensitive}
                onChange={(event) =>
                  setDraft((current) => ({ ...current, complianceSensitive: event.target.checked }))
                }
                disabled={disabled}
              />
              Compliance sensitive
            </label>
            <label className="flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={draft.safetySensitive}
                onChange={(event) =>
                  setDraft((current) => ({ ...current, safetySensitive: event.target.checked }))
                }
                disabled={disabled}
              />
              Safety sensitive
            </label>
            <label className="flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={draft.canSupervise}
                onChange={(event) =>
                  setDraft((current) => ({ ...current, canSupervise: event.target.checked }))
                }
                disabled={disabled}
              />
              Can supervise
            </label>
            <label className="flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={draft.canApprove}
                onChange={(event) =>
                  setDraft((current) => ({ ...current, canApprove: event.target.checked }))
                }
                disabled={disabled}
              />
              Can approve
            </label>
          </div>
        </div>
      ) : null}
    </div>
  )
}

export function canManageOrgHierarchy(roleKey: string, isPlatformAdmin: boolean): boolean {
  return isPlatformAdmin || WRITER_ROLES.has(roleKey)
}

export function OrgHierarchyManager({
  orgUnits,
  peopleOptions = [],
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
  const [selectedOrgUnitId, setSelectedOrgUnitId] = useState<string | null>(null)
  const [createDraft, setCreateDraft] = useState<OrgUnitDraft>(() => emptyDraft('department'))
  const [editDraft, setEditDraft] = useState<OrgUnitDraft>(() => emptyDraft('department'))

  const orgTree = useMemo(() => buildTree(orgUnits), [orgUnits])
  const rows = useMemo(() => flattenTree(orgTree), [orgTree])
  const byId = useMemo(() => new Map(orgUnits.map((unit) => [unit.orgUnitId, unit])), [orgUnits])
  const selected = selectedOrgUnitId
    ? orgUnits.find((unit) => unit.orgUnitId === selectedOrgUnitId) ?? null
    : null
  const managerOptions = useMemo<PickerOption[]>(
    () => peopleOptions.map((person) => ({ value: person.personId, label: person.displayName })),
    [peopleOptions],
  )
  const defaultSiteOptions = useMemo<PickerOption[]>(
    () =>
      orgUnits
        .filter((unit) => unit.unitType === 'site' && (unit.status === 'planned' || unit.status === 'active'))
        .sort((left, right) => left.name.localeCompare(right.name))
        .map((unit) => ({ value: unit.orgUnitId, label: unit.name })),
    [orgUnits],
  )

  const createParentRows = useMemo(
    () => rows.filter(({ node }) => isAllowedParentType(createDraft.unitType, node.unitType)),
    [createDraft.unitType, rows],
  )
  const editParentRows = useMemo(() => {
    if (!selected) {
      return rows.filter(({ node }) => isAllowedParentType(editDraft.unitType, node.unitType))
    }

    return rows.filter(({ node }) =>
      node.orgUnitId !== selected.orgUnitId
      && !isDescendantOf(node.orgUnitId, selected.orgUnitId, byId)
      && isAllowedParentType(editDraft.unitType, node.unitType))
  }, [byId, editDraft.unitType, rows, selected])

  const createParentOptions = useMemo(() => toIndentedOptions(createParentRows), [createParentRows])
  const editParentOptions = useMemo(() => toIndentedOptions(editParentRows), [editParentRows])

  useEffect(() => {
    if (selected) {
      setEditDraft(hydrateDraft(selected))
    }
  }, [selected])

  useEffect(() => {
    if (createDraft.parentOrgUnitId && !createParentOptions.some((option) => option.value === createDraft.parentOrgUnitId)) {
      setCreateDraft((current) => ({ ...current, parentOrgUnitId: '' }))
    }
  }, [createDraft.parentOrgUnitId, createParentOptions])

  useEffect(() => {
    if (editDraft.parentOrgUnitId && !editParentOptions.some((option) => option.value === editDraft.parentOrgUnitId)) {
      setEditDraft((current) => ({ ...current, parentOrgUnitId: '' }))
    }
  }, [editDraft.parentOrgUnitId, editParentOptions])

  const handlePickForEdit = (orgUnit: OrgUnitResponse) => {
    setSelectedOrgUnitId(orgUnit.orgUnitId)
    setEditDraft(hydrateDraft(orgUnit))
  }

  const handleCreate = async (event: FormEvent) => {
    event.preventDefault()
    await onCreate(serializeDraft(createDraft))
    setCreateDraft(emptyDraft(createDraft.unitType))
  }

  const handleUpdate = async (event: FormEvent) => {
    event.preventDefault()
    if (!selected) {
      return
    }

    await onUpdate(selected.orgUnitId, serializeDraft(editDraft))
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-medium text-slate-300">Org hierarchy management</h2>
        <span className={`text-xs ${canManage ? 'text-emerald-300' : 'text-slate-500'}`}>
          {canManage ? 'Write enabled' : 'Read only'}
        </span>
      </div>
      <p className="mt-2 text-xs text-slate-500">
        Structure units with typed metadata, explicit lifecycle status, and placement-specific fields.
      </p>

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
            <li key={node.orgUnitId} className="flex items-start justify-between gap-4 py-3 text-sm">
              <button
                type="button"
                onClick={() => handlePickForEdit(node)}
                className="text-left text-white hover:text-sky-300"
                style={{ paddingLeft: `${depth * 16}px` }}
              >
                <div className="font-medium">{node.name}</div>
                {node.description ? (
                  <div className="mt-1 text-xs text-slate-500">{node.description}</div>
                ) : null}
              </button>
              <span className="text-right text-xs uppercase tracking-wide text-slate-500">
                {humanize(node.unitType)} · {node.status}
              </span>
            </li>
          ))}
        </ul>
      ) : null}

      {canManage && !isLoading && !isError ? (
        <div className="mt-6 grid gap-6 xl:grid-cols-2">
          <form className="space-y-4" onSubmit={handleCreate}>
            <h3 className="text-sm font-medium text-slate-300">Create org unit</h3>
            <OrgUnitFormFields
              prefix="create-org-unit"
              draft={createDraft}
              setDraft={setCreateDraft}
              parentOptions={createParentOptions}
              managerOptions={managerOptions}
              defaultSiteOptions={defaultSiteOptions}
              disabled={isSubmitting}
            />
            <button
              type="submit"
              className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Saving…' : 'Create'}
            </button>
          </form>

          <form className="space-y-4" onSubmit={handleUpdate}>
            <h3 className="text-sm font-medium text-slate-300">Edit selected org unit</h3>
            {!selected ? (
              <p className="text-sm text-slate-500">Select a unit from the hierarchy to edit.</p>
            ) : null}
            <OrgUnitFormFields
              prefix="edit-org-unit"
              draft={editDraft}
              setDraft={setEditDraft}
              parentOptions={editParentOptions}
              managerOptions={managerOptions}
              defaultSiteOptions={defaultSiteOptions}
              disabled={!selected || isSubmitting}
            />
            <div className="flex flex-wrap gap-3">
              <button
                type="submit"
                className="rounded bg-slate-700 px-3 py-2 text-sm text-white disabled:opacity-50"
                disabled={!selected || isSubmitting}
              >
                Save changes
              </button>
              {selected
                ? STATUS_OPTIONS.filter((option) => option.value !== selected.status).map((option) => (
                  <button
                    key={option.value}
                    type="button"
                    className="rounded border border-amber-700 px-3 py-2 text-sm text-amber-200 disabled:opacity-50"
                    onClick={() => onStatusChange(selected.orgUnitId, option.value)}
                    disabled={isSubmitting}
                  >
                    {statusActionLabel(option.value)}
                  </button>
                ))
                : null}
            </div>
          </form>
        </div>
      ) : !isLoading && !isError ? (
        <p className="mt-4 text-xs text-slate-500">Your role does not include org hierarchy write permission.</p>
      ) : null}
    </section>
  )
}
