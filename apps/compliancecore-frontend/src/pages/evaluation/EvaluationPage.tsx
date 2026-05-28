import { createWorkspacePage } from '../../lib/createWorkspacePage'
import { ComplianceCoreWorkspacePage } from '../../workspace/ComplianceCoreWorkspacePage'

export const EvaluationPage = createWorkspacePage(ComplianceCoreWorkspacePage, 'evaluation')
