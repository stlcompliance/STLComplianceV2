export const WORKSPACE_SECTIONS = [
  'programs',
  'assignments',
  'remediation',
  'citations',
  'rule-packs',
  'qualifications',
  'reports',
  'settings',
] as const

export type WorkspaceSection = (typeof WORKSPACE_SECTIONS)[number]

export const DEFAULT_WORKSPACE_SECTION: WorkspaceSection = 'programs'

export function workspaceSectionFromPathname(pathname: string): WorkspaceSection {
  const segment = pathname.split('/').filter(Boolean)[0]
  if (segment === 'assignments' && pathname.includes('/evidence')) {
    return 'assignments'
  }
  if (segment && WORKSPACE_SECTIONS.includes(segment as WorkspaceSection)) {
    return segment as WorkspaceSection
  }
  return DEFAULT_WORKSPACE_SECTION
}

export const workspaceSectionHeaders: Record<
  WorkspaceSection,
  { title: string; subtitle: string }
> = {
  programs: { title: 'Training programs', subtitle: 'Program builder and content structure' },
  assignments: { title: 'Assignments', subtitle: 'Assign, evidence, and signoff workflow' },
  remediation: { title: 'Remediation', subtitle: 'Remediation assignments and follow-up' },
  citations: { title: 'Citations', subtitle: 'Citation attachments and regulatory links' },
  'rule-packs': { title: 'Rule packs', subtitle: 'Requirements, impact, and compliance mapping' },
  qualifications: {
    title: 'Qualifications',
    subtitle: 'Batch qualification checks and publishing',
  },
  reports: { title: 'Reports', subtitle: 'Assignment, qualification, and compliance rollups' },
  settings: { title: 'Settings', subtitle: 'Notification preferences' },
}
