import { useQuery } from '@tanstack/react-query'
import { type FormEvent, useEffect, useMemo, useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { listSiteLocations } from '../api/client'
import type { OrgUnitResponse, StaffPersonDetailResponse } from '../api/types'

const WRITER_ROLES = new Set(['tenant_admin', 'staffarr_admin', 'hr_admin'])

const EMPLOYMENT_STATUS_OPTIONS = [
  { value: 'applicant', label: 'Applicant' },
  { value: 'pending_start', label: 'Pending start' },
  { value: 'onboarding', label: 'Onboarding' },
  { value: 'active', label: 'Active' },
  { value: 'leave', label: 'Leave' },
  { value: 'suspended', label: 'Suspended' },
  { value: 'terminated', label: 'Terminated' },
  { value: 'inactive', label: 'Inactive' },
  { value: 'archived', label: 'Archived' },
] as const

const WORK_RELATIONSHIP_OPTIONS = [
  { value: 'employee', label: 'Employee' },
  { value: 'contractor', label: 'Contractor' },
  { value: 'temp', label: 'Temp' },
  { value: 'vendor_worker', label: 'Vendor worker' },
  { value: 'customer_contact', label: 'Customer contact' },
  { value: 'auditor', label: 'Auditor' },
  { value: 'service_account_contact', label: 'Service account contact' },
  { value: 'other', label: 'Other' },
] as const

const EMPLOYMENT_TYPE_OPTIONS = [
  { value: 'full_time', label: 'Full time' },
  { value: 'part_time', label: 'Part time' },
  { value: 'seasonal', label: 'Seasonal' },
  { value: 'temporary', label: 'Temporary' },
  { value: 'contract', label: 'Contract' },
  { value: 'non_employee', label: 'Non-employee' },
] as const

interface PersonProfileEditorPanelProps {
  accessToken: string
  profile: StaffPersonDetailResponse
  orgUnits: OrgUnitResponse[]
  peopleOptions: Array<{ personId: string; displayName: string }>
  siteContextOrgUnitId?: string | null
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
  onUpdate: (request: {
    legalFirstName?: string | null
    legalMiddleName?: string | null
    legalLastName?: string | null
    preferredName?: string | null
    pronouns?: string | null
    givenName?: string | null
    familyName?: string | null
    primaryEmail: string
    alternateEmail?: string | null
    primaryPhone?: string | null
    alternatePhone?: string | null
    workPhone?: string | null
    workRelationshipType?: string | null
    employmentType?: string | null
    startDate?: string | null
    expectedStartDate?: string | null
    primaryOrgUnitId?: string | null
    siteOrgUnitId?: string | null
    managerPersonId?: string | null
    jobTitle?: string | null
    homeBaseLocationId?: string | null
    canLoginSnapshot?: boolean | null
  }) => Promise<void>
  onEmploymentStatusChange: (request: { employmentStatus: string; reason: string | null }) => Promise<void>
}

export function canManagePeople(roleKey: string, isPlatformAdmin: boolean): boolean {
  return isPlatformAdmin || WRITER_ROLES.has(roleKey)
}

function toOrgUnitOptions(orgUnits: OrgUnitResponse[]): PickerOption[] {
  return orgUnits.map((unit) => ({
    value: unit.orgUnitId,
    label: `${unit.unitType} · ${unit.name}`,
  }))
}

export function PersonProfileEditorPanel({
  accessToken,
  profile,
  orgUnits,
  peopleOptions,
  siteContextOrgUnitId = null,
  canManage,
  isSubmitting,
  errorMessage,
  onUpdate,
  onEmploymentStatusChange,
}: PersonProfileEditorPanelProps) {
  const [legalFirstName, setLegalFirstName] = useState(profile.legalFirstName)
  const [legalMiddleName, setLegalMiddleName] = useState(profile.legalMiddleName ?? '')
  const [legalLastName, setLegalLastName] = useState(profile.legalLastName)
  const [preferredName, setPreferredName] = useState(profile.preferredName ?? '')
  const [pronouns, setPronouns] = useState(profile.pronouns ?? '')
  const [primaryEmail, setPrimaryEmail] = useState(profile.primaryEmail)
  const [alternateEmail, setAlternateEmail] = useState(profile.alternateEmail ?? '')
  const [primaryPhone, setPrimaryPhone] = useState(profile.primaryPhone ?? '')
  const [alternatePhone, setAlternatePhone] = useState(profile.alternatePhone ?? '')
  const [workPhone, setWorkPhone] = useState(profile.workPhone ?? '')
  const [primaryOrgUnitId, setPrimaryOrgUnitId] = useState(profile.primaryOrgUnitId ?? '')
  const [managerPersonId, setManagerPersonId] = useState(profile.managerPersonId ?? '')
  const [jobTitle, setJobTitle] = useState(profile.jobTitle ?? '')
  const [workRelationshipType, setWorkRelationshipType] = useState(profile.workRelationshipType ?? 'employee')
  const [employmentType, setEmploymentType] = useState(profile.employmentType ?? 'full_time')
  const [startDate, setStartDate] = useState(profile.startDate ? profile.startDate.slice(0, 10) : '')
  const [expectedStartDate, setExpectedStartDate] = useState(
    profile.expectedStartDate ? profile.expectedStartDate.slice(0, 10) : '',
  )
  const [homeBaseLocationId, setHomeBaseLocationId] = useState(profile.homeBaseLocationId ?? '')
  const [canLoginSnapshot, setCanLoginSnapshot] = useState(profile.canLoginSnapshot)
  const [statusReason, setStatusReason] = useState('')
  const [statusDraft, setStatusDraft] = useState(profile.employmentStatus)

  useEffect(() => {
    setLegalFirstName(profile.legalFirstName)
    setLegalMiddleName(profile.legalMiddleName ?? '')
    setLegalLastName(profile.legalLastName)
    setPreferredName(profile.preferredName ?? '')
    setPronouns(profile.pronouns ?? '')
    setPrimaryEmail(profile.primaryEmail)
    setAlternateEmail(profile.alternateEmail ?? '')
    setPrimaryPhone(profile.primaryPhone ?? '')
    setAlternatePhone(profile.alternatePhone ?? '')
    setWorkPhone(profile.workPhone ?? '')
    setPrimaryOrgUnitId(profile.primaryOrgUnitId ?? '')
    setManagerPersonId(profile.managerPersonId ?? '')
    setJobTitle(profile.jobTitle ?? '')
    setWorkRelationshipType(profile.workRelationshipType ?? 'employee')
    setEmploymentType(profile.employmentType ?? 'full_time')
    setStartDate(profile.startDate ? profile.startDate.slice(0, 10) : '')
    setExpectedStartDate(profile.expectedStartDate ? profile.expectedStartDate.slice(0, 10) : '')
    setHomeBaseLocationId(profile.homeBaseLocationId ?? '')
    setCanLoginSnapshot(profile.canLoginSnapshot)
    setStatusReason('')
    setStatusDraft(profile.employmentStatus)
  }, [profile])

  const managerChoices = peopleOptions.filter((person) => person.personId !== profile.personId)
  const orgUnitOptions = toOrgUnitOptions(orgUnits)
  const selectedOrgUnitOption = orgUnitOptions.find((option) => option.value === primaryOrgUnitId)
  const managerOptions: PickerOption[] = managerChoices.map((person) => ({
    value: person.personId,
    label: person.displayName,
  }))
  const selectedManagerOption = managerOptions.find((option) => option.value === managerPersonId)

  const locationQuery = useQuery({
    queryKey: ['staffarr-site-locations', accessToken, siteContextOrgUnitId],
    queryFn: () => listSiteLocations(accessToken, siteContextOrgUnitId!),
    enabled: Boolean(accessToken && siteContextOrgUnitId),
  })

  const locationOptions = useMemo<PickerOption[]>(
    () =>
      (locationQuery.data ?? []).map((location) => ({
        value: location.locationId,
        label: location.parentPathSnapshot,
      })),
    [locationQuery.data],
  )
  const selectedLocationOption = locationOptions.find((option) => option.value === homeBaseLocationId)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onUpdate({
      legalFirstName: legalFirstName.trim(),
      legalMiddleName: legalMiddleName.trim() || null,
      legalLastName: legalLastName.trim(),
      preferredName: preferredName.trim() || null,
      pronouns: pronouns.trim() || null,
      givenName: legalFirstName.trim(),
      familyName: legalLastName.trim(),
      primaryEmail: primaryEmail.trim(),
      alternateEmail: alternateEmail.trim() || null,
      primaryPhone: primaryPhone.trim() || null,
      alternatePhone: alternatePhone.trim() || null,
      workPhone: workPhone.trim() || null,
      workRelationshipType: workRelationshipType || null,
      employmentType: employmentType || null,
      startDate: startDate || null,
      expectedStartDate: expectedStartDate || null,
      primaryOrgUnitId: primaryOrgUnitId || null,
      siteOrgUnitId: siteContextOrgUnitId || null,
      managerPersonId: managerPersonId || null,
      jobTitle: jobTitle.trim() || null,
      homeBaseLocationId: homeBaseLocationId || null,
      canLoginSnapshot,
    })
  }

  return (
    <section className="mt-6 space-y-4 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Profile management</h2>
          <p className="mt-1 text-xs text-slate-500">
            Edit the StaffArr workforce profile, placement snapshot, and NexArr login intent.
          </p>
        </div>
        <span className={`text-xs ${canManage ? 'text-emerald-300' : 'text-slate-500'}`}>
          {canManage ? 'Write enabled' : 'Read only'}
        </span>
      </header>

      {canManage ? (
        <form className="grid gap-4 md:grid-cols-2" onSubmit={handleSubmit}>
          <label htmlFor="edit-person-legal-first-name" className="block text-sm text-slate-300">
            Legal first name
            <input
              id="edit-person-legal-first-name"
              value={legalFirstName}
              onChange={(event) => setLegalFirstName(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              required
            />
          </label>
          <label htmlFor="edit-person-legal-middle-name" className="block text-sm text-slate-300">
            Legal middle name
            <input
              id="edit-person-legal-middle-name"
              value={legalMiddleName}
              onChange={(event) => setLegalMiddleName(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="edit-person-legal-last-name" className="block text-sm text-slate-300">
            Legal last name
            <input
              id="edit-person-legal-last-name"
              value={legalLastName}
              onChange={(event) => setLegalLastName(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              required
            />
          </label>
          <label htmlFor="edit-person-preferred-name" className="block text-sm text-slate-300">
            Preferred name
            <input
              id="edit-person-preferred-name"
              value={preferredName}
              onChange={(event) => setPreferredName(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="edit-person-pronouns" className="block text-sm text-slate-300 md:col-span-2">
            Pronouns
            <input
              id="edit-person-pronouns"
              value={pronouns}
              onChange={(event) => setPronouns(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="edit-person-primary-email" className="block text-sm text-slate-300">
            Primary email
            <input
              id="edit-person-primary-email"
              type="email"
              value={primaryEmail}
              onChange={(event) => setPrimaryEmail(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              required
            />
          </label>
          <label htmlFor="edit-person-alternate-email" className="block text-sm text-slate-300">
            Alternate email
            <input
              id="edit-person-alternate-email"
              type="email"
              value={alternateEmail}
              onChange={(event) => setAlternateEmail(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="edit-person-primary-phone" className="block text-sm text-slate-300">
            Primary phone
            <input
              id="edit-person-primary-phone"
              value={primaryPhone}
              onChange={(event) => setPrimaryPhone(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="edit-person-alternate-phone" className="block text-sm text-slate-300">
            Alternate phone
            <input
              id="edit-person-alternate-phone"
              value={alternatePhone}
              onChange={(event) => setAlternatePhone(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="edit-person-work-phone" className="block text-sm text-slate-300">
            Work phone
            <input
              id="edit-person-work-phone"
              value={workPhone}
              onChange={(event) => setWorkPhone(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="edit-person-job-title" className="block text-sm text-slate-300">
            Job title
            <input
              id="edit-person-job-title"
              value={jobTitle}
              onChange={(event) => setJobTitle(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="edit-person-work-relationship" className="block text-sm text-slate-300">
            Work relationship
            <select
              id="edit-person-work-relationship"
              value={workRelationshipType}
              onChange={(event) => setWorkRelationshipType(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              {WORK_RELATIONSHIP_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="edit-person-employment-type" className="block text-sm text-slate-300">
            Employment type
            <select
              id="edit-person-employment-type"
              value={employmentType}
              onChange={(event) => setEmploymentType(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              {EMPLOYMENT_TYPE_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="edit-person-start-date" className="block text-sm text-slate-300">
            Start date
            <input
              id="edit-person-start-date"
              type="date"
              value={startDate}
              onChange={(event) => setStartDate(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="edit-person-expected-start-date" className="block text-sm text-slate-300">
            Expected start date
            <input
              id="edit-person-expected-start-date"
              type="date"
              value={expectedStartDate}
              onChange={(event) => setExpectedStartDate(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label htmlFor="edit-person-primary-org-unit" className="block text-sm text-slate-300">
            Primary org unit
            <StaticSearchPicker
              id="edit-person-primary-org-unit"
              label="Primary org unit"
              value={primaryOrgUnitId}
              onChange={setPrimaryOrgUnitId}
              options={orgUnitOptions}
              placeholder="Search org units..."
              testId="edit-person-primary-org-unit-picker"
              selectedOption={selectedOrgUnitOption}
            />
          </label>
          <label htmlFor="edit-person-manager" className="block text-sm text-slate-300">
            Manager
            <StaticSearchPicker
              id="edit-person-manager"
              label="Manager"
              value={managerPersonId}
              onChange={setManagerPersonId}
              options={managerOptions}
              placeholder="Search managers..."
              testId="edit-person-manager-picker"
              selectedOption={selectedManagerOption}
            />
          </label>
          <label htmlFor="edit-person-home-base-location" className="block text-sm text-slate-300">
            Home base location
            <StaticSearchPicker
              id="edit-person-home-base-location"
              label="Home base location"
              value={homeBaseLocationId}
              onChange={setHomeBaseLocationId}
              options={locationOptions}
              placeholder={siteContextOrgUnitId ? 'Search site locations...' : 'Select a site assignment first'}
              testId="edit-person-home-base-location-picker"
              selectedOption={selectedLocationOption}
              disabled={!siteContextOrgUnitId || locationQuery.isLoading}
            />
            {locationQuery.isLoading ? (
              <p className="mt-1 text-xs text-slate-500">Loading site locations...</p>
            ) : null}
          </label>
          <label className="flex items-center gap-2 text-sm text-slate-300 md:col-span-2">
            <input
              type="checkbox"
              checked={canLoginSnapshot}
              onChange={(event) => setCanLoginSnapshot(event.target.checked)}
            />
            Login requested through NexArr
          </label>
          <div className="md:col-span-2">
            <button
              type="submit"
              disabled={isSubmitting}
              className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            >
              {isSubmitting ? 'Saving...' : 'Save profile changes'}
            </button>
          </div>
        </form>
      ) : (
        <p className="text-sm text-slate-500">
          Profile edits require tenant admin, StaffArr admin, or HR admin role.
        </p>
      )}

      {canManage ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <h3 className="text-sm font-medium text-slate-200">Employment status</h3>
          <div className="mt-3 grid gap-3 md:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_auto]">
            <label htmlFor="edit-person-status" className="block text-sm text-slate-300">
              Status
              <select
                id="edit-person-status"
                value={statusDraft}
                onChange={(event) => setStatusDraft(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              >
                {EMPLOYMENT_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="edit-person-status-reason" className="block text-sm text-slate-300">
              Reason (optional)
              <input
                id="edit-person-status-reason"
                value={statusReason}
                onChange={(event) => setStatusReason(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <div className="self-end">
              <button
                type="button"
                disabled={isSubmitting || statusDraft === profile.employmentStatus}
                className="rounded-md border border-amber-700 px-3 py-2 text-xs text-amber-200 hover:bg-amber-950/40 disabled:opacity-50"
                onClick={() =>
                  onEmploymentStatusChange({ employmentStatus: statusDraft, reason: statusReason || null })
                }
              >
                Apply status
              </button>
            </div>
          </div>
        </div>
      ) : null}

      {errorMessage ? <ApiErrorCallout title="Profile update failed" message={errorMessage} /> : null}
    </section>
  )
}
