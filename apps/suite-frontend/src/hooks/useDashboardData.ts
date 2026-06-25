import { useQuery } from '@tanstack/react-query'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'

export function useDashboardData() {
  const { me, session } = useAuth()

  const tenantsQuery = useQuery({
    queryKey: ['me-tenants', me?.userId],
    queryFn: () => nexarr.getMyTenants(),
    enabled: me !== undefined,
  })

  const navigationQuery = useQuery({
    queryKey: ['navigation', me?.tenantId],
    queryFn: () => nexarr.getNavigation(),
    enabled: me !== undefined,
  })

  const isLoading = tenantsQuery.isLoading || navigationQuery.isLoading

  const error = tenantsQuery.error ?? navigationQuery.error ?? null

  return {
    me,
    session,
    tenants: tenantsQuery.data ?? [],
    navigationProducts: navigationQuery.data?.products ?? [],
    isLoading,
    error,
  }
}
