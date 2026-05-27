import { ProductAppShell } from '@stl/shared-ui'
import { Outlet } from 'react-router-dom'

export function ProductWorkspaceLayout() {
  return (
    <ProductAppShell
      productName="Companion"
      workspaceSubtitle="Field inbox and mobile tasks"
    >
      <Outlet />
    </ProductAppShell>
  )
}
