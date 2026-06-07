import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { LoadVisibilityPanel } from '../../components/LoadVisibilityPanel'
import { TripPartsDemandPanel } from '../../components/TripPartsDemandPanel'

type Props = { state: RoutArrWorkspaceState }

export function LoadVisibilitySection({ state }: Props) {
  return (
    <>
      <LoadVisibilityPanel accessToken={state.session.accessToken} />

      <div className="mt-8">
        <TripPartsDemandPanel accessToken={state.session.accessToken} />
      </div>
    </>
  )
}
