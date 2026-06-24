import { describe, expect, it } from 'vitest'

import {
  buildExceptionExemptionSummary,
  buildExceptionExemptionTechnicalDetails,
} from './exceptionExemptionDisplay'
import type { ComplianceExceptionExemptionResponse } from '../api/types'

const sampleExemption: ComplianceExceptionExemptionResponse = {
  exceptionExemptionId: 'ex-1',
  tenantId: 'tenant-1',
  key: 'driving-holiday-relief',
  label: 'Holiday driving relief',
  type: 'regulatory_exception',
  governingBody: 'fmcsa',
  programKey: 'driver_qualification',
  packKey: 'holiday_schedule',
  citationKey: '49_cfr_391',
  applicabilityKey: 'holiday_driving',
  appliesToSubjectKind: 'driver',
  appliesToSourceProduct: 'routarr',
  appliesToSourceEntity: 'trip',
  effectType: 'makes_requirement_not_applicable',
  conditionLogicJson: '{"all":[{"fact":"holiday","equals":true}]}',
  requiredEvidenceOptionGroupId: 'group-1',
  issuingAuthority: 'Compliance Office',
  authorizationNumber: 'AUTH-1001',
  effectiveAt: '2026-06-01T00:00:00Z',
  expiresAt: '2026-12-31T23:59:59Z',
  active: true,
  description: 'Temporary relief for holiday operations.',
  createdAt: '2026-06-01T00:00:00Z',
  updatedAt: '2026-06-02T00:00:00Z',
}

describe('exceptionExemptionDisplay', () => {
  it('summarizes the exemption and keeps raw JSON in technical details', () => {
    const summary = buildExceptionExemptionSummary(sampleExemption)
    const technical = buildExceptionExemptionTechnicalDetails(sampleExemption)

    expect(summary.find((entry) => entry.label === 'Label')?.value).toBe('Holiday driving relief')
    expect(summary.find((entry) => entry.label === 'Condition logic')?.value).toContain('Top-level keys')
    expect(summary.some((entry) => entry.value.includes('ex-1'))).toBe(false)
    expect(technical.find((entry) => entry.label === 'Exception exemption ID')?.value).toBe('ex-1')
    expect(technical.find((entry) => entry.label === 'Condition logic JSON')?.value).toContain('"all"')
  })
})
