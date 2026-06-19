export function BenefitsCompensationPage() {
  return (
    <div className="space-y-6">
      <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
        <p className="text-xs font-semibold uppercase tracking-[0.24em] text-sky-300">StaffArr benefits and compensation</p>
        <h1 className="mt-3 text-3xl font-semibold text-slate-50">Eligibility, enrollments, dependents, and pay metadata</h1>
        <p className="mt-3 max-w-4xl text-sm text-slate-300">
          StaffArr keeps HR-facing benefits and compensation records in one place: coverage status, dependents,
          beneficiaries, pay basis, bands, rates, and change requests stay with the workforce record while payroll
          execution remains outside StaffArr.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {[
          ['Benefits', 'Eligibility, enrollment state, carrier export state, and open enrollment records.'],
          ['Dependents', 'Coverage-linked dependent records with relationship and status metadata.'],
          ['Beneficiaries', 'Allocation percentages and designation types for benefit-related plans.'],
          ['Compensation', 'Pay basis, band, rate, salary, and approval-oriented change requests.'],
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
