import { Suspense, lazy, type ComponentType } from 'react'
import type { WorkspaceSection } from '../lib/workspaceSection'
import { useStaffArrWorkspaceState } from './useStaffArrWorkspaceState'
import { WorkspaceShell } from './WorkspaceShell'

const PeopleSection = lazy(() =>
  import('./sections/PeopleSection').then((module) => ({ default: module.PeopleSection })),
)
const OrgSection = lazy(() =>
  import('./sections/OrgSection').then((module) => ({ default: module.OrgSection })),
)
const OrganizationStructureSection = lazy(() =>
  import('./sections/OrganizationStructureSection').then((module) => ({
    default: module.OrganizationStructureSection,
  })),
)
const LocationsSection = lazy(() =>
  import('./sections/LocationsSection').then((module) => ({ default: module.LocationsSection })),
)
const PermissionsSection = lazy(() =>
  import('./sections/PermissionsSection').then((module) => ({ default: module.PermissionsSection })),
)
const ReadinessSection = lazy(() =>
  import('./sections/ReadinessSection').then((module) => ({ default: module.ReadinessSection })),
)
const IncidentsSection = lazy(() =>
  import('./sections/IncidentsSection').then((module) => ({ default: module.IncidentsSection })),
)
const RestrictionsSection = lazy(() =>
  import('./sections/RestrictionsSection').then((module) => ({ default: module.RestrictionsSection })),
)
const TrainingAcknowledgementsSection = lazy(() =>
  import('./sections/TrainingAcknowledgementsSection').then((module) => ({
    default: module.TrainingAcknowledgementsSection,
  })),
)
const ReportsSection = lazy(() =>
  import('./sections/ReportsSection').then((module) => ({ default: module.ReportsSection })),
)
const CertificationsSection = lazy(() =>
  import('./sections/CertificationsSection').then((module) => ({ default: module.CertificationsSection })),
)
const AdminSection = lazy(() =>
  import('./sections/AdminSection').then((module) => ({ default: module.AdminSection })),
)
const EmploymentApplicationsSection = lazy(() =>
  import('../pages/employment-applications/EmploymentApplicationsPage').then((module) => ({
    default: module.EmploymentApplicationsPage,
  })),
)

const sectionComponents: Record<WorkspaceSection, ComponentType<{ state: ReturnType<typeof useStaffArrWorkspaceState> }>> = {
  people: PeopleSection,
  org: OrgSection,
  'organization-structure': OrganizationStructureSection,
  locations: LocationsSection,
  permissions: PermissionsSection,
  readiness: ReadinessSection,
  incidents: IncidentsSection,
  restrictions: RestrictionsSection,
  'training-acknowledgements': TrainingAcknowledgementsSection,
  reports: ReportsSection,
  certifications: CertificationsSection,
  'employment-applications': EmploymentApplicationsSection,
  admin: AdminSection,
}

const sectionLabels: Record<WorkspaceSection, string> = {
  people: 'People',
  org: 'Org',
  'organization-structure': 'Organization Structure',
  locations: 'Locations',
  permissions: 'Permissions',
  readiness: 'Readiness',
  incidents: 'Incidents',
  restrictions: 'Restrictions',
  'training-acknowledgements': 'Training Acknowledgements',
  reports: 'Reports',
  certifications: 'Certifications',
  'employment-applications': 'Employment Applications',
  admin: 'Admin',
}

export function StaffArrWorkspacePage({ section }: { section: WorkspaceSection }) {
  const state = useStaffArrWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>
  const SectionComponent = sectionComponents[section]
  const sectionLabel = sectionLabels[section]

  return (
    <WorkspaceShell section={section} me={state.me} apiError={state.apiError}>
      <Suspense fallback={<p className="text-sm text-slate-400">Loading {sectionLabel}…</p>}>
        <SectionComponent state={state} />
      </Suspense>
    </WorkspaceShell>
  )
}
