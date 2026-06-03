import { type FormEvent, useState } from 'react'
import { ApiErrorCallout, ControlledSelect, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import type { OrgUnitResponse } from '../api/types'

const EMPLOYMENT_STATUS_OPTIONS = [
  { value: 'active', label: 'Active' },
  { value: 'inactive', label: 'Inactive' },
  { value: 'terminated', label: 'Terminated' },
]

interface CreatePersonPanelProps {
  orgUnits: OrgUnitResponse[]
  peopleOptions: Array<{ personId: string; displayName: string }>
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
  onCreate: (request: {
    givenName: string
    familyName: string
    primaryEmail: string
    employmentStatus: string
    primaryOrgUnitId: string | null
    managerPersonId: string | null
    jobTitle: string | null
  }) => Promise<void>
}

export function CreatePersonPanel({
  orgUnits,
  peopleOptions,
  canManage,
  isSubmitting,
  errorMessage,
  onCreate,
}: CreatePersonPanelProps) {
  const [givenName, setGivenName] = useState('')
  const [familyName, setFamilyName] = useState('')
  const [primaryEmail, setPrimaryEmail] = useState('')
  const [employmentStatus, setEmploymentStatus] = useState('active')
  const [primaryOrgUnitId, setPrimaryOrgUnitId] = useState('')
  const [managerPersonId, setManagerPersonId] = useState('')
  const [jobTitle, setJobTitle] = useState('')

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onCreate({
      givenName: givenName.trim(),
      familyName: familyName.trim(),
      primaryEmail: primaryEmail.trim(),
      employmentStatus,
      primaryOrgUnitId: primaryOrgUnitId || null,
      managerPersonId: managerPersonId || null,
      jobTitle: jobTitle.trim() || null,
    })
    setGivenName('')
    setFamilyName('')
    setPrimaryEmail('')
    setEmploymentStatus('active')
    setPrimaryOrgUnitId('')
    setManagerPersonId('')
    setJobTitle('')
  }

  if (!canManage) {
    return null
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6" data-testid="create-person-panel">
      <header>
        <h2 className="text-sm font-medium text-slate-300">Create person</h2>
        <p className="mt-1 text-xs text-slate-500">
          Add a workforce record to the people directory. docs/13 people directory.
        </p>
      </header>

      <form className="mt-4 grid gap-4 md:grid-cols-2" onSubmit={handleSubmit}>
        <label htmlFor="create-person-given-name" className="block text-sm text-slate-300">
          Given name
          <input
            id="create-person-given-name"
            value={givenName}
            onChange={(event) => setGivenName(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            required
            minLength={1}
          />
        </label>
        <label htmlFor="create-person-family-name" className="block text-sm text-slate-300">
          Family name
          <input
            id="create-person-family-name"
            value={familyName}
            onChange={(event) => setFamilyName(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            required
            minLength={1}
          />
        </label>
        <label htmlFor="create-person-primary-email" className="block text-sm text-slate-300 md:col-span-2">
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
        <ControlledSelect
          label="Employment status"
          value={employmentStatus}
          onChange={setEmploymentStatus}
          options={EMPLOYMENT_STATUS_OPTIONS}
          emptyLabel="Select status…"
          testId="create-person-employment-status"
        />
        <label htmlFor="create-person-job-title" className="block text-sm text-slate-300">
          Job title
          <input
            id="create-person-job-title"
            value={jobTitle}
            onChange={(event) => setJobTitle(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
        <label className="block text-sm text-slate-300">
          Primary org unit
          <div className="mt-1">
            <OrgUnitPicker
              value={primaryOrgUnitId}
              onChange={setPrimaryOrgUnitId}
              orgUnits={orgUnits}
              testId="create-person-primary-org-unit"
              emptyLabel="Unassigned"
            />
          </div>
        </label>
        <label className="block text-sm text-slate-300">
          Manager
          <div className="mt-1">
            <PersonPicker
              value={managerPersonId}
              onChange={setManagerPersonId}
              peopleOptions={peopleOptions}
              testId="create-person-manager"
              emptyLabel="None"
            />
          </div>
        </label>
        <div className="md:col-span-2">
          {errorMessage ? (
            <div className="mb-3">
              <ApiErrorCallout title="Create person failed" message={errorMessage} />
            </div>
          ) : null}
          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            {isSubmitting ? 'Creating…' : 'Create person'}
          </button>
        </div>
      </form>
    </section>
  )
}

function OrgUnitPicker({
  value,
  onChange,
  orgUnits,
  testId,
  emptyLabel,
}: {
  value: string
  onChange: (value: string) => void
  orgUnits: OrgUnitResponse[]
  testId: string
  emptyLabel: string
}) {
  const options = orgUnits.map((orgUnit) => ({
    value: orgUnit.orgUnitId,
    label: `${orgUnit.name} (${orgUnit.unitType})`,
  })) satisfies PickerOption[]
  const selectedOption = options.find((option) => option.value === value)

  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={options}
      selectedOption={selectedOption}
      placeholder={emptyLabel}
      testId={testId}
    />
  )
}

function PersonPicker({
  value,
  onChange,
  peopleOptions,
  testId,
  emptyLabel,
}: {
  value: string
  onChange: (value: string) => void
  peopleOptions: Array<{ personId: string; displayName: string }>
  testId: string
  emptyLabel: string
}) {
  const options = peopleOptions.map((person) => ({
    value: person.personId,
    label: person.displayName,
  })) satisfies PickerOption[]
  const selectedOption = options.find((option) => option.value === value)

  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={options}
      selectedOption={selectedOption}
      placeholder={emptyLabel}
      testId={testId}
    />
  )
}
