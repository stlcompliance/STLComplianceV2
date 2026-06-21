import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PrintActionBar } from './PrintActionBar'
import type { PrintableSurfaceRegistration } from './types'

function renderActionBar({
  isPreviewMode,
  surface,
  onEnterPreview = vi.fn(),
  onExitPreview = vi.fn(),
}: {
  isPreviewMode: boolean
  surface: PrintableSurfaceRegistration
  onEnterPreview?: ReturnType<typeof vi.fn>
  onExitPreview?: ReturnType<typeof vi.fn>
}) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  })

  render(
    <QueryClientProvider client={queryClient}>
      <PrintActionBar
        apiBase="https://staffarr.example"
        accessToken="token-1"
        productKey="staffarr"
        currentRouteRef="/people/person-1"
        isPreviewMode={isPreviewMode}
        surface={surface}
        onEnterPreview={onEnterPreview}
        onExitPreview={onExitPreview}
      />
    </QueryClientProvider>,
  )

  return {
    onEnterPreview,
    onExitPreview,
  }
}

describe('PrintActionBar', () => {
  afterEach(() => {
    cleanup()
    vi.unstubAllGlobals()
  })

  it('opens preview instead of printing from the normal workspace route', () => {
    const printMock = vi.fn()
    vi.stubGlobal('print', printMock)

    const { onEnterPreview } = renderActionBar({
      isPreviewMode: false,
      surface: {
        title: 'Person profile',
      },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Open preview to print' }))

    expect(onEnterPreview).toHaveBeenCalledTimes(1)
    expect(printMock).not.toHaveBeenCalled()
  })

  it('logs browser print intent and then calls window.print in preview mode', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(
        JSON.stringify({
          logId: '7f69585c-bca8-4f36-a064-c32e3dbaf8ab',
          productKey: 'staffarr',
          action: 'browser_print',
          documentStatus: 'working_copy',
          templateKey: 'staffarr.current_page.working_copy',
          templateVersion: '1',
          requestedAtUtc: '2026-06-20T12:00:00Z',
        }),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        },
      ),
    )
    const printMock = vi.fn()
    vi.stubGlobal('fetch', fetchMock)
    vi.stubGlobal('print', printMock)

    renderActionBar({
      isPreviewMode: true,
      surface: {
        title: 'Person profile',
        sourceEntityType: 'person',
        sourceEntityId: 'person-1',
        metadata: {
          tenantDisplayName: 'North Yard Logistics',
        },
      },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Print' }))

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1))
    expect(fetchMock).toHaveBeenCalledWith(
      'https://staffarr.example/api/v1/print/browser-print-log',
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          Authorization: 'Bearer token-1',
          'Content-Type': 'application/json',
        }),
      }),
    )
    const [, init] = fetchMock.mock.calls[0]
    expect(JSON.parse(init.body)).toMatchObject({
      sourceEntityType: 'person',
      sourceEntityId: 'person-1',
      sourceDisplayRef: 'Person profile',
      templateKey: 'staffarr.current_page.working_copy',
      templateVersion: '1',
      documentStatus: 'working_copy',
      metadataJson: expect.any(String),
    })
    expect(JSON.parse(JSON.parse(init.body).metadataJson)).toMatchObject({
      tenantDisplayName: 'North Yard Logistics',
      routeRef: '/people/person-1',
      previewMode: true,
    })
    expect(printMock).toHaveBeenCalledTimes(1)
  })

  it('downloads a PDF through the shared print endpoint when configured', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response('pdf', {
        status: 200,
        headers: {
          'Content-Type': 'application/pdf',
          'Content-Disposition': 'attachment; filename="person-profile.pdf"',
        },
      }),
    )
    const createObjectURL = vi.fn(() => 'blob:pdf')
    const revokeObjectURL = vi.fn()
    vi.stubGlobal('fetch', fetchMock)
    vi.stubGlobal('URL', {
      createObjectURL,
      revokeObjectURL,
    })

    renderActionBar({
      isPreviewMode: true,
      surface: {
        title: 'Person profile',
        downloadPdf: {
          request: {
            sourceEntityType: 'person',
            sourceEntityId: 'person-1',
          },
        },
      },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Download PDF' }))

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1))
    expect(fetchMock).toHaveBeenCalledWith(
      'https://staffarr.example/api/v1/print/pdf',
      expect.objectContaining({
        method: 'POST',
      }),
    )
    const [, init] = fetchMock.mock.calls[0]
    expect(JSON.parse(init.body)).toMatchObject({
      sourceEntityType: 'person',
      sourceEntityId: 'person-1',
      sourceDisplayRef: 'Person profile',
      templateKey: 'staffarr.current_page.working_copy',
    })
    expect(createObjectURL).toHaveBeenCalledTimes(1)
    expect(revokeObjectURL).toHaveBeenCalledTimes(1)
  })

  it('records reprint reason and triggers follow-up PDF download when configured', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            logId: 'reprint-log-1',
            productKey: 'staffarr',
            action: 'reprint',
            documentStatus: 'official',
            templateKey: 'staffarr.current_page.official',
            templateVersion: '1',
            requestedAtUtc: '2026-06-20T12:00:00Z',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        ),
      )
      .mockResolvedValueOnce(
        new Response('pdf', {
          status: 200,
          headers: {
            'Content-Type': 'application/pdf',
            'Content-Disposition': 'attachment; filename="person-profile-reprint.pdf"',
          },
        }),
      )
    const createObjectURL = vi.fn(() => 'blob:reprint')
    const revokeObjectURL = vi.fn()
    vi.stubGlobal('fetch', fetchMock)
    vi.stubGlobal('URL', {
      createObjectURL,
      revokeObjectURL,
    })

    renderActionBar({
      isPreviewMode: true,
      surface: {
        title: 'Person profile',
        downloadPdf: {
          request: {
            sourceEntityType: 'person',
            sourceEntityId: 'person-1',
            templateKey: 'staffarr.current_page.official',
            documentStatus: 'official',
          },
        },
        reprint: {
          sourceEntityType: 'person',
          sourceEntityId: 'person-1',
          templateKey: 'staffarr.current_page.official',
          documentStatus: 'official',
          followUpAction: 'download_pdf',
        },
      },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Reprint Copy' }))
    fireEvent.change(screen.getByLabelText('Reprint reason'), {
      target: { value: 'Customer requested another copy' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Record reprint' }))

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(2))
    const [, reprintInit] = fetchMock.mock.calls[0]
    expect(JSON.parse(reprintInit.body)).toMatchObject({
      sourceEntityType: 'person',
      sourceEntityId: 'person-1',
      reprintReason: 'Customer requested another copy',
    })
    const [, pdfInit] = fetchMock.mock.calls[1]
    expect(JSON.parse(pdfInit.body)).toMatchObject({
      sourceEntityType: 'person',
      sourceEntityId: 'person-1',
      reprintReason: 'Customer requested another copy',
    })
    expect(createObjectURL).toHaveBeenCalledTimes(1)
    expect(revokeObjectURL).toHaveBeenCalledTimes(1)
  })
})
