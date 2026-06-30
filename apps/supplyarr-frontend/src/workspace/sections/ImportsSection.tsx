import { useQuery } from '@tanstack/react-query'
import {
  ApiErrorCallout,
  ProductImportCenter,
  getErrorMessage,
  type ProductImportCenterTab,
  type ProductImportCenterTabKey,
  type ProductImportHistoryEntry,
  type ProductImportManifest,
} from '@stl/shared-ui'
import { useState } from 'react'
import {
  downloadImportTemplate,
  listImportHistory,
  listImportManifests,
} from '../../api/client'
import { ContractsImportPanel } from '../../components/ContractsImportPanel'
import { GenericCsvImportPanel } from '../../components/GenericCsvImportPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function ImportsSection({ state }: Props) {
  const canUseImportCenter =
    state.canManage
    || state.canManageCatalog
    || state.canManageInv
    || state.canCreatePr
    || state.canApprovePr
    || state.canCreatePo
  const [selectedImportTypeKey, setSelectedImportTypeKey] = useState<string | null>(null)
  const [activeTabKey, setActiveTabKey] = useState<ProductImportCenterTabKey>('import')
  const [templateError, setTemplateError] = useState<string | null>(null)
  const [templateDownloadPending, setTemplateDownloadPending] = useState(false)

  const manifestsQuery = useQuery({
    queryKey: ['supplyarr-import-manifests', state.accessToken],
    queryFn: () => listImportManifests(state.accessToken),
    enabled: Boolean(state.accessToken && canUseImportCenter),
  })
  const manifests = manifestsQuery.data ?? []
  const selectedManifest =
    manifests.find((manifest) => manifest.importTypeKey === selectedImportTypeKey) ?? manifests[0] ?? null
  const canUseSelectedManifest = selectedManifest ? canManageManifest(state, selectedManifest) : canUseImportCenter

  const historyQuery = useQuery({
    queryKey: ['supplyarr-import-history', state.accessToken, selectedManifest?.importTypeKey ?? 'all'],
    queryFn: () => listImportHistory(state.accessToken, selectedManifest?.importTypeKey),
    enabled: Boolean(state.accessToken && canUseImportCenter),
  })
  const historyItems = historyQuery.data ?? []

  const refreshImportedData = () => {
    void Promise.all([
      state.suppliersQuery.refetch(),
      state.catalogsQuery.refetch(),
      state.partsQuery.refetch(),
      state.purchaseRequestsQuery.refetch(),
      state.purchaseOrdersQuery.refetch(),
      state.pricingSnapshotsQuery.refetch(),
      state.leadTimeSnapshotsQuery.refetch(),
      state.availabilitySnapshotsQuery.refetch(),
      state.reorderEvaluationQuery.refetch(),
      state.demandRefsQuery.refetch(),
      state.contractsQuery.refetch(),
      historyQuery.refetch(),
    ])
  }

  const handleDownloadTemplate = async () => {
    if (!selectedManifest || !canUseSelectedManifest) {
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
        <PermissionNotice message="Your current role does not have access to SupplyArr product imports." />
      ) : manifestsQuery.isLoading ? (
        <SurfaceMessage message="Loading SupplyArr import types…" />
      ) : !selectedManifest ? (
        <SurfaceMessage message="No SupplyArr import manifests are currently available." />
      ) : !canUseSelectedManifest ? (
        <PermissionNotice
          message={`You do not currently have the required permission for ${selectedManifest.displayName}. Required permission: ${selectedManifest.requiredPermission}.`}
        />
      ) : (
        <div className="space-y-4">
          <ProductGuidance
            title="Import types"
            body="Choose the exact import type first, validate it, and commit through the normal supplier directory, procurement, and catalog flows. References stay resolved instead of becoming free-text values."
          />
          {selectedManifest.importTypeKey === 'contracts_csv' ? (
            <ContractsImportPanel
              accessToken={state.accessToken}
              canManage={canUseSelectedManifest}
              onComplete={refreshImportedData}
            />
          ) : (
            <GenericCsvImportPanel
              accessToken={state.accessToken}
              manifest={selectedManifest}
              canManage={canUseSelectedManifest}
              onComplete={refreshImportedData}
            />
          )}
        </div>
      ),
    },
    {
      key: 'templates',
      label: 'Templates',
      badge: selectedManifest ? `v${selectedManifest.templateVersion}` : undefined,
      content: !canUseImportCenter ? (
        <PermissionNotice message="Your current role does not have access to SupplyArr product imports." />
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
              disabled={templateDownloadPending || !canUseSelectedManifest}
              className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] transition hover:bg-[var(--color-accent-hover)] disabled:cursor-not-allowed disabled:opacity-60"
            >
              {templateDownloadPending ? 'Preparing template…' : 'Download template'}
            </button>
          </div>

          {!canUseSelectedManifest ? (
            <PermissionNotice
              message={`Template download for this import type requires ${selectedManifest.requiredPermission}.`}
            />
          ) : null}

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
        <PermissionNotice message="Your current role does not have access to SupplyArr product imports." />
      ) : !canUseSelectedManifest && selectedManifest ? (
        <PermissionNotice
          message={`Import history for ${selectedManifest.displayName} requires ${selectedManifest.requiredPermission}.`}
        />
      ) : historyQuery.isLoading ? (
        <SurfaceMessage message="Loading SupplyArr import history…" />
      ) : historyQuery.error instanceof Error ? (
        <ApiErrorCallout
          title="Import history unavailable"
          message={getErrorMessage(historyQuery.error, 'Failed to load import history.')}
        />
      ) : (
        <ImportHistoryList items={historyItems} emptyMessage="No SupplyArr imports have been recorded yet." />
      ),
    },
    {
      key: 'saved-mappings',
      label: 'Saved mappings',
      content: (
        <SurfaceMessage message="Saved mappings are not yet enabled for SupplyArr product imports. Future mappings will stay product-scoped and auditable before reuse." />
      ),
    },
    {
      key: 'help',
      label: 'Help',
      content: (
        <div className="grid gap-4 xl:grid-cols-2">
          <ProductGuidance
            title="Pick the explicit import type first"
            body="Start with the exact destination type, such as supplier identities or sub-units, contacts, catalogs, contracts, price lists, or purchase history."
          />
          <ProductGuidance
            title="References stay resolved"
            body="If an import references another area of the suite, the reference stays with the selected record. It does not silently create a new record elsewhere."
          />
          <ProductGuidance
            title="Deterministic validation remains authoritative"
            body="AI may eventually help with cleanup suggestions or mapping hints, but validators, duplicate detection, and user review stay authoritative before any commit."
          />
          <ProductGuidance
            title="Use Smart Import only for unknown intake"
            body="Global Smart Import remains the suite-level intake path for unknown or mixed files. Use this Import Center when the destination record type is already known to belong in SupplyArr."
          />
        </div>
      ),
    },
  ]

  return (
    <div className="space-y-4">
      {manifestsQuery.error instanceof Error ? (
        <ApiErrorCallout
          title="Import center unavailable"
          message={getErrorMessage(manifestsQuery.error, 'Failed to load import manifests.')}
        />
      ) : null}
      <ProductImportCenter
        title="Import center"
        description={
          canUseImportCenter
            ? 'Choose an import type, validate it, and commit through the normal catalog, supplier directory, and procurement flows.'
            : 'Your current role does not have access to imports.'
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

function canManageManifest(state: SupplyArrWorkspaceState, manifest: ProductImportManifest) {
  switch (manifest.importTypeKey) {
    case 'external_parties_csv':
    case 'contacts_csv':
    case 'vendor_documents_csv':
      return state.canManage
    case 'part_catalog_csv':
    case 'vendor_catalog_csv':
      return state.canManageCatalog
    case 'inventory_counts_csv':
      return state.canManageInv
    case 'price_list_csv':
    case 'lead_time_list_csv':
    case 'availability_list_csv':
    case 'contracts_csv':
    case 'open_purchase_orders_csv':
    case 'purchase_history_csv':
      return state.canCreatePr || state.canApprovePr || state.canCreatePo
    default:
      return (
        state.canManage
        || state.canManageCatalog
        || state.canManageInv
        || state.canCreatePr
        || state.canApprovePr
        || state.canCreatePo
      )
  }
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

function PermissionNotice({ message }: { message: string }) {
  return <SurfaceMessage message={message} />
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
