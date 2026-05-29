export const WORKSPACE_SECTIONS = [
  'dispatch',
  'driver-portal',
  'trips',
  'routes',
  'availability',
  'calendar',
  'reports',
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
  dispatch: {
    title: 'Dispatch command center',
    subtitle: 'Command center, unassigned queue, exceptions, active trips',
  },
  'driver-portal': {
    title: 'Driver portal',
    subtitle: "Today's assignments, upcoming trips, and execution actions",
  },
  trips: { title: 'Trips', subtitle: 'Trip execution and status' },
  routes: { title: 'Routes', subtitle: 'Route definitions and planning' },
  availability: {
    title: 'Availability',
    subtitle: 'Driver and equipment availability',
  },
  calendar: { title: 'Route calendar', subtitle: 'Scheduled routes and capacity' },
  reports: {
    title: 'Dispatch reports',
    subtitle: 'Trip, exception, and delay rollups with CSV export',
  },
  settings: { title: 'Settings', subtitle: 'Notifications, integration events, and capture policy' },
}
