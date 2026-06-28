import { describe, expect, it } from 'vitest'
import {
  buildTenantSettingsDiff,
  validateTenantSettingsDraft,
} from './TenantSettingsPanel'
import type { LoadArrTenantSettingsOptionsResponse } from '../api/client'

const options: LoadArrTenantSettingsOptionsResponse = {
  sections: [
    {
      key: 'receiving',
      label: 'Receiving',
      description: 'Receiving policy',
      defaultValue: {},
      fields: [
        {
          key: 'overReceiptTolerancePercent',
          label: 'Over-receipt tolerance',
          inputType: 'number',
          min: 0,
          max: 100,
          enumKey: null,
          risky: false,
        },
      ],
    },
    {
      key: 'traceability',
      label: 'Traceability',
      description: 'Traceability policy',
      defaultValue: {},
      fields: [
        {
          key: 'requireLpnScan',
          label: 'Require LPN scan',
          inputType: 'boolean',
          min: null,
          max: null,
          enumKey: null,
          risky: false,
        },
      ],
    },
  ],
  enumOptions: {},
  eventNames: [],
}

describe('TenantSettingsPanel helpers', () => {
  it('flags blocking errors for invalid receiving tolerance and LPN dependencies', () => {
    const result = validateTenantSettingsDraft({
      receiving: {
        allowOverReceipt: false,
        overReceiptTolerancePercent: 25,
      },
      traceability: {
        enableLpn: false,
        requireLpnScan: true,
      },
      labelingAndDocuments: {
        generateLpnLabels: true,
      },
    })

    expect(result.errors.map((error) => error.code)).toEqual([
      'loadarr.ui.receiving.over_receipt_disabled_tolerance',
      'loadarr.ui.traceability.lpn_scan_requires_lpn',
      'loadarr.ui.documents.lpn_labels_require_lpn',
    ])
  })

  it('flags risky draft choices as warnings', () => {
    const result = validateTenantSettingsDraft({
      inventoryControl: {
        allowNegativeInventory: true,
      },
      compliance: {
        enableComplianceCoreChecks: false,
      },
      mobileScanner: {
        allowOfflineTaskExecution: true,
      },
    })

    expect(result.warnings.map((warning) => warning.code)).toEqual([
      'loadarr.ui.inventory.negative_inventory',
      'loadarr.ui.compliance.disabled',
      'loadarr.ui.mobile.offline_execution',
    ])
    expect(result.warnings.find((warning) => warning.code === 'loadarr.ui.mobile.offline_execution')?.message).toContain(
      'readiness policy',
    )
  })

  it('uses settings metadata to produce human-readable diff labels', () => {
    const diffs = buildTenantSettingsDiff(
      {
        receiving: {
          overReceiptTolerancePercent: 5,
        },
      },
      {
        receiving: {
          overReceiptTolerancePercent: 10,
        },
        traceability: {
          requireLpnScan: true,
        },
      },
      options,
    )

    expect(diffs).toEqual([
      {
        path: 'receiving.overReceiptTolerancePercent',
        label: 'Receiving: Over-receipt tolerance',
        before: 5,
        after: 10,
      },
      {
        path: 'traceability.requireLpnScan',
        label: 'Traceability: Require LPN scan',
        before: null,
        after: true,
      },
    ])
  })
})
