import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { TripProofDvirReadPanel } from '../../components/TripProofDvirReadPanel'

type Props = { state: RoutArrWorkspaceState }

export function ProofReviewSection({ state }: Props) {
  const { session } = state

  return (
    <div className="mt-8">
      <TripProofDvirReadPanel accessToken={session.accessToken} />
    </div>
  )
}
