export function PayrollPage() {
  return (
    <div className="ledgarr-page">
      <div className="rounded-2xl border border-slate-700/70 bg-slate-950/70 p-6">
        <p className="ledgarr-label">LedgArr payroll</p>
        <h1 className="mt-3 text-3xl font-semibold text-slate-50">Payroll preparation and export</h1>
        <p className="mt-3 max-w-4xl text-sm text-slate-300">
          LedgArr owns payroll calendars, payroll batches, code mappings, provider export packets, and payroll journal
          snapshots. StaffArr remains the source of truth for worker time, approvals, and pay-policy classification.
        </p>
      </div>
    </div>
  )
}

