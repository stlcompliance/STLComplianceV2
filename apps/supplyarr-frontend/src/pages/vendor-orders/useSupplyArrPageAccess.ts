import { useQuery } from '@tanstack/react-query'
import { getMe } from '../../api/client'
import {
  canCreateVendorOrders,
  canManageVendorOrderSettings,
  canReadVendorOrders,
  canUpdateVendorOrders,
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
    canReadVendorOrders: canReadVendorOrders(tenantRoleKey, isPlatformAdmin),
    canCreateVendorOrders: canCreateVendorOrders(tenantRoleKey, isPlatformAdmin),
    canUpdateVendorOrders: canUpdateVendorOrders(tenantRoleKey, isPlatformAdmin),
    canManageVendorOrderSettings: canManageVendorOrderSettings(tenantRoleKey, isPlatformAdmin),
  }
}
