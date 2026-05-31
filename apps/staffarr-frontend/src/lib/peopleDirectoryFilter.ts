import type { StaffPersonSummaryResponse } from '../api/types'

function normalize(value: string | null | undefined): string {
  return (value ?? '').trim().toLocaleLowerCase()
}

export function personMatchesDirectoryQuery(person: StaffPersonSummaryResponse, rawQuery: string): boolean {
  const query = normalize(rawQuery)
  if (!query) {
    return true
  }

  const haystack = [
    person.displayName,
    person.primaryEmail,
    person.jobTitle,
    person.primaryOrgUnitName,
    person.employmentStatus,
  ]
    .map((part) => normalize(part))
    .join(' ')

  return haystack.includes(query)
}

export function filterPeopleDirectory(
  people: StaffPersonSummaryResponse[],
  rawQuery: string,
): StaffPersonSummaryResponse[] {
  return people.filter((person) => personMatchesDirectoryQuery(person, rawQuery))
}
