export function readinessClass(readiness: string): string {
  if (readiness === 'ready') {
    return 'text-[var(--tone-success-text)]'
  }
  if (readiness === 'tenant_suspended') {
    return 'text-[var(--tone-danger-text)]'
  }
  return 'text-[var(--tone-warning-text)]'
}

export function resultClass(result: string): string {
  if (result.toLowerCase() === 'success') {
    return 'text-[var(--tone-success-text)]'
  }
  if (result.toLowerCase() === 'denied') {
    return 'text-[var(--tone-danger-text)]'
  }
  return 'text-[var(--color-text-secondary)]'
}
