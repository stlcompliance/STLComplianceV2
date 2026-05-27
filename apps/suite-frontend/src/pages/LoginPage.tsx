import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Navigate, useLocation } from 'react-router-dom'
import { z } from 'zod'
import { useAuth } from '../auth/AuthProvider'
import { getNexarrApiBaseUrl } from '../api/nexarrBaseUrl'
import { NexarrApiError } from '../api/types'

const loginSchema = z.object({
  email: z.email('Enter a valid email'),
  password: z.string().min(1, 'Password is required'),
})

type LoginForm = z.infer<typeof loginSchema>

const demoTenantId =
  import.meta.env.VITE_DEMO_TENANT_ID || '11111111-1111-1111-1111-111111111101'

export function LoginPage() {
  const { login, isAuthenticated } = useAuth()
  const location = useLocation()
  const [error, setError] = useState<string | null>(null)

  const from =
    (location.state as { from?: string } | null)?.from?.toString() ?? '/app'

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: 'admin@demo.stl',
      password: 'ChangeMe!Demo2026',
    },
  })

  if (isAuthenticated) {
    return <Navigate to={from} replace />
  }

  const onSubmit = handleSubmit(async (values) => {
    setError(null)
    try {
      await login(values.email, values.password, demoTenantId)
    } catch (err) {
      if (err instanceof NexarrApiError) {
        setError(err.message)
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
          Uses NexArr <code className="text-xs text-slate-300">/api/auth/login</code> (demo tenant).
        </p>

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

        {error && (
          <p className="mt-4 text-sm text-red-300" role="alert">
            {error}
          </p>
        )}

        <button
          type="submit"
          disabled={isSubmitting}
          className="mt-6 w-full rounded-md bg-stl-teal px-4 py-2 text-sm font-medium text-white hover:bg-stl-teal/90 disabled:opacity-60"
        >
          {isSubmitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </div>
  )
}
