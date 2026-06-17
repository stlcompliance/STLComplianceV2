interface OpenStreetMapCardProps {
  latitude: number | string | null | undefined
  longitude: number | string | null | undefined
  label: string
  addressQuery?: string | null | undefined
  className?: string
  heightClassName?: string
  emptyMessage?: string
  zoom?: number
}

interface MapState {
  latitude: number
  longitude: number
  embedUrl: string
  openUrl: string
}

interface OpenStreetMapLocationInput {
  latitude: number | string | null | undefined
  longitude: number | string | null | undefined
  addressQuery?: string | null | undefined
  zoom?: number
}

interface OpenStreetMapLink {
  source: 'address' | 'coordinates'
  url: string
}

function toCoordinate(value: number | string | null | undefined): number | null {
  if (typeof value === 'number') {
    return Number.isFinite(value) ? value : null
  }

  if (typeof value !== 'string' || !value.trim()) {
    return null
  }

  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : null
}

function clamp(value: number, minimum: number, maximum: number) {
  return Math.min(Math.max(value, minimum), maximum)
}

function buildMapState(
  latitude: number | string | null | undefined,
  longitude: number | string | null | undefined,
  zoom = 16,
): MapState | null {
  const lat = toCoordinate(latitude)
  const lon = toCoordinate(longitude)

  if (lat == null || lon == null || lat < -90 || lat > 90 || lon < -180 || lon > 180) {
    return null
  }

  const delta = 0.005
  const left = clamp(lon - delta, -180, 180)
  const right = clamp(lon + delta, -180, 180)
  const bottom = clamp(lat - delta, -90, 90)
  const top = clamp(lat + delta, -90, 90)
  const bbox = `${left},${bottom},${right},${top}`

  return {
    latitude: lat,
    longitude: lon,
    embedUrl: `https://www.openstreetmap.org/export/embed.html?bbox=${encodeURIComponent(bbox)}&layer=mapnik&marker=${lat},${lon}`,
    openUrl: `https://www.openstreetmap.org/?mlat=${lat}&mlon=${lon}#map=${zoom}/${lat}/${lon}`,
  }
}

export function buildOpenStreetMapAddressUrl(addressQuery: string | null | undefined): string | null {
  const trimmed = addressQuery?.trim()
  return trimmed ? `https://www.openstreetmap.org/search?query=${encodeURIComponent(trimmed)}` : null
}

export function buildOpenStreetMapUrl({
  latitude,
  longitude,
  addressQuery,
  zoom,
}: OpenStreetMapLocationInput): OpenStreetMapLink | null {
  const addressUrl = buildOpenStreetMapAddressUrl(addressQuery)
  if (addressUrl) {
    return { source: 'address', url: addressUrl }
  }

  const mapState = buildMapState(latitude, longitude, zoom)
  return mapState ? { source: 'coordinates', url: mapState.openUrl } : null
}

export function OpenStreetMapCard({
  latitude,
  longitude,
  label,
  addressQuery,
  className,
  heightClassName = 'h-64',
  emptyMessage = 'Add latitude and longitude to preview this location in OpenStreetMap.',
  zoom,
}: OpenStreetMapCardProps) {
  const mapState = buildMapState(latitude, longitude, zoom)
  const addressText = addressQuery?.trim() ?? ''
  const primaryLink = buildOpenStreetMapUrl({ latitude, longitude, addressQuery, zoom })
  const coordinateLink = addressText && mapState ? mapState.openUrl : null

  return (
    <div className={`rounded-xl border border-slate-800 bg-slate-950/80 p-4 ${className ?? ''}`}>
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h4 className="text-sm font-semibold text-white">OpenStreetMap</h4>
          <p className="mt-1 text-xs text-slate-400">{label}</p>
          {addressText ? (
            <p className="mt-1 text-xs text-slate-500">{addressText}</p>
          ) : mapState ? (
            <p className="mt-1 text-xs text-slate-500">
              {mapState.latitude.toFixed(5)}, {mapState.longitude.toFixed(5)}
            </p>
          ) : null}
          {addressText && mapState ? (
            <p className="mt-1 text-xs text-slate-600">
              Geofence fallback {mapState.latitude.toFixed(5)}, {mapState.longitude.toFixed(5)}
            </p>
          ) : null}
        </div>
        <div className="flex flex-wrap gap-2">
          {primaryLink ? (
            <a
              href={primaryLink.url}
              target="_blank"
              rel="noreferrer"
              className="rounded border border-sky-700 px-3 py-1 text-xs font-medium text-sky-200 hover:border-sky-500 hover:text-sky-100"
            >
              {primaryLink.source === 'address' ? 'Open address' : 'Open map'}
            </a>
          ) : null}
          {coordinateLink ? (
            <a
              href={coordinateLink}
              target="_blank"
              rel="noreferrer"
              className="rounded border border-slate-700 px-3 py-1 text-xs font-medium text-slate-200 hover:border-slate-500 hover:text-white"
            >
              Open coordinates
            </a>
          ) : null}
        </div>
      </div>

      {mapState ? (
        <>
          <iframe
            title={`OpenStreetMap preview for ${label}`}
            src={mapState.embedUrl}
            className={`mt-3 w-full rounded-lg border border-slate-800 ${heightClassName}`}
            loading="lazy"
            referrerPolicy="no-referrer-when-downgrade"
          />
          <p className="mt-2 text-xs text-slate-500">
            {addressText
              ? 'Open map uses the RoutArr stop address; this preview uses optional geofence coordinates.'
              : 'Preview uses RoutArr stop coordinates and OpenStreetMap tiles.'}
          </p>
        </>
      ) : addressText ? (
        <p className="mt-3 text-sm text-slate-400">
          OpenStreetMap will search the RoutArr stop address. Geofence coordinates are optional for proximity checks.
        </p>
      ) : (
        <p className="mt-3 text-sm text-slate-400">{emptyMessage}</p>
      )}
    </div>
  )
}
