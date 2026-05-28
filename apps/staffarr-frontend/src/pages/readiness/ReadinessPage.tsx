import { createWorkspacePage } from '../../lib/createWorkspacePage'
import { StaffArrWorkspacePage } from '../../workspace/StaffArrWorkspacePage'

export const ReadinessPage = createWorkspacePage(StaffArrWorkspacePage, 'readiness')
