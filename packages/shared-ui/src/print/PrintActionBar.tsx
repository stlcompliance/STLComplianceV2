import { Archive, ArrowLeft, Eye, FileDown, Printer, Repeat, Tags } from 'lucide-react'
import { useMemo, useState } from 'react'
import { ReprintReasonDialog } from './PrintComponents'
import {
  archiveOfficialCopy,
  downloadPrintPdf,
  logBrowserPrint,
  logReprint,
} from './printClient'
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
      console.error('Print PDF download failed', error)
      setActionError('Print export is temporarily unavailable. Please try again.')
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
      console.error('Browser print logging failed', error)
      setLoggingError('Print logging is temporarily unavailable. The print dialog will still open.')
    } finally {
      globalThis.window?.print?.()
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
      console.error('Official copy archive failed', error)
      setActionError('Archive is temporarily unavailable. Please try again.')
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
      console.error('Reprint failed', error)
      setActionError('Reprint is temporarily unavailable. Please try again.')
    } finally {
      setPendingAction(null)
    }
  }

  return (
    <div
      className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-4 py-3 text-[var(--color-text-primary)] shadow-sm"
      data-print-hide
    >
      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          className="inline-flex items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] transition hover:bg-[var(--color-bg-control-hover)]"
          onClick={isPreviewMode ? onExitPreview : onEnterPreview}
        >
          {isPreviewMode ? <ArrowLeft className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
          {isPreviewMode ? 'Back' : 'Preview'}
        </button>
        {surface.allowBrowserPrint === false ? null : (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-[var(--color-accent)] bg-[var(--color-accent)] px-3 py-2 text-sm font-medium text-[var(--color-on-accent)] transition hover:bg-[var(--color-accent-hover)]"
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
            className="inline-flex items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] transition hover:bg-[var(--color-bg-control-hover)] disabled:cursor-not-allowed disabled:opacity-60"
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
            className="inline-flex items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] transition hover:bg-[var(--color-bg-control-hover)] disabled:cursor-not-allowed disabled:opacity-60"
            disabled={pendingAction === 'download_label_pdf'}
            onClick={() =>
              void runPdfDownload(
                downloadLabelPdfAction,
                'download_label_pdf',
                'Label PDF download started.',
              )
            }
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
            className="inline-flex items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] transition hover:bg-[var(--color-bg-control-hover)] disabled:cursor-not-allowed disabled:opacity-60"
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
            className="inline-flex items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] transition hover:bg-[var(--color-bg-control-hover)] disabled:cursor-not-allowed disabled:opacity-60"
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
            className="inline-flex items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] transition hover:bg-[var(--color-bg-control-hover)] disabled:cursor-not-allowed disabled:opacity-60"
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
        {surface.toolbarActions}
      </div>
      {loggingError ? (
        <p className="mt-2 text-sm text-[var(--color-warning-text)]">
          Print will still open, but the request log could not be recorded: {loggingError}
        </p>
      ) : null}
      {actionError ? <p className="mt-2 text-sm text-[var(--color-destructive-text)]">{actionError}</p> : null}
      {actionMessage ? <p className="mt-2 text-sm text-[var(--color-text-secondary)]">{actionMessage}</p> : null}
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
    </div>
  )
}
