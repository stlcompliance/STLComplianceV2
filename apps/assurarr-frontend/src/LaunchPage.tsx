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
      setError('Missing handoff code. Launch AssurArr from the suite.')
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
          setError(resolveNexArrLaunchFailureMessage('AssurArr', err))
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [navigate, searchParams])

  return (
    <main className="flex min-h-screen items-center justify-center p-6">
      <div className="max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center shadow-lg">
        {error ? (
          <>
            <h1 className="text-lg font-semibold text-rose-300">Launch failed</h1>
            <p className="mt-3 text-sm text-slate-300">{error}</p>
          </>
        ) : (
          <p className="text-slate-300">Redeeming AssurArr handoff...</p>
        )}
      </div>
    </main>
  )
}
