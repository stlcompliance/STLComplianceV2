import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { DriverPortalPanel } from '../../components/DriverPortalPanel'

type Props = { state: RoutArrWorkspaceState }

export function DriverPortalSection({ state }: Props) {
  return <DriverPortalPanel accessToken={state.session.accessToken} />
}
