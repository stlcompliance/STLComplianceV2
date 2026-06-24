import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, ConfirmDialog, getErrorMessage } from '@stl/shared-ui'
import {
  disablePersonLogin,
  enablePersonLogin,
  getPersonAccountAccess,
  provisionPersonAccount,
  requestPersonPasswordReset,
  resetPersonMfa,
  updatePersonLoginEmail,
} from '../api/client'
import type { PersonAccountAccessSummaryResponse } from '../api/types'

interface PersonAccountAccessPanelProps {
  accessToken: string
  personId: string
  displayName: string
  workEmail: string
  canManage: boolean
}

type PendingConfirmAction = 'reset-mfa' | 'disable-login' | null

function accountStateLabel(summary: PersonAccountAccessSummaryResponse): string {
  switch (summary.accountState) {
    case 'no_platform_login':
      return 'No platform login'
    case 'invite_pending':
      return 'Pending sign-in setup'
    case 'login_disabled':
      return 'Login disabled'
    case 'login_locked':
      return 'Login locked'
    case 'password_change_required':
      return 'Password change required'
    case 'pending_verification':
      return 'Verification pending'
    case 'login_enabled':
      return 'Login enabled'
    case 'account_unavailable':
    default:
      return 'Account details unavailable'
  }
}

function formatDateTime(value: string | null): string {
  if (!value) {
    return 'Not recorded'
  }

  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) {
    return 'Not recorded'
  }

  return parsed.toLocaleString(undefined, {
    month: 'short',
    day: '2-digit',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  })
}

function FieldRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex min-h-11 items-center justify-between gap-4 rounded-xl border border-slate-800 bg-slate-950/50 px-4 py-2">
      <span className="text-sm text-slate-400">{label}</span>
      <span className="text-right text-sm font-semibold text-slate-100">{value}</span>
    </div>
  )
}

export function PersonAccountAccessPanel({
  accessToken,
  personId,
  displayName,
  workEmail,
  canManage,
}: PersonAccountAccessPanelProps) {
  const queryClient = useQueryClient()
  const [loginEmail, setLoginEmail] = useState(workEmail)
  const [temporaryPassword, setTemporaryPassword] = useState('')
  const [syncWorkEmail, setSyncWorkEmail] = useState(false)
  const [reason, setReason] = useState('')
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [pendingConfirmAction, setPendingConfirmAction] = useState<PendingConfirmAction>(null)

  const accountQuery = useQuery({
    queryKey: ['staffarr-person-account-access', accessToken, personId],
    queryFn: () => getPersonAccountAccess(accessToken, personId),
    enabled: Boolean(accessToken && personId),
  })

  useEffect(() => {
    const summary = accountQuery.data
    setLoginEmail(summary?.loginEmail ?? summary?.workEmail ?? workEmail)
    setSyncWorkEmail(Boolean(summary?.loginEmail && summary.loginEmail === summary.workEmail))
  }, [accountQuery.data, workEmail])

  const invalidate = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['staffarr-person-account-access', accessToken, personId] }),
      queryClient.invalidateQueries({ queryKey: ['staffarr-person', accessToken, personId] }),
      queryClient.invalidateQueries({ queryKey: ['staffarr-people', accessToken] }),
    ])
  }

  const provisionMutation = useMutation({
    mutationFn: () =>
      provisionPersonAccount(accessToken, personId, {
        loginEmail: loginEmail.trim(),
        temporaryPassword,
        syncWorkEmail,
      }),
    onSuccess: async (result) => {
      setSuccessMessage(result.message)
      setTemporaryPassword('')
      await invalidate()
    },
  })

  const updateEmailMutation = useMutation({
    mutationFn: () =>
      updatePersonLoginEmail(accessToken, personId, {
        loginEmail: loginEmail.trim(),
        syncWorkEmail,
      }),
    onSuccess: async (result) => {
      setSuccessMessage(result.message)
      await invalidate()
    },
  })

  const passwordResetMutation = useMutation({
    mutationFn: () => requestPersonPasswordReset(accessToken, personId, { reason: reason.trim() || null }),
    onSuccess: async (result) => {
      setSuccessMessage(result.message)
      setReason('')
      await invalidate()
    },
  })

  const mfaResetMutation = useMutation({
    mutationFn: () => resetPersonMfa(accessToken, personId, { reason: reason.trim() || null }),
    onSuccess: async (result) => {
      setSuccessMessage(result.message)
      setReason('')
      await invalidate()
    },
  })

  const disableMutation = useMutation({
    mutationFn: () => disablePersonLogin(accessToken, personId, { reason: reason.trim() || null }),
    onSuccess: async (result) => {
      setSuccessMessage(result.message)
      setReason('')
      await invalidate()
    },
  })

  const enableMutation = useMutation({
    mutationFn: () => enablePersonLogin(accessToken, personId, { reason: reason.trim() || null }),
    onSuccess: async (result) => {
      setSuccessMessage(result.message)
      setReason('')
      await invalidate()
    },
  })

  const mutationError =
    provisionMutation.error
    ?? updateEmailMutation.error
    ?? passwordResetMutation.error
    ?? mfaResetMutation.error
    ?? disableMutation.error
    ?? enableMutation.error
    ?? null

  const summary = accountQuery.data
  const isBusy =
    provisionMutation.isPending
    || updateEmailMutation.isPending
    || passwordResetMutation.isPending
    || mfaResetMutation.isPending
    || disableMutation.isPending
    || enableMutation.isPending

  const canProvision =
    summary?.accountState === 'no_platform_login'
    || summary?.accountState === 'invite_pending'
  const confirmResetMfa = pendingConfirmAction === 'reset-mfa'
  const confirmDisableLogin = pendingConfirmAction === 'disable-login'
  const confirmLoading = mfaResetMutation.isPending || disableMutation.isPending

  async function confirmPendingAction() {
    try {
      setSuccessMessage(null)
      if (pendingConfirmAction === 'reset-mfa') {
        await mfaResetMutation.mutateAsync()
      } else if (pendingConfirmAction === 'disable-login') {
        await disableMutation.mutateAsync()
      }
      setPendingConfirmAction(null)
    } catch {
      // Keep the dialog open so the user can retry or cancel after a failure.
    }
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h3 className="text-sm font-medium text-slate-200">Account access</h3>
          <p className="mt-1 text-xs text-slate-400">
            StaffArr manages this experience, but NexArr remains the source of truth for login state, security actions, and launch eligibility.
          </p>
        </div>
        <span className={`text-xs ${canManage ? 'text-emerald-300' : 'text-slate-400'}`}>
          {canManage ? 'Delegated actions enabled' : 'Read only'}
        </span>
      </header>

      {accountQuery.isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading account access...</p>
      ) : accountQuery.isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Account access unavailable"
            message={getErrorMessage(accountQuery.error, 'Failed to load account access.')}
            onRetry={() => void accountQuery.refetch()}
            retryLabel="Retry"
          />
        </div>
      ) : summary ? (
        <div className="mt-4 space-y-4">
          <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
            <FieldRow label="Account state" value={accountStateLabel(summary)} />
            <FieldRow label="Login email" value={summary.loginEmail ?? 'Not assigned'} />
            <FieldRow label="Last login" value={formatDateTime(summary.lastLoginAt)} />
            <FieldRow label="MFA" value={summary.hasPlatformLogin ? (summary.isMfaEnabled ? 'Enabled' : 'Not enabled') : 'No sign-in'} />
          </div>

          <div className="grid gap-5 xl:grid-cols-[1.1fr_0.9fr]">
            <div className="space-y-3 rounded-xl border border-slate-800 bg-slate-950/40 p-4">
              <FieldRow label="Work email" value={summary.workEmail} />
              <FieldRow label="Role coverage" value={summary.tenantRoleSummary ?? 'Not assigned'} />
              <FieldRow label="Product launch" value={summary.launchEligible ? 'Eligible through NexArr' : 'Unavailable right now'} />
              <FieldRow label="Last suite launch" value={formatDateTime(summary.lastProductLaunchAt)} />
              {summary.notice ? (
                <p className="rounded-lg border border-slate-800 bg-slate-950 px-3 py-2 text-xs text-slate-300">
                  {summary.notice}
                </p>
              ) : null}
            </div>

            {canManage ? (
              <div className="space-y-3 rounded-xl border border-slate-800 bg-slate-950/40 p-4">
                <label className="block text-sm text-slate-300" htmlFor="person-account-login-email">
                  Login email
                  <input
                    id="person-account-login-email"
                    type="email"
                    value={loginEmail}
                    onChange={(event) => setLoginEmail(event.target.value)}
                    className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  />
                </label>

                {canProvision ? (
                  <label className="block text-sm text-slate-300" htmlFor="person-account-temp-password">
                    Temporary sign-in password
                    <input
                      id="person-account-temp-password"
                      type="password"
                      value={temporaryPassword}
                      onChange={(event) => setTemporaryPassword(event.target.value)}
                      className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    />
                  </label>
                ) : null}

                <label className="flex items-center gap-2 text-sm text-slate-300">
                  <input
                    type="checkbox"
                    checked={syncWorkEmail}
                    onChange={(event) => setSyncWorkEmail(event.target.checked)}
                  />
                  Keep StaffArr work email aligned with the login email
                </label>

                <label className="block text-sm text-slate-300" htmlFor="person-account-reason">
                  Action note
                  <input
                    id="person-account-reason"
                    value={reason}
                    onChange={(event) => setReason(event.target.value)}
                    className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    placeholder="Optional context for the audit trail"
                  />
                </label>

                <div className="flex flex-wrap gap-2 pt-1">
                  {canProvision ? (
                    <button
                      type="button"
                      disabled={isBusy}
                      onClick={() => {
                        setSuccessMessage(null)
                        void provisionMutation.mutateAsync()
                      }}
                      className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
                    >
                      {summary.accountState === 'invite_pending' ? 'Complete sign-in setup' : 'Provision login'}
                    </button>
                  ) : (
                    <button
                      type="button"
                      disabled={isBusy}
                      onClick={() => {
                        setSuccessMessage(null)
                        void updateEmailMutation.mutateAsync()
                      }}
                      className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
                    >
                      Update login email
                    </button>
                  )}

                  {summary.hasPlatformLogin ? (
                    <>
                      <button
                        type="button"
                        disabled={isBusy}
                        onClick={() => {
                          setSuccessMessage(null)
                          void passwordResetMutation.mutateAsync()
                        }}
                        className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:bg-slate-800 disabled:opacity-50"
                      >
                        Send password reset
                      </button>
                      <button
                        type="button"
                        disabled={isBusy}
                        onClick={() => {
                          setPendingConfirmAction('reset-mfa')
                        }}
                        className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:bg-slate-800 disabled:opacity-50"
                      >
                        Reset MFA
                      </button>
                    </>
                  ) : null}

                  {summary.hasPlatformIdentity && summary.accountState === 'login_disabled' ? (
                    <button
                      type="button"
                      disabled={isBusy}
                      onClick={() => {
                        setSuccessMessage(null)
                        void enableMutation.mutateAsync()
                      }}
                      className="rounded-md border border-emerald-700 px-3 py-2 text-sm text-emerald-200 hover:bg-emerald-950/40 disabled:opacity-50"
                    >
                      Re-enable login
                    </button>
                  ) : null}

                  {summary.hasPlatformIdentity && summary.hasPlatformLogin && summary.accountState !== 'login_disabled' ? (
                    <button
                      type="button"
                      disabled={isBusy}
                      onClick={() => {
                        setPendingConfirmAction('disable-login')
                      }}
                      className="rounded-md border border-amber-700 px-3 py-2 text-sm text-amber-200 hover:bg-amber-950/40 disabled:opacity-50"
                    >
                      Disable login
                    </button>
                  ) : null}
                </div>

                <p className="text-xs text-slate-400">
                  Product launch eligibility comes from NexArr. Product roles and scopes remain visible in the Permissions tab.
                </p>
              </div>
            ) : null}
          </div>

          {successMessage ? (
            <p className="rounded-lg border border-emerald-800 bg-emerald-950/30 px-4 py-3 text-sm text-emerald-200">
              {successMessage}
            </p>
          ) : null}

          {mutationError ? (
            <ApiErrorCallout
              title="Account action failed"
              message={getErrorMessage(mutationError, 'The account action could not be completed.')}
            />
          ) : null}
        </div>
      ) : null}

      <ConfirmDialog
        open={confirmResetMfa || confirmDisableLogin}
        title={
          confirmResetMfa
            ? `Reset MFA for ${displayName}?`
            : `Disable login for ${displayName}?`
        }
        description={
          confirmResetMfa
            ? 'This will revoke active sessions and require the person to sign in again.'
            : 'This will block login until staff re-enables it.'
        }
        confirmLabel={confirmResetMfa ? 'Reset MFA' : 'Disable login'}
        danger
        loading={confirmLoading}
        onConfirm={() => {
          void confirmPendingAction()
        }}
        onCancel={() => setPendingConfirmAction(null)}
      />
    </section>
  )
}
