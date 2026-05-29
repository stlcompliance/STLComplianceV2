export function entitlementStatusClass(status: string): string {
  const normalized = status.trim().toLowerCase()
  if (normalized === 'active') {
    return 'text-emerald-300'
  }
  return 'text-slate-400'
}
