export function formatWhen(value: string | null | undefined) {
  if (!value) {
    return 'Never'
  }
  return new Date(value).toLocaleString()
}

export function healthBadgeClass(status: string) {
  if (status === 'Healthy') {
    return 'bg-emerald-100 text-emerald-800'
  }
  if (status === 'Degraded' || status === 'NotConfigured') {
    return 'bg-amber-100 text-amber-800'
  }
  return 'bg-red-100 text-red-800'
}
