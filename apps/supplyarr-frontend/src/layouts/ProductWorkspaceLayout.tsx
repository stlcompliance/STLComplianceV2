import { ProductAppShell } from '@stl/shared-ui'
import { Outlet } from 'react-router-dom'

export function ProductWorkspaceLayout() {
  return (
    <ProductAppShell
      productName="SupplyArr"
      workspaceSubtitle="Vendors, procurement, and inventory"
    >
      <Outlet />
    </ProductAppShell>
  )
}
