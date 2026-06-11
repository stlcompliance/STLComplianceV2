import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { resolveNexArrLaunchFailureMessage, resolveProductLaunchCallbackPath } from '@stl/shared-ui'
import { redeemHandoff } from './api/client'
import { saveSession, toStoredSession } from './auth/sessionStorage'

export function LaunchPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const handoff = searchParams.get('handoff')
    if (!handoff) {
      setError('Missing handoff code. Launch ReportArr from the suite.')
      return
    }

    let cancelled = false

    ;(async () => {
      try {
        const session = await redeemHandoff(handoff)
        if (cancelled) {
          return
        }
        saveSession(toStoredSession(session))
        navigate(resolveProductLaunchCallbackPath(session.callbackUrl), { replace: true })
      } catch (err) {
        if (!cancelled) {
          setError(resolveNexArrLaunchFailureMessage('ReportArr', err))
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [navigate, searchParams])

  return (
    <main className="reportarr-launch-screen">
      <div className="reportarr-launch-card">
        {error ? (
          <>
            <p className="reportarr-eyebrow">ReportArr</p>
            <h1>Launch failed</h1>
            <p>{error}</p>
          </>
        ) : (
          <>
            <p className="reportarr-eyebrow">ReportArr</p>
            <h1>Redeeming handoff</h1>
            <p>Loading your reporting workspace…</p>
          </>
        )}
      </div>
    </main>
  )
}
