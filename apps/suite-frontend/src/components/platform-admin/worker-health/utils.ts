export function formatWhen(value: string | null | undefined) {
  if (!value) {
    return 'Never'
  }
  return new Date(value).toLocaleString()
}

export function healthBadgeClass(status: string) {
  if (status === 'Healthy') {
    return 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]'
  }
  if (status === 'Degraded' || status === 'NotConfigured') {
    return 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)]'
  }
  return 'bg-[var(--color-destructive-bg)] text-[var(--color-destructive-text)]'
}
