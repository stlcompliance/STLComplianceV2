export function readinessClass(readiness: string): string {
  if (readiness === 'ready') {
    return 'text-green-700'
  }
  if (readiness === 'tenant_suspended') {
    return 'text-red-700'
  }
  return 'text-amber-700'
}

export function resultClass(result: string): string {
  if (result.toLowerCase() === 'success') {
    return 'text-green-700'
  }
  if (result.toLowerCase() === 'denied') {
    return 'text-red-700'
  }
  return 'text-slate-700'
}
