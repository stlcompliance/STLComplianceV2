import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { RouteCalendarPanel } from '../../components/RouteCalendarPanel'

type Props = { state: RoutArrWorkspaceState }

export function CalendarSection({ state }: Props) {
  const { session, boardScope, setBoardScope } = state

  return (
    <div className="mt-8">
      <RouteCalendarPanel
        accessToken={session.accessToken}
        scope={boardScope}
        onScopeChange={setBoardScope}
      />
    </div>
  )
}
