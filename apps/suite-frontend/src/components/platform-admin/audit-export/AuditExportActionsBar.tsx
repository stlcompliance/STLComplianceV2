type Props = {
  exportBusy: boolean
  zipPending: boolean
  csvPending: boolean
  jsonFilePending: boolean
  backgroundPending: boolean
  previewPending: boolean
  onZip: () => void
  onCsv: () => void
  onJsonFile: () => void
  onBackgroundZip: () => void
  onPreviewJson: () => void
}

export function AuditExportActionsBar(props: Props) {
  return (
    <div className="flex flex-wrap gap-3">
      <button
        type="button"
        onClick={props.onZip}
        disabled={props.exportBusy}
        className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
      >
        {props.zipPending ? 'Exporting…' : 'Download ZIP package'}
      </button>
      <button
        type="button"
        onClick={props.onCsv}
        disabled={props.exportBusy}
        data-testid="platform-audit-download-csv"
        className="rounded-md bg-[var(--color-success-bg)] px-4 py-2 text-sm font-medium text-[var(--color-success-text)] hover:bg-[var(--color-success-bg)] disabled:opacity-50"
      >
        {props.csvPending ? 'Exporting…' : 'Download audit CSV'}
      </button>
      <button
        type="button"
        onClick={props.onJsonFile}
        disabled={props.exportBusy}
        data-testid="platform-audit-download-json"
        className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
      >
        {props.jsonFilePending ? 'Exporting…' : 'Download JSON package'}
      </button>
      <button
        type="button"
        onClick={props.onBackgroundZip}
        disabled={props.exportBusy}
        className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
      >
        {props.backgroundPending ? 'Background export…' : 'Background ZIP export'}
      </button>
      <button
        type="button"
        onClick={props.onPreviewJson}
        disabled={props.previewPending}
        className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
      >
        {props.previewPending ? 'Loading…' : 'Preview JSON export'}
      </button>
    </div>
  )
}
