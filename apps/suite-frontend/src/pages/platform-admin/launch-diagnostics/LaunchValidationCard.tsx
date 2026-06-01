import { useMemo, useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
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
  const validateLaunchMutation = useMutation({
    mutationFn: nexarr.validatePlatformLaunch,
  })

  return (
    <section className="rounded-lg border border-slate-200 bg-white p-4">
      <h4 className="text-sm font-semibold text-stl-navy">Validate launch eligibility</h4>
      <p className="mt-1 text-xs text-slate-500">
        Check whether a tenant can launch a product right now and see the denial reason code.
      </p>
      <div className="mt-3 grid gap-3 md:grid-cols-3">
        <label className="text-xs font-medium text-slate-600">
          Tenant
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-2 py-1 text-sm"
            value={selectedTenantId}
            onChange={(event) => setSelectedTenantId(event.target.value)}
          >
            <option value="">Select tenant…</option>
            {tenants.map((tenant) => (
              <option key={tenant.tenantId} value={tenant.tenantId}>
                {tenant.tenantDisplayName} ({tenant.tenantSlug})
              </option>
            ))}
          </select>
        </label>
        <label className="text-xs font-medium text-slate-600">
          Product
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-2 py-1 text-sm"
            value={selectedProductKey}
            onChange={(event) => setSelectedProductKey(event.target.value)}
          >
            <option value="">Select product…</option>
            {products.map((product) => (
              <option key={product.productKey} value={product.productKey}>
                {product.productDisplayName}
              </option>
            ))}
          </select>
        </label>
        <div className="flex items-end">
          <button
            type="button"
            className="rounded-md bg-stl-navy px-3 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-60"
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
        <div className="mt-3 rounded-md border border-slate-200 bg-slate-50 p-3 text-sm">
          <p>
            <span className="font-medium text-stl-navy">Can launch:</span>{' '}
            {validateLaunchMutation.data.canLaunch ? 'Yes' : 'No'}
          </p>
          <p>
            <span className="font-medium text-stl-navy">Reason:</span>{' '}
            {validateLaunchMutation.data.reasonCode ?? 'none'}
          </p>
          <p className="break-all">
            <span className="font-medium text-stl-navy">Launch URL:</span>{' '}
            {validateLaunchMutation.data.launchUrl ?? 'none'}
          </p>
        </div>
      ) : null}
    </section>
  )
}
