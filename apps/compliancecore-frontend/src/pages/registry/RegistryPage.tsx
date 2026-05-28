import { createWorkspacePage } from '../../lib/createWorkspacePage'
import { ComplianceCoreWorkspacePage } from '../../workspace/ComplianceCoreWorkspacePage'

export const RegistryPage = createWorkspacePage(ComplianceCoreWorkspacePage, 'registry')
