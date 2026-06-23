import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { z } from 'zod'
import * as nexarr from '../api/nexarrClient'
import { NexarrApiError } from '../api/types'
import { ApiErrorCallout } from '@stl/shared-ui/ApiErrorCallout'

const forgotSchema = z.object({
  email: z.email('Enter a valid email'),
})

type ForgotForm = z.infer<typeof forgotSchema>

export function ForgotPasswordPage() {
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)
  const [devResetToken, setDevResetToken] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotForm>({
    resolver: zodResolver(forgotSchema),
    defaultValues: { email: '' },
  })

  const onSubmit = handleSubmit(async (values) => {
    setError(null)
    setMessage(null)
    setDevResetToken(null)
    try {
      const response = await nexarr.requestPasswordReset(values.email)
      setMessage(response.message)
      if (response.devResetToken) {
        setDevResetToken(response.devResetToken)
      }
    } catch (err) {
      if (err instanceof NexarrApiError) {
        setError(err.message)
      } else {
        setError('Could not reach NexArr. Try again later.')
      }
    }
  })

  return (
    <div className="flex min-h-screen items-center justify-center bg-[var(--color-bg-app)] px-4">
      <form
        onSubmit={onSubmit}
        className="w-full max-w-md rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-8 shadow-[var(--shadow-surface)]"
      >
        <p className="text-xs font-semibold uppercase tracking-wide text-[var(--color-accent)]">
          STL Compliance Suite
        </p>
        <h1 className="mt-1 text-2xl font-semibold text-[var(--color-text-primary)]">Reset password</h1>
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">
          Enter your account email. If it exists, NexArr will issue a reset link (email delivery is
          not wired in local dev).
        </p>

        <label className="mt-6 block text-sm font-medium text-[var(--color-text-secondary)]" htmlFor="email">
          Email
        </label>
        <input
          id="email"
          type="email"
          autoComplete="username"
          className="mt-1 w-full rounded-md border border-[var(--color-border-default)] bg-[var(--color-field-bg)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
          {...register('email')}
        />
        {errors.email && (
          <p className="mt-1 text-xs text-[var(--tone-danger-text)]">{errors.email.message}</p>
        )}

        {message && (
          <p className="mt-4 text-sm text-[var(--tone-success-text)]" role="status">
            {message}
          </p>
        )}

        {devResetToken && (
          <div className="mt-4 rounded-md border border-[var(--tone-warning-border)] bg-[var(--tone-warning-bg)] p-3 text-sm text-[var(--tone-warning-text)]">
            <p className="font-medium">Local dev reset token</p>
            <p className="mt-1 break-all font-mono text-xs">{devResetToken}</p>
            <Link
              to={`/reset-password?token=${encodeURIComponent(devResetToken)}`}
              className="mt-2 inline-block text-[var(--color-accent)] hover:underline"
            >
              Continue to set a new password
            </Link>
          </div>
        )}

        {error && (
          <div className="mt-4">
            <ApiErrorCallout message={error} />
          </div>
        )}

        <button
          type="submit"
          disabled={isSubmitting}
          className="mt-6 w-full rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-60"
        >
          {isSubmitting ? 'Sending…' : 'Send reset link'}
        </button>

        <p className="mt-4 text-center text-sm text-[var(--color-text-muted)]">
          <Link to="/login" className="text-[var(--color-accent)] hover:underline">
            Back to sign in
          </Link>
        </p>
      </form>
    </div>
  )
}
