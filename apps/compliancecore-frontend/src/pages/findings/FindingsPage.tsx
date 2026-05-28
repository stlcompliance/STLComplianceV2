import { createWorkspacePage } from '../../lib/createWorkspacePage'
import { ComplianceCoreWorkspacePage } from '../../workspace/ComplianceCoreWorkspacePage'

export const FindingsPage = createWorkspacePage(ComplianceCoreWorkspacePage, 'findings')
