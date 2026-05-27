import { ProductAppShell } from '@stl/shared-ui'
import { Outlet } from 'react-router-dom'

export function ProductWorkspaceLayout() {
  return (
    <ProductAppShell
      productName="StaffArr"
      workspaceSubtitle="People, org, and readiness"
    >
      <Outlet />
    </ProductAppShell>
  )
}
