import { createWorkspacePage } from '../../lib/createWorkspacePage'
import { ComplianceCoreWorkspacePage } from '../../workspace/ComplianceCoreWorkspacePage'

export const MappingsPage = createWorkspacePage(ComplianceCoreWorkspacePage, 'mappings')
