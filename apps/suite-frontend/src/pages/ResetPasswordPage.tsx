import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import * as nexarr from '../api/nexarrClient'
import { NexarrApiError } from '../api/types'
import { ApiErrorCallout } from '@stl/shared-ui/ApiErrorCallout'

const resetSchema = z
  .object({
    token: z.string().min(1, 'Reset token is required'),
    newPassword: z
      .string()
      .min(12, 'Password must be at least 12 characters')
      .regex(/[a-z]/, 'Include a lowercase letter')
      .regex(/[A-Z]/, 'Include an uppercase letter')
      .regex(/\d/, 'Include a digit'),
    confirmPassword: z.string().min(1, 'Confirm your password'),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

type ResetForm = z.infer<typeof resetSchema>

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const tokenFromQuery = searchParams.get('token') ?? ''

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ResetForm>({
    resolver: zodResolver(resetSchema),
    defaultValues: {
      token: tokenFromQuery,
      newPassword: '',
      confirmPassword: '',
    },
  })

  const onSubmit = handleSubmit(async (values) => {
    setError(null)
    try {
      await nexarr.resetPassword(values.token, values.newPassword)
      navigate('/login', { replace: true, state: { passwordReset: true } })
    } catch (err) {
      if (err instanceof NexarrApiError) {
        setError(err.message)
      } else {
        setError('NexArr is temporarily unavailable. Please try again later.')
      }
    }
  })

  return (
    <div className="flex min-h-screen items-center justify-center bg-[var(--color-bg-app)] px-4">
      <form
        onSubmit={onSubmit}
        className="w-full max-w-md rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-8"
      >
        <p className="text-xs font-semibold uppercase tracking-wide text-[var(--color-accent)]">
          STL Compliance Suite
        </p>
        <h1 className="mt-1 text-2xl font-semibold text-[var(--color-text-primary)]">Choose a new password</h1>
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">
          Use at least 12 characters with uppercase, lowercase, and a number.
        </p>

        <label className="mt-6 block text-sm font-medium text-[var(--color-text-secondary)]" htmlFor="token">
          Reset token
        </label>
        <input
          id="token"
          type="text"
          autoComplete="off"
          className="mt-1 w-full rounded-md border border-[var(--color-border-default)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 font-mono text-xs text-[var(--color-text-primary)]"
          {...register('token')}
        />
        {errors.token && (
          <p className="mt-1 text-xs text-[var(--color-danger-text)]">{errors.token.message}</p>
        )}

        <label className="mt-4 block text-sm font-medium text-[var(--color-text-secondary)]" htmlFor="newPassword">
          New password
        </label>
        <input
          id="newPassword"
          type="password"
          autoComplete="new-password"
          className="mt-1 w-full rounded-md border border-[var(--color-border-default)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
          {...register('newPassword')}
        />
        {errors.newPassword && (
          <p className="mt-1 text-xs text-[var(--color-danger-text)]">{errors.newPassword.message}</p>
        )}

        <label className="mt-4 block text-sm font-medium text-[var(--color-text-secondary)]" htmlFor="confirmPassword">
          Confirm password
        </label>
        <input
          id="confirmPassword"
          type="password"
          autoComplete="new-password"
          className="mt-1 w-full rounded-md border border-[var(--color-border-default)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
          {...register('confirmPassword')}
        />
        {errors.confirmPassword && (
          <p className="mt-1 text-xs text-[var(--color-danger-text)]">{errors.confirmPassword.message}</p>
        )}

        {error && (
          <div className="mt-4">
            <ApiErrorCallout message={error} />
          </div>
        )}

        <button
          type="submit"
          disabled={isSubmitting}
          className="mt-6 w-full rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-button-primary-text)] transition-colors hover:bg-[var(--color-accent-strong)] disabled:opacity-60"
        >
          {isSubmitting ? 'Updating…' : 'Update password'}
        </button>

        <p className="mt-4 text-center text-sm text-[var(--color-text-muted)]">
          <Link to="/forgot-password" className="text-[var(--color-accent)] hover:underline">
            Request a new token
          </Link>
          {' · '}
          <Link to="/login" className="text-[var(--color-accent)] hover:underline">
            Sign in
          </Link>
        </p>
      </form>
    </div>
  )
}
