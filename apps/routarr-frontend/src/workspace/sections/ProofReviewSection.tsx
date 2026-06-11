import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canExportDispatchReports, canReadDispatchReports } from '../../auth/sessionStorage'
import { ProofDvirReportsPanel } from '../../components/ProofDvirReportsPanel'
import { TripProofDvirReadPanel } from '../../components/TripProofDvirReadPanel'

type Props = { state: RoutArrWorkspaceState }

export function ProofReviewSection({ state }: Props) {
  const { session, roleKey, isPlatformAdmin } = state
  const canRead = canReadDispatchReports(roleKey, isPlatformAdmin)
  const canExport = canExportDispatchReports(roleKey, isPlatformAdmin)

  return (
    <>
      <div className="mt-8">
        <TripProofDvirReadPanel accessToken={session.accessToken} />
      </div>
      <div className="mt-8">
        <ProofDvirReportsPanel
          accessToken={session.accessToken}
          canRead={canRead}
          canExport={canExport}
        />
      </div>
    </>
  )
}
