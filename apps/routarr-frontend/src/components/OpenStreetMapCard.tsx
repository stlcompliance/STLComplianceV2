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
  const searchUrl = addressQuery?.trim()
    ? `https://www.openstreetmap.org/search?query=${encodeURIComponent(addressQuery.trim())}`
    : null

  return (
    <div className={`rounded-xl border border-slate-800 bg-slate-950/80 p-4 ${className ?? ''}`}>
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h4 className="text-sm font-semibold text-white">OpenStreetMap</h4>
          <p className="mt-1 text-xs text-slate-400">{label}</p>
          {mapState ? (
            <p className="mt-1 text-xs text-slate-500">
              {mapState.latitude.toFixed(5)}, {mapState.longitude.toFixed(5)}
            </p>
          ) : null}
        </div>
        <div className="flex flex-wrap gap-2">
          {mapState ? (
            <a
              href={mapState.openUrl}
              target="_blank"
              rel="noreferrer"
              className="rounded border border-sky-700 px-3 py-1 text-xs font-medium text-sky-200 hover:border-sky-500 hover:text-sky-100"
            >
              Open map
            </a>
          ) : null}
          {searchUrl ? (
            <a
              href={searchUrl}
              target="_blank"
              rel="noreferrer"
              className="rounded border border-slate-700 px-3 py-1 text-xs font-medium text-slate-200 hover:border-slate-500 hover:text-white"
            >
              Search address
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
            Preview uses RoutArr stop coordinates and OpenStreetMap tiles.
          </p>
        </>
      ) : (
        <p className="mt-3 text-sm text-slate-400">{emptyMessage}</p>
      )}
    </div>
  )
}
