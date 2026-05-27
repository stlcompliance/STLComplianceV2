/** NexArr API origin; empty string uses same-origin (Vite dev proxy). */
export function getNexarrApiBaseUrl(): string {
  const configured = import.meta.env.VITE_NEXARR_API_URL?.trim()
  if (configured) {
    return configured.replace(/\/$/, '')
  }
  return ''
}
