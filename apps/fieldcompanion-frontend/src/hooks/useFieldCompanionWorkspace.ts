import { useQuery } from '@tanstack/react-query'

import { getMe } from '../api/client'
import { getAccessToken, loadSession } from '../auth/sessionStorage'

export function useFieldCompanionWorkspace() {
  const session = loadSession()
  const accessToken = getAccessToken(session) ?? ''

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
