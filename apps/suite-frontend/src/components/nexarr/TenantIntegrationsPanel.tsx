import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ExternalLink, KeyRound, RefreshCw, Settings2, Shuffle, TestTube2 } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import type {
  TenantIntegrationConnectionResponse,
  TenantIntegrationMappingTemplateResponse,
  TenantIntegrationProviderResponse,
} from '../../api/types'
import { useAuth } from '../../auth/AuthProvider'
import { IntegrationBrandMark } from './IntegrationBrandMark'

const CONNECTION_STATUSES = ['not_configured', 'configured', 'connected', 'degraded', 'disabled', 'needs_review']
const SYNC_DIRECTIONS = ['read_only', 'inbound', 'outbound', 'bidirectional', 'writeback']
const TARGET_PRODUCTS = [
  'nexarr',
  'staffarr',
  'supplyarr',
  'customarr',
  'ordarr',
  'loadarr',
  'routarr',
  'maintainarr',
  'recordarr',
  'reportarr',
  'compliancecore',
]

type TenantIntegrationsPanelProps = {
  providerKey?: string
  mode?: 'list' | 'detail' | 'mappings'
}

function pretty(value: string): string {
  return value
    .replace(/[_-]/g, ' ')
    .replace(/\b\w/g, (match) => match.toUpperCase())
}

function statusTone(status: string): string {
  const normalized = status.trim().toLowerCase()
  if (normalized === 'connected' || normalized === 'succeeded') {
    return 'success'
  }
  if (normalized === 'configured' || normalized === 'queued' || normalized === 'running') {
    return 'info'
  }
  if (normalized === 'degraded' || normalized === 'needs_review' || normalized === 'source_unavailable') {
    return 'needs_review'
  }
  if (normalized === 'failed' || normalized === 'dead_letter') {
    return 'danger'
  }
  if (normalized === 'disabled' || normalized === 'not_configured') {
    return 'inactive'
  }
  return 'neutral'
}

function formatDate(value: string | null | undefined): string {
  return value ? new Date(value).toLocaleString() : 'None'
}

function validateJson(value: string): string | null {
  try {
    JSON.parse(value || '{}')
    return null
  } catch (error) {
    return error instanceof Error ? error.message : 'Invalid JSON'
  }
}

function byProviderKey(
  connections: readonly TenantIntegrationConnectionResponse[],
): Map<string, TenantIntegrationConnectionResponse> {
  return new Map(connections.map((connection) => [connection.providerKey, connection]))
}

function filterProviders(
  providers: readonly TenantIntegrationProviderResponse[],
  search: string,
  category: string,
): TenantIntegrationProviderResponse[] {
  const normalizedSearch = search.trim().toLowerCase()
  return providers.filter((provider) => {
    const matchesCategory = category === 'all' || provider.category === category
    const haystack = [
      provider.displayName,
      provider.providerKey,
      provider.category,
      provider.connectorFamily,
      provider.authType,
      ...provider.capabilities,
      ...provider.owningProducts,
    ]
      .join(' ')
      .toLowerCase()
    return matchesCategory && (!normalizedSearch || haystack.includes(normalizedSearch))
  })
}

export function TenantIntegrationsPanel({
  providerKey,
  mode = 'list',
}: TenantIntegrationsPanelProps) {
  const { me } = useAuth()
  const tenantId = me?.tenantId ?? ''

  const catalogQuery = useQuery({
    queryKey: ['tenant-integration-catalog'],
    queryFn: () => nexarr.getTenantIntegrationCatalog(),
    enabled: Boolean(me),
  })

  const connectionsQuery = useQuery({
    queryKey: ['tenant-integrations', tenantId],
    queryFn: () => nexarr.listTenantIntegrations(tenantId, { pageSize: 250 }),
    enabled: Boolean(tenantId),
  })

  if (!me) {
    return <p className="text-sm text-slate-400">Loading integration workspace...</p>
  }

  if (catalogQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(catalogQuery.error, 'Failed to load integration catalog.')}
        onRetry={() => void catalogQuery.refetch()}
        retryLabel="Retry catalog"
      />
    )
  }

  if (connectionsQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(connectionsQuery.error, 'Failed to load tenant integrations.')}
        onRetry={() => void connectionsQuery.refetch()}
        retryLabel="Retry integrations"
      />
    )
  }

  const providers = catalogQuery.data?.providers ?? []
  const provider = providerKey
    ? providers.find((candidate) => candidate.providerKey === providerKey)
    : undefined

  if (providerKey && catalogQuery.isSuccess && !provider) {
    return (
      <ApiErrorCallout
        message={`Integration provider "${providerKey}" is not registered in the NexArr catalog.`}
      />
    )
  }

  if (provider && mode === 'mappings') {
    return <TenantIntegrationMappings tenantId={tenantId} provider={provider} />
  }

  if (provider) {
    return <TenantIntegrationDetail tenantId={tenantId} provider={provider} />
  }

  return (
    <TenantIntegrationList
      providers={providers}
      connections={connectionsQuery.data?.items ?? []}
      isLoading={catalogQuery.isLoading || connectionsQuery.isLoading}
    />
  )
}

function TenantIntegrationList({
  providers,
  connections,
  isLoading,
}: {
  providers: TenantIntegrationProviderResponse[]
  connections: TenantIntegrationConnectionResponse[]
  isLoading: boolean
}) {
  const [category, setCategory] = useState('all')
  const [search, setSearch] = useState('')
  const connectionMap = useMemo(() => byProviderKey(connections), [connections])
  const categories = useMemo(
    () => ['all', ...Array.from(new Set(providers.map((provider) => provider.category))).sort()],
    [providers],
  )
  const filteredProviders = useMemo(
    () => filterProviders(providers, search, category),
    [providers, search, category],
  )
  const configuredCount = connections.filter(
    (connection) => connection.status !== 'not_configured',
  ).length
  const writebackCount = connections.filter((connection) => connection.writebacksEnabled).length
  const issueCount = connections.filter((connection) =>
    ['degraded', 'needs_review'].includes(connection.status),
  ).length

  return (
    <div className="max-w-7xl space-y-5" data-testid="tenant-integrations-panel">
      <header>
        <h3 className="text-xl font-semibold text-white">NexArr integrations</h3>
        <p className="mt-1 text-sm text-slate-400">
          Tenant-scoped connector catalog, credentials, mappings, health, and sync routing.
        </p>
      </header>

      <div className="grid gap-3 sm:grid-cols-4">
        <Metric label="Catalog providers" value={providers.length} />
        <Metric label="Configured" value={configuredCount} tone="emerald" />
        <Metric label="Needs attention" value={issueCount} tone="amber" />
        <Metric label="Writebacks on" value={writebackCount} tone={writebackCount ? 'amber' : 'slate'} />
      </div>

      <div className="flex flex-col gap-3 rounded-lg border border-slate-800 bg-slate-950/40 p-3 sm:flex-row">
        <label className="block flex-1 text-sm text-slate-300">
          Search
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            placeholder="Provider, capability, owner"
          />
        </label>
        <label className="block text-sm text-slate-300 sm:w-72">
          Category
          <select
            value={category}
            onChange={(event) => setCategory(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          >
            {categories.map((item) => (
              <option key={item} value={item}>
                {item === 'all' ? 'All categories' : item}
              </option>
            ))}
          </select>
        </label>
      </div>

      {isLoading ? (
        <p className="text-sm text-slate-400">Loading integrations...</p>
      ) : (
        <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
          {filteredProviders.map((provider) => {
            const connection = connectionMap.get(provider.providerKey)
            return (
              <Link
                key={provider.providerKey}
                to={`/app/nexarr/integrations/${provider.providerKey}`}
                className="rounded-lg border border-slate-800 bg-slate-900/60 p-4 transition hover:border-teal-600/70 hover:bg-slate-900"
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="flex min-w-0 items-start gap-3">
                    <IntegrationBrandMark brand={provider.brand} label={provider.displayName} />
                    <div className="min-w-0">
                      <p className="truncate text-sm font-semibold text-white">{provider.displayName}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">{provider.category}</p>
                      <p className="mt-1 truncate text-xs text-[var(--color-text-muted)]">{provider.brand.assetSourceLabel}</p>
                    </div>
                  </div>
                  <span
                    className="stl-tone-badge shrink-0 rounded-full border px-2 py-0.5 text-xs font-medium"
                    data-tone={statusTone(connection?.status ?? 'not_configured')}
                  >
                    {pretty(connection?.status ?? 'not_configured')}
                  </span>
                </div>
                <div className="mt-3 flex flex-wrap gap-1.5">
                  {provider.owningProducts.slice(0, 4).map((product) => (
                    <span key={product} className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-300">
                      {product}
                    </span>
                  ))}
                </div>
                <div className="mt-3 grid grid-cols-2 gap-2 text-xs text-slate-400">
                  <span>Auth: {provider.authType}</span>
                  <span>Sync: {pretty(provider.defaultDirection)}</span>
                  <span>{provider.requiresManualMapping ? 'Manual mapping' : 'Adapter mapped'}</span>
                  <span>{provider.supportsWriteback ? 'Writeback capable' : 'Read-first only'}</span>
                </div>
              </Link>
            )
          })}
        </div>
      )}
    </div>
  )
}

function TenantIntegrationDetail({
  tenantId,
  provider,
}: {
  tenantId: string
  provider: TenantIntegrationProviderResponse
}) {
  const queryClient = useQueryClient()
  const [status, setStatus] = useState('not_configured')
  const [syncDirection, setSyncDirection] = useState(provider.defaultDirection)
  const [writebacksEnabled, setWritebacksEnabled] = useState(false)
  const [manualMappingRequired, setManualMappingRequired] = useState(provider.requiresManualMapping)
  const [configurationJson, setConfigurationJson] = useState('{}')
  const [writebackConfirmed, setWritebackConfirmed] = useState(false)
  const [credentialKind, setCredentialKind] = useState(provider.authType)
  const [secretLabel, setSecretLabel] = useState('')
  const [credentialExpiresAt, setCredentialExpiresAt] = useState('')
  const [credentialPayloadJson, setCredentialPayloadJson] = useState('{\n  "apiKey": ""\n}')
  const [message, setMessage] = useState<string | null>(null)
  const [formError, setFormError] = useState<string | null>(null)

  const detailQuery = useQuery({
    queryKey: ['tenant-integration', tenantId, provider.providerKey],
    queryFn: () => nexarr.getTenantIntegration(tenantId, provider.providerKey),
    enabled: Boolean(tenantId),
  })

  const syncRunsQuery = useQuery({
    queryKey: ['tenant-integration-sync-runs', tenantId, provider.providerKey],
    queryFn: () => nexarr.listTenantIntegrationSyncRuns(tenantId, provider.providerKey, 8),
    enabled: Boolean(tenantId),
  })

  const connection = detailQuery.data

  useEffect(() => {
    if (!connection) {
      return
    }
    setStatus(connection.status)
    setSyncDirection(connection.syncDirection)
    setWritebacksEnabled(connection.writebacksEnabled)
    setManualMappingRequired(connection.manualMappingRequired)
    setConfigurationJson(connection.configurationJson || '{}')
    setWritebackConfirmed(connection.writebacksEnabled)
    setCredentialKind(connection.credential?.credentialKind ?? provider.authType)
    setSecretLabel(connection.credential?.redactedLabel ?? '')
  }, [connection, provider.authType])

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['tenant-integration', tenantId, provider.providerKey] })
    void queryClient.invalidateQueries({ queryKey: ['tenant-integrations', tenantId] })
    void queryClient.invalidateQueries({ queryKey: ['tenant-integration-sync-runs', tenantId, provider.providerKey] })
  }

  const saveMutation = useMutation({
    mutationFn: () =>
      nexarr.upsertTenantIntegration(tenantId, provider.providerKey, {
        status,
        syncDirection,
        writebacksEnabled,
        manualMappingRequired,
        configurationJson: configurationJson.trim() || '{}',
      }),
    onSuccess: () => {
      setMessage('Integration configuration saved.')
      setFormError(null)
      invalidate()
    },
    onError: (error: Error) => setFormError(error.message),
  })

  const credentialMutation = useMutation({
    mutationFn: () => {
      const parsed = JSON.parse(credentialPayloadJson || '{}') as Record<string, string>
      return nexarr.upsertTenantIntegrationCredential(tenantId, provider.providerKey, {
        credentialKind: credentialKind.trim() || provider.authType,
        secretLabel: secretLabel.trim() || `${provider.displayName} credential`,
        payload: parsed,
        expiresAt: credentialExpiresAt ? new Date(credentialExpiresAt).toISOString() : null,
      })
    },
    onSuccess: () => {
      setMessage('Credential encrypted and saved.')
      setFormError(null)
      setCredentialPayloadJson('{\n  "apiKey": ""\n}')
      invalidate()
    },
    onError: (error: Error) => setFormError(error.message),
  })

  const deleteCredentialMutation = useMutation({
    mutationFn: () => nexarr.deleteTenantIntegrationCredential(tenantId, provider.providerKey),
    onSuccess: () => {
      setMessage('Credential removed.')
      setFormError(null)
      invalidate()
    },
    onError: (error: Error) => setFormError(error.message),
  })

  const testMutation = useMutation({
    mutationFn: () => nexarr.testTenantIntegration(tenantId, provider.providerKey),
    onSuccess: (result) => {
      setMessage(`Connection test ${pretty(result.status).toLowerCase()}.`)
      setFormError(null)
      invalidate()
    },
    onError: (error: Error) => setFormError(error.message),
  })

  const syncMutation = useMutation({
    mutationFn: () =>
      nexarr.triggerTenantIntegrationSync(tenantId, provider.providerKey, {
        idempotencyKey: `suite-${provider.providerKey}-${Date.now()}`,
        force: true,
      }),
    onSuccess: () => {
      setMessage('Sync run queued.')
      setFormError(null)
      invalidate()
    },
    onError: (error: Error) => setFormError(error.message),
  })

  const configJsonError = validateJson(configurationJson)
  let credentialJsonError: string | null = null
  try {
    const parsed = JSON.parse(credentialPayloadJson || '{}') as Record<string, unknown>
    if (Object.values(parsed).some((value) => typeof value !== 'string')) {
      credentialJsonError = 'Credential payload values must be strings.'
    }
  } catch (error) {
    credentialJsonError = error instanceof Error ? error.message : 'Invalid JSON'
  }
  const enablingWriteback = writebacksEnabled && !connection?.writebacksEnabled
  const canSave = !configJsonError && (!enablingWriteback || writebackConfirmed)

  if (detailQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading {provider.displayName}...</p>
  }

  if (detailQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(detailQuery.error, `Failed to load ${provider.displayName}.`)}
        onRetry={() => void detailQuery.refetch()}
        retryLabel="Retry connection"
      />
    )
  }

  return (
    <div className="max-w-7xl space-y-5" data-testid="tenant-integration-detail">
      <header className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <Link to="/app/nexarr/integrations" className="text-sm text-teal-300 hover:text-teal-200">
            Back to integrations
          </Link>
          <div className="mt-2 flex items-start gap-3">
            <IntegrationBrandMark brand={provider.brand} label={provider.displayName} size="lg" />
            <div>
              <h3 className="text-xl font-semibold text-white">{provider.displayName}</h3>
              <p className="mt-1 text-sm text-slate-400">
                {provider.category} connector for {provider.owningProducts.join(', ')}.
              </p>
              <a
                href={provider.brand.assetSourceUrl}
                target="_blank"
                rel="noreferrer"
                className="mt-2 inline-flex items-center gap-1 text-xs text-slate-400 hover:text-teal-200"
              >
                {provider.brand.assetSourceLabel}
                <ExternalLink className="h-3 w-3" aria-hidden />
              </a>
            </div>
          </div>
        </div>
        <Link
          to={`/app/nexarr/integrations/${provider.providerKey}/mappings`}
          className="inline-flex items-center gap-2 rounded-md border border-teal-600 px-3 py-2 text-sm font-medium text-teal-200 hover:bg-teal-600/10"
        >
          <Shuffle className="h-4 w-4" aria-hidden />
          Mappings
        </Link>
      </header>

      <div className="grid gap-3 md:grid-cols-4">
        <Metric label="Status" value={pretty(connection?.status ?? 'not_configured')} />
        <Metric label="Direction" value={pretty(connection?.syncDirection ?? provider.defaultDirection)} />
        <Metric label="Credential" value={connection?.credential ? 'Encrypted' : 'Missing'} />
        <Metric label="Health" value={pretty(connection?.health?.status ?? 'untested')} />
      </div>

      {message ? <p className="text-sm text-emerald-300">{message}</p> : null}
      {formError ? <p className="text-sm text-rose-300">{formError}</p> : null}

      <section className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
        <div className="flex items-center gap-2">
          <Settings2 className="h-4 w-4 text-slate-400" aria-hidden />
          <h4 className="text-sm font-semibold text-white">Connection configuration</h4>
        </div>
        <div className="mt-4 grid gap-3 md:grid-cols-2">
          <label className="block text-sm text-slate-300">
            Status
            <select
              value={status}
              onChange={(event) => setStatus(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              {CONNECTION_STATUSES.map((item) => (
                <option key={item} value={item}>
                  {pretty(item)}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300">
            Sync direction
            <select
              value={syncDirection}
              onChange={(event) => setSyncDirection(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              {SYNC_DIRECTIONS.map((item) => (
                <option key={item} value={item}>
                  {pretty(item)}
                </option>
              ))}
            </select>
          </label>
          <label className="flex items-center gap-2 text-sm text-slate-300">
            <input
              type="checkbox"
              checked={manualMappingRequired}
              onChange={(event) => setManualMappingRequired(event.target.checked)}
              className="h-4 w-4 rounded border-slate-600 bg-slate-950 text-teal-500"
            />
            Manual mapping required
          </label>
          <label className="flex items-center gap-2 text-sm text-slate-300">
            <input
              type="checkbox"
              checked={writebacksEnabled}
              disabled={!provider.supportsWriteback}
              onChange={(event) => setWritebacksEnabled(event.target.checked)}
              className="h-4 w-4 rounded border-slate-600 bg-slate-950 text-teal-500 disabled:opacity-50"
            />
            Enable audited writebacks
          </label>
        </div>
        {enablingWriteback ? (
          <label className="mt-3 flex items-start gap-2 rounded-md border border-amber-800 bg-amber-950/30 p-3 text-sm text-amber-100">
            <input
              type="checkbox"
              checked={writebackConfirmed}
              onChange={(event) => setWritebackConfirmed(event.target.checked)}
              className="mt-0.5 h-4 w-4 rounded border-amber-700 bg-slate-950 text-amber-500"
            />
            I reviewed the impact preview and accept idempotent audited writebacks for this provider.
          </label>
        ) : null}
        <label className="mt-3 block text-sm text-slate-300">
          Configuration JSON
          <textarea
            value={configurationJson}
            onChange={(event) => setConfigurationJson(event.target.value)}
            rows={8}
            spellCheck={false}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs text-slate-100"
          />
        </label>
        {configJsonError ? <p className="mt-2 text-xs text-rose-300">{configJsonError}</p> : null}
        <div className="mt-3 flex flex-wrap gap-2">
          <button
            type="button"
            disabled={!canSave || saveMutation.isPending}
            onClick={() => saveMutation.mutate()}
            className="rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
          >
            {saveMutation.isPending ? 'Saving...' : 'Save configuration'}
          </button>
          <button
            type="button"
            disabled={testMutation.isPending}
            onClick={() => testMutation.mutate()}
            className="inline-flex items-center gap-2 rounded-md border border-slate-600 px-4 py-2 text-sm font-medium text-slate-200 hover:bg-slate-800 disabled:opacity-50"
          >
            <TestTube2 className="h-4 w-4" aria-hidden />
            {testMutation.isPending ? 'Testing...' : 'Test'}
          </button>
          <button
            type="button"
            disabled={syncMutation.isPending}
            onClick={() => syncMutation.mutate()}
            className="inline-flex items-center gap-2 rounded-md border border-slate-600 px-4 py-2 text-sm font-medium text-slate-200 hover:bg-slate-800 disabled:opacity-50"
          >
            <RefreshCw className="h-4 w-4" aria-hidden />
            {syncMutation.isPending ? 'Queueing...' : 'Queue sync'}
          </button>
        </div>
      </section>

      <section className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
        <div className="flex items-center gap-2">
          <KeyRound className="h-4 w-4 text-slate-400" aria-hidden />
          <h4 className="text-sm font-semibold text-white">Encrypted credential</h4>
        </div>
        {connection?.credential ? (
          <div className="mt-3 grid gap-2 rounded-md border border-slate-800 bg-slate-950/50 p-3 text-sm text-slate-300 md:grid-cols-3">
            <span>Label: {connection.credential.redactedLabel}</span>
            <span>Key: {connection.credential.encryptionKeyId}</span>
            <span>Expires: {formatDate(connection.credential.expiresAt)}</span>
            <span>Validated: {formatDate(connection.credential.lastValidatedAt)}</span>
            <span>Updated: {formatDate(connection.credential.updatedAt)}</span>
          </div>
        ) : (
          <p className="mt-3 text-sm text-amber-300">No credential is stored for this tenant provider.</p>
        )}
        <div className="mt-4 grid gap-3 md:grid-cols-3">
          <label className="block text-sm text-slate-300">
            Credential kind
            <input
              value={credentialKind}
              onChange={(event) => setCredentialKind(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-sm text-slate-300">
            Secret label
            <input
              value={secretLabel}
              onChange={(event) => setSecretLabel(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-sm text-slate-300">
            Expires
            <input
              type="datetime-local"
              value={credentialExpiresAt}
              onChange={(event) => setCredentialExpiresAt(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
        </div>
        <label className="mt-3 block text-sm text-slate-300">
          Credential payload JSON
          <textarea
            value={credentialPayloadJson}
            onChange={(event) => setCredentialPayloadJson(event.target.value)}
            rows={6}
            spellCheck={false}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs text-slate-100"
          />
        </label>
        {credentialJsonError ? <p className="mt-2 text-xs text-rose-300">{credentialJsonError}</p> : null}
        <div className="mt-3 flex flex-wrap gap-2">
          <button
            type="button"
            disabled={Boolean(credentialJsonError) || credentialMutation.isPending}
            onClick={() => credentialMutation.mutate()}
            className="rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
          >
            {credentialMutation.isPending ? 'Saving...' : 'Encrypt and save credential'}
          </button>
          <button
            type="button"
            disabled={!connection?.credential || deleteCredentialMutation.isPending}
            onClick={() => deleteCredentialMutation.mutate()}
            className="rounded-md border border-rose-700 px-4 py-2 text-sm font-medium text-rose-200 hover:bg-rose-950/40 disabled:opacity-50"
          >
            Remove credential
          </button>
        </div>
      </section>

      <section className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
        <h4 className="text-sm font-semibold text-white">Routes and recent sync runs</h4>
        <div className="mt-3 grid gap-4 xl:grid-cols-2">
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-800 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-3 py-2 font-medium">Route</th>
                  <th className="px-3 py-2 font-medium">Method</th>
                  <th className="px-3 py-2 font-medium">Path</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800">
                {provider.routes.map((route) => (
                  <tr key={route.routeKey}>
                    <td className="px-3 py-2 text-slate-300">{route.routeKey}</td>
                    <td className="px-3 py-2 text-slate-400">{route.method}</td>
                    <td className="px-3 py-2 font-mono text-xs text-slate-400">{route.path}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-800 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-3 py-2 font-medium">Status</th>
                  <th className="px-3 py-2 font-medium">Started</th>
                  <th className="px-3 py-2 font-medium">Output</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800">
                {(syncRunsQuery.data ?? []).map((run) => (
                  <tr key={run.syncRunId}>
                    <td className="px-3 py-2">
                      <span
                        className="stl-tone-badge rounded-full border px-2 py-0.5 text-xs font-medium"
                        data-tone={statusTone(run.status)}
                      >
                        {pretty(run.status)}
                      </span>
                    </td>
                    <td className="px-3 py-2 text-slate-400">{formatDate(run.startedAt)}</td>
                    <td className="px-3 py-2 text-slate-300">
                      {run.snapshotCount} snapshots / {run.mappingCount} mappings
                    </td>
                  </tr>
                ))}
                {syncRunsQuery.data?.length === 0 ? (
                  <tr>
                    <td className="px-3 py-3 text-[var(--color-text-muted)]" colSpan={3}>
                      No sync runs for this provider.
                    </td>
                  </tr>
                ) : null}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    </div>
  )
}

function TenantIntegrationMappings({
  tenantId,
  provider,
}: {
  tenantId: string
  provider: TenantIntegrationProviderResponse
}) {
  const queryClient = useQueryClient()
  const [templateName, setTemplateName] = useState('default')
  const [sourceEntityType, setSourceEntityType] = useState(provider.capabilities[0] ?? 'external_record')
  const [targetProductKey, setTargetProductKey] = useState(provider.owningProducts[0] ?? 'nexarr')
  const [targetEntityType, setTargetEntityType] = useState('snapshot')
  const [mappingJson, setMappingJson] = useState('{\n  "fields": []\n}')
  const [isActive, setIsActive] = useState(true)
  const [formError, setFormError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)

  const mappingsQuery = useQuery({
    queryKey: ['tenant-integration-mappings', tenantId, provider.providerKey],
    queryFn: () => nexarr.listTenantIntegrationMappings(tenantId, provider.providerKey),
    enabled: Boolean(tenantId),
  })

  const mappingJsonError = validateJson(mappingJson)

  const saveMutation = useMutation({
    mutationFn: () =>
      nexarr.upsertTenantIntegrationMapping(tenantId, provider.providerKey, {
        templateName: templateName.trim(),
        sourceEntityType: sourceEntityType.trim(),
        targetProductKey,
        targetEntityType: targetEntityType.trim(),
        mappingJson: mappingJson.trim() || '{}',
        isActive,
      }),
    onSuccess: () => {
      setMessage('Mapping template saved.')
      setFormError(null)
      void queryClient.invalidateQueries({ queryKey: ['tenant-integration-mappings', tenantId, provider.providerKey] })
    },
    onError: (error: Error) => setFormError(error.message),
  })

  const deleteMutation = useMutation({
    mutationFn: (mappingTemplateId: string) =>
      nexarr.deleteTenantIntegrationMapping(tenantId, provider.providerKey, mappingTemplateId),
    onSuccess: () => {
      setMessage('Mapping template deleted.')
      setFormError(null)
      void queryClient.invalidateQueries({ queryKey: ['tenant-integration-mappings', tenantId, provider.providerKey] })
    },
    onError: (error: Error) => setFormError(error.message),
  })

  const loadMapping = (mapping: TenantIntegrationMappingTemplateResponse) => {
    setTemplateName(mapping.templateName)
    setSourceEntityType(mapping.sourceEntityType)
    setTargetProductKey(mapping.targetProductKey)
    setTargetEntityType(mapping.targetEntityType)
    setMappingJson(mapping.mappingJson || '{}')
    setIsActive(mapping.isActive)
  }

  return (
    <div className="max-w-7xl space-y-5" data-testid="tenant-integration-mappings">
      <header>
        <Link
          to={`/app/nexarr/integrations/${provider.providerKey}`}
          className="text-sm text-teal-300 hover:text-teal-200"
        >
          Back to {provider.displayName}
        </Link>
        <div className="mt-2 flex items-start gap-3">
          <IntegrationBrandMark brand={provider.brand} label={provider.displayName} size="lg" />
          <div>
            <h3 className="text-xl font-semibold text-white">{provider.displayName} mappings</h3>
            <p className="mt-1 text-sm text-slate-400">
              Tenant-specific entity, field, folder, role, GL, carrier account, and cost-center mapping.
            </p>
          </div>
        </div>
      </header>

      {mappingsQuery.isError ? (
        <ApiErrorCallout
          message={getErrorMessage(mappingsQuery.error, 'Failed to load mapping templates.')}
          onRetry={() => void mappingsQuery.refetch()}
          retryLabel="Retry mappings"
        />
      ) : null}

      {message ? <p className="text-sm text-emerald-300">{message}</p> : null}
      {formError ? <p className="text-sm text-rose-300">{formError}</p> : null}

      <section className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
        <h4 className="text-sm font-semibold text-white">Mapping template</h4>
        <div className="mt-3 grid gap-3 md:grid-cols-2">
          <label className="block text-sm text-slate-300">
            Template name
            <input
              value={templateName}
              onChange={(event) => setTemplateName(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-sm text-slate-300">
            Source entity
            <input
              value={sourceEntityType}
              onChange={(event) => setSourceEntityType(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-sm text-slate-300">
            Target product
            <select
              value={targetProductKey}
              onChange={(event) => setTargetProductKey(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              {TARGET_PRODUCTS.map((product) => (
                <option key={product} value={product}>
                  {product}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300">
            Target entity
            <input
              value={targetEntityType}
              onChange={(event) => setTargetEntityType(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
        </div>
        <label className="mt-3 block text-sm text-slate-300">
          Mapping JSON
          <textarea
            value={mappingJson}
            onChange={(event) => setMappingJson(event.target.value)}
            rows={10}
            spellCheck={false}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs text-slate-100"
          />
        </label>
        {mappingJsonError ? <p className="mt-2 text-xs text-rose-300">{mappingJsonError}</p> : null}
        <label className="mt-3 flex items-center gap-2 text-sm text-slate-300">
          <input
            type="checkbox"
            checked={isActive}
            onChange={(event) => setIsActive(event.target.checked)}
            className="h-4 w-4 rounded border-slate-600 bg-slate-950 text-teal-500"
          />
          Enabled
        </label>
        <button
          type="button"
          disabled={!templateName.trim() || !sourceEntityType.trim() || !targetEntityType.trim() || Boolean(mappingJsonError) || saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          className="mt-3 rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
        >
          {saveMutation.isPending ? 'Saving...' : 'Save mapping'}
        </button>
      </section>

      <section className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
        <h4 className="text-sm font-semibold text-white">Saved templates</h4>
        {mappingsQuery.isLoading ? (
          <p className="mt-3 text-sm text-slate-400">Loading mappings...</p>
        ) : (
          <div className="mt-3 overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-800 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-3 py-2 font-medium">Template</th>
                  <th className="px-3 py-2 font-medium">Source</th>
                  <th className="px-3 py-2 font-medium">Target</th>
                  <th className="px-3 py-2 font-medium">Status</th>
                  <th className="px-3 py-2 font-medium">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800">
                {(mappingsQuery.data ?? []).map((mapping) => (
                  <tr key={mapping.mappingTemplateId}>
                    <td className="px-3 py-2 text-slate-200">{mapping.templateName}</td>
                    <td className="px-3 py-2 text-slate-400">{mapping.sourceEntityType}</td>
                    <td className="px-3 py-2 text-slate-400">
                      {mapping.targetProductKey}:{mapping.targetEntityType}
                    </td>
                    <td className="px-3 py-2">
                      <span
                        className="stl-tone-badge rounded-full border px-2 py-0.5 text-xs font-medium"
                        data-tone={statusTone(mapping.isActive ? 'connected' : 'disabled')}
                      >
                        {mapping.isActive ? 'Enabled' : 'Disabled'}
                      </span>
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex flex-wrap gap-2">
                        <button
                          type="button"
                          onClick={() => loadMapping(mapping)}
                          className="rounded-md border border-slate-700 px-2 py-1 text-xs font-medium text-slate-200 hover:bg-slate-800"
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          disabled={deleteMutation.isPending}
                          onClick={() => deleteMutation.mutate(mapping.mappingTemplateId)}
                          className="rounded-md border border-rose-800 px-2 py-1 text-xs font-medium text-rose-200 hover:bg-rose-950/40 disabled:opacity-50"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
                {mappingsQuery.data?.length === 0 ? (
                  <tr>
                    <td className="px-3 py-3 text-[var(--color-text-muted)]" colSpan={5}>
                      No mapping templates saved for this provider.
                    </td>
                  </tr>
                ) : null}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}

function Metric({
  label,
  value,
  tone = 'slate',
}: {
  label: string
  value: string | number
  tone?: 'slate' | 'emerald' | 'amber'
}) {
  const valueTone = tone === 'emerald' ? 'success' : tone === 'amber' ? 'warning' : 'neutral'

  return (
    <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
      <p className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-[var(--tone-text)]" data-tone={valueTone}>{value}</p>
    </div>
  )
}
