import type { ComponentType } from 'react'
import type { WorkspaceSection } from './workspaceSection'

export function createWorkspacePage(
  Workspace: ComponentType<{ section: WorkspaceSection }>,
  section: WorkspaceSection,
) {
  function WorkspaceRoutePage() {
    return <Workspace section={section} />
  }
  WorkspaceRoutePage.displayName = `MaintainArr${section}Page`
  return WorkspaceRoutePage
}
