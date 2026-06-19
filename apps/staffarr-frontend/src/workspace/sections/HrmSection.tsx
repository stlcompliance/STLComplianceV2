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
    title: 'Onboarding and offboarding',
    description: 'Task-driven worker activation, separation, and handoff orchestration.',
    route: '/employment-applications',
    label: 'Applications and journeys',
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
    title: 'Recruiting',
    description: 'Requisitions, candidates, interviews, offers, and the application-to-hire bridge. ',
    route: '/recruiting',
    label: 'Recruiting',
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
    'The new controlled fieldsets and document categories let us add future HR forms without fragmenting ownership.',
    'RecordArr remains the file store, TrainArr still owns training, Compliance Core still owns rule meaning, and LedgArr still owns payroll execution.',
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
      <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
        <p className="text-xs font-semibold uppercase tracking-[0.24em] text-sky-300">StaffArr HRM program</p>
        <h1 className="mt-3 text-3xl font-semibold text-slate-50">Full HRM/HCM operating layer</h1>
        <p className="mt-3 max-w-4xl text-sm text-slate-300">
          StaffArr now groups the employee lifecycle, personnel file, onboarding/offboarding, position control,
          time/leave, classification, casework, performance, benefits, compensation-adjacent records, recruiting,
          labor relations, injury, self-service, and analytics under one operational surface.
        </p>
        <div className="mt-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          {metrics.map((metric) => (
            <div key={metric.label} className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
              <p className="text-xs uppercase tracking-[0.24em] text-[var(--color-text-muted)]">{metric.label}</p>
              <p className="mt-2 text-2xl font-semibold text-slate-50">{metric.value}</p>
              <p className="mt-1 text-xs text-slate-400">{metric.note}</p>
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
              className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5 transition hover:border-slate-600"
            >
              <div className="flex items-start justify-between gap-3">
                <div className="flex items-center gap-3">
                  <div className="rounded-xl bg-sky-500/10 p-2 text-sky-300 ring-1 ring-sky-500/20">
                    <Icon className="h-5 w-5" />
                  </div>
                  <div>
                    <h2 className="text-base font-semibold text-slate-50">{card.title}</h2>
                    <p className="text-xs uppercase tracking-[0.22em] text-[var(--color-text-muted)]">{card.label}</p>
                  </div>
                </div>
                <span className="rounded-full border border-emerald-500/30 bg-emerald-500/10 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-[0.18em] text-emerald-300">
                  Active
                </span>
              </div>
              <p className="mt-3 text-sm text-slate-300">{card.description}</p>
              <Link
                to={card.route}
                className="mt-4 inline-flex rounded-lg bg-slate-800 px-3 py-2 text-sm font-medium text-slate-100 hover:bg-slate-700"
              >
                Open {card.label}
              </Link>
            </article>
          )
        })}
      </section>

      <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
        <h2 className="text-base font-semibold text-slate-50">Implementation notes</h2>
        <ul className="mt-3 space-y-2 text-sm text-slate-300">
          {implementationNotes.map((note) => (
            <li key={note} className="rounded-lg border border-slate-800/80 bg-slate-900/50 p-3">
              {note}
            </li>
          ))}
        </ul>
      </section>
    </div>
  )
}
