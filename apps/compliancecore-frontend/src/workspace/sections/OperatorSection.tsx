import { OperatorDashboardPanel } from '../../components/OperatorDashboardPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function OperatorSection({ state }: Props) {
  const s = state
  return <OperatorDashboardPanel accessToken={s.accessToken} />
}
