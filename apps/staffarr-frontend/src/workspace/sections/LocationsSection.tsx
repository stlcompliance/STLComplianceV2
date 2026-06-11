import { useEffect, useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, DetailBadge, getErrorMessage } from '@stl/shared-ui'
import { getLocation, listLocationChildren, listSiteLocations } from '../../api/client'
import { OpenStreetMapLookupCard } from '../../components/OpenStreetMapLookupCard'
import type {
  InternalLocationResponse,
  OrgUnitResponse,
  StaffArrIntegrationLocationResponse,
} from '../../api/types'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

function locationSummary(location: StaffArrIntegrationLocationResponse) {
  return [location.locationType, location.allowedProductUsage, location.status]
    .filter(Boolean)
    .join(' · ')
}

function buildOpenStreetMapSearchUrl(query: string | null) {
  if (!query?.trim()) return null
  return `https://www.openstreetmap.org/search?query=${encodeURIComponent(query.trim())}`
}

function buildSiteSearchQuery(site: OrgUnitResponse | null) {
  if (!site) return null
  return [site.name, site.siteType, 'site']
    .filter((value): value is string => Boolean(value?.trim()))
    .join(' ')
}

function buildLocationSearchQuery(location: InternalLocationResponse | StaffArrIntegrationLocationResponse | null) {
  if (!location) return null
  return [location.siteNameSnapshot, location.parentPathSnapshot, location.name, location.locationType]
    .filter((value): value is string => Boolean(value?.trim()))
    .join(' ')
}

export function LocationsSection({ state }: Props) {
  const sites = useMemo(
    () => state.orgUnits.filter((unit) => unit.unitType === 'site'),
    [state.orgUnits],
  )
  const selectedPersonSiteId = useMemo(() => {
    if (!state.selectedPerson?.primaryOrgUnitId) {
      return null
    }
    const selectedSite = state.orgUnits.find((unit) => unit.orgUnitId === state.selectedPerson?.primaryOrgUnitId)
    return selectedSite?.unitType === 'site' ? selectedSite.orgUnitId : null
  }, [state.orgUnits, state.selectedPerson?.primaryOrgUnitId])
  const [selectedSiteId, setSelectedSiteId] = useState<string | null>(selectedPersonSiteId)

  useEffect(() => {
    if (selectedPersonSiteId) {
      setSelectedSiteId(selectedPersonSiteId)
      return
    }
    if (!selectedSiteId && sites.length > 0) {
      setSelectedSiteId(sites[0]!.orgUnitId)
    }
  }, [selectedPersonSiteId, selectedSiteId, sites])

  const selectedSite = sites.find((site) => site.orgUnitId === selectedSiteId) ?? null
  const locationsQuery = useQuery({
    queryKey: ['staffarr-site-locations', state.accessToken, selectedSiteId],
    queryFn: () => listSiteLocations(state.accessToken, selectedSiteId!),
    enabled: Boolean(selectedSiteId),
  })

  const selectedSiteLocations = locationsQuery.data ?? []
  const [selectedLocationId, setSelectedLocationId] = useState<string | null>(null)

  useEffect(() => {
    if (selectedSiteLocations.length === 0) {
      setSelectedLocationId(null)
      return
    }

    const nextSelectedLocation = selectedSiteLocations.find((location) => location.locationId === selectedLocationId)
      ?? selectedSiteLocations[0]!
    if (nextSelectedLocation.locationId !== selectedLocationId) {
      setSelectedLocationId(nextSelectedLocation.locationId)
    }
  }, [selectedLocationId, selectedSiteLocations])

  const selectedLocation = selectedSiteLocations.find((location) => location.locationId === selectedLocationId) ?? null
  const locationDetailQuery = useQuery({
    queryKey: ['staffarr-location-detail', state.accessToken, selectedLocationId],
    queryFn: () => getLocation(state.accessToken, selectedLocationId!),
    enabled: Boolean(selectedLocationId),
  })
  const childLocationsQuery = useQuery({
    queryKey: ['staffarr-location-children', state.accessToken, selectedLocationId],
    queryFn: () => listLocationChildren(state.accessToken, selectedLocationId!),
    enabled: Boolean(selectedLocationId),
  })

  const selectedLocationDetail = locationDetailQuery.data ?? selectedLocation
  const childLocations = childLocationsQuery.data ?? []
  const selectedSiteMapUrl = buildOpenStreetMapSearchUrl(buildSiteSearchQuery(selectedSite))
  const selectedLocationMapUrl = buildOpenStreetMapSearchUrl(buildLocationSearchQuery(selectedLocationDetail))
  const selectedSiteMapQuery = buildSiteSearchQuery(selectedSite)
  const selectedLocationMapQuery = buildLocationSearchQuery(selectedLocationDetail)
  const assignedPeople = selectedLocationDetail
    ? state.people.filter((person) => person.primaryOrgUnitId === selectedLocationDetail.siteOrgUnitId)
    : []

  return (
    <section className="space-y-6">
      <div className="grid gap-6 lg:grid-cols-[minmax(0,320px)_minmax(0,1fr)]">
        <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-950/60 p-4">
          <div>
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Sites</h2>
            <p className="mt-1 text-sm text-slate-500">
              StaffArr owns the internal site identity. Locations are a reference view for operational use.
            </p>
          </div>
          <div className="space-y-3">
            {sites.length === 0 ? (
              <p className="text-sm text-slate-500">No active sites are available for this tenant.</p>
            ) : (
              sites.map((site) => (
                <button
                  key={site.orgUnitId}
                  type="button"
                  onClick={() => setSelectedSiteId(site.orgUnitId)}
                  className={`w-full rounded-lg border p-3 text-left transition ${
                    selectedSiteId === site.orgUnitId
                      ? 'border-sky-500 bg-sky-500/10'
                      : 'border-slate-700 bg-slate-900/50 hover:border-slate-500'
                  }`}
                >
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <p className="font-medium text-slate-100">{site.name}</p>
                      <p className="mt-1 text-xs text-slate-500">
                        {site.siteType ?? 'site'} · {site.status}
                      </p>
                    </div>
                    <DetailBadge
                      label={site.status}
                      tone={site.status === 'active' ? 'good' : site.status === 'inactive' ? 'neutral' : 'warn'}
                    />
                  </div>
                  <dl className="mt-3 grid gap-1 text-xs text-slate-500">
                    {site.timezone ? <div>Timezone: {site.timezone}</div> : null}
                    {site.phone ? <div>Phone: {site.phone}</div> : null}
                    {site.emergencyContact ? <div>Emergency: {site.emergencyContact}</div> : null}
                  </dl>
                </button>
              ))
            )}
          </div>
        </div>

        <div className="space-y-4 rounded-xl border border-slate-800 bg-slate-950/60 p-4">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Locations</h2>
              <p className="mt-1 text-sm text-slate-500">
                {selectedSite
                  ? `${selectedSite.name} and its operating locations`
                  : 'Select a site to see its operational location map.'}
              </p>
              {selectedSite ? (
                <p className="mt-2 text-xs text-slate-500">
                  Embedded OpenStreetMap uses StaffArr site and location labels because this view does not store
                  coordinates.
                </p>
              ) : null}
            </div>
            <div className="flex flex-wrap items-center gap-2">
              {selectedSite ? (
                <DetailBadge label={selectedSite.status} tone={selectedSite.status === 'active' ? 'good' : 'warn'} />
              ) : null}
              {selectedSiteMapUrl ? (
                <a
                  href={selectedSiteMapUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="rounded border border-sky-700 px-3 py-1 text-xs font-medium text-sky-200 hover:border-sky-500 hover:text-sky-100"
                >
                  Search site in OpenStreetMap
                </a>
              ) : null}
            </div>
          </div>

          {locationsQuery.isError ? (
            <ApiErrorCallout
              title="Location load failed"
              message={getErrorMessage(locationsQuery.error, 'Unable to load site locations.')}
              onRetry={() => void locationsQuery.refetch()}
              retryLabel="Retry locations"
            />
          ) : locationsQuery.isLoading ? (
            <p className="text-sm text-slate-400">Loading site locations…</p>
          ) : selectedSiteLocations.length === 0 ? (
            <p className="text-sm text-slate-500">
              No locations are registered for this site yet. The site still exists as the canonical StaffArr site.
            </p>
          ) : (
            <div className="grid gap-3 md:grid-cols-2">
              {selectedSiteLocations.map((location) => (
                <button
                  key={location.locationId}
                  type="button"
                  onClick={() => setSelectedLocationId(location.locationId)}
                  className={`rounded-lg border p-4 text-left transition ${
                    selectedLocationId === location.locationId
                      ? 'border-sky-500 bg-sky-500/10'
                      : 'border-slate-700 bg-slate-900/60 hover:border-slate-500'
                  }`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-slate-100">{location.name}</p>
                      <p className="mt-1 font-mono text-xs text-slate-500">{location.locationNumber}</p>
                    </div>
                    <DetailBadge
                      label={location.status}
                      tone={location.status === 'active' ? 'good' : location.status === 'restricted' ? 'warn' : 'neutral'}
                    />
                  </div>
                  <p className="mt-3 text-sm text-slate-300">{locationSummary(location)}</p>
                  <dl className="mt-3 grid gap-1 text-xs text-slate-500">
                    <div>Path: {location.parentPathSnapshot}</div>
                    <div>Site reference: {location.siteNameSnapshot}</div>
                    <div>Product usage: {location.allowedProductUsage}</div>
                  </dl>
                </button>
              ))}
            </div>
          )}
        </div>
      </div>

      <section className="rounded-xl border border-slate-800 bg-slate-950/60 p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Location detail</h2>
            <p className="mt-1 text-sm text-slate-500">
              {selectedLocationDetail
                ? `Selected location ${selectedLocationDetail.name} and its immediate children.`
                : 'Select a location to inspect its detail snapshot.'}
            </p>
          </div>
          {selectedLocationDetail ? (
            <DetailBadge
              label={selectedLocationDetail.status}
              tone={
                selectedLocationDetail.status === 'active'
                  ? 'good'
                  : selectedLocationDetail.status === 'restricted'
                    ? 'warn'
                    : 'neutral'
              }
            />
          ) : null}
        </div>

        {locationDetailQuery.isError ? (
          <ApiErrorCallout
            title="Location detail failed to load"
            message={getErrorMessage(locationDetailQuery.error, 'Unable to load location detail.')}
            onRetry={() => void locationDetailQuery.refetch()}
            retryLabel="Retry detail"
          />
        ) : selectedLocationDetail ? (
          <div className="mt-4 grid gap-4 lg:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
            <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
              <h3 className="text-sm font-medium text-slate-200">{selectedLocationDetail.name}</h3>
              <dl className="mt-3 grid gap-2 text-sm text-slate-300">
                <div className="flex justify-between gap-4">
                  <dt className="text-slate-500">Location ID</dt>
                  <dd className="font-mono text-xs">{selectedLocationDetail.locationId}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-slate-500">Parent path</dt>
                  <dd className="text-right">{selectedLocationDetail.parentPathSnapshot}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-slate-500">Site context</dt>
                  <dd className="text-right">{selectedLocationDetail.siteNameSnapshot}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-slate-500">Allowed usage</dt>
                  <dd className="text-right">{selectedLocationDetail.allowedProductUsage}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-slate-500">Location type</dt>
                  <dd className="text-right">{selectedLocationDetail.locationType}</dd>
                </div>
                <div className="flex justify-between gap-4">
                  <dt className="text-slate-500">Parent location</dt>
                  <dd className="text-right">
                    {selectedLocationDetail.parentLocationId ?? 'None'}
                  </dd>
                </div>
              </dl>
            </div>

            <div className="space-y-4">
              <OpenStreetMapLookupCard
                query={selectedLocationMapQuery ?? selectedSiteMapQuery}
                label={selectedLocationDetail.name}
                description="StaffArr owns the site and internal location identity. This embedded map resolves the current canonical labels through OpenStreetMap for visual context."
                emptyMessage="Select a site or location with a canonical StaffArr label to load an embedded map."
              />

              <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
                <h3 className="text-sm font-medium text-slate-200">Child locations</h3>
                {childLocationsQuery.isLoading ? (
                  <p className="mt-3 text-sm text-slate-400">Loading child locations…</p>
                ) : childLocationsQuery.isError ? (
                  <ApiErrorCallout
                    title="Child locations failed to load"
                    message={getErrorMessage(childLocationsQuery.error, 'Unable to load child locations.')}
                    onRetry={() => void childLocationsQuery.refetch()}
                    retryLabel="Retry children"
                  />
                ) : childLocations.length === 0 ? (
                  <p className="mt-3 text-sm text-slate-500">No child locations are registered beneath this location.</p>
                ) : (
                  <ul className="mt-3 space-y-2">
                    {childLocations.map((childLocation) => (
                      <li key={childLocation.locationId} className="rounded-md border border-slate-800 bg-slate-950/50 p-3">
                        <div className="flex items-center justify-between gap-3">
                          <div>
                            <p className="text-sm font-medium text-slate-100">{childLocation.name}</p>
                            <p className="mt-1 text-xs text-slate-500">{childLocation.locationNumber}</p>
                          </div>
                          <DetailBadge
                            label={childLocation.status}
                            tone={childLocation.status === 'active' ? 'good' : 'neutral'}
                          />
                        </div>
                        <p className="mt-2 text-xs text-slate-500">{locationSummary(childLocation)}</p>
                      </li>
                    ))}
                  </ul>
                )}
              </div>

              <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
                <h3 className="text-sm font-medium text-slate-200">People assigned</h3>
                <p className="mt-2 text-sm text-slate-500">
                  {selectedLocationDetail.siteNameSnapshot} primary assignment matches.
                </p>
                {assignedPeople.length === 0 ? (
                  <p className="mt-3 text-sm text-slate-500">
                    No people are currently assigned to the selected site context.
                  </p>
                ) : (
                  <ul className="mt-3 space-y-2">
                    {assignedPeople.slice(0, 8).map((person) => (
                      <li key={person.personId} className="rounded-md border border-slate-800 bg-slate-950/50 p-3">
                        <p className="text-sm font-medium text-slate-100">{person.displayName}</p>
                        <p className="mt-1 text-xs text-slate-500">
                          {person.jobTitle ?? 'No title'} · {person.employmentStatus}
                        </p>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            </div>
          </div>
        ) : (
          <p className="mt-4 text-sm text-slate-500">No location detail is available yet.</p>
        )}
      </section>

      <section className="rounded-xl border border-slate-800 bg-slate-950/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Scope note</h2>
        <p className="mt-2 text-sm text-slate-400">
          StaffArr owns internal sites and location identity. LoadArr, MaintainArr, RoutArr, and TrainArr consume these
          references for their own execution workflows.
        </p>
      </section>
    </section>
  )
}
