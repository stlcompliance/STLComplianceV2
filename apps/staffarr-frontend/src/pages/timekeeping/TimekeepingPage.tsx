export function TimekeepingPage() {
  return (
    <div className="space-y-6">
      <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
        <p className="text-xs font-semibold uppercase tracking-[0.24em] text-sky-300">StaffArr timekeeping</p>
        <h1 className="mt-3 text-3xl font-semibold text-slate-50">Time capture, review, and payroll readiness</h1>
        <p className="mt-3 max-w-4xl text-sm text-slate-300">
          Manage worker timekeeping, leave, attendance, timesheets, approvals, pay policy assignment, labor allocations,
          correction history, and payroll locking.
        </p>
      </div>

      <div className="grid gap-4 xl:grid-cols-3">
        {[
          ['My time', 'Clock events, work sessions, leave requests, and personal review flow.'],
          ['Team review', 'Supervisor approval, exceptions, attendance points, and correction routing.'],
          ['Payroll handoff', 'Payroll-ready periods and leave states ready for export.'],
        ].map(([title, body]) => (
          <section key={title} className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <h2 className="text-lg font-semibold text-slate-50">{title}</h2>
            <p className="mt-2 text-sm text-slate-300">{body}</p>
          </section>
        ))}
      </div>

      <div className="grid gap-4 xl:grid-cols-4">
        {[
          ['Availability', 'Recurring availability blocks and schedule preferences.'],
          ['Leave', 'PTO, sick, jury, military, and other leave lifecycle records.'],
          ['Attendance', 'Tardy, absence, no-call/no-show, and point tracking.'],
          ['Manager view', 'Team coverage, approvals, and lock status.'],
        ].map(([title, body]) => (
          <section key={title} className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <h2 className="text-lg font-semibold text-slate-50">{title}</h2>
            <p className="mt-2 text-sm text-slate-300">{body}</p>
          </section>
        ))}
      </div>
    </div>
  )
}
