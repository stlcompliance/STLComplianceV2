import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  ApiErrorCallout,
  StaticSearchPicker,
  formatRoleDisplayName,
  formatStatusLabel,
  getErrorMessage,
  type PickerOption,
} from '@stl/shared-ui'
import { useEffect, useMemo, useState, type FormEvent } from 'react'
import * as nexarr from '../../api/nexarrClient'
import {
  PlatformAdminKpiCard,
  PlatformAdminPageHeader,
  PlatformAdminScopeNote,
  PlatformAdminSection,
} from '../../components/platform-admin/PlatformAdminPageChrome'
import { isActiveTenantStatus } from '../../lib/tenantStatus'
import type {
  CreatePlatformUserRequest,
  AssignPlatformUserRoleRequest,
  AssignPlatformUserTenantMembershipRequest,
  InvitePlatformUserRequest,
  PlatformUserAccessHistoryItemResponse,
  PlatformUserDetailResponse,
  PlatformUserIdentityAuditHistoryItemResponse,
  PlatformUserExternalIdentityProviderMappingItemResponse,
  PlatformUserListItemResponse,
  PlatformUserMfaResponse,
  PlatformUserRoleItemResponse,
  PlatformUserSessionItemResponse,
  PlatformUserTenantMembershipItemResponse,
} from '../../api/types'
import { ConfirmDialog, useToast } from '../../feedback'

function formatDateTime(value: string | null | undefined): string {
  if (!value) return '—'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '—' : date.toLocaleString()
}

function formatStatus(user: PlatformUserListItemResponse | PlatformUserDetailResponse): string {
  if (!user.canLogin) return 'Cannot log in'
  if (user.status !== 'active') return formatStatusLabel(user.status)
  if (user.isMfaEnabled) return 'Login enabled, MFA enabled'
  return 'Login enabled'
}

function userStatusLabel(status: string | null | undefined): string {
  if (!status) return 'Platform user'
  const normalized = status.trim().toLowerCase()
  if (normalized === 'active') return 'Enabled'
  if (normalized === 'suspended') return 'Suspended'
  return formatStatusLabel(status)
}

function formatSessionStatus(session: PlatformUserSessionItemResponse): string {
  if (session.isCurrent) return 'Current session'
  if (session.revokedAt) return 'Revoked'
  if (!session.isActive) return 'Expired'
  return session.isRemembered ? 'Remembered session' : 'Live session'
}

export function PlatformUsersPage() {
  const queryClient = useQueryClient()
  const { pushToast } = useToast()
  const [searchInput, setSearchInput] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [page, setPage] = useState(1)
  const [selectedUserId, setSelectedUserId] = useState('')
  const [pendingSessionRevoke, setPendingSessionRevoke] = useState<{
    sessionId: string
    isCurrent: boolean
  } | null>(null)
  const [pendingUserAction, setPendingUserAction] = useState<
    | { kind: 'enable' | 'disable' | 'lock' | 'unlock' | 'reset-password' }
    | null
  >(null)
  const [newPassword, setNewPassword] = useState('')
  const [membershipTenantId, setMembershipTenantId] = useState('')
  const [membershipRoleKey, setMembershipRoleKey] = useState('tenant_user')
  const [roleKey, setRoleKey] = useState('platform_support')
  const [roleTenantId, setRoleTenantId] = useState('')
  const [pendingMembershipRemoval, setPendingMembershipRemoval] = useState<PlatformUserTenantMembershipItemResponse | null>(null)
  const [pendingRoleRemoval, setPendingRoleRemoval] = useState<PlatformUserRoleItemResponse | null>(null)
  const [mfaSetupResult, setMfaSetupResult] = useState<PlatformUserMfaResponse | null>(null)
  const [createMode, setCreateMode] = useState<'create' | 'invite'>('invite')
  const [createEmail, setCreateEmail] = useState('')
  const [createDisplayName, setCreateDisplayName] = useState('')
  const [createPassword, setCreatePassword] = useState('')
  const [createIsPlatformAdmin, setCreateIsPlatformAdmin] = useState(false)
  const [createIsActive, setCreateIsActive] = useState(true)
  const [createRequireEmailVerification, setCreateRequireEmailVerification] = useState(false)
  const [externalProviderKey, setExternalProviderKey] = useState('')
  const [externalSubject, setExternalSubject] = useState('')
  const [externalEmail, setExternalEmail] = useState('')
  const [pendingExternalMappingRemoval, setPendingExternalMappingRemoval] = useState<PlatformUserExternalIdentityProviderMappingItemResponse | null>(null)

  const usersQuery = useQuery({
    queryKey: ['platform-admin-users', searchTerm, page],
    queryFn: () => nexarr.listPlatformUsers(searchTerm, page, 20),
  })

  const selectedUserIdResolved = selectedUserId || usersQuery.data?.items[0]?.userId || ''

  const userDetailQuery = useQuery({
    queryKey: ['platform-admin-user', selectedUserIdResolved],
    queryFn: () => nexarr.getPlatformUser(selectedUserIdResolved),
    enabled: Boolean(selectedUserIdResolved),
  })

  const tenantOptionsQuery = useQuery({
    queryKey: ['platform-admin-user-tenant-options'],
    queryFn: () => nexarr.listTenants(1, 200),
  })

  const userTenantMembershipsQuery = useQuery({
    queryKey: ['platform-admin-user-tenant-memberships', selectedUserIdResolved],
    queryFn: () => nexarr.getPlatformUserTenantMemberships(selectedUserIdResolved),
    enabled: Boolean(selectedUserIdResolved),
  })

  const userRolesQuery = useQuery({
    queryKey: ['platform-admin-user-roles', selectedUserIdResolved],
    queryFn: () => nexarr.getPlatformUserRoles(selectedUserIdResolved),
    enabled: Boolean(selectedUserIdResolved),
  })

  const userExternalIdentityMappingsQuery = useQuery({
    queryKey: ['platform-admin-user-external-identity-mappings', selectedUserIdResolved],
    queryFn: () => nexarr.getPlatformUserExternalIdentityMappings(selectedUserIdResolved),
    enabled: Boolean(selectedUserIdResolved),
  })

  const userSessionsQuery = useQuery({
    queryKey: ['platform-admin-user-sessions', selectedUserIdResolved],
    queryFn: () => nexarr.getPlatformUserSessions(selectedUserIdResolved),
    enabled: Boolean(selectedUserIdResolved),
  })

  const loginHistoryQuery = useQuery({
    queryKey: ['platform-admin-user-login-history', selectedUserIdResolved],
    queryFn: () => nexarr.getPlatformUserLoginHistory(selectedUserIdResolved),
    enabled: Boolean(selectedUserIdResolved),
  })

  const launchHistoryQuery = useQuery({
    queryKey: ['platform-admin-user-launch-history', selectedUserIdResolved],
    queryFn: () => nexarr.getPlatformUserLaunchHistory(selectedUserIdResolved),
    enabled: Boolean(selectedUserIdResolved),
  })

  const identityAuditHistoryQuery = useQuery({
    queryKey: ['platform-admin-user-identity-audit-history', selectedUserIdResolved],
    queryFn: () => nexarr.getPlatformUserIdentityAuditHistory(selectedUserIdResolved),
    enabled: Boolean(selectedUserIdResolved),
  })

  const revokeSessionMutation = useMutation({
    mutationFn: ({ userId, sessionId }: { userId: string; sessionId: string }) =>
      nexarr.revokePlatformUserSession(userId, sessionId),
    onSuccess: async () => {
      setPendingSessionRevoke(null)
      pushToast({ message: 'Session revoked.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user-sessions', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message || 'Unable to revoke the session.', variant: 'error' })
    },
  })

  const enableMutation = useMutation({
    mutationFn: () => nexarr.enablePlatformUser(selectedUserIdResolved),
    onSuccess: async () => {
      setPendingUserAction(null)
      pushToast({ message: 'User enabled.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
    },
    onError: (error: Error) => pushToast({ message: error.message || 'Unable to enable the user.', variant: 'error' }),
  })

  const disableMutation = useMutation({
    mutationFn: () => nexarr.disablePlatformUser(selectedUserIdResolved),
    onSuccess: async () => {
      setPendingUserAction(null)
      pushToast({ message: 'User disabled.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
    },
    onError: (error: Error) => pushToast({ message: error.message || 'Unable to disable the user.', variant: 'error' }),
  })

  const lockMutation = useMutation({
    mutationFn: () => nexarr.lockPlatformUser(selectedUserIdResolved),
    onSuccess: async () => {
      setPendingUserAction(null)
      pushToast({ message: 'User locked.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
    },
    onError: (error: Error) => pushToast({ message: error.message || 'Unable to lock the user.', variant: 'error' }),
  })

  const unlockMutation = useMutation({
    mutationFn: () => nexarr.unlockPlatformUser(selectedUserIdResolved),
    onSuccess: async () => {
      setPendingUserAction(null)
      pushToast({ message: 'User unlocked.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
    },
    onError: (error: Error) => pushToast({ message: error.message || 'Unable to unlock the user.', variant: 'error' }),
  })

  const resetPasswordMutation = useMutation({
    mutationFn: () => nexarr.resetPlatformUserPassword(selectedUserIdResolved, newPassword),
    onSuccess: async () => {
      setPendingUserAction(null)
      setNewPassword('')
      pushToast({ message: 'Password reset.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user-sessions', selectedUserIdResolved] })
    },
    onError: (error: Error) => pushToast({ message: error.message || 'Unable to reset the password.', variant: 'error' }),
  })

  const mfaMutation = useMutation({
    mutationFn: (isEnabled: boolean) => nexarr.setPlatformUserMfa(selectedUserIdResolved, isEnabled),
    onSuccess: async (result) => {
      setMfaSetupResult(result.isMfaEnabled && result.mfaSecret ? result : null)
      pushToast({
        message: result.wasAlreadySet
          ? 'MFA state unchanged.'
          : result.recoveryCodes?.length
            ? 'MFA setting updated. Recovery codes generated.'
            : 'MFA setting updated.',
        variant: 'success',
      })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message || 'Unable to update MFA.', variant: 'error' })
    },
  })

  const assignMembershipMutation = useMutation({
    mutationFn: (request: AssignPlatformUserTenantMembershipRequest) =>
      nexarr.assignPlatformUserTenantMembership(selectedUserIdResolved, request),
    onSuccess: async () => {
      setMembershipTenantId('')
      setMembershipRoleKey('tenant_user')
      pushToast({ message: 'Tenant membership updated.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user-tenant-memberships', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message || 'Unable to update tenant membership.', variant: 'error' })
    },
  })

  const removeMembershipMutation = useMutation({
    mutationFn: (tenantId: string) => nexarr.removePlatformUserTenantMembership(selectedUserIdResolved, tenantId),
    onSuccess: async () => {
      setPendingMembershipRemoval(null)
      pushToast({ message: 'Tenant membership removed.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user-tenant-memberships', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message || 'Unable to remove the tenant membership.', variant: 'error' })
    },
  })

  const assignRoleMutation = useMutation({
    mutationFn: (request: AssignPlatformUserRoleRequest) =>
      nexarr.assignPlatformUserRole(selectedUserIdResolved, request),
    onSuccess: async () => {
      setRoleKey('platform_support')
      setRoleTenantId('')
      pushToast({ message: 'Platform role updated.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user-roles', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message || 'Unable to update the platform role.', variant: 'error' })
    },
  })

  const createUserMutation = useMutation({
    mutationFn: (request: CreatePlatformUserRequest | InvitePlatformUserRequest) =>
      createMode === 'create'
        ? nexarr.createPlatformUser(request as CreatePlatformUserRequest)
        : nexarr.invitePlatformUser(request as InvitePlatformUserRequest),
    onSuccess: async (created) => {
      setCreateEmail('')
      setCreateDisplayName('')
      setCreatePassword('')
      setCreateIsPlatformAdmin(false)
      setCreateIsActive(true)
      setCreateRequireEmailVerification(false)
      setCreateMode('invite')
      setSelectedUserId(created.userId)
      pushToast({
        message: createMode === 'create' ? 'Platform user created.' : 'Platform user invited.',
        variant: 'success',
      })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', created.userId] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user-sessions', created.userId] })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message || 'Unable to create or invite the user.', variant: 'error' })
    },
  })

  const removeRoleMutation = useMutation({
    mutationFn: (payload: { roleKey: string; tenantId: string | null }) =>
      nexarr.removePlatformUserRole(selectedUserIdResolved, payload.roleKey, payload.tenantId),
    onSuccess: async () => {
      setPendingRoleRemoval(null)
      pushToast({ message: 'Platform role removed.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user-roles', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-users'] })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message || 'Unable to remove the platform role.', variant: 'error' })
    },
  })

  const upsertExternalIdentityMutation = useMutation({
    mutationFn: (request: {
      providerKey: string
      externalSubject: string
      externalEmail?: string | null
    }) => nexarr.upsertPlatformUserExternalIdentityMapping(selectedUserIdResolved, request),
    onSuccess: async () => {
      setExternalProviderKey('')
      setExternalSubject('')
      setExternalEmail('')
      pushToast({ message: 'External identity mapping saved.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user-external-identity-mappings', selectedUserIdResolved] })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message || 'Unable to save the external identity mapping.', variant: 'error' })
    },
  })

  const removeExternalIdentityMutation = useMutation({
    mutationFn: (mappingId: string) =>
      nexarr.removePlatformUserExternalIdentityMapping(selectedUserIdResolved, mappingId),
    onSuccess: async () => {
      setPendingExternalMappingRemoval(null)
      pushToast({ message: 'External identity mapping removed.', variant: 'success' })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user', selectedUserIdResolved] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-user-external-identity-mappings', selectedUserIdResolved] })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message || 'Unable to remove the external identity mapping.', variant: 'error' })
    },
  })

  useEffect(() => {
    setPendingMembershipRemoval(null)
    setPendingRoleRemoval(null)
    setMembershipTenantId('')
    setMembershipRoleKey('tenant_user')
    setRoleKey('platform_support')
    setRoleTenantId('')
    setMfaSetupResult(null)
    setExternalProviderKey('')
    setExternalSubject('')
    setExternalEmail('')
    setPendingExternalMappingRemoval(null)
  }, [selectedUserIdResolved])

  useEffect(() => {
    if (!membershipTenantId && tenantOptionsQuery.data?.items?.length) {
      setMembershipTenantId(tenantOptionsQuery.data.items[0].tenantId)
    }
  }, [membershipTenantId, tenantOptionsQuery.data?.items])

  const users = usersQuery.data?.items ?? []
  const selectedUser = userDetailQuery.data ?? null
  const sessions = userSessionsQuery.data?.sessions ?? []
  const tenants = tenantOptionsQuery.data?.items ?? []
  const memberships = userTenantMembershipsQuery.data?.items ?? []
  const roles = userRolesQuery.data?.items ?? []
  const externalMappings = userExternalIdentityMappingsQuery.data?.items ?? []
  const tenantPickerOptions = useMemo<PickerOption[]>(
    () =>
      tenants.map((tenant) => ({
        value: tenant.tenantId,
        label: tenant.displayName,
        inactive: !isActiveTenantStatus(tenant.status),
      })),
    [tenants],
  )

  const totalPages = useMemo(() => {
    const totalCount = usersQuery.data?.totalCount ?? 0
    return Math.max(1, Math.ceil(totalCount / (usersQuery.data?.pageSize ?? 20)))
  }, [usersQuery.data?.pageSize, usersQuery.data?.totalCount])

  const handleSearchSubmit = (event: FormEvent) => {
    event.preventDefault()
    setPage(1)
    setSearchTerm(searchInput)
  }

  if (usersQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading users…</p>
  }

  if (usersQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(usersQuery.error, 'Failed to load users.')}
        onRetry={() => void usersQuery.refetch()}
        retryLabel="Retry users"
      />
    )
  }

  return (
    <div className="space-y-6" data-testid="platform-users-page">
      <ConfirmDialog
        open={pendingSessionRevoke !== null}
        title={pendingSessionRevoke?.isCurrent ? 'Sign out this session?' : 'Revoke session?'}
        description={
          pendingSessionRevoke?.isCurrent
            ? 'This will end the current session and require the user to sign in again.'
            : 'The selected device will be signed out immediately.'
        }
        confirmLabel={pendingSessionRevoke?.isCurrent ? 'Sign out' : 'Revoke session'}
        danger
        loading={revokeSessionMutation.isPending}
        onCancel={() => {
          if (!revokeSessionMutation.isPending) {
            setPendingSessionRevoke(null)
          }
        }}
        onConfirm={() => {
          if (pendingSessionRevoke && selectedUserIdResolved) {
            revokeSessionMutation.mutate({
              userId: selectedUserIdResolved,
              sessionId: pendingSessionRevoke.sessionId,
            })
          }
        }}
      />
      <ConfirmDialog
        open={pendingUserAction !== null}
        title={
          pendingUserAction?.kind === 'enable'
            ? 'Enable user?'
            : pendingUserAction?.kind === 'disable'
              ? 'Disable user?'
              : pendingUserAction?.kind === 'lock'
                ? 'Lock user?'
                : pendingUserAction?.kind === 'unlock'
                  ? 'Unlock user?'
                  : 'Reset password?'
        }
        description={
          pendingUserAction?.kind === 'enable'
            ? 'The user will be allowed to log in again.'
            : pendingUserAction?.kind === 'disable'
              ? 'The account will be disabled and active login sessions will be locked out.'
              : pendingUserAction?.kind === 'lock'
                ? 'The user will be temporarily locked out until manually unlocked.'
                : pendingUserAction?.kind === 'unlock'
                  ? 'The lock will be cleared and failed login counters reset.'
                  : 'The password will be replaced immediately and active sessions will be revoked.'
        }
        confirmLabel={pendingUserAction?.kind === 'reset-password' ? 'Reset password' : 'Confirm'}
        danger={pendingUserAction?.kind === 'disable' || pendingUserAction?.kind === 'lock'}
        loading={
          enableMutation.isPending ||
          disableMutation.isPending ||
          lockMutation.isPending ||
          unlockMutation.isPending ||
          resetPasswordMutation.isPending
        }
        onCancel={() => {
          if (
            !enableMutation.isPending &&
            !disableMutation.isPending &&
            !lockMutation.isPending &&
            !unlockMutation.isPending &&
            !resetPasswordMutation.isPending
          ) {
            setPendingUserAction(null)
          }
        }}
        onConfirm={() => {
          if (!pendingUserAction) return
          switch (pendingUserAction.kind) {
            case 'enable':
              enableMutation.mutate()
              break
            case 'disable':
              disableMutation.mutate()
              break
            case 'lock':
              lockMutation.mutate()
              break
            case 'unlock':
              unlockMutation.mutate()
              break
            case 'reset-password':
              resetPasswordMutation.mutate()
              break
          }
        }}
      />
      <ConfirmDialog
        open={pendingMembershipRemoval !== null}
        title="Remove tenant membership?"
        description={
          pendingMembershipRemoval
            ? `${pendingMembershipRemoval.tenantDisplayName} will lose the selected membership.`
            : 'Remove this tenant membership from the selected user.'
        }
        confirmLabel="Remove membership"
        danger
        loading={removeMembershipMutation.isPending}
        onCancel={() => {
          if (!removeMembershipMutation.isPending) {
            setPendingMembershipRemoval(null)
          }
        }}
        onConfirm={() => {
          if (pendingMembershipRemoval) {
            removeMembershipMutation.mutate(pendingMembershipRemoval.tenantId)
          }
        }}
      />
      <ConfirmDialog
        open={pendingRoleRemoval !== null}
        title="Remove platform role?"
        description={
          pendingRoleRemoval
            ? `${formatRoleDisplayName(pendingRoleRemoval.roleKey)}${pendingRoleRemoval.tenantId ? ' for the selected tenant' : ''} will be removed from the selected user.`
            : 'Remove this platform role from the selected user.'
        }
        confirmLabel="Remove role"
        danger
        loading={removeRoleMutation.isPending}
        onCancel={() => {
          if (!removeRoleMutation.isPending) {
            setPendingRoleRemoval(null)
          }
        }}
        onConfirm={() => {
          if (pendingRoleRemoval) {
            removeRoleMutation.mutate({
              roleKey: pendingRoleRemoval.roleKey,
              tenantId: pendingRoleRemoval.tenantId,
            })
          }
        }}
      />
      <ConfirmDialog
        open={pendingExternalMappingRemoval !== null}
        title="Remove external identity mapping?"
        description={
          pendingExternalMappingRemoval
            ? `This external identity mapping will be removed from the selected user.`
            : 'Remove this external identity mapping from the selected user.'
        }
        confirmLabel="Remove mapping"
        danger
        loading={removeExternalIdentityMutation.isPending}
        onCancel={() => {
          if (!removeExternalIdentityMutation.isPending) {
            setPendingExternalMappingRemoval(null)
          }
        }}
        onConfirm={() => {
          if (pendingExternalMappingRemoval) {
            removeExternalIdentityMutation.mutate(pendingExternalMappingRemoval.mappingId)
          }
        }}
      />

      <PlatformAdminPageHeader
        title="User administration"
        summary="NexArr platform account record, tenant membership, MFA, session control, and identity audit history."
        badge={userStatusLabel(selectedUser?.status)}
        updatedAt={selectedUser ? formatDateTime(selectedUser.modifiedAt ?? selectedUser.createdAt) : undefined}
      />

      {selectedUser ? (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <PlatformAdminKpiCard
            label="Account state"
            value={formatStatus(selectedUser)}
            hint="Login capability and MFA state for this platform identity."
            tone={selectedUser.canLogin ? 'good' : 'bad'}
          />
          <PlatformAdminKpiCard
            label="Tenant memberships"
            value={memberships.length}
            hint="Tenant memberships are validated after NexArr session checks."
            tone={memberships.length > 0 ? 'good' : 'warn'}
          />
          <PlatformAdminKpiCard
            label="Roles"
            value={roles.length}
            hint="Platform roles and scoped authority assignments."
            tone={roles.length > 0 ? 'info' : 'neutral'}
          />
          <PlatformAdminKpiCard
            label="Session status"
            value={sessions.filter((session) => session.isActive).length}
            hint="Sessions currently signed in to NexArr."
            tone={sessions.some((session) => session.isActive) ? 'warn' : 'good'}
          />
        </div>
      ) : null}

      <header className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold text-[var(--color-text-primary)]">User administration</h2>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            Search platform users, inspect account state, toggle MFA, and revoke live sessions.
          </p>
        </div>
        <form className="flex gap-2" onSubmit={handleSearchSubmit}>
          <input
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            placeholder="Search email or name"
            className="min-w-64 rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
          />
          <button
            type="submit"
            className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)]"
          >
            Search
          </button>
        </form>
      </header>

      {selectedUser ? (
        <PlatformAdminSection
          title="Decision summary"
          description="NexArr identity and session posture for the selected platform user."
        >
          <div className="grid gap-4 md:grid-cols-2">
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Current state</p>
              <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">{formatStatus(selectedUser)}</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                {selectedUser.canLogin
                  ? 'Login status is enabled and this record can reach permitted product surfaces.'
                  : 'Login status is disabled until NexArr identity is restored.'}
              </p>
            </div>
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Source of truth</p>
              <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">NexArr user and session records</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Platform identity, tenant membership, and session history remain canonical in NexArr. Product-local permissions are projected downstream.
              </p>
            </div>
          </div>
        </PlatformAdminSection>
      ) : null}

      <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h3 className="text-lg font-semibold text-[var(--color-text-primary)]">Create or invite user</h3>
            <p className="mt-1 text-sm text-[var(--color-text-muted)]">
              Add a new platform account directly or invite someone without credentials yet.
            </p>
          </div>
          <label className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
            Mode
            <select
              value={createMode}
              onChange={(event) => setCreateMode(event.target.value as 'create' | 'invite')}
              className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            >
              <option value="invite">Invite</option>
              <option value="create">Create</option>
            </select>
          </label>
        </div>

        <div className="mt-4 grid gap-3 md:grid-cols-2">
          <label className="block text-sm text-[var(--color-text-secondary)]">
            Email
            <input
              value={createEmail}
              onChange={(event) => setCreateEmail(event.target.value)}
              type="email"
              className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label className="block text-sm text-[var(--color-text-secondary)]">
            Display name
            <input
              value={createDisplayName}
              onChange={(event) => setCreateDisplayName(event.target.value)}
              className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          {createMode === 'create' ? (
            <label className="block text-sm text-[var(--color-text-secondary)]">
              Temporary password
              <input
                value={createPassword}
                onChange={(event) => setCreatePassword(event.target.value)}
                type="password"
                className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
              />
            </label>
          ) : null}
          <div className="grid gap-2 rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3 text-sm text-[var(--color-text-secondary)] md:col-span-2 md:grid-cols-2">
            <label className="flex items-center gap-2">
              <input
                checked={createIsActive}
                onChange={(event) => setCreateIsActive(event.target.checked)}
                type="checkbox"
              />
              Enabled account
            </label>
            <label className="flex items-center gap-2">
              <input
                checked={createIsPlatformAdmin}
                onChange={(event) => setCreateIsPlatformAdmin(event.target.checked)}
                type="checkbox"
              />
              Platform admin
            </label>
            {createMode === 'create' ? (
              <label className="flex items-center gap-2">
                <input
                  checked={createRequireEmailVerification}
                  onChange={(event) => setCreateRequireEmailVerification(event.target.checked)}
                  type="checkbox"
                />
                Require email verification
              </label>
            ) : null}
          </div>
        </div>

        <div className="mt-4 flex items-center justify-end gap-2">
          <button
            type="button"
            disabled={
              !createEmail.trim() ||
              !createDisplayName.trim() ||
              (createMode === 'create' && createPassword.trim().length < 8) ||
              createUserMutation.isPending
            }
            onClick={() => {
              if (createMode === 'create') {
                createUserMutation.mutate({
                  email: createEmail,
                  displayName: createDisplayName,
                  password: createPassword,
                  isPlatformAdmin: createIsPlatformAdmin,
                  isActive: createIsActive,
                  requireEmailVerification: createRequireEmailVerification,
                })
              } else {
                createUserMutation.mutate({
                  email: createEmail,
                  displayName: createDisplayName,
                  isPlatformAdmin: createIsPlatformAdmin,
                  isActive: createIsActive,
                })
              }
            }}
            className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
          >
            {createMode === 'create' ? 'Create user' : 'Invite user'}
          </button>
        </div>
      </section>

      <div className="grid gap-6">
        <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
          <div className="flex items-center justify-between gap-3">
            <h3 className="text-lg font-semibold text-[var(--color-text-primary)]">Users</h3>
            <p className="text-xs text-[var(--color-text-muted)]">
              Page {usersQuery.data?.page ?? 1} of {totalPages}
            </p>
          </div>

          <div className="mt-4 overflow-x-auto rounded-lg border border-[var(--color-border-subtle)]">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-3 py-2">User</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2">Last login</th>
                  <th className="px-3 py-2">MFA</th>
                </tr>
              </thead>
              <tbody>
                {users.length === 0 ? (
                  <tr>
                    <td colSpan={4} className="px-3 py-4 text-[var(--color-text-muted)]">
                      No users matched the current search.
                    </td>
                  </tr>
                ) : (
                  users.map((user) => {
                    const isSelected = user.userId === selectedUserIdResolved
                    return (
                      <tr
                        key={user.userId}
                        className={`cursor-pointer border-t border-[var(--color-border-subtle)] ${isSelected ? 'bg-[var(--color-warning-bg)]/40' : ''}`}
                        onClick={() => setSelectedUserId(user.userId)}
                      >
                        <td className="px-3 py-2">
                          <div className="font-medium text-[var(--color-text-primary)]">{user.displayName}</div>
                          <div className="text-xs text-[var(--color-text-muted)]">{user.email}</div>
                        </td>
                        <td className="px-3 py-2 text-[var(--color-text-secondary)]">{formatStatus(user)}</td>
                        <td className="px-3 py-2 text-[var(--color-text-secondary)]">{formatDateTime(user.lastLoginAt)}</td>
                        <td className="px-3 py-2 text-[var(--color-text-secondary)]">{user.isMfaEnabled ? 'Enabled' : 'Disabled'}</td>
                      </tr>
                    )
                  })
                )}
              </tbody>
            </table>
          </div>

          <div className="mt-4 flex items-center justify-between gap-3">
              <button
                type="button"
                disabled={page <= 1}
                onClick={() => setPage((current) => Math.max(1, current - 1))}
                className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-1.5 text-sm text-[var(--color-text-primary)] disabled:opacity-50"
              >
                Previous
              </button>
              <button
                type="button"
                disabled={!usersQuery.data?.hasNextPage}
                onClick={() => setPage((current) => current + 1)}
                className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-1.5 text-sm text-[var(--color-text-primary)] disabled:opacity-50"
              >
                Next
              </button>
          </div>
        </section>

        <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
          {!selectedUserIdResolved ? (
            <p className="text-sm text-[var(--color-text-muted)]">Select a user to view account details.</p>
          ) : userDetailQuery.isLoading ? (
            <p className="text-sm text-[var(--color-text-muted)]">Loading user details…</p>
          ) : userDetailQuery.isError ? (
            <ApiErrorCallout
              message={getErrorMessage(userDetailQuery.error, 'Failed to load user details.')}
              onRetry={() => void userDetailQuery.refetch()}
              retryLabel="Retry user"
            />
          ) : selectedUser ? (
            <div className="space-y-5">
              <div>
                <h3 className="text-xl font-semibold text-[var(--color-text-primary)]">{selectedUser.displayName}</h3>
                <p className="mt-1 text-sm text-[var(--color-text-muted)]">{selectedUser.email}</p>
                <div className="mt-3 flex flex-wrap gap-2 text-xs">
                  <span className="rounded-full border border-[var(--color-border-subtle)] px-2 py-1 text-[var(--color-text-secondary)]">
                    {formatStatus(selectedUser)}
                  </span>
                  <span className="rounded-full border border-[var(--color-border-subtle)] px-2 py-1 text-[var(--color-text-secondary)]">
                    {selectedUser.isPlatformAdmin ? 'Platform admin' : 'Standard user'}
                  </span>
                  <span className="rounded-full border border-[var(--color-border-subtle)] px-2 py-1 text-[var(--color-text-secondary)]">
                    Failed logins: {selectedUser.failedLoginCount}
                  </span>
                </div>
              </div>

              <dl className="grid gap-3 text-sm sm:grid-cols-2">
                <InfoRow label="Created" value={formatDateTime(selectedUser.createdAt)} />
                <InfoRow label="Modified" value={formatDateTime(selectedUser.modifiedAt)} />
                <InfoRow label="Last login" value={formatDateTime(selectedUser.lastLoginAt)} />
                <InfoRow label="Last launch" value={formatDateTime(selectedUser.lastProductLaunchAt)} />
                <InfoRow label="Locked until" value={formatDateTime(selectedUser.lockedUntil)} />
                <InfoRow label="Login status" value={selectedUser.canLogin ? 'Enabled' : 'Disabled'} />
              </dl>

              <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <h4 className="font-semibold text-[var(--color-text-primary)]">MFA</h4>
                    <p className="text-xs text-[var(--color-text-muted)]">
                      {selectedUser.isMfaEnabled
                        ? 'MFA is enabled for this account.'
                        : 'MFA is disabled for this account.'}
                    </p>
                  </div>
                  <button
                    type="button"
                    disabled={mfaMutation.isPending}
                    onClick={() => mfaMutation.mutate(!selectedUser.isMfaEnabled)}
                    className="rounded-md bg-[var(--color-accent)] px-3 py-1.5 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                  >
                    {selectedUser.isMfaEnabled ? 'Disable MFA' : 'Enable MFA'}
                  </button>
                </div>
                {mfaSetupResult?.mfaSecret ? (
                  <div className="mt-4 space-y-4 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
                    <div className="grid gap-3 md:grid-cols-2">
                      <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3">
                        <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Secret</p>
                        <p className="mt-2 break-all font-mono text-sm text-[var(--color-text-primary)]">
                          {mfaSetupResult.mfaSecret}
                        </p>
                      </div>
                      <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3">
                        <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Provisioning URI</p>
                        <p className="mt-2 break-all font-mono text-xs text-[var(--color-text-primary)]">
                          {mfaSetupResult.provisioningUri ?? '—'}
                        </p>
                      </div>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Recovery codes</p>
                      {mfaSetupResult.recoveryCodes?.length ? (
                        <ul className="mt-2 grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
                          {mfaSetupResult.recoveryCodes.map((code) => (
                            <li
                              key={code}
                              className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 font-mono text-sm text-[var(--color-text-primary)]"
                            >
                              {code}
                            </li>
                          ))}
                        </ul>
                      ) : (
                        <p className="mt-2 text-sm text-[var(--color-text-muted)]">
                          Recovery codes are only returned when MFA is freshly enabled.
                        </p>
                      )}
                    </div>
                  </div>
                ) : null}
              </div>

              <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <h4 className="font-semibold text-[var(--color-text-primary)]">Account controls</h4>
                    <p className="text-xs text-[var(--color-text-muted)]">
                      Enable, disable, lock, unlock, or reset this platform account.
                    </p>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {selectedUser.isActive ? (
                      <button
                        type="button"
                        disabled={disableMutation.isPending}
                        onClick={() => setPendingUserAction({ kind: 'disable' })}
                        className="rounded-md border border-[var(--color-destructive-border)] px-3 py-1.5 text-sm font-medium text-[var(--color-destructive-text)] hover:bg-[var(--color-destructive-bg)] disabled:opacity-50"
                      >
                        Disable user
                      </button>
                    ) : (
                      <button
                        type="button"
                        disabled={enableMutation.isPending}
                        onClick={() => setPendingUserAction({ kind: 'enable' })}
                        className="rounded-md border border-[var(--color-success-border)] px-3 py-1.5 text-sm font-medium text-[var(--color-success-text)] hover:bg-[var(--color-success-bg)] disabled:opacity-50"
                      >
                        Enable user
                      </button>
                    )}
                    {selectedUser.lockedUntil && new Date(selectedUser.lockedUntil).getTime() > Date.now() ? (
                      <button
                        type="button"
                        disabled={unlockMutation.isPending}
                        onClick={() => setPendingUserAction({ kind: 'unlock' })}
                        className="rounded-md border border-[var(--color-success-border)] px-3 py-1.5 text-sm font-medium text-[var(--color-success-text)] hover:bg-[var(--color-success-bg)] disabled:opacity-50"
                      >
                        Unlock user
                      </button>
                    ) : (
                      <button
                        type="button"
                        disabled={lockMutation.isPending}
                        onClick={() => setPendingUserAction({ kind: 'lock' })}
                        className="rounded-md border border-[var(--color-warning-border)] px-3 py-1.5 text-sm font-medium text-[var(--color-warning-text)] hover:bg-[var(--color-warning-bg)] disabled:opacity-50"
                      >
                        Lock user
                      </button>
                    )}
                  </div>
                </div>
                <div className="mt-4 grid gap-3 sm:grid-cols-[1fr_auto]">
                  <label className="block text-sm text-[var(--color-text-secondary)]">
                    New password
                    <input
                      value={newPassword}
                      onChange={(event) => setNewPassword(event.target.value)}
                      type="password"
                      placeholder="Enter a new password"
                      className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                    />
                  </label>
                  <div className="flex items-end">
                    <button
                      type="button"
                      disabled={newPassword.trim().length < 8 || resetPasswordMutation.isPending}
                      onClick={() => setPendingUserAction({ kind: 'reset-password' })}
                      className="rounded-md bg-[var(--color-accent)] px-3 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                    >
                      Reset password
                    </button>
                  </div>
                </div>
              </div>

              <div className="grid gap-4 lg:grid-cols-2">
                <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
                  <div>
                    <h4 className="font-semibold text-[var(--color-text-primary)]">Tenant memberships</h4>
                    <p className="text-xs text-[var(--color-text-muted)]">
                      Assign or remove tenant membership for this platform account.
                    </p>
                  </div>
                  <div className="mt-4 grid gap-3 md:grid-cols-[1fr_1fr_auto]">
                    <StaticSearchPicker
                      label="Tenant"
                      id="platform-user-membership-tenant"
                      value={membershipTenantId}
                      onChange={setMembershipTenantId}
                      options={tenantPickerOptions}
                      placeholder="Search tenants"
                      testId="platform-user-membership-tenant-picker"
                    />
                    <label className="block text-sm text-[var(--color-text-secondary)]">
                      Tenant role
                      <select
                        value={membershipRoleKey}
                        onChange={(event) => setMembershipRoleKey(event.target.value)}
                        className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                      >
                        <option value="tenant_user">{formatRoleDisplayName('tenant_user')}</option>
                        <option value="tenant_admin">{formatRoleDisplayName('tenant_admin')}</option>
                      </select>
                    </label>
                    <div className="flex items-end">
                      <button
                        type="button"
                        disabled={!membershipTenantId || assignMembershipMutation.isPending}
                        onClick={() =>
                          assignMembershipMutation.mutate({
                            tenantId: membershipTenantId,
                            roleKey: membershipRoleKey,
                          })
                        }
                        className="rounded-md bg-[var(--color-accent)] px-3 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                      >
                        Assign membership
                      </button>
                    </div>
                  </div>
                  <div className="mt-4 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
                    {userTenantMembershipsQuery.isLoading ? (
                      <p className="px-3 py-4 text-sm text-[var(--color-text-muted)]">Loading memberships…</p>
                    ) : userTenantMembershipsQuery.isError ? (
                      <ApiErrorCallout
                        message={getErrorMessage(
                          userTenantMembershipsQuery.error,
                          'Failed to load tenant memberships.',
                        )}
                        onRetry={() => void userTenantMembershipsQuery.refetch()}
                        retryLabel="Retry memberships"
                      />
                    ) : memberships.length === 0 ? (
                      <p className="px-3 py-4 text-sm text-[var(--color-text-muted)]">No tenant memberships found.</p>
                    ) : (
                      <ul className="divide-y divide-[var(--color-border-subtle)]">
                        {memberships.map((membership) => (
                          <li
                            key={membership.tenantId}
                            className="flex flex-wrap items-center justify-between gap-3 px-3 py-3 text-sm"
                          >
                            <div>
                              <p className="font-medium text-[var(--color-text-primary)]">{membership.tenantDisplayName}</p>
                              <p className="text-xs text-[var(--color-text-muted)]">
                                {formatRoleDisplayName(membership.roleKey)}
                                {membership.isActive ? '' : ' · inactive'}
                              </p>
                              <p className="mt-1 text-xs text-[var(--color-text-muted)]">{formatDateTime(membership.createdAt)}</p>
                            </div>
                            <button
                              type="button"
                              disabled={removeMembershipMutation.isPending}
                              onClick={() => setPendingMembershipRemoval(membership)}
                              className="rounded-md border border-[var(--color-destructive-border)] px-3 py-1.5 text-xs font-medium text-[var(--color-destructive-text)] hover:bg-[var(--color-destructive-bg)] disabled:opacity-50"
                            >
                              Remove
                            </button>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                </section>

                <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
                  <div>
                    <h4 className="font-semibold text-[var(--color-text-primary)]">Platform roles</h4>
                    <p className="text-xs text-[var(--color-text-muted)]">
                      Assign or remove platform roles, including tenant-scoped roles.
                    </p>
                  </div>
                  <div className="mt-4 grid gap-3 md:grid-cols-[1.2fr_1fr_auto]">
                    <label className="block text-sm text-[var(--color-text-secondary)]">
                      Role
                      <select
                        value={roleKey}
                        onChange={(event) => setRoleKey(event.target.value)}
                        className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                      >
                        <option value="platform_support">{formatRoleDisplayName('platform_support')}</option>
                        <option value="platform_admin">{formatRoleDisplayName('platform_admin')}</option>
                        <option value="platform_owner">{formatRoleDisplayName('platform_owner')}</option>
                        <option value="tenant_admin">{formatRoleDisplayName('tenant_admin')}</option>
                        <option value="tenant_user">{formatRoleDisplayName('tenant_user')}</option>
                        <option value="service_client">{formatRoleDisplayName('service_client')}</option>
                        <option value="product_service">{formatRoleDisplayName('product_service')}</option>
                        <option value="read_only_auditor">{formatRoleDisplayName('read_only_auditor')}</option>
                      </select>
                    </label>
                    <StaticSearchPicker
                      label="Tenant scope"
                      id="platform-user-role-tenant"
                      value={roleTenantId}
                      onChange={setRoleTenantId}
                      options={tenantPickerOptions}
                      placeholder="Search tenants"
                      testId="platform-user-role-tenant-picker"
                    />
                    <div className="flex items-end">
                      <button
                        type="button"
                        disabled={!roleKey.trim() || assignRoleMutation.isPending}
                        onClick={() =>
                          assignRoleMutation.mutate({
                            roleKey,
                            tenantId: roleTenantId.trim() ? roleTenantId : null,
                          })
                        }
                        className="rounded-md bg-[var(--color-accent)] px-3 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                      >
                        Assign role
                      </button>
                    </div>
                  </div>
                  <div className="mt-4 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
                    {userRolesQuery.isLoading ? (
                      <p className="px-3 py-4 text-sm text-[var(--color-text-muted)]">Loading roles…</p>
                    ) : userRolesQuery.isError ? (
                      <ApiErrorCallout
                        message={getErrorMessage(userRolesQuery.error, 'Failed to load platform roles.')}
                        onRetry={() => void userRolesQuery.refetch()}
                        retryLabel="Retry roles"
                      />
                    ) : roles.length === 0 ? (
                      <p className="px-3 py-4 text-sm text-[var(--color-text-muted)]">No platform roles found.</p>
                    ) : (
                      <ul className="divide-y divide-[var(--color-border-subtle)]">
                        {roles.map((role) => (
                          <li
                            key={`${role.roleKey}-${role.tenantId ?? 'global'}`}
                            className="flex flex-wrap items-center justify-between gap-3 px-3 py-3 text-sm"
                          >
                            <div>
                              <p className="font-medium text-[var(--color-text-primary)]">{formatRoleDisplayName(role.roleKey)}</p>
                              <p className="text-xs text-[var(--color-text-muted)]">
                                {role.tenantId ? 'Tenant-scoped role' : 'Global role'}
                                {role.isAssigned ? ' · assigned' : ' · unassigned'}
                              </p>
                            </div>
                            {role.isAssigned ? (
                              <button
                                type="button"
                                disabled={removeRoleMutation.isPending}
                                onClick={() => setPendingRoleRemoval(role)}
                                className="rounded-md border border-[var(--color-destructive-border)] px-3 py-1.5 text-xs font-medium text-[var(--color-destructive-text)] hover:bg-[var(--color-destructive-bg)] disabled:opacity-50"
                              >
                                Remove
                              </button>
                            ) : null}
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                </section>

                <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
                  <div>
                    <h4 className="font-semibold text-[var(--color-text-primary)]">External identity mappings</h4>
                    <p className="text-xs text-[var(--color-text-muted)]">
                      Link provider subjects to this platform user for external identity sync.
                    </p>
                  </div>
                  <div className="mt-4 grid gap-3 md:grid-cols-2">
                    <label className="block text-sm text-[var(--color-text-secondary)]">
                      Identity provider
                      <input
                        value={externalProviderKey}
                        onChange={(event) => setExternalProviderKey(event.target.value)}
                        placeholder="okta"
                        className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                      />
                    </label>
                    <label className="block text-sm text-[var(--color-text-secondary)]">
                      Provider subject
                      <input
                        value={externalSubject}
                        onChange={(event) => setExternalSubject(event.target.value)}
                        placeholder="00u123abc"
                        className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                      />
                    </label>
                    <label className="block text-sm text-[var(--color-text-secondary)] md:col-span-2">
                      External email
                      <input
                        value={externalEmail}
                        onChange={(event) => setExternalEmail(event.target.value)}
                        placeholder="optional@example.com"
                        className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                      />
                    </label>
                  </div>
                  <div className="mt-4 flex items-center justify-end gap-2">
                    <button
                      type="button"
                      disabled={
                        !externalProviderKey.trim() ||
                        !externalSubject.trim() ||
                        upsertExternalIdentityMutation.isPending
                      }
                      onClick={() =>
                        upsertExternalIdentityMutation.mutate({
                          providerKey: externalProviderKey,
                          externalSubject,
                          externalEmail: externalEmail.trim() ? externalEmail : null,
                        })
                      }
                      className="rounded-md bg-[var(--color-accent)] px-3 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                    >
                      Save mapping
                    </button>
                  </div>
                  <div className="mt-4 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
                    {userExternalIdentityMappingsQuery.isLoading ? (
                      <p className="px-3 py-4 text-sm text-[var(--color-text-muted)]">Loading mappings…</p>
                    ) : userExternalIdentityMappingsQuery.isError ? (
                      <ApiErrorCallout
                        message={getErrorMessage(
                          userExternalIdentityMappingsQuery.error,
                          'Failed to load external identity mappings.',
                        )}
                        onRetry={() => void userExternalIdentityMappingsQuery.refetch()}
                        retryLabel="Retry mappings"
                      />
                    ) : externalMappings.length === 0 ? (
                      <p className="px-3 py-4 text-sm text-[var(--color-text-muted)]">No external identity mappings found.</p>
                    ) : (
                      <ul className="divide-y divide-[var(--color-border-subtle)]">
                        {externalMappings.map((mapping) => (
                          <li
                            key={mapping.mappingId}
                            className="flex flex-wrap items-center justify-between gap-3 px-3 py-3 text-sm"
                          >
                            <div>
                              <p className="font-medium text-[var(--color-text-primary)]">{mapping.providerKey}</p>
                              <p className="text-xs text-[var(--color-text-muted)]">{mapping.externalSubject}</p>
                              <p className="text-xs text-[var(--color-text-muted)]">
                                {mapping.externalEmail ?? 'No external email'}
                                {mapping.modifiedAt ? ` · ${formatDateTime(mapping.modifiedAt)}` : ''}
                              </p>
                            </div>
                            <button
                              type="button"
                              disabled={removeExternalIdentityMutation.isPending}
                              onClick={() => setPendingExternalMappingRemoval(mapping)}
                              className="rounded-md border border-[var(--color-destructive-border)] px-3 py-1.5 text-xs font-medium text-[var(--color-destructive-text)] hover:bg-[var(--color-destructive-bg)] disabled:opacity-50"
                            >
                              Remove
                            </button>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                </section>
              </div>

              <div>
                <h4 className="font-semibold text-[var(--color-text-primary)]">Session status</h4>
                {userSessionsQuery.isLoading ? (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading sessions…</p>
                ) : userSessionsQuery.isError ? (
                  <ApiErrorCallout
                    message={getErrorMessage(userSessionsQuery.error, 'Failed to load sessions.')}
                    onRetry={() => void userSessionsQuery.refetch()}
                    retryLabel="Retry sessions"
                  />
                ) : sessions.length === 0 ? (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">No sessions found.</p>
                ) : (
                  <ul className="mt-3 divide-y divide-[var(--color-border-subtle)] rounded-lg border border-[var(--color-border-subtle)]">
                    {sessions.map((session) => (
                      <li
                        key={session.sessionId}
                        className="flex flex-col gap-2 px-3 py-3 sm:flex-row sm:items-center sm:justify-between"
                      >
                        <div className="min-w-0 text-sm">
                          <p className="font-medium text-[var(--color-text-primary)]">{formatSessionStatus(session)}</p>
                          <p className="text-xs text-[var(--color-text-muted)]">
                            Signed in {formatDateTime(session.createdAt)}
                            {session.ipAddress ? ` · ${session.ipAddress}` : ''}
                          </p>
                          <p className="text-xs text-[var(--color-text-muted)]">
                            Expires {formatDateTime(session.expiresAt)}
                            {session.activeTenantId ? ` · tenant ${session.activeTenantId.slice(0, 8)}…` : ''}
                          </p>
                        </div>
                        {session.isActive ? (
                          <button
                            type="button"
                            disabled={revokeSessionMutation.isPending}
                            onClick={() =>
                              setPendingSessionRevoke({
                                sessionId: session.sessionId,
                                isCurrent: session.isCurrent,
                              })
                            }
                            className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-1.5 text-xs font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
                          >
                            {session.isCurrent ? 'Sign out' : 'Revoke'}
                          </button>
                        ) : null}
                      </li>
                    ))}
                  </ul>
                )}
              </div>

              <div className="grid gap-4 lg:grid-cols-2">
                <HistoryPanel
                  title="Login history"
                  items={loginHistoryQuery.data?.items ?? []}
                  isLoading={loginHistoryQuery.isLoading}
                  isError={loginHistoryQuery.isError}
                  error={getErrorMessage(loginHistoryQuery.error, 'Failed to load login history.')}
                  onRetry={() => void loginHistoryQuery.refetch()}
                  retryLabel="Retry login history"
                  emptyLabel="No login history found."
                />
                <HistoryPanel
                  title="Launch history"
                  items={launchHistoryQuery.data?.items ?? []}
                  isLoading={launchHistoryQuery.isLoading}
                  isError={launchHistoryQuery.isError}
                  error={getErrorMessage(launchHistoryQuery.error, 'Failed to load launch history.')}
                  onRetry={() => void launchHistoryQuery.refetch()}
                  retryLabel="Retry launch history"
                  emptyLabel="No launch history found."
                />
                <IdentityAuditPanel
                  title="Identity audit history"
                  items={identityAuditHistoryQuery.data?.items ?? []}
                  isLoading={identityAuditHistoryQuery.isLoading}
                  isError={identityAuditHistoryQuery.isError}
                  error={getErrorMessage(
                    identityAuditHistoryQuery.error,
                    'Failed to load identity audit history.',
                  )}
                  onRetry={() => void identityAuditHistoryQuery.refetch()}
                  retryLabel="Retry identity audit history"
                  emptyLabel="No identity audit history found."
                />
              </div>
            </div>
          ) : null}
        </section>
      </div>

      <PlatformAdminScopeNote>
        Detail scope: NexArr covers platform login, MFA, sessions, tenant membership, platform roles, and identity audit history. Product permissions remain with the target product after identity is validated.
      </PlatformAdminScopeNote>
    </div>
  )
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3">
      <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</dt>
      <dd className="mt-1 text-sm text-[var(--color-text-primary)]">{value}</dd>
    </div>
  )
}

function HistoryPanel({
  title,
  items,
  isLoading,
  isError,
  error,
  onRetry,
  retryLabel,
  emptyLabel,
}: {
  title: string
  items: PlatformUserAccessHistoryItemResponse[]
  isLoading: boolean
  isError: boolean
  error: string
  onRetry: () => void
  retryLabel: string
  emptyLabel: string
}) {
  return (
    <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
      <h4 className="font-semibold text-[var(--color-text-primary)]">{title}</h4>
      {isLoading ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading…</p>
      ) : isError ? (
        <ApiErrorCallout message={error} onRetry={onRetry} retryLabel={retryLabel} />
      ) : items.length === 0 ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">{emptyLabel}</p>
      ) : (
        <ul className="mt-2 space-y-2">
          {items.slice(0, 5).map((item) => (
            <li key={item.auditEventId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-sm">
              <div className="font-medium text-[var(--color-text-primary)]">{item.action}</div>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                {item.result}
                {item.productDisplayName ? ` · ${item.productDisplayName}` : ''}
              </p>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">{formatDateTime(item.occurredAt)}</p>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}

function IdentityAuditPanel({
  title,
  items,
  isLoading,
  isError,
  error,
  onRetry,
  retryLabel,
  emptyLabel,
}: {
  title: string
  items: PlatformUserIdentityAuditHistoryItemResponse[]
  isLoading: boolean
  isError: boolean
  error: string
  onRetry: () => void
  retryLabel: string
  emptyLabel: string
}) {
  return (
    <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
      <h4 className="font-semibold text-[var(--color-text-primary)]">{title}</h4>
      {isLoading ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading…</p>
      ) : isError ? (
        <ApiErrorCallout message={error} onRetry={onRetry} retryLabel={retryLabel} />
      ) : items.length === 0 ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">{emptyLabel}</p>
      ) : (
        <ul className="mt-2 space-y-2">
          {items.slice(0, 5).map((item) => (
            <li key={item.auditEventId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-sm">
              <div className="font-medium text-[var(--color-text-primary)]">{item.action}</div>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                {item.result}
                {item.actorDisplayName ? ` · by ${item.actorDisplayName}` : ''}
              </p>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">{formatDateTime(item.occurredAt)}</p>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
