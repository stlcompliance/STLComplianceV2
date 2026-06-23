import { BriefcaseBusiness, CalendarClock, FileText, GraduationCap, ShieldAlert, Users } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

const moduleCards = [
  {
    title: 'Employment lifecycle',
    description: 'Hire, rehire, transfer, leave, return, suspension, termination, and rehire eligibility history.',
    route: '/people',
    label: 'People directory',
    icon: Users,
  },
  {
    title: 'Personnel file',
    description: 'Controlled document categories, access levels, retention, and restricted HR records.',
    route: '/people/details',
    label: 'Personnel documents',
    icon: FileText,
  },
  {
    title: 'Applicant intake',
    description: 'Build public application forms, review submissions, and bridge qualified applicants into hiring.',
    route: '/applications',
    label: 'Applications',
    icon: BriefcaseBusiness,
  },
  {
    title: 'Position control',
    description: 'Job architecture, headcount, reporting structure, and vacancy management.',
    route: '/organization-structure',
    label: 'Org structure',
    icon: Users,
  },
  {
    title: 'Time, leave, and attendance',
    description: 'Schedules, PTO, leave, approvals, attendance, and payroll readiness.',
    route: '/timekeeping',
    label: 'Timekeeping',
    icon: CalendarClock,
  },
  {
    title: 'Classification',
    description: 'Worker category, FLSA status, and effective-dated classification review.',
    route: '/people/details',
    label: 'Profile snapshot',
    icon: ShieldAlert,
  },
  {
    title: 'Casework and injury',
    description: 'HR complaints, investigations, grievances, restrictions, and injury tracking.',
    route: '/incidents',
    label: 'Incidents',
    icon: ShieldAlert,
  },
  {
    title: 'Hiring pipeline',
    description: 'Requisitions, candidates, interviews, offers, and the applicant-to-person bridge.',
    route: '/hiring',
    label: 'Hiring',
    icon: BriefcaseBusiness,
  },
  {
    title: 'Benefits and compensation',
    description: 'Eligibility, enrollment, dependents, beneficiaries, pay bands, and change requests.',
    route: '/benefits-compensation',
    label: 'Benefits',
    icon: GraduationCap,
  },
]

export function HrmSection({ state }: Props) {
  const implementationNotes = [
    'StaffArr now surfaces the HRM backbone in the same shell as the existing workforce, readiness, and timekeeping workflows.',
    'The new controlled fieldsets and document categories let us add future HR forms without fragmenting responsibilities.',
    'RecordArr remains the file store, training stays in the training workflow, rule meaning stays in Compliance Core, and payroll execution stays in LedgArr.',
  ]

  const metrics = [
    { label: 'People', value: state.people.length.toString(), note: 'Canonical workforce records' },
    { label: 'Org units', value: state.orgUnits.length.toString(), note: 'Sites, departments, teams, positions' },
    { label: 'Permission rules', value: (state.effectivePermissions?.permissions.length ?? 0).toString(), note: 'Selected person projection' },
    { label: 'Certifications', value: state.certificationDefinitions.length.toString(), note: 'TrainArr readiness signals' },
    { label: 'Open incidents', value: state.personIncidents.length.toString(), note: 'Personnel and safety cases' },
    { label: 'Documents', value: state.personDocuments.length.toString(), note: 'Current selected person records' },
  ]

  return (
    <div className="space-y-6">
      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 shadow-[var(--shadow-surface)]">
        <p className="text-xs font-semibold uppercase tracking-[0.24em] text-[var(--color-accent)]">StaffArr HRM program</p>
        <h1 className="mt-3 text-3xl font-semibold text-[var(--color-text-primary)]">Full HRM/HCM operating layer</h1>
        <p className="mt-3 max-w-4xl text-sm text-[var(--color-text-secondary)]">
          StaffArr now groups the employee lifecycle, personnel file, onboarding/offboarding, position control,
          time/leave, classification, casework, performance, benefits, compensation-adjacent records, hiring,
          labor relations, injury, self-service, and analytics under one operational surface.
        </p>
        <div className="mt-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          {metrics.map((metric) => (
            <div key={metric.label} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
              <p className="text-xs uppercase tracking-[0.24em] text-[var(--color-text-muted)]">{metric.label}</p>
              <p className="mt-2 text-2xl font-semibold text-[var(--color-text-primary)]">{metric.value}</p>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">{metric.note}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        {moduleCards.map((card) => {
          const Icon = card.icon
          return (
            <article
              key={card.title}
              className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 transition hover:border-[var(--color-border-strong)] hover:bg-[var(--color-bg-control-hover)]"
            >
              <div className="flex items-start justify-between gap-3">
                <div className="flex items-center gap-3">
                  <div className="rounded-xl bg-[var(--color-accent-soft)] p-2 text-[var(--color-accent)] ring-1 ring-[var(--color-accent-border)]">
                    <Icon className="h-5 w-5" />
                  </div>
                  <div>
                    <h2 className="text-base font-semibold text-[var(--color-text-primary)]">{card.title}</h2>
                    <p className="text-xs uppercase tracking-[0.22em] text-[var(--color-text-muted)]">{card.label}</p>
                  </div>
                </div>
                <span className="rounded-full border border-[var(--tone-success-border)] bg-[var(--tone-success-bg)] px-2.5 py-1 text-[11px] font-semibold uppercase tracking-[0.18em] text-[var(--tone-success-text)]">
                  Active
                </span>
              </div>
              <p className="mt-3 text-sm text-[var(--color-text-secondary)]">{card.description}</p>
              <Link
                to={card.route}
                className="mt-4 inline-flex rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
              >
                Open {card.label}
              </Link>
            </article>
          )
        })}
      </section>

      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 shadow-[var(--shadow-surface)]">
        <h2 className="text-base font-semibold text-[var(--color-text-primary)]">Implementation notes</h2>
        <ul className="mt-3 space-y-2 text-sm text-[var(--color-text-secondary)]">
          {implementationNotes.map((note) => (
            <li key={note} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3">
              {note}
            </li>
          ))}
        </ul>
      </section>
    </div>
  )
}
