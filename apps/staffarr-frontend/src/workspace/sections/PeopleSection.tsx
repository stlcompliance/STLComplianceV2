import {
  DetailBadge as Badge,
  ApiErrorCallout,
  getErrorMessage,
  ProfileDetailsLayout,
  type DetailTabConfig,
  type DetailTone,
} from '@stl/shared-ui'
import {
  AlertTriangle,
  Award,
  BriefcaseBusiness,
  CalendarClock,
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
import { useEffect, useMemo, useState } from 'react'
import { createLaunchHandoff } from '../../api/client'
import { CreatePersonPanel } from '../../components/CreatePersonPanel'
import { ManagerHierarchyPanel } from '../../components/ManagerHierarchyPanel'
import { PersonHistorySummaryPanel } from '../../components/PersonHistorySummaryPanel'
import { PersonLookupPanel } from '../../components/PersonLookupPanel'
import { PersonOffboardingPanel } from '../../components/PersonOffboardingPanel'
import { PersonOrgAssignmentsManager } from '../../components/PersonOrgAssignmentsManager'
import { PersonProfileEditorPanel } from '../../components/PersonProfileEditorPanel'
import { PersonTimelinePanel } from '../../components/PersonTimelinePanel'
import { PersonTrainarrTrainingHistoryPanel } from '../../components/PersonTrainarrTrainingHistoryPanel'
import { PersonnelDocumentsPanel } from '../../components/PersonnelDocumentsPanel'
import { PersonnelNotesPanel } from '../../components/PersonnelNotesPanel'
import { TrainingAcknowledgementsPanel } from '../../components/TrainingAcknowledgementsPanel'
import { WorkforceOnboardingJourneyPanel } from '../../components/WorkforceOnboardingJourneyPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'
import { CertificationsSection } from './CertificationsSection'
import { IncidentsSection } from './IncidentsSection'
import { PermissionsSection } from './PermissionsSection'

type Props = { state: StaffArrWorkspaceState }
type PeopleViewMode = 'drawer' | 'details' | 'create'
type DrawerColumnKey = 'name' | 'email' | 'jobTitle' | 'orgUnit' | 'status' | 'manager'
type Tone = DetailTone

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
  const [isLaunchingTraining, setIsLaunchingTraining] = useState(false)
  const [trainingLaunchError, setTrainingLaunchError] = useState<string | null>(null)
  const [showMoreActions, setShowMoreActions] = useState(false)
  const [showOffboardingShortcut, setShowOffboardingShortcut] = useState(false)
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
    setShowOffboardingShortcut(false)
    setShowMoreActions(false)
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
                            to={`/people/details?person=${encodeURIComponent(person.personId)}&tab=overview`}
                            onClick={() => s.setSelectedPersonId(person.personId, { syncDetailQuery: true, tab: 'overview' })}
                            className="text-sky-300 hover:text-sky-200 hover:underline"
                          >
                            View
                          </Link>
                          <Link
                            to={`/people/details?person=${encodeURIComponent(person.personId)}&tab=overview`}
                            state={{ openEditor: true }}
                            onClick={() => s.setSelectedPersonId(person.personId, { syncDetailQuery: true, tab: 'overview' })}
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
                      s.setSelectedPersonId(person.personId, { syncDetailQuery: true })
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
      setTrainingLaunchError(
        error instanceof Error ? error.message : 'Failed to launch TrainArr via suite handoff.',
      )
      setIsLaunchingTraining(false)
    }
  }

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
    const hasUserAccount = profile?.hasUserAccountSnapshot ?? Boolean(profile?.externalUserId ?? selectedPerson?.externalUserId)
    const canLogin = profile?.canLoginSnapshot ?? false
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
    const siteContextOrgUnitId = activeAssignment?.siteOrgUnitId ?? null

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

    const renderOverviewTab = () => (
      <>
        {s.personSummaryQuery.isLoading ? (
          <p className="rounded-xl border border-slate-800 bg-slate-950/60 px-4 py-3 text-sm text-slate-400">
            Loading integrated person summary…
          </p>
        ) : s.personSummaryQuery.isError ? (
          <ApiErrorCallout
            title="Integrated person summary unavailable"
            message={getErrorMessage(s.personSummaryQuery.error, 'Failed to load person summary.')}
            onRetry={() => void s.personSummaryQuery.refetch()}
            retryLabel="Retry summary"
          />
        ) : s.personSummaryQuery.data ? (
          <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <h3 className="text-lg font-bold text-white">Integrated person summary</h3>
                <p className="mt-1 text-sm text-slate-400">
                  StaffArr-owned summary snapshot combining readiness, permissions, training history, and history counts.
                </p>
              </div>
              <Badge
                label={s.personSummaryQuery.data.readiness.readinessStatus === 'ready' ? 'Ready' : 'Not ready'}
                tone={s.personSummaryQuery.data.readiness.readinessStatus === 'ready' ? 'good' : 'bad'}
              />
            </div>
            <dl className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
              <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
                <dt className="text-xs uppercase tracking-wide text-slate-500">Qualifications snapshot</dt>
                <dd className="mt-2 text-sm text-slate-100">
                  {s.personSummaryQuery.data.qualificationsSnapshot.totalCount} event
                  {s.personSummaryQuery.data.qualificationsSnapshot.totalCount === 1 ? '' : 's'}
                </dd>
                <p className="mt-2 text-xs text-slate-500">{s.personSummaryQuery.data.qualificationsSnapshot.sourceNote}</p>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
                <dt className="text-xs uppercase tracking-wide text-slate-500">History summary</dt>
                <dd className="mt-2 text-sm text-slate-100">
                  {s.personSummaryQuery.data.historySummary.eventCount} total events
                </dd>
                <p className="mt-2 text-xs text-slate-500">
                  {s.personSummaryQuery.data.historySummary.incidentCount} incidents ·{' '}
                  {s.personSummaryQuery.data.historySummary.certificationCount} certifications ·{' '}
                  {s.personSummaryQuery.data.historySummary.permissionCount} permissions
                </p>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
                <dt className="text-xs uppercase tracking-wide text-slate-500">Active restrictions</dt>
                <dd className="mt-2 text-sm text-slate-100">
                  {s.personSummaryQuery.data.activeRestrictions.length} active
                </dd>
                <p className="mt-2 text-xs text-slate-500">
                  {s.personSummaryQuery.data.activeRestrictions.length > 0
                    ? s.personSummaryQuery.data.activeRestrictions[0]!.reason
                    : 'No active restriction records.'}
                </p>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-4">
                <dt className="text-xs uppercase tracking-wide text-slate-500">Permission projection</dt>
                <dd className="mt-2 text-sm text-slate-100">
                  {s.personSummaryQuery.data.permissionProjection.permissions.length} permissions
                </dd>
                <p className="mt-2 text-xs text-slate-500">
                  Last recomputed {new Date(s.personSummaryQuery.data.permissionProjection.computedAt).toLocaleString()}
                </p>
              </div>
            </dl>
          </section>
        ) : null}
        {showEditor ? editorPanel : null}
        {trainingLaunchError ? (
          <p className="rounded-xl border border-rose-800 bg-rose-950/20 px-4 py-3 text-sm text-rose-200">
            {trainingLaunchError}
          </p>
        ) : null}
        {showOffboardingShortcut && selectedPerson ? (
          <PersonOffboardingPanel
            personId={selectedPerson.personId}
            personDisplayName={selectedPerson.displayName}
            peopleOptions={s.people.map((person) => ({
              personId: person.personId,
              displayName: person.displayName,
            }))}
            offboarding={s.personOffboardingQuery.data ?? null}
            isLoading={s.personOffboardingQuery.isLoading}
            isError={s.personOffboardingQuery.isError}
            readErrorMessage={
              s.personOffboardingQuery.isError
                ? getErrorMessage(
                    s.personOffboardingQuery.error,
                    'Failed to load offboarding workflow.',
                  )
                : null
            }
            onRetryRead={() => void s.personOffboardingQuery.refetch()}
            canManage={s.canManagePeopleProfiles}
            isSubmitting={s.startOffboardingMutation.isPending || s.executeOffboardingMutation.isPending}
            actionErrorMessage={
              s.offboardingMutationError
                ? getErrorMessage(s.offboardingMutationError, 'Failed to save offboarding changes.')
                : null
            }
            onStart={async (request) => {
              await s.startOffboardingMutation.mutateAsync({
                personId: selectedPerson.personId,
                ...request,
              })
            }}
            onExecute={async (request) => {
              const offboardingId = s.personOffboardingQuery.data?.offboardingId
              if (!offboardingId) return
              await s.executeOffboardingMutation.mutateAsync({
                personId: selectedPerson.personId,
                offboardingId,
                ...request,
              })
            }}
          />
        ) : null}

        <div className="grid gap-5 lg:grid-cols-2">
          <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <div className="mb-4 flex items-center justify-between gap-3">
              <h3 className="text-lg font-bold text-white">Product permissions</h3>
              <Badge label="StaffArr-owned access" tone="info" />
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
                detail={hasProductSignal('trainarr') ? 'Published training history and acknowledgements' : 'No direct access'}
                allowed={hasProductSignal('trainarr')}
              />
              <ProductPermissionCard
                product="StaffArr"
                role={hasProductSignal('staffarr') || activeRoleAssignments.length > 0 ? 'People workspace user' : 'No product entitlement'}
                detail={hasProductSignal('staffarr') || activeRoleAssignments.length > 0 ? 'Profile, access, incident, history, and document workflows' : 'No direct access'}
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
              <button
                type="button"
                onClick={() => s.setPeopleDetailTab('certifications')}
                className="text-sm font-semibold text-sky-300 hover:text-sky-200"
              >
                Open certifications
              </button>
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

        {selectedPerson ? (
          <PersonLookupPanel
            personId={selectedPerson.personId}
            personDisplayName={selectedPerson.displayName}
            lookup={s.personLookupQuery.data ?? null}
            isLoading={s.personLookupQuery.isLoading}
            isError={s.personLookupQuery.isError}
            readErrorMessage={
              s.personLookupQuery.isError
                ? getErrorMessage(s.personLookupQuery.error, 'Failed to load person lookup.')
                : null
            }
            onRetryRead={() => void s.personLookupQuery.refetch()}
          />
        ) : null}
      </>
    )

    const renderAssignmentsTab = () => {
      if (!selectedPerson) {
        return <p className="text-sm text-slate-400">Select a person to manage assignments.</p>
      }

      return (
        <>
          <PersonOrgAssignmentsManager
            personId={selectedPerson.personId}
            personDisplayName={selectedPerson.displayName}
            orgUnits={s.orgUnits}
            assignments={s.assignments}
            isLoading={s.assignmentQuery.isLoading}
            isError={s.assignmentQuery.isError}
            readErrorMessage={
              s.assignmentQuery.isError
                ? getErrorMessage(s.assignmentQuery.error, 'Failed to load org assignments.')
                : null
            }
            onRetryRead={() => void s.assignmentQuery.refetch()}
            canManage={s.canManageHierarchy}
            isSubmitting={
              s.createAssignmentMutation.isPending ||
              s.updateAssignmentMutation.isPending ||
              s.updateAssignmentStatusMutation.isPending
            }
            actionErrorMessage={
              s.assignmentMutationError
                ? getErrorMessage(s.assignmentMutationError, 'Failed to save assignment changes.')
                : null
            }
            onCreate={async (request) => {
              await s.createAssignmentMutation.mutateAsync({
                personId: selectedPerson.personId,
                request,
              })
            }}
            onUpdate={async (assignmentId, request) => {
              await s.updateAssignmentMutation.mutateAsync({
                personId: selectedPerson.personId,
                assignmentId,
                request,
              })
            }}
            onStatusChange={async (assignmentId, request) => {
              await s.updateAssignmentStatusMutation.mutateAsync({
                personId: selectedPerson.personId,
                assignmentId,
                request,
              })
            }}
          />

          <ManagerHierarchyPanel
            selectedPersonId={selectedPerson.personId}
            selectedPersonDisplayName={selectedPerson.displayName}
            people={s.people}
            managerChain={s.managerChain}
            subordinates={s.subordinates}
            selectedSubordinateId={s.selectedSubordinateId}
            selectedSubordinate={s.selectedSubordinateDetail}
            isLoading={s.managerChainQuery.isLoading || s.subordinatesQuery.isLoading}
            isError={s.managerChainQuery.isError || s.subordinatesQuery.isError}
            readErrorMessage={
              s.managerChainQuery.isError
                ? getErrorMessage(s.managerChainQuery.error, 'Failed to load manager chain.')
                : s.subordinatesQuery.isError
                  ? getErrorMessage(s.subordinatesQuery.error, 'Failed to load subordinate hierarchy.')
                  : null
            }
            onRetryRead={() => {
              void s.managerChainQuery.refetch()
              void s.subordinatesQuery.refetch()
            }}
            isLoadingSubordinateDetail={s.subordinateDetailQuery.isLoading}
            isSubordinateDetailError={s.subordinateDetailQuery.isError}
            subordinateDetailErrorMessage={
              s.subordinateDetailQuery.isError
                ? getErrorMessage(s.subordinateDetailQuery.error, 'Failed to load subordinate detail.')
                : null
            }
            onRetrySubordinateDetail={() => void s.subordinateDetailQuery.refetch()}
            canManage={s.canManageHierarchy}
            isSubmitting={s.updateManagerMutation.isPending}
            actionErrorMessage={
              s.managerMutationError
                ? getErrorMessage(s.managerMutationError, 'Failed to update manager.')
                : null
            }
            onSelectSubordinate={s.setSelectedSubordinateId}
            onUpdateManager={async (managerPersonId) => {
              await s.updateManagerMutation.mutateAsync({
                personId: selectedPerson.personId,
                managerPersonId,
              })
            }}
          />
        </>
      )
    }

    const renderTrainingTab = () => (
      <>
        <WorkforceOnboardingJourneyPanel
          accessToken={s.accessToken}
          personDisplayName={displayName}
          journey={s.workforceOnboardingJourneyQuery.data ?? null}
          isLoading={s.workforceOnboardingJourneyQuery.isLoading}
          isError={s.workforceOnboardingJourneyQuery.isError}
          readErrorMessage={
            s.workforceOnboardingJourneyQuery.isError
              ? getErrorMessage(s.workforceOnboardingJourneyQuery.error, 'Failed to load workforce onboarding journey.')
              : null
          }
          onRetryRead={() => void s.workforceOnboardingJourneyQuery.refetch()}
        />

        {selectedPerson ? (
          <TrainingAcknowledgementsPanel
            accessToken={s.accessToken}
            personId={selectedPerson.personId}
            displayName={selectedPerson.displayName}
          />
        ) : null}

        <PersonTrainarrTrainingHistoryPanel
          personDisplayName={displayName}
          history={s.trainarrTrainingHistoryQuery.data ?? null}
          isLoading={s.trainarrTrainingHistoryQuery.isLoading}
          isError={s.trainarrTrainingHistoryQuery.isError}
          readErrorMessage={
            s.trainarrTrainingHistoryQuery.isError
              ? getErrorMessage(s.trainarrTrainingHistoryQuery.error, 'Failed to load TrainArr training history.')
              : null
          }
          onRetryRead={() => void s.trainarrTrainingHistoryQuery.refetch()}
        />
      </>
    )

    const renderDocumentsTab = () => {
      if (!selectedPerson) {
        return <p className="text-sm text-slate-400">Select a person to view documents.</p>
      }

      return (
        <PersonnelDocumentsPanel
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          accessToken={s.accessToken}
          documents={s.personDocuments}
          selectedDocumentId={s.selectedDocumentId}
          selectedDocument={s.documentDetailQuery.data ?? null}
          isLoading={s.personDocumentsQuery.isLoading}
          isError={s.personDocumentsQuery.isError}
          readErrorMessage={
            s.personDocumentsQuery.isError
              ? getErrorMessage(s.personDocumentsQuery.error, 'Failed to load personnel documents.')
              : null
          }
          onRetryRead={() => void s.personDocumentsQuery.refetch()}
          isLoadingDetail={s.documentDetailQuery.isLoading}
          isDetailError={s.documentDetailQuery.isError}
          detailErrorMessage={
            s.documentDetailQuery.isError
              ? getErrorMessage(s.documentDetailQuery.error, 'Failed to load document detail.')
              : null
          }
          onRetryDetail={() => void s.documentDetailQuery.refetch()}
          canManage={s.canManagePersonDocuments}
          isSubmitting={s.uploadDocumentMutation.isPending}
          actionErrorMessage={
            s.documentMutationError
              ? getErrorMessage(s.documentMutationError, 'Failed to upload personnel document.')
              : null
          }
          onSelectDocument={s.setSelectedDocumentId}
          onUploadDocument={async (request) => {
            await s.uploadDocumentMutation.mutateAsync(request)
          }}
          contentUrlFor={(documentId) => s.personnelDocumentContentUrl(selectedPerson.personId, documentId)}
        />
      )
    }

    const renderHistoryTab = () => {
      if (!selectedPerson) {
        return <p className="text-sm text-slate-400">Select a person to review history.</p>
      }

      return (
        <>
          <PersonnelNotesPanel
            personId={selectedPerson.personId}
            personDisplayName={selectedPerson.displayName}
            notes={s.personNotes}
            selectedNoteId={s.selectedNoteId}
            selectedNote={s.noteDetailQuery.data ?? null}
            isLoading={s.personNotesQuery.isLoading}
            isError={s.personNotesQuery.isError}
            readErrorMessage={
              s.personNotesQuery.isError
                ? getErrorMessage(s.personNotesQuery.error, 'Failed to load personnel notes.')
                : null
            }
            onRetryRead={() => void s.personNotesQuery.refetch()}
            isLoadingDetail={s.noteDetailQuery.isLoading}
            isDetailError={s.noteDetailQuery.isError}
            detailErrorMessage={
              s.noteDetailQuery.isError
                ? getErrorMessage(s.noteDetailQuery.error, 'Failed to load note detail.')
                : null
            }
            onRetryDetail={() => void s.noteDetailQuery.refetch()}
            canManage={s.canManagePersonNotes}
            isSubmitting={s.createNoteMutation.isPending}
            actionErrorMessage={
              s.noteMutationError
                ? getErrorMessage(s.noteMutationError, 'Failed to save personnel note.')
                : null
            }
            onSelectNote={s.setSelectedNoteId}
            onCreateNote={async (request) => {
              await s.createNoteMutation.mutateAsync(request)
            }}
          />

          <PersonHistorySummaryPanel
            personDisplayName={selectedPerson.displayName}
            summary={s.personHistorySummaryQuery.data ?? null}
            isLoading={s.personHistorySummaryQuery.isLoading}
            isError={s.personHistorySummaryQuery.isError}
            readErrorMessage={
              s.personHistorySummaryQuery.isError
                ? getErrorMessage(s.personHistorySummaryQuery.error, 'Failed to load history summary.')
                : null
            }
            onRetryRead={() => void s.personHistorySummaryQuery.refetch()}
          />

          <PersonTimelinePanel
            personDisplayName={selectedPerson.displayName}
            entries={s.personTimelineEntries}
            totalCount={s.personTimelineTotalCount}
            page={s.personTimelinePage}
            pageSize={s.personTimelinePageSize}
            hasNextPage={s.personTimelineHasNextPage}
            categoryFilter={s.personTimelineCategoryFilter}
            isLoading={s.personTimelineQuery.isLoading}
            isError={s.personTimelineQuery.isError}
            readErrorMessage={
              s.personTimelineQuery.isError
                ? getErrorMessage(s.personTimelineQuery.error, 'Failed to load timeline.')
                : null
            }
            onRetryRead={() => void s.personTimelineQuery.refetch()}
            onCategoryFilterChange={s.setPersonTimelineCategoryFilter}
            onPageChange={s.setPersonTimelinePage}
            onPageSizeChange={s.setPersonTimelinePageSize}
          />
        </>
      )
    }

    const renderMainContent = () => {
      switch (s.peopleDetailTab) {
        case 'permissions':
          return <PermissionsSection state={s} />
        case 'certifications':
          return <CertificationsSection state={s} />
        case 'assignments':
          return renderAssignmentsTab()
        case 'training':
          return renderTrainingTab()
        case 'incidents':
          return <IncidentsSection state={s} />
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
      <ProfileDetailsLayout
        testId="staffarr-person-profile"
        backLabel="People"
        backTo="/people/drawer"
        breadcrumbs={[siteName, displayName]}
        icon={<User className="h-9 w-9" />}
        title={displayName}
        subtitle={(
          <span className="flex flex-wrap items-center gap-2">
            <BriefcaseBusiness className="h-4 w-4 text-slate-400" />
            <span>{jobTitle}</span>
            <span className="text-slate-600">-</span>
            <span>{siteName}</span>
          </span>
        )}
        badges={[
          { label: profile?.externalUserId ?? 'StaffArr profile', tone: 'info' },
          { label: humanize(employmentStatus), tone: employmentStatus === 'active' ? 'good' : 'warn' },
          { label: hasUserAccount ? 'Has user account' : 'No user account', tone: hasUserAccount ? 'good' : 'neutral' },
        ]}
        actions={(
          <>
            <button
              type="button"
              onClick={() => {
                s.setPeopleDetailTab('overview')
                setShowEditor((current) => !current)
                setShowOffboardingShortcut(false)
              }}
              className="inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-3 text-sm font-bold text-slate-950 hover:bg-sky-400"
            >
              <Pencil className="h-4 w-4" />
              Edit person
            </button>
            <button
              type="button"
              onClick={() => void handleAssignTraining()}
              disabled={isLaunchingTraining}
              className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm font-bold text-white hover:bg-slate-800 disabled:opacity-60"
            >
              <GraduationCap className="h-4 w-4" />
              {isLaunchingTraining ? 'Opening TrainArr...' : 'Assign training'}
            </button>
            <button
              type="button"
              onClick={() => {
                s.setPeopleDetailTab('permissions')
                setShowEditor(false)
                setShowOffboardingShortcut(false)
              }}
              className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm font-bold text-white hover:bg-slate-800"
            >
              <ShieldCheck className="h-4 w-4" />
              Manage access
            </button>
            <div className="relative">
              <button
                type="button"
                aria-label="More person actions"
                onClick={() => setShowMoreActions((current) => !current)}
                className="inline-flex h-12 w-12 items-center justify-center rounded-xl border border-slate-700 bg-slate-900 text-slate-300 hover:bg-slate-800 hover:text-white"
              >
                <MoreHorizontal className="h-5 w-5" />
              </button>
              {showMoreActions ? (
                <div className="absolute right-0 z-20 mt-2 w-48 rounded-xl border border-slate-700 bg-slate-950 p-2 shadow-xl">
                  <button
                    type="button"
                    onClick={() => {
                      s.setPeopleDetailTab('overview')
                      setShowOffboardingShortcut(true)
                      setShowEditor(false)
                      setShowMoreActions(false)
                    }}
                    className="block w-full rounded-lg px-3 py-2 text-left text-sm text-slate-200 hover:bg-slate-900"
                  >
                    Offboarding
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      s.setPeopleDetailTab('history')
                      setShowOffboardingShortcut(false)
                      setShowMoreActions(false)
                    }}
                    className="block w-full rounded-lg px-3 py-2 text-left text-sm text-slate-200 hover:bg-slate-900"
                  >
                    Notes
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      s.setPeopleDetailTab('overview')
                      setShowOffboardingShortcut(false)
                      setShowEditor(false)
                      setShowMoreActions(false)
                    }}
                    className="block w-full rounded-lg px-3 py-2 text-left text-sm text-slate-200 hover:bg-slate-900"
                  >
                    Full lookup
                  </button>
                </div>
              ) : null}
            </div>
          </>
        )}
        metrics={[
          {
            label: 'Account state',
            value: hasUserAccount ? 'Linked' : 'Not linked',
            hint: canLogin ? 'Login requested in NexArr' : 'No login requested',
            icon: <KeyRound className="h-5 w-5" />,
            tone: hasUserAccount ? 'good' : 'neutral',
          },
          {
            label: 'Open training',
            value: openTrainingCount,
            hint: openTrainingCount === 1 ? '1 item needs action' : `${openTrainingCount} items need action`,
            icon: <GraduationCap className="h-5 w-5" />,
            tone: openTrainingCount > 0 ? 'warn' : 'good',
          },
          {
            label: 'Active certifications',
            value: activeCertifications.length,
            hint: `${expiringCertifications.length} expire in 60 days`,
            icon: <Award className="h-5 w-5" />,
            tone: expiringCertifications.length > 0 ? 'warn' : 'good',
          },
          {
            label: 'Incidents',
            value: incidents.length,
            hint: incidents.length > 0 ? 'Review personnel history' : 'No active restriction',
            icon: <AlertTriangle className="h-5 w-5" />,
            tone: incidents.length > 0 ? 'warn' : 'good',
          },
        ]}
        tabs={DETAIL_TABS}
        activeTab={s.peopleDetailTab}
        onTabChange={(tabKey) => {
          s.setPeopleDetailTab(tabKey as typeof s.peopleDetailTab)
          setShowOffboardingShortcut(false)
          if (tabKey !== 'overview') {
            setShowEditor(false)
          }
        }}
        snapshotTitle="Person snapshot"
        snapshotSubtitle="Identity, employment placement, login capability, assignments, and source-of-truth references."
        snapshotFields={[
          { label: 'Person ID', value: personId ?? 'Not selected', source: 'NexArr source of truth' },
          { label: 'Display name', value: displayName, source: 'StaffArr person record' },
          { label: 'Legal name', value: profile ? `${profile.legalFirstName} ${profile.legalLastName}`.trim() : 'Not recorded', source: 'StaffArr profile' },
          { label: 'Preferred name', value: profile?.preferredName ?? profile?.givenName ?? lookup?.givenName ?? 'Not recorded', source: 'StaffArr profile' },
          { label: 'Status', value: humanize(employmentStatus), source: 'Employment lifecycle' },
          { label: 'Work relationship', value: humanize(profile?.workRelationshipType), source: 'StaffArr profile' },
          { label: 'Email', value: email, source: 'Login/contact' },
          { label: 'Phone', value: profile?.primaryPhone ?? lookup?.workPhone ?? 'Not recorded', source: 'Contact profile' },
          { label: 'Site', value: siteName, source: 'StaffArr org structure' },
          { label: 'Department', value: departmentName, source: 'StaffArr org structure' },
          { label: 'Position', value: positionName, source: 'StaffArr position catalog' },
          { label: 'Manager', value: managerName, source: 'Reporting line' },
          { label: 'Home base', value: profile?.homeBaseLocationName ?? profile?.homeBaseLocationId ?? 'Not recorded', source: 'Location snapshot' },
          { label: 'Can login', value: canLogin ? 'Requested' : 'No', source: 'NexArr capability snapshot' },
          { label: 'Has account', value: hasUserAccount ? 'Yes' : 'No', source: 'NexArr account snapshot' },
          { label: 'Start date', value: formatDate(profile?.startDate ?? profile?.expectedStartDate), source: 'Personnel record' },
        ]}
        mainContent={renderMainContent()}
        decisionTitle="Authorization decision"
        decisionBadge={{ label: readinessAllowed ? 'Allowed' : 'Blocked', tone: readinessAllowed ? 'good' : 'bad' }}
        decisionIcon={readinessAllowed ? <UserCheck className="h-5 w-5 text-emerald-300" /> : <XCircle className="h-5 w-5 text-red-300" />}
        decisionSummary={readinessAllowed ? 'Can perform assigned work' : 'Restrictions require review'}
        decisionDetail={
          readinessAllowed
            ? 'No active restrictions. Training and role checks allow normal StaffArr work.'
            : readiness?.blockers[0]?.message ?? 'One or more authorization checks are blocked.'
        }
        allowedChecks={allowedChecks}
        blockedChecks={blockedChecks}
        railSections={[
          {
            title: 'Account and identity',
            icon: <IdCard className="h-5 w-5" />,
            content: (
              <div className="space-y-3">
                <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                  <p className="text-sm font-bold text-white">Identity source</p>
                  <p className="mt-1 text-sm text-sky-100/75">StaffArr tracks the person record; NexArr credentials remain authoritative for auth.</p>
                </div>
                <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                  <p className="text-sm font-bold text-white">Login capability</p>
                  <p className="mt-1 text-sm text-sky-100/75">
                    canLoginSnapshot = {canLogin ? 'true' : 'false'}; hasUserAccountSnapshot = {hasUserAccount ? 'true' : 'false'}.
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
            ),
          },
          {
            title: 'Upcoming requirements',
            icon: <CalendarClock className="h-5 w-5" />,
            content: (
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
            ),
          },
          {
            title: 'Documents',
            icon: <FileText className="h-5 w-5" />,
            content: (
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
            ),
          },
          {
            title: 'Recent activity',
            icon: <CalendarClock className="h-5 w-5" />,
            content: (
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
            ),
          },
        ]}
      />
    )
  }

  return (
    <>
      {mode === 'create' ? (
        <div className="rounded-xl border border-teal-700/50 bg-teal-950/20 p-4 text-sm text-teal-100">
          <p>Create people in a guided flow using business-aligned identity, placement, login intent, and initial access fields.</p>
          <ol className="mt-2 list-decimal space-y-1 pl-5">
            <li>Step 1: Capture the canonical identity fields used across StaffArr and NexArr.</li>
            <li>Step 2: Set contact, lifecycle status, and work relationship details.</li>
            <li>Step 3: Seed placement and optional role assignments in one creation flow.</li>
          </ol>
        </div>
      ) : null}

      {renderDirectorySection()}

      {mode === 'create' ? (
        <CreatePersonPanel
          accessToken={s.accessToken}
          orgUnits={s.orgUnits}
          peopleOptions={s.people.map((person) => ({
            personId: person.personId,
            displayName: person.displayName,
          }))}
          roleTemplates={s.roleTemplates}
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
