import { describe, expect, it } from 'vitest'
import type { StaffPersonSummaryResponse } from '../api/types'
import { filterPeopleDirectory, personMatchesDirectoryQuery } from './peopleDirectoryFilter'

const people: StaffPersonSummaryResponse[] = [
  {
    personId: '1',
    externalUserId: null,
    displayName: 'Alex Rivera',
    primaryEmail: 'alex.rivera@example.com',
    employmentStatus: 'active',
    primaryOrgUnitId: 'ou-1',
    primaryOrgUnitName: 'Operations',
    managerPersonId: null,
    jobTitle: 'Operator',
  },
  {
    personId: '2',
    externalUserId: null,
    displayName: 'Sam Patel',
    primaryEmail: 'sam.patel@example.com',
    employmentStatus: 'inactive',
    primaryOrgUnitId: 'ou-2',
    primaryOrgUnitName: 'Quality',
    managerPersonId: '1',
    jobTitle: 'Auditor',
  },
]

describe('peopleDirectoryFilter', () => {
  it('matches on display name and email case-insensitively', () => {
    expect(personMatchesDirectoryQuery(people[0], 'alex')).toBe(true)
    expect(personMatchesDirectoryQuery(people[0], 'RIVERA@EXAMPLE.COM')).toBe(true)
  })

  it('matches on job title, org unit, and employment status', () => {
    expect(personMatchesDirectoryQuery(people[0], 'operator')).toBe(true)
    expect(personMatchesDirectoryQuery(people[1], 'quality')).toBe(true)
    expect(personMatchesDirectoryQuery(people[1], 'inactive')).toBe(true)
  })

  it('returns all people for empty query', () => {
    expect(filterPeopleDirectory(people, '')).toHaveLength(2)
    expect(filterPeopleDirectory(people, '   ')).toHaveLength(2)
  })

  it('filters people for non-matching queries', () => {
    const filtered = filterPeopleDirectory(people, 'auditor')
    expect(filtered).toHaveLength(1)
    expect(filtered[0]?.personId).toBe('2')
    expect(filterPeopleDirectory(people, 'nope')).toHaveLength(0)
  })
})
