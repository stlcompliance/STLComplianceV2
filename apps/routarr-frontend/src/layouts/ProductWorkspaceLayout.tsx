import { ProductAppShell } from '@stl/shared-ui'
import { Outlet } from 'react-router-dom'

export function ProductWorkspaceLayout() {
  return (
    <ProductAppShell
      productName="RoutArr"
      workspaceSubtitle="Dispatch, routes, and trips"
    >
      <Outlet />
    </ProductAppShell>
  )
}
