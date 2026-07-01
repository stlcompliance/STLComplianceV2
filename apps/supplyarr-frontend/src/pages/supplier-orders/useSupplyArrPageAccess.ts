import { useQuery } from '@tanstack/react-query'
import { getMe } from '../../api/client'
import {
  canCreateSupplierOrders,
  canManageSupplierOrderSettings,
  canReadSupplierOrders,
  canUpdateSupplierOrders,
  loadSession,
} from '../../auth/sessionStorage'

export function useSupplyArrPageAccess() {
  const session = loadSession()
  const meQuery = useQuery({
    queryKey: ['supplyarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const tenantRoleKey = meQuery.data?.tenantRoleKey ?? 'tenant_member'
  const isPlatformAdmin = meQuery.data?.isPlatformAdmin ?? false

  return {
    session,
    meQuery,
    canReadSupplierOrders: canReadSupplierOrders(tenantRoleKey, isPlatformAdmin),
    canCreateSupplierOrders: canCreateSupplierOrders(tenantRoleKey, isPlatformAdmin),
    canUpdateSupplierOrders: canUpdateSupplierOrders(tenantRoleKey, isPlatformAdmin),
    canManageSupplierOrderSettings: canManageSupplierOrderSettings(tenantRoleKey, isPlatformAdmin),
  }
}
