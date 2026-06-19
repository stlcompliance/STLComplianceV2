import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useAuth } from '../auth/AuthProvider'
import * as nexarr from '../api/nexarrClient'
import { canLaunchFromContext, isInSuiteProduct } from '../lib/permissions'
import { redirectToSuiteLoginIfSessionExpired } from '../lib/sessionRedirect'

export function useLaunchContextGate(productKey: string) {
  const { me } = useAuth()
  const query = useQuery({
    queryKey: ['launch-context', productKey, me?.tenantId],
    queryFn: () => nexarr.getLaunchContext(productKey),
    enabled: Boolean(me) && !isInSuiteProduct(productKey),
    select: (ctx) => canLaunchFromContext(ctx),
  })

  useEffect(() => {
    if (query.isError) {
      redirectToSuiteLoginIfSessionExpired(query.error, productKey)
    }
  }, [productKey, query.error, query.isError])

  return query
}
