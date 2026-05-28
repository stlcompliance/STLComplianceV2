import { PageHeader } from '@stl/shared-ui'
import type { ReactNode } from 'react'
import { workspaceSectionHeaders } from '../lib/workspaceSection'
import type { WorkspaceSection } from '../lib/workspaceSection'
import type { StaffArrMeResponse } from '../api/types'

type Props = {
  section: WorkspaceSection
  me: StaffArrMeResponse
  apiError: string | null
  children: ReactNode
}

export function WorkspaceShell({ section, me, apiError, children }: Props) {
  const sectionHeader = workspaceSectionHeaders[section]
  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <PageHeader
        title={sectionHeader.title}
        subtitle={`${sectionHeader.subtitle} · ${me.displayName} (${me.tenantRoleKey})`}
      />
      {apiError ? (
        <p className="mb-4 rounded-lg border border-red-800 bg-red-950/40 p-3 text-sm text-red-200">
          {apiError}
        </p>
      ) : null}
      {children}
    </div>
  )
}
