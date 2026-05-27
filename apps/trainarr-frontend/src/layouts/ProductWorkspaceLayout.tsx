import { ProductAppShell } from '@stl/shared-ui'
import { Outlet } from 'react-router-dom'

export function ProductWorkspaceLayout() {
  return (
    <ProductAppShell
      productName="TrainArr"
      workspaceSubtitle="Training, evidence, and qualifications"
    >
      <Outlet />
    </ProductAppShell>
  )
}
