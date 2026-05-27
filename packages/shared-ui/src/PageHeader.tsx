export function PageHeader({
  title,
  subtitle,
}: {
  title: string
  subtitle?: string
}) {
  return (
    <header className="mb-6 border-b border-slate-700 pb-4">
      <p className="text-xs uppercase tracking-wide text-slate-500">STL Compliance</p>
      <h1 className="mt-1 text-2xl font-semibold text-white">{title}</h1>
      {subtitle ? <p className="mt-1 text-sm text-slate-400">{subtitle}</p> : null}
    </header>
  )
}
