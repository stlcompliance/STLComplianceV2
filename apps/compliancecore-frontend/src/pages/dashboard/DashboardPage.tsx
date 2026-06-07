import { createWorkspacePage } from '../../lib/createWorkspacePage'
import { ComplianceCoreWorkspacePage } from '../../workspace/ComplianceCoreWorkspacePage'

export const DashboardPage = createWorkspacePage(ComplianceCoreWorkspacePage, 'dashboard')
