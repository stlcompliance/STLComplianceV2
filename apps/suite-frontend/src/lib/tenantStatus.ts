export function normalizeTenantStatusValue(status: string | null | undefined): string {
  return (status ?? '').trim().toLowerCase()
}

export function isActiveTenantStatus(status: string | null | undefined): boolean {
  return normalizeTenantStatusValue(status) === 'active'
}
