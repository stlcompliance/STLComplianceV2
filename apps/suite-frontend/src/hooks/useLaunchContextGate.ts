import { useQuery } from '@tanstack/react-query'
import { useAuth } from '../auth/AuthProvider'
import * as nexarr from '../api/nexarrClient'
import { canLaunchFromContext, isInSuiteProduct } from '../lib/permissions'

export function useLaunchContextGate(productKey: string) {
  const { me } = useAuth()
  return useQuery({
    queryKey: ['launch-context', productKey, me?.tenantId],
    queryFn: () => nexarr.getLaunchContext(productKey),
    enabled: Boolean(me) && !isInSuiteProduct(productKey),
    select: (ctx) => canLaunchFromContext(ctx),
  })
}
