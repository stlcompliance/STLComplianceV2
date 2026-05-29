import { ServiceTokenCleanupSettingsPanel } from '../../components/platform-admin/ServiceTokenCleanupSettingsPanel'
import { ServiceTokenAdminPanel } from '../../components/platform-admin/ServiceTokenAdminPanel'

export function ServiceTokenCleanupPage() {
  return (
    <div className="space-y-6">
      <ServiceTokenAdminPanel />
      <ServiceTokenCleanupSettingsPanel />
    </div>
  )
}
