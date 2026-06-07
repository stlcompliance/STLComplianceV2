export const WORKSPACE_SECTIONS = [
  'dashboard',
  'dispatch',
  'dispatch-plans',
  'route-planner',
  'driver-portal',
  'trips',
  'routes',
  'stops',
  'exceptions',
  'proof-review',
  'dock-appointments',
  'load-visibility',
  'validation-blockers',
  'availability',
  'calendar',
  'settings',
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
    title: 'Dispatch dashboard',
    subtitle: 'Current dispatch health, blockers, execution, and next actions',
  },
  dispatch: {
    title: 'Dispatch command center',
    subtitle: 'Command center, unassigned queue, exceptions, active trips',
  },
  'dispatch-plans': {
    title: 'Dispatch plans',
    subtitle: 'Plan release, assignment readiness, and closeout',
  },
  'route-planner': {
    title: 'Route planner',
    subtitle: 'Route drafting, stop sequencing, and trip linkage',
  },
  'driver-portal': {
    title: 'Driver portal',
    subtitle: "Today's assignments, upcoming trips, and execution actions",
  },
  trips: { title: 'Trips', subtitle: 'Trip execution and status' },
  routes: { title: 'Routes', subtitle: 'Route definitions and planning' },
  stops: { title: 'Stops', subtitle: 'Stop progress, requirements, and proof readiness' },
  exceptions: { title: 'Exceptions', subtitle: 'Route and trip exceptions requiring triage' },
  'proof-review': { title: 'Proof review', subtitle: 'Pickup and delivery proof with DVIR review' },
  'dock-appointments': {
    title: 'Dock appointments',
    subtitle: 'Inbound appointment visibility, ETAs, and handoff timing',
  },
  'load-visibility': {
    title: 'Load visibility',
    subtitle: 'Transportation load snapshots and trip-linked load context',
  },
  'validation-blockers': {
    title: 'Validation and blockers',
    subtitle: 'Readiness checks, assignment gates, and release blockers',
  },
  availability: {
    title: 'Availability',
    subtitle: 'Driver and equipment availability',
  },
  calendar: { title: 'Route calendar', subtitle: 'Scheduled routes and capacity' },
  settings: { title: 'Settings', subtitle: 'Notifications, integration events, and capture policy' },
}
