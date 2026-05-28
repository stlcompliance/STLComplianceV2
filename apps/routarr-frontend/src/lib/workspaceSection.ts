export const WORKSPACE_SECTIONS = [
  'dispatch',
  'trips',
  'routes',
  'availability',
  'calendar',
  'settings',
] as const

export type WorkspaceSection = (typeof WORKSPACE_SECTIONS)[number]

export const DEFAULT_WORKSPACE_SECTION: WorkspaceSection = 'dispatch'

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
  dispatch: { title: 'Dispatch board', subtitle: 'Assignments, closeout, and bulk dispatch' },
  trips: { title: 'Trips', subtitle: 'Trip execution and status' },
  routes: { title: 'Routes', subtitle: 'Route definitions and planning' },
  availability: {
    title: 'Availability',
    subtitle: 'Driver and equipment availability',
  },
  calendar: { title: 'Route calendar', subtitle: 'Scheduled routes and capacity' },
  settings: { title: 'Settings', subtitle: 'Notification preferences' },
}
