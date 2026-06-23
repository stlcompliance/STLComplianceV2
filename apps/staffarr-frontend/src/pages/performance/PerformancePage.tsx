export function PerformancePage() {
  return (
    <div className="space-y-6">
      <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6">
        <p className="text-xs font-semibold uppercase tracking-[0.24em] text-[var(--color-accent)]">StaffArr performance</p>
        <h1 className="mt-3 text-3xl font-semibold text-[var(--color-text-primary)]">Goals, cycles, feedback, and improvement plans</h1>
        <p className="mt-3 max-w-4xl text-sm text-[var(--color-text-secondary)]">
          Manage review cycles, goal tracking, competency assessments, feedback, and improvement plans in one place.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {[
          ['Review cycles', 'Annual, midyear, probation, project, and 1:1 review cycles.'],
          ['Goals', 'Progress, priorities, due dates, and result summaries.'],
          ['Competencies', 'Expected vs. current level and manager assessments.'],
          ['Improvement plans', 'PIP status, cadence, check-ins, and success criteria.'],
        ].map(([title, body]) => (
          <section key={title} className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
            <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">{title}</h2>
            <p className="mt-2 text-sm text-[var(--color-text-secondary)]">{body}</p>
          </section>
        ))}
      </div>

      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
        <h2 className="text-base font-semibold text-[var(--color-text-primary)]">Expected workflow</h2>
        <ol className="mt-3 space-y-2 text-sm text-[var(--color-text-secondary)]">
          <li className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3">Create a cycle and align it to the person and manager.</li>
          <li className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3">Track goals, competency assessments, and self-review or manager feedback.</li>
          <li className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3">Escalate to a PIP when required and keep the record effective-dated.</li>
        </ol>
      </section>
    </div>
  )
}
