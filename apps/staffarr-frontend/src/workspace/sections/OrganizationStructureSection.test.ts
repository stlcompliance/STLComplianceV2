import { describe, expect, it } from 'vitest'

import {
  restoreStoredOrgUnitDraft,
  toStoredOrgUnitDraft,
  type OrgUnitDraft,
} from './OrganizationStructureSection'

function buildDraft(overrides: Partial<OrgUnitDraft> = {}): OrgUnitDraft {
  return {
    unitType: 'site',
    name: 'North Terminal',
    code: 'NT-1',
    parentOrgUnitId: 'parent-1',
    status: 'active',
    description: 'Primary field site',
    managerPersonId: 'person-1',
    defaultSiteOrgUnitId: 'site-1',
    siteType: 'terminal',
    teamType: '',
    positionCode: '',
    effectiveStartDate: '2026-06-01',
    effectiveEndDate: '',
    timezone: 'America/Chicago',
    phone: '314-555-0100',
    emergencyContact: 'Security desk 314-555-0199',
    complianceSensitive: true,
    safetySensitive: true,
    canSupervise: false,
    canApprove: false,
    allowPeopleAssignment: true,
    visibleInDirectory: true,
    useInReporting: true,
    ...overrides,
  }
}

describe('OrganizationStructureSection draft storage', () => {
  it('omits emergency contact details from browser storage payloads', () => {
    const storedDraft = toStoredOrgUnitDraft(buildDraft())

    expect('emergencyContact' in storedDraft).toBe(false)
    expect(JSON.stringify(storedDraft)).not.toContain('Security desk')
  })

  it('does not restore legacy emergency contact details from browser storage', () => {
    const legacyStoredDraft = {
      ...toStoredOrgUnitDraft(buildDraft({ name: 'South Terminal' })),
      emergencyContact: 'Legacy emergency contact',
    } as unknown as Parameters<typeof restoreStoredOrgUnitDraft>[0]

    const restoredDraft = restoreStoredOrgUnitDraft(legacyStoredDraft, 'fallback-parent', 'fallback-site')

    expect(restoredDraft.name).toBe('South Terminal')
    expect(restoredDraft.emergencyContact).toBe('')
  })
})
