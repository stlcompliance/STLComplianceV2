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
        'flex items-center justify-center rounded-lg border border-slate-700/70 bg-slate-900/60 px-4 py-8',
        fullScreen ? 'min-h-screen rounded-none border-0' : 'min-h-[16rem]',
      ].join(' ')}
      role="status"
      aria-live="polite"
    >
      <span className="text-sm text-slate-400">{label}</span>
    </div>
  )
}
