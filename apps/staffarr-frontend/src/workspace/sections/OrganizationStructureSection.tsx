import { useEffect, useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  Archive,
  Building2,
  ChevronRight,
  CircleCheck,
  Download,
  Edit3,
  Factory,
  Info,
  Layers3,
  MapPin,
  Plus,
  Search,
  Trash2,
  UserRound,
  Users,
  X,
} from 'lucide-react'
import { getLocation, listLocationTree } from '../../api/client'
import type {
  CreateOrgUnitRequest,
  OrgUnitResponse,
  OrgUnitSiteType,
  OrgUnitStatus,
  OrgUnitTeamType,
  OrgUnitType,
} from '../../api/types'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'
import { LocationsAdminSection } from './LocationsAdminSection'

type Props = { state: StaffArrWorkspaceState }

type OrganizationStructureTab = 'organization' | 'locations' | 'people'
type ViewMode = 'browse' | 'create-unit' | 'edit-unit' | 'location-admin'

export type OrgUnitDraft = {
  unitType: OrgUnitType
  name: string
  code: string
  parentOrgUnitId: string
  status: OrgUnitStatus
  description: string
  managerPersonId: string
  defaultSiteOrgUnitId: string
  siteType: OrgUnitSiteType | ''
  teamType: OrgUnitTeamType | ''
  positionCode: string
  effectiveStartDate: string
  effectiveEndDate: string
  timezone: string
  phone: string
  emergencyContact: string
  complianceSensitive: boolean
  safetySensitive: boolean
  canSupervise: boolean
  canApprove: boolean
  allowPeopleAssignment: boolean
  visibleInDirectory: boolean
  useInReporting: boolean
}

type StoredOrgUnitDraft = Omit<OrgUnitDraft, 'emergencyContact'>

type OrgNode = OrgUnitResponse & { children: OrgNode[] }

const DRAFT_STORAGE_KEY = 'staffarr.organization-structure.unit-draft.v1'

const unitTypeOptions: Array<{ value: OrgUnitType; label: string }> = [
  { value: 'company', label: 'Company' },
  { value: 'division', label: 'Division' },
  { value: 'region', label: 'Region' },
  { value: 'business_unit', label: 'Business Unit' },
  { value: 'cost_center', label: 'Cost Center' },
  { value: 'site', label: 'Site' },
  { value: 'department', label: 'Department' },
  { value: 'team', label: 'Team' },
  { value: 'position', label: 'Position' },
  { value: 'other', label: 'Other' },
]

const unitStatusOptions: Array<{ value: OrgUnitStatus; label: string }> = [
  { value: 'planned', label: 'Planned' },
  { value: 'active', label: 'Active' },
  { value: 'inactive', label: 'Inactive' },
  { value: 'archived', label: 'Archived' },
]

const siteTypeOptions: Array<{ value: OrgUnitSiteType; label: string }> = [
  { value: 'office', label: 'Office' },
  { value: 'warehouse', label: 'Warehouse' },
  { value: 'plant', label: 'Plant' },
  { value: 'shop', label: 'Shop' },
  { value: 'yard', label: 'Yard' },
  { value: 'terminal', label: 'Terminal' },
  { value: 'customer_embedded', label: 'Customer Embedded' },
  { value: 'mixed', label: 'Mixed Use' },
  { value: 'other', label: 'Other' },
]

const teamTypeOptions: Array<{ value: OrgUnitTeamType; label: string }> = [
  { value: 'operational', label: 'Operational' },
  { value: 'maintenance', label: 'Maintenance' },
  { value: 'warehouse', label: 'Warehouse' },
  { value: 'dispatch', label: 'Dispatch' },
  { value: 'safety', label: 'Safety' },
  { value: 'quality', label: 'Quality' },
  { value: 'training', label: 'Training' },
  { value: 'admin', label: 'Admin' },
  { value: 'project', label: 'Project' },
  { value: 'emergency_response', label: 'Emergency Response' },
]

const rootEligibleTypes = new Set<OrgUnitType>([
  'company',
  'division',
  'region',
  'business_unit',
  'cost_center',
  'site',
  'other',
])

const primaryButtonClass =
  'inline-flex items-center gap-2 rounded-2xl bg-blue-600 px-5 py-3 text-sm font-semibold text-white transition hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-50'

const secondaryButtonClass =
  'inline-flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/40 px-5 py-3 text-sm font-medium text-slate-100 transition hover:border-slate-500'

const surfaceClass = 'rounded-[28px] border border-slate-800 bg-slate-900/80 shadow-2xl'

function normalizeTab(value: string | null): OrganizationStructureTab {
  if (value === 'locations' || value === 'people') {
    return value
  }

  return 'organization'
}

function normalizeMode(value: string | null): ViewMode {
  if (value === 'create-unit' || value === 'edit-unit' || value === 'location-admin') {
    return value
  }

  return 'browse'
}

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

function humanize(value: string | null | undefined): string {
  if (!value) {
    return 'None'
  }

  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (character) => character.toUpperCase())
}

function emptyDraft(): OrgUnitDraft {
  return {
    unitType: 'department',
    name: '',
    code: '',
    parentOrgUnitId: '',
    status: 'active',
    description: '',
    managerPersonId: '',
    defaultSiteOrgUnitId: '',
    siteType: '',
    teamType: '',
    positionCode: '',
    effectiveStartDate: '',
    effectiveEndDate: '',
    timezone: '',
    phone: '',
    emergencyContact: '',
    complianceSensitive: false,
    safetySensitive: false,
    canSupervise: false,
    canApprove: false,
    allowPeopleAssignment: true,
    visibleInDirectory: true,
    useInReporting: true,
  }
}

export function toStoredOrgUnitDraft(draft: OrgUnitDraft): StoredOrgUnitDraft {
  return {
    unitType: draft.unitType,
    name: draft.name,
    code: draft.code,
    parentOrgUnitId: draft.parentOrgUnitId,
    status: draft.status,
    description: draft.description,
    managerPersonId: draft.managerPersonId,
    defaultSiteOrgUnitId: draft.defaultSiteOrgUnitId,
    siteType: draft.siteType,
    teamType: draft.teamType,
    positionCode: draft.positionCode,
    effectiveStartDate: draft.effectiveStartDate,
    effectiveEndDate: draft.effectiveEndDate,
    timezone: draft.timezone,
    phone: draft.phone,
    complianceSensitive: draft.complianceSensitive,
    safetySensitive: draft.safetySensitive,
    canSupervise: draft.canSupervise,
    canApprove: draft.canApprove,
    allowPeopleAssignment: draft.allowPeopleAssignment,
    visibleInDirectory: draft.visibleInDirectory,
    useInReporting: draft.useInReporting,
  }
}

export function restoreStoredOrgUnitDraft(
  storedDraft: Partial<StoredOrgUnitDraft>,
  fallbackParentOrgUnitId: string,
  fallbackDefaultSiteOrgUnitId: string,
): OrgUnitDraft {
  return {
    ...emptyDraft(),
    ...storedDraft,
    emergencyContact: '',
    parentOrgUnitId: storedDraft.parentOrgUnitId || fallbackParentOrgUnitId,
    defaultSiteOrgUnitId: storedDraft.defaultSiteOrgUnitId || fallbackDefaultSiteOrgUnitId,
  }
}

function hydrateDraft(unit: OrgUnitResponse): OrgUnitDraft {
  return {
    unitType: unit.unitType,
    name: unit.name,
    code: unit.code ?? '',
    parentOrgUnitId: unit.parentOrgUnitId ?? '',
    status: unit.status,
    description: unit.description ?? '',
    managerPersonId: unit.managerPersonId ?? '',
    defaultSiteOrgUnitId: unit.defaultSiteOrgUnitId ?? '',
    siteType: unit.siteType ?? '',
    teamType: unit.teamType ?? '',
    positionCode: unit.positionCode ?? '',
    effectiveStartDate: unit.effectiveStartDate?.slice(0, 10) ?? '',
    effectiveEndDate: unit.effectiveEndDate?.slice(0, 10) ?? '',
    timezone: unit.timezone ?? '',
    phone: unit.phone ?? '',
    emergencyContact: unit.emergencyContact ?? '',
    complianceSensitive: unit.complianceSensitive ?? false,
    safetySensitive: unit.safetySensitive ?? false,
    canSupervise: unit.canSupervise ?? false,
    canApprove: unit.canApprove ?? false,
    allowPeopleAssignment: true,
    visibleInDirectory: true,
    useInReporting: true,
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
    parentOrgUnitId: draft.parentOrgUnitId || null,
    code: draft.code.trim() || null,
    description: draft.description.trim() || null,
    managerPersonId: draft.managerPersonId || null,
    defaultSiteOrgUnitId: draft.defaultSiteOrgUnitId || null,
    siteType: draft.siteType || null,
    teamType: draft.teamType || null,
    positionCode: draft.positionCode.trim() || null,
    effectiveStartDate: toIsoDate(draft.effectiveStartDate),
    effectiveEndDate: toIsoDate(draft.effectiveEndDate),
    timezone: draft.timezone.trim() || null,
    phone: draft.phone.trim() || null,
    emergencyContact: draft.emergencyContact.trim() || null,
    complianceSensitive: draft.complianceSensitive,
    safetySensitive: draft.safetySensitive,
    canSupervise: draft.canSupervise,
    canApprove: draft.canApprove,
    status: draft.status,
  }
}

function isAllowedParentType(unitType: OrgUnitType, parentType: OrgUnitType | null): boolean {
  if (parentType == null) {
    return rootEligibleTypes.has(unitType)
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

function isDescendantOf(candidateId: string, ancestorId: string, byId: Map<string, OrgUnitResponse>): boolean {
  let cursor = byId.get(candidateId)?.parentOrgUnitId ?? null
  while (cursor) {
    if (cursor === ancestorId) {
      return true
    }

    cursor = byId.get(cursor)?.parentOrgUnitId ?? null
  }

  return false
}

function formatOrgPath(unit: OrgUnitResponse | null, byId: Map<string, OrgUnitResponse>): string {
  if (!unit) {
    return ''
  }

  const path: string[] = [unit.name]
  let cursor = unit.parentOrgUnitId ? byId.get(unit.parentOrgUnitId) ?? null : null
  while (cursor) {
    path.unshift(cursor.name)
    cursor = cursor.parentOrgUnitId ? byId.get(cursor.parentOrgUnitId) ?? null : null
  }

  return path.join(' / ')
}

function formatDraftPath(draft: OrgUnitDraft, byId: Map<string, OrgUnitResponse>): string {
  const name = draft.name.trim() || 'New Unit'
  if (!draft.parentOrgUnitId) {
    return name
  }

  const parent = byId.get(draft.parentOrgUnitId) ?? null
  if (!parent) {
    return name
  }

  return `${formatOrgPath(parent, byId)} / ${name}`
}

function summaryTone(status: string): string {
  if (status === 'active' || status === 'planned' || status === 'enabled') {
    return 'success'
  }

  if (status === 'restricted' || status === 'inactive' || status === 'expiring_soon') {
    return 'warning'
  }

  return 'neutral'
}

function matchText(...values: Array<string | null | undefined>): string {
  return values.filter(Boolean).join(' ').toLowerCase()
}

function TabButton({
  active,
  icon: Icon,
  label,
  onClick,
}: {
  active: boolean
  icon: typeof Layers3
  label: string
  onClick: () => void
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`inline-flex items-center gap-2 rounded-2xl px-5 py-3 text-sm font-semibold transition ${
        active
          ? 'bg-[var(--color-accent)] text-white shadow-lg'
          : 'text-slate-400 hover:bg-slate-900/70 hover:text-slate-100'
      }`}
    >
      <Icon className="h-4 w-4" />
      {label}
    </button>
  )
}

function SummaryPill({ label }: { label: string }) {
  return (
    <span
      className="stl-tone-badge inline-flex rounded-full border px-3 py-1 text-xs font-semibold"
      data-tone={summaryTone(label.toLowerCase())}
    >
      {label}
    </span>
  )
}

function DetailTable({ rows }: { rows: Array<{ label: string; value: string | number }> }) {
  return (
    <div className="overflow-hidden rounded-[22px] border border-slate-800 bg-slate-950/45">
      {rows.map((row, index) => (
        <div
          key={row.label}
          className={`grid grid-cols-[200px_minmax(0,1fr)] ${index !== rows.length - 1 ? 'border-b border-slate-800' : ''}`}
        >
          <div className="bg-slate-900/80 px-5 py-4 text-slate-400">{row.label}</div>
          <div className="px-5 py-4 text-slate-50">{row.value}</div>
        </div>
      ))}
    </div>
  )
}

function ToggleCard({
  title,
  description,
  enabled,
  onToggle,
}: {
  title: string
  description: string
  enabled: boolean
  onToggle: () => void
}) {
  return (
    <button
      type="button"
      onClick={onToggle}
      className="flex min-h-36 flex-col justify-between rounded-[22px] border border-slate-800 bg-slate-950/45 p-5 text-left transition hover:border-slate-700"
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-lg font-semibold text-white">{title}</p>
          <p className="mt-3 text-sm leading-7 text-slate-400">{description}</p>
        </div>
        <span className={`flex h-7 w-12 items-center rounded-full p-1 transition ${enabled ? 'bg-cyan-500/90 justify-end' : 'bg-slate-700 justify-start'}`}>
          <span className="h-5 w-5 rounded-full bg-[var(--color-bg-surface)]" />
        </span>
      </div>
    </button>
  )
}

export function OrganizationStructureSection({ state }: Props) {
  const [searchParams, setSearchParams] = useSearchParams()
  const navigate = useNavigate()
  const tab = normalizeTab(searchParams.get('tab'))
  const mode = normalizeMode(searchParams.get('mode'))
  const selectedOrgUnitId = searchParams.get('orgUnitId')
  const selectedLocationId = searchParams.get('locationId')
  const selectedPersonId = searchParams.get('personId')
  const [orgSearch, setOrgSearch] = useState('')
  const [locationSearch, setLocationSearch] = useState('')
  const [peopleSearch, setPeopleSearch] = useState('')
  const [unitDraft, setUnitDraft] = useState<OrgUnitDraft>(emptyDraft)
  const [draftNotice, setDraftNotice] = useState<string | null>(null)

  const setParam = (updates: Record<string, string | null>, replace = false) => {
    const nextParams = new URLSearchParams(searchParams)
    for (const [key, value] of Object.entries(updates)) {
      if (value == null || value === '') {
        nextParams.delete(key)
      } else {
        nextParams.set(key, value)
      }
    }

    setSearchParams(nextParams, { replace })
  }

  const activeOrgUnits = useMemo(
    () => state.orgUnits.filter((unit) => unit.status !== 'archived'),
    [state.orgUnits],
  )
  const orgUnitsById = useMemo(
    () => new Map(activeOrgUnits.map((unit) => [unit.orgUnitId, unit])),
    [activeOrgUnits],
  )
  const orgTreeRows = useMemo(
    () => flattenTree(buildTree(activeOrgUnits)),
    [activeOrgUnits],
  )
  const filteredOrgRows = useMemo(() => {
    const query = orgSearch.trim().toLowerCase()
    if (!query) {
      return orgTreeRows
    }

    return orgTreeRows.filter(({ node }) =>
      matchText(node.name, node.code, node.description, node.unitType).includes(query),
    )
  }, [orgSearch, orgTreeRows])
  const selectedOrgUnit =
    (selectedOrgUnitId ? orgUnitsById.get(selectedOrgUnitId) : null)
    ?? filteredOrgRows[0]?.node
    ?? orgTreeRows[0]?.node
    ?? null

  const siteUnits = useMemo(
    () => activeOrgUnits.filter((unit) => unit.unitType === 'site'),
    [activeOrgUnits],
  )
  const [selectedSiteId, setSelectedSiteId] = useState<string>(siteUnits[0]?.orgUnitId ?? '')

  useEffect(() => {
    if (tab !== 'organization') {
      return
    }

    if (selectedOrgUnit && selectedOrgUnit.orgUnitId !== selectedOrgUnitId) {
      setParam({ orgUnitId: selectedOrgUnit.orgUnitId }, true)
    }
  }, [selectedOrgUnit, selectedOrgUnitId, tab])

  useEffect(() => {
    if (siteUnits.length === 0) {
      return
    }

    if (!siteUnits.some((unit) => unit.orgUnitId === selectedSiteId)) {
      setSelectedSiteId(siteUnits[0]!.orgUnitId)
    }
  }, [selectedSiteId, siteUnits])

  const locationsQuery = useQuery({
    queryKey: ['staffarr-org-structure-locations', state.accessToken, selectedSiteId, locationSearch],
    queryFn: () =>
      listLocationTree(state.accessToken, {
        siteOrgUnitId: selectedSiteId || undefined,
        search: locationSearch.trim() || undefined,
      }),
    enabled: Boolean(state.accessToken && selectedSiteId),
  })

  const filteredLocations = useMemo(
    () => locationsQuery.data ?? [],
    [locationsQuery.data],
  )
  const locationById = useMemo(
    () => new Map(filteredLocations.map((location) => [location.locationId, location])),
    [filteredLocations],
  )

  useEffect(() => {
    if (tab !== 'locations' || filteredLocations.length === 0) {
      return
    }

    if (!selectedLocationId || !locationById.has(selectedLocationId)) {
      setParam({ locationId: filteredLocations[0]!.locationId }, true)
    }
  }, [filteredLocations, locationById, selectedLocationId, tab])

  const selectedLocationQuery = useQuery({
    queryKey: ['staffarr-org-structure-location', state.accessToken, selectedLocationId],
    queryFn: () => getLocation(state.accessToken, selectedLocationId!),
    enabled: Boolean(state.accessToken && selectedLocationId),
  })

  const selectedLocation =
    selectedLocationQuery.data
    ?? (selectedLocationId ? locationById.get(selectedLocationId) : null)
    ?? filteredLocations[0]
    ?? null

  const filteredPeople = useMemo(() => {
    const query = peopleSearch.trim().toLowerCase()
    if (!query) {
      return state.people
    }

    return state.people.filter((person) =>
      matchText(
        person.displayName,
        person.primaryEmail,
        person.jobTitle,
        person.primaryOrgUnitName,
        person.employmentStatus,
      ).includes(query),
    )
  }, [peopleSearch, state.people])

  const selectedPerson =
    (selectedPersonId
      ? state.people.find((person) => person.personId === selectedPersonId) ?? null
      : state.selectedPerson)
    ?? filteredPeople[0]
    ?? state.selectedPerson
    ?? null

  useEffect(() => {
    if (tab !== 'people' || !selectedPerson) {
      return
    }

    if (selectedPerson.personId !== state.selectedPersonId) {
      state.setSelectedPersonId(selectedPerson.personId, { syncDetailQuery: false, replace: true })
    }

    if (selectedPerson.personId !== selectedPersonId) {
      setParam({ personId: selectedPerson.personId }, true)
    }
  }, [selectedPerson, selectedPersonId, state, tab])

  useEffect(() => {
    if (mode === 'create-unit') {
      const savedDraft = window.sessionStorage.getItem(DRAFT_STORAGE_KEY)
      if (!savedDraft) {
        setUnitDraft((current) =>
          current.name || current.parentOrgUnitId || current.description
            ? current
            : {
                ...emptyDraft(),
                parentOrgUnitId: selectedOrgUnit?.orgUnitId ?? '',
                defaultSiteOrgUnitId: siteUnits[0]?.orgUnitId ?? '',
              },
        )
        return
      }

      try {
        const parsed = JSON.parse(savedDraft) as Partial<StoredOrgUnitDraft>
        setUnitDraft(restoreStoredOrgUnitDraft(
          parsed,
          selectedOrgUnit?.orgUnitId ?? '',
          siteUnits[0]?.orgUnitId ?? '',
        ))
        setDraftNotice('Restored a locally saved draft.')
      } catch {
        setUnitDraft({
          ...emptyDraft(),
          parentOrgUnitId: selectedOrgUnit?.orgUnitId ?? '',
          defaultSiteOrgUnitId: siteUnits[0]?.orgUnitId ?? '',
        })
      }

      return
    }

    if (mode === 'edit-unit' && selectedOrgUnit) {
      setUnitDraft(hydrateDraft(selectedOrgUnit))
      setDraftNotice(null)
    }
  }, [mode, selectedOrgUnit, siteUnits])

  const availableParentUnits = useMemo(() => {
    return activeOrgUnits.filter((unit) => {
      if (!isAllowedParentType(unitDraft.unitType, unit.unitType)) {
        return false
      }

      if (mode === 'edit-unit' && selectedOrgUnit) {
        if (unit.orgUnitId === selectedOrgUnit.orgUnitId) {
          return false
        }

        if (isDescendantOf(unit.orgUnitId, selectedOrgUnit.orgUnitId, orgUnitsById)) {
          return false
        }
      }

      return true
    })
  }, [activeOrgUnits, mode, orgUnitsById, selectedOrgUnit, unitDraft.unitType])

  const managerName = unitDraft.managerPersonId
    ? state.people.find((person) => person.personId === unitDraft.managerPersonId)?.displayName ?? 'Assigned'
    : 'None'
  const defaultSiteName = unitDraft.defaultSiteOrgUnitId
    ? orgUnitsById.get(unitDraft.defaultSiteOrgUnitId)?.name ?? 'Unknown site'
    : selectedLocation?.siteNameSnapshot ?? siteUnits[0]?.name ?? 'None'

  const openCreateUnit = () => {
    setDraftNotice(null)
    setUnitDraft({
      ...emptyDraft(),
      parentOrgUnitId: selectedOrgUnit?.orgUnitId ?? '',
      defaultSiteOrgUnitId: siteUnits[0]?.orgUnitId ?? '',
    })
    setParam({ mode: 'create-unit' })
  }

  const openEditUnit = () => {
    if (!selectedOrgUnit) {
      return
    }

    setDraftNotice(null)
    setUnitDraft(hydrateDraft(selectedOrgUnit))
    setParam({ mode: 'edit-unit', orgUnitId: selectedOrgUnit.orgUnitId })
  }

  const cancelComposer = () => {
    setDraftNotice(null)
    setParam({ mode: null })
  }

  const saveDraftLocally = () => {
    window.sessionStorage.setItem(DRAFT_STORAGE_KEY, JSON.stringify(toStoredOrgUnitDraft(unitDraft)))
    setDraftNotice('Draft saved locally in this browser session.')
  }

  const submitUnit = async () => {
    const payload = serializeDraft(unitDraft)
    if (!payload.name.trim()) {
      return
    }

    if (mode === 'edit-unit' && selectedOrgUnit) {
      await state.updateOrgUnitMutation.mutateAsync({
        orgUnitId: selectedOrgUnit.orgUnitId,
        request: payload,
      })
      window.sessionStorage.removeItem(DRAFT_STORAGE_KEY)
      setDraftNotice(null)
      setParam({ mode: null, orgUnitId: selectedOrgUnit.orgUnitId }, true)
      return
    }

    const created = await state.createOrgUnitMutation.mutateAsync(payload)
    window.sessionStorage.removeItem(DRAFT_STORAGE_KEY)
    setDraftNotice(null)
    setParam({ mode: null, orgUnitId: created.orgUnitId }, true)
  }

  const archiveSelectedUnit = async () => {
    if (!selectedOrgUnit) {
      return
    }

    const confirmed = window.confirm(`Archive ${selectedOrgUnit.name}?`)
    if (!confirmed) {
      return
    }

    await state.updateOrgUnitStatusMutation.mutateAsync({
      orgUnitId: selectedOrgUnit.orgUnitId,
      status: 'archived',
    })
  }

  const peoplePlacement = state.personLookupQuery.data?.placement
  const primaryAssignment = peoplePlacement?.activeAssignments[0] ?? null
  const currentPersonRows = selectedPerson
    ? [
        { label: 'Email', value: selectedPerson.primaryEmail },
        { label: 'Title', value: state.profile?.jobTitle ?? selectedPerson.jobTitle ?? 'None' },
        { label: 'Department', value: primaryAssignment?.departmentName ?? selectedPerson.primaryOrgUnitName ?? 'None' },
        { label: 'Manager', value: peoplePlacement?.managerDisplayName ?? 'None' },
        { label: 'Site', value: primaryAssignment?.siteName ?? 'None' },
        { label: 'Status', value: humanize(selectedPerson.employmentStatus) },
        { label: 'Can Login', value: selectedPerson.canLoginSnapshot ? 'Yes' : 'No' },
        { label: 'Permissions', value: state.effectivePermissions?.permissions.length ?? 0 },
        { label: 'Qualifications', value: state.personCertifications.length },
      ]
    : []

  const selectedSite = siteUnits.find((site) => site.orgUnitId === selectedSiteId) ?? null
  const actionErrorMessage =
    state.orgMutationError
      ? getErrorMessage(state.orgMutationError, 'Unable to save the organization unit.')
      : null

  if (mode === 'location-admin') {
    return (
      <section className="space-y-6">
        <div className={`${surfaceClass} p-6`}>
          <button
            type="button"
            onClick={() => setParam({ mode: null })}
            className="inline-flex items-center gap-2 text-sm text-slate-400 transition hover:text-slate-100"
          >
            <ChevronRight className="h-4 w-4 rotate-180" />
            Back to Organization Structure
          </button>
          <div className="mt-5 flex flex-wrap items-start justify-between gap-4">
            <div>
              <p className="text-sm font-semibold text-cyan-400">StaffArr / Locations</p>
              <h2 className="mt-2 text-4xl font-semibold text-white">Location Manager</h2>
              <p className="mt-3 max-w-3xl text-base text-slate-400">
                Use the full StaffArr location manager when you need creation, archiving, map lookup, or detailed location editing.
              </p>
            </div>
            <button type="button" onClick={() => setParam({ mode: null })} className={secondaryButtonClass}>
              <X className="h-4 w-4" />
              Close
            </button>
          </div>
        </div>

        <LocationsAdminSection state={state} />
      </section>
    )
  }

  if (mode === 'create-unit' || mode === 'edit-unit') {
    const previewParent = unitDraft.parentOrgUnitId ? orgUnitsById.get(unitDraft.parentOrgUnitId) ?? null : null
    const previewStatus = humanize(unitDraft.status)
    const previewType = humanize(unitDraft.unitType)

    return (
      <section className="space-y-6">
        <div className={`${surfaceClass} p-6`}>
          <button
            type="button"
            onClick={cancelComposer}
            className="inline-flex items-center gap-2 text-sm text-slate-400 transition hover:text-slate-100"
          >
            <ChevronRight className="h-4 w-4 rotate-180" />
            Back to Organization Structure
          </button>
          <div className="mt-5 flex flex-wrap items-start justify-between gap-4">
            <div className="flex items-start gap-4">
              <div className="rounded-[20px] bg-slate-800/80 p-4 text-cyan-400">
                <Factory className="h-8 w-8" />
              </div>
              <div>
                <p className="text-sm font-semibold text-cyan-400">StaffArr / Organization</p>
                <h2 className="mt-2 text-4xl font-semibold text-white">
                  {mode === 'edit-unit' ? 'Edit Unit' : 'Create Unit'}
                </h2>
                <p className="mt-3 max-w-3xl text-base text-slate-400">
                  Add a company unit, department, team, position, or work group to the StaffArr organization hierarchy.
                </p>
              </div>
            </div>
            <div className="flex flex-wrap gap-3">
              <button type="button" onClick={cancelComposer} className={secondaryButtonClass}>
                <X className="h-4 w-4" />
                Cancel
              </button>
              <button type="button" onClick={saveDraftLocally} className={secondaryButtonClass}>
                Save Draft
              </button>
              <button
                type="button"
                onClick={() => void submitUnit()}
                disabled={
                  state.createOrgUnitMutation.isPending
                  || state.updateOrgUnitMutation.isPending
                  || !unitDraft.name.trim()
                }
                className={primaryButtonClass}
              >
                <Building2 className="h-4 w-4" />
                {mode === 'edit-unit'
                  ? state.updateOrgUnitMutation.isPending
                    ? 'Saving...'
                    : 'Save Unit'
                  : state.createOrgUnitMutation.isPending
                    ? 'Creating...'
                    : 'Create Unit'}
              </button>
            </div>
          </div>
        </div>

        {draftNotice ? (
          <div className="rounded-2xl border border-cyan-500/30 bg-cyan-950/30 px-4 py-3 text-sm text-cyan-100">
            {draftNotice}
          </div>
        ) : null}

        {actionErrorMessage ? (
          <ApiErrorCallout title="Organization unit update failed" message={actionErrorMessage} />
        ) : null}

        <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_400px]">
          <div className="space-y-6">
            <section className={`${surfaceClass} p-6`}>
              <div className="flex items-start gap-4">
                <div className="rounded-[18px] bg-slate-800/80 p-4 text-cyan-400">
                  <Building2 className="h-7 w-7" />
                </div>
                <div>
                  <h3 className="text-3xl font-semibold text-white">Unit Details</h3>
                  <p className="mt-2 text-base text-slate-400">
                    Basic identity and classification for the new organization unit.
                  </p>
                </div>
              </div>

              <div className="mt-8 grid gap-5 md:grid-cols-2">
                <label className="block text-sm font-medium text-slate-200">
                  Unit Name *
                  <input
                    value={unitDraft.name}
                    onChange={(event) => setUnitDraft((current) => ({ ...current, name: event.target.value }))}
                    placeholder="Example: Fleet Maintenance"
                    className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                  />
                </label>

                <label className="block text-sm font-medium text-slate-200">
                  Unit Type *
                  <select
                    value={unitDraft.unitType}
                    onChange={(event) =>
                      setUnitDraft((current) => ({
                        ...current,
                        unitType: event.target.value as OrgUnitType,
                        parentOrgUnitId: '',
                        siteType: '',
                        teamType: '',
                        positionCode: '',
                      }))
                    }
                    className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                  >
                    {unitTypeOptions.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="block text-sm font-medium text-slate-200">
                  Status *
                  <select
                    value={unitDraft.status}
                    onChange={(event) => setUnitDraft((current) => ({ ...current, status: event.target.value as OrgUnitStatus }))}
                    className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                  >
                    {unitStatusOptions.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="block text-sm font-medium text-slate-200">
                  External Code
                  <input
                    value={unitDraft.code}
                    onChange={(event) => setUnitDraft((current) => ({ ...current, code: event.target.value }))}
                    placeholder="Optional HRIS / ERP code"
                    className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                  />
                </label>

                <label className="block text-sm font-medium text-slate-200 md:col-span-2">
                  Description
                  <textarea
                    value={unitDraft.description}
                    onChange={(event) => setUnitDraft((current) => ({ ...current, description: event.target.value }))}
                    placeholder="Optional notes about what this unit owns or represents."
                    className="mt-3 min-h-32 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                  />
                </label>
              </div>
            </section>

            <section className={`${surfaceClass} p-6`}>
              <div className="flex items-start gap-4">
                <div className="rounded-[18px] bg-slate-800/80 p-4 text-cyan-400">
                  <Layers3 className="h-7 w-7" />
                </div>
                <div>
                  <h3 className="text-3xl font-semibold text-white">Hierarchy</h3>
                  <p className="mt-2 text-base text-slate-400">
                    Choose where this unit lives in the StaffArr organization tree.
                  </p>
                </div>
              </div>

              <div className="mt-8 grid gap-5 md:grid-cols-2">
                <label className="block text-sm font-medium text-slate-200">
                  Parent Unit
                  <select
                    value={unitDraft.parentOrgUnitId}
                    onChange={(event) => setUnitDraft((current) => ({ ...current, parentOrgUnitId: event.target.value }))}
                    className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                  >
                    <option value="">No parent (root)</option>
                    {availableParentUnits.map((unit) => (
                      <option key={unit.orgUnitId} value={unit.orgUnitId}>
                        {formatOrgPath(unit, orgUnitsById)}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="block text-sm font-medium text-slate-200">
                  Default Site
                  <select
                    value={unitDraft.defaultSiteOrgUnitId}
                    onChange={(event) => setUnitDraft((current) => ({ ...current, defaultSiteOrgUnitId: event.target.value }))}
                    className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                  >
                    <option value="">None</option>
                    {siteUnits.map((site) => (
                      <option key={site.orgUnitId} value={site.orgUnitId}>
                        {site.name}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="block text-sm font-medium text-slate-200">
                  Cost Center
                  <input
                    value={unitDraft.code}
                    onChange={(event) => setUnitDraft((current) => ({ ...current, code: event.target.value }))}
                    placeholder="Optional"
                    className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                  />
                </label>

                <label className="block text-sm font-medium text-slate-200">
                  Manager
                  <select
                    value={unitDraft.managerPersonId}
                    onChange={(event) => setUnitDraft((current) => ({ ...current, managerPersonId: event.target.value }))}
                    className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                  >
                    <option value="">No manager assigned</option>
                    {state.people.map((person) => (
                      <option key={person.personId} value={person.personId}>
                        {person.displayName}
                      </option>
                    ))}
                  </select>
                </label>
              </div>
            </section>

            <section className={`${surfaceClass} p-6`}>
              <div className="flex items-start gap-4">
                <div className="rounded-[18px] bg-slate-800/80 p-4 text-cyan-400">
                  <Users className="h-7 w-7" />
                </div>
                <div>
                  <h3 className="text-3xl font-semibold text-white">People Defaults</h3>
                  <p className="mt-2 text-base text-slate-400">
                    Optional defaults used when people are assigned to this unit later.
                  </p>
                </div>
              </div>

              <div className="mt-8 grid gap-5 lg:grid-cols-3">
                <ToggleCard
                  title="Allow people assignment"
                  description="People can be directly assigned to this unit."
                  enabled={unitDraft.allowPeopleAssignment}
                  onToggle={() =>
                    setUnitDraft((current) => ({
                      ...current,
                      allowPeopleAssignment: !current.allowPeopleAssignment,
                    }))
                  }
                />
                <ToggleCard
                  title="Visible in directory"
                  description="Show this unit in people filters and profile assignments."
                  enabled={unitDraft.visibleInDirectory}
                  onToggle={() =>
                    setUnitDraft((current) => ({
                      ...current,
                      visibleInDirectory: !current.visibleInDirectory,
                    }))
                  }
                />
                <ToggleCard
                  title="Use in reporting"
                  description="Include this unit in StaffArr reporting rollups."
                  enabled={unitDraft.useInReporting}
                  onToggle={() =>
                    setUnitDraft((current) => ({
                      ...current,
                      useInReporting: !current.useInReporting,
                    }))
                  }
                />
              </div>

              <p className="mt-5 text-sm text-[var(--color-text-muted)]">
                These three switches currently shape the draft preview only. StaffArr does not yet expose dedicated persisted fields for directory visibility or reporting rollups on org units.
              </p>
            </section>

            {(unitDraft.unitType === 'site' || unitDraft.unitType === 'team' || unitDraft.unitType === 'position') ? (
              <section className={`${surfaceClass} p-6`}>
                <h3 className="text-2xl font-semibold text-white">Advanced Unit Settings</h3>
                <p className="mt-2 text-base text-slate-400">
                  Keep the richer StaffArr org-unit fields available when the unit type needs them.
                </p>
                <div className="mt-8 grid gap-5 md:grid-cols-2">
                  {unitDraft.unitType === 'site' ? (
                    <>
                      <label className="block text-sm font-medium text-slate-200">
                        Site Type
                        <select
                          value={unitDraft.siteType}
                          onChange={(event) => setUnitDraft((current) => ({ ...current, siteType: event.target.value as OrgUnitSiteType | '' }))}
                          className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                        >
                          <option value="">Not set</option>
                          {siteTypeOptions.map((option) => (
                            <option key={option.value} value={option.value}>
                              {option.label}
                            </option>
                          ))}
                        </select>
                      </label>
                      <label className="block text-sm font-medium text-slate-200">
                        Timezone
                        <input
                          value={unitDraft.timezone}
                          onChange={(event) => setUnitDraft((current) => ({ ...current, timezone: event.target.value }))}
                          placeholder="America/Chicago"
                          className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                        />
                      </label>
                      <label className="block text-sm font-medium text-slate-200">
                        Phone
                        <input
                          value={unitDraft.phone}
                          onChange={(event) => setUnitDraft((current) => ({ ...current, phone: event.target.value }))}
                          className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                        />
                      </label>
                      <label className="block text-sm font-medium text-slate-200">
                        Emergency Contact
                        <input
                          value={unitDraft.emergencyContact}
                          onChange={(event) => setUnitDraft((current) => ({ ...current, emergencyContact: event.target.value }))}
                          className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                        />
                      </label>
                    </>
                  ) : null}

                  {unitDraft.unitType === 'team' ? (
                    <label className="block text-sm font-medium text-slate-200">
                      Team Type
                      <select
                        value={unitDraft.teamType}
                        onChange={(event) => setUnitDraft((current) => ({ ...current, teamType: event.target.value as OrgUnitTeamType | '' }))}
                        className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                      >
                        <option value="">Not set</option>
                        {teamTypeOptions.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </label>
                  ) : null}

                  {unitDraft.unitType === 'position' ? (
                    <>
                      <label className="block text-sm font-medium text-slate-200">
                        Position Code
                        <input
                          value={unitDraft.positionCode}
                          onChange={(event) => setUnitDraft((current) => ({ ...current, positionCode: event.target.value }))}
                          className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                        />
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/45 px-4 py-4 text-sm text-slate-200">
                        <input
                          type="checkbox"
                          checked={unitDraft.canSupervise}
                          onChange={(event) => setUnitDraft((current) => ({ ...current, canSupervise: event.target.checked }))}
                          className="h-4 w-4 rounded border-slate-700 bg-slate-950"
                        />
                        Can supervise
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/45 px-4 py-4 text-sm text-slate-200">
                        <input
                          type="checkbox"
                          checked={unitDraft.canApprove}
                          onChange={(event) => setUnitDraft((current) => ({ ...current, canApprove: event.target.checked }))}
                          className="h-4 w-4 rounded border-slate-700 bg-slate-950"
                        />
                        Can approve
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/45 px-4 py-4 text-sm text-slate-200">
                        <input
                          type="checkbox"
                          checked={unitDraft.safetySensitive}
                          onChange={(event) => setUnitDraft((current) => ({ ...current, safetySensitive: event.target.checked }))}
                          className="h-4 w-4 rounded border-slate-700 bg-slate-950"
                        />
                        Safety sensitive
                      </label>
                      <label className="flex items-center gap-3 rounded-2xl border border-slate-800 bg-slate-950/45 px-4 py-4 text-sm text-slate-200">
                        <input
                          type="checkbox"
                          checked={unitDraft.complianceSensitive}
                          onChange={(event) => setUnitDraft((current) => ({ ...current, complianceSensitive: event.target.checked }))}
                          className="h-4 w-4 rounded border-slate-700 bg-slate-950"
                        />
                        Compliance sensitive
                      </label>
                    </>
                  ) : null}

                  <label className="block text-sm font-medium text-slate-200">
                    Effective Start
                    <input
                      type="date"
                      value={unitDraft.effectiveStartDate}
                      onChange={(event) => setUnitDraft((current) => ({ ...current, effectiveStartDate: event.target.value }))}
                      className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                    />
                  </label>

                  <label className="block text-sm font-medium text-slate-200">
                    Effective End
                    <input
                      type="date"
                      value={unitDraft.effectiveEndDate}
                      onChange={(event) => setUnitDraft((current) => ({ ...current, effectiveEndDate: event.target.value }))}
                      className="mt-3 w-full rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-base text-white outline-none transition focus:border-cyan-500"
                    />
                  </label>
                </div>
              </section>
            ) : null}
          </div>

          <div className="space-y-6">
            <section className={`${surfaceClass} p-6`}>
              <div className="flex items-center gap-3">
                <CircleCheck className="h-5 w-5 text-cyan-400" />
                <h3 className="text-2xl font-semibold text-white">
                  {mode === 'edit-unit' ? 'Update Preview' : 'Create Preview'}
                </h3>
              </div>

              <div className="mt-6 rounded-[22px] border border-slate-800 bg-slate-950/50 p-5">
                <p className="text-sm font-semibold uppercase tracking-[0.18em] text-slate-400">Organization Unit</p>
                <h4 className="mt-3 text-4xl font-semibold text-white">{unitDraft.name.trim() || 'New Unit'}</h4>
                <div className="mt-5 flex flex-wrap gap-2">
                  <SummaryPill label={previewType} />
                  <SummaryPill label={previewStatus} />
                </div>

                <div className="mt-6 space-y-5 text-base">
                  <div className="border-t border-slate-800 pt-5">
                    <p className="text-sm text-slate-400">Parent</p>
                    <p className="mt-2 text-white">{previewParent?.name ?? 'Top level unit'}</p>
                  </div>
                  <div className="border-t border-slate-800 pt-5">
                    <p className="text-sm text-slate-400">Path</p>
                    <p className="mt-2 text-white">{formatDraftPath(unitDraft, orgUnitsById)}</p>
                  </div>
                  <div className="border-t border-slate-800 pt-5">
                    <p className="text-sm text-slate-400">Default Site</p>
                    <p className="mt-2 text-white">{defaultSiteName}</p>
                  </div>
                  <div className="border-t border-slate-800 pt-5">
                    <p className="text-sm text-slate-400">Manager</p>
                    <p className="mt-2 text-white">{managerName}</p>
                  </div>
                  <div className="border-t border-slate-800 pt-5">
                    <p className="text-sm text-slate-400">Cost Center</p>
                    <p className="mt-2 text-white">{unitDraft.code.trim() || 'None'}</p>
                  </div>
                </div>
              </div>
            </section>

            <section className={`${surfaceClass} p-6`}>
              <div className="flex items-center gap-3">
                <Info className="h-5 w-5 text-amber-400" />
                <h3 className="text-2xl font-semibold text-white">What this creates</h3>
              </div>
              <ul className="mt-6 space-y-4 text-base text-slate-300">
                <li className="flex gap-3">
                  <CircleCheck className="mt-1 h-5 w-5 shrink-0 text-cyan-400" />
                  Creates one StaffArr organization unit.
                </li>
                <li className="flex gap-3">
                  <CircleCheck className="mt-1 h-5 w-5 shrink-0 text-cyan-400" />
                  Places the unit under the selected parent.
                </li>
                <li className="flex gap-3">
                  <CircleCheck className="mt-1 h-5 w-5 shrink-0 text-cyan-400" />
                  Makes the unit available for person assignment through StaffArr references.
                </li>
              </ul>
            </section>

            <section className={`${surfaceClass} p-6`}>
              <div className="flex items-center gap-3">
                <MapPin className="h-5 w-5 text-cyan-400" />
                <h3 className="text-2xl font-semibold text-white">Related setup</h3>
              </div>
              <div className="mt-6 space-y-3">
                <button type="button" onClick={() => setParam({ mode: 'location-admin', tab: 'locations' })} className={`${secondaryButtonClass} w-full justify-between`}>
                  Create Location
                  <ChevronRight className="h-4 w-4" />
                </button>
                <button type="button" onClick={() => setParam({ tab: 'people', mode: null })} className={`${secondaryButtonClass} w-full justify-between`}>
                  Assign People
                  <ChevronRight className="h-4 w-4" />
                </button>
                <button type="button" onClick={() => navigate('/roles')} className={`${secondaryButtonClass} w-full justify-between`}>
                  Review Permission Assignments
                  <ChevronRight className="h-4 w-4" />
                </button>
              </div>
            </section>
          </div>
        </div>
      </section>
    )
  }

  const title = tab === 'organization' ? 'Organization' : tab === 'locations' ? 'Locations' : 'People'
  const subtitle =
    tab === 'organization'
      ? 'Create, edit, archive, and manage company units, departments, teams, and reporting structure.'
      : tab === 'locations'
        ? 'Manage canonical internal sites, buildings, warehouses, docks, rooms, yards, staging areas, and inventory locations.'
        : 'Create, read, update, archive, and manage people, login eligibility, department assignment, manager relationships, site assignment, and StaffArr role assignments.'

  const contextButton =
    tab === 'organization'
      ? { label: 'Create Unit', onClick: openCreateUnit }
      : tab === 'locations'
        ? { label: 'Create Location', onClick: () => setParam({ mode: 'location-admin' }) }
        : { label: 'Create Person', onClick: () => navigate('/people/create') }

  return (
    <section className="space-y-6">
      <div className={`${surfaceClass} p-6`}>
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold text-cyan-400">StaffArr</p>
            <h2 className="mt-2 text-4xl font-semibold text-white">Organization Structure</h2>
            <p className="mt-3 max-w-4xl text-base text-slate-400">
              Manage internal organization units, physical locations, and people from one StaffArr setup screen.
            </p>
          </div>
          <div className="flex flex-wrap gap-3">
            <button type="button" className={secondaryButtonClass}>
              Import
            </button>
            <button type="button" className={secondaryButtonClass}>
              <Download className="h-4 w-4" />
              Export
            </button>
            <button type="button" onClick={contextButton.onClick} className={primaryButtonClass}>
              <Plus className="h-4 w-4" />
              {tab === 'people' ? 'Create' : 'Create'}
            </button>
          </div>
        </div>
      </div>

      <section className={`${surfaceClass} overflow-hidden`}>
        <div className="border-b border-slate-800 px-5 py-3">
          <div className="flex flex-wrap gap-3">
            <TabButton active={tab === 'organization'} icon={Layers3} label="Organization" onClick={() => setParam({ tab: 'organization', mode: null })} />
            <TabButton active={tab === 'locations'} icon={MapPin} label="Locations" onClick={() => setParam({ tab: 'locations', mode: null })} />
            <TabButton active={tab === 'people'} icon={Users} label="People" onClick={() => setParam({ tab: 'people', mode: null })} />
          </div>
        </div>

        <div className="p-5">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <h3 className="text-4xl font-semibold text-white">{title}</h3>
              <p className="mt-3 max-w-4xl text-base text-slate-400">{subtitle}</p>
            </div>
            <div className="flex flex-wrap items-center gap-3">
              {tab === 'locations' ? (
                <select
                  value={selectedSiteId}
                  onChange={(event) => setSelectedSiteId(event.target.value)}
                  className="rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-sm text-slate-100 outline-none transition focus:border-cyan-500"
                >
                  {siteUnits.map((site) => (
                    <option key={site.orgUnitId} value={site.orgUnitId}>
                      {site.name}
                    </option>
                  ))}
                </select>
              ) : null}

              <label className="relative block">
                <Search className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-[var(--color-text-muted)]" />
                <input
                  value={tab === 'organization' ? orgSearch : tab === 'locations' ? locationSearch : peopleSearch}
                  onChange={(event) => {
                    if (tab === 'organization') setOrgSearch(event.target.value)
                    if (tab === 'locations') setLocationSearch(event.target.value)
                    if (tab === 'people') setPeopleSearch(event.target.value)
                  }}
                  placeholder="Search..."
                  className="w-64 rounded-2xl border border-slate-700 bg-slate-950 py-3 pl-11 pr-4 text-sm text-white outline-none transition focus:border-cyan-500"
                />
              </label>

              <button type="button" onClick={contextButton.onClick} className={primaryButtonClass}>
                <Plus className="h-4 w-4" />
                {contextButton.label}
              </button>
            </div>
          </div>

          <div className="mt-6 grid gap-6 xl:grid-cols-[460px_minmax(0,1fr)]">
            {tab === 'organization' ? (
              <>
                <div className="rounded-[24px] border border-slate-800 bg-slate-950/45 p-4">
                  <div className="flex items-center justify-between gap-3">
                    <h4 className="text-2xl font-semibold text-white">Organization Tree</h4>
                    <button type="button" className="rounded-2xl border border-slate-700 px-4 py-2 text-sm text-slate-200 transition hover:border-slate-500">
                      Expand all
                    </button>
                  </div>

                  <div className="mt-5 space-y-2">
                    {filteredOrgRows.map(({ node, depth }) => (
                      <button
                        key={node.orgUnitId}
                        type="button"
                        onClick={() => setParam({ orgUnitId: node.orgUnitId })}
                        className={`w-full rounded-[20px] border px-4 py-4 text-left transition ${
                          selectedOrgUnit?.orgUnitId === node.orgUnitId
                            ? 'border-blue-500 bg-blue-500/15'
                            : 'border-transparent hover:border-slate-700 hover:bg-slate-900/70'
                        }`}
                        style={{ paddingLeft: `${18 + depth * 42}px` }}
                      >
                        <div className="flex items-start gap-3">
                          {depth > 0 ? <ChevronRight className="mt-1 h-4 w-4 rotate-90 text-[var(--color-text-muted)]" /> : <span className="mt-1 h-4 w-4" />}
                          <div>
                            <p className="text-2xl font-semibold text-white">{node.name}</p>
                            <p className="mt-1 text-base text-slate-400">{humanize(node.unitType)}</p>
                          </div>
                        </div>
                      </button>
                    ))}
                  </div>
                </div>

                <div className="rounded-[24px] border border-slate-800 bg-slate-950/45 p-6">
                  <div className="flex flex-wrap items-start justify-between gap-4">
                    <div className="flex items-start gap-4">
                      <div className="rounded-[18px] bg-slate-800/80 p-4 text-cyan-400">
                        <Building2 className="h-7 w-7" />
                      </div>
                      <div>
                        <p className="text-sm font-semibold uppercase tracking-[0.16em] text-slate-400">Organization Unit</p>
                        <h4 className="mt-2 text-4xl font-semibold text-white">{selectedOrgUnit?.name ?? 'No unit selected'}</h4>
                      </div>
                    </div>
                    {selectedOrgUnit ? (
                      <div className="flex flex-wrap gap-3">
                        <button type="button" onClick={openEditUnit} className={secondaryButtonClass}>
                          <Edit3 className="h-4 w-4" />
                          Edit
                        </button>
                        <button type="button" onClick={() => void archiveSelectedUnit()} className={secondaryButtonClass}>
                          <Archive className="h-4 w-4" />
                          Archive
                        </button>
                        <button type="button" disabled className="inline-flex items-center gap-2 rounded-2xl border border-red-500/35 px-5 py-3 text-sm font-medium text-red-200 opacity-70">
                          <Trash2 className="h-4 w-4" />
                          Delete
                        </button>
                      </div>
                    ) : null}
                  </div>

                  {selectedOrgUnit ? (
                    <div className="mt-8">
                      <DetailTable
                        rows={[
                          { label: 'Type', value: humanize(selectedOrgUnit.unitType) },
                          { label: 'Parent', value: selectedOrgUnit.parentOrgUnitId ? orgUnitsById.get(selectedOrgUnit.parentOrgUnitId)?.name ?? 'Unknown' : 'Top level unit' },
                          { label: 'Status', value: humanize(selectedOrgUnit.status) },
                          { label: 'Assigned People', value: selectedOrgUnit.assignmentCount ?? 0 },
                          { label: 'Child Units', value: selectedOrgUnit.descendantCount ?? 0 },
                          { label: 'Default Site', value: selectedOrgUnit.defaultSiteOrgUnitId ? orgUnitsById.get(selectedOrgUnit.defaultSiteOrgUnitId)?.name ?? 'Unknown' : 'None' },
                          { label: 'Manager', value: selectedOrgUnit.managerPersonId ? state.people.find((person) => person.personId === selectedOrgUnit.managerPersonId)?.displayName ?? 'Assigned' : 'None' },
                          { label: 'Path', value: formatOrgPath(selectedOrgUnit, orgUnitsById) },
                        ]}
                      />
                    </div>
                  ) : (
                    <p className="mt-6 text-sm text-slate-400">Select a unit from the tree to inspect it.</p>
                  )}
                </div>
              </>
            ) : null}

            {tab === 'locations' ? (
              <>
                <div className="rounded-[24px] border border-slate-800 bg-slate-950/45 p-4">
                  <div className="flex items-center justify-between gap-3">
                    <h4 className="text-2xl font-semibold text-white">Location Tree</h4>
                    <button type="button" className="rounded-2xl border border-slate-700 px-4 py-2 text-sm text-slate-200 transition hover:border-slate-500">
                      Expand all
                    </button>
                  </div>

                  {locationsQuery.isError ? (
                    <div className="mt-5">
                      <ApiErrorCallout
                        title="Locations unavailable"
                        message={getErrorMessage(locationsQuery.error, 'Unable to load locations for this site.')}
                        onRetry={() => void locationsQuery.refetch()}
                        retryLabel="Retry locations"
                      />
                    </div>
                  ) : locationsQuery.isLoading ? (
                    <p className="mt-5 text-sm text-slate-400">Loading locations…</p>
                  ) : filteredLocations.length === 0 ? (
                    <p className="mt-5 text-sm text-slate-400">No matching locations were found for this site.</p>
                  ) : (
                    <div className="mt-5 space-y-2">
                      {filteredLocations.map((location) => {
                        const depth = Math.max(location.parentPathSnapshot.split(' / ').length - 1, 0)
                        return (
                          <button
                            key={location.locationId}
                            type="button"
                            onClick={() => setParam({ locationId: location.locationId })}
                            className={`w-full rounded-[20px] border px-4 py-4 text-left transition ${
                              selectedLocation?.locationId === location.locationId
                                ? 'border-blue-500 bg-blue-500/15'
                                : 'border-transparent hover:border-slate-700 hover:bg-slate-900/70'
                            }`}
                            style={{ paddingLeft: `${18 + depth * 42}px` }}
                          >
                            <div className="flex items-start gap-3">
                              {depth > 0 ? <ChevronRight className="mt-1 h-4 w-4 rotate-90 text-[var(--color-text-muted)]" /> : <span className="mt-1 h-4 w-4" />}
                              <div>
                                <p className="text-2xl font-semibold text-white">{location.name}</p>
                                <p className="mt-1 text-base text-slate-400">{humanize(location.locationType)}</p>
                              </div>
                            </div>
                          </button>
                        )
                      })}
                    </div>
                  )}
                </div>

                <div className="rounded-[24px] border border-slate-800 bg-slate-950/45 p-6">
                  <div className="flex flex-wrap items-start justify-between gap-4">
                    <div className="flex items-start gap-4">
                      <div className="rounded-[18px] bg-slate-800/80 p-4 text-cyan-400">
                        <MapPin className="h-7 w-7" />
                      </div>
                      <div>
                        <p className="text-sm font-semibold uppercase tracking-[0.16em] text-slate-400">Location</p>
                        <h4 className="mt-2 text-4xl font-semibold text-white">{selectedLocation?.name ?? 'No location selected'}</h4>
                      </div>
                    </div>
                    {selectedLocation ? (
                      <div className="flex flex-wrap gap-3">
                        <button type="button" onClick={() => setParam({ mode: 'location-admin' })} className={secondaryButtonClass}>
                          <Edit3 className="h-4 w-4" />
                          Edit
                        </button>
                        <button type="button" onClick={() => setParam({ mode: 'location-admin' })} className={secondaryButtonClass}>
                          <Archive className="h-4 w-4" />
                          Archive
                        </button>
                        <button type="button" disabled className="inline-flex items-center gap-2 rounded-2xl border border-red-500/35 px-5 py-3 text-sm font-medium text-red-200 opacity-70">
                          <Trash2 className="h-4 w-4" />
                          Delete
                        </button>
                      </div>
                    ) : null}
                  </div>

                  {selectedLocation ? (
                    <div className="mt-8">
                      <DetailTable
                        rows={[
                          { label: 'Type', value: humanize(selectedLocation.locationType) },
                          { label: 'Parent', value: selectedLocation.parentLocationId ? locationById.get(selectedLocation.parentLocationId)?.name ?? selectedLocation.parentPathSnapshot : selectedSite?.name ?? 'Root' },
                          { label: 'Status', value: humanize(selectedLocation.status) },
                          { label: 'Child Locations', value: selectedLocation.descendantCount ?? 0 },
                          { label: 'Canonical Owner', value: 'StaffArr' },
                          { label: 'Site', value: selectedLocation.siteNameSnapshot },
                          { label: 'Allowed Usage', value: humanize(selectedLocation.allowedProductUsage) },
                          { label: 'Code', value: selectedLocation.locationNumber },
                        ]}
                      />
                    </div>
                  ) : (
                    <p className="mt-6 text-sm text-slate-400">Select a location from the tree to inspect it.</p>
                  )}
                </div>
              </>
            ) : null}

            {tab === 'people' ? (
              <>
                <div className="rounded-[24px] border border-slate-800 bg-slate-950/45 p-4">
                  <div className="flex items-center justify-between gap-3">
                    <h4 className="text-2xl font-semibold text-white">People Directory</h4>
                    <span className="rounded-2xl border border-slate-700 bg-slate-900/80 px-4 py-2 text-sm text-slate-100">
                      Active
                    </span>
                  </div>

                  <div className="mt-5 space-y-3">
                    {filteredPeople.map((person) => (
                      <button
                        key={person.personId}
                        type="button"
                        onClick={() => {
                          state.setSelectedPersonId(person.personId, { syncDetailQuery: false, replace: true })
                          setParam({ personId: person.personId })
                        }}
                        className={`w-full rounded-[22px] border px-4 py-4 text-left transition ${
                          selectedPerson?.personId === person.personId
                            ? 'border-blue-500 bg-blue-500/15'
                            : 'border-slate-800 bg-slate-950/35 hover:border-slate-700'
                        }`}
                      >
                        <div className="flex items-start justify-between gap-3">
                          <div>
                            <p className="text-2xl font-semibold text-white">{person.displayName}</p>
                            <p className="mt-2 text-base text-slate-300">
                              {person.jobTitle ?? 'No title'} · {person.primaryOrgUnitName ?? 'Unassigned'}
                            </p>
                            <p className="mt-2 text-sm text-slate-400">{person.primaryEmail}</p>
                          </div>
                          <SummaryPill label={humanize(person.employmentStatus)} />
                        </div>
                      </button>
                    ))}
                  </div>
                </div>

                <div className="rounded-[24px] border border-slate-800 bg-slate-950/45 p-6">
                  <div className="flex flex-wrap items-start justify-between gap-4">
                    <div className="flex items-start gap-4">
                      <div className="rounded-[18px] bg-slate-800/80 p-4 text-cyan-400">
                        <UserRound className="h-7 w-7" />
                      </div>
                      <div>
                        <p className="text-sm font-semibold uppercase tracking-[0.16em] text-slate-400">Person</p>
                        <h4 className="mt-2 text-4xl font-semibold text-white">{selectedPerson?.displayName ?? 'No person selected'}</h4>
                      </div>
                    </div>
                    {selectedPerson ? (
                      <div className="flex flex-wrap gap-3">
                        <button
                          type="button"
                          onClick={() =>
                            navigate(`/people/details?person=${encodeURIComponent(selectedPerson.personId)}&tab=overview`, {
                              state: { openEditor: true },
                            })
                          }
                          className={secondaryButtonClass}
                        >
                          <Edit3 className="h-4 w-4" />
                          Edit
                        </button>
                        <Link to={`/people/details?person=${encodeURIComponent(selectedPerson.personId)}&tab=history`} className={secondaryButtonClass}>
                          <Archive className="h-4 w-4" />
                          Archive
                        </Link>
                        <button type="button" disabled className="inline-flex items-center gap-2 rounded-2xl border border-red-500/35 px-5 py-3 text-sm font-medium text-red-200 opacity-70">
                          <Trash2 className="h-4 w-4" />
                          Delete
                        </button>
                      </div>
                    ) : null}
                  </div>

                  {selectedPerson ? (
                    <>
                      <div className="mt-8">
                        <DetailTable rows={currentPersonRows} />
                      </div>

                      <div className="mt-6 grid gap-5 xl:grid-cols-2">
                        <div className="rounded-[22px] border border-slate-800 bg-slate-950/45 p-5">
                          <h5 className="text-2xl font-semibold text-white">Assignments</h5>
                          <div className="mt-5 space-y-3">
                            <div className="flex items-center justify-between rounded-2xl bg-slate-900/80 px-4 py-3">
                              <span className="text-slate-400">Primary Department</span>
                              <span className="text-white">{primaryAssignment?.departmentName ?? selectedPerson.primaryOrgUnitName ?? 'None'}</span>
                            </div>
                            <div className="flex items-center justify-between rounded-2xl bg-slate-900/80 px-4 py-3">
                              <span className="text-slate-400">Primary Site</span>
                              <span className="text-white">{primaryAssignment?.siteName ?? 'None'}</span>
                            </div>
                            <div className="flex items-center justify-between rounded-2xl bg-slate-900/80 px-4 py-3">
                              <span className="text-slate-400">Manager</span>
                              <span className="text-white">{peoplePlacement?.managerDisplayName ?? 'None'}</span>
                            </div>
                          </div>
                        </div>

                        <div className="rounded-[22px] border border-slate-800 bg-slate-950/45 p-5">
                          <h5 className="text-2xl font-semibold text-white">Access Snapshot</h5>
                          <div className="mt-5 space-y-3">
                            <div className="flex items-center justify-between rounded-2xl bg-slate-900/80 px-4 py-3">
                              <span className="text-slate-400">Login</span>
                              <span className="text-white">{selectedPerson.canLoginSnapshot ? 'Enabled' : 'Disabled'}</span>
                            </div>
                            <div className="flex items-center justify-between rounded-2xl bg-slate-900/80 px-4 py-3">
                              <span className="text-slate-400">Permissions</span>
                              <span className="text-white">{state.effectivePermissions?.permissions.length ?? 0}</span>
                            </div>
                            <div className="flex items-center justify-between rounded-2xl bg-slate-900/80 px-4 py-3">
                              <span className="text-slate-400">Status</span>
                              <span className="text-white">{humanize(selectedPerson.employmentStatus)}</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    </>
                  ) : (
                    <p className="mt-6 text-sm text-slate-400">Select a person from the directory to inspect them.</p>
                  )}
                </div>
              </>
            ) : null}
          </div>
        </div>
      </section>

      <div className="rounded-2xl border border-slate-800 bg-slate-950/40 px-4 py-3 text-sm text-slate-400">
        StaffArr remains the canonical owner of internal people, organization structure, and internal locations. Other STL products consume these references without becoming their source of truth.
      </div>
    </section>
  )
}
