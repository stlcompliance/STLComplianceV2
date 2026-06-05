import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ToastProvider } from '../../feedback'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      placeholder,
      testId,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      placeholder?: string
      testId?: string
    }) => (
      <label>
        <span>{label}</span>
        <input
          aria-label={label}
          data-testid={testId}
          placeholder={placeholder}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <div>
          {options.map((option) => (
            <button key={option.value} type="button" onClick={() => onChange(option.value)}>
              {option.label}
            </button>
          ))}
        </div>
      </label>
    ),
  }
})

import * as nexarr from '../../api/nexarrClient'
import { PlatformUsersPage } from './PlatformUsersPage'

vi.mock('../../api/nexarrClient', () => ({
  listPlatformUsers: vi.fn(),
  getPlatformUser: vi.fn(),
  getPlatformUserSessions: vi.fn(),
  getPlatformUserTenantMemberships: vi.fn(),
  getPlatformUserRoles: vi.fn(),
  getPlatformUserExternalIdentityMappings: vi.fn(),
  getPlatformUserLoginHistory: vi.fn(),
  getPlatformUserLaunchHistory: vi.fn(),
  getPlatformUserIdentityAuditHistory: vi.fn(),
  createPlatformUser: vi.fn(),
  invitePlatformUser: vi.fn(),
  enablePlatformUser: vi.fn(),
  disablePlatformUser: vi.fn(),
  lockPlatformUser: vi.fn(),
  unlockPlatformUser: vi.fn(),
  resetPlatformUserPassword: vi.fn(),
  setPlatformUserMfa: vi.fn(),
  revokePlatformUserSession: vi.fn(),
  assignPlatformUserTenantMembership: vi.fn(),
  removePlatformUserTenantMembership: vi.fn(),
  assignPlatformUserRole: vi.fn(),
  removePlatformUserRole: vi.fn(),
  upsertPlatformUserExternalIdentityMapping: vi.fn(),
  removePlatformUserExternalIdentityMapping: vi.fn(),
  listTenants: vi.fn(),
}))

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <PlatformUsersPage />
      </ToastProvider>
    </QueryClientProvider>,
  )
}

describe('PlatformUsersPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows user details and allows MFA, session, and lifecycle actions', async () => {
    vi.mocked(nexarr.listPlatformUsers).mockResolvedValue({
      totalCount: 1,
      page: 1,
      pageSize: 20,
      hasNextPage: false,
      items: [
        {
          userId: '11111111-1111-1111-1111-111111111111',
          email: 'user@example.com',
          displayName: 'Test User',
          isActive: true,
          isPlatformAdmin: true,
          failedLoginCount: 1,
          lockedUntil: null,
          createdAt: '2026-06-01T00:00:00Z',
          modifiedAt: '2026-06-02T00:00:00Z',
          lastLoginAt: '2026-06-03T10:00:00Z',
          lastProductLaunchAt: '2026-06-03T11:00:00Z',
          canLogin: true,
          status: 'active',
          isMfaEnabled: false,
        },
      ],
    })
    vi.mocked(nexarr.getPlatformUser).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      email: 'user@example.com',
      displayName: 'Test User',
      isActive: true,
      isPlatformAdmin: true,
      failedLoginCount: 1,
      lockedUntil: null,
      createdAt: '2026-06-01T00:00:00Z',
      modifiedAt: '2026-06-02T00:00:00Z',
      lastLoginAt: '2026-06-03T10:00:00Z',
      lastProductLaunchAt: '2026-06-03T11:00:00Z',
      canLogin: true,
      status: 'active',
      isMfaEnabled: false,
    })
    vi.mocked(nexarr.getPlatformUserSessions).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      sessions: [
        {
          sessionId: '22222222-2222-2222-2222-222222222222',
          createdAt: '2026-06-03T08:00:00Z',
          expiresAt: '2026-06-10T08:00:00Z',
          revokedAt: null,
          userAgent: 'Chrome on Windows',
          ipAddress: '127.0.0.1',
          activeTenantId: '33333333-3333-3333-3333-333333333333',
          isCurrent: false,
          isActive: true,
          isRemembered: false,
        },
      ],
    })
    vi.mocked(nexarr.listTenants).mockResolvedValue({
      totalCount: 2,
      page: 1,
      pageSize: 200,
      hasNextPage: false,
      items: [
        {
          tenantId: '33333333-3333-3333-3333-333333333333',
          slug: 'main',
          displayName: 'Main Tenant',
          status: 'active',
          subscriptionTier: 'standard',
          billingCustomerId: null,
          billingSubscriptionId: null,
          billingGraceDays: null,
          isTrial: false,
          isInternalTenant: false,
          createdAt: '2026-06-01T00:00:00Z',
          modifiedAt: '2026-06-02T00:00:00Z',
        },
        {
          tenantId: '55555555-5555-5555-5555-555555555555',
          slug: 'backup',
          displayName: 'Backup Tenant',
          status: 'active',
          subscriptionTier: 'standard',
          billingCustomerId: null,
          billingSubscriptionId: null,
          billingGraceDays: null,
          isTrial: false,
          isInternalTenant: false,
          createdAt: '2026-06-01T00:00:00Z',
          modifiedAt: '2026-06-02T00:00:00Z',
        },
      ],
    })
    vi.mocked(nexarr.getPlatformUserTenantMemberships).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      items: [
        {
          tenantId: '33333333-3333-3333-3333-333333333333',
          tenantSlug: 'main',
          tenantDisplayName: 'Main Tenant',
          roleKey: 'tenant_user',
          isActive: true,
          createdAt: '2026-06-03T08:00:00Z',
        },
      ],
    })
    vi.mocked(nexarr.getPlatformUserRoles).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      items: [
        { roleKey: 'platform_support', isAssigned: true, tenantId: null },
        { roleKey: 'tenant_admin', isAssigned: true, tenantId: '33333333-3333-3333-3333-333333333333' },
      ],
    })
    vi.mocked(nexarr.getPlatformUserExternalIdentityMappings).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      items: [
        {
          mappingId: '77777777-7777-7777-7777-777777777777',
          userId: '11111111-1111-1111-1111-111111111111',
          providerKey: 'okta',
          externalSubject: '00u123abc',
          externalEmail: 'user@example.com',
          createdAt: '2026-06-03T08:30:00Z',
          modifiedAt: '2026-06-03T08:35:00Z',
        },
      ],
    })
    vi.mocked(nexarr.getPlatformUserLoginHistory).mockResolvedValue({
      items: [
        {
          auditEventId: '44444444-4444-4444-4444-444444444444',
          userId: '11111111-1111-1111-1111-111111111111',
          userEmail: 'user@example.com',
          userDisplayName: 'Test User',
          tenantId: '33333333-3333-3333-3333-333333333333',
          tenantSlug: 'main',
          action: 'auth.login',
          result: 'success',
          reasonCode: null,
          targetType: 'user',
          targetId: null,
          correlationId: '55555555-5555-5555-5555-555555555555',
          occurredAt: '2026-06-03T09:00:00Z',
          productKey: null,
          productDisplayName: null,
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformUserLaunchHistory).mockResolvedValue({
      items: [
        {
          auditEventId: '66666666-6666-6666-6666-666666666666',
          userId: '11111111-1111-1111-1111-111111111111',
          userEmail: 'user@example.com',
          userDisplayName: 'Test User',
          tenantId: '33333333-3333-3333-3333-333333333333',
          tenantSlug: 'main',
          action: 'launch.handoff.create',
          result: 'success',
          reasonCode: null,
          targetType: 'handoff',
          targetId: null,
          correlationId: '77777777-7777-7777-7777-777777777777',
          occurredAt: '2026-06-03T09:05:00Z',
          productKey: 'maintainarr',
          productDisplayName: 'MaintainArr',
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformUserIdentityAuditHistory).mockResolvedValue({
      items: [
        {
          auditEventId: '88888888-8888-8888-8888-888888888888',
          userId: '11111111-1111-1111-1111-111111111111',
          userEmail: 'user@example.com',
          userDisplayName: 'Test User',
          tenantId: '33333333-3333-3333-3333-333333333333',
          tenantSlug: 'main',
          actorUserId: '99999999-9999-9999-9999-999999999999',
          actorEmail: 'admin@example.com',
          actorDisplayName: 'Platform Admin',
          action: 'user.updated',
          result: 'Success',
          reasonCode: null,
          targetType: 'user',
          targetId: '11111111-1111-1111-1111-111111111111',
          correlationId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          occurredAt: '2026-06-03T09:10:00Z',
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.setPlatformUserMfa).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      isMfaEnabled: true,
      wasAlreadySet: false,
      modifiedAt: '2026-06-03T12:00:00Z',
      mfaSecret: 'JBSWY3DPEHPK3PXP',
      provisioningUri: 'otpauth://totp/STL%20Compliance%20Suite:user%40example.com?secret=JBSWY3DPEHPK3PXP&issuer=STL%20Compliance%20Suite&digits=6&period=30',
      recoveryCodes: ['ABCD-EFGH-IJKL', 'MNOP-QRST-UVWX'],
    })
    vi.mocked(nexarr.disablePlatformUser).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      wasAlreadyDisabled: false,
    })
    vi.mocked(nexarr.lockPlatformUser).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      wasAlreadyLocked: false,
      lockedUntil: '2026-06-04T12:00:00Z',
    })
    vi.mocked(nexarr.resetPlatformUserPassword).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      passwordChangedAt: '2026-06-03T12:07:00Z',
    })
    vi.mocked(nexarr.revokePlatformUserSession).mockResolvedValue()
    vi.mocked(nexarr.assignPlatformUserTenantMembership).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      tenantId: '33333333-3333-3333-3333-333333333333',
      wasReactivated: false,
    })
    vi.mocked(nexarr.removePlatformUserTenantMembership).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      tenantId: '33333333-3333-3333-3333-333333333333',
      wasAlreadyRemoved: false,
    })
    vi.mocked(nexarr.assignPlatformUserRole).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      roleKey: 'platform_support',
      wasAlreadyAssigned: false,
      tenantId: null,
    })
    vi.mocked(nexarr.removePlatformUserRole).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      roleKey: 'platform_support',
      wasAlreadyRemoved: false,
      tenantId: null,
    })
    vi.mocked(nexarr.invitePlatformUser).mockResolvedValue({
      userId: '99999999-9999-9999-9999-999999999999',
      email: 'invite@example.com',
      displayName: 'Invitee User',
      isActive: true,
      isPlatformAdmin: false,
      failedLoginCount: 0,
      lockedUntil: null,
      createdAt: '2026-06-03T13:00:00Z',
      modifiedAt: '2026-06-03T13:00:00Z',
      lastLoginAt: null,
      lastProductLaunchAt: null,
      canLogin: false,
      status: 'invited',
      isMfaEnabled: false,
    })
    vi.mocked(nexarr.createPlatformUser).mockResolvedValue({
      userId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      email: 'create@example.com',
      displayName: 'Created User',
      isActive: true,
      isPlatformAdmin: false,
      failedLoginCount: 0,
      lockedUntil: null,
      createdAt: '2026-06-03T14:00:00Z',
      modifiedAt: '2026-06-03T14:00:00Z',
      lastLoginAt: null,
      lastProductLaunchAt: null,
      canLogin: true,
      status: 'active',
      isMfaEnabled: false,
    })
    vi.mocked(nexarr.upsertPlatformUserExternalIdentityMapping).mockResolvedValue({
      mappingId: '88888888-8888-8888-8888-888888888888',
      userId: '11111111-1111-1111-1111-111111111111',
      providerKey: 'azuread',
      externalSubject: 'abc-123',
      externalEmail: 'user@example.com',
      wasUpdated: false,
    })
    vi.mocked(nexarr.removePlatformUserExternalIdentityMapping).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      mappingId: '77777777-7777-7777-7777-777777777777',
      wasAlreadyRemoved: false,
    })

    renderPage()

    expect(await screen.findByText('Test User')).toBeInTheDocument()
    expect(await screen.findByRole('button', { name: 'Enable MFA' })).toBeInTheDocument()
    expect(screen.getByText('Active sessions')).toBeInTheDocument()
    expect(screen.getByText('Login history')).toBeInTheDocument()
    expect(screen.getByText('Launch history')).toBeInTheDocument()
    expect(screen.getByText('Identity audit history')).toBeInTheDocument()
    expect(screen.getByText('Tenant memberships')).toBeInTheDocument()
    expect(screen.getByText('Platform roles')).toBeInTheDocument()
    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Assign membership' })).not.toBeDisabled()
    })

    fireEvent.click(screen.getByRole('button', { name: 'Enable MFA' }))

    await waitFor(() => {
      expect(nexarr.setPlatformUserMfa).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        true,
      )
    })
    expect(await screen.findByText('Secret')).toBeInTheDocument()
    expect(screen.getByText('JBSWY3DPEHPK3PXP')).toBeInTheDocument()
    expect(screen.getByText('ABCD-EFGH-IJKL')).toBeInTheDocument()
    expect(screen.getByText('MNOP-QRST-UVWX')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Revoke' }))
    expect(screen.getByRole('alertdialog')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Revoke session' }))

    await waitFor(() => {
      expect(nexarr.revokePlatformUserSession).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        '22222222-2222-2222-2222-222222222222',
      )
    })

    fireEvent.click(screen.getByRole('button', { name: 'Disable user' }))
    fireEvent.click(screen.getByRole('button', { name: 'Confirm' }))

    await waitFor(() => {
      expect(nexarr.disablePlatformUser).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
      )
    })

    fireEvent.click(screen.getByRole('button', { name: 'Lock user' }))
    fireEvent.click(screen.getByRole('button', { name: 'Confirm' }))

    await waitFor(() => {
      expect(nexarr.lockPlatformUser).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
      )
    })

    fireEvent.change(screen.getByLabelText('New password'), {
      target: { value: 'TempPass!123' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Reset password' }))
    fireEvent.click(within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Reset password' }))

    await waitFor(() => {
      expect(nexarr.resetPlatformUserPassword).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        'TempPass!123',
      )
    })

    const membershipSection = screen.getByText('Tenant memberships').closest('section')
    expect(membershipSection).toBeTruthy()
    expect(within(membershipSection!).getByText('Main Tenant (main)')).toBeInTheDocument()
    expect(within(membershipSection!).getByText('Backup Tenant (backup)')).toBeInTheDocument()
    fireEvent.click(within(membershipSection!).getByRole('button', { name: 'Backup Tenant (backup)' }))
    fireEvent.click(within(membershipSection!).getByRole('button', { name: 'Assign membership' }))

    await waitFor(() => {
      expect(nexarr.assignPlatformUserTenantMembership).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        {
          tenantId: '55555555-5555-5555-5555-555555555555',
          roleKey: 'tenant_user',
        },
      )
    })

    fireEvent.click(within(membershipSection!).getByRole('button', { name: 'Remove' }))
    fireEvent.click(screen.getByRole('button', { name: 'Remove membership' }))

    await waitFor(() => {
      expect(nexarr.removePlatformUserTenantMembership).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        '33333333-3333-3333-3333-333333333333',
      )
    })

    const roleSection = screen.getByText('Platform roles').closest('section')
    expect(roleSection).toBeTruthy()
    expect(within(roleSection!).getByText('Main Tenant (main)')).toBeInTheDocument()
    expect(within(roleSection!).getByText('Backup Tenant (backup)')).toBeInTheDocument()
    fireEvent.click(within(roleSection!).getByRole('button', { name: 'Backup Tenant (backup)' }))
    fireEvent.click(within(roleSection!).getByRole('button', { name: 'Assign role' }))

    await waitFor(() => {
      expect(nexarr.assignPlatformUserRole).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        {
          roleKey: 'platform_support',
          tenantId: '55555555-5555-5555-5555-555555555555',
        },
      )
    })

    fireEvent.click(within(roleSection!).getAllByRole('button', { name: 'Remove' })[0])
    fireEvent.click(screen.getByRole('button', { name: 'Remove role' }))

    await waitFor(() => {
      expect(nexarr.removePlatformUserRole).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        'platform_support',
        null,
      )
    })

    const mappingSection = screen.getByText('External identity mappings').closest('section')
    expect(mappingSection).toBeTruthy()
    fireEvent.change(within(mappingSection!).getByLabelText('Provider key'), {
      target: { value: 'azuread' },
    })
    fireEvent.change(within(mappingSection!).getByLabelText('External subject'), {
      target: { value: 'abc-123' },
    })
    fireEvent.change(within(mappingSection!).getByLabelText('External email'), {
      target: { value: 'user@example.com' },
    })
    fireEvent.click(within(mappingSection!).getByRole('button', { name: 'Save mapping' }))

    await waitFor(() => {
      expect(nexarr.upsertPlatformUserExternalIdentityMapping).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        {
          providerKey: 'azuread',
          externalSubject: 'abc-123',
          externalEmail: 'user@example.com',
        },
      )
    })

    fireEvent.click(within(mappingSection!).getByRole('button', { name: 'Remove' }))
    fireEvent.click(screen.getByRole('button', { name: 'Remove mapping' }))

    await waitFor(() => {
      expect(nexarr.removePlatformUserExternalIdentityMapping).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        '77777777-7777-7777-7777-777777777777',
      )
    })

    const createSection = screen.getByText('Create or invite user').closest('section')
    expect(createSection).toBeTruthy()
    fireEvent.change(within(createSection!).getByLabelText('Email'), {
      target: { value: 'invite@example.com' },
    })
    fireEvent.change(within(createSection!).getByLabelText('Display name'), {
      target: { value: 'Invitee User' },
    })
    fireEvent.click(within(createSection!).getByRole('button', { name: 'Invite user' }))

    await waitFor(() => {
      expect(nexarr.invitePlatformUser).toHaveBeenCalledWith({
        email: 'invite@example.com',
        displayName: 'Invitee User',
        isPlatformAdmin: false,
        isActive: true,
      })
    })

    fireEvent.change(within(createSection!).getByRole('combobox', { name: 'Mode' }), {
      target: { value: 'create' },
    })
    fireEvent.change(within(createSection!).getByLabelText('Email'), {
      target: { value: 'create@example.com' },
    })
    fireEvent.change(within(createSection!).getByLabelText('Display name'), {
      target: { value: 'Created User' },
    })
    fireEvent.change(within(createSection!).getByLabelText('Temporary password'), {
      target: { value: 'TempPass!123' },
    })
    fireEvent.click(within(createSection!).getByRole('button', { name: 'Create user' }))

    await waitFor(() => {
      expect(nexarr.createPlatformUser).toHaveBeenCalledWith({
        email: 'create@example.com',
        displayName: 'Created User',
        password: 'TempPass!123',
        isPlatformAdmin: false,
        isActive: true,
        requireEmailVerification: false,
      })
    })
  }, 10000)

  it('can enable and unlock accounts when the user is inactive and locked', async () => {
    vi.mocked(nexarr.listPlatformUsers).mockResolvedValue({
      totalCount: 1,
      page: 1,
      pageSize: 20,
      hasNextPage: false,
      items: [
        {
          userId: '11111111-1111-1111-1111-111111111111',
          email: 'user@example.com',
          displayName: 'Test User',
          isActive: false,
          isPlatformAdmin: true,
          failedLoginCount: 5,
          lockedUntil: '2099-06-04T12:00:00Z',
          createdAt: '2026-06-01T00:00:00Z',
          modifiedAt: '2026-06-02T00:00:00Z',
          lastLoginAt: '2026-06-03T10:00:00Z',
          lastProductLaunchAt: '2026-06-03T11:00:00Z',
          canLogin: false,
          status: 'locked',
          isMfaEnabled: false,
        },
      ],
    })
    vi.mocked(nexarr.getPlatformUser).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      email: 'user@example.com',
      displayName: 'Test User',
      isActive: false,
      isPlatformAdmin: true,
      failedLoginCount: 5,
      lockedUntil: '2099-06-04T12:00:00Z',
      createdAt: '2026-06-01T00:00:00Z',
      modifiedAt: '2026-06-02T00:00:00Z',
      lastLoginAt: '2026-06-03T10:00:00Z',
      lastProductLaunchAt: '2026-06-03T11:00:00Z',
      canLogin: false,
      status: 'locked',
      isMfaEnabled: false,
    })
    vi.mocked(nexarr.getPlatformUserSessions).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      sessions: [],
    })
    vi.mocked(nexarr.listTenants).mockResolvedValue({
      totalCount: 0,
      page: 1,
      pageSize: 200,
      hasNextPage: false,
      items: [],
    })
    vi.mocked(nexarr.getPlatformUserTenantMemberships).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      items: [],
    })
    vi.mocked(nexarr.getPlatformUserRoles).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      items: [],
    })
    vi.mocked(nexarr.getPlatformUserExternalIdentityMappings).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      items: [],
    })
    vi.mocked(nexarr.getPlatformUserLoginHistory).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformUserLaunchHistory).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.enablePlatformUser).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      wasAlreadyEnabled: false,
    })
    vi.mocked(nexarr.unlockPlatformUser).mockResolvedValue({
      userId: '11111111-1111-1111-1111-111111111111',
      wasAlreadyUnlocked: false,
    })

    renderPage()

    expect(await screen.findByText('Test User')).toBeInTheDocument()
    expect(await screen.findByRole('button', { name: 'Enable user' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Unlock user' })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Enable user' }))
    fireEvent.click(screen.getByRole('button', { name: 'Confirm' }))

    await waitFor(() => {
      expect(nexarr.enablePlatformUser).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
      )
    })

    fireEvent.click(screen.getByRole('button', { name: 'Unlock user' }))
    fireEvent.click(screen.getByRole('button', { name: 'Confirm' }))

    await waitFor(() => {
      expect(nexarr.unlockPlatformUser).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
      )
    })
  })
})
