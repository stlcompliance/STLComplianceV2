export function TimesheetDetailPage() {
  return (
    <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
      <p className="text-xs font-semibold uppercase tracking-[0.24em] text-sky-300">Timesheet detail</p>
      <h1 className="mt-3 text-3xl font-semibold text-slate-50">Worker timesheet workspace</h1>
      <p className="mt-3 max-w-3xl text-sm text-slate-300">
        This route is reserved for the worker-level timesheet detail experience, including approvals, exceptions,
        attestations, corrections, leave context, attendance context, and payroll-ready locking.
      </p>
    </div>
  )
}
