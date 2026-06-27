import { useState } from 'react'
import { RefreshCw, ShieldCheck } from 'lucide-react'

import { formatWhen } from '../lib/fieldInbox'
import {
  buildDeviceCapabilitySnapshot,
  formatDeviceCapabilityDiagnosticSummary,
  type DeviceCapabilityItem,
  type DeviceCapabilityStatus,
} from '../lib/deviceCapabilities'

interface DeviceCapabilityPanelProps {
  subtitle?: string
  title?: string
}

export function DeviceCapabilityPanel({
  subtitle = 'Camera, location, storage, connection, and permission checks for the current browser.',
  title = 'Device readiness',
}: DeviceCapabilityPanelProps) {
  const [snapshot, setSnapshot] = useState(() => buildDeviceCapabilitySnapshot())
  const [copyStatus, setCopyStatus] = useState<string | null>(null)

  const unsupportedCount = snapshot.capabilities.filter((item) => item.status !== 'ready').length
  const bannerTestId =
    unsupportedCount > 0
      ? 'fieldcompanion-device-capability-warning'
      : 'fieldcompanion-device-capability-summary'
  const summaryCopy =
    unsupportedCount > 0
    ? `${unsupportedCount} capabilit${unsupportedCount === 1 ? 'y' : 'ies'} need fallback paths.`
      : 'This device is ready for field capture, offline queueing, push delivery, and adaptive uploads.'
  const diagnosticSummary = formatDeviceCapabilityDiagnosticSummary(snapshot)

  const handleRefreshDiagnostics = () => {
    setCopyStatus(null)
    setSnapshot(buildDeviceCapabilitySnapshot())
  }

  const handleCopyDiagnostics = async () => {
    setCopyStatus(null)

    try {
      if (!navigator.clipboard?.writeText) {
        throw new Error('Clipboard unavailable')
      }

      await navigator.clipboard.writeText(diagnosticSummary)
      setCopyStatus('Diagnostic summary copied to clipboard.')
    } catch {
      setCopyStatus('Copy failed. Select the device diagnostics manually if needed.')
    }
  }

  return (
    <section
      className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="fieldcompanion-device-capability-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex items-center gap-2">
          <ShieldCheck className="mt-0.5 h-5 w-5 text-teal-300" aria-hidden />
          <div>
            <h2 className="text-lg font-semibold text-white">{title}</h2>
            <p className="text-sm text-slate-400">{subtitle}</p>
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            className="inline-flex min-h-11 items-center gap-2 rounded-lg border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-500"
            onClick={handleRefreshDiagnostics}
            data-testid="fieldcompanion-device-capability-refresh"
          >
            <RefreshCw className="h-4 w-4" aria-hidden />
            Refresh diagnostics
          </button>
          <button
            type="button"
            className="inline-flex min-h-11 items-center gap-2 rounded-lg border border-teal-500 px-4 py-2 text-sm font-medium text-teal-100 hover:border-teal-400"
            onClick={() => {
              void handleCopyDiagnostics()
            }}
            data-testid="fieldcompanion-device-capability-copy"
          >
            Copy diagnostic summary
          </button>
        </div>
      </div>

      <div
        className={`mt-4 rounded-xl border px-4 py-3 text-sm ${
          unsupportedCount > 0
            ? 'border-amber-500/40 bg-amber-950/30 text-amber-50'
            : 'border-emerald-500/30 bg-emerald-950/30 text-emerald-50'
        }`}
        data-testid={bannerTestId}
      >
        <p className="font-medium">{summaryCopy}</p>
        {unsupportedCount > 0 ? (
          <ul className="mt-2 list-disc space-y-1 pl-5 text-amber-50/90">
            {snapshot.warnings.slice(0, 4).map((warning) => (
              <li key={warning}>{warning}</li>
            ))}
          </ul>
        ) : (
          <p className="mt-1 text-emerald-50/90">No fallback guidance is needed right now.</p>
        )}
      </div>

      {copyStatus ? (
        <p
          className="mt-3 rounded-lg border border-slate-700 bg-slate-950/50 px-3 py-2 text-xs text-slate-300"
          data-testid="fieldcompanion-device-capability-copy-status"
        >
          {copyStatus}
        </p>
      ) : null}

      <dl className="mt-4 grid gap-3 sm:grid-cols-2">
        {snapshot.capabilities.map((item) => (
          <CapabilityCard key={item.key} item={item} />
        ))}
      </dl>

      <div className="mt-4 grid gap-2 text-xs text-slate-400 sm:grid-cols-2">
        <p data-testid="fieldcompanion-device-capability-check-time">
          Checked {formatWhen(snapshot.checkedAt)}
        </p>
        <p data-testid="fieldcompanion-device-capability-browser">
          {snapshot.browserUserAgent} · {snapshot.platform} · {snapshot.language} · Build {snapshot.appVersion}
        </p>
      </div>
    </section>
  )
}

function CapabilityCard({ item }: { item: DeviceCapabilityItem }) {
  return (
    <div
      className={`rounded-xl border px-4 py-3 ${
        item.status === 'ready'
          ? 'border-emerald-500/20 bg-emerald-950/20'
          : item.status === 'degraded'
            ? 'border-amber-500/30 bg-amber-950/20'
            : 'border-rose-500/30 bg-rose-950/20'
      }`}
      data-testid={`fieldcompanion-device-capability-item-${item.key}`}
    >
      <div className="flex items-start justify-between gap-3">
        <dt className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
          {item.label}
        </dt>
        <StatusPill status={item.status} />
      </div>
      <dd className="mt-1 text-sm text-slate-100">{item.value}</dd>
      <p className="mt-2 text-xs text-slate-400">{item.fallback}</p>
    </div>
  )
}

function StatusPill({ status }: { status: DeviceCapabilityStatus }) {
  const label =
    status === 'ready' ? 'Ready' : status === 'degraded' ? 'Needs fallback' : 'Unavailable'

  const className =
    status === 'ready'
      ? 'border-emerald-500/30 bg-emerald-950/60 text-emerald-100'
      : status === 'degraded'
        ? 'border-amber-500/30 bg-amber-950/60 text-amber-100'
        : 'border-rose-500/30 bg-rose-950/60 text-rose-100'

  return (
    <span
      className={`rounded-full border px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide ${className}`}
      data-testid={`fieldcompanion-device-capability-status-${status}`}
    >
      {label}
    </span>
  )
}
