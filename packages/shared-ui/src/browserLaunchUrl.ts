function getCurrentLocation(): Location | null {
  if (typeof window === 'undefined') {
    return null
  }

  return window.location ?? null
}

export function normalizeBrowserLaunchUrl(candidate: string): string {
  const currentLocation = getCurrentLocation()
  if (!currentLocation) {
    return candidate
  }

  let parsed: URL
  try {
    parsed = new URL(candidate, currentLocation.href)
  } catch {
    return candidate
  }

  const sameHost =
    parsed.hostname === currentLocation.hostname && parsed.port === currentLocation.port

  if (sameHost && currentLocation.protocol === 'https:' && parsed.protocol === 'http:') {
    parsed.protocol = 'https:'
    return parsed.toString().replace(/\/$/, '')
  }

  return candidate
}
