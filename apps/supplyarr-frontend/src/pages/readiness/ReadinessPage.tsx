import { createWorkspacePage } from '../../lib/createWorkspacePage'
import { SupplyArrWorkspacePage } from '../../workspace/SupplyArrWorkspacePage'

export const ReadinessPage = createWorkspacePage(SupplyArrWorkspacePage, 'dashboard')
