import { IdentityProfilePanel } from './IdentityProfilePanel'
import { SessionManagementPanel } from './SessionManagementPanel'

export function IdentityAccessPanel() {
  return (
    <div className="max-w-3xl space-y-10">
      <IdentityProfilePanel />
      <SessionManagementPanel />
    </div>
  )
}
