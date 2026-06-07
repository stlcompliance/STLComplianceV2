import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { DockAppointmentsPanel } from '../../components/DockAppointmentsPanel'

type Props = { state: RoutArrWorkspaceState }

export function DockAppointmentsSection({ state }: Props) {
  return (
    <DockAppointmentsPanel accessToken={state.session.accessToken} />
  )
}
