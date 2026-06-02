import { getErrorMessage } from '@stl/shared-ui'
import {
  AlertTriangle,
  ArrowLeft,
  Award,
  BriefcaseBusiness,
  CalendarClock,
  CheckCircle2,
  ChevronDown,
  FileText,
  GraduationCap,
  IdCard,
  KeyRound,
  MoreHorizontal,
  Pencil,
  ShieldCheck,
  User,
  UserCheck,
  XCircle,
} from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { CreatePersonPanel } from '../../components/CreatePersonPanel'
import { PersonProfileEditorPanel } from '../../components/PersonProfileEditorPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }
type PeopleViewMode = 'drawer' | 'details' | 'create'
type DrawerColumnKey = 'name' | 'email' | 'jobTitle' | 'orgUnit' | 'status' | 'manager'
type Tone = 'good' | 'warn' | 'bad' | 'info' | 'neutral'

const PEOPLE_DRAWER_COLUMN_STORAGE_KEY = 'staffarr.people.drawer.columns.v1'

const ALL_DRAWER_COLUMNS: Array<{ key: DrawerColumnKey; label: string }> = [
  { key: 'name', label: 'Name' },
  { key: 'email', label: 'Email' },
  { key: 'jobTitle', label: 'Job title' },
  { key: 'orgUnit', label: 'Org unit' },
  { key: 'status', label: 'Status' },
  { key: 'manager', label: 'Manager' },
]

const DEFAULT_DRAWER_COLUMNS: DrawerColumnKey[] = ['name', 'email', 'jobTitle', 'orgUnit', 'status']

const detailTabs = ['Overview', 'Permissions', 'Certifications', 'Assignments', 'Training', 'Incidents', 'Documents', 'History']

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatDate(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleDateString(undefined, { month: 'short', day: '2-digit', year: 'numeric' })
}

function daysUntil(value: string | null | undefined): number | null {
  if (!value) return null
  const timestamp = Date.parse(value)
  if (!Number.isFinite(timestamp)) return null
  return Math.ceil((timestamp - Date.now()) / 86_400_000)
}

function badgeClass(tone: Tone): string {
  if (tone === 'good') return 'border-emerald-400/30 bg-emerald-500/15 text-emerald-200'
  if (tone === 'warn') return 'border-amber-400/30 bg-amber-500/15 text-amber-200'
  if (tone === 'bad') return 'border-red-400/30 bg-red-500/15 text-red-200'
  if (tone === 'info') return 'border-sky-400/30 bg-sky-500/15 text-sky-200'
  return 'border-slate-500/30 bg-slate-500/10 text-slate-300'
}

function Badge({ label, tone = 'neutral' }: { label: string; tone?: Tone }) {
  return (
    <span className={`inline-flex items-center rounded-full border px-3 py-1 text-xs font-semibold ${badgeClass(tone)}`}>
      {label}
    </span>
  )
}

function MetricCard({
  label,
  value,
  hint,
  icon,
  tone = 'neutral',
}: {
  label: string
  value: string | number
  hint: string
  icon: ReactNode
  tone?: Tone
}) {
  const iconClass = {
    good: 'bg-emerald-500/15 text-emerald-300',
    warn: 'bg-amber-500/15 text-amber-300',
    bad: 'bg-red-500/15 text-red-300',
    info: 'bg-sky-500/15 text-sky-300',
    neutral: 'bg-slate-700/60 text-slate-300',
  }[tone]

  return (
    <section className="min-h-32 rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-sm text-sky-200/80">{label}</p>
          <p className="mt-3 text-3xl font-bold tracking-normal text-white">{value}</p>
        </div>
        <div className={`rounded-xl p-3 ${iconClass}`}>{icon}</div>
      </div>
      <p className="mt-2 text-xs text-slate-400">{hint}</p>
    </section>
  )
}

function SnapshotField({
  label,
  value,
  source,
}: {
  label: string
  value: string
  source: string
}) {
  return (
    <div className="min-h-[4.5rem] rounded-xl border border-slate-800 bg-slate-950/60 p-3">
      <div className="flex items-start justify-between gap-2">
        <dt className="text-xs font-semibold uppercase tracking-normal text-sky-200/55">{label}</dt>
        <span className="shrink-0 text-[10px] text-slate-500">{source}</span>
      </div>
      <dd className="mt-2 break-words text-sm font-semibold text-white">{value}</dd>
    </div>
  )
}

function ProductPermissionCard({
  product,
  role,
  detail,
  allowed,
}: {
  product: string
  role: string
  detail: string
  allowed: boolean
}) {
  return (
    <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="font-bold text-white">{product}</p>
          <p className="mt-3 text-sm text-slate-100">{role}</p>
          <p className="mt-2 text-xs text-slate-400">{detail}</p>
        </div>
        <Badge label={allowed ? 'Allowed' : 'Blocked'} tone={allowed ? 'good' : 'bad'} />
      </div>
    </div>
  )
}

function certificationTone(expiresAt: string | null, status: string): Tone {
  if (status !== 'active') return 'bad'
  const remaining = daysUntil(expiresAt)
  if (remaining != null && remaining <= 60) return 'warn'
  return 'good'
}

function certificationLabel(expiresAt: string | null, status: string): string {
  if (status !== 'active') return humanize(status)
  const remaining = daysUntil(expiresAt)
  if (remaining != null && remaining <= 60) return 'Expiring soon'
  return 'Current'
}

export function PeopleSection({ state }: Props) {
  const s = state
  const location = useLocation()
  const [selectedColumns, setSelectedColumns] = useState<DrawerColumnKey[]>(DEFAULT_DRAWER_COLUMNS)
  const [showEditor, setShowEditor] = useState(false)
  const managerDisplayName = s.profile?.managerPersonId
    ? s.people.find((person) => person.personId === s.profile!.managerPersonId)?.displayName ?? 'Assigned'
    : 'None'
  const selectedPersonId = s.selectedPerson?.personId ?? null
  const activeFilteredPersonId = (() => {
    if (!s.peopleDirectoryQuery.trim() || s.filteredPeople.length === 0) {
      return null
    }
    if (s.activeDirectoryPersonId && s.filteredPeople.some((person) => person.personId === s.activeDirectoryPersonId)) {
      return s.activeDirectoryPersonId
    }
    if (selectedPersonId && s.filteredPeople.some((person) => person.personId === selectedPersonId)) {
      return selectedPersonId
    }
    return s.filteredPeople[0]!.personId
  })()
  const mode: PeopleViewMode = location.pathname.startsWith('/people/create')
    ? 'create'
    : location.pathname.startsWith('/people/details')
      ? 'details'
      : 'drawer'

  useEffect(() => {
    try {
      const raw = window.localStorage.getItem(PEOPLE_DRAWER_COLUMN_STORAGE_KEY)
      if (!raw) return
      const parsed = JSON.parse(raw) as DrawerColumnKey[]
      const valid = parsed.filter((column) => ALL_DRAWER_COLUMNS.some((candidate) => candidate.key === column))
      if (valid.length > 0) {
        setSelectedColumns(valid.slice(0, 5))
      }
    } catch {
      // Ignore malformed persisted state.
    }
  }, [])

  useEffect(() => {
    window.localStorage.setItem(PEOPLE_DRAWER_COLUMN_STORAGE_KEY, JSON.stringify(selectedColumns))
  }, [selectedColumns])

  useEffect(() => {
    setShowEditor(false)
  }, [s.effectivePersonId, mode])

  const visibleColumns = useMemo(() => {
    const picked = selectedColumns
      .filter((column) => ALL_DRAWER_COLUMNS.some((candidate) => candidate.key === column))
      .slice(0, 5)
    return picked.length > 0 ? picked : DEFAULT_DRAWER_COLUMNS
  }, [selectedColumns])

  const toggleColumn = (column: DrawerColumnKey) => {
    setSelectedColumns((previous) => {
      if (previous.includes(column)) {
        const next = previous.filter((item) => item !== column)
        return next.length > 0 ? next : previous
      }
      if (previous.length >= 5) {
        return previous
      }
      return [...previous, column]
    })
  }

  const managerNameByPersonId = useMemo(() => {
    return new Map(s.people.map((person) => [person.personId, person.displayName]))
  }, [s.people])

  const cellValue = (person: (typeof s.filteredPeople)[number], column: DrawerColumnKey): string => {
    switch (column) {
      case 'name':
        return person.displayName
      case 'email':
        return person.primaryEmail
      case 'jobTitle':
        return person.jobTitle ?? 'Unspecified'
      case 'orgUnit':
        return person.primaryOrgUnitName ?? 'Unassigned'
      case 'status':
        return person.employmentStatus
      case 'manager':
        return person.managerPersonId ? managerNameByPersonId.get(person.managerPersonId) ?? 'Assigned' : 'None'
      default:
        return ''
    }
  }

  const renderDirectorySection = () => (
    <section className={mode === 'details' ? '' : 'mt-8'}>
      <div
        className={[
          'border border-slate-700 bg-slate-900/60',
          mode === 'details' ? 'rounded-lg p-4' : 'rounded-xl p-6',
        ].join(' ')}
      >
        <h2 className="text-sm font-medium text-slate-300">People directory</h2>
        <div className="mt-3 space-y-2">
          <label className="block text-xs font-medium uppercase tracking-wide text-slate-400" htmlFor="workspace-directory-filter">
            Quick filter
          </label>
          <div className="flex items-center gap-2">
            <input
              id="workspace-directory-filter"
              type="search"
              aria-label="People quick filter"
              data-testid="workspace-people-directory-filter"
              value={s.peopleDirectoryQuery}
              onChange={(event) => s.setPeopleDirectoryQuery(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === 'Escape' && s.peopleDirectoryQuery) {
                  event.preventDefault()
                  s.setPeopleDirectoryQuery('')
                  return
                }
                if (
                  (event.key === 'ArrowDown' || event.key === 'ArrowUp') &&
                  s.peopleDirectoryQuery.trim() &&
                  s.filteredPeople.length > 0
                ) {
                  event.preventDefault()
                  const anchorId = activeFilteredPersonId ?? s.filteredPeople[0]!.personId
                  const currentIndex = s.filteredPeople.findIndex((person) => person.personId === anchorId)
                  const startIndex = currentIndex >= 0 ? currentIndex : 0
                  const nextIndex =
                    event.key === 'ArrowDown'
                      ? (startIndex + 1) % s.filteredPeople.length
                      : (startIndex - 1 + s.filteredPeople.length) % s.filteredPeople.length
                  s.setActiveDirectoryPersonId(s.filteredPeople[nextIndex]!.personId)
                  return
                }
                if (event.key === 'Enter' && s.peopleDirectoryQuery.trim() && s.filteredPeople.length > 0) {
                  event.preventDefault()
                  s.setSelectedPersonId(activeFilteredPersonId ?? s.filteredPeople[0]!.personId)
                }
              }}
              placeholder="Search by name, email, title, org unit, or status"
              className="w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white placeholder:text-slate-500 focus:border-sky-500 focus:outline-none"
            />
            {s.peopleDirectoryQuery ? (
              <button
                type="button"
                onClick={() => s.setPeopleDirectoryQuery('')}
                className="rounded-md border border-slate-700 px-3 py-2 text-xs text-slate-300 hover:border-slate-500 hover:text-white"
              >
                Clear
              </button>
            ) : null}
          </div>
          {!s.peopleQuery.isLoading && s.people.length > 0 ? (
            <p className="text-xs text-slate-500" aria-live="polite">
              Showing {s.filteredPeople.length} of {s.people.length} people
            </p>
          ) : null}
          {!s.peopleQuery.isLoading && s.peopleDirectoryQuery.trim() && s.filteredPeople.length > 0 ? (
            <p className="text-xs text-slate-500">Use ↑/↓ to move through results, then press Enter to select.</p>
          ) : null}
          {s.selectedPersonHiddenByFilter ? (
            <div className="rounded-md border border-amber-700/60 bg-amber-950/20 p-2 text-xs text-amber-200">
              The selected person is hidden by the current filter.
              <button
                type="button"
                onClick={() => s.setPeopleDirectoryQuery('')}
                className="ml-2 underline decoration-amber-400/70 underline-offset-2 hover:text-amber-100"
              >
                Clear filter to show selection
              </button>
            </div>
          ) : null}
        </div>
        {s.peopleQuery.isLoading ? (
          <p className="mt-4 text-sm text-slate-400">Loading people...</p>
        ) : s.people.length === 0 ? (
          <p className="mt-4 text-sm text-slate-400">No people have been added yet for this tenant.</p>
        ) : s.filteredPeople.length === 0 ? (
          <p className="mt-4 text-sm text-slate-400" aria-live="polite">
            No people match the current filter. Try a different name, email, or status.
          </p>
        ) : mode === 'drawer' ? (
          <div className="mt-4 space-y-3">
            <div className="rounded-md border border-slate-700 p-2">
              <p className="text-xs text-slate-400">Visible columns (max 5)</p>
              <div className="mt-2 flex flex-wrap gap-3">
                {ALL_DRAWER_COLUMNS.map((column) => (
                  <label key={column.key} className="inline-flex items-center gap-2 text-xs text-slate-300">
                    <input
                      type="checkbox"
                      checked={visibleColumns.includes(column.key)}
                      onChange={() => toggleColumn(column.key)}
                    />
                    {column.label}
                  </label>
                ))}
              </div>
            </div>
            <div className="overflow-x-auto rounded-md border border-slate-700">
              <table className="min-w-full text-left text-sm">
                <thead className="bg-slate-950/70">
                  <tr>
                    {visibleColumns.map((column) => (
                      <th key={column} className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-slate-400">
                        {ALL_DRAWER_COLUMNS.find((item) => item.key === column)?.label}
                      </th>
                    ))}
                    <th className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-slate-400">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {s.filteredPeople.map((person) => (
                    <tr key={person.personId} className="border-t border-slate-800">
                      {visibleColumns.map((column) => (
                        <td key={`${person.personId}-${column}`} className="px-3 py-2 text-slate-200">
                          {cellValue(person, column)}
                        </td>
                      ))}
                      <td className="px-3 py-2">
                        <div className="flex items-center gap-2 text-xs">
                          <Link
                            to="/people/details"
                            onClick={() => s.setSelectedPersonId(person.personId)}
                            className="text-sky-300 hover:text-sky-200 hover:underline"
                          >
                            View
                          </Link>
                          <Link
                            to="/people/create"
                            onClick={() => s.setSelectedPersonId(person.personId)}
                            className="text-emerald-300 hover:text-emerald-200 hover:underline"
                          >
                            Edit
                          </Link>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        ) : (
          <ul className="mt-4 divide-y divide-slate-700">
            {s.filteredPeople.map((person) => {
              const isSelected = s.effectivePersonId === person.personId
              const isActive =
                Boolean(s.peopleDirectoryQuery.trim()) && activeFilteredPersonId === person.personId
              const buttonClass = isSelected
                ? 'w-full rounded-md px-1 py-1 text-left text-sky-200'
                : isActive
                  ? 'w-full rounded-md px-1 py-1 text-left text-slate-100 ring-1 ring-sky-500/70'
                  : 'w-full rounded-md px-1 py-1 text-left'

              return (
                <li key={person.personId} className="flex items-center justify-between gap-4 py-3">
                  <button
                    type="button"
                    onMouseEnter={() => s.setActiveDirectoryPersonId(person.personId)}
                    onClick={() => {
                      s.setActiveDirectoryPersonId(person.personId)
                      s.setSelectedPersonId(person.personId)
                    }}
                    className={buttonClass}
                  >
                    <p className="text-sm text-white">{person.displayName}</p>
                    <p className="text-xs text-slate-400">
                      {person.jobTitle ?? 'No title'} - {person.primaryEmail}
                    </p>
                  </button>
                  <span className="text-xs uppercase tracking-wide text-slate-500">{person.employmentStatus}</span>
                </li>
              )
            })}
          </ul>
        )}
      </div>
    </section>
  )

  if (mode === 'details') {
    const profile = s.profile
    const selectedPerson = s.selectedPerson
    const lookup = s.personLookupQuery.data
    const certifications = s.personCertifications ?? []
    const incidents = s.personIncidents ?? []
    const roleAssignments = s.roleAssignments ?? []
    const documents = s.personDocuments ?? []
    const recentActivity = s.personTimelineEntries ?? []
    const permissions = s.effectivePermissions?.permissions ?? []
    const readiness = s.personReadinessQuery?.data
    const personId = profile?.personId ?? selectedPerson?.personId ?? s.effectivePersonId
    const displayName = profile?.displayName ?? selectedPerson?.displayName ?? 'No profile selected'
    const email = profile?.primaryEmail ?? selectedPerson?.primaryEmail ?? 'Not recorded'
    const employmentStatus = profile?.employmentStatus ?? selectedPerson?.employmentStatus ?? 'unknown'
    const jobTitle = profile?.jobTitle ?? selectedPerson?.jobTitle ?? 'Unassigned role'
    const primaryOrg = profile?.primaryOrgUnitName ?? selectedPerson?.primaryOrgUnitName ?? 'Unassigned'
    const activeAssignment = lookup?.placement.activeAssignments[0] ?? null
    const siteName = activeAssignment?.siteName ?? primaryOrg
    const departmentName = activeAssignment?.departmentName ?? 'Not assigned'
    const positionName = activeAssignment?.positionName ?? jobTitle
    const managerName = lookup?.placement.managerDisplayName ?? managerDisplayName
    const hasUserAccount = Boolean(profile?.externalUserId ?? selectedPerson?.externalUserId)
    const activeCertifications = certifications.filter((cert) => cert.effectiveStatus === 'active')
    const expiringCertifications = activeCertifications.filter((cert) => {
      const remaining = daysUntil(cert.expiresAt)
      return remaining != null && remaining <= 60
    })
    const openTrainingCount =
      s.workforceOnboardingJourneyQuery?.data?.steps?.filter((step) => step.status !== 'complete').length ??
      s.trainarrTrainingHistoryQuery?.data?.items?.filter((item) => item.eventKind !== 'completed').length ??
      0
    const readinessAllowed = readiness?.readinessStatus !== 'not_ready'
    const allowedChecks = readiness?.requirements.filter((requirement) => requirement.requirementStatus === 'satisfied').length
      ?? permissions.length
      ?? 0
    const blockedChecks = readiness?.blockers.length ?? 0
    const activeRoleAssignments = roleAssignments.filter((assignment) => assignment.status === 'active')
    const permissionKeys = permissions.map((permission) => permission.permissionKey.toLowerCase())
    const hasProductSignal = (product: string) =>
      permissionKeys.some((permission) => permission.includes(product.toLowerCase())) ||
      activeRoleAssignments.some((assignment) => assignment.roleKey.toLowerCase().includes(product.toLowerCase()))
    const certificationCards = certifications.slice(0, 3)
    const upcomingRequirements = [
      ...expiringCertifications.slice(0, 2).map((cert) => ({
        title: `${cert.certificationName} review`,
        detail: 'Certification expires soon',
        badge: cert.expiresAt ? `Due ${formatDate(cert.expiresAt)}` : 'Due soon',
        tone: 'warn' as Tone,
      })),
      ...(readiness?.blockers ?? []).slice(0, 2).map((blocker) => ({
        title: blocker.certificationName ?? blocker.qualificationName ?? humanize(blocker.blockerType),
        detail: blocker.message,
        badge: 'Required',
        tone: 'bad' as Tone,
      })),
    ].slice(0, 3)
    const documentCards = documents.slice(0, 4)
    const recentActivityCards = recentActivity.slice(0, 5)

    const editorPanel = profile ? (
      <PersonProfileEditorPanel
        profile={profile}
        orgUnits={s.orgUnits}
        peopleOptions={s.people.map((person) => ({
          personId: person.personId,
          displayName: person.displayName,
        }))}
        canManage={s.canManagePeopleProfiles}
        isSubmitting={s.updatePersonMutation.isPending || s.updateEmploymentStatusMutation.isPending}
        errorMessage={
          s.personProfileMutationError
            ? getErrorMessage(s.personProfileMutationError, 'Failed to update person profile.')
            : null
        }
        onUpdate={async (request) => {
          await s.updatePersonMutation.mutateAsync({
            personId: profile.personId,
            ...request,
          })
        }}
        onEmploymentStatusChange={async (request) => {
          await s.updateEmploymentStatusMutation.mutateAsync({
            personId: profile.personId,
            ...request,
          })
        }}
      />
    ) : null

    return (
      <div className="w-full max-w-[1500px] space-y-6 pb-10">
        <section className="rounded-[1.4rem] border border-slate-800 bg-slate-950/80 p-5 shadow-[0_24px_70px_rgba(2,6,23,0.32)]">
          <div className="flex flex-wrap items-start justify-between gap-5">
            <div className="min-w-0">
              <nav className="flex flex-wrap items-center gap-3 text-sm text-sky-200/80" aria-label="Person breadcrumb">
                <Link
                  to="/people/drawer"
                  className="inline-flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-950 px-3 py-2 text-slate-300 hover:text-white"
                >
                  <ArrowLeft className="h-4 w-4" />
                  People
                </Link>
                <span className="text-slate-500">/</span>
                <span>{siteName}</span>
                <span className="text-slate-500">/</span>
                <span className="font-semibold text-white">{displayName}</span>
              </nav>

              <div className="mt-7 flex items-center gap-4">
                <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-2xl border border-sky-400/20 bg-sky-500/15 text-sky-300">
                  <User className="h-9 w-9" />
                </div>
                <div className="min-w-0">
                  <div className="mb-3 flex flex-wrap items-center gap-2">
                    <Badge label={profile?.externalUserId ?? 'StaffArr profile'} tone="info" />
                    <Badge label={humanize(employmentStatus)} tone={employmentStatus === 'active' ? 'good' : 'warn'} />
                    <Badge label={hasUserAccount ? 'Has user account' : 'No user account'} tone={hasUserAccount ? 'good' : 'neutral'} />
                  </div>
                  <h1 className="truncate text-3xl font-bold tracking-normal text-white md:text-4xl">{displayName}</h1>
                  <p className="mt-2 flex flex-wrap items-center gap-2 text-sm text-sky-100/75">
                    <BriefcaseBusiness className="h-4 w-4 text-slate-400" />
                    <span>{jobTitle}</span>
                    <span className="text-slate-600">-</span>
                    <span>{siteName}</span>
                  </p>
                </div>
              </div>
            </div>

            <div className="flex flex-wrap items-center justify-end gap-2">
              <button
                type="button"
                onClick={() => setShowEditor((current) => !current)}
                className="inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-3 text-sm font-bold text-slate-950 hover:bg-sky-400"
              >
                <Pencil className="h-4 w-4" />
                Edit person
              </button>
              <button
                type="button"
                className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm font-bold text-white hover:bg-slate-800"
              >
                <GraduationCap className="h-4 w-4" />
                Assign training
              </button>
              <button
                type="button"
                className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm font-bold text-white hover:bg-slate-800"
              >
                <ShieldCheck className="h-4 w-4" />
                Manage access
              </button>
              <button
                type="button"
                aria-label="More person actions"
                className="inline-flex h-12 w-12 items-center justify-center rounded-xl border border-slate-700 bg-slate-900 text-slate-300 hover:bg-slate-800 hover:text-white"
              >
                <MoreHorizontal className="h-5 w-5" />
              </button>
            </div>
          </div>
        </section>

        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          <MetricCard
            label="Account state"
            value={hasUserAccount ? 'Enabled' : 'Not linked'}
            hint={hasUserAccount ? 'NexArr login allowed' : 'No NexArr login account'}
            icon={<KeyRound className="h-5 w-5" />}
            tone={hasUserAccount ? 'good' : 'neutral'}
          />
          <MetricCard
            label="Open training"
            value={openTrainingCount}
            hint={openTrainingCount === 1 ? '1 item needs action' : `${openTrainingCount} items need action`}
            icon={<GraduationCap className="h-5 w-5" />}
            tone={openTrainingCount > 0 ? 'warn' : 'good'}
          />
          <MetricCard
            label="Active certifications"
            value={activeCertifications.length}
            hint={`${expiringCertifications.length} expire in 60 days`}
            icon={<Award className="h-5 w-5" />}
            tone={expiringCertifications.length > 0 ? 'warn' : 'good'}
          />
          <MetricCard
            label="Incidents"
            value={incidents.length}
            hint={incidents.length > 0 ? 'Review personnel history' : 'No active restriction'}
            icon={<AlertTriangle className="h-5 w-5" />}
            tone={incidents.length > 0 ? 'warn' : 'good'}
          />
        </div>

        {showEditor ? <div>{editorPanel}</div> : null}

        <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_397px]">
          <main className="min-w-0 rounded-2xl border border-slate-800 bg-slate-950/70">
            <div className="flex gap-2 overflow-x-auto border-b border-slate-800 p-3" role="tablist" aria-label="Person detail sections">
              {detailTabs.map((tab) => (
                <button
                  key={tab}
                  type="button"
                  role="tab"
                  aria-selected={tab === 'Overview'}
                  className={`shrink-0 rounded-xl px-4 py-3 text-sm font-semibold ${tab === 'Overview' ? 'bg-slate-900 text-sky-300' : 'text-sky-200/70 hover:bg-slate-900/70 hover:text-white'}`}
                >
                  {tab}
                </button>
              ))}
            </div>

            <section className="p-5">
              <div className="mb-5 flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h2 className="text-xl font-bold text-white">Person snapshot</h2>
                  <p className="mt-1 text-sm text-sky-100/75">
                    Identity, employment placement, login capability, assignments, and source-of-truth references.
                  </p>
                </div>
                <button
                  type="button"
                  className="inline-flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-900 px-4 py-3 text-sm font-semibold text-slate-200"
                >
                  Field sources
                  <ChevronDown className="h-4 w-4" />
                </button>
              </div>

              <dl className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                <SnapshotField label="Person ID" value={personId ?? 'Not selected'} source="NexArr source of truth" />
                <SnapshotField label="Employee number" value={profile?.externalUserId ?? 'Not linked'} source="StaffArr" />
                <SnapshotField label="Display name" value={displayName} source="NexArr person record" />
                <SnapshotField label="Preferred name" value={profile?.givenName ?? lookup?.givenName ?? 'Not recorded'} source="StaffArr profile" />
                <SnapshotField label="Email" value={email} source="Login/contact" />
                <SnapshotField label="Phone" value={lookup?.workPhone ?? 'Not recorded'} source="Contact profile" />
                <SnapshotField label="Site" value={siteName} source="StaffArr org structure" />
                <SnapshotField label="Department" value={departmentName} source="StaffArr org structure" />
                <SnapshotField label="Position" value={positionName} source="StaffArr position catalog" />
                <SnapshotField label="Manager" value={managerName} source="Reporting line" />
                <SnapshotField label="Hire date" value={formatDate(profile?.createdAt)} source="Personnel record" />
                <SnapshotField label="Shift" value="Not assigned" source="Schedule profile" />
              </dl>

              <div className="mt-5 grid gap-5 lg:grid-cols-2">
                <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
                  <div className="mb-4 flex items-center justify-between gap-3">
                    <h3 className="text-lg font-bold text-white">Product permissions</h3>
                    <Badge label="NexArr entitlement checked" tone="info" />
                  </div>
                  <div className="space-y-3">
                    <ProductPermissionCard
                      product="MaintainArr"
                      role={hasProductSignal('maintainarr') ? 'Fleet Technician' : 'No product entitlement'}
                      detail={hasProductSignal('maintainarr') ? 'Work orders, inspections, defects, asset notes' : 'No direct access'}
                      allowed={hasProductSignal('maintainarr')}
                    />
                    <ProductPermissionCard
                      product="TrainArr"
                      role={hasProductSignal('trainarr') ? 'Trainee / Evaluator' : 'No product entitlement'}
                      detail={hasProductSignal('trainarr') ? 'Assigned training, evaluator signoffs where qualified' : 'No direct access'}
                      allowed={hasProductSignal('trainarr')}
                    />
                    <ProductPermissionCard
                      product="StaffArr"
                      role={hasProductSignal('staffarr') || activeRoleAssignments.length > 0 ? 'Self Service' : 'No product entitlement'}
                      detail={hasProductSignal('staffarr') || activeRoleAssignments.length > 0 ? 'Own profile, own documents, own training history' : 'No direct access'}
                      allowed={hasProductSignal('staffarr') || activeRoleAssignments.length > 0}
                    />
                    <ProductPermissionCard
                      product="SupplyArr"
                      role={hasProductSignal('supplyarr') ? 'Supply workspace user' : 'No product entitlement'}
                      detail={hasProductSignal('supplyarr') ? 'Parts and procurement access' : 'No direct access'}
                      allowed={hasProductSignal('supplyarr')}
                    />
                  </div>
                </section>

                <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
                  <div className="mb-4 flex items-center justify-between gap-3">
                    <h3 className="text-lg font-bold text-white">Certification status</h3>
                    <Link to="/certifications" className="text-sm font-semibold text-sky-300 hover:text-sky-200">
                      View all
                    </Link>
                  </div>
                  <div className="space-y-3">
                    {certificationCards.length > 0 ? certificationCards.map((cert) => (
                      <div key={cert.personCertificationId} className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
                        <div className="flex items-start justify-between gap-3">
                          <div>
                            <p className="font-bold text-white">{cert.certificationName}</p>
                            <p className="mt-3 text-sm text-slate-100">Expires: {formatDate(cert.expiresAt)}</p>
                            <p className="mt-2 text-xs text-slate-400">{humanize(cert.sourceType)} - Training completion</p>
                          </div>
                          <Badge label={certificationLabel(cert.expiresAt, cert.effectiveStatus)} tone={certificationTone(cert.expiresAt, cert.effectiveStatus)} />
                        </div>
                      </div>
                    )) : (
                      <p className="rounded-xl border border-slate-800 bg-slate-950/70 p-4 text-sm text-slate-400">
                        No certifications are recorded for this person.
                      </p>
                    )}
                  </div>
                </section>
              </div>
            </section>
          </main>

          <aside className="space-y-5">
            <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
              <div className="flex items-start justify-between gap-3">
                <h2 className="text-lg font-bold text-white">Authorization decision</h2>
                <Badge label={readinessAllowed ? 'Allowed' : 'Blocked'} tone={readinessAllowed ? 'good' : 'bad'} />
              </div>
              <div className={`mt-4 rounded-2xl border p-4 ${readinessAllowed ? 'border-emerald-500/30 bg-emerald-500/10' : 'border-red-500/30 bg-red-500/10'}`}>
                <div className="flex gap-3">
                  {readinessAllowed ? (
                    <UserCheck className="mt-0.5 h-5 w-5 shrink-0 text-emerald-300" />
                  ) : (
                    <XCircle className="mt-0.5 h-5 w-5 shrink-0 text-red-300" />
                  )}
                  <div>
                    <p className="font-semibold text-white">
                      {readinessAllowed ? 'Can perform assigned work' : 'Restrictions require review'}
                    </p>
                    <p className="mt-2 text-sm leading-6 text-slate-200">
                      {readinessAllowed
                        ? 'No active restrictions. Training and role checks allow normal StaffArr work.'
                        : readiness?.blockers[0]?.message ?? 'One or more authorization checks are blocked.'}
                    </p>
                  </div>
                </div>
              </div>
              <div className="mt-4 grid grid-cols-2 gap-3">
                <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                  <CheckCircle2 className="h-5 w-5 text-emerald-300" />
                  <p className="mt-4 text-xs text-slate-400">Allowed checks</p>
                  <p className="text-xl font-bold text-white">{allowedChecks}</p>
                </div>
                <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                  <XCircle className="h-5 w-5 text-red-300" />
                  <p className="mt-4 text-xs text-slate-400">Blocked checks</p>
                  <p className="text-xl font-bold text-white">{blockedChecks}</p>
                </div>
              </div>
            </section>

            <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-lg font-bold text-white">Account and identity</h2>
                <IdCard className="h-5 w-5 text-sky-300" />
              </div>
              <div className="space-y-3">
                <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                  <p className="text-sm font-bold text-white">Person source</p>
                  <p className="mt-1 text-sm text-sky-100/75">NexArr personId is the human identity source of truth.</p>
                </div>
                <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                  <p className="text-sm font-bold text-white">Login capability</p>
                  <p className="mt-1 text-sm text-sky-100/75">
                    hasUserAccount = {hasUserAccount ? 'true' : 'false'}; credentials managed by NexArr.
                  </p>
                </div>
                <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                  <p className="text-sm font-bold text-white">Product access</p>
                  <p className="mt-1 text-sm leading-6 text-sky-100/75">
                    {[
                      hasProductSignal('maintainarr') ? 'MaintainArr' : null,
                      hasProductSignal('staffarr') || activeRoleAssignments.length > 0 ? 'StaffArr' : null,
                      hasProductSignal('trainarr') ? 'TrainArr' : null,
                    ].filter(Boolean).join(' - ') || 'No direct product access'}
                  </p>
                </div>
              </div>
            </section>

            <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-lg font-bold text-white">Upcoming requirements</h2>
                <CalendarClock className="h-5 w-5 text-sky-300" />
              </div>
              <div className="space-y-3">
                {upcomingRequirements.length > 0 ? upcomingRequirements.map((item) => (
                  <div key={`${item.title}-${item.badge}`} className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="font-bold text-white">{item.title}</p>
                        <p className="mt-2 text-xs text-slate-400">{item.detail}</p>
                      </div>
                      <Badge label={item.badge} tone={item.tone} />
                    </div>
                  </div>
                )) : (
                  <p className="rounded-xl border border-slate-800 bg-slate-900/70 p-4 text-sm text-slate-400">
                    No upcoming requirements are currently flagged.
                  </p>
                )}
              </div>
            </section>

            <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-lg font-bold text-white">Documents</h2>
                <FileText className="h-5 w-5 text-sky-300" />
              </div>
              <div className="overflow-hidden rounded-xl border border-slate-800">
                {documentCards.length > 0 ? documentCards.map((document) => (
                  <div key={document.documentId} className="flex items-start justify-between gap-3 border-b border-slate-800 bg-slate-950/70 p-4 last:border-b-0">
                    <div>
                      <p className="font-semibold text-white">{document.title}</p>
                      <p className="mt-1 text-xs text-slate-400">
                        {document.expiresAt ? `Expires ${formatDate(document.expiresAt)}` : `Uploaded ${formatDate(document.createdAt)}`}
                      </p>
                    </div>
                    <Badge label={document.status === 'active' ? 'Current' : humanize(document.status)} tone={document.status === 'active' ? 'good' : 'warn'} />
                  </div>
                )) : (
                  <p className="bg-slate-950/70 p-4 text-sm text-slate-400">No documents uploaded yet.</p>
                )}
              </div>
            </section>

            <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-lg font-bold text-white">Recent activity</h2>
                <CalendarClock className="h-5 w-5 text-sky-300" />
              </div>
              <ol className="space-y-4">
                {recentActivityCards.length > 0 ? recentActivityCards.map((entry) => (
                  <li key={entry.entryId} className="relative pl-7">
                    <span className="absolute left-0 top-1.5 h-3 w-3 rounded-full bg-sky-400 shadow-[0_0_0_4px_rgba(14,165,233,0.18)]" />
                    <p className="text-xs font-bold uppercase tracking-normal text-sky-300">{humanize(entry.category)}</p>
                    <p className="mt-2 text-sm font-semibold text-white">{entry.title}</p>
                    <p className="mt-2 text-xs text-slate-400">
                      {formatDate(entry.occurredAt)} - {humanize(entry.sourceEntityType)}
                    </p>
                  </li>
                )) : (
                  <li className="rounded-xl border border-slate-800 bg-slate-900/70 p-4 text-sm text-slate-400">
                    No recent activity recorded.
                  </li>
                )}
              </ol>
            </section>
          </aside>
        </div>
      </div>
    )
  }

  return (
    <>
      {mode === 'create' ? (
        <div className="rounded-xl border border-teal-700/50 bg-teal-950/20 p-4 text-sm text-teal-100">
          <p>Create people in a guided flow using friendly business fields only.</p>
          <ol className="mt-2 list-decimal space-y-1 pl-5">
            <li>Step 1: Add identity fields so this person can be recognized in staffing workflows.</li>
            <li>Step 2: Set organization placement to route assignments and approvals correctly.</li>
            <li>Step 3: Confirm role and status so readiness and training logic stays accurate.</li>
          </ol>
        </div>
      ) : null}

      {renderDirectorySection()}

      {mode === 'create' ? (
        <CreatePersonPanel
          orgUnits={s.orgUnits}
          peopleOptions={s.people.map((person) => ({
            personId: person.personId,
            displayName: person.displayName,
          }))}
          canManage={s.canManagePeopleProfiles}
          isSubmitting={s.createPersonMutation.isPending}
          errorMessage={
            s.createPersonMutation.error
              ? getErrorMessage(s.createPersonMutation.error, 'Failed to create person profile.')
              : null
          }
          onCreate={async (request) => {
            await s.createPersonMutation.mutateAsync(request)
          }}
        />
      ) : null}
    </>
  )
}
