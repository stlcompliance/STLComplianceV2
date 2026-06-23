import { useMemo, useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'
import type { LaunchDiagnosticRow } from '../../../api/types'
import * as nexarr from '../../../api/nexarrClient'

type Props = {
  rows: LaunchDiagnosticRow[]
}

export function LaunchValidationCard({ rows }: Props) {
  const [selectedTenantId, setSelectedTenantId] = useState('')
  const [selectedProductKey, setSelectedProductKey] = useState('')
  const tenants = useMemo(
    () => [...new Map(rows.map((row) => [row.tenantId, row])).values()],
    [rows],
  )
  const products = useMemo(
    () => [...new Map(rows.map((row) => [row.productKey, row])).values()],
    [rows],
  )
  const tenantOptions = useMemo<PickerOption[]>(
    () =>
      tenants.map((tenant) => ({
        value: tenant.tenantId,
        label: tenant.tenantDisplayName,
      })),
    [tenants],
  )
  const productOptions = useMemo<PickerOption[]>(
    () =>
      products.map((product) => ({
        value: product.productKey,
        label: product.productDisplayName,
      })),
    [products],
  )
  const validateLaunchMutation = useMutation({
    mutationFn: nexarr.validatePlatformLaunch,
  })

  return (
    <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
      <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Validate launch eligibility</h4>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        Check whether a tenant can launch a product right now and see the denial reason code.
      </p>
      <div className="mt-3 grid gap-3 md:grid-cols-3">
        <StaticSearchPicker
          label="Tenant"
          id="launch-validation-tenant"
          value={selectedTenantId}
          onChange={setSelectedTenantId}
          options={tenantOptions}
          placeholder="Search tenants"
          testId="launch-validation-tenant"
        />
        <StaticSearchPicker
          label="Product"
          id="launch-validation-product"
          value={selectedProductKey}
          onChange={setSelectedProductKey}
          options={productOptions}
          placeholder="Search products"
          testId="launch-validation-product"
        />
        <div className="flex items-end">
          <button
            type="button"
            className="rounded-md bg-[var(--color-accent)] px-3 py-2 text-sm font-medium text-[var(--color-on-accent)] disabled:cursor-not-allowed disabled:opacity-60"
            disabled={!selectedTenantId || !selectedProductKey || validateLaunchMutation.isPending}
            onClick={() =>
              validateLaunchMutation.mutate({
                tenantId: selectedTenantId,
                productKey: selectedProductKey,
              })
            }
          >
            {validateLaunchMutation.isPending ? 'Validating…' : 'Validate launch'}
          </button>
        </div>
      </div>
      {validateLaunchMutation.isError ? (
        <ApiErrorCallout
          className="mt-3"
          message={getErrorMessage(validateLaunchMutation.error, 'Failed to validate launch.')}
        />
      ) : null}
      {validateLaunchMutation.data ? (
        <div className="mt-3 rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3 text-sm">
          <p>
            <span className="font-medium text-[var(--color-text-primary)]">Can launch:</span>{' '}
            {validateLaunchMutation.data.canLaunch ? 'Yes' : 'No'}
          </p>
          <p>
            <span className="font-medium text-[var(--color-text-primary)]">Reason:</span>{' '}
            {validateLaunchMutation.data.reasonCode ?? 'none'}
          </p>
          <p className="break-all">
            <span className="font-medium text-[var(--color-text-primary)]">Launch URL:</span>{' '}
            {validateLaunchMutation.data.launchUrl ?? 'none'}
          </p>
        </div>
      ) : null}
    </section>
  )
}
