import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, Navigate, useLocation } from 'react-router-dom'
import { z } from 'zod'
import { useAuth } from '../auth/AuthProvider'
import { getNexarrApiBaseUrl } from '../api/nexarrBaseUrl'
import { NexarrApiError } from '../api/types'
import * as nexarr from '../api/nexarrClient'
import { formatLaunchFailureError } from '../lib/launchFailure'
import { resolveLoginRedirectTarget } from '../lib/loginRedirect'
import { ApiErrorCallout } from '@stl/shared-ui/ApiErrorCallout'
import { buildProductLaunchUrlMap } from '@stl/shared-ui/productLaunchUrls'

const loginSchema = z.object({
  email: z.email('Enter a valid email'),
  password: z.string().min(1, 'Password is required'),
  rememberDevice: z.boolean(),
  mfaMethod: z.enum(['totp', 'recovery']),
  mfaCode: z.string().optional(),
  recoveryCode: z.string().optional(),
})

type LoginForm = z.infer<typeof loginSchema>

const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)

function formatProductLaunchRedirectError(error: unknown): string {
  if (error instanceof NexarrApiError) {
    return formatLaunchFailureError(error.code ?? error.message)
  }
  if (error instanceof Error) {
    return error.message
  }
  return 'NexArr could not relaunch this product.'
}

function LoginStatusPanel({
  title,
  message,
  error,
}: {
  title: string
  message: string
  error?: string | null
}) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-[#0f172a] px-4">
      <div className="w-full max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8">
        <p className="text-xs font-semibold uppercase tracking-wide text-stl-teal">
          STL Compliance Suite
        </p>
        <h1 className="mt-1 text-2xl font-semibold text-white">{title}</h1>
        <p className="mt-2 text-sm text-slate-400">{message}</p>
        {error ? (
          <div className="mt-4">
            <ApiErrorCallout message={error} />
            <Link to="/app" className="mt-4 inline-flex text-sm text-stl-teal hover:underline">
              Return to suite
            </Link>
          </div>
        ) : null}
      </div>
    </div>
  )
}

export function LoginPage() {
  const { login, isAuthenticated } = useAuth()
  const location = useLocation()
  const [error, setError] = useState<string | null>(null)
  const [launchRedirectError, setLaunchRedirectError] = useState<string | null>(null)
  const [isLaunchingProduct, setIsLaunchingProduct] = useState(false)
  const [mfaChallengeRequired, setMfaChallengeRequired] = useState(false)

  const locationState = location.state as { from?: string; passwordReset?: boolean } | null
  const redirectTarget = useMemo(
    () => resolveLoginRedirectTarget(location.search, productLaunchUrls),
    [location.search],
  )
  const from =
    redirectTarget?.kind === 'internal'
      ? redirectTarget.to
      : locationState?.from?.toString() ?? '/app'
  const passwordResetDone = locationState?.passwordReset === true

  useEffect(() => {
    setLaunchRedirectError(null)
  }, [location.search])

  useEffect(() => {
    if (!isAuthenticated || redirectTarget?.kind !== 'product' || launchRedirectError) {
      return
    }

    let cancelled = false
    setIsLaunchingProduct(true)
    void nexarr
      .createHandoff(redirectTarget.productKey, redirectTarget.callbackUrl)
      .then((handoff) => {
        if (!cancelled) {
          window.location.assign(handoff.launchUrl)
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setLaunchRedirectError(formatProductLaunchRedirectError(err))
        }
      })
      .finally(() => {
        if (!cancelled) {
          setIsLaunchingProduct(false)
        }
      })

    return () => {
      cancelled = true
    }
  }, [isAuthenticated, launchRedirectError, redirectTarget])

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
      rememberDevice: false,
      mfaMethod: 'totp',
      mfaCode: '',
      recoveryCode: '',
    },
  })
  const mfaMethod = watch('mfaMethod')

  if (isAuthenticated && redirectTarget?.kind === 'product') {
    return (
      <LoginStatusPanel
        title={launchRedirectError ? 'Product launch blocked' : 'Launching product'}
        message={
          launchRedirectError
            ? 'NexArr could not authorize the product handoff for this callback.'
            : isLaunchingProduct
              ? 'Creating a fresh NexArr handoff for your product workspace…'
              : 'Preparing your product workspace…'
        }
        error={launchRedirectError}
      />
    )
  }

  if (isAuthenticated) {
    return <Navigate to={from} replace />
  }

  const onSubmit = handleSubmit(async (values) => {
    setError(null)
    try {
      const mfaCode = mfaChallengeRequired && values.mfaMethod === 'totp' ? values.mfaCode?.trim() || null : undefined
      const recoveryCode =
        mfaChallengeRequired && values.mfaMethod === 'recovery'
          ? values.recoveryCode?.trim() || null
          : undefined

      if (mfaChallengeRequired) {
        if (values.mfaMethod === 'totp' && !mfaCode) {
          setError('Enter the six-digit authentication code from your authenticator app.')
          return
        }

        if (values.mfaMethod === 'recovery' && !recoveryCode) {
          setError('Enter one of your recovery codes.')
          return
        }
      }

      await login(values.email, values.password, null, values.rememberDevice, mfaCode, recoveryCode)
    } catch (err) {
      if (err instanceof NexarrApiError) {
        setError(err.message)
        if (err.code === 'auth.mfa_required') {
          setMfaChallengeRequired(true)
        }
      } else {
        const apiBase = getNexarrApiBaseUrl() || '(same origin — dev proxy only)'
        setError(`Sign-in failed. Could not reach NexArr at ${apiBase}.`)
      }
    }
  })

  return (
    <div className="flex min-h-screen items-center justify-center bg-[#0f172a] px-4">
      <form
        onSubmit={onSubmit}
        className="w-full max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8"
      >
        <p className="text-xs font-semibold uppercase tracking-wide text-stl-teal">
          STL Compliance Suite
        </p>
        <h1 className="mt-1 text-2xl font-semibold text-white">Sign in</h1>
        <p className="mt-2 text-sm text-slate-400">
          Uses NexArr <code className="text-xs text-slate-300">/api/auth/login</code>
          .
        </p>

        {passwordResetDone && (
          <p className="mt-4 text-sm text-emerald-300" role="status">
            Password updated. Sign in with your new password.
          </p>
        )}

        <label className="mt-6 block text-sm font-medium text-slate-300" htmlFor="email">
          Email
        </label>
        <input
          id="email"
          type="email"
          autoComplete="username"
          className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          {...register('email')}
        />
        {errors.email && (
          <p className="mt-1 text-xs text-red-300">{errors.email.message}</p>
        )}

        <label className="mt-4 block text-sm font-medium text-slate-300" htmlFor="password">
          Password
        </label>
        <input
          id="password"
          type="password"
          autoComplete="current-password"
          className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          {...register('password')}
        />
        {errors.password && (
          <p className="mt-1 text-xs text-red-300">{errors.password.message}</p>
        )}

        <label className="mt-4 flex items-center gap-2 text-sm text-slate-300" htmlFor="rememberDevice">
          <input
            id="rememberDevice"
            type="checkbox"
            className="h-4 w-4 rounded border-slate-600 bg-slate-950"
            {...register('rememberDevice')}
          />
          Remember this device
        </label>
        <p className="mt-1 text-xs text-slate-500">
          When enabled, NexArr issues a longer-lived refresh token for this browser.
        </p>

        {mfaChallengeRequired && (
          <div className="mt-4 rounded-lg border border-amber-700 bg-amber-950/40 p-4">
            <p className="text-sm font-medium text-amber-100">Multi-factor authentication required</p>
            <p className="mt-1 text-xs text-amber-200/80">
              Enter a 6-digit code from your authenticator app or use a recovery code to finish sign-in.
            </p>
            <div className="mt-3 flex flex-wrap gap-4 text-sm text-slate-200">
              <label className="inline-flex items-center gap-2">
                <input
                  type="radio"
                  value="totp"
                  className="h-4 w-4 border-slate-600 bg-slate-950"
                  {...register('mfaMethod')}
                />
                Authenticator code
              </label>
              <label className="inline-flex items-center gap-2">
                <input
                  type="radio"
                  value="recovery"
                  className="h-4 w-4 border-slate-600 bg-slate-950"
                  {...register('mfaMethod')}
                />
                Recovery code
              </label>
            </div>
            {mfaMethod === 'recovery' ? (
              <div className="mt-3">
                <label className="block text-sm font-medium text-slate-300" htmlFor="recoveryCode">
                  Recovery code
                </label>
                <input
                  id="recoveryCode"
                  type="text"
                  autoComplete="one-time-code"
                  className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  {...register('recoveryCode')}
                />
              </div>
            ) : (
              <div className="mt-3">
                <label className="block text-sm font-medium text-slate-300" htmlFor="mfaCode">
                  Authentication code
                </label>
                <input
                  id="mfaCode"
                  type="text"
                  inputMode="numeric"
                  autoComplete="one-time-code"
                  className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  {...register('mfaCode')}
                />
              </div>
            )}
          </div>
        )}

        <p className="mt-4 text-right text-sm">
          <Link to="/forgot-password" className="text-stl-teal hover:underline">
            Forgot password?
          </Link>
        </p>

        {error && (
          <div className="mt-4">
            <ApiErrorCallout message={error} />
          </div>
        )}

        <button
          type="submit"
          disabled={isSubmitting}
          className="mt-6 w-full rounded-md bg-stl-teal px-4 py-2 text-sm font-medium text-white hover:bg-stl-teal/90 disabled:opacity-60"
        >
          {isSubmitting ? 'Signing in…' : mfaChallengeRequired ? 'Verify and sign in' : 'Sign in'}
        </button>
      </form>
    </div>
  )
}
