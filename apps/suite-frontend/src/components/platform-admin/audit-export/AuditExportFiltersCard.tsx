import { ControlledSelect, type PickerOption } from '@stl/shared-ui'

type Props = {
  tenantId: string
  fromDate: string
  toDate: string
  action: string
  result: string
  targetType: string
  actorUserId: string
  productKey: string
  tenantOptions: PickerOption[]
  actionOptions: PickerOption[]
  resultOptions: PickerOption[]
  targetTypeOptions: PickerOption[]
  productKeyOptions: PickerOption[]
  actorOptions: PickerOption[]
  onTenantIdChange: (value: string) => void
  onFromDateChange: (value: string) => void
  onToDateChange: (value: string) => void
  onActionChange: (value: string) => void
  onResultChange: (value: string) => void
  onTargetTypeChange: (value: string) => void
  onActorUserIdChange: (value: string) => void
  onProductKeyChange: (value: string) => void
}

export function AuditExportFiltersCard(props: Props) {
  return (
    <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
      <h3 className="text-sm font-medium text-slate-200">Export filters</h3>
      <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        <ControlledSelect
          label="Tenant scope (optional)"
          value={props.tenantId}
          onChange={props.onTenantIdChange}
          options={props.tenantOptions}
          emptyLabel="All tenants"
          testId="platform-audit-filter-tenant"
          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 sm:col-span-2 lg:col-span-3"
        />
        <label htmlFor="platform-audit-filter-from" className="block text-sm text-slate-300">
          Audit events from (optional)
          <input
            id="platform-audit-filter-from"
            type="date"
            value={props.fromDate}
            onChange={(event) => props.onFromDateChange(event.target.value)}
            data-testid="platform-audit-filter-from"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
        <label htmlFor="platform-audit-filter-to" className="block text-sm text-slate-300">
          Audit events to (optional)
          <input
            id="platform-audit-filter-to"
            type="date"
            value={props.toDate}
            onChange={(event) => props.onToDateChange(event.target.value)}
            data-testid="platform-audit-filter-to"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
        <ControlledSelect
          label="Action"
          value={props.action}
          onChange={props.onActionChange}
          options={props.actionOptions}
          emptyLabel="All actions"
          testId="platform-audit-filter-action"
        />
        <ControlledSelect
          label="Result"
          value={props.result}
          onChange={props.onResultChange}
          options={props.resultOptions}
          emptyLabel="All results"
          testId="platform-audit-filter-result"
        />
        <ControlledSelect
          label="Target type"
          value={props.targetType}
          onChange={props.onTargetTypeChange}
          options={props.targetTypeOptions}
          emptyLabel="All target types"
          testId="platform-audit-filter-target-type"
        />
        <ControlledSelect
          label="Product key"
          value={props.productKey}
          onChange={props.onProductKeyChange}
          options={props.productKeyOptions}
          emptyLabel="All products"
          testId="platform-audit-filter-product-key"
        />
        <ControlledSelect
          label="Actor user (optional)"
          value={props.actorUserId}
          onChange={props.onActorUserIdChange}
          options={props.actorOptions}
          emptyLabel="Any actor"
          testId="platform-audit-filter-actor"
          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 sm:col-span-2"
        />
      </div>
    </div>
  )
}
