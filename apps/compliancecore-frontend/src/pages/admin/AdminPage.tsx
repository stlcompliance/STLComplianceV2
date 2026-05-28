import { createWorkspacePage } from '../../lib/createWorkspacePage'
import { ComplianceCoreWorkspacePage } from '../../workspace/ComplianceCoreWorkspacePage'

export const AdminPage = createWorkspacePage(ComplianceCoreWorkspacePage, 'admin')
