import { useQuery } from '@tanstack/react-query'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'

export function useDashboardData() {
  const { me, session } = useAuth()

  const entitlementsQuery = useQuery({
    queryKey: ['me-entitlements', me?.tenantId],
    queryFn: () => nexarr.getMyEntitlements(),
    enabled: me !== undefined,
  })

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

  const isLoading =
    entitlementsQuery.isLoading || tenantsQuery.isLoading || navigationQuery.isLoading

  const error =
    entitlementsQuery.error ?? tenantsQuery.error ?? navigationQuery.error ?? null

  return {
    me,
    session,
    entitlements: entitlementsQuery.data ?? [],
    tenants: tenantsQuery.data ?? [],
    navigationProducts: navigationQuery.data?.products ?? [],
    isLoading,
    error,
  }
}
