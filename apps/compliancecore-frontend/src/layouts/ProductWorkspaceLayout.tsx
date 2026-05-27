import { ProductAppShell } from '@stl/shared-ui'
import { Outlet } from 'react-router-dom'

export function ProductWorkspaceLayout() {
  return (
    <ProductAppShell
      productName="Compliance Core"
      workspaceSubtitle="Vocabulary, rules, and evaluation"
    >
      <Outlet />
    </ProductAppShell>
  )
}
