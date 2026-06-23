import { Bell } from 'lucide-react'
import { PageHeader } from '@stl/shared-ui'

import { NotificationSettingsPanel } from '../components/NotificationSettingsPanel'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'

export function NotificationsPage() {
  const { session, meQuery, accessToken } = useFieldCompanionWorkspace()

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading notification settings…</p>
  }

  const canManage = meQuery.data.isPlatformAdmin || meQuery.data.tenantRoleKey === 'tenant_admin'

  return (
    <div className="mx-auto max-w-4xl space-y-5">
      <PageHeader
        title="Notifications"
        subtitle="Operational alerts, push readiness, and webhook routing for Field Companion events."
      />

      <section className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5">
        <div className="flex items-center gap-2">
          <Bell className="h-5 w-5 text-teal-300" aria-hidden />
          <h2 className="text-lg font-semibold text-white">Operational delivery</h2>
        </div>
        <p className="mt-2 text-sm text-slate-400">
          These settings control Field Companion notifications. The linked task or inbox record remains available in the destination workflow.
        </p>
        <div className="mt-4">
          <NotificationSettingsPanel accessToken={accessToken} canManage={canManage} />
        </div>
      </section>
    </div>
  )
}
