import { useQuery } from '@tanstack/react-query'
import { type FormEvent, useMemo, useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { listSiteLocations } from '../api/client'
import type {
  CreatePersonRoleAssignmentRequest,
  OrgUnitResponse,
  RoleTemplateResponse,
} from '../api/types'

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

const ROLE_SCOPE_OPTIONS: Array<{
  value: CreatePersonRoleAssignmentRequest['scopeType']
  label: string
}> = [
  { value: 'tenant', label: 'Tenant' },
  { value: 'site', label: 'Site' },
  { value: 'department', label: 'Department' },
  { value: 'team', label: 'Team' },
  { value: 'position', label: 'Position' },
] as const

type WizardStep = 0 | 1 | 2 | 3 | 4

type InitialRoleAssignmentDraft = {
  id: string
  roleTemplateId: string
  scopeType: CreatePersonRoleAssignmentRequest['scopeType']
  scopeValue: string
  expiresAt: string
}

interface CreatePersonPanelProps {
  accessToken: string
  orgUnits: OrgUnitResponse[]
  peopleOptions: Array<{ personId: string; displayName: string }>
  roleTemplates?: RoleTemplateResponse[]
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
  onCreate: (request: {
    legalFirstName?: string | null
    legalMiddleName?: string | null
    legalLastName?: string | null
    preferredName?: string | null
    pronouns?: string | null
    givenName?: string | null
    familyName?: string | null
    primaryEmail: string
    employmentStatus: string
    workRelationshipType?: string | null
    employmentType?: string | null
    alternateEmail?: string | null
    primaryPhone?: string | null
    alternatePhone?: string | null
    workPhone?: string | null
    startDate?: string | null
    expectedStartDate?: string | null
    primaryOrgUnitId?: string | null
    siteOrgUnitId?: string | null
    departmentOrgUnitId?: string | null
    teamOrgUnitId?: string | null
    positionOrgUnitId?: string | null
    managerPersonId?: string | null
    jobTitle?: string | null
    homeBaseLocationId?: string | null
    canLogin?: boolean
    initialRoleAssignments?: CreatePersonRoleAssignmentRequest[] | null
  }) => Promise<void>
}

function byType(orgUnits: OrgUnitResponse[], unitType: string): OrgUnitResponse[] {
  return orgUnits
    .filter((orgUnit) => orgUnit.status === 'active' && orgUnit.unitType.toLowerCase() === unitType)
    .sort((left, right) => left.name.localeCompare(right.name))
}

function toPickerOptions(items: Array<{ value: string; label: string }>): PickerOption[] {
  return items
}

function orgUnitOptions(orgUnits: OrgUnitResponse[], unitType: string): PickerOption[] {
  return byType(orgUnits, unitType).map((orgUnit) => ({
    value: orgUnit.orgUnitId,
    label: orgUnit.name,
  }))
}

function buildDisplayName(legalFirstName: string, legalLastName: string, preferredName: string): string {
  const first = preferredName.trim() || legalFirstName.trim()
  return `${first} ${legalLastName.trim()}`.trim()
}

function stepTitle(step: WizardStep): string {
  switch (step) {
    case 0:
      return 'Identity'
    case 1:
      return 'Contact & Status'
    case 2:
      return 'Placement'
    case 3:
      return 'Access Setup'
    default:
      return 'Review & Create'
  }
}

function isStepValid(
  step: WizardStep,
  values: {
    legalFirstName: string
    legalLastName: string
    primaryEmail: string
    workRelationshipType: string
    employmentStatus: string
    siteOrgUnitId: string
    departmentOrgUnitId: string
    teamOrgUnitId: string
    positionOrgUnitId: string
  },
): boolean {
  if (step === 0) {
    return Boolean(values.legalFirstName.trim() && values.legalLastName.trim())
  }

  if (step === 1) {
    return Boolean(values.primaryEmail.trim() && values.workRelationshipType && values.employmentStatus)
  }

  if (step === 2) {
    return Boolean(
      values.siteOrgUnitId &&
      values.departmentOrgUnitId &&
      values.teamOrgUnitId &&
      values.positionOrgUnitId,
    )
  }

  return true
}

function ScopeValuePicker({
  scopeType,
  value,
  onChange,
  orgUnits,
  testId,
}: {
  scopeType: CreatePersonRoleAssignmentRequest['scopeType']
  value: string
  onChange: (value: string) => void
  orgUnits: OrgUnitResponse[]
  testId: string
}) {
  if (scopeType === 'tenant') {
    return null
  }

  const options = orgUnitOptions(orgUnits, scopeType)
  const selectedOption = options.find((option) => option.value === value)

  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={options}
      selectedOption={selectedOption}
      placeholder={`Select ${scopeType} scope`}
      testId={testId}
    />
  )
}

export function CreatePersonPanel({
  accessToken,
  orgUnits,
  peopleOptions,
  roleTemplates = [],
  canManage,
  isSubmitting,
  errorMessage,
  onCreate,
}: CreatePersonPanelProps) {
  const [step, setStep] = useState<WizardStep>(0)
  const [legalFirstName, setLegalFirstName] = useState('')
  const [legalMiddleName, setLegalMiddleName] = useState('')
  const [legalLastName, setLegalLastName] = useState('')
  const [preferredName, setPreferredName] = useState('')
  const [pronouns, setPronouns] = useState('')
  const [primaryEmail, setPrimaryEmail] = useState('')
  const [alternateEmail, setAlternateEmail] = useState('')
  const [primaryPhone, setPrimaryPhone] = useState('')
  const [alternatePhone, setAlternatePhone] = useState('')
  const [workPhone, setWorkPhone] = useState('')
  const [employmentStatus, setEmploymentStatus] = useState('pending_start')
  const [workRelationshipType, setWorkRelationshipType] = useState('employee')
  const [employmentType, setEmploymentType] = useState('full_time')
  const [startDate, setStartDate] = useState('')
  const [expectedStartDate, setExpectedStartDate] = useState('')
  const [siteOrgUnitId, setSiteOrgUnitId] = useState('')
  const [departmentOrgUnitId, setDepartmentOrgUnitId] = useState('')
  const [teamOrgUnitId, setTeamOrgUnitId] = useState('')
  const [positionOrgUnitId, setPositionOrgUnitId] = useState('')
  const [managerPersonId, setManagerPersonId] = useState('')
  const [jobTitle, setJobTitle] = useState('')
  const [homeBaseLocationId, setHomeBaseLocationId] = useState('')
  const [canLogin, setCanLogin] = useState(false)
  const [initialRoleAssignments, setInitialRoleAssignments] = useState<InitialRoleAssignmentDraft[]>([])

  const locationQuery = useQuery({
    queryKey: ['staffarr-site-locations', accessToken, siteOrgUnitId],
    queryFn: () => listSiteLocations(accessToken, siteOrgUnitId),
    enabled: Boolean(accessToken && siteOrgUnitId),
  })

  const siteOptions = useMemo(() => orgUnitOptions(orgUnits, 'site'), [orgUnits])
  const departmentOptions = useMemo(() => orgUnitOptions(orgUnits, 'department'), [orgUnits])
  const teamOptions = useMemo(() => orgUnitOptions(orgUnits, 'team'), [orgUnits])
  const positionOptions = useMemo(() => orgUnitOptions(orgUnits, 'position'), [orgUnits])
  const managerOptions = useMemo(
    () => toPickerOptions(peopleOptions.map((person) => ({ value: person.personId, label: person.displayName }))),
    [peopleOptions],
  )
  const locationOptions = useMemo(
    () =>
      toPickerOptions(
        (locationQuery.data ?? []).map((location) => ({
          value: location.locationId,
          label: location.parentPathSnapshot,
        })),
      ),
    [locationQuery.data],
  )
  const activeRoleTemplates = useMemo(
    () => roleTemplates.filter((roleTemplate) => roleTemplate.status === 'active'),
    [roleTemplates],
  )

  const displayNamePreview = buildDisplayName(legalFirstName, legalLastName, preferredName)
  const selectedSiteOption = siteOptions.find((option) => option.value === siteOrgUnitId)
  const selectedDepartmentOption = departmentOptions.find((option) => option.value === departmentOrgUnitId)
  const selectedTeamOption = teamOptions.find((option) => option.value === teamOrgUnitId)
  const selectedPositionOption = positionOptions.find((option) => option.value === positionOrgUnitId)
  const selectedManagerOption = managerOptions.find((option) => option.value === managerPersonId)
  const selectedHomeBaseLocationOption = locationOptions.find((option) => option.value === homeBaseLocationId)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()

    await onCreate({
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
      employmentStatus,
      workRelationshipType,
      employmentType,
      startDate: startDate || null,
      expectedStartDate: expectedStartDate || null,
      primaryOrgUnitId: departmentOrgUnitId || siteOrgUnitId || null,
      siteOrgUnitId,
      departmentOrgUnitId,
      teamOrgUnitId,
      positionOrgUnitId,
      managerPersonId: managerPersonId || null,
      jobTitle: jobTitle.trim() || null,
      homeBaseLocationId: homeBaseLocationId || null,
      canLogin,
      initialRoleAssignments: initialRoleAssignments.map((assignment) => ({
        roleTemplateId: assignment.roleTemplateId,
        scopeType: assignment.scopeType,
        scopeValue: assignment.scopeType === 'tenant' ? null : assignment.scopeValue || null,
        expiresAt: assignment.expiresAt ? new Date(assignment.expiresAt).toISOString() : null,
      })),
    })
  }

  if (!canManage) {
    return null
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6" data-testid="create-person-panel">
      <header className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Create person</h2>
          <p className="mt-1 text-xs text-slate-500">
            Guided StaffArr creation aligned to the person model: identity, contact, placement, login intent,
            and optional initial access.
          </p>
        </div>
        <div className="rounded-full border border-slate-700 bg-slate-950/60 px-3 py-1 text-xs text-slate-300">
          Step {step + 1} of 5: {stepTitle(step)}
        </div>
      </header>

      <ol className="mt-4 grid gap-2 text-xs text-slate-400 sm:grid-cols-5">
        {[0, 1, 2, 3, 4].map((index) => (
          <li
            key={index}
            className={`rounded-lg border px-3 py-2 ${
              index === step
                ? 'border-sky-500/60 bg-sky-950/30 text-sky-200'
                : index < step
                  ? 'border-emerald-500/40 bg-emerald-950/20 text-emerald-200'
                  : 'border-slate-700 bg-slate-950/40'
            }`}
          >
            {stepTitle(index as WizardStep)}
          </li>
        ))}
      </ol>

      <form className="mt-6 space-y-6" onSubmit={handleSubmit}>
        {step === 0 ? (
          <div className="grid gap-4 md:grid-cols-2">
            <label htmlFor="create-person-legal-first-name" className="block text-sm text-slate-300">
              Legal first name
              <input
                id="create-person-legal-first-name"
                value={legalFirstName}
                onChange={(event) => setLegalFirstName(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                required
              />
            </label>
            <label htmlFor="create-person-legal-middle-name" className="block text-sm text-slate-300">
              Legal middle name
              <input
                id="create-person-legal-middle-name"
                value={legalMiddleName}
                onChange={(event) => setLegalMiddleName(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label htmlFor="create-person-legal-last-name" className="block text-sm text-slate-300">
              Legal last name
              <input
                id="create-person-legal-last-name"
                value={legalLastName}
                onChange={(event) => setLegalLastName(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                required
              />
            </label>
            <label htmlFor="create-person-preferred-name" className="block text-sm text-slate-300">
              Preferred name
              <input
                id="create-person-preferred-name"
                value={preferredName}
                onChange={(event) => setPreferredName(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label htmlFor="create-person-pronouns" className="block text-sm text-slate-300 md:col-span-2">
              Pronouns
              <input
                id="create-person-pronouns"
                value={pronouns}
                onChange={(event) => setPronouns(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <div className="rounded-xl border border-slate-700 bg-slate-950/50 p-4 md:col-span-2">
              <p className="text-xs uppercase tracking-wide text-slate-500">Display preview</p>
              <p className="mt-2 text-lg font-semibold text-white">{displayNamePreview || 'Not enough information yet'}</p>
            </div>
          </div>
        ) : null}

        {step === 1 ? (
          <div className="grid gap-4 md:grid-cols-2">
            <label htmlFor="create-person-primary-email" className="block text-sm text-slate-300">
              Primary email
              <input
                id="create-person-primary-email"
                type="email"
                value={primaryEmail}
                onChange={(event) => setPrimaryEmail(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                required
              />
            </label>
            <label htmlFor="create-person-alternate-email" className="block text-sm text-slate-300">
              Alternate email
              <input
                id="create-person-alternate-email"
                type="email"
                value={alternateEmail}
                onChange={(event) => setAlternateEmail(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label htmlFor="create-person-primary-phone" className="block text-sm text-slate-300">
              Primary phone
              <input
                id="create-person-primary-phone"
                value={primaryPhone}
                onChange={(event) => setPrimaryPhone(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label htmlFor="create-person-alternate-phone" className="block text-sm text-slate-300">
              Alternate phone
              <input
                id="create-person-alternate-phone"
                value={alternatePhone}
                onChange={(event) => setAlternatePhone(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label htmlFor="create-person-work-phone" className="block text-sm text-slate-300">
              Work phone
              <input
                id="create-person-work-phone"
                value={workPhone}
                onChange={(event) => setWorkPhone(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label htmlFor="create-person-job-title" className="block text-sm text-slate-300">
              Job title
              <input
                id="create-person-job-title"
                value={jobTitle}
                onChange={(event) => setJobTitle(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label htmlFor="create-person-work-relationship" className="block text-sm text-slate-300">
              Work relationship
              <select
                id="create-person-work-relationship"
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
            <label htmlFor="create-person-employment-type" className="block text-sm text-slate-300">
              Employment type
              <select
                id="create-person-employment-type"
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
            <label htmlFor="create-person-employment-status" className="block text-sm text-slate-300">
              Status
              <select
                id="create-person-employment-status"
                value={employmentStatus}
                onChange={(event) => setEmploymentStatus(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              >
                {EMPLOYMENT_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="create-person-expected-start-date" className="block text-sm text-slate-300">
              Expected start date
              <input
                id="create-person-expected-start-date"
                type="date"
                value={expectedStartDate}
                onChange={(event) => setExpectedStartDate(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <label htmlFor="create-person-start-date" className="block text-sm text-slate-300">
              Start date
              <input
                id="create-person-start-date"
                type="date"
                value={startDate}
                onChange={(event) => setStartDate(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              />
            </label>
          </div>
        ) : null}

        {step === 2 ? (
          <div className="grid gap-4 md:grid-cols-2">
            <label className="block text-sm text-slate-300">
              Site
              <div className="mt-1">
                <StaticSearchPicker
                  value={siteOrgUnitId}
                  onChange={(value) => {
                    setSiteOrgUnitId(value)
                    setHomeBaseLocationId('')
                  }}
                  options={siteOptions}
                  selectedOption={selectedSiteOption}
                  placeholder="Select site"
                  testId="create-person-site-org-unit"
                />
              </div>
            </label>
            <label className="block text-sm text-slate-300">
              Department
              <div className="mt-1">
                <StaticSearchPicker
                  value={departmentOrgUnitId}
                  onChange={setDepartmentOrgUnitId}
                  options={departmentOptions}
                  selectedOption={selectedDepartmentOption}
                  placeholder="Select department"
                  testId="create-person-department-org-unit"
                />
              </div>
            </label>
            <label className="block text-sm text-slate-300">
              Team
              <div className="mt-1">
                <StaticSearchPicker
                  value={teamOrgUnitId}
                  onChange={setTeamOrgUnitId}
                  options={teamOptions}
                  selectedOption={selectedTeamOption}
                  placeholder="Select team"
                  testId="create-person-team-org-unit"
                />
              </div>
            </label>
            <label className="block text-sm text-slate-300">
              Position
              <div className="mt-1">
                <StaticSearchPicker
                  value={positionOrgUnitId}
                  onChange={setPositionOrgUnitId}
                  options={positionOptions}
                  selectedOption={selectedPositionOption}
                  placeholder="Select position"
                  testId="create-person-position-org-unit"
                />
              </div>
            </label>
            <label className="block text-sm text-slate-300">
              Manager
              <div className="mt-1">
                <StaticSearchPicker
                  value={managerPersonId}
                  onChange={setManagerPersonId}
                  options={managerOptions}
                  selectedOption={selectedManagerOption}
                  placeholder="No manager"
                  testId="create-person-manager"
                />
              </div>
            </label>
            <label className="block text-sm text-slate-300">
              Home base location
              <div className="mt-1">
                <StaticSearchPicker
                  value={homeBaseLocationId}
                  onChange={setHomeBaseLocationId}
                  options={locationOptions}
                  selectedOption={selectedHomeBaseLocationOption}
                  placeholder={siteOrgUnitId ? 'Select home base location' : 'Select a site first'}
                  testId="create-person-home-base-location"
                  disabled={!siteOrgUnitId || locationQuery.isLoading}
                />
              </div>
              {locationQuery.isLoading ? (
                <p className="mt-1 text-xs text-slate-500">Loading site locations...</p>
              ) : null}
            </label>
          </div>
        ) : null}

        {step === 3 ? (
          <div className="space-y-4">
            <label className="flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={canLogin}
                onChange={(event) => setCanLogin(event.target.checked)}
              />
              Person can log in through NexArr
            </label>
            <div className="rounded-xl border border-slate-700 bg-slate-950/40 p-4">
              <div className="flex items-center justify-between gap-3">
                <div>
                  <h3 className="text-sm font-medium text-slate-200">Initial role assignments</h3>
                  <p className="mt-1 text-xs text-slate-500">
                    StaffArr stores assignment intent here. NexArr credentials and TrainArr training remain external.
                  </p>
                </div>
                <button
                  type="button"
                  onClick={() =>
                    setInitialRoleAssignments((current) => [
                      ...current,
                      {
                        id: `${Date.now()}-${current.length}`,
                        roleTemplateId: '',
                        scopeType: 'tenant',
                        scopeValue: '',
                        expiresAt: '',
                      },
                    ])
                  }
                  className="rounded-md border border-slate-700 px-3 py-2 text-xs text-slate-200 hover:border-slate-500"
                >
                  Add assignment
                </button>
              </div>

              {initialRoleAssignments.length === 0 ? (
                <p className="mt-4 text-sm text-slate-500">No initial role assignments selected.</p>
              ) : (
                <div className="mt-4 space-y-4">
                  {initialRoleAssignments.map((assignment, index) => {
                    const roleTemplateOptions = toPickerOptions(
                      activeRoleTemplates.map((roleTemplate) => ({
                        value: roleTemplate.roleTemplateId,
                        label: `${roleTemplate.name} (${roleTemplate.roleKey})`,
                      })),
                    )
                    const selectedRoleTemplateOption = roleTemplateOptions.find(
                      (option) => option.value === assignment.roleTemplateId,
                    )

                    return (
                      <div key={assignment.id} className="rounded-lg border border-slate-800 bg-slate-900/70 p-4">
                        <div className="flex items-center justify-between gap-3">
                          <h4 className="text-sm font-medium text-slate-200">Assignment {index + 1}</h4>
                          <button
                            type="button"
                            onClick={() =>
                              setInitialRoleAssignments((current) =>
                                current.filter((currentAssignment) => currentAssignment.id !== assignment.id),
                              )
                            }
                            className="text-xs text-rose-300 hover:text-rose-200"
                          >
                            Remove
                          </button>
                        </div>
                        <div className="mt-3 grid gap-3 md:grid-cols-2">
                          <label className="block text-sm text-slate-300">
                            Role template
                            <div className="mt-1">
                              <StaticSearchPicker
                                value={assignment.roleTemplateId}
                                onChange={(value) =>
                                  setInitialRoleAssignments((current) =>
                                    current.map((currentAssignment) =>
                                      currentAssignment.id === assignment.id
                                        ? { ...currentAssignment, roleTemplateId: value }
                                        : currentAssignment,
                                    ),
                                  )
                                }
                                options={roleTemplateOptions}
                                selectedOption={selectedRoleTemplateOption}
                                placeholder="Select active role template"
                                testId={`create-person-role-template-${assignment.id}`}
                              />
                            </div>
                          </label>
                          <label htmlFor={`create-person-role-scope-${assignment.id}`} className="block text-sm text-slate-300">
                            Scope type
                            <select
                              id={`create-person-role-scope-${assignment.id}`}
                              value={assignment.scopeType}
                              onChange={(event) =>
                                setInitialRoleAssignments((current) =>
                                  current.map((currentAssignment) =>
                                    currentAssignment.id === assignment.id
                                      ? {
                                          ...currentAssignment,
                                          scopeType: event.target.value as CreatePersonRoleAssignmentRequest['scopeType'],
                                          scopeValue: '',
                                        }
                                      : currentAssignment,
                                  ),
                                )
                              }
                              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                            >
                              {ROLE_SCOPE_OPTIONS.map((option) => (
                                <option key={option.value} value={option.value}>
                                  {option.label}
                                </option>
                              ))}
                            </select>
                          </label>
                          <label className="block text-sm text-slate-300">
                            Scope value
                            <div className="mt-1">
                              <ScopeValuePicker
                                scopeType={assignment.scopeType}
                                value={assignment.scopeValue}
                                onChange={(value) =>
                                  setInitialRoleAssignments((current) =>
                                    current.map((currentAssignment) =>
                                      currentAssignment.id === assignment.id
                                        ? { ...currentAssignment, scopeValue: value }
                                        : currentAssignment,
                                    ),
                                  )
                                }
                                orgUnits={orgUnits}
                                testId={`create-person-role-scope-value-${assignment.id}`}
                              />
                              {assignment.scopeType === 'tenant' ? (
                                <p className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-400">
                                  Tenant-wide scope
                                </p>
                              ) : null}
                            </div>
                          </label>
                          <label htmlFor={`create-person-role-expires-${assignment.id}`} className="block text-sm text-slate-300">
                            Expires at
                            <input
                              id={`create-person-role-expires-${assignment.id}`}
                              type="datetime-local"
                              value={assignment.expiresAt}
                              onChange={(event) =>
                                setInitialRoleAssignments((current) =>
                                  current.map((currentAssignment) =>
                                    currentAssignment.id === assignment.id
                                      ? { ...currentAssignment, expiresAt: event.target.value }
                                      : currentAssignment,
                                  ),
                                )
                              }
                              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                            />
                          </label>
                        </div>
                      </div>
                    )
                  })}
                </div>
              )}
            </div>
          </div>
        ) : null}

        {step === 4 ? (
          <div className="space-y-4 rounded-xl border border-slate-700 bg-slate-950/40 p-5">
            <div>
              <p className="text-xs uppercase tracking-wide text-slate-500">Identity</p>
              <p className="mt-2 text-lg font-semibold text-white">{displayNamePreview}</p>
              <p className="mt-1 text-sm text-slate-400">{primaryEmail || 'No primary email entered yet'}</p>
            </div>
            <dl className="grid gap-3 text-sm md:grid-cols-2">
              <div>
                <dt className="text-slate-500">Status</dt>
                <dd className="text-slate-200">{employmentStatus.replaceAll('_', ' ')}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Work relationship</dt>
                <dd className="text-slate-200">{workRelationshipType.replaceAll('_', ' ')}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Placement</dt>
                <dd className="text-slate-200">
                  {[siteOrgUnitId, departmentOrgUnitId, teamOrgUnitId, positionOrgUnitId].filter(Boolean).length === 4
                    ? 'Ready'
                    : 'Incomplete'}
                </dd>
              </div>
              <div>
                <dt className="text-slate-500">Login intent</dt>
                <dd className="text-slate-200">{canLogin ? 'Login requested' : 'No login requested'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Initial access</dt>
                <dd className="text-slate-200">
                  {initialRoleAssignments.length} assignment{initialRoleAssignments.length === 1 ? '' : 's'}
                </dd>
              </div>
              <div>
                <dt className="text-slate-500">Manager</dt>
                <dd className="text-slate-200">
                  {peopleOptions.find((person) => person.personId === managerPersonId)?.displayName ?? 'None'}
                </dd>
              </div>
            </dl>
            <p className="text-xs text-slate-500">
              Create will save the StaffArr person record, seed the initial assignment, and create any selected role
              assignments in one flow. TrainArr training and NexArr credentials remain handoff-based.
            </p>
          </div>
        ) : null}

        {errorMessage ? (
          <div>
            <ApiErrorCallout title="Create person failed" message={errorMessage} />
          </div>
        ) : null}

        <div className="flex flex-wrap items-center justify-between gap-3 border-t border-slate-700 pt-4">
          <button
            type="button"
            onClick={() => setStep((current) => Math.max(0, current - 1) as WizardStep)}
            disabled={step === 0 || isSubmitting}
            className="rounded-md border border-slate-700 px-4 py-2 text-sm text-slate-200 hover:border-slate-500 disabled:opacity-50"
          >
            Back
          </button>

          <div className="flex flex-wrap items-center gap-3">
            {step < 4 ? (
              <button
                type="button"
                onClick={() => setStep((current) => Math.min(4, current + 1) as WizardStep)}
                disabled={
                  isSubmitting ||
                  !isStepValid(step, {
                    legalFirstName,
                    legalLastName,
                    primaryEmail,
                    workRelationshipType,
                    employmentStatus,
                    siteOrgUnitId,
                    departmentOrgUnitId,
                    teamOrgUnitId,
                    positionOrgUnitId,
                  })
                }
                className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              >
                Next
              </button>
            ) : (
              <button
                type="submit"
                disabled={isSubmitting}
                className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              >
                {isSubmitting ? 'Creating...' : 'Create person'}
              </button>
            )}
          </div>
        </div>
      </form>
    </section>
  )
}
