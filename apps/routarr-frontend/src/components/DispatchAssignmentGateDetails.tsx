import {
  buildDispatchAssignmentGateLines,
  type DispatchGateLine,
  type DispatchGateLineSeverity,
} from '../lib/dispatchGateMessaging'
import type { DispatchAssignmentPreviewResponse } from '../api/types'

type Props = {
  preview: DispatchAssignmentPreviewResponse | null | undefined
  title?: string
  compact?: boolean
  'data-testid'?: string
}

function severityClass(severity: DispatchGateLineSeverity): string {
  switch (severity) {
    case 'block':
      return 'text-red-200'
    case 'warn':
      return 'text-amber-200'
    default:
      return 'text-emerald-200'
  }
}

function GateLineRow({ line, compact }: { line: DispatchGateLine; compact?: boolean }) {
  return (
    <li className={`${severityClass(line.severity)} ${compact ? 'text-xs' : 'text-sm'}`}>
      <span className="font-medium text-slate-300">{line.label}</span>
      <span className="text-slate-400"> — </span>
      <span>{line.detail}</span>
      {line.reasonCode ? (
        <span className="ml-1 font-mono text-[10px] text-slate-500">{line.reasonCode}</span>
      ) : null}
    </li>
  )
}

export function DispatchAssignmentGateDetails({
  preview,
  title,
  compact,
  'data-testid': testId,
}: Props) {
  if (!preview) {
    return null
  }

  const lines = buildDispatchAssignmentGateLines(preview)
  const blocking = lines.some((line) => line.severity === 'block')
  const warning = lines.some((line) => line.severity === 'warn')

  return (
    <div
      className={`rounded border p-2 ${
        blocking
          ? 'border-red-700/60 bg-red-950/25'
          : warning
            ? 'border-amber-700/60 bg-amber-950/20'
            : 'border-emerald-700/50 bg-emerald-950/15'
      }`}
      data-testid={testId}
    >
      {title ? <p className="mb-1 text-xs font-medium text-slate-400">{title}</p> : null}
      <ul className="space-y-0.5">
        {lines.map((line, index) => (
          <GateLineRow key={`${line.category}-${line.label}-${index}`} line={line} compact={compact} />
        ))}
      </ul>
    </div>
  )
}
