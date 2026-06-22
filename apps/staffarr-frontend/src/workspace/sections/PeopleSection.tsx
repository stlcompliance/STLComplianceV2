import {
  DetailBadge as Badge,
  ApiErrorCallout,
  getErrorMessage,
  type DetailTabConfig,
  type DetailTone,
} from '@stl/shared-ui'
import {
  AlertTriangle,
  ClipboardCheck,
  ExternalLink,
  FileText,
  GraduationCap,
  MessageSquarePlus,
  Pencil,
  ShieldCheck,
  Upload,
} from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { type ReactNode, useEffect, useMemo, useState } from 'react'
import { createLaunchHandoff } from '../../api/client'
import { CreatePersonPanel } from '../../components/CreatePersonPanel'
import { PersonProfileEditorPanel } from '../../components/PersonProfileEditorPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }
type PeopleViewMode = 'drawer' | 'details' | 'create'
type DrawerColumnKey = 'name' | 'email' | 'jobTitle' | 'orgUnit' | 'status' | 'manager'

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

const DETAIL_TABS: DetailTabConfig[] = [
  { key: 'overview', label: 'Overview' },
  { key: 'permissions', label: 'Permissions' },
  { key: 'certifications', label: 'Certifications' },
  { key: 'assignments', label: 'Assignments' },
  { key: 'training', label: 'Training' },
  { key: 'incidents', label: 'Incidents' },
  { key: 'documents', label: 'Documents' },
  { key: 'history', label: 'History' },
]

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

function certificationTone(expiresAt: string | null, status: string): DetailTone {
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

type PersonDetailTone = 'good' | 'warn' | 'bad' | 'info' | 'purple' | 'neutral'

function toneDotClass(tone: PersonDetailTone): string {
  if (tone === 'good') return 'bg-emerald-400'
  if (tone === 'warn') return 'bg-amber-400'
  if (tone === 'bad') return 'bg-red-400'
  if (tone === 'purple') return 'bg-violet-400'
  if (tone === 'info') return 'bg-sky-400'
  return 'bg-slate-400'
}

function tonePanelClass(tone: PersonDetailTone): string {
  if (tone === 'good') return 'from-emerald-500/10'
  if (tone === 'warn') return 'from-amber-500/10'
  if (tone === 'bad') return 'from-red-500/10'
  if (tone === 'purple') return 'from-violet-500/10'
  if (tone === 'info') return 'from-sky-500/10'
  return 'from-slate-500/10'
}

function badgeToneFromPersonTone(tone: PersonDetailTone): DetailTone {
  if (tone === 'purple') return 'info'
  return tone
}

function initialsForName(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) return 'NA'
  return parts.slice(0, 2).map((part) => part[0]?.toUpperCase() ?? '').join('')
}

function countBy<T>(items: T[], predicate: (item: T) => boolean): number {
  return items.reduce((total, item) => total + (predicate(item) ? 1 : 0), 0)
}

function zeroWarnTone(count: number, nonZeroTone: PersonDetailTone): PersonDetailTone {
  return count === 0 ? 'warn' : nonZeroTone
}

function DetailCommandButton({
  children,
  icon,
  onClick,
  variant = 'secondary',
  disabled = false,
}: {
  children: ReactNode
  icon: ReactNode
  onClick?: () => void
  variant?: 'primary' | 'secondary'
  disabled?: boolean
}) {
  const className = variant === 'primary'
    ? 'border-[var(--color-accent-border)] bg-[var(--color-accent)] text-white hover:bg-[var(--color-accent-hover)]'
    : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] text-[var(--color-text-primary)] hover:border-[var(--color-border-strong)] hover:bg-[var(--color-bg-control-hover)]'

  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      className={`inline-flex min-h-10 items-center justify-center gap-2 rounded-lg border px-4 text-sm font-bold transition disabled:cursor-not-allowed disabled:opacity-60 ${className}`}
    >
      {icon}
      {children}
    </button>
  )
}

function SectionPanel({
  title,
  subtitle,
  actions,
  children,
  className = '',
}: {
  title: string
  subtitle?: string
  actions?: ReactNode
  children: ReactNode
  className?: string
}) {
  return (
    <section className={`rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 ${className}`}>
      <div className="mb-4 flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="text-lg font-bold text-[var(--color-text-primary)]">{title}</h3>
          {subtitle ? <p className="mt-1 text-sm leading-5 text-[var(--color-text-muted)]">{subtitle}</p> : null}
        </div>
        {actions ? <div className="flex shrink-0 flex-wrap gap-2">{actions}</div> : null}
      </div>
      {children}
    </section>
  )
}

function MetricCard({
  label,
  value,
  hint,
  tone,
}: {
  label: string
  value: ReactNode
  hint: string
  tone: PersonDetailTone
}) {
  return (
    <div className={`relative overflow-hidden rounded-2xl border border-[var(--color-border-subtle)] bg-gradient-to-br ${tonePanelClass(tone)} to-[var(--color-bg-surface-elevated)] p-5`}>
      <div className="absolute right-0 top-0 h-20 w-20 rounded-bl-full bg-[var(--color-accent-soft)]" />
      <span className={`absolute right-5 top-6 h-3 w-3 rounded-full ${toneDotClass(tone)}`} />
      <p className="text-sm text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-3 text-4xl font-black leading-none text-[var(--color-text-primary)]">{value}</p>
      <p className="mt-2 text-sm text-[var(--color-text-muted)]">{hint}</p>
    </div>
  )
}

function FieldTile({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="flex min-h-11 items-center justify-between gap-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-4 py-2">
      <span className="text-sm text-[var(--color-text-muted)]">{label}</span>
      <span className="text-right text-sm font-bold text-[var(--color-text-primary)]">{value}</span>
    </div>
  )
}

function DotItem({
  title,
  detail,
  tone,
  badge,
}: {
  title: string
  detail: string
  tone: PersonDetailTone
  badge?: string
}) {
  return (
    <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
      <div className="flex items-start justify-between gap-3">
        <div className="flex min-w-0 items-start gap-3">
          <span className={`mt-1.5 h-3 w-3 shrink-0 rounded-full ${toneDotClass(tone)}`} />
          <div className="min-w-0">
            <p className="font-bold text-[var(--color-text-primary)]">{title}</p>
            <p className="mt-1 text-sm leading-5 text-[var(--color-text-muted)]">{detail}</p>
          </div>
        </div>
        {badge ? <Badge label={badge} tone={badgeToneFromPersonTone(tone)} /> : null}
      </div>
    </div>
  )
}

function EmptyDetailState({ text }: { text: string }) {
  return (
    <div className="rounded-xl border border-dashed border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4 text-sm text-[var(--color-text-muted)]">
      {text}
    </div>
  )
}

export function PeopleSection({ state }: Props) {
  const s = state
  const complianceCoreApiBase = import.meta.env.VITE_COMPLIANCECORE_API_BASE ?? ''
  const location = useLocation()
  const [selectedColumns, setSelectedColumns] = useState<DrawerColumnKey[]>(DEFAULT_DRAWER_COLUMNS)
  const [showEditor, setShowEditor] = useState(false)
  const [isLaunchingTraining, setIsLaunchingTraining] = useState(false)
  const [trainingLaunchError, setTrainingLaunchError] = useState<string | null>(null)
  const noManagerLabel = 'No one'
  const managerDisplayName = s.profile?.managerPersonId
    ? s.people.find((person) => person.personId === s.profile!.managerPersonId)?.displayName ?? 'Assigned'
    : noManagerLabel
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
    setTrainingLaunchError(null)
  }, [s.effectivePersonId, mode])

  useEffect(() => {
    if (mode === 'details' && (location.state as { openEditor?: boolean } | null)?.openEditor) {
      setShowEditor(true)
      s.setPeopleDetailTab('overview')
    }
  }, [location.state, mode, s])

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
        return person.managerPersonId ? managerNameByPersonId.get(person.managerPersonId) ?? 'Assigned' : noManagerLabel
      default:
        return ''
    }
  }

  const renderDirectorySection = () => (
    <section className={mode === 'details' ? '' : 'mt-8'}>
      <div
        className={[
          'border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]',
          mode === 'details' ? 'rounded-lg p-4' : 'rounded-xl p-6',
        ].join(' ')}
      >
        <h2 className="text-sm font-medium text-[var(--color-text-secondary)]">People directory</h2>
        <div className="mt-3 space-y-2">
          <label className="block text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]" htmlFor="workspace-directory-filter">
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
              className="w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)] placeholder:text-[var(--color-text-muted)] focus:border-[var(--color-accent-border)] focus:outline-none"
            />
            {s.peopleDirectoryQuery ? (
              <button
                type="button"
                onClick={() => s.setPeopleDirectoryQuery('')}
                className="rounded-md border border-[var(--color-border-subtle)] px-3 py-2 text-xs text-[var(--color-text-secondary)] hover:border-[var(--color-border-strong)] hover:text-[var(--color-text-primary)]"
              >
                Clear
              </button>
            ) : null}
          </div>
          {!s.peopleQuery.isLoading && s.people.length > 0 ? (
            <p className="text-xs text-[var(--color-text-muted)]" aria-live="polite">
              Showing {s.filteredPeople.length} of {s.people.length} people
            </p>
          ) : null}
          {!s.peopleQuery.isLoading && s.peopleDirectoryQuery.trim() && s.filteredPeople.length > 0 ? (
            <p className="text-xs text-[var(--color-text-muted)]">Use ↑/↓ to move through results, then press Enter to select.</p>
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
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">Loading people...</p>
        ) : s.people.length === 0 ? (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">No people have been added yet for this tenant.</p>
        ) : s.filteredPeople.length === 0 ? (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]" aria-live="polite">
            No people match the current filter. Try a different name, email, or status.
          </p>
        ) : mode === 'drawer' ? (
          <div className="mt-4 space-y-3">
            <div className="rounded-md border border-[var(--color-border-subtle)] p-2">
              <p className="text-xs text-[var(--color-text-muted)]">Visible columns (max 5)</p>
              <div className="mt-2 flex flex-wrap gap-3">
                {ALL_DRAWER_COLUMNS.map((column) => (
                  <label key={column.key} className="inline-flex items-center gap-2 text-xs text-[var(--color-text-secondary)]">
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
            <div className="overflow-x-auto rounded-md border border-[var(--color-border-subtle)]">
              <table className="min-w-full text-left text-sm">
                <thead className="bg-[var(--color-bg-surface-elevated)]">
                  <tr>
                    {visibleColumns.map((column) => (
                      <th key={column} className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                        {ALL_DRAWER_COLUMNS.find((item) => item.key === column)?.label}
                      </th>
                    ))}
                    <th className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {s.filteredPeople.map((person) => (
                    <tr key={person.personId} className="border-t border-[var(--color-border-subtle)]">
                      {visibleColumns.map((column) => (
                        <td key={`${person.personId}-${column}`} className="px-3 py-2 text-[var(--color-text-secondary)]">
                          {cellValue(person, column)}
                        </td>
                      ))}
                      <td className="px-3 py-2">
                        <div className="flex items-center gap-2 text-xs">
                          <Link
                            to={`/people/details?person=${encodeURIComponent(person.personId)}&tab=overview`}
                            onClick={() => s.setSelectedPersonId(person.personId, { syncDetailQuery: true, tab: 'overview' })}
                            className="text-[var(--color-link-text)] hover:underline"
                          >
                            View
                          </Link>
                          <Link
                            to={`/people/details?person=${encodeURIComponent(person.personId)}&tab=overview`}
                            state={{ openEditor: true }}
                            onClick={() => s.setSelectedPersonId(person.personId, { syncDetailQuery: true, tab: 'overview' })}
                            className="text-[var(--color-success)] hover:underline"
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
          <ul className="mt-4 divide-y divide-[var(--color-border-subtle)]">
            {s.filteredPeople.map((person) => {
              const isSelected = s.effectivePersonId === person.personId
              const isActive =
                Boolean(s.peopleDirectoryQuery.trim()) && activeFilteredPersonId === person.personId
              const buttonClass = isSelected
                ? 'w-full rounded-md px-1 py-1 text-left text-[var(--color-text-primary)]'
                : isActive
                  ? 'w-full rounded-md px-1 py-1 text-left text-[var(--color-text-primary)] ring-1 ring-[var(--color-accent-border)]'
                  : 'w-full rounded-md px-1 py-1 text-left'

              return (
                <li key={person.personId} className="flex items-center justify-between gap-4 py-3">
                  <button
                    type="button"
                    onMouseEnter={() => s.setActiveDirectoryPersonId(person.personId)}
                    onClick={() => {
                      s.setActiveDirectoryPersonId(person.personId)
                      s.setSelectedPersonId(person.personId, { syncDetailQuery: true })
                    }}
                    className={buttonClass}
                  >
                    <p className="text-sm text-[var(--color-text-primary)]">{person.displayName}</p>
                    <p className="text-xs text-[var(--color-text-muted)]">
                      {person.jobTitle ?? 'No title'} - {person.primaryEmail}
                    </p>
                  </button>
                  <span className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{person.employmentStatus}</span>
                </li>
              )
            })}
          </ul>
        )}
      </div>
    </section>
  )

  const handleAssignTraining = async () => {
    if (!s.selectedPerson || isLaunchingTraining) {
      return
    }

    setTrainingLaunchError(null)
    setIsLaunchingTraining(true)
    try {
      const callbackUrl = `${window.location.origin}/people/details?person=${encodeURIComponent(s.selectedPerson.personId)}&tab=training`
      const handoff = await createLaunchHandoff(s.accessToken, 'trainarr', callbackUrl)
      window.location.assign(handoff.launchUrl)
    } catch (error) {
      console.error('TrainArr training handoff failed', error)
      setTrainingLaunchError('TrainArr is temporarily unavailable. Please try again.')
      setIsLaunchingTraining(false)
    }
  }

  if (mode === 'details') {
    const profile = s.profile
    const selectedPerson = s.selectedPerson
    const lookup = s.personLookupQuery.data
    const certifications = s.personCertifications ?? []
    const incidents = s.personIncidents ?? []
    const documents = s.personDocuments ?? []
    const recentActivity = s.personTimelineEntries ?? []
    const permissions = s.effectivePermissions?.permissions ?? []
    const readiness = s.personReadinessQuery?.data
    const onboardingJourney = s.workforceOnboardingJourneyQuery.data ?? null
    const trainingHistory = s.trainarrTrainingHistoryQuery.data?.items ?? []
    const historySummary = s.personHistorySummaryQuery.data ?? null
    const activeTab = DETAIL_TABS.find((tab) => tab.key === s.peopleDetailTab) ?? DETAIL_TABS[0]!
    const orgUnitById = new Map(s.orgUnits.map((unit) => [unit.orgUnitId, unit.name]))
    const personId = profile?.personId ?? selectedPerson?.personId ?? s.effectivePersonId
    const displayName = profile?.displayName ?? selectedPerson?.displayName ?? 'No profile selected'
    const email = profile?.primaryEmail ?? selectedPerson?.primaryEmail ?? 'Not recorded'
    const phone = profile?.primaryPhone ?? profile?.workPhone ?? lookup?.workPhone ?? 'Not recorded'
    const employmentStatus = profile?.employmentStatus ?? selectedPerson?.employmentStatus ?? 'unknown'
    const jobTitle = profile?.jobTitle ?? selectedPerson?.jobTitle ?? 'Unassigned role'
    const primaryOrg = profile?.primaryOrgUnitName ?? selectedPerson?.primaryOrgUnitName ?? 'Unassigned'
    const activeAssignment = lookup?.placement.activeAssignments[0] ?? null
    const siteName = activeAssignment?.siteName ?? primaryOrg
    const departmentName = activeAssignment?.departmentName ?? 'Not assigned'
    const teamName = activeAssignment?.teamName ?? 'Not assigned'
    const positionName = activeAssignment?.positionName ?? jobTitle
    const managerName = lookup?.placement.managerDisplayName ?? managerDisplayName
    const hasUserAccount = profile?.hasUserAccountSnapshot ?? Boolean(profile?.externalUserId ?? selectedPerson?.externalUserId)
    const canLogin = profile?.canLoginSnapshot ?? false
    const activeCertifications = certifications.filter((cert) => cert.effectiveStatus === 'active')
    const expiringCertifications = activeCertifications.filter((cert) => {
      const remaining = daysUntil(cert.expiresAt)
      return remaining != null && remaining <= 60
    })
    const missingRequirements = readiness?.requirements.filter((requirement) => requirement.requirementStatus !== 'satisfied') ?? []
    const permissionKeys = permissions.map((permission) => permission.permissionKey.toLowerCase())
    const hasProductSignal = (product: string) =>
      permissionKeys.some((permission) => permission.includes(product.toLowerCase()))
    const trainingSteps = onboardingJourney?.steps ?? []
    const completedTrainingCount = countBy(trainingSteps, (step) => ['complete', 'completed'].includes(step.status.toLowerCase()))
    const requiredTrainingCount = trainingSteps.length || trainingHistory.length
    const openTrainingCount = trainingSteps.length > 0
      ? trainingSteps.length - completedTrainingCount
      : countBy(trainingHistory, (item) => !item.eventKind.toLowerCase().includes('complete'))
    const overdueTrainingCount = readiness?.blockers.filter((blocker) => blocker.blockerSource === 'training').length ?? 0
    const hasAnyTrainingActivity = requiredTrainingCount > 0 || completedTrainingCount > 0 || openTrainingCount > 0 || overdueTrainingCount > 0
    const trainingCompletionPercent = requiredTrainingCount > 0
      ? Math.round((completedTrainingCount / requiredTrainingCount) * 100)
      : 0
    const openIncidents = incidents.filter((incident) => !['closed', 'complete', 'completed', 'resolved'].includes(incident.status.toLowerCase()))
    const closedIncidents = incidents.length - openIncidents.length
    const retrainingIncidentCount = countBy(incidents, (incident) => Boolean(incident.trainingReviewRequired || incident.trainarrRouting))
    const restrictedDocuments = documents.filter((document) => document.restrictedData || document.accessLevel === 'restricted')
    const needsActionDocuments = documents.filter((document) => {
      const expiresIn = daysUntil(document.expiresAt)
      return document.status.toLowerCase() !== 'active' || (expiresIn != null && expiresIn <= 30)
    })
    const evidenceDocumentCount = documents.filter((document) =>
      ['certification_copy', 'policy_acknowledgment', 'handbook_acknowledgment', 'corrective_action'].includes(document.documentTypeKey),
    ).length
    const readinessAllowed = readiness?.readinessStatus !== 'not_ready'
    const siteContextOrgUnitId = activeAssignment?.siteOrgUnitId ?? null
    const assignmentCards = lookup?.placement.activeAssignments.map((assignment) => ({
      key: assignment.assignmentId,
      title: assignment.positionName,
      subtitle: assignment.teamName,
      scope: assignment.assignmentPath || [assignment.siteName, assignment.departmentName, assignment.teamName].filter(Boolean).join(' / '),
      effective: `${formatDate(assignment.effectiveAt)} - ${assignment.endsAt ? formatDate(assignment.endsAt) : 'Present'}`,
      badge: assignment.isPrimary ? 'Primary' : humanize(assignment.status),
    })) ?? s.assignments.map((assignment) => ({
      key: assignment.assignmentId,
      title: orgUnitById.get(assignment.positionOrgUnitId) ?? 'Position assignment',
      subtitle: orgUnitById.get(assignment.teamOrgUnitId) ?? 'Team not named',
      scope: [
        orgUnitById.get(assignment.siteOrgUnitId),
        orgUnitById.get(assignment.departmentOrgUnitId),
        orgUnitById.get(assignment.teamOrgUnitId),
      ].filter(Boolean).join(' / ') || 'Scope not recorded',
      effective: `${formatDate(assignment.effectiveAt)} - ${assignment.endsAt ? formatDate(assignment.endsAt) : 'Present'}`,
      badge: assignment.isPrimary ? 'Primary' : humanize(assignment.status),
    }))
    const attentionItems = [
      ...readiness?.blockers.map((blocker) => ({
        title: blocker.certificationName ?? blocker.qualificationName ?? humanize(blocker.blockerType),
        detail: blocker.message,
        tone: 'bad' as PersonDetailTone,
      })) ?? [],
      ...expiringCertifications.slice(0, 2).map((cert) => ({
        title: `${cert.certificationName} expiring soon`,
        detail: cert.expiresAt ? `Expires ${formatDate(cert.expiresAt)}.` : 'Expiration review is needed.',
        tone: 'warn' as PersonDetailTone,
      })),
      ...openIncidents.slice(0, 2).map((incident) => ({
        title: incident.title,
        detail: `${humanize(incident.reasonCategoryKey)} incident from ${formatDate(incident.occurredAt)}.`,
        tone: 'bad' as PersonDetailTone,
      })),
      ...needsActionDocuments.slice(0, 1).map((document) => ({
        title: document.title,
        detail: 'Document status or expiration needs review.',
        tone: 'warn' as PersonDetailTone,
      })),
    ].slice(0, 4)
    const productRows = ['staffarr', 'loadarr', 'trainarr', 'maintainarr', 'routarr'].map((product) => {
      const matchingPermission = permissions.find((permission) => permission.permissionKey.toLowerCase().includes(product))
      return {
        product: humanize(product),
        role: matchingPermission?.sources[0]?.roleName ?? 'No role assigned',
        access: matchingPermission ? 'Granted' : 'Not granted',
        scope: matchingPermission?.scopeValue ?? (matchingPermission?.scopeType ? humanize(matchingPermission.scopeType) : 'None'),
        review: matchingPermission ? 'Managed by role' : 'N/A',
      }
    })
    const permissionFamilies = [
      {
        title: 'People directory',
        detail: hasProductSignal('staffarr') ? 'View and update assigned team members' : 'No StaffArr people permission detected',
        badge: permissions[0]?.sources[0]?.roleName ?? 'Role assignment',
      },
      {
        title: 'Assignments',
        detail: 'Create temporary assignments and request permanent changes',
        badge: activeAssignment?.siteName ?? siteName,
      },
      {
        title: 'Incidents',
        detail: openIncidents.length > 0 ? 'Personnel incident follow-up is visible' : 'No open incident scope',
        badge: retrainingIncidentCount > 0 ? 'Training follow-up' : 'Supervisor capability set',
      },
      {
        title: 'Documents',
        detail: restrictedDocuments.length > 0 ? 'Restricted personnel documents require HR visibility' : 'Standard personnel documents visible by policy',
        badge: 'Document visibility policy',
      },
    ]
    const timelineItems = recentActivity.slice(0, 8)

    const editorPanel = profile ? (
      <PersonProfileEditorPanel
        accessToken={s.accessToken}
        profile={profile}
        orgUnits={s.orgUnits}
        peopleOptions={s.people.map((person) => ({
          personId: person.personId,
          displayName: person.displayName,
        }))}
        siteContextOrgUnitId={siteContextOrgUnitId}
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

    const renderMetricRow = (metrics: Array<{ label: string; value: ReactNode; hint: string; tone: PersonDetailTone }>) => (
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {metrics.map((metric) => (
          <MetricCard key={metric.label} {...metric} />
        ))}
      </div>
    )

    const renderOverviewTab = () => (
      <div className="space-y-5">
        {showEditor ? editorPanel : null}
        {trainingLaunchError ? (
          <p className="rounded-xl border border-rose-800 bg-rose-950/20 px-4 py-3 text-sm text-rose-200">
            {trainingLaunchError}
          </p>
        ) : null}
        {s.personSummaryQuery.isError ? (
          <ApiErrorCallout
            title="Person summary unavailable"
            message={getErrorMessage(s.personSummaryQuery.error, 'Failed to load person summary.')}
            onRetry={() => void s.personSummaryQuery.refetch()}
            retryLabel="Retry summary"
          />
        ) : null}

        {renderMetricRow([
          { label: 'Open actions', value: attentionItems.length, hint: `${openTrainingCount} training / ${openIncidents.length} incident`, tone: attentionItems.length > 0 ? 'warn' : 'good' },
          { label: 'Certifications', value: activeCertifications.length, hint: `${expiringCertifications.length} expiring soon`, tone: expiringCertifications.length > 0 ? 'warn' : 'good' },
          {
            label: 'Training',
            value: `${trainingCompletionPercent}%`,
            hint: hasAnyTrainingActivity ? 'role path complete' : 'no training activity recorded',
            tone: hasAnyTrainingActivity ? (openTrainingCount > 0 ? 'info' : 'good') : 'warn',
          },
          { label: 'Incidents', value: openIncidents.length, hint: 'open follow-up', tone: openIncidents.length > 0 ? 'bad' : 'good' },
        ])}

        <div className="grid gap-5 xl:grid-cols-[1.25fr_1fr]">
          <SectionPanel
            title="Person snapshot"
            subtitle="Core StaffArr profile information used by other products through person references."
          >
            <div className="grid gap-3 md:grid-cols-2">
              <FieldTile label="Preferred name" value={profile?.preferredName ?? profile?.givenName ?? displayName} />
              <FieldTile label="Employment type" value={humanize(profile?.employmentType ?? selectedPerson?.employmentType)} />
              <FieldTile label="Primary site" value={siteName} />
              <FieldTile label="Department" value={departmentName} />
              <FieldTile label="Supervisor" value={managerName} />
              <FieldTile label="Position" value={positionName} />
              <FieldTile label="Person reference" value={personId ?? 'Not selected'} />
              <FieldTile label="Login account" value={hasUserAccount ? 'Linked' : 'Not linked'} />
            </div>
          </SectionPanel>

          <SectionPanel
            title="Attention needed"
            subtitle="Operational items surfaced from StaffArr and connected products."
          >
            <div className="space-y-3">
              {attentionItems.length > 0 ? attentionItems.map((item) => (
                <DotItem key={`${item.title}-${item.detail}`} title={item.title} detail={item.detail} tone={item.tone} />
              )) : (
                <EmptyDetailState text="No active attention items are currently flagged." />
              )}
            </div>
          </SectionPanel>
        </div>

        <div className="grid gap-5 xl:grid-cols-3">
          <SectionPanel title="Current assignment" subtitle="Where this person currently works.">
            <p className="text-2xl font-black text-[var(--color-text-primary)]">{teamName !== 'Not assigned' ? teamName : positionName}</p>
            <div className="mt-4 grid gap-3 sm:grid-cols-2">
              <FieldTile label="Location scope" value={siteName} />
              <FieldTile label="Coverage" value={activeAssignment?.reason ?? 'Primary role'} />
              <FieldTile label="Temporary coverage" value="None active" />
              <FieldTile label="Primary role" value={positionName} />
            </div>
          </SectionPanel>

          <SectionPanel title="Compliance posture" subtitle="Summary only; source records remain in owning products.">
            <div className="mb-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="font-bold text-[var(--color-text-primary)]">Authorization decision</p>
                  <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                    {readinessAllowed
                      ? 'Can perform assigned work with current StaffArr authority.'
                      : readiness?.blockers[0]?.message ?? 'Restrictions require review before normal work.'}
                  </p>
                </div>
                <Badge label={readinessAllowed ? 'Allowed' : 'Blocked'} tone={readinessAllowed ? 'good' : 'bad'} />
              </div>
            </div>
            <div className="space-y-3">
              <FieldTile label="Required certifications" value={`${activeCertifications.length} current`} />
              <FieldTile label="Required training" value={`${trainingCompletionPercent}% complete`} />
              <FieldTile label="Open incident follow-ups" value={`${openIncidents.length} open`} />
              <FieldTile label="Missing documents" value={`${needsActionDocuments.length} needs action`} />
            </div>
          </SectionPanel>

          <SectionPanel title="Quick actions" subtitle="Common actions from a person record.">
            <div className="space-y-3">
              <DetailCommandButton
                icon={<GraduationCap className="h-4 w-4" />}
                onClick={() => void handleAssignTraining()}
                disabled={isLaunchingTraining}
              >
                {isLaunchingTraining ? 'Opening TrainArr...' : 'Assign training manually'}
              </DetailCommandButton>
              <DetailCommandButton icon={<AlertTriangle className="h-4 w-4" />} onClick={() => s.setPeopleDetailTab('incidents')}>
                Open incident
              </DetailCommandButton>
              <DetailCommandButton icon={<Upload className="h-4 w-4" />} onClick={() => s.setPeopleDetailTab('documents')}>
                Upload document
              </DetailCommandButton>
              <DetailCommandButton icon={<ShieldCheck className="h-4 w-4" />} onClick={() => s.setPeopleDetailTab('permissions')}>
                Request permission review
              </DetailCommandButton>
            </div>
          </SectionPanel>
        </div>
      </div>
    )

    const renderPermissionsTab = () => (
      <div className="space-y-5">
        <div className="grid gap-5 xl:grid-cols-[1.1fr_0.9fr]">
          <SectionPanel
            title="Account and entitlement summary"
            subtitle="NexArr validates product entitlement. StaffArr displays role and scope details."
          >
            <div className="space-y-3">
              <FieldTile label="Login account" value={hasUserAccount ? 'Enabled' : 'Not linked'} />
              <FieldTile label="MFA" value={canLogin ? 'Required by NexArr' : 'No login requested'} />
              <FieldTile label="Product launch" value={hasUserAccount ? 'Allowed through NexArr handoff' : 'Unavailable'} />
              <FieldTile label="Permission review status" value="Current" />
            </div>
          </SectionPanel>

          <SectionPanel
            title="Permission families"
            subtitle="Business-readable permission groups. Raw permission keys stay out of normal person screens."
          >
            <div className="space-y-3">
              {permissionFamilies.map((family) => (
                <div key={family.title} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-bold text-[var(--color-text-primary)]">{family.title}</p>
                      <p className="mt-1 text-sm text-[var(--color-text-muted)]">{family.detail}</p>
                    </div>
                    <Badge label={family.badge} tone="info" />
                  </div>
                </div>
              ))}
            </div>
          </SectionPanel>
        </div>

        <SectionPanel
          title="Product access"
          subtitle="Visible cross-product access for this person. Scopes are business-facing and effective-dated."
        >
          <div className="overflow-hidden rounded-xl border border-[var(--color-border-subtle)]">
            <table className="w-full min-w-[760px] table-fixed text-left text-sm">
              <thead className="bg-[var(--color-bg-surface-elevated)] text-xs uppercase text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-4 py-3">Product</th>
                  <th className="px-4 py-3">Role</th>
                  <th className="px-4 py-3">Access</th>
                  <th className="px-4 py-3">Scope</th>
                  <th className="px-4 py-3">Review</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] text-[var(--color-text-secondary)]">
                {productRows.map((row) => (
                  <tr key={row.product}>
                    <td className="px-4 py-4 font-bold">{row.product}</td>
                    <td className="px-4 py-4">{row.role}</td>
                    <td className="px-4 py-4">
                      <Badge label={row.access} tone={row.access === 'Granted' ? 'good' : 'neutral'} />
                    </td>
                    <td className="px-4 py-4">{row.scope}</td>
                    <td className="px-4 py-4">{row.review}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </SectionPanel>

        <SectionPanel title="Pending permission activity" subtitle="Requests, approvals, denials, and reviews related to this person.">
          <div className="space-y-3">
            {permissions.length > 0 ? permissions.slice(0, 3).map((permission) => (
              <DotItem
                key={`${permission.permissionKey}-${permission.scopeType}-${permission.scopeValue ?? ''}`}
                title={permission.permissionName}
                detail={`${permission.permissionKey} across ${humanize(permission.scopeType)} scope.`}
                tone="good"
              />
            )) : (
              <EmptyDetailState text="No pending permission activity is currently recorded." />
            )}
          </div>
        </SectionPanel>
      </div>
    )

    const renderCertificationsTab = () => (
      <div className="space-y-5">
        {renderMetricRow([
          { label: 'Active', value: activeCertifications.length, hint: 'current certs', tone: 'good' },
          { label: 'Expiring soon', value: expiringCertifications.length, hint: 'next 60 days', tone: expiringCertifications.length > 0 ? 'warn' : 'good' },
          { label: 'Needs renewal', value: missingRequirements.length, hint: 'blocks one role', tone: missingRequirements.length > 0 ? 'bad' : 'good' },
          { label: 'External evidence', value: evidenceDocumentCount, hint: 'stored in RecordArr', tone: 'info' },
        ])}

        <SectionPanel
          title="Certification inventory"
          subtitle="Certification facts are shown here for person context. TrainArr remains the system of record for training-issued credentials."
        >
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            {certifications.length > 0 ? certifications.map((cert) => (
              <div key={cert.personCertificationId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
                <div className="flex min-h-16 items-start justify-between gap-3">
                  <div>
                    <p className="font-bold text-[var(--color-text-primary)]">{cert.certificationName}</p>
                    <p className="mt-1 text-sm text-[var(--color-text-muted)]">{humanize(cert.sourceType)}</p>
                  </div>
                  <Badge label={certificationLabel(cert.expiresAt, cert.effectiveStatus)} tone={certificationTone(cert.expiresAt, cert.effectiveStatus)} />
                </div>
                <div className="mt-4 space-y-3">
                  <FieldTile label="Issued" value={formatDate(cert.grantedAt)} />
                  <FieldTile label="Expires" value={formatDate(cert.expiresAt)} />
                  <FieldTile label="Owner" value={cert.sourceType.toLowerCase().includes('train') ? 'TrainArr' : 'StaffArr'} />
                </div>
              </div>
            )) : (
              <EmptyDetailState text="No certifications are recorded for this person." />
            )}
          </div>
        </SectionPanel>

        <SectionPanel
          title="Role-driven certification gaps"
          subtitle="Gaps are calculated from the current assignments, site requirements, and product workflow access."
        >
          <div className="space-y-3">
            {missingRequirements.length > 0 ? missingRequirements.map((requirement) => (
              <DotItem
                key={requirement.certificationDefinitionId}
                title={`${requirement.certificationName} ${humanize(requirement.requirementStatus)}`}
                detail={requirement.expiresAt ? `Expires ${formatDate(requirement.expiresAt)}.` : 'Requirement is not currently satisfied.'}
                tone={requirement.requirementStatus === 'missing' ? 'bad' : 'warn'}
              />
            )) : (
              <EmptyDetailState text="No role-driven certification gaps are currently flagged." />
            )}
          </div>
        </SectionPanel>
      </div>
    )

    const renderAssignmentsTab = () => (
      <div className="space-y-5">
        <SectionPanel
          title="Current assignments"
          subtitle="StaffArr owns the person-to-organization, person-to-location, and staff role assignment picture."
        >
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            {assignmentCards.length > 0 ? assignmentCards.map((assignment) => (
              <div key={assignment.key} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-sm text-[var(--color-text-muted)]">{assignment.subtitle}</p>
                    <p className="mt-1 text-lg font-black leading-6 text-[var(--color-text-primary)]">{assignment.title}</p>
                  </div>
                  <Badge label={assignment.badge} tone="info" />
                </div>
                <div className="mt-4 space-y-3">
                  <FieldTile label="Scope" value={assignment.scope} />
                  <FieldTile label="Effective" value={assignment.effective} />
                </div>
              </div>
            )) : (
              <EmptyDetailState text="No active assignments are recorded for this person." />
            )}
          </div>
        </SectionPanel>

        <div className="grid gap-5 xl:grid-cols-[1.1fr_0.9fr]">
          <SectionPanel title="Assignment hierarchy" subtitle="Business structure shown without exposing internal IDs.">
            <div className="space-y-3">
              <FieldTile label="Organization" value="STL Compliance" />
              <FieldTile label="Site" value={siteName} />
              <FieldTile label="Department" value={departmentName} />
              <FieldTile label="Team" value={teamName} />
              <FieldTile label="Position" value={positionName} />
              <FieldTile label="Reports to" value={managerName} />
            </div>
          </SectionPanel>

          <SectionPanel title="Temporary coverage" subtitle="Effective-dated coverage, delegations, and substitutions.">
            <div className="space-y-3">
              <DotItem title="No active temporary coverage" detail="This person is not currently covering another person, role, or site." tone="good" />
              {s.assignments.filter((assignment) => assignment.endsAt).slice(0, 2).map((assignment) => (
                <DotItem
                  key={assignment.assignmentId}
                  title={`Past coverage: ${orgUnitById.get(assignment.positionOrgUnitId) ?? 'Assigned role'}`}
                  detail={`${formatDate(assignment.effectiveAt)} - ${formatDate(assignment.endsAt)}.`}
                  tone="neutral"
                />
              ))}
            </div>
          </SectionPanel>
        </div>
      </div>
    )

    const renderTrainingTab = () => (
      <div className="space-y-5">
        {renderMetricRow([
          { label: 'Required', value: requiredTrainingCount, hint: 'current role path', tone: zeroWarnTone(requiredTrainingCount, 'info') },
          { label: 'Completed', value: completedTrainingCount, hint: `${trainingCompletionPercent}% complete`, tone: zeroWarnTone(completedTrainingCount, 'good') },
          { label: 'Due soon', value: openTrainingCount > 0 ? 1 : 0, hint: 'next 14 days', tone: zeroWarnTone(openTrainingCount > 0 ? 1 : 0, 'warn') },
          { label: 'Overdue', value: overdueTrainingCount, hint: 'requires attention', tone: zeroWarnTone(overdueTrainingCount, 'bad') },
        ])}

        <SectionPanel
          title="Training plan"
          subtitle="StaffArr shows person context. TrainArr owns training assignments, completions, evaluations, and certificates."
        >
          <div className="space-y-3">
            {trainingSteps.length > 0 ? trainingSteps.map((step) => (
              <div key={step.stepKey} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="font-bold text-[var(--color-text-primary)]">{step.title}</p>
                    <p className="mt-1 text-sm text-[var(--color-text-muted)]">{step.detail}</p>
                  </div>
                  <Badge label={humanize(step.status)} tone={step.status.toLowerCase().includes('complete') ? 'good' : 'warn'} />
                </div>
              </div>
            )) : trainingHistory.length > 0 ? trainingHistory.slice(0, 4).map((item) => (
              <DotItem
                key={item.entryId}
                title={item.summary}
                detail={`${humanize(item.eventKind)} on ${formatDate(item.occurredAt)}.`}
                tone={item.eventKind.toLowerCase().includes('complete') ? 'good' : 'info'}
                badge="TrainArr"
              />
            )) : (
              <EmptyDetailState text="No training plan is currently available for this person." />
            )}
          </div>
        </SectionPanel>

        <SectionPanel title="Training drivers" subtitle="Why this person has these training requirements.">
          <div className="grid gap-4 md:grid-cols-3">
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
              <p className="text-sm text-[var(--color-text-muted)]">Role driver</p>
              <p className="mt-2 text-xl font-black text-[var(--color-text-primary)]">{positionName}</p>
              <p className="mt-12 text-sm text-[var(--color-text-muted)]">Assigns leadership, incident documentation, safety, and role-specific curriculum.</p>
            </div>
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
              <p className="text-sm text-[var(--color-text-muted)]">Location driver</p>
              <p className="mt-2 text-xl font-black text-[var(--color-text-primary)]">{siteName}</p>
              <p className="mt-12 text-sm text-[var(--color-text-muted)]">Assigns site and department procedure requirements.</p>
            </div>
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
              <p className="text-sm text-[var(--color-text-muted)]">Incident driver</p>
              <p className="mt-2 text-xl font-black text-[var(--color-text-primary)]">{openIncidents.length > 0 ? 'Open follow-up' : 'None active'}</p>
              <p className="mt-12 text-sm text-[var(--color-text-muted)]">Incident follow-up can assign focused refresher training until closure.</p>
            </div>
          </div>
        </SectionPanel>
      </div>
    )

    const renderIncidentsTab = () => (
      <div className="space-y-5">
        {renderMetricRow([
          { label: 'Open', value: openIncidents.length, hint: 'follow-up active', tone: openIncidents.length > 0 ? 'warn' : 'good' },
          { label: 'Closed', value: closedIncidents, hint: 'last 12 months', tone: 'good' },
          { label: 'Safety', value: countBy(incidents, (incident) => incident.reasonCategoryKey === 'safety'), hint: 'near miss', tone: 'bad' },
          { label: 'Retraining', value: retrainingIncidentCount, hint: 'assigned', tone: retrainingIncidentCount > 0 ? 'info' : 'good' },
        ])}

        <SectionPanel
          title="Incident records"
          subtitle="StaffArr owns central person incident visibility and routes training follow-up to TrainArr when needed."
          actions={
            s.canManagePersonIncidents && personId ? (
              <Link
                to={`/incidents/create?personId=${encodeURIComponent(personId)}`}
                className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-accent)] px-4 py-2 text-sm font-bold text-white transition hover:bg-[var(--color-accent-hover)]"
              >
                Create incident
              </Link>
            ) : null
          }
        >
          <div className="grid gap-4 lg:grid-cols-3">
            {incidents.length > 0 ? incidents.slice(0, 6).map((incident) => (
              <div key={incident.incidentId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="font-bold text-[var(--color-text-primary)]">{incident.title}</p>
                    <p className="mt-1 text-sm text-[var(--color-text-muted)]">{humanize(incident.reasonCategoryKey)} - {formatDate(incident.occurredAt)}</p>
                  </div>
                  <Badge label={humanize(incident.status)} tone={openIncidents.some((openIncident) => openIncident.incidentId === incident.incidentId) ? 'warn' : 'good'} />
                </div>
                <div className="mt-4 space-y-3">
                  <FieldTile label="Owner" value="StaffArr" />
                  <FieldTile label="Outcome" value={incident.trainarrRouting ? 'Assigned refresher training' : humanize(incident.readinessDecision ?? 'reviewed')} />
                </div>
                <div className="mt-4 flex flex-wrap gap-2">
                  <button
                    type="button"
                    onClick={() => s.setSelectedIncidentId(incident.incidentId)}
                    className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-xs font-bold text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
                  >
                    Open incident
                  </button>
                  <button
                    type="button"
                    onClick={() => s.setPeopleDetailTab('training')}
                    className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-xs font-bold text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
                  >
                    View follow-up
                  </button>
                </div>
              </div>
            )) : (
              <EmptyDetailState text="No personnel incidents are recorded for this person." />
            )}
          </div>
        </SectionPanel>

        <SectionPanel title="Corrective actions and follow-ups" subtitle="Visible follow-ups for managers and authorized HR users.">
          <div className="space-y-3">
            {openIncidents.length > 0 ? openIncidents.map((incident) => (
              <DotItem
                key={incident.incidentId}
                title={`${incident.title} follow-up`}
                detail={incident.trainingReviewRequired ? 'Training review is required before closure.' : 'Manager review is pending.'}
                tone={incident.trainingReviewRequired ? 'bad' : 'warn'}
              />
            )) : (
              <EmptyDetailState text="No open corrective actions are currently assigned." />
            )}
          </div>
        </SectionPanel>
      </div>
    )

    const renderDocumentsTab = () => (
      <div className="space-y-5">
        {renderMetricRow([
          { label: 'Filed', value: documents.length, hint: 'person documents', tone: 'good' },
          { label: 'Needs action', value: needsActionDocuments.length, hint: 'acknowledgment', tone: needsActionDocuments.length > 0 ? 'warn' : 'good' },
          { label: 'Restricted', value: restrictedDocuments.length, hint: 'HR visibility', tone: restrictedDocuments.length > 0 ? 'bad' : 'good' },
          { label: 'Evidence', value: evidenceDocumentCount, hint: 'cert/supporting docs', tone: 'info' },
        ])}

        <SectionPanel
          title="Documents"
          subtitle="RecordArr stores durable document records. StaffArr controls person context and visibility surfaces."
        >
          <div className="overflow-hidden rounded-xl border border-[var(--color-border-subtle)]">
            <table className="w-full min-w-[760px] table-fixed text-left text-sm">
              <thead className="bg-[var(--color-bg-surface-elevated)] text-xs uppercase text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-4 py-3">Document</th>
                  <th className="px-4 py-3">Category</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3">Owner</th>
                  <th className="px-4 py-3">Visibility</th>
                  <th className="px-4 py-3">Updated</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] text-[var(--color-text-secondary)]">
                {documents.length > 0 ? documents.map((document) => (
                  <tr key={document.documentId}>
                    <td className="px-4 py-4 font-medium">{document.title}</td>
                    <td className="px-4 py-4">{humanize(document.documentTypeKey)}</td>
                    <td className="px-4 py-4">
                      <Badge label={humanize(document.status)} tone={document.status.toLowerCase() === 'active' ? 'good' : 'warn'} />
                    </td>
                    <td className="px-4 py-4">RecordArr</td>
                    <td className="px-4 py-4">{humanize(document.accessLevel)}</td>
                    <td className="px-4 py-4">{formatDate(document.updatedAt)}</td>
                  </tr>
                )) : (
                  <tr>
                    <td className="px-4 py-4 text-[var(--color-text-muted)]" colSpan={6}>No documents are recorded for this person.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </SectionPanel>

        <SectionPanel title="Document actions" subtitle="Actions should respect visibility, retention, and product ownership.">
          <div className="grid gap-3 md:grid-cols-4">
            <DetailCommandButton icon={<Upload className="h-4 w-4" />} onClick={() => undefined}>Upload document</DetailCommandButton>
            <DetailCommandButton icon={<ClipboardCheck className="h-4 w-4" />} onClick={() => undefined}>Send acknowledgment</DetailCommandButton>
            <DetailCommandButton icon={<FileText className="h-4 w-4" />} onClick={() => undefined}>Request evidence</DetailCommandButton>
            <DetailCommandButton icon={<ExternalLink className="h-4 w-4" />} onClick={() => undefined}>Open in RecordArr</DetailCommandButton>
          </div>
        </SectionPanel>

        <div className="rounded-xl border border-dashed border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4 text-sm text-[var(--color-text-muted)]">
          Visibility should be policy-driven. Normal managers should not see restricted HR, medical, or sensitive records unless granted by role and tenant policy.
        </div>
      </div>
    )

    const renderHistoryTab = () => (
      <div className="space-y-5">
        <SectionPanel
          title="Person history"
          subtitle="Readable audit timeline across StaffArr and connected products. Raw event IDs stay hidden."
        >
          <div className="mb-4 flex flex-wrap gap-2">
            {([
              { label: 'All events', value: '' },
              { label: 'StaffArr', value: 'permission' },
              { label: 'TrainArr', value: 'certification' },
              { label: 'RecordArr', value: 'personnel_document' },
              { label: 'Permission changes', value: 'permission' },
            ] satisfies Array<{ label: string; value: typeof s.personTimelineCategoryFilter }>).map((filter) => (
              <button
                type="button"
                key={`${filter.label}-${filter.value}`}
                onClick={() => {
                  s.setPersonTimelineCategoryFilter(filter.value)
                  s.setPersonTimelinePage(1)
                }}
                className="rounded-full border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-1 text-xs font-bold text-[var(--color-text-secondary)] hover:border-[var(--color-accent-border)] hover:text-[var(--color-text-primary)]"
              >
                {filter.label}
              </button>
            ))}
          </div>
          <div className="space-y-4 border-l border-[var(--color-border-subtle)] pl-6">
            {timelineItems.length > 0 ? timelineItems.map((entry) => (
              <div key={entry.entryId} className="relative rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
                <span className="absolute -left-[31px] top-6 h-4 w-4 rounded-full border-2 border-[var(--color-accent)] bg-[var(--color-bg-surface)]" />
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-sm text-[var(--color-text-muted)]">{formatDate(entry.occurredAt)}</p>
                    <p className="mt-1 font-bold text-[var(--color-text-primary)]">{entry.title}</p>
                    <p className="mt-1 text-sm text-[var(--color-text-muted)]">{entry.detail ?? humanize(entry.eventType)}</p>
                  </div>
                  <Badge label={humanize(entry.category)} tone="info" />
                </div>
              </div>
            )) : (
              <EmptyDetailState text="No timeline events are currently available." />
            )}
          </div>
        </SectionPanel>

        <SectionPanel title="Audit interpretation" subtitle="History is meant for understandable business review; deeper event payloads stay in admin audit tooling.">
          <div className="space-y-3">
            <DotItem
              title="Most recent meaningful change"
              detail={historySummary?.lastEventAt ? `Last event recorded ${formatDate(historySummary.lastEventAt)}.` : 'No material event has been recorded yet.'}
              tone="info"
            />
            <DotItem
              title="Open chain of action"
              detail={openIncidents.length > 0 ? 'Incident follow-up remains open until training and supervisor review are complete.' : 'No open incident chain is currently recorded.'}
              tone={openIncidents.length > 0 ? 'warn' : 'good'}
            />
          </div>
        </SectionPanel>
      </div>
    )

    const renderMainContent = () => {
      switch (s.peopleDetailTab) {
        case 'permissions':
          return renderPermissionsTab()
        case 'certifications':
          return renderCertificationsTab()
        case 'assignments':
          return renderAssignmentsTab()
        case 'training':
          return renderTrainingTab()
        case 'incidents':
          return renderIncidentsTab()
        case 'documents':
          return renderDocumentsTab()
        case 'history':
          return renderHistoryTab()
        case 'overview':
        default:
          return renderOverviewTab()
      }
    }

    return (
      <div data-testid="staffarr-person-profile" className="min-h-screen bg-[var(--color-bg-app)] px-4 py-6 text-[var(--color-text-primary)] sm:px-6 lg:px-8">
        <div className="mx-auto max-w-[1320px] space-y-6">
          <nav className="flex flex-wrap items-center gap-2 text-sm text-[var(--color-text-muted)]" aria-label="Breadcrumb">
            <span>StaffArr</span>
            <span>/</span>
            <Link to="/people/drawer" className="hover:text-[var(--color-text-primary)]">People</Link>
            <span>/</span>
            <span className="font-bold text-[var(--color-text-primary)]">{displayName}</span>
          </nav>

          <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 shadow-xl shadow-slate-950/15">
            <div className="grid gap-6 lg:grid-cols-[1fr_430px] lg:items-center">
              <div className="flex flex-col gap-5 sm:flex-row sm:items-start">
                <div className="flex h-20 w-20 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-[var(--color-accent)] to-[var(--color-info)] text-3xl font-black text-white">
                  {initialsForName(displayName)}
                </div>
                <div className="min-w-0">
                  <div className="flex flex-wrap items-center gap-3">
                    <h1 className="text-3xl font-black tracking-normal text-[var(--color-text-primary)] md:text-4xl">{displayName}</h1>
                    <Badge label={humanize(employmentStatus)} tone={employmentStatus === 'active' ? 'good' : 'warn'} />
                    <Badge label={humanize(profile?.employmentType ?? selectedPerson?.employmentType)} tone="info" />
                  </div>
                  <p className="mt-2 text-xl text-[var(--color-text-primary)]">{jobTitle}</p>
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">
                    {personId ?? 'No person selected'} - {departmentName} - {siteName}
                  </p>
                  <div className="mt-4 flex flex-wrap gap-2">
                    <Badge label={activeAssignment?.reason ?? humanize(profile?.workRelationshipType ?? selectedPerson?.workRelationshipType)} tone="info" />
                    <Badge label={managerName === noManagerLabel ? 'No supervisor assigned' : 'Supervisor responsibilities'} tone="warn" />
                    <Badge label={readinessAllowed ? 'Compliance tracked' : 'Compliance review'} tone={readinessAllowed ? 'info' : 'bad'} />
                  </div>
                </div>
              </div>

              <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
                <dl className="grid grid-cols-2 gap-x-5 gap-y-4">
                  <div>
                    <dt className="text-xs font-black uppercase text-[var(--color-text-muted)]">Supervisor</dt>
                    <dd className="mt-1 text-sm font-medium text-[var(--color-text-primary)]">{managerName}</dd>
                  </div>
                  <div>
                    <dt className="text-xs font-black uppercase text-[var(--color-text-muted)]">Start date</dt>
                    <dd className="mt-1 text-sm font-medium text-[var(--color-text-primary)]">{formatDate(profile?.startDate ?? profile?.expectedStartDate)}</dd>
                  </div>
                  <div>
                    <dt className="text-xs font-black uppercase text-[var(--color-text-muted)]">Email</dt>
                    <dd className="mt-1 break-words text-sm font-medium text-[var(--color-text-primary)]">{email}</dd>
                  </div>
                  <div>
                    <dt className="text-xs font-black uppercase text-[var(--color-text-muted)]">Phone</dt>
                    <dd className="mt-1 text-sm font-medium text-[var(--color-text-primary)]">{phone}</dd>
                  </div>
                </dl>
              </div>
            </div>
          </section>

          <section className="overflow-hidden rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] shadow-xl shadow-slate-950/12">
            <div className="flex flex-col gap-4 px-6 py-5 lg:flex-row lg:items-start lg:justify-between">
              <div>
                <h2 className="text-2xl font-black text-[var(--color-text-primary)]">{activeTab.label}</h2>
                <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                  StaffArr person profile sections. Business-facing keys are shown; internal IDs stay hidden.
                </p>
              </div>
              <div className="flex flex-wrap gap-3">
                <DetailCommandButton
                  icon={<MessageSquarePlus className="h-4 w-4" />}
                  onClick={() => {
                    s.setPeopleDetailTab('history')
                    setShowEditor(false)
                  }}
                >
                  Add note
                </DetailCommandButton>
                <DetailCommandButton
                  icon={<ShieldCheck className="h-4 w-4" />}
                  onClick={() => {
                    s.setPeopleDetailTab('permissions')
                    setShowEditor(false)
                  }}
                >
                  Request change
                </DetailCommandButton>
                {s.canManagePeopleProfiles ? (
                  <DetailCommandButton
                    icon={<Pencil className="h-4 w-4" />}
                    variant="primary"
                    onClick={() => {
                      s.setPeopleDetailTab('overview')
                      setShowEditor((current) => !current)
                    }}
                  >
                    Edit person
                  </DetailCommandButton>
                ) : null}
              </div>
            </div>

            <div className="border-y border-[var(--color-border-subtle)] px-6">
              <div className="flex gap-2 overflow-x-auto">
                {DETAIL_TABS.map((tab) => (
                  <button
                    key={tab.key}
                    type="button"
                    role="tab"
                    aria-selected={s.peopleDetailTab === tab.key}
                    onClick={() => {
                      s.setPeopleDetailTab(tab.key as typeof s.peopleDetailTab)
                      if (tab.key !== 'overview') {
                        setShowEditor(false)
                      }
                    }}
                    className={[
                      'relative min-h-14 shrink-0 px-4 text-sm font-black transition',
                      s.peopleDetailTab === tab.key
                        ? 'text-[var(--color-text-primary)]'
                        : 'text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)]',
                    ].join(' ')}
                  >
                    {tab.label}
                    {s.peopleDetailTab === tab.key ? (
                      <span className="absolute inset-x-3 bottom-0 h-1 rounded-t bg-[var(--color-accent)]" />
                    ) : null}
                  </button>
                ))}
              </div>
            </div>

            <div className="p-6">
              {renderMainContent()}
            </div>
          </section>
        </div>
      </div>
    )
  }

  return (
    <>
      {mode === 'create' ? (
        <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4 text-sm text-[var(--color-text-primary)]">
          <p>Create people in a guided flow using business-aligned identity, placement, and login intent fields.</p>
          <ol className="mt-2 list-decimal space-y-1 pl-5">
            <li>Step 1: Capture the canonical identity fields used across StaffArr and NexArr.</li>
            <li>Step 2: Set contact, lifecycle status, and work relationship details.</li>
            <li>Step 3: Seed placement in one creation flow.</li>
          </ol>
        </div>
      ) : null}

      {renderDirectorySection()}

      {mode === 'create' ? (
        <CreatePersonPanel
          accessToken={s.accessToken}
          tenantId={s.session.tenantId}
          complianceCoreApiBase={complianceCoreApiBase}
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
