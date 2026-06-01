import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import {
  buildSemanticKey,
  CheckboxMultiSelect,
  ControlledSelect,
  GeneratedKeyField,
  type PickerOption,
} from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import type { ServiceTokenIssueResult } from '../../api/types'
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
  const [issuedToken, setIssuedToken] = useState<ServiceTokenIssueResult | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const clientsQuery = useQuery({
    queryKey: ['platform-service-clients', clientsPage],
    queryFn: () => nexarr.listServiceClients(clientsPage, 25),
  })

  const tokensQuery = useQuery({
    queryKey: ['platform-service-tokens', tokensPage],
    queryFn: () => nexarr.listServiceTokens(undefined, tokensPage, 25),
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

  return (
    <section
      data-testid="service-token-admin-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Service token administration</h2>
        <p className="mt-1 text-sm text-slate-400">
          Register service clients and issue or revoke service tokens via NexArr{' '}
          <code className="text-xs">/api/service-tokens</code>. Issued bearer tokens are shown once.
        </p>
      </header>

      {errorMessage ? (
        <p className="text-sm text-rose-400" data-testid="service-token-admin-error">
          {errorMessage}
        </p>
      ) : null}

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Register service client</h3>
        <div className="mt-3 grid gap-3 sm:grid-cols-2">
          <label htmlFor="service-token-client-display-name" className="block text-sm text-slate-300">
            Service client display name
            <input
              id="service-token-client-display-name"
              value={clientDisplayName}
              onChange={(event) => {
                setClientDisplayName(event.target.value)
                setConfirmedClientKey(null)
              }}
              data-testid="service-token-client-display-name"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
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
          className="mt-3 rounded-md bg-indigo-700 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-600 disabled:opacity-50"
        >
          {registerMutation.isPending ? 'Registering…' : 'Register client'}
        </button>
      </div>

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Issue service token</h3>
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
          <label htmlFor="service-token-issue-scope" className="block text-sm text-slate-300">
            Token action scope (optional)
            <input
              id="service-token-issue-scope"
              value={issueActionScope}
              onChange={(event) => setIssueActionScope(event.target.value)}
              data-testid="service-token-issue-scope"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="service-token-issue-lifetime" className="block text-sm text-slate-300">
            Token lifetime (minutes)
            <input
              id="service-token-issue-lifetime"
              type="number"
              min={1}
              max={1440}
              value={issueLifetimeMinutes}
              onChange={(event) => setIssueLifetimeMinutes(event.target.value)}
              data-testid="service-token-issue-lifetime"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
        </div>
        <button
          type="button"
          onClick={() => issueMutation.mutate()}
          disabled={!issueClientId || issueMutation.isPending}
          data-testid="service-token-issue"
          className="mt-3 rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
        >
          {issueMutation.isPending ? 'Issuing…' : 'Issue token'}
        </button>

        {issuedToken ? (
          <div
            className="mt-4 rounded-md border border-amber-700/50 bg-amber-950/30 p-3 text-sm"
            data-testid="service-token-issued-result"
          >
            <p className="font-medium text-amber-200">Token issued — copy now; it will not be shown again.</p>
            <p className="mt-2 break-all font-mono text-xs text-slate-200">{issuedToken.accessToken}</p>
            <p className="mt-2 text-xs text-slate-400">
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
      />

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
    </section>
  )
}
