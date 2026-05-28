import { PmDuePanel } from '../../components/PmDuePanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function OverviewSection({ state }: Props) {
  return (
    <div className="mb-8">
      <PmDuePanel dueSchedules={state.duePmQuery.data ?? []} isLoading={state.duePmQuery.isLoading} />
    </div>
  )
}
