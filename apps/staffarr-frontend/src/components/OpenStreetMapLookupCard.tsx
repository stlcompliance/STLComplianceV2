import { useEffect, useMemo, useState } from 'react'

type OpenStreetMapLookupCardProps = {
  query: string | null | undefined
  label: string
  description: string
  emptyMessage: string
  heightClassName?: string
}

type GeocodeMatch = {
  lat: string
  lon: string
}

type LookupState =
  | { kind: 'idle' }
  | { kind: 'loading' }
  | { kind: 'resolved'; match: GeocodeMatch }
  | { kind: 'unresolved' }
  | { kind: 'error' }

function toCoordinate(value: string): number | null {
  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : null
}

function clamp(value: number, minimum: number, maximum: number) {
  return Math.min(Math.max(value, minimum), maximum)
}

function buildEmbedUrl(match: GeocodeMatch) {
  const latitude = toCoordinate(match.lat)
  const longitude = toCoordinate(match.lon)

  if (latitude == null || longitude == null) {
    return null
  }

  const delta = 0.005
  const left = clamp(longitude - delta, -180, 180)
  const right = clamp(longitude + delta, -180, 180)
  const bottom = clamp(latitude - delta, -90, 90)
  const top = clamp(latitude + delta, -90, 90)
  const bbox = `${left},${bottom},${right},${top}`

  return {
    latitude,
    longitude,
    embedUrl: `https://www.openstreetmap.org/export/embed.html?bbox=${encodeURIComponent(bbox)}&layer=mapnik&marker=${latitude},${longitude}`,
    openUrl: `https://www.openstreetmap.org/?mlat=${latitude}&mlon=${longitude}#map=16/${latitude}/${longitude}`,
  }
}

export function OpenStreetMapLookupCard({
  query,
  label,
  description,
  emptyMessage,
  heightClassName = 'h-72',
}: OpenStreetMapLookupCardProps) {
  const normalizedQuery = query?.trim() ?? ''
  const [state, setState] = useState<LookupState>(normalizedQuery ? { kind: 'loading' } : { kind: 'idle' })

  useEffect(() => {
    if (!normalizedQuery) {
      setState({ kind: 'idle' })
      return
    }

    const controller = new AbortController()
    setState({ kind: 'loading' })

    void fetch(
      `https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&q=${encodeURIComponent(normalizedQuery)}`,
      {
        signal: controller.signal,
        headers: {
          Accept: 'application/json',
        },
      },
    )
      .then(async (response) => {
        if (!response.ok) {
          throw new Error(`Lookup failed with status ${response.status}`)
        }

        const results = (await response.json()) as GeocodeMatch[]
        if (results.length === 0) {
          setState({ kind: 'unresolved' })
          return
        }

        setState({ kind: 'resolved', match: results[0]! })
      })
      .catch((error: unknown) => {
        if (controller.signal.aborted) {
          return
        }

        console.warn('OpenStreetMap lookup failed', error)
        setState({ kind: 'error' })
      })

    return () => controller.abort()
  }, [normalizedQuery])

  const searchUrl = normalizedQuery
    ? `https://www.openstreetmap.org/search?query=${encodeURIComponent(normalizedQuery)}`
    : null
  const mapState = useMemo(
    () => (state.kind === 'resolved' ? buildEmbedUrl(state.match) : null),
    [state],
  )

  return (
    <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="text-sm font-medium text-slate-200">Embedded map</h3>
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">{description}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {mapState ? (
            <a
              href={mapState.openUrl}
              target="_blank"
              rel="noreferrer"
              className="rounded border border-sky-700 px-3 py-2 text-xs font-medium text-sky-200 hover:border-sky-500 hover:text-sky-100"
            >
              Open map
            </a>
          ) : null}
          {searchUrl ? (
            <a
              href={searchUrl}
              target="_blank"
              rel="noreferrer"
              className="rounded border border-slate-700 px-3 py-2 text-xs font-medium text-slate-200 hover:border-slate-500 hover:text-white"
            >
              Search in OpenStreetMap
            </a>
          ) : null}
        </div>
      </div>

      <p className="mt-3 text-xs text-[var(--color-text-muted)]">Lookup label: {label}</p>

      {state.kind === 'idle' ? (
        <p className="mt-3 text-sm text-slate-400">{emptyMessage}</p>
      ) : null}

      {state.kind === 'loading' ? (
        <p className="mt-3 text-sm text-slate-400">Looking up this StaffArr site/location in OpenStreetMap…</p>
      ) : null}

      {state.kind === 'unresolved' ? (
        <p className="mt-3 text-sm text-slate-400">
          OpenStreetMap could not resolve this label precisely enough for an embedded map. Use the search action to refine it.
        </p>
      ) : null}

      {state.kind === 'error' ? (
        <p className="mt-3 text-sm text-slate-400">
          OpenStreetMap lookup is currently unavailable. You can still open the search result manually.
        </p>
      ) : null}

      {mapState ? (
        <>
          <iframe
            title={`OpenStreetMap preview for ${label}`}
            src={mapState.embedUrl}
            className={`mt-3 w-full rounded-lg border border-slate-800 ${heightClassName}`}
            loading="lazy"
            referrerPolicy="no-referrer-when-downgrade"
          />
          <p className="mt-2 text-xs text-[var(--color-text-muted)]">
            Resolved near {mapState.latitude.toFixed(5)}, {mapState.longitude.toFixed(5)} from the current StaffArr label.
          </p>
        </>
      ) : null}
    </div>
  )
}
