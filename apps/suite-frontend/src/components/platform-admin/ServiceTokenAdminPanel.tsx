import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import {
  buildSemanticKey,
  CheckboxMultiSelect,
  ControlledSelect,
  GeneratedKeyField,
  type PickerOption,
} from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import type { ServiceTokenDiscoveryResponse, ServiceTokenIssueResult } from '../../api/types'
import { ConfirmDialog } from '../../feedback'
import { ServiceClientsCard } from './service-token/ServiceClientsCard'
import { ServiceTokensCard } from './service-token/ServiceTokensCard'

export function ServiceTokenAdminPanel() {
  const queryClient = useQueryClient()
  const [clientDisplayName, setClientDisplayName] = useState('')
  const [confirmedClientKey, setConfirmedClientKey] = useState<string | null>(null)
  const [sourceProductKey, setSourceProductKey] = useState('')
  const [allowedProductKeys, setAllowedProductKeys] = useState<string[]>([])
  const [issueClientId, setIssueClientId] = useState('')
  const [issueTenantId, setIssueTenantId] = useState('')
  const [issueActionScope, setIssueActionScope] = useState('')
  const [issueLifetimeMinutes, setIssueLifetimeMinutes] = useState('60')
  const [clientsPage, setClientsPage] = useState(1)
  const [tokensPage, setTokensPage] = useState(1)
  const [auditTenantId, setAuditTenantId] = useState('')
  const [auditServiceClientId, setAuditServiceClientId] = useState('')
  const [auditPage, setAuditPage] = useState(1)
  const [issuedToken, setIssuedToken] = useState<ServiceTokenIssueResult | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [pendingClientAction, setPendingClientAction] = useState<{
    kind: 'rotate' | 'revoke'
    serviceClientId: string
  } | null>(null)
  const [selectedServiceClientId, setSelectedServiceClientId] = useState('')
  const [editAllowedProductKeys, setEditAllowedProductKeys] = useState<string[]>([])
  const [editAllowedTenantIds, setEditAllowedTenantIds] = useState<string[]>([])

  const clientsQuery = useQuery({
    queryKey: ['platform-service-clients', clientsPage],
    queryFn: () => nexarr.listServiceClients(clientsPage, 25),
  })

  const tokensQuery = useQuery({
    queryKey: ['platform-service-tokens', tokensPage],
    queryFn: () => nexarr.listServiceTokens(undefined, tokensPage, 25),
  })

  const discoveryQuery = useQuery<ServiceTokenDiscoveryResponse>({
    queryKey: ['platform-service-token-discovery'],
    queryFn: () => nexarr.getServiceTokenDiscovery(),
    staleTime: 60_000,
  })

  const auditQuery = useQuery({
    queryKey: ['platform-service-token-audit', auditTenantId, auditServiceClientId, auditPage],
    queryFn: () =>
      nexarr.getServiceTokenAuditHistory({
        tenantId: auditTenantId || undefined,
        serviceClientId: auditServiceClientId || undefined,
        page: auditPage,
        pageSize: 10,
      }),
  })

  const tenantsQuery = useQuery({
    queryKey: ['platform-admin-tenant-overview'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 100),
  })

  const productsQuery = useQuery({
    queryKey: ['platform-admin-product-overview'],
    queryFn: () => nexarr.getPlatformAdminProductOverview(),
  })

  const existingClientKeys = useMemo(
    () => (clientsQuery.data?.items ?? []).map((client) => client.clientKey),
    [clientsQuery.data?.items],
  )

  const generatedClientKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'product',
        kind: 'serviceclient',
        title: clientDisplayName,
        existingKeys: existingClientKeys,
        maxLength: 128,
      }),
    [clientDisplayName, existingClientKeys],
  )

  const productOptions: PickerOption[] = useMemo(
    () =>
      (productsQuery.data ?? [])
        .filter((product) => product.isActive)
        .map((product) => ({
          value: product.productKey,
          label: product.displayName,
        })),
    [productsQuery.data],
  )

  const tenantOptions: PickerOption[] = useMemo(
    () =>
      (tenantsQuery.data?.items ?? []).map((tenant) => ({
        value: tenant.tenantId,
        label: `${tenant.displayName} (${tenant.slug})`,
      })),
    [tenantsQuery.data?.items],
  )

  const clientOptions: PickerOption[] = useMemo(
    () =>
      (clientsQuery.data?.items ?? []).map((client) => ({
        value: client.serviceClientId,
        label: `${client.displayName} (${client.clientKey})`,
      })),
    [clientsQuery.data?.items],
  )

  const selectedClient = useMemo(
    () => (clientsQuery.data?.items ?? []).find((client) => client.serviceClientId === selectedServiceClientId) ?? null,
    [clientsQuery.data?.items, selectedServiceClientId],
  )

  const registerMutation = useMutation({
    mutationFn: () =>
      nexarr.registerServiceClient({
        clientKey: generatedClientKey.trim(),
        displayName: clientDisplayName.trim(),
        sourceProductKey,
        allowedProductKeys,
      }),
    onSuccess: (client) => {
      setErrorMessage(null)
      setConfirmedClientKey(client.clientKey)
      setClientDisplayName('')
      setAllowedProductKeys([])
      void queryClient.invalidateQueries({ queryKey: ['platform-service-clients'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const issueMutation = useMutation({
    mutationFn: () =>
      nexarr.issueServiceToken({
        serviceClientId: issueClientId,
        tenantId: issueTenantId.trim() || null,
        actionScope: issueActionScope.trim() || null,
        lifetimeMinutes: Number.parseInt(issueLifetimeMinutes, 10) || 60,
      }),
    onSuccess: (result) => {
      setErrorMessage(null)
      setIssuedToken(result)
      void queryClient.invalidateQueries({ queryKey: ['platform-service-tokens'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const revokeMutation = useMutation({
    mutationFn: (tokenId: string) => nexarr.revokeServiceToken(tokenId),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-service-tokens'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const rotateClientMutation = useMutation({
    mutationFn: (serviceClientId: string) => nexarr.rotateServiceClient(serviceClientId),
    onSuccess: () => {
      setPendingClientAction(null)
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-service-clients'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-service-tokens'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-service-token-audit'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const revokeClientMutation = useMutation({
    mutationFn: (serviceClientId: string) => nexarr.revokeServiceClient(serviceClientId),
    onSuccess: () => {
      setPendingClientAction(null)
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-service-clients'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-service-tokens'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-service-token-audit'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const updateAudienceMutation = useMutation({
    mutationFn: () =>
      nexarr.updateServiceClientAudience(selectedServiceClientId, editAllowedProductKeys),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-service-clients'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-service-token-audit'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const updateTenantScopeMutation = useMutation({
    mutationFn: () =>
      nexarr.updateServiceClientTenantScope(selectedServiceClientId, editAllowedTenantIds),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-service-clients'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-service-token-audit'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  useEffect(() => {
    if (!selectedServiceClientId) {
      const fallback = clientsQuery.data?.items?.[0]?.serviceClientId ?? ''
      if (fallback) {
        setSelectedServiceClientId(fallback)
      }
    }
  }, [clientsQuery.data?.items, selectedServiceClientId])

  useEffect(() => {
    if (!selectedClient) {
      setEditAllowedProductKeys([])
      setEditAllowedTenantIds([])
      return
    }

    setEditAllowedProductKeys(selectedClient.allowedProductKeys ?? [])
    setEditAllowedTenantIds(selectedClient.allowedTenantIds ?? [])
  }, [selectedClient])

  return (
    <section
      data-testid="service-token-admin-panel"
      className="space-y-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5"
    >
      <ConfirmDialog
        open={pendingClientAction !== null}
        title={pendingClientAction?.kind === 'rotate' ? 'Rotate service client?' : 'Revoke service client?'}
        description={
          pendingClientAction?.kind === 'rotate'
            ? 'This revokes active tokens for the client and keeps the client available for future issuance.'
            : 'This disables the client and revokes active tokens issued to it.'
        }
        confirmLabel={pendingClientAction?.kind === 'rotate' ? 'Rotate client' : 'Revoke client'}
        danger={pendingClientAction?.kind === 'revoke'}
        loading={rotateClientMutation.isPending || revokeClientMutation.isPending}
        onCancel={() => {
          if (!rotateClientMutation.isPending && !revokeClientMutation.isPending) {
            setPendingClientAction(null)
          }
        }}
        onConfirm={() => {
          if (!pendingClientAction) return
          if (pendingClientAction.kind === 'rotate') {
            rotateClientMutation.mutate(pendingClientAction.serviceClientId)
          } else {
            revokeClientMutation.mutate(pendingClientAction.serviceClientId)
          }
        }}
      />
      <header>
        <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Service token administration</h2>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Register service clients and issue or revoke service tokens via NexArr{' '}
          <code className="text-xs">/api/service-tokens</code>. Issued bearer tokens are shown once.
        </p>
      </header>

      {errorMessage ? (
        <p className="text-sm text-[var(--color-danger-text)]" data-testid="service-token-admin-error">
          {errorMessage}
        </p>
      ) : null}

      <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Service token discovery</h3>
            <p className="mt-1 text-xs text-[var(--color-text-muted)]">
              Public trust metadata for product token validation and service-client bootstrap.
            </p>
          </div>
          <span
            className={[
              'rounded-full px-2 py-0.5 text-xs font-medium',
              discoveryQuery.data?.publicKeyAvailable
                ? 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]'
                : 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)]',
            ].join(' ')}
            data-testid="service-token-discovery-key-status"
          >
            {discoveryQuery.data?.publicKeyAvailable ? 'JWKS available' : 'HS256 only'}
          </span>
        </div>

        {discoveryQuery.isLoading ? (
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading discovery metadata…</p>
        ) : discoveryQuery.isError ? (
          <p className="mt-3 text-sm text-[var(--color-danger-text)]" data-testid="service-token-discovery-error">
            {discoveryQuery.error instanceof Error
              ? discoveryQuery.error.message
              : 'Failed to load service token discovery.'}
          </p>
        ) : discoveryQuery.data ? (
          <dl className="mt-3 grid gap-3 md:grid-cols-2">
            <DiscoveryRow label="Issuer" value={discoveryQuery.data.issuer} />
            <DiscoveryRow label="Audience" value={discoveryQuery.data.audience} />
            <DiscoveryRow label="JWKS URI" value={discoveryQuery.data.jwksUri} mono />
            <DiscoveryRow
              label="Supported algorithms"
              value={discoveryQuery.data.supportedAlgorithms.join(', ')}
            />
          </dl>
        ) : null}
      </div>

      <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Register service client</h3>
        <div className="mt-3 grid gap-3 sm:grid-cols-2">
          <label htmlFor="service-token-client-display-name" className="block text-sm text-[var(--color-text-secondary)]">
            Service client display name
            <input
              id="service-token-client-display-name"
              value={clientDisplayName}
              onChange={(event) => {
                setClientDisplayName(event.target.value)
                setConfirmedClientKey(null)
              }}
              data-testid="service-token-client-display-name"
              className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <GeneratedKeyField
            sourceLabel={clientDisplayName}
            generatedKey={generatedClientKey}
            confirmedKey={confirmedClientKey}
            manualOverride=""
            onManualOverrideChange={() => {}}
            label="Client key"
          />
          <ControlledSelect
            label="Source product"
            value={sourceProductKey}
            onChange={setSourceProductKey}
            options={productOptions}
            emptyLabel="Select product…"
            testId="service-token-source-product"
          />
          <CheckboxMultiSelect
            label="Allowed products"
            values={allowedProductKeys}
            onChange={setAllowedProductKeys}
            options={productOptions}
            testId="service-token-allowed-products"
          />
        </div>
        <button
          type="button"
          onClick={() => registerMutation.mutate()}
          disabled={
            !generatedClientKey ||
            !clientDisplayName ||
            !sourceProductKey ||
            allowedProductKeys.length === 0 ||
            registerMutation.isPending
          }
          data-testid="service-token-register-client"
          className="mt-3 rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
        >
          {registerMutation.isPending ? 'Registering…' : 'Register client'}
        </button>
      </div>

      <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Issue service token</h3>
        <div className="mt-3 grid gap-3 sm:grid-cols-2">
          <ControlledSelect
            label="Service client"
            value={issueClientId}
            onChange={setIssueClientId}
            options={clientOptions}
            emptyLabel="Select client…"
            testId="service-token-issue-client"
          />
          <ControlledSelect
            label="Tenant scope (optional)"
            value={issueTenantId}
            onChange={setIssueTenantId}
            options={tenantOptions}
            emptyLabel="Platform-wide"
            testId="service-token-issue-tenant"
          />
          <label htmlFor="service-token-issue-scope" className="block text-sm text-[var(--color-text-secondary)]">
            Token action scope (optional)
            <input
              id="service-token-issue-scope"
              value={issueActionScope}
              onChange={(event) => setIssueActionScope(event.target.value)}
              data-testid="service-token-issue-scope"
              className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <label htmlFor="service-token-issue-lifetime" className="block text-sm text-[var(--color-text-secondary)]">
            Token lifetime (minutes)
            <input
              id="service-token-issue-lifetime"
              type="number"
              min={1}
              max={1440}
              value={issueLifetimeMinutes}
              onChange={(event) => setIssueLifetimeMinutes(event.target.value)}
              data-testid="service-token-issue-lifetime"
              className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
        </div>
        <button
          type="button"
          onClick={() => issueMutation.mutate()}
          disabled={!issueClientId || issueMutation.isPending}
          data-testid="service-token-issue"
          className="mt-3 rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
        >
          {issueMutation.isPending ? 'Issuing…' : 'Issue token'}
        </button>

        {issuedToken ? (
          <div
            className="mt-4 rounded-md border border-[var(--color-warning-border)] bg-[var(--color-warning-bg)] p-3 text-sm"
            data-testid="service-token-issued-result"
          >
            <p className="font-medium text-[var(--color-warning-text)]">Token issued — copy now; it will not be shown again.</p>
            <p className="mt-2 break-all font-mono text-xs text-[var(--color-text-primary)]">{issuedToken.accessToken}</p>
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">
              Expires {new Date(issuedToken.expiresAt).toLocaleString()} · ID {issuedToken.tokenId}
            </p>
          </div>
        ) : null}
      </div>

      <ServiceClientsCard
        clientsQuery={clientsQuery}
        page={clientsPage}
        onPreviousPage={() => setClientsPage((value) => Math.max(1, value - 1))}
        onNextPage={() => {
          if (clientsQuery.data?.hasNextPage) {
            setClientsPage((value) => value + 1)
          }
        }}
        selectedServiceClientId={selectedServiceClientId}
        onSelectClient={setSelectedServiceClientId}
        onRotate={(serviceClientId) => setPendingClientAction({ kind: 'rotate', serviceClientId })}
        onRevoke={(serviceClientId) => setPendingClientAction({ kind: 'revoke', serviceClientId })}
        actionPending={rotateClientMutation.isPending || revokeClientMutation.isPending}
      />

      {selectedClient ? (
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Manage selected client</h3>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                Update audience and tenant scope for {selectedClient.displayName} ({selectedClient.clientKey}).
              </p>
            </div>
            <p className="text-xs text-[var(--color-text-muted)]">
              {selectedClient.isActive ? 'Enabled' : 'Disabled'}
              {selectedClient.lastUsedAt ? ` · last used ${new Date(selectedClient.lastUsedAt).toLocaleString()}` : ''}
            </p>
          </div>

          <div className="mt-4 grid gap-4 lg:grid-cols-2">
            <div className="space-y-3">
              <CheckboxMultiSelect
                label="Allowed products"
                values={editAllowedProductKeys}
                onChange={setEditAllowedProductKeys}
                options={productOptions}
                testId="service-token-edit-allowed-products"
              />
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => updateAudienceMutation.mutate()}
                  disabled={updateAudienceMutation.isPending}
                className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                >
                  Save audience
                </button>
                <button
                  type="button"
                  onClick={() => setEditAllowedProductKeys(selectedClient.allowedProductKeys ?? [])}
                  disabled={updateAudienceMutation.isPending}
                className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
                >
                  Reset
                </button>
              </div>
            </div>

            <div className="space-y-3">
              <CheckboxMultiSelect
                label="Allowed tenants"
                values={editAllowedTenantIds}
                onChange={setEditAllowedTenantIds}
                options={tenantOptions}
                testId="service-token-edit-allowed-tenants"
              />
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={() => updateTenantScopeMutation.mutate()}
                  disabled={updateTenantScopeMutation.isPending}
                className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                >
                  Save tenant scope
                </button>
                <button
                  type="button"
                  onClick={() => setEditAllowedTenantIds(selectedClient.allowedTenantIds ?? [])}
                  disabled={updateTenantScopeMutation.isPending}
                className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
                >
                  Reset
                </button>
              </div>
            </div>
          </div>
        </div>
      ) : null}

      <ServiceTokensCard
        tokensQuery={tokensQuery}
        revokePending={revokeMutation.isPending}
        onRevoke={(tokenId) => revokeMutation.mutate(tokenId)}
        page={tokensPage}
        onPreviousPage={() => setTokensPage((value) => Math.max(1, value - 1))}
        onNextPage={() => {
          if (tokensQuery.data?.hasNextPage) {
            setTokensPage((value) => value + 1)
          }
        }}
      />

      <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Service token audit history</h3>
            <p className="mt-1 text-xs text-[var(--color-text-muted)]">
              Review token issuance, revocation, validation, and service-client activity.
            </p>
          </div>
          <button
            type="button"
            onClick={() => {
              setAuditTenantId('')
              setAuditServiceClientId('')
              setAuditPage(1)
            }}
            className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-1.5 text-xs font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
          >
            Clear filters
          </button>
        </div>

        <div className="mt-3 grid gap-3 sm:grid-cols-2">
          <ControlledSelect
            label="Filter by service client"
            value={auditServiceClientId}
            onChange={(value) => {
              setAuditServiceClientId(value)
              setAuditPage(1)
            }}
            options={clientOptions}
            emptyLabel="All clients"
            testId="service-token-audit-client"
          />
          <ControlledSelect
            label="Filter by tenant"
            value={auditTenantId}
            onChange={(value) => {
              setAuditTenantId(value)
              setAuditPage(1)
            }}
            options={tenantOptions}
            emptyLabel="All tenants"
            testId="service-token-audit-tenant"
          />
        </div>

        {auditQuery.isLoading ? (
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading audit history…</p>
        ) : auditQuery.isError ? (
          <p className="mt-3 text-sm text-[var(--color-danger-text)]">{auditQuery.error.message}</p>
        ) : auditQuery.data?.items.length ? (
          <ul className="mt-3 space-y-2">
            {auditQuery.data.items.map((item) => (
              <li
                key={item.auditEventId}
                className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3 text-sm"
              >
                <div className="font-medium text-[var(--color-text-primary)]">{item.action}</div>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  {item.result}
                  {item.targetType ? ` · ${item.targetType}` : ''}
                  {item.targetId ? ` · ${item.targetId}` : ''}
                </p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  {new Date(item.occurredAt).toLocaleString()}
                </p>
              </li>
            ))}
          </ul>
        ) : (
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">No service token audit events found.</p>
        )}

        <div className="mt-4 flex items-center justify-between gap-3">
          <button
            type="button"
            disabled={auditPage <= 1}
            onClick={() => setAuditPage((value) => Math.max(1, value - 1))}
            className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-1.5 text-xs font-medium text-[var(--color-text-primary)] disabled:opacity-50"
          >
            Previous
          </button>
          <button
            type="button"
            disabled={!auditQuery.data?.hasNextPage}
            onClick={() => setAuditPage((value) => value + 1)}
            className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-1.5 text-xs font-medium text-[var(--color-text-primary)] disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
    </section>
  )
}

function DiscoveryRow({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3">
      <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</div>
      <div className={['mt-1 text-sm text-[var(--color-text-primary)]', mono ? 'break-all font-mono text-xs' : ''].join(' ')}>
        {value}
      </div>
    </div>
  )
}
