export const WORKSPACE_SECTIONS = [
  'people',
  'org',
  'permissions',
  'readiness',
  'incidents',
  'certifications',
  'admin',
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
  certifications: {
    title: 'Certifications',
    subtitle: 'Definitions and person certifications',
  },
  admin: {
    title: 'Admin',
    subtitle: 'Bulk import, export, and audit packages',
  },
}
