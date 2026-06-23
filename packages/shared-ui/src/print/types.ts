import type { ReactNode } from 'react'

export type PrintDocumentStatus = 'draft' | 'working_copy' | 'official' | 'copy' | 'redacted'

export type PrintActionKind =
  | 'preview'
  | 'browser_print'
  | 'download_pdf'
  | 'download_label_pdf'
  | 'download_packet'
  | 'archive_official'
  | 'send'
  | 'reprint'

export interface PrintTemplateDescriptor {
  templateKey: string
  productKey: string
  name: string
  description: string
  version: string
  dataContractVersion: string
  format: 'html_print' | 'pdf' | 'label_pdf' | 'packet_pdf' | 'badge_pdf'
  paperSize: string
  orientation: 'portrait' | 'landscape'
  documentStatus: PrintDocumentStatus
  isSystemTemplate: boolean
  tenantOverrideAllowed: boolean
  requiresArchive: boolean
  requiresOfficialIssue: boolean
  requiresReprintReason: boolean
  retentionClass?: string | null
  defaultFileNamePattern?: string | null
}

export interface PrintTemplateCatalogResponse {
  templates: PrintTemplateDescriptor[]
}

export interface BrowserPrintLogRequest {
  sourceEntityType: string
  sourceEntityId: string
  sourceDisplayRef: string
  templateKey?: string
  templateVersion?: string
  documentStatus?: PrintDocumentStatus
  metadataJson?: string
}

export interface BrowserPrintLogResponse {
  logId: string
  productKey: string
  action: PrintActionKind
  documentStatus: PrintDocumentStatus
  templateKey: string
  templateVersion: string
  requestedAtUtc: string
}

export interface PrintDocumentRequest {
  sourceEntityType: string
  sourceEntityId: string
  sourceDisplayRef?: string
  templateKey?: string
  templateVersion?: string
  documentStatus?: PrintDocumentStatus
  optionsJson?: string
  reprintReason?: string
}

export interface PrintPreviewResponse {
  documentTitle: string
  sourceDisplayRef: string
  templateKey: string
  templateVersion: string
  previewHtml?: string | null
  previewRoute?: string | null
  warnings: string[]
  missingRequirements: string[]
  logId: string
}

export interface ArchiveOfficialResponse {
  documentTitle: string
  sourceDisplayRef: string
  templateKey: string
  templateVersion: string
  recordArrDocumentId?: string | null
  warnings: string[]
  missingRequirements: string[]
  logId: string
}

export interface ReprintRequest extends PrintDocumentRequest {
  requireReason?: boolean
  dialogTitle?: string
  confirmLabel?: string
  followUpAction?: 'download_pdf' | 'download_label_pdf' | 'download_packet' | null
}

export interface PrintHistoryItem {
  id: string
  productKey: string
  sourceEntityType: string
  sourceEntityId: string
  sourceDisplayRef: string
  templateKey: string
  templateVersion: string
  action: PrintActionKind
  documentStatus: PrintDocumentStatus
  requestedByPersonId: string
  requestedAtUtc: string
  completedAtUtc?: string | null
  recordArrDocumentId?: string | null
  fileName?: string | null
  contentHash?: string | null
  reprintReason?: string | null
  failureReason?: string | null
  metadataJson?: string | null
}

export interface PrintHistoryResponse {
  items: PrintHistoryItem[]
}

export interface PrintActionRequestConfig {
  label?: string
  request: PrintDocumentRequest
}

export interface PrintableSurfaceRegistration {
  title: string
  subtitle?: string
  productLabel?: string
  tenantLabel?: string
  sourceDisplayRef?: string
  sourceEntityType?: string
  sourceEntityId?: string
  templateKey?: string
  templateVersion?: string
  documentStatus?: PrintDocumentStatus
  generatedAt?: string
  generatedBy?: string
  statusLabel?: string
  watermarkLabel?: string | false
  pageFooter?: ReactNode
  signatureSection?: ReactNode
  appendixSection?: ReactNode
  previewLayout?: 'document' | 'custom'
  metadata?: Record<string, unknown>
  allowBrowserPrint?: boolean
  previewSearchParam?: string
  downloadPdf?: PrintActionRequestConfig | false
  downloadLabelPdf?: PrintActionRequestConfig | false
  downloadPacket?: PrintActionRequestConfig | false
  archiveOfficial?: PrintActionRequestConfig | false
  reprint?: ReprintRequest | false
  toolbarActions?: ReactNode
}
