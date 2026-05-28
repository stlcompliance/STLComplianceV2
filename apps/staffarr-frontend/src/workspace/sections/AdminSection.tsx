import { AuditPackageExportPanel } from '../../components/AuditPackageExportPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function AdminSection({ state }: Props) {
  const s = state
  return (
    <AuditPackageExportPanel accessToken={s.accessToken} canExport={s.canExportAudit} />
  )
}
