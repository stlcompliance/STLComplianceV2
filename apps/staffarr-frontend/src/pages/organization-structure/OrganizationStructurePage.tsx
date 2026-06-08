import { createWorkspacePage } from '../../lib/createWorkspacePage'
import { StaffArrWorkspacePage } from '../../workspace/StaffArrWorkspacePage'

export const OrganizationStructurePage = createWorkspacePage(
  StaffArrWorkspacePage,
  'organization-structure',
)
