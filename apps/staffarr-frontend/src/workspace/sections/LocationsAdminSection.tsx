import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import {
  ApiErrorCallout,
  DetailBadge,
  QuestionnaireFlow,
  StaticSearchPicker,
  getErrorMessage,
  type PickerOption,
} from '@stl/shared-ui'
import { OpenStreetMapLookupCard } from '../../components/OpenStreetMapLookupCard'
import {
  archiveLocation,
  createLocation,
  getLocation,
  listLocationTree,
  listLocations,
  updateLocation,
} from '../../api/client'
import type {
  ArchiveInternalLocationRequest,
  CreateInternalLocationRequest,
  InternalLocationResponse,
  LocationAllowedProductUsage,
  LocationStatus,
  LocationType,
  UpdateInternalLocationRequest,
} from '../../api/types'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

type LocationDraft = {
  name: string
  locationType: LocationType
  parentLocationId: string
  siteOrgUnitId: string
  code: string
  description: string
  status: LocationStatus
  allowedProductUsage: LocationAllowedProductUsage
}

type EditorMode = 'create' | 'edit'

const LOCATION_TYPE_OPTIONS: Array<{ value: LocationType; label: string }> = [
  { value: 'building', label: 'Building' },
  { value: 'warehouse', label: 'Warehouse' },
  { value: 'dock', label: 'Dock' },
  { value: 'room', label: 'Room' },
  { value: 'yard', label: 'Yard' },
  { value: 'parts_room', label: 'Parts room' },
  { value: 'staging_area', label: 'Staging area' },
  { value: 'quarantine_area', label: 'Quarantine area' },
  { value: 'inspection_hold', label: 'Inspection hold' },
  { value: 'receiving_staging', label: 'Receiving staging' },
  { value: 'putaway_queue', label: 'Putaway queue' },
  { value: 'maintenance_handoff', label: 'Maintenance handoff' },
  { value: 'service_counter', label: 'Service counter' },
  { value: 'technician_pickup', label: 'Technician pickup' },
  { value: 'service_truck', label: 'Service truck' },
  { value: 'shelf', label: 'Shelf' },
  { value: 'bin', label: 'Bin' },
  { value: 'parking_area', label: 'Parking area' },
  { value: 'work_cell', label: 'Work cell' },
  { value: 'production_line', label: 'Production line' },
  { value: 'office', label: 'Office' },
  { value: 'training_room', label: 'Training room' },
  { value: 'break_room', label: 'Break room' },
  { value: 'restricted_area', label: 'Restricted area' },
  { value: 'site', label: 'Site marker' },
  { value: 'company', label: 'Company marker' },
  { value: 'division', label: 'Division marker' },
  { value: 'region', label: 'Region marker' },
  { value: 'business_unit', label: 'Business unit marker' },
  { value: 'cost_center', label: 'Cost center marker' },
  { value: 'department', label: 'Department marker' },
  { value: 'team', label: 'Team marker' },
  { value: 'position', label: 'Position marker' },
  { value: 'other', label: 'Other' },
]

const LOCATION_STATUS_OPTIONS: Array<{ value: LocationStatus; label: string }> = [
  { value: 'planned', label: 'Planned' },
  { value: 'active', label: 'Active' },
  { value: 'inactive', label: 'Inactive' },
  { value: 'restricted', label: 'Restricted' },
  { value: 'archived', label: 'Archived' },
]

const ALLOWED_USAGE_OPTIONS: Array<{ value: LocationAllowedProductUsage; label: string }> = [
  { value: 'all', label: 'All products' },
  { value: 'maintainarr', label: 'MaintainArr' },
  { value: 'loadarr', label: 'LoadArr' },
  { value: 'routarr', label: 'RoutArr' },
  { value: 'trainarr', label: 'TrainArr' },
  { value: 'staffarr', label: 'StaffArr' },
  { value: 'compliancecore', label: 'ComplianceCore' },
]

function hasAnyPermission(
  permissions: StaffArrWorkspaceState['effectivePermissionsQuery']['data'],
  permissionKeys: string[],
) {
  if (!permissions) {
    return false
  }

  return permissions.permissions.some((permission) =>
    permissionKeys.some((key) => permission.permissionKey.toLowerCase() === key.toLowerCase()),
  )
}

function makeDraft(siteOrgUnitId: string, parentLocationId = ''): LocationDraft {
  return {
    name: '',
    locationType: 'building',
    parentLocationId,
    siteOrgUnitId,
    code: '',
    description: '',
    status: 'planned',
    allowedProductUsage: 'all',
  }
}

function hydrateDraft(location: InternalLocationResponse): LocationDraft {
  return {
    name: location.name,
    locationType: location.locationType,
    parentLocationId: location.parentLocationId ?? '',
    siteOrgUnitId: location.siteOrgUnitId ?? '',
    code: location.code ?? location.locationNumber ?? '',
    description: location.description ?? '',
    status: location.status,
    allowedProductUsage: location.allowedProductUsage,
  }
}

function locationTone(status: LocationStatus) {
  return status === 'active' ? 'good' : status === 'restricted' ? 'warn' : status === 'archived' ? 'neutral' : 'neutral'
}

function formatLocationLabel(location: InternalLocationResponse) {
  return `${location.parentPathSnapshot} · ${location.locationNumber}`
}

function buildSiteSearchQuery(siteName: string | null | undefined) {
  if (!siteName?.trim()) {
    return null
  }

  return `${siteName.trim()} site`
}

function buildLocationSearchQuery(location: InternalLocationResponse | null) {
  if (!location) {
    return null
  }

  return [location.siteNameSnapshot, location.parentPathSnapshot, location.name, location.locationType]
    .filter((value): value is string => Boolean(value?.trim()))
    .join(' ')
}

export function LocationsAdminSection({ state }: Props) {
  const sites = useMemo(
    () => state.orgUnits.filter((unit) => unit.unitType === 'site'),
    [state.orgUnits],
  )

  const preferredSiteId = useMemo(() => {
    if (!state.selectedPerson?.primaryOrgUnitId) {
      return sites[0]?.orgUnitId ?? null
    }

    const candidateSite = state.orgUnits.find((unit) => unit.orgUnitId === state.selectedPerson?.primaryOrgUnitId)
    if (candidateSite?.unitType === 'site') {
      return candidateSite.orgUnitId
    }

    return sites[0]?.orgUnitId ?? null
  }, [sites, state.orgUnits, state.selectedPerson?.primaryOrgUnitId])

  const [selectedSiteId, setSelectedSiteId] = useState<string | null>(preferredSiteId)
  const [includeArchived, setIncludeArchived] = useState(false)
  const [search, setSearch] = useState('')
  const [selectedType, setSelectedType] = useState<LocationType | ''>('')
  const [selectedStatus, setSelectedStatus] = useState<LocationStatus | ''>('')
  const [treeView, setTreeView] = useState(true)
  const [selectedLocationId, setSelectedLocationId] = useState<string | null>(null)
  const [editorMode, setEditorMode] = useState<EditorMode | null>(null)
  const [draft, setDraft] = useState<LocationDraft | null>(null)
  const [archiveReason, setArchiveReason] = useState('')
  const [archiveOpen, setArchiveOpen] = useState(false)

  useEffect(() => {
    if (selectedSiteId && sites.some((site) => site.orgUnitId === selectedSiteId)) {
      return
    }

    const nextSiteId = preferredSiteId ?? sites[0]?.orgUnitId ?? null
    if (nextSiteId !== selectedSiteId) {
      setSelectedSiteId(nextSiteId)
    }
  }, [preferredSiteId, selectedSiteId, sites])

  useEffect(() => {
    if (selectedStatus === 'archived' && !includeArchived) {
      setIncludeArchived(true)
    }
  }, [includeArchived, selectedStatus])

  const canReadLocations =
    state.effectivePermissionsQuery.isLoading ||
    state.effectivePermissionsQuery.isFetching ||
    state.me.isPlatformAdmin ||
    state.me.tenantRoleKey === 'tenant_admin' ||
    state.me.tenantRoleKey === 'staffarr_admin' ||
    state.me.tenantRoleKey === 'hr_admin' ||
    state.me.tenantRoleKey === 'supervisor' ||
    hasAnyPermission(state.effectivePermissionsQuery.data, [
      'staffarr.locations.read',
      'staffarr.locations.manage',
    ])

  const canManageLocations =
    state.effectivePermissionsQuery.isLoading ||
    state.effectivePermissionsQuery.isFetching ||
    state.me.isPlatformAdmin ||
    state.me.tenantRoleKey === 'tenant_admin' ||
    state.me.tenantRoleKey === 'staffarr_admin' ||
    state.me.tenantRoleKey === 'hr_admin' ||
    hasAnyPermission(state.effectivePermissionsQuery.data, [
      'staffarr.locations.create',
      'staffarr.locations.update',
      'staffarr.locations.archive',
      'staffarr.locations.manage',
    ])

  const locationsQuery = useQuery({
    queryKey: [
      'staffarr-locations',
      state.accessToken,
      selectedSiteId,
      includeArchived,
      search,
      selectedType,
      selectedStatus,
      treeView,
    ],
    queryFn: () =>
      treeView
        ? listLocationTree(state.accessToken, {
            includeArchived,
            search: search || undefined,
            type: selectedType || undefined,
            siteOrgUnitId: selectedSiteId || undefined,
          })
        : listLocations(state.accessToken, {
            includeArchived,
            search: search || undefined,
            type: selectedType || undefined,
            siteOrgUnitId: selectedSiteId || undefined,
          }),
    enabled: Boolean(state.accessToken && selectedSiteId),
  })

  const availableLocations = useMemo(() => {
    const items = locationsQuery.data ?? []
    if (!selectedStatus) {
      return items
    }
    return items.filter((location) => location.status === selectedStatus)
  }, [locationsQuery.data, selectedStatus])

  useEffect(() => {
    if (availableLocations.length === 0) {
      setSelectedLocationId(null)
      return
    }

    const selectedExists = selectedLocationId
      ? availableLocations.some((location) => location.locationId === selectedLocationId)
      : false
    if (!selectedExists) {
      setSelectedLocationId(availableLocations[0]!.locationId)
    }
  }, [availableLocations, selectedLocationId])

  const selectedLocationQuery = useQuery({
    queryKey: ['staffarr-location-detail', state.accessToken, selectedLocationId],
    queryFn: () => getLocation(state.accessToken, selectedLocationId!),
    enabled: Boolean(state.accessToken && selectedLocationId),
  })

  const selectedLocation =
    selectedLocationQuery.data ?? availableLocations.find((location) => location.locationId === selectedLocationId) ?? null

  const selectedSite = sites.find((site) => site.orgUnitId === selectedSiteId) ?? null
  const selectedSiteMapQuery = buildSiteSearchQuery(selectedSite?.name)
  const selectedLocationMapQuery = buildLocationSearchQuery(selectedLocation)
  const parentLocationOptions = useMemo<PickerOption[]>(
    () =>
      (locationsQuery.data ?? [])
        .filter((location) => location.status !== 'archived')
        .filter((location) => location.locationId !== selectedLocationId)
        .map((location) => ({
          value: location.locationId,
          label: formatLocationLabel(location),
        })),
    [locationsQuery.data, selectedLocationId],
  )

  const siteOptions = useMemo<PickerOption[]>(
    () =>
      sites.map((site) => ({
        value: site.orgUnitId,
        label: site.name,
      })),
    [sites],
  )

  useEffect(() => {
    if (editorMode !== 'create' || !draft) {
      return
    }

    if (draft.siteOrgUnitId !== selectedSiteId && selectedSiteId) {
      setDraft((current) =>
        current
          ? {
              ...current,
              siteOrgUnitId: selectedSiteId,
              parentLocationId: '',
            }
          : current,
      )
    }
  }, [draft, editorMode, selectedSiteId])

  const createMutation = useMutation({
    mutationFn: (request: CreateInternalLocationRequest) =>
      createLocation(state.accessToken, request),
    onSuccess: async () => {
      await state.queryClient.invalidateQueries({ queryKey: ['staffarr-locations', state.accessToken] })
      await state.queryClient.invalidateQueries({ queryKey: ['staffarr-location-detail', state.accessToken] })
      setEditorMode(null)
      setDraft(null)
    },
  })

  const updateMutation = useMutation({
    mutationFn: (payload: { locationId: string; request: UpdateInternalLocationRequest }) =>
      updateLocation(state.accessToken, payload.locationId, payload.request),
    onSuccess: async () => {
      await state.queryClient.invalidateQueries({ queryKey: ['staffarr-locations', state.accessToken] })
      await state.queryClient.invalidateQueries({ queryKey: ['staffarr-location-detail', state.accessToken] })
      setEditorMode(null)
      setDraft(null)
      setArchiveOpen(false)
      setArchiveReason('')
    },
  })

  const archiveMutation = useMutation({
    mutationFn: (payload: { locationId: string; request: ArchiveInternalLocationRequest }) =>
      archiveLocation(state.accessToken, payload.locationId, payload.request),
    onSuccess: async () => {
      await state.queryClient.invalidateQueries({ queryKey: ['staffarr-locations', state.accessToken] })
      await state.queryClient.invalidateQueries({ queryKey: ['staffarr-location-detail', state.accessToken] })
      setArchiveOpen(false)
      setArchiveReason('')
    },
  })

  const startCreate = () => {
    const nextSiteId = selectedSiteId ?? sites[0]?.orgUnitId ?? ''
    setEditorMode('create')
    setArchiveOpen(false)
    setArchiveReason('')
    setDraft(makeDraft(nextSiteId, selectedLocation?.locationId ?? ''))
  }

  const startEdit = () => {
    if (!selectedLocation) {
      return
    }

    setEditorMode('edit')
    setArchiveOpen(false)
    setArchiveReason('')
    setDraft(hydrateDraft(selectedLocation))
  }

  const saveDraft = async () => {
    if (!draft || !selectedSiteId) {
      return
    }

    const request = {
      name: draft.name.trim(),
      locationType: draft.locationType,
      parentLocationId: draft.parentLocationId || null,
      siteOrgUnitId: draft.siteOrgUnitId || null,
      code: draft.code.trim() || null,
      description: draft.description.trim() || null,
      status: draft.status,
      allowedProductUsage: draft.allowedProductUsage,
    }

    if (editorMode === 'create') {
      await createMutation.mutateAsync(request)
    } else if (selectedLocation) {
      await updateMutation.mutateAsync({
        locationId: selectedLocation.locationId,
        request,
      })
    }
  }

  const submitArchive = async () => {
    if (!selectedLocation || !archiveReason.trim()) {
      return
    }

    await archiveMutation.mutateAsync({
      locationId: selectedLocation.locationId,
      request: { reason: archiveReason.trim() },
    })
  }

  const editorSelectedSiteOption = siteOptions.find((option) => option.value === draft?.siteOrgUnitId)
  const editorSelectedParentOption = parentLocationOptions.find(
    (option) => option.value === draft?.parentLocationId,
  )

  const filteredParentOptions = useMemo(() => {
    if (!draft) {
      return parentLocationOptions
    }

    return parentLocationOptions.filter((option) => option.value !== selectedLocationId)
  }, [draft, parentLocationOptions, selectedLocationId])

  if (!canReadLocations) {
    return (
      <section className="space-y-4 rounded-xl border border-slate-800 bg-slate-950/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Locations</h2>
        <p className="text-sm text-[var(--color-text-muted)]">
          Your current permissions do not include location read access.
        </p>
      </section>
    )
  }

  const statusTone = selectedLocation ? locationTone(selectedLocation.status) : 'neutral'
  const locationList = availableLocations
  const treeDepth = (location: InternalLocationResponse) =>
    Math.max(location.parentPathSnapshot.split(' / ').length - 1, 0)

  return (
    <section className="space-y-6">
      <div className="grid gap-6 lg:grid-cols-[minmax(0,320px)_minmax(0,1fr)]">
        <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-950/60 p-4">
          <div>
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Sites</h2>
            <p className="mt-1 text-sm text-[var(--color-text-muted)]">
              Choose the canonical StaffArr site that owns the location tree you want to manage.
            </p>
          </div>

          <div className="space-y-2">
            {sites.length === 0 ? (
              <p className="text-sm text-[var(--color-text-muted)]">No sites are available for this tenant.</p>
            ) : (
              sites.map((site) => (
                <button
                  key={site.orgUnitId}
                  type="button"
                  onClick={() => {
                    setSelectedSiteId(site.orgUnitId)
                    setSelectedLocationId(null)
                    setEditorMode(null)
                    setDraft(null)
                    setArchiveOpen(false)
                  }}
                  className={`w-full rounded-lg border p-3 text-left transition ${
                    selectedSiteId === site.orgUnitId
                      ? 'border-sky-500 bg-sky-500/10'
                      : 'border-slate-700 bg-slate-900/50 hover:border-slate-500'
                  }`}
                >
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <p className="font-medium text-slate-100">{site.name}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">{site.siteType ?? 'site'} · {site.status}</p>
                    </div>
                    <DetailBadge label={site.status} tone={site.status === 'active' ? 'good' : 'neutral'} />
                  </div>
                </button>
              ))
            )}
          </div>
        </div>

        <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-950/60 p-4">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Locations</h2>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                {selectedSite
                  ? `Managing ${selectedSite.name}. Switch between tree and list views, then create or edit locations in place.`
                  : 'Select a site to manage its internal locations.'}
              </p>
            </div>
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => setTreeView((current) => !current)}
                className="rounded-md border border-slate-700 px-3 py-2 text-xs text-slate-200 hover:border-slate-500"
              >
                {treeView ? 'Tree view' : 'List view'}
              </button>
              <button
                type="button"
                onClick={() => setIncludeArchived((current) => !current)}
                className={`rounded-md border px-3 py-2 text-xs ${
                  includeArchived
                    ? 'border-amber-500/60 bg-amber-500/10 text-amber-100'
                    : 'border-slate-700 text-slate-200 hover:border-slate-500'
                }`}
              >
                {includeArchived ? 'Showing archived' : 'Hide archived'}
              </button>
              {canManageLocations ? (
                <button
                  type="button"
                  onClick={startCreate}
                  className="rounded-md bg-sky-600 px-3 py-2 text-xs font-medium text-white hover:bg-sky-500"
                >
                  Create location
                </button>
              ) : null}
            </div>
          </div>

          <div className="grid gap-3 md:grid-cols-3">
            <label className="block text-sm text-slate-300">
              Search
              <input
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Search names, codes, descriptions..."
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label className="block text-sm text-slate-300">
              Type
              <select
                value={selectedType}
                onChange={(event) => setSelectedType(event.target.value as LocationType | '')}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              >
                <option value="">All types</option>
                {LOCATION_TYPE_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
            <label className="block text-sm text-slate-300">
              Status
              <select
                value={selectedStatus}
                onChange={(event) => setSelectedStatus(event.target.value as LocationStatus | '')}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              >
                <option value="">All statuses</option>
                {LOCATION_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
          </div>

          {locationsQuery.isError ? (
            <ApiErrorCallout
              title="Locations unavailable"
              message={getErrorMessage(locationsQuery.error, 'Unable to load locations for this site.')}
              onRetry={() => void locationsQuery.refetch()}
              retryLabel="Retry locations"
            />
          ) : locationsQuery.isLoading ? (
            <p className="text-sm text-slate-400">Loading locations…</p>
          ) : locationList.length === 0 ? (
            <p className="text-sm text-[var(--color-text-muted)]">No matching locations were found for this site.</p>
          ) : (
            <div className="space-y-2">
              {locationList.map((location) => {
                const depth = treeDepth(location)
                return (
                  <button
                    key={location.locationId}
                    type="button"
                    onClick={() => {
                      setSelectedLocationId(location.locationId)
                      setEditorMode(null)
                      setArchiveOpen(false)
                    }}
                    className={`w-full rounded-lg border p-3 text-left transition ${
                      selectedLocationId === location.locationId
                        ? 'border-sky-500 bg-sky-500/10'
                        : 'border-slate-700 bg-slate-900/60 hover:border-slate-500'
                    }`}
                    style={{ paddingLeft: `${12 + depth * 16}px` }}
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="font-medium text-slate-100">{location.name}</p>
                        <p className="mt-1 font-mono text-xs text-[var(--color-text-muted)]">{location.locationNumber}</p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">{formatLocationLabel(location)}</p>
                      </div>
                      <DetailBadge label={location.status} tone={locationTone(location.status)} />
                    </div>
                  </button>
                )
              })}
            </div>
          )}
        </div>
      </div>

      <section className="rounded-xl border border-slate-800 bg-slate-950/60 p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Location detail</h2>
            <p className="mt-1 text-sm text-[var(--color-text-muted)]">
              {selectedLocation
                ? `Selected location ${selectedLocation.name} and its computed counts.`
                : 'Select a location to inspect, edit, or archive it.'}
            </p>
          </div>
          {selectedLocation ? (
            <div className="flex flex-wrap gap-2">
              <DetailBadge label={selectedLocation.status} tone={statusTone} />
              {canManageLocations ? (
                <>
                  <button
                    type="button"
                    onClick={startEdit}
                    className="rounded-md border border-slate-700 px-3 py-2 text-xs text-slate-200 hover:border-slate-500"
                  >
                    Edit
                  </button>
                  <button
                    type="button"
                    onClick={() => setArchiveOpen((current) => !current)}
                    className="rounded-md border border-amber-500/40 px-3 py-2 text-xs text-amber-100 hover:border-amber-400"
                  >
                    {archiveOpen ? 'Cancel archive' : 'Archive'}
                  </button>
                </>
              ) : null}
            </div>
          ) : null}
        </div>

        {selectedLocationQuery.isError ? (
          <ApiErrorCallout
            title="Location detail unavailable"
            message={getErrorMessage(selectedLocationQuery.error, 'Unable to load location detail.')}
            onRetry={() => void selectedLocationQuery.refetch()}
            retryLabel="Retry detail"
          />
        ) : selectedLocation ? (
          <div className="mt-4 grid gap-4 lg:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
            <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
              <h3 className="text-sm font-medium text-slate-200">{selectedLocation.name}</h3>
              <dl className="mt-3 grid gap-2 text-sm text-slate-300">
                <div className="flex justify-between gap-4">
                  <dt className="text-[var(--color-text-muted)]">Code</dt>
                  <dd className="font-mono text-xs">{selectedLocation.locationNumber}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-[var(--color-text-muted)]">Type</dt>
                  <dd className="text-right">{selectedLocation.locationType}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-[var(--color-text-muted)]">Site</dt>
                  <dd className="text-right">{selectedLocation.siteNameSnapshot}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-[var(--color-text-muted)]">Parent path</dt>
                  <dd className="text-right">{selectedLocation.parentPathSnapshot}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-[var(--color-text-muted)]">Allowed usage</dt>
                  <dd className="text-right">{selectedLocation.allowedProductUsage}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-[var(--color-text-muted)]">Children</dt>
                  <dd className="text-right">{selectedLocation.descendantCount ?? 0}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-[var(--color-text-muted)]">Assignments</dt>
                  <dd className="text-right">{selectedLocation.assignmentCount ?? 0}</dd>
                </div>
                {selectedLocation.description ? (
                  <div className="flex flex-col gap-1 pt-2">
                    <dt className="text-[var(--color-text-muted)]">Description</dt>
                    <dd>{selectedLocation.description}</dd>
                  </div>
                ) : null}
              </dl>
            </div>

            <div className="space-y-4">
              <OpenStreetMapLookupCard
                query={selectedLocationMapQuery ?? selectedSiteMapQuery}
                label={selectedLocation?.name ?? selectedSite?.name ?? 'StaffArr site'}
                description="StaffArr manages the canonical site and location labels here. The embedded map resolves those labels through OpenStreetMap without adding new persisted map fields."
                emptyMessage="Select a StaffArr site or location to load an embedded map."
              />

              {archiveOpen && canManageLocations ? (
                <div className="rounded-lg border border-amber-500/40 bg-amber-950/20 p-4">
                  <h3 className="text-sm font-medium text-amber-100">Archive location</h3>
                  <label className="mt-3 block text-sm text-slate-300">
                    Archive reason
                    <textarea
                      value={archiveReason}
                      onChange={(event) => setArchiveReason(event.target.value)}
                      className="mt-1 min-h-24 w-full rounded-md border border-amber-500/30 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                      placeholder="Why is this location being archived?"
                    />
                  </label>
                  <div className="mt-3 flex flex-wrap gap-2">
                    <button
                      type="button"
                      onClick={() => void submitArchive()}
                      disabled={archiveMutation.isPending || !archiveReason.trim()}
                      className="rounded-md bg-amber-600 px-3 py-2 text-sm font-medium text-white hover:bg-amber-500 disabled:opacity-50"
                    >
                      {archiveMutation.isPending ? 'Archiving...' : 'Archive location'}
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setArchiveOpen(false)
                        setArchiveReason('')
                      }}
                      className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:border-slate-500"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : null}

              {editorMode && draft ? (
                <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
              <h3 className="text-sm font-medium text-slate-200">
                    {editorMode === 'create' ? 'Create location' : 'Edit location'}
                  </h3>
                  {editorMode === 'create' ? (
                    <div className="mt-4">
                      <QuestionnaireFlow
                        apiBase={import.meta.env.VITE_COMPLIANCECORE_API_BASE ?? ''}
                        accessToken={state.session.accessToken}
                        tenantId={state.session.tenantId}
                        productKey="staffarr"
                        workflowKey="location_create"
                        subjectType="location"
                        subjectId=""
                        subjectLabel={draft.siteOrgUnitId ? `Location draft for ${siteOptions.find((site) => site.value === draft.siteOrgUnitId)?.label ?? 'selected site'}` : 'StaffArr location draft'}
                        sourceRecordId={`location-create-${draft.siteOrgUnitId || selectedSite?.orgUnitId || 'pending'}`}
                        sourceEntity="location"
                        knownFacts={{
                          ...(draft.locationType ? { 'location.kind': draft.locationType } : {}),
                        }}
                        title="Location questionnaire"
                        subtitle="Answer the short operational questions that help Compliance Core decide the right defaults for this location."
                      />
                    </div>
                  ) : null}
                  <div className="mt-4 grid gap-4 md:grid-cols-2">
                    <label className="block text-sm text-slate-300">
                      Name
                      <input
                        value={draft.name}
                        onChange={(event) => setDraft((current) => (current ? { ...current, name: event.target.value } : current))}
                        className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                        required
                      />
                    </label>
                    <label className="block text-sm text-slate-300">
                      Code
                      <input
                        value={draft.code}
                        onChange={(event) => setDraft((current) => (current ? { ...current, code: event.target.value } : current))}
                        className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                        placeholder="Optional. Auto-generated if blank."
                      />
                    </label>
                    <label className="block text-sm text-slate-300">
                      Location type
                      <select
                        value={draft.locationType}
                        onChange={(event) =>
                          setDraft((current) => (current ? { ...current, locationType: event.target.value as LocationType } : current))
                        }
                        className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                      >
                        {LOCATION_TYPE_OPTIONS.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label className="block text-sm text-slate-300">
                      Status
                      <select
                        value={draft.status}
                        onChange={(event) =>
                          setDraft((current) => (current ? { ...current, status: event.target.value as LocationStatus } : current))
                        }
                        className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                      >
                        {LOCATION_STATUS_OPTIONS.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label className="block text-sm text-slate-300">
                      Allowed product usage
                      <select
                        value={draft.allowedProductUsage}
                        onChange={(event) =>
                          setDraft((current) =>
                            current
                              ? { ...current, allowedProductUsage: event.target.value as LocationAllowedProductUsage }
                              : current,
                          )
                        }
                        className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                      >
                        {ALLOWED_USAGE_OPTIONS.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label className="block text-sm text-slate-300">
                      Site
                      <div className="mt-1">
                        <StaticSearchPicker
                          value={draft.siteOrgUnitId}
                          onChange={(value) =>
                            setDraft((current) =>
                              current
                                ? {
                                    ...current,
                                    siteOrgUnitId: value,
                                    parentLocationId: current.siteOrgUnitId === value ? current.parentLocationId : '',
                                  }
                                : current,
                            )
                          }
                          options={siteOptions}
                          selectedOption={editorSelectedSiteOption}
                          placeholder="Select a site"
                        />
                      </div>
                    </label>
                    <label className="block text-sm text-slate-300">
                      Parent location
                      <div className="mt-1">
                        <StaticSearchPicker
                          value={draft.parentLocationId}
                          onChange={(value) =>
                            setDraft((current) => (current ? { ...current, parentLocationId: value } : current))
                          }
                          options={filteredParentOptions}
                          selectedOption={editorSelectedParentOption}
                          placeholder="No parent (root)"
                        />
                      </div>
                    </label>
                    <label className="block text-sm text-slate-300 md:col-span-2">
                      Description
                      <textarea
                        value={draft.description}
                        onChange={(event) =>
                          setDraft((current) => (current ? { ...current, description: event.target.value } : current))
                        }
                        className="mt-1 min-h-24 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                      />
                    </label>
                  </div>
                  <div className="mt-4 flex flex-wrap gap-2">
                    <button
                      type="button"
                      onClick={() => void saveDraft()}
                      disabled={createMutation.isPending || updateMutation.isPending || !draft.name.trim() || !draft.siteOrgUnitId}
                      className="rounded-md bg-sky-600 px-3 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
                    >
                      {editorMode === 'create'
                        ? createMutation.isPending
                          ? 'Creating...'
                          : 'Create location'
                        : updateMutation.isPending
                          ? 'Saving...'
                          : 'Save changes'}
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setEditorMode(null)
                        setDraft(null)
                      }}
                      className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:border-slate-500"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              ) : selectedLocation ? (
                <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
                  <h3 className="text-sm font-medium text-slate-200">Selected location snapshot</h3>
                  <p className="mt-2 text-sm text-slate-400">
                    Use edit or archive actions to change this record.
                  </p>
                </div>
              ) : (
                <p className="text-sm text-[var(--color-text-muted)]">Select a location to see the detail snapshot.</p>
              )}
            </div>
          </div>
        ) : (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">No location detail is available yet.</p>
        )}
      </section>

      <section className="rounded-xl border border-slate-800 bg-slate-950/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Scope note</h2>
        <p className="mt-2 text-sm text-slate-400">
          StaffArr owns the canonical site org unit identity and the internal location model. Other products consume
          these references without becoming their source of truth.
        </p>
      </section>
    </section>
  )
}
