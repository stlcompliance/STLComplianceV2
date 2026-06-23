import { PageHeader } from '@stl/shared-ui'
import { useStaffArrWorkspaceState } from '../../workspace/useStaffArrWorkspaceState'
import { ReportsSection } from '../../workspace/sections/ReportsSection'

export function ReportsPage() {
  const state = useStaffArrWorkspaceState()

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-[var(--color-text-muted)]">{state.loadingMessage}</p>

  return (
    <div className="mx-auto max-w-6xl space-y-6 px-4 py-6">
      <PageHeader
        title="Reports"
        subtitle="Personnel, readiness, incident, certification, and audit package summaries"
      />
      <ReportsSection state={state} />
    </div>
  )
}
