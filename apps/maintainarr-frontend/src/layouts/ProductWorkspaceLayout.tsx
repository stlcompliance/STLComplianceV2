import { ProductAppShell } from '@stl/shared-ui'
import { Outlet } from 'react-router-dom'

export function ProductWorkspaceLayout() {
  return (
    <ProductAppShell
      productName="MaintainArr"
      workspaceSubtitle="Assets, inspections, and work orders"
    >
      <Outlet />
    </ProductAppShell>
  )
}
