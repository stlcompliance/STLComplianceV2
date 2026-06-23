import { FileDown, ShieldAlert } from 'lucide-react'
import { useState } from 'react'
import {
  PrintableDocumentHeader,
  PrintableDocumentShell,
  downloadPrintPdf,
  type PrintDocumentRequest,
} from '@stl/shared-ui'
import type {
  RecordArrAccessLog,
  RecordArrFile,
  RecordArrPackage,
  RecordArrRecord,
  RecordArrRedaction,
  RecordArrRetentionStatus,
} from '../api/client'

const printApiBase =
  import.meta.env.VITE_RECORDARR_API_BASE ??
  (typeof globalThis.location?.origin === 'string' ? globalThis.location.origin : '')

type ToolbarProps = {
  accessToken: string
  record: RecordArrRecord
  actorDisplayName?: string
  tenantDisplayName?: string
  redactions: RecordArrRedaction[]
}

type PreviewProps = {
  record: RecordArrRecord
  files: RecordArrFile[]
  retentionStatus: RecordArrRetentionStatus | null
  accessLogs: RecordArrAccessLog[]
  packages: RecordArrPackage[]
  redactions: RecordArrRedaction[]
  actorPersonId: string
  actorDisplayName?: string
  tenantDisplayName?: string
}

function buildMetadataJson(record: RecordArrRecord, actorDisplayName?: string, tenantDisplayName?: string) {
  return JSON.stringify({
    tenantDisplayName,
    actorDisplayName,
    title: record.title,
    sourceDisplayRef: record.recordNumber,
  })
}

function buildRequest(
  record: RecordArrRecord,
  templateKey: string,
  documentStatus: PrintDocumentRequest['documentStatus'],
  actorDisplayName?: string,
  tenantDisplayName?: string,
): PrintDocumentRequest {
  return {
    sourceEntityType: 'record',
    sourceEntityId: record.recordId,
    sourceDisplayRef: record.recordNumber,
    templateKey,
    documentStatus,
    optionsJson: buildMetadataJson(record, actorDisplayName, tenantDisplayName),
  }
}

function triggerDownload(blob: Blob, fileName: string | null) {
  const objectUrl = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = objectUrl
  anchor.download = fileName ?? 'recordarr-print.pdf'
  document.body.appendChild(anchor)
  anchor.click()
  document.body.removeChild(anchor)
  URL.revokeObjectURL(objectUrl)
}

function buttonClassName() {
  return 'inline-flex items-center gap-2 rounded-lg border border-[var(--color-border-default)] bg-[var(--color-bg-surface)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] transition hover:bg-[var(--color-bg-control-hover)] disabled:cursor-not-allowed disabled:opacity-60'
}

function formatDateTime(value: string | null | undefined) {
  if (!value) {
    return 'Not set'
  }

  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) {
    return value
  }

  return parsed.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    timeZoneName: 'short',
  })
}

function formatActorLabel(entry: RecordArrAccessLog, actorPersonId: string, actorDisplayName?: string) {
  if (entry.actorServiceClientId) {
    return 'System service'
  }

  if (entry.actorPersonId && entry.actorPersonId === actorPersonId && actorDisplayName) {
    return actorDisplayName
  }

  if (entry.actorPersonId) {
    return 'Authorized user'
  }

  return 'System'
}

export function RecordPrintToolbarActions({
  accessToken,
  record,
  actorDisplayName,
  tenantDisplayName,
  redactions,
}: ToolbarProps) {
  const [pendingAction, setPendingAction] = useState<string | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  async function startDownload(
    actionKey: string,
    templateKey: string,
    documentStatus: PrintDocumentRequest['documentStatus'],
  ) {
    if (!accessToken || !printApiBase) {
      return
    }

    setPendingAction(actionKey)
    setErrorMessage(null)
    try {
      const file = await downloadPrintPdf(
        printApiBase,
        accessToken,
        buildRequest(record, templateKey, documentStatus, actorDisplayName, tenantDisplayName),
      )
      triggerDownload(file.blob, file.fileName)
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Print export failed.')
    } finally {
      setPendingAction(null)
    }
  }

  return (
    <>
      <button
        type="button"
        className={buttonClassName()}
        disabled={pendingAction === 'original'}
        onClick={() => {
          void startDownload('original', 'recordarr.document.original', 'official')
        }}
      >
        <FileDown className="h-4 w-4" />
        {pendingAction === 'original' ? 'Preparing original...' : 'Download Original PDF'}
      </button>
      {redactions.length > 0 ? (
        <button
          type="button"
          className={buttonClassName()}
          disabled={pendingAction === 'redacted'}
          onClick={() => {
            void startDownload('redacted', 'recordarr.document.redacted_copy', 'redacted')
          }}
        >
          <ShieldAlert className="h-4 w-4" />
          {pendingAction === 'redacted' ? 'Preparing redacted copy...' : 'Download Redacted PDF'}
        </button>
      ) : null}
      {errorMessage ? <span className="text-sm text-[var(--tone-danger-text)]">{errorMessage}</span> : null}
    </>
  )
}

export function RecordPrintPreview({
  record,
  files,
  retentionStatus,
  accessLogs,
  packages,
  redactions,
  actorPersonId,
  actorDisplayName,
  tenantDisplayName,
}: PreviewProps) {
  const activeLegalHold = record.legalHoldRefs.length > 0 ? `Active refs: ${record.legalHoldRefs.length}` : 'No active hold refs'
  const recentAccess = accessLogs.slice(0, 6)
  const recentPackages = packages.slice(0, 6)
  const visibleFiles = files.slice(0, 6)

  return (
    <PrintableDocumentShell
      title={`${record.title} cover sheet`}
      subtitle={`${tenantDisplayName || 'Current tenant workspace'} · RecordArr · ${record.recordNumber}`}
      productLabel="RecordArr"
      tenantLabel={tenantDisplayName}
      sourceDisplayRef={record.recordNumber}
      documentStatus="working_copy"
      generatedBy={actorDisplayName}
      watermarkLabel="Working copy"
      footer={
        <div className="space-y-1">
          <p>
            Generated {formatDateTime(new Date().toISOString())} by {actorDisplayName || 'Authorized user'}.
          </p>
          <p>Printed output hides workspace chrome and preserves only approved record-facing details.</p>
        </div>
      }
    >
      <div className="relative space-y-6">
        <PrintableDocumentHeader
          title={record.title}
          metadata={
            <div className="flex flex-wrap items-center gap-2">
              <span className="inline-flex items-center rounded-full border border-[var(--color-border-default)] bg-[var(--color-bg-surface)] px-3 py-1 text-xs font-semibold uppercase tracking-wide text-[var(--color-text-secondary)]">
                {record.recordNumber}
              </span>
              <span className="inline-flex items-center rounded-full border border-[var(--color-border-default)] bg-[var(--color-bg-surface)] px-3 py-1 text-xs font-semibold uppercase tracking-wide text-[var(--color-text-secondary)]">
                Working copy
              </span>
              {redactions.length > 0 ? (
                <span className="inline-flex items-center rounded-full border border-[var(--tone-warning-border)] bg-[var(--tone-warning-bg)] px-3 py-1 text-xs font-semibold uppercase tracking-wide text-[var(--tone-warning-text)]">
                  {redactions.length} redaction event{redactions.length === 1 ? '' : 's'} on file
                </span>
              ) : null}
            </div>
          }
        />

        <section className="grid gap-4 md:grid-cols-2">
          <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
            <h3 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Record summary</h3>
            <dl className="mt-3 space-y-2 text-sm text-[var(--color-text-secondary)]">
              <div>
                <dt className="font-medium text-[var(--color-text-primary)]">Document path</dt>
                <dd>{record.documentClass} / {record.documentType} / {record.documentSubtype}</dd>
              </div>
              <div>
                <dt className="font-medium text-[var(--color-text-primary)]">Classification</dt>
                <dd>{record.classification}</dd>
              </div>
              <div>
                <dt className="font-medium text-[var(--color-text-primary)]">Source</dt>
                <dd>{record.sourceProduct} · {record.sourceObjectDisplayName}</dd>
              </div>
              <div>
                <dt className="font-medium text-[var(--color-text-primary)]">Current file</dt>
                <dd>{record.currentFileName}</dd>
              </div>
              <div>
                <dt className="font-medium text-[var(--color-text-primary)]">Lifecycle</dt>
                <dd>{record.status} · version {record.versionNumber}</dd>
              </div>
            </dl>
          </div>
          <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
            <h3 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Governance</h3>
            <dl className="mt-3 space-y-2 text-sm text-[var(--color-text-secondary)]">
              <div>
                <dt className="font-medium text-[var(--color-text-primary)]">Uploaded</dt>
                <dd>{formatDateTime(record.uploadedAt)}</dd>
              </div>
              <div>
                <dt className="font-medium text-[var(--color-text-primary)]">Effective</dt>
                <dd>{formatDateTime(record.effectiveAt)}</dd>
              </div>
              <div>
                <dt className="font-medium text-[var(--color-text-primary)]">Expires</dt>
                <dd>{formatDateTime(record.expiresAt)}</dd>
              </div>
              <div>
                <dt className="font-medium text-[var(--color-text-primary)]">Retention</dt>
                <dd>{retentionStatus?.status ?? 'Not assigned'}</dd>
              </div>
              <div>
                <dt className="font-medium text-[var(--color-text-primary)]">Legal hold</dt>
                <dd>{activeLegalHold}</dd>
              </div>
            </dl>
          </div>
        </section>

        <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Approved print notes</h3>
          <ul className="mt-3 space-y-2 text-sm text-[var(--color-text-secondary)]">
            <li>{record.description}</li>
            <li>Tags: {record.tags.length > 0 ? record.tags.join(', ') : 'No tags on file'}</li>
            <li>Retention policy: {retentionStatus?.retentionPolicyRef ?? 'Not assigned'}</li>
            <li>Redaction events: {redactions.length}</li>
            <li>Generated by: {actorDisplayName || 'Authorized user'}</li>
          </ul>
        </section>

        <section className="grid gap-4 md:grid-cols-2">
          <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
            <h3 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Files</h3>
            <ul className="mt-3 space-y-2 text-sm text-[var(--color-text-secondary)]">
              {visibleFiles.length > 0 ? visibleFiles.map((file) => (
                <li key={file.fileId}>{file.originalFilename} ({file.mimeType})</li>
              )) : <li>No files on record.</li>}
            </ul>
          </div>
          <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
            <h3 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Packages</h3>
            <ul className="mt-3 space-y-2 text-sm text-[var(--color-text-secondary)]">
              {recentPackages.length > 0 ? recentPackages.map((pkg) => (
                <li key={pkg.packageId}>{pkg.packageNumber} · {pkg.title} ({pkg.status})</li>
              )) : <li>No packages currently reference this record.</li>}
            </ul>
          </div>
        </section>

        <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
          <h3 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Access trail snapshot</h3>
          <ul className="mt-3 space-y-2 text-sm text-[var(--color-text-secondary)]">
            {recentAccess.length > 0 ? recentAccess.map((entry) => (
              <li key={entry.accessLogId}>
                {formatDateTime(entry.occurredAt)} · {entry.action} · {entry.result} · {formatActorLabel(entry, actorPersonId, actorDisplayName)}
              </li>
            )) : <li>No access events are available for this record.</li>}
          </ul>
        </section>
      </div>
    </PrintableDocumentShell>
  )
}
