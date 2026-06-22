import { useQuery } from '@tanstack/react-query'
import {
  ApiErrorCallout,
  ProductImportCenter,
  type ProductImportHistoryEntry,
  type ProductImportManifest,
  type ProductImportCenterTab,
  type ProductImportCenterTabKey,
} from '@stl/shared-ui'
import { useState } from 'react'
import {
  downloadImportTemplate,
  listImportHistory,
  listImportManifests,
} from '../../api/client'
import { PersonBulkImportPanel } from '../../components/PersonBulkImportPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function ImportsSection({ state }: Props) {
  const canUseImportCenter = state.canManagePeopleProfiles
  const [selectedImportTypeKey, setSelectedImportTypeKey] = useState<string | null>(null)
  const [activeTabKey, setActiveTabKey] = useState<ProductImportCenterTabKey>('import')
  const [templateError, setTemplateError] = useState<string | null>(null)
  const [templateDownloadPending, setTemplateDownloadPending] = useState(false)

  const manifestsQuery = useQuery({
    queryKey: ['staffarr-import-manifests', state.accessToken],
    queryFn: () => listImportManifests(state.accessToken),
    enabled: Boolean(state.accessToken && canUseImportCenter),
  })
  const manifests = manifestsQuery.data ?? []
  const selectedManifest =
    manifests.find((manifest) => manifest.importTypeKey === selectedImportTypeKey) ?? manifests[0] ?? null

  const historyQuery = useQuery({
    queryKey: ['staffarr-import-history', state.accessToken],
    queryFn: () => listImportHistory(state.accessToken),
    enabled: Boolean(state.accessToken && canUseImportCenter),
  })
  const historyItems = (historyQuery.data ?? []).filter(
    (item) => !selectedManifest || item.importTypeKey === selectedManifest.importTypeKey,
  )

  const handleDownloadTemplate = async () => {
    if (!selectedManifest) {
      return
    }

    setTemplateError(null)
    setTemplateDownloadPending(true)
    try {
      const blob = await downloadImportTemplate(state.accessToken, selectedManifest.importTypeKey)
      downloadBlob(
        blob,
        `${selectedManifest.productKey}-${selectedManifest.importTypeKey}-template-v${selectedManifest.templateVersion}.csv`,
      )
    } catch (error) {
      setTemplateError(error instanceof Error ? error.message : 'Failed to download template.')
    } finally {
      setTemplateDownloadPending(false)
    }
  }

  const tabs: ProductImportCenterTab[] = [
    {
      key: 'import',
      label: 'Import',
      content: !canUseImportCenter ? (
        <PermissionNotice />
      ) : manifestsQuery.isLoading ? (
        <SurfaceMessage message="Loading StaffArr import types…" />
      ) : selectedManifest?.importTypeKey === 'people' ? (
        <div className="space-y-4">
          <ProductGuidance
            title="StaffArr owns person and workforce identity"
            body="Run deterministic imports here when you already know the destination shape. StaffArr validates each row, resolves known references, and records audit history before any profile writes happen."
          />
          <PersonBulkImportPanel
            accessToken={state.accessToken}
            canImport={canUseImportCenter}
            onComplete={() => {
              void state.peopleQuery.refetch()
              void state.personLookupQuery.refetch()
              void historyQuery.refetch()
            }}
          />
        </div>
      ) : selectedManifest ? (
        <SurfaceMessage
          message={`${selectedManifest.displayName} is registered for StaffArr, but its interactive import step has not been enabled in this workspace yet.`}
        />
      ) : (
        <SurfaceMessage message="No StaffArr import manifests are currently available." />
      ),
    },
    {
      key: 'templates',
      label: 'Templates',
      badge: selectedManifest ? `v${selectedManifest.templateVersion}` : undefined,
      content: !canUseImportCenter ? (
        <PermissionNotice />
      ) : selectedManifest ? (
        <div className="space-y-4">
          <div className="flex flex-wrap items-start justify-between gap-3 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
            <div>
              <h3 className="text-base font-semibold text-[var(--color-text-primary)]">
                {selectedManifest.displayName} template
              </h3>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Template version {selectedManifest.templateVersion} · Permission {selectedManifest.requiredPermission}
              </p>
            </div>
            <button
              type="button"
              onClick={() => {
                void handleDownloadTemplate()
              }}
              disabled={templateDownloadPending}
              className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-slate-950 transition hover:bg-[var(--color-accent-hover)] disabled:cursor-not-allowed disabled:opacity-60"
            >
              {templateDownloadPending ? 'Preparing template…' : 'Download template'}
            </button>
          </div>

          {templateError ? (
            <ApiErrorCallout title="Template download failed" message={templateError} />
          ) : null}

          <div className="grid gap-4 xl:grid-cols-2">
            <ManifestFieldList title="Required columns" fields={selectedManifest.requiredColumns} />
            <ManifestFieldList title="Optional columns" fields={selectedManifest.optionalColumns} />
            <ManifestFieldList title="Reference columns" fields={selectedManifest.referenceColumns} />
            <ManifestFieldList
              title="Controlled vocabulary"
              fields={selectedManifest.controlledVocabularyColumns}
            />
          </div>

          <ManifestNotes manifest={selectedManifest} />
        </div>
      ) : (
        <SurfaceMessage message="Select an import type to review its template columns and download package." />
      ),
    },
    {
      key: 'history',
      label: 'History',
      badge: historyItems.length,
      content: !canUseImportCenter ? (
        <PermissionNotice />
      ) : historyQuery.isLoading ? (
        <SurfaceMessage message="Loading StaffArr import history…" />
      ) : historyQuery.error instanceof Error ? (
        <ApiErrorCallout
          title="Import history unavailable"
          message={historyQuery.error.message}
        />
      ) : (
        <ImportHistoryList items={historyItems} emptyMessage="No StaffArr imports have been recorded yet." />
      ),
    },
    {
      key: 'saved-mappings',
      label: 'Saved mappings',
      content: (
        <SurfaceMessage message="Saved mappings are not yet enabled for StaffArr product imports. When added, mappings will stay product-scoped and reviewable before reuse." />
      ),
    },
    {
      key: 'help',
      label: 'Help',
      content: (
        <div className="grid gap-4 xl:grid-cols-2">
          <ProductGuidance
            title="Deterministic validation is authoritative"
            body="AI may eventually help suggest mappings or cleanup, but StaffArr import validation remains the final authority for required fields, duplicate checks, and business rules."
          />
          <ProductGuidance
            title="Cross-product references stay resolved"
            body="If a workforce import needs another product’s data, StaffArr resolves the reference through the owning product’s APIs or known public keys. It does not silently create records in other products."
          />
          <ProductGuidance
            title="Use Smart Import only for unknown files"
            body="Global Smart Import remains the AI-assisted intake flow for mixed or unknown files. Use this Import Center when you already know you are importing a StaffArr-owned record type."
          />
          <ProductGuidance
            title="History stays auditable"
            body="Import history records who ran the batch, when it ran, whether it was validation-only, and the row-level outcome summary before and after final commit."
          />
        </div>
      ),
    },
  ]

  return (
    <div className="space-y-4">
      {manifestsQuery.error instanceof Error ? (
        <ApiErrorCallout title="Import center unavailable" message={manifestsQuery.error.message} />
      ) : null}
      <ProductImportCenter
        title="StaffArr import center"
        description={
          canUseImportCenter
            ? 'Choose a StaffArr-owned import type, validate it deterministically, and commit through the product’s normal services.'
            : 'Your current role does not have access to StaffArr product imports.'
        }
        manifests={manifests}
        selectedImportTypeKey={selectedManifest?.importTypeKey ?? null}
        onSelectImportType={setSelectedImportTypeKey}
        tabs={tabs}
        activeTabKey={activeTabKey}
        onSelectTab={setActiveTabKey}
      />
    </div>
  )
}

function ManifestFieldList({ title, fields }: { title: string; fields: readonly string[] }) {
  return (
    <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
      <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">{title}</h3>
      {fields.length > 0 ? (
        <div className="mt-3 flex flex-wrap gap-2">
          {fields.map((field) => (
            <span
              key={field}
              className="rounded-full border border-[var(--color-border-subtle)] px-3 py-1 text-xs text-[var(--color-text-secondary)]"
            >
              {field}
            </span>
          ))}
        </div>
      ) : (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">No additional fields are declared.</p>
      )}
    </section>
  )
}

function ManifestNotes({ manifest }: { manifest: ProductImportManifest }) {
  return (
    <section className="grid gap-4 xl:grid-cols-2">
      <ProductGuidance
        title="Allowed operations"
        body={manifest.allowedOperations.join(', ') || 'create'}
      />
      <ProductGuidance
        title="Duplicate detection"
        body={manifest.duplicateDetectionRules.join('; ') || 'Product-local duplicate detection applies.'}
      />
      <ProductGuidance
        title="Validation rules"
        body={manifest.validationRules.join('; ') || 'Required field and product validation rules apply.'}
      />
      <ProductGuidance
        title="Events and audit"
        body={`${manifest.emittedEvents.join(', ')} · Audit category ${manifest.auditCategory}`}
      />
    </section>
  )
}

function ImportHistoryList({
  items,
  emptyMessage,
}: {
  items: readonly ProductImportHistoryEntry[]
  emptyMessage: string
}) {
  if (items.length === 0) {
    return <SurfaceMessage message={emptyMessage} />
  }

  return (
    <div className="space-y-3">
      {items.map((item) => (
        <article
          key={item.importHistoryId}
          className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4"
        >
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">
                {item.displayName}
              </h3>
              <p className="mt-1 text-xs uppercase tracking-wide text-[var(--color-accent)]">
                {item.status}
                {item.dryRun ? ' · validation only' : ''}
              </p>
            </div>
            <span className="text-xs text-[var(--color-text-muted)]">
              {formatOccurredAt(item.occurredAt)}
            </span>
          </div>
          <dl className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            <HistoryMetric label="Rows" value={String(item.rowCount)} />
            <HistoryMetric label="Successful" value={String(item.successCount)} />
            <HistoryMetric label="Errors" value={String(item.errorCount)} />
            <HistoryMetric
              label="Actor"
              value={item.actorDisplayName ?? item.actorUserId ?? 'Unknown'}
            />
          </dl>
          {item.summary ? (
            <p className="mt-3 text-sm text-[var(--color-text-muted)]">{item.summary}</p>
          ) : null}
        </article>
      ))}
    </div>
  )
}

function HistoryMetric({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-[11px] font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
        {label}
      </dt>
      <dd className="mt-1 text-sm text-[var(--color-text-primary)]">{value}</dd>
    </div>
  )
}

function ProductGuidance({ title, body }: { title: string; body: string }) {
  return (
    <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
      <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">{title}</h3>
      <p className="mt-2 text-sm leading-6 text-[var(--color-text-muted)]">{body}</p>
    </section>
  )
}

function PermissionNotice() {
  return (
    <SurfaceMessage message="StaffArr imports require tenant admin, StaffArr admin, or HR admin access for this workspace." />
  )
}

function SurfaceMessage({ message }: { message: string }) {
  return (
    <div className="rounded-xl border border-dashed border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4 text-sm text-[var(--color-text-muted)]">
      {message}
    </div>
  )
}

function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  anchor.rel = 'noopener'
  anchor.click()
  setTimeout(() => URL.revokeObjectURL(url), 1000)
}

function formatOccurredAt(value: string) {
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? value : parsed.toLocaleString()
}
