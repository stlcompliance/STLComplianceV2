import { getErrorMessage } from '@stl/shared-ui'
import { PermissionCheckPanel } from '../../components/PermissionCheckPanel'
import { PermissionProjectionTimelinePanel } from '../../components/PermissionProjectionTimelinePanel'
import { ProductPermissionCatalogPanel } from '../../components/ProductPermissionCatalogPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function PermissionsSection({ state }: Props) {
  const s = state
  if (!s.selectedPerson) {
    return <p className="text-sm text-slate-400">Select a person on the People page to manage permissions.</p>
  }

  return (
    <>
      <ProductPermissionCatalogPanel
        productKeyFilter={s.productPermissionCatalogProductKey}
        catalog={s.productPermissionCatalogQuery.data ?? []}
        isLoading={s.productPermissionCatalogQuery.isLoading}
        isError={s.productPermissionCatalogQuery.isError}
        readErrorMessage={
          s.productPermissionCatalogQuery.isError
            ? getErrorMessage(
                s.productPermissionCatalogQuery.error,
                'Failed to load product permission catalog.',
              )
            : null
        }
        onRetryRead={() => {
          void s.productPermissionCatalogQuery.refetch()
        }}
        onProductKeyFilterChange={s.setProductPermissionCatalogProductKey}
      />

      <PermissionCheckPanel
        personId={s.selectedPerson.personId}
        personDisplayName={s.selectedPerson.displayName}
        permissionCheckInput={s.permissionCheckInput}
        checkResult={s.permissionCheckMutation.data ?? null}
        isChecking={s.permissionCheckMutation.isPending}
        errorMessage={
          s.permissionCheckMutationError
            ? getErrorMessage(
                s.permissionCheckMutationError,
                'Failed to run permission check.',
              )
            : null
        }
        onPermissionCheckInputChange={s.setPermissionCheckInput}
        onCheckPermissions={async () => {
          const permissionKeys = s.permissionCheckInput
            .split(/[\n,]/)
            .map((entry) => entry.trim())
            .filter(Boolean)
          await s.permissionCheckMutation.mutateAsync({
            personId: s.selectedPerson!.personId,
            permissionKeys,
          })
        }}
      />

      <PermissionProjectionTimelinePanel
        personDisplayName={s.selectedPerson.displayName}
        orgUnits={s.orgUnits}
        projection={s.effectivePermissions}
        isLoading={s.effectivePermissionsQuery.isLoading}
        isError={s.effectivePermissionsQuery.isError}
        readErrorMessage={
          s.effectivePermissionsQuery.isError
            ? getErrorMessage(
                s.effectivePermissionsQuery.error,
                'Failed to load effective permission projection.',
              )
            : null
        }
        onRetryRead={() => {
          void s.effectivePermissionsQuery.refetch()
        }}
      />
    </>
  )
}
