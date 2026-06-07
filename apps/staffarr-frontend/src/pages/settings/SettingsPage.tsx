import { PageHeader } from '@stl/shared-ui'
import { useStaffArrWorkspaceState } from '../../workspace/useStaffArrWorkspaceState'
import { AdminSection } from '../../workspace/sections/AdminSection'

export function SettingsPage() {
  const state = useStaffArrWorkspaceState()

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="mx-auto max-w-6xl space-y-6 px-4 py-6">
      <PageHeader
        title="Settings"
        subtitle="Worker controls, export delivery, and operational admin settings"
      />
      <AdminSection state={state} />
    </div>
  )
}
