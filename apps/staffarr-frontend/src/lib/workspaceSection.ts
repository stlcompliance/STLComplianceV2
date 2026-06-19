export const WORKSPACE_SECTIONS = [
  'people',
  'org',
  'locations',
  'permissions',
  'readiness',
  'incidents',
  'restrictions',
  'training-acknowledgements',
  'reports',
  'certifications',
  'admin',
  'organization-structure',
  'employment-applications',
] as const

export type WorkspaceSection = (typeof WORKSPACE_SECTIONS)[number]

export const DEFAULT_WORKSPACE_SECTION: WorkspaceSection = 'people'

export function workspaceSectionFromPathname(pathname: string): WorkspaceSection {
  const segment = pathname.split('/').filter(Boolean)[0]
  if (segment && WORKSPACE_SECTIONS.includes(segment as WorkspaceSection)) {
    return segment as WorkspaceSection
  }
  return DEFAULT_WORKSPACE_SECTION
}

export const workspaceSectionHeaders: Record<
  WorkspaceSection,
  { title: string; subtitle: string }
> = {
  people: {
    title: 'People directory',
    subtitle: 'Profiles, notes, documents, and person timeline',
  },
  org: {
    title: 'Org structure',
    subtitle: 'Units, assignments, and manager hierarchy',
  },
  locations: {
    title: 'Locations',
    subtitle: 'Sites and internal operational location references',
  },
  permissions: {
    title: 'Permissions',
    subtitle: 'Role templates, assignments, and projection history',
  },
  readiness: {
    title: 'Readiness',
    subtitle: 'Rollups and person readiness overrides',
  },
  incidents: {
    title: 'Incidents',
    subtitle: 'Personnel incidents and TrainArr routing',
  },
  restrictions: {
    title: 'Restrictions',
    subtitle: 'Active readiness restrictions and blockers',
  },
  'training-acknowledgements': {
    title: 'Training acknowledgements',
    subtitle: 'Pending TrainArr assignment receipts for your person record',
  },
  reports: { title: 'Reports', subtitle: 'Personnel, readiness, certification, incident, and audit dashboards' },
  certifications: {
    title: 'Certifications',
    subtitle: 'TrainArr-published qualification and certification status',
  },
  admin: {
    title: 'Admin',
    subtitle: 'Bulk import, exports, and worker operations',
  },
  'organization-structure': {
    title: 'Organization structure',
    subtitle: 'Canonical org units and internal locations',
  },
  'employment-applications': {
    title: 'Employment applications',
    subtitle: 'Builder, public intake link, and applicant submissions',
  },
}
