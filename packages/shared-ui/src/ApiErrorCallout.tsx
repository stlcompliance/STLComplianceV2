import type { ReactNode } from 'react'

type ApiErrorCalloutTone = 'error' | 'warning' | 'info'

const toneStyles: Record<ApiErrorCalloutTone, string> = {
  error: 'border-red-800/60 bg-red-950/20 text-red-200',
  warning: 'border-amber-800/60 bg-amber-950/20 text-amber-200',
  info: 'border-sky-800/60 bg-sky-950/20 text-sky-200',
}

export function getErrorMessage(error: unknown, fallback = 'Something went wrong.'): string {
  if (error instanceof Error && error.message.trim()) {
    return error.message
  }
  if (typeof error === 'string' && error.trim()) {
    return error
  }
  return fallback
}

export function ApiErrorCallout({
  message,
  title = 'Unable to load data',
  tone = 'error',
  retryLabel = 'Retry',
  onRetry,
  footer,
  className,
  testId,
}: {
  message: string
  title?: string
  tone?: ApiErrorCalloutTone
  retryLabel?: string
  onRetry?: () => void
  footer?: ReactNode
  className?: string
  testId?: string
}) {
  const classes = ['rounded-md border p-3 text-sm', toneStyles[tone], className]
    .filter(Boolean)
    .join(' ')

  return (
    <div className={classes} role="alert" data-testid={testId}>
      <p className="font-medium">{title}</p>
      <p className="mt-1">{message}</p>
      {onRetry || footer ? (
        <div className="mt-2 flex flex-wrap items-center gap-2">
          {onRetry ? (
            <button
              type="button"
              className="rounded border border-current/40 px-2 py-1 text-xs hover:bg-[var(--color-bg-control-hover)]"
              onClick={onRetry}
            >
              {retryLabel}
            </button>
          ) : null}
          {footer}
        </div>
      ) : null}
    </div>
  )
}
