import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useAuth } from '../../auth/AuthProvider'
import * as nexarr from '../../api/nexarrClient'
import type { UserSessionSummary } from '../../api/types'
import { ConfirmDialog, useToast } from '../../feedback'

function formatDevice(session: UserSessionSummary): string {
  if (session.userAgent?.trim()) {
    return session.userAgent.length > 80
      ? `${session.userAgent.slice(0, 77)}…`
      : session.userAgent
  }
  return 'Unknown device'
}

function statusLabel(session: UserSessionSummary): string {
  if (session.isCurrent) {
    return session.isRemembered ? 'Current remembered session' : 'Current session'
  }
  if (session.revokedAt) {
    return 'Revoked'
  }
  if (!session.isActive) {
    return 'Expired'
  }
  return session.isRemembered ? 'Remembered session' : 'Active'
}

type PendingRevoke = {
  sessionId: string
  isCurrent: boolean
}

export function SessionManagementPanel() {
  const { logout, session: currentSession } = useAuth()
  const queryClient = useQueryClient()
  const { pushToast } = useToast()
  const [pendingRevoke, setPendingRevoke] = useState<PendingRevoke | null>(null)

  const sessionsQuery = useQuery({
    queryKey: ['my-sessions'],
    queryFn: () => nexarr.getMySessions(),
  })

  const revokeMutation = useMutation({
    mutationFn: (sessionId: string) => nexarr.revokeMySession(sessionId),
    onSuccess: async (_data, sessionId) => {
      await queryClient.invalidateQueries({ queryKey: ['my-sessions'] })
      setPendingRevoke(null)

      if (currentSession?.sessionId === sessionId) {
        pushToast({ message: 'Signed out of this device.', variant: 'success' })
        await logout()
        return
      }

      pushToast({ message: 'Session revoked.', variant: 'success' })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message || 'Could not revoke session.', variant: 'error' })
    },
  })

  if (sessionsQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading sessions…</p>
  }

  if (sessionsQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(sessionsQuery.error, 'Failed to load sessions.')}
        onRetry={() => void sessionsQuery.refetch()}
        retryLabel="Retry sessions"
      />
    )
  }

  const sessions = sessionsQuery.data?.sessions ?? []

  const confirmTitle = pendingRevoke?.isCurrent ? 'Sign out this device?' : 'Revoke session?'
  const confirmDescription = pendingRevoke?.isCurrent
    ? 'You will be signed out of NexArr on this device and need to sign in again.'
    : 'This device will lose access immediately. The user must sign in again on that device.'

  return (
    <div className="max-w-3xl space-y-4">
      <ConfirmDialog
        open={pendingRevoke !== null}
        title={confirmTitle}
        description={confirmDescription}
        confirmLabel={pendingRevoke?.isCurrent ? 'Sign out' : 'Revoke session'}
        danger
        loading={revokeMutation.isPending}
        onCancel={() => {
          if (!revokeMutation.isPending) {
            setPendingRevoke(null)
          }
        }}
        onConfirm={() => {
          if (pendingRevoke) {
            revokeMutation.mutate(pendingRevoke.sessionId)
          }
        }}
      />

      <div>
        <h3 className="text-xl font-semibold text-white">Active sessions</h3>
        <p className="mt-1 text-sm text-slate-400">
          Devices signed in to NexArr with your account. Revoke any session you do not recognize.
        </p>
      </div>

      {sessions.length === 0 ? (
        <p className="text-sm text-slate-400">No sessions found.</p>
      ) : (
        <ul className="divide-y divide-slate-700 rounded-lg border border-slate-700 bg-slate-900/60">
          {sessions.map((session) => (
            <li key={session.sessionId} className="flex flex-col gap-2 px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
              <div className="min-w-0 space-y-1 text-sm">
                <p className="font-medium text-white">{formatDevice(session)}</p>
                <p className="text-xs text-slate-400">
                  Signed in {new Date(session.createdAt).toLocaleString()}
                  {session.ipAddress ? ` · ${session.ipAddress}` : ''}
                </p>
                <p className="text-xs text-[var(--color-text-muted)]">
                  {statusLabel(session)}
                  {!session.revokedAt && session.isActive
                    ? ` · refresh expires ${new Date(session.expiresAt).toLocaleString()}`
                    : ''}
                </p>
              </div>
              {session.isActive && (
                <button
                  type="button"
                  disabled={revokeMutation.isPending}
                  onClick={() =>
                    setPendingRevoke({
                      sessionId: session.sessionId,
                      isCurrent: session.isCurrent,
                    })
                  }
                  className="shrink-0 rounded-md border border-slate-600 px-3 py-1.5 text-xs font-medium text-slate-200 hover:bg-slate-800 disabled:opacity-60"
                >
                  {session.isCurrent ? 'Sign out this device' : 'Revoke'}
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
