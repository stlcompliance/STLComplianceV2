import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { formatProductLaunchError, resolveProductLaunchCallbackPath, saveThemePreferenceFromSession } from '@stl/shared-ui'
import { redeemHandoff } from './api/client'
import { saveSession, toStoredSession } from './auth/sessionStorage'

export function LaunchPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const handoff = searchParams.get('handoff')
    if (!handoff) {
      setError('Missing handoff code. Launch LedgArr from the suite.')
      return
    }

    let cancelled = false

    ;(async () => {
      try {
        const session = await redeemHandoff(handoff)
        if (cancelled) {
          return
        }
        saveThemePreferenceFromSession(session)
        saveSession(toStoredSession(session))
        navigate(resolveProductLaunchCallbackPath(session.callbackUrl), { replace: true })
      } catch (err) {
        if (!cancelled) {
          setError(formatProductLaunchError(err))
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [navigate, searchParams])

  return (
    <main className="flex min-h-screen items-center justify-center p-6">
      <div className="ledgarr-panel max-w-md p-8 text-center shadow-2xl">
        {error ? (
          <>
            <h1 className="text-lg font-semibold text-rose-200">Launch failed</h1>
            <p className="mt-3 text-sm text-slate-300">{error}</p>
          </>
        ) : (
          <p className="text-slate-300">Redeeming LedgArr handoff...</p>
        )}
      </div>
    </main>
  )
}
