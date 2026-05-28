import { useSearchParams } from 'react-router-dom'
import { TrainingAcknowledgementsPanel } from '../../components/TrainingAcknowledgementsPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function TrainingAcknowledgementsSection({ state }: Props) {
  const [searchParams] = useSearchParams()
  const highlightAcknowledgementId = searchParams.get('acknowledgementId')

  return (
    <TrainingAcknowledgementsPanel
      accessToken={state.accessToken}
      personId={state.me.personId}
      displayName={state.me.displayName}
      highlightAcknowledgementId={highlightAcknowledgementId}
    />
  )
}
