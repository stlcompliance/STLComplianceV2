export const WORKSPACE_SECTIONS = [
  'my-training',
  'dashboard',
  'catalog',
  'programs',
  'assignments',
  'instructor',
  'evaluator',
  'remediation',
  'citations',
  'rule-packs',
  'matrix',
  'certificates',
  'qualifications',
  'reports',
  'settings',
] as const

export type WorkspaceSection = (typeof WORKSPACE_SECTIONS)[number]

export const DEFAULT_WORKSPACE_SECTION: WorkspaceSection = 'my-training'

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
  'my-training': { title: 'My Training', subtitle: 'Learner dashboard, due items, and active credentials' },
  dashboard: { title: 'My Training', subtitle: 'Learner dashboard, due items, and active credentials' },
  catalog: { title: 'Training Catalog', subtitle: 'Browse, build, and version courses and learning paths' },
  programs: { title: 'Training Catalog', subtitle: 'Browse, build, and version courses and learning paths' },
  assignments: { title: 'Course Player', subtitle: 'Assigned learning, evidence, and signoff workflow' },
  instructor: { title: 'Instructor Console', subtitle: 'Rosters, attendance, and classroom completion' },
  evaluator: { title: 'Evaluator Console', subtitle: 'Practical evaluations, scoring, and remediation' },
  remediation: { title: 'Remediation', subtitle: 'Remediation assignments and follow-up' },
  citations: { title: 'Content Library', subtitle: 'Course content references and regulatory links' },
  'rule-packs': { title: 'Compliance Mapping', subtitle: 'Requirements, impact, and compliance mapping' },
  matrix: { title: 'Training Matrix', subtitle: 'Role, site, equipment, and task requirement coverage' },
  certificates: { title: 'Certificate Registry', subtitle: 'Active credentials, expirations, and revocations' },
  qualifications: { title: 'Certificate Registry', subtitle: 'Active credentials, expirations, and revocations' },
  reports: { title: 'Reports', subtitle: 'Completion, readiness, and certificate analytics' },
  settings: { title: 'Settings', subtitle: 'Notification, certificate, and assignment preferences' },
}
