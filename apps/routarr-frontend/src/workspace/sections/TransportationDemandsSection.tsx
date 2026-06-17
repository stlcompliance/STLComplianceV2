import { TransportationDemandsPanel } from '../../components/TransportationDemandsPanel'
import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'

type Props = { state: RoutArrWorkspaceState }

export function TransportationDemandsSection({ state }: Props) {
  return <TransportationDemandsPanel accessToken={state.session.accessToken} />
}
