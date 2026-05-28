import { AuditPackageExportPanel } from '../../components/AuditPackageExportPanel'
import { CsvImportExportPanel } from '../../components/CsvImportExportPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function AdminSection({ state }: Props) {
  const s = state
  return (
    <>
      <CsvImportExportPanel accessToken={s.accessToken} canManage={s.canManage} />
      <AuditPackageExportPanel accessToken={s.accessToken} canExport={s.canExportAudit} />
    </>
  )
}
