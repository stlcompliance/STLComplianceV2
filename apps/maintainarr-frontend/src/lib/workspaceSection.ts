export const WORKSPACE_SECTIONS = [
  'overview',
  'assets',
  'pm-programs',
  'meters',
  'work-orders',
  'defects',
  'inspections',
  'inspection-templates',
  'parts-kits',
  'history',
  'downtime',
  'settings',
] as const

export type WorkspaceSection = (typeof WORKSPACE_SECTIONS)[number]

export const DEFAULT_WORKSPACE_SECTION: WorkspaceSection = 'overview'

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
  overview: {
    title: 'Maintenance readiness',
    subtitle: 'Asset readiness, active work, defects, PM risk, and execution blockers',
  },
  assets: {
    title: 'Asset registry',
    subtitle: 'Classes, types, assets, and readiness',
  },
  'pm-programs': {
    title: 'PM programs',
    subtitle: 'Program scope and schedule assignment',
  },
  meters: {
    title: 'Meters and readings',
    subtitle: 'Meter definitions, readings, and PM forecast',
  },
  'work-orders': {
    title: 'Work orders',
    subtitle: 'Create, assign, execute, and close work',
  },
  defects: {
    title: 'Defects',
    subtitle: 'Report, triage, and link defects to work',
  },
  inspections: {
    title: 'Inspection runs',
    subtitle: 'Execute checklists and complete runs',
  },
  'inspection-templates': {
    title: 'Inspection templates',
    subtitle: 'Build and activate inspection checklists',
  },
  'parts-kits': {
    title: 'Parts kits',
    subtitle: 'Reusable maintenance parts kits and line items',
  },
  history: {
    title: 'Maintenance history',
    subtitle: 'Asset maintenance timeline',
  },
  downtime: {
    title: 'Downtime',
    subtitle: 'Availability metrics and downtime events',
  },
  settings: {
    title: 'Workspace settings',
    subtitle: 'Bulk import, notifications, and audit exports',
  },
}
