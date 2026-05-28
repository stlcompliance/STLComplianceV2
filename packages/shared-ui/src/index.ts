export { PageHeader } from './PageHeader'
export { PermissionHint } from './PermissionHint'
export { ProductAppShell } from './ProductAppShell'
export type { ProductAppShellProps, ProductNavItem } from './ProductAppShell'
export { ProductSwitcher } from './ProductSwitcher'
export type { ProductSwitcherProps } from './ProductSwitcher'
export {
  getSuiteProductIcon,
  hasProductEntitlement,
  listEntitledSuiteProducts,
  normalizeProductKey,
  SUITE_PRODUCT_CATALOG,
} from './productCatalog'
export type { SuiteProductCatalogEntry } from './productCatalog'
export { ProductWorkspaceFrame } from './ProductWorkspaceFrame'
export type { ProductWorkspaceFrameProps, ProductWorkspaceSession } from './ProductWorkspaceFrame'
export { resolveSuiteHomeUrl } from './suiteWorkspaceEnv'
export {
  isProductWorkspaceAuthError,
  resolveProductWorkspaceBootstrapError,
} from './productWorkspaceAuth'
