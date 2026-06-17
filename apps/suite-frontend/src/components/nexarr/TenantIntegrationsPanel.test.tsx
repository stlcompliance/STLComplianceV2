import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { ReactElement } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import type {
  PagedResult,
  TenantIntegrationCatalogResponse,
  TenantIntegrationConnectionResponse,
  TenantIntegrationSyncRunResponse,
} from '../../api/types'
import { TenantIntegrationsPanel } from './TenantIntegrationsPanel'

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({
    me: {
      userId: 'user-1',
      email: 'alex@example.com',
      displayName: 'Alex Operator',
      isPlatformAdmin: false,
      tenantId: 'tenant-a',
      tenantSlug: 'alpha',
      tenantDisplayName: 'Alpha Corp',
      entitlements: ['nexarr'],
    },
  }),
}))

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    getTenantIntegrationCatalog: vi.fn(),
    listTenantIntegrations: vi.fn(),
    getTenantIntegration: vi.fn(),
    listTenantIntegrationSyncRuns: vi.fn(),
    listTenantIntegrationMappings: vi.fn(),
    upsertTenantIntegration: vi.fn(),
    upsertTenantIntegrationMapping: vi.fn(),
  }
})

function brand(mark: string, accentColor: string) {
  return {
    mark,
    accentColor,
    backgroundColor: '#0F172A',
    textColor: '#F8FAFC',
    websiteUrl: 'https://example.com',
    assetSourceUrl: 'https://example.com/brand',
    assetSourceLabel: `${mark} brand source`,
    usageNote: 'Vendor-owned trademark metadata.',
  }
}

const catalog: TenantIntegrationCatalogResponse = {
  providers: [
    {
      providerKey: 'quickbooks',
      displayName: 'QuickBooks',
      category: 'Finance / ERP',
      brand: brand('QB', '#2CA01C'),
      connectorFamily: 'oauth2_api',
      authType: 'OAuth2',
      defaultDirection: 'read_only',
      supportsWriteback: true,
      requiresManualMapping: false,
      owningProducts: ['supplyarr', 'customarr', 'ordarr', 'reportarr'],
      capabilities: ['customers', 'vendors'],
      routes: [
        {
          routeKey: 'api_config',
          method: 'GET/PUT',
          path: '/api/v1/tenants/{tenantId}/integrations/quickbooks',
          description: 'Tenant-scoped integration configuration.',
        },
      ],
    },
    {
      providerKey: 'edi-x12',
      displayName: 'EDI X12',
      category: 'Generic Protocols',
      brand: brand('X12', '#38BDF8'),
      connectorFamily: 'edi_x12',
      authType: 'AS2/SFTP',
      defaultDirection: 'inbound',
      supportsWriteback: true,
      requiresManualMapping: true,
      owningProducts: ['ordarr', 'loadarr', 'routarr', 'supplyarr'],
      capabilities: ['x12_204'],
      routes: [],
    },
  ],
}

const quickbooksConnection: TenantIntegrationConnectionResponse = {
  connectionId: 'connection-1',
  tenantId: 'tenant-a',
  tenantSlug: 'alpha',
  tenantDisplayName: 'Alpha Corp',
  providerKey: 'quickbooks',
  providerDisplayName: 'QuickBooks',
  category: 'Finance / ERP',
  brand: catalog.providers[0].brand,
  status: 'configured',
  syncDirection: 'read_only',
  writebacksEnabled: false,
  manualMappingRequired: false,
  configurationJson: '{}',
  lastSuccessfulSyncAt: null,
  lastFailedSyncAt: null,
  lastErrorCategory: null,
  lastErrorMessage: null,
  credential: {
    credentialId: 'credential-1',
    credentialKind: 'oauth2',
    redactedLabel: 'QuickBooks production (****1234)',
    encryptionKeyId: 'key-1',
    expiresAt: null,
    lastValidatedAt: null,
    updatedAt: '2026-06-17T00:00:00Z',
  },
  health: {
    status: 'healthy',
    checkedAt: '2026-06-17T00:00:00Z',
    latencyMs: null,
    errorCategory: null,
    errorMessage: null,
  },
  latestSyncRun: null,
  routes: catalog.providers[0].routes,
  createdAt: '2026-06-17T00:00:00Z',
  updatedAt: '2026-06-17T00:00:00Z',
}

const emptyPage: PagedResult<TenantIntegrationConnectionResponse> = {
  items: [quickbooksConnection],
  page: 1,
  pageSize: 100,
  totalCount: 1,
  hasNextPage: false,
}

const syncRuns: TenantIntegrationSyncRunResponse[] = [
  {
    syncRunId: 'sync-1',
    tenantId: 'tenant-a',
    connectionId: 'connection-1',
    providerKey: 'quickbooks',
    status: 'failed',
    direction: 'read_only',
    triggeredBy: 'manual',
    attemptCount: 1,
    startedAt: '2026-06-17T00:00:00Z',
    completedAt: '2026-06-17T00:00:01Z',
    nextRetryAt: null,
    snapshotCount: 0,
    mappingCount: 0,
    errorCategory: 'credentials_missing',
    errorMessage: 'Credentials are required.',
    destinationProductsJson: '[]',
    resultSummaryJson: '{}',
  },
]

function renderPanel(ui: ReactElement) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <MemoryRouter>
      <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>
    </MemoryRouter>,
  )
}

describe('TenantIntegrationsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders the provider catalog and tenant connection status', async () => {
    vi.mocked(nexarr.getTenantIntegrationCatalog).mockResolvedValue(catalog)
    vi.mocked(nexarr.listTenantIntegrations).mockResolvedValue(emptyPage)

    renderPanel(<TenantIntegrationsPanel />)

    expect(screen.getByRole('heading', { name: 'NexArr integrations' })).toBeInTheDocument()
    expect(await screen.findByText('QuickBooks')).toBeInTheDocument()
    expect(screen.getByText('EDI X12')).toBeInTheDocument()
    expect(screen.getAllByText('Configured').length).toBeGreaterThanOrEqual(1)
  })

  it('shows redacted credentials and requires writeback confirmation', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.getTenantIntegrationCatalog).mockResolvedValue(catalog)
    vi.mocked(nexarr.listTenantIntegrations).mockResolvedValue(emptyPage)
    vi.mocked(nexarr.getTenantIntegration).mockResolvedValue(quickbooksConnection)
    vi.mocked(nexarr.listTenantIntegrationSyncRuns).mockResolvedValue(syncRuns)

    renderPanel(<TenantIntegrationsPanel providerKey="quickbooks" mode="detail" />)

    expect(await screen.findByText(/QuickBooks production \(\*\*\*\*1234\)/)).toBeInTheDocument()
    expect(screen.queryByText('super-secret-token-1234')).not.toBeInTheDocument()

    const saveButton = screen.getByRole('button', { name: 'Save configuration' })
    expect(saveButton).toBeEnabled()

    await user.click(screen.getByLabelText('Enable audited writebacks'))
    await waitFor(() => expect(saveButton).toBeDisabled())

    await user.click(
      screen.getByLabelText(
        'I reviewed the impact preview and accept idempotent audited writebacks for this provider.',
      ),
    )
    await waitFor(() => expect(saveButton).toBeEnabled())
  })

  it('renders a manual mapping workspace for mapping providers', async () => {
    vi.mocked(nexarr.getTenantIntegrationCatalog).mockResolvedValue(catalog)
    vi.mocked(nexarr.listTenantIntegrations).mockResolvedValue(emptyPage)
    vi.mocked(nexarr.listTenantIntegrationMappings).mockResolvedValue([])

    renderPanel(<TenantIntegrationsPanel providerKey="edi-x12" mode="mappings" />)

    expect(await screen.findByRole('heading', { name: 'EDI X12 mappings' })).toBeInTheDocument()
    expect(screen.getByLabelText('Template name')).toHaveValue('default')
    expect(screen.getByRole('button', { name: 'Save mapping' })).toBeEnabled()
  })
})
