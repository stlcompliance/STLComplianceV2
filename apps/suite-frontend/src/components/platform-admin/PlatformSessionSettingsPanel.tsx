import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'

export function PlatformSessionSettingsPanel() {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [accessTokenMinutes, setAccessTokenMinutes] = useState('15')
  const [refreshTokenDays, setRefreshTokenDays] = useState('7')
  const [rememberedRefreshTokenDays, setRememberedRefreshTokenDays] = useState('7')
  const [requirePlatformAdminMfa, setRequirePlatformAdminMfa] = useState(false)
  const [passwordMinLength, setPasswordMinLength] = useState('12')
  const [requirePasswordComplexity, setRequirePasswordComplexity] = useState(true)

  const settingsQuery = useQuery({
    queryKey: ['platform-session-settings'],
    queryFn: () => nexarr.getPlatformSessionSettings(),
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setAccessTokenMinutes(String(data.accessTokenMinutes))
    setRefreshTokenDays(String(data.refreshTokenDays))
    setRememberedRefreshTokenDays(String(data.rememberedRefreshTokenDays))
    setRequirePlatformAdminMfa(data.requirePlatformAdminMfa)
    setPasswordMinLength(String(data.passwordMinLength))
    setRequirePasswordComplexity(data.requirePasswordComplexity)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      nexarr.upsertPlatformSessionSettings({
        accessTokenMinutes: Number.parseInt(accessTokenMinutes, 10) || 15,
        refreshTokenDays: Number.parseInt(refreshTokenDays, 10) || 7,
        rememberedRefreshTokenDays: Number.parseInt(rememberedRefreshTokenDays, 10) || 7,
        requirePlatformAdminMfa,
        passwordMinLength: Number.parseInt(passwordMinLength, 10) || 12,
        requirePasswordComplexity,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-session-settings'] })
    },
  })

  return (
    <section
      className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4"
      data-testid="platform-session-settings-panel"
    >
      <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Session policy</h2>
      <p className="mt-1 text-sm text-[var(--color-text-muted)]">
        Controls access-token and refresh-session lifetimes for NexArr, Field Companion, and launched
        product sessions.
      </p>

      {settingsQuery.isError && (
        <ApiErrorCallout
          className="mt-3"
          message={getErrorMessage(settingsQuery.error, 'Failed to load session settings.')}
          onRetry={() => void settingsQuery.refetch()}
          retryLabel="Retry settings"
        />
      )}

      <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        <label htmlFor="platform-session-access-minutes" className="block text-sm text-[var(--color-text-secondary)]">
          Access token minutes
          <input
            id="platform-session-access-minutes"
            className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            type="number"
            min={5}
            max={480}
            value={accessTokenMinutes}
            onChange={(event) => setAccessTokenMinutes(event.target.value)}
            data-testid="platform-session-access-minutes"
          />
        </label>

        <label htmlFor="platform-session-refresh-days" className="block text-sm text-[var(--color-text-secondary)]">
          Refresh session days
          <input
            id="platform-session-refresh-days"
            className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            type="number"
            min={1}
            max={90}
            value={refreshTokenDays}
            onChange={(event) => setRefreshTokenDays(event.target.value)}
            data-testid="platform-session-refresh-days"
          />
        </label>

        <label htmlFor="platform-session-remembered-days" className="block text-sm text-[var(--color-text-secondary)]">
          Remembered device days
          <input
            id="platform-session-remembered-days"
            className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            type="number"
            min={1}
            max={365}
            value={rememberedRefreshTokenDays}
            onChange={(event) => setRememberedRefreshTokenDays(event.target.value)}
            data-testid="platform-session-remembered-days"
          />
        </label>
        <label htmlFor="platform-session-password-min-length" className="block text-sm text-[var(--color-text-secondary)]">
          Password minimum length
          <input
            id="platform-session-password-min-length"
            className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            type="number"
            min={8}
            max={128}
            value={passwordMinLength}
            onChange={(event) => setPasswordMinLength(event.target.value)}
            data-testid="platform-session-password-min-length"
          />
        </label>
      </div>

      <div className="mt-4 grid gap-3 lg:grid-cols-2">
        <label className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]" htmlFor="platform-session-require-admin-mfa">
          <input
            id="platform-session-require-admin-mfa"
            className="h-4 w-4 rounded border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)]"
            type="checkbox"
            checked={requirePlatformAdminMfa}
            onChange={(event) => setRequirePlatformAdminMfa(event.target.checked)}
            data-testid="platform-session-require-admin-mfa"
          />
          Require MFA for platform admins
        </label>
        <label className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]" htmlFor="platform-session-require-password-complexity">
          <input
            id="platform-session-require-password-complexity"
            className="h-4 w-4 rounded border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)]"
            type="checkbox"
            checked={requirePasswordComplexity}
            onChange={(event) => setRequirePasswordComplexity(event.target.checked)}
            data-testid="platform-session-require-password-complexity"
          />
          Require uppercase, lowercase, and a digit in passwords
        </label>
      </div>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        When enabled, platform-admin sign-in requires an account with MFA already turned on.
      </p>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        Password policy applies to platform-user creation and password resets.
      </p>

      <div className="mt-4 flex items-center gap-3">
        <button
          type="button"
          className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="platform-session-save"
        >
          {saveMutation.isPending ? 'Saving...' : 'Save settings'}
        </button>
        {saveMutation.isError && (
          <ApiErrorCallout
            message={getErrorMessage(saveMutation.error, 'Failed to save session settings.')}
          />
        )}
        {saveMutation.isSuccess && <span className="text-sm text-[var(--color-success-text)]">Saved.</span>}
      </div>
    </section>
  )
}
