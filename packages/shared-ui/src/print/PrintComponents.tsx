import { useId, useState, type ReactNode } from 'react'

export function PrintablePageShell({
  title,
  subtitle,
  footer,
  children,
}: {
  title: string
  subtitle?: string
  footer?: ReactNode
  children: ReactNode
}) {
  return (
    <section className="stl-print-page mx-auto max-w-5xl rounded-xl border border-slate-200 bg-white p-6 text-slate-900 shadow-sm">
      <header className="border-b border-slate-200 pb-4">
        <h1 className="text-2xl font-semibold text-slate-950">{title}</h1>
        {subtitle ? <p className="mt-2 text-sm text-slate-600">{subtitle}</p> : null}
      </header>
      <div className="mt-6 space-y-6">{children}</div>
      {footer ? <footer className="mt-8 border-t border-slate-200 pt-4 text-sm text-slate-500">{footer}</footer> : null}
    </section>
  )
}

export function PrintableDocumentHeader({
  title,
  metadata,
}: {
  title: string
  metadata?: ReactNode
}) {
  return (
    <header className="mb-6 border-b border-slate-200 pb-4">
      <h2 className="text-xl font-semibold text-slate-950">{title}</h2>
      {metadata ? <div className="mt-2 text-sm text-slate-600">{metadata}</div> : null}
    </header>
  )
}

export function PrintableDocumentFooter({ children }: { children: ReactNode }) {
  return <footer className="mt-8 border-t border-slate-200 pt-4 text-sm text-slate-500">{children}</footer>
}

export function DraftWatermark({ label = 'Draft' }: { label?: string }) {
  return (
    <div
      aria-hidden
      className="pointer-events-none absolute inset-0 flex items-center justify-center overflow-hidden"
    >
      <span className="rotate-[-24deg] text-6xl font-bold uppercase tracking-[0.25em] text-slate-200/70">
        {label}
      </span>
    </div>
  )
}

function badgeClasses() {
  return 'inline-flex items-center rounded-full border border-slate-300 bg-white px-3 py-1 text-xs font-semibold uppercase tracking-wide text-slate-700'
}

export function OfficialCopyBadge() {
  return <span className={badgeClasses()}>Official copy</span>
}

export function RedactedCopyBadge() {
  return <span className={badgeClasses()}>Redacted copy</span>
}

export function LabelPreview({
  title,
  children,
}: {
  title: string
  children: ReactNode
}) {
  return (
    <section className="inline-flex min-h-40 min-w-64 flex-col rounded-lg border border-slate-300 bg-white p-4 text-slate-900 shadow-sm">
      <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">{title}</p>
      <div className="mt-3 flex-1">{children}</div>
    </section>
  )
}

export function PacketPreview({
  title,
  sections,
}: {
  title: string
  sections: Array<{ title: string; content: ReactNode }>
}) {
  return (
    <section className="rounded-xl border border-slate-200 bg-white p-6 text-slate-900 shadow-sm">
      <h2 className="text-xl font-semibold text-slate-950">{title}</h2>
      <div className="mt-4 space-y-4">
        {sections.map((section) => (
          <article key={section.title} className="rounded-lg border border-slate-200 p-4">
            <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-500">
              {section.title}
            </h3>
            <div className="mt-3">{section.content}</div>
          </article>
        ))}
      </div>
    </section>
  )
}

type ActionButtonProps = {
  label: string
  busyLabel?: string
  isPending?: boolean
  disabled?: boolean
  onClick?: () => void
}

function ActionButton({
  label,
  busyLabel,
  isPending = false,
  disabled = false,
  onClick,
}: ActionButtonProps) {
  return (
    <button
      type="button"
      className="inline-flex items-center rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900 shadow-sm transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
      onClick={onClick}
      disabled={disabled || isPending}
    >
      {isPending ? busyLabel ?? label : label}
    </button>
  )
}

export function DownloadPdfButton(props: Omit<ActionButtonProps, 'busyLabel'>) {
  return <ActionButton {...props} busyLabel="Preparing PDF…" />
}

export function ArchiveOfficialCopyButton(props: Omit<ActionButtonProps, 'busyLabel'>) {
  return <ActionButton {...props} busyLabel="Archiving…" />
}

export function ReprintReasonDialog({
  open,
  title = 'Reason required',
  confirmLabel = 'Continue',
  onClose,
  onConfirm,
}: {
  open: boolean
  title?: string
  confirmLabel?: string
  onClose: () => void
  onConfirm: (reason: string) => void
}) {
  const reasonId = useId()
  const [reason, setReason] = useState('')

  if (!open) {
    return null
  }

  return (
    <div className="rounded-xl border border-slate-300 bg-white p-4 text-slate-900 shadow-lg" data-print-hide>
      <h3 className="text-base font-semibold text-slate-950">{title}</h3>
      <label htmlFor={reasonId} className="mt-3 block text-sm text-slate-700">
        Reprint reason
      </label>
      <textarea
        id={reasonId}
        value={reason}
        onChange={(event) => setReason(event.target.value)}
        rows={4}
        className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900"
      />
      <div className="mt-4 flex flex-wrap gap-2">
        <button
          type="button"
          className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900"
          onClick={onClose}
        >
          Cancel
        </button>
        <button
          type="button"
          className="rounded-lg border border-sky-700 bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
          disabled={!reason.trim()}
          onClick={() => {
            onConfirm(reason.trim())
            setReason('')
          }}
        >
          {confirmLabel}
        </button>
      </div>
    </div>
  )
}

export function PrintPreviewRoute({
  title,
  subtitle,
  actions,
  children,
}: {
  title: string
  subtitle?: string
  actions?: ReactNode
  children: ReactNode
}) {
  return (
    <div className="space-y-4" data-print-preview="true">
      {actions ? <div className="flex flex-wrap gap-2" data-print-hide>{actions}</div> : null}
      <PrintablePageShell title={title} subtitle={subtitle}>
        {children}
      </PrintablePageShell>
    </div>
  )
}
