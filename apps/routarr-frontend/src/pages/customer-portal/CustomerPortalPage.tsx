import { useQuery } from '@tanstack/react-query'

import { getMe } from '../../api/client'
import { CustomerPortalPanel } from '../../components/CustomerPortalPanel'
import { canReadDispatchReports, loadSession } from '../../auth/sessionStorage'

export function CustomerPortalPage() {
  const session = loadSession()
  const meQuery = useQuery({
    queryKey: ['routarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading customer portal…</p>
  }

  return (
    <CustomerPortalPanel
      accessToken={session.accessToken}
      canRead={canReadDispatchReports(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)}
    />
  )
}
