import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function AdminSection({ state }: Props) {
  const {
    accessToken,
    canExportAudit,
    session,
  } = state
  return (
    <>
      <>
              <CsvImportExportPanel accessToken={session!.accessToken} canManage={canManage} />
              <AuditPackageExportPanel accessToken={session!.accessToken} canExport={canExportAudit} />
              </>
    </>
  )
}
