import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function OperatorSection({ state }: Props) {
  const {
    accessToken,
    session,
  } = state
  return (
    <>
      <OperatorDashboardPanel accessToken={session!.accessToken} />
    </>
  )
}
