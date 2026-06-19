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
    title: 'Compliance Core overview',
    subtitle: 'Rulepack health, mapping readiness, and evaluation attention',
  },
  registry: {
    title: 'Regulatory Registry',
    subtitle: 'Governing bodies, citations, vocabulary, evidence requirements, and retained rule sources',
  },
  mappings: {
    title: 'Mapping Center',
    subtitle: 'Where product data, evidence, vocabulary, subjects, and output signals connect to rule logic',
  },
  findings: { title: 'Review Queue', subtitle: 'Unknown facts, conflicts, unmapped evidence, and rulepack items that need human attention' },
  evaluation: { title: 'Evaluations', subtitle: 'Recent runs, situation tests, calculation traces, and result explanations' },
  'fact-sources': { title: 'Fact sources', subtitle: 'Fact source configuration' },
  reports: { title: 'Compliance reports', subtitle: 'Operational compliance reporting' },
  operator: { title: 'Operator console', subtitle: 'Operational compliance overview' },
  admin: { title: 'Settings', subtitle: 'Imports, exports, audit packages, scheduled jobs, and product configuration' },
}
