import { useQuery } from '@tanstack/react-query'

import { getMe } from '../api/client'
import { loadSession } from '../auth/sessionStorage'

export function useFieldCompanionWorkspace() {
  const session = loadSession()
  const accessToken = session?.accessToken ?? ''

  const meQuery = useQuery({
    queryKey: ['fieldcompanion-me', accessToken],
    queryFn: () => getMe(accessToken),
    enabled: Boolean(accessToken),
    retry: false,
  })

  return {
    session,
    accessToken,
    meQuery,
  }
}
