import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
  AdvancedReferenceField,
  ControlledSelect,
  StaticSearchPicker,
  type PickerOption,
} from '@stl/shared-ui'

import { getRulePacks } from '../api/client'

const SCOPE_KEY_OPTIONS: PickerOption[] = [
  { value: 'tenant', label: 'Tenant' },
  { value: 'purchase_request', label: 'Purchase request' },
  { value: 'work_order', label: 'Work order' },
  { value: 'trip', label: 'Trip' },
]

export type M12AnalyticsScopeFiltersProps = {
  accessToken: string
  scopeKey: string
  onScopeKeyChange: (value: string) => void
  rulePackKey: string
  onRulePackKeyChange: (value: string) => void
  purchaseRequestId: string
  onPurchaseRequestIdChange: (value: string) => void
}

export function M12AnalyticsScopeFilters({
  accessToken,
  scopeKey,
  onScopeKeyChange,
  rulePackKey,
  onRulePackKeyChange,
  purchaseRequestId,
  onPurchaseRequestIdChange,
}: M12AnalyticsScopeFiltersProps) {
  const rulePacksQuery = useQuery({
    queryKey: ['compliancecore-rule-packs-picker', accessToken],
    queryFn: () => getRulePacks(accessToken),
  })

  const rulePackOptions: PickerOption[] = useMemo(
    () =>
      (rulePacksQuery.data ?? []).map((pack) => ({
        value: pack.packKey,
        label: `${pack.label} (${pack.packKey})`,
        inactive: !pack.isActive,
      })),
    [rulePacksQuery.data],
  )

  return (
    <div className="flex flex-wrap gap-3">
      <ControlledSelect
        label="Scope key"
        value={scopeKey}
        onChange={onScopeKeyChange}
        options={SCOPE_KEY_OPTIONS}
        emptyLabel="Select scope…"
        testId="m12-analytics-scope-key"
      />
      <StaticSearchPicker
        label="Rule pack (optional)"
        value={rulePackKey}
        onChange={onRulePackKeyChange}
        options={rulePackOptions}
        placeholder="All published packs"
        testId="m12-analytics-rule-pack"
      />
      <div className="min-w-[16rem] flex-1">
        <AdvancedReferenceField
          value={purchaseRequestId}
          onChange={onPurchaseRequestIdChange}
          label="Purchase request ID (context)"
          followUpId="compliancecore-supplyarr-pr-picker"
          testId="m12-analytics-purchase-request"
        />
      </div>
    </div>
  )
}
