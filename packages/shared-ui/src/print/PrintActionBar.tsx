import { useQuery } from '@tanstack/react-query'
import { Archive, Eye, FileDown, History, Printer, Repeat, Tags, X } from 'lucide-react'
import { useMemo, useState } from 'react'
import { ReprintReasonDialog } from './PrintComponents'
import {
  archiveOfficialCopy,
  downloadPrintPdf,
  getPrintHistory,
  logBrowserPrint,
  logReprint,
} from './printClient'
import { PrintHistoryPanel } from './PrintHistoryPanel'
import type {
  PrintActionRequestConfig,
  PrintDocumentRequest,
  PrintableSurfaceRegistration,
  ReprintRequest,
} from './types'

type Props = {
  apiBase?: string
  accessToken?: string
  productKey: string
  currentRouteRef: string
  isPreviewMode: boolean
  surface: PrintableSurfaceRegistration
  onEnterPreview: () => void
  onExitPreview: () => void
}

export function PrintActionBar({
  apiBase,
  accessToken,
  productKey,
  currentRouteRef,
  isPreviewMode,
  surface,
  onEnterPreview,
  onExitPreview,
}: Props) {
  const [historyOpen, setHistoryOpen] = useState(false)
  const [loggingError, setLoggingError] = useState<string | null>(null)
  const [actionMessage, setActionMessage] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [pendingAction, setPendingAction] = useState<string | null>(null)
  const [reprintOpen, setReprintOpen] = useState(false)

  const sourceEntityType = surface.sourceEntityType ?? 'page'
  const sourceEntityId = surface.sourceEntityId ?? currentRouteRef
  const sourceDisplayRef = surface.sourceDisplayRef ?? surface.title
  const templateKey =
    surface.templateKey ?? `${productKey}.current_page.${surface.documentStatus ?? 'working_copy'}`
  const templateVersion = surface.templateVersion ?? '1'
  const documentStatus = surface.documentStatus ?? 'working_copy'

  const historyEnabled = Boolean(apiBase && accessToken && historyOpen)
  const historyQuery = useQuery({
    queryKey: ['stl-print-history', apiBase, sourceEntityType, sourceEntityId],
    queryFn: () =>
      getPrintHistory(apiBase!, accessToken!, {
        sourceEntityType,
        sourceEntityId,
        limit: 12,
      }),
    enabled: historyEnabled,
  })

  const routeMetadata = useMemo(
    () =>
      JSON.stringify({
        ...(surface.metadata ?? {}),
        routeRef: currentRouteRef,
        previewMode: isPreviewMode,
        title: surface.title,
      }),
    [currentRouteRef, isPreviewMode, surface.metadata, surface.title],
  )

  const hasApiAccess = Boolean(apiBase && accessToken)
  const downloadPdfAction: PrintActionRequestConfig | null =
    surface.downloadPdf && typeof surface.downloadPdf === 'object' ? surface.downloadPdf : null
  const downloadLabelPdfAction: PrintActionRequestConfig | null =
    surface.downloadLabelPdf && typeof surface.downloadLabelPdf === 'object'
      ? surface.downloadLabelPdf
      : null
  const downloadPacketAction: PrintActionRequestConfig | null =
    surface.downloadPacket && typeof surface.downloadPacket === 'object'
      ? surface.downloadPacket
      : null
  const archiveOfficialAction: PrintActionRequestConfig | null =
    surface.archiveOfficial && typeof surface.archiveOfficial === 'object'
      ? surface.archiveOfficial
      : null
  const reprintAction: ReprintRequest | null =
    surface.reprint && typeof surface.reprint === 'object' ? surface.reprint : null

  const buildRequest = (request: PrintDocumentRequest): PrintDocumentRequest => ({
    ...request,
    sourceDisplayRef: request.sourceDisplayRef ?? sourceDisplayRef,
    templateKey: request.templateKey ?? templateKey,
    templateVersion: request.templateVersion ?? templateVersion,
    documentStatus: request.documentStatus ?? documentStatus,
    optionsJson: request.optionsJson ?? routeMetadata,
  })

  const triggerDownload = ({
    blob,
    fileName,
  }: {
    blob: Blob
    fileName: string | null
  }) => {
    const objectUrl = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = objectUrl
    anchor.download = fileName ?? `${surface.title.replace(/\s+/g, '-').toLowerCase()}.pdf`
    document.body.appendChild(anchor)
    anchor.click()
    document.body.removeChild(anchor)
    URL.revokeObjectURL(objectUrl)
  }

  const runPdfDownload = async (
    requestConfig: PrintActionRequestConfig,
    pendingKey: string,
    successMessage: string,
  ) => {
    if (!apiBase || !accessToken) {
      return
    }

    setPendingAction(pendingKey)
    setActionError(null)
    setActionMessage(null)
    try {
      const file = await downloadPrintPdf(apiBase, accessToken, buildRequest(requestConfig.request))
      triggerDownload(file)
      setActionMessage(successMessage)
    } catch (error) {
      setActionError(error instanceof Error ? error.message : 'Print export failed.')
    } finally {
      setPendingAction(null)
    }
  }

  const handlePrint = async () => {
    setLoggingError(null)
    try {
      if (apiBase && accessToken) {
        await logBrowserPrint(apiBase, accessToken, {
          sourceEntityType,
          sourceEntityId,
          sourceDisplayRef,
          templateKey,
          templateVersion,
          documentStatus,
          metadataJson: routeMetadata,
        })
      }
    } catch (error) {
      setLoggingError(error instanceof Error ? error.message : 'Unable to log browser print request.')
    } finally {
      globalThis.print?.()
    }
  }

  const handleArchiveOfficial = async () => {
    if (!apiBase || !accessToken || !archiveOfficialAction) {
      return
    }

    setPendingAction('archive_official')
    setActionError(null)
    setActionMessage(null)
    try {
      const response = await archiveOfficialCopy(
        apiBase,
        accessToken,
        buildRequest(archiveOfficialAction.request),
      )
      setActionMessage(
        response.recordArrDocumentId
          ? `Archived official copy to RecordArr document ${response.recordArrDocumentId}.`
          : 'Archived official copy.',
      )
    } catch (error) {
      setActionError(error instanceof Error ? error.message : 'Archive failed.')
    } finally {
      setPendingAction(null)
    }
  }

  const handleReprintConfirm = async (reason: string) => {
    if (!apiBase || !accessToken || !reprintAction) {
      return
    }

    const {
      requireReason: _requireReason,
      dialogTitle: _dialogTitle,
      confirmLabel: _confirmLabel,
      followUpAction,
      ...requestFields
    } = reprintAction

    setPendingAction('reprint')
    setActionError(null)
    setActionMessage(null)
    setReprintOpen(false)
    try {
      await logReprint(apiBase, accessToken, {
        ...buildRequest(requestFields as PrintDocumentRequest),
        reprintReason: reason,
      } as ReprintRequest)

      switch (followUpAction) {
        case 'download_pdf':
          if (downloadPdfAction) {
            const file = await downloadPrintPdf(apiBase, accessToken, {
              ...buildRequest(downloadPdfAction.request),
              reprintReason: reason,
            })
            triggerDownload(file)
            setActionMessage('Reprint recorded and PDF download started.')
            break
          }
          setActionMessage('Reprint recorded.')
          break
        case 'download_label_pdf':
          if (downloadLabelPdfAction) {
            const file = await downloadPrintPdf(apiBase, accessToken, {
              ...buildRequest(downloadLabelPdfAction.request),
              reprintReason: reason,
            })
            triggerDownload(file)
            setActionMessage('Reprint recorded and label PDF download started.')
            break
          }
          setActionMessage('Reprint recorded.')
          break
        case 'download_packet':
          if (downloadPacketAction) {
            const file = await downloadPrintPdf(apiBase, accessToken, {
              ...buildRequest(downloadPacketAction.request),
              reprintReason: reason,
            })
            triggerDownload(file)
            setActionMessage('Reprint recorded and packet download started.')
            break
          }
          setActionMessage('Reprint recorded.')
          break
        default:
          setActionMessage('Reprint recorded.')
          break
      }
    } catch (error) {
      setActionError(error instanceof Error ? error.message : 'Reprint failed.')
    } finally {
      setPendingAction(null)
    }
  }

  return (
    <div className="border-b border-slate-200/80 bg-white/95 px-4 py-3 text-slate-900 shadow-sm" data-print-hide>
      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          className="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900 transition hover:bg-slate-50"
          onClick={isPreviewMode ? onExitPreview : onEnterPreview}
        >
          {isPreviewMode ? <X className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
          {isPreviewMode ? 'Exit preview' : 'Preview'}
        </button>
        {surface.allowBrowserPrint === false ? null : (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-sky-700 bg-sky-700 px-3 py-2 text-sm font-medium text-white transition hover:bg-sky-600"
            onClick={() => {
              if (!isPreviewMode) {
                onEnterPreview()
                return
              }
              void handlePrint()
            }}
          >
            <Printer className="h-4 w-4" />
            {isPreviewMode ? 'Print' : 'Open preview to print'}
          </button>
        )}
        {hasApiAccess && downloadPdfAction ? (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={pendingAction === 'download_pdf'}
            onClick={() => void runPdfDownload(downloadPdfAction, 'download_pdf', 'PDF download started.')}
          >
            <FileDown className="h-4 w-4" />
            {pendingAction === 'download_pdf'
              ? 'Preparing PDF...'
              : downloadPdfAction.label ?? 'Download PDF'}
          </button>
        ) : null}
        {hasApiAccess && downloadLabelPdfAction ? (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={pendingAction === 'download_label_pdf'}
            onClick={() => void runPdfDownload(downloadLabelPdfAction, 'download_label_pdf', 'Label PDF download started.')}
          >
            <Tags className="h-4 w-4" />
            {pendingAction === 'download_label_pdf'
              ? 'Preparing label PDF...'
              : downloadLabelPdfAction.label ?? 'Download Label PDF'}
          </button>
        ) : null}
        {hasApiAccess && downloadPacketAction ? (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={pendingAction === 'download_packet'}
            onClick={() => void runPdfDownload(downloadPacketAction, 'download_packet', 'Packet download started.')}
          >
            <FileDown className="h-4 w-4" />
            {pendingAction === 'download_packet'
              ? 'Preparing packet...'
              : downloadPacketAction.label ?? 'Download Packet'}
          </button>
        ) : null}
        {hasApiAccess && archiveOfficialAction ? (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={pendingAction === 'archive_official'}
            onClick={() => void handleArchiveOfficial()}
          >
            <Archive className="h-4 w-4" />
            {pendingAction === 'archive_official'
              ? 'Archiving...'
              : archiveOfficialAction.label ?? 'Archive Official Copy'}
          </button>
        ) : null}
        {hasApiAccess && reprintAction ? (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={pendingAction === 'reprint'}
            onClick={() => {
              if (reprintAction.requireReason === false) {
                void handleReprintConfirm('')
                return
              }

              setReprintOpen(true)
            }}
          >
            <Repeat className="h-4 w-4" />
            {pendingAction === 'reprint' ? 'Recording reprint...' : 'Reprint Copy'}
          </button>
        ) : null}
        {hasApiAccess ? (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900 transition hover:bg-slate-50"
            onClick={() => setHistoryOpen((current) => !current)}
          >
            <History className="h-4 w-4" />
            {historyOpen ? 'Hide history' : 'View history'}
          </button>
        ) : null}
        {surface.toolbarActions}
      </div>
      {loggingError ? (
        <p className="mt-2 text-sm text-amber-700">
          Print will still open, but the request log could not be recorded: {loggingError}
        </p>
      ) : null}
      {actionError ? <p className="mt-2 text-sm text-rose-700">{actionError}</p> : null}
      {actionMessage ? <p className="mt-2 text-sm text-slate-700">{actionMessage}</p> : null}
      {surface.reprint && surface.reprint.requireReason !== false ? (
        <div className="mt-3">
          <ReprintReasonDialog
            open={reprintOpen}
            title={reprintAction?.dialogTitle ?? 'Reason required'}
            confirmLabel={reprintAction?.confirmLabel ?? 'Record reprint'}
            onClose={() => setReprintOpen(false)}
            onConfirm={(reason) => {
              void handleReprintConfirm(reason)
            }}
          />
        </div>
      ) : null}
      {historyOpen ? (
        <div className="mt-3">
          <PrintHistoryPanel
            items={historyQuery.data?.items ?? []}
            isLoading={historyQuery.isLoading}
            errorMessage={historyQuery.isError ? 'Unable to load print history for this surface.' : null}
          />
        </div>
      ) : null}
    </div>
  )
}
