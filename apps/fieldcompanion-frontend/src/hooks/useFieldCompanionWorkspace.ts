import { useEffect, useState } from 'react'
import { useQuery } from '@tanstack/react-query'

import { getMe } from '../api/client'
import { getAccessToken, loadSession } from '../auth/sessionStorage'
import { getFieldCompanionSessionAccessTokenRenewalDeadlineMs } from '../lib/sessionSafety'

export function useFieldCompanionWorkspace() {
  const session = loadSession()
  const accessToken = getAccessToken(session) ?? ''
  const [sessionPulse, setSessionPulse] = useState(0)

  useEffect(() => {
    if (!session || !accessToken) {
      return
    }

    const renewalDeadlineMs = getFieldCompanionSessionAccessTokenRenewalDeadlineMs(session)
    if (renewalDeadlineMs === null) {
      return
    }

    const delayMs = Math.max(1_000, renewalDeadlineMs - Date.now())
    const timeoutId = window.setTimeout(() => {
      setSessionPulse((value) => value + 1)
    }, delayMs)

    return () => {
      window.clearTimeout(timeoutId)
    }
  }, [accessToken, session?.accessTokenExpiresAt, session?.sessionId, sessionPulse])

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
