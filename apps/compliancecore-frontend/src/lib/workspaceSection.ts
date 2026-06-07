export const WORKSPACE_SECTIONS = [
  'dashboard',
  'registry',
  'mappings',
  'findings',
  'evaluation',
  'fact-sources',
  'reports',
  'operator',
  'admin',
] as const

export type WorkspaceSection = (typeof WORKSPACE_SECTIONS)[number]

export const DEFAULT_WORKSPACE_SECTION: WorkspaceSection = 'dashboard'

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
  dashboard: {
    title: 'Compliance dashboard',
    subtitle: 'Rulepack health and evidence posture',
  },
  registry: {
    title: 'Compliance registry',
    subtitle: 'Vocabulary and regulatory registry',
  },
  mappings: {
    title: 'Mappings',
    subtitle: 'Regulatory mappings and citation catalog',
  },
  findings: { title: 'Findings', subtitle: 'Findings workflow gates' },
  evaluation: { title: 'Rule evaluation', subtitle: 'Evaluate rules against facts' },
  'fact-sources': { title: 'Fact sources', subtitle: 'Fact source configuration' },
  reports: { title: 'Compliance reports', subtitle: 'Operational compliance reporting' },
  operator: { title: 'Operator dashboard', subtitle: 'Operational compliance overview' },
  admin: { title: 'Admin', subtitle: 'Import, export, and audit packages' },
}
