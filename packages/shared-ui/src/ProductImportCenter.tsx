import type { ReactNode } from 'react'

export type ProductImportCenterTabKey =
  | 'import'
  | 'templates'
  | 'history'
  | 'saved-mappings'
  | 'help'
  | (string & {})

export interface ProductImportManifest {
  productKey: string
  importTypeKey: string
  displayName: string
  description: string
  supportedFileTypes: string[]
  templateVersion: string
  requiredPermission: string
  targetEntity: string
  allowedOperations: string[]
  requiredColumns: string[]
  optionalColumns: string[]
  controlledVocabularyColumns: string[]
  referenceColumns: string[]
  uniquenessRules: string[]
  duplicateDetectionRules: string[]
  validationRules: string[]
  previewColumns: string[]
  commitBehavior: string
  emittedEvents: string[]
  rollbackSupport: boolean
  auditCategory: string
}

export interface ProductImportHistoryEntry {
  importHistoryId: string
  importTypeKey: string
  displayName: string
  status: string
  dryRun: boolean
  rowCount: number
  successCount: number
  errorCount: number
  actorUserId?: string | null
  actorDisplayName?: string | null
  occurredAt: string
  summary?: string | null
}

export interface ProductImportCenterTab {
  key: ProductImportCenterTabKey
  label: string
  content: ReactNode
  badge?: string | number
  disabled?: boolean
}

export function ProductImportCenter({
  title = 'Import center',
  description,
  manifests,
  selectedImportTypeKey,
  onSelectImportType,
  tabs,
  activeTabKey,
  onSelectTab,
}: {
  title?: string
  description?: string
  manifests: readonly ProductImportManifest[]
  selectedImportTypeKey?: string | null
  onSelectImportType?: (importTypeKey: string) => void
  tabs: readonly ProductImportCenterTab[]
  activeTabKey: ProductImportCenterTabKey
  onSelectTab?: (tabKey: ProductImportCenterTabKey) => void
}) {
  const selectedManifest =
    manifests.find((manifest) => manifest.importTypeKey === selectedImportTypeKey) ?? manifests[0] ?? null
  const activeTab = tabs.find((tab) => tab.key === activeTabKey) ?? tabs[0] ?? null

  return (
    <div className="space-y-6">
      <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 shadow-sm shadow-slate-950/10">
        <div className="flex flex-col gap-5 xl:grid xl:grid-cols-[minmax(0,1.45fr)_minmax(19rem,0.95fr)]">
          <div className="space-y-4">
            <div className="space-y-1">
              <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">{title}</h2>
              {description ? (
                <p className="text-sm leading-6 text-[var(--color-text-muted)]">{description}</p>
              ) : null}
            </div>

            {manifests.length > 0 ? (
              <div className="grid gap-3 md:grid-cols-2">
                {manifests.map((manifest) => {
                  const isSelected = manifest.importTypeKey === selectedManifest?.importTypeKey
                  return (
                    <button
                      key={manifest.importTypeKey}
                      type="button"
                      onClick={() => onSelectImportType?.(manifest.importTypeKey)}
                      className={`rounded-xl border p-4 text-left transition ${
                        isSelected
                          ? 'border-[var(--color-accent-border)] bg-[var(--color-accent-soft)]'
                          : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] hover:border-[var(--color-accent-border)] hover:bg-[var(--color-bg-control-hover)]'
                      }`}
                      data-selected={isSelected}
                    >
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <p className="text-sm font-semibold text-[var(--color-text-primary)]">
                            {manifest.displayName}
                          </p>
                          <p className="mt-1 text-xs uppercase tracking-wide text-[var(--color-accent)]">
                            {manifest.importTypeKey}
                          </p>
                        </div>
                        <span className="rounded-full border border-[var(--color-border-subtle)] px-2 py-1 text-[11px] uppercase tracking-wide text-[var(--color-text-secondary)]">
                          v{manifest.templateVersion}
                        </span>
                      </div>
                      <p className="mt-3 text-sm text-[var(--color-text-muted)]">
                        {manifest.description}
                      </p>
                      <div className="mt-3 flex flex-wrap gap-2">
                        {manifest.supportedFileTypes.map((fileType) => (
                          <span
                            key={fileType}
                            className="rounded-full border border-[var(--color-border-subtle)] px-2 py-1 text-[11px] uppercase tracking-wide text-[var(--color-text-secondary)]"
                          >
                            {fileType}
                          </span>
                        ))}
                      </div>
                    </button>
                  )
                })}
              </div>
            ) : (
              <div className="rounded-xl border border-dashed border-[var(--color-border-subtle)] p-4 text-sm text-[var(--color-text-muted)]">
                No product-local import types are registered yet.
              </div>
            )}
          </div>

          <aside className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
            {selectedManifest ? (
              <div className="space-y-4">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-wide text-[var(--color-accent)]">
                    Selected import type
                  </p>
                  <h3 className="mt-1 text-base font-semibold text-[var(--color-text-primary)]">
                    {selectedManifest.displayName}
                  </h3>
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">
                    {selectedManifest.description}
                  </p>
                </div>

                <dl className="grid gap-3 sm:grid-cols-2 xl:grid-cols-1">
                  <ImportDetail label="Target entity" value={selectedManifest.targetEntity} />
                  <ImportDetail
                    label="Operations"
                    value={selectedManifest.allowedOperations.join(', ') || 'create'}
                  />
                  <ImportDetail
                    label="Required columns"
                    value={String(selectedManifest.requiredColumns.length)}
                  />
                  <ImportDetail
                    label="Reference columns"
                    value={String(selectedManifest.referenceColumns.length)}
                  />
                  <ImportDetail
                    label="Rollback"
                    value={selectedManifest.rollbackSupport ? 'Supported' : 'Product-specific'}
                  />
                  <ImportDetail label="Audit category" value={selectedManifest.auditCategory} />
                </dl>
              </div>
            ) : (
              <p className="text-sm text-[var(--color-text-muted)]">
                Select a product-owned import type to review its schema, template, validation, and history.
              </p>
            )}
          </aside>
        </div>
      </section>

      <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] shadow-sm shadow-slate-950/10">
        <nav
          aria-label="Import center tabs"
          className="flex flex-wrap gap-2 border-b border-[var(--color-border-subtle)] px-4 py-3"
        >
          {tabs.map((tab) => {
            const isActive = tab.key === activeTabKey
            return (
              <button
                key={tab.key}
                type="button"
                disabled={tab.disabled}
                onClick={() => {
                  if (!tab.disabled) {
                    onSelectTab?.(tab.key)
                  }
                }}
                className={`inline-flex items-center gap-2 rounded-full border px-3 py-1.5 text-sm transition ${
                  isActive
                    ? 'border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] text-[var(--color-text-primary)]'
                    : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] text-[var(--color-text-secondary)] hover:border-[var(--color-accent-border)] hover:text-[var(--color-text-primary)]'
                } ${tab.disabled ? 'cursor-not-allowed opacity-60' : ''}`}
                aria-current={isActive ? 'page' : undefined}
              >
                <span>{tab.label}</span>
                {tab.badge !== undefined ? (
                  <span className="rounded-full border border-[var(--color-border-subtle)] px-2 py-0.5 text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">
                    {tab.badge}
                  </span>
                ) : null}
              </button>
            )
          })}
        </nav>

        <div className="p-4 sm:p-5">{activeTab?.content}</div>
      </section>
    </div>
  )
}

function ImportDetail({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3">
      <dt className="text-[11px] font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
        {label}
      </dt>
      <dd className="mt-1 text-sm text-[var(--color-text-primary)]">{value}</dd>
    </div>
  )
}
