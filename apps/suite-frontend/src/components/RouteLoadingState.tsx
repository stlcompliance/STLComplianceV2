export function RouteLoadingState({
  label = 'Loading page…',
  fullScreen = false,
}: {
  label?: string
  fullScreen?: boolean
}) {
  return (
    <div
      className={[
        'flex items-center justify-center rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-4 py-8 text-[var(--color-text-secondary)] shadow-sm',
        fullScreen ? 'min-h-screen rounded-none border-0' : 'min-h-[16rem]',
      ].join(' ')}
      role="status"
      aria-live="polite"
    >
      <span className="text-sm">{label}</span>
    </div>
  )
}
