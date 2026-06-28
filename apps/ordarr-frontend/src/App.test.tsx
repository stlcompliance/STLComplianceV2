import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
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
    upsertCompletionPacket: vi.fn(),
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
      launchableProductKeys: ['ordarr'],
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
      launchableProductKeys: ['ordarr'],
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
      launchableProductKeys: ['ordarr'],
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
    expect(screen.getAllByText((_, element) => element?.textContent?.includes('OrdArr') ?? false)).not.toHaveLength(0)
    expect(screen.queryByText('ordarr')).not.toBeInTheDocument()
    expect(screen.queryAllByText((_, element) => element?.textContent?.includes('Replacement pump kit') ?? false)).not.toHaveLength(0)
  })

  it('marks completion packets ready from the order detail workspace', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'ordarr-ops',
      isPlatformAdmin: true,
      productKey: 'ordarr',
      launchableProductKeys: ['ordarr'],
    })
    vi.mocked(client.getDashboard).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      orderCount: 1,
      requestCount: 1,
      activeHandoffCount: 0,
      completionPacketCount: 1,
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
      completionPacketCount: 1,
      invoiceReadyPacketCount: 0,
      billReadyPacketCount: 0,
      featuredOrders: [],
    })
    vi.mocked(client.listOrders).mockResolvedValue([])
    vi.mocked(client.listHandoffs).mockResolvedValue([])
    vi.mocked(client.listCompletionPackets).mockResolvedValue([])

    const initialOrder = {
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
      nextAction: 'Finalize completion packet',
      buyerPoNumber: null,
      billToRef: null,
      shipToRef: null,
      shippingMethodPreference: null,
      paymentTerms: null,
      customerNotes: null,
      internalNotes: null,
      sourceReference: null,
      handoffs: [],
      completionPackets: [
        {
          packetId: 'packet-1',
          orderNumber: 'ORD-2026-1001',
          packetType: 'completion',
          status: 'draft',
          recordRefs: [],
        },
      ],
      events: [],
      lines: [],
      holds: [],
      timeline: [],
      returns: [],
    } as any
    const updatedOrder = {
      ...initialOrder,
      updatedAt: new Date(Date.now() + 1_000).toISOString(),
      completionState: 'ready',
      completionPackets: [
        {
          packetId: 'packet-1',
          orderNumber: 'ORD-2026-1001',
          packetType: 'completion',
          status: 'ready',
          recordRefs: [],
        },
      ],
    }
    vi.mocked(client.getOrder).mockResolvedValueOnce(initialOrder).mockResolvedValue(updatedOrder as any)
    vi.mocked(client.upsertCompletionPacket).mockResolvedValue(updatedOrder as any)

    renderOrderDetail('/orders/order-1')

    await screen.findByText('ORD-2026-1001')
    const completionPanel = screen.getByText('Completion packets').closest('section')
    expect(completionPanel).not.toBeNull()
    expect(within(completionPanel!).getByText('Draft')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Mark completion ready' }))

    await waitFor(() =>
      expect(vi.mocked(client.upsertCompletionPacket)).toHaveBeenCalledWith(
        'token-1',
        'order-1',
        { packetType: 'completion' },
        expect.any(String),
      ),
    )
    await waitFor(() => expect(within(completionPanel!).getByText('Ready')).toBeInTheDocument())
  })

  it('creates an order and navigates to the new detail page', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'ordarr-ops',
      isPlatformAdmin: true,
      productKey: 'ordarr',
      launchableProductKeys: ['ordarr'],
    })
    vi.mocked(client.getDashboard).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      orderCount: 0,
      requestCount: 0,
      activeHandoffCount: 0,
      completionPacketCount: 0,
      invoiceReadyPacketCount: 0,
      billReadyPacketCount: 0,
      openOrderCount: 0,
      openHoldCount: 0,
      blockedOrderCount: 0,
      lateOrderCount: 0,
      returnCount: 0,
      featuredOrders: [],
      recentActivity: [],
    })
    vi.mocked(client.getReportSummary).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      orderCount: 0,
      openOrderCount: 0,
      closedOrderCount: 0,
      blockedOrderCount: 0,
      openHoldCount: 0,
      lateOrderCount: 0,
      lineCount: 0,
      returnedQuantity: 0,
      fillRatePercent: 0,
      onTimePercent: 0,
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
    vi.mocked(client.createOrder).mockResolvedValue({
      orderId: 'order-2',
      orderNumber: 'ORD-2026-2002',
      requestType: 'customer_order',
      lifecycleStatus: 'draft',
      customerRef: {
        productKey: 'customarr',
        objectType: 'customer',
        objectId: 'cust-2',
        objectNumber: 'CUST-2',
      },
      customerName: 'Northwind Logistics',
      ownerPersonId: 'person-1',
      requestedAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      requestedWindowStart: null,
      requestedWindowEnd: null,
      promisedWindowStart: null,
      promisedWindowEnd: null,
      handoffState: 'requested',
      completionState: 'not_started',
      financialPacketState: 'not_ready',
      summary: 'Fresh order',
      sourceChannel: 'manual_entry',
      orderType: 'customer_order',
      priority: 'normal',
      lineCount: 1,
      holdCount: 0,
      approvalState: 'draft',
      customerFacingStatus: 'draft',
      nextAction: 'Review draft',
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
      lines: [],
      holds: [],
      timeline: [],
      returns: [],
    } as any)
    vi.mocked(client.getOrder).mockImplementation(async (_accessToken, orderId) => {
      if (orderId === 'order-2') {
        return {
          orderId: 'order-2',
          orderNumber: 'ORD-2026-2002',
          requestType: 'customer_order',
          lifecycleStatus: 'draft',
          customerRef: {
            productKey: 'customarr',
            objectType: 'customer',
            objectId: 'cust-2',
            objectNumber: 'CUST-2',
          },
          customerName: 'Northwind Logistics',
          ownerPersonId: 'person-1',
          requestedAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          requestedWindowStart: null,
          requestedWindowEnd: null,
          promisedWindowStart: null,
          promisedWindowEnd: null,
          handoffState: 'requested',
          completionState: 'not_started',
          financialPacketState: 'not_ready',
          summary: 'Fresh order',
          sourceChannel: 'manual_entry',
          orderType: 'customer_order',
          priority: 'normal',
          lineCount: 1,
          holdCount: 0,
          approvalState: 'draft',
          customerFacingStatus: 'draft',
          nextAction: 'Review draft',
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
          lines: [],
          holds: [],
          timeline: [],
          returns: [],
        }
      }

      throw new Error(`Unexpected order lookup: ${orderId}`)
    })

    renderApp('/orders/new')

    await screen.findByRole('heading', { name: 'Create order' })
    fireEvent.click(screen.getByRole('button', { name: 'Create order' }))

    await screen.findByText('ORD-2026-2002')
    expect(vi.mocked(client.createOrder)).toHaveBeenCalledTimes(1)
  })

  it('adds a hold from the order detail workspace', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'ordarr-ops',
      isPlatformAdmin: true,
      productKey: 'ordarr',
      launchableProductKeys: ['ordarr'],
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
    vi.mocked(client.addHold).mockResolvedValue({} as any)
    vi.mocked(client.getOrder).mockImplementation(async (_accessToken, orderId) => {
      if (orderId !== 'order-3') {
        throw new Error(`Unexpected order lookup: ${orderId}`)
      }

      const hold = {
        holdId: 'hold-1',
        holdType: 'compliance',
        reason: 'Manual hold from OrdArr workspace',
        ownerProductKey: 'compliancecore',
        releasePermission: 'ordarr.order_requests.update',
        status: 'open',
        ownerPersonId: 'person-1',
      }

      return {
        orderId: 'order-3',
        orderNumber: 'ORD-2026-3003',
        requestType: 'customer_order',
        lifecycleStatus: 'accepted',
        customerRef: {
          productKey: 'customarr',
          objectType: 'customer',
          objectId: 'cust-3',
          objectNumber: 'CUST-3',
        },
        customerName: 'Blue Yonder',
        ownerPersonId: 'person-1',
        requestedAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        requestedWindowStart: null,
        requestedWindowEnd: null,
        promisedWindowStart: null,
        promisedWindowEnd: null,
        handoffState: 'requested',
        completionState: 'not_started',
        financialPacketState: 'not_ready',
        summary: 'Order with hold',
        sourceChannel: 'manual_entry',
        orderType: 'customer_order',
        priority: 'normal',
        lineCount: 1,
        holdCount: 1,
        approvalState: 'approved',
        customerFacingStatus: 'accepted',
        nextAction: 'Review hold',
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
        lines: [],
        holds: [hold],
        timeline: [],
        returns: [],
      } as any
    })

    renderOrderDetail('/orders/order-3')

    await screen.findByRole('heading', { name: 'ORD-2026-3003' })
    fireEvent.click(screen.getByRole('button', { name: 'Hold' }))

    await screen.findByText('Manual hold from OrdArr workspace')
    expect(vi.mocked(client.addHold)).toHaveBeenCalledTimes(1)
  })

  it('renders downstream handoffs in the order detail workspace', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'ordarr-ops',
      isPlatformAdmin: true,
      productKey: 'ordarr',
      launchableProductKeys: ['ordarr'],
    })
    vi.mocked(client.getDashboard).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      orderCount: 1,
      requestCount: 1,
      activeHandoffCount: 1,
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
      activeHandoffCount: 1,
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
      orderId: 'order-4',
      orderNumber: 'ORD-2026-4004',
      requestType: 'customer_order',
      lifecycleStatus: 'accepted',
      customerRef: {
        productKey: 'customarr',
        objectType: 'customer',
        objectId: 'cust-4',
        objectNumber: 'CUST-4',
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
      completionState: 'not_started',
      financialPacketState: 'not_ready',
      summary: 'Order with downstream demand',
      sourceChannel: 'manual_entry',
      orderType: 'customer_order',
      priority: 'normal',
      lineCount: 1,
      holdCount: 0,
      approvalState: 'approved',
      customerFacingStatus: 'accepted',
      nextAction: 'Review handoff',
      buyerPoNumber: null,
      billToRef: null,
      shipToRef: null,
      shippingMethodPreference: null,
      paymentTerms: null,
      customerNotes: null,
      internalNotes: null,
      sourceReference: null,
      handoffs: [
        {
          handoffId: 'handoff-1',
          targetProductKey: 'loadarr',
          handoffType: 'fulfillment',
          state: 'requested',
          summary: 'Reserve inventory for order 4004',
          requestedAt: new Date().toISOString(),
          completedAt: null,
        },
      ],
      completionPackets: [],
      events: [],
      lines: [
        {
          orderLineId: 'line-1',
          lineNumber: 1,
          lineType: 'item',
          itemRef: null,
          description: 'Replacement kit',
          quantity: 1,
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
      timeline: [],
      returns: [],
    } as any)

    renderOrderDetail('/orders/order-4')

    await screen.findByRole('heading', { name: 'ORD-2026-4004' })
    expect(screen.getAllByText('LoadArr')).not.toHaveLength(0)
    expect(screen.queryByText('loadarr')).not.toBeInTheDocument()
    expect(screen.getByText('Reserve inventory for order 4004')).toBeInTheDocument()
  })

  it('renders settings without exposing local ports or endpoint coordinates', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'ordarr-ops',
      isPlatformAdmin: true,
      productKey: 'ordarr',
      launchableProductKeys: ['ordarr'],
    })
    vi.mocked(client.getDashboard).mockResolvedValue({ generatedAt: new Date().toISOString() } as any)
    vi.mocked(client.getReportSummary).mockResolvedValue({ generatedAt: new Date().toISOString() } as any)
    vi.mocked(client.listOrders).mockResolvedValue([])
    vi.mocked(client.listHandoffs).mockResolvedValue([])
    vi.mocked(client.listCompletionPackets).mockResolvedValue([])

    renderApp('/settings')

    await screen.findByRole('heading', { name: 'Workspace wiring' })
    expect(screen.getByText('API connection:')).toBeInTheDocument()
    expect(screen.getByText('Session state:')).toBeInTheDocument()
    expect(screen.queryByText('API port:')).not.toBeInTheDocument()
    expect(screen.queryByText('Frontend port:')).not.toBeInTheDocument()
    expect(screen.queryByText('5112')).not.toBeInTheDocument()
    expect(screen.queryByText('5187')).not.toBeInTheDocument()
  })

  it('adds an order line in the routed order detail workspace', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'ordarr-ops',
      isPlatformAdmin: true,
      productKey: 'ordarr',
      launchableProductKeys: ['ordarr'],
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
    vi.mocked(client.addOrderLine).mockResolvedValue({} as any)

    let lineAdded = false
    vi.mocked(client.getOrder).mockImplementation(async (_accessToken, orderId) => {
      if (orderId !== 'order-5') {
        throw new Error(`Unexpected order lookup: ${orderId}`)
      }

      return {
        orderId: 'order-5',
        orderNumber: 'ORD-2026-5005',
        requestType: 'customer_order',
        lifecycleStatus: 'accepted',
        customerRef: {
          productKey: 'customarr',
          objectType: 'customer',
          objectId: 'cust-5',
          objectNumber: 'CUST-5',
        },
        customerName: 'Northwind Logistics',
        ownerPersonId: 'person-1',
        requestedAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        requestedWindowStart: null,
        requestedWindowEnd: null,
        promisedWindowStart: null,
        promisedWindowEnd: null,
        handoffState: 'requested',
        completionState: 'not_started',
        financialPacketState: 'not_ready',
        summary: 'Order awaiting lines',
        sourceChannel: 'manual_entry',
        orderType: 'customer_order',
        priority: 'normal',
        lineCount: lineAdded ? 1 : 0,
        holdCount: 0,
        approvalState: 'approved',
        customerFacingStatus: 'accepted',
        nextAction: 'Add line',
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
        lines: lineAdded
          ? [
              {
                orderLineId: 'line-5',
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
            ]
          : [],
        holds: [],
        timeline: [],
        returns: [],
      } as any
    })
    vi.mocked(client.addOrderLine).mockImplementation(async () => {
      lineAdded = true
      return {} as any
    })

    renderOrderDetail('/orders/order-5')

    await screen.findByRole('heading', { name: 'ORD-2026-5005' })
    fireEvent.change(screen.getByLabelText('Description'), { target: { value: 'Replacement pump kit' } })
    fireEvent.click(screen.getByRole('button', { name: 'Add line' }))

    await waitFor(() => {
      expect(vi.mocked(client.addOrderLine)).toHaveBeenCalledTimes(1)
    })
    expect(vi.mocked(client.addOrderLine)).toHaveBeenCalledWith(
      'token-1',
      'order-5',
      expect.objectContaining({
        description: 'Replacement pump kit',
        targetProductKey: 'loadarr',
        quantity: 1,
        unitOfMeasure: 'ea',
      }),
      expect.any(String),
    )
  })

  it('redirects legacy handoff URLs to the canonical launch route without dropping the query string', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'ordarr-ops',
      isPlatformAdmin: true,
      productKey: 'ordarr',
      launchableProductKeys: ['ordarr'],
    })
    vi.mocked(client.getDashboard).mockResolvedValue({ generatedAt: new Date().toISOString() } as any)
    vi.mocked(client.getReportSummary).mockResolvedValue({ generatedAt: new Date().toISOString() } as any)
    vi.mocked(client.listOrders).mockResolvedValue([])
    vi.mocked(client.listHandoffs).mockResolvedValue([])
    vi.mocked(client.listCompletionPackets).mockResolvedValue([])

    renderApp('/handoff?handoff=handoff-code-1')

    await waitFor(() => {
      expect(window.location.pathname).toBe('/launch')
    })
    expect(window.location.search).toBe('?handoff=handoff-code-1')
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
