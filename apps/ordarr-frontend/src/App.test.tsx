import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import type { ReactNode } from 'react'
import { BrowserRouter, MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'
import { saveSession } from './auth/sessionStorage'
import * as client from './api/client'
import { OrderDetailPage } from './App'

vi.mock('@stl/shared-ui', () => ({
  ApiErrorCallout: ({ title, message }: { title: string; message: string }) => (
    <div>
      <strong>{title}</strong>
      <p>{message}</p>
    </div>
  ),
  ProductWorkspaceFrame: ({ children, productName }: { children: ReactNode; productName: string }) => (
    <div data-testid="workspace-frame">
      <h2>{productName}</h2>
      {children}
    </div>
  ),
  buildProductLaunchUrlMap: () => ({}),
  formatProductLaunchError: (error: unknown) => String(error),
  getErrorMessage: (error: unknown, fallback: string) => (error instanceof Error ? error.message : fallback),
  getLaunchCatalog: vi.fn().mockResolvedValue({ products: [{ productKey: 'ordarr' }] }),
  resolveProductWorkspaceBootstrapError: () => null,
  resolveSuiteHomeUrl: () => '/',
  useProductWorkspaceLaunch: () => ({
    mutate: vi.fn(),
    isPending: false,
    isError: false,
    error: null,
  }),
}))

vi.mock('./api/client', async () => {
  const actual = await vi.importActual<typeof client>('./api/client')
  return {
    ...actual,
    getSessionBootstrap: vi.fn(),
    getDashboard: vi.fn(),
    getReportSummary: vi.fn(),
    listOrders: vi.fn(),
    getOrder: vi.fn(),
    listHandoffs: vi.fn(),
    listCompletionPackets: vi.fn(),
    createOrder: vi.fn(),
    submitOrder: vi.fn(),
    addOrderLine: vi.fn(),
    addHold: vi.fn(),
    releaseHold: vi.fn(),
    approveOrder: vi.fn(),
    cancelOrder: vi.fn(),
    createReturn: vi.fn(),
  }
})

describe('OrdArr app', () => {
  afterEach(() => {
    cleanup()
  })

  beforeEach(() => {
    sessionStorage.clear()
    saveSession({
      accessToken: 'token-1',
      accessTokenExpiresAt: new Date(Date.now() + 60_000).toISOString(),
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      tenantSlug: 'tenant-one',
      tenantDisplayName: 'Tenant One',
      displayName: 'Ops User',
      email: 'ops@example.com',
      tenantRoleKey: 'ordarr-ops',
      isPlatformAdmin: true,
      entitlements: ['ordarr'],
    })
  })

  it('renders the dashboard console', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'ordarr-ops',
      isPlatformAdmin: true,
      productKey: 'ordarr',
      hasOrdArrEntitlement: true,
      entitlements: ['ordarr'],
    })
    vi.mocked(client.getDashboard).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      orderCount: 3,
      requestCount: 1,
      activeHandoffCount: 2,
      completionPacketCount: 1,
      invoiceReadyPacketCount: 1,
      billReadyPacketCount: 0,
      openOrderCount: 2,
      openHoldCount: 1,
      blockedOrderCount: 1,
      lateOrderCount: 1,
      returnCount: 1,
      featuredOrders: [],
      recentActivity: [],
    })
    vi.mocked(client.getReportSummary).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      orderCount: 3,
      openOrderCount: 2,
      closedOrderCount: 1,
      blockedOrderCount: 1,
      openHoldCount: 1,
      lateOrderCount: 1,
      lineCount: 4,
      returnedQuantity: 1,
      fillRatePercent: 33.3,
      onTimePercent: 66.7,
      activeHandoffCount: 2,
      returnCount: 1,
      completionPacketCount: 1,
      invoiceReadyPacketCount: 1,
      billReadyPacketCount: 0,
      featuredOrders: [],
    })
    vi.mocked(client.listOrders).mockResolvedValue([])
    vi.mocked(client.listHandoffs).mockResolvedValue([])
    vi.mocked(client.listCompletionPackets).mockResolvedValue([])

    renderApp('/dashboard')

    await screen.findByText('Order orchestration console')
    expect(screen.getByText('Open orders')).toBeInTheDocument()
    expect(screen.getByText('Reporting snapshot')).toBeInTheDocument()
  })

  it('renders the order detail console', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'ordarr-ops',
      isPlatformAdmin: true,
      productKey: 'ordarr',
      hasOrdArrEntitlement: true,
      entitlements: ['ordarr'],
    })
    vi.mocked(client.getDashboard).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      orderCount: 1,
      requestCount: 1,
      activeHandoffCount: 0,
      completionPacketCount: 0,
      invoiceReadyPacketCount: 0,
      billReadyPacketCount: 0,
      openOrderCount: 1,
      openHoldCount: 0,
      blockedOrderCount: 0,
      lateOrderCount: 0,
      returnCount: 0,
      featuredOrders: [],
      recentActivity: [],
    })
    vi.mocked(client.getReportSummary).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      orderCount: 1,
      openOrderCount: 1,
      closedOrderCount: 0,
      blockedOrderCount: 0,
      openHoldCount: 0,
      lateOrderCount: 0,
      lineCount: 1,
      returnedQuantity: 0,
      fillRatePercent: 0,
      onTimePercent: 100,
      activeHandoffCount: 0,
      returnCount: 0,
      completionPacketCount: 0,
      invoiceReadyPacketCount: 0,
      billReadyPacketCount: 0,
      featuredOrders: [],
    })
    vi.mocked(client.listOrders).mockResolvedValue([])
    vi.mocked(client.listHandoffs).mockResolvedValue([])
    vi.mocked(client.listCompletionPackets).mockResolvedValue([])
    vi.mocked(client.getOrder).mockResolvedValue({
      orderId: 'order-1',
      orderNumber: 'ORD-2026-1001',
      requestType: 'customer_order',
      lifecycleStatus: 'accepted',
      customerRef: {
        productKey: 'customarr',
        objectType: 'customer',
        objectId: 'cust-1',
        objectNumber: 'CUST-1',
      },
      customerName: 'Contoso Freight',
      ownerPersonId: 'person-1',
      requestedAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      requestedWindowStart: null,
      requestedWindowEnd: null,
      promisedWindowStart: null,
      promisedWindowEnd: null,
      handoffState: 'requested',
      completionState: 'in_progress',
      financialPacketState: 'not_ready',
      summary: 'Sample order',
      sourceChannel: 'manual_entry',
      orderType: 'customer_order',
      priority: 'normal',
      lineCount: 1,
      holdCount: 0,
      approvalState: 'approved',
      customerFacingStatus: 'accepted',
      nextAction: 'Open detail',
      buyerPoNumber: null,
      billToRef: null,
      shipToRef: null,
      shippingMethodPreference: null,
      paymentTerms: null,
      customerNotes: null,
      internalNotes: null,
      sourceReference: null,
      handoffs: [],
      completionPackets: [],
      events: [],
      lines: [
        {
          orderLineId: 'line-1',
          lineNumber: 1,
          lineType: 'item',
          itemRef: null,
          description: 'Replacement pump kit',
          quantity: 2,
          unitOfMeasure: 'ea',
          requestedDate: null,
          promisedDate: null,
          unitPrice: 0,
          discount: 0,
          taxable: true,
          allowSubstitution: true,
          canCancel: true,
          canReturn: true,
          targetProductKey: 'loadarr',
          complianceFlag: null,
          linkedDemandReference: null,
          fulfillmentStatus: 'open',
          allocationStatus: 'unallocated',
          createdAt: new Date().toISOString(),
        },
      ],
      holds: [],
      timeline: [
        {
          timelineId: 'tl-1',
          eventType: 'order.approved',
          status: 'accepted',
          message: 'Order approved for orchestration.',
          actorPersonId: 'person-1',
          sourceProductKey: 'ordarr',
          occurredAt: new Date().toISOString(),
        },
      ],
      returns: [],
    })

    renderOrderDetail('/orders/order-1')

    await screen.findByText('ORD-2026-1001')
    expect(screen.getByText('Timeline')).toBeInTheDocument()
    expect(screen.queryAllByText((_, element) => element?.textContent?.includes('Replacement pump kit') ?? false)).not.toHaveLength(0)
  })
})

function renderApp(pathname: string) {
  window.history.pushState({}, '', pathname)
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: 0,
      },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </QueryClientProvider>,
  )
}

function renderOrderDetail(pathname: string) {
  window.history.pushState({}, '', pathname)
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: 0,
      },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[pathname]}>
        <Routes>
          <Route path="/orders/:orderId" element={<OrderDetailPage accessToken="token-1" />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}
